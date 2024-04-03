using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Helpers
{
    public class LogsEnricherHelper
    {
        public class HttpContextInfo
        {
            public string IPAddress { get; set; }
            public string Host { get; set; }
            public string Protocol { get; set; }
            public string Scheme { get; set; }
            public string User { get; set; }
        }

        public static class Enricher
        {
            public static void HttpRequestEnricher(IDiagnosticContext diagnosticContext, HttpContext httpContext)
            {
                var httpContextInfo = new HttpContextInfo
                {
                    Protocol = httpContext.Request.Protocol,
                    Scheme = httpContext.Request.Scheme,
                    IPAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                    Host = httpContext.Request.Host.ToString(),
                    User = GetUserInfo(httpContext.User)
                };

                diagnosticContext.Set("HttpContext", httpContextInfo, true);
            }

            private static string GetUserInfo(ClaimsPrincipal user)
            {
                if (user.Claims.Any())
                {
                    return user.Claims.FirstOrDefault(c => c.Type == "Id") + " " +  user.Claims.FirstOrDefault(c => c.Type == "Username");
                }
                return Environment.UserName;
            }
        }
    }
}
