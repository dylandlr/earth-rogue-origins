using System;
using System.Collections.Generic;
using UnityEngine;
using EarthRogueOrigins.Anatomy;
using EarthRogueOrigins.Evolution;
using EarthRogueOrigins.PixelCreatures;

namespace EarthRogueOrigins.Ecology
{
    public enum TerrainKind { Water, Mud, Forest, Rock, Floodplain }
    public enum DietKind { Grazer, Predator, Omnivore, Detritivore }

    [Serializable]
    public sealed class HabitatCell
    {
        public TerrainKind terrain;
        public float elevation, moisture, temperature, fertility;
        public float plants, detritus, soilNutrients;
        public float waterDepth;
    }

    [Serializable]
    public sealed class EvolvingCreature
    {
        public int id, motherId = -1, fatherId = -1, generation;
        public int speciesId;
        public DietKind diet;
        public Vector2 position;
        public float energy = 70, health = 100, age, cooldown;
        public float[] genes = new float[12]; // six diploid loci
        public DevelopmentalGenome development;
        public bool Alive { get { return health > 0; } }
        public float Trait(int locus) { return Mathf.Clamp01((genes[2*locus] + genes[2*locus+1])*0.5f); }
    }

    [Serializable]
    public sealed class EcosystemSnapshot
    {
        public int tick, population, births, deaths, predatorCount;
        public float totalPlants, totalDetritus, totalNutrients;
        public float meanArmor, meanSpeed;
    }

    /// <summary>
    /// Seeded, fixed-step ecosystem. Each cell is authoritative for local resources;
    /// creatures are authoritative for energy, position, genotype, birth and death.
    /// Rendering must consume this state, never change it.
    /// </summary>
    public sealed class EcosystemWorld
    {
        public readonly int width, height, seed;
        public readonly HabitatCell[] cells;
        public readonly List<EvolvingCreature> creatures = new List<EvolvingCreature>();
        public readonly List<string> events = new List<string>();
        readonly SortedDictionary<int,ScientificTaxon> speciesCatalog =
            new SortedDictionary<int,ScientificTaxon>();
        public IEnumerable<ScientificTaxon> NamedSpecies { get { return speciesCatalog.Values; } }
        public ScientificTaxon GetSpecies(int speciesId)
        {
            ScientificTaxon species;
            if(!speciesCatalog.TryGetValue(speciesId,out species))
                throw new KeyNotFoundException("Unknown simulation species ID "+speciesId);
            return species;
        }
        public string ScientificName(EvolvingCreature creature)
        {
            if(creature==null)throw new ArgumentNullException(nameof(creature));
            return GetSpecies(creature.speciesId).Binomial;
        }
        public readonly SpeciationSettings speciationSettings=new SpeciationSettings();
        readonly SpeciationDetector speciationDetector=new SpeciationDetector();
        readonly System.Random random;
        readonly List<EvolvingCreature> newborns = new List<EvolvingCreature>();
        readonly List<EvolvingCreature> dead = new List<EvolvingCreature>();
        int nextId = 1;
        public int tick, births, deaths;
        public float atmosphericCO2 = 0.6f, temperatureOffset;

        public EcosystemWorld(int width, int height, int seed)
        {
            if (width < 4 || height < 4) throw new ArgumentOutOfRangeException("World dimensions must be >= 4");
            this.width = width; this.height = height; this.seed = seed;
            random = new System.Random(seed);
            cells = new HabitatCell[width*height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                // Smooth deterministic terrain without Unity's global RNG.
                float ridge = Mathf.Sin(x*0.21f+seed*.013f)*.23f + Mathf.Cos(y*.19f-seed*.017f)*.22f;
                float variance = (float)random.NextDouble()*.20f;
                float elevation = Mathf.Clamp01(.48f+ridge+variance);
                float moisture = Mathf.Clamp01(.83f-elevation*.68f + (float)random.NextDouble()*.17f);
                TerrainKind kind = elevation < .37f ? TerrainKind.Water :
                    elevation > .82f ? TerrainKind.Rock : moisture > .48f ? TerrainKind.Forest : TerrainKind.Mud;
                float fertility = kind == TerrainKind.Rock ? .1f : kind == TerrainKind.Water ? .25f : .5f+moisture*.45f;
                cells[y*width+x] = new HabitatCell {
                    elevation = elevation, moisture = moisture, fertility = fertility,
                    terrain = kind, waterDepth = Mathf.Max(0, .37f-elevation),
                    temperature = 28f, soilNutrients=30f+fertility*30f,
                    plants = kind == TerrainKind.Rock ? 2f : fertility*46f,
                    detritus = 5f
                };
            }
        }

