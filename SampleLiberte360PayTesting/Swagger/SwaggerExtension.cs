using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleLiberteTesting.Swagger
{
    public static class SwaggerExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns> 
        public static IServiceCollection AddSwaggerOption(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Payment Core Api",
                    Version = "v1.0",
                    Description = "Payment Api v1.0",
                    TermsOfService = new Uri("https://evat.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "SWD API Developer",
                        Email = "michael.ameyaw@persol.net",
                        Url = new Uri("http://www.evat.net/")
                    },
                    License = new OpenApiLicense { Name = "Payment", Url = new Uri("http://www.evat.net/") }
                });

            });
            return services;
        }


        /// <summary>
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaggerOption(this IApplicationBuilder app, IConfiguration configuration)
        {

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{configuration["AppSettings:Folder"]}/swagger/v1/swagger.json", "Payment v1");
                c.OAuthClientId(configuration["IdpSettings:ClientId"]);
                c.OAuthClientSecret(configuration["IdpSettings:ClientSecret"]);
                c.OAuthAppName("Payment");
                c.OAuthScopeSeparator(" ");
                c.OAuthUsePkce();
            });

            return app;
        }
    }
}