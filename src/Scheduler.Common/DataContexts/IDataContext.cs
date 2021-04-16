using SqlKata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scheduler.Common.DataContexts {

	public interface IDataContext {

		Task<IEnumerable<T>> GetItemsAsync<T> ( Query query );

		Task<T?> GetItemAsync<T> ( Query query );

		T? GetItem<T> ( Query query );

		Task AddOrUpdateAsync<T> ( Query query , T model , bool insert = false );

		void AddOrUpdate<T> ( Query query , T model , bool insert = false );

		Task NonResultQueryAsync ( Query query );

		Task NonResultQueryAsync ( string query );

		Task NonResultQueriesAsync ( IEnumerable<string> queries );

	}

}
