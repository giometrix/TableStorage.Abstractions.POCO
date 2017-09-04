using System;
using System.Linq.Expressions;

namespace TableStorage.Abstractions.POCO
{
	public class KeyMapper<T, TKey>
	{
		private Func<T, string> _toKey;
		private Func<TKey, string> _toKeyFromParameter;
		private Func<string, TKey> _fromKey;
		private Expression<Func<T, object>> _keyProperty;
		public KeyMapper(Func<T, string> toKey, Func<string, TKey> fromKey, Expression<Func<T, object>> keyProperty, Func<TKey, string> toKeyFromParameter)
		{
			if (toKey == null) throw new ArgumentNullException(nameof(toKey));
			//if (fromKey == null) throw new ArgumentNullException(nameof(fromKey));
	
			this._toKey = toKey;
			_fromKey = fromKey;
			_keyProperty = keyProperty;
			_toKeyFromParameter = toKeyFromParameter;
		}

		public Expression<Func<T, object>> KeyProperty
		{
			get { return _keyProperty; }
		}

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