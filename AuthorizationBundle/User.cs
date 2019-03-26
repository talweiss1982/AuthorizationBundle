using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationBundle
{
    public class User
    {
        public string PasswordHash { get; set; }
        public List<string> Groups { get; set; } 
        public Permission Permissions { get; set; }
        public string Id { get; set; }
    }
}
