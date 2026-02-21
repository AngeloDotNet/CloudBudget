namespace CloudBudget.API.Services.ReportGenerators.Interfaces;

public interface IReportGenerator
{
    Task<Stream> GenerateMonthlyReportAsync(DateTime monthDate, CancellationToken ct = default);
}