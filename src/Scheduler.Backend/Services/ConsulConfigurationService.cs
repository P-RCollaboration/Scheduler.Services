using Consul;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Scheduler.Common.Configuration;
using Scheduler.Backend.PresentationClasses.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Backend.Services.Implementations {

	/// <summary>
	/// Configuration service based on Consul tool.
	/// </summary>
	public class ConsulConfigurationService : IConfigurationService {

		ConsulClient m_ConsulClient;

		public ConsulConfigurationService ( IOptions<ProjectOptions> options ) {
			m_ConsulClient = new ConsulClient (
				( config ) => {
					config.Address = new Uri ( options.Value.ConsulUrl );
				}
			);
		}

		public async Task DeleteValue ( string path ) => await m_ConsulClient.KV.Delete ( path );

		/// <inheritdoc cref="IConfigurationService.GetValue" />
		public async Task<string> GetValue ( string path ) {
			var pair = await m_ConsulClient.KV.Get ( path );

			if ( pair.StatusCode == HttpStatusCode.NotFound ) return "";

			return Encoding.UTF8.GetString ( pair.Response.Value , 0 , pair.Response.Value.Length );
		}

		private async Task SaveSettingsItem ( SettingsItemModel settingsItemModel , string path ) {
			if ( settingsItemModel.Value != null ) {
				var existingKey = await m_ConsulClient.KV.Get ( path );
				if ( existingKey.StatusCode == HttpStatusCode.NotFound ) {
					var putAttempt = await m_ConsulClient.KV.Put (
						new KVPair ( path ) {
							Value = Encoding.UTF8.GetBytes ( settingsItemModel.Value )
						}
					);
					if ( !putAttempt.Response ) throw new InvalidProgramException ( $"Failed while attempt save value {settingsItemModel.Value} to path {path}" );
				}
			}

			if ( settingsItemModel.Items == null || !settingsItemModel.Items.Any () ) return;

			foreach ( var item in settingsItemModel.Items ) await SaveSettingsItem ( item , path + "/" + item.Name );
		}

		///<inheritdoc cref="IConfigurationService.Initialization" />
		public async Task Initialization ( string settings ) {
			var settingItems = JsonConvert.DeserializeObject<IEnumerable<SettingsItemModel>> ( settings );

			foreach ( var settingItem in settingItems ) await SaveSettingsItem ( settingItem , settingItem.Name );
		}

	}

}
