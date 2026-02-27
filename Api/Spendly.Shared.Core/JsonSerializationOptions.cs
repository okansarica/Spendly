namespace Spendly.Shared.Core;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonSerializationOptions
{
	public readonly static JsonSerializerOptions Default = new JsonSerializerOptions
	{
		// Serialize ederken camelCase yap
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

		// Deserialize ederken case'e duyarsız ol
		PropertyNameCaseInsensitive = true,

		// Enum'ları isim olarak serialize et (ismini değiştirmeden)
		Converters = { new JsonStringEnumConverter() },

		// Dilersen pretty print de açabilirsin
		// WriteIndented = true
	};
}
