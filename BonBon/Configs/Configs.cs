using System;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace BonBon.Config
{
    internal class BonBonConfigs
    {
        private const int byteDim = 4;
        private const int byteDimConfig = 21;

        private static int BonBonSpawnRateLocal = 10;
        private static float BonBonRangeLocal = 35;
        private static float BonBonCooldownLocal = 10;
        private static float BonBonStunTimeLocal = 7;
        private static float BonBonBatteryUsageLocal = 0.34f;

        public static int BonBonSpawnRate = 10;
        public static float BonBonRange = 35;
        public static float BonBonCooldown = 10;
        public static float BonBonStunTime = 7;
        public static float BonBonBatteryUsage = 0.34f;

        private static void SetValues(int SpawnRate, float Range, float Cooldown, float StunTime, float BatteryUsage)
        {
            BonBonSpawnRate = SpawnRate;
            BonBonRange = Range;
            BonBonCooldown = Cooldown;
            BonBonStunTime = StunTime;
            BonBonBatteryUsage = BatteryUsage;
        }
        private static void SetToLocalValues() => SetValues(BonBonSpawnRateLocal, BonBonRangeLocal, BonBonCooldownLocal, BonBonStunTimeLocal, BonBonBatteryUsageLocal);

        public static void LoadConfig(ConfigFile config)
        {
            BonBonSpawnRateLocal = Math.Clamp(config.Bind("General", "BonBonSpawnRate", 10, "Sets the BonBon spawn rate, the higher the value, the more common it will be (recommended to keep it between 5 and 40).").Value, 0, 9999);
            BonBonRangeLocal = Mathf.Clamp(config.Bind("General", "BonBonRange", 35, "Sets the BonBon stun range.").Value, 0, 9999);
            BonBonCooldownLocal = Mathf.Clamp(config.Bind("General", "BonBonCooldown", 10, "Sets the BonBon cooldown.").Value, 0, 9999);
            BonBonStunTimeLocal = Mathf.Clamp(config.Bind("General", "BonBonStunTime", 7, "Sets the BonBon stun time.").Value, 0, 9999);
            BonBonBatteryUsageLocal = Mathf.Clamp(config.Bind("General", "BonBonBatteryUsage", 0.34f, "Sets the BonBon battery usage (in the case of the default value it removes 34% of the battery with each use).").Value, 0, 9999);

            SetToLocalValues();
        }

        public static byte[] GetSettings()
        {
            byte[] data = new byte[byteDimConfig];
            data[0] = 1;
            Array.Copy(BitConverter.GetBytes(BonBonSpawnRateLocal), 0, data, 1, byteDim);
            Array.Copy(BitConverter.GetBytes(BonBonRangeLocal), 0, data, 5, byteDim);
            Array.Copy(BitConverter.GetBytes(BonBonCooldownLocal), 0, data, 9, byteDim);
            Array.Copy(BitConverter.GetBytes(BonBonStunTimeLocal), 0, data, 13, byteDim);
            Array.Copy(BitConverter.GetBytes(BonBonBatteryUsageLocal), 0, data, 17, byteDim);
            return data;
        }

        public static void SetSettings(byte[] data)
        {
            switch (data[0])
            {
                case 1:
                    {
                        BonBonSpawnRate = BitConverter.ToInt32(data, 1);
                        BonBonRange = BitConverter.ToSingle(data, 5);
                        BonBonCooldown = BitConverter.ToSingle(data, 9);
                        BonBonStunTime = BitConverter.ToSingle(data, 13);
                        BonBonBatteryUsage = BitConverter.ToSingle(data, 17);
                        Debug.Log("BonBonLog: Host config set successfully");
                        break;
                    }
                default:
                    {
                        throw new Exception("BonBonLog: Invalid version byte");
                    }
            }
        }

        // networking

        private static bool IsHost() => NetworkManager.Singleton.IsHost;

        public static void OnRequestSync(ulong clientID, FastBufferReader reader)
        {
            if (!IsHost()) return;

            Debug.Log("BonBonLog: Sending config to client " + clientID);
            byte[] data = GetSettings();
            FastBufferWriter dataOut = new(data.Length, Unity.Collections.Allocator.Temp, data.Length);
            try
            {
                dataOut.WriteBytes(data);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("BonBon_OnReceiveConfigSync", clientID, dataOut, NetworkDelivery.Reliable);
            }
            catch (Exception e)
            {
                Debug.LogError("BonBonLog: Failed to send config: " + e);
            }
            finally
            {
                dataOut.Dispose();
            }
        }

        public static void OnReceiveSync(ulong clientID, FastBufferReader reader)
        {
            Debug.Log("BonBonLog: Received config from host");
            byte[] data = new byte[byteDimConfig];
            try
            {
                reader.ReadBytes(ref data, byteDimConfig);
                SetSettings(data);
            }
            catch (Exception e)
            {
                Debug.LogError("BonBonLog: Failed to receive config: " + e);
                SetToLocalValues();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        static void ServerConnect()
        {
            if (IsHost())
            {
                Debug.Log("BonBonLog: Started hosting, using local settings");
                SetToLocalValues();
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("BonBon_OnRequestConfigSync", OnRequestSync);
            }
            else
            {
                Debug.Log("BonBonLog: Connected to server, requesting settings");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("BonBon_OnReceiveConfigSync", OnReceiveSync);
                FastBufferWriter blankOut = new(byteDim, Unity.Collections.Allocator.Temp);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("BonBon_OnRequestConfigSync", 0uL, blankOut, NetworkDelivery.Reliable);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        static void ServerDisconnect()
        {
            Debug.Log("BonBonLog: Server disconnect");
            SetToLocalValues();
        }
    }
}