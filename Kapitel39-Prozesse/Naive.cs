namespace Library.Lending;

public interface ICatalog
{
    Task<string?> FindTitleAsync(string isbn, CancellationToken cancellationToken);
}

public interface IPartnerLibrary
{
    // orderKey: the same key means the same order, however often it arrives.
    Task<string> OrderAsync(
        string isbn, string memberId, string? orderKey,
        CancellationToken cancellationToken);
}

public sealed class NaiveLending(ICatalog catalog, IPartnerLibrary partner)
{
    // Deliberately wrong: no time limit. If the catalog never answers,
    // neither does this method.
    public Task<string?> TitleAsync(string isbn) =>
        catalog.FindTitleAsync(isbn, CancellationToken.None);

    // Deliberately wrong: every failure is retried, even a lost answer.
    public async Task<string> OrderAsync(string isbn, string memberId)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await partner.OrderAsync(
                    isbn, memberId, null, CancellationToken.None);
            }
            catch (Exception) when (attempt < 3)
            {
                // try again at once
            }
        }
    }
}
