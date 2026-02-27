namespace Spendly.Shared.Core.MongoSerialization;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public static class MongoSerializationConfigurator
{
    /// <summary>
    /// Apply default serializers we want across all apps:
    /// - Guid as Standard representation
    /// - Enums serialized as strings
    /// </summary>
    public static void ApplyDefaultSerializers()
    {
        RegisterGuidAsStandard();
        RegisterEnumSerializersAsString();
    }

    /// <summary>
    /// Ensures Guid values are stored with Standard representation.
    /// Safe to call multiple times.
    /// </summary>
    public static void RegisterGuidAsStandard()
    {
        try
        {
            // Register Guid serializer with Standard representation only if not already registered as such.
            var currentGuidSerializer = BsonSerializer.LookupSerializer(typeof(Guid));
            if (currentGuidSerializer is not GuidSerializer guidSerializer || guidSerializer.GuidRepresentation != GuidRepresentation.Standard)
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
        }
        catch
        {
            // avoid throwing on startup; consumer apps can choose to log if needed
        }
    }

    /// <summary>
    /// Registers EnumSerializer for all non-open generic enums in loaded assemblies to serialize as strings.
    /// Idempotent and resilient to failures in specific assemblies/types.
    /// </summary>
    public static void RegisterEnumSerializersAsString()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .ToArray();

        foreach (var assembly in assemblies)
        {
            IEnumerable<Type> enumTypes;
            try
            {
                enumTypes = assembly.GetTypes().Where(t => t.IsEnum);
            }
            catch
            {
                // Skip problematic assemblies
                continue;
            }

            foreach (var enumType in enumTypes)
            {
                if (enumType.ContainsGenericParameters)
                {
                    continue;
                }

                try
                {
                    var existingSerializer = BsonSerializer.LookupSerializer(enumType);
                    var existingType = existingSerializer.GetType();
                    if (existingType.IsGenericType && existingType.GetGenericTypeDefinition() == typeof(EnumSerializer<>))
                    {
                        // already configured
                        continue;
                    }

                    var serializerType = typeof(EnumSerializer<>).MakeGenericType(enumType);
                    var serializer = (IBsonSerializer)Activator.CreateInstance(serializerType, BsonType.String)!;
                    BsonSerializer.RegisterSerializer(enumType, serializer);
                }
                catch
                {
                    // ignore and continue with other enums
                }
            }
        }
    }
}

