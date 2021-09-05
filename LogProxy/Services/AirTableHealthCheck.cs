using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LogProxy.Services
{
    public class AirTableHealthCheck : IHealthCheck
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await HttpClient.GetAsync("https://api.airtable.com", cancellationToken);
            if (response.IsSuccessStatusCode)
                return HealthCheckResult.Healthy();
            return HealthCheckResult.Unhealthy();
        }
    }
}