﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galt
{
    public static class HttpContextExtensions
    {
        public static IEnumerable<AuthenticationDescription> GetExternalProviders( this HttpContext context )
        {
            if ( context == null ) throw new ArgumentNullException( nameof( context ) );

            IEnumerable<AuthenticationDescription> schemes = context.Authentication.GetAuthenticationSchemes();

            return from description in context.Authentication.GetAuthenticationSchemes()
                   where !string.IsNullOrWhiteSpace( description.DisplayName )
                   select description;
        }

        public static bool IsProviderSupported( this HttpContext context, string provider )
        {
            if ( context == null ) throw new ArgumentNullException( nameof( context ) );

            return (from description in context.GetExternalProviders()
                    where string.Equals( description.AuthenticationScheme, provider, StringComparison.OrdinalIgnoreCase )
                    select description).Any();
        }
    }
}
