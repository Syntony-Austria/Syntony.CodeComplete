namespace Library.Lending;

public static class RenewalDeep
{
    public static string Decide(RenewalInput input)
    {
        string answer;
        if (input.RenewalsSoFar < 2)
        {
            if (input.DaysOverdue <= 14)
            {
                if (!input.HasOpenFee)
                {
                    if (!input.SomeoneWaiting || input.IsPrioritised)
                    {
                        answer = "renewed";
                    }
                    else
                    {
                        answer = "reserved";
                    }
                }
                else
                {
                    answer = "open fee";
                }
            }
            else
            {
                answer = "too late";
            }
        }
        else
        {
            answer = "too many";
        }

        return answer;
    }
}
