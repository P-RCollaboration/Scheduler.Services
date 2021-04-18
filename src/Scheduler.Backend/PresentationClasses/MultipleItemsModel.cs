using System.Collections.Generic;
using System.Linq;

namespace Scheduler.Backend.PresentationClasses {

	/// <summary>
	/// Multiple items model.
	/// </summary>
	public record MultipleItemsModel<T> {

		public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T> ();

		public string Message { get; set; } = "";

	}

}
