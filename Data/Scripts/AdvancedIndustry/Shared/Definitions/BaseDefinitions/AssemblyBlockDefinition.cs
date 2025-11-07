using System;
using System.Collections.Generic;
using ProtoBuf;
using VRageMath;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class AssemblyBlockDefinition : DefinitionBase
    {
        /// <summary>
        /// Determines what assemblies can connect to this block.
        /// </summary>
        [ProtoMember(100)] public string[] AssemblyTags;
        [ProtoMember(101)] public string[] SubtypeIds;
        [ProtoMember(102)] public Dictionary<string, Dictionary<Vector3I, string[]>> AllowedConnections;

        public override bool Verify(out string reason)
        {
            bool isValid = true;
            reason = "";

            if (AssemblyTags == null || AssemblyTags.Length == 0)
            {
                reason += "Invalid AssemblyTags array!\n";
                isValid = false;
            }

            if (SubtypeIds == null || SubtypeIds.Length == 0)
            {
                reason += "Invalid SubtypeId array!\n";
                isValid = false;
            }

            if (AllowedConnections == null || AllowedConnections.Count == 0)
            {
                reason += "Invalid AllowedConnections set!\n";
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
