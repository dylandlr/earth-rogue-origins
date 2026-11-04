using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Deterministic image-space diagnostics computed from final composited
    /// creature pixels, not anatomy envelopes or mocked scene artwork.
    /// </summary>
    public static class PixelCreatureVisualMetrics
    {
        [Serializable]
        public sealed class Result
        {
            public int nonTransparentPixels;
            public int dominantConnectedPixels;
            public int components;
            public int distinctOpaqueColors;
            public int minX,minY,maxX,maxY;
            public float dominantFraction;
            public float horizontalToVerticalRatio;
        }

        public static Result Analyze(Color32[] pixels,int width=128,int height=96)
        {
            if(pixels==null || pixels.Length!=width*height || width<1 || height<1)
                throw new ArgumentException("Invalid pixel array dimensions.");
            var result=new Result {
                minX=width,minY=height,maxX=-1,maxY=-1
            };
            var visited=new bool[pixels.Length];
            var queue=new int[pixels.Length];
            var colors=new HashSet<uint>();
            for(int index=0;index<pixels.Length;index++)
            {
                if(pixels[index].a==0)continue;
                int x=index%width,y=index/width;
                result.nonTransparentPixels++;
                result.minX=Math.Min(result.minX,x);
                result.maxX=Math.Max(result.maxX,x);
                result.minY=Math.Min(result.minY,y);
                result.maxY=Math.Max(result.maxY,y);
                var c=pixels[index];
                colors.Add(((uint)c.r<<24)|((uint)c.g<<16)|((uint)c.b<<8)|c.a);
                if(visited[index])continue;
                result.components++;
                int first=0,last=0;
                queue[last++]=index;visited[index]=true;
                while(first<last)
                {
                    int current=queue[first++];
                    int px=current%width,py=current/width;
                    int left=px>0?current-1:-1;
                    int right=px<width-1?current+1:-1;
                    int below=py>0?current-width:-1;
                    int above=py<height-1?current+width:-1;
                    if(left>=0 && !visited[left] && pixels[left].a>0)
                    {visited[left]=true;queue[last++]=left;}
                    if(right>=0 && !visited[right] && pixels[right].a>0)
                    {visited[right]=true;queue[last++]=right;}
                    if(below>=0 && !visited[below] && pixels[below].a>0)
                    {visited[below]=true;queue[last++]=below;}
                    if(above>=0 && !visited[above] && pixels[above].a>0)
                    {visited[above]=true;queue[last++]=above;}
                }
                result.dominantConnectedPixels=Math.Max(result.dominantConnectedPixels,last);
            }
            result.distinctOpaqueColors=colors.Count;
            if(result.nonTransparentPixels>0)
            {
                result.dominantFraction=result.dominantConnectedPixels/
                    (float)result.nonTransparentPixels;
                int paintedWidth=result.maxX-result.minX+1;
                int paintedHeight=result.maxY-result.minY+1;
                result.horizontalToVerticalRatio=paintedWidth/
                    (float)Math.Max(1,paintedHeight);
            }
            else
            {
                result.minX=result.minY=result.maxX=result.maxY=-1;
            }
            return result;
        }
    }
}
