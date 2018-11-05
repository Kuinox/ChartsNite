using CK.Setup;
using CK.SqlServer.Setup;

namespace ChartsNite.Data
{
    [SqlPackage(
         ResourcePath = "Res",
         Schema = "ChartsNite",
         Database = typeof(SqlDefaultDatabase),
         ResourceType = typeof(Package)),
     Versions("1.0.0")]
    public abstract class Package : SqlPackage
    {
        void StObjConstruct(
            CK.DB.Actor.Package userTable
        )
        {

        }
    }
}
