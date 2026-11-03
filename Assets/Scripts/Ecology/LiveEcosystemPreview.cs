using System;
using System.Collections.Generic;
using UnityEngine;
using EarthRogueOrigins.Ecology;
using EarthRogueOrigins.Anatomy;
using EarthRogueOrigins.Evolution;

namespace EarthRogueOrigins.Preview
{
    /// <summary>
    /// Standalone scene preview for the new ecosystem architecture.
    /// Dynamically visualizes the authoritative cell/creature simulation.
    /// It is intentionally independent from the earlier illustrated HUD.
    /// </summary>
    public sealed class LiveEcosystemPreview : MonoBehaviour
    {
        public int seed = 2046;
        public int worldWidth = 42, worldHeight = 28;
        public float secondsPerTick = .2f;
        public Sprite[] forestTiles, mudTiles, waterTiles, rockTiles, floodplainTiles;
        public Sprite[] organismSprites;
        public bool useAnatomicalSprites = true;
        readonly List<Sprite> createdSprites = new List<Sprite>();
        readonly List<Texture2D> createdTextures = new List<Texture2D>();
        public Camera worldCamera;

        EcosystemWorld world;
        readonly Dictionary<int,SpriteRenderer> creatureRenderers = new Dictionary<int,SpriteRenderer>();
        readonly List<GameObject> createdObjects = new List<GameObject>();
        float accumulator;
        bool paused;
        float clockSpeed = 1f;
        GUIStyle label, panel;

        void Start()
        {
            if(worldCamera==null)worldCamera=Camera.main;
            Restart();
        }

        void Restart()
        {
            foreach(var o in createdObjects)if(o!=null)Destroy(o);
            foreach(var sprite in createdSprites)if(sprite!=null)Destroy(sprite);
            foreach(var texture in createdTextures)if(texture!=null)Destroy(texture);
            createdSprites.Clear();createdTextures.Clear();
            createdObjects.Clear();creatureRenderers.Clear();
            world=new EcosystemWorld(worldWidth,worldHeight,seed);
            for(int y=0;y<worldHeight;y++)
            for(int x=0;x<worldWidth;x++)
            {
                var tile=world.Cell(x,y);
                var pool=Tiles(tile.terrain);
                Sprite sprite=pool!=null && pool.Length>0?pool[((x*13+y*17+seed)&int.MaxValue)%pool.Length]:null;
                // Asset-free fallback uses a simple square sprite if the generator has not run.
                if(sprite==null)continue;
                var go=new GameObject("Ground "+x+","+y);
                go.transform.SetParent(transform,false);
                go.transform.position=new Vector3(x+.5f,y+.5f,0);
                go.transform.localScale=Vector3.one*(1f/Mathf.Max(.05f,sprite.bounds.size.x));
                var renderer=go.AddComponent<SpriteRenderer>();
                renderer.sprite=sprite;renderer.sortingOrder=-100;
                renderer.color=new Color(1,1,1,.88f);
                createdObjects.Add(go);
            }
            var random=new System.Random(seed+17);
            for(int i=0;i<62;i++)
            {
                var diet=i<38?DietKind.Grazer:i<50?DietKind.Omnivore:
                    i<56?DietKind.Detritivore:DietKind.Predator;
                int species=diet==DietKind.Predator?2:diet==DietKind.Detritivore?3:1;
                world.AddFounder(new Vector2(2+random.Next(worldWidth-4),2+random.Next(worldHeight-4)),diet,species);
            }
            if(worldCamera!=null)
            {
                worldCamera.orthographic=true;
                worldCamera.orthographicSize=14.5f;
                worldCamera.transform.position=new Vector3(worldWidth*.5f,worldHeight*.5f,-10);
                worldCamera.backgroundColor=new Color(.10f,.16f,.14f);
                worldCamera.clearFlags=CameraClearFlags.SolidColor;
            }
            paused=false;accumulator=0;
        }

        Sprite[] Tiles(TerrainKind kind)
        {
            switch(kind)
            {
                case TerrainKind.Water: return waterTiles;
                case TerrainKind.Rock: return rockTiles;
                case TerrainKind.Mud: return mudTiles;
                case TerrainKind.Floodplain: return floodplainTiles;
                default: return forestTiles;
            }
        }

