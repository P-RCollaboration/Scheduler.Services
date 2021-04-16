namespace Scheduler.Common.Security {

	/// <summary>
	/// Used for has password and validate hashes.
	/// </summary>
	public interface IPasswordHasher {

		string GenerateHash ( string password );

		bool ValidateHash ( string password, string hash );

	}

}
