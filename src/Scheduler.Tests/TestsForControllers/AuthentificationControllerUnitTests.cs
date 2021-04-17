using FakeItEasy;
using Scheduler.Backend.Controllers;
using Scheduler.Backend.PresentationClasses.Authentification;
using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Common.Security;
using SqlKata;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Scheduler.Tests.TestsForControllers {

	public class AuthentificationControllerUnitTests {

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_DataContext_Null () {
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailAccountApproveFake = A.Fake<IEmailAccountApprove> ();

			Assert.Throws<ArgumentNullException> (
				() => {
					new AuthentificationController ( null , passwordHasherFake , userTokensFake , emailAccountApproveFake );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_PasswordHasher_Null () {
			var dataContextFake = A.Fake<IDataContext> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailAccountApproveFake = A.Fake<IEmailAccountApprove> ();

			Assert.Throws<ArgumentNullException> (
				() => {
					new AuthentificationController ( dataContextFake , null , userTokensFake , emailAccountApproveFake );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_UserTokens_Null () {
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var emailAccountApproveFake = A.Fake<IEmailAccountApprove> ();

			Assert.Throws<ArgumentNullException> (
				() => {
					new AuthentificationController ( dataContextFake , passwordHasherFake , null , emailAccountApproveFake );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void Constructor_EmailAccountApprove_Null () {
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();

			Assert.Throws<ArgumentNullException> (
				() => {
					new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , null );
				}
			);
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_Model_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Authentification ( null );

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_Password_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = "blablba@bllb.com" ,
					Password = null
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_Email_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = null ,
					Password = "fdsfds"
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_Password_Empty () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = "blablba@bllb.com" ,
					Password = ""
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_Email_Empty () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = "" ,
					Password = "fdsfds"
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
		}

		[Theory]
		[Trait ( "Category" , "Unit" )]
		[InlineData ( "argus" )]
		[InlineData ( "margus@" )]
		[InlineData ( "@bluherka" )]
		[InlineData ( "@" )]
		public async Task Authentification_Validate_Email_Validity ( string email ) {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = email ,
					Password = "fdsfdsdsfsdfsd"
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_User_NotFound () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );
			User userNull = null;
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( userNull );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = "argus@burgus.com" ,
					Password = "fdsfds"
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).MustHaveHappened ();
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_Validate_Hash_Incorrect () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( new User { Password = "fdsfds" } ) );
			A.CallTo ( () => passwordHasherFake.ValidateHash ( "fdsfds" , "fdsfds" ) ).Returns ( false );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = "argus@burgus.com" ,
					Password = "fdsfds"
				}
			);

			//assert
			Assert.Equal ( "Wrong email or password" , result.Message );
			Assert.False ( result.IsAuthentificated );
			Assert.Empty ( result.Token );
			A.CallTo ( () => passwordHasherFake.ValidateHash ( "fdsfds" , "fdsfds" ) ).MustHaveHappened ();
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Authentification_UserTokens_GenerateToken () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var tokenId = Guid.NewGuid ();
			var userId = Guid.NewGuid ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( new User { Password = "fdsfds" , Id = userId } ) );
			A.CallTo ( () => passwordHasherFake.ValidateHash ( "fdsfds" , "fdsfds" ) ).Returns ( true );
			A.CallTo ( () => userTokensFake.GenerateToken ( userId ) ).Returns ( tokenId );

			//action
			var result = await controller.Authentification (
				new AuthentificationModel {
					Email = "argus@burgus.com" ,
					Password = "fdsfds"
				}
			);

			//assert
			Assert.Empty ( result.Message );
			Assert.True ( result.IsAuthentificated );
			Assert.Equal ( tokenId.ToString () , result.Token );
			A.CallTo ( () => userTokensFake.GenerateToken ( userId ) ).MustHaveHappened ();
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Model_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration ( null );

			//assert
			Assert.Equal ( "Specified incorrect parameters." , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Email_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = null ,
					Password = "-" ,
					UserName = "-"
				}
			);

			//assert
			Assert.Equal ( "Specified incorrect parameters." , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Password_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "-" ,
					Password = null ,
					UserName = "-"
				}
			);

			//assert
			Assert.Equal ( "Specified incorrect parameters." , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_UserName_Null () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "-" ,
					Password = "-" ,
					UserName = null
				}
			);

			//assert
			Assert.Equal ( "Specified incorrect parameters." , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Theory]
		[Trait ( "Category" , "Unit" )]
		[InlineData ( "argus" )]
		[InlineData ( "margus@" )]
		[InlineData ( "@bluherka" )]
		[InlineData ( "@" )]
		public async Task Registration_Validate_Email_IncorrectFormat ( string email ) {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = email ,
					Password = "-" ,
					UserName = "-"
				}
			);

			//assert
			Assert.Equal ( "Specified incorrect parameters." , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Theory]
		[Trait ( "Category" , "Unit" )]
		[InlineData ( "muh(erka" )]
		[InlineData ( "e-lf" )]
		[InlineData ( "@bluherka" )]
		[InlineData ( "lapa/huherka" )]
		[InlineData ( "pupochka\\" )]
		[InlineData ( "'muhaherka" )]
		[InlineData ( "pruherk(a" )]
		[InlineData ( "bruh[erka" )]
		[InlineData ( "bru]erka" )]
		[InlineData ( "puhahe{rka" )]
		[InlineData ( "luboc}hka" )]
		public async Task Registration_Validate_UserName_Incorrect ( string userName ) {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "-" ,
					UserName = userName
				}
			);

			//assert
			Assert.Equal ( "User name can contain only characters, digits and one character from  ! # & * % ?" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Theory]
		[Trait ( "Category" , "Unit" )]
		[InlineData ( "muh(erka" )]
		[InlineData ( "e-lf" )]
		[InlineData ( "@bluherka" )]
		[InlineData ( "lapa/huherka" )]
		[InlineData ( "pupochka\\" )]
		[InlineData ( "'muhaherka" )]
		[InlineData ( "pruherk(a" )]
		[InlineData ( "bruh[erka" )]
		[InlineData ( "bru]erka" )]
		[InlineData ( "puhahe{rka" )]
		[InlineData ( "luboc}hka" )]
		public async Task Registration_Validate_Password_Incorrect ( string password ) {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = password ,
					UserName = "muhaherka"
				}
			);

			//assert
			Assert.Equal ( "Password name can contain only characters, digits and one character from  ! # & * % ?" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Password_DontHaveRangeABeetwenZ () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "abcdefghigklmnopqrstuyxyz1234567890!#&*%?" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "The password must contain at least one uppercase and one lowercase Latin letter, a number, and one character from ! # & * % ?" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Password_DontHaveRangeaBeetwenz () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "ABCDEFGHIGKLMNOPQRSTUYXYZ1234567890!#&*%?" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "The password must contain at least one uppercase and one lowercase Latin letter, a number, and one character from ! # & * % ?" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Password_DontHaveRange0Beetwen9 () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "abcdefghigklmnopqrstuyxyzABCDEFGHIGKLMNOPQRSTUYXYZ!#&*%?" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "The password must contain at least one uppercase and one lowercase Latin letter, a number, and one character from ! # & * % ?" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Password_DontHaveSpecialSymbols () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "abcdefghigklmnopqrstuyxyzABCDEFGHIGKLMNOPQRSTUYXYZ1234567890" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "The password must contain at least one uppercase and one lowercase Latin letter, a number, and one character from ! # & * % ?" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Password_IncorrectLenght () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "aA1&" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "Password must be long at least 8 characters" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_Validate_Email_AlreadyRegistered () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( new User () ) );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "abcABC123&" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "User with same email already registered" , result.Message );
			Assert.False ( result.IsRegistered );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public async Task Registration_HappyPath () {
			//arrange
			var dataContextFake = A.Fake<IDataContext> ();
			var passwordHasherFake = A.Fake<IPasswordHasher> ();
			var userTokensFake = A.Fake<IUserTokens> ();
			var emailApproveFake = A.Fake<IEmailAccountApprove> ();
			var controller = new AuthentificationController ( dataContextFake , passwordHasherFake , userTokensFake , emailApproveFake );
			User userNull = null;
			Guid userId = Guid.NewGuid ();
			A.CallTo ( () => dataContextFake.GetItemAsync<User> ( A<Query>._ ) ).Returns ( Task.FromResult ( userNull ) );
			A.CallTo ( () => dataContextFake.AddOrUpdateAsync ( A<Query>._ , A<User>._ , true ) )
				.Invokes (
					( Query query , User user , bool insert ) => {
						user.Id = userId;
					}
				)
				.Returns ( Task.CompletedTask );
			A.CallTo ( () => emailApproveFake.SendApproveEmail ( userId ) ).Returns ( Task.CompletedTask );

			//action
			var result = await controller.Registration (
				new RegistrationModel {
					Email = "test@test.com" ,
					Password = "abcABC123&" ,
					UserName = "bluherka"
				}
			);

			//assert
			Assert.Equal ( "" , result.Message );
			Assert.True ( result.IsRegistered );
			A.CallTo ( () => emailApproveFake.SendApproveEmail ( userId ) ).MustHaveHappened ();
			A.CallTo ( () => dataContextFake.AddOrUpdateAsync ( A<Query>._ , A<User>._ , true ) ).MustHaveHappened ();
		}

	}

}
