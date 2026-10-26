using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LL_Unity_Utils.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;
using Object = System.Object;
using Random = System.Random;

namespace Generation
{
    // Breadth-First-Search Algorithmus
    public class LevelGenerator
    {
        readonly int generationWidth;
        
        readonly int generationHeight;
            
        readonly int minRoomCount;
        
        readonly float roomCountIncrementPerLevel;
        
        int seed;
        
        readonly int level;
        
        readonly bool debug;
        
        readonly bool useStaticSeed;
        
        readonly float cellSize;
        
        ObjectGrid<ERoomTypes> grid;
        
        int maxRoomCount => (int)Mathf.Min((generationWidth + generationHeight) * 0.25f, 60);
        
        Random random;
        
        List<Vector2Int> endRooms;


        public LevelGenerator(LevelGenerationData _data,int _level = 1, bool _debug = false)
        {
            generationWidth = _data.GenerationGridWidth;
            generationHeight = _data.GenerationGridHeight;
            minRoomCount = _data.MinRoomCount;
            roomCountIncrementPerLevel = _data.RoomCountIncrement;
            seed = _data.Seed;
            useStaticSeed = _data.UseStaticSeed;
            level = _level;
            debug = _debug;
            cellSize = _data.GridCellSize;
        }

        public ObjectGrid<ERoomTypes> GenerateLevel()
        {
            ResetSeed();
            return InitMap();
        }

        ObjectGrid<ERoomTypes> InitMap()
        {
            random = new Random(seed);
            endRooms = new List<Vector2Int>();
            grid = new ObjectGrid<ERoomTypes>(generationWidth, generationHeight, cellSize, new Vector3(0,0,0));
            GenerateLevelMap(level);
            if (debug) Debug.Log(GetLevelLog());
            return grid;
        }

        void GenerateLevelMap(int _level)
        {
            int roomCountToGenerate = (int)(random.Next(0, 2) + minRoomCount + Mathf.Min(_level * roomCountIncrementPerLevel, maxRoomCount));
            var center = new Vector2Int(generationWidth, generationHeight) / 2;
            bool isValid = false;
            const int maxIteration = 100;
            int currentIterations = 0;
            int rooms = 0;
            while (!isValid && currentIterations < maxIteration)
            {
                ResetMap();
                rooms = GenerateRooms(roomCountToGenerate, center);
                isValid = ValidateMap(roomCountToGenerate, rooms);
                ++currentIterations;
            }
            GenerateSpecialRooms();
        }
        
        void GenerateSpecialRooms()
        {
            var lastRoom = endRooms.Last();
            grid.SetValue(lastRoom, ERoomTypes.Boss);
        }

        bool ValidateMap(int _roomsToGenerate, int _generatedRooms)
        {
            return _generatedRooms == _roomsToGenerate && endRooms.Count >= 2;
        }

        int GenerateRooms(int _roomCountToGenerate, Vector2Int _center)
        {
            grid.SetValue(_center, ERoomTypes.Normal);
            
            Queue<Vector2Int> discoveryQueue = new Queue<Vector2Int>();
            discoveryQueue.Enqueue(_center);

            int generatedRooms = 1;

            while (discoveryQueue.Count > 0)
            {
                var currentCoord = discoveryQueue.Dequeue();
                var neighbourCoords = new[]
                {
                    currentCoord + Vector2Int.right,
                    currentCoord + Vector2Int.left,
                    currentCoord + Vector2Int.up,
                    currentCoord + Vector2Int.down,
                };
                bool hasGenerate = false;
                foreach (var currentNeighbourCoord in neighbourCoords)
                {
                    if (grid.IsOutsideBounds(currentNeighbourCoord)) continue;
                    if (generatedRooms >= _roomCountToGenerate) break;
                    if (grid.GetValue(currentNeighbourCoord) != ERoomTypes.Free) continue;
                    if (random.Next(0, 2) == 0) continue;
                    if (GetNeighbourCount(currentNeighbourCoord) > 1) continue;
                    
                    grid.SetValue(currentNeighbourCoord, ERoomTypes.Normal);
                    ++generatedRooms;
                    hasGenerate = true;
                    discoveryQueue.Enqueue(currentNeighbourCoord);
                }
                if (!hasGenerate) endRooms.Add(currentCoord);
            }
            return generatedRooms;
        }

        int GetNeighbourCount(Vector2Int _coord)
        {
            var neighbourCoords = new Vector2Int[]
            {
                _coord + Vector2Int.right,
                _coord + Vector2Int.left,
                _coord + Vector2Int.up,
                _coord + Vector2Int.down,
            };
            return neighbourCoords.Count(_currentCoord => grid.GetValue(_currentCoord) != ERoomTypes.Free);
        }

        void ResetMap()
        {
            endRooms.Clear();
            for (int y = 0; y < generationHeight; y++)
            {
                for (int x = 0; x < generationWidth; x++)
                {
                    grid.SetValue(x,y, ERoomTypes.Free);
                }
            }
        }

        void ResetSeed()
        {
            if (useStaticSeed && seed != 0) return;
            seed = DateTime.Now.Millisecond;
        }
        
        string GetLevelLog()
        {
            var stringBuilder = new StringBuilder();
            for (int y = 0; y < generationHeight; y++)
            {
                stringBuilder.Append("|");
                for (int x = 0; x < generationWidth; x++)
                {
                    int value = (int)grid.GetValue(x, y);
                    if (value >= 0) stringBuilder.Append(" ");
                    stringBuilder.Append(value.ToString() + "|");
                }

                stringBuilder.Append("\n");
            }

            return stringBuilder.ToString();
        }
    }
    
    
}