﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace Fiery.Api.Identity
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Data.ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetSection("Databases:Sqlite:ConnectionString").Value));

            services.AddIdentity<Models.ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<Data.ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add Mvc with custom views location
            services.AddMvc()
                .AddRazorOptions(razor => razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander()));

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddConfigurationStore(Configuration.GetSection("Databases:MongoDb"))
                .AddOperationalStore(Configuration.GetSection("Databases:MongoDb"))
                .AddSigningCredential(new X509Certificate2(Configuration["PrimaryCert"], "T5er41E2bf7im0f1Rt3VW637Hjki083f4G4n4N6d80Rty7uBe0"))
                .AddJwtBearerClientAuthentication()
                .AddAppAuthRedirectUriValidator()
                .AddAspNetIdentity<Models.ApplicationUser>();

            services.AddExternalIdentityProviders(Configuration.GetSection("Authentication:ExternalProviders"));

            return services.BuildServiceProvider(validateScopes: true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseIdentityServer();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }
}
