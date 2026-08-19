using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Planning.Api.Swagger;

/// <summary>
/// Marks non-nullable properties as required, so the generated client types them without
/// <c>| undefined</c>.
/// <para>
/// <c>SupportNonNullableReferenceTypes()</c> only sets <c>nullable</c> on the schema; without this
/// filter every property still comes out optional, and a guaranteed <c>string Title</c> reaches
/// TypeScript as <c>string | undefined</c>. Bridging that gap by hand is exactly what the
/// frontend normalizers used to do.
/// </para>
/// <para>
/// Requiredness is decided from the CLR type rather than from the emitted schema, because a
/// nullable enum is emitted as a bare <c>$ref</c> that carries no nullability at all - reading the
/// schema would wrongly mark <c>Weekday?</c> as required.
/// </para>
/// </summary>
public sealed class NonNullableAsRequiredSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete || concrete.Properties is null || concrete.Properties.Count == 0)
        {
            return;
        }

        var required = RequiredMemberNames(context.Type);

        if (required.Count == 0)
        {
            return;
        }

        concrete.Required ??= new HashSet<string>();

        foreach (var name in concrete.Properties.Keys.Where(required.Contains))
        {
            concrete.Required.Add(name);
        }
    }

    private static HashSet<string> RequiredMemberNames(Type type)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
        {
            return names;
        }

        // A constructor parameter with a default value is genuinely optional: the server fills it
        // in when the caller omits it (for example PlanningStatus Status = Confirmed).
        var optional = OptionalParameterNames(type);
        var nullability = new NullabilityInfoContext();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (optional.Contains(property.Name) || IsNullable(property, nullability))
            {
                continue;
            }

            names.Add(property.Name);
        }

        return names;
    }

    private static bool IsNullable(PropertyInfo property, NullabilityInfoContext nullability) =>
        Nullable.GetUnderlyingType(property.PropertyType) is not null
        || nullability.Create(property).ReadState is NullabilityState.Nullable;

    private static HashSet<string> OptionalParameterNames(Type type)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(x => x.GetParameters().Length)
            .FirstOrDefault();

        foreach (var parameter in constructor?.GetParameters() ?? [])
        {
            if (parameter is { HasDefaultValue: true, Name: { } name })
            {
                names.Add(name);
            }
        }

        return names;
    }
}