        public HabitatCell Cell(int x, int y)
        {
            x = Mathf.Clamp(x, 0, width-1);
            y = Mathf.Clamp(y, 0, height-1);
            return cells[y*width+x];
        }

        public EvolvingCreature AddFounder(Vector2 location, DietKind diet, int speciesId = 0)
        {
            var c = new EvolvingCreature { id=nextId++, position=Clamp(location),
                diet=diet, speciesId=speciesId, age=3f };
            for (int i=0; i<c.genes.Length; i++)
                c.genes[i] = .25f+(float)random.NextDouble()*.5f;
            c.development=DevelopmentalGenome.Founder(random);
            if(!speciesCatalog.ContainsKey(speciesId))
            {
                var phenotype=Development.Express(c.development,ToGenome(c.genes));
                var recipe=PixelRecipeAdapter.FromDevelopment(c.development,phenotype,
                    unchecked(seed*37+speciesId));
                var taxon=ScientificNameGenerator.Describe(recipe,speciesId);
                // Taxonomic names are created once per species, never derived
                // anew for each individual or subsequent mutation.
                foreach(var existing in speciesCatalog.Values)
                    if(existing.Binomial==taxon.Binomial)
                    {
                        taxon=ScientificNameGenerator.Distinguish(taxon,speciesId);
                        break;
                    }
                speciesCatalog.Add(speciesId,taxon);
                events.Add("Species "+speciesId+" established: "+taxon.Binomial+
                    " ("+taxon.diagnosticTraits+")");
            }
            creatures.Add(c);
            return c;
        }

