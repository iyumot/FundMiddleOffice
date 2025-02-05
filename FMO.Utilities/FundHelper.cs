using FMO.Models;

namespace FMO.Utilities;

public static class FundHelper
{

    private static Dictionary<int, string> FundStorageMap = new();

    public static DirectoryInfo Folder(this Fund fund)
    {
        return new DirectoryInfo(FundStorageMap[fund.Id]);
    }

    public static DirectoryInfo GetFolder(int fundId)
    {
        return new DirectoryInfo(FundStorageMap[fundId]);
    }

    public static string GetFolder(int fundId, string sub)
    {
        return Path.Combine(FundStorageMap[fundId], sub);
    }


    public static void Map(Fund fund, string folder)
    {
        FundStorageMap[fund.Id] = folder;
    }







}
