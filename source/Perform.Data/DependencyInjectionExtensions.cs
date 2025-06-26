using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Perform.Interfaces;
using SQLitePCL;

namespace Perform.Data;

public static class DependencyInjectionExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        Batteries_V2.Init(); // Initialize SQLite native provider

        // Create SQLite connection (hard-coded as per Perform.TestApp\Program.cs)
        var dbPath = "C:\\dev\\perform\\data\\Perform.db";
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        // Register repositories with the connection
        //services.AddSingleton<IDeviceRepository>(new DeviceRepository(connection));
        services.AddSingleton<IDeviceItemRepository>(new DeviceItemRepository(connection));
        services.AddSingleton<IShowRepository>(new ShowRepository(connection));
        services.AddSingleton<ISongRepository>(new SongRepository(connection));
    }
}