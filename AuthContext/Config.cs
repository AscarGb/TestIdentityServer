using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Types;

namespace DataLayer
{
    public class Config
    {
        IOptions<ConfigurationManager> _configurationManager;
        public Config(IOptions<ConfigurationManager> configurationManager)
        {
            _configurationManager = configurationManager;
        }
        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(_configurationManager.Value.ApiName, _configurationManager.Value.ApiName)
            };
        }

        public IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = _configurationManager.Value.ClientId,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequireClientSecret = false,
                    AllowedScopes = { _configurationManager.Value.ApiName,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile},
                    AllowOfflineAccess = true
                }
        };
        }
    }
}
