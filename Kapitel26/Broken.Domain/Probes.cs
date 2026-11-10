using Library.Storage;

namespace Broken.Domain;

// The positive control: these classes break the rules on purpose.
public class NotSealedRule
{
    public static string UsesStorageInBody()
    {
        FileLoanStore store = new("x");
        return store.GetType().Name;
    }
}

public sealed class SneaksInByName
{
    public static Type? Find() =>
        Type.GetType("Library.Storage.FileLoanStore, Library.Storage");
}

public sealed class UsesAConstant
{
    public static int Read() => StorageLimits.MaxLines;
}
