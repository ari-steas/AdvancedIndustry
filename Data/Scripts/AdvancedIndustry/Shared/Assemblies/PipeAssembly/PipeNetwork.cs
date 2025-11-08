using System.Collections.Generic;
using AdvancedIndustry.Shared.Definitions;
using AdvancedIndustry.Shared.Definitions.BaseDefinitions;
using AdvancedIndustry.Shared.Utils;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace AdvancedIndustry.Shared.Assemblies.PipeAssembly
{
    internal class PipeNetwork
    {
        public readonly int AssemblyId;
        public HashSet<IMyCubeBlock> Blocks = new HashSet<IMyCubeBlock>();

        public string FluidId = null;
        public float MaxStorage { get; private set; } = 0;
        public float CurrentStorage { get; private set; } = 0;

        private float _fluidToRemove = 0;

        public PipeNetwork(int assemblyId)
        {
            AssemblyId = assemblyId;
        }

        public bool AddFluid(string id, float amount)
        {
            if (CurrentStorage + amount > MaxStorage)
                return false;

            if (FluidId == null)
                FluidId = id;

            if (id != FluidId)
                return false;

            CurrentStorage += amount;
            return true;
        }

        public bool RemoveFluid(string id, float amount)
        {
            if (id != FluidId || amount > CurrentStorage)
                return false;

            CurrentStorage -= amount;
            if (CurrentStorage <= 0)
                FluidId = null;
            return true;
        }

        public void AddPart(IMyCubeBlock block)
        {
            FluidPipeDefinition def;
            if (!DefinitionManager.GetDefinitionForSubtype(block.BlockDefinition.SubtypeName, out def) || !Blocks.Add(block))
                return;

            // TODO: retrieve fluid % from block
            MaxStorage += def.Storage;
        }

        public void RemovePart(IMyCubeBlock block)
        {
            FluidPipeDefinition def;
            if (!Blocks.Remove(block) || !DefinitionManager.GetDefinitionForSubtype(block.BlockDefinition.SubtypeName, out def))
                return;

            if (!block.MarkedForClose)
                SaveFluidLevel(block);
            _fluidToRemove += CurrentStorage * (def.Storage / MaxStorage); // delay removing by a tick to ensure split blocks have the correct fluid level
            MaxStorage -= def.Storage;
        }

        public void Update()
        {
            MyAPIGateway.Utilities.ShowNotification($"[{AssemblyId}] Fluid: {FluidId} ({CurrentStorage:N1}/{MaxStorage:N1}) [{Blocks.Count} part{(Blocks.Count > 1 ? "s" : "")}]", 1000/60);
            if (_fluidToRemove > 0)
            {
                CurrentStorage -= _fluidToRemove;
                _fluidToRemove = 0;
            }
        }

        public void Close()
        {
            foreach (var block in Blocks)
            {
                SaveFluidLevel(block);
            }
        }

        private void SaveFluidLevel(IMyCubeBlock block)
        {
            // TODO: store fluid % in blocks
        }
    }
}
