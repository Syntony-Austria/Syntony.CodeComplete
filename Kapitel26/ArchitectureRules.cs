using System.Reflection;
using Library.Domain;
using NetArchTest.Rules;

namespace Library.ArchitectureTests;

public static class ArchitectureRules
{
    public static readonly Assembly Domain = typeof(ILoanStore).Assembly;

    public static TestResult DomainStaysFreeOfStorage(Assembly assembly) =>
        Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("Library.Storage")
            .GetResult();

    public static TestResult ClassesAreSealed(Assembly assembly) =>
        Types.InAssembly(assembly)
            .That().AreClasses().And().AreNotAbstract()
            .Should().BeSealed()
            .GetResult();
}
