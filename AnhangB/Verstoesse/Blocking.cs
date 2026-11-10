namespace Library;

public static class Probe
{
    public static int Run(Task<int> work) => work.Result;
}
