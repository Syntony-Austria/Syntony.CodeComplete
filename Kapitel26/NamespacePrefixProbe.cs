// A false ring: NetArchTest compares namespaces as the start of a name.
// Library.PrefixProbe uses Library, and its two classes know each other.
// NamespacesAreComparedAsPrefixes: Find reports (Library, Library.PrefixProbe).
namespace Library
{
    public static class PrefixBase
    {
        public static int One => 1;
    }
}

namespace Library.PrefixProbe
{
    public static class First
    {
        public static int Value() => Second.Value() + Library.PrefixBase.One;
    }

    public static class Second
    {
        public static int Value() => 1;
    }
}
