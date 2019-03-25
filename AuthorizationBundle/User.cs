using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationBundle
{
    public class User
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public List<string> Groups { get; set; } 
        public List<Permission> Permissions { get; set; }
    }
}
