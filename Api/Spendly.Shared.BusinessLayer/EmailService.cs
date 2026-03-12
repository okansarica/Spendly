using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace Spendly.Shared.BusinessLayer;

using Microsoft.Extensions.Logging;
using ViewModels.Settings;

public class EmailService(IAmazonSimpleEmailService sesClient, EmailSettings  emailSettings, Logger<EmailService> logger)
{

	public async Task<bool> SendEmailAsync(string toAddress, string subject, string bodyHtml)
	{
		var sendRequest = new SendEmailRequest
		{
			Source = emailSettings.From,
			Destination = new Destination
			{
				ToAddresses = new List<string> { toAddress }
			},
			Message = new Message
			{
				Subject = new Content(subject),
				Body = new Body
				{
					Html = new Content
					{
						Charset = "UTF-8",
						Data = bodyHtml
					}
				}
			}
		};

		var response = await sesClient.SendEmailAsync(sendRequest);
		return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
	}

	public async Task SendAlarmEmailAsync(string bodyHtml)
	{
		try
		{
			await SendEmailAsync("okansarica@gmail.com","SPENDLY ALARM",bodyHtml);
		}
		catch (Exception e)
		{
			logger.LogError(e,"Can not send alarm email");
		}
		
	}
	
	public async Task SendAlarmEmailAsync(string message, Exception exception)
	{
		try
		{
			var body = $"{message} {exception.Message} {exception.StackTrace}, {exception.InnerException?.Message } {exception.InnerException?.StackTrace}";
			
			await SendEmailAsync("okansarica@gmail.com","SPENDLY ALARM",body);
		}
		catch (Exception e)
		{
			logger.LogError(e,"Can not send alarm email");
		}
		
	}
}


