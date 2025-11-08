using System.Collections.Generic;
using AdvancedIndustry.Shared.ExternalAPI;
using AdvancedIndustry.Shared.Utils;
using VRage.Game.ModAPI;
using VRageMath;

namespace AdvancedIndustry.Shared.Assemblies.PipeAssembly
{
    internal static class PipeNetworkManager
    {
        private static ModularDefinitionApi ModularApi => ApiManager.ModularApi;
        private static Dictionary<int, PipeNetwork> _assemblies = new Dictionary<int, PipeNetwork>();

        public static void OnInit()
        {
            Log.Info("PipeNetworkManager", "Ready.");
        }

        public static void Update()
        {
            foreach (var assembly in _assemblies.Values)
            {
                assembly.Update();
            }
        }

        public static void Unload()
        {
            _assemblies = null;
            Log.Info("PipeNetworkManager", "Unloaded.");
        }

        public static PipeNetwork GetNetworkAt(IMyCubeGrid grid, Vector3I position, string tag)
        {
            var block = grid.GetCubeBlock(position)?.FatBlock;
            if (block == null)
                return null;

            int asmId = ModularApi.GetContainingAssembly(block, tag);
            PipeNetwork net;
            if (asmId == -1 || !_assemblies.TryGetValue(asmId, out net))
                return null;

            return net;
        }

        public static void OnPartAdd(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            PipeNetwork assembly;
            if (!_assemblies.TryGetValue(assemblyId, out assembly))
            {
                assembly = new PipeNetwork(assemblyId);
                _assemblies.Add(assemblyId, assembly);
            }
            assembly.AddPart(block);
        }

        public static void OnPartRemove(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            PipeNetwork assembly;
            if (!_assemblies.TryGetValue(assemblyId, out assembly))
                return;
            assembly.RemovePart(block);
        }

        public static void OnPartDestroy(int assemblyId, IMyCubeBlock block, bool isBaseBlock)
        {
            // TODO
        }

        public static void OnAssemblyClose(int assemblyId)
        {
            PipeNetwork assembly;
            if (!_assemblies.TryGetValue(assemblyId, out assembly))
                return;
            assembly.Close();
            _assemblies.Remove(assemblyId);
        }
    }
}
