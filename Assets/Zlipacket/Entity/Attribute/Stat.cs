using System;
using System.Collections.Generic;

namespace Zlipacket.Entity.Attribute
{
    [Serializable]
    public class Stat
    {
        private float _baseValue;
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                CalculateStat();
            }
        }
        
        private float _value;
        public virtual float Value => _value;
        
        protected readonly List<StatModifier> statModifiers;
        public IReadOnlyCollection<StatModifier> StatModifiers => statModifiers.AsReadOnly();

        public Action<float> OnValueChanged = null;
        
        public Stat(float baseValue)
        {
            statModifiers = new List<StatModifier>();
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
            
            _value = (float)Math.Round(finalValue, 6);
            OnValueChanged?.Invoke(_value);
            return _value;
        }
        
        public virtual void AddModifier(StatModifier modifier)
        {
            statModifiers.Add(modifier);
            
            //Sort from lowest order to highest.
            statModifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
            CalculateStat();
        }

        public virtual bool RemoveModifier(StatModifier modifier)
        {
            if (statModifiers.Remove(modifier))
            {
                CalculateStat();
                return true;
            }
            return false;
        }

        public virtual void RemoveAllModifiers()
        {
            statModifiers.Clear();
            CalculateStat();
        }

        public virtual bool RemoveAllModifiersBySource(object source)
        {
            if (statModifiers.RemoveAll(mod => mod.Source == source) > 0)
            {
                CalculateStat();
                return true;
            }
            return false;
        }
    }
}