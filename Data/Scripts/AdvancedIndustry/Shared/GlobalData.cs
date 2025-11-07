using AdvancedIndustry.Shared.Utils;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Net;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.Utils;
using static AdvancedIndustry.Shared.Utils.IniConfig;

namespace AdvancedIndustry.Shared
{
    public static class GlobalData
    {
        public static bool Killswitch = false;
        public const ushort ServerNetworkId = 55693;
        public const ushort DataNetworkId = 55694;
        public const ushort ClientNetworkId = 55695;
        public static int MainThreadId { get; private set; } = -1;
        public static IMyModContext ModContext { get; private set; }
        public static int DebugLevel = 0;

        private static bool _isReady = false;

        public static double SyncRange => MyAPIGateway.Session.SessionSettings.SyncDistance;
        public static double SyncRangeSq => (double) MyAPIGateway.Session.SessionSettings.SyncDistance * MyAPIGateway.Session.SessionSettings.SyncDistance;
        public static List<IMyPlayer> Players = new List<IMyPlayer>();
        public static List<MyPlanet> Planets = new List<MyPlanet>();

        public static readonly MyDefinitionId ElectricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        public static readonly MyDefinitionId HydrogenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");

        private static List<Action<MyEntity>> _onEntityAddActions = new List<Action<MyEntity>>();
        private static List<Action<MyEntity>> _onEntityRemoveActions = new List<Action<MyEntity>>();

        public static event Action<MyEntity> OnEntityAdd
        {
            add
            {
                if (value == null)
                    return;

                _onEntityAddActions.Add(value);
                MyEntities.OnEntityAdd += value;
                foreach (var entity in MyEntities.GetEntities())
                {
                    value.Invoke(entity);
                }
            }
            remove
            {
                if (value == null)
                    return;

                _onEntityAddActions.Remove(value);
                MyEntities.OnEntityAdd -= value;
            }
        }

        public static event Action<MyEntity> OnEntityRemove
        {
            add
            {
                if (value == null)
                    return;

                _onEntityRemoveActions.Add(value);
                MyEntities.OnEntityRemove += value;
            }
            remove
            {
                if (value == null)
                    return;

                _onEntityRemoveActions.Remove(value);
                MyEntities.OnEntityRemove -= value;
            }
        }

        #region General Config

        private static IniConfig _generalConfig = new IniConfig(
            FileLocation.WorldStorage,
            "config.ini",
            "General Config",
            " AdvancedIndustry World Settings\n\n Set config values below,\n   then restart the world.\n Delete a line to reset it to default.\n ");

        #endregion

        #region Init Functions

        internal static void Init()
        {
            Log.Info("GlobalData", "Start initialize...");
            Log.IncreaseIndent();

            // Wait for and read config
            if (MyAPIGateway.Session.IsServer)
            {
                _generalConfig.ReadSettings();
                _generalConfig.WriteSettings();
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(DataNetworkId, ServerMessageHandler);
                _isReady = true;
            }
            else if (!_isReady)
            {
                Log.Info("GlobalData", "Reading config data from network. Default configs will temporarily be used.");
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(DataNetworkId, ClientMessageHandler);
                MyAPIGateway.Multiplayer.SendMessageToServer(DataNetworkId, Array.Empty<byte>());
            }

            // Mod context handling
            {
                ModContext = SharedMain.I.ModContext;
                string modId = ModContext.ModId.Replace(".sbm", "");
                long discard;

                Log.Info("GlobalData", "ModContext:\n" +
                                       $"\tName: {ModContext.ModName}\n" +
                                       $"\tItem: {(long.TryParse(modId, out discard) ? "https://steamcommunity.com/workshop/filedetails/?id=" : "LocalMod ")}{modId}\n" +
                                       $"\tService: {ModContext.ModServiceName} (if this isn't steam, please report the mod)");
            }

            {
                MainThreadId = Environment.CurrentManagedThreadId;
                Log.Info("GlobalData", $"Main thread ID: {MainThreadId}");
            }

            // Planet cache
            {
                OnEntityAdd += OnPlanetEntityAdd;
                OnEntityRemove += OnPlanetEntityRemove;
            }

            Log.DecreaseIndent();
            Log.Info("GlobalData", "Ready.");
            _isReady = true;
        }

        internal static void Unload()
        {
            // fields
            Players = null;
            Planets = null;
            _isReady = false;

            // configs
            if (MyAPIGateway.Session.IsServer)
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(DataNetworkId, ServerMessageHandler);
            else
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(DataNetworkId, ClientMessageHandler);
            _generalConfig = null;

            // delegates
            foreach (var action in _onEntityAddActions)
                MyEntities.OnEntityAdd -= action;
            _onEntityAddActions = null;
            foreach (var action in _onEntityRemoveActions)
                MyEntities.OnEntityRemove -= action;
            _onEntityRemoveActions = null;

            Log.Info("GlobalData", "Data cleared.");
        }

        internal static bool CheckShouldLoad()
        {
            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                if (mod.GetModContext().ModPath == SharedMain.I.ModContext.ModPath)
                    continue;

                if (mod.GetModContext().ModId.RemoveChars(' ').ToLower().Contains("advancedindustry"))
                {
                    Killswitch = true;
                    MyLog.Default.WriteLineAndConsole($"[Advanced Industry] Found local AdvIn version \"{mod.GetPath()}\" - cancelling init and disabling mod. This ModId: {SharedMain.I.ModContext.ModId}");
                    return false;
                }
            }

            Killswitch = false;
            return true;
        }

        private static void ServerMessageHandler(ushort channelId, byte[] serialized, ulong senderSteamId, bool isSenderServer)
        {
            try
            {
                Log.Info("GlobalData", $"Received data request from {senderSteamId}.");
                if (isSenderServer)
                    return;

                var file = _generalConfig.ReadFile();

                MyAPIGateway.Multiplayer.SendMessageTo(DataNetworkId, MyAPIGateway.Utilities.SerializeToBinary(file), senderSteamId);
            }
            catch (Exception ex)
            {
                Log.Exception("GlobalData", ex, true);
            }
        }

        private static void ClientMessageHandler(ushort channelId, byte[] serialized, ulong senderSteamId, bool isSenderServer)
        {
            if (!isSenderServer)
                return;

            try
            {
                var data = MyAPIGateway.Utilities.SerializeFromBinary<string>(serialized);
                if (data == null)
                {
                    Log.Info("GlobalData", "Null message!");
                    return;
                }

                Log.Info("GlobalData",
                    $"Reading settings data from network:\n===========================================\n\n{data}\n===========================================\n");

                var ini = new MyIni();
                if (!ini.TryParse(data))
                {
                    Log.Info("GlobalData", "Failed to read settings data!");
                    return;
                }

                foreach (var setting in _generalConfig.AllSettings)
                    setting.Read(ini, _generalConfig.SectionName);

                // Can't unregister network handlers inside a network handler call
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                {
                    MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(DataNetworkId, ClientMessageHandler);
                });
            }
            catch (Exception ex)
            {
                Log.Exception("GlobalData", ex, true);
            }
        }

        private static void OnPlanetEntityAdd(IMyEntity entity)
        {
            var planet = entity as MyPlanet;
            if (planet != null)
                Planets.Add(planet);
        }

        private static void OnPlanetEntityRemove(IMyEntity entity)
        {
            var planet = entity as MyPlanet;
            if (planet != null)
                Planets.Remove(planet);
        }

        #endregion
    }
}
