using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts
{
    [Serializable]
    public class TrackSnapshot
    {
        [Serializable]
        public class Agent
        {
            public string id;
            public Location position;
        }

        [Serializable]
        public class  Location
        {
            public int x;
            public int y;
        }

        [Serializable]
        public enum Status
        {
            Active, Crushed, Interrupted
        }

        public Agent my_agent;
        public Agent[] other_agents;
        public Location[] obstacles;
        public string status;
    }

    [Serializable]
    public class Response
    {
        public string id;
        public string move;
    }

    public class TrackSnapshotMaker
    {
        public TrackSnapshotMaker(Track<GameEntity> track, Vector2Int searchRadius)
        {
            _track = track;
            _searchRadius = searchRadius;
        }

        public TrackSnapshot GetSnapshot(Player target, IEnumerable<Player> allPlayers)
        {
            Debug.Assert(_track.GetAt(target.Pos) == GameEntity.Player, $"Snapshot from {target.Pos.ToString()} requested but it's not a player.");
            TrackSnapshot snapshot = new TrackSnapshot()
            {
                my_agent = GetSnapshotAgent(target),
                other_agents = allPlayers.Where(p => p != target).Select(p => GetSnapshotAgent(p)).ToArray(),
                status = TrackSnapshot.Status.Active.ToString(),
                obstacles = GetObstacles(target.Pos)
            };

            return snapshot;
        }

        private TrackSnapshot.Agent GetSnapshotAgent(Player player)
        {
            return new TrackSnapshot.Agent() { id = player.Name, position = new TrackSnapshot.Location() { x = player.Pos.x, y = player.Pos.y } };
        }

        private TrackSnapshot.Location[] GetObstacles(Vector2Int pos)
        {
            List<TrackSnapshot.Location> locations = new List<TrackSnapshot.Location>();

            for (int x = Mathf.Max(0, pos.x - _searchRadius.x); x < Mathf.Min(_track.Width, pos.x + _searchRadius.x); x++)
            {
                for (int y = Mathf.Max(0, pos.y - _searchRadius.y); y < Mathf.Min(_track.Length, pos.y + _searchRadius.y); y++)
                {
                    if (_track.GetAt(x, y) == GameEntity.Traffic)
                    {
                        locations.Add(new TrackSnapshot.Location() { x = x, y = y });
                    }
                }
            }

            return locations.ToArray();
        }

        private readonly Track<GameEntity> _track;
        private readonly Vector2Int _searchRadius;
    }
}
