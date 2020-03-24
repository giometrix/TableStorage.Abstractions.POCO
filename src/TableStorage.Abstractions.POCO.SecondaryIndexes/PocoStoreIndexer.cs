using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using TableStorage.Abstractions.Models;

namespace TableStorage.Abstractions.POCO.SecondaryIndexes
{
	public static class PocoStoreIndexer
	{
		private static readonly Dictionary<string, object> _indexes = new Dictionary<string, object>();

		private static object _indexLock = new object();

		/// <summary>
		/// Adds an index to a table, but does not seed it with any data.  Records not already in the index will not be found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <typeparam name="TIndexRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="indexStore"></param>
		public static void AddIndex<T, TPartitionKey, TRowKey, TIndexPartitionKey, TIndexRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName,
			IPocoTableStore<T, TIndexPartitionKey, TIndexRowKey> indexStore)
		{
			lock (_indexLock)
			{
				if (_indexes.ContainsKey(indexName))
				{
					throw new ArgumentException($"{indexName} has already been added");
				}

				_indexes[indexName] = indexStore;
				tableStore.OnRecordInsertedOrUpdated += indexStore.InsertOrReplace;
				tableStore.OnRecordInsertedOrUpdatedAsync += indexStore.InsertOrReplaceAsync;
				tableStore.OnRecordsInserted += indexStore.Insert;
				tableStore.OnRecordsInsertedAsync += indexStore.InsertAsync;
				tableStore.OnRecordDeleted += indexStore.Delete;
				tableStore.OnRecordDeletedAsync += indexStore.DeleteAsync;
				tableStore.OnTableDeleted += (name, table) =>
				{
					lock (_indexLock)
					{
						_indexes.Remove(indexName);
					}
					indexStore.DeleteTable();
				};
				tableStore.OnTableDeletedAsync += async (name, table) =>
				{
					lock (_indexLock)
					{
						_indexes.Remove(indexName);
					}
					await indexStore.DeleteTableAsync();
				};
			}

		}

		
		/// <summary>
		/// Removes an index but does not drop it.  Newly added records will not be indexed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		public static void RemoveIndex<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName)
		{
			lock (_indexLock)
			{
				if (_indexes.ContainsKey(indexName))
				{
					_indexes.Remove(indexName);
				}
			}
		}

		/// <summary>
		/// Removes and drops (deletes) the index table
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		public static void DropIndex<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName)
		{
			dynamic indexStore = _indexes[indexName];
			indexStore.DeleteTable();
			RemoveIndex(tableStore, indexName);
		}

		/// <summary>
		/// Removes and drops (deletes) the index table asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		public static Task DropIndexAsync<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName)
		{
			dynamic indexStore = _indexes[indexName];
			RemoveIndex(tableStore, indexName);
			return indexStore.DeleteTableAsync();
		}

		/// <summary>
		/// Gets a record from the index table
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="rowKey"></param>
		/// <returns></returns>
		public static T GetRecordByIndex<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, string partitionKey, string rowKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetRecord(partitionKey, rowKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets a record from the index table
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <typeparam name="TIndexRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="rowKey"></param>
		/// <returns></returns>
		public static T GetRecordByIndex<T, TPartitionKey, TRowKey, TIndexPartitionKey, TIndexRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, TIndexPartitionKey partitionKey, TIndexRowKey rowKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetRecord(partitionKey, rowKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets a record from the index table asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="rowKey"></param>
		/// <returns></returns>
		public static Task<T> GetRecordByIndexAsync<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, string partitionKey, string rowKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetRecordAsync(partitionKey, rowKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets a record from the index table asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <typeparam name="TIndexRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="rowKey"></param>
		/// <returns></returns>
		public static Task<T> GetRecordByIndexAsync<T, TPartitionKey, TRowKey, TIndexPartitionKey, TIndexRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, TIndexPartitionKey partitionKey, TIndexRowKey rowKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetRecordAsync(partitionKey, rowKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetByIndexPartitionKey<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, string partitionKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKey(partitionKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key asynchronously
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <returns></returns>
		public static Task<IEnumerable<T>> GetByIndexPartitionKeyAsync<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, string partitionKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKeyAsync(partitionKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetByIndexPartitionKey<T, TPartitionKey, TRowKey, TIndexPartitionKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, TIndexPartitionKey partitionKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKey(partitionKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <returns></returns>
		public static Task<IEnumerable<T>> GetByIndexPartitionKeyAsync<T, TPartitionKey, TRowKey, TIndexPartitionKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, TIndexPartitionKey partitionKey)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKeyAsync(partitionKey);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}


		/// <summary>
		/// Gets all of the records by index's partition key in a paged record set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="pageSize"></param>
		/// <param name="continuationTokenJson"></param>
		/// <returns></returns>
		public static PagedResult<T> GetByIndexPartitionKeyPaged<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, string partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKeyPaged(partitionKey, pageSize, continuationTokenJson);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key in a paged record set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="pageSize"></param>
		/// <param name="continuationTokenJson"></param>
		/// <returns></returns>
		public static PagedResult<T> GetByIndexPartitionKeyPaged<T, TPartitionKey, TRowKey, TIndexPartitionKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, TIndexPartitionKey partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKeyPaged(partitionKey, pageSize, continuationTokenJson);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key in a paged record set, async
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="pageSize"></param>
		/// <param name="continuationTokenJson"></param>
		/// <returns></returns>
		public static Task<PagedResult<T>> GetByIndexPartitionKeyPagedAsync<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, string partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKeyPagedAsync(partitionKey, pageSize, continuationTokenJson);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Gets all of the records by index's partition key in a paged record set, async
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <param name="partitionKey"></param>
		/// <param name="pageSize"></param>
		/// <param name="continuationTokenJson"></param>
		/// <returns></returns>
		public static Task<PagedResult<T>> GetByIndexPartitionKeyPagedAsync<T, TPartitionKey, TRowKey, TIndexPartitionKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, TIndexPartitionKey partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore.GetByPartitionKeyPagedAsync(partitionKey, pageSize, continuationTokenJson);
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}

		/// <summary>
		/// Returns the table store for the underlying index.  Use this for the full feature set of the TableStorage.Abstractions.POCO library, but please note that data mutations (insert, update, delete) will not be tracked.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey"></typeparam>
		/// <typeparam name="TRowKey"></typeparam>
		/// <typeparam name="TIndexPartitionKey"></typeparam>
		/// <typeparam name="TIndexRowKey"></typeparam>
		/// <param name="tableStore"></param>
		/// <param name="indexName"></param>
		/// <returns></returns>
		public static IPocoTableStore<T, TIndexPartitionKey, TIndexRowKey> Index<T, TPartitionKey, TRowKey, TIndexPartitionKey, TIndexRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName)
		{
			try
			{
				dynamic indexStore = _indexes[indexName];
				return indexStore;
			}
			catch (KeyNotFoundException e)
			{
				throw new ArgumentException($"{indexName} is not a defined secondary index");
			}
		}


	}
}