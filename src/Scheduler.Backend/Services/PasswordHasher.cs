using Scheduler.Common.Security;
using System;
using System.Security.Cryptography;

namespace Scheduler.Services.Implementations {

	/// <summary>
	/// Password hasher based on standart .net core classes.
	/// </summary>
	public class PasswordHasher : IPasswordHasher {

		public string GenerateHash ( string password ) {
			//generate salt
			byte[] salt;
			new RNGCryptoServiceProvider ().GetBytes ( salt = new byte[16] );

			//generate hash
			var pbkdf2 = new Rfc2898DeriveBytes ( password , salt , 10000 );
			byte[] hash = pbkdf2.GetBytes ( 20 );
			
			//join hash and salt in one array
			byte[] hashBytes = new byte[36];
			Array.Copy ( salt , 0 , hashBytes , 0 , 16 );
			Array.Copy ( hash , 0 , hashBytes , 16 , 20 );

			return Convert.ToBase64String ( hashBytes );
		}

		public bool ValidateHash ( string password , string hash ) {
			var hashBytes = Convert.FromBase64String ( hash );
			
			byte[] salt = new byte[16];
			Array.Copy ( hashBytes , 0 , salt , 0 , 16 );
			
			var pbkdf2 = new Rfc2898DeriveBytes ( password , salt , 10000 );
			byte[] enteredHash = pbkdf2.GetBytes ( 20 );

			for (var i = 0 ; i < 20 ; i++ ) {
				if (hashBytes[i + 16] != enteredHash[i] ) return false;
			}

			return true;
		}

	}

}
