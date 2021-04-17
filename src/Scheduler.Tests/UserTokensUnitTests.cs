using FakeItEasy;
using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Services.Implementations;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Scheduler.Tests {

	public class UserTokensUnitTests {

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_Throw_DataContext_Null () {
			Assert.Throws<ArgumentNullException> (
				() => {
					var userTokens = new PostgresUserTokens ( null );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task ValidaToken_Positive () {
			//arrange
			var token = Guid.NewGuid ();
			var dataContextFake = A.Fake<IDataContext> ();
			A.CallTo ( () => dataContextFake.GetItemsAsync<Token> ( A<Query>._ ) ).Returns (
				Task.FromResult (
					new List<Token> {
						new Token {
							Id = token,
							UserId = Guid.NewGuid(),
							Logined = DateTime.Now
						}
					}.AsEnumerable ()
				)
			);
			var userTokens = new PostgresUserTokens ( dataContextFake );
			await userTokens.LoadTokens ();

			//action
			var result = userTokens.ValidToken ( token );

			//assert
			Assert.True ( result );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task ValidaToken_NotValid () {
			//arrange
			var token = Guid.NewGuid ();
			var dataContextFake = A.Fake<IDataContext> ();
			A.CallTo ( () => dataContextFake.GetItemsAsync<Token> ( A<Query>._ ) ).Returns (
				Task.FromResult (
					new List<Token> {
						new Token {
							Id = Guid.NewGuid(),
							UserId = Guid.NewGuid(),
							Logined = DateTime.Now
						}
					}.AsEnumerable ()
				)
			);
			var userTokens = new PostgresUserTokens ( dataContextFake );
			await userTokens.LoadTokens ();

			//action
			var result = userTokens.ValidToken ( token );

			//assert
			Assert.False ( result );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task GenerateToken_WithoutExistingTokens () {
			//arrange
			var tokenId = Guid.NewGuid ();
			var userId = Guid.NewGuid ();
			var dataContextFake = A.Fake<IDataContext> ();
			A.CallTo ( () => dataContextFake.GetItemsAsync<Token> ( A<Query>._ ) ).Returns ( Task.FromResult ( new List<Token> ().AsEnumerable () ) );
			A.CallTo ( () => dataContextFake.AddOrUpdateAsync ( A<Query>._ , A<Token>._ , true ) )
				.Invokes ( ( Query query , Token token , bool insert ) => {
					token.Id = tokenId;
				}
			);
			var userTokens = new PostgresUserTokens ( dataContextFake );

			//action
			var result = await userTokens.GenerateToken ( userId );

			//assert
			Assert.Equal ( tokenId , result );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task GenerateToken_WithExistingTokens () {
			//arrange
			var tokenId = Guid.NewGuid ();
			var userId = new Guid ( "02390BDB-D5D4-4CA1-A44F-2D33B970FC2F" );
			var existingToken1 = Guid.NewGuid ();
			var existingToken2 = Guid.NewGuid ();
			var dataContextFake = A.Fake<IDataContext> ();
			A.CallTo ( () => dataContextFake.AddOrUpdateAsync ( A<Query>._ , A<Token>._ , true ) )
				.Invokes ( ( Query query , Token token , bool insert ) => {
					token.Id = tokenId;
				}
			);
			A.CallTo ( () => dataContextFake.GetItemsAsync<Token> ( A<Query>._ ) ).Returns (
				Task.FromResult (
					new List<Token> {
						new Token {
							Id = existingToken1,
							UserId = userId,
							Logined = DateTime.Now
						},
						new Token {
							Id = existingToken2,
							UserId = userId,
							Logined = DateTime.Now
						}
					}.AsEnumerable ()
				)
			);
			var userTokens = new PostgresUserTokens ( dataContextFake );
			await userTokens.LoadTokens ();

			//action
			var result = await userTokens.GenerateToken ( userId );

			//assert
			Assert.False ( userTokens.ValidToken ( existingToken1 ) );
			Assert.False ( userTokens.ValidToken ( existingToken2 ) );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task GetUserIdByToken_NoToken () {
			//arrange
			var token = Guid.NewGuid ();
			var dataContextFake = A.Fake<IDataContext> ();
			A.CallTo ( () => dataContextFake.GetItemsAsync<Token> ( A<Query>._ ) ).Returns (
				Task.FromResult (
					new List<Token> {
						new Token {
							Id = Guid.NewGuid(),
							UserId = Guid.NewGuid(),
							Logined = DateTime.Now
						}
					}.AsEnumerable ()
				)
			);
			var userTokens = new PostgresUserTokens ( dataContextFake );
			await userTokens.LoadTokens ();

			//assert
			Assert.Throws<KeyNotFoundException> (
				() => {
					//action
					var result = userTokens.GetUserIdByToken ( Guid.NewGuid () );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task GetUserIdByToken_HasToken () {
			//arrange
			var token = Guid.NewGuid ();
			var dataContextFake = A.Fake<IDataContext> ();
			var userId = Guid.NewGuid ();
			A.CallTo ( () => dataContextFake.GetItemsAsync<Token> ( A<Query>._ ) ).Returns (
				Task.FromResult (
					new List<Token> {
						new Token {
							Id = token,
							UserId = userId,
							Logined = DateTime.Now
						}
					}.AsEnumerable ()
				)
			);
			var userTokens = new PostgresUserTokens ( dataContextFake );
			await userTokens.LoadTokens ();

			//action
			var result = userTokens.GetUserIdByToken ( token );

			//assert
			Assert.Equal ( userId , result );
		}


	}
}
