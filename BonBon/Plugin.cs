using BepInEx;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Reflection;
using LethalLib.Modules;
using BonBon.Scripts;
using System.Collections.Generic;
using BonBon.Config;

namespace BonBon
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib", "0.14.2")]
    public class Plugin : BaseUnityPlugin
    {
        Harmony harmony = new Harmony(GUID);
        const string GUID = "Entity378.BonBon";
        const string NAME = "BonBon";
        const string VERSION = "1.1.0";

        private void Awake()
        {
            BonBonConfigs.LoadConfig(Config);
            Harmony.CreateAndPatchAll(typeof(BonBonConfigs));
            Logger.LogInfo("BonBonLog: Config loaded");

            string assetsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "bonbon");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetsDir);

            List<AudioClip> UseVoicelinesSFX = new List<AudioClip>
            {
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse01.ogg"),
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse02.ogg"),
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse03.ogg"),
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse04.ogg"),
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse05.ogg"),
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse06.ogg"),
                bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUse07.ogg"),
            };

            AudioClip BonBonErrorSFX = bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonError.ogg");
            AudioClip BonBonUseButtonSFX = bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonUseButton.ogg");
            AudioClip BonBonDropSFX = bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonDrop.ogg");
            AudioClip BonBonGrabSFX = bundle.LoadAsset<AudioClip>("Assets/SFX/BonBonGrab.ogg");

            Item BonBon = bundle.LoadAsset<Item>("Assets/Items/BonBonItem.asset");

            BonBonScript SpawnedBonBon = BonBon.spawnPrefab.AddComponent<BonBonScript>();
            SpawnedBonBon.itemProperties = BonBon;
            SpawnedBonBon.UseVoicelinesSFX = UseVoicelinesSFX;
            SpawnedBonBon.errorSFX = BonBonErrorSFX;
            SpawnedBonBon.useButtonSFX = BonBonUseButtonSFX;
            SpawnedBonBon.dropSFX = BonBonDropSFX;
            SpawnedBonBon.grabSFX = BonBonGrabSFX;

            NetworkPrefabs.RegisterNetworkPrefab(BonBon.spawnPrefab);
            Utilities.FixMixerGroups(BonBon.spawnPrefab);
            Items.RegisterScrap(BonBon, BonBonConfigs.BonBonSpawnRate, Levels.LevelTypes.All);
            
            Logger.LogInfo("BonBonLog: BonBon, go get Him!");
        }
    }
}