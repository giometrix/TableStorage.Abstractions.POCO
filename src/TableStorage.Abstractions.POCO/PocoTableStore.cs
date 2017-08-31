using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using TableStorage.Abstractions.TableEntityConverters;

namespace TableStorage.Abstractions.POCO
{
	public class PocoTableStore<T, TPartitionKey, TRowKey> : ITableStore<T> where T : new()
	{
		//hack because this is internal.  Need to get this exposed.
		private static readonly ConcurrentDictionary<Type, ConstructorInfo> _pagedResultConstructors =
			new ConcurrentDictionary<Type, ConstructorInfo>();

		private readonly Func<T, string> _calculatedPartitionKey;
		private readonly Func<T, string> _calculatedRowKey;
		private readonly Func<TPartitionKey, string> _calculatedPartitionKeyFromParameter;
		private readonly Func<TRowKey, string> _calculatedRowKeyFromParameter;

		private readonly Func<string, TPartitionKey> _convertPartitionKey;
		private readonly Func<string, TRowKey> _convertRowKey;


		private readonly Expression<Func<T, object>>[] _ignoredProperties;

		private readonly Expression<Func<T, object>> _partitionProperty;
		private readonly Expression<Func<T, object>> _rowProperty;
		private readonly TableStore<DynamicTableEntity> _tableStore;
		private readonly bool _useCalculatedKeys;

		public PocoTableStore(string tableName, string storageConnectionString, Expression<Func<T, object>> partitionProperty,
			Expression<Func<T, object>> rowProperty, params Expression<Func<T, object>>[] ignoredProperties)
		{
			if (tableName == null) throw new ArgumentNullException(nameof(tableName));
			if (storageConnectionString == null) throw new ArgumentNullException(nameof(storageConnectionString));
			if (partitionProperty == null) throw new ArgumentNullException(nameof(partitionProperty));
			if (rowProperty == null) throw new ArgumentNullException(nameof(rowProperty));


			_partitionProperty = partitionProperty;
			_rowProperty = rowProperty;
			_ignoredProperties = ignoredProperties;
			_tableStore = new TableStore<DynamicTableEntity>(tableName, storageConnectionString);

			_useCalculatedKeys = false;
			_calculatedPartitionKey = null;
			_calculatedRowKey = null;
		}



		public PocoTableStore(string tableName, string storageConnectionString, int retries, double retryWaitTimeInSeconds,
			Expression<Func<T, object>> partitionProperty, Expression<Func<T, object>> rowProperty,
			params Expression<Func<T, object>>[] ignoredProperties)
		{
			if (tableName == null) throw new ArgumentNullException(nameof(tableName));
			if (storageConnectionString == null) throw new ArgumentNullException(nameof(storageConnectionString));
			if (partitionProperty == null) throw new ArgumentNullException(nameof(partitionProperty));
			if (rowProperty == null) throw new ArgumentNullException(nameof(rowProperty));

			_partitionProperty = partitionProperty;
			_rowProperty = rowProperty;
			_ignoredProperties = ignoredProperties;
			_tableStore = new TableStore<DynamicTableEntity>(tableName, storageConnectionString, retries,
				retryWaitTimeInSeconds);

			_useCalculatedKeys = false;
			_calculatedPartitionKey = null;
			_calculatedRowKey = null;
		}


		public PocoTableStore(string tableName, string storageConnectionString,
			Expression<Func<T, object>> partitionProperty, Expression<Func<T, object>> rowProperty,
			Func<T, string> calculatedPartitionKey, Func<T, string> calculatedRowKey,
			Func<TPartitionKey, string> calculatedPartitionKeyFromParameter,
			Func<TRowKey, string> calculatedRowKeyFromParameter,
			Func<string, TPartitionKey> convertPartitionKey, Func<string, TRowKey> convertRowKey,
			params Expression<Func<T, object>>[] ignoredProperties)
		{
			if (tableName == null) throw new ArgumentNullException(nameof(tableName));
			if (storageConnectionString == null) throw new ArgumentNullException(nameof(storageConnectionString));
			if (calculatedPartitionKey == null) throw new ArgumentNullException(nameof(calculatedPartitionKey));
			if (calculatedRowKey == null) throw new ArgumentNullException(nameof(calculatedRowKey));
			if (calculatedPartitionKeyFromParameter == null)
				throw new ArgumentNullException(nameof(calculatedPartitionKeyFromParameter));
			if (calculatedRowKeyFromParameter == null) throw new ArgumentNullException(nameof(calculatedRowKeyFromParameter));

			_calculatedPartitionKey = calculatedPartitionKey;
			_calculatedRowKey = calculatedRowKey;
			_calculatedPartitionKeyFromParameter = calculatedPartitionKeyFromParameter;
			_calculatedRowKeyFromParameter = calculatedRowKeyFromParameter;
			_convertPartitionKey = convertPartitionKey;
			_convertRowKey = convertRowKey;
			_ignoredProperties = ignoredProperties;
			_tableStore = new TableStore<DynamicTableEntity>(tableName, storageConnectionString);

			_useCalculatedKeys = true;
			_partitionProperty = partitionProperty;
			_rowProperty = rowProperty;

		}

