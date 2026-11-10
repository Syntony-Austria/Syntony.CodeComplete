using NUnit.Framework;

namespace Library.Lending;

[TestFixture]
public sealed class LoanRulesTests
{
    [TestCase(3, ExpectedResult = 0)]
    [TestCase(4, ExpectedResult = 100)]
    public int FeeStartsOnTheFourthDay(int daysLate) => LoanRules.FeeInCents(daysLate);
}
