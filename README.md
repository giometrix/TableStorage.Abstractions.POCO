# TableStorage.Abstractions.POCO
[![Build status](https://ci.appveyor.com/api/projects/status/fx9j8yc06s9ib4n9?svg=true)](https://ci.appveyor.com/project/giometrix/tablestorage-abstractions-poco)
[![NuGet](https://img.shields.io/nuget/v/TableStorage.Abstractions.POCO.svg)](https://www.nuget.org/packages/TableStorage.Abstractions.POCO/1.0.0)

This project builds on top of [TableStorage.Abstractions](https://github.com/Tazmainiandevil/TableStorage.Abstractions) (a repository wrapper over [Azure Table Storage](https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-how-to-use-dotnet)) and [TableStorage.Abstractions.TableEntityConverters](https://github.com/giometrix/TableStorage.Abstractions.TableEntityConverters) such that objects to be serialized to and from Azure Table Storage are Plain Old CLR Objects (POCO) rather than TableEntities.

## Examples
Assume we have the following two classes, which we wish to serialize to and from Azure Table Storage:

```csharp
public class Employee
{
  public int CompanyId { get; set; }
  public int Id { get; set; }
  public string Name { get; set; }
  public Department Department { get; set; }
}
  
public class Department
{
  public int Id { get; set; }
  public string Name { get; set; }
}
```

In our examples we will be using CompanyId as the partition key and (Employee) Id as the row key.

### Instantiating
```charp
var tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", 
     e => e.CompanyId, e => e.Id);
```
Here we create our table store and specify our partition key (CompanyId) and row key (Id).

### Inserting
```charp
var employee = new Employee
			{
				Name = "Test",
				CompanyId = 99,
				Id = 99,
				Department = new Department {Id = 5, Name = "Test"}
			};
tableStore.Insert(employee);
```

### Updating
```charp
employee.Name = "Test2";
tableStore.Update(employee);
```

### Get Record By Partition Key And Row Key
```charp
var employee = tableStore.GetRecord(1, 42);
```

### Get All Records In Partition
```charp
var employees = tableStore.GetByPartitionKey(1);
```

### Delete Record
```charp
tableStore.Delete(employee);
```
### Excluding Properties From Serialization
You may have some properties that you don't want to persist to Azure Table Storage.  To ignore properties, use the ```ignoredProperties``` parameter.
```charp
var tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
          e => e.CompanyId, e => e.Id, e=>e.Department);
	  
tableStore.Insert(employee);
```
In this example we ignored the ```Department``` property.

### Calculated Keys
There may be situations where you want the partition key or row key to be calculated from information outside of your object (such as date, which can be a useful partition key) or where you want to use a fixed key (e.g. you don't need a row key).

Here's a contrived example of using date as a partition key:

```csharp
var date = new DateTime(2017, 8, 31).ToString("yyyyMMdd");

var tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
partitionProperty: e => e.CompanyId, 
rowProperty: e => e.Id, 
calculatedPartitionKey: e => $"{date}_{e.CompanyId}", 
calculatedRowKey: e => e.Id.ToString(),
calculatedPartitionKeyFromParameter: x => $"{date}_{x}",
calculatedRowKeyFromParameter: x => x.ToString(),
convertPartitionKey: pk => int.Parse(pk.Substring("yyyyMMdd_".Length)), 
convertRowKey: int.Parse);
```

It's a little more complicated than I would have liked (I'm open to suggestions), but I'll try to explain the best I can:

```calculatedPartitionKey```: how to build the partition key from the object.  In our case it's date + ```CompanyId```.

```calculatedRowKey```: how to build the row key from the object.  In our case it's ```Id```.

```calculatedPartitionKeyFromParameter```: how to build the calculated partition key from the given partition key.  In our case we would provide ```CompanyId``` and the output would be date + ```CompanyId```.

```calculatedRowFromParameter```: how to build the calculated row key from the given row key.  In our case we would provide ```Id``` and the output would be the stringified version.

Whew.... that was a lot of work.  Was it worth it?  I sure think so, since it makes working with the records feel much more natural.  To insert a record you continue to use syntax such as the following:
```csharp
var employee = new Employee
{
	CompanyId = 1,
	Id = 142,
	Name = "Mr. Jim CEO",
	Department = new Department { Id = 22, Name = "Executive" }
};
tableStore.Insert(employee);
```

Notice that you don't need to worry about constructing the partition key.  Similarly, to query, you continue to query using just the simple identifiers like so:
```charp
var record = tableStore.GetRecord(1, 142);
```
#### Fixed Keys
Fixed keys are really just a specialization of calculated keys.  A scenario that you may run into sometimes is where you only need a single key, which is the case when you only query the data using point queries ("get by id").  In this scenario, you'll probably choose to supply a partition key and not a row key since in this case you'd get better throughput using partition keys in a high volume system (again, we are assuming a point-query-only scenario).

Again, we will use a contrived example.  Here we have use ```Id``` as partition key , and we always use the word "user" for rowkey, since this will not be used.

```charp
tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true",
				partitionProperty: e => e.Id, rowProperty: null, calculatedPartitionKey: e => e.Id.ToString(), calculatedRowKey: e => "user",
calculatedPartitionKeyFromParameter: x => x.ToString(),
calculatedRowKeyFromParameter: x => "user",
convertPartitionKey: int.Parse, convertRowKey: null);
```	

Inserting the data remains the same:
```csharp
var employee = new Employee
{
	Id = 1,
	Name = "Mr. Jim CEO",
	Department = new Department { Id = 22, Name = "Executive" }
};

tableStore.Insert(employee);
```

As always, we have 2 ways of querying the data:

```csharp
var record = tableStore.GetRecord("1", "user");
```

We can also get the record using the typed overload, though in this case the second parameter is thrown away since there is no row key.  I prefer to use ```int.Min``` to show that this value is thrown away.

```csharp
record = tableStore.GetRecord(1, int.MinValue);
```

Note that our table store was ```PocoTableStore<Employee, int, int>```, but that last generic could have been anything since it is thrown away.  So if you prefer, you can make it ```PocoTableStore<Employee, int, string>``` and then query like so: 
```var record = tableStore.GetRecord(142, "user");```
which is both clear and provides type safety.

### Further Filtering (Beyond Parition & Row Keys)
New to v1.2, we now include the ability to filter on properties outside of partition and row keys.  Please note that this filtering occurs outside of table storage, so please consider using at least the partition key for best results.

Example:

```charp
var records = tableStore.GetByPartitionKey(1, e=>e.Name == "Jim CEO");
```

In this example we get all records in parition "1" where the name is "Jim CEO".
