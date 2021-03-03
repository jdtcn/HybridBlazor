using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc.Server;
using HybridBlazor.Shared;
using HybridBlazor.Client;
using HybridBlazor.Server.Data;
using HybridBlazor.Server.Data.Models;
using HybridBlazor.Server.Services;

namespace HybridBlazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(nameof(ApplicationDbContext)));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);

                options.User.RequireUniqueEmail = true;
            });

            services
                .AddAuthentication(cfg =>
                {
                    cfg.DefaultScheme = IdentityConstants.ApplicationScheme;
                    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer()
                .AddCookie();

            services.Configure<HybridOptions>(Configuration);
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<GrpcExceptionInterceptor>();
                options.ResponseCompressionAlgorithm = "gzip";
                options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                options.EnableDetailedErrors = true;
            });
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddCodeFirstGrpc(config =>
            {
                config.EnableDetailedErrors = true;
                config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            });

            services.AddHttpContextAccessor();

            services.AddTransient(sp =>
            {
                var env = sp.GetService<IWebHostEnvironment>();
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext;

                var cookies = httpContext.Request.Cookies;
                var cookieContainer = new System.Net.CookieContainer();
                foreach (var c in cookies)
                {
                    cookieContainer.Add(new System.Net.Cookie(c.Key, c.Value) { Domain = httpContext.Request.Host.Host });
                }

                var handler = new HttpClientHandler { CookieContainer = cookieContainer };
                if (env.IsDevelopment())
                {
                    handler.ServerCertificateCustomValidationCallback = (c, v, b, n) => { return true; };
                }

                return handler;
            });

            services.AddTransient(sp =>
            {
                var handler = sp.GetService<HttpClientHandler>();
                var navManager = sp.GetService<NavigationManager>();

                return new HttpClient(handler) { BaseAddress = new Uri(navManager.BaseUri) };
            });
            services.AddScoped<IAuthService, ServerAuthService>();
            services.AddSingleton<CounterStateStorageService>();
            Client.Program.ConfigureCommonServices(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UpdateDatabase<ApplicationDbContext>(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Services.WeatherForecastService>();
                endpoints.MapGrpcService<Services.CounterService>();
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private static void UpdateDatabase<T>(IApplicationBuilder app) where T : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetService<T>();
            context.Database.EnsureCreated();
        }
    }

    public enum HybridType
    {
        ServerSide,
        WebAssembly,
        HybridManual,
        HybridOnNavigation,
        HybridOnReady
    }

    public class HybridOptions
    {
        public HybridType HybridType { get; set; }
    }
}