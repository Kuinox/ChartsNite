using CK.Setup;
using CK.SqlServer.Setup;

namespace ChartsNite.Data
{
    [SqlTable("tEvent", Package = typeof(Package))]
    [Versions("1.0.0")]
    public abstract class EventTable : SqlTable
    {
        void StObjConstruct(ReplayTable replayTable)
        {
            
        }
    }
}
