using Perform.Factories;
using Perform.Reaper;
using Perform.Web;
using System.Diagnostics;
using Perform.Data;
using Perform.UI24R;
using Perform.DMX;
using Perform.MidiFootPedal;

// Prevent the machine from going to sleep
Caffeine.PreventSleep();

var builder = WebApplication.CreateBuilder(args);

var processName = "Reaper";

// Find all processes with the given name
var processes = Process.GetProcessesByName(processName);

if (processes.Length == 0)
{
    var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "REAPER (x64)");
    var reaperProject = Path.Combine(Directory.GetCurrentDirectory(), "Settings/Reaper/Live.RPP");
    var process = Process.Start(new ProcessStartInfo
    {
        FileName = Path.Combine(folderPath, "Reaper.exe"),
        Arguments = $"-ignoreerrors -nosplash -close:exit {reaperProject}",
        UseShellExecute = true,
        CreateNoWindow = false,
        WorkingDirectory = folderPath
    });

    if (process == null)
    {
        throw new Exception("Failed to start Reaper. Please ensure it is installed and accessible.");
    }

    // Wait for the main window to be created (timeout after 30 seconds)
    var timeout = TimeSpan.FromSeconds(30);
    var sw = Stopwatch.StartNew();
    while (process.MainWindowHandle == IntPtr.Zero)
    {
        if (process.HasExited)
            throw new Exception("Reaper exited unexpectedly while waiting for readiness.");

        if (sw.Elapsed > timeout)
            throw new TimeoutException("Timed out waiting for Reaper to be ready.");

        Thread.Sleep(200); // Wait a bit before checking again
        process.Refresh(); // Update process info
    }
}

// Configure Kestrel to listen on port 80 and bind to all IP addresses
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // Listen on port 80 for all IP addresses
});

// Add services to the container.
builder.Services.AddSingleton<IDeviceFactory, DmxControllerFactory>();
builder.Services.AddSingleton<IDeviceFactory, MidiFootPedalFactory>();
builder.Services.AddSingleton<IDeviceFactory, ReaperFactory>();
builder.Services.AddSingleton<IDeviceFactory, UI24RFactory>();
builder.Services.AddSingleton<DeviceFactory>();
builder.Services.AddSingleton<ShowService>();

builder.Services.AddMediatR(options =>
{
    options.RegisterServicesFromAssemblyContaining<ShowService>();
});

// Repositories
builder.Services.AddRepositories();

var app = builder.Build();

app.UseDefaultFiles(
    new DefaultFilesOptions
    {
        DefaultFileNames = new List<string> { "default.html" }
    });

app.UseStaticFiles();
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var showService = context.RequestServices.GetRequiredService<ShowService>();
            await showService.WebSocketHandler(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

app.Run();