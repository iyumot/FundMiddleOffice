using FMO.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Utilities;

public static class DatabaseAssist
{
    /// <summary>
    /// 自检
    /// </summary>
    public static void SystemValidation()
    {
        using (var db = new BaseDatabase())
        {
            db.GetCollection<ICustomer>().EnsureIndex(x => x.Identity);
            
        }
    }
}



public class BaseDatabase : LiteDatabase
{

    private const string connectionString = @"FileName=data\base.db;Password=891uiu89f41uf9dij432u89;Connection=Shared";

    public BaseDatabase() : base(connectionString, null)
    {
    }
}


public class TrusteeDatabase : LiteDatabase
{

    private const string connectionString = @"FileName=data\trustee.db;Password=f34902ufdisuf8s1;Connection=Shared";

    public TrusteeDatabase() : base(connectionString, null)
    { 
    }
}