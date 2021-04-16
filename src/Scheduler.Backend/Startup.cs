using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Scheduler.Backend.PresentationClasses.Configurations;
using Microsoft.Extensions.Configuration;
using Scheduler.Migrations.Migrators;
using System.Linq;
using Scheduler.Common.Configuration;
using Scheduler.Common.DataContexts;
using Scheduler.Migrations;
using Scheduler.Backend.Services.Implementations;
using Scheduler.Common.Security;
using Scheduler.Services.Implementations;

namespace Scheduler.Backend {

	public class Startup {

		public Startup ( IConfiguration configuration ) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		public void ConfigureServices ( IServiceCollection services ) {
			services.AddCors ();

			services.AddControllers ();
			services.AddSwaggerGen (
				setupAction => {
					setupAction.SwaggerDoc (
						"Documentation" ,
						new OpenApiInfo {
							Title = "Scheduler api specification" ,
							Version = "v1"
						}
					);
				}
			);

			services.Configure<ProjectOptions> (
				options => Configuration.GetSection ( "Project" ).Bind ( options )
			);
			
			services.AddSingleton<IConfigurationService , ConsulConfigurationService> ();
			services.AddTransient<IDataContext , SqlKataDataContext> ();
			services.AddTransient<IMigrator , ConventionMigrator> ();
			services.AddTransient<IUserTokens , PostgresUserTokens> ();
			services.AddTransient<IPasswordHasher , PasswordHasher> ();
			services.AddTransient<IEmailAccountApprove , EmailAccountApprove> ();
			services.AddTransient<ISmtpClient , FakeSmtpClient> ();

			RegisterMigrations ( services );
		}

		private void RegisterMigrations ( IServiceCollection services ) {
			var migrationTypes = typeof ( ConventionMigrator ).Assembly.GetTypes ()
				.Where ( a => a.GetInterface ( "IMigration" ) != null )
				.ToList ();

			foreach ( var migrationType in migrationTypes ) {
				services.AddTransient ( migrationType );
			}
		}

		public void Configure ( IApplicationBuilder applicationBuilder , IWebHostEnvironment env ) {
			if ( env.IsDevelopment () ) {
				applicationBuilder.UseDeveloperExceptionPage ();
				applicationBuilder.UseSwagger ();
				applicationBuilder.UseSwaggerUI ( c => c.SwaggerEndpoint ( "/swagger/v1/swagger.json" , "Scheduler api v1" ) );
			}

			applicationBuilder.UseRouting ();

			applicationBuilder.UseCors (
				builder => builder.AllowAnyOrigin ()
					.AllowAnyHeader ()
					.AllowAnyMethod ()
			);

			applicationBuilder.UseEndpoints (
				endpoints => {
					endpoints.MapControllers ();
				}
			);
		}
	}

}
