using System;

namespace Scheduler.Common.Entities {

	public class Token {

		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public DateTime Logined { get; set; }

	}

}
