using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;

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
		public void get_records_by_partition_key_typed()
		{
			var records = tableStore.GetByPartitionKey(1);
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_partition_key_async()
		{
			var records = await tableStore.GetByPartitionKeyAsync("1");
			Assert.AreEqual(2, records.Count());
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
		public void get_records_by_partition_key_paged_typed()
		{
			var records = tableStore.GetByPartitionKeyPaged(1, 1);
			Assert.AreEqual(1, records.Items.Count());
		}


		[TestMethod]
		public async Task get_records_by_partition_key_paged_async()
		{
			var records = await tableStore.GetByPartitionKeyPagedAsync("1", 1);
			Assert.AreEqual(1, records.Items.Count());
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
		public void get_records_by_row_key_typed()
		{
			var records = tableStore.GetByRowKey(1);
			Assert.AreEqual(2, records.Count());
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
		public async Task get_records_by_row_key_async()
		{
			var records = await tableStore.GetByRowKeyAsync("1");
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_row_key_typed_async()
		{
			var records = await tableStore.GetByRowKeyAsync(1);
			Assert.AreEqual(2, records.Count());
		}

		[TestMethod]
		public async Task get_records_by_row_key_paged_async()
		{
			var records = await tableStore.GetByRowKeyPagedAsync("1", 1);
			Assert.AreEqual(1, records.Items.Count());
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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", 
				partitionProperty: null, rowProperty: e=>e.Id, calculatedPartitionKey: e => "SomeString", calculatedRowKey: e=>e.Id.ToString(), 
				calculatedPartitionKeyFromParameter: x=>null,
				calculatedRowKeyFromParameter:x=>x.ToString(),
				convertPartitionKey: null, convertRowKey: int.Parse );

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: null, rowProperty: e => e.Id, calculatedPartitionKey: e => "SomeString", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => "SomeString",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: e=>0, convertRowKey: int.Parse);
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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: null, rowProperty: e => e.Id, calculatedPartitionKey: e => "SomeString", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => "SomeString",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: e => 0, convertRowKey: int.Parse);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e=>e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => "SomeString_" + e.CompanyId, calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => "SomeString_"+x,
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk=>int.Parse(pk.Substring("SomeString_".Length)), convertRowKey: int.Parse);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e=>e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => "SomeString_" + e.CompanyId, calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => "SomeString_" + x,
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("SomeString_".Length)), convertRowKey: int.Parse);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => "SomeString_" + e.CompanyId, calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => "SomeString_" + x,
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("SomeString_".Length)), convertRowKey: int.Parse);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e=>e.CompanyId, rowProperty: null, calculatedPartitionKey: e => e.CompanyId.ToString(), calculatedRowKey: e => "UserRecord",
				calculatedPartitionKeyFromParameter: x => x.ToString(),
				calculatedRowKeyFromParameter: x => "UserRecord",
				convertPartitionKey: int.Parse, convertRowKey: null);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: null, calculatedPartitionKey: e => e.CompanyId.ToString(), calculatedRowKey: e => "UserRecord",
				calculatedPartitionKeyFromParameter: x => x.ToString(),
				calculatedRowKeyFromParameter: x => "UserRecord",
				convertPartitionKey: int.Parse, convertRowKey: null);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: null, calculatedPartitionKey: e => e.CompanyId.ToString(), calculatedRowKey: e => "UserRecord",
				calculatedPartitionKeyFromParameter: x => x.ToString(),
				calculatedRowKeyFromParameter: x => "UserRecord",
				convertPartitionKey: int.Parse, convertRowKey: null);

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e=>e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => e.CompanyId.ToString(), calculatedRowKey: e => "UserRecord_" + e.Id,
				calculatedPartitionKeyFromParameter: x => x.ToString(),
				calculatedRowKeyFromParameter: x => "UserRecord_" + x,
				convertPartitionKey: int.Parse, convertRowKey: rk=>int.Parse(rk.Substring("UserRecord_".Length)));

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => e.CompanyId.ToString(), calculatedRowKey: e => "UserRecord_" + e.Id,
				calculatedPartitionKeyFromParameter: x => x.ToString(),
				calculatedRowKeyFromParameter: x => "UserRecord_" + x,
				convertPartitionKey: int.Parse, convertRowKey: rk => int.Parse(rk.Substring("UserRecord_".Length)));

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => e.CompanyId.ToString(), calculatedRowKey: e => "UserRecord_" + e.Id,
				calculatedPartitionKeyFromParameter: x => x.ToString(),
				calculatedRowKeyFromParameter: x => "UserRecord_" + x,
				convertPartitionKey: int.Parse, convertRowKey: rk => int.Parse(rk.Substring("UserRecord_".Length)));

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
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: null, rowProperty: e => e.Id, calculatedPartitionKey: e => "SomeString", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => null,
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: null, convertRowKey: int.Parse);

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


			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{date}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{date}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);


			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{anotherDate}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{anotherDate}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);

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


			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{date}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{date}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);


			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{anotherDate}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{anotherDate}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);

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


			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{date}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{date}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);


			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{anotherDate}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{anotherDate}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);

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
			var anotherDate = new DateTime(2017, 9, 1).ToString("yyyyMMdd");


			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.CompanyId, rowProperty: e => e.Id, calculatedPartitionKey: e => $"{date}_{e.CompanyId}", calculatedRowKey: e => e.Id.ToString(),
				calculatedPartitionKeyFromParameter: x => $"{date}_{x}",
				calculatedRowKeyFromParameter: x => x.ToString(),
				convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), convertRowKey: int.Parse);




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
		[TestCleanup]
		public void Cleanup()
		{
			tableStore.DeleteTable();
		}
	}
}