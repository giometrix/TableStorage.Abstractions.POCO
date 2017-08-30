using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TableStorage.Abstractions.POCO.Tests
{
	[TestClass]
	public class PocoTableStoreTests
	{
		PocoTableStore<Employee, int, int> tableStore;
		
		[TestInitialize]
		public void CreateData()
		{
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id);

			var employee = new Employee { CompanyId = 1, Id = 1, Name = "Jim CEO", Department = new Department { Id = 22, Name = "Executive" } };
			var employee2 = new Employee { CompanyId = 1, Id = 2, Name = "Mary CTO", Department = new Department { Id = 22, Name = "Executive" } };
			var employee3 = new Employee { CompanyId = 2, Id = 1, Name = "Lucy CEO", Department = new Department { Id = 1, Name = "E Team" } };
			tableStore.Insert(new Employee[]{employee, employee2, employee3});
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
				Department = new Department { Id = 5, Name = "Test" }
			};

			var employee2 = new Employee
			{
				Name = "Test2",
				CompanyId = 299,
				Id = 299,
				Department = new Department { Id = 52, Name = "Test2" }
			};
			tableStore.Insert(new Employee[]{employee, employee2});
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
				Department = new Department { Id = 5, Name = "Test" }
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
				Department = new Department { Id = 5, Name = "Test" }
			};

			var employee2 = new Employee
			{
				Name = "Test2",
				CompanyId = 299,
				Id = 299,
				Department = new Department { Id = 52, Name = "Test2" }
			};
			await tableStore.InsertAsync(new Employee[] { employee, employee2 });
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
				Department = new Department { Id = 5, Name = "Test" }
			};
			tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id, e=>e.Department);
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
			var records = await tableStore.GetByPartitionKeyPagedAsync("1",1);
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
			var records = tableStore.GetByRowKeyPaged(1,1);
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
			var employee = new Employee { CompanyId = 1, Id = 1, Name = "Mr. Jim CEO", Department = new Department { Id = 22, Name = "Executive" } };
			tableStore.Update(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}

		[TestMethod]
		public void update_record_wildcard_etag()
		{
			var employee = new Employee { CompanyId = 1, Id = 1, Name = "Mr. Jim CEO", Department = new Department { Id = 22, Name = "Executive" } };
			tableStore.UpdateUsingWildcardEtag(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}

		[TestMethod]
		public async Task update_record_async()
		{
			var employee = new Employee { CompanyId = 1, Id = 1, Name = "Mr. Jim CEO", Department = new Department { Id = 22, Name = "Executive" } };
			await tableStore.UpdateAsync(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}

		[TestMethod]
		public async Task update_record_wildcard_etag_async()
		{
			var employee = new Employee { CompanyId = 1, Id = 1, Name = "Mr. Jim CEO", Department = new Department { Id = 22, Name = "Executive" } };
			await tableStore.UpdateUsingWildcardEtagAsync(employee);
			var record = tableStore.GetRecord(1, 1);
			Assert.AreEqual("Mr. Jim CEO", record.Name);

		}


		[TestMethod]
		public void delete_record()
		{
			var employee = new Employee { CompanyId = 1, Id = 1 };
			tableStore.Delete(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());

		}



		[TestMethod]
		public void delete_record_wildcard_etag()
		{
			var employee = new Employee { CompanyId = 1, Id = 1 };
			tableStore.DeleteUsingWildcardEtag(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());

		}


		[TestMethod]
		public async Task delete_record_async()
		{
			var employee = new Employee { CompanyId = 1, Id = 1 };
			await tableStore.DeleteAsync(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());

		}



		[TestMethod]
		public async Task delete_record_wildcard_etag_async()
		{
			var employee = new Employee { CompanyId = 1, Id = 1 };
			await tableStore.DeleteUsingWildcardEtagAsync(employee);
			Assert.AreEqual(2, tableStore.GetRecordCount());

		}


		[TestCleanup]
		public void Cleanup()
		{
			tableStore.DeleteTable();
		}
	}
}
