using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Zlipacket.Core.Tools.Utilities
{
    public static class ZlipUtilities
    {
        public static bool CastMouseCickRaycast(UnityEngine.Camera cam, Vector2 mousePosition, out RaycastHit raycastHit)
        {
            raycastHit = new RaycastHit();
            
            Vector3 sceneMousePositionNear = new Vector3(
                mousePosition.x,
                mousePosition.y,
                cam.nearClipPlane);
            Vector3 sceneMousePositionFar = new Vector3(
                mousePosition.x,
                mousePosition.y,
                cam.farClipPlane);
            
            Vector3 worldMousePositionNear = cam.ScreenToWorldPoint(sceneMousePositionNear);
            Vector3 worldMousePositionFar = cam.ScreenToWorldPoint(sceneMousePositionFar);

            //Debug.DrawRay(worldMousePositionNear, worldMousePositionFar - worldMousePositionNear, Color.green, 1f);
            if (Physics.Raycast(worldMousePositionNear, worldMousePositionFar - worldMousePositionNear, out RaycastHit hit, float.PositiveInfinity))
            {
                raycastHit = hit;
                return true;
            }
            
            return false;
        }
        
        public static bool ApproximatelyWithMargin(float a, float b, float margin)
        {
            return Mathf.Abs(a - b) < margin;
        }
        
        /// <summary>
        /// Remap Distance of a vector3 between 2 vectors, then return interpolation/extrapolation.
        /// </summary>
        /// <param name="inputPos"></param>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <returns></returns>
        public static float RemapVector3Distance(Vector3 inputPos, Vector3 near, Vector3 far)
        {
            //No idea how dot product of these vectors work, but it works, so just leave it.
            return Vector3.Dot(inputPos - near, far - near) / Vector3.Dot(far - near, far - near);
        }
        
        /// <summary>
        /// Returns a random unit vector within a cone defined by a direction and an angle.
        /// </summary>
        /// <param name="coneDirection">The central axis direction of the cone (normalized).</param>
        /// <param name="angleDegrees">The maximum angle from the central axis (e.g., 10f for a 20 degree total arc).</param>
        /// <returns>A random unit vector within the cone.</returns>
        public static Vector3 GetRandomDirectionInCone(Vector3 coneDirection, float angleDegrees)
        {
            // 1. Get a random rotation around the cone's forward axis (random spin)
            Quaternion randomSpin = Quaternion.AngleAxis(Random.Range(0f, 360f), coneDirection);

            // 2. Get a random tilt/angle from the forward axis
            // We use Random.Range(0f, angleDegrees) to get an angle within the cone's limit
            Quaternion randomTilt = Quaternion.AngleAxis(Random.Range(0f, angleDegrees), Vector3.Cross(coneDirection, Vector3.up));

            // Note: If coneDirection is Vector3.up, the Cross product is zero. A safer way to get a perpendicular axis:
            Vector3 axis = Vector3.Cross(coneDirection, Vector3.right);
            if (axis == Vector3.zero) axis = Vector3.Cross(coneDirection, Vector3.up); // Fallback for edge case

            randomTilt = Quaternion.AngleAxis(Random.Range(0f, angleDegrees), axis);


            // 3. Combine the rotations and apply to the original direction
            Vector3 result = (randomSpin * randomTilt) * coneDirection;

            // Ensure the result is normalized (should be by quaternion math, but good practice)
            return result.normalized;
        }
        
        // Matches <mark=#RRGGBB> or <mark=#RRGGBBAA>, with optional spaces around '='
        private static readonly Regex markPattern = 
            new Regex(@"<mark\s*=\s*#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})>", RegexOptions.Compiled);
        /// <summary>
        /// Replaces <mark> tags at specific match indices with a new color.
        /// </summary>
        /// <param name="input">Original text containing TMP rich text tags</param>
        /// <param name="targetIndices">0-based indices of matches to replace</param>
        /// <param name="newColorHex">New color, e.g. "0000FFAA" (no '#')</param>
        /// <param name="maxIndex">Outputs the highest valid match index (totalMatches - 1), or -1 if no matches found</param>
        public static string ReplaceMarkColorsAtIndices(string input, int[] targetIndices, string newColorHex, out int maxIndex)
        {
            var targets = new HashSet<int>(targetIndices);
            int counter = 0;

            string result = markPattern.Replace(input, match =>
            {
                int currentIndex = counter;
                counter++;

                if (targets.Contains(currentIndex))
                {
                    return $"<mark=#{newColorHex}>";
                }
                return match.Value; // unchanged
            });

            maxIndex = counter - 1; // -1 if no matches were found (counter stays 0)
            return result;
        }
        
        /// <summary>
        /// Gets the hex color (without '#') of the <mark> tag at the given match index.
        /// </summary>
        /// <param name="input">Text containing TMP rich text tags</param>
        /// <param name="index">0-based match index to retrieve</param>
        /// <returns>Hex color string (e.g. "FF0000AA"), or null if index is out of range</returns>
        public static string GetMarkColor(string input, int index)
        {
            MatchCollection matches = markPattern.Matches(input);

            if (index < 0 || index >= matches.Count)
            {
                Debug.LogWarning($"GetMarkColor: index {index} out of range (found {matches.Count} matches).");
                return null;
            }

            return matches[index].Groups[1].Value;
        }
        
        public static void DeleteRuntimeFolder(string path)
        {
            // Define your targeted path
            string folderPath = path;

            // Verify the directory actually exists first
            if (Directory.Exists(folderPath))
            {
                // The 'true' parameter forces a recursive deletion (deletes all files and subfolders inside)
                Directory.Delete(folderPath, true);
                Debug.Log("Folder successfully deleted!");
            }
            else
            {
                Debug.LogWarning("Folder does not exist at path: " + folderPath);
            }
        }
        
        /// <summary>
        /// Returns a random point uniformly distributed within the given BoxCollider's volume, in world space.
        /// Accounts for the collider's local center/size offset as well as the transform's position, rotation, and scale.
        /// </summary>
        /// <param name="box">The BoxCollider to sample a point from.</param>
        /// <returns>A random world-space point inside the box collider.</returns>
        public static Vector3 GetRandomPointInBoxCollider(BoxCollider box)
        {
            Vector3 localPoint = new Vector3(
                Random.Range(-0.5f, 0.5f) * box.size.x,
                Random.Range(-0.5f, 0.5f) * box.size.y,
                Random.Range(-0.5f, 0.5f) * box.size.z
            ) + box.center;

            return box.transform.TransformPoint(localPoint);
        }
    }
}