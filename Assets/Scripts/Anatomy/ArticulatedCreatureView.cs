using System;
using System.Collections.Generic;
using UnityEngine;

namespace EarthRogueOrigins.Anatomy
{
    /// <summary>
    /// First articulated, texture-free phenotype renderer. Shapes are generated once per
    /// organism and limbs/tail are animated by Transform rotations at runtime.
    /// An illustrated component atlas can replace the primitive mesh art without
    /// changing this rig's attachment graph or gait contract.
    /// </summary>
    public sealed class ArticulatedCreatureView : MonoBehaviour
    {
        public AnatomicalPhenotype Phenotype { get; private set; }
        public Transform HeadJoint { get; private set; }
        public Transform TailJoint { get; private set; }
        public Transform[] LegJoints { get; private set; }
        public bool Swimming { get; private set; }
        public int AllocatedMeshes { get { return ownedMeshes.Count; } }

        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        Material material;
        Transform body;
        Transform[] armorJoints;
        float gaitPhase;
        bool facingLeft;
        bool initialized;

        public void Initialize(AnatomicalPhenotype phenotype, Material sharedMaterial = null)
        {
            if(initialized)throw new InvalidOperationException("Anatomy rig already initialized.");
            if(!Development.IsValid(phenotype))throw new ArgumentException("Invalid phenotype.");
            Phenotype=phenotype;
            material=sharedMaterial;
            if(material==null)
            {
                Shader shader=Shader.Find("Sprites/Default");
                if(shader==null)shader=Shader.Find("Unlit/Color");
                if(shader==null)throw new InvalidOperationException("No supported shader found for articulated view.");
                material=new Material(shader){name="EarthRogueOrigins/RuntimeMesh"};
                ownsMaterial=true;
            }

            var palette=Color.Lerp(new Color(.58f,.64f,.43f),new Color(.73f,.48f,.37f),phenotype.hue);
            Color outline=new Color(.12f,.15f,.12f,1);
            Color shadow=palette*.67f; shadow.a=1f;
            Color highlight=Color.Lerp(palette,Color.white,.22f);

            // The local +Y axis is dorsal; all attachment locations are inherited.
            body=Child("Torso",transform,new Vector3(0,0,0));
            Ellipse("Outline",body,Vector3.zero,phenotype.trunkLength*.55f,
                phenotype.trunkDepth*1.07f,outline,-.005f);
            Ellipse("Trunk",body,new Vector3(0,.018f,0),phenotype.trunkLength*.53f,
                phenotype.trunkDepth,shadow,-.011f);
            Ellipse("Paint",body,new Vector3(-.025f,phenotype.trunkDepth*.14f,0),
                phenotype.trunkLength*.48f,phenotype.trunkDepth*.79f,palette,-.017f);
            Ellipse("Highlight",body,new Vector3(-.10f,phenotype.trunkDepth*.47f,0),
                phenotype.trunkLength*.29f,phenotype.trunkDepth*.16f,highlight,-.019f);

            // Tail root and tip share one transform, so swimming motion never detaches the fin.
            TailJoint=Child("Tail root",transform,phenotype.tailRoot);
            Ellipse("Tail outline",TailJoint,new Vector3(-phenotype.tailLength*.48f,-.045f,0),
                phenotype.tailLength*.57f,.105f,outline,.007f);
            Ellipse("Tail",TailJoint,new Vector3(-phenotype.tailLength*.48f,-.04f,0),
                phenotype.tailLength*.55f,.072f,shadow,.002f);
            if(phenotype.tailFinSize>.025f)
            {
                Triangle("Tail fin outline",TailJoint,
                    new Vector2(-phenotype.tailLength*.94f,-.04f),
                    new Vector2(-phenotype.tailLength*1.17f,phenotype.tailFinSize),
                    new Vector2(-phenotype.tailLength*1.19f,-phenotype.tailFinSize),
                    outline,0);
                Triangle("Tail fin",TailJoint,
                    new Vector2(-phenotype.tailLength*.97f,-.04f),
                    new Vector2(-phenotype.tailLength*1.14f,phenotype.tailFinSize*.85f),
                    new Vector2(-phenotype.tailLength*1.16f,-phenotype.tailFinSize*.85f),
                    new Color(.35f,.67f,.64f,1),-.005f);
            }

            LegJoints=new Transform[4];
            // Hind legs rendered behind the torso; foreground limbs rendered in front.
            for(int i=0;i<4;i++)
            {
                bool front=i>=2;
                var anchor=front?phenotype.frontLeg:phenotype.rearLeg;
                anchor+=new Vector2(i%2==0?-.045f:.045f,i%2==0?.07f:-.07f);
                LegJoints[i]=Child((front?"Fore":"Hind")+"leg "+(i%2),transform,anchor);
                float layer=i%2==0?.025f:-.035f;
                float len=phenotype.limbLength;
                Ellipse("Limb contour",LegJoints[i],new Vector3(0,-len*.45f,0),
                    phenotype.limbThickness*.92f,len*.55f,outline,layer);
                Ellipse("Limb paint",LegJoints[i],new Vector3(0,-len*.44f,0),
                    phenotype.limbThickness*.70f,len*.52f,palette,layer-.005f);
                Ellipse("Foot",LegJoints[i],new Vector3(.045f,-len*.98f,0),
                    phenotype.footWebbing>.65f?.15f:.085f,
                    phenotype.footWebbing>.65f?.055f:.04f,outline,layer-.009f);
            }

            HeadJoint=Child("Head joint",transform,phenotype.head);
            Ellipse("Head contour",HeadJoint,Vector3.zero,phenotype.headRadius*1.27f,
                phenotype.headRadius*1.08f,outline,-.045f);
            Ellipse("Head paint",HeadJoint,new Vector3(.012f,.02f,0),
                phenotype.headRadius*1.17f,phenotype.headRadius*.95f,palette,-.05f);
            Ellipse("Eye",HeadJoint,new Vector3(phenotype.headRadius*.40f,
                phenotype.headRadius*.26f,0),phenotype.eyeRadius*1.4f,
                phenotype.eyeRadius*1.4f,outline,-.059f);
            Ellipse("Eye glint",HeadJoint,new Vector3(phenotype.headRadius*.43f,
                phenotype.headRadius*.31f,0),phenotype.eyeRadius*.48f,
                phenotype.eyeRadius*.48f,Color.white,-.064f);

            int plates=Mathf.RoundToInt(phenotype.armorCoverage*9);
            armorJoints=new Transform[plates];
            for(int i=0;i<plates;i++)
            {
                int index=Mathf.Clamp(Mathf.RoundToInt((i+1f)/(plates+1f)*16),1,15);
                Vector2 root=phenotype.dorsalSurface[index];
                Vector2 tangent=(phenotype.dorsalSurface[index+1]-
                    phenotype.dorsalSurface[index-1]).normalized;
                Vector2 normal=new Vector2(-tangent.y,tangent.x);
                var joint=Child("Dorsal plate "+i,transform,root);
                joint.up=normal; // World/local root rotation is identity on initialization.
                armorJoints[i]=joint;
                Triangle("Plate contour",joint,new Vector2(-.058f,0),
                    new Vector2(0,phenotype.armorHeight),
                    new Vector2(.058f,0),outline,-.035f);
                Triangle("Plate paint",joint,new Vector2(-.039f,.012f),
                    new Vector2(0,phenotype.armorHeight*.83f),
                    new Vector2(.039f,.012f),new Color(.72f,.67f,.50f,1),-.04f);
            }
            initialized=true;
        }

