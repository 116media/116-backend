using System.Reflection;
using _116.Architecture.Tests.Common;
using AwesomeAssertions;
using Xunit;
using Module = _116.Architecture.Tests.Common.Module;

namespace _116.Architecture.Tests;

/// <summary>
/// The service taxonomy: an application service is implemented in Application, an infrastructure
/// service is implemented in Infrastructure, and the <c>Ports</c> folder is the only place the two
/// meet. The folder answers "who implements this" on sight, so these rules keep it truthful.
/// </summary>
public class ServiceTaxonomyTests
{
    private const string PortsSegment = ".Ports";

    [Fact]
    public void NoFactoryInterface_IsImplementedInApplication()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            Assembly application = Assembly.Load($"{module.Name}.Application");

            IEnumerable<string> offenders = ConcreteClasses(application)
                .Where(type => type.GetInterfaces().Any(contract => contract.Name.EndsWith("Factory")))
                .Select(type => type.FullName!);

            offenders.ShouldHold($"{module.Name}: a class that injects collaborators is a service, not a factory");
        }
    }

    [Fact]
    public void Ports_AreImplementedOnlyInInfrastructure()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            Assembly application = Assembly.Load($"{module.Name}.Application");
            Assembly infrastructure = Assembly.Load($"{module.Name}.Infrastructure");
            Type[] ports = [.. application.GetTypes().Where(type => type.IsInterface && IsPort(type))];

            IEnumerable<string> implementedInApplication = ConcreteClasses(application)
                .Where(type => type.GetInterfaces().Any(ports.Contains))
                .Select(type => type.FullName!);

            IEnumerable<string> unimplemented = ports
                .Where(port => !ConcreteClasses(infrastructure).Any(type => port.IsAssignableFrom(type)))
                .Select(port => port.FullName!);

            implementedInApplication.ShouldHold($"{module.Name}: a port is implemented in Infrastructure");
            unimplemented.ShouldHold($"{module.Name}: a port with no Infrastructure implementation is not a port");
        }
    }

    [Fact]
    public void InfrastructureServices_ImplementOnlyPorts()
    {
        foreach (Module module in ArchitectureRule.Modules)
        {
            Assembly infrastructure = Assembly.Load($"{module.Name}.Infrastructure");
            string servicesNamespace = $"{module.Root}.Infrastructure.Services";
            string applicationNamespace = $"{module.Root}.Application.";

            IEnumerable<string> offenders = ConcreteClasses(infrastructure)
                .Where(type => type.Namespace?.StartsWith(servicesNamespace) == true)
                .Where(type =>
                    type.GetInterfaces()
                        .Any(contract =>
                            contract.Namespace?.StartsWith(applicationNamespace) == true && !IsPort(contract)
                        )
                )
                .Select(type => type.FullName!);

            offenders.ShouldHold(
                $"{module.Name}: an Infrastructure service implements a Ports interface; anything else is an application service in the wrong layer"
            );
        }
    }

    private static bool IsPort(Type type)
    {
        return type.Namespace?.EndsWith(PortsSegment) == true;
    }

    private static IEnumerable<Type> ConcreteClasses(Assembly assembly)
    {
        return assembly.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false });
    }
}
