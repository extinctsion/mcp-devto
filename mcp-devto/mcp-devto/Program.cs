using mcp_devto.ToolType;
using Service;

var builder = Host.CreateApplicationBuilder(args);

// Add services to the container.
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton<IDevToService, DevToService>();
builder.Services.AddSingleton<DevTools>();

await builder.Build().RunAsync();
