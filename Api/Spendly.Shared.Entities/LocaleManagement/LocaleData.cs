namespace Spendly.Shared.Entities.LocaleManagement;

public static class LocaleData
{
	public static string DefaultLanguageCode => "en";
	public static string DefaultRegionCode => "US";
	public static string DefaultCurrencyAcronym => "USD";
	public static string DefaultLocaleCode => "en-US";
	public readonly static List<Language> Languages = new()
	{
		new Language {Code = "en", Name = "English"},
		new Language {Code = "fr", Name = "Français"},
		new Language {Code = "de", Name = "Deutsch"},
		new Language {Code = "es", Name = "Español"},
		new Language {Code = "it", Name = "Italiano"},
		new Language {Code = "pt", Name = "Português"},
		new Language {Code = "sv", Name = "Svenska"},
		new Language {Code = "fi", Name = "Suomi"},
		new Language {Code = "da", Name = "Dansk"},
		new Language {Code = "nl", Name = "Nederlands"},
		new Language {Code = "no", Name = "Norsk"},
		new Language {Code = "pl", Name = "Polski"}
	};

	public readonly static List<Region> Regions = new()
	{
		// --- Europe ---
		new Region
		{
			Code = "AT", Name = "Austria",
			Locales = new List<Locale>
			{
				new Locale {Code = "de-AT", DisplayName = "Deutsch - Austria", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "BE", Name = "Belgium",
			Locales = new List<Locale>
			{
				new Locale {Code = "fr-BE", DisplayName = "Français - Belgium", DecimalSeparator = ",", ThousandSeparator = " "},
				new Locale {Code = "nl-BE", DisplayName = "Nederlands - Belgium", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "CH", Name = "Switzerland",
			Locales = new List<Locale>
			{
				new Locale {Code = "de-CH", DisplayName = "Deutsch - Switzerland", DecimalSeparator = ".", ThousandSeparator = "'"},
				new Locale {Code = "fr-CH", DisplayName = "Français - Switzerland", DecimalSeparator = ",", ThousandSeparator = " "},
				new Locale {Code = "it-CH", DisplayName = "Italiano - Switzerland", DecimalSeparator = ".", ThousandSeparator = "'"}
			}
		},
		new Region
		{
			Code = "DE", Name = "Germany",
			Locales = new List<Locale>
			{
				new Locale {Code = "de-DE", DisplayName = "Deutsch - Germany", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "ES", Name = "Spain",
			Locales = new List<Locale>
			{
				new Locale {Code = "es-ES", DisplayName = "Español - Spain", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "FI", Name = "Finland",
			Locales = new List<Locale>
			{
				new Locale {Code = "fi-FI", DisplayName = "Suomi - Finland", DecimalSeparator = ",", ThousandSeparator = " "}
			}
		},
		new Region
		{
			Code = "FR", Name = "France",
			Locales = new List<Locale>
			{
				new Locale {Code = "fr-FR", DisplayName = "Français - France", DecimalSeparator = ",", ThousandSeparator = " "}
			}
		},
		new Region
		{
			Code = "GB", Name = "United Kingdom",
			Locales = new List<Locale>
			{
				new Locale {Code = "en-GB", DisplayName = "English - United Kingdom", DecimalSeparator = ".", ThousandSeparator = ","}
			}
		},
		new Region
		{
			Code = "IE", Name = "Ireland",
			Locales = new List<Locale>
			{
				new Locale {Code = "en-IE", DisplayName = "English - Ireland", DecimalSeparator = ".", ThousandSeparator = ","},
				new Locale {Code = "ga-IE", DisplayName = "Gaeilge - Ireland", DecimalSeparator = ".", ThousandSeparator = ","}
			}
		},
		new Region
		{
			Code = "IT", Name = "Italy",
			Locales = new List<Locale>
			{
				new Locale {Code = "it-IT", DisplayName = "Italiano - Italy", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "NL", Name = "Netherlands",
			Locales = new List<Locale>
			{
				new Locale {Code = "nl-NL", DisplayName = "Nederlands - Netherlands", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "NO", Name = "Norway",
			Locales = new List<Locale>
			{
				new Locale {Code = "no-NO", DisplayName = "Norsk - Norway", DecimalSeparator = ",", ThousandSeparator = " "}
			}
		},
		new Region
		{
			Code = "PT", Name = "Portugal",
			Locales = new List<Locale>
			{
				new Locale {Code = "pt-PT", DisplayName = "Português - Portugal", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "SE", Name = "Sweden",
			Locales = new List<Locale>
			{
				new Locale {Code = "sv-SE", DisplayName = "Svenska - Sweden", DecimalSeparator = ",", ThousandSeparator = " "}
			}
		},
		new Region
		{
			Code = "DK", Name = "Denmark",
			Locales = new List<Locale>
			{
				new Locale {Code = "da-DK", DisplayName = "Dansk - Denmark", DecimalSeparator = ",", ThousandSeparator = "."}
			}
		},
		new Region
		{
			Code = "PL", Name = "Poland",
			Locales = new List<Locale>
			{
				new Locale {Code = "pl-PL", DisplayName = "Polski - Poland", DecimalSeparator = ",", ThousandSeparator = " "}
			}
		},

		// --- North America ---
		new Region
		{
			Code = "US", Name = "United States",
			Locales = new List<Locale>
			{
				new Locale {Code = "en-US", DisplayName = "English - United States", DecimalSeparator = ".", ThousandSeparator = ","},
				new Locale {Code = "es-US", DisplayName = "Español - United States", DecimalSeparator = ".", ThousandSeparator = ","}
			}
		},
		new Region
		{
			Code = "CA", Name = "Canada",
			Locales = new List<Locale>
			{
				new Locale {Code = "en-CA", DisplayName = "English - Canada", DecimalSeparator = ".", ThousandSeparator = ","},
				new Locale {Code = "fr-CA", DisplayName = "Français - Canada", DecimalSeparator = ",", ThousandSeparator = " "}
			}
		},

		// --- Australia ---
		new Region
		{
			Code = "AU", Name = "Australia",
			Locales = new List<Locale>
			{
				new Locale {Code = "en-AU", DisplayName = "English - Australia", DecimalSeparator = ".", ThousandSeparator = ","}
			}
		}
	};

}
