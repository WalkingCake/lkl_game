using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerClient : MonoBehaviour
    {

        public string Port
        {
            private get; set;
        }

        public string Url
        {
            get => $"http://127.0.0.1:{Port}/api/v1/next_move";
        }

        public Response Response
        {
            get
            {
                return _response ?? new Response() { id = _snapshot == null ? "" : _snapshot.my_agent.id, move = "Idle" };
            }
            private set
            {
                _response = value;
            }
        }

        public void RequestMove(TrackSnapshot snapshot)
        {
            _snapshot = snapshot;
            Response = null;
        }


        private async void Awake()
        {
            _client = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            await StartClient();
        }

        private async Task StartClient()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                while (_snapshot == null)
                {
                    await Task.Yield();
                }
                Debug.Log($"Snapshot got from {_snapshot.my_agent.id}");
                string json = JsonUtility.ToJson(_snapshot);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(Url, content);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"Response got for {_snapshot.my_agent.id}");
                    string responseContent = await response.Content.ReadAsStringAsync();
                    _snapshot = null;
                    Response = JsonUtility.FromJson<Response>(responseContent);
                }
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private TrackSnapshot _snapshot;
        private HttpClient _client;
        private Response _response;
    }
}
