namespace Spendly.Shared.ViewModels.Settings;

public class AwsSettings
{

	public required string AccessKeyId { get; set; }
	public required string SecretAccessKey { get; set; }
	public required string EmailQueueUrl { get; set; }
	public required string InvoiceQueueUrl { get; set; }
	public required string Region { get; set; }
	public required string PaymentServiceLambda { get; set; }
	public required string QrCodeQueueUrl { get; set; }
	public required string S3BuckerUrl { get; set; }
	public required string S3BucketName { get; set; }
}
