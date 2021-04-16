using System;
using System.Threading.Tasks;

namespace Scheduler.Common.Security {

	/// <summary>
	/// After the registration process need approve account email sending mail to the user address.
	/// </summary>
	public interface IEmailAccountApprove {

		Task SendApproveEmail ( Guid userId );

		Task<bool> VerifyApproveIdFromEmail ( string approveId );

	}

}
