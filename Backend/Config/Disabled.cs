﻿namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Disable [AllowAnonymous] attribute, because it breaks the code.
    /// Even if you use [AllowAnonymous] higher in heirarhy of attributes it still works.
    /// You must not use it.
    /// </summary>

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AllowAnonymousAttribute : Attribute
    {
        public AllowAnonymousAttribute()
        {
            throw new NotSupportedException("Usage of [AllowAnonymous] in the codebase isn't allowed.");
        }
    }
}

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// Disable SignInManager, because we use JWT.
    /// </summary>
    public class SignInManager<TUser> where TUser : class
    {
        public SignInManager()
        {
            throw new NotSupportedException("Usage of SignInManager in the codebase isn't allowed. Because we use JWT.");
        }
    }
}