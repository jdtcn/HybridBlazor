using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FluentValidation;
using HybridBlazor.Shared;
using HybridBlazor.Shared.Services;

namespace HybridBlazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            ConfigureCommonServices(builder.Services);

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddSingleton<IAuthService, WasmAuthService>();

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

            var host = builder.Build();
            await host.RunAsync();
        }

        public static void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddTransient<GrpcClientInterceptor>();

            services.AddGrpcClient<IWeatherForecastService>();
            services.AddGrpcClient<ICounterService>();

            services.AddValidatorsFromAssemblyContaining<HybridBlazor.Shared.LoginRequest>();

            services.AddScoped<HostAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<HostAuthenticationStateProvider>());
        }
    }
}
