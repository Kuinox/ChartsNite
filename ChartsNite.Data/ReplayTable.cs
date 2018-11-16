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
            TimeSpan duration, string codeName, Kill[] kills);
    }


    public struct Kill
    {
        public TimeSpan OccuredAt;
        public string KillerUserName;
        public string VictimUserName;
        public byte WeaponType;
        public bool KnockedDown;

        public Kill(TimeSpan occuredAt, string killerUserName, string victimUserName, byte weaponType, bool knockedDown)
        {
            OccuredAt = occuredAt;
            KillerUserName = killerUserName;
            VictimUserName = victimUserName;
            WeaponType = weaponType;
            KnockedDown = knockedDown;
        }
    }
}
