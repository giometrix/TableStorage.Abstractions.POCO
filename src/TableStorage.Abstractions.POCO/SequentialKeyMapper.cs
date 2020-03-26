using System;
using Xtensible.Time;

namespace TableStorage.Abstractions.POCO
{
	public class SequentialKeyMapper<T, TKey> : KeyMapper<T, TKey>
	{
		private static Random _rng = new Random();

		private static string GetSequence(bool reverseOrder)
		{
			long sequence;
			if (!reverseOrder)
			{
				sequence = (Clock.Default.UtcNow.Ticks);
			}
			else
			{
				sequence = (DateTimeOffset.MaxValue.Ticks - Clock.Default.UtcNow.Ticks);
			}

			return $"{sequence.ToString("D20")}.{_rng.Next(9999)}";
		}
		public SequentialKeyMapper(bool reverseOrder = true) : base(x=>GetSequence(reverseOrder), null, null,x=>GetSequence(reverseOrder))
		{
		}

	}
}