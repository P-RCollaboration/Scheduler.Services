using System.Collections.Generic;
using System.Linq;

namespace Scheduler.Common.Configuration {

	/// <summary>
	/// Model for read from local settings file.
	/// </summary>
	public class SettingsItemModel {

		/// <summary>
		/// Name of item.
		/// </summary>
		public string Name { get; set; } = "";

		/// <summary>
		/// Value.
		/// </summary>
		public string Value { get; set; } = "";

		/// <summary>
		/// Children items.
		/// </summary>
		public IEnumerable<SettingsItemModel> Items { get; set; } = Enumerable.Empty<SettingsItemModel> ();

	}

}
