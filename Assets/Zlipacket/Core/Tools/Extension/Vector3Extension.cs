using UnityEngine;

// See https://github.com/adammyhre/Unity-Utils for more extension methods
namespace Zlipacket.Core.Tools.Extension
{
    public static class Vector3Extension {
        /// <summary>
        /// Sets any x y z values of a Vector3
        /// </summary>
        public static Vector3 Insert(this Vector3 vector, float? x = null, float? y = null, float? z = null) {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }
    }
}