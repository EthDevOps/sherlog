using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;

namespace Sherlog.Server;

[ApiController]
[Route("api/logbook")]
public class LogbookController : ControllerBase
{
    private readonly NetboxAPi _netboxApi;

    public LogbookController()
    {
       
        _netboxApi = new NetboxAPi("https://netbox.ethquokkaops.io", "60f53d1002164803b460228193b9e3b8");
    }
    [HttpPost("add-log-entry")]
    public async Task<ActionResult> AddLogEntry(LogEntryDto logEntry)
    {
        // Save the entry to database

        // Get all contacts
        List<Contact> contacts = new();
        foreach (var group in logEntry.RecipientGroups)
        {
            contacts.AddRange(await _netboxApi.GetContactAssignmentsForTenents(logEntry.Tenant, group));
        }        
        
        // Compose message
        string header = "This is a notification";
        if(logEntry.LogType == "Incident started")
            header = "Devops opened an incident";
        else if(logEntry.LogType == "Incident resolved")
            header = "Devops resolved an incident";
        else if(logEntry.LogType == "Incident updated")
            header = "Devops updated an incident";

        string subject = $"{header} for {logEntry.Tenant} - {logEntry.Project}";
        string message = 
            @$"{subject}:

{logEntry.Message}

--- You are receiving this message because you are listed as Stakeholder in the Service Manifest for {logEntry.Tenant} - {logEntry.Project} ---"; 
        
        
        // Send message to 
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(Program.Configuration.MailFrom);
        foreach (var contact in contacts)
        {
            mail.To.Add(contact.Email);
        }
        mail.CC.Add(Program.Configuration.MailFrom); 
        mail.Subject = subject;
        mail.Body = message;

        SmtpClient smtpServer = new SmtpClient(Program.Configuration.SmtpServer);
        smtpServer.Port = 587; // This is the default port for SMTP. Change it according to your server
        smtpServer.Credentials = new NetworkCredential(Program.Configuration.SmtpUser, Program.Configuration.SmtpPassword);
        smtpServer.EnableSsl = true;

        smtpServer.Send(mail);
        
        
        return CreatedAtAction(nameof(AddLogEntry), new { id = logEntry.Id }, logEntry);
    }

    [HttpGet("get-tenants")]
    public async Task<ActionResult> GetTenants()
    {
        var result = await _netboxApi.GetNetboxTenants();
        
        return new JsonResult(result.Select(x => x.Slug));
    }

    [HttpGet("get-tenant-roles/{tenantSlug}")]
    public async Task<ActionResult> GetContactRolesForTenant(string tenantSlug)
    {
        var result = await _netboxApi.GetContactRolesForTenant(slug: tenantSlug);
        
        return new JsonResult(result);
    }
}