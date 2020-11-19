using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace AdvancedMedical.Utils
{
    public class Config
    {
        public int downedHealth { get; set; }

        public int downedProtectionTime { get; set; }

        public int recentlyDownedTime { get; set; }

        public int revivedHealth { get; set; }

        public bool explosionInstaKill { get; set; }

        public int bleedOutTime { get; set; }

        public float downedMovementMultiplier { get; set; }

        public float survivalChance { get; set; }
    }

    public class ConfigHelper
    {
        public static void EnsureConfig(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("No config.json");

                JObject advancedMedicalConfig = new JObject();

                advancedMedicalConfig.Add("downedHealth", 10);

                advancedMedicalConfig.Add("downedProtectionTime", 2);

                advancedMedicalConfig.Add("recentlyDownedTime", 300);

                advancedMedicalConfig.Add("revivedHealth", 10);

                advancedMedicalConfig.Add("explosionInstaKill", true);

                advancedMedicalConfig.Add("bleedOutTime", 10);

                advancedMedicalConfig.Add("downedMovementMultiplier", 0);

                advancedMedicalConfig.Add("survivalChance", 0.5);

                using (StreamWriter file = File.CreateText(path))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    advancedMedicalConfig.WriteTo(writer);

                    Console.WriteLine("Generated AdvancedMedical config");
                }
            }
        }

        public static Config ReadConfig(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                return JsonConvert.DeserializeObject<Config>(JToken.ReadFrom(reader).ToString());
            }
        }
    }
}
