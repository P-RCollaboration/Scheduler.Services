using System.Threading.Tasks;

namespace Scheduler.Common.Security {

	public interface ISmtpClient {

		Task SendEmail ( string to , string body , string subject );

	}

}
