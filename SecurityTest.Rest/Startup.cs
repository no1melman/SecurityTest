using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SecurityTest.Rest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public static TokenValidationParameters TokenParameters => new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = "https://localhost:5000/",

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5000/",
            IssuerSigningKey = new X509SecurityKey(GetX509Certificate2()),
        };

        public static CookieBuilder CookieBuilder => new CookieBuilder
        {
            Expiration = TimeSpan.FromDays(30),
            HttpOnly = true,
            Name = "auth-wrapper",
            SecurePolicy = CookieSecurePolicy.None
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddJsonFormatters(s =>
            {
                s.ContractResolver = new CamelCasePropertyNamesContractResolver();
                s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = TokenParameters;
                    options.Events = new JwtBearerEvents();
                });
            services.AddTransient<IClientAuthenticationHandler, ClientAuthenticationHandler>();
            services.AddSingleton<ISecurityConfiguration>(new SecurityConfiguration(CookieBuilder, TokenParameters));
        }

        private static X509Certificate2 GetX509Certificate2()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var sec = new DirectoryInfo(currentDirectory).EnumerateDirectories().First(x => x.Name == "Security");
            var certFile = sec.EnumerateFiles().First(x => x.Extension.Contains("pfx"));
            var actualCert = new X509Certificate2(certFile.FullName, "password");
            return actualCert;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Map(new PathString("/api/login"), async b => b.UseClientAuthenticationServer(CookieBuilder, TokenParameters));

            app.UseTokenExtraction(CookieBuilder);

            app.UseAuthentication();

            app.UseAuthenticateAllRequests();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class UserLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
