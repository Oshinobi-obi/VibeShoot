using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;

namespace VibeShoot.Services
{
    /// <summary>Rate limits for public forms and standard security headers.</summary>
    public static class WebSecurity
    {
        /// <summary>
        /// Per-visitor (IP) limits so forms can't be hammered by scripts:
        ///  - "track":   Transaction ID + mobile lookups (stops guessing other people's bookings);
        ///  - "forms":   booking and payment submissions;
        ///  - "reviews": review submissions.
        /// </summary>
        public static IServiceCollection AddVibeShootRateLimits(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("track", ctx => PerIp(ctx, permits: 8, window: TimeSpan.FromMinutes(5)));
                options.AddPolicy("forms", ctx => PerIp(ctx, permits: 10, window: TimeSpan.FromMinutes(10)));
                options.AddPolicy("reviews", ctx => PerIp(ctx, permits: 5, window: TimeSpan.FromHours(1)));
                options.OnRejected = async (context, token) =>
                {
                    var http = context.HttpContext;
                    const string message = "Too many attempts. Please wait a few minutes and try again.";
                    if (http.Request.Path.Value?.Contains("/api/", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        await http.Response.WriteAsJsonAsync(new { ok = false, error = message }, token);
                    }
                    else
                    {
                        http.Response.ContentType = "text/html; charset=utf-8";
                        await http.Response.WriteAsync(
                            "<!doctype html><meta charset=utf-8><meta name=viewport content='width=device-width,initial-scale=1'>" +
                            "<body style='background:#0a0a0c;color:#f5f5f2;font-family:Poppins,system-ui,sans-serif;display:grid;place-items:center;min-height:100vh;margin:0;text-align:center'>" +
                            $"<div><h1 style='font-family:Anton,sans-serif;font-weight:400;letter-spacing:.04em'>SLOW DOWN</h1><p>{message}</p>" +
                            "<p><a href='javascript:history.back()' style='color:#e3bf4a'>&larr; Go back</a></p></div>", token);
                    }
                };
            });
            return services;
        }

        private static RateLimitPartition<string> PerIp(HttpContext ctx, int permits, TimeSpan window) =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = window, QueueLimit = 0 });

        /// <summary>Headers that stop the site being framed (clickjacking), MIME sniffing and referrer leaks.</summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
            app.Use(async (context, next) =>
            {
                var h = context.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Content-Security-Policy"] = "frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'";
                h["Referrer-Policy"] = "strict-origin-when-cross-origin";
                h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
                await next();
            });
    }
}
