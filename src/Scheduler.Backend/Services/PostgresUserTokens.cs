using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Common.PresentationClasses.Security;
using Scheduler.Common.Security;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler.Services.Implementations {

	/// <summary>
	/// Implementation for <see cref="IUserTokens"/> based on postgresql database as external storage.
	/// </summary>
	public class PostgresUserTokens : IUserTokens {

		private static Dictionary<Guid , UserTokenModel> m_Tokens = new Dictionary<Guid , UserTokenModel> ();

		private readonly IDataContext m_DataContext;

		private static SpinLock m_SpinLock = new SpinLock ();

		/// <summary>
		/// Create new instance <see cref="PostgresUserTokens"/>.
		/// </summary>
		/// <param name="dataContext">Date context.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public PostgresUserTokens ( IDataContext dataContext ) => m_DataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );

		public async Task<Guid> GenerateToken ( Guid userId ) {
			var userToken = new Token { UserId = userId };
			await m_DataContext.AddOrUpdateAsync ( new Query ( "tokens" ) , userToken , insert: true );
			m_Tokens.Add (
				userToken.Id ,
				new UserTokenModel {
					Logined = userToken.Logined ,
					UserId = userToken.UserId
				}
			);

			//removing all another sessions
			var existingTokens = await m_DataContext.GetItemsAsync<Token> (
				new Query ( "tokens" )
					.Where ( "userid" , userId )
					.Where ( "id" , "<>" , userToken.Id ) 
			);
			if ( existingTokens.Any () ) {
				await m_DataContext.NonResultQueryAsync (
					new Query ( "tokens" )
						.Where ( "userid" , userId )
						.Where ( "id" , "<>" , userToken.Id )
						.AsDelete ()
				);

				bool lockTaken = false;
				try {
					m_SpinLock.Enter ( ref lockTaken );

					foreach ( var existingToken in existingTokens ) {
						m_Tokens.Remove ( existingToken.Id );
					}
				} finally {
					if ( lockTaken ) m_SpinLock.Exit ( false );
				}
			}

			return userToken.Id;
		}

		public async Task LoadTokens () {
			var tokens = await m_DataContext.GetItemsAsync<Token> ( new Query ( "tokens" ) );
			foreach ( var token in tokens ) {
				m_Tokens.Add (
					token.Id ,
					new UserTokenModel {
						Logined = token.Logined ,
						UserId = token.UserId
					}
				);
			}
		}

		public bool ValidToken ( Guid token ) => m_Tokens.ContainsKey ( token );

		public Guid GetUserIdByToken ( Guid token ) => m_Tokens[token].UserId;

	}

}