        void Update()
        {
            if(world==null)return;
            if(Input.GetKeyDown(KeyCode.Space))paused=!paused;
            if(Input.GetKeyDown(KeyCode.R))Restart();
            if(Input.GetKeyDown(KeyCode.Alpha1))clockSpeed=1;
            if(Input.GetKeyDown(KeyCode.Alpha2))clockSpeed=3;
            if(Input.GetKeyDown(KeyCode.Alpha3))clockSpeed=8;
            if(Input.GetKeyDown(KeyCode.P))world.temperatureOffset+=2f;
            if(Input.GetKeyDown(KeyCode.O))world.temperatureOffset-=2f;
            if(Input.GetKeyDown(KeyCode.C))world.atmosphericCO2=Mathf.Clamp01(world.atmosphericCO2+.1f);
            if(worldCamera!=null)
            {
                float move=14f*Time.unscaledDeltaTime;
                var t=worldCamera.transform;
                if(Input.GetKey(KeyCode.A))t.position+=Vector3.left*move;
                if(Input.GetKey(KeyCode.D))t.position+=Vector3.right*move;
                if(Input.GetKey(KeyCode.W))t.position+=Vector3.up*move;
                if(Input.GetKey(KeyCode.S))t.position+=Vector3.down*move;
                worldCamera.orthographicSize=Mathf.Clamp(worldCamera.orthographicSize-
                    Input.mouseScrollDelta.y*1.2f,4,28);
            }
            if(!paused)
            {
                accumulator+=Time.deltaTime*clockSpeed;
                int steps=0;
                while(accumulator>=secondsPerTick && steps++<15)
                {
                    world.Step(secondsPerTick);
                    accumulator-=secondsPerTick;
                }
            }
            UpdateCreatures();
        }

        void UpdateCreatures()
        {
            var active=new HashSet<int>();
            foreach(var c in world.creatures)
            {
                active.Add(c.id);
                SpriteRenderer renderer;
                if(!creatureRenderers.TryGetValue(c.id,out renderer))
                {
                    var go=new GameObject("Organism "+c.id+" G"+c.generation);
                    go.transform.SetParent(transform,false);
                    renderer=go.AddComponent<SpriteRenderer>();
                    renderer.sortingOrder=10;
                    creatureRenderers[c.id]=renderer;
                    createdObjects.Add(go);
                }
                if(renderer.sprite==null && useAnatomicalSprites && c.development!=null)
                {
                    var genome=new DiploidGenome();
                    Array.Copy(c.genes,genome.alleles,Math.Min(c.genes.Length,genome.alleles.Length));
                    var phenotype=Development.Express(c.development,genome);
                    var sprite=AnatomySpriteBaker.BakeSprite(phenotype,c.id+seed*17);
                    createdSprites.Add(sprite);
                    createdTextures.Add(sprite.texture);
                    renderer.sprite=sprite;
                }
                else if(renderer.sprite==null && organismSprites!=null && organismSprites.Length>0)
                {
                    int index=Mathf.Clamp(Mathf.RoundToInt(c.Trait(1)*5),0,5);
                    renderer.sprite=organismSprites[(index+Mathf.Max(0,c.speciesId-1)*6)%organismSprites.Length];
                }
                renderer.color=c.diet==DietKind.Predator?new Color(1,.68f,.6f):
                    c.diet==DietKind.Detritivore?new Color(.9f,.85f,1f):Color.white;
                renderer.transform.position=new Vector3(c.position.x,c.position.y,-.03f);
                renderer.transform.localScale=Vector3.one*(useAnatomicalSprites?1f:.78f);
            }
            var expired=new List<int>();
            foreach(var pair in creatureRenderers)
                if(!active.Contains(pair.Key)){if(pair.Value!=null)Destroy(pair.Value.gameObject);expired.Add(pair.Key);}
            foreach(int id in expired)creatureRenderers.Remove(id);
        }

        void OnGUI()
        {
            if(world==null)return;
            if(label==null)
            {
                label=new GUIStyle(GUI.skin.label) { fontSize=14,richText=true };
                label.normal.textColor=new Color(.96f,.91f,.80f);
                panel=new GUIStyle(GUI.skin.box);
            }
            var s=world.Snapshot();
            GUI.Box(new Rect(12,12,365,175),"EARTH: ROGUE ORIGINS · ECOSYSTEM LAB",panel);
            GUI.Label(new Rect(24,40,350,22),
                "Tick "+s.tick+"  |  Population "+s.population+"  |  Predators "+s.predatorCount,label);
            GUI.Label(new Rect(24,63,350,22),
                "Births "+s.births+"  |  Deaths "+s.deaths+"  |  Mean armor "+s.meanArmor.ToString("0.00"),label);
            GUI.Label(new Rect(24,86,350,22),
                "Plants "+s.totalPlants.ToString("0")+"  |  Detritus "+s.totalDetritus.ToString("0"),label);
            GUI.Label(new Rect(24,109,350,22),
                "Temperature offset "+world.temperatureOffset.ToString("+0;-0;0")+"C",label);
            GUI.Label(new Rect(24,135,355,42),
                "SPACE pause | 1/2/3 speed | WASD pan | Wheel zoom\nP/O heat/cool | C CO2 | R reset",label);
        }

        public EcosystemSnapshot CurrentSnapshot(){return world!=null?world.Snapshot():null;}
    }
}
