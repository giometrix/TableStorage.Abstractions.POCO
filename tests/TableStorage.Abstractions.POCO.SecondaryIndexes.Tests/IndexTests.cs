using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TableStorage.Abstractions.POCO.SecondaryIndexes.Tests
{
	[TestClass]
	public class IndexTests
	{
		private PocoTableStore<Employee, int, int> TableStore;
		private PocoTableStore<Employee, int, string> IndexStore;

		[TestInitialize]
		public void Init()
		{
			TableStore = new PocoTableStore<Employee, int, int>("IXTestEmployee", "UseDevelopmentStorage=true",
				e => e.CompanyId, e => e.Id);

			IndexStore = new PocoTableStore<Employee, int, string>("IXTestEmployeeNameIndex", "UseDevelopmentStorage=true",
				e => e.CompanyId, e => e.Name);

			TableStore.AddIndex("Name", IndexStore);
		}

		[TestCleanup]
		public void Cleanup()
		{
			TableStore.DeleteTable();
			
		}

		[TestMethod]
		public async Task add_index()
		{
			var exists = IndexStore.TableExists();
			Assert.IsTrue(exists);
		}

		[TestMethod]
		public async Task delete_table_also_deletes_index()
		{
			TableStore.DeleteTable();
			var exists = IndexStore.TableExists();
			Assert.IsFalse(exists);
		}

		[TestMethod]
		public async Task drop_index()
		{
			TableStore.DropIndex("Name");
			var exists = IndexStore.TableExists();
			Assert.IsFalse(exists);
		}

		[TestMethod]
		public async Task drop_index_async()
		{
			TableStore.DropIndex("Name");
			var exists = await  IndexStore.TableExistsAsync();
			Assert.IsFalse(exists);
		}

		[TestMethod]
		public async Task insert_record()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			TableStore.Insert(employee);
			var e = IndexStore.GetRecord(99, "Test");
			Assert.IsNotNull(e);

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
			await TableStore.InsertAsync(employee);
			var e = IndexStore.GetRecord(99, "Test");
			Assert.IsNotNull(e);

		}

		[TestMethod]
		public async Task insert_record_by_index()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			var e = TableStore.GetRecordByIndex("Name", 99, "Test");
			Assert.IsNotNull(e);
		}

		[TestMethod]
		public async Task insert_record_by_index_as_string()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			var e = TableStore.GetRecordByIndex("Name", "99", "Test");
			Assert.IsNotNull(e);
		}

		[TestMethod]
		public async Task insert_record_by_index_async()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			var e = await TableStore.GetRecordByIndexAsync("Name", 99, "Test");
			Assert.IsNotNull(e);
		}


		[TestMethod]
		public async Task insert_record_by_index_as_string_async()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			var e = await TableStore.GetRecordByIndexAsync("Name", "99", "Test");
			Assert.IsNotNull(e);
		}

		[TestMethod]
		public async Task delete_record()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			TableStore.Delete(employee);
			var e = await TableStore.GetRecordByIndexAsync("Name", "99", "Test");
			Assert.IsNull(e);
		}

		[TestMethod]
		public async Task delete_record_async()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.DeleteAsync(employee);
			var e = await TableStore.GetRecordByIndexAsync("Name", "99", "Test");
			Assert.IsNull(e);
		}

		[TestMethod]
		public async Task get_index_as_poco_store()
		{
			var index = TableStore.Index<Employee, int, int, int, string>("Name");
			Assert.IsNotNull(index);
		}

		[TestMethod]
		public async Task get_by_index_partition_key()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var e = TableStore.GetByIndexPartitionKey("Name", 99);
			Assert.AreEqual(2, e.Count());
		}


		[TestMethod]
		public async Task get_by_index_partition_key_string()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var e = TableStore.GetByIndexPartitionKey("Name", "99");
			Assert.AreEqual(2, e.Count());
		}

		[TestMethod]
		public async Task get_by_index_partition_key_async()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var e = await TableStore.GetByIndexPartitionKeyAsync("Name", 99);
			Assert.AreEqual(2, e.Count());
		}

		[TestMethod]
		public async Task get_by_index_partition_key_string_async()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var e = await TableStore.GetByIndexPartitionKeyAsync("Name", "99");
			Assert.AreEqual(2, e.Count());
		}


		[TestMethod]
		public async Task get_by_index_partition_key_paged()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var p = TableStore.GetByIndexPartitionKeyPaged("Name", 99, 1);
			Assert.AreEqual(1,p.Items.Count);
			Assert.IsNotNull(p.ContinuationToken);
			p = TableStore.GetByIndexPartitionKeyPaged("Name", 99, 1, p.ContinuationToken);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNull(p.ContinuationToken);

		}

		[TestMethod]
		public async Task get_by_index_partition_key_string_paged()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var p = TableStore.GetByIndexPartitionKeyPaged("Name", "99", 1);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNotNull(p.ContinuationToken);
			p = TableStore.GetByIndexPartitionKeyPaged("Name", 99, 1, p.ContinuationToken);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNull(p.ContinuationToken);

		}

		[TestMethod]
		public async Task get_by_index_partition_key_paged_async()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var p = await TableStore.GetByIndexPartitionKeyPagedAsync("Name", 99, 1);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNotNull(p.ContinuationToken);
			p = await TableStore.GetByIndexPartitionKeyPagedAsync("Name", 99, 1, p.ContinuationToken);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNull(p.ContinuationToken);

		}

		[TestMethod]
		public async Task get_by_index_partition_key_string_paged_async()
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
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};
			await TableStore.InsertAsync(employee);
			await TableStore.InsertAsync(employee2);

			var p = await TableStore.GetByIndexPartitionKeyPagedAsync("Name", "99", 1);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNotNull(p.ContinuationToken);
			p = await TableStore.GetByIndexPartitionKeyPagedAsync("Name", 99, 1, p.ContinuationToken);
			Assert.AreEqual(1, p.Items.Count);
			Assert.IsNull(p.ContinuationToken);

		}
	}
}
