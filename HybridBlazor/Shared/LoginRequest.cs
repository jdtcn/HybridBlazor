using FluentValidation;

namespace HybridBlazor.Shared
{
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(p => p.UserName).NotEmpty()
                .WithName(n => nameof(n.UserName)).WithMessage("UserName must not be empty");

            RuleFor(p => p.Password).NotEmpty()
                .WithName(n => nameof(n.Password)).WithMessage("Password must not be empty");
        }
    }
}
