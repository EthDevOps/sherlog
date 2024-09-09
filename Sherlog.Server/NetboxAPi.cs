using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Sherlog.Server;

public class NetboxAPi
{
    private readonly HttpClient _client;

    public NetboxAPi(string netboxUrl, string netboxApiKey)
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(netboxUrl);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", netboxApiKey);
    }
    
    public async Task<List<NetboxTenant>> GetNetboxTenants()
    {
        string jsonResponse = await _client.GetStringAsync("/api/tenancy/tenants/");
        JsonNode? tenents = JsonNode.Parse(jsonResponse);
        List<NetboxTenant> tenants = new();
        foreach (var tenant in tenents["results"] as JsonArray)
        {
            tenants.Add(new NetboxTenant
            {
                Slug = tenant["slug"].GetValue<string>(),
                Id = tenant["id"].GetValue<int>()
            });
        }
        return tenants;
    }

    public async Task<List<string>> GetContactRolesForTenant(string slug)
    {
        var tenants = await GetNetboxTenants();
        var tenantId = tenants.FirstOrDefault(x => x.Slug == slug)?.Id;
       
        string jsonResponse = await _client.GetStringAsync($"/api/tenancy/contact-assignments/?content_type=tenancy.tenant&object_id={tenantId}");
        JsonNode? assignment = JsonNode.Parse(jsonResponse);
       
        // get roles
        List<string> roles = new(); 
        foreach (var a in assignment["results"] as JsonArray)
        {
            roles.Add(a["role"]["slug"].GetValue<string>());
        }
       
        return roles.Distinct().ToList();
       
    }

    public async Task<IEnumerable<Contact>> GetContactAssignmentsForTenents(string logEntryTenant, string group)
    {
        // Get tenant id
        var tenants = await GetNetboxTenants();
        var tenantId = tenants.FirstOrDefault(x => x.Slug == logEntryTenant)?.Id;
      
        // Get assignments
        string jsonResponse = await _client.GetStringAsync($"/api/tenancy/contact-assignments/?content_type=tenancy.tenant&object_id={tenantId}");
        JsonNode? assignment = JsonNode.Parse(jsonResponse);
       
        // get roles
        List<Contact> contacts = new(); 
        foreach (var a in assignment["results"] as JsonArray)
        {
            if(a["role"]["slug"].GetValue<string>() != group) { continue; }

            int contactId = a["contact"]["id"].GetValue<int>();
            string jrContact = await _client.GetStringAsync($"/api/tenancy/contacts/{contactId}/");
            JsonNode? contact = JsonNode.Parse(jrContact);
            
            contacts.Add(new Contact()
            {
                Name = a["contact"]["name"].GetValue<string>(),
                Email = contact["email"].GetValue<string>()
            });
            
        }
       
        return contacts;
       
    }
}