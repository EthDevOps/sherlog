using Microsoft.EntityFrameworkCore;

namespace Sherlog.Server;

public class Program
{
    public static Config Configuration = new Config(); 
    
    public static void Main(string[] args)
    {
        
        // Load config
        Configuration.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
        Configuration.SmtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
        Configuration.SmtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        Configuration.MailFrom = Environment.GetEnvironmentVariable("MAIL_FROM");
        Configuration.MailReplyTo = Environment.GetEnvironmentVariable("MAIL_REPLYTO");
       
        // Database migration
        Console.WriteLine("Running Database migrations...");
        using (var db = new LogbookContext())
        {
            db.Database.Migrate();
        }

        Console.WriteLine("Migrations complete.");
        
        
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        // API mappings
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();

        app.Run();
    }

}

public class LogbookContext
    : DbContext
{
   public DbSet<LogEntry> LogEntries { get; set; }

   public string DbPath { get; set; }
   public LogbookContext()
   {
       DbPath = Path.Combine(Environment.GetEnvironmentVariable("DB_PATH") ?? ".","sherlog.db");
   } 
   
   protected override void OnConfiguring(DbContextOptionsBuilder options)
       => options.UseSqlite($"Data Source={DbPath}"); 
}

public class LogEntry
{
    public int LogEntryId { get; set; }
    public string Tenant { get; set; }
    public string RecipientGroups { get; set; }
    public string LogType { get; set; }
    public string Project { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
