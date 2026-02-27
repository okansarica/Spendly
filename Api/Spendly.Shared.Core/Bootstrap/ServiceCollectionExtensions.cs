namespace Spendly.Shared.Core.Bootstrap;

using DataLayer;
using Entities.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using Spendly.Shared.ViewModels.Settings;
using System;
using System.Linq;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    private static bool _mongoConventionsRegistered;

    public static IServiceCollection AddMongoRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterMongoConventions();

        var dbSettings = services.BuildServiceProvider().GetRequiredService<DbSettings>();
        services.AddSingleton(dbSettings);

        RegisterRepositoriesForBaseEntity(services, dbSettings);

        return services;
    }

    public static IServiceCollection AddReportRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterMongoConventions();

        var reportDbSettings = services.BuildServiceProvider().GetRequiredService<ReportDbSettings>();
        services.AddSingleton(reportDbSettings);

        RegisterRepositoriesForReportEntity(services, reportDbSettings);

        return services;
    }

    private static void RegisterMongoConventions()
    {
        if (_mongoConventionsRegistered)
            return;

        var pack = new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String)
        };

        ConventionRegistry.Register("EnumStringConvention", pack, _ => true);
        _mongoConventionsRegistered = true;
    }

    private static void RegisterRepositoriesForBaseEntity(IServiceCollection services, DbSettings dbSettings)
    {
        var assembly = Assembly.GetAssembly(typeof(BaseEntity)) 
                       ?? throw new InvalidOperationException($"Assembly not found for {nameof(BaseEntity)}");

        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseEntity)) && !t.IsAbstract);

        foreach (var type in entityTypes)
        {
            var repoType = typeof(Repository<>).MakeGenericType(type);
            var interfaceType = typeof(IRepository<>).MakeGenericType(type);

            services.AddSingleton(interfaceType, _ => Activator.CreateInstance(repoType, dbSettings)!);
            services.AddSingleton(repoType, _ => Activator.CreateInstance(repoType, dbSettings)!);
        }
    }

    private static void RegisterRepositoriesForReportEntity(IServiceCollection services, ReportDbSettings reportDbSettings)
    {
        var assembly = Assembly.GetAssembly(typeof(BaseReportEntity)) 
                       ?? throw new InvalidOperationException($"Assembly not found for {nameof(BaseReportEntity)}");

        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseReportEntity)) && !t.IsAbstract);

        foreach (var type in entityTypes)
        {
            var repoType = typeof(ReportRepository<>).MakeGenericType(type);
            var interfaceType = typeof(IRepository<>).MakeGenericType(type);

            services.AddSingleton(interfaceType, _ => Activator.CreateInstance(repoType, reportDbSettings)!);
            services.AddSingleton(repoType, _ => Activator.CreateInstance(repoType, reportDbSettings)!);
        }
    }
    
    public static IServiceCollection AddLocalQueueRepositories(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        RegisterMongoConventions();

        var localQueueDbSettings = services.BuildServiceProvider().GetRequiredService<LocalQueueDbSettings>();
        services.AddSingleton(localQueueDbSettings);

        RegisterRepositoriesForLocalQueueEntity(services, localQueueDbSettings);

        return services;
    }

    private static void RegisterRepositoriesForLocalQueueEntity(
        IServiceCollection services, 
        LocalQueueDbSettings localQueueDbSettings)
    {
        var assembly = Assembly.GetAssembly(typeof(BaseLocalQueueEntity))
                       ?? throw new InvalidOperationException($"Assembly not found for {nameof(BaseLocalQueueEntity)}");

        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseLocalQueueEntity)) && !t.IsAbstract);

        foreach (var type in entityTypes)
        {
            var repoType = typeof(LocalQueueRepository<>).MakeGenericType(type);
            var interfaceType = typeof(IRepository<>).MakeGenericType(type);

            services.AddSingleton(interfaceType, _ => Activator.CreateInstance(repoType, localQueueDbSettings)!);
            services.AddSingleton(repoType, _ => Activator.CreateInstance(repoType, localQueueDbSettings)!);
        }
    }

}
