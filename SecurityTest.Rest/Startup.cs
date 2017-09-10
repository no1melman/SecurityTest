using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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
            services.AddMvcCore();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = TokenParameters;
                    options.Events = new JwtBearerEvents();
                    options.Events.OnTokenValidated += context =>
                    {
                        if (context?.HttpContext == null)
                        {
                            return Task.CompletedTask;
                        }

                        return context.HttpContext.SignInAsync(context.Principal);
                    };

                });
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

            app.Map(new PathString("/api/login"), async b =>
            {
                b.Use((context, next) =>
                {
                    var dp = (IDataProtectionProvider)b.ApplicationServices.GetService(typeof(IDataProtectionProvider));

                    var streamReader = new StreamReader(context.Request.Body);
                    var requestBody = streamReader.ReadToEnd();
                    streamReader.Dispose();
                    var userLogin = JsonConvert.DeserializeObject<UserLogin>(requestBody);

                    if (userLogin.UserName != "Callum" && userLogin.Password != "letmein")
                    {
                        context.Response.StatusCode = 401;
                        var unauthorisedBytes = Encoding.UTF8.GetBytes("Unauthorized");
                        return context.Response.Body.WriteAsync(unauthorisedBytes, 0, unauthorisedBytes.Length);
                    }

                    var token = new JwtSecurityToken(TokenParameters.ValidIssuer, TokenParameters.ValidAudience,
                        new ClaimsIdentity().Claims, null, DateTime.UtcNow.AddDays(1),
                        new SigningCredentials(TokenParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256));
                    
                    var bearerToken = new JwtSecurityTokenHandler().WriteToken(token);

                    context.Response.StatusCode = 200;
                    ICookieManager cm = new ChunkingCookieManager();
                    cm.AppendResponseCookie(context, CookieBuilder.Name,
                        bearerToken,
                        CookieBuilder.Build(context));

                    var yay = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = true }));
                    return context.Response.Body.WriteAsync(yay, 0, yay.Length);
                });
            });

            app.Use((context, next) =>
            {
                if (context.Request.Cookies.Count <= 0)
                {
                    return next();
                }

                var authCookie = context.Request.Cookies.FirstOrDefault(x => x.Key == CookieBuilder.Name);

                if (!authCookie.Equals(default(KeyValuePair<string, string>)))
                {
                    context.Request.Headers.Add("Authorization", $"Bearer {authCookie.Value}");
                }

                return next();
            });

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                var user = context.User; // We can do this because of  options.AutomaticAuthenticate = true;
                if (user?.Identity?.IsAuthenticated ?? false)
                {
                    await next();
                }
                else
                {
                    // We can do this because of options.AutomaticChallenge = true;
                    await context.ChallengeAsync();
                }
            });

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

    public class DIII : IAuthenticationSignInHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync(AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }

        public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            throw new NotImplementedException();
        }
    }
}
