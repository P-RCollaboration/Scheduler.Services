using System.Threading.Tasks;

namespace Scheduler.Migrations {

	/// <summary>
	/// Migration for database.
	/// </summary>
	public interface IMigration {

		/// <summary>
		/// Description for migration.
		/// </summary>
		string Description {
			get;
		}

		/// <summary>
		/// Apply migration.
		/// </summary>
		Task Up ();

		/// <summary>
		/// Backup migration.
		/// </summary>
		Task Down ();

	}

}
