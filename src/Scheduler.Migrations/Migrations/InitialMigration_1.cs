using Scheduler.Common.DataContexts;
using System;
using System.Threading.Tasks;

namespace Scheduler.Migrations {

	public class InitialMigration_1 : IMigration {

		private readonly IDataContext m_DataContext;

		public InitialMigration_1 (IDataContext dataContext ) => m_DataContext = dataContext ?? throw new ArgumentNullException ( nameof ( dataContext ) );

		public string Description => "Migration create initial table set.";

		public async Task Down () {
			await m_DataContext.NonResultQueriesAsync (
				new string[] {
					"DROP TABLE public.tokens;",
					"DROP TABLE public.users;",
					"DROP TABLE public.migrations;",
					"DROP EXTENSION \"uuid-ossp\";"
				}
			);
		}

		public async Task Up () {
			await m_DataContext.NonResultQueriesAsync (
				new string[] {
					"CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";",
					"CREATE TABLE public.migrations (id text NOT NULL, created timestamp without time zone NOT NULL DEFAULT now(), CONSTRAINT migrations_pkey PRIMARY KEY (id));",
					"ALTER TABLE public.migrations OWNER TO postgres;",
					"CREATE TABLE public.users (id uuid NOT NULL DEFAULT uuid_generate_v4(), login text NOT NULL, password text NOT NULL, email text, CONSTRAINT users_pkey PRIMARY KEY (id));",
					"ALTER TABLE public.users OWNER TO postgres;",
					"CREATE TABLE public.tokens (id uuid NOT NULL DEFAULT uuid_generate_v4(), userid uuid NOT NULL, logined timestamp without time zone NOT NULL DEFAULT now(), CONSTRAINT tokens_pkey PRIMARY KEY (id), CONSTRAINT tokens_users_userid FOREIGN KEY (userid) REFERENCES public.users (id) ON DELETE NO ACTION ON UPDATE NO ACTION)",
					"ALTER TABLE public.tokens OWNER TO postgres;",
					$"INSERT INTO public.migrations(id) VALUES ('{nameof(InitialMigration_1)}')"
				}
			);
		}

	}

}
