using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DataLayer;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SigningCredential;
using Types;

namespace TestIdentityServer
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
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            // services.AddTransient<ConfigurationDbContext>();
            services.AddTransient<DbInitializer>();

            services.AddTransient<Config>();

            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            //конфиги из файла
            services.AddOptions();
            services.Configure<ConfigurationManager>(Configuration.GetSection("ConfigurationManager"));

            services.AddDbContext<AuthContext>(options => options.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AuthContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
             .AddAspNetIdentity<ApplicationUser>()
             // this adds the config data from DB (clients, resources)
             .AddConfigurationStore(options =>
             {
                 options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                     sql => sql.MigrationsAssembly(migrationsAssembly));
             })
             // this adds the operational data from DB (codes, tokens, consents)
             .AddOperationalStore(options =>
             {
                 options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                 // this enables automatic token cleanup. this is optional.
                 //  options.EnableTokenCleanup = true;
                 // options.TokenCleanupInterval = 15; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
             });

            SigningCredentialConfig signingCredentialConfig = Configuration.GetSection("SigningCredentialConfig").Get<SigningCredentialConfig>();


            switch (signingCredentialConfig.SigningCredentialType)
            {
                case "default":
                    {
                        builder.AddDeveloperSigningCredential();
                    }
                    break;
                case "customRsa":
                    {
                        builder.AddSigningCredential(RSA.GenerateRsaKeys());
                    }
                    break;
                case "cert":
                    {
                        builder.AddSigningCredential(new X509Certificate2(signingCredentialConfig.SertName, signingCredentialConfig.SertPsw));
                    }
                    break;
                default:
                    {
                        builder.AddDeveloperSigningCredential();
                    }
                    break;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            /*
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });*/
        }
    }
}
