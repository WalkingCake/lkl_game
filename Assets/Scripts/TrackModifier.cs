using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public enum ActionStatus
    {
        Success,
        Crashed,
        Interrupted
    }

    public struct Move
    {
        public Vector2Int From {  get; set; }
        public Vector2Int To { get; set; }

        public override string ToString() => $"From: {From}, To: {To}";
    }

    [Serializable]
    public enum MoveAction
    {
        Forward,
        Backward,
        Left,
        Right,
        Idle
    }

    public struct MoveRequest
    {
        public Vector2Int From { get; set; }

        public MoveAction Side { get; set; }

    }

    public struct Crash
    {
        public int X { get; }
        public int Y { get; }

        public Crash(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class ActionConsequances
    {
        public ActionConsequances(ActionStatus status)
        {
            Status = status;
            _fromTo = new LinkedList<Move>();
        }

        public ActionStatus Status { get; }

        public void AddMove(int xFrom, int yFrom, int xTo, int yTo)
        {
            _fromTo.AddLast(
                new Move()
                {
                    From = new Vector2Int(xFrom, yFrom),
                    To = new Vector2Int(xTo, yTo)
                });
        }

        public IEnumerable<Move> GetMoves()
        {
            return _fromTo;
        }

        private readonly LinkedList<Move> _fromTo;
    }

    public class TrackModifier
    {

        public event Action<int, int> OnPlayerFinished;
        public event Action<int, int> OnPlayerLosed;
        public event Action<Move> OnAfterCarMoved;
        public event Action<Move> OnBeforeCarMoved;
        public event Action<int, int> OnCarDisapeared;

        public int FinishOverride { get; set; } = -1;

        public TrackModifier(Track<GameEntity> track)
        {
            _track = track;
        }

        public void QueueMove(MoveRequest req)
        {
            _moveRequests.Add(req);
        }

        public void ApplyMove()
        {
            _moveRequests.Sort( (a, b) => b.From.y - a.From.y );
            foreach (MoveRequest req in _moveRequests)
            {
                int x = req.From.x;
                int y = req.From.y;

                switch (req.Side)
                {
                    case MoveAction.Left:
                        MoveLeft(x, y);
                        break;
                    case MoveAction.Right:
                        MoveRight(x, y);
                        break;
                    case MoveAction.Backward:
                        MoveBackward(x, y);
                        break;
                    case MoveAction.Forward:
                        MoveForward(x, y);
                        break;
                }
            }
            _moveRequests.Clear();

            List<Crash> crashes = _crashes.ToList();
            crashes.Sort((a, b) => a.Y - b.Y);
            foreach (Crash crash in crashes)
            {
                HandleCrash(crash.X, crash.Y);
            }
            _crashes.Clear();

            if (FinishOverride > 0)
            {
                for (int x = 0; x < _track.Width; x++)
                {
                    for (int y = FinishOverride - 1; y < _track.Length; y++)
                    {
                        if (_track.GetAt(x, y) == GameEntity.Player)
                        {
                            FirePlayerFinished(x, y);
                        }
                    }
                }
            }
        }

        private void FirePlayerFinished(int x, int y)
        {
            Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Only player can finish track");
            OnPlayerFinished?.Invoke(x, y);
            _track.SetAt(x, y, GameEntity.Free);
        }

        private ActionStatus MoveForward(int x, int y)
        {
            Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Only players can move forward");
            if (y + 1 >= _track.Length)
            {
                FirePlayerFinished(x, y);
                return ActionStatus.Success;
            }

            if (_track.GetAt(x, y + 1) == GameEntity.Free)
            {
                Move move = new Move() { From = new Vector2Int(x, y), To = new Vector2Int(x, y + 1) };
                MoveCar(move);
                return ActionStatus.Success;
            }

            AddCrash(x, y + 1);
            AddCrash(x, y);
            return ActionStatus.Interrupted;
        }

        private ActionStatus MoveBackward(int x, int y)
        {
            Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Only players can move backward");
            if (y - 1 < 0)
            {
                OnPlayerLosed?.Invoke(x, y);
                return ActionStatus.Interrupted;
            }

            if (_track.GetAt(x, y - 1) == GameEntity.Free)
            {
                Move move = new Move() { From = new Vector2Int(x, y), To = new Vector2Int(x, y - 1) };
                MoveCar(move);
                return ActionStatus.Success;
            }

            AddCrash(x, y);
            AddCrash(x, y - 1);
            return ActionStatus.Interrupted;
        }

        private ActionStatus MoveLeft(int x, int y)
        {
            Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Only players can move left");
            if (x - 1 < 0)
            {
                AddCrash(x, y);
                return ActionStatus.Crashed;
            }

            if (_track.GetAt(x - 1, y) == GameEntity.Free)
            {
                Move move = new Move() { From = new Vector2Int(x, y), To = new Vector2Int(x - 1, y) };
                MoveCar(move);
                return ActionStatus.Success;
            }

            AddCrash(x, y);
            AddCrash(x - 1, y);
            return ActionStatus.Interrupted;
        }

        private ActionStatus MoveRight(int x, int y)
        {
            Debug.Assert(_track.GetAt(x, y) == GameEntity.Player, "Only players can move right");

            if (x + 1 >= _track.Length)
            {
                AddCrash(x, y);
                return ActionStatus.Crashed;
            }

            if (_track.GetAt(x + 1, y) == GameEntity.Free)
            {
                Move move = new Move() { From = new Vector2Int(x, y), To = new Vector2Int(x + 1, y) };
                MoveCar(move);
                return ActionStatus.Success;
            }

            AddCrash(x + 1, y);
            AddCrash(x, y);
            return ActionStatus.Interrupted;
        }

        private void AddCrash(int x, int y) => _crashes.Add(new Crash(x, y));

        private void HandleCrash(int x, int y)
        {
            Debug.Log($"Crash handled at [{x}, {y}]");
            if (y == 0)
            {
                if (_track.GetAt(x, y) == GameEntity.Player)
                {
                    OnPlayerLosed?.Invoke(x, y);
                }
                else
                {
                    OnCarDisapeared?.Invoke(x, 0);
                }
                _track.SetAt(x, 0, GameEntity.Free);
                return;
            }

            if (_track.GetAt(x, y - 1) != GameEntity.Free)
            {
                HandleCrash(x, y - 1);
            }
            Move move = new Move() { From = new Vector2Int(x, y), To = new Vector2Int(x, y - 1) };
            MoveCar(move);
        }

        private void MoveCar(Move move)
        {
            Debug.Assert(_track.GetAt(move.From) != GameEntity.Free, "Only cars can move");
            Debug.Assert(_track.GetAt(move.To) == GameEntity.Free, "Only free destination allowed");

            Debug.Log($"Player [{_track.GetAt(move.From)}] requested move [{move}]");
            OnBeforeCarMoved?.Invoke(move);
            
            _track.SetAt(move.To, _track.GetAt(move.From));
            _track.SetAt(move.From, GameEntity.Free);

            OnAfterCarMoved?.Invoke(move);
        }

        private Track<GameEntity> _track;
        private HashSet<Crash> _crashes = new HashSet<Crash>();
        private List<MoveRequest> _moveRequests = new List<MoveRequest>();
    }
}
