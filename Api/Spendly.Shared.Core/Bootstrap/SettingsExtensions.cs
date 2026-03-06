// CHANGED_BY_AI: 2026-03-05 - Register Firebase settings for notification service
namespace Spendly.Shared.Core.Bootstrap;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spendly.Shared.ViewModels;
using Spendly.Shared.ViewModels.Settings;

public static class SettingsExtensions
{
	public static IServiceCollection AddSettingsConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		return services
				.Configure<LogDbSettings>(configuration.GetSection(nameof(LogDbSettings)))
				.AddSingleton<LogDbSettings>(sp => sp.GetRequiredService<IOptions<LogDbSettings>>().Value)
				.Configure<DbSettings>(configuration.GetSection(nameof(DbSettings)))
				.AddSingleton<DbSettings>(sp => sp.GetRequiredService<IOptions<DbSettings>>().Value)
				.Configure<ReportDbSettings>(configuration.GetSection(nameof(ReportDbSettings)))
				.AddSingleton<ReportDbSettings>(sp => sp.GetRequiredService<IOptions<ReportDbSettings>>().Value)
				.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)))
				.AddSingleton<EmailSettings>(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value)
				.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)))
				.AddSingleton<JwtSettings>(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value)
				.Configure<AwsSettings>(configuration.GetSection(nameof(AwsSettings)))
				.AddSingleton<AwsSettings>(sp => sp.GetRequiredService<IOptions<AwsSettings>>().Value)
				.Configure<LocalQueueDbSettings>(configuration.GetSection(nameof(LocalQueueDbSettings)))
				.AddSingleton<LocalQueueDbSettings>(sp => sp.GetRequiredService<IOptions<LocalQueueDbSettings>>().Value)
				// .Configure<FirebaseSettings>(configuration.GetSection(nameof(FirebaseSettings)))
				// .AddSingleton<IFirebaseSettings>(sp => sp.GetRequiredService<IOptions<FirebaseSettings>>().Value)
				// .Configure<GeneralSettings>(configuration.GetSection(nameof(GeneralSettings)))
				// .AddSingleton<IGeneralSettings>(sp => sp.GetRequiredService<IOptions<GeneralSettings>>().Value)
				// .Configure<SaferPaySettings>(configuration.GetSection(nameof(SaferPaySettings)))
				// .AddSingleton<ISaferPaySettings>(sp => sp.GetRequiredService<IOptions<SaferPaySettings>>().Value)
				// .Configure<HostedServiceSettings>(configuration.GetSection(nameof(HostedServiceSettings)))
				// .AddSingleton<IHostedServiceSettings>(sp => sp.GetRequiredService<IOptions<HostedServiceSettings>>().Value)
				// .Configure<QueueDbSettings>(configuration.GetSection(nameof(QueueDbSettings)))
				// .AddSingleton<IQueueDbSettings>(sp => sp.GetRequiredService<IOptions<QueueDbSettings>>().Value)
				.Configure<FacebookSettings>(configuration.GetSection(nameof(FacebookSettings)))
				.AddSingleton<FacebookSettings>(sp => sp.GetRequiredService<IOptions<FacebookSettings>>().Value)
				.Configure<StripeSettings>(configuration.GetSection(nameof(StripeSettings)))
				.AddSingleton<StripeSettings>(sp => sp.GetRequiredService<IOptions<StripeSettings>>().Value)
				.Configure<FirebaseSettings>(configuration.GetSection(nameof(FirebaseSettings)))
				.AddSingleton<FirebaseSettings>(sp => sp.GetRequiredService<IOptions<FirebaseSettings>>().Value)
				.Configure<VersionSettings>(configuration.GetSection(nameof(VersionSettings)))
				.AddSingleton<VersionSettings>(sp => sp.GetRequiredService<IOptions<VersionSettings>>().Value)
				.Configure<PlaidSettings>(configuration.GetSection(nameof(PlaidSettings)))
				.AddSingleton<PlaidSettings>(sp => sp.GetRequiredService<IOptions<PlaidSettings>>().Value)
			// .Configure<UploadSettings>(configuration.GetSection(nameof(UploadSettings)))
			// .AddSingleton<IUploadSettings>(sp => sp.GetRequiredService<IOptions<UploadSettings>>().Value)
			// .Configure<RestaurantAppAvailabilityDbSettings>(configuration.GetSection(nameof(RestaurantAppAvailabilityDbSettings)))
			// .AddSingleton<IRestaurantAppAvailabilityDbSettings>(sp => sp.GetRequiredService<IOptions<RestaurantAppAvailabilityDbSettings>>().Value);;
				// .Configure<PaypalSettings>(configuration.GetSection(nameof(PaypalSettings)))
				// .AddSingleton<PaypalSettings>(sp => sp.GetRequiredService<IOptions<PaypalSettings>>().Value)
				.AddScoped<RequestContextViewModel>()
			;
	}
}
