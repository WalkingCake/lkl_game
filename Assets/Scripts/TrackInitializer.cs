using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Assets.Scripts
{
    public class TrackInitializer
    {
        public TrackInitializer(int seed, double trafficProbability) { 
            _rand = new Random(seed);
            _trafficProbability = trafficProbability;
        }

        public Track<GameEntity> CreateTrack(int width, int length)
        {
            GameEntity[][] track = null;
            InitTrack(ref track, width, length);
            int currDepth = FillStart(ref track, 0);
            FillLabirinth(ref track, currDepth);

            return new Track<GameEntity>(track);
        }

        private void InitTrack(ref GameEntity[][] track, int width, int length)
        {
            track = new GameEntity[width][];
            for (int x = 0; x < track.Length; x++)
            {
                track[x] = new GameEntity[length];
                for (int y = 0; y < length; y++)
                {
                    track[x][y] = GameEntity.Undef;
                }
            }
        }

        private int FillStart(ref GameEntity[][] track, int currDepth)
        {
            for (int x = 0; x < track.Length; x++)
            {
                track[x][currDepth] = x % 2 == 0 ? GameEntity.Free : GameEntity.Player;
            }

            currDepth++;
            for (int x = 0; x < track.Length; x++)
            {
                track[x][currDepth] = GameEntity.Free;
            }

            currDepth++;
            return currDepth;
        }

        private enum PathTurn
        {
            Left,
            Right,
            Forward
        }

        private void FillLabirinth(ref GameEntity[][] track, int currDepth)
        {
            int currCell = _rand.Next(track.Length);
            List<PathTurn> turns = new List<PathTurn>(3);
            PathTurn currTurn = PathTurn.Forward;

            while (currDepth < track[0].Length)
            {
                track[currCell][currDepth] = GameEntity.Free;
                turns.Clear();
                turns.Add(PathTurn.Forward);
                if (currTurn != PathTurn.Left && currCell < track.Length - 1) turns.Add(PathTurn.Right);
                if (currTurn != PathTurn.Right && currCell > 0) turns.Add(PathTurn.Left);
                currTurn = turns[_rand.Next(turns.Count)];

                switch (currTurn)
                {
                    case PathTurn.Left:
                        currCell--;
                        break;
                    case PathTurn.Right:
                        currCell++;
                        break;
                    case PathTurn.Forward:
                        for (int x = 0; x < track.Length; x++)
                        {
                            if (track[x][currDepth] == GameEntity.Undef)
                            {
                                track[x][currDepth] = _rand.NextDouble() < _trafficProbability ? GameEntity.Traffic : GameEntity.Free;
                            }
                        }
                        currDepth++;
                        break;
                }
            }
        }

        private Random _rand;
        private double _trafficProbability;
    }
}
