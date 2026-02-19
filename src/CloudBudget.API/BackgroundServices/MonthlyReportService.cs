using CloudBudget.API.Services.EmailSender.Interfaces;
using CloudBudget.API.Services.ReportGenerators.Interfaces;

namespace CloudBudget.API.BackgroundServices
{
    public class MonthlyReportService(IServiceProvider serviceProvider, ILogger<MonthlyReportService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("MonthlyReportService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    // next 1st of next month at 02:00 UTC
                    var next = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddHours(2);
                    var delay = next - now;
                    if (delay < TimeSpan.Zero)
                    {
                        delay = TimeSpan.Zero;
                    }

                    logger.LogInformation("MonthlyReportService sleeping until {Next}", next);
                    await Task.Delay(delay, stoppingToken);

                    // build report for month just finished
                    var reportMonth = DateTime.UtcNow.AddMonths(-1);
                    using var scope = serviceProvider.CreateScope();
                    var generator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
                    var mailer = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var stream = await generator.GenerateMonthlyReportAsync(reportMonth, stoppingToken);
                    var fileName = $"report-{reportMonth:yyyy-MM}.csv";

                    // recipients could be configured; for demo read from config? using single sample recipient here
                    var recipients = new[] { "utente@example.com" };

                    await mailer.SendAsync(
                        recipients,
                        $"Report spese {reportMonth:yyyy-MM}",
                        "In allegato il report mensile delle spese.",
                        stream,
                        fileName,
                        stoppingToken);

                    logger.LogInformation("Monthly report generated and sent for {Month}", reportMonth.ToString("yyyy-MM"));
                }
                catch (OperationCanceledException)
                {
                    // cancellation requested
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errore durante generazione/invio report mensile");
                    // attendi un po' prima di riprovare per non spam logs in caso di failure
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            logger.LogInformation("MonthlyReportService stopping.");
        }
    }
}