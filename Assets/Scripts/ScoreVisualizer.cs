using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class ScoreVisualizer : MonoBehaviour
    {
        private void Start()
        {
            _scoreController.OnScoreFilled += HandleScoreFilled;
        }

        private void OnDestroy()
        {
            _scoreController.OnScoreFilled -= HandleScoreFilled;
        }

        private void HandleScoreFilled(EndGameInfo[] infos)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < infos.Length; i++)
            {
                builder.AppendLine($"{i}. {infos[i]}");
            }
            _text.text = builder.ToString();
        }

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private ScoreController _scoreController;
    }
}
