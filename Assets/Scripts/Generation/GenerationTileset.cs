using System.Collections.Generic;
using UnityEngine;

namespace Generation
{
    [CreateAssetMenu(menuName = "Scriptables/Generation/GenerationTileset", fileName = "NewTileset")]
    public class GenerationTileset : ScriptableObject
    {
        public List<Room> Rooms = new();
    }
}