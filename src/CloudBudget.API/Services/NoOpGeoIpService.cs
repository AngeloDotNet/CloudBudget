using CloudBudget.API.Services.Interfaces;

namespace CloudBudget.API.Services
{
    public class NoOpGeoIpService : IGeoIpService
    {
        public Task<GeoResult?> LookupAsync(string ip, CancellationToken ct = default) => Task.FromResult<GeoResult?>(null);
    }
}