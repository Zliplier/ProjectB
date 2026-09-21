using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Zlipacket.Entity.Stat
{
    [Serializable]
    public class EntityStat
    {
        public float BaseValue;

        public virtual float Value => CalculateStat();

        protected readonly List<StatModifier> statModifiers;
        public IReadOnlyCollection<StatModifier> StatModifiers => statModifiers.AsReadOnly();
        
        public EntityStat()
        {
            statModifiers = new List<StatModifier>();
        }
        
        public EntityStat(float baseValue) : this ()
        {
            BaseValue = baseValue;
        }
        
        public float CalculateStat()
        {
            float finalValue = 0f;
            float percentAdd = 0f;
            for (int i = 0; i < statModifiers.Count; i++)
            {
                StatModifier mod = statModifiers[i]; 
                
                switch (mod.StatModType)
                {
                    case StatModType.Flat:
                        finalValue += mod.Value;
                        break;
                    case StatModType.PercentAdd:
                        percentAdd += mod.Value;
                        if (i + 1 >= statModifiers.Count || statModifiers[i + 1].StatModType != StatModType.PercentAdd)
                        {
                            finalValue *= 1 + percentAdd;
                            percentAdd = 0;
                        }
                        break;
                    case StatModType.PercentMult:
                        finalValue *= 1 + mod.Value;
                        break;
                }
            }
            
            return (float)Math.Round(finalValue, 6);
        }
        
        public virtual void AddModifier(StatModifier modifier)
        {
            statModifiers.Add(modifier);
            
            //Sort from lowest order to highest.
            statModifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public virtual bool RemoveModifier(StatModifier modifier)
        {
            return statModifiers.Remove(modifier);
        }

        public virtual void RemoveAllModifiers()
        {
            statModifiers.Clear();
        }

        public virtual bool RemoveAllModifiersBySource(object source)
        {
            return statModifiers.RemoveAll(mod => mod.Source == source) > 0;
        }
    }
}