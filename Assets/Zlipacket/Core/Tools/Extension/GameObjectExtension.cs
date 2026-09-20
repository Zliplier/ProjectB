using System.Collections.Generic;
using UnityEngine;

// See https://github.com/adammyhre/Unity-Utils for more extension methods
namespace Zlipacket.Core.Tools.Extension
{
    public static class GameObjectExtension {
        /// <summary>
        /// Returns the object itself if it exists, null otherwise.
        /// </summary>
        /// <remarks>
        /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
        /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
        /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
        /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object being checked.</param>
        /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
        
        public static List<GameObject> AllChilds(this GameObject root)
        {
            List<GameObject> result = new List<GameObject>();
            if (root.transform.childCount > 0)
            {
                foreach (Transform VARIABLE in root.transform)
                {
                    Searcher(result, VARIABLE.gameObject);
                }
            }
            return result;
        }

        public static void Searcher(List<GameObject> list, GameObject root)
        {
            list.Add(root);
            if (root.transform.childCount > 0)
            {
                foreach (Transform VARIABLE in root.transform)
                {
                    Searcher(list, VARIABLE.gameObject);
                }
            }
        }
        
        public static bool TryGetComponentInChildren<T>(this GameObject root, out T component, bool includeSelf = true, bool includeInactive = false) where T : class
        {
            component = null;

            if (includeSelf && (includeInactive || root.activeInHierarchy))
            {
                component = root.GetComponent<T>();
                if (component != null)
                    return true;
            }

            foreach (GameObject child in root.AllChilds())
            {
                if (!includeInactive && !child.activeInHierarchy)
                    continue;

                component = child.GetComponent<T>();
                if (component != null)
                    return true;
            }

            return false;
        }
    }
}