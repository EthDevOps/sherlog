using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Sherlog.Server.Pages;

public class LogbookModel : PageModel
{
    private readonly ILogger<LogbookModel> _logger;

    public LogbookModel(ILogger<LogbookModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        var db = new LogbookContext();
        LogEntries = await db.LogEntries.ToListAsync();
    }

    public List<LogEntry> LogEntries { get; set; }
}