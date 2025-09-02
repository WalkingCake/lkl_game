using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class CountdownVisualizer : MonoBehaviour
    {
        private void Update()
        {
            if (_game.InitialCountDown < float.Epsilon)
            {
                Destroy(gameObject);
                return;
            }

            _text.text = $"{(int)_game.InitialCountDown}";
        }

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Game _game;
    }
}
