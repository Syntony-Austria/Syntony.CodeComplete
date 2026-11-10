using System.Runtime.CompilerServices;
using System.Text.Json;
using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class FileTests
{
    [Test]
    public void ACorrectFileLoads()
    {
        IReadOnlyList<FeeStep> steps = FeeStepsFile.Load(
            """[{"FromDay":0,"CentsPerDay":0},{"FromDay":4,"CentsPerDay":25}]""");

        Assert.That(steps[1], Is.EqualTo(new FeeStep(4, 25)));
    }

    [Test]
    public void TheDefaultReaderAcceptsATypoAndAMissingField()
    {
        List<FeeStep> typo = JsonSerializer.Deserialize<List<FeeStep>>(
            """[{"FromDays":4,"CentsPerDay":25}]""")!;
        List<FeeStep> missing = JsonSerializer.Deserialize<List<FeeStep>>(
            """[{"CentsPerDay":25}]""")!;

        Assert.Multiple(() =>
        {
            Assert.That(typo[0].FromDay, Is.Zero);
            Assert.That(missing[0].FromDay, Is.Zero);
        });
    }

    [Test]
    public void TheStrictReaderRefusesBoth()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<JsonException>(() => FeeStepsFile.Load(
                """[{"FromDays":4,"CentsPerDay":25}]"""));
            Assert.Throws<JsonException>(() => FeeStepsFile.Load(
                """[{"CentsPerDay":25}]"""));
        });
    }

    [Test]
    public void StepsOutOfOrderAndAnEmptyFileAreRefused()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidDataException>(() => FeeStepsFile.Load(
                """[{"FromDay":15,"CentsPerDay":50},{"FromDay":4,"CentsPerDay":25}]"""));
            Assert.Throws<InvalidDataException>(() => FeeStepsFile.Load("[]"));
        });
    }

    [Test]
    public void TheTariffDependsOnTheDay()
    {
        Assert.Multiple(() =>
        {
            Assert.That(FeeSchedule.CentsPerDayOn(new DateOnly(2026, 3, 31)), Is.EqualTo(20));
            Assert.That(FeeSchedule.CentsPerDayOn(new DateOnly(2026, 4, 1)), Is.EqualTo(25));
        });
    }

    [Test]
    public void EveryTableCanBeTouchedOnPurpose()
    {
        Type[] tables = [typeof(BookRates), typeof(FeeSteps), typeof(RenewalTable), typeof(ReminderTexts)];

        foreach (Type table in tables)
        {
            RuntimeHelpers.RunClassConstructor(table.TypeHandle);
        }

        Assert.Throws<TypeInitializationException>(
            () => RuntimeHelpers.RunClassConstructor(typeof(BrokenTable).TypeHandle));
    }

    private static class BrokenTable
    {
        public static readonly Dictionary<string, int> Rows = new()
        {
            { "a", 1 },
            { "a", 2 },
        };
    }
}
