namespace Spendly.Shared.Localization;

using System.Collections.Concurrent;
using System.Text.Json;

public class TranslationService(string? translationsPath = null)
{
	private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();
	private readonly string _translationsPath = translationsPath ?? Path.Combine(AppContext.BaseDirectory, "translations");

	public string Translate(string lang, string key)
	{
		var translations = _cache.GetOrAdd(lang, LoadTranslations);
		return translations.GetValueOrDefault(key, key);
	}

	private Dictionary<string, string> LoadTranslations(string lang)
	{
		var path = Path.Combine(_translationsPath, $"{lang}.json");
        
		if (!File.Exists(path))
		{
			throw new FileNotFoundException($"Translation file not found: {path}");
		}

		var json = File.ReadAllText(path);
		var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        
		if (data == null)
		{
			throw new InvalidOperationException($"Failed to deserialize translation file: {path}");
		}
            
		return data;
	}
}
