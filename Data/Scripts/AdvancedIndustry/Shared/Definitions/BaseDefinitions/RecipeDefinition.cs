using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VRage.Game.ModAPI;

namespace AdvancedIndustry.Shared.Definitions.BaseDefinitions
{
    [ProtoContract]
    public class RecipeDefinition : DefinitionBase
    {
        /// <summary>
        /// Required input materials and quantities.
        /// </summary>
        [ProtoMember(1)] public Dictionary<RecipeMaterial, float> Inputs;
        /// <summary>
        /// Produced materials and quantities.
        /// </summary>
        [ProtoMember(2)] public Dictionary<RecipeMaterial, float> Outputs;
        /// <summary>
        /// Base process time, seconds.
        /// </summary>
        [ProtoMember(3)] public float ProcessTime;
        /// <summary>
        /// Optional environment restriction. Leave null for no restriction.
        /// </summary>
        [ProtoMember(4)] [DefaultValue(null)] public RecipeEnvironment? RequiredEnvironment;

        /// <summary>
        /// Allows custom recipe restrictions. Leave null if unused.<br/>
        /// In: Factory block, recipe subtype, input subtypes.<br/>
        /// Out: True if recipe is allowed.
        /// </summary>
        public Func<IMyCubeBlock, string, string[], bool> CustomRecipeRestriction = null;


        

        public override bool Verify(out string reason)
        {
            bool isValid = true;
            reason = "";

            if (Inputs == null || Inputs.Count == 0)
            {
                reason += "WARNING - No recipe inputs!\n";
            }

            if (Inputs == null || Outputs.Count == 0)
            {
                reason += "WARNING - No recipe outputs!\n";
            }

            return isValid;
        }

        protected override void AssignDelegates(Dictionary<string, Delegate> delegates)
        {
            AssignDelegate(delegates, "CustomRecipeRestriction", out CustomRecipeRestriction);
        }

        public override Dictionary<string, Delegate> GenerateDelegates()
        {
            return new Dictionary<string, Delegate>
            {
                ["CustomRecipeRestriction"] = CustomRecipeRestriction,
            };
        }

        [ProtoContract]
        public struct RecipeMaterial
        {
            [ProtoMember(1)] public string SubtypeId;
            [ProtoMember(2)] public MaterialType Type;

            public enum MaterialType
            {
                Fluid = 1,
                Item = 2,
                Gas = 3,
                Power = 4,
            }
        }

        [ProtoContract]
        public struct RecipeEnvironment
        {
            /// <summary>
            /// Optional planet restriction. Leave null for no restriction. Space is an empty string.
            /// </summary>
            [ProtoMember(1)] [DefaultValue(null)] public string[] PlanetWhitelist;

            /// <summary>
            /// Minimum atmospheric pressure, in atmospheres. Negative to ignore.
            /// </summary>
            [ProtoMember(2)] [DefaultValue(-1)] public float MinPressure;
            /// <summary>
            /// Maximum atmospheric pressure, in atmospheres. Negative to ignore.
            /// </summary>
            [ProtoMember(3)] [DefaultValue(-1)] public float MaxPressure;
            /// <summary>
            /// If false, only planetary atmospheres count for the above restrictions.
            /// </summary>
            [ProtoMember(4)] [DefaultValue(true)] public bool AllowArtificialAtmosphere;

            /// <summary>
            /// Minimum gravity, in Gs. Negative to ignore.
            /// </summary>
            [ProtoMember(5)] [DefaultValue(-1)] public float MinGravity;
            /// <summary>
            /// Maximum gravity, in Gs. Negative to ignore.
            /// </summary>
            [ProtoMember(6)] [DefaultValue(-1)] public float MaxGravity;
            /// <summary>
            /// If false, only planetary, thrust, and centripetal gravity count for the above restrictions.
            /// </summary>
            [ProtoMember(7)] [DefaultValue(true)] public bool AllowArtificialGravity;
        }
    }
}
