namespace CloudBudget.API.Services.Interfaces;

public interface IGeoIpService
{
    /// <summary>
    /// Ritorna country code (ISO 2-letter) per l'ip; null se non disponibile.
    /// </summary>
    Task<GeoResult?> LookupAsync(string ip, CancellationToken ct = default);
}