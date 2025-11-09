using AdvancedIndustry.Shared.Assemblies.FactoryAssembly;
using AdvancedIndustry.Shared.Assemblies.PipeAssembly;
using AdvancedIndustry.Shared.Definitions.BaseDefinitions;
using AdvancedIndustry.Shared.ExternalAPI;
using AdvancedIndustry.Shared.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace AdvancedIndustry.Shared.Definitions
{
    internal static class DefinitionManager
    {
        private static DefinitionApi DefinitionApi => ApiManager.DefinitionApi;
        private static ModularDefinitionApi ModularApi => ApiManager.ModularApi;

        public static DefinitionContainer<FluidPipeDefinition> FluidPipeDefinitions { get; private set; }
        public static DefinitionContainer<FactoryDefinition> FactoryDefinitions { get; private set; }
        public static DefinitionContainer<RecipeDefinition> RecipeDefinitions { get; private set; }
        public static DefinitionContainer<FluidDefinition> FluidDefinitions { get; private set; }

        private static bool _needsModularRegister = false;
        private static bool _hasShownApiFailMsg = false;
        private static int _apiFailCheckCount = 0;

        public static void Init()
        {
            _hasShownApiFailMsg = false;
            _apiFailCheckCount = 0;
            ApiManager.DefinitionApiOnLoadRegisterOrInvoke(OnDefinitionApiReady);
        }

        public static void Unload()
        {
            FluidPipeDefinitions.Close();
            FactoryDefinitions.Close();
            RecipeDefinitions.Close();
            FluidDefinitions.Close();

            FluidPipeDefinitions = null;
            FactoryDefinitions = null;
            RecipeDefinitions = null;
            FluidDefinitions = null;
        }

        public static void Update()
        {
            if (_needsModularRegister && MyAPIGateway.Session.GameplayFrameCounter > 30) // wait a few ticks for everything to init
            {
                RegisterModularDefinitions();
                _needsModularRegister = false;
            }

            if ((DefinitionApi.IsReady && ModularApi.IsReady) || _hasShownApiFailMsg || _apiFailCheckCount++ < 1)
                return;

            DisplayApiFail();
        }

        public static bool GetDefinitionForSubtype<TDefinition>(string subtypeId, out TDefinition def) where TDefinition : AssemblyBlockDefinition
        {
            if (typeof(TDefinition) == typeof(FluidPipeDefinition))
            {
                foreach (var definition in FluidPipeDefinitions)
                {
                    foreach (var subtype in definition.SubtypeIds)
                    {
                        if (subtype != subtypeId)
                            continue;
                        def = definition as TDefinition;
                        return true;
                    }
                }
            }
            else if (typeof(TDefinition) == typeof(FactoryDefinition))
            {
                foreach (var definition in FactoryDefinitions)
                {
                    foreach (var subtype in definition.SubtypeIds)
                    {
                        if (subtype != subtypeId)
                            continue;
                        def = definition as TDefinition;
                        return true;
                    }
                }
            }

            def = null;
            return false;
        }

        /// <summary>
        ///     Converts and registers internal AdvancedIndustry AssemblyBlockDefinitions into valid Modular Assemblies definitions.
        /// <remarks>
        ///     AdvancedIndustry uses its own version of an assembly definition that's incompatible with Modular Assemblies; that framework was never meant for this kind of abuse.
        /// </remarks>
        /// </summary>
        private static void RegisterModularDefinitions()
        {
            // ModularPhysicalDefinition uses fixed-size arrays, so introducing DynamicAssemblyDef struct with dynamic collections & some validation
            Dictionary<string, DynamicAssemblyDef> assemblyDefinitions = new Dictionary<string, DynamicAssemblyDef>();

            // no clean way to combine all AssemblyBlockDefinitions into a single collection
            foreach (var pipeDef in FluidPipeDefinitions)
            {
                foreach (var assemblyTag in pipeDef.AssemblyTags)
                {
                    DynamicAssemblyDef modDef;
                    if (!assemblyDefinitions.TryGetValue(assemblyTag, out modDef))
                    {
                        modDef = new DynamicAssemblyDef(assemblyTag);
                        assemblyDefinitions.Add(assemblyTag, modDef);
                    }

                    modDef.AddPartDef(pipeDef);
                }
            }

            foreach (var facDef in FactoryDefinitions)
            {
                foreach (var assemblyTag in facDef.AssemblyTags)
                {
                    DynamicAssemblyDef modDef;
                    if (!assemblyDefinitions.TryGetValue(assemblyTag, out modDef))
                    {
                        modDef = new DynamicAssemblyDef(assemblyTag, facDef.SubtypeIds);
                        assemblyDefinitions.Add(assemblyTag, modDef);
                    }

                    modDef.AddPartDef(facDef);
                }
            }

            Log.Info("DefinitionManager", $"Generated {assemblyDefinitions.Count} dynamic assembly definitions.");

            // clone definitions into container
            var container = new DefinitionDefs.ModularDefinitionContainer
            {
                PhysicalDefs = new DefinitionDefs.ModularPhysicalDefinition[assemblyDefinitions.Count]
            };
            int i = 0;
            foreach (var definition in assemblyDefinitions.Values)
            {
                // TODO onPartAdd, remove, destroyed callbacks in factory assemblies
                var modDef = (DefinitionDefs.ModularPhysicalDefinition)definition;

                switch (definition.Type)
                {
                    case DynamicAssemblyDef.DefinitionType.Pipe:
                        modDef.OnInit = PipeNetworkManager.OnInit;
                        modDef.OnPartAdd = PipeNetworkManager.OnPartAdd;
                        modDef.OnPartRemove = PipeNetworkManager.OnPartRemove;
                        modDef.OnPartDestroy = PipeNetworkManager.OnPartDestroy;
                        modDef.OnAssemblyClose = PipeNetworkManager.OnAssemblyClose;
                        Log.Info("DefinitionManager", $"Assigned PipeNetworkManager delegates for definition {definition.Name}.");
                        break;
                    case DynamicAssemblyDef.DefinitionType.Factory:
                        // TODO: better method of differentiating factory assemblies
                        modDef.BaseBlockSubtypes = definition.MainFactoryBlock;

                        modDef.OnInit = FactoryManager.OnInit;
                        modDef.OnPartAdd = FactoryManager.OnPartAdd;
                        modDef.OnPartRemove = FactoryManager.OnPartRemove;
                        modDef.OnPartDestroy = FactoryManager.OnPartDestroy;
                        modDef.OnAssemblyClose = FactoryManager.OnAssemblyClose;
                        Log.Info("DefinitionManager", $"Assigned FactoryManager delegates for definition {definition.Name}.");
                        break;
                }

                container.PhysicalDefs[i++] = modDef;
            }

            // send generated definitions to framework
            var validDefinitions = ModularApi.RegisterDefinitions(container);

            // validation fail trigger should never occur, but just in case
            foreach (var asmDefId in assemblyDefinitions.Keys)
            {
                if (!Enumerable.Contains(validDefinitions, asmDefId))
                {
                    Log.Exception("DefinitionManager", new Exception($"Failed to register assembly definition with ID {asmDefId}."), true);
                }
            }

            Log.Info("DefinitionManager", $"Registered {validDefinitions.Length} assembly definitions.");
        }

        private static void OnDefinitionApiReady()
        {
            FluidPipeDefinitions = new DefinitionContainer<FluidPipeDefinition>();
            FactoryDefinitions = new DefinitionContainer<FactoryDefinition>();
            RecipeDefinitions = new DefinitionContainer<RecipeDefinition>();
            FluidDefinitions = new DefinitionContainer<FluidDefinition>();

            ApiManager.ModularApiOnLoadRegisterOrInvoke(OnModularApiReady);
        }

        private static void OnModularApiReady()
        {
            _needsModularRegister = true;
            Log.Info("DefinitionManager", "ModularAPI ready, awaiting definition registration...");
        }

        private static void DisplayApiFail()
        {
            if (!DefinitionApi.IsReady)
            {
                MyAPIGateway.Utilities.ShowNotification("AdvancedIndustry - DefinitionApi isn't loaded!", int.MaxValue, "Red");

                Log.Info("AdvancedIndustry", "DefinitionApi failed to load!\n" +
                                             "==================================\n" +
                                             "\n" +
                                             "AdvancedIndustry *requires* the Definition Helper mod to load factory definitions.\n" +
                                             "For whatever reason, the API was unable to register within 10 ticks. This is most likely because the mod wasn't included in the world.\n" +
                                             "The Definition Helper library can be found at https://steamcommunity.com/sharedfiles/filedetails/?id=3407764326.\n" +
                                             "If you included the mod in the world, or there's an exception log above this, please reach out to @aristeas. on discord.\n" +
                                             "\n" +
                                             "Best of luck,\n" +
                                             "Aristeas\n" +
                                             "\n" +
                                             "==================================");
            }

            if (!ModularApi.IsReady)
            {
                MyAPIGateway.Utilities.ShowNotification("AdvancedIndustry - ModularApi isn't loaded!", int.MaxValue, "Red");

                Log.Info("AdvancedIndustry", "ModularApi failed to load!\n" +
                                             "==================================\n" +
                                             "\n" +
                                             "AdvancedIndustry *requires* the Modular Assemblies mod to function.\n" +
                                             "For whatever reason, the API was unable to register within 10 ticks. This is most likely because the mod wasn't included in the world.\n" +
                                             "The Modular Assemblies framework can be found at https://steamcommunity.com/sharedfiles/filedetails/?id=3130656054.\n" +
                                             "If you included the mod in the world, or there's an exception log above this, please reach out to @aristeas. on discord.\n" +
                                             "\n" +
                                             "Best of luck,\n" +
                                             "Aristeas\n" +
                                             "\n" +
                                             "==================================");
            }

            MyAPIGateway.Utilities.ShowNotification("Check logs (%AppData%\\Roaming\\Space Engineers\\Storage\\AdvancedIndustry.log) for more info.", int.MaxValue, "Red");

            _hasShownApiFailMsg = true;
        }

        /// <summary>
        /// ModularPhysicalDefinition generator struct with dynamic collections & light validation
        /// <remarks>
        ///     ModularPhysicalDefinition uses fixed-size arrays; using them directly would be most inefficient.
        /// </remarks>
        /// </summary>
        private class DynamicAssemblyDef
        {
            public readonly string Name;
            public DefinitionType Type;
            public string[] MainFactoryBlock;
            public readonly HashSet<string> SubtypeIds;
            public readonly Dictionary<string, Dictionary<Vector3I, string[]>> AllowedConnections;

            public DynamicAssemblyDef(string name)
            {
                Name = name;
                SubtypeIds = new HashSet<string>();
                AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>();
                Type = DefinitionType.Pipe;
                MainFactoryBlock = null;
            }

            public DynamicAssemblyDef(string name, string[] mainFactoryBlock)
            {
                Name = name;
                SubtypeIds = new HashSet<string>();
                AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>();
                Type = DefinitionType.Factory;
                MainFactoryBlock = mainFactoryBlock;
            }

            public void AddPartDef(AssemblyBlockDefinition def)
            {
                foreach (var id in def.SubtypeIds)
                {
                    if (!SubtypeIds.Add(id))
                        Log.Info("DynamicAssemblyDef", $"WARN - Duplicate SubtypeId {id} in assembly {Name}!");

                    Dictionary<Vector3I, string[]> allowedConnections;
                    if (def.AllowedConnections.TryGetValue(Name, out allowedConnections))
                        AllowedConnections[id] = allowedConnections;
                }
            }

            public static explicit operator DefinitionDefs.ModularPhysicalDefinition(DynamicAssemblyDef def)
            {
                DefinitionDefs.ModularPhysicalDefinition mDef = new DefinitionDefs.ModularPhysicalDefinition
                {
                    Name = def.Name,
                    AllowedBlockSubtypes = def.SubtypeIds.ToArray(),
                    AllowedConnections = def.AllowedConnections
                };
                return mDef;
            }

            public enum DefinitionType
            {
                Pipe,
                Factory,
            }
        }
    }
}
