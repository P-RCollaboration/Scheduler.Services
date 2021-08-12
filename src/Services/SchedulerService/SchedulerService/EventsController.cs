using Microsoft.AspNetCore.Mvc;
using SchedulerService;
using System;
using System.Collections.Generic;

namespace UserService {

	[Route ( "/api/event" )]
	[ApiController]
	public class Event {

		[Route("latest")]
		public IEnumerable<EventModel> GetLatest () {
			return new List<EventModel> {
				new EventModel {
					Name = "Routine day",
					Description = "Routine description",
					Date = new DateTime(2021, 8, 11)
				},
				new EventModel {
					Name = "Start of weekend",
					Description = "It is happy day",
					Date = new DateTime(2021, 8, 14)
				},
				new EventModel {
					Name = "End of weekend",
					Description = "This is the worst day of the week",
					Date = new DateTime(2021, 8, 16)
				}
			};
		}

	}

}
