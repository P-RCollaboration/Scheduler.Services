using Scheduler.Common.DataContexts;
using Scheduler.Migrations.PresentationClasses;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler.Migrations.Migrators {

	/// <summary>
	/// Convention migrator.
	/// </summary>
	public class ConventionMigrator : IMigrator {

		private readonly IEnumerable<IMigration> m_Migrations;

		private readonly IDataContext m_DataContext;

		private readonly IServiceProvider m_ServiceProvider;

		/// <summary>
		/// Create new instance <see cref="ConventionMigrator"/>.
		/// </summary>
		/// <param name="dataContext">Data context.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ConventionMigrator ( IDataContext dataContext , IServiceProvider serviceProvider ) {
			m_DataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );
			m_ServiceProvider = serviceProvider ?? throw new ArgumentNullException ( nameof ( serviceProvider ) );

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			m_Migrations = GetType ().Assembly.GetTypes ()
				.Where ( a => a.GetInterface ( "IMigration" ) != null )
				.OrderBy (
					a => {
						var name = a.Name;
						var index = name.LastIndexOf ( "_" ) + 1;
						return Convert.ToInt32 ( name.Substring ( index , name.Length - index ) );
					}
				)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
			.Select ( a => (IMigration) serviceProvider.GetService ( a ) )
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
			.ToList ();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
		}

		public async Task RevertAll () {
			if ( !await CheckIfMigrationTableExists () ) return;

			var appliedMigrations = await m_DataContext.GetItemsAsync<MigrationEntity> (
				new Query ( "migrations" )
			);

			var revertingMigrations = m_Migrations
				.Where ( a => appliedMigrations.Any ( b => b.Id == a.GetType ().Name ) )
				.ToList ();
			revertingMigrations.Reverse ();

			foreach ( var revertingMigration in revertingMigrations ) await revertingMigration.Down ();
		}

		public Task RevertToMigration ( string migrationName ) {
			throw new NotImplementedException ();
		}

		public async Task Update () {
			var newMigrations = Enumerable.Empty<IMigration> ();
			if ( !await CheckIfMigrationTableExists () ) {
				newMigrations = m_Migrations;
			} else {
				var appliedMigrations = await m_DataContext.GetItemsAsync<MigrationEntity> (
					new Query ( "migrations" )
				);

				newMigrations = m_Migrations
					.Where ( a => !appliedMigrations.Any ( b => b.Id == a.GetType ().Name ) )
					.ToList ();
			}

			foreach ( var newMigration in newMigrations ) await newMigration.Up ();
		}

		private async Task<bool> CheckIfMigrationTableExists () {
			var migrationTable = await m_DataContext.GetItemAsync<dynamic> (
				new Query ( "information_schema.tables" )
					.Where ( "table_schema" , "public" )
					.Where ( "table_name" , "migrations" )
			);

			return migrationTable != null;
		}

	}

}
