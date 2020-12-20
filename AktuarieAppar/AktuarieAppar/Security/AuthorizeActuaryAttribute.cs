using Microsoft.AspNetCore.Authorization;

namespace AktuarieAppar.Security
{
    public sealed class AuthorizeActuaryAttribute : AuthorizeAttribute
    {
        public AuthorizeActuaryAttribute()
        {
            Roles = "GRolAktuarie";
        }
    }

    public sealed class AuthorizeActuaryNETAttribute : AuthorizeAttribute
    {
        public AuthorizeActuaryNETAttribute()
        {
            Roles = "GRolAktuarie, GRolDotnetSupport";
        }
    }

    //ROLL - KAPFÖRV OPERATION ANST
}
