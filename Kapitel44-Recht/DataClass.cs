namespace Library.Lending;

// Ordered: a higher value is more sensitive. New values only at the end.
public enum DataClass
{
    Unspecified = 0,
    Public,
    Internal,
    Personal,
    Secret,
}
