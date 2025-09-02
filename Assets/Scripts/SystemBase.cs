using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class SystemBase : MonoBehaviour
    {
        public event System.Action OnInited;
        public bool IsInited { get; private set; }

        public void SubscribeOrExecute(System.Action action)
        {
            if (IsInited)
            {
                action?.Invoke();
                return;
            }

            OnInited += action;
        }

        public void Init()
        {
            InitInternal();
            IsInited = true;
            OnInited?.Invoke();
        }

        protected abstract void InitInternal();

        private void Update()
        {
            if (!IsInited) return;
            UpdateInternal();
        }

        protected virtual void UpdateInternal() { }
    }
}
