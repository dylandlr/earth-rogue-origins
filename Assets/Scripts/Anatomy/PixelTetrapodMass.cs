using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Heritable visual body-plan expression, not an archetype-specific sprite
    /// override. The same developmental values always produce the same mass,
    /// head integration and leg support stations.
    /// </summary>
    public static class PixelTetrapodMass
    {
        public struct Expression
        {
            public float squat;
            public int bodyDepth,headX,headWidth,legHeight;
            public float foreStation,rearStation,rootDepthRatio;
            public int jointWidth,jointHeight;
        }

        public static Expression Resolve(PixelCreatureRecipe r,int baseBodyDepth,
            int baseHeadWidth)
        {
            float styleMass=r.trunkStyle==PartStyle.Robust?1f:
                r.trunkStyle==PartStyle.Slender?0f:.65f;
            // Depth tunes an inherited mass phenotype; style establishes
            // the family, so a shallow robust animal stays recognizable.
            float squat=Mathf.Clamp01(styleMass*
                Mathf.Lerp(.86f,1f,r.trunkDepth));
            return new Expression {
                squat=squat,
                bodyDepth=baseBodyDepth+Mathf.RoundToInt(4f*squat),
                headX=30+Mathf.RoundToInt(2f*squat),
                headWidth=baseHeadWidth+Mathf.RoundToInt(3f*squat),
                legHeight=Mathf.RoundToInt(Mathf.Lerp(
                    Mathf.Lerp(10f,23f,r.legLength),
                    Mathf.Lerp(9f,17f,r.legLength),squat)),
                foreStation=Mathf.Lerp(.26f,.20f,squat),
                rearStation=Mathf.Lerp(.23f,.17f,squat),
                // Smaller ratio positions joint origins higher inside
                // the body rather than below the ventral contour.
                rootDepthRatio=Mathf.Lerp(.29f,.15f,squat),
                jointWidth=11+Mathf.RoundToInt(5f*squat),
                jointHeight=8+Mathf.RoundToInt(4f*squat)
            };
        }
    }
}