        public void Step(float dt = 0.2f)
        {
            if (dt <= 0 || dt > 1f) throw new ArgumentOutOfRangeException(nameof(dt));
            tick++;
            UpdateEnvironment(dt);
            newborns.Clear(); dead.Clear();
            foreach (var c in creatures)
            {
                if (!c.Alive) continue;
                c.age += dt*.025f;
                c.cooldown = Mathf.Max(0,c.cooldown-dt);
                var tile = Cell(Mathf.FloorToInt(c.position.x), Mathf.FloorToInt(c.position.y));
                float thermal = Mathf.Abs(tile.temperature-(25f+c.Trait(5)*7f));
                float metabolic = (.5f+(1f-c.Trait(2))*.6f)*(1f+.04f*thermal);
                if(c.development!=null)
                    metabolic*=Development.Express(c.development,ToGenome(c.genes)).upkeep;
                c.energy -= metabolic*dt;
                if (c.energy < 5) c.health -= dt*6f;
                if (c.age > 25f+c.Trait(5)*20f) c.health -= dt*3f;
                if (!c.Alive) { dead.Add(c); continue; }

                Vector2 target = FindTarget(c);
                var delta = target-c.position;
                float movementSpeed = 1.05f+c.Trait(0)*2f;
                if(c.development!=null)
                {
                    var phenotype=Development.Express(c.development,ToGenome(c.genes));
                    movementSpeed=tile.terrain==TerrainKind.Water?phenotype.swimmingSpeed:phenotype.terrestrialSpeed;
                }
                float terrainPenalty = tile.terrain == TerrainKind.Water ? .5f : 1f;
                Vector2 movement = delta.sqrMagnitude > .05f
                    ? delta.normalized*Mathf.Min(delta.magnitude, movementSpeed*terrainPenalty*dt) : Vector2.zero;
                c.position = Clamp(c.position+movement);
                c.energy -= movement.magnitude*.12f;
                tile = Cell(Mathf.FloorToInt(c.position.x),Mathf.FloorToInt(c.position.y));

                if (c.diet != DietKind.Predator)
                {
                    float eaten = Mathf.Min(tile.plants, (c.diet==DietKind.Grazer?7f:3.2f)*dt);
                    tile.plants -= eaten;
                    c.energy = Mathf.Min(100,c.energy+eaten*1.5f);
                    if (c.diet==DietKind.Detritivore)
                    {
                        float recycled=Mathf.Min(tile.detritus,5f*dt);
                        tile.detritus-=recycled; tile.soilNutrients+=recycled*.45f;
                        c.energy=Mathf.Min(100,c.energy+recycled*1.2f);
                    }
                }
                if (c.diet != DietKind.Grazer && c.diet != DietKind.Detritivore)
                    AttemptPredation(c, dt);
                if (c.energy > 77 && c.age > 2 && c.cooldown<=0)
                    TryBreed(c);
                if (!c.Alive) dead.Add(c);
            }

            // Capture all deaths, including animals killed by a predator after
            // their own turn has already executed in this simulation tick.
            dead.Clear();
            foreach (var c in creatures)
                if (!c.Alive) dead.Add(c);
            // Events are applied after the iteration to avoid invalidating enumerators.
            foreach (var c in dead)
            {
                var tile=Cell(Mathf.FloorToInt(c.position.x),Mathf.FloorToInt(c.position.y));
                tile.detritus=Mathf.Min(100,tile.detritus+15f);
                deaths++;
            }
            creatures.RemoveAll(c=>!c.Alive);
            if (creatures.Count + newborns.Count <= 600)
            {
                creatures.AddRange(newborns);
                births += newborns.Count;
            }
            if(speciationSettings.enabled &&
               speciationSettings.evaluationInterval>0 &&
               tick%speciationSettings.evaluationInterval==0)
                CheckSpeciation();
            if (tick % 100 == 0)
                events.Add("Tick "+tick+": "+creatures.Count+" organisms, "+births+" births, "+deaths+" deaths.");
            if (events.Count > 100) events.RemoveRange(0,events.Count-100);
        }

        /// <summary>
        /// Promotion changes only taxonomy, never physiology. The high-x
        /// population retains its actual genome, position, pedigree and diet.
        /// Previous names and ancestors remain in the catalog.
        /// </summary>
        void CheckSpeciation()
        {
            // Snapshot IDs to avoid modifying the catalog during enumeration.
            var speciesIds=new List<int>(speciesCatalog.Keys);
            foreach(int parentId in speciesIds)
            {
                var evidence=speciationDetector.Observe(
                    parentId,creatures,speciationSettings);
                if(evidence==null ||
                   evidence.observations<speciationSettings.requiredConsecutiveWindows)
                    continue;
                var candidates=new HashSet<int>(evidence.candidateIndividualIds);
                EvolvingCreature founder=null;
                foreach(var c in creatures)
                    if(candidates.Contains(c.id) &&
                       (founder==null || c.id<founder.id))
                        founder=c;
                if(founder==null)continue;
                int childId=0;
                foreach(var id in speciesCatalog.Keys)
                    childId=Math.Max(childId,id+1);
                var phenotype=Development.Express(
                    founder.development,ToGenome(founder.genes));
                var recipe=PixelRecipeAdapter.FromDevelopment(
                    founder.development,phenotype,
                    unchecked(seed*37+childId));
                var taxon=ScientificNameGenerator.Describe(recipe,childId);
                taxon.parentSpeciesId=parentId;
                taxon.foundedTick=tick;
                taxon.foundingGeneticDistance=evidence.geneticDistance;
                taxon.foundingGeographicGap=evidence.geographicGap;
                int disambiguation=0;
                while(NameUsed(taxon.Binomial))
                {
                    // A new numeric discriminator guarantees progress even
                    // when an earlier taxon uses the same generated suffix.
                    if(disambiguation++>10000)
                        throw new InvalidOperationException(
                            "Unable to assign a unique descendant scientific name.");
                    taxon=ScientificNameGenerator.Distinguish(
                        ScientificNameGenerator.Describe(recipe,childId),
                        checked(childId+disambiguation-1));
                }
                speciesCatalog.Add(childId,taxon);
                foreach(var c in creatures)
                    if(c.speciesId==parentId&&candidates.Contains(c.id))
                        c.speciesId=childId;
                // Both populations must retain at least the configured
                // viable count. The evaluator has already checked this.
                events.Add("Tick "+tick+": divergence observed: "+
                    taxon.Binomial+" branched from "+
                    speciesCatalog[parentId].Binomial+
                    " (distance="+evidence.geneticDistance.ToString("F3",
                        System.Globalization.CultureInfo.InvariantCulture)+
                    ", gap="+evidence.geographicGap.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)+
                    ", windows="+evidence.observations+").");
                speciationDetector.Clear();
            }
        }

