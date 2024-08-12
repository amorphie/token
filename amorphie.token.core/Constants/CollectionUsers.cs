using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using amorphie.token.core.Models.Collection;
using amorphie.token.core.Models.User;
using Microsoft.Extensions.Configuration;

namespace amorphie.token.core.Constants
{
    public class CollectionUsers
    {
        private List<User> _users;
        public List<User> Users {get => _users ?? new List<User>();}
        public CollectionUsers(IConfiguration configuration)
        {
            try
            {
                _users = JsonSerializer.Deserialize<List<User>>(configuration["CollectionUsers"]);
            }
            catch (Exception)
            {
                
                _users = null;
            }
            
        }
    }
}