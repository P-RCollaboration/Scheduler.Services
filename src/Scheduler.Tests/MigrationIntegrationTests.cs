using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scheduler.Backend.PresentationClasses.Configurations;
using Scheduler.Backend.Services.Implementations;
using Scheduler.Common.Configuration;
using Scheduler.Common.DataContexts;
using Scheduler.Migrations.Migrators;
using Scheduler.Tests.PresentationClasses;
using SqlKata;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Scheduler.Tests {

	public class MigrationIntegrationTests {

		private (IServiceProvider, int) CreateMigrationIoC ( IDataContext dataContext ) {
			var serviceProvider = new ServiceCollection ();

			var migrationTypes = typeof ( ConventionMigrator ).Assembly.GetTypes ()
				.Where ( a => a.GetInterface ( "IMigration" ) != null )
				.ToList ();

			foreach ( var migrationType in migrationTypes ) {
				serviceProvider.AddTransient ( migrationType );
			}

			serviceProvider.AddSingleton ( ( serviceProvider ) => dataContext );

			return (serviceProvider.BuildServiceProvider (), migrationTypes.Count);
		}

		[Fact]
		[Trait ( "Category" , "Integration" )]
		public async Task Apply_Restore_Apply () {
			//arrange
			var optionsFake = A.Fake<IOptions<ProjectOptions>> ();
			A.CallTo ( () => optionsFake.Value ).Returns ( new ProjectOptions { ConsulUrl = "http://localhost:8500" } );

			var configurationFake = A.Fake<IConfigurationService> ();
			A.CallTo ( () => configurationFake.GetValue ( "ConnectionStrings/Postgres" ) ).Returns ( Task.FromResult ( "Server=localhost;Port=5432;Database=scheduler_migration;User Id=postgres;Password=postgres" ) );

			var dataContext = new SqlKataDataContext ( configurationFake );
			var (serviceProvider, countMigrations) = CreateMigrationIoC ( dataContext );
			var migrator = new ConventionMigrator ( dataContext , serviceProvider );

			//action
			await migrator.RevertAll ();

			//assert
			var migrationTable = await dataContext.GetItemAsync<dynamic> (
				new Query ( "information_schema.tables" )
					.Where ( "table_schema" , "public" )
					.Where ( "table_name" , "migrations" )
			);
			Assert.Null ( migrationTable );

			//action
			await migrator.Update ();

			//assert
			var allMigrations = await dataContext.GetItemAsync<MigrationsCount> (
				new Query ( "migrations" )
					.SelectRaw ( "COUNT(*) AS count" )
			);
			Assert.Equal ( countMigrations , allMigrations.Count );

			//action
			await migrator.RevertAll ();

			//assert
			var migrationTablePart2 = await dataContext.GetItemAsync<dynamic> (
				new Query ( "information_schema.tables" )
					.Where ( "table_schema" , "public" )
					.Where ( "table_name" , "migrations" )
			);
			Assert.Null ( migrationTablePart2 );

			//action
			await migrator.Update ();

			//assert
			var allMigrationsPart2 = await dataContext.GetItemAsync<MigrationsCount> (
				new Query ( "migrations" )
					.SelectRaw ( "COUNT(*) AS count" )
			);
			Assert.Equal ( countMigrations , allMigrationsPart2.Count );
		}

	}

}
