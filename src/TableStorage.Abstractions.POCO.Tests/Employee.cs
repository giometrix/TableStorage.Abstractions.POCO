using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableStorage.Abstractions.POCO.Tests
{
	public class Employee
	{
		public int CompanyId { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public Department Department { get; set; }
	}
}
