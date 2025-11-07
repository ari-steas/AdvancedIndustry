using AdvancedIndustry.Shared.ExternalAPI;
using AdvancedIndustry.Shared.Utils;
using System;
using AdvancedIndustry.Shared.Definitions;
using VRage.Game.Components;

namespace AdvancedIndustry.Shared
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, Priority = int.MinValue)]
    // ReSharper disable once UnusedType.Global
    internal class SharedMain : MySessionComponentBase
    {
        public static SharedMain I { get; private set; }

        public override void LoadData()
        {
            try
            {
                I = this;

                // Killswitch for debugging
                if (!GlobalData.CheckShouldLoad())
                {
                    I = null;
                    return;
                }

                Log.Init(ModContext);
                Log.Info("SharedMain", "Start initialize...");
                Log.IncreaseIndent();

                GlobalData.Init();
                ApiManager.Init();
                DefinitionManager.Init();
                
                Log.DecreaseIndent();
                Log.Info("SharedMain", "Initialized.");
            }
            catch (Exception ex)
            {
                Log.Exception("SharedMain", ex, true);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (GlobalData.Killswitch)
                return;

            try
            {
                DefinitionManager.Update();
            }
            catch (Exception ex)
            {
                Log.Exception("SharedMain", ex, false);
            }
        }

        protected override void UnloadData()
        {
            if (GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("SharedMain", "Start unload...");
                Log.IncreaseIndent();
                
                DefinitionManager.Unload();
                ApiManager.Unload();
                GlobalData.Unload();

                I = null;

                Log.DecreaseIndent();
                Log.Info("SharedMain", "Unloaded.");
            }
            catch (Exception ex)
            {
                Log.Exception("SharedMain", ex, true);
            }
        }
    }
}
