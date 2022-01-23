using Azure.Data.Tables;

namespace TableStorage.Abstractions.POCO;

	public interface IKeysConverter<T, in TPartitionKey, in TRowKey>
	{
		TableEntity ToEntity(T obj);
		T FromEntity(TableEntity entity);
		string PartitionKey(TPartitionKey key);
		string RowKey(TRowKey key);
	}