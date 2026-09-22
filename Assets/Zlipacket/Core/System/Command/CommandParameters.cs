using System.Collections.Generic;

namespace Zlipacket.Core.System.Command
{
    public class CommandParameters
    {
        private const char PARAMETER_IDENTIFIER = '-';
        
        private Dictionary<string, string> parameters = new();
        private List<string> unLabeledParameters = new();

        public CommandParameters(string[] parameterArray, int startingIndex = 0)
        {
            for (var i = startingIndex; i < parameterArray.Length; i++)
            {
                if (parameterArray[i].StartsWith(PARAMETER_IDENTIFIER) && !float.TryParse(parameterArray[i], out _))
                {
                    string pName = parameterArray[i];
                    string pValue = "";

                    if (i + 1 < parameterArray.Length && !parameterArray[i + 1].StartsWith(PARAMETER_IDENTIFIER))
                    {
                        pValue = parameterArray[i + 1];
                        i++;
                    }
                    
                    parameters.Add(pName, pValue);
                }
                else
                {
                    unLabeledParameters.Add(parameterArray[i]);
                }
            }
        }

        public bool TryGetValue<T>(string parameterName, out T value, T defaultValue = default(T)) => TryGetValue(new string[] { parameterName }, out value, defaultValue);
        
        public bool TryGetValue<T>(string[] parameterNames, out T value, T defaultValue = default(T))
        {
            foreach (var parameterName in parameterNames)
            {
                if (parameters.TryGetValue(parameterName, out string parameterValue))
                {
                    if (TryCastParameter(parameterName, out value))
                    {
                        return true;
                    }
                }
            }
            
            //If we reach here, no match was found by identifier so search for unlabeled one if available.
            foreach (var parameterName in unLabeledParameters)
            {
                if (TryCastParameter(parameterName, out value))
                {
                    unLabeledParameters.Remove(parameterName);
                    return true;
                }
            }
            
            value = defaultValue;
            return false;
        }

        private bool TryCastParameter<T>(string parameterValue, out T value)
        {
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(parameterValue, out bool boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(parameterValue, out int boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                if (float.TryParse(parameterValue, out float boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                value = (T)(object)parameterValue;
                return true;
            }
            
            value = default(T);
            return false;
        }
    }
}