using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.Collection;
using amorphie.token.core.Models.User;
using Dapr.Client;
using Microsoft.Extensions.Configuration;

namespace amorphie.token.core.Constants
{
    public class CollectionUsers
    {
        private DaprClient _daprClient;
        private IConfiguration _configuration;
        private List<User> _users;
        public List<User> Users {get => _users ?? new List<User>();}
        public CollectionUsers(IConfiguration configuration, DaprClient daprClient)
        {
            _configuration = configuration;
            _daprClient = daprClient;

            try
            {
                _users = JsonSerializer.Deserialize<List<User>>(configuration["CollectionUsers"]);
            }
            catch (Exception)
            {
                
                _users = null;
            }
            
        }

        public async Task ReloadUsers()
        {
            try
            {
                var secrets = await _daprClient.GetSecretAsync(_configuration["DAPR_SECRET_STORE_NAME"], "ServiceConnections");
                var usersJson = secrets.FirstOrDefault(s => s.Key.Equals("CollectionUsers"));
                if(usersJson is {})
                {
                    _users = JsonSerializer.Deserialize<List<User>>(usersJson.Value)!;
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}