using System.Threading.Tasks;

namespace Scheduler.Common.Configuration {

	/// <summary>
	/// Service for manage project configuration.
	/// </summary>
	public interface IConfigurationService {

		/// <summary>
		/// Run initialization for configuration.
		/// </summary>
		/// <param name="settings">Settings as string.</param>
		Task Initialization (string settings);

		/// <summary>
		/// Get value for specified path.
		/// </summary>
		/// <param name="path">The path where need to get value.</param>
		/// <returns></returns>
		Task<string> GetValue ( string path );

	}

}