		public PocoTableStore(string tableName, string storageConnectionString, int retries, double retryWaitTimeInSeconds,
			Expression<Func<T, object>> partitionProperty, Expression<Func<T, object>> rowProperty,
			Func<T, string> calculatedPartitionKey, Func<T, string> calculatedRowKey,
			Func<TPartitionKey, string> calculatedPartitionKeyFromParameter,
			Func<TRowKey, string> calculatedRowKeyFromParameter,
			Func<string, TPartitionKey> convertPartitionKey, Func<string, TRowKey> convertRowKey,
			params Expression<Func<T, object>>[] ignoredProperties)
		{
			if (tableName == null) throw new ArgumentNullException(nameof(tableName));
			if (storageConnectionString == null) throw new ArgumentNullException(nameof(storageConnectionString));
			if (calculatedPartitionKey == null) throw new ArgumentNullException(nameof(calculatedPartitionKey));
			if (calculatedRowKey == null) throw new ArgumentNullException(nameof(calculatedRowKey));
			if (calculatedPartitionKeyFromParameter == null)
				throw new ArgumentNullException(nameof(calculatedPartitionKeyFromParameter));
			if (calculatedRowKeyFromParameter == null) throw new ArgumentNullException(nameof(calculatedRowKeyFromParameter));

			_calculatedPartitionKey = calculatedPartitionKey;
			_calculatedRowKey = calculatedRowKey;
			_calculatedPartitionKeyFromParameter = calculatedPartitionKeyFromParameter;
			_calculatedRowKeyFromParameter = calculatedRowKeyFromParameter;
			_convertPartitionKey = convertPartitionKey;
			_convertRowKey = convertRowKey;
			_ignoredProperties = ignoredProperties;
			_tableStore = new TableStore<DynamicTableEntity>(tableName, storageConnectionString, retries,
				retryWaitTimeInSeconds);

			_useCalculatedKeys = true;
			_partitionProperty = partitionProperty;
			_rowProperty = rowProperty;

		}

		public void CreateTable()
		{
			_tableStore.CreateTable();
		}

		public bool TableExists()
		{
			return _tableStore.TableExists();
		}

		public void Insert(T record)
		{
			if (record == null)
				throw new ArgumentNullException(nameof(record));

			var entity = CreateEntity(record);

			_tableStore.Insert(entity);
		}


		public void Insert(IEnumerable<T> records)
		{
			if (records == null)
				throw new ArgumentNullException(nameof(records));

			var entities = CreateEntities(records);
			_tableStore.Insert(entities);
		}


		public void Update(T record)
		{
			var entity = CreateEntityWithEtag(record);

			_tableStore.Update(entity);
		}


		public void UpdateUsingWildcardEtag(T record)
		{
			var entity = CreateEntity(record);
			_tableStore.UpdateUsingWildcardEtag(entity);
		}

		public void Delete(T record)
		{
			var entity = CreateEntityWithEtag(record);
			_tableStore.Delete(entity);
		}

		public void DeleteUsingWildcardEtag(T record)
		{
			var entity = CreateEntity(record);
			_tableStore.DeleteUsingWildcardEtag(entity);
		}

		public void DeleteTable()
		{
			_tableStore.DeleteTable();
		}

		public T GetRecord(string partitionKey, string rowKey)
		{
			var entity = _tableStore.GetRecord(partitionKey, rowKey);
			return CreateRecord(entity);
		}


		public IEnumerable<T> GetByPartitionKey(string partitionKey)
		{
			var entities = _tableStore.GetByPartitionKey(partitionKey);
			return CreateRecords(entities);
		}


		public PagedResult<T> GetByPartitionKeyPaged(string partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			var result = _tableStore.GetByPartitionKeyPaged(partitionKey, pageSize, continuationTokenJson);
			return CreatePagedResult(result);
		}


		public IEnumerable<T> GetByRowKey(string rowKey)
		{
			return CreateRecords(_tableStore.GetByRowKey(rowKey));
		}

