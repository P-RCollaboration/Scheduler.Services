using System;
using System.Threading.Tasks;

namespace Scheduler.Common.Security {

	/// <summary>
	/// Interface for handle user tokens.
	/// </summary>
	public interface IUserTokens {

		/// <summary>
		/// Check if token is valid.
		/// </summary>
		/// <param name="token">Token</param>
		/// <returns>Check result</returns>
		bool ValidToken ( Guid token );

		/// <summary>
		/// Get userId by token.
		/// </summary>
		/// <param name="token">Token.</param>
		/// <returns>User identifier.</returns>
		Guid GetUserIdByToken ( Guid token );

		/// <summary>
		/// Generate token.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		Task<Guid> GenerateToken ( Guid userId );

		/// <summary>
		/// Load tokens from external storage.
		/// </summary>
		Task LoadTokens ();

	}

}
