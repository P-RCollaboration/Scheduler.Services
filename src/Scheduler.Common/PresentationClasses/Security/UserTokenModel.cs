using System;

namespace Scheduler.Common.PresentationClasses.Security {

	/// <summary>
	/// Model for store user's tokens.
	/// </summary>
	public class UserTokenModel {

		public Guid UserId { get; set; }

		public DateTime Logined { get; set; }

	}

}
