using AdvancedIndustry.Shared.Definitions.BaseDefinitions;
using AdvancedIndustry.Shared.ExternalAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;

namespace AdvancedIndustry.Shared.Definitions
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    internal class TestDefinitions : MySessionComponentBase
    {
        private static DefinitionApi DefinitionApi => ApiManager.DefinitionApi;

        private FluidPipeDefinition[] _pipeDefinitions = 
        {
            new FluidPipeDefinition
            {
                Name = "ConveyorPipe",
                SubtypeIds = new[]
                {
                    "ConveyorTube",
                },
                AssemblyTags = new[]
                {
                    "Pipe",
                },
                AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
                {
                    ["Pipe"] = new Dictionary<Vector3I, string[]>
                    {
                        [Vector3I.Up] = Array.Empty<string>(),
                        [Vector3I.Down] = Array.Empty<string>(),
                    },
                },
                Storage = 1,
            },
        };

        private FactoryDefinition[] _factoryDefinitions = 
        {
            new FactoryDefinition
            {
                Name = "ReactorFactory",
                SubtypeIds = new[]
                {
                    "LargeBlockSmallGenerator",
                },
                AssemblyTags = new[]
                {
                    "PipeIn",
                    "PipeOut",
                },
                AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>
                {
                    ["PipeIn"] = new Dictionary<Vector3I, string[]>
                    {
                        [Vector3I.Up] = Array.Empty<string>(),
                    },
                    ["PipeOut"] = new Dictionary<Vector3I, string[]>
                    {
                        [Vector3I.Backward] = Array.Empty<string>(),
                    },
                },
                Recipes = new[]
                {
                    "TestOutputRecipe",
                }
            },
        };

        private RecipeDefinition[] _recipeDefinitions = 
        {
            new RecipeDefinition
            {
                Name = "TestOutputRecipe",
                ProcessTime = 1,
                Outputs = new Dictionary<RecipeDefinition.RecipeMaterial, float>
                {
                    [new RecipeDefinition.RecipeMaterial { SubtypeId = "TestFluid", Type = RecipeDefinition.RecipeMaterial.MaterialType.Fluid }] = 1,
                }
            }
        };

        private FluidDefinition[] _fluidDefinitions = 
        {

        };

        public override void LoadData()
        {
            ApiManager.DefinitionApiOnLoadRegisterOrInvoke(OnDefApiReady);
        }

        private void OnDefApiReady()
        {
            foreach (var def in _pipeDefinitions)
                DefinitionApi.RegisterDefinition(def.Name, def);
            foreach (var def in _factoryDefinitions)
                DefinitionApi.RegisterDefinition(def.Name, def);
            foreach (var def in _recipeDefinitions)
                DefinitionApi.RegisterDefinition(def.Name, def);
            foreach (var def in _fluidDefinitions)
                DefinitionApi.RegisterDefinition(def.Name, def);
        }
    }
}
