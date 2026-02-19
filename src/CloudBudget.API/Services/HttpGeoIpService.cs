using CloudBudget.API.Services.Interfaces;
using CloudBudget.API.Settings;
using Microsoft.Extensions.Options;

namespace CloudBudget.API.Services;

public class HttpGeoIpService(HttpClient http, IOptions<GeoIpSettings> options) : IGeoIpService
{
    private readonly GeoIpSettings options = options.Value;

    public async Task<GeoResult?> LookupAsync(string ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return null;
        }

        if (options.Provider?.Equals("ipapi", StringComparison.OrdinalIgnoreCase) == true)
        {
            // ipapi.co JSON: https://ipapi.co/{ip}/json/
            try
            {
                var url = $"https://ipapi.co/{ip}/json/";
                var rsp = await http.GetFromJsonAsync<Dictionary<string, object?>>(url, ct);

                if (rsp != null && rsp.TryGetValue("country_code", out var cc) && cc != null)
                {
                    return new GeoResult
                    {
                        CountryCode = cc.ToString(),
                        CountryName = rsp.GetValueOrDefault("country_name")?.ToString(),
                        Raw = System.Text.Json.JsonSerializer.Serialize(rsp)
                    };
                }
            }
            catch
            {
                // swallow errors and return null (fallback)
            }
        }
        else if (options.Provider?.Equals("ipinfo", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                // ipinfo.io/{ip}/json?token=TOKEN
                var tokenPart = string.IsNullOrEmpty(options.ApiKey) ? "" : $"?token={options.ApiKey}";
                var url = $"https://ipinfo.io/{ip}/json{tokenPart}";
                var rsp = await http.GetFromJsonAsync<Dictionary<string, object?>>(url, ct);
                if (rsp != null && rsp.TryGetValue("country", out var cc) && cc != null)
                {
                    return new GeoResult
                    {
                        CountryCode = cc.ToString(),
                        CountryName = null,
                        Raw = System.Text.Json.JsonSerializer.Serialize(rsp)
                    };
                }
            }
            catch { }
        }

        return null;
    }
}