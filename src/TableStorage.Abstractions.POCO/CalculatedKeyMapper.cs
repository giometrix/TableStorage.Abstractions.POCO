using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TableStorage.Abstractions.POCO
{
	public class CalculatedKeyMapper<T, TKey> : KeyMapper<T,TKey>
	{
		public CalculatedKeyMapper(Func<T, string> toKey, Func<string, TKey> fromKey, Func<TKey, string> toKeyFromParameter) : base(toKey, fromKey, null, toKeyFromParameter)
		{
		}
	}
}
