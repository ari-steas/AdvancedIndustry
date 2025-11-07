using System;
using AdvancedIndustry.Data.Scripts.AdvancedIndustry.Shared.ExternalAPI;
using AdvancedIndustry.Shared.Utils;
using RichHudFramework.Client;

namespace AdvancedIndustry.Shared.ExternalAPI
{
    internal static class ApiManager
    {
        public static ModularDefinitionApi ModularApi;
        public static DefinitionApi DefinitionApi;

        private static Action _onRichHudReady = () => Log.Info("RichHud", "Ready.");
        private static Action _onModularApiReady = () => Log.Info("ModularDefinitionApi", "Ready.");
        private static Action _onDefinitionApiReady = () => Log.Info("DefinitionApi", "Ready.");

        public static void Init()
        {
            Log.IncreaseIndent();

            try
            {
                ModularApi = new ModularDefinitionApi();
                ModularApi.Init(GlobalData.ModContext, () => _onModularApiReady.Invoke());
            }
            catch (Exception ex)
            {
                Log.Exception("ApiManager", new Exception("Failed to load ModularDefinitionApi!", ex));
            }

            try
            {
                DefinitionApi = new DefinitionApi();
                DefinitionApi.Init(GlobalData.ModContext, () => _onDefinitionApiReady.Invoke());
            }
            catch (Exception ex)
            {
                Log.Exception("ApiManager", new Exception("Failed to load DefinitionApi!", ex));
            }

            try
            {
                RichHudClient.Init("AdvancedIndustry", () => _onRichHudReady.Invoke(), null);
            }
            catch (Exception ex)
            {
                Log.Exception("ApiManager", new Exception("Failed to load RichHudClient!", ex));
            }

            Log.DecreaseIndent();
            Log.Info("ApiManager", "Ready.");
        }

        public static void Unload()
        {
            Log.IncreaseIndent();

            _onRichHudReady = null;
            _onModularApiReady = null;
            _onDefinitionApiReady = null;
            ModularApi = null;

            Log.DecreaseIndent();
            Log.Info("ApiManager", "Unloaded.");
        }

        /// <summary>
        /// Registers an action to invoke when the API is ready, or calls it immediately if ready.
        /// </summary>
        /// <param name="action"></param>
        public static void RichHudOnLoadRegisterOrInvoke(Action action)
        {
            if (RichHudClient.Registered)
                action.Invoke();
            else
                _onRichHudReady += action;
        }

        /// <summary>
        /// Registers an action to invoke when the API is ready, or calls it immediately if ready.
        /// </summary>
        /// <param name="action"></param>
        public static void ModularApiOnLoadRegisterOrInvoke(Action action)
        {
            if (ModularApi.IsReady)
                action.Invoke();
            else
                _onModularApiReady += action;
        }

        /// <summary>
        /// Registers an action to invoke when the API is ready, or calls it immediately if ready.
        /// </summary>
        /// <param name="action"></param>
        public static void DefinitionApiOnLoadRegisterOrInvoke(Action action)
        {
            if (DefinitionApi.IsReady)
                action.Invoke();
            else
                _onDefinitionApiReady += action;
        }
    }
}
