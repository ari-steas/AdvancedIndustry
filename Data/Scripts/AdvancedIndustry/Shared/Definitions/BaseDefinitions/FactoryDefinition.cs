using System;
using System.Collections.Generic;
using ProtoBuf;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class FactoryDefinition : AssemblyBlockDefinition
    {
        //[ProtoMember(1)] public string[] AssemblyTags; // TODO: PipeInput, PipeOutput, Pipe tags. Conveyor belts?
        [ProtoMember(2)] public string[] Recipes;
        [ProtoMember(3)] public float GlobalOutputMultiplier;
        /// <summary>
        /// Per-recipe output multipliers
        /// </summary>
        [ProtoMember(4)] public Dictionary<string, float> OutputMultipliers;
        [ProtoMember(5)] public float GlobalSpeedMultiplier;
        /// <summary>
        /// Per-recipe speed multipliers
        /// </summary>
        [ProtoMember(6)] public Dictionary<string, float> SpeedMultipliers;

        /// <summary>
        /// If true, automatically craft any available recipe.
        /// </summary>
        [ProtoMember(7)] public bool DoAutocraft;

        // TODO: Upgrade modules
        // only unlock certain recipes with given modules?

        public override bool Verify(out string reason)
        {
            bool isValid = base.Verify(out reason);

            

            return isValid;
        }
    }
}
