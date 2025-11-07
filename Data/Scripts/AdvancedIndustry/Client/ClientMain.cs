using System;
using AdvancedIndustry.Client.Interface;
using AdvancedIndustry.Shared;
using AdvancedIndustry.Shared.Utils;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace AdvancedIndustry.Client
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once UnusedType.Global
    internal class ClientMain : MySessionComponentBase
    {
        public override void LoadData()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("ClientMain", "Start initialize...");
                Log.IncreaseIndent();

                BlockCategoryManager.Init();

                Log.DecreaseIndent();
                Log.Info("ClientMain", "Initialized.");
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex, true);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex);
            }
        }

        public override void Draw()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex);
            }
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("ClientMain", "Start unload...");
                Log.IncreaseIndent();

                BlockCategoryManager.Close();

                Log.DecreaseIndent();
                Log.Info("ClientMain", "Unloaded.");
                Log.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex, true);
            }
        }
    }
}
