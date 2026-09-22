using System;
using UnityEngine;
using Zlipacket.Entity.Attribute;

namespace Gameplay.Character
{
    [Serializable]
    public class CharacterStats
    {
        public Stat health;
        
        public Stat strength;
        public Stat dexterity;
        public Stat constitution;
        public Stat intelligence;
        public Stat wisdom;
        public Stat agility;
        public Stat luck;
    }
}