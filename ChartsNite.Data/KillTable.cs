using CK.Setup;
using CK.SqlServer.Setup;

namespace ChartsNite.Data
{
    [SqlTable("tKill", Package = typeof(Package))]
    [Versions("1.0.0")]
    [SqlObjectItem("transform:CK.sUserDestroy")]
    public abstract class KillTable : SqlTable
    {
        void StObjConstruct(EventTable eventTable, CK.DB.Actor.UserTable userTable)
        {

        }
    }
}
