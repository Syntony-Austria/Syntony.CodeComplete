using Library.Fees;
using NetArchTest.Rules;
using NUnit.Framework;

namespace Library.ArchitectureTests;

[TestFixture]
public sealed class ArchitectureTests
{
    private static readonly System.Reflection.Assembly s_broken = typeof(Broken.Domain.NotSealedRule).Assembly;

    [Test]
    public void TheDomainDoesNotKnowTheStorage()
    {
        Assert.That(ArchitectureRules.DomainStaysFreeOfStorage(ArchitectureRules.Domain).IsSuccessful, Is.True);
    }

    [Test]
    public void TheRuleFailsOnPurposeAndNamesTheType()
    {
        TestResult result = ArchitectureRules.DomainStaysFreeOfStorage(s_broken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.FailingTypeNames, Does.Contain("Broken.Domain.NotSealedRule"));
        });
    }

    [Test]
    public void ItSeesTheMethodBodyButNotTheStringAndNotTheConstant()
    {
        IEnumerable<string> failing = ArchitectureRules.DomainStaysFreeOfStorage(s_broken).FailingTypeNames;

        Assert.Multiple(() =>
        {
            Assert.That(failing, Does.Contain("Broken.Domain.NotSealedRule"));
            Assert.That(failing, Does.Not.Contain("Broken.Domain.SneaksInByName"));
            Assert.That(failing, Does.Not.Contain("Broken.Domain.UsesAConstant"));
        });
    }

    [Test]
    public void TheSealedRuleFindsTheOpenClass()
    {
        TestResult result = ArchitectureRules.ClassesAreSealed(s_broken);

        Assert.That(result.FailingTypeNames, Is.EqualTo(new[] { "Broken.Domain.NotSealedRule" }));
    }

    [Test]
    public void AKnownExceptionIsListedAndTheListCanOnlyShrink()
    {
        string[] allowed = ["Broken.Domain.NotSealedRule"];
        string[] failing = [.. ArchitectureRules.ClassesAreSealed(s_broken).FailingTypeNames];

        Assert.Multiple(() =>
        {
            Assert.That(failing.Except(allowed), Is.Empty, "new violation");
            Assert.That(allowed.Except(failing), Is.Empty, "fixed: remove it from the list");
        });
    }

    [Test]
    public void TheRingBetweenNamespacesIsFound()
    {
        Assert.Multiple(() =>
        {
            Assert.That(NamespaceCycles.Find(typeof(FeeRule).Assembly), Is.EqualTo(new[] { ("Library.Fees", "Library.Members") }));
            Assert.That(NamespaceCycles.Find(ArchitectureRules.Domain), Is.Empty);
        });
    }
}
