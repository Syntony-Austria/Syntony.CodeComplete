using System.Globalization;

namespace Library.Lending;

public static class FeeImport
{
    // The file from the town hall says: 1.5 means one and a half euros.
    // Deliberately wrong: this reads the text with the culture of the machine.
    public static decimal ReadTangled(string text) => decimal.Parse(text);

    public static decimal Read(string text) =>
        decimal.Parse(text, NumberStyles.Number, CultureInfo.InvariantCulture);
}
