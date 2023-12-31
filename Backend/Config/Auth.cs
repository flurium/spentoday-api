﻿using Backend.Auth;
using Data;
using Data.Models.UserTables;
using Lib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Backend.Config;

public static class Auth
{
    public static void AddJwt(this IServiceCollection services)
    {
        JwtSecrets jwtSecrets = new(Secrets.JWT_ISSUER, Secrets.JWT_AUDIENCE, Secrets.JWT_SECRET);
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
                options.DefaultForbidScheme = RefreshOnly.Scheme;
                options.DefaultChallengeScheme = RefreshOnly.Scheme;
                options.DefaultScheme = RefreshOnly.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, RefreshOnlyHandler>(RefreshOnly.Scheme, options => { });
    }
}