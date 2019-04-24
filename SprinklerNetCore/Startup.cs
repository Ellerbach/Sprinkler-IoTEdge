using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SprinklerNetCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI;
using SprinklerNetCore.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using Microsoft.AspNetCore.Localization;
using SprinklerNetCore.Resources;

namespace SprinklerNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            if (Environment.GetEnvironmentVariable("DB_CONNECTION") == "SQL")
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("DefaultConnection")));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(
                        Configuration.GetConnectionString("LocalConnection")));
            }

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var site = new SiteInformation(
                new AzureBlobSetings(Configuration.GetSection("AzureBlobSetings")["AccountName"],
                Configuration.GetSection("AzureBlobSetings")["AccountKey"],
                Configuration.GetSection("AzureBlobSetings")["ContainerName"]));
            services.AddSingleton<ISiteInformation>(site);
            services.AddSingleton<IAzureIoT>(new AzureIoT(site));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services.AddLocalization(options => options.ResourcesPath = nameof(Resources));
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en-US"),
                        new CultureInfo("fr")
                    };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Culture.FromLanguageToCulture(site.Settings.Language));
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Culture.FromLanguageToCulture(site.Settings.Language));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Avoid issues with coma and dots...
            //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-us");
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("fr"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });


            //Setup Roles if none existing
            try
            {
                RolesInitialization.SeedRoles(app.ApplicationServices).Wait();
            }
            catch (Exception)
            {
                // Run without the connection to SQL Azure database
                // Or the local one
            }
            
            
        }
    }
}
