using System;
using UnityEngine;

namespace EarthRogueOrigins.Anatomy
{
    /// <summary>
    /// Author-authored 2.5D creature parts. Each Sprite is a transparent PNG with
    /// consistent painted light from upper-left and a dark silhouette contour.
    /// Parts can be updated independently without changing genome or ecology code.
    /// </summary>
    [CreateAssetMenu(fileName = "CreatureArtLibrary", menuName = "Earth Rogue Origins/Creature Art Library")]
    public sealed class CreatureArtLibrary : ScriptableObject
    {
        public Sprite torso;
        public Sprite torsoHighlight;
        public Sprite head;
        public Sprite eye;
        public Sprite forelimb;
        public Sprite hindlimb;
        public Sprite foot;
        public Sprite tail;
        public Sprite tailFin;
        public Sprite armorPlate;
        public Sprite groundShadow;
        public Sprite skinMarking;

        [Header("Color / materials")]
        public Color baseTint = Color.white;
        [Range(0f,1f)] public float highlightOpacity = .48f;
        [Range(0f,1f)] public float shadowOpacity = .36f;

        /// <summary>
        /// Ensures every authored asset is positioned using the same anchor contract.
        /// Sprites may have different dimensions; their pivot must be centered by import.
        /// </summary>
        public bool HasCoreParts
        {
            get { return torso != null && head != null && forelimb != null && hindlimb != null && tail != null; }
        }
    }
}
