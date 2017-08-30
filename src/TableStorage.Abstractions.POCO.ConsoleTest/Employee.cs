using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TableStorage.Abstractions.POCO.ConsoleTest
{
	public class Department
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
	public class Employee
	{
		public int CompanyId { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		[JsonIgnore]
		public Department Department { get; set; }
	}
}
