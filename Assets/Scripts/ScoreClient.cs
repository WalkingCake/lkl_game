using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class ScoreClient : MonoBehaviour
    {
        public string Port { get; set; }
        public string Url => $"http://127.0.0.1:{Port}/api/v1/score";
        public string ScoreSerialized { get; set; } = string.Empty;

        private async void Start()
        {
            HttpClient client = new HttpClient();
            StringContent content = new StringContent(ScoreSerialized, Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            while (response == null || !response.IsSuccessStatusCode)
            {
                response = await client.PostAsync(Url, content);
            }
            Debug.Log("Score successfully sent");
            Application.Quit();
        }

    }
}
