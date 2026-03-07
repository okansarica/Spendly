namespace Spendly.Shared.Core.Bootstrap;

using Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Serilog;
using System.IO.Compression;
using System.Reflection;
using System.Threading.RateLimiting;
using ViewModels.Settings;

/// <summary>
/// Console uygulamaları ve Web API'ler için ortak bootstrapper.
/// Başlatma, servis kayıtları ve log yapılandırmalarını yönetir.
/// </summary>
public class AppBootstrapper
{
	private readonly string _appName;
	private readonly string[] _args;
	private Action<IServiceCollection, IConfiguration>? _configureServices; // Ek servis konfigürasyonu için delegate
	private Action<LoggerConfiguration, IConfiguration>? _configureSerilog; // Ek log konfigürasyonu için delegate
	private Action<IServiceCollection, IConfiguration>? _configureCors;
	private Assembly[]? _assembliesToScan;
	private Func<Type, bool>? _serviceFilter;


	private AppBootstrapper(string appName, string[] args)
	{
		_appName = appName;
		_args = args;
	}

	/// <summary>
	/// Yeni bir AppBootstrapper oluştur
	/// </summary>
	public static AppBootstrapper Create(string appName, string[] args)
	{
		return new AppBootstrapper(appName, args);
	}

	/// <summary>
	/// Özel servisleri konfigüre et
	/// </summary>
	public AppBootstrapper ConfigureServices(Action<IServiceCollection, IConfiguration> configure)
	{
		_configureServices = configure;
		return this;
	}
	
	public AppBootstrapper ConfigureCors(Action<IServiceCollection, IConfiguration> configure)
	{
		_configureCors = configure;
		return this;
	}
	
	public AppBootstrapper WithServiceScanning(Assembly[] assemblies, Func<Type, bool>? filter = null)
	{
		_assembliesToScan = assemblies;
		_serviceFilter = filter;
		return this;
	}


	/// <summary>
	/// Serilog için ek log sink'lerini konfigüre et
	/// </summary>
	public AppBootstrapper ConfigureSerilog(Action<LoggerConfiguration, IConfiguration> configure)
	{
		_configureSerilog = configure;
		return this;
	}

	/// <summary>
	/// Console Job olarak çalıştır
	/// </summary>
	public async Task<int> RunAsJobAsync(Func<IServiceProvider, Task> jobLogic,
		Func<Type, bool>? serviceFilter = null)
	{
		try
		{
			var configuration = SetupConfigurationFiles();

			ConfigureLogging(configuration, "Job");
			
			try
			{
				BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
			}
			catch (MongoDB.Bson.BsonSerializationException)
			{
				// serializer already registered by another test run / hostthanks
			}

			Log.Information("Job Başlatılıyor: {AppName}", _appName);

			var services = new ServiceCollection();

			services.AddSettingsConfiguration(configuration);
			services.AddMongoRepositories(configuration);
			services.AddLocalQueueRepositories(configuration);

			if (_assembliesToScan != null)
			{
				// Varsayılan filtre
				var filter = serviceFilter ?? (t => t.Name.EndsWith("Service"));

				services.Scan(scan => scan
					.FromAssemblies(_assembliesToScan)
					.AddClasses(c => c.Where(filter))
					.AsSelf()
					.AsImplementedInterfaces()
					.WithScopedLifetime()
				);
			}

			_configureServices?.Invoke(services, configuration);

			var provider = services.BuildServiceProvider();

			await jobLogic(provider);

			Log.Information("Job tamamlandı: {AppName}", _appName);
			return 0;
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Job sırasında yakalanmamış hata: {AppName}", _appName);
			return 1;
		}
		finally
		{
			Log.Information("Uygulama kapanıyor: {AppName}", _appName);
			await Log.CloseAndFlushAsync();
		}
	}


