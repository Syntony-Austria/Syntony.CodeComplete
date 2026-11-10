using System.Security.Cryptography;

namespace Library.Lending;

// A new project says nothing about MD5.
// dotnet build -p:CryptoProbe=true                          -> green
// dotnet build -p:CryptoProbe=true -p:AnalysisMode=Minimum  -> CA1850 only
// dotnet build -p:CryptoProbe=true -p:AnalysisMode=Recommended
//                                                   -> CA1850 and CA5351
public static class CryptoProbe
{
    public static byte[] Hash(byte[] data)
    {
        using MD5 md5 = MD5.Create();
        return md5.ComputeHash(data);
    }
}
