using CloudBudget.API.Enums;

namespace CloudBudget.API.Settings;

public class ReportSettings
{
    public ReportFormat DefaultFormat { get; set; } = ReportFormat.Csv;
}