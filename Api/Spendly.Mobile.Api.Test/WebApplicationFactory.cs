using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mongo2Go;
using MongoDB.Driver;
using Spendly.Shared.ViewModels.Settings;

namespace Spendly.Mobile.Api.Test;

public class WebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly MongoDbRunner _mongoRunner;

    public WebApplicationFactory()
    {
        _mongoRunner = MongoDbRunner.Start(singleNodeReplSet: false);
        var mongoUrl = new MongoUrl(_mongoRunner.ConnectionString);
        var port = mongoUrl.Server?.Port ?? 27017;
        var host = mongoUrl.Server?.Host ?? "localhost";

        Environment.SetEnvironmentVariable("TEST_MONGO_PORT", port.ToString());
        Environment.SetEnvironmentVariable("TEST_MONGO_HOST", host);

        Environment.SetEnvironmentVariable("DbSettings__DatabaseName", "SpendlyTestDb");
        Environment.SetEnvironmentVariable("DbSettings__UserName", string.Empty);
        Environment.SetEnvironmentVariable("DbSettings__Password", string.Empty);

        Environment.SetEnvironmentVariable("LogDbSettings__ConnectionString", string.Empty);
        Environment.SetEnvironmentVariable("LogDbSettings__DatabaseName", "SpendlyTestLog");
        Environment.SetEnvironmentVariable("LogDbSettings__UserName", string.Empty);
        Environment.SetEnvironmentVariable("LogDbSettings__Password", string.Empty);

        Environment.SetEnvironmentVariable("ReportDbSettings__DatabaseName", "SpendlyTestReport");
        Environment.SetEnvironmentVariable("ReportDbSettings__UserName", string.Empty);
        Environment.SetEnvironmentVariable("ReportDbSettings__Password", string.Empty);

        Environment.SetEnvironmentVariable("LocalQueueDbSettings__DatabaseName", "SpendlyTestQueue");
        Environment.SetEnvironmentVariable("LocalQueueDbSettings__UserName", string.Empty);
        Environment.SetEnvironmentVariable("LocalQueueDbSettings__Password", string.Empty);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            var dict = new Dictionary<string, string?>
            {
                {"JwtSettings:SecretKey", "spendly-dev-secret-key-min-32-bytes-long!!"},
                {"JwtSettings:Issuer", "https://api.spendly.io"},
                {"JwtSettings:Audience", "spendly-mobile"},
                {"JwtSettings:RefreshRenewDays", "5"}
            };

            conf.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbSettings>();
            services.RemoveAll<LogDbSettings>();
            services.RemoveAll<ReportDbSettings>();
            services.RemoveAll<LocalQueueDbSettings>();

            services.AddSingleton(new DbSettings
            {
                DatabaseName = "SpendlyTestDb",
                UserName = string.Empty,
                Password = string.Empty
            });

            services.AddSingleton(new LogDbSettings
            {
                ConnectionString = string.Empty,
                DatabaseName = "SpendlyTestLog",
                UserName = string.Empty,
                Password = string.Empty
            });

            services.AddSingleton(new ReportDbSettings
            {
                DatabaseName = "SpendlyTestReport",
                UserName = string.Empty,
                Password = string.Empty
            });

            services.AddSingleton(new LocalQueueDbSettings
            {
                DatabaseName = "SpendlyTestQueue",
                UserName = string.Empty,
                Password = string.Empty
            });

            // Use NewtonsoftJson to avoid System.Text.Json PipeWriter incompatibility in TestServer
            services.AddControllers().AddNewtonsoftJson();
        });

        base.ConfigureWebHost(builder);
    }

    public new void Dispose()
    {
        base.Dispose();
        _mongoRunner.Dispose();
    }
}