		public PagedResult<T> GetByRowKeyPaged(string rowKey, int pageSize = 100, string continuationTokenJson = null)
		{
			var result = _tableStore.GetByRowKeyPaged(rowKey, pageSize, continuationTokenJson);
			return CreatePagedResult(result);
		}

		public IEnumerable<T> GetAllRecords()
		{
			return CreateRecords(_tableStore.GetAllRecords());
		}

		public PagedResult<T> GetAllRecordsPaged(int pageSize = 100, string pageToken = null)
		{
			return CreatePagedResult(_tableStore.GetAllRecordsPaged(pageSize, pageToken));
		}

		public int GetRecordCount()
		{
			return _tableStore.GetRecordCount();
		}

		public Task CreateTableAsync()
		{
			return _tableStore.CreateTableAsync();
		}

		public Task<bool> TableExistsAsync()
		{
			return _tableStore.TableExistsAsync();
		}

		public Task InsertAsync(T record)
		{
			if (record == null)
				throw new ArgumentNullException(nameof(record));

			var entity = CreateEntity(record);

			return _tableStore.InsertAsync(entity);
		}

		public Task InsertAsync(IEnumerable<T> records)
		{
			if (records == null)
				throw new ArgumentNullException(nameof(records));

			var entities = CreateEntities(records);
			return _tableStore.InsertAsync(entities);
		}

		public Task UpdateAsync(T record)
		{
			var entity = CreateEntityWithEtag(record);
			return _tableStore.UpdateAsync(entity);
		}

		public Task UpdateUsingWildcardEtagAsync(T record)
		{
			var entity = CreateEntity(record);
			return _tableStore.UpdateUsingWildcardEtagAsync(entity);
		}

		public Task DeleteAsync(T record)
		{
			var entity = CreateEntityWithEtag(record);
			return _tableStore.DeleteAsync(entity);
		}

		public Task DeleteUsingWildcardEtagAsync(T record)
		{
			var entity = CreateEntity(record);
			return _tableStore.DeleteUsingWildcardEtagAsync(entity);
		}

		public Task DeleteTableAsync()
		{
			return _tableStore.DeleteTableAsync();
		}

		public async Task<T> GetRecordAsync(string partitionKey, string rowKey)
		{
			var entity = await _tableStore.GetRecordAsync(partitionKey, rowKey).ConfigureAwait(false);
			return CreateRecord(entity);
		}

		public async Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey)
		{
			var entities = await _tableStore.GetByPartitionKeyAsync(partitionKey).ConfigureAwait(false);
			return CreateRecords(entities);
		}

		public async Task<PagedResult<T>> GetByPartitionKeyPagedAsync(string partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			var result = await _tableStore.GetByPartitionKeyPagedAsync(partitionKey, pageSize, continuationTokenJson)
				.ConfigureAwait(false);
			return CreatePagedResult(result);
		}

		public async Task<IEnumerable<T>> GetByRowKeyAsync(string rowKey)
		{
			return CreateRecords(await _tableStore.GetByRowKeyAsync(rowKey).ConfigureAwait(false));
		}

		public async Task<PagedResult<T>> GetByRowKeyPagedAsync(string rowKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			var result = await _tableStore.GetByRowKeyPagedAsync(rowKey, pageSize, continuationTokenJson).ConfigureAwait(false);
			return CreatePagedResult(result);
		}

		public async Task<IEnumerable<T>> GetAllRecordsAsync()
		{
			return CreateRecords(await _tableStore.GetAllRecordsAsync().ConfigureAwait(false));
		}

		public async Task<PagedResult<T>> GetAllRecordsPagedAsync(int pageSize = 100, string pageToken = null)
		{
			return CreatePagedResult(await _tableStore.GetAllRecordsPagedAsync(pageSize, pageToken).ConfigureAwait(false));
		}

		public Task<int> GetRecordCountAsync()
		{
			return _tableStore.GetRecordCountAsync();
		}

