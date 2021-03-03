using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;

namespace HybridBlazor.Client
{
    public class FluentValidator : ComponentBase
    {
        private readonly static char[] separators = new[] { '.', '[' };
        private IValidator validator;

        [Inject] IServiceProvider ServiceProvider { get; set; }
        [CascadingParameter] private EditContext EditContext { get; set; }

        protected override void OnInitialized()
        {
            if (EditContext == null)
            {
                throw new InvalidOperationException($"{nameof(FluentValidator)} requires a cascading " +
                    $"parameter of type {nameof(EditContext)}.");
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(EditContext.Model.GetType());
            validator = ServiceProvider.GetService(validatorType) as IValidator;

            if (validator == null)
            {
                throw new InvalidOperationException($"{nameof(FluentValidator)} model validator not found");
            }

            var messages = new ValidationMessageStore(EditContext);

            EditContext.OnFieldChanged += (sender, eventArgs)
                => ValidateModel((EditContext)sender, messages);

            EditContext.OnValidationRequested += (sender, eventArgs)
                => ValidateModel((EditContext)sender, messages, onSubmit: true);
        }

        private void ValidateModel(EditContext editContext, ValidationMessageStore messages, bool onSubmit = false)
        {
            var context = new ValidationContext<object>(editContext.Model);
            var validationResult = validator.Validate(context);
            messages.Clear();
            foreach (var error in validationResult.Errors)
            {
                var fieldIdentifier = ToFieldIdentifier(editContext, error.PropertyName);
                if (!editContext.IsModified(fieldIdentifier) && !onSubmit) continue;
                messages.Add(fieldIdentifier, error.ErrorMessage);
            }
            editContext.NotifyValidationStateChanged();
        }

        private static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
        {
            var obj = editContext.Model;

            while (true)
            {
                var nextTokenEnd = propertyPath.IndexOfAny(separators);
                if (nextTokenEnd < 0)
                {
                    return new FieldIdentifier(obj, propertyPath);
                }

                var nextToken = propertyPath.Substring(0, nextTokenEnd);
                propertyPath = propertyPath[(nextTokenEnd + 1)..];

                object newObj;
                if (nextToken.EndsWith("]"))
                {
                    nextToken = nextToken[0..^1];
                    var prop = obj.GetType().GetProperty("Item");
                    var indexerType = prop.GetIndexParameters()[0].ParameterType;
                    var indexerValue = Convert.ChangeType(nextToken, indexerType);
                    newObj = prop.GetValue(obj, new object[] { indexerValue });
                }
                else
                {
                    var prop = obj.GetType().GetProperty(nextToken);
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.");
                    }
                    newObj = prop.GetValue(obj);
                }

                if (newObj == null)
                {
                    return new FieldIdentifier(obj, nextToken);
                }

                obj = newObj;
            }
        }
    }
}
