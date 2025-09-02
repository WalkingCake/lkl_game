using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    internal class SystemBundle : MonoBehaviour
    {
        private void Awake()
        {
            foreach (SystemBase sys in _systems)
            {
                sys.Init();
            }
        }

        [SerializeField] private SystemBase[] _systems;
    }
}
