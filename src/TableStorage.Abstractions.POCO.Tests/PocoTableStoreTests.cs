using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;
using System.Reactive;
namespace TableStorage.Abstractions.POCO.Tests
{
	[TestClass]
	public class PocoTableStoreTests
	{
		private PocoTableStore<Employee, int, int> tableStore;

		[TestInitialize]
		public void CreateData()
		{
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", e => e.CompanyId,
				e => e.Id);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Jim CEO",
				Department = new Department {Id = 22, Name = "Executive"}
			};
			var employee2 = new Employee
			{
				CompanyId = 1,
				Id = 2,
				Name = "Mary CTO",
				Department = new Department {Id = 22, Name = "Executive"}
			};
			var employee3 = new Employee
			{
				CompanyId = 2,
				Id = 1,
				Name = "Lucy CEO",
				Department = new Department {Id = 1, Name = "E Team"}
			};
			tableStore.Insert(new[] {employee, employee2, employee3});
		}

		[TestMethod]
		public void insert_record()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department {Id = 5, Name = "Test"}
			};
			tableStore.Insert(employee);
			Assert.AreEqual(4, tableStore.GetRecordCount());
		}

		[TestMethod]
		public void insert_records()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department {Id = 5, Name = "Test"}
			};

			var employee2 = new Employee
			{
				Name = "Test2",
				CompanyId = 299,
				Id = 299,
				Department = new Department {Id = 52, Name = "Test2"}
			};
			tableStore.Insert(new[] {employee, employee2});
			Assert.AreEqual(5, tableStore.GetRecordCount());
		}

		[TestMethod]
		public async Task insert_record_async()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department {Id = 5, Name = "Test"}
			};
			await tableStore.InsertAsync(employee);
			Assert.AreEqual(4, tableStore.GetRecordCount());
		}


		[TestMethod]
		public async Task insert_records_async()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department {Id = 5, Name = "Test"}
			};

			var employee2 = new Employee
			{
				Name = "Test2",
				CompanyId = 299,
				Id = 299,
				Department = new Department {Id = 52, Name = "Test2"}
			};
			await tableStore.InsertAsync(new[] {employee, employee2});
			Assert.AreEqual(5, tableStore.GetRecordCount());
		}


		[TestMethod]
		public void insert_record_ignore_field()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department {Id = 5, Name = "Test"}
			};
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", e => e.CompanyId,
				e => e.Id, e => e.Department);
			tableStore.Insert(employee);
			var record = tableStore.GetRecord(99, 99);
			Assert.IsNull(record.Department);
		}

		[TestMethod]
		public void get_record()
		{
			var record = tableStore.GetRecord("1", "1");
			Assert.IsNotNull(record);
		}

		[TestMethod]
		public void get_record_with_no_results()
		{
			var record = tableStore.GetRecord("100", "10");
			Assert.IsNull(record);
		}

		[TestMethod]
		public async Task get_record_async()
		{
			var record = await tableStore.GetRecordAsync("1", "1");
			Assert.IsNotNull(record);
		}

		[TestMethod]
		public void get_record_typed()
		{
			var record = tableStore.GetRecord(1, 1);
			Assert.IsNotNull(record);
		}

		[TestMethod]
		public async Task get_record_typed_async()
		{
			var record = await tableStore.GetRecordAsync(1, 1);
			Assert.IsNotNull(record);
		}


		[TestMethod]
		public void get_records_by_partition_key()
		{
			var records = tableStore.GetByPartitionKey("1");
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public void get_records_by_partition_key_and_filter()
		{
			var records = tableStore.GetByPartitionKey("1", e=>e.Name == "Jim CEO");
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public void get_records_by_partition_key_typed()
		{
			var records = tableStore.GetByPartitionKey(1);
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public void get_records_by_partition_key_and_filter_typed()
		{
			var records = tableStore.GetByPartitionKey(1, e=>e.Name == "Jim CEO");
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_partition_key_async()
		{
			var records = await tableStore.GetByPartitionKeyAsync("1");
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_partition_key_and_filter_async()
		{
			var records = await tableStore.GetByPartitionKeyAsync("1", e=>e.Name == "Jim CEO");
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_partition_key_and_filter_typed_async()
		{
			var records = await tableStore.GetByPartitionKeyAsync(1, e => e.Name == "Jim CEO");
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_partition_key_async_typed()
		{
			var records = await tableStore.GetByPartitionKeyAsync(1);
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public void get_records_by_partition_key_paged()
		{
			var records = tableStore.GetByPartitionKeyPaged("1", 1);
			Assert.AreEqual(1, records.Items.Count());
		}

		[TestMethod]
		public void get_records_by_partition_key_and_filter_paged()
		{
			var records = tableStore.GetByPartitionKeyPaged("1", e=>e.Name == "Jim CEO", 1);
			Assert.AreEqual(1, records.Items.Count());

			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = tableStore.GetByPartitionKeyPaged("1", e => e.Name == "Jim CEO", 1, token);

			
			Assert.AreEqual(0, records.Items.Count); 
		}


		[TestMethod]
		public void get_records_by_partition_key_paged_typed()
		{
			var records = tableStore.GetByPartitionKeyPaged(1, 1);
			Assert.AreEqual(1, records.Items.Count());
		}


		[TestMethod]
		public void get_records_by_partition_key_and_filter_paged_typed()
		{
			var records = tableStore.GetByPartitionKeyPaged(1, e => e.Name == "Jim CEO", 1);
			Assert.AreEqual(1, records.Items.Count());

			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = tableStore.GetByPartitionKeyPaged(1, e => e.Name == "Jim CEO", 1, token);


			Assert.AreEqual(0, records.Items.Count);
		}

		[TestMethod]
		public async Task get_records_by_partition_key_paged_async()
		{
			var records = await tableStore.GetByPartitionKeyPagedAsync("1", 1);
			Assert.AreEqual(1, records.Items.Count());
		}

		[TestMethod]
		public async Task get_records_by_partition_key_paged_and_filtered_async()
		{
			var records = await tableStore.GetByPartitionKeyPagedAsync("1", e => e.Name == "Jim CEO", 1);
			Assert.AreEqual(1, records.Items.Count());

			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = await tableStore.GetByPartitionKeyPagedAsync("1", e => e.Name == "Jim CEO", 1, token);


			Assert.AreEqual(0, records.Items.Count);
		}

		[TestMethod]
		public async Task get_records_by_partition_key_paged_and_filtered_async_typed()
		{
			var records = await tableStore.GetByPartitionKeyPagedAsync(1, e => e.Name == "Jim CEO", 1);
			Assert.AreEqual(1, records.Items.Count());

			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = await tableStore.GetByPartitionKeyPagedAsync(1, e => e.Name == "Jim CEO", 1, token);


			Assert.AreEqual(0, records.Items.Count);
		}

		[TestMethod]
		public async Task get_records_by_partition_key_paged_async_typed()
		{
			var records = await tableStore.GetByPartitionKeyPagedAsync(1, 1);
			Assert.AreEqual(1, records.Items.Count());
		}

		[TestMethod]
		public void get_records_by_row_key()
		{
			var records = tableStore.GetByRowKey("1");
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public void get_records_by_row_key_and_filter()
		{
			var records = tableStore.GetByRowKey("1", e=>e.Name.Contains("Jim"));

			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public void get_records_by_row_key_typed()
		{
			var records = tableStore.GetByRowKey(1);
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public void get_records_by_row_key_and_filter_typed()
		{
			var records = tableStore.GetByRowKey(1, e=>e.Name.Contains("Jim"));
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public void get_records_by_row_key_paged()
		{
			var records = tableStore.GetByRowKeyPaged("1", 1);
			Assert.AreEqual(1, records.Items.Count());
		}


		[TestMethod]
		public void get_records_by_row_key_paged_typed()
		{
			var records = tableStore.GetByRowKeyPaged(1, 1);
			Assert.AreEqual(1, records.Items.Count());
		}

		[TestMethod]
		public void get_records_by_row_key_and_filtered_paged()
		{
			var records = tableStore.GetByRowKeyPaged("1", e=>e.Name.Contains("Jim"), 1);

			Assert.AreEqual(1, records.Items.Count());


			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = tableStore.GetByPartitionKeyPaged("1", e => e.Name.Contains("Jim"), 1, token);


			Assert.AreEqual(0, records.Items.Count);

		}


		[TestMethod]
		public void get_records_by_row_key_and_filtered_paged_typed()
		{
			var records = tableStore.GetByRowKeyPaged(1, e => e.Name.Contains("Jim"), 1);

			Assert.AreEqual(1, records.Items.Count());


			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = tableStore.GetByPartitionKeyPaged(1, e => e.Name.Contains("Jim"), 1, token);


			Assert.AreEqual(0, records.Items.Count);

		}

		[TestMethod]
		public async Task get_records_by_row_key_async()
		{
			var records = await tableStore.GetByRowKeyAsync("1");
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_row_key_and_filter_async()
		{
			var records = await tableStore.GetByRowKeyAsync("1", e=>e.Name.Contains("Jim"));
			Assert.AreEqual(1, records.Count());
		}


		[TestMethod]
		public async Task get_records_by_row_key_and_filter_typed_async()
		{
			var records = await tableStore.GetByRowKeyAsync(1, e => e.Name.Contains("Jim"));
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_row_key_typed_async()
		{
			var records = await tableStore.GetByRowKeyAsync(1);
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public void get_records_by_filter()
		{
			var records = tableStore.GetRecordsByFilter(x => x.Name == "Jim CEO");
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public void get_records_by_filter_no_results()
		{
			var records = tableStore.GetRecordsByFilter(x => x.Name == "Jim CEOxxx");
			Assert.AreEqual(0, records.Count());
		}


		[TestMethod]
		public void get_records_by_filter_paged()
		{
			var records = tableStore.GetRecordsByFilter(x => x.Name.Length > 0,0,1);
			Assert.AreEqual(1, records.Count());
		}

		[TestMethod]
		public void get_records_observable()
		{
			var observer = tableStore.GetAllRecordsObservable();
			int count = 0;
			observer.Subscribe(x =>
			{
				count++;
			});

			Assert.AreEqual(3, count);


		}

		[TestMethod]
		public void get_records_filtered_observable()
		{
			var observer = tableStore.GetRecordsByFilterObservable(e=>e.Name.Contains("CEO"), 0, 10);
			int count = 0;
			observer.Subscribe(x =>
			{
				count++;
			});

			Assert.AreEqual(2, count);


		}

		[TestMethod]
		public void get_records_filtered_observable_paged()
		{
			var observer = tableStore.GetRecordsByFilterObservable(e => e.Name.Contains("CEO"), 0, 1);
			int count = 0;
			observer.Subscribe(x =>
			{
				count++;
			});

			Assert.AreEqual(1, count);


		}

		[TestMethod]
		public async Task get_records_by_row_key_paged_async()
		{
			var records = await tableStore.GetByRowKeyPagedAsync("1", 1);
			Assert.AreEqual(1, records.Items.Count());
		}


		[TestMethod]
		public async Task get_records_by_row_key_and_filtered_paged_async()
		{
			var records = await tableStore.GetByRowKeyPagedAsync("1", e => e.Name.Contains("Jim"), 1);

			Assert.AreEqual(1, records.Items.Count());


			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = await tableStore.GetByPartitionKeyPagedAsync("1", e => e.Name.Contains("Jim"), 1, token);


			Assert.AreEqual(0, records.Items.Count);

		}

		[TestMethod]
		public async Task get_records_by_row_key_and_filtered_paged_async_typed()
		{
			var records = await tableStore.GetByRowKeyPagedAsync(1, e => e.Name.Contains("Jim"), 1);

			Assert.AreEqual(1, records.Items.Count());


			// this is a weird side effect due to filtering being done OUTSIDE of table storage.....
			//isFinalPage ends up being false because filtering was done outside of TS.  Sucks, but it is what it is for now.
			var token = records.ContinuationToken;
			records = await tableStore.GetByPartitionKeyPagedAsync(1, e => e.Name.Contains("Jim"), 1, token);


			Assert.AreEqual(0, records.Items.Count);

		}



		[TestMethod]
		public async Task get_records_by_row_key_paged_typed_async()
		{
			var records = await tableStore.GetByRowKeyPagedAsync(1, 1);
			Assert.AreEqual(1, records.Items.Count());
		}


		[TestMethod]
		public async Task get_record_count()
		{
			Assert.AreEqual(3, tableStore.GetRecordCount());
		}

		[TestMethod]
		public async Task get_record_count_async()
		{
			Assert.AreEqual(3, await tableStore.GetRecordCountAsync());
		}

		[TestMethod]
		public void update_record()
		{
			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department {Id = 22, Name = "Executive"}
			};
			tableStore.Update(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);
		}

		[TestMethod]
		public void update_record_wildcard_etag()
		{
			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department {Id = 22, Name = "Executive"}
			};
			tableStore.UpdateUsingWildcardEtag(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);
		}

		[TestMethod]
		public async Task update_record_async()
		{
			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department {Id = 22, Name = "Executive"}
			};
			await tableStore.UpdateAsync(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);
		}

		[TestMethod]
		public async Task update_record_wildcard_etag_async()
		{
			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department {Id = 22, Name = "Executive"}
			};
			await tableStore.UpdateUsingWildcardEtagAsync(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);
		}


		[TestMethod]
		public void delete_record()
		{
			var employee = new Employee {CompanyId = 1, Id = 1};
			tableStore.Delete(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());
		}


		[TestMethod]
		[ExpectedException(typeof(Microsoft.WindowsAzure.Storage.StorageException))]
		public void delete_record_that_doesnt_exist()
		{
			var employee = new Employee { CompanyId = 11, Id = 100 };
			tableStore.Delete(employee);
			
		}


		[TestMethod]
		public void delete_record_wildcard_etag()
		{
			var employee = new Employee {CompanyId = 1, Id = 1};
			tableStore.DeleteUsingWildcardEtag(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());
		}


		[TestMethod]
		public async Task delete_record_async()
		{
			var employee = new Employee {CompanyId = 1, Id = 1};
			await tableStore.DeleteAsync(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());
		}


		[TestMethod]
		public async Task delete_record_wildcard_etag_async()
		{
			var employee = new Employee {CompanyId = 1, Id = 1};
			await tableStore.DeleteUsingWildcardEtagAsync(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());
		}
		
		[TestMethod]
		public void insert_record_with_fixed_partition_key()
		{
			var pKeyMapper = new FixedKeyMapper<Employee, int>("SomeString");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",  tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var ts = new TableStore<DynamicTableEntity>("TestEmployee", "UseDevelopmentStorage=true");
			var record = ts.GetRecord("SomeString", "1");

			Assert.AreEqual("SomeString", record.PartitionKey);
			Assert.AreEqual("1", record.RowKey);
			Assert.AreEqual("Mr. Jim CEO", record.Properties["Name"].StringValue);

		}
		
		[TestMethod]
		public void get_record_with_fixed_partition_key()
		{
			var pKeyMapper = new FixedKeyMapper<Employee, int>("SomeString");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);
			tableStore.DeleteTable();
			tableStore.CreateTable();
			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var record = tableStore.GetRecord("SomeString", "1");
			Assert.AreEqual(1, record.Id);
		}
		
		[TestMethod]
		public void get_record_with_fixed_partition_key_typed()
		{
			var pKeyMapper = new FixedKeyMapper<Employee, int>("SomeString");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var record = tableStore.GetRecord(int.MinValue, 1); //partition key is bogus in this case
			Assert.AreEqual(1, record.Id);
		}


		[TestMethod]
		public void insert_record_with_calculated_partition_key()
		{

			var pKeyMapper = new KeyMapper<Employee, int>(e=>"SomeString_"+e.CompanyId, pk => int.Parse(pk.Substring("SomeString_".Length)), e=>e.CompanyId, id=>"SomeString_"+id);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);



			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var ts = new TableStore<DynamicTableEntity>("TestEmployee", "UseDevelopmentStorage=true");
			var record = ts.GetRecord("SomeString_1", "1");

			Assert.AreEqual("SomeString_1", record.PartitionKey);
			Assert.AreEqual("1", record.RowKey);
			Assert.AreEqual("Mr. Jim CEO", record.Properties["Name"].StringValue);

		}


		[TestMethod]
		public void get_record_with_calculated_partition_key()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => "SomeString_" + e.CompanyId, pk => int.Parse(pk.Substring("SomeString_".Length)), e => e.CompanyId, id => "SomeString_" + id);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var record = tableStore.GetRecord("SomeString_1", "1");

			Assert.AreEqual(1, record.CompanyId);
			Assert.AreEqual(1, record.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}


		[TestMethod]
		public void get_record_with_calculated_partition_key_typed()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => "SomeString_" + e.CompanyId, pk => int.Parse(pk.Substring("SomeString_".Length)), e => e.CompanyId, id => "SomeString_" + id);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var record = tableStore.GetRecord(1, 1);

			Assert.AreEqual(1, record.CompanyId);
			Assert.AreEqual(1, record.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}


		[TestMethod]
		public void insert_record_with_fixed_row_key()
		{

			var pKeyMapper = new KeyMapper<Employee, int>(e =>e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id =>id.ToString());
			var rKeyMapper = new FixedKeyMapper<Employee, int>("UserRecord");

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var ts = new TableStore<DynamicTableEntity>("TestEmployee", "UseDevelopmentStorage=true");
			var record = ts.GetRecord("1", "UserRecord");

			Assert.AreEqual("1", record.PartitionKey);
			Assert.AreEqual("UserRecord", record.RowKey);
			Assert.AreEqual("Mr. Jim CEO", record.Properties["Name"].StringValue);

		}


		[TestMethod]
		public void get_record_with_fixed_row_key()
		{

			var pKeyMapper = new KeyMapper<Employee, int>(e => e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id => id.ToString());
			var rKeyMapper = new FixedKeyMapper<Employee, int>("UserRecord");

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			
			var record = tableStore.GetRecord("1", "UserRecord");

			Assert.AreEqual(1, record.CompanyId);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}

		
		[TestMethod]
		public void get_record_with_fixed_row_key_typed()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id => id.ToString());
			var rKeyMapper = new FixedKeyMapper<Employee, int>("UserRecord");

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);


			var record = tableStore.GetRecord(1, int.MinValue); // rowkey is bogus in this case

			Assert.AreEqual(1, record.CompanyId);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}


		[TestMethod]
		public void insert_record_with_calculated_row_key()
		{

			var pKeyMapper = new KeyMapper<Employee, int>(e => e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id => id.ToString());
			var rKeyMapper = new KeyMapper<Employee, int>(e=>"UserRecord_" + e.Id, id=> int.Parse(id.Substring("UserRecord_".Length)), e=>e.Id, id=>"UserRecord_" + id);

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);



			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var ts = new TableStore<DynamicTableEntity>("TestEmployee", "UseDevelopmentStorage=true");
			var record = ts.GetRecord("1", "UserRecord_1");

			Assert.AreEqual("1", record.PartitionKey);
			Assert.AreEqual("UserRecord_1", record.RowKey);
			Assert.AreEqual("Mr. Jim CEO", record.Properties["Name"].StringValue);

		}


		[TestMethod]
		public void get_record_with_calculated_row_key()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id => id.ToString());
			var rKeyMapper = new KeyMapper<Employee, int>(e => "UserRecord_" + e.Id, id => int.Parse(id.Substring("UserRecord_".Length)), e => e.Id, id => "UserRecord_" + id);

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);



			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);


			var record = tableStore.GetRecord("1", "UserRecord_1");

			Assert.AreEqual(1, record.CompanyId);
			Assert.AreEqual(1, record.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}


		[TestMethod]
		public void get_record_with_calculated_row_key_typed()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id => id.ToString());
			var rKeyMapper = new KeyMapper<Employee, int>(e => "UserRecord_" + e.Id, id => int.Parse(id.Substring("UserRecord_".Length)), e => e.Id, id => "UserRecord_" + id);

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);



			var employee = new Employee
			{
				CompanyId = 1,
				Id = 22,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);


			var record = tableStore.GetRecord(1, 22);

			Assert.AreEqual(1, record.CompanyId);
			Assert.AreEqual(22, record.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}



		[TestMethod]
		public void insert_multiple_record_with_fixed_partition_key()
		{
			var pKeyMapper = new FixedKeyMapper<Employee, int>("SomeString");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};

			var employee2 = new Employee
			{
				CompanyId = 1,
				Id = 55,
				Name = "Mr. Ted QA",
				Department = new Department { Id = 27, Name = "IT" }
			};

			tableStore.Insert(new Employee[]{employee, employee2});

			var ts = new TableStore<DynamicTableEntity>("TestEmployee", "UseDevelopmentStorage=true");
			var records = ts.GetAllRecords();

			Assert.AreEqual(5, records.Count());
			Assert.IsTrue(records.Any(r=>r.RowKey == "55" && r.Properties["Name"].StringValue == "Mr. Ted QA"));

		}



		[TestMethod]
		public void insert_record_with_calculated_partition_key_using_date()
		{
			var date = new DateTime(2017, 8, 31).ToString("yyyyMMdd");
			var anotherDate = new DateTime(2017, 9, 1).ToString("yyyyMMdd");

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{date}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{date}_{id}");
			var pKeyMapper2 = new KeyMapper<Employee, int>(e => $"{anotherDate}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{anotherDate}_{id}");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var tableConverter2 = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper2, rKeyMapper);

			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter2);


			

			tableStore2.DeleteTable();
			tableStore2.CreateTable();

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 142,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);
			tableStore2.Insert(employee);

			var record1 = tableStore.GetRecord(1, 142);
			var record2 = tableStore2.GetRecord(1, 142);

			Assert.IsNotNull(record1);
			Assert.IsNotNull(record2);

			tableStore2.DeleteTable();
		}


		[TestMethod]
		public void update_record_with_calculated_partition_key_using_date()
		{
			var date = new DateTime(2017, 8, 31).ToString("yyyyMMdd");
			var anotherDate = new DateTime(2017, 9, 1).ToString("yyyyMMdd");

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{date}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{date}_{id}");
			var pKeyMapper2 = new KeyMapper<Employee, int>(e => $"{anotherDate}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{anotherDate}_{id}");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var tableConverter2 = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper2, rKeyMapper);

			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter2);


			tableStore2.DeleteTable();
			tableStore2.CreateTable();

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 142,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);
			tableStore2.Insert(employee);

			employee.Name = "XXX";
			tableStore.Update(employee);

			var record1 = tableStore.GetRecord(1, 142);
			var record2 = tableStore2.GetRecord(1, 142);

			Assert.IsNotNull(record1);
			Assert.IsNotNull(record2);

			Assert.AreEqual("XXX", record1.Name);
			Assert.AreEqual("Mr. Jim CEO", record2.Name);
			tableStore2.DeleteTable();
		}


		[TestMethod]
		public void update_record_with_calculated_partition_key_using_date_using_wildcard_etag()
		{
			var date = new DateTime(2017, 8, 31).ToString("yyyyMMdd");
			var anotherDate = new DateTime(2017, 9, 1).ToString("yyyyMMdd");

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{date}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{date}_{id}");
			var pKeyMapper2 = new KeyMapper<Employee, int>(e => $"{anotherDate}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{anotherDate}_{id}");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var tableConverter2 = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper2, rKeyMapper);

			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter2);


			tableStore2.DeleteTable();
			tableStore2.CreateTable();

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 142,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);
			tableStore2.Insert(employee);

			employee.Name = "XXX";
			tableStore.UpdateUsingWildcardEtag(employee);

			var record1 = tableStore.GetRecord(1, 142);
			var record2 = tableStore2.GetRecord(1, 142);

			Assert.IsNotNull(record1);
			Assert.IsNotNull(record2);

			Assert.AreEqual("XXX", record1.Name);
			Assert.AreEqual("Mr. Jim CEO", record2.Name);
			tableStore2.DeleteTable();
		}


		[TestMethod]
		public void delete_record_with_calculated_partition_key_using_date()
		{
			var date = new DateTime(2017, 8, 31).ToString("yyyyMMdd");
		

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{date}_{e.CompanyId}", pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), e => e.CompanyId, id => $"{date}_{id}");
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 142,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			tableStore.Delete(employee);

			Assert.AreEqual(3, tableStore.GetRecordCount());

		}


		[TestMethod]
		public void insert_record_with_calculated_partition_key_from_multiple_properties()
		{


			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{e.CompanyId}.{e.Department.Id}", null, null, null);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var ts = new TableStore<DynamicTableEntity>("TestEmployee", "UseDevelopmentStorage=true");
			var record = ts.GetRecord("1.22", "1");

			Assert.AreEqual("1.22", record.PartitionKey);
			Assert.AreEqual("1", record.RowKey);
			Assert.AreEqual("Mr. Jim CEO", record.Properties["Name"].StringValue);

		}

		[TestMethod]
		public void get_record_with_calculated_partition_key_from_multiple_properties()
		{

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{e.CompanyId}.{e.Department.Id}", null, null, null);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var record = tableStore.GetRecord("1.22", "1");

			Assert.AreEqual(1, record.Id);
			Assert.AreEqual(22, record.Department.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}

		[TestMethod]
		public void update_record_with_calculated_partition_key_from_multiple_properties()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{e.CompanyId}.{e.Department.Id}", null, null, null);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);

			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			employee.Name = "Ted";

			tableStore.Update(employee);

			var record = tableStore.GetRecord("1.22", "1");

			Assert.AreEqual(1, record.Id);
			Assert.AreEqual(22, record.Department.Id);
			Assert.AreEqual("Ted", record.Name);

		}

		[TestMethod]
		public void get_record_with_calculated_partition_key_from_multiple_properties_using_extension_method()
		{
			KeyGenerator.DefineParitionKey(typeof(Employee), e=>$"{e.CompanyId}.{e.DepartmentId}");
			KeyGenerator.DefineRowKey(typeof(Employee), e => $"{e.Id}");

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{e.CompanyId}.{e.Department.Id}", null, null, null);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			var record = tableStore.GetRecord(KeyGenerator.PartitionKey<Employee>(new {CompanyId=1, DepartmentId=22}), 
				KeyGenerator.RowKey<Employee>(new { Id = 1 }));

			Assert.AreEqual(1, record.Id);
			Assert.AreEqual(22, record.Department.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}

		public class PartitionKey
		{
			public PartitionKey(int companyId, int departmentId)
			{
				CompanyId = companyId;
				DepartmentId = departmentId;
			}
			public int CompanyId { get; }
			public int DepartmentId { get; }
		}


		[TestMethod]
		public void get_record_with_calculated_partition_key_from_multiple_properties_using_class_as_key()
		{
			

			var pKeyMapper = new CalculatedKeyMapper<Employee, PartitionKey>(e => $"{e.CompanyId}.{e.Department.Id}", key =>
			{
				var parts = key.Split('.');
				var companyId = int.Parse(parts[0]);
				var departmentId = int.Parse(parts[1]);
				return new PartitionKey(companyId, departmentId);
			}, key=>$"{key.CompanyId}.{key.DepartmentId}");

			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, PartitionKey, int>(pKeyMapper, rKeyMapper);

			var tableStore2 = new PocoTableStore<Employee, PartitionKey, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore2.Insert(employee);

			var record = tableStore2.GetRecord(new PartitionKey(1, 22), 1);

			Assert.AreEqual(1, record.Id);
			Assert.AreEqual(22, record.Department.Id);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}



		[TestMethod]
		public void update_record_with_calculated_partition_key_from_multiple_properties_using_extension_method()
		{
			KeyGenerator.DefineParitionKey(typeof(Employee), e => $"{e.CompanyId}.{e.DepartmentId}");
			KeyGenerator.DefineRowKey(typeof(Employee), e => $"{e.Id}");

			var pKeyMapper = new KeyMapper<Employee, int>(e => $"{e.CompanyId}.{e.Department.Id}", null, null, null);
			var rKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());

			var tableConverter = new CalculatedKeysTableConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", tableConverter);


			var employee = new Employee
			{
				CompanyId = 1,
				Id = 1,
				Name = "Mr. Jim CEO",
				Department = new Department { Id = 22, Name = "Executive" }
			};
			tableStore.Insert(employee);

			employee.Name = "Ted";

			tableStore.Update(employee);

			var record = tableStore.GetRecord(
				KeyGenerator.PartitionKey<Employee>(new { CompanyId = 1, DepartmentId = 22 }),
				KeyGenerator.RowKey<Employee>(new { Id = 1 }));

			Assert.AreEqual(1, record.Id);
			Assert.AreEqual(22, record.Department.Id);
			Assert.AreEqual("Ted", record.Name);

		}

		[TestCleanup]
		public void Cleanup()
		{
			tableStore.DeleteTable();
		}
		
	}
}