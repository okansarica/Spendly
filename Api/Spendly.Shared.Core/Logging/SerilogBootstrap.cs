// filepath: /Users/okan/Documents/Software/Projects/SimplePay/SimplePay/SimplePay.Shared.Core/Logging/SerilogBootstrap.cs
namespace Spendly.Shared.Core.Logging;

using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;

public static class SerilogBootstrap
{
    /// <summary>
    /// Creates and assigns the global Serilog logger using common defaults and configuration.
    /// Adds Application property if provided. Automatically includes Console sink and any sinks defined in configuration.
    /// Optional override allows adding programmatic sinks (e.g., extra sinks) without hard dependency on sink packages.
    /// </summary>
    /// <param name="configuration">Application configuration (expects Serilog section).</param>
    /// <param name="applicationName">Application name added as an enrich property.</param>
    /// <param name="configure">Optional extra sink configuration callback (executed directly on LoggerConfiguration).</param>
    /// <param name="addMongoFromSimpleKeys">If true, will look for simple keys Serilog:MongoDB:ConnectionString, Database, Collection and add them via configuration overlay (no compile-time sink dep).</param>
    public static ILogger ConfigureGlobalLogger(
        IConfiguration configuration,
        string applicationName,
        Action<LoggerConfiguration>? configure = null,
        bool addMongoFromSimpleKeys = true)
    {
        // Optionally create an overlay configuration that injects a MongoDB sink using simple keys
        IConfiguration effectiveConfig = configuration;
        if (addMongoFromSimpleKeys)
        {
            var mongoConn = configuration["Serilog:MongoDB:ConnectionString"];
            var mongoDb = configuration["Serilog:MongoDB:Database"];
            var mongoCollection = configuration["Serilog:MongoDB:Collection"] ?? "logs";

            var serilogSection = configuration.GetSection("Serilog");
            var writeToSection = serilogSection.GetSection("WriteTo");
            var hasMongoInConfig = writeToSection.GetChildren().Any(c => string.Equals(c["Name"], "MongoDB", StringComparison.OrdinalIgnoreCase));

            if (!hasMongoInConfig && !string.IsNullOrWhiteSpace(mongoConn) && !string.IsNullOrWhiteSpace(mongoDb))
            {
                var existingCount = writeToSection.GetChildren().Count();
                var idx = existingCount; // append at the end

                var overlay = new Dictionary<string, string?>
                {
                    [$"Serilog:Using:0"] = "Serilog.Sinks.Console", // ensure Console available
                    [$"Serilog:Using:1"] = "Serilog.Sinks.MongoDB",
                    [$"Serilog:WriteTo:{idx}:Name"] = "MongoDB",
                    [$"Serilog:WriteTo:{idx}:Args:databaseUrl"] = mongoConn!.TrimEnd('/') + "/" + mongoDb,
                    [$"Serilog:WriteTo:{idx}:Args:collectionName"] = mongoCollection
                };

                effectiveConfig = new ConfigurationBuilder()
                    .AddConfiguration(configuration)
                    .AddInMemoryCollection(overlay)
                    .Build();
            }
        }

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(effectiveConfig)
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails();

        // Ensure console sink present if not defined in config
        var hasConsoleInConfig = effectiveConfig.GetSection("Serilog").GetSection("WriteTo").GetChildren()
            .Any(w => string.Equals(w["Name"], "Console", StringComparison.OrdinalIgnoreCase));
        if (!hasConsoleInConfig)
        {
            loggerConfig.WriteTo.Console();
        }

        // Allow caller to add programmatic sinks (when the package is referenced in the caller project)
        configure?.Invoke(loggerConfig);

        Log.Logger = loggerConfig.CreateLogger();
        return Log.Logger;
    }
}
