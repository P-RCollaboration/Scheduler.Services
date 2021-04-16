using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Scheduler.Common.Configuration;
using Scheduler.Common.Security;
using Scheduler.Migrations;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Scheduler.Backend {

	public class Program {

		public static async Task Main ( string[] args ) {
			var webHost = CreateHostBuilder ( args ).Build ();

			var configurationService = webHost.Services.GetService ( typeof ( IConfigurationService ) ) as IConfigurationService;
			if ( configurationService != null ) {
				var mainFile = Process.GetCurrentProcess ()?.MainModule?.FileName;
				if ( mainFile != null ) {
					var directoryName = Path.GetDirectoryName ( mainFile );
					if ( directoryName != null ) await configurationService.Initialization ( File.ReadAllText ( Path.Combine ( directoryName , "consul.json" ) ) );
				}
			}

			var migrator = webHost.Services.GetService ( typeof ( IMigrator ) ) as IMigrator;
			if ( migrator != null ) await migrator.Update ();

			var userTokens = webHost.Services.GetService ( typeof ( IUserTokens ) ) as IUserTokens;
			if ( userTokens != null ) await userTokens.LoadTokens ();

			await webHost.RunAsync ();
		}

		public static IHostBuilder CreateHostBuilder ( string[] args ) {
			return Host.CreateDefaultBuilder ( args )
				.ConfigureWebHostDefaults (
					webBuilder => {
						webBuilder.UseStartup<Startup> ();
					}
				);
		}

	}

}
