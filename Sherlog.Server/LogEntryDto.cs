namespace Sherlog.Server;

public class LogEntryDto
{
    public string Tenant { get; set; }
    public List<string> RecipientGroups { get; set; }
    public string LogType { get; set; }
    public string Project { get; set; }
    public string Message { get; set; }
}