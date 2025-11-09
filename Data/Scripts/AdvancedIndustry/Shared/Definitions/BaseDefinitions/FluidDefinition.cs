using ProtoBuf;
using System;
using System.Collections.Generic;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class FluidDefinition : DefinitionBase
    {
        [ProtoMember(1)] public string[] AllowedPipeTags;
        [ProtoMember(2)] public float Density;

        public override bool Verify(out string reason)
        {
            bool isValid = true;
            reason = "";

            if (AllowedPipeTags == null || AllowedPipeTags.Length == 0)
            {
                reason += "Null or empty AllowedPipeTags array!\n";
                isValid = false;
            }

            return isValid;
        }

        protected override void AssignDelegates(Dictionary<string, Delegate> delegates)
        {
            // no delegates
        }

        public override Dictionary<string, Delegate> GenerateDelegates()
        {
            // no delegates
            return null;
        }
    }
}
