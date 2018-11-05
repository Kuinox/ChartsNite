using CK.Core;
using CK.DB.Auth;
using CK.DB.User.UserPassword;
using CK.SqlServer;
using NUnit.Framework;
using static CK.Testing.DBSetupTestHelper;

namespace ChartsNite.Data.Tests
{
    [TestFixture]
    public class DBSetup : CK.DB.Tests.DBSetup
    {
        [Test]
        [Explicit]
        public void Add_Admin()
        {
            var u = TestHelper.StObjMap.StObjs.Obtain<UserPasswordTable>();
            using (var ctx = new SqlStandardCallContext(TestHelper.Monitor))
            {
                var result = u.CreateOrUpdatePasswordUser(ctx, 1, 1, "a");
                Assert.That(result.OperationResult == UCResult.Created || result.OperationResult == UCResult.Updated);
            }
        }
    }
}
