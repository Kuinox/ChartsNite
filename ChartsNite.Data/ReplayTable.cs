using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace ChartsNite.Data
{
    [SqlTable("tReplay", Package = typeof(Package))]
    [Versions("1.0.0")]
    [SqlObjectItem("transform:CK.sUserDestroy")]
    [SqlObjectItem("sReplayCreate")]
    public abstract class ReplayTable : SqlTable
    {
        void StObjConstruct(CK.DB.Actor.UserTable userTable)
        {

        }

        public async Task<int> CreateAsync(ISqlCallContext ctx, int actorId, int ownerId, DateTime replayDate,
            TimeSpan duration, string codeName, int version, Kill[] kills)
        {
            using (SqlCommand sqlCommand = new SqlCommand("ChartsNite.sReplayCreate"))
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add("@ActorId", SqlDbType.Int).Value = actorId;
                sqlCommand.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
                sqlCommand.Parameters.Add("@ReplayDate", SqlDbType.DateTime2).Value = replayDate;
                sqlCommand.Parameters.Add("@Duration", SqlDbType.Time).Value = duration;
                sqlCommand.Parameters.Add("@CodeName", SqlDbType.NVarChar).Value = codeName;
                sqlCommand.Parameters.Add("@FortniteVersion", SqlDbType.Int).Value = version;

                DataTable killTable = new DataTable();
                killTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = typeof(TimeSpan),
                        ColumnName = "OccuredAt",
                        ReadOnly = true
                    });
                killTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "KillerUserName",
                        ReadOnly = true
                    });
                killTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = typeof(string),
                        ColumnName = "VictimUserName",
                        ReadOnly = true
                    });
                killTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = typeof(byte),
                        ColumnName = "WeaponType",
                        ReadOnly = true
                    });
                killTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = typeof(bool),
                        ColumnName = "KnockedDown",
                        ReadOnly = true
                    });

                foreach (Kill kill in kills)
                {
                    var row = killTable.NewRow();
                    row["OccuredAt"] = kill.OccuredAt;
                    row["KillerUserName"] = kill.KillerUserName;
                    row["VictimUserName"] = kill.VictimUserName;
                    row["WeaponType"] = kill.WeaponType;
                    row["KnockedDown"] = kill.KnockedDown;
                    killTable.Rows.Add(row);
                }
                sqlCommand.Parameters.Add("@Kills", SqlDbType.Structured).Value = killTable;
                    sqlCommand.Parameters.Add("@Output", SqlDbType.Int).Direction = ParameterDirection.Output;
                sqlCommand.Prepare();
                int id = await ctx[Database].ExecuteNonQueryAsync(sqlCommand);
                return (int)sqlCommand.Parameters["@Output"].Value;
            }
        }
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
