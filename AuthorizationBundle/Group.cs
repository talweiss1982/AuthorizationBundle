using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationBundle
{
    public class Group
    {
        public string Id { get; set; }
        public Permission Permission { get; set; }
        public HashSet<string> Members { get; set; }
        public string Description { get; set; }
        public string Parent { get; set; }

        public class GroupVersion
        {
            public string Creator { get; set; }
            public int Version { get; set; }
        }
    }
}
