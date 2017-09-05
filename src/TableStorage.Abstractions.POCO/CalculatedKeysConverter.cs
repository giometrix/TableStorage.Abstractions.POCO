using System;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.Storage.Table;
using TableStorage.Abstractions.TableEntityConverters;

namespace TableStorage.Abstractions.POCO
{
	public class CalculatedKeysConverter<T, TPartitionKey, TRowKey> : IKeysConverter<T, TPartitionKey, TRowKey>
		where T: new()
	{
		private readonly KeyMapper<T, TPartitionKey> _partitionMapper;
		private readonly KeyMapper<T, TRowKey> _rowMapper;
		private readonly Expression<Func<T, object>>[] _ignoredProperties;

		public CalculatedKeysConverter(KeyMapper<T, TPartitionKey> partitionMapper, KeyMapper<T,TRowKey> rowMapper, 
			params Expression<Func<T, object>>[] ignoredProperties)
		{
			_partitionMapper = partitionMapper;
			_rowMapper = rowMapper;
			_ignoredProperties = ignoredProperties;
		}
		public DynamicTableEntity ToEntity(T obj)
		{
			return obj.ToTableEntity(_partitionMapper.ToKey(obj), _rowMapper.ToKey(obj), _ignoredProperties);
		}

		public T FromEntity(DynamicTableEntity entity)
		{
			return entity.FromTableEntity(_partitionMapper.KeyProperty, _partitionMapper.FromKey, _rowMapper.KeyProperty,
				_rowMapper.FromKey);
		}

		public string PartitionKey(TPartitionKey key)
		{
			return _partitionMapper.ToKeyFromParameter(key);
		}

		public string RowKey(TRowKey key)
		{
			return _rowMapper.ToKeyFromParameter(key);
		}
	}
}