using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerVisualizer : MonoBehaviour
    {
        public int Seed { get; set; }

        private void Start()
        {
            _nameText.text = _player.Name;
            _carMesh = Instantiate(_samples[_player.Index % _samples.Count], transform);
        }

        private void Update()
        {
            UpdateNamePosition();
        }

        private void UpdateNamePosition()
        {
            Transform camTrs = Camera.main.transform;
            _nameTrs.transform.position = transform.position + camTrs.up * _nameOffset;
            _nameTrs.transform.LookAt(_nameTrs.position + camTrs.forward);
        }

        private GameObject _carMesh;

        [SerializeField] private float _nameOffset = 3f;
        [SerializeField] private Transform _nameTrs;
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private List<GameObject> _samples;
        [SerializeField] private Player _player;
    }
}