		public T GetRecord(TPartitionKey partitionKey, TRowKey rowKey)
		{
			if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
			if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));

			
			return GetRecord(GetPartitionKeyString(partitionKey), GetRowKeyString(rowKey));
		}

		public IEnumerable<T> GetByPartitionKey(TPartitionKey partitionKey)
		{
			if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));

			return GetByPartitionKey(GetPartitionKeyString(partitionKey));
		}

		public PagedResult<T> GetByPartitionKeyPaged(TPartitionKey partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
			return GetByPartitionKeyPaged(GetPartitionKeyString(partitionKey), pageSize, continuationTokenJson);
		}

		public IEnumerable<T> GetByRowKey(TRowKey rowKey)
		{
			if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));
			return GetByRowKey(GetRowKeyString(rowKey));
		}

		public PagedResult<T> GetByRowKeyPaged(TRowKey rowKey, int pageSize = 100, string continuationTokenJson = null)
		{
			if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));
			return GetByRowKeyPaged(GetRowKeyString(rowKey), pageSize, continuationTokenJson);
		}

		public Task<T> GetRecordAsync(TPartitionKey partitionKey, TRowKey rowKey)
		{
			if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
			if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));
			return GetRecordAsync(GetPartitionKeyString(partitionKey), GetRowKeyString(rowKey));
		}

		public Task<IEnumerable<T>> GetByPartitionKeyAsync(TPartitionKey partitionKey)
		{
			if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
			return GetByPartitionKeyAsync(GetPartitionKeyString(partitionKey));
		}

		public Task<PagedResult<T>> GetByPartitionKeyPagedAsync(TPartitionKey partitionKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
			return GetByPartitionKeyPagedAsync(GetPartitionKeyString(partitionKey), pageSize, continuationTokenJson);
		}

		public Task<IEnumerable<T>> GetByRowKeyAsync(TRowKey rowKey)
		{
			if (rowKey == null) throw new ArgumentNullException(nameof(rowKey));
			return GetByRowKeyAsync(GetRowKeyString(rowKey));
		}

		public Task<PagedResult<T>> GetByRowKeyPagedAsync(TRowKey rowKey, int pageSize = 100,
			string continuationTokenJson = null)
		{
			return GetByRowKeyPagedAsync(GetRowKeyString(rowKey), pageSize, continuationTokenJson);
		}

		private DynamicTableEntity CreateEntity(T record)
		{
			DynamicTableEntity entity;

			if (_useCalculatedKeys)
			{
				entity = record.ToTableEntity(_calculatedPartitionKey(record), _calculatedRowKey(record), _ignoredProperties);
			}
			else
			{
				entity = record.ToTableEntity(_partitionProperty, _rowProperty, _ignoredProperties);
			}

			return entity;
		}

		private IEnumerable<DynamicTableEntity> CreateEntities(IEnumerable<T> records)
		{
			var entities = records.Select(CreateEntity);
			return entities;
		}

		private T CreateRecord(DynamicTableEntity entity)
		{
			T record;

			if (_useCalculatedKeys)
			{
				record = entity.FromTableEntity<T, TPartitionKey, TRowKey>(_partitionProperty, _convertPartitionKey, _rowProperty,
					_convertRowKey);
			}
			else
			{
				record = entity.FromTableEntity<T, TPartitionKey, TRowKey>(_partitionProperty, _rowProperty);
			}

			return record;
		}

		private IEnumerable<T> CreateRecords(IEnumerable<DynamicTableEntity> entities)
		{
			return entities.Select(CreateRecord);
		}

		//this whole method is a hack, will be removed when PagedResult can be directly invoked.
		private PagedResult<T> CreatePagedResult(PagedResult<DynamicTableEntity> result)
		{
			_pagedResultConstructors.TryGetValue(typeof(T), out ConstructorInfo ctor);
			if (ctor == null)
			{
				var t = typeof(PagedResult<T>);
				ctor = t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
				_pagedResultConstructors.TryAdd(t, ctor);
			}

			return ctor.Invoke(new object[]
				{CreateRecords(result.Items).ToList(), result.ContinuationToken, result.IsFinalPage}) as PagedResult<T>;
		}

		private DynamicTableEntity CreateEntityWithEtag(T record)
		{
			DynamicTableEntity dynamicEntity;

			if (_useCalculatedKeys)
			{
				dynamicEntity = record.ToTableEntity(_calculatedPartitionKey(record), _calculatedRowKey(record), _ignoredProperties);
			}
			else
			{
				dynamicEntity = record.ToTableEntity(_partitionProperty, _rowProperty, _ignoredProperties);
			}
			
			var original = _tableStore.GetRecord(dynamicEntity.PartitionKey, dynamicEntity.RowKey);

			var entity = CreateEntity(record);
			entity.ETag = original.ETag;
			return entity;
		}

		private string GetPartitionKeyString(TPartitionKey key)
		{
			if (_useCalculatedKeys)
			{
				return _calculatedPartitionKeyFromParameter(key);
			}
			else
			{
				return key.ToString();
			}
		}

		private string GetRowKeyString(TRowKey key)
		{
			if (_useCalculatedKeys)
			{
				return _calculatedRowKeyFromParameter(key);
			}
			else
			{
				return key.ToString();
			}
		}
	}
}