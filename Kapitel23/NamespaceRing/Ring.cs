namespace Library.Fees
{
    public static class FeeRule
    {
        public static int Days(string id) =>
            Library.Members.MemberRule.Grace(id) + 1;
    }
}

namespace Library.Members
{
    // A ring between two namespaces builds without a word.
    public static class MemberRule
    {
        public static int Grace(string id) => id.Length;

        public static int Twice(string id) => Library.Fees.FeeRule.Days(id) * 2;
    }
}
