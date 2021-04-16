using MailKit.Net.Smtp;
using MimeKit;
using Scheduler.Common.Configuration;
using System;
using System.Threading.Tasks;
using ISmtpClient = Scheduler.Common.Security.ISmtpClient;

namespace Scheduler.Services.Implementations {

	public class SimpleSmtpClient : ISmtpClient {

		private readonly IConfigurationService m_configurationService;

		public SimpleSmtpClient ( IConfigurationService configurationService ) {
			m_configurationService = configurationService ?? throw new ArgumentNullException ( nameof ( configurationService ) );
		}

		public async Task SendEmail ( string to , string body , string subject ) {
			var smtpHost = await m_configurationService.GetValue ( "SmtpClient/Host" );
			var smtpPort = Convert.ToInt32 ( await m_configurationService.GetValue ( "SmtpClient/Port" ) );
			var fromAddress = await m_configurationService.GetValue ( "SmtpClient/UserName" );
			var password = await m_configurationService.GetValue ( "SmtpClient/Password" );
			var login = await m_configurationService.GetValue ( "SmtpClient/Login" );

			var message = new MimeMessage ();
			message.From.Add ( new MailboxAddress ( "Scheduler" , fromAddress ) );
			message.To.Add ( new MailboxAddress ( "New user" , to ) );
			message.Subject = subject;

			var bodyBuilder = new BodyBuilder ();
			bodyBuilder.HtmlBody = body;
			bodyBuilder.TextBody = "";

			message.Body = bodyBuilder.ToMessageBody ();

			using ( var client = new SmtpClient () ) {
				client.Connect ( smtpHost , smtpPort , true );

				client.Authenticate ( login , password );

				await client.SendAsync ( message );
				client.Disconnect ( true );
			}
		}

	}

}
