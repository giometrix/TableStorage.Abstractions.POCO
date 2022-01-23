using System;
using System.Linq.Expressions;
using Azure.Data.Tables;
using Newtonsoft.Json;
using TableStorage.Abstractions.TableEntityConverters;

namespace TableStorage.Abstractions.POCO ;

	public class SimpleKeysConverter<T, TPartitionKey, TRowKey> : IKeysConverter<T, TPartitionKey, TRowKey>
		where T : new()
	{
		private readonly Expression<Func<T, object>>[] _ignoredProperties;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly Expression<Func<T, object>> _partitionProperty;
		private readonly PropertyConverters<T> _propertyConverters;
		private readonly Expression<Func<T, object>> _rowProperty;

		public SimpleKeysConverter(Expression<Func<T, object>> partitionProperty,
			Expression<Func<T, object>> rowProperty, JsonSerializerSettings jsonSerializerSettings,
			PropertyConverters<T> propertyConverters,
			Expression<Func<T, object>>[] ignoredProperties)
		{
			_partitionProperty = partitionProperty;
			_rowProperty = rowProperty;
			_ignoredProperties = ignoredProperties;
			_jsonSerializerSettings = jsonSerializerSettings;
			_propertyConverters = propertyConverters ?? new PropertyConverters<T>();
		}

		public TableEntity ToEntity(T obj)
		{
			return obj.ToTableEntity(_partitionProperty, _rowProperty, _jsonSerializerSettings, _propertyConverters, _ignoredProperties);
		}

		public T FromEntity(TableEntity entity)
		{
			return entity.FromTableEntity<T, TPartitionKey, TRowKey>(_partitionProperty, _rowProperty, _jsonSerializerSettings, _propertyConverters);
		}

		public string PartitionKey(TPartitionKey key)
		{
			return key.ToString();
		}

		public string RowKey(TRowKey key)
		{
			return key.ToString();
		}
	}