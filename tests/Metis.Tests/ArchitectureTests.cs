using System.Reflection;
using System.Runtime.CompilerServices;
using Metis.Intake;
using Metis.Triage;

namespace Metis.Tests;

/// <summary>Boundary rules: modules talk through public contracts; vertical slices do not reach into each other.</summary>
public class ArchitectureTests
{
    private static readonly Assembly Intake = typeof(IntakeModule).Assembly;
    private static readonly Assembly Triage = typeof(TriageModule).Assembly;
    private static readonly Assembly[] Modules = [Intake, Triage];

    [Fact]
    public void Intake_does_not_depend_on_Triage()
    {
        Assert.DoesNotContain(Intake.GetReferencedAssemblies(), a => a.Name == Triage.GetName().Name);
    }

    [Fact]
    public void Modules_only_open_their_internals_to_the_test_project()
    {
        foreach (var module in Modules)
        {
            var friends = module.GetCustomAttributes<InternalsVisibleToAttribute>().Select(a => a.AssemblyName).ToList();
            Assert.All(friends, f => Assert.Equal("Metis.Tests", f));
        }
    }

    [Fact]
    public void Triage_uses_only_public_types_from_the_Intake_contracts_namespace()
    {
        var used = Triage.GetTypes().SelectMany(ReferencedTypes).Where(t => t.Assembly == Intake).Distinct().ToList();

        Assert.NotEmpty(used);
        Assert.All(used, t =>
        {
            Assert.True(t.IsVisible, $"{t.FullName} is internal to Intake");
            Assert.StartsWith("Metis.Intake.Contracts", t.Namespace ?? string.Empty);
        });
    }

    [Fact]
    public void A_feature_slice_never_references_another_feature_slice()
    {
        foreach (var module in Modules)
        {
            foreach (var type in module.GetTypes().Where(t => Feature(t) is not null))
            {
                var own = Feature(type);
                var foreign = ReferencedTypes(type).Where(t => Feature(t) is { } f && f != own).Select(t => t.FullName).ToList();
                Assert.True(foreign.Count == 0, $"{type.FullName} references another slice: {string.Join(", ", foreign)}");
            }
        }
    }

    [Fact]
    public void Every_feature_slice_has_request_handler_validator_and_endpoint_together()
    {
        var features = Modules.SelectMany(m => m.GetTypes()).Where(t => Feature(t) is not null).GroupBy(Feature).ToList();

        Assert.True(features.Count >= 4);
        foreach (var feature in features)
        {
            var names = feature.Select(t => t.Name).ToList();
            foreach (var suffix in new[] { "Request", "Handler", "Validator", "Endpoint" })
            {
                Assert.True(names.Any(n => n.EndsWith(suffix, StringComparison.Ordinal)), $"{feature.Key} has no *{suffix}");
            }
        }
    }

    private static string? Feature(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        var at = ns.IndexOf(".Features.", StringComparison.Ordinal);
        return at < 0 ? null : ns;
    }

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var direct = new List<Type?> { type.BaseType };
        direct.AddRange(type.GetInterfaces());
        direct.AddRange(type.GetFields(all).Select(f => f.FieldType));
        direct.AddRange(type.GetProperties(all).Select(p => p.PropertyType));
        foreach (var method in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
        {
            direct.AddRange(method.GetParameters().Select(p => p.ParameterType));
            if (method is MethodInfo mi) direct.Add(mi.ReturnType);
        }

        return direct.Where(t => t is not null).SelectMany(t => Expand(t!));
    }

    private static IEnumerable<Type> Expand(Type type)
    {
        if (type.HasElementType) return Expand(type.GetElementType()!);
        if (!type.IsGenericType) return [type];
        return new[] { type.GetGenericTypeDefinition() }.Concat(type.GetGenericArguments().SelectMany(Expand));
    }
}
