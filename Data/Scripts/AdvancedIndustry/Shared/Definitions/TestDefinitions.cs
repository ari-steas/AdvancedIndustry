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
                Recipes = new[]
                {
                    "TestOutputRecipe",
                },
                PipeInputs = new Dictionary<string, Vector3I[]>
                {
                    ["Pipe"] = new []
                    {
                        Vector3I.Up,
                    },
                },
                PipeOutputs = new Dictionary<string, Vector3I[]>
                {
                    ["Pipe"] = new []
                    {
                        Vector3I.Backward,
                    },
                },
            }
        };

        private RecipeDefinition[] _recipeDefinitions = 
        {
            new RecipeDefinition
            {
                Name = "TestOutputRecipe",
                ProcessTime = 1,
                Outputs = new Dictionary<RecipeDefinition.RecipeMaterial, float>
                {
                    [new RecipeDefinition.RecipeMaterial { SubtypeId = "TestFluid", Type = RecipeDefinition.RecipeMaterial.MaterialType.Fluid }] = 0.5f/60f,
                }
            }
        };

        private FluidDefinition[] _fluidDefinitions = 
        {
            new FluidDefinition
            {
                Name = "TestFluid",
                AllowedPipeTags = new []
                {
                    "Pipe",
                },
                Density = 1,
            }
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
