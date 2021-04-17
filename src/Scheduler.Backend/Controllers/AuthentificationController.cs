using Microsoft.AspNetCore.Mvc;
using Scheduler.Backend.PresentationClasses.Authentification;
using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Common.Security;
using SqlKata;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scheduler.Backend.Controllers {

	[ApiController]
	[Route ( "api/authentification" )]
	public class AuthentificationController : ControllerBase {

		private readonly IDataContext m_dataContext;

		private readonly IPasswordHasher m_passwordHasher;

		private readonly IUserTokens m_userTokens;

		private readonly IEmailAccountApprove m_emailAccountApprove;

		public AuthentificationController ( IDataContext dataContext , IPasswordHasher passwordHasher , IUserTokens userTokens , IEmailAccountApprove emailAccountApprove ) {
			m_dataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );
			m_passwordHasher = passwordHasher ?? throw new ArgumentNullException ( nameof ( passwordHasher ) );
			m_userTokens = userTokens ?? throw new ArgumentNullException ( nameof ( userTokens ) );
			m_emailAccountApprove = emailAccountApprove ?? throw new ArgumentNullException ( nameof ( emailAccountApprove ) );
		}

		[HttpPost]
		[Route ( "signin" )]
		public async Task<AuthentificationResultModel> Authentification ( [FromBody] AuthentificationModel model ) {
			var result = new AuthentificationResultModel {
				IsAuthentificated = false ,
				Message = "Wrong email or password"
			};

			if ( model == null ) return result;
			if ( string.IsNullOrEmpty ( model.Password ) || string.IsNullOrEmpty ( model.Email ) ) return result;
			if ( !model.Email.Contains ( "@" ) || model.Email.Split ( "@" ).Where ( a => !string.IsNullOrEmpty ( a ) ).Count () != 2 ) return result;

			var user = await m_dataContext.GetItemAsync<User> (
				new Query ( "users" )
					.Where ( "email" , model.Email )
			);

			if ( user == null ) return result;
			if ( !m_passwordHasher.ValidateHash ( model.Password , user.Password ) ) return result;

			var token = await m_userTokens.GenerateToken ( user.Id );

			result.IsAuthentificated = true;
			result.Message = "";
			result.Token = token.ToString ();
			result.DisplayName = user.Login;

			return result;
		}

		[HttpPost]
		[Route ( "signup" )]
		public async Task<RegistrationResultModel> Registration ( [FromBody] RegistrationModel model ) {
			var result = new RegistrationResultModel {
				IsRegistered = false ,
				Message = "Specified incorrect parameters."
			};

			if ( model == null ) return result;
			if ( string.IsNullOrEmpty ( model.Email ) || string.IsNullOrEmpty ( model.Password ) || string.IsNullOrEmpty ( model.UserName ) ) return result;
			if ( !model.Email.Contains ( "@" ) || model.Email.Split ( "@" ).Where ( a => !string.IsNullOrEmpty ( a ) ).Count () != 2 ) return result;

			var userNameMatches = Regex.Matches ( model.UserName , "[A-Za-z0-9\\!\\#\\&\\*\\%\\?]{0,}" );
			if ( userNameMatches.Any () && userNameMatches.First ().Value != model.UserName ) {
				result.Message = "User name can contain only characters, digits and one character from  ! # & * % ?";
				return result;
			}
			var passwordMatches = Regex.Matches ( model.Password , "[A-Za-z0-9\\!\\#\\&\\*\\%\\?]{0,}" );
			if ( passwordMatches.Any() && passwordMatches.First().Value != model.Password ) {
				result.Message = "Password name can contain only characters, digits and one character from  ! # & * % ?";
				return result;
			}
			if ( !( Regex.Matches ( model.Password , "[A-Z]{1,}" ).Any () && Regex.Matches ( model.Password , "[a-z]{1,}" ).Any () &&
				Regex.Matches ( model.Password , "[0-9]{1,}" ).Any () && Regex.Matches ( model.Password , "[\\!\\#\\&\\*\\%\\?]{1,}" ).Any () ) ) {
				result.Message = "The password must contain at least one uppercase and one lowercase Latin letter, a number, and one character from ! # & * % ?";
				return result;
			}
			if ( model.Password.Length < 8 ) {
				result.Message = "Password must be long at least 8 characters";
				return result;
			}

			var existingUser = await m_dataContext.GetItemAsync<User> (
				new Query ( "users" )
					.Where ( "email" , model.Email )
			);
			if ( existingUser != null ) {
				result.Message = "User with same email already registered";
				return result;
			}

			var user = new User {
				Email = model.Email ,
				Login = model.UserName ,
				Password = m_passwordHasher.GenerateHash ( model.Password ) ,
				IsEmailApproved = false
			};
			await m_dataContext.AddOrUpdateAsync ( new Query ( "users" ) , user , insert: true );

			await m_emailAccountApprove.SendApproveEmail ( user.Id );

			result.IsRegistered = true;
			result.Message = "";

			return result;
		}

	}

}
