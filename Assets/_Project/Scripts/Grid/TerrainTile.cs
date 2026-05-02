using UnityEngine;

namespace HollowGround.Grid
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TerrainTile : MonoBehaviour
    {
        public TerrainType TerrainType;
        public int GridX;
        public int GridZ;
    }
}
