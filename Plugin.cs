using System;
using HarmonyLib;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.IO;
using BepInEx.Logging;
using Nautilus.Handlers;
using System.Security.Cryptography;

namespace ResourceMonitor
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    internal class Plugin : BaseUnityPlugin
    {
        public const string BepInExDir = "./BepInEx/plugins/ResourceMonitor/";
        public const string QModsDir = "./QMods/ResourceMonitor/";

        public static string LARGE_PNG_LOCATION;
        public static string SMALL_PNG_LOCATION;
        public static string ASSET_BUNDLE_LOCATION;
        public static string DONT_TRACK_LOCATION;

        public static GameObject RESOURCE_MONITOR_DISPLAY_UI_PREFAB { get; private set; }
        public static GameObject RESOURCE_MONITOR_DISPLAY_ITEM_UI_PREFAB { get; private set; }
        public static GameObject RESOURCE_MONITOR_DISPLAY_MODEL { get; private set; }

        public new static ManualLogSource Logger { get; private set; }
        public static Options Options { get; private set; }

        #region[Declarations]
        private const string
            MODNAME = "ResourceMonitor",
            AUTHOR = "0x4b",
            GUID = "katemods.resourcemonitor.unofficial",
            VERSION = "2.0.38";
        #endregion

        public void Awake()
        {
            // Setup Project Logger
            Logger = base.Logger;

            // Load External Files
            if (!LoadAssets())
            {
                Logger.LogError("ResourceMonitor failed to load!");
                return;
            }

            // Load Tracking list
            LoadDontTrackList();

            // Load Options from the BepInEx config and setup the options menu
            Options = OptionsPanelHandler.RegisterModOptions<Options>();

            // Run harmony patches
            var harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Register Game Prefabs
            RegisterPrefabs();

            Logger.LogInfo("ResourceMonitor Loading Complete!");
        }

        private static bool LoadAssets()
        {
            // Determine the folder paths for each file separately
            LARGE_PNG_LOCATION = File.Exists(BepInExDir + "Assets/ResourceMonitorLarge.png") ? BepInExDir + "Assets/ResourceMonitorLarge.png" : QModsDir + "Assets/ResourceMonitorLarge.png";
            SMALL_PNG_LOCATION = File.Exists(BepInExDir + "Assets/ResourceMonitorSmall.png") ? BepInExDir + "Assets/ResourceMonitorSmall.png" : QModsDir + "Assets/ResourceMonitorSmall.png";
            ASSET_BUNDLE_LOCATION = File.Exists(BepInExDir + "Assets/resources") ? BepInExDir + "Assets/resources" : QModsDir + "Assets/resources";
            DONT_TRACK_LOCATION = File.Exists(BepInExDir + "DontTrackList.txt") ? BepInExDir + "DontTrackList.txt" : QModsDir + "DontTrackList.txt";

            // Check if required files and folders exist based on the resolved paths
            bool picLargeExists = File.Exists(LARGE_PNG_LOCATION);
            bool picSmallExists = File.Exists(SMALL_PNG_LOCATION);
            bool assetBundleExists = File.Exists(ASSET_BUNDLE_LOCATION);
            bool dontTrackListExists = File.Exists(DONT_TRACK_LOCATION);

            // Verify Files were found 
            if (!picLargeExists || !picSmallExists || !assetBundleExists || !dontTrackListExists)
            {
                // Log specific errors for each missing component
                if (!picLargeExists) {
                    Logger.LogError($"ResourceMonitorLarge.png is Missing! [{LARGE_PNG_LOCATION}]");
                }

                if (!picSmallExists) {
                    Logger.LogError($"ResourceMonitorSmall.png is missing! [{SMALL_PNG_LOCATION}]");
                }

                if (!assetBundleExists) {
                    Logger.LogError($"Asset Bundle is missing! [{ASSET_BUNDLE_LOCATION}]");
                }

                if (!dontTrackListExists) {
                    Logger.LogError($"Don't Track List is missing! [{DONT_TRACK_LOCATION}]");
                }

                // Log the overall installation instructions
                Logger.LogError("ResourceMonitor Unofficial Patch is installed incorrectly!");
                Logger.LogError("Please install the original mod to the QMods folder via Vortex.");
                Logger.LogError("If you chose to install the original to the BepInEx/plugins folder, you must overwrite the DLL with the patch!");

                // Return early to prevent further execution
                return false;
            }

            Logger.LogInfo("Found the asset bundle: " + ASSET_BUNDLE_LOCATION);
            Logger.LogInfo("Found ResourceMonitorLarge.png: " + LARGE_PNG_LOCATION);
            Logger.LogInfo("Found ResourceMonitorSmall.png: " + SMALL_PNG_LOCATION);

            var ab = AssetBundle.LoadFromFile(ASSET_BUNDLE_LOCATION);
            RESOURCE_MONITOR_DISPLAY_UI_PREFAB = ab.LoadAsset("ResourceMonitorDisplayUI") as GameObject;
            RESOURCE_MONITOR_DISPLAY_ITEM_UI_PREFAB = ab.LoadAsset("ResourceItem") as GameObject;
            RESOURCE_MONITOR_DISPLAY_MODEL = ab.LoadAsset("ResourceMonitorModel") as GameObject;

            return true;
        }
        private static void LoadDontTrackList()
        {
            if (File.Exists(DONT_TRACK_LOCATION))
            {
                Logger.LogInfo("[ResourceMonitor] Found the dont track list at location: " + DONT_TRACK_LOCATION);
                using (var reader = new StreamReader(DONT_TRACK_LOCATION))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line) == false)
                        {
                            Components.ResourceMonitorLogic.DONT_TRACK_GAMEOBJECTS.Add(line.ToLower());
                        }
                    }
                }
                Components.ResourceMonitorLogic.DONT_TRACK_GAMEOBJECTS.Sort();
            }
            else
            {
                Logger.LogInfo("[ResourceMonitor] Did not find the dont track list at location: " + DONT_TRACK_LOCATION);
            }
        }

        private static void RegisterPrefabs()
        {
            ResourceMonitorLargePrefab.Register();
            ResourceMonitorSmallPrefab.Register();
        }
    }
}
