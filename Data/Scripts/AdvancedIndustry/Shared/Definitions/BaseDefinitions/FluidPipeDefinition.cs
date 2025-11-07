using ProtoBuf;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class FluidPipeDefinition : AssemblyBlockDefinition
    {
        [ProtoMember(1)] public float Storage;

        public override bool Verify(out string reason)
        {
            bool isValid = base.Verify(out reason);

            

            return isValid;
        }

    }
}
