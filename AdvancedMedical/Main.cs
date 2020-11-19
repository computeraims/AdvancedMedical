using AdvancedMedical.Utils;
using SDG.Framework.Modules;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AdvancedMedical
{
    public class Main : MonoBehaviour, IModuleNexus
    {
        private static GameObject AdvancedMedicalObject;

        public static Main Instance;

        public static Config Config;

        public void initialize()
        {
            Instance = this;

            Patcher patch = new Patcher();
            Patcher.DoPatching();

            UnityThread.initUnityThread();

            string callingPath = $"{Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)}";
            DirectoryInfo configDirectory = Directory.CreateDirectory($"{callingPath}/config");
            ConfigHelper.EnsureConfig($"{callingPath}/config/advancedmedical.json");
            Config = ConfigHelper.ReadConfig($"{callingPath}/config/advancedmedical.json");

            AdvancedMedicalObject = new GameObject("AdvancedMedical");
            DontDestroyOnLoad(AdvancedMedicalObject);

            AdvancedMedicalObject.AddComponent<MedicalManager>();

            Console.WriteLine("AdvancedMedical by Corbyn loaded");
        }

        public void shutdown()
        {
            Instance = null;
        }
    }
}
