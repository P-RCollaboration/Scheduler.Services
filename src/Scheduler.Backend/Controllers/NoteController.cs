using Microsoft.AspNetCore.Mvc;
using Scheduler.Backend.PresentationClasses;
using Scheduler.Common.Constants;
using Scheduler.Common.DataContexts;
using Scheduler.Common.Entities;
using Scheduler.Common.Security;
using SqlKata;
using System;
using System.Threading.Tasks;

namespace Scheduler.Backend.Controllers {

	[ApiController]
	[Route ( "api/note" )]
	public class NoteController : ControllerBase {

		private readonly IDataContext m_dataContext;

		private readonly IUserTokens m_userTokens;

		public NoteController ( IDataContext dataContext , IUserTokens userTokens ) {
			m_dataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );
			m_userTokens = userTokens ?? throw new ArgumentNullException ( nameof ( userTokens ) );
		}

		[HttpGet ( "single/{id}/{token}" )]
		public async Task<SingleItemModel<Note>> GetId ( Guid id , Guid token ) {
			if ( token == Guid.Empty ) return new SingleItemModel<Note> { Message = "Token is incorrect" };
			if ( !m_userTokens.ValidToken ( token ) ) return new SingleItemModel<Note> { Message = "Token is incorrect" };
			if ( id == Guid.Empty ) return new SingleItemModel<Note> { Message = "Id is incorrect" };

			var item = await m_dataContext.GetItemAsync<Note> (
				new Query ( TableNames.Notes )
					.Where ( "id" , id )
					.Where ( "userid" , m_userTokens.GetUserIdByToken ( token ) )
			);
			return new SingleItemModel<Note> {
				Item = item ,
				Message = item == null ? $"Note with id {id} not found" : ""
			};
		}

		[HttpPost ( "addoredit/{token}" )]
		public async Task<ChangeItemModel> AddOrEdit ( [FromBody] Note note , [FromRoute] Guid token ) {
			if ( note == null ) return new ChangeItemModel { Message = "Model incorrect!" };
			if ( string.IsNullOrEmpty ( note.Title ) ) return new ChangeItemModel { Message = "Title is empty!" };
			if ( string.IsNullOrEmpty ( note.Body ) ) return new ChangeItemModel { Message = "Body is empty!" };
			if ( !m_userTokens.ValidToken ( token ) ) return new ChangeItemModel { Message = "Token is incorrect" };

			note.UserId = m_userTokens.GetUserIdByToken ( token );

			if ( note.Id != Guid.Empty ) {
				var existingItem = await m_dataContext.GetItemAsync<Note> ( new Query ( TableNames.Notes ).Where ( "id" , note.Id ).Where ( "userid" , note.UserId ) );
				if ( existingItem == null ) return new ChangeItemModel { Message = $"Note with id {note.Id} don't exist!" };
			}

			await m_dataContext.AddOrUpdateAsync (
				new Query ( TableNames.Notes ) ,
				note ,
				insert: note.Id == Guid.Empty
			);
			return new ChangeItemModel { Result = true };
		}

		[HttpGet ( "list/{page}/{timestamp}/{limit}/{token}" )]
		public async Task<MultipleItemsModel<Note>> GetPage ( int page , DateTime timestamp , int limit , Guid token ) {
			if ( token == Guid.Empty ) return new MultipleItemsModel<Note> { Message = "Token is incorrect" };
			if ( !m_userTokens.ValidToken ( token ) ) return new MultipleItemsModel<Note> { Message = "Token is incorrect" };
			if ( page < 1 ) return new MultipleItemsModel<Note> { Message = "Page is incorrect" };
			if ( limit < 5 || limit > 100 ) return new MultipleItemsModel<Note> { Message = "Limit is incorrect" };
			if ( timestamp < DateTime.Now.AddHours ( -24 ) ) return new MultipleItemsModel<Note> { Message = "Timestamp is incorrect" };

			var items = await m_dataContext.GetItemsAsync<Note> (
				new Query ( TableNames.Notes )
					.Where ( "created" , "<" , timestamp )
					.Skip ( ( page - 1 ) * 10 )
					.Limit ( 10 )
					.Where ( "userid" , m_userTokens.GetUserIdByToken ( token ) )
			);
			return new MultipleItemsModel<Note> {
				Items = items ,
				Message = ""
			};
		}

	}

}
