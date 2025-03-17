using Elsa.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.EntityFrameworkCore.Extensions;





var builder = WebApplication.CreateBuilder(args);

var elsaServerUrl = builder.Configuration.GetValue<string>("Elsa:Server:BaseUrl");

// Elsa v3 builder.Services
builder.Services.AddElsa(elsa =>
{
    // Replace EntityFrameworkPersistence with new persistence
    // Configure Management layer to use EF Core.
    elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef => ef.UseSqlite()));

    // Configure Runtime layer to use EF Core.
    elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef => ef.UseSqlite()));

    // Add activities - note API changes in v3

    elsa.UseHttp();
    elsa.UseWorkflowsApi();
    // Add temporal activities (previously Quartz)
    elsa.UseScheduling();

    // Add workflows from assembly
    elsa.AddWorkflowsFrom<Program>();
});

// API endpoints are configured differently in v3

builder.Services.AddRazorPages();


// Elsa 3.3.4 builder.Services configuration


var app = builder.Build();

app.UseWorkflowsApi();



app.Run();
