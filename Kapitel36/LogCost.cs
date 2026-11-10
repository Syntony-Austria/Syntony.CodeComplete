namespace Library.Lending;

public sealed class QuietLog
{
    public bool DebugEnabled => false;

    // Switched off: and yet the caller has already built the text.
    public void Debug(string text)
    {
        if (DebugEnabled)
        {
            Console.WriteLine(text);
        }
    }
}
