using FakeItEasy;
using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Common.Security;
using Scheduler.Services.Implementations;
using SqlKata;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Scheduler.Tests {

	public class EmailAccountApproveUnitTests {

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_DataContext_Null () {
			//arrange
			var smtpClientFake = A.Fake<ISmtpClient> ();

			//assert
			Assert.Throws<ArgumentNullException> (
				() => {
					//action
					new EmailAccountApprove ( null , smtpClientFake );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_SmtpClient_Null () {
			//arrange
			var datContextFake = A.Fake<IDataContext> ();

			//assert
			Assert.Throws<ArgumentNullException> (
				() => {
					//action
					new EmailAccountApprove ( datContextFake , null );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task VerifyApproveIdFromEmail_No_EmailApprove () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var smtpClientFake = A.Fake<ISmtpClient> ();
			EmailApprove nullApprove = null;
			A.CallTo ( () => dataContextFake.GetItemAsync<EmailApprove> ( A<Query>._ ) ).Returns ( nullApprove );
			var emailApprove = new EmailAccountApprove ( dataContextFake , smtpClientFake );

			//action
			var result = await emailApprove.VerifyApproveIdFromEmail ( "testapprove" );

			//assert
			Assert.False ( result );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task VerifyApproveIdFromEmail_UserAlreadyApproved () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var smtpClientFake = A.Fake<ISmtpClient> ();
			A.CallTo ( () => dataContextFake.GetItemAsync<EmailApprove> ( A<Query>._ ) ).Returns ( Task.FromResult ( new EmailApprove { Id = Guid.NewGuid () , ApproveId = "testapprove" , UserId = Guid.NewGuid () } ) );
			User nullUser = null;
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( nullUser ) );
			var emailApprove = new EmailAccountApprove ( dataContextFake , smtpClientFake );

			//action
			var result = await emailApprove.VerifyApproveIdFromEmail ( "testapprove" );

			//assert
			Assert.False ( result );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task VerifyApproveIdFromEmail_EmailApproved () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var smtpClientFake = A.Fake<ISmtpClient> ();
			var user = new User {
				Id = Guid.NewGuid () ,
				IsEmailApproved = false
			};
			A.CallTo ( () => dataContextFake.GetItemAsync<EmailApprove> ( A<Query>._ ) ).Returns ( Task.FromResult ( new EmailApprove { Id = Guid.NewGuid () , ApproveId = "testapprove" , UserId = user.Id } ) );
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( user ) );
			var emailApprove = new EmailAccountApprove ( dataContextFake , smtpClientFake );

			//action
			var result = await emailApprove.VerifyApproveIdFromEmail ( "testapprove" );

			//assert
			Assert.True ( user.IsEmailApproved );
			Assert.True ( result );
			A.CallTo ( () => dataContextFake.NonResultQueryAsync ( A<Query>._ ) ).MustHaveHappened ();
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task SendApproveEmail_UserDontExists () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var smtpClientFake = A.Fake<ISmtpClient> ();
			User nullUser = null;
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( nullUser ) );
			var emailApprove = new EmailAccountApprove ( dataContextFake , smtpClientFake );

			//assert
			await Assert.ThrowsAsync<ArgumentException> (
				async () => {
					//action
					await emailApprove.SendApproveEmail ( Guid.NewGuid () );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task SendApproveEmail_SendEmail () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var smtpClientFake = A.Fake<ISmtpClient> ();
			var user = new User {
				Email = "test@test.com" ,
			};
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( user ) );

			var emailApprove = new EmailAccountApprove ( dataContextFake , smtpClientFake );

			//action
			await emailApprove.SendApproveEmail ( Guid.NewGuid () );

			//assert
			A.CallTo ( () => smtpClientFake.SendEmail ( "test@test.com" , A<string>._ , "Confirm Email" + '\uF680' ) ).MustHaveHappened ();
			A.CallTo ( () => dataContextFake.AddOrUpdateAsync ( A<Query>._ , A<EmailApprove>._ , true ) ).MustHaveHappened ();
		}

	}

}
