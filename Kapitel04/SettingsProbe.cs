namespace Library.Lending;

// Deliberately wrong. Built only with -p:SettingsProbe=true, so the settings bite.
public static class SettingsProbe
{
    public static int NameLength(MemberDirectory directory, string memberId) =>
        directory.FindName(memberId).Length;
}
