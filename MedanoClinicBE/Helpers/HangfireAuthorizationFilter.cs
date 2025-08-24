using Hangfire.Dashboard;

namespace MedanoClinicBE.Helpers
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            
            // For production, add proper authentication checks
            // For now, allow localhost access in development
            if (httpContext.Request.IsLocal())
            {
                return true;
            }

            // In production, you would check:
            // - User authentication
            // - User roles (Admin only)
            // - IP whitelist
            // - Other security measures

            return false;
        }
    }

    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this Microsoft.AspNetCore.Http.HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                else
                {
                    return System.Net.IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // For in-process
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
}