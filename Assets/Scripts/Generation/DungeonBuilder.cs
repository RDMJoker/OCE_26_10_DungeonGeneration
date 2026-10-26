using System;
using System.Collections.Generic;
using System.Linq;
using LL_Unity_Utils.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Generation
{
    public class DungeonBuilder : MonoBehaviour
    {
        Transform groupParent;


        List<GameObject> roomObjects = new();
        ObjectGrid<ERoomTypes> grid;
        GenerationTileset tileset;

        Room currentRoom;

        public void BuildDungeon(ObjectGrid<ERoomTypes> _levelGrid, GenerationTileset _tileset, Transform _groupParent, int _yOffset = 0)
        {
            grid = _levelGrid;
            tileset = _tileset;
            groupParent = _groupParent;
            roomObjects ??= new List<GameObject>();
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (grid.GetValue(x, y) == ERoomTypes.Free) continue;
                    var actualPosition = grid.GetWorldPosition(x, y);
                    var buildRoom = BuildRoom(new Vector2Int(x, y), grid.GetValue(x, y), new Vector2Int((int)actualPosition.x, (int)actualPosition.y), _yOffset);
                    roomObjects.Add(buildRoom);
                }
            }
        }

        GameObject BuildRoom(Vector2Int _position, ERoomTypes _roomType, Vector2Int _placingPosition, int _yOffset = 0)
        {
            Vector3 rotation;
            ERoomDoorType chosenDoorType;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            bool[] hasDirection = { false, false, false, false };

            for (int i = 0; i < directions.Length; i++)
            {
                if (grid.GetValue(_position + directions[i]) != ERoomTypes.Free)
                {
                    hasDirection[i] = true;
                }
            }

            bool hasNorth = hasDirection[0];
            bool hasEast = hasDirection[1];
            bool hasSouth = hasDirection[2];
            bool hasWest = hasDirection[3];

            int doorAmount = GetNeighbourCount(_position);
            (rotation, chosenDoorType) = GetRotationAndDoorType(doorAmount, hasNorth, hasEast, hasSouth, hasWest);
            var correctRoomType = tileset.Rooms.Find(_room => _room.DoorType == chosenDoorType && _room.RoomType == _roomType).gameObject;
            if (correctRoomType == null) throw new ArgumentException("No valid room prefab found!");
            return Instantiate(correctRoomType, new Vector3(_placingPosition.x, -_yOffset, _placingPosition.y), Quaternion.Euler(rotation), groupParent);
        }

        (Vector3 rotation, ERoomDoorType doorType) GetRotationAndDoorType(int _doorAmount, bool _hasNorth, bool _hasEast, bool _hasSouth, bool _hasWest)
        {
            return _doorAmount switch
            {
                1 => GetSingleDoorRotation(_hasNorth, _hasEast, _hasSouth, _hasWest),
                2 => GetDoubleDoorRotation(_hasNorth, _hasEast, _hasSouth, _hasWest),
                3 => GetTripleDoorRotation(_hasNorth, _hasEast, _hasSouth, _hasWest),
                4 => (new Vector3(0, 0, 0), ERoomDoorType.Full),
                _ => throw new ArgumentException("Invalid door amount!")
            };
        }

        (Vector3 rotation, ERoomDoorType doorType) GetSingleDoorRotation(bool _hasNorth, bool _hasEast, bool _hasSouth, bool _hasWest)
        {
            if (_hasNorth) return (new Vector3(0, 0, 0), ERoomDoorType.North);
            if (_hasEast) return (new Vector3(0, 90, 0), ERoomDoorType.North);
            if (_hasSouth) return (new Vector3(0, 180, 0), ERoomDoorType.North);
            if (_hasWest) return (new Vector3(0, -90, 0), ERoomDoorType.North);
            throw new ArgumentException("Invalid door configuration!");
        }

        (Vector3 rotation, ERoomDoorType doorType) GetDoubleDoorRotation(bool _hasNorth, bool _hasEast, bool _hasSouth, bool _hasWest)
        {
            if (_hasNorth && _hasEast) return (new Vector3(0, 0, 0), ERoomDoorType.NorthEast);
            if (_hasNorth && _hasSouth) return (new Vector3(0, 0, 0), ERoomDoorType.NorthSouth);
            if (_hasNorth && _hasWest) return (new Vector3(0, -90, 0), ERoomDoorType.NorthEast);
            if (_hasEast && _hasWest) return (new Vector3(0, 90, 0), ERoomDoorType.NorthSouth);
            if (_hasEast && _hasSouth) return (new Vector3(0, 90, 0), ERoomDoorType.NorthEast);
            if (_hasSouth && _hasWest) return (new Vector3(0, 180, 0), ERoomDoorType.NorthEast);
            throw new ArgumentException("Invalid door configuration!");
        }

        (Vector3 rotation, ERoomDoorType doorType) GetTripleDoorRotation(bool _hasNorth, bool _hasEast, bool _hasSouth, bool _hasWest)
        {
            if (_hasNorth && _hasEast && _hasWest) return (new Vector3(0, 0, 0), ERoomDoorType.NorthEastWest);
            if (_hasNorth && _hasEast && _hasSouth) return (new Vector3(0, 90, 0), ERoomDoorType.NorthEastWest);
            if (_hasEast && _hasSouth && _hasWest) return (new Vector3(0, 180, 0), ERoomDoorType.NorthEastWest);
            if (_hasNorth && _hasSouth && _hasWest) return (new Vector3(0, -90, 0), ERoomDoorType.NorthEastWest);
            throw new ArgumentException("Invalid door configuration!");
        }


        int GetNeighbourCount(Vector2Int _position)
        {
            var neighbourCoords = new Vector2Int[]
            {
                _position + Vector2Int.right,
                _position + Vector2Int.left,
                _position + Vector2Int.up,
                _position + Vector2Int.down,
            };

            return neighbourCoords.Count(_currentPos => grid.GetValue(_currentPos) != ERoomTypes.Free);
        }

        [Button]
        public void ResetDungeon()
        {
            if (roomObjects.Count == 0) return;
            foreach (var roomObject in roomObjects)
            {
                DestroyImmediate(roomObject.gameObject);
            }

            roomObjects.Clear();
        }
    }
}