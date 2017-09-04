using System;

namespace TableStorage.Abstractions.POCO
{
	public class FixedKeyMapper<T, TKey> : KeyMapper<T, TKey>
	{
		public FixedKeyMapper(string key) : base(x=>key, null, null, x=>key)
		{
		}
	}
}