namespace Library.Lending;

public sealed class MailOptions
{
    public string[] Hosts { get; set; } = [];

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public decimal Factor { get; set; } = 1m;
}
