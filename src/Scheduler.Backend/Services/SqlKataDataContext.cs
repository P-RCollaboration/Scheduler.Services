using Npgsql;
using Scheduler.Common.Configuration;
using Scheduler.Common.DataContexts;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Scheduler.Backend.Services.Implementations {

	/// <summary>
	/// Data context based on SqlKata library.
	/// </summary>
	public class SqlKataDataContext : IDataContext {

		private readonly IConfigurationService m_ConfigurationService;

		private static string m_ConnectionString = "";

		public SqlKataDataContext ( IConfigurationService configurationService ) {
			m_ConfigurationService = configurationService ?? throw new ArgumentNullException ( nameof ( configurationService ) );
		}

		private async ValueTask<QueryFactory> GetContext () {
			if ( string.IsNullOrEmpty ( m_ConnectionString ) ) m_ConnectionString = await m_ConfigurationService.GetValue ( "ConnectionStrings/Postgres" );

			var connection = new NpgsqlConnection ( m_ConnectionString );
			return new QueryFactory ( connection , new PostgresCompiler () );
		}

		public async Task<IEnumerable<T>> GetItemsAsync<T> ( Query query ) {
			var context = await GetContext ();

#if DEBUG
			Debug.WriteLine ( "GetItems sql: " + new PostgresCompiler ().Compile ( query ).ToString () );
#endif

			return await context.FromQuery ( query ).GetAsync<T> ( timeout: 10 );
		}

		public async Task<T?> GetItemAsync<T> ( Query query ) {
			var context = await GetContext ();

#if DEBUG
			Debug.WriteLine ( "GetItem sql: " + new PostgresCompiler ().Compile ( query ).ToString () );
#endif

			return ( await context.FromQuery ( query ).GetAsync<T> ( timeout: 10 ) ).FirstOrDefault ();
		}

		public T? GetItem<T> ( Query query ) {
#if DEBUG
			Debug.WriteLine ( "GetItem sql: " + new PostgresCompiler ().Compile ( query ).ToString () );
#endif

			var task = GetContext ().AsTask ();
			Task.WaitAll ( task );
			var context = task.Result;

			var queryTask = context.FromQuery ( query ).GetAsync<T> ( timeout: 10 );
			Task.WaitAll ( queryTask );
			return queryTask.Result.FirstOrDefault ();
		}

		public async Task NonResultQueryAsync ( Query query ) {
#if DEBUG
			Debug.WriteLine ( "NonResultQueryAsync sql: " + new PostgresCompiler ().Compile ( query ).ToString () );
#endif
			var context = await GetContext ();

			await context.FromQuery ( query ).GetAsync ( timeout: 10 );
		}

		public async Task NonResultQueryAsync ( string query ) {
			if ( string.IsNullOrEmpty ( m_ConnectionString ) ) await GetContext ();

			await using var connection = new NpgsqlConnection ( m_ConnectionString );
			await connection.OpenAsync ();

			await using ( var command = new NpgsqlCommand ( query , connection ) ) {
				await command.ExecuteNonQueryAsync ();
			}
		}

		public async Task NonResultQueriesAsync ( IEnumerable<string> queries ) {
			if ( string.IsNullOrEmpty ( m_ConnectionString ) ) await GetContext ();

			await using var connection = new NpgsqlConnection ( m_ConnectionString );
			await connection.OpenAsync ();

			var transaction = await connection.BeginTransactionAsync ();

			try {
				foreach ( var query in queries ) {
					await using var command = new NpgsqlCommand ( query , connection );
					await command.ExecuteNonQueryAsync ();
				}
			} catch {
				await transaction.RollbackAsync ();
				throw;
			}

			await transaction.CommitAsync ();
		}

		public async Task AddOrUpdateAsync<T> ( Query query , T model , bool insert = false ) {
			if ( model == null ) return;

			var context = await GetContext ();

			var (sql, properties) = GenerateSql ( query , model , insert );

			var resultRows = await context.SelectAsync ( sql , timeout: 10 );

			MapModel ( model , properties , resultRows );
		}

		private static void MapModel<T> ( T model , PropertyInfo[] properties , IEnumerable<dynamic> resultRows ) {
			if ( properties == null ) return;

			var modelRow = (IDictionary<string , object>) resultRows.First ();

			foreach ( var property in properties ) {
				var propertyName = property.Name.ToLowerInvariant ();

				property.GetSetMethod ()?.Invoke ( model , new object[] { modelRow[propertyName] } );
			}
		}

		private static (string, PropertyInfo[]) GenerateSql<T> ( Query query , T model , bool insert ) {
			if ( model == null ) return ("", new PropertyInfo[0]);
			object? id = null;
			var values = new Dictionary<string , object> ();
			var properties = model.GetType ().GetProperties ();
			foreach ( var property in properties ) {
				if ( property.Name == "Id" ) {
					if ( !insert ) id = property.GetGetMethod ()?.Invoke ( model , null );
					continue;
				}

				var propertyValue = property.GetGetMethod ()?.Invoke ( model , null );
				if ( propertyValue != null ) values.Add ( property.Name.ToLowerInvariant () , propertyValue );
			}

			return (
				new PostgresCompiler ().Compile ( insert ? query.AsInsert ( values ) : query.Where ( "id" , id ).AsUpdate ( values ) ).ToString () + " RETURNING *;",
				properties
			);
		}

		public void AddOrUpdate<T> ( Query query , T model , bool insert = false ) {
			if ( model == null ) return;

			var task = GetContext ().AsTask ();
			Task.WhenAll ( task );
			var context = task.Result;

			var (sql, properties) = GenerateSql ( query , model , insert );

			var resultRows = context.Select ( sql , timeout: 10 );

			MapModel ( model , properties , resultRows );
		}

	}

}
