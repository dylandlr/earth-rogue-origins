using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using EarthRogueOrigins.Ecology;
using EarthRogueOrigins.Anatomy;
using EarthRogueOrigins.Evolution;
using EarthRogueOrigins.PixelCreatures;

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
        public Sprite[] fernProps;
        public bool useAnatomicalSprites = true;
        public bool useArticulatedRig = true;
        [Tooltip("Render original composited 128x96 pixel-art tetrapods; overrides articulated art.")]
        public bool usePixelCreatureSprites = true;
        readonly Dictionary<int,Sprite[]> pixelSprites = new Dictionary<int,Sprite[]>();
        readonly Dictionary<int,Sprite> groundShadows = new Dictionary<int,Sprite>();
        [Tooltip("When assigned, illustrated anatomical parts replace primitive rig meshes.")]
        public CreatureArtLibrary creatureArt;
        readonly Dictionary<int,ArticulatedCreatureView> articulatedViews = new Dictionary<int,ArticulatedCreatureView>();
        Material sharedRigMaterial;
        readonly Dictionary<int,string> organismMorphs = new Dictionary<int,string>();
        readonly Dictionary<int,Vector2> previousPositions = new Dictionary<int,Vector2>();
        readonly Dictionary<int,float> traveled = new Dictionary<int,float>();
        PhenotypeSpriteCache spriteCache;
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
            if(spriteCache!=null)spriteCache.Dispose();
            spriteCache=new PhenotypeSpriteCache(64);
            foreach(var entry in pixelSprites)DestroyPixelFrames(entry.Value);
            pixelSprites.Clear();
            foreach(var shadow in groundShadows.Values)DestroyPixelAsset(shadow);
            groundShadows.Clear();
            organismMorphs.Clear();previousPositions.Clear();traveled.Clear();articulatedViews.Clear();
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
            // Additional height-ordered foliage is visual only. It doesn't change
            // plant biomass, accessibility, or organism rewards.
            if(fernProps!=null && fernProps.Length>0)
            {
                for(int y=1;y<worldHeight-1;y++)
                for(int x=1;x<worldWidth-1;x++)
                {
                    var habitat=world.Cell(x,y);
                    if(habitat.terrain!=TerrainKind.Forest && habitat.terrain!=TerrainKind.Floodplain)
                        continue;
                    int hash=unchecked(x*73856093 ^ y*19349663 ^ seed*83492791);
                    if((hash & 7)>2)continue;
                    var sprite=fernProps[(hash & int.MaxValue)%fernProps.Length];
                    if(sprite==null)continue;
                    var prop=new GameObject("Fern "+x+","+y);
                    prop.transform.SetParent(transform,false);
                    // Sprite is 192 px wide at 192 PPU: 1 ground-tile footprint.
                    float scale=.62f+(hash & 3)*.11f;
                    prop.transform.localPosition=new Vector3(x+.5f,y+.57f,-.015f);
                    prop.transform.localScale=Vector3.one*scale;
                    var sr=prop.AddComponent<SpriteRenderer>();
                    sr.sprite=sprite;
                    // Plants sort by their ground anchor, not their crown height.
                    sr.sortingOrder=1500-Mathf.FloorToInt((y+.57f)*8f);
                    createdObjects.Add(prop);
                }
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
                    go.AddComponent<SortingGroup>();
                    renderer.sortingOrder=10;
                    creatureRenderers[c.id]=renderer;
                    createdObjects.Add(go);
                }
                if(usePixelCreatureSprites && c.development!=null)
                {
                    Sprite[] frames;
                    if(!pixelSprites.TryGetValue(c.id,out frames))
                    {
                        var genome=new DiploidGenome();
                        Array.Copy(c.genes,genome.alleles,
                            Math.Min(c.genes.Length,genome.alleles.Length));
                        var phenotype=Development.Express(c.development,genome);
                        var recipe=PixelRecipeAdapter.FromDevelopment(
                            c.development,phenotype,c.id+seed*17);
                        frames=new Sprite[PixelCreaturePoses.Frames*2];
                        for(int mode=0;mode<2;mode++)
                        for(int frame=0;frame<PixelCreaturePoses.Frames;frame++)
                        {
                            var texture=PixelCreaturePainter.Render(recipe,frame,mode==1);
                            frames[mode*PixelCreaturePoses.Frames+frame]=Sprite.Create(
                                texture,new Rect(0,0,texture.width,texture.height),
                                new Vector2(.5f,.5f),64f);
                        }
                        pixelSprites.Add(c.id,frames);
                        var shadowTexture=PixelCreaturePainter.TextureFromPixels(
                            PixelCreaturePainter.RenderGroundShadowPixels(recipe));
                        var shadowSprite=Sprite.Create(shadowTexture,
                            new Rect(0,0,shadowTexture.width,shadowTexture.height),
                            new Vector2(.5f,.5f),64f);
                        groundShadows[c.id]=shadowSprite;
                        var shadowObject=new GameObject("Contact Shadow");
                        shadowObject.transform.SetParent(renderer.transform,false);
                        var shadowRenderer=shadowObject.AddComponent<SpriteRenderer>();
                        shadowRenderer.sprite=shadowSprite;
                        shadowRenderer.sortingOrder=1;
                        previousPositions[c.id]=c.position;
                        traveled[c.id]=0f;
                    }
                    Vector2 old=previousPositions[c.id];
                    float moved=Vector2.Distance(old,c.position);
                    traveled[c.id]=traveled[c.id]+moved;
                    var habitat=world.Cell(Mathf.FloorToInt(c.position.x),
                        Mathf.FloorToInt(c.position.y));
                    bool swim=habitat.terrain==TerrainKind.Water;
                    int frameIndex=moved<.002f?0:
                        Mathf.FloorToInt(traveled[c.id]*7f)%PixelCreaturePoses.Frames;
                    renderer.sprite=frames[(swim?PixelCreaturePoses.Frames:0)+frameIndex];
                    renderer.enabled=true;
                    if(moved>.002f)renderer.flipX=c.position.x>old.x;
                    var shadowLayer=renderer.transform.Find("Contact Shadow");
                    if(shadowLayer!=null)
                    {
                        var shadowRenderer=shadowLayer.GetComponent<SpriteRenderer>();
                        if(shadowRenderer!=null)shadowRenderer.enabled=!swim;
                    }
                    previousPositions[c.id]=c.position;
                }
                else if(useArticulatedRig && c.development!=null)
                {
                    ArticulatedCreatureView rig;
                    if(!articulatedViews.TryGetValue(c.id,out rig))
                    {
                        var genome=new DiploidGenome();
                        Array.Copy(c.genes,genome.alleles,Math.Min(c.genes.Length,genome.alleles.Length));
                        var phenotype=Development.Express(c.development,genome);
                        rig=renderer.gameObject.AddComponent<ArticulatedCreatureView>();
                        if(sharedRigMaterial==null)
                        {
                            var shader=Shader.Find("Sprites/Default");
                            if(shader==null)shader=Shader.Find("Unlit/Color");
                            sharedRigMaterial=new Material(shader);
                        }
                        rig.Initialize(phenotype,sharedRigMaterial);
                        if(creatureArt!=null && creatureArt.HasCoreParts)
                        {
                            var skin=renderer.gameObject.AddComponent<IllustratedCreatureSkin>();
                            skin.Apply(creatureArt);
                        }
                        articulatedViews.Add(c.id,rig);
                        previousPositions[c.id]=c.position;
                    }
                    renderer.enabled=false;
                    Vector2 old=previousPositions[c.id];
                    float moved=Vector2.Distance(old,c.position);
                    var tile=world.Cell(Mathf.FloorToInt(c.position.x),Mathf.FloorToInt(c.position.y));
                    rig.SetMotion(moved,tile.terrain==TerrainKind.Water,moved>.002f,
                        moved>.002f?c.position.x<old.x:rig.transform.localScale.x<0);
                    previousPositions[c.id]=c.position;
                }
                else if(useAnatomicalSprites && c.development!=null)
                {
                    string key;
                    if(!organismMorphs.TryGetValue(c.id,out key))
                    {
                        var genome=new DiploidGenome();
                        Array.Copy(c.genes,genome.alleles,Math.Min(c.genes.Length,genome.alleles.Length));
                        var phenotype=Development.Express(c.development,genome);
                        key=spriteCache.Acquire(phenotype);
                        organismMorphs.Add(c.id,key);
                        previousPositions[c.id]=c.position;
                        traveled[c.id]=0f;
                    }
                    Vector2 previous;
                    if(!previousPositions.TryGetValue(c.id,out previous))previous=c.position;
                    float delta=Vector2.Distance(previous,c.position);
                    traveled[c.id]=traveled[c.id]+delta;
                    previousPositions[c.id]=c.position;
                    var cell=world.Cell(Mathf.FloorToInt(c.position.x),Mathf.FloorToInt(c.position.y));
                    bool swimming=cell.terrain==TerrainKind.Water;
                    int pose=delta<.002f?0:Mathf.FloorToInt(traveled[c.id]*4f)%PhenotypeSpriteCache.FrameCount;
                    renderer.sprite=spriteCache.Frame(key,swimming,pose);
                    if(delta>.002f)
                        renderer.flipX=c.position.x<previous.x;
                }
                else if(renderer.sprite==null && organismSprites!=null && organismSprites.Length>0)
                {
                    int index=Mathf.Clamp(Mathf.RoundToInt(c.Trait(1)*5),0,5);
                    renderer.sprite=organismSprites[(index+Mathf.Max(0,c.speciesId-1)*6)%organismSprites.Length];
                }
                renderer.color=c.diet==DietKind.Predator?new Color(1,.68f,.6f):
                    c.diet==DietKind.Detritivore?new Color(.9f,.85f,1f):Color.white;
                renderer.transform.position=new Vector3(c.position.x,c.position.y,-.03f);
                var sorting=renderer.GetComponent<SortingGroup>();
                if(sorting!=null)
                    sorting.sortingOrder=1500-Mathf.FloorToInt(c.position.y*8f);
                if(usePixelCreatureSprites)
                    renderer.transform.localScale=Vector3.one;
                else if(!useArticulatedRig)
                    renderer.transform.localScale=Vector3.one*(useAnatomicalSprites?1f:.78f);
            }
            var expired=new List<int>();
            foreach(var pair in creatureRenderers)
                if(!active.Contains(pair.Key)){if(pair.Value!=null)Destroy(pair.Value.gameObject);expired.Add(pair.Key);}
            foreach(int id in expired)
            {
                creatureRenderers.Remove(id);
                articulatedViews.Remove(id);
                Sprite[] pixelFrames;
                if(pixelSprites.TryGetValue(id,out pixelFrames))
                {
                    pixelSprites.Remove(id);
                    DestroyPixelFrames(pixelFrames);
                }
                Sprite shadow;
                if(groundShadows.TryGetValue(id,out shadow))
                {
                    groundShadows.Remove(id);
                    DestroyPixelAsset(shadow);
                }
                string key;
                if(organismMorphs.TryGetValue(id,out key))
                {
                    spriteCache.Release(key);
                    organismMorphs.Remove(id);
                }
                previousPositions.Remove(id);
                traveled.Remove(id);
            }
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

        void DestroyPixelAsset(Sprite sprite)
        {
            if(sprite==null)return;
            var texture=sprite.texture;
            Destroy(sprite);
            if(texture!=null)Destroy(texture);
        }

        void DestroyPixelFrames(Sprite[] frames)
        {
            if(frames==null)return;
            foreach(var sprite in frames)DestroyPixelAsset(sprite);
        }

        void OnDestroy()
        {
            foreach(var frames in pixelSprites.Values)
                DestroyPixelFrames(frames);
            pixelSprites.Clear();
            foreach(var shadow in groundShadows.Values)DestroyPixelAsset(shadow);
            groundShadows.Clear();
            if(spriteCache!=null)spriteCache.Dispose();
            spriteCache=null;
            if(sharedRigMaterial!=null)Destroy(sharedRigMaterial);
            sharedRigMaterial=null;
        }

        public EcosystemSnapshot CurrentSnapshot(){return world!=null?world.Snapshot():null;}
    }
}