	/// <summary>
	/// Web API olarak yapılandır ve döndür
	/// </summary>
	 public WebApplication BuildWebApi(string logPrefix = "Api",bool useMvcViews = false,
            Action<WebApplicationBuilder>? configureBuilder = null)
        {
            var builder = WebApplication.CreateBuilder(_args);

            // ---- CONFIG ----
            var customConfig = SetupConfigurationFiles();
            builder.Configuration.Sources.Clear();
            builder.Configuration.AddConfiguration(customConfig);
            
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (MongoDB.Bson.BsonSerializationException)
            {
                // serializer already registered by another host in same process
            }
            
            // ---- SERVICE SCANNING ----
            if (_assembliesToScan != null)
            {
	            var filter = _serviceFilter ?? (t => t.Name.EndsWith("Service"));

	            builder.Services.Scan(scan => scan
		            .FromAssemblies(_assembliesToScan)
		            .AddClasses(c => c.Where(filter))
		            .AsSelf()
		            .AsImplementedInterfaces()
		            .WithScopedLifetime()
	            );
            }

            // ---- LOGGING ----
            ConfigureLogging(builder.Configuration, logPrefix);
            builder.Host.UseSerilog();

	
            // ---- KESTREL ----
            builder.WebHost.ConfigureKestrel(k =>
            {
                k.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            });

            builder.Services.Configure<IISServerOptions>(opts =>
            {
                opts.MaxRequestBodySize = 10 * 1024 * 1024;
            });

            // ---- FORWARDED HEADERS ----
            builder.Services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
                opts.RequireHeaderSymmetry = false;
                opts.ForwardLimit = 2;
            });

            // ---- RESPONSE COMPRESSION (2.6) ----
            builder.Services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

            // ---- CACHE SERVICES ----
            builder.Services.AddResponseCaching();
            builder.Services.AddOutputCache();

            // ---- HEALTH CHECKS (2.13) ----
            builder.Services.AddHealthChecks();

            builder.Services.AddDataProtection();

            // ---- SETTINGS & SHARED REPOS ----
            builder.Services.AddSettingsConfiguration(builder.Configuration);
            builder.Services.AddMongoRepositories(builder.Configuration);
            builder.Services.AddLocalQueueRepositories(builder.Configuration);
            
            // ---- RATE LIMITER ----
            builder.Services.AddRateLimiter(options =>
            {
	            // Example configuration: 10 requests per second globally
	            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
		            RateLimitPartition.GetFixedWindowLimiter("GlobalLimiter", _ => new FixedWindowRateLimiterOptions
		            {
			            PermitLimit = 50,
			            Window = TimeSpan.FromSeconds(1),
			            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
			            QueueLimit = 2
		            }));
            });
            
            // ---- CONTROLLERS ----
            if (useMvcViews)
            {
	            builder.Services.AddControllersWithViews().AddJsonOptions(options =>
	            {
		            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
		            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(namingPolicy: null));
	            });
            }
            else
            {
	            builder.Services.AddControllers().AddJsonOptions(options =>
	            {
		            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
		            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(namingPolicy: null));
	            });
            }


            // CUSTOM SERVICE CONFIG HOOK
            _configureServices?.Invoke(builder.Services, builder.Configuration);

            configureBuilder?.Invoke(builder);
            _configureCors?.Invoke(builder.Services, builder.Configuration);

            // Build app and configure common middleware and endpoints
            var app = builder.Build();

            // ---- COMMON MIDDLEWARE MOVED FROM Program.cs ----
            // Rate Limiter 
            app.UseRateLimiter();


            // HealthCheck endpoint
            app.MapHealthChecks("/health");

            // Controllers & filters 
            app.MapControllers();
            

            return app;
        }

	// =======================================
	// PRIVATE HELPER METHODS
	// =======================================

	/// <summary>
	/// JSON ve environment variables ile IConfiguration oluşturur
	/// </summary>
	private IConfiguration SetupConfigurationFiles()
	{
		var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

		// AppContext.BaseDirectory -> /Api.X/bin/Debug/...
		// Try to locate solution root or a parent folder containing local.shared.json by walking up the directory tree
		string? FindLocalSharedJson()
		{
			var cur = AppContext.BaseDirectory;
			for (int i = 0; i < 10 && !string.IsNullOrEmpty(cur); i++)
			{
				var candidate = Path.Combine(cur, "shared.local.json");
				if (File.Exists(candidate))
					return candidate;

				var parent = Directory.GetParent(cur);
				if (parent == null)
					break;
				cur = parent.FullName;
			}

			// Also try current working directory as a last resort
			var cwdCandidate = Path.Combine(Directory.GetCurrentDirectory(), "shared.local.json");
			if (File.Exists(cwdCandidate))
				return cwdCandidate;

			return null;
		}

		var builder = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{env}.local.json", optional: true, reloadOnChange: true);

		var localSharedPath = FindLocalSharedJson();
		if (!string.IsNullOrEmpty(localSharedPath))
		{
			var sharedDir = Path.GetDirectoryName(localSharedPath)!;
			var sharedFile = Path.GetFileName(localSharedPath);
			// Use a physical provider so the shared DB settings file is read with change tracking support.
			builder.AddJsonFile(new PhysicalFileProvider(sharedDir), sharedFile, optional: true, reloadOnChange: true);
		}

		builder.AddEnvironmentVariables();

		return builder.Build();
	}



	/// <summary>
	/// Serilog'u MongoDB ile birlikte yapılandırır
	/// </summary>
	 private void ConfigureLogging(IConfiguration configuration, string prefix)
	{
		var logDbSettings = configuration.Get<LogDbSettings>()!;

		SerilogBootstrap.ConfigureGlobalLogger(configuration,
			_appName,
			loggerConfig =>
			{
				var mongoDatabase = logDbSettings.DatabaseName;
				var mongoCollection = $"{prefix}_{_appName.Replace(".", "_")}Log";

				if (!string.IsNullOrWhiteSpace(mongoDatabase))
				{
					var mongoConn = logDbSettings.ConnectionString?.TrimEnd('/');
					var databaseUrl = !string.IsNullOrWhiteSpace(mongoConn) ? mongoConn + "/" + mongoDatabase : mongoDatabase;

					loggerConfig.WriteTo.MongoDB(databaseUrl,
					collectionName: mongoCollection,
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug
					);
				}

				_configureSerilog?.Invoke(loggerConfig, configuration);
			});
	}

}
