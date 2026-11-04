using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Diagnostic rasterizer for the unconstrained growth graph. This is not
    /// production skin or skeleton physics: every generated capsule is real
    /// output from the developmental phenotype, rendered at native pixels.
    /// </summary>
    public static class OpenMorphRasterizer
    {
        public const int Width=128,Height=96;

        static float DistanceSquared(Vector2 p,Vector2 a,Vector2 b)
        {
            Vector2 v=b-a;
            float denominator=Mathf.Max(.0001f,Vector2.Dot(v,v));
            float t=Mathf.Clamp01(Vector2.Dot(p-a,v)/denominator);
            Vector2 delta=p-(a+v*t);
            return Vector2.Dot(delta,delta);
        }

        public static Color32[] Render(OpenMorphGenome genome)
        {
            var image=new Color32[Width*Height];
            var phenotype=OpenMorphDevelopment.FitToCanvas(
                OpenMorphDevelopment.Grow(genome),Width,Height);
            foreach(var segment in phenotype)
            {
                int left=Mathf.Max(1,Mathf.FloorToInt(
                    Mathf.Min(segment.start.x,segment.end.x)-segment.radius-2));
                int right=Mathf.Min(Width-2,Mathf.CeilToInt(
                    Mathf.Max(segment.start.x,segment.end.x)+segment.radius+2));
                int low=Mathf.Max(1,Mathf.FloorToInt(
                    Mathf.Min(segment.start.y,segment.end.y)-segment.radius-2));
                int high=Mathf.Min(Height-2,Mathf.CeilToInt(
                    Mathf.Max(segment.start.y,segment.end.y)+segment.radius+2));
                byte pigment=(byte)Mathf.RoundToInt(
                    Mathf.Lerp(90f,170f,segment.pigment));
                var skin=new Color32(
                    (byte)Mathf.Min(255,pigment+18),
                    pigment,(byte)Mathf.Max(0,pigment-32),255);
                var contour=new Color32(28,34,26,255);
                float inner=Mathf.Max(0f,segment.radius-1.1f);
                float outer2=segment.radius*segment.radius;
                float inner2=inner*inner;
                for(int y=low;y<=high;y++)
                for(int x=left;x<=right;x++)
                {
                    float distance=DistanceSquared(new Vector2(x,y),
                        segment.start,segment.end);
                    if(distance>outer2)continue;
                    image[y*Width+x]=distance>inner2?contour:skin;
                }
            }
            return image;
        }
    }
}
