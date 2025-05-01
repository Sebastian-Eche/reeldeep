using UnityEngine;

namespace Junchen_Edit
{
    [CreateAssetMenu(fileName = "HabitatInfo", menuName = "Scriptable Objects/HabitatInfo")]
    public class HabitatInfo : ScriptableObject
    {
        public string habitatName;
        public HabitatDepth depthZone; // Which depth zone this habitat belongs to
        public Sprite icon;
        [TextArea]
        public string description;

        public enum HabitatDepth
        {
            Shallow,
            Mid,
            Deep
        }
    }
}