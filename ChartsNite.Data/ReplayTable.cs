using System;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace ChartsNite.Data
{
    [SqlTable("tReplay", Package = typeof(Package))]
    [Versions("1.0.0")]
    [SqlObjectItem("transform:CK.sUserDestroy")]
    public abstract class ReplayTable : SqlTable
    {
        void StObjConstruct(CK.DB.Actor.UserTable userTable)
        {

        }

        [SqlProcedure("sReplayCreate")]
        public abstract int Create(ISqlCallContext ctx, int actorId, int ownerId, DateTime replayDate,
            TimeSpan duration, string codeName);
    }
}
