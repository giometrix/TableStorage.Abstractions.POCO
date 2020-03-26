using System;
using System.Linq.Expressions;

namespace TableStorage.Abstractions.POCO
{
	public class KeyMapper<T, TKey>
	{
		private readonly Func<T, string> _toKey;
		private readonly Func<TKey, string> _toKeyFromParameter;
		private readonly Func<string, TKey> _fromKey;

		public KeyMapper(Func<T, string> toKey, Func<string, TKey> fromKey, Expression<Func<T, object>> keyProperty, Func<TKey, string> toKeyFromParameter)
		{
			this._toKey = toKey ?? throw new ArgumentNullException(nameof(toKey));
			_fromKey = fromKey;
			KeyProperty = keyProperty;
			_toKeyFromParameter = toKeyFromParameter;
		}

		public Expression<Func<T, object>> KeyProperty { get; }

		public string ToKey(T obj)
		{
			return _toKey(obj);
		}

		public string ToKeyFromParameter(TKey key)
		{
			return _toKeyFromParameter(key);
		}

		public TKey FromKey(string key)
		{
			return _fromKey(key);
		}
	}
}