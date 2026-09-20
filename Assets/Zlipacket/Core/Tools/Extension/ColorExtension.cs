using UnityEngine;

namespace Zlipacket.Core.Tools.Extension
{
    public static class ColorExtension
    {
        public static Color Copy(this Color original) => new Color(original.r, original.g, original.b, original.a);
        
        public static Color SetAlpha(this Color original, float alpha) => new Color(original.r, original.g, original.b, alpha);

        public static Color GetColorFromName(string name)
        {
            switch (name.ToLower())
            {
                case "red":
                    return Color.red;
                case "green":
                    return Color.green;
                case "blue":
                    return Color.blue;
                case "yellow":
                    return Color.yellow;
                case "magenta":
                    return Color.magenta;
                case "cyan":
                    return Color.cyan;
                case "white":
                    return Color.white;
                case "black":
                    return Color.black;
                case "gray":
                    return Color.gray;
                case "orange":
                    return new Color(1f, 0.5f, 0f);
                default:
                    Debug.LogError($"Unrecognized color name: {name}");
                    return Color.clear;
            }
        }

        public static string ToHexRGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);
        public static string ToHexRGBA(this Color color) => ColorUtility.ToHtmlStringRGBA(color);
    }
}