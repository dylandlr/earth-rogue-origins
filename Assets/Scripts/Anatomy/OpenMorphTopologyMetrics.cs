using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Read-only, shape-first tissue diagnostics. Distinguishes changes in
    /// area from topology, boundary clipping, branching and shape compactness.
    /// These are observations, never selection objectives or viability scores.
    /// </summary>
    public static class OpenMorphTopologyMetrics
    {
        public struct Result
        {
            public int occupied,components,holes,perimeter,borderCells;
            public int minX,minY,maxX,maxY;
            public int bboxWidth,bboxHeight;
            public float compactness,fillFraction;
        }

        public static Result Analyze(OpenMorphTissue.Field field)
        {
            if(field==null)throw new ArgumentNullException(nameof(field));
            int w=field.width,h=field.height;
            var present=new bool[w*h];
            int minX=w,minY=h,maxX=-1,maxY=-1;
            var result=new Result();
            for(int y=0;y<h;y++)
            for(int x=0;x<w;x++)
            {
                int i=y*w+x;
                if(field.cells[i].fate==OpenMorphTissue.Fate.Empty)continue;
                present[i]=true;result.occupied++;
                minX=Math.Min(x,minX);maxX=Math.Max(x,maxX);
                minY=Math.Min(y,minY);maxY=Math.Max(y,maxY);
                if(x==0 || y==0 || x==w-1 || y==h-1)
                    result.borderCells++;
                if(x==0 || !present[i-1])result.perimeter++;
                // Right and bottom are not scanned yet; inspect the
                // authoritative occupancy state directly.
                if(x==w-1 || field.cells[i+1].fate==OpenMorphTissue.Fate.Empty)
                    result.perimeter++;
                if(y==0 || field.cells[i-w].fate==OpenMorphTissue.Fate.Empty)
                    result.perimeter++;
                if(y==h-1 || field.cells[i+w].fate==OpenMorphTissue.Fate.Empty)
                    result.perimeter++;
            }
            result.minX=result.occupied==0?-1:minX;
            result.minY=result.occupied==0?-1:minY;
            result.maxX=maxX;result.maxY=maxY;
            result.bboxWidth=result.occupied==0?0:maxX-minX+1;
            result.bboxHeight=result.occupied==0?0:maxY-minY+1;
            result.fillFraction=result.occupied==0?0f:
                result.occupied/(float)(result.bboxWidth*result.bboxHeight);
            result.compactness=result.perimeter==0?0f:
                4f*Mathf.PI*result.occupied/
                (result.perimeter*(float)result.perimeter);
            var seen=new bool[present.Length];
            int[] offsets={-1,1,-w,w};
            var queue=new Queue<int>();
            for(int start=0;start<present.Length;start++)
            {
                if(!present[start] || seen[start])continue;
                result.components++;
                seen[start]=true;queue.Enqueue(start);
                while(queue.Count>0)
                {
                    int i=queue.Dequeue();
                    int x=i%w,y=i/w;
                    for(int n=0;n<4;n++)
                    {
                        int nx=x+(n==0?-1:n==1?1:0);
                        int ny=y+(n==2?-1:n==3?1:0);
                        if(nx<0||ny<0||nx>=w||ny>=h)continue;
                        int j=ny*w+nx;
                        if(!present[j]||seen[j])continue;
                        seen[j]=true;queue.Enqueue(j);
                    }
                }
            }
            // A hole is an empty four-connected component that never
            // touches the world boundary.
            Array.Clear(seen,0,seen.Length);
            for(int start=0;start<present.Length;start++)
            {
                if(present[start]||seen[start])continue;
                bool border=false;
                seen[start]=true;queue.Enqueue(start);
                while(queue.Count>0)
                {
                    int i=queue.Dequeue(),x=i%w,y=i/w;
                    if(x==0||y==0||x==w-1||y==h-1)border=true;
                    for(int n=0;n<4;n++)
                    {
                        int nx=x+(n==0?-1:n==1?1:0);
                        int ny=y+(n==2?-1:n==3?1:0);
                        if(nx<0||ny<0||nx>=w||ny>=h)continue;
                        int j=ny*w+nx;
                        if(present[j]||seen[j])continue;
                        seen[j]=true;queue.Enqueue(j);
                    }
                }
                if(!border)result.holes++;
            }
            return result;
        }
    }
}
