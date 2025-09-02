using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class BuildingSpawner : MonoBehaviour
    {
        public void Init(Vector3 position, Vector3 velocity, float travelDistance)
        {
            transform.position = position;
            _velocity = velocity;
            _travelDistance = travelDistance;
            CreateInitial();
        }

        private void CreateInitial()
        {
            float filledDist = 0f;
            Vector3 pos = transform.position;
            while (filledDist < _travelDistance)
            {
                Vector3 stepOffset = UnityEngine.Random.Range(_minSpawnDelay, _maxSpawnDelay) * _velocity;
                filledDist += stepOffset.magnitude;
                pos += stepOffset;
                CreateBuilding(pos, _travelDistance - filledDist);
            }
        }

        private void CreateBuilding(Vector3 pos, float travelDist)
        {
            GameObject buildingGO = Instantiate(_buildingSample, transform);
            Building building = buildingGO.GetComponent<Building>();
            building.Init(_velocity, pos, travelDist);
        }

        private void Update()
        {
            if ((_currentDelay -= Time.deltaTime) > 0f)
            {
                return;
            }

            _currentDelay = UnityEngine.Random.Range(_minSpawnDelay, _maxSpawnDelay);
            CreateBuilding(transform.position, _travelDistance);
        }

        private Vector3 _velocity;
        private float _travelDistance;
        private float _currentDelay = -1f;
        [SerializeField] private float _minSpawnDelay = .5f;
        [SerializeField] private float _maxSpawnDelay = 2f;
        [SerializeField] private GameObject _buildingSample;
    }
}
