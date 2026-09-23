using UnityEngine;

namespace Gameplay.Character
{
    [CreateAssetMenu(menuName = "Characters/Character Data", fileName = "New Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Data")]
        public string id;
        public string displayName;
        
        [Header("Stats")]
        public CharacterStats stats;
        
        [Header("Combat")]
        public GameObject combatPrefab;
    }
}