using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using TableStorage.Abstractions.Models;
using TableStorage.Abstractions.Store;
using Useful.Extensions;

namespace TableStorage.Abstractions.POCO;

public class PocoTableStore<T, TPartitionKey, TRowKey> : IPocoTableStore<T, TPartitionKey, TRowKey>
	where T : class, new()
{
	private readonly IKeysConverter<T, TPartitionKey, TRowKey> _keysConverter;
	private readonly string _tableName;
	private readonly TableStore<TableEntity> _tableStore;


	/// <summary>
	///     Initializes a new instance of the <see cref="PocoTableStore{T, TPartitionKey, TRowKey}" /> class.
	/// </summary>
	/// <param name="tableName">Name of the azure storage table.</param>
	/// <param name="storageConnectionString">The azure storage connection string.</param>
	/// <param name="partitionProperty">The property to be used as a partition key.</param>
	/// <param name="rowProperty">The property to be used as a row key.</param>
	/// <param name="options">The table storage options.</param>
	/// <param name="ignoredProperties">The properties that should not be serialized.</param>
	/// <exception cref="ArgumentNullException">
	///     tableName
	///     or
	///     storageConnectionString
	///     or
	///     partitionProperty
	///     or
	///     rowProperty
	/// </exception>
	public PocoTableStore(string tableName, string storageConnectionString,
		Expression<Func<T, object>> partitionProperty,
		Expression<Func<T, object>> rowProperty, PocoTableStoreOptions options = null,
		params Expression<Func<T, object>>[] ignoredProperties)
	{
		if (tableName == null) {
			throw new ArgumentNullException(nameof(tableName));
		}

		if (storageConnectionString == null) {
			throw new ArgumentNullException(nameof(storageConnectionString));
		}

		if (partitionProperty == null) {
			throw new ArgumentNullException(nameof(partitionProperty));
		}

		if (rowProperty == null) {
			throw new ArgumentNullException(nameof(rowProperty));
		}

		options ??= new PocoTableStoreOptions();
		_keysConverter = new SimpleKeysConverter<T, TPartitionKey, TRowKey>(partitionProperty, rowProperty,
			options.JsonSerializerSettings, default,
			ignoredProperties);
		_tableName = tableName;
		_tableStore = new TableStore<TableEntity>(tableName, storageConnectionString, options.TableStorageOptions);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="PocoTableStore{T, TPartitionKey, TRowKey}" /> class.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <param name="storageConnectionString">The storage connection string.</param>
	/// <param name="keysConverter">The table converter.</param>
	/// <param name="tableStorageOptions">The table storage options.</param>
	/// <exception cref="ArgumentNullException">
	///     tableName
	///     or
	///     storageConnectionString
	///     or
	///     tableConverter
	/// </exception>
	public PocoTableStore(string tableName, string storageConnectionString,
		SimpleKeysConverter<T, TPartitionKey, TRowKey> keysConverter, TableStorageOptions tableStorageOptions = null)
	{
		if (tableName == null) {
			throw new ArgumentNullException(nameof(tableName));
		}

		if (storageConnectionString == null) {
			throw new ArgumentNullException(nameof(storageConnectionString));
		}

		if (keysConverter == null) {
			throw new ArgumentNullException(nameof(keysConverter));
		}


		_keysConverter = keysConverter;
		_tableName = tableName;
		tableStorageOptions ??= new TableStorageOptions();
		_tableStore = new TableStore<TableEntity>(tableName, storageConnectionString, tableStorageOptions);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="PocoTableStore{T, TPartitionKey, TRowKey}" /> class.
	/// </summary>
	/// <param name="tableName">Name of the table.</param>
	/// <param name="storageConnectionString">The storage connection string.</param>
	/// <param name="keysConverter">The table converter.</param>
	/// <param name="tableStorageOptions">The table storage options.</param>
	/// <exception cref="ArgumentNullException">
	///     tableName
	///     or
	///     storageConnectionString
	///     or
	///     tableConverter
	/// </exception>
	public PocoTableStore(string tableName, string storageConnectionString,
		CalculatedKeysConverter<T, TPartitionKey, TRowKey> keysConverter,
		TableStorageOptions tableStorageOptions = null)
	{
		if (tableName == null) {
			throw new ArgumentNullException(nameof(tableName));
		}

		if (storageConnectionString == null) {
			throw new ArgumentNullException(nameof(storageConnectionString));
		}

		if (keysConverter == null) {
			throw new ArgumentNullException(nameof(keysConverter));
		}

		_keysConverter = keysConverter;
		_tableName = tableName;
		tableStorageOptions = tableStorageOptions ?? new TableStorageOptions();
		_tableStore = new TableStore<TableEntity>(tableName, storageConnectionString, tableStorageOptions);
	}

	/// <summary>
	///     Creates the table storage table.
	/// </summary>
	public void CreateTable()
	{
		_tableStore.CreateTable();
		OnTableCreated?.Invoke(_tableName, this);
	}

	/// <summary>
	///     Checks if the table storage table exists exists.
	/// </summary>
	/// <returns><c>true</c> if table exists, <c>false</c> otherwise.</returns>
	public bool TableExists()
	{
		return _tableStore.TableExists();
	}

	/// <summary>
	///     Inserts the record into a table storage table.
	/// </summary>
	/// <param name="record">The record.</param>
	/// <exception cref="ArgumentNullException">record</exception>
	public void Insert(T record)
	{
		if (record == null) {
			throw new ArgumentNullException(nameof(record));
		}

		TableEntity entity = CreateEntity(record);

		_tableStore.Insert(entity);
		OnRecordInsertedOrUpdated?.Invoke(record);
	}

	/// <summary>
	///     Inserts or replaces the record
	/// </summary>
	/// <param name="record"></param>
	/// <exception cref="ArgumentNullException">record</exception>
	public void InsertOrReplace(T record)
	{
		if (record == null) {
			throw new ArgumentNullException(nameof(record));
		}

		TableEntity entity = CreateEntity(record);

		_tableStore.InsertOrReplace(entity);
		OnRecordInsertedOrUpdated?.Invoke(record);
	}

	/// <summary>
	///     Inserts the records into a table storage table.
	/// </summary>
	/// <param name="records">The records.</param>
	/// <exception cref="ArgumentNullException">records</exception>
	public void Insert(IEnumerable<T> records)
	{
		if (records == null) {
			throw new ArgumentNullException(nameof(records));
		}

		IEnumerable<TableEntity> entities = CreateEntities(records);
		_tableStore.Insert(entities);
		OnRecordsInserted?.Invoke(records);
	}

	/// <summary>
	///     Updates the specified record.
	/// </summary>
	/// <param name="record">The record.</param>
	public void Update(T record)
	{
		TableEntity entity = CreateEntityWithEtag(record);

		_tableStore.Update(entity);
		OnRecordInsertedOrUpdated?.Invoke(record);
	}

	/// <summary>
	///     Updates the record without requiring an etag.
	/// </summary>
	/// <param name="record">The record.</param>
	public void UpdateUsingWildcardEtag(T record)
	{
		TableEntity entity = CreateEntity(record);
		_tableStore.UpdateUsingWildcardEtag(entity);
		OnRecordInsertedOrUpdated?.Invoke(record);
	}

	/// <summary>
	///     Deletes the specified record.
	/// </summary>
	/// <param name="record">The record.</param>
	public void Delete(T record)
	{
		TableEntity entity = CreateEntityWithEtag(record);
		_tableStore.Delete(entity);
		OnRecordDeleted?.Invoke(record);
	}

	/// <summary>
	///     Deletes the using recording without requiring an etag.
	/// </summary>
	/// <param name="record">The record.</param>
	public void DeleteUsingWildcardEtag(T record)
	{
		TableEntity entity = CreateEntity(record);
		_tableStore.DeleteUsingWildcardEtag(entity);
		OnRecordDeleted?.Invoke(record);
	}

	/// <summary>
	///     Deletes the table storage table.
	/// </summary>
	public void DeleteTable()
	{
		_tableStore.DeleteTable();
		OnTableDeleted?.Invoke(_tableName, this);
	}

	/// <summary>
	///     Gets the record by partition key and row key.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="rowKey">The row key.</param>
	/// <returns>T.</returns>
	public T GetRecord(string partitionKey, string rowKey)
	{
		TableEntity entity = null;
		try {
			entity = _tableStore.GetRecord(partitionKey, rowKey);
			return CreateRecord(entity);
		}
		catch (RequestFailedException e) when (e.Status == 404) {
			// do nothing, we will return null to maintain backward compability with behavior of previous table storage libraries
		}

		return CreateRecord(entity);
	}

	/// <summary>
	///     Gets records by partition key.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetByPartitionKey(string partitionKey)
	{
		return GetByPartitionKey(partitionKey, null);
	}

	/// <summary>
	///     Gets records by partition key, paged.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetByPartitionKeyPaged(string partitionKey, int pageSize = 100,
		string continuationTokenJson = null)
	{
		return GetByPartitionKeyPaged(partitionKey, null, pageSize, continuationTokenJson);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKey(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetByRowKey(string rowKey)
	{
		return GetByRowKey(rowKey, null);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByRowKeyPaged(string,int,string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetByRowKeyPaged(string rowKey, int pageSize = 100, string continuationTokenJson = null)
	{
		return GetByRowKeyPaged(rowKey, null, pageSize, continuationTokenJson);
	}

	/// <summary>
	///     Gets all records.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKey(string)" />
	/// </summary>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetAllRecords()
	{
		return CreateRecords(_tableStore.GetAllRecords());
	}

	/// <summary>
	///     Gets all records.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyPaged(string,int,string)" />
	/// </summary>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="pageToken">The page token.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetAllRecordsPaged(int pageSize = 100, string pageToken = null)
	{
		return CreatePagedResult(_tableStore.GetAllRecordsPaged(pageSize, pageToken));
	}

	/// <summary>
	///     Gets the record count.  This method may be slow if there is a high volume of
	///     data across many partitions
	/// </summary>
	/// <returns>System.Int32.</returns>
	public int GetRecordCount()
	{
		return _tableStore.GetRecordCount();
	}

	/// <summary>
	///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use Gets
	///     the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
	///     <see cref="GetByPartitionKey(string, Func&lt;T, bool>)" />
	/// </summary>
	/// <param name="filter">The filter.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetRecordsByFilter(Func<T, bool> filter)
	{
		return GetAllRecords().Where(filter);
	}

	/// <summary>
	///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use Gets
	///     the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
	///     <see cref="GetByPartitionKey(string, Func&lt;T, bool>)" />
	/// </summary>
	/// <param name="filter">The filter.</param>
	/// <param name="start">The start.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetRecordsByFilter(Func<T, bool> filter, int start, int pageSize)
	{
		return GetRecordsByFilter(filter).Page(start, pageSize);
	}

	/// <summary>
	///     Gets all records in an observable.  Note that the filter is applied after the table storage query, so prefer to use
	///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
	///     <see cref="GetByPartitionKeyPaged(string)" />
	/// </summary>
	/// <returns>IObservable&lt;T&gt;.</returns>
	public IObservable<T> GetAllRecordsObservable()
	{
		return Observable.Create<T>(o => {
			foreach (T result in GetAllRecords()) {
				o.OnNext(result);
			}

			return Disposable.Empty;
		});
	}

	/// <summary>
	///     Gets the records by filter observable.  Note that the filter is applied after the table storage query, so prefer to
	///     use <see cref="GetByPartitionKeyPaged(string)" />
	/// </summary>
	/// <param name="filter">The filter.</param>
	/// <param name="start">The start.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <returns>IObservable&lt;T&gt;.</returns>
	public IObservable<T> GetRecordsByFilterObservable(Func<T, bool> filter, int start, int pageSize)
	{
		return Observable.Create<T>(o => {
			foreach (T result in GetAllRecords().Where(filter).Page(start, pageSize)) {
				o.OnNext(result);
			}

			return Disposable.Empty;
		});
	}

	/// <summary>
	///     Gets all records asynchronously.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public async Task<IEnumerable<T>> GetAllRecordsAsync(CancellationToken cancellationToken = default)
	{
		return CreateRecords(await _tableStore.GetAllRecordsAsync(cancellationToken).ConfigureAwait(false));
	}

	/// <summary>
	///     Gets all records asynchronously.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="pageToken">The page token.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public async Task<PagedResult<T>> GetAllRecordsPagedAsync(int pageSize = 100, string pageToken = null, CancellationToken cancellationToken = default)
	{
		return CreatePagedResult(await _tableStore.GetAllRecordsPagedAsync(pageSize, pageToken, cancellationToken).ConfigureAwait(false));
	}

	/// <summary>
	///     Gets the record count asynchronously.  This method may be slow if there is a high volume of
	///     data across many partitions
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;System.Int32&gt;.</returns>
	public Task<int> GetRecordCountAsync(CancellationToken cancellationToken = default)
	{
		return _tableStore.GetRecordCountAsync(cancellationToken);
	}

	/// <summary>
	///     Gets the records by filter.  Note that the filter is applied after the table storage query, so prefer to use Gets
	///     the records by filter.  Note that the filter is applied after the table storage query, so prefer to use
	///     <see cref="GetByPartitionKeyAsync(string, Func&lt;T, bool>)" />
	/// </summary>
	/// <param name="filter">The filter.</param>
	/// <param name="start">The start.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public async Task<IEnumerable<T>> GetRecordsByFilterAsync(Func<T, bool> filter, int start, int pageSize, CancellationToken cancellationToken = default)
	{
		IEnumerable<T> a = await GetAllRecordsAsync(cancellationToken).ConfigureAwait(false);
		IEnumerable<T> data = a.Where(filter).Page(start, pageSize);

		return data;
	}

	/// <summary>
	///     Gets the records by partition key async.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetByPartitionKey(string partitionKey, Func<T, bool> filter)
	{
		IEnumerable<TableEntity> entities = _tableStore.GetByPartitionKey(partitionKey);
		IEnumerable<T> records = CreateRecords(entities);

		if (filter != null) {
			records = records.Where(filter);
		}

		return records;
	}

	/// <summary>
	///     ets the records by partition key async
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetByPartitionKeyPaged(string partitionKey, Func<T, bool> filter, int pageSize = 100,
		string continuationTokenJson = null)
	{
		PagedResult<TableEntity> result =
			_tableStore.GetByPartitionKeyPaged(partitionKey, pageSize, continuationTokenJson);
		return CreatePagedResult(result, filter);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKey(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetByRowKey(string rowKey, Func<T, bool> filter)
	{
		IEnumerable<T> records = CreateRecords(_tableStore.GetByRowKey(rowKey));

		if (filter != null) {
			records = records.Where(filter);
		}

		return records;
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKey(string)" />
	/// </summary>
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetByRowKeyPaged(string rowKey, Func<T, bool> filter, int pageSize = 100,
		string continuationTokenJson = null)
	{
		PagedResult<TableEntity> result = _tableStore.GetByRowKeyPaged(rowKey, pageSize, continuationTokenJson);
		return CreatePagedResult(result, filter);
	}

	/// <summary>
	///     Gets records by partition key as an asynchronous operation.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public async Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, Func<T, bool> filter, CancellationToken cancellationToken = default)
	{
		IEnumerable<TableEntity> entities =
			await _tableStore.GetByPartitionKeyAsync(partitionKey, cancellationToken).ConfigureAwait(false);
		IEnumerable<T> records = CreateRecords(entities);
		if (filter != null) {
			records = records.Where(filter);
		}

		return records;
	}

	/// <summary>
	///     Gets records by partition key as an asynchronous operation.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public async Task<PagedResult<T>> GetByPartitionKeyPagedAsync(string partitionKey, Func<T, bool> filter,
		int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		PagedResult<TableEntity> result = await _tableStore
			.GetByPartitionKeyPagedAsync(partitionKey, pageSize, continuationTokenJson, cancellationToken)
			.ConfigureAwait(false);
		return CreatePagedResult(result, filter);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public async Task<IEnumerable<T>> GetByRowKeyAsync(string rowKey, Func<T, bool> filter, CancellationToken cancellationToken = default)
	{
		IEnumerable<T> records = CreateRecords(await _tableStore.GetByRowKeyAsync(rowKey, cancellationToken).ConfigureAwait(false));

		if (filter != null) {
			records = records.Where(filter);
		}


		return records;
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public async Task<PagedResult<T>> GetByRowKeyPagedAsync(string rowKey, Func<T, bool> filter, int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		PagedResult<TableEntity> result = await _tableStore
			.GetByRowKeyPagedAsync(rowKey, pageSize, continuationTokenJson, cancellationToken).ConfigureAwait(false);
		return CreatePagedResult(result, filter);
	}

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
	public T GetRecord(TPartitionKey partitionKey, TRowKey rowKey)
	{
		if (partitionKey is null) {
			throw new ArgumentNullException(nameof(partitionKey));
		}

		if (rowKey is null) {
			throw new ArgumentNullException(nameof(rowKey));
		}


		return GetRecord(GetPartitionKeyString(partitionKey), GetRowKeyString(rowKey));
	}

	/// <summary>
	///     Gets the records by partition key.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetByPartitionKey(TPartitionKey partitionKey)
	{
		return GetByPartitionKey(partitionKey, null);
	}

	/// <summary>
	///     Gets the records by partition key.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	/// <exception cref="ArgumentNullException">partitionKey</exception>
	public IEnumerable<T> GetByPartitionKey(TPartitionKey partitionKey, Func<T, bool> filter)
	{
		if (partitionKey == null) {
			throw new ArgumentNullException(nameof(partitionKey));
		}

		return GetByPartitionKey(GetPartitionKeyString(partitionKey), filter);
	}

	/// <summary>
	///     Gets the records by partition key paged.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetByPartitionKeyPaged(TPartitionKey partitionKey, int pageSize = 100,
		string continuationTokenJson = null)
	{
		return GetByPartitionKeyPaged(partitionKey, null, pageSize, continuationTokenJson);
	}

	/// <summary>
	///     Gets the records by partition key paged.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	/// <exception cref="ArgumentNullException">partitionKey</exception>
	public PagedResult<T> GetByPartitionKeyPaged(TPartitionKey partitionKey, Func<T, bool> filter, int pageSize = 100,
		string continuationTokenJson = null)
	{
		if (partitionKey == null) {
			throw new ArgumentNullException(nameof(partitionKey));
		}

		return GetByPartitionKeyPaged(GetPartitionKeyString(partitionKey), filter, pageSize, continuationTokenJson);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	public IEnumerable<T> GetByRowKey(TRowKey rowKey)
	{
		return GetByRowKey(rowKey, null);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <returns>IEnumerable&lt;T&gt;.</returns>
	/// <exception cref="ArgumentNullException">rowKey</exception>
	public IEnumerable<T> GetByRowKey(TRowKey rowKey, Func<T, bool> filter)
	{
		if (rowKey == null) {
			throw new ArgumentNullException(nameof(rowKey));
		}

		return GetByRowKey(GetRowKeyString(rowKey), filter);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	public PagedResult<T> GetByRowKeyPaged(TRowKey rowKey, int pageSize = 100, string continuationTokenJson = null)
	{
		return GetByRowKeyPaged(rowKey, null, pageSize, continuationTokenJson);
	}

	/// <summary>
	///     Gets records by row key. This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <returns>PagedResult&lt;T&gt;.</returns>
	/// <exception cref="ArgumentNullException">rowKey</exception>
	public PagedResult<T> GetByRowKeyPaged(TRowKey rowKey, Func<T, bool> filter, int pageSize = 100,
		string continuationTokenJson = null)
	{
		if (rowKey == null) {
			throw new ArgumentNullException(nameof(rowKey));
		}

		return GetByRowKeyPaged(GetRowKeyString(rowKey), filter, pageSize, continuationTokenJson);
	}

	/// <summary>
	///     Gets the record by partition key and row key asynchronous.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="rowKey">The row key.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;T&gt;.</returns>
	/// <exception cref="ArgumentNullException">
	///     partitionKey
	///     or
	///     rowKey
	/// </exception>
	public Task<T> GetRecordAsync(TPartitionKey partitionKey, TRowKey rowKey, CancellationToken cancellationToken = default)
	{
		if (partitionKey == null) {
			throw new ArgumentNullException(nameof(partitionKey));
		}

		if (rowKey == null) {
			throw new ArgumentNullException(nameof(rowKey));
		}

		return GetRecordAsync(GetPartitionKeyString(partitionKey), GetRowKeyString(rowKey), cancellationToken);
	}

	/// <summary>
	///     Gets the records by partition key asynchronous.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public Task<IEnumerable<T>> GetByPartitionKeyAsync(TPartitionKey partitionKey, CancellationToken cancellationToken = default)
	{
		return GetByPartitionKeyAsync(partitionKey, null, cancellationToken);
	}

	/// <summary>
	///     Gets the by records partition key asynchronous.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	/// <exception cref="ArgumentNullException">partitionKey</exception>
	public Task<IEnumerable<T>> GetByPartitionKeyAsync(TPartitionKey partitionKey, Func<T, bool> filter, CancellationToken cancellationToken = default)
	{
		if (partitionKey == null) {
			throw new ArgumentNullException(nameof(partitionKey));
		}

		return GetByPartitionKeyAsync(GetPartitionKeyString(partitionKey), filter, cancellationToken);
	}

	/// <summary>
	///     Gets the records by partition key paged asynchronous.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public Task<PagedResult<T>> GetByPartitionKeyPagedAsync(TPartitionKey partitionKey, int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		return GetByPartitionKeyPagedAsync(partitionKey, null, pageSize, continuationTokenJson, cancellationToken);
	}

	/// <summary>
	///     Gets the records by partition key paged asynchronous.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	/// <exception cref="ArgumentNullException">partitionKey</exception>
	public Task<PagedResult<T>> GetByPartitionKeyPagedAsync(TPartitionKey partitionKey, Func<T, bool> filter,
		int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		if (partitionKey == null) {
			throw new ArgumentNullException(nameof(partitionKey));
		}

		return GetByPartitionKeyPagedAsync(GetPartitionKeyString(partitionKey), filter, pageSize,
			continuationTokenJson, cancellationToken);
	}

	/// <summary>
	///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public Task<IEnumerable<T>> GetByRowKeyAsync(TRowKey rowKey, CancellationToken cancellationToken = default)
	{
		return GetByRowKeyAsync(rowKey, null, cancellationToken);
	}

	/// <summary>
	///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	/// <exception cref="ArgumentNullException">rowKey</exception>
	public Task<IEnumerable<T>> GetByRowKeyAsync(TRowKey rowKey, Func<T, bool> filter, CancellationToken cancellationToken = default)
	{
		if (rowKey == null) {
			throw new ArgumentNullException(nameof(rowKey));
		}

		return GetByRowKeyAsync(GetRowKeyString(rowKey), filter, cancellationToken);
	}

	/// <summary>
	///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public Task<PagedResult<T>> GetByRowKeyPagedAsync(TRowKey rowKey, int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		return GetByRowKeyPagedAsync(rowKey, null, pageSize, continuationTokenJson, cancellationToken);
	}


	/// <summary>
	///     Gets the records by row key asynchronous.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public Task<PagedResult<T>> GetByRowKeyPagedAsync(TRowKey rowKey, Func<T, bool> filter, int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		return GetByRowKeyPagedAsync(GetRowKeyString(rowKey), filter, pageSize, continuationTokenJson, cancellationToken);
	}

	/// <summary>
	///     Occurs when on table created.
	/// </summary>
	public event Action<string, IPocoTableStore<T, TPartitionKey, TRowKey>> OnTableCreated;

	/// <summary>
	///     Occurs when on table created.
	/// </summary>
	public event Func<string, IPocoTableStore<T, TPartitionKey, TRowKey>, CancellationToken, Task> OnTableCreatedAsync;

	/// <summary>
	///     Occurs when on table deleted.
	/// </summary>
	public event Action<string, IPocoTableStore<T, TPartitionKey, TRowKey>> OnTableDeleted;

	/// <summary>
	///     Occurs when on table deleted.
	/// </summary>
	public event Func<string, IPocoTableStore<T, TPartitionKey, TRowKey>, CancellationToken, Task> OnTableDeletedAsync;

	/// <summary>
	///     Occurs when on record inserted or updated.
	/// </summary>
	public event Action<T> OnRecordInsertedOrUpdated;

	/// <summary>
	///     Occurs when on record inserted or updated.
	/// </summary>
	public event Func<T, CancellationToken, Task> OnRecordInsertedOrUpdatedAsync;


	/// <summary>
	///     Occurs when on records inserted.
	/// </summary>
	public event Action<IEnumerable<T>> OnRecordsInserted;

	/// <summary>
	///     Occurs when on records inserted.
	/// </summary>
	public event Func<IEnumerable<T>, CancellationToken, Task> OnRecordsInsertedAsync;

	/// <summary>
	///     Occurs when on record deleted.
	/// </summary>
	public event Action<T> OnRecordDeleted;

	/// <summary>
	///     Occurs when on record deleted.
	/// </summary>
	public event Func<T, CancellationToken, Task> OnRecordDeletedAsync;

	/// <summary>
	///     Deletes the record asynchronously.
	/// </summary>
	/// <param name="record">The record.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	public async Task DeleteAsync(T record, CancellationToken cancellationToken = default)
	{
		TableEntity entity = CreateEntityWithEtag(record);
		await _tableStore.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
		if (OnRecordDeletedAsync != null) {
			await OnRecordDeletedAsync(record, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Deletes the record without an etag asynchronously.
	/// </summary>
	/// <param name="record">The record.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	public async Task DeleteUsingWildcardEtagAsync(T record, CancellationToken cancellationToken = default)
	{
		TableEntity entity = CreateEntity(record);
		await _tableStore.DeleteUsingWildcardEtagAsync(entity, cancellationToken).ConfigureAwait(false);
		if (OnRecordDeletedAsync != null) {
			await OnRecordDeletedAsync(record, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Deletes the table asynchronously.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	public async Task DeleteTableAsync(CancellationToken cancellationToken = default)
	{
		await _tableStore.DeleteTableAsync(cancellationToken).ConfigureAwait(false);
		if (OnTableDeletedAsync != null) {
			await OnTableDeletedAsync(_tableName, this, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     get the  record by partition key and row key asynchronously."/>
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="rowKey">The row key.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;T&gt;.</returns>
	public async Task<T> GetRecordAsync(string partitionKey, string rowKey,
		CancellationToken cancellationToken = default)
	{
		TableEntity entity = null;
		try {
			entity = await _tableStore.GetRecordAsync(partitionKey, rowKey, cancellationToken);
			return CreateRecord(entity);
		}
		catch (RequestFailedException e) when (e.Status == 404) {
			// do nothing, we will return null to maintain backward compability with behavior of previous table storage libraries
		}

		return CreateRecord(entity);
	}

	/// <summary>
	///     Gets the records by partition key asynchronously.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey,
		CancellationToken cancellationToken = default)
	{
		return GetByPartitionKeyAsync(partitionKey, null, cancellationToken);
	}

	/// <summary>
	///     Gets the by records partition key paged asynchronous.
	/// </summary>
	/// <param name="partitionKey">The partition key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public Task<PagedResult<T>> GetByPartitionKeyPagedAsync(string partitionKey, int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		return GetByPartitionKeyPagedAsync(partitionKey, null, pageSize, continuationTokenJson, cancellationToken);
	}

	/// <summary>
	///     Gets the by records by row key asynchronously.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;IEnumerable&lt;T&gt;&gt;.</returns>
	public Task<IEnumerable<T>> GetByRowKeyAsync(string rowKey, CancellationToken cancellationToken = default)
	{
		return GetByRowKeyAsync(rowKey, null, cancellationToken);
	}

	/// <summary>
	///     Gets the by records by row key asynchronously.  This method may be slow if there is a high volume of
	///     data across many partitions, prefer to use <see cref="GetByPartitionKeyAsync(string)" />
	/// </summary>
	/// <param name="rowKey">The row key.</param>
	/// <param name="pageSize">Size of the page.</param>
	/// <param name="continuationTokenJson">The continuation token json.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;PagedResult&lt;T&gt;&gt;.</returns>
	public Task<PagedResult<T>> GetByRowKeyPagedAsync(string rowKey, int pageSize = 100,
		string continuationTokenJson = null, CancellationToken cancellationToken = default)
	{
		return GetByRowKeyPagedAsync(rowKey, null, pageSize, continuationTokenJson, cancellationToken);
	}

	/// <summary>
	///     Creates the table storage table asynchronously.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	public async Task CreateTableAsync(CancellationToken cancellationToken = default)
	{
		await _tableStore.CreateTableAsync(cancellationToken).ConfigureAwait(false);
		if (OnTableCreatedAsync != null) {
			await OnTableCreatedAsync(_tableName, this, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Checks if the table storage table exists asynchronously.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Task&lt;System.Boolean&gt;.</returns>
	public Task<bool> TableExistsAsync(CancellationToken cancellationToken = default)
	{
		return _tableStore.TableExistsAsync(cancellationToken);
	}

	/// <summary>
	///     Inserts the record into the table storage table, asynchronously.
	/// </summary>
	/// <param name="record">The record.</param>
	/// <param name="cancellationToken"
	/// <returns>Task.</returns>
	/// <exception cref="ArgumentNullException">record</exception>
	public async Task InsertAsync(T record, CancellationToken cancellationToken = default)
	{
		if (record == null) {
			throw new ArgumentNullException(nameof(record));
		}

		TableEntity entity = CreateEntity(record);

		await _tableStore.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
		if (OnRecordInsertedOrUpdatedAsync != null) {
			await OnRecordInsertedOrUpdatedAsync(record, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Inserts or replaces the record
	/// </summary>
	/// <param name="record"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	/// <exception cref="ArgumentNullException">record</exception>
	public async Task InsertOrReplaceAsync(T record, CancellationToken cancellationToken = default)
	{
		if (record == null) {
			throw new ArgumentNullException(nameof(record));
		}

		TableEntity entity = CreateEntity(record);

		await _tableStore.InsertOrReplaceAsync(entity, cancellationToken).ConfigureAwait(false);
		if (OnRecordInsertedOrUpdatedAsync != null) {
			await OnRecordInsertedOrUpdatedAsync(record, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Inserts the records asynchronously.
	/// </summary>
	/// <param name="records">The records.</param>
	/// <param name="cancellationToken"
	/// <returns>Task.</returns>
	/// <exception cref="ArgumentNullException">records</exception>
	public async Task InsertAsync(IEnumerable<T> records, CancellationToken cancellationToken = default)
	{
		if (records == null) {
			throw new ArgumentNullException(nameof(records));
		}

		IEnumerable<TableEntity> entities = CreateEntities(records);
		await _tableStore.InsertAsync(entities, cancellationToken).ConfigureAwait(false);
		if (OnRecordsInsertedAsync != null) {
			await OnRecordsInsertedAsync(records, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Updates the record asynchronously.
	/// </summary>
	/// <param name="record">The record.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	public async Task UpdateAsync(T record, CancellationToken cancellationToken = default)
	{
		TableEntity entity = CreateEntityWithEtag(record);
		await _tableStore.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
		if (OnRecordInsertedOrUpdatedAsync != null) {
			await OnRecordInsertedOrUpdatedAsync(record, cancellationToken).ConfigureAwait(false);
		}
	}

	/// <summary>
	///     Updates the using record without an etag asynchronously.
	/// </summary>
	/// <param name="record">The record.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>Task.</returns>
	public async Task UpdateUsingWildcardEtagAsync(T record, CancellationToken cancellationToken = default)
	{
		TableEntity entity = CreateEntity(record);
		await _tableStore.UpdateUsingWildcardEtagAsync(entity, cancellationToken).ConfigureAwait(false);
		if (OnRecordInsertedOrUpdatedAsync != null) {
			await OnRecordInsertedOrUpdatedAsync(record, cancellationToken).ConfigureAwait(false);
		}
	}

	private TableEntity CreateEntity(T record)
	{
		return _keysConverter.ToEntity(record);
	}

	private IEnumerable<TableEntity> CreateEntities(IEnumerable<T> records)
	{
		IEnumerable<TableEntity> entities = records.Select(CreateEntity);
		return entities;
	}

	private T CreateRecord(TableEntity entity)
	{
		if (entity == null) {
			return default;
		}

		return _keysConverter.FromEntity(entity);
	}

	private IEnumerable<T> CreateRecords(IEnumerable<TableEntity> entities)
	{
		return entities.Select(CreateRecord);
	}

	//this whole method is a hack, will be removed when PagedResult can be directly invoked.
	private PagedResult<T> CreatePagedResult(PagedResult<TableEntity> result, Func<T, bool> filter = null)
	{
		PagedResultCache.PagedResultConstructors.TryGetValue(typeof(T), out ConstructorInfo ctor);
		if (ctor == null) {
			Type t = typeof(PagedResult<T>);
			ctor = t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
			PagedResultCache.PagedResultConstructors.TryAdd(t, ctor);
		}

		IEnumerable<T> records = CreateRecords(result.Items);

		if (filter != null) {
			records = records.Where(filter);
		}

		return ctor.Invoke(new object[]
			{ records.ToList(), result.ContinuationToken, result.IsFinalPage }) as PagedResult<T>;
	}

	private TableEntity CreateEntityWithEtag(T record)
	{
		TableEntity dynamicEntity = _keysConverter.ToEntity(record);

		TableEntity original = _tableStore.GetRecord(dynamicEntity.PartitionKey, dynamicEntity.RowKey);

		TableEntity entity = CreateEntity(record);
		entity.ETag = original.ETag;
		return entity;
	}

	private string GetPartitionKeyString(TPartitionKey key)
	{
		return _keysConverter.PartitionKey(key);
	}

	private string GetRowKeyString(TRowKey key)
	{
		return _keysConverter.RowKey(key);
	}
}

static internal class PagedResultCache
{
	public static ConcurrentDictionary<Type, ConstructorInfo> PagedResultConstructors { get; } = new();
}