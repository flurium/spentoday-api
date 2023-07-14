﻿using Backend.Auth;
using Backend.Lib;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Backend.Config;

public static class Auth
{
    public static void AddJwt(this IServiceCollection services)
    {
        JwtSecrets jwtSecrets = new(
            Env.Get("JWT_ISSUER"),
            Env.Get("JWT_AUDIENCE"),
            Env.Get("JWT_SECRET")
        );
        services.AddScoped<Jwt>(_ => new(jwtSecrets));
    }

    public static void AddAuth(this IServiceCollection services)
    {
        services
            .AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<Db>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<DataProtectorTokenProvider<User>>("email");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = RefreshOnly.Scheme;
                options.DefaultScheme = RefreshOnly.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, RefreshOnlyAuthenticationHandler>(RefreshOnly.Scheme, null);
    }
}