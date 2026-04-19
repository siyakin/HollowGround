using UnityEngine;

namespace HollowGround.Grid
{
    public class PlacementValidator : MonoBehaviour
    {
        [SerializeField] private GridSystem _gridSystem;

        public bool CanPlace(Vector3 worldPosition, int sizeX, int sizeZ)
        {
            var coords = _gridSystem.GetGridCoordinates(worldPosition);

            if (coords.x + sizeX > _gridSystem.Width || coords.y + sizeZ > _gridSystem.Height)
                return false;

            return _gridSystem.IsAreaBuildable(coords.x, coords.y, sizeX, sizeZ);
        }

        public bool CanPlace(int startX, int startZ, int sizeX, int sizeZ)
        {
            if (startX + sizeX > _gridSystem.Width || startZ + sizeZ > _gridSystem.Height)
                return false;

            return _gridSystem.IsAreaBuildable(startX, startZ, sizeX, sizeZ);
        }

        public Vector3 GetSnappedPosition(Vector3 worldPosition, int sizeX, int sizeZ)
        {
            var coords = _gridSystem.GetGridCoordinates(worldPosition);
            return _gridSystem.GetWorldPosition(coords.x, coords.y);
        }
    }
}
