using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Scripts
{
    public struct TargetCameraOrientation
    {
        public Vector3 Pos { get; set; }
        public Vector3 Fwd { get; set; }
        public Vector3 Up { get; set; }
        public bool NeedSwitch { get; set; }
    }

    public class CameraDirector : MonoBehaviour
    {
        private void Start()
        {
            _poiList.AddRange(GameObject.FindGameObjectsWithTag("POI").Select(p => p.transform));

            if (_game.ExclusiveShowPlayerName != string.Empty)
            {
                _poiList.RemoveAll(p => !p.TryGetComponent(out Player player) || player.Name != _game.ExclusiveShowPlayerName);
                _cameraModes = new CameraDirectorMode[] { _thirdPersonBack };
            }
            else
            {
                _cameraModes = new CameraDirectorMode[] { _thirdPersonBack, _thirdPersonFace, _thirdPersonSky };
            }

            _currentMode = _game.SkipIntro ? _cameraModes[UnityEngine.Random.Range(0, _cameraModes.Length)] : _initial;
            _currentMode.Activate(_poiList, _game.SkipIntro ? -1f : _game.InitialCountDown);
        }

        private void Update()
        {
            TargetCameraOrientation orientation = _currentMode.Update(transform, Time.deltaTime);
            if (_poiList.Count == 0)
            {
                Destroy(this);
                return;
            }

            if (orientation.NeedSwitch)
            {
                _currentMode.Deactivate();
                _currentMode = _cameraModes[UnityEngine.Random.Range(0, _cameraModes.Length)];
                while (_poiList.Count > 0 && (_poiList.Last()).IsDestroyed())
                {
                    _poiList.RemoveAt(_poiList.Count - 1);
                }

                _currentMode.Activate(_poiList);
            }

            Vector3 targetPosition = orientation.Pos;
            Quaternion targetRotation = Quaternion.LookRotation(orientation.Fwd, orientation.Up);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _lerpValueMove);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _lerpValueAngle);
        }

        private List<Transform> _poiList = new List<Transform>();
        private CameraDirectorMode[] _cameraModes;
        private CameraDirectorMode _currentMode = null;

        [SerializeField] private float _lerpValueMove = 5f;
        [SerializeField] private float _lerpValueAngle = 5f;
        
        [SerializeField] private CameraDirectorInitial _initial;
        [SerializeField] private CameraDirectorThirdPerson _thirdPersonFace;
        [SerializeField] private CameraDirectorThirdPerson _thirdPersonBack;
        [SerializeField] private CameraDirectorThirdPerson _thirdPersonSky;

        [SerializeField] private Game _game;
        
    }

    [Serializable]
    public abstract class CameraDirectorMode
    {
        public float TimeInProgress { get; private set; } = 0f;
        public float CurrentEstimatedTime { get; private set; }

        public virtual void Activate(List<Transform> poiList, float estimatedTime = -1f)
        {
            TimeInProgress = 0f;
            CurrentEstimatedTime = estimatedTime <= 0f ? UnityEngine.Random.Range(_minEstimatedTime, _maxEstimatedTime) : estimatedTime;
            _poiList = new List<Transform>();
            _poiList.AddRange(poiList);
        }

        public virtual void Deactivate() { }

        public TargetCameraOrientation Update(Transform transform, float dt)
        {
            int i = 0;
            while (i < _poiList.Count)
            {
                if (_poiList[i].IsDestroyed())
                {
                    _poiList.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (_poiList.Count == 0)
            {
                return new TargetCameraOrientation() { NeedSwitch = true };
            }

            TimeInProgress += dt;
            TargetCameraOrientation result = UpdateInternal(dt);
            if (TimeInProgress > CurrentEstimatedTime)
            {
                result.NeedSwitch = true;
            }

            return result;
        }

        protected abstract TargetCameraOrientation UpdateInternal(float dt);

        protected List<Transform> _poiList;
        
        [SerializeField] private float _minEstimatedTime = -1f;
        [SerializeField] private float _maxEstimatedTime = -1f;
    }

    [Serializable]
    public class CameraDirectorInitial : CameraDirectorMode
    {
        public override void Activate(List<Transform> poiList, float estimatedTime = -1f)
        {
            base.Activate(poiList, estimatedTime);

            Transform startPoi = poiList[0];
            Transform endPoi = poiList[0];

            foreach (Transform poi in poiList)
            {
                if ((startPoi.position - poi.position).sqrMagnitude > (startPoi.position - endPoi.position).sqrMagnitude)
                {
                    endPoi = poi;
                }
            }

            foreach(Transform poi in poiList)
            {
                if ((startPoi.position - poi.position).sqrMagnitude > (startPoi.position - endPoi.position).sqrMagnitude)
                {
                    startPoi = poi;
                }
            }

            _movementDir = (endPoi.position - startPoi.position).normalized;
            _startPoint = startPoi.position + _frontOffset - _movementDir * _frontPathOffset;
            _endPoint = endPoi.position + _frontOffset + _movementDir * _frontPathOffset;
            _cameraFwd = -_frontOffset.normalized;
            _cameraUp = Vector3.Cross(_movementDir, _cameraFwd);
        }

        protected override TargetCameraOrientation UpdateInternal(float dt)
        {
            TargetCameraOrientation orientation = new TargetCameraOrientation();
            orientation.Pos = Vector3.Lerp(_startPoint, _endPoint, TimeInProgress / CurrentEstimatedTime);
            orientation.Fwd = _cameraFwd;
            orientation.Up = _cameraUp;

            return orientation;
        }


        private Vector3 _cameraUp;
        private Vector3 _cameraFwd;
        private Vector3 _movementDir;
        private Vector3 _startPoint;
        private Vector3 _endPoint;
        [SerializeField] private Vector3 _frontOffset;
        [SerializeField] private float _frontPathOffset;
    }

    [Serializable]
    public class CameraDirectorThirdPerson : CameraDirectorMode
    {
        public override void Activate(List<Transform> poiList, float estimatedTime = -1f)
        {
            base.Activate(poiList, estimatedTime);
            
            _poiList.Sort((a, b) => (int)(a.position.z - b.position.z) * 1000 + (int)(a.position.x - b.position.x));
            _camFwd = (_poiOffset - _cameraOffset).normalized;
            Vector3 cross = Vector3.Cross(Vector3.up, _camFwd);
            if (cross.sqrMagnitude < .5f)
            {
                _camUp = Vector3.forward;
            }
            else
            {
                _camUp = Vector3.Cross(_camFwd, cross);
            }
        }

        protected override TargetCameraOrientation UpdateInternal(float dt)
        {
            TargetCameraOrientation result = new TargetCameraOrientation();
            result.Pos = _poiList[GetPoiIdx()].position + _cameraOffset;
            result.Up = _camUp;
            result.Fwd = _camFwd;
            return result;
        }

        private int GetPoiIdx()
        {
            return (int)(TimeInProgress / CurrentEstimatedTime * _poiList.Count) % _poiList.Count;
        }

        private Vector3 _camFwd;
        private Vector3 _camUp;
        
        [SerializeField] private Vector3 _poiOffset;
        [SerializeField] private Vector3 _cameraOffset;
    }
}
