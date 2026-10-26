using UnityEngine;

namespace Generation
{
    [CreateAssetMenu(menuName = "Scriptables/Generation/LevelGenerationData", fileName = "NewGenerationData")]
    public class LevelGenerationData : ScriptableObject
    {
        public int GenerationGridWidth;
        public int GenerationGridHeight;
        public int MinRoomCount;
        public float RoomCountIncrement;
        public bool UseStaticSeed;
        public int Seed;
        [Tooltip("The GridSize defines how big each of the used Prefabs is! Please make sure, this value is equal to the size in the x and z directions of the prefab you use!")]
        public float GridCellSize;
        public GenerationTileset Tileset;
    }
}