        bool ownsMaterial;

        public void SetMotion(float travelDistance, bool swimming, bool moving, bool headingLeft)
        {
            if(!initialized)return;
            Swimming=swimming;
            if(moving)gaitPhase+=Mathf.Max(0,travelDistance)*(swimming?9f:15f);
            float activity=moving?1f:0f;
            float swing=swimming?.20f:.46f;
            for(int i=0;i<LegJoints.Length;i++)
            {
                float diagonal=i==0||i==3?0f:Mathf.PI;
                float angle=Mathf.Sin(gaitPhase+diagonal)*swing*activity;
                LegJoints[i].localRotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg);
            }
            TailJoint.localRotation=Quaternion.Euler(0,0,
                Mathf.Sin(gaitPhase*(swimming?1f:.5f))*(swimming?19f:5f)*activity);
            HeadJoint.localRotation=Quaternion.Euler(0,0,
                Mathf.Sin(gaitPhase)*1.8f*activity);
            if(headingLeft!=facingLeft)
            {
                facingLeft=headingLeft;
                transform.localScale=new Vector3(facingLeft?-1f:1f,1f,1f);
            }
        }

        static Transform Child(string name,Transform parent,Vector3 localPosition)
        {
            var obj=new GameObject(name);
            obj.transform.SetParent(parent,false);
            obj.transform.localPosition=localPosition;
            return obj.transform;
        }

        void Ellipse(string name,Transform parent,Vector3 center,float radiusX,
            float radiusY,Color color,float z)
        {
            if(radiusX<=0||radiusY<=0)return;
            const int steps=24;
            var vertices=new Vector3[steps+1];
            var triangles=new int[steps*3];
            vertices[0]=new Vector3(center.x,center.y,z);
            for(int i=0;i<steps;i++)
            {
                float angle=2*Mathf.PI*i/steps;
                vertices[i+1]=new Vector3(center.x+Mathf.Cos(angle)*radiusX,
                    center.y+Mathf.Sin(angle)*radiusY,z);
                int t=i*3;triangles[t]=0;triangles[t+1]=i+1;triangles[t+2]=(i+1)%steps+1;
            }
            MeshPart(name,parent,vertices,triangles,color);
        }

        void Triangle(string name,Transform parent,Vector2 a,Vector2 b,Vector2 c,Color color,float z)
        {
            MeshPart(name,parent,new[]{new Vector3(a.x,a.y,z),new Vector3(b.x,b.y,z),
                new Vector3(c.x,c.y,z)},new[]{0,1,2},color);
        }

        void MeshPart(string name,Transform parent,Vector3[] vertices,int[] triangles,Color color)
        {
            var obj=Child(name,parent,Vector3.zero).gameObject;
            var mesh=new Mesh {name="ERO "+name};
            mesh.vertices=vertices;mesh.triangles=triangles;
            var uv=new Vector2[vertices.Length];
            var colors=new Color32[vertices.Length];
            for(int i=0;i<vertices.Length;i++)
            {
                colors[i]=color;
                uv[i]=new Vector2(vertices[i].x,vertices[i].y);
            }
            mesh.uv=uv;mesh.colors32=colors;
            mesh.RecalculateBounds();
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.sortingOrder=10;
            ownedMeshes.Add(mesh);
        }

        void OnDestroy()
        {
            foreach(var mesh in ownedMeshes)
                if(mesh!=null)
                {
                    if(Application.isPlaying)Destroy(mesh);
                    else DestroyImmediate(mesh);
                }
            ownedMeshes.Clear();
            if(ownsMaterial && material!=null)
            {
                if(Application.isPlaying)Destroy(material);
                else DestroyImmediate(material);
            }
        }
    }
}
