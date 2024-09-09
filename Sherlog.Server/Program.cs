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