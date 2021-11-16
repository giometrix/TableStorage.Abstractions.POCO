using System;

namespace TableStorage.Abstractions.POCO.Tests
{
	public class Employee
	{
		public int CompanyId { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public Department Department { get; set; }
		public DateTimeOffset Timestamp { get; set; }
	}
	
	public class EmployeeWithHireDate : Employee
	{
		public DateTime HireDate { get; set; }
	}
}