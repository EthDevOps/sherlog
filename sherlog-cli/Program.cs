using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Sharprompt;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace sherlog_cli;

class NetboxTenant
{
    public string Name { get; set; }
    public int Id { get; set; }
}

public class Config
{
    public string Tenant { get; set; }
    public string Project { get; set; }
}

class Program
{
    public static string? FindSherlogFile()
    {
        // Start at the current directory
        string currentDir = Directory.GetCurrentDirectory();

        // Traverse up the directory tree
        while (true)
        {
            string? filePath = Path.Combine(currentDir, ".sherlog");

            // Check if the .sherlog file exists in the current directory
            if (File.Exists(filePath))
            {
                return filePath; // Return the full path to the file
            }

            // Move up to the parent directory
            DirectoryInfo parentDir = Directory.GetParent(currentDir);
            if (parentDir == null)
            {
                break; // No more parent directories, stop the loop
            }
            currentDir = parentDir.FullName;
        }

        // Return null if the file was not found
        return null;
    }
    
    private static string ReadFileIgnoringComments(string filePath)
    {
        StringBuilder contentBuilder = new();
        try
        {
            using StreamReader reader = new(filePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Check if the line starts with '#'
                if (!line.TrimStart().StartsWith("#"))
                {
                    // Process the line as it does not start with '#'
                    contentBuilder.AppendLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
        return contentBuilder.ToString();
    }
 
    
    static async Task<int> Main(string[] args)
    {
        string sherlogServer = Environment.GetEnvironmentVariable("SHERLOG_SERVER") ?? "http://localhost:5062";
        Console.WriteLine("\n\n======[ Sherlog CLI ]======\n");
   
        // look for sherlog config
        string tenant;
        string project;
        string? sherlogPath = FindSherlogFile();
  
        // Query for tenants
        HttpClient hc = new();
        hc.BaseAddress = new Uri(sherlogServer);

        List<string> tenants = [];
        try
        {
            string respJson = await hc.GetStringAsync("/api/logbook/get-tenants");
            JsonNode? jtenents = JsonNode.Parse(respJson);

            foreach (JsonNode? jtenent in jtenents as JsonArray)
            {
                tenants.Add(jtenent.GetValue<string>());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to talk to Sherlog Server: {ex.Message}");
            return -1;
        }


        if (string.IsNullOrEmpty(sherlogPath))
        {
            tenant = Prompt.Select("Select Tenant", tenants);
            project = Prompt.Input<string>("Log message for which project");
        }
        else
        {
            Console.WriteLine($"Using configuration from: {sherlogPath}");
            string configContents = File.ReadAllText(sherlogPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
            var config = deserializer.Deserialize<Config>(configContents);
            tenant = config.Tenant;
            project = config.Project;
            Console.WriteLine($"Tenant (from config): {tenant}");
            if (!tenants.Contains(tenant))
            {
                Console.WriteLine($"Tenant {tenant} unknown. Please select:");
                tenant = Prompt.Select("Select Tenant", tenants);
            }
        }
        
        // TODO: Get recipient groups

        List<string> recipients = [];
        try
        {
            string respJson = await hc.GetStringAsync($"/api/logbook/get-tenant-roles/{tenant}");
            JsonNode? jroles = JsonNode.Parse(respJson);
            foreach (JsonNode? jrole in jroles as JsonArray)
            {
                recipients.Add(jrole.GetValue<string>());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to talk to Sherlog Server: {ex.Message}");
            return -1;
        }

        Console.Write("\n");
        var selectedRecipients = Prompt.MultiSelect("Select stakeholders to be notified on this log entry?", recipients, pageSize: 3).ToList();
        
        List<string> types = ["Update notification", "Incident started", "Incident updated", "Incident resolved"];
        string logtype = Prompt.Select("Select Logtype", types);
        
        // Prepare not
        string note;
        string tempFile = Path.GetTempFileName();
        
        await File.WriteAllTextAsync(tempFile, "# Please add your log entry here.");
    
        string editor = Environment.GetEnvironmentVariable("EDITOR") ?? "nano";
        try
        {
            // Start the editor with the temporary file
            Process editorProcess = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = editor,
                    Arguments = tempFile,
                    UseShellExecute = true
                }
            };

            editorProcess.Start();
            await editorProcess.WaitForExitAsync(); // Wait for the editor to close

            // Read the content of the temporary file
            note = ReadFileIgnoringComments(tempFile);
        }
        finally
        {
            // Clean up the temporary file
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

      
        // make a summary
        Console.WriteLine("\n\n=== Summary ===\n");
        Console.WriteLine($"    Tenant: {tenant}");
        Console.WriteLine($"   Project: {project}");
        Console.WriteLine($"  Log type: {logtype}");
        Console.WriteLine($"Recipients: {string.Join(", ", selectedRecipients)}");
        Console.WriteLine("\nNote:");
        Console.WriteLine(note);
        bool confirm = Prompt.Confirm("Is this correct?");

        if (confirm)
        {
            if (string.IsNullOrEmpty(note))
            {
                Console.WriteLine("Note empty. Aborting.");
                return -1;
            }
            
            Console.Write("Creating log entry and sending notification...");

            // TODO : Send shit
            try
            {
                await hc.PostAsJsonAsync("/api/logbook/add-log-entry", new LogEntryDto
                {
                    Tenant = tenant,
                    RecipientGroups = selectedRecipients.ToList(),
                    LogType = logtype,
                    Project = project,
                    Message = note
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to talk to Sherlog Server: {ex.Message}");
                return -1;
            }

            Console.WriteLine("done.");
            return 0;
        }
        else
        {
            Console.WriteLine("Abort.");
            return -1;
        }
    }
}