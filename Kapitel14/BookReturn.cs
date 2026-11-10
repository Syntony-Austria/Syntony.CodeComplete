namespace Library.Lending;

public enum MemberKind
{
    Adult,
    Child,
    Staff,
}

public sealed record BookReturn(
    string MemberId,
    string Title,
    DateOnly DueDate,
    MemberKind Member,
    bool IsReference,
    decimal BookPrice);
