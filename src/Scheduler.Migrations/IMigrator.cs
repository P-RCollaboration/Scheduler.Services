using System.Threading.Tasks;

namespace Scheduler.Migrations {

	/// <summary>
	/// Migrator.
	/// </summary>
	public interface IMigrator {

		/// <summary>
		/// Apply all not applied migration to database.
		/// </summary>
		/// <returns></returns>
		Task Update ();

		/// <summary>
		/// Revert database to concrete migration.
		/// </summary>
		/// <param name="migrationName">Revert to this migration.</param>
		/// <returns></returns>
		Task RevertToMigration ( string migrationName );

		/// <summary>
		/// Revert all migrations and restore database to initial state.
		/// </summary>
		Task RevertAll ();

	}

}
