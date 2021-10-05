using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TableStorage.Abstractions.POCO.Tests
{
	public class KeysJsonConverter : JsonConverter
	{
		private readonly Type[] _types;

		public override bool CanRead => false;

		public KeysJsonConverter(params Type[] types)
		{
			_types = types;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var t = JToken.FromObject(value);

			if (t.Type != JTokenType.Object)
			{
				t.WriteTo(writer);
			}
			else
			{
				var o = (JObject)t;
				IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

				o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

				o.WriteTo(writer);
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			throw new NotImplementedException(
				"Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override bool CanConvert(Type objectType)
		{
			return _types.Any(t => t == objectType);
		}
	}
}