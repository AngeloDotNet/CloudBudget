using CloudBudget.API.Enums;
using CloudBudget.API.Services.EmailSender.Interfaces;
using CloudBudget.API.Services.ReportGenerators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudBudget.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IReportGenerator reportGenerator, IEmailSender emailSender) : ControllerBase
{
    /// <summary>
    /// Genera il report per il mese specificato (se month non fornito, usa il mese passato).
    /// query params:
    /// - format: Csv (default) | Excel
    /// - month: yyyy-MM (es. 2026-01) opzionale
    /// - sendEmail: true|false (opzionale) se true invia il report via email usando la configurazione Smtp e destinatari hardcoded o configurati
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GenerateAsync([FromQuery] string format = nameof(ReportFormat.Csv), [FromQuery] string? month = null, [FromQuery] bool sendEmail = false, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ReportFormat>(format, true, out var rf))
        {
            return BadRequest(new { message = "Formato non valido. Usare Csv o Excel." });
        }

        DateTime targetMonth;
        if (!string.IsNullOrEmpty(month))
        {
            if (!DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out targetMonth))
            {
                return BadRequest(new { message = "Parametro month non valido. Usare yyyy-MM." });
            }
        }
        else
        {
            // mese passato
            targetMonth = DateTime.UtcNow.AddMonths(-1);
        }

        var stream = await reportGenerator.GenerateMonthlyReportAsync(targetMonth, ct);

        var ext = rf == ReportFormat.Excel ? "xlsx" : "csv";
        var mime = rf == ReportFormat.Excel ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
        var fileName = $"report-{targetMonth:yyyy-MM}.{ext}";

        if (sendEmail)
        {
            // esempio: invia a indirizzo hardcoded; potresti leggere destinatari da DB o config
            var recipients = new[] { "utente@example.com" };

            await emailSender.SendAsync(recipients, $"Report spese {targetMonth:yyyy-MM}", "In allegato il report richiesto.", stream, fileName, ct);
            return Accepted(new { message = "Report generato e inviato via email." });
        }

        stream.Position = 0;
        return File(stream, mime, fileName);
    }
}