using Microsoft.WindowsAzure.Storage.Table;

namespace TableStorage.Abstractions.POCO
{
	public interface IKeysConverter<T, TPartitionKey, TRowKey> 
	{
		DynamicTableEntity ToEntity(T obj);
		T FromEntity(DynamicTableEntity entity);
		string PartitionKey(TPartitionKey key);
		string RowKey(TRowKey key);
	}
}