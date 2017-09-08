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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
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
            ValidAudience = "https://yourapplication.example.com",

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5000/",
            IssuerSigningKey = new X509SecurityKey(GetX509Certificate2()),
        };

        public static CookieBuilder CookieBuilder => new CookieBuilder
        {
            Domain = "custom-auth",
            Expiration = TimeSpan.FromDays(30),
            HttpOnly = true,
            Name = "auth-wrapper",
            SecurePolicy = CookieSecurePolicy.None
        };

    public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore();
            services.AddAuthentication()
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/login");
                    options.Events.OnValidatePrincipal = ctx =>
                    {
                        var requestCookie = ctx.Options.CookieManager.GetRequestCookie(ctx.HttpContext, ctx.Options.Cookie.Domain);
                        ctx.HttpContext.Request.Headers.Add("Authentication", $"Bearer {requestCookie}");
                        return Task.CompletedTask;
                    };
                    options.Cookie = CookieBuilder;
                })
                .AddJwtBearer(options => options.TokenValidationParameters = TokenParameters);
        }

        private static X509Certificate2 GetX509Certificate2()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var sec = new DirectoryInfo(currentDirectory).EnumerateDirectories().First(x => x.Name == "Security");
            var certFile = sec.EnumerateFiles().First(x => x.Extension.Contains("pfx"));
            var actualCert = new X509Certificate2(certFile.FullName, "password");
            return actualCert;
        }

        private static X509Certificate2 GetX509Certificate2Key()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var sec = new DirectoryInfo(currentDirectory).EnumerateDirectories().First(x => x.Name == "Security");
            var certFile = sec.EnumerateFiles().First(x => x.Extension.Contains("pfx"));
            var actualCert = new X509Certificate2(certFile.FullName);
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

                    var token = new JwtSecurityToken(TokenParameters.ValidIssuer, null,
                        new ClaimsIdentity().Claims, null, null,
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

            app.UseAuthentication();

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