        bool NameUsed(string name)
        {
            foreach(var t in speciesCatalog.Values)
                if(t.Binomial==name)return true;
            return false;
        }

        void UpdateEnvironment(float dt)
        {
            var updated = new float[cells.Length];
            for(int y=0;y<height;y++)
            for(int x=0;x<width;x++)
            {
                var c=Cell(x,y);
                c.temperature=28f+temperatureOffset-c.elevation*5f;
                float capacity=8f+c.fertility*(75f+atmosphericCO2*18f);
                float growth = .22f*c.plants*(1f-c.plants/capacity)
                    *Mathf.Clamp01(c.moisture*1.8f)*Mathf.Clamp01(c.soilNutrients/40f);
                float decomposition=Mathf.Min(c.detritus,dt*(.4f+c.moisture*.5f));
                c.detritus-=decomposition; c.soilNutrients+=decomposition*.65f;
                c.soilNutrients=Mathf.Max(0,c.soilNutrients-growth*dt*.06f);
                c.plants=Mathf.Clamp(c.plants+growth*dt,0,capacity);
                float neighbours=0; int n=0;
                if(x>0){neighbours+=Cell(x-1,y).soilNutrients;n++;}
                if(x<width-1){neighbours+=Cell(x+1,y).soilNutrients;n++;}
                if(y>0){neighbours+=Cell(x,y-1).soilNutrients;n++;}
                if(y<height-1){neighbours+=Cell(x,y+1).soilNutrients;n++;}
                updated[y*width+x]=Mathf.Max(0,c.soilNutrients+dt*.08f*(neighbours-n*c.soilNutrients));
            }
            for(int i=0;i<cells.Length;i++)cells[i].soilNutrients=updated[i];
        }

        Vector2 FindTarget(EvolvingCreature c)
        {
            int cx=Mathf.FloorToInt(c.position.x), cy=Mathf.FloorToInt(c.position.y);
            float best=-100000f; Vector2 target=c.position;
            for(int oy=-3;oy<=3;oy++)
            for(int ox=-3;ox<=3;ox++)
            {
                int x=cx+ox,y=cy+oy;
                if(x<0 || y<0 || x>=width || y>=height)continue;
                var cell=Cell(x,y);
                float food=c.diet==DietKind.Predator?0:
                    c.diet==DietKind.Detritivore?cell.detritus:cell.plants;
                if(c.diet==DietKind.Predator)
                    foreach(var other in creatures)
                        if(other.Alive && other.id!=c.id && other.diet!=DietKind.Predator &&
                            Vector2.Distance(other.position,new Vector2(x+.5f,y+.5f))<1.5f)
                            food+=80f;
                float jitter=(float)random.NextDouble()*.8f;
                float score=food - (Mathf.Abs(ox)+Mathf.Abs(oy))*3f
                    - (cell.terrain==TerrainKind.Water?12f:0f) + jitter;
                if(score>best){best=score;target=new Vector2(x+.5f,y+.5f);}
            }
            return target;
        }

