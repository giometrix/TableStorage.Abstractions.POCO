using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using TableStorage.Abstractions.Models;

namespace TableStorage.Abstractions.POCO.SecondaryIndexes
{
	public static class PocoStoreIndexer
	{
		private static readonly Dictionary<string, dynamic> _indexes = new Dictionary<string, dynamic>();

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
				tableStore.OnRecordDeleted += obj =>
				{
					try
					{
						indexStore.Delete(obj);
					}
					catch (StorageException e) when (e.Message == "Not Found")
					{
						// if the index row wasn't there there is nothing to do
					}
				};
				tableStore.OnRecordDeletedAsync += async obj =>
				{
					try
					{
						await indexStore.DeleteAsync(obj);
					}
					catch (StorageException e) when (e.Message == "Not Found")
					{
						// if the index row wasn't there there is nothing to do
					}

				};
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


		/// <summary>
		/// Reindexes a table.  You will need to call this for existing tables that have never had an index, or if things get out of sync.  This can take a while.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TPartitionKey">The type of the t partition key.</typeparam>
		/// <typeparam name="TRowKey">The type of the t row key.</typeparam>
		/// <param name="tableStore">The table store.</param>
		/// <param name="indexName">Name of the index.</param>
		/// <param name="maxDegreeOfParallelism">The maximum degree of parallelism.</param>
		/// <param name="recordsIndexedCallback">The records indexed callback.</param>
		/// <param name="failedIndexCallback">The failed index callback.</param>
		/// <exception cref="ArgumentException"></exception>
		public static async Task ReindexAsync<T, TPartitionKey, TRowKey>(this IPocoTableStore<T, TPartitionKey, TRowKey> tableStore, string indexName, int? maxDegreeOfParallelism = null, Action<int> recordsIndexedCallback = null, Action<T, Exception> failedIndexCallback = null)
		{
			maxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount * 20;

			string pageToken = null;
			int count = 0;
			using (var semaphore = new SemaphoreSlim(maxDegreeOfParallelism.Value, maxDegreeOfParallelism.Value))
			{
				try
				{
					var indexStore = _indexes[indexName];
					do
					{
						var result = await tableStore.GetAllRecordsPagedAsync(1000, pageToken);
						pageToken = result.ContinuationToken;
						var insertOrReplaceAsync = indexStore.GetType().GetMethod("InsertOrReplaceAsync");
						if (result.Items.Count > 0)
						{
							foreach (var record in result.Items)
							{
								await semaphore.WaitAsync(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
								//Task task = indexStore.InsertOrReplaceAsync(record); //this line worked in the unit tests but not in a console app.  Not sure why.
								var task = (Task) insertOrReplaceAsync.Invoke(indexStore, new object[] { record });
								task.ContinueWith(r =>
								{
									if (r.IsFaulted)
									{
										failedIndexCallback?.Invoke(record, r.Exception);
									}
									Interlocked.Increment(ref count);
									semaphore.Release(1);
								});

							}
						}

						recordsIndexedCallback?.Invoke(count);
					} while (pageToken != null);

					while (semaphore.CurrentCount < maxDegreeOfParallelism)
					{

						await Task.Delay(5).ConfigureAwait(false);
					}
					recordsIndexedCallback?.Invoke(count);
				}
				catch (KeyNotFoundException e)
				{
					throw new ArgumentException($"{indexName} is not a defined secondary index");
				}
			}
		}


	}
}