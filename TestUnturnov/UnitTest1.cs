using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Controlers;

namespace SpeedMann.Unturnov
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            HideoutControler.freeHideout(player);
            HideoutControler.claimHideout(player);
            Hideout hideout = HideoutControler.getHideout(player.CSteamID);
        }
    }
}