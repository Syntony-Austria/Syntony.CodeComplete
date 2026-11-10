namespace Library.Lending;

// Three methods, nine cases each. CA1502 at threshold 5 (CodeMetricsConfig.txt):
// dotnet build -p:MetricsProbe=true -> CA1502 "WithStatement" 10,
// "WithIfChain" 9, nothing for "WithExpression".
public static class MetricsProbe
{
    public static string WithExpression(int code) => code switch
    {
        0 => "reserved",
        1 => "on loan",
        2 => "overdue",
        3 => "returned",
        4 => "lost",
        5 => "damaged",
        6 => "in repair",
        7 => "withdrawn",
        _ => "unknown",
    };

    public static string WithStatement(int code)
    {
        switch (code)
        {
            case 0: return "reserved";
            case 1: return "on loan";
            case 2: return "overdue";
            case 3: return "returned";
            case 4: return "lost";
            case 5: return "damaged";
            case 6: return "in repair";
            case 7: return "withdrawn";
            default: return "unknown";
        }
    }

    public static string WithIfChain(int code)
    {
        if (code == 0)
        {
            return "reserved";
        }

        if (code == 1)
        {
            return "on loan";
        }

        if (code == 2)
        {
            return "overdue";
        }

        if (code == 3)
        {
            return "returned";
        }

        if (code == 4)
        {
            return "lost";
        }

        if (code == 5)
        {
            return "damaged";
        }

        if (code == 6)
        {
            return "in repair";
        }

        if (code == 7)
        {
            return "withdrawn";
        }

        return "unknown";
    }
}
