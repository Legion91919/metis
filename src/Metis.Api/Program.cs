using Metis.Api;

var builder = WebApplication.CreateBuilder(args);

// Lending criteria live in their own file so the head of credit can change weights without a deploy.
// reloadOnChange + IOptionsMonitor: the next request uses the new weights. Override the path with CriteriaFile.
builder.Configuration.AddJsonFile(builder.Configuration["CriteriaFile"] ?? "criteria.json", optional: false, reloadOnChange: true);

AppComposition.ConfigureServices(builder);
var app = builder.Build();
AppComposition.Configure(app);
app.Run();

/// <summary>Entry point (public for test hosts).</summary>
public partial class Program;
