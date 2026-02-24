using System.Text;
using CloudBudget.API.Data;
using CloudBudget.API.Enums;
using CloudBudget.API.Services.ReportGenerators.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Services.ReportGenerators;

public class CsvReportGenerator(CloudBudgetDbContext db) : IReportGenerator
{
    public ReportFormat Format => ReportFormat.Csv;

    public async Task<Stream> GenerateMonthlyReportAsync(DateTime monthDate, CancellationToken ct = default)
    {
        var target = new DateTime(monthDate.Year, monthDate.Month, 1);
        var next = target.AddMonths(1);

        var rows = await db.Expenses
            .Where(e => e.Date >= target && e.Date < next)
            .Include(e => e.Category)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);

        var ms = new MemoryStream();
        using (var sw = new StreamWriter(ms, Encoding.UTF8, 1024, leaveOpen: true))
        {
            sw.WriteLine("Date,Description,Category,Amount");
            foreach (var r in rows)
            {
                var desc = r.Description?.Replace("\"", "\"\"") ?? "";
                var cat = r.Category?.Name?.Replace("\"", "\"\"") ?? "";
                sw.WriteLine($"{r.Date:yyyy-MM-dd},\"{desc}\",\"{cat}\",{r.Amount:F2}");
            }

            sw.Flush();
        }

        ms.Position = 0;
        return ms;
    }
}
