namespace Spendly.Shared.ViewModels.Settings;

public class DbSettings
{
	public string DatabaseName { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
}

public class LogDbSettings
{
	public string ConnectionString { get; set; }
	public string DatabaseName { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
}

public class ReportDbSettings
{
	public string DatabaseName { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
}


public class LocalQueueDbSettings
{
	public string DatabaseName { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
}
