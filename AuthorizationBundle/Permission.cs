using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationBundle
{
    public class Permission
    {
        public string Description { get; set; }
        public HashSet<string> Collections { get; set; }
        public HashSet<string> Ids { get; set; }
    }
}
