using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Player : MonoBehaviour
    {
        public string Name { get; private set; }
        public int Index { get; private set; }
        
        public Vector2Int Pos { get; set; }

        public void RequestMove(TrackSnapshot trackSnapshot)
        {
            _playerClient.RequestMove(trackSnapshot);
        }

        public MoveAction GetMove()
        {
            if(Enum.TryParse(_playerClient.Response.move, out MoveAction action))
            {
                return action;
            }

            return MoveAction.Idle;
        }

        public void Init(string name, int idx)
        {
            Name = name;
            Index = idx;
        }

        private MoveAction _action;

        [SerializeField] private PlayerClient _playerClient;
    }
}
