using AdvancedIndustry.Shared.Definitions.BaseDefinitions;
using AdvancedIndustry.Shared.ExternalAPI;
using AdvancedIndustry.Shared.Utils;
using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AdvancedIndustry.Shared.Definitions
{
    internal class DefinitionContainer<TDefinition> : IEnumerable<TDefinition> where TDefinition : DefinitionBase
    {
        private static DefinitionApi DefinitionApi => ApiManager.DefinitionApi;
        private static readonly string DefinitionTypeName = typeof(TDefinition).Name;

        private readonly Dictionary<int, TDefinition> _definitions = new Dictionary<int, TDefinition>();

        public TDefinition this[int definitionId]
        {
            get
            {
                TDefinition def;
                if (!_definitions.TryGetValue(definitionId, out def))
                    return null;
                return def;
            }
        }

        public TDefinition this[string definitionId]
        {
            get
            {
                TDefinition def;
                if (!_definitions.TryGetValue(definitionId.GetHashCode(), out def))
                    return null;
                return def;
            }
        }

        public IEnumerator<TDefinition> GetEnumerator() => _definitions.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _definitions.Values.GetEnumerator();

        public DefinitionContainer()
        {
            Log.Info("DefinitionContainer", $"Registering definition receiver for {DefinitionTypeName}...");
            Log.IncreaseIndent();
            DefinitionApi.RegisterOnUpdate<TDefinition>(OnDefinitionUpdate);
            foreach (string definitionId in DefinitionApi.GetDefinitionsOfType<TDefinition>())
                OnDefinitionUpdate(definitionId, 0);
            Log.DecreaseIndent();
            Log.Info("DefinitionContainer", $"Finished registering definition receiver for {DefinitionTypeName}.");
        }

        public void Close()
        {
            DefinitionApi.UnregisterOnUpdate<TDefinition>(OnDefinitionUpdate);
            Log.Info("DefinitionContainer", $"Closed receiver for {DefinitionTypeName}.");
        }

        private void OnDefinitionUpdate(string definitionId, int updateType)
        {
            try
            {
                // We're caching data because getting it from the API is inefficient.
                switch (updateType)
                {
                    case 0:
                        TDefinition definition;
                        if (!InitAndVerify(definitionId, out definition))
                            return;

                        _definitions[definition.Id] = definition;
                        if (!MyAPIGateway.Utilities.IsDedicated && definition is AssemblyBlockDefinition)
                            Client.Interface.BlockCategoryManager.RegisterFromDefinition(definition as AssemblyBlockDefinition);

                        Log.Info("DefinitionManager", $"Registered new {DefinitionTypeName} definition {definitionId} (internal ID {definition.Id})"); // TODO spawn new
                        break;
                    case 1:
                        _definitions.Remove(definitionId.GetHashCode()); // TODO cleanup existing
                        Log.Info("DefinitionManager", $"Unregistered {DefinitionTypeName} definition " + definitionId);
                        break;
                    case 2:
                        // Live methods
                        _definitions[definitionId.GetHashCode()].RetrieveDelegates<TDefinition>();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("DefinitionManager", ex);
            }
        }

        private static bool InitAndVerify(string definitionId, out TDefinition definition)
        {
            definition = DefinitionApi.GetDefinition<TDefinition>(definitionId);
            if (definition == null)
            {
                Log.Info(definitionId, "Definition null!");
                return false;
            }

            definition.Init<TDefinition>(definitionId);

            string reason;
            bool valid = definition.Verify(out reason);
            if (reason != "")
            {
                Log.Info("DefinitionManager",
                    valid
                        ? $"Potential issues were found with {DefinitionTypeName} {definitionId}:"
                        : $"Validation failed on {DefinitionTypeName} {definitionId}!");
                Log.IncreaseIndent();
                Log.Info(definitionId, reason.Trim());
                Log.DecreaseIndent();
            }
            
            if (valid)
                return true;

            Log.Info("DefinitionManager", $"Did not register {definitionId}.");
            MyAPIGateway.Utilities.ShowMessage("AdvancedIndustry", $"{DefinitionTypeName} {definitionId} failed validation! Check logs for more info.");
            return false;
        }
    }
}
