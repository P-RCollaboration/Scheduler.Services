using FakeItEasy;
using Microsoft.Extensions.Options;
using Scheduler.Backend.PresentationClasses.Configurations;
using Scheduler.Backend.Services.Implementations;
using System.Threading.Tasks;
using Xunit;

namespace Scheduler.Tests {

	public class ConsulConfigurationServiceIntegrationTests {
		
		[Fact]
		[Trait ( "Category" , "Integration" )]
		public async Task Initialization_HappyPath () {
			//arrange
			var optionsFake = A.Fake<IOptions<ProjectOptions>> ();
			A.CallTo ( () => optionsFake.Value ).Returns ( new ProjectOptions { ConsulUrl = "http://localhost:8500" } );
			
			var consulConfigurationService = new ConsulConfigurationService ( optionsFake );
			await consulConfigurationService.DeleteValue ( "tester/muher" );
			await consulConfigurationService.DeleteValue ( "tester/bluherka" );
			await consulConfigurationService.DeleteValue ( "tester/muhaherka/gruber" );

			//action
			await consulConfigurationService.Initialization ( "[{\"name\": \"tester\",\"items\": [{\"name\": \"muher\",\"value\": \"testerka\"},{\"name\": \"bluherka\",\"value\": \"bruherka\"},{\"name\": \"muhaherka\",items:[{\"name\": \"gruber\",\"value\": \"buhahuherka\"}]}]}]" );

			//assert
			var muher = await consulConfigurationService.GetValue ( "tester/muher" );
			var bluherka = await consulConfigurationService.GetValue ( "tester/bluherka" );
			var gruber = await consulConfigurationService.GetValue ( "tester/muhaherka/gruber" );
			Assert.Equal ( "testerka" , muher );
			Assert.Equal ( "bruherka" , bluherka );
			Assert.Equal ( "buhahuherka" , gruber );
		}

	}

}
