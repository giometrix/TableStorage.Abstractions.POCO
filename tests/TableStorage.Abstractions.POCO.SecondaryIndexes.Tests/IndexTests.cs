using System;
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
		public async Task get_record_by_index()
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
		public async Task get_record_by_index_as_string()
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
		public async Task get_record_by_index_async()
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
		public async Task get_record_by_index_as_string_async()
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


		[TestMethod]
		public async Task insert_record_with_secondary_index_using_sequential_key_mapper()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());
			var rKeyMapper = new SequentialKeyMapper<Employee, int>(true);

			var keysConverter = new CalculatedKeysConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			var logStore =
				new PocoTableStore<Employee, int, int>("IXLogIndex", "UseDevelopmentStorage=true", keysConverter);

			TableStore.AddIndex("Log", logStore);


			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};
			
			await TableStore.InsertAsync(employee);

			var records = TableStore.GetByPartitionKey(99);
			Assert.AreEqual(1, records.Count());

		}

		[TestMethod]
		public async Task update_record_with_secondary_index_using_sequential_key_mapper()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());
			var rKeyMapper = new SequentialKeyMapper<Employee, int>(true);

			var keysConverter = new CalculatedKeysConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			var logStore =
				new PocoTableStore<Employee, int, int>("IXLogIndex", "UseDevelopmentStorage=true", keysConverter);

			TableStore.AddIndex("Log", logStore);


			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};

			await TableStore.InsertAsync(employee);
			employee.Name = "XXX";
			await TableStore.UpdateAsync(employee);

			var records = await TableStore.GetByIndexPartitionKeyAsync("Log", 99);
			Assert.AreEqual(2, records.Count());

		}

		[TestMethod]
		public async Task delete_record_with_secondary_index_using_sequential_key_mapper()
		{
			var pKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
				id => id.ToString());
			var rKeyMapper = new SequentialKeyMapper<Employee, int>(true);

			var keysConverter = new CalculatedKeysConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

			var logStore =
				new PocoTableStore<Employee, int, int>("IXLogIndex", "UseDevelopmentStorage=true", keysConverter);

			TableStore.AddIndex("Log", logStore);


			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" }
			};

			await TableStore.InsertAsync(employee);
			await TableStore.DeleteAsync(employee);

			var records = await TableStore.GetByIndexPartitionKeyAsync("Log", 99);
			Assert.AreEqual(1, records.Count());

		}

		[TestMethod]
		public async Task reindex()
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

			int count = 0;

			await TableStore.ReindexAsync("Name", maxDegreeOfParallelism: 20, recordsIndexedCallback: i=>count = i);

			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public async Task delete_from_index_using_fixed_key_row()
		{
			var pkMapper = new KeyMapper<Employee, string>(e => e.Name, n=>n, e=>e.Name, n=>n);
			var rkMapper = new FixedKeyMapper<Employee,string>("emp");
			var keyConverter = new CalculatedKeysConverter<Employee,string,string>(pkMapper, rkMapper);
			PocoTableStore<Employee, int, string> table = null;
			try
			{
				table = new PocoTableStore<Employee, int, string>("IXTestEmployeeUT1", "UseDevelopmentStorage=true", e=>e.CompanyId, e=>e.Id);

				var index = new PocoTableStore<Employee, string, string>("IXTestEmployeeNameIndexUT1", "UseDevelopmentStorage=true",
				keyConverter);

				table.AddIndex("Name2", index);

			
				var employee = new Employee
				{
					Name = "Test",
					CompanyId = 99,
					Id = 99,
					Department = new Department { Id = 5, Name = "Test" }
				};
				await table.InsertOrReplaceAsync(employee);
				await table.DeleteAsync(employee);
			}
			finally
			{
				await table.DeleteTableAsync();
			}
		}

		[TestMethod]
		public async Task conditional_index()
		{
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee,int,int>("IXActiveEmployees", "UseDevelopmentStorage=true", e=>e.CompanyId, e=>e.Id), e=>e.IsActive);
			var employee = new Employee
			{
				Name = "Active",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
			};
			TableStore.Insert(employee);

			var employee2 = new Employee
			{
				Name = "Inactive",
				CompanyId = 99,
				Id = 100,
				Department = new Department {Id = 5, Name = "Test"}
			};

			TableStore.Insert(employee2);

			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);

			Assert.AreEqual(1, activeEmployees.Count());
			Assert.AreEqual("Active", activeEmployees.Single().Name);

		}

		[TestMethod]
		public async Task updating_record_in_conditional_index_stays_up_to_date()
		{
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id), e => e.IsActive);
			var employee = new Employee
			{
				Name = "Active",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
			};
			TableStore.Insert(employee);

			var employee2 = new Employee
			{
				Name = "Inactive",
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};

			TableStore.Insert(employee2);
			employee.Name = "ActiveX";
			TableStore.Update(employee);
			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);

			Assert.AreEqual(1, activeEmployees.Count());
			Assert.AreEqual("ActiveX", activeEmployees.Single().Name);

		}

		[TestMethod]
		public async Task updating_record_in_conditional_index_with_value_that_disqualifies_it_from_index_gets_removed()
		{
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id), e => e.IsActive);
			var employee = new Employee
			{
				Name = "Active",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
			};
			TableStore.Insert(employee);

			var employee2 = new Employee
			{
				Name = "Inactive",
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};

			TableStore.Insert(employee2);
			employee.IsActive = false;
			TableStore.Update(employee);
			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);

			Assert.AreEqual(0, activeEmployees.Count());
			
		}

		[TestMethod]
		public async Task updating_record_in_conditional_index_with_value_that_qualifies_it_for_index_gets_added()
		{
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id), e => e.IsActive);
			var employee = new Employee
			{
				Name = "Active",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
			};
			TableStore.Insert(employee);

			var employee2 = new Employee
			{
				Name = "Inactive",
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};

			TableStore.Insert(employee2);
			employee2.IsActive = true;
			TableStore.Update(employee2);
			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);

			Assert.AreEqual(2, activeEmployees.Count());

		}

		[TestMethod]
		public async Task deleting_record_also_removes_it_from_conditional_index()
		{
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id), e => e.IsActive);
			var employee = new Employee
			{
				Name = "Active",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
			};
			TableStore.Insert(employee);

			var employee2 = new Employee
			{
				Name = "Inactive",
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};

			TableStore.Insert(employee2);
			TableStore.Delete(employee);
			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);

			Assert.AreEqual(0, activeEmployees.Count());

		}

		[TestMethod]
		public async Task bulk_insert_with_conditional_index()
		{
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id), e => e.IsActive);
			var employee = new Employee
			{
				Name = "Active",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
			};
			
			var employee2 = new Employee
			{
				Name = "Inactive",
				CompanyId = 99,
				Id = 100,
				Department = new Department { Id = 5, Name = "Test" }
			};

			TableStore.Insert(new Employee[]{employee,employee2});
			
			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);

			Assert.AreEqual(1, activeEmployees.Count());
			Assert.AreEqual("Active", activeEmployees.First().Name);

		}

		[TestMethod]
		public async Task reindex_conditional_index()
		{
			var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department { Id = 5, Name = "Test" },
				IsActive = true
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
			TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id), e => e.IsActive);
			int count = 0;

			await TableStore.ReindexAsync("ActiveEmployee", maxDegreeOfParallelism: 20, recordsIndexedCallback: i => count = i);

			var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);


			Assert.AreEqual(1, activeEmployees.Count());
		}
	}
}
