using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EarthRogueOrigins.Anatomy
{
    /// <summary>
    /// Shared, reference-counted animation frames. Visual phenotype quantization
    /// is LOD-only; ecological phenotype remains full precision and authoritative.
    /// All Unity object allocation/destruction must happen on the main thread.
    /// </summary>
    public sealed class PhenotypeSpriteCache : IDisposable
    {
        public const int FrameCount = 4;
        public const int ModeCount = 2;
        public int Capacity { get; private set; }
        public int Count { get { return entries.Count; } }
        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        sealed class Entry
        {
            public string key;
            public Sprite[] sprites;
            public int references;
            public long touched;
        }

        readonly Dictionary<string,Entry> entries = new Dictionary<string,Entry>();
        long counter;
        bool disposed;

        public PhenotypeSpriteCache(int capacity = 64)
        {
            if(capacity<1)throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity=capacity;
        }

        /// <summary>
        /// Quantization intentionally limits displayed changes, not selection.
        /// Three shape tiers plus four hue tiers produce stable atlas identities.
        /// </summary>
        public static string VisualKey(AnatomicalPhenotype p)
        {
            if(!Development.IsValid(p))throw new ArgumentException("Invalid phenotype.");
            var b=new StringBuilder(30);
            b.Append(p.bodyPlan).Append(':');
            Add(b,p.trunkLength,.65f,1.35f,3);
            Add(b,p.trunkDepth,.29f,.62f,3);
            Add(b,p.headRadius,.14f,.30f,3);
            Add(b,p.tailLength,.28f,1.15f,3);
            Add(b,p.tailFinSize,0f,.33f,3);
            Add(b,p.limbLength,.19f,.70f,3);
            Add(b,p.limbThickness,.06f,.18f,3);
            Add(b,p.footWebbing,0f,1f,3);
            Add(b,p.armorCoverage,0f,1f,5);
            Add(b,p.armorHeight,.03f,.19f,3);
            Add(b,p.patternDensity,0f,1f,3);
            Add(b,p.hue,0f,1f,4);
            Add(b,p.eyeRadius,.018f,.055f,3);
            Add(b,p.neckLength,.04f,.22f,3);
            return b.ToString();
        }

        static void Add(StringBuilder b,float value,float min,float max,int levels)
        {
            float n=Mathf.Clamp01((value-min)/Mathf.Max(.0001f,max-min));
            b.Append((char)('0'+Mathf.Clamp(Mathf.RoundToInt(n*(levels-1)),0,levels-1)));
        }

        public string Acquire(AnatomicalPhenotype phenotype)
        {
            if(disposed)throw new ObjectDisposedException(nameof(PhenotypeSpriteCache));
            string key=VisualKey(phenotype);
            Entry e;
            if(entries.TryGetValue(key,out e))
            {
                e.references++;
                e.touched=++counter;
                HitCount++;
                return key;
            }
            EvictUnusedIfFull();
            var created=new Entry{key=key,references=1,touched=++counter,
                sprites=new Sprite[FrameCount*ModeCount]};
            int seed=StableHash(key);
            for(int mode=0;mode<ModeCount;mode++)
            for(int frame=0;frame<FrameCount;frame++)
            {
                float phase=frame*(Mathf.PI*2f/FrameCount);
                created.sprites[mode*FrameCount+frame]=
                    AnatomySpriteBaker.BakeSprite(phenotype,seed,phase,mode==1);
            }
            entries.Add(key,created);
            MissCount++;
            return key;
        }

        public Sprite Frame(string key,bool swimming,int frame)
        {
            Entry e;
            if(!entries.TryGetValue(key,out e))
                throw new KeyNotFoundException("Morphology was not acquired: "+key);
            e.touched=++counter;
            int index=(swimming?FrameCount:0)+((frame%FrameCount+FrameCount)%FrameCount);
            return e.sprites[index];
        }

        public void Release(string key)
        {
            Entry e;
            if(!entries.TryGetValue(key,out e))
                throw new KeyNotFoundException("Morphology was not acquired: "+key);
            if(e.references<=0)throw new InvalidOperationException("Unbalanced Release for "+key);
            e.references--;
        }

        void EvictUnusedIfFull()
        {
            if(entries.Count<Capacity)return;
            Entry oldest=null;
            foreach(var e in entries.Values)
                if(e.references==0 && (oldest==null || e.touched<oldest.touched))
                    oldest=e;
            if(oldest==null)return; // Never evict live organisms.
            DestroyEntry(oldest);
            entries.Remove(oldest.key);
        }

        static void DestroyEntry(Entry e)
        {
            foreach(var sprite in e.sprites)
            {
                if(sprite==null)continue;
                var texture=sprite.texture;
#if UNITY_EDITOR
                if(!Application.isPlaying)
                {
                    UnityEngine.Object.DestroyImmediate(sprite);
                    if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);
                }
                else
#endif
                {
                    UnityEngine.Object.Destroy(sprite);
                    if(texture!=null)UnityEngine.Object.Destroy(texture);
                }
            }
        }

        // FNV-1a over ASCII digits and separators. Does not depend on runtime string.GetHashCode().
        public static int StableHash(string key)
        {
            if(key==null)throw new ArgumentNullException(nameof(key));
            unchecked
            {
                uint h=2166136261;
                foreach(char c in key){h^=c;h*=16777619;}
                return (int)h;
            }
        }

        public void Dispose()
        {
            if(disposed)return;
            disposed=true;
            foreach(var e in entries.Values)DestroyEntry(e);
            entries.Clear();
        }
    }
}
