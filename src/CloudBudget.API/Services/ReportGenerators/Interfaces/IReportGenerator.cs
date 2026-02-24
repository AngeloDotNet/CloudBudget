using CloudBudget.API.Enums;

namespace CloudBudget.API.Services.ReportGenerators.Interfaces;

public interface IReportGenerator
{
    /// <summary>
    /// Indica il formato supportato da questo generatore.
    /// </summary>
    ReportFormat Format { get; }

    /// <summary>
    /// Genera il report per il mese che contiene monthDate (es. passare una data qualunque nel mese target).
    /// Restituisce uno Stream posizionato a 0 contenente il file (CSV o XLSX).
    /// </summary>
    Task<Stream> GenerateMonthlyReportAsync(DateTime monthDate, CancellationToken ct = default);
}