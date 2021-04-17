using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scheduler.Backend.PresentationClasses.Configurations;
using Scheduler.Backend.Services.Implementations;
using Scheduler.Common.Configuration;
using Scheduler.Common.DataContexts;
using Scheduler.Migrations.Migrators;
using SqlKata;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior ( DisableTestParallelization = true )]

namespace Scheduler.Tests {

	public class ConventionMigratorIntegrationTests {

		private IServiceProvider CreateMigrationIoC ( IDataContext dataContext ) {
			var serviceProvider = new ServiceCollection ();

			var migrationTypes = typeof ( ConventionMigrator ).Assembly.GetTypes ()
				.Where ( a => a.GetInterface ( "IMigration" ) != null )
				.ToList ();

			foreach ( var migrationType in migrationTypes ) {
				serviceProvider.AddTransient ( migrationType );
			}

			serviceProvider.AddSingleton ( ( serviceProvider ) => dataContext );

			return serviceProvider.BuildServiceProvider ();
		}

		[Fact]
		[Trait ( "Category" , "Integration" )]
		public async Task Update_FromZero () {
			//arrange
			var optionsFake = A.Fake<IOptions<ProjectOptions>> ();
			A.CallTo ( () => optionsFake.Value ).Returns ( new ProjectOptions { ConsulUrl = "http://localhost:8500" } );

			var configurationFake = A.Fake<IConfigurationService> ();
			A.CallTo ( () => configurationFake.GetValue ( "ConnectionStrings/Postgres" ) ).Returns ( Task.FromResult ( "Server=localhost;Port=5432;Database=scheduler_migration;User Id=postgres;Password=postgres" ) );

			var dataContext = new SqlKataDataContext ( configurationFake );
			var migrator = new ConventionMigrator ( dataContext , CreateMigrationIoC ( dataContext ) );
			await migrator.RevertAll ();

			//action
			await migrator.Update ();

			//assert
			var migrationTable = await dataContext.GetItemAsync<dynamic> (
				new Query ( "information_schema.tables" )
					.Where ( "table_schema" , "public" )
					.Where ( "table_name" , "migrations" )
			);
			Assert.NotNull ( migrationTable );
		}

		[Fact]
		[Trait ( "Category" , "Integration" )]
		public async Task Update_WithAlreadyApplied () {
			//arrange
			var optionsFake = A.Fake<IOptions<ProjectOptions>> ();
			A.CallTo ( () => optionsFake.Value ).Returns ( new ProjectOptions { ConsulUrl = "http://localhost:8500" } );

			var configurationFake = A.Fake<IConfigurationService> ();
			A.CallTo ( () => configurationFake.GetValue ( "ConnectionStrings/Postgres" ) ).Returns ( Task.FromResult ( "Server=localhost;Port=5432;Database=scheduler_migration;User Id=postgres;Password=postgres" ) );

			var dataContext = new SqlKataDataContext ( configurationFake );
			var migrator = new ConventionMigrator ( dataContext , CreateMigrationIoC ( dataContext ) );
			await migrator.RevertAll ();

			//action
			await migrator.Update ();

			var firstMigrations = await dataContext.GetItemAsync<dynamic> (
				new Query ( "migrations" )
					.SelectRaw ( "COUNT(*) AS count" )
			);

			await migrator.Update ();

			var secondMigrations = await dataContext.GetItemAsync<dynamic> (
				new Query ( "migrations" )
					.SelectRaw ( "COUNT(*) AS count" )
			);

			//assert
			Assert.Same ( firstMigrations.Count , secondMigrations.Count );
		}

	}

}
