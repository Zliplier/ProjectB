namespace Zlipacket.Entity.Attribute
{
    public enum StatModType
    {
        Flat = 100, 
        PercentAdd = 200, 
        PercentMult = 300
    }
    
    public class StatModifier
    {
        public readonly object Source;
        public readonly StatModType StatModType;
        public readonly float Value;
        public readonly int Order;
        
        public StatModifier(float value, StatModType statModType, int order, object source)
        {
            StatModType = statModType;
            Value = value;
            Order = order;
            Source = source;
        }

        public StatModifier(float value, StatModType statModType) : this(value, statModType, (int)statModType, null) { }
        public StatModifier(float value, StatModType statModType, int order) : this(value, statModType, order, null) { }
        public StatModifier(float value, StatModType statModType, object source) : this(value, statModType, (int)statModType, source) { }
    }
}