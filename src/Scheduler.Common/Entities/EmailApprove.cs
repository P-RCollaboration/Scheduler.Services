using System;

namespace Scheduler.Common.Entities {

	public class EmailApprove {

		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public string ApproveId { get; set; } = "";

	}

}
