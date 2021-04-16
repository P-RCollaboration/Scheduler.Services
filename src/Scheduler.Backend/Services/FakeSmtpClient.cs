using System.Threading.Tasks;
using ISmtpClient = Scheduler.Common.Security.ISmtpClient;

namespace Scheduler.Services.Implementations {

	/// <summary>
	/// Fake smtp client.
	/// </summary>
	public class FakeSmtpClient : ISmtpClient {

		public Task SendEmail ( string to , string body , string subject ) {
			return Task.CompletedTask;
		}

	}

}
