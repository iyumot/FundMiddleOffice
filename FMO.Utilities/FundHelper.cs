using FMO.Models;

namespace FMO.Utilities;

public static class FundHelper
{

    private static Dictionary<int, string> FundStorageMap = new();

    public static DirectoryInfo Folder(this Fund fund)
    {
        return new DirectoryInfo(FundStorageMap[fund._id]);
    }





    public static void Map(Fund fund, string folder)
    {
        FundStorageMap[fund._id] = folder;
    }







}
