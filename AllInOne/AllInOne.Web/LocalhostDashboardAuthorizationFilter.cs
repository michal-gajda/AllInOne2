namespace AllInOne.Web
{
    using Hangfire.Dashboard;

    public class LocalhostDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var remoteIp = httpContext.Connection.RemoteIpAddress;

            if (remoteIp != null)
            {
                var isLocalhost = remoteIp.ToString() == "127.0.0.1" || remoteIp.ToString() == "::1" || httpContext.Request.Host.Host.ToLower() == "localhost";

                if (isLocalhost)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
