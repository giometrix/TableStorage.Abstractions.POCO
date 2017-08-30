using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableStorage.Abstractions.POCO.ConsoleTest
{
	class Program
	{
		static void Main(string[] args)
		{

			PocoTableStore<Employee, int, int> tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", e=>e.CompanyId, e=>e.Id);
			tableStore.DeleteTable();
			tableStore.CreateTable();

			
			var employee = new Employee {CompanyId = 1, Id = 1, Name = "Jim CEO", Department = new Department{Id = 22, Name = "Executive"}};
			var employee2 = new Employee { CompanyId = 1, Id = 2, Name = "Mary CTO" };
			var employee3 = new Employee{CompanyId = 2, Id=1, Name = "Lucy CEO"};

			//test insert enum
			tableStore.Insert(new Employee[]{employee, employee2, employee3});

			var employee4 = new Employee { CompanyId = 2, Id = 2, Name = "Mike CFO" };

			//test single insert
			tableStore.Insert(employee4);

			//test get record
			var record = tableStore.GetRecord("1", "1");

			//test get record typed
			record = tableStore.GetRecord(1, 1);

			//test get by partition
			var records = tableStore.GetByPartitionKey("1");

			//test get by partition typed
			 records = tableStore.GetByPartitionKey(1);

			//test get by partition key paged
			var pagedResult = tableStore.GetByPartitionKeyPaged("1", 1);


			//test get by partition key paged typed
			pagedResult = tableStore.GetByPartitionKeyPaged(1, 1);

			//test bet by row key
			records = tableStore.GetByRowKey("2");

			//test bet by row key typed
			records = tableStore.GetByRowKey(1);

			//test bet by row key paged
			pagedResult = tableStore.GetByRowKeyPaged("2",1);


			//test bet by row key paged typed
			pagedResult = tableStore.GetByRowKeyPaged(2, 1);

			//test get all records
			records = tableStore.GetAllRecords();

			//test get all records paged
			pagedResult = tableStore.GetAllRecordsPaged(2);

			//test insert async
			var employee5 = new Employee {CompanyId = 3, Id = 22, Name = "Larry Accountant"};
			tableStore.InsertAsync(employee5).Wait();


			//test update 
			employee5.Name = "Larry Bird";
			tableStore.Update(employee5);

			record = tableStore.GetRecord(3, 22);


			//test update async
			employee5.Name = "Michael Jordan";
			tableStore.UpdateAsync(employee5).Wait();

			record = tableStore.GetRecord(3, 22);


			//test update wildcard etag
			employee5.Name = "Larry Jordan";
			tableStore.UpdateUsingWildcardEtag(employee5);

			record = tableStore.GetRecord(3, 22);


			//test update wildcard etag async
			employee5.Name = "XXXX Jordan";
			tableStore.UpdateUsingWildcardEtagAsync(employee5);

			record = tableStore.GetRecord(3, 22);


			//test delete
			tableStore.Delete(employee5);

			records = tableStore.GetAllRecords();


			//test delete wildcard
			tableStore.DeleteUsingWildcardEtag(employee4);

			records = tableStore.GetAllRecords();


			//test delete async
			tableStore.DeleteAsync(employee3).Wait();

			records = tableStore.GetAllRecords();


			//test delete wildcard async
			tableStore.DeleteUsingWildcardEtagAsync(employee2).Wait();

			records = tableStore.GetAllRecords();

			//test get record async
			record = tableStore.GetRecordAsync("1", "1").Result;

			//test get record async typed
			record = tableStore.GetRecordAsync(1, 1).Result;

			//test get by partition key async
			records = tableStore.GetByPartitionKeyAsync("1").Result;

			//test get by partition key async typed
			records = tableStore.GetByPartitionKeyAsync(1).Result;


			//test get by partition key paged async
			pagedResult = tableStore.GetByPartitionKeyPagedAsync("1").Result;

			//test get by partition key paged async typed
			pagedResult = tableStore.GetByPartitionKeyPagedAsync(1).Result;


			//test get by row key async
			records = tableStore.GetByRowKeyAsync("1").Result;

			//test get by row key async typed
			records = tableStore.GetByRowKeyAsync(1).Result;


			//test get by row key paged async
			pagedResult = tableStore.GetByRowKeyPagedAsync("1").Result;

			//test get by row key paged async typed
			pagedResult = tableStore.GetByRowKeyPagedAsync(1).Result;


			//test getall records async
			records = tableStore.GetAllRecordsAsync().Result;


			//test getall records paged async
			pagedResult = tableStore.GetAllRecordsPagedAsync().Result;

			//test get record count async
			var count = tableStore.GetRecordCountAsync().Result;

			tableStore.DeleteTable();


			//test insert with ignored property
			var tableStore2 = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id, e=>e.Department);
			tableStore2.Insert(employee);
			record = tableStore2.GetRecord(1, 1);
			tableStore.DeleteTable();
			
		}
	}
}
