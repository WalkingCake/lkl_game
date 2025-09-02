using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Building : MonoBehaviour
    {
        public void Init(Vector3 velocity, Vector3 start, float travelDistance)
        {
            Vector3 shiftAxis = Vector3.Cross(velocity, Vector3.up).normalized;
            float shiftValue = UnityEngine.Random.Range(-_xAxisShift, _xAxisShift);

            Vector3 scale = new Vector3(
                UnityEngine.Random.Range(_scaleMin.x, _scaleMax.x),
                UnityEngine.Random.Range(_scaleMin.y, _scaleMax.y),
                UnityEngine.Random.Range(_scaleMin.z, _scaleMax.z));
            transform.position = start + Vector3.up * scale.y / 2f + shiftAxis * shiftValue;
            transform.localScale = scale;
            _velocity = velocity;
            _travelDistance = travelDistance;
        }

        private void Update()
        {
            if (_velocity.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            Vector3 movement = _velocity * Time.deltaTime;
            transform.position += movement;
            _travelDistance -= movement.magnitude;
            if (_travelDistance <0f)
            {
                Destroy(gameObject);
            }
        }

        private Vector3 _velocity = Vector3.zero;
        private float _travelDistance = 0f;

        [SerializeField] private Vector3 _scaleMin = Vector3.one;
        [SerializeField] private Vector3 _scaleMax = Vector3.one;
        [SerializeField] private float _xAxisShift = 5f;
    }
}
