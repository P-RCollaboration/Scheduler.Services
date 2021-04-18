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
	[Route ( "api/event" )]
	public class EventController : ControllerBase {

		private readonly IDataContext m_dataContext;

		private readonly IUserTokens m_userTokens;

		public EventController ( IDataContext dataContext , IUserTokens userTokens ) {
			m_dataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );
			m_userTokens = userTokens ?? throw new ArgumentNullException ( nameof ( userTokens ) );
		}

		[HttpGet ( "single/{id}/{token}" )]
		public async Task<SingleItemModel<Event>> GetId ( Guid id , Guid token ) {
			if ( token == Guid.Empty ) return new SingleItemModel<Event> { Message = "Token is incorrect" };
			if ( !m_userTokens.ValidToken ( token ) ) return new SingleItemModel<Event> { Message = "Token is incorrect" };
			if ( id == Guid.Empty ) return new SingleItemModel<Event> { Message = "Id is incorrect" };

			var eventItem = await m_dataContext.GetItemAsync<Event> (
				new Query ( "events" )
					.Where ( "id" , id )
					.Where ( "userid" , m_userTokens.GetUserIdByToken ( token ) )
			);
			return new SingleItemModel<Event> {
				Item = eventItem ,
				Message = eventItem == null ? $"Event with id {id} not found" : ""
			};
		}

		[HttpPost ( "addoredit/{token}" )]
		public async Task<ChangeItemModel> AddOrEdit ( [FromBody] Event @event , [FromRoute] Guid token ) {
			if ( @event == null ) return new ChangeItemModel { Message = "Model incorrect!" };
			if ( string.IsNullOrEmpty ( @event.Title ) ) return new ChangeItemModel { Message = "Title empty!" };
			if ( @event.StartTime < DateTime.Now.AddYears ( -2 ) ) return new ChangeItemModel { Message = "StartTime less then two years!" };
			if ( @event.EndTime.HasValue && @event.EndTime > @event.StartTime ) return new ChangeItemModel { Message = "StartTime greater then EndTime!" };
			if ( !m_userTokens.ValidToken ( token ) ) return new ChangeItemModel { Message = "Token is incorrect" };

			@event.UserId = m_userTokens.GetUserIdByToken ( token );

			if ( @event.Id != Guid.Empty ) {
				var existingItem = await m_dataContext.GetItemAsync<Note> ( new Query ( TableNames.Events ).Where ( "id" , @event.Id ).Where ( "userid" , @event.UserId ) );
				if ( existingItem == null ) return new ChangeItemModel { Message = $"Note with id {@event.Id} don't exist!" };
			}

			await m_dataContext.AddOrUpdateAsync (
				new Query ( "events" ) ,
				@event ,
				insert: @event.Id == Guid.Empty
			);
			return new ChangeItemModel { Result = true };
		}

		[HttpGet ( "list/{page}/{timestamp}/{limit}/{token}" )]
		public async Task<MultipleItemsModel<Event>> GetUserEvents ( int page , DateTime timestamp , int limit , Guid token ) {
			if ( token == Guid.Empty ) return new MultipleItemsModel<Event> { Message = "Token is incorrect" };
			if ( !m_userTokens.ValidToken ( token ) ) return new MultipleItemsModel<Event> { Message = "Token is incorrect" };
			if ( page < 1 ) return new MultipleItemsModel<Event> { Message = "Page is incorrect" };
			if ( limit < 5 || limit > 100 ) return new MultipleItemsModel<Event> { Message = "Limit is incorrect" };
			if ( timestamp < DateTime.Now.AddHours ( -24 ) ) return new MultipleItemsModel<Event> { Message = "Timestamp is incorrect" };

			var items = await m_dataContext.GetItemsAsync<Event> (
				new Query ( TableNames.Events )
					.Where ( "created" , "<" , timestamp )
					.Skip ( ( page - 1 ) * 10 )
					.Limit ( 10 )
					.Where ( "userid" , m_userTokens.GetUserIdByToken ( token ) )
			);
			return new MultipleItemsModel<Event> {
				Items = items ,
				Message = ""
			};
		}

	}

}
