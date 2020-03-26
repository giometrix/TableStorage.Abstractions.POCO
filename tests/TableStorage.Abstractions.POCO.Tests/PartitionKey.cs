namespace TableStorage.Abstractions.POCO.Tests
{
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
	
}