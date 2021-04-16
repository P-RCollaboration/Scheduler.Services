using System;

namespace Scheduler.Common.Entities {

	public class User {

		public Guid Id { get; set; }

		public string Email { get; set; } = "";

		public string Login { get; set; } = "";

		public string Password { get; set; } = "";

		public bool IsEmailApproved { get; set; }

	}

}
