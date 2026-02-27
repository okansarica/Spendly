namespace Spendly.Shared.DataLayer;

using Spendly.Shared.Entities.Core;
using ViewModels.Settings;

public class ReportRepository<T>(ReportDbSettings settings) : Repository<T>(new DbSettings{ DatabaseName = settings.DatabaseName, UserName = settings.UserName, Password = settings.Password })
	where T : BaseReportEntity;
