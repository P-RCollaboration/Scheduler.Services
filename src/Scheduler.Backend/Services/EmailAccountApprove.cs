using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Common.Security;
using SqlKata;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Services.Implementations {

	/// <summary>
	/// Simple email account approve.
	/// </summary>
	public class EmailAccountApprove : IEmailAccountApprove {

		private readonly IDataContext m_dataContext;

		private readonly ISmtpClient m_smtpClient;

		private static Random m_RandomGenerator = new( DateTime.Now.Millisecond );

		/// <summary>
		/// Create instance of <see cref="EmailAccountApprove"/>.
		/// </summary>
		/// <param name="dataContext">Date context.</param>
		/// <param name="configurationService">Configuration service.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public EmailAccountApprove ( IDataContext dataContext , ISmtpClient smtpClient ) {
			m_dataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );
			m_smtpClient = smtpClient ?? throw new ArgumentNullException ( nameof ( smtpClient ) );
		}

		private string GenerateString ( int length , Random random ) {
			string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			StringBuilder result = new( length );
			for ( int i = 0 ; i < length ; i++ ) {
				result.Append ( characters[random.Next ( characters.Length )] );
			}
			return result.ToString ();
		}

		private string GetApproveId () {
			var countParts = m_RandomGenerator.Next ( 3 , 5 );
			var result = new StringBuilder ();
			for ( var i = 0 ; i < countParts ; i++ ) {
				result.Append ( ( i > 0 ? "/" : "" ) + GenerateString ( 10 , m_RandomGenerator ) );
			}

			return result.ToString ();
		}

		/// <summary>
		/// Send email to user for approval account.
		/// </summary>
		/// <param name="userId">User identifier.</param>
		/// <exception cref="ArgumentException"></exception>
		public async Task SendApproveEmail ( Guid userId ) {
			var user = await m_dataContext.GetItemAsync<User> (
				new Query ( "users" )
					.Where ( "id" , userId )
			);
			if ( user == null ) throw new ArgumentException ( $"User with id {userId} don't exists!" );

			var emailApprove = new EmailApprove {
				ApproveId = GetApproveId () ,
				UserId = userId
			};
			await m_dataContext.AddOrUpdateAsync ( new Query ( "emailapprove" ) , emailApprove , insert: true );

			var body = "<p><font size='14' color='blue'>Thank you for registration!</font><p>";
			body += "<p><font size='10' color='black'>To verify your email address, just follow the link below.</font><p>";
			body += $"<p><a href='{emailApprove.ApproveId}'></a><p>";
			body += "<p><font size='8' color='black'>*Please note that unconfirmed accounts will be deleted after 3 days without confirmation.</font><p>";

			await m_smtpClient.SendEmail ( user.Email , body , "Confirm Email" + '\uF680' );
		}

		public async Task<bool> VerifyApproveIdFromEmail ( string approveId ) {
			var emailApprove = await m_dataContext.GetItemAsync<EmailApprove> (
				new Query ( "emailapprove" )
					.Where ( "approveid" , approveId )
			);
			if ( emailApprove == null ) return false;

			var user = await m_dataContext.GetItemAsync<User> (
				new Query ( "users" )
					.Where ( "id" , emailApprove.UserId )
					.Where ( "isemailapproved" , false )
			);
			if ( user == null ) return false;

			user.IsEmailApproved = true;
			await m_dataContext.NonResultQueryAsync (
				new Query ( "users" )
					.Where ( "id" , user.Id )
					.AsUpdate ( user )
			);

			return true;
		}
	}

}
