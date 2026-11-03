using System;
using UnityEngine;

namespace EarthRogueOrigins.PixelCreatures
{
    /// <summary>
    /// Generates plausible, explicitly fictional Latin-style binomials for
    /// the simulation. A binomial describes a founder population's inherited
    /// morphology; it is not an individual nickname or valid ICZN publication.
    /// Species IDs and registry membership, not superficial appearance,
    /// determine whether two organisms share a taxon.
    /// </summary>
    [Serializable]
    public sealed class ScientificTaxon
    {
        public int speciesId;
        public string genus;
        public string epithet;
        public string diagnosticTraits;
        public string Binomial { get { return genus+" "+epithet; } }
    }

    public static class ScientificNameGenerator
    {
        public static ScientificTaxon Describe(PixelCreatureRecipe recipe,int speciesId)
        {
            if(recipe==null)throw new ArgumentNullException(nameof(recipe));
            if(speciesId<0)throw new ArgumentOutOfRangeException(nameof(speciesId));
            recipe.Validate();

            string genus,epithet,reason;
            switch(recipe.plan)
            {
                case CreaturePlan.Fish:
                    genus=recipe.finSize>.6f?"Pterolimnus":"Limnophysis";
                    reason=recipe.finSize>.6f?"expanded swimming fins":"streamlined aquatic body";
                    bool masculine=genus=="Pterolimnus";
                    epithet=recipe.armor>.65f?(masculine?"loricatus":"loricata"):
                        recipe.tailLength>.65f?(masculine?"longicaudus":"longicauda"):
                        recipe.headStyle==PartStyle.LongSnout?
                            (masculine?"rostratus":"rostrata"):"palustris";
                    break;
                case CreaturePlan.Arthropod:
                    genus=recipe.armor>.55f?"Scutarthron":"Paludarthron";
                    reason=recipe.armor>.55f?"heavily segmented dorsal armor":"segmented marsh exoskeleton";
                    epithet=recipe.trunkLength>.7f?"elongatum":
                        recipe.palette==CreaturePalette.RustClay?"ferrugineum":"maculatum";
                    break;
                default:
                    genus=recipe.armor>.6f?"Scutoderma":
                        recipe.tailLength>.75f?"Caudolimnus":
                        recipe.headStyle==PartStyle.LongSnout?"Rostrolimnus":"Paludoderma";
                    reason=recipe.armor>.6f?"heavy dorsal armor":
                        recipe.tailLength>.75f?"elongated tail":
                        recipe.headStyle==PartStyle.LongSnout?"long rostrum":"broad low-slung trunk";
                    bool masculine=genus.EndsWith("limnus",StringComparison.Ordinal);
                    epithet=recipe.armor>.6f?(masculine?"armatus":"armatum"):
                        recipe.legLength>.7f?"longipes":
                        recipe.palette==CreaturePalette.RustClay?
                            (masculine?"ferrugineus":"ferrugineum"):
                        recipe.palette==CreaturePalette.PaleSand?
                            (masculine?"pallidus":"pallidum"):
                        recipe.spots>.55f?(masculine?"maculatus":"maculatum"):
                            (masculine?"olivaceus":"olivaceum");
                    break;
            }
            return new ScientificTaxon {
                speciesId=speciesId,genus=genus,epithet=epithet,
                diagnosticTraits=reason+"; "+Pigment(recipe.palette)+
                    "; "+(recipe.spots>.55f?"pronounced markings":"subtle markings")
            };
        }

        // Registry-only collision resolution. Keep the main descriptive
        // epithet; add a deterministic locality-like ending when two distinct
        // founding populations would otherwise have the identical binomial.
        public static ScientificTaxon Distinguish(ScientificTaxon original,int speciesId)
        {
            if(original==null)throw new ArgumentNullException(nameof(original));
            var copy=new ScientificTaxon {
                speciesId=speciesId,genus=original.genus,
                diagnosticTraits=original.diagnosticTraits
            };
            // Distinct positive IDs use a reversible lower-case alphabetic
            // suffix, avoiding digits or punctuation inside the binomial.
            uint value=unchecked((uint)speciesId)+1u;
            string token="";
            do
            {
                token=(char)('a'+value%26u)+token;
                value/=26u;
            }
            while(value!=0);
            copy.epithet=original.epithet+token+"ensis";
            return copy;
        }

        static string Pigment(CreaturePalette palette)
        {
            switch(palette)
            {
                case CreaturePalette.RustClay:return "rust pigmentation";
                case CreaturePalette.Slate:return "slate pigmentation";
                case CreaturePalette.PaleSand:return "pale pigmentation";
                case CreaturePalette.Moss:return "moss-colored pigmentation";
                case CreaturePalette.MarshTeal:return "teal pigmentation";
                default:return "olive pigmentation";
            }
        }
    }
}
