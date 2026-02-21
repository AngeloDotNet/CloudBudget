using ClosedXML.Excel;
using CloudBudget.API.Data;
using CloudBudget.API.Enums;
using CloudBudget.API.Services.ReportGenerators.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudBudget.API.Services.ReportGenerators;

public class ExcelReportGenerator(CloudBudgetDbContext db) : IReportGenerator
{
    public ReportFormat Format => ReportFormat.Excel;

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

        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("Report");
            // Header
            ws.Cell(1, 1).Value = "Date";
            ws.Cell(1, 2).Value = "Description";
            ws.Cell(1, 3).Value = "Category";
            ws.Cell(1, 4).Value = "Amount";

            // Data
            var row = 2;
            foreach (var r in rows)
            {
                ws.Cell(row, 1).Value = r.Date;
                ws.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd";
                ws.Cell(row, 2).Value = r.Description ?? "";
                ws.Cell(row, 3).Value = r.Category?.Name ?? "";
                ws.Cell(row, 4).Value = r.Amount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                row++;
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();

            workbook.SaveAs(ms);
        }

        ms.Position = 0;
        return ms;
    }
}