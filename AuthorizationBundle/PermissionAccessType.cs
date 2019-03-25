using System;

namespace AuthorizationBundle
{
    [Flags]
    public enum PermissionAccessType
    {
        Read,
        Write,
        GiveRead,
        GiveWrite
    }
}