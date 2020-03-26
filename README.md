# TableStorage.Abstractions.POCO
[![Build status](https://ci.appveyor.com/api/projects/status/fx9j8yc06s9ib4n9?svg=true)](https://ci.appveyor.com/project/giometrix/tablestorage-abstractions-poco)
[![NuGet](https://img.shields.io/nuget/v/TableStorage.Abstractions.POCO.svg)](https://www.nuget.org/packages/TableStorage.Abstractions.POCO)
[![Nuget Downloads](https://img.shields.io/nuget/dt/TableStorage.Abstractions.POCO.svg?color=purple&logo=nuget)](https://www.nuget.org/packages/TableStorage.Abstractions.POCO)

This project builds on top of [TableStorage.Abstractions](https://github.com/Tazmainiandevil/TableStorage.Abstractions) (a repository wrapper over [Azure Table Storage](https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-how-to-use-dotnet)) and [TableStorage.Abstractions.TableEntityConverters](https://github.com/giometrix/TableStorage.Abstractions.TableEntityConverters) such that objects to be serialized to and from Azure Table Storage are Plain Old CLR Objects (POCO) rather than TableEntities.

### For secondary index support, check out [TableStorage.Abstractions.POCO.SecondaryIndexes](https://github.com/giometrix/TableStorage.Abstractions.POCO/tree/master/src/TableStorage.Abstractions.POCO.SecondaryIndexes).

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
### Insert Or Replace (Update)
```csharp
var employee = new Employee
{
	Name = "Test",
	CompanyId = 99,
	Id = 99,
	Department = new Department { Id = 5, Name = "Test" }
};
tableStore.InsertOrReplace(employee);
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
There may be situations where you want the partition key or row key to be calculated from information outside of your object (such as date, which can be a useful partition key), from multiple properties, or a fixed key (e.g. you don't need a row key).

Here's an example of using the ```CompanyId``` and ```DepartmentId``` as partition keys.

```csharp


var partitionKeyMapper = new CalculatedKeyMapper<Employee, PartitionKey>(e => $"{e.CompanyId}.{e.Department.Id}", key =>
{
	var parts = key.Split('.');
	var companyId = int.Parse(parts[0]);
	var departmentId = int.Parse(parts[1]);
	return new PartitionKey(companyId, departmentId);
}, key=>$"{key.CompanyId}.{key.DepartmentId}");

var rowKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id,
	id => id.ToString());

var keysConverter = new CalculatedKeysConverter<Employee, PartitionKey, int>(partitionKeyMapper, rowKeyMapper);

var tableStore = new PocoTableStore<Employee, PartitionKey, int>("TestEmployee", "UseDevelopmentStorage=true", keysConverter);
```

If you used a previous version of this library, you may remember a more complicated, more limited constructor for ```PocoTableStore```.  We've simplified things and added some flexibility by introducing ```IKeysConverter```, where implementations encapsulate the rules for converting to/from table storage keys.

Notice that we introduced a new class called ```PartitionKey```.  This class is a simple DTO to capture ```CompanyId``` and ```DepartmentId```.  A nice side effect of having a class for this is that we gain type safety and intellisense.

```csharp
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
```
Inserting data is the same as always:

```csharp
var employee = new Employee
{
	CompanyId = 1,
	Id = 1,
	Name = "Mr. Jim CEO",
	Department = new Department { Id = 22, Name = "Executive" }
};
tableStore.Insert(employee);
```
In table storage, the partition key for the above example would be "1.22" and its row key would be "1".

To retrieve the record, we can use ```PartitionKey``` to build the multi-part key.
```csharp
var record = tableStore.GetRecord(new PartitionKey(1, 22), 1);
```


#### Fixed Keys
Fixed keys are really just a specialization of calculated keys.  A scenario that you may run into sometimes is where you only need a single key, which is the case when you only query the data using point queries ("get by id").  In this scenario, you'll probably choose to supply a partition key and not a row key since in this case you'd get better throughput using partition keys in a high volume system (again, we are assuming a point-query-only scenario).

Note that in v1.3 of the library we've simplified fixed key scenarios by introducing a new ```FixedKey``` mapper, which will be consumed by the ```CalculatedKeysConverter```.

Again, we will use a contrived example.  Here we have use ```Id``` as partition key , and we always use the word "user" for rowkey, since this will not be used.

```charp
var partitionKeyMapper = new KeyMapper<Employee, int>(e =>e.CompanyId.ToString(), int.Parse, e => e.CompanyId, id =>id.ToString());
var rowKeyMapper = new FixedKeyMapper<Employee, int>("user");

var keysConverter = new CalculatedKeysConverter<Employee, int, int>(partitionKeyMapper, rowKeyMapper);
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

#### Sequential Keys
`SequentialKeyMapper` was introduced in v2.6 and is a bit different from other key mappers because the output isn't meant for point lookups.  This key mapper assigns keys in sequential order (forward or backward).  Because Azure Table Storage orders rows by row key, a sequential key allows you to use Azure Table Storage as a log.  

Coupled with [TableStorage.Abstractions.POCO.SecondaryIndexes](https://github.com/giometrix/TableStorage.Abstractions.POCO/tree/master/src/TableStorage.Abstractions.POCO.SecondaryIndexes) , you can do things like saving a historical record when mutating your main table entity.

Example:
```csharp
var pKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id, id => id.ToString());

var rKeyMapper = new SequentialKeyMapper<Employee, int>(true);

var keysConverter = new CalculatedKeysConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

tableStore = new PocoTableStore<Employee, int, int>("TestEmployee", "UseDevelopmentStorage=true", keysConverter);

var employee = new Employee
			{
				CompanyId = 1,
				Id = 242443,
				Name = "1",
				Department = new Department { Id = 22, Name = "Executive" }
			};

tableStore.Insert(employee);

employee.Name = "2";
tableStore.Insert(employee);

employee.Name = "3";
tableStore.Insert(employee);

// order will be 3, 2, 1 because we are sorting in sequential order						
```

### Further Filtering (Beyond Partition & Row Keys)
New to v1.2, we now include the ability to filter on properties outside of partition and row keys.  Please note that this filtering occurs outside of table storage, so please consider using at least the partition key for best results.

Example:

```charp
var records = tableStore.GetByPartitionKey(1, e=>e.Name == "Jim CEO");
```

In this example we get all records in parition "1" where the name is "Jim CEO". 

#### Timestamp
Azure Table Storage entities always have a timestamp.  If your POCO has a field named Timestamp, that is a `DateTimeOffset`, `DateTime` or `String`, then this property will automatically be hydrated wit the timestamp provided by Azure Table Storage.

Modifications to the Timestamp property do not get persisited.  This is exactly how it works with the Azure Table Storage SDK.

Considerations for taking a similar approach to ETag are being considered.
