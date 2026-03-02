// CHANGED_BY_AI: 2026-02-28 - Added JWT authentication and HttpClient registration
// using Amazon;
// using Amazon.Lambda;
// using Amazon.Runtime;
// using Amazon.SQS;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Spendly.Mobile.Api.Infrastructure;
using Spendly.Mobile.Api.Infrastructure.Aop;
using Spendly.Mobile.BusinessLayer.Services.Auth;
using Spendly.Shared.Core.Bootstrap;
using Spendly.Shared.Core.Interception;
using Spendly.Shared.Entities.LocaleManagement;
using Spendly.Shared.Localization;
using Spendly.Shared.ViewModels.Settings;

var app = AppBootstrapper
	.Create("Spendly.Mobile.Api", args)
	.WithServiceScanning([
		typeof(Constants).Assembly,
		typeof(AuthService).Assembly,
		typeof(TranslationService).Assembly
	])
	.ConfigureServices((services, config) =>
	{
		// ----------------------------
		// DI / business dependencies, AWS, validators, controllers, filters, etc.
		// ----------------------------

		services.AddTransient<MethodLoggingInterceptor>();
		services.AddTransient<CacheableMethodInterceptor>();
		services.AddMemoryCache();
		services.AddHttpContextAccessor();
		services.AddReportRepositories(config);
		// services.AddSingleton<IAmazonSQS>(sp =>
		// {
		// 	var awsSettings = sp.GetRequiredService<AwsSettings>();
		// 	var credentials = new BasicAWSCredentials(awsSettings.AccessKeyId, awsSettings.SecretAccessKey);
		// 	var region = RegionEndpoint.GetBySystemName(awsSettings.Region);
		// 	return new AmazonSQSClient(credentials, region);
		// });
		
		// Changed to scoped so it can consume scoped RequestContextViewModel
		services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

		services.AddHttpClient();

		var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>()!;
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = jwtSettings.Issuer,
					ValidateAudience = true,
					ValidAudience = jwtSettings.Audience,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
					ClockSkew = TimeSpan.Zero
				};
			});
		services.AddAuthorization();
		
		// Register IAmazonLambda using AwsSettings for Payment services
		// services.AddSingleton<IAmazonLambda>(sp =>
		// {
		// 	var awsSettings = sp.GetRequiredService<AwsSettings>();
		// 	var credentials = new BasicAWSCredentials(awsSettings.AccessKeyId, awsSettings.SecretAccessKey);
		// 	var region = RegionEndpoint.GetBySystemName(awsSettings.Region);
		// 	return new AmazonLambdaClient(credentials, region);
		// });
	})
	.ConfigureCors((services, _) =>
	{
		services.AddCors(options =>
		{
			options.AddPolicy("MobileClient", policy =>
			{
				policy.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod();
			});
		});
	})
	.BuildWebApi(configureBuilder: builder =>
	{
		builder.Host.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
		builder.Services.ConfigureDynamicProxy(config =>
		{
			config.Interceptors.AddTyped<MethodLoggingInterceptor>();
			config.Interceptors.AddTyped<CacheableMethodInterceptor>();
		});
		
		builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
		{
			options.Filters.Add<LoggingActionFilter>();
			options.Filters.Add<CacheControlHeaderFilter>();
			options.Filters.Add<RequestContextFilter>();
		});
		// Ensure FluentValidation runs for API models and discover validators in API assembly
		builder.Services.AddFluentValidationAutoValidation();
		builder.Services.AddValidatorsFromAssemblyContaining<Program>();
		builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = ControllerExtensions.InvalidModelStateResponse;
        });
		
	});

// ----------------------------
// Middleware pipeline
// ----------------------------

// Localization middleware
var supportedCultures = LocaleData.Languages.Select(p => p.Code).ToList();
app.UseRequestLocalization(new RequestLocalizationOptions
{
	DefaultRequestCulture = new RequestCulture("en"),
	SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),
	SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
});

// Enable CORS for mobile client before authentication/authorization/endpoints
app.UseCors("MobileClient");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Add request session middleware early so SessionId is available to logging
app.UseMiddleware<RequestSessionMiddleware>();
app.UseMiddleware<SerilogContextEnricherMiddleware>();

// Serilog request logging
app.UseSerilogRequestLogging(opts =>
{
	opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
	{
		var sid = RequestSession.Get(httpCtx);
		if (!string.IsNullOrEmpty(sid))
		{
			diagCtx.Set("SessionId", sid);
		}
	};

});


app.Run();
