using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class CarMover : MonoBehaviour
    {
        public float Tick { get; set; }

        public Vector3 Target { get => _target;
            set
            {
                _offset = value - _target;
                _target = value;
                _currentPath = Vector3.zero;
                Vector3 cross = Vector3.Cross(_offset, _initialFwd);
                _needRotate = cross.sqrMagnitude > Mathf.Epsilon;
                if (_needRotate)
                {
                    _isLeftRotate = Vector3.Dot(cross, Vector3.up) > 0f;
                }
            }
        }

        private void Start()
        {
            _initialFwd = transform.forward;
            _target = transform.position;
        }

        private void Update()
        {
            Vector3 move = Vector3.Lerp(Vector3.zero, _offset, Time.deltaTime / Tick);
            _currentPath += move;

            if (_currentPath.sqrMagnitude > _offset.sqrMagnitude)
            {
                _offset = Vector3.zero;
                transform.position = Target;
                transform.rotation = Quaternion.identity;
                _needRotate = false;
                return;
            }

            transform.position += move;

            if (!_needRotate)
            {
                return;
            }
            float rot = _rotation * Time.deltaTime / Tick * 2f;
            _currentRot += rot * (_isLeftRotate != _currentPath.magnitude * 2f < _offset.magnitude ? 1 : -1);
            transform.rotation = Quaternion.Euler(0f, _currentRot, 0f);
        }

        private Vector3 _target;
        private Vector3 _initialFwd;
        private Vector3 _offset = Vector3.zero;
        private Vector3 _currentPath = Vector3.zero;
        private float _currentRot = 0f;
        private bool _isLeftRotate;
        private bool _needRotate = false;
        [SerializeField] private float _rotation = 30f;
    }
}
