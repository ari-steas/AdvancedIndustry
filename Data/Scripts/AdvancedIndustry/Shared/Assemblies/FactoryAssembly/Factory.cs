using System;
using AdvancedIndustry.Shared.Definitions;
using AdvancedIndustry.Shared.Definitions.BaseDefinitions;
using Sandbox.ModAPI;
using System.Collections.Generic;
using AdvancedIndustry.Shared.Assemblies.PipeAssembly;
using AdvancedIndustry.Shared.Utils;
using VRage.Game.ModAPI;
using VRageMath;

namespace AdvancedIndustry.Shared.Assemblies.FactoryAssembly
{
    internal class Factory
    {
        public readonly int AssemblyId;
        public IMyCubeBlock MainBlock;
        public HashSet<IMyCubeBlock> Blocks = new HashSet<IMyCubeBlock>();
        public readonly FactoryDefinition Definition;

        public RecipeDefinition CurrentRecipe { get; private set; } = null;


        public Factory(int assemblyId, IMyCubeBlock mainBlock)
        {
            AssemblyId = assemblyId;
            MainBlock = mainBlock;

            DefinitionManager.GetDefinitionForSubtype(mainBlock.BlockDefinition.SubtypeId, out Definition);
            if (Definition == null)
                throw new Exception("Null mainblock factory definition!");

            if (Definition.Recipes.Length == 1)
                SetRecipe(Definition.Recipes[0]);
        }

        public void AddPart(IMyCubeBlock block)
        {
            FactoryDefinition def;
            if (!DefinitionManager.GetDefinitionForSubtype(block.BlockDefinition.SubtypeName, out def) || !Blocks.Add(block))
                return;

            
        }

        public void RemovePart(IMyCubeBlock block)
        {
            FactoryDefinition def;
            if (!Blocks.Remove(block) || !DefinitionManager.GetDefinitionForSubtype(block.BlockDefinition.SubtypeName, out def))
                return;

            
        }

        public void Update()
        {
            MyAPIGateway.Utilities.ShowNotification($"[{AssemblyId}] Factory: (Recipe {CurrentRecipe?.Name ?? "None"}) [{Blocks.Count} part{(Blocks.Count == 1 ? "" : "s")}]", 1000/60);

            foreach (var output in Definition.PipeOutputs)
            {
                Color c = new Color(output.Key.GetHashCode());
                foreach (var vec in output.Value)
                {
                    DebugDraw.AddGridPoint(MiscUtils.BlockToGridCoords(MainBlock, vec), MainBlock.CubeGrid, c, 0);
                }
            }

            if (CurrentRecipe == null)
                return;

            // TODO this is just temporary, replace something better & functional
            Dictionary<string, int> perOutIdx = new Dictionary<string, int>();
            foreach (var outKvp in CurrentRecipe.Outputs)
            {
                if (outKvp.Key.Type != RecipeDefinition.RecipeMaterial.MaterialType.Fluid)
                    continue;

                var fluid = DefinitionManager.FluidDefinitions[outKvp.Key.SubtypeId]; // TODO cache this?
                MyAPIGateway.Utilities.ShowNotification($"Out Fluid: {outKvp.Key.SubtypeId} ({outKvp.Value:N}L, {fluid?.Id.ToString() ?? "NULL"})", 1000/60);

                // get valid pipe network ID from fluid's allowed pipes
                string pipeNetworkId = null;
                foreach (var allowedNetwork in fluid.AllowedPipeTags)
                {
                    foreach (var output in Definition.PipeOutputs.Keys) // TODO definitely cache this
                    {
                        if (output == allowedNetwork)
                        {
                            pipeNetworkId = output;
                            break;
                        }
                    }

                    if (pipeNetworkId != null)
                        break;
                }

                // handling for multiple outputs
                int outIdx;
                if (!perOutIdx.TryGetValue(pipeNetworkId, out outIdx))
                {
                    outIdx = 0;
                    perOutIdx[pipeNetworkId] = 1;
                }
                else
                {
                    perOutIdx[pipeNetworkId]++;
                }

                var pipeNetwork = PipeNetworkManager.GetNetworkAt(MainBlock.CubeGrid, MiscUtils.BlockToGridCoords(MainBlock, Definition.PipeOutputs[pipeNetworkId][outIdx]), pipeNetworkId);

                pipeNetwork?.AddFluid(fluid.Name, outKvp.Value);
            }
        }

        public void Close()
        {
            
        }

        public void SetRecipe(string recipeId)
        {
            CurrentRecipe = DefinitionManager.RecipeDefinitions[recipeId]; // TODO cache this?
        }
    }
}
