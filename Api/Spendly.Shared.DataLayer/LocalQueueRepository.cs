namespace Spendly.Shared.DataLayer;

using Spendly.Shared.Entities.Core;
using Spendly.Shared.ViewModels.Settings;

public class LocalQueueRepository<T>(LocalQueueDbSettings settings) : Repository<T>(new DbSettings{ DatabaseName = settings.DatabaseName, UserName = settings.UserName, Password = settings.Password })
	where T : BaseLocalQueueEntity;

