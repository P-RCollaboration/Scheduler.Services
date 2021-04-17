using Scheduler.Services.Implementations;
using Xunit;

namespace Scheduler.Tests {

	/// <summary>
	/// Unit tests for <see cref="PasswordHasher"/>.
	/// </summary>
	public class PasswordHasherUnitTests {

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void GenerateHash_UniqueSalt () {
			//arrange
			var passwordHasher = new PasswordHasher ();
			var password = "bruherkamuherka";

			//action
			var hash1 = passwordHasher.GenerateHash ( password );
			var hash2 = passwordHasher.GenerateHash ( password );

			//assert
			Assert.NotSame ( hash1 , hash2 );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void GenerateHash_ValidateHash_HappyPath () {
			//arrange
			var passwordHasher = new PasswordHasher ();
			var password = "bruherka";

			//action
			var hash = passwordHasher.GenerateHash ( password );
			var result = passwordHasher.ValidateHash ( password, hash );

			//assert
			Assert.True ( result );
		}

		[Fact]
		[Trait ( "Category" , "Unit" )]
		public void GenerateHash_ValidateHash_WrongPassword () {
			//arrange
			var passwordHasher = new PasswordHasher ();

			//action
			var hash = passwordHasher.GenerateHash ( "bruherka" );
			var result = passwordHasher.ValidateHash ( "bruherkb" , hash );

			//assert
			Assert.False ( result );
		}

	}

}
