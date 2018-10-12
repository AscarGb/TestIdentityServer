using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Types;
using IdentityServer4.Models;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Security.Claims;
using IdentityModel;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class DbInitializer
    {
        UserManager<ApplicationUser> _userManager;
        RoleManager<IdentityRole> _roleManager;
        IOptions<ConfigurationManager> _configurationManager;
        AuthContext _context;
        ConfigurationDbContext _configurationDbContext;
        PersistedGrantDbContext _persistedGrantDbContext;
        Config _config;

        public DbInitializer(IOptions<ConfigurationManager> configurationManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AuthContext context,
            ConfigurationDbContext configurationDbContext,
            PersistedGrantDbContext persistedGrantDbContext,
            Config config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configurationManager = configurationManager;
            _context = context;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _config = config;
        }

        public void Initialize()
        {
           // _context.Database.EnsureDeleted();

            _context.Database.Migrate();
            _configurationDbContext.Database.Migrate();
            _persistedGrantDbContext.Database.Migrate();

            if (!_context.Users.Any())
            {
                ApplicationUser user = new ApplicationUser { UserName = "admin" };

                var r = _userManager.CreateAsync(user, _configurationManager.Value.defaultAdminPsw).GetAwaiter().GetResult();

                if (r.Succeeded)
                {
                    _roleManager.CreateAsync(new IdentityRole { Name = ServerRoles.Admin }).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole { Name = ServerRoles.User }).GetAwaiter().GetResult();

                    _userManager.AddToRoleAsync(user, ServerRoles.Admin).GetAwaiter().GetResult();
                }
                else
                {
                    throw new Exception(string.Join("; ", r.Errors.Select(a => a.Description)));
                }

                r = _userManager.AddClaimsAsync(user, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address,
                        JsonConvert.SerializeObject(
                        new {
                            street_address = "One Hacker Way",
                            locality = "Heidelberg",
                            postal_code = 69118,
                            country = "Germany" }),
                        IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("location", "somewhere")
                    }).GetAwaiter().GetResult();

                if (!r.Succeeded)
                {
                    throw new Exception(string.Join("; ", r.Errors.Select(a => a.Description)));
                }

            }

            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var client in _config.GetClients().ToList())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in _config.GetIdentityResources().ToList())
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in _config.GetApiResources().ToList())
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }
        }
    }
}
