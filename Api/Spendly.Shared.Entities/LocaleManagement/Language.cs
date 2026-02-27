namespace Spendly.Shared.Entities.LocaleManagement;

public class Language
{
	public string Code { get; set; }   // "tr"
	public string Name { get; set; }   // "Türkçe"
}


public class Region
{
	public string Code { get; set; } // "FR", "CH"
	public string Name { get; set; } // "France", "Switzerland"
	public List<Locale> Locales { get; set; } = []; // o ülke için geçerli locale'ler
}

public class Locale
{
	public string Code { get; set; } // "fr-CH"
	public string DisplayName { get; set; } // "Français - Switzerland"
	public string DecimalSeparator { get; set; }
	public string ThousandSeparator { get; set; }
}
