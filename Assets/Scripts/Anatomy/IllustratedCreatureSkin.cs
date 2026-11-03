using System;
using UnityEngine;

namespace EarthRogueOrigins.Anatomy
{
    /// <summary>
    /// Maps separately authored artwork onto the exact phenotype-driven rig.
    /// No movement/selection logic here: this is a cosmetic projection of the
    /// authoritative morphology. Mesh primitives remain as fallback.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArticulatedCreatureView))]
    public sealed class IllustratedCreatureSkin : MonoBehaviour
    {
        [SerializeField] CreatureArtLibrary art;
        ArticulatedCreatureView rig;
        GameObject skinRoot;
        bool initialized;

        public bool IsIllustrated { get { return initialized; } }

        public bool Apply(CreatureArtLibrary library)
        {
            rig=GetComponent<ArticulatedCreatureView>();
            if(rig==null || rig.Phenotype==null)throw new InvalidOperationException("Initialize anatomy rig before applying art.");
            if(skinRoot!=null)
            {
                if(Application.isPlaying)Destroy(skinRoot);
                else DestroyImmediate(skinRoot);
                skinRoot=null;
            }
            rig.SetPrimitiveVisible(true);
            initialized=false;
            art=library;
            if(art==null || !art.HasCoreParts)return false;

            var p=rig.Phenotype;
            skinRoot=new GameObject("Illustrated Anatomy");
            skinRoot.transform.SetParent(transform,false);
            var tint=art.baseTint;
            float headWidth=p.headRadius*2.5f;
            // Sort numbers refer to render stacking, not gameplay depth.
            RenderPart("Shadow",art.groundShadow,skinRoot.transform,
                new Vector3(0,-p.limbLength*.9f,.1f),p.trunkLength*1.35f,.16f,
                -50,Color.white);
            RenderPart("Tail",art.tail,rig.TailJoint,
                new Vector3(-p.tailLength*.49f,0,0),p.tailLength*1.15f,.25f,
                11,tint);
            if(art.tailFin!=null && p.tailFinSize>.025f)
                RenderPart("Tail Fin",art.tailFin,rig.TailJoint,
                    new Vector3(-p.tailLength*.97f,0,0),
                    Mathf.Max(.14f,p.tailFinSize*1.7f),p.tailFinSize*2.1f,
                    12,tint);

            for(int i=0;i<rig.LegJoints.Length;i++)
            {
                bool foreground=i%2==1;
                Sprite limb=i<2?art.hindlimb:art.forelimb;
                Transform anchor=rig.LegJoints[i];
                RenderPart("Leg "+i,limb,anchor,
                    new Vector3(0,-p.limbLength*.48f,0),
                    p.limbThickness*2.1f,p.limbLength*1.08f,
                    foreground?36:14,tint);
                if(art.foot!=null)
                    RenderPart("Foot "+i,art.foot,anchor,
                        new Vector3(.04f,-p.limbLength,0),
                        p.footWebbing>.5f?.27f:.17f,.13f,
                        foreground?37:15,tint);
            }

            RenderPart("Torso",art.torso,skinRoot.transform,
                Vector3.zero,p.trunkLength*1.1f,p.trunkDepth*2.15f,
                22,tint);
            if(art.torsoHighlight!=null)
            {
                Color highlight=Color.white;
                highlight.a=art.highlightOpacity;
                RenderPart("Torso highlights",art.torsoHighlight,skinRoot.transform,
                    new Vector3(-p.trunkLength*.08f,p.trunkDepth*.2f,0),
                    p.trunkLength*.94f,p.trunkDepth*1.5f,23,highlight);
            }
            RenderPart("Head",art.head,rig.HeadJoint,
                Vector3.zero,headWidth,p.headRadius*2.18f,41,tint);
            if(art.eye!=null)
                RenderPart("Eye",art.eye,rig.HeadJoint,
                    new Vector3(p.headRadius*.39f,p.headRadius*.27f,0),
                    p.eyeRadius*3f,p.eyeRadius*3f,43,Color.white);
            if(art.armorPlate!=null && rig.ArmorJoints!=null)
            {
                foreach(var anchor in rig.ArmorJoints)
                    RenderPart("Armored scute",art.armorPlate,anchor,
                        new Vector3(0,p.armorHeight*.43f,0),
                        .13f,p.armorHeight*1.1f,32,tint);
            }
            rig.SetPrimitiveVisible(false);
            initialized=true;
            return true;
        }

        static SpriteRenderer RenderPart(string name, Sprite sprite,Transform parent,
            Vector3 localPosition,float width,float height,int sortOrder,Color tint)
        {
            if(sprite==null)return null;
            var obj=new GameObject(name);
            obj.transform.SetParent(parent,false);
            obj.transform.localPosition=localPosition;
            var bounds=sprite.bounds.size;
            // Safe even if the artist supplies a tiny or incorrectly sized sprite.
            obj.transform.localScale=new Vector3(
                width/Mathf.Max(.001f,bounds.x),
                height/Mathf.Max(.001f,bounds.y),1f);
            var renderer=obj.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite;
            renderer.color=tint;
            renderer.sortingOrder=sortOrder;
            return renderer;
        }

        void OnDestroy()
        {
            // Generated parts are GameObjects parented under existing rig joints.
            // Unity will destroy them with the organism root.
        }
    }
}
