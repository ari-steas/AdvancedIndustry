using ProtoBuf;
using System;
using System.Collections.Generic;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class FluidDefinition : DefinitionBase
    {
        public string[] AllowedPipeTags;
        public float Density;

        public override bool Verify(out string reason)
        {
            bool isValid = true;
            reason = "";

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
