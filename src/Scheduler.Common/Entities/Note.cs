using System;

namespace Scheduler.Common.Entities {

	public class Note {

		public Guid Id { get; set; }

		public string Title { get; set; }

		public string Body { get; set; }

		public Guid UserId { get; set; }

	}

}
