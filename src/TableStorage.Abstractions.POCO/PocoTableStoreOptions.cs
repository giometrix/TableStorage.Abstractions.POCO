using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TableStorage.Abstractions.Store;

namespace TableStorage.Abstractions.POCO
{
	public class PocoTableStoreOptions
	{
		public PocoTableStoreOptions(TableStorageOptions tableStorageOptions, JsonSerializerSettings jsonSerializerSettings)
		{
			TableStorageOptions = tableStorageOptions;
			JsonSerializerSettings = jsonSerializerSettings;
		}
		public PocoTableStoreOptions(TableStorageOptions tableStorageOptions) : this(tableStorageOptions, new JsonSerializerSettings())
		{
		}
		public PocoTableStoreOptions(JsonSerializerSettings jsonSerializerSettings) : this(new TableStorageOptions(), jsonSerializerSettings)
		{
		}

		public PocoTableStoreOptions() : this(new TableStorageOptions(), new JsonSerializerSettings())
		{
		}

		public TableStorageOptions TableStorageOptions { get; }
		public JsonSerializerSettings JsonSerializerSettings { get; }
	}
}