        void AttemptPredation(EvolvingCreature attacker, float dt)
        {
            if(attacker.diet==DietKind.Omnivore && attacker.energy>35)return;
            EvolvingCreature victim=null;float distance=1.1f;
            foreach(var other in creatures)
            {
                if(!other.Alive || other.id==attacker.id ||
                    other.diet==DietKind.Predator || other.speciesId==attacker.speciesId)continue;
                float d=Vector2.Distance(attacker.position,other.position);
                if(d<distance){victim=other;distance=d;}
            }
            if(victim==null)return;
            float defense=victim.Trait(1)*.7f;
            if(victim.development!=null)
                defense=Development.Express(victim.development,ToGenome(victim.genes)).defense*.7f;
            float damage=(6f+attacker.Trait(0)*6f)*(1f-defense)*dt;
            victim.health-=damage;
            if(!victim.Alive) attacker.energy=Mathf.Min(100,attacker.energy+45f);
        }

        void TryBreed(EvolvingCreature parent)
        {
            if (creatures.Count+newborns.Count>=600)return;
            EvolvingCreature mate=null;
            foreach(var other in creatures)
            {
                if(other.id==parent.id || !other.Alive || other.speciesId!=parent.speciesId ||
                    other.diet!=parent.diet || other.age<2 || other.energy<65 || other.cooldown>0)continue;
                if(Vector2.Distance(parent.position,other.position)<1.8f){mate=other;break;}
            }
            if(mate==null)return;
            parent.cooldown=15f;mate.cooldown=15f;
            parent.energy-=20;mate.energy-=15;
            // Reproduction is not guaranteed; fecundity changes expected offspring count.
            if(random.NextDouble()>.25+.65*parent.Trait(3))return;
            var child=new EvolvingCreature { id=nextId++, position=Clamp(parent.position+new Vector2(.3f,.2f)),
                diet=parent.diet, speciesId=parent.speciesId, generation=Math.Max(parent.generation,mate.generation)+1,
                motherId=parent.id, fatherId=mate.id, age=0, energy=48, health=80, cooldown=20 };
            for(int locus=0;locus<6;locus++)
            {
                int i=locus*2;
                child.genes[i]=Mutate(parent.genes[i+random.Next(2)]);
                child.genes[i+1]=Mutate(mate.genes[i+random.Next(2)]);
            }
            child.development=DevelopmentalGenome.Child(parent.development,mate.development,random);
            newborns.Add(child);
        }

        float Mutate(float v)
        {
            if(random.NextDouble()<.018)v+=(float)(random.NextDouble()*2-1)*.16f;
            return Mathf.Clamp01(v);
        }

        Vector2 Clamp(Vector2 p)
        {
            return new Vector2(Mathf.Clamp(p.x,.01f,width-.01f),Mathf.Clamp(p.y,.01f,height-.01f));
        }

        static DiploidGenome ToGenome(float[] genes)
        {
            var genome=new DiploidGenome();
            Array.Copy(genes,genome.alleles,Math.Min(genes.Length,genome.alleles.Length));
            return genome;
        }

        public EcosystemSnapshot Snapshot()
        {
            var s=new EcosystemSnapshot{tick=tick,population=creatures.Count,births=births,deaths=deaths};
            foreach(var cell in cells) {s.totalPlants+=cell.plants;s.totalDetritus+=cell.detritus;s.totalNutrients+=cell.soilNutrients;}
            foreach(var c in creatures)
            {
                s.meanArmor+=c.Trait(1);s.meanSpeed+=c.Trait(0);
                if(c.diet==DietKind.Predator)s.predatorCount++;
            }
            if(s.population>0){s.meanArmor/=s.population;s.meanSpeed/=s.population;}
            return s;
        }
    }
}
