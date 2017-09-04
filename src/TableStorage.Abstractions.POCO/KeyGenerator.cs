using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TableStorage.Abstractions.POCO
{
	public static class KeyGenerator
	{
		private static readonly ConcurrentDictionary<Type, Func<dynamic, string>> _partitionKeyExpressions = 
			new ConcurrentDictionary<Type, Func<dynamic, string>>();

		private static readonly ConcurrentDictionary<Type, Func<dynamic, string>> _rowKeyExpressions =
			new ConcurrentDictionary<Type, Func<dynamic, string>>();

		public static void DefineParitionKey(Type type, Func<dynamic, string> partitionKeyExpression)
		{
			_partitionKeyExpressions.TryGetValue(type, out Func<dynamic, string> expr);
			if (expr == null)
			{
				_partitionKeyExpressions[type] = partitionKeyExpression;
			}

		}

		public static void DefineRowKey(Type type, Func<dynamic, string> rowKeyExpression)
		{
			_rowKeyExpressions.TryGetValue(type, out Func<dynamic, string> expr);
			if (expr == null)
			{
				_rowKeyExpressions[type] = rowKeyExpression;
			}

		}

		
		public static string PartitionKey<T>(object obj)
		{
			try
			{
				return _partitionKeyExpressions[typeof(T)](obj);
			}
			catch (NullReferenceException e)
			{
				throw new Exception("Partition definition was not provided", e);
			}
			
		}

		public static string RowKey<T>(object obj)
		{
			try
			{
				return _rowKeyExpressions[typeof(T)](obj);
			}
			catch (NullReferenceException e)
			{
				throw new Exception("Row key definition was not provided", e);
			}

		}
	}
}
