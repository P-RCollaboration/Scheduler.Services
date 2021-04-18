using Scheduler.Common.DataContexts;
using System;
using System.Threading.Tasks;

namespace Scheduler.Migrations {

	public class NoteMigration_2 : IMigration {
		
		private readonly IDataContext m_DataContext;

		public string Description => "Creating note tables";

		public NoteMigration_2 ( IDataContext dataContext ) => m_DataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );

		public async Task Down () {
			await m_DataContext.NonResultQueriesAsync (
				new string[] {
					"DROP TABLE public.notes;"
				}
			);
		}

		public async Task Up () {
			await m_DataContext.NonResultQueriesAsync (
				new string[] {
					"CREATE TABLE public.notes (id uuid NOT NULL DEFAULT uuid_generate_v4(), created timestamp without time zone NOT NULL DEFAULT now(), title text not null, body text not null, userid uuid NOT NULL, CONSTRAINT notes_pkey PRIMARY KEY (id), CONSTRAINT notes_users_userid FOREIGN KEY (userid) REFERENCES public.users (id) ON DELETE NO ACTION ON UPDATE NO ACTION);",
					"ALTER TABLE public.notes OWNER TO postgres;",
					$"INSERT INTO public.migrations(id) VALUES ('{nameof(NoteMigration_2)}')"
				}
			);
		}

	}

}
