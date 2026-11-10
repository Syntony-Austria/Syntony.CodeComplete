namespace Library.Lending
{
    // Deliberately wrong. Built only with -p:StyleProbe=true: the rules must bite.
    public sealed class StyleProbe
    {
        private int count;

        public int Next()
        {
            var next = count + 1;
              count = next;
            return next;
        }
    }
}
