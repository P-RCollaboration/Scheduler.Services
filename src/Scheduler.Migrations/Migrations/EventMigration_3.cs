using Scheduler.Common.DataContexts;
using System;
using System.Threading.Tasks;

namespace Scheduler.Migrations {

	public class EventMigration_3 : IMigration {

		private readonly IDataContext m_DataContext;

		public string Description => "Creating event tables";

		public EventMigration_3 ( IDataContext dataContext ) => m_DataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );

		public async Task Down () {
			await m_DataContext.NonResultQueriesAsync (
				new string[] {
					"DROP TABLE public.events;",
					$"DELETE FROM public.migrations WHERE id ='{nameof(EventMigration_3)}';"
				}
			);
		}

		public async Task Up () {
			await m_DataContext.NonResultQueriesAsync (
				new string[] {
					"CREATE TABLE public.events (id uuid NOT NULL DEFAULT uuid_generate_v4(), created timestamp without time zone NOT NULL DEFAULT now(), title text not null, description text not null, userid uuid NOT NULL, starttime timestamp without time zone NOT NULL, endtime timestamp without time zone, CONSTRAINT events_pkey PRIMARY KEY (id), CONSTRAINT events_users_userid FOREIGN KEY (userid) REFERENCES public.users (id) ON DELETE NO ACTION ON UPDATE NO ACTION);",
					"ALTER TABLE public.events OWNER TO postgres;",
					$"INSERT INTO public.migrations(id) VALUES ('{nameof(EventMigration_3)}')"
				}
			);
		}

	}

}
