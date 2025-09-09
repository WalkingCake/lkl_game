using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public enum EndGameStatus { Finished = 1, Losed = 0 }

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

    [Serializable]
    public class EndGameInfoSerialized
    {
        public string id = string.Empty;
        public int score = -1;
    }

    [Serializable]
    public class EndGameInfoCollection
    {
        public EndGameInfoCollection(int playerCount)
        {
            scores = new EndGameInfoSerialized[playerCount];
        }

        public EndGameInfoSerialized[] scores;
    }

    public class ScoreController : SystemBase
    {
        public event Action<EndGameInfo[]> OnScoreFilled;

        public string SqlAddress { get; set; } = string.Empty;
        public string SqlTable { get; set; } = string.Empty;
        public int SqlMatchId { get; set; } = -1;

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
        protected override void InitInternal()
        {
            _infos = new EndGameInfo[_game.PlayerCount];
            _finishIdx = 0;
            _loseIdx = _infos.Length - 1;
        }

        private void Update()
        {
            if (!_isScoreFilled)
            {
                return;
            }
            
            _autoCloseDelay -= Time.deltaTime;
            if (_autoCloseDelay < 0f)
            {
                Application.Quit();
            }
        }

        private void FireScoreFilled()
        {
            WriteScore();
            OnScoreFilled?.Invoke(_infos);
            _isScoreFilled = true;
        }

        private void WriteScore()
        {
            EndGameInfoCollection scores = new EndGameInfoCollection(_infos.Length);
            for (int i = 0; i < _infos.Length; i++)
            {
                scores.scores[i] = new EndGameInfoSerialized() { id = _infos[i].PlayerName, score = _infos.Length - i - 1 };
            }

            ScoreClient scoreClient = gameObject.AddComponent<ScoreClient>();
            scoreClient.Port = _game.Port;
            scoreClient.ScoreSerialized = JsonUtility.ToJson(scores);
        }

        private EndGameInfo[] _infos;
        private int _finishIdx, _loseIdx;
        private bool _isScoreFilled = false;

        [SerializeField] private Game _game;
        [SerializeField] private float _autoCloseDelay = 10f;
    }
}
