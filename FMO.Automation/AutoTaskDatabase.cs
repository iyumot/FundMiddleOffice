using LiteDB;

namespace FMO.Schedule;

public class AutoTaskDatabase : LiteDatabase
{

    public AutoTaskDatabase() : base(@"FileName=data\autotask.ldb;Connection=Shared")
    {
    }



}
