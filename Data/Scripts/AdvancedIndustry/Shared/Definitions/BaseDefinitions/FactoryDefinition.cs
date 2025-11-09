using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using VRageMath;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class FactoryDefinition : AssemblyBlockDefinition
    {
        //[ProtoMember(1)] public string[] AssemblyTags; // TODO: PipeInput, PipeOutput, Pipe tags. Conveyor belts?
        [ProtoMember(2)] public string[] Recipes;
        [ProtoMember(3)] [DefaultValue(1)] public float GlobalOutputMultiplier;
        /// <summary>
        /// Per-recipe output multipliers
        /// </summary>
        [ProtoMember(4)] public Dictionary<string, float> OutputMultipliers;
        [ProtoMember(5)] [DefaultValue(1)] public float GlobalSpeedMultiplier;
        /// <summary>
        /// Per-recipe speed multipliers
        /// </summary>
        [ProtoMember(6)] public Dictionary<string, float> SpeedMultipliers;

        /// <summary>
        /// If true, automatically craft any available recipe.
        /// </summary>
        [ProtoMember(7)] public bool DoAutocraft;

        [ProtoMember(8)] public Dictionary<string, Vector3I[]> PipeInputs;
        [ProtoMember(9)] public Dictionary<string, Vector3I[]> PipeOutputs;

        // TODO: Upgrade modules
        // only unlock certain recipes with given modules?

        public override bool Verify(out string reason)
        {
            base.Verify(out reason);
            bool isValid = true; // TODO

            if (AllowedConnections == null)
                AllowedConnections = new Dictionary<string, Dictionary<Vector3I, string[]>>();

            // assembly tags should always contain the main def tag
            if (AssemblyTags == null)
            {
                AssemblyTags = new [] {Name};

                // horrible jank to get modular assemblies to allow no connections
                AllowedConnections[Name] = new Dictionary<Vector3I, string[]> { [Vector3I.Forward] = new [] {""} };
            }
            else if (!AssemblyTags.Contains(Name))
            {
                var prevTags = AssemblyTags;
                AssemblyTags = new string[AssemblyTags.Length];
                Array.Copy(prevTags, AssemblyTags, prevTags.Length);
                AssemblyTags[AssemblyTags.Length - 1] = Name;

                // horrible jank to get modular assemblies to allow no connections
                AllowedConnections[Name] = new Dictionary<Vector3I, string[]> { [Vector3I.Forward] = new [] {""} };
            }

            // TODO verify sufficient output & input pipes for recipes

            return isValid;
        }
    }
}
