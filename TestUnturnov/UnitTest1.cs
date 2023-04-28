using SpeedMann.Unturnov.Helper;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Controlers;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

namespace SpeedMann.Unturnov
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            JsonManager.Init("Test", false);
            int objectCount = 1000;
            string SaveDirectory = @"C:\Users\micha\Desktop\UnturnedModding\RocketPlugins\UnturnovServer\TestResults\JsonTest\";


            var jsonObject = buildJsonString(objectCount);
            //var jsonObject = buildBarricadeWrapperList(objectCount);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Directory.CreateDirectory(SaveDirectory);
            JsonManager.tryWriteToDisc(@$"{SaveDirectory}test.json", jsonObject);
            stopwatch.Stop();

            Console.WriteLine("Elapsed time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        private List<BarricadeWrapper> buildBarricadeWrapperList(int objectCount)
        {
            var wrappers = new List<BarricadeWrapper>();
            var wrapper = new BarricadeWrapper(
                SDG.Unturned.EBuild.BARRICADE,
                328,
                new UnityEngine.Vector3(1.87890625f, 0.842285156f, 1.29101563f),
                new UnityEngine.Quaternion(0.7071068f, 0, 0, -0.7071068f),
                Convert.FromBase64String("PcRQAwEAEAFCsFECAABwAQIABABIAQFkEQAAAAAAAAAAAAAAAAAAAAAAAAAASAEBZBEAAAAAAAAAAAAAAAAAAAAAAA=="));
            for (int i = 0; i < objectCount; i++)
            {
                wrappers.Append(wrapper);
            }

            return wrappers;
        }
        private string buildJsonString(int objectCount)
        {
            string jsonObject = "{barricadeType:STORAGE,id:328,position:[1.87890625,0.842285156,1.29101563],rotation:[0.7071068,0.0,0.0,-0.7071068],state:PcRQAwEAEAFCsFECAABwAQIABABIAQFkEQAAAAAAAAAAAAAAAAAAAAAAAAAASAEBZBEAAAAAAAAAAAAAAAAAAAAAAA==}";
            StringBuilder jsonArrayBuilder = new StringBuilder();
            jsonArrayBuilder.Append("[");
            for (int i = 0; i < objectCount; i++)
            {
                jsonArrayBuilder.Append(jsonObject + ",");
            }
            jsonArrayBuilder.Append("]");

            return jsonArrayBuilder.ToString();
        }

    }
}