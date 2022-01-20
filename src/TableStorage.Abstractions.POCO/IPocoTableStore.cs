using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableStorage.Abstractions.Models;

namespace TableStorage.Abstractions.POCO ;

	public interface IPocoTableStore<T, TPartitionKey, TRowKey>
	{
		/// <summary>
		///     Creates the table storage table.
		/// </summary>
		void CreateTable();

		/// <summary>
		///     Checks if the table storage table exists exists.
		/// </summary>
		/// <returns><c>true</c> if table exists, <c>false</c> otherwise.</returns>
		bool TableExists();

		/// <summary>
		///     Inserts the record into a table storage table.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <exception cref="ArgumentNullException">record</exception>
		void Insert(T record);

		/// <summary>
		///     Inserts the record if it does not exist, else updates the record.
		/// </summary>
		/// <param name="record">The record</param>
		/// <exception cref="ArgumentNullException">record</exception>
		void InsertOrReplace(T record);

		/// <summary>
		///     Inserts the records into a table storage table.
		/// </summary>
		/// <param name="records">The records.</param>
		/// <exception cref="ArgumentNullException">records</exception>
		void Insert(IEnumerable<T> records);

		/// <summary>
		///     Updates the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		void Update(T record);

		/// <summary>
		///     Updates the record without requiring an etag.
		/// </summary>
		/// <param name="record">The record.</param>
		void UpdateUsingWildcardEtag(T record);

		/// <summary>
		///     Deletes the specified record.
		/// </summary>
		/// <param name="record">The record.</param>
		void Delete(T record);

		/// <summary>
		///     Deletes the using recording without requiring an etag.
		/// </summary>
		/// <param name="record">The record.</param>
		void DeleteUsingWildcardEtag(T record);

		/// <summary>
		///     Deletes the table storage table.
		/// </summary>
		void DeleteTable();

		/// <summary>
		///     Gets the record by partition key and row key.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="rowKey">The row key.</param>
		/// <returns>T.</returns>
		T GetRecord(string partitionKey, string rowKey);

		/// <summary>
		///     Gets records by partition key.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetByPartitionKey(string partitionKey);

		/// <summary>
		///     Gets records by partition key, paged.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetByPartitionKeyPaged(string partitionKey, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKey(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetByRowKey(string rowKey);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByRowKeyPaged(string,int,string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetByRowKeyPaged(string rowKey, int pageSize = 100, string continuationTokenJson = null);

		/// <summary>
		///     Gets all records.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKey(string)" />
		/// </summary>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetAllRecords();

		/// <summary>
		///     Gets all records.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyPaged(string,int,string)" />
		/// </summary>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="pageToken">The page token.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetAllRecordsPaged(int pageSize = 100, string pageToken = null);

		/// <summary>
		///     Gets the record count.  This method may be slow if there is a high volume of
		///     data across many partitions
		/// </summary>
		/// <returns>System.Int32.</returns>
		int GetRecordCount();

		/// <summary>
		///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use Gets
		///     the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKey(string,System.Func{T,bool})" />
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetRecordsByFilter(Func<T, bool> filter);

		/// <summary>
		///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use Gets
		///     the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKey(string,System.Func{T,bool})" />
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <param name="start">The start.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetRecordsByFilter(Func<T, bool> filter, int start, int pageSize);

		/// <summary>
		///     Gets all records in an observable.  Note that the filter is applied after the table storage query, so prefer to use
		///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
		///     <see cref="GetByPartitionKeyPaged(string)" />
		/// </summary>
		/// <returns>IObservable&lt;T&gt;.</returns>
		IObservable<T> GetAllRecordsObservable();

		/// <summary>
		///     Gets the records by filter observable.  Note that the filter is applied after the table storage query, so prefer to
		///     use <see cref="GetByPartitionKeyPaged(string)" />
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <param name="start">The start.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <returns>IObservable&lt;T&gt;.</returns>
		IObservable<T> GetRecordsByFilterObservable(Func<T, bool> filter, int start, int pageSize);

		/// <summary>
		///     Creates the table storage table asynchronously.
		/// </summary>
		/// <returns>Task.</returns>
		Task CreateTableAsync();

		/// <summary>
		///     Checks if the table storage table exists asynchronously.
		/// </summary>
		/// <returns>Task&lt;System.Boolean&gt;.</returns>
		Task<bool> TableExistsAsync();

		/// <summary>
		///     Inserts the record into the table storage table, asynchronously.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <returns>Task.</returns>
		/// <exception cref="ArgumentNullException">record</exception>
		Task InsertAsync(T record);

		/// <summary>
		///     Inserts the records asynchronously.
		/// </summary>
		/// <param name="records">The records.</param>
		/// <returns>Task.</returns>
		/// <exception cref="ArgumentNullException">records</exception>
		Task InsertAsync(IEnumerable<T> records);

		/// <summary>
		///     Inserts the record if it does not exist, else updates the record.
		/// </summary>
		/// <param name="record">The record</param>
		/// <returns>Task</returns>
		/// <exception cref="ArgumentNullException">record</exception>
		Task InsertOrReplaceAsync(T record);

		/// <summary>
		///     Updates the record asynchronously.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <returns>Task.</returns>
		Task UpdateAsync(T record);

		/// <summary>
		///     Updates the using record without an etag asynchronously.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <returns>Task.</returns>
		Task UpdateUsingWildcardEtagAsync(T record);

		/// <summary>
		///     Deletes the record asynchronously.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <returns>Task.</returns>
		Task DeleteAsync(T record);

		/// <summary>
		///     Deletes the record without an etag asynchronously.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <returns>Task.</returns>
		Task DeleteUsingWildcardEtagAsync(T record);

		/// <summary>
		///     Deletes the table asynchronously.
		/// </summary>
		/// <returns>Task.</returns>
		Task DeleteTableAsync();

		/// <summary>
		///     get the  record by partition key and row key asyncronously."/>
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="rowKey">The row key.</param>
		/// <returns>Task&lt;T&gt;.</returns>
		Task<T> GetRecordAsync(string partitionKey, string rowKey);

		/// <summary>
		///     Gets the records by partition key asynchronously.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey);

		/// <summary>
		///     Gets the by records partition key paged asynchronous.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByPartitionKeyPagedAsync(string partitionKey, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the by records by row key asynchronously.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetByRowKeyAsync(string rowKey);

		/// <summary>
		///     Gets the by records by row key asynchronously.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByRowKeyPagedAsync(string rowKey, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets all records asynchronously.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetAllRecordsAsync();

		/// <summary>
		///     Gets all records asynchronously.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="pageToken">The page token.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetAllRecordsPagedAsync(int pageSize = 100, string pageToken = null);

		/// <summary>
		///     Gets the record count asynchronously.  This method may be slow if there is a high volume of
		///     data across many partitions
		/// </summary>
		/// <returns>Task&lt;System.Int32&gt;.</returns>
		Task<int> GetRecordCountAsync();

		/// <summary>
		///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use Gets
		///     the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string,System.Func{T,bool})" />
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <param name="start">The start.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetRecordsByFilterAsync(Func<T, bool> filter, int start, int pageSize);

		/// <summary>
		///     Gets the records by partition key async.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetByPartitionKey(string partitionKey, Func<T, bool> filter);

		/// <summary>
		///     ets the records by partition key async
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetByPartitionKeyPaged(string partitionKey, Func<T, bool> filter, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKey(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetByRowKey(string rowKey, Func<T, bool> filter);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKey(string)" />
		/// </summary>
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetByRowKeyPaged(string rowKey, Func<T, bool> filter, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets records by partition key as an asynchronous operation.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, Func<T, bool> filter);

		/// <summary>
		///     Gets records by partition key as an asynchronous operation.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByPartitionKeyPagedAsync(string partitionKey, Func<T, bool> filter,
			int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetByRowKeyAsync(string rowKey, Func<T, bool> filter);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByRowKeyPagedAsync(string rowKey, Func<T, bool> filter, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the record by partition key and row key.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="rowKey">The row key.</param>
		/// <returns>T.</returns>
		/// <exception cref="ArgumentNullException">
		///     partitionKey
		///     or
		///     rowKey
		/// </exception>
		T GetRecord(TPartitionKey partitionKey, TRowKey rowKey);

		/// <summary>
		///     Gets therecords by partition key.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetByPartitionKey(TPartitionKey partitionKey);

		/// <summary>
		///     Gets the records by partition key.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		/// <exception cref="ArgumentNullException">partitionKey</exception>
		IEnumerable<T> GetByPartitionKey(TPartitionKey partitionKey, Func<T, bool> filter);

		/// <summary>
		///     Gets the records by partition key paged.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetByPartitionKeyPaged(TPartitionKey partitionKey, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the records by partition key paged.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		/// <exception cref="ArgumentNullException">partitionKey</exception>
		PagedResult<T> GetByPartitionKeyPaged(TPartitionKey partitionKey, Func<T, bool> filter, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		IEnumerable<T> GetByRowKey(TRowKey rowKey);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>IEnumerable&lt;T&gt;.</returns>
		/// <exception cref="ArgumentNullException">rowKey</exception>
		IEnumerable<T> GetByRowKey(TRowKey rowKey, Func<T, bool> filter);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		PagedResult<T> GetByRowKeyPaged(TRowKey rowKey, int pageSize = 100, string continuationTokenJson = null);

		/// <summary>
		///     Gets records by row key. This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>PagedResult&lt;T&gt;.</returns>
		/// <exception cref="ArgumentNullException">rowKey</exception>
		PagedResult<T> GetByRowKeyPaged(TRowKey rowKey, Func<T, bool> filter, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the record by partition key and row key asynchronous.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="rowKey">The row key.</param>
		/// <returns>Task&lt;T&gt;.</returns>
		/// <exception cref="ArgumentNullException">
		///     partitionKey
		///     or
		///     rowKey
		/// </exception>
		Task<T> GetRecordAsync(TPartitionKey partitionKey, TRowKey rowKey);

		/// <summary>
		///     Gets the records by partition key asynchronous.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetByPartitionKeyAsync(TPartitionKey partitionKey);

		/// <summary>
		///     Gets the by records partition key asynchronous.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		/// <exception cref="ArgumentNullException">partitionKey</exception>
		Task<IEnumerable<T>> GetByPartitionKeyAsync(TPartitionKey partitionKey, Func<T, bool> filter);

		/// <summary>
		///     Gets the records by partition key paged asynchronous.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByPartitionKeyPagedAsync(TPartitionKey partitionKey, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the records by partition key paged asynchronous.
		/// </summary>
		/// <param name="partitionKey">The partition key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		/// <exception cref="ArgumentNullException">partitionKey</exception>
		Task<PagedResult<T>> GetByPartitionKeyPagedAsync(TPartitionKey partitionKey, Func<T, bool> filter,
			int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		Task<IEnumerable<T>> GetByRowKeyAsync(TRowKey rowKey);

		/// <summary>
		///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
		/// <exception cref="ArgumentNullException">rowKey</exception>
		Task<IEnumerable<T>> GetByRowKeyAsync(TRowKey rowKey, Func<T, bool> filter);

		/// <summary>
		///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByRowKeyPagedAsync(TRowKey rowKey, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
		///     data across many partitions, prefer to use
		///     <see cref="PocoTableStore{T,TPartitionKey,TRowKey}.GetByPartitionKeyAsync(string)" />
		/// </summary>
		/// <param name="rowKey">The row key.</param>
		/// <param name="filter">The filter.</param>
		/// <param name="pageSize">Size of the page.</param>
		/// <param name="continuationTokenJson">The continuation token json.</param>
		/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
		Task<PagedResult<T>> GetByRowKeyPagedAsync(TRowKey rowKey, Func<T, bool> filter, int pageSize = 100,
			string continuationTokenJson = null);

		/// <summary>
		///     Occurs when on table created.
		/// </summary>
		event Action<string, IPocoTableStore<T, TPartitionKey, TRowKey>> OnTableCreated;

		/// <summary>
		///     Occurs when on table created.
		/// </summary>
		event Func<string, IPocoTableStore<T, TPartitionKey, TRowKey>, Task> OnTableCreatedAsync;

		/// <summary>
		///     Occurs when on table deleted.
		/// </summary>
		event Action<string, IPocoTableStore<T, TPartitionKey, TRowKey>> OnTableDeleted;

		/// <summary>
		///     Occurs when on table deleted.
		/// </summary>
		event Func<string, IPocoTableStore<T, TPartitionKey, TRowKey>, Task> OnTableDeletedAsync;

		/// <summary>
		///     Occurs when on record inserted or updated.
		/// </summary>
		event Action<T> OnRecordInsertedOrUpdated;

		/// <summary>
		///     Occurs when on record inserted or updated.
		/// </summary>
		event Func<T, Task> OnRecordInsertedOrUpdatedAsync;


		/// <summary>
		///     Occurs when on records inserted.
		/// </summary>
		event Action<IEnumerable<T>> OnRecordsInserted;

		/// <summary>
		///     Occurs when on records inserted.
		/// </summary>
		event Func<IEnumerable<T>, Task> OnRecordsInsertedAsync;

		/// <summary>
		///     Occurs when on record deleted.
		/// </summary>
		event Action<T> OnRecordDeleted;

		/// <summary>
		///     Occurs when on record deleted.
		/// </summary>
		event Func<T, Task> OnRecordDeletedAsync;
	}