using System;
using System.Linq.Expressions;
using Azure.Data.Tables;
using Newtonsoft.Json;
using TableStorage.Abstractions.TableEntityConverters;

namespace TableStorage.Abstractions.POCO ;

	public class CalculatedKeysConverter<T, TPartitionKey, TRowKey> : IKeysConverter<T, TPartitionKey, TRowKey>
		where T : new()
	{
		private readonly Expression<Func<T, object>>[] _ignoredProperties;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly KeyMapper<T, TPartitionKey> _partitionMapper;
		private readonly PropertyConverters<T> _propertyConverters;
		private readonly KeyMapper<T, TRowKey> _rowMapper;

		public CalculatedKeysConverter(KeyMapper<T, TPartitionKey> partitionMapper, KeyMapper<T, TRowKey> rowMapper, JsonSerializerSettings jsonSerializerSettings,
			PropertyConverters<T> propertyConverters = default,
			params Expression<Func<T, object>>[] ignoredProperties)
		{
			_partitionMapper = partitionMapper;
			_rowMapper = rowMapper;
			_jsonSerializerSettings = jsonSerializerSettings;
			_propertyConverters = propertyConverters ?? new PropertyConverters<T>();
			_ignoredProperties = ignoredProperties;
		}

		public CalculatedKeysConverter(KeyMapper<T, TPartitionKey> partitionMapper, KeyMapper<T, TRowKey> rowMapper,
			PropertyConverters<T> propertyConverters = default,
			params Expression<Func<T, object>>[] ignoredProperties)
			: this(partitionMapper, rowMapper, new JsonSerializerSettings(), propertyConverters, ignoredProperties)
		{
		}

		public string PartitionKey(TPartitionKey key)
		{
			return _partitionMapper.ToKeyFromParameter(key);
		}

		public string RowKey(TRowKey key)
		{
			return _rowMapper.ToKeyFromParameter(key);
		}

		public TableEntity ToEntity(T obj)
		{
			return obj.ToTableEntity(_partitionMapper.ToKey(obj), _rowMapper.ToKey(obj), _jsonSerializerSettings, _propertyConverters, _ignoredProperties);
		}

		public T FromEntity(TableEntity entity)
		{
			return entity.FromTableEntity(_partitionMapper.KeyProperty, _partitionMapper.FromKey, _rowMapper.KeyProperty,
				_rowMapper.FromKey, _jsonSerializerSettings, _propertyConverters);
		}
	}