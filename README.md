# TableStorage.Abstractions.POCO
This project builds on top of [TableStorage.Abstractions](https://github.com/Tazmainiandevil/TableStorage.Abstractions) (an abstraction over [Azure Table Storage](https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-how-to-use-dotnet)) and [TableStorage.Abstractions.TableEntityConverters](https://github.com/giometrix/TableStorage.Abstractions.TableEntityConverters) such that objects to be serialized to and from Azure Table Storage are Plain Old CLR Objects (POCO) rather than TableEntities.

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

### Delete record
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
