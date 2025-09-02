using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public enum EndGameStatus { Finished, Losed }

    public class EndGameInfo
    {
        public EndGameStatus Status { get; set; }
        public string PlayerName { get; set; }
        public int StepId { get; set; }

        public override string ToString()
        {
            return $"[{PlayerName}: {Status} at step {StepId}]";
        }
    }

    public class ScoreController : SystemBase
    {
        public event Action<EndGameInfo[]> OnScoreFilled;

        protected override void InitInternal()
        {
            _infos = new EndGameInfo[_game.PlayerCount];
            _finishIdx = 0;
            _loseIdx = _infos.Length - 1;
        }

        public void NotifyPlayerLosed(Player player)
        {
            Debug.Assert(_infos[_loseIdx] == null, $"Info already set at lose idx {_loseIdx}");
            _infos[_loseIdx] = new EndGameInfo() { PlayerName = player.Name, StepId = _game.StepId, Status = EndGameStatus.Losed };
            _loseIdx--;
            if (_finishIdx > _loseIdx)
            {
                FireScoreFilled();
            }
        }

        public void NotifyPlayerFinished(Player player)
        {
            Debug.Assert(_infos[_finishIdx] == null, $"Info already set at lose idx {_finishIdx}");
            _infos[_finishIdx] = new EndGameInfo() { PlayerName = player.Name , StepId = _game.StepId, Status = EndGameStatus.Finished };
            _finishIdx++;
            if (_finishIdx > _loseIdx)
            {
                FireScoreFilled();
            }
        }

        private void FireScoreFilled()
        {
            OnScoreFilled?.Invoke(_infos);
        }

        private EndGameInfo[] _infos;
        private int _finishIdx, _loseIdx;

        [SerializeField] private Game _game;
    }
}
