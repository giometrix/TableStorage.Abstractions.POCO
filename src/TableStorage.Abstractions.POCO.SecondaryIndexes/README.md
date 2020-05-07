# TableStorage.Abstractions.POCO.SecondaryIndexes
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4341bd0912b741f797f0ac204562fe18)](https://app.codacy.com/manual/giometrix/TableStorage.Abstractions.POCO?utm_source=github.com&utm_medium=referral&utm_content=giometrix/TableStorage.Abstractions.POCO&utm_campaign=Badge_Grade_Dashboard)
[![Build status](https://ci.appveyor.com/api/projects/status/fx9j8yc06s9ib4n9?svg=true)](https://ci.appveyor.com/project/giometrix/tablestorage-abstractions-poco) [![Nuget](https://img.shields.io/nuget/v/TableStorage.Abstractions.POCO.SecondaryIndexes)](https://www.nuget.org/packages/TableStorage.Abstractions.POCO.SecondaryIndexes/)

This project builds on top of [TableStorage.Abstractions.POCO](https://github.com/giometrix/TableStorage.Abstractions.POCO) to introduce "secondary indexes" to [Azure Table Storage](https://github.com/giometrix/TableStorage.Abstractions.POCO). Internally this library uses an [intra/inter partition (or table) secondary index pattern](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-patterns).  When data gets mutated on your table store, the library takes care of reflecting the change in your secondary indexes.

## Caveats And Notes
1.  Indexes are managed through a library, _not_ Table Storage, thus data mutated outside of the library will not automatically be reflected in your indexes.
2.  Though Azure Table Storage does offer transactions within partitions, this library does not leverage this at this time.
3.  This library is intended for Azure Table Storage, not CosmosDB, which offers an Azure Table Storage API.  CosmosDB does offer secondary indexes, so this library may not be as useful there.

## Examples
Note that it may be useful to read about [TableStorage.Abstractions.POCO](https://github.com/giometrix/TableStorage.Abstractions.POCO) to better understand the examples below.

All of the examples will use the following classes:
```csharp
public class Employee
{
	public int CompanyId { get; set; }
	public int Id { get; set; }
	public string Name { get; set; }
	public Department Department { get; set; }
	public bool IsActive {get; set;} = true;
}

public class Department
{
	public int Id { get; set; }
	public string Name { get; set; }
}
```
### Instantiation
Indexes are just regular `PocoTableStore`s so you instantiate them like any other `PocoTableStore`.  Here we instantiate the entity store and an index store.  The `PocoTableStore` named `TableStore` will store records using `CompanyId` as a partition key, and `Id` as the row key.  The `PocoTableStore` named `IndexStore` will store records using `CompanyId` as the partition key, and `Name` as the row key.  In this example they use different tables.

```csharp
TableStore = new PocoTableStore<Employee, int, int>("IXTestEmployee", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Id);

IndexStore = new PocoTableStore<Employee, int, string>("IXTestEmployeeNameIndex", "UseDevelopmentStorage=true", e => e.CompanyId, e => e.Name);
```

Next we tie them together by using `AddIndex()`.  Indexes must be given a name so that you can specify which index to use when querying.  Here we name our index "Name."

```charp
TableStore.AddIndex("Name", IndexStore);
```
After adding the index, mutations that happen on `TableStore` will result in mutations in `IndexStore`.  For instance, if we insert a record as seen below, we can expect to find a corresponding record in `IndexStore.`

```charp
var employee = new Employee
{
	Name = "Test",
	CompanyId = 99,
	Id = 99,
	Department = new Department { Id = 5, Name = "Test" }
};
TableStore.Insert(employee);
```
### Conditional Indexes
Introduced in 1.1, you can now easily utilize conditional indexes.  Conditional indexes allow you to add data to table storage only when a certain condition is true.  Effectively this lets you easily place data into "buckets" that you can efficiently query later.

For example, suppose we want to quickly query only active employees.
We can add a new index as described below:
```charp
TableStore.AddIndex("ActiveEmployee", new PocoTableStore<Employee, 
int, int>("IXActiveEmployees", "UseDevelopmentStorage=true", 
e => e.CompanyId, e => e.Id), e => e.IsActive);
```
Getting all active employees is now as easy as
```charp
var activeEmployees = TableStore.GetByIndexPartitionKey("ActiveEmployee", 99);
```
This query would yield all active employees for company `99`, without penalty of an expensive partition scan at the server.

Note that conditional indexes are kept up to date, such that if a record were to no longer meet the condition (or later meet the condition), they will be removed or added to the index accordingly.
### Fetching Data
To fetch a single data point from the index, we use the `GetRecordByIndex` (or `GetRecordByIndexAsync`) extension method on the entity `PocoTableStore` (note that we are doing this on the main data store, not on the index, as a convenience):
```charp
var e = TableStore.GetRecordByIndex("Name", 99, "Test");
```

Sometimes it may be useful to fetch all of the records from a partition for an index, such as historical data (described later).  Example:
```csharp
var records = await TableStore.GetByIndexPartitionKeyAsync("Name", 99);
```

One use of this pattern can be to store the current entity in the main entity store, and to keep historical data in a separate table.  Here is an example of this pattern:
```csharp
var pKeyMapper = new KeyMapper<Employee, int>(e => e.Id.ToString(), int.Parse, e => e.Id, id => id.ToString());

var rKeyMapper = new SequentialKeyMapper<Employee, int>(true);

var keysConverter = new CalculatedKeysConverter<Employee, int, int>(pKeyMapper, rKeyMapper);

var logStore = new PocoTableStore<Employee, int, int>("IXLogIndex", "UseDevelopmentStorage=true", keysConverter);

TableStore.AddIndex("Log", logStore);

```
In the example above we create an index called "Log", which will use `Id` as the partition key and a decreasing sequence number for row key (so that the most recent record is always on top).  

If we want to fetch the history for employee 99, we do the following:
```csharp
var records = TableStore.GetByPartitionKey(99);
```

### Removing An Index
To remove an index without deleting data, use the `Reindex()` or `ReindexAsync()` extension method.

### Dropping An Index
To remove _and_ drop an index without deleting data, use the `DropIndex()` or `DropIndexAsync()` extension method.  Deleting the original table will also drop all indexes on that table.

### Seeding Or Reindexing
If you are adding an index to an existing table that already has data, or if for some reason data gets out of sync, you can use the `Reindex()` extension method, shown below.  Note that this method is not  yet optimized (for instance no batching is currently used).  On my machine home internet connection, and data size, it took 22 minutes to index 1 million rows.

```charp
await TableStore.ReindexAsync("Name", maxDegreeOfParallelism: 20, recordsIndexedCallback: i=>count = i);
```
Call backs are available to get status updates and errors.
