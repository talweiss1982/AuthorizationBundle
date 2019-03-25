﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AuthorizationBundle
{
    public class Permission
    {
        public string Description { get; set; }
        public PermissionAccessType AccessType { get; set; }
        public List<string> Databases { get; set; }
        public List<string> Collections { get; set; }
        public List<string> Ids { get; set; }
    }
}
