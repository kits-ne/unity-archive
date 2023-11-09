using AndroidImmediateKit;
using TMPro;
using UnityEngine;

namespace NativeInspector
{
    public class ConnectivityInspector : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public TextMeshProUGUI text2;


        private INetworkFeature _networkFeature;

        private void Awake()
        {
            _networkFeature = Features.GetNetwork();
        }

        public void UpdateState()
        {
            var (isConnected, networkType, wifiLevel) = _networkFeature.GetInfo(3);
            text.text = isConnected ? $"{networkType.ToString()}" : "disconnected";
            text2.text = $"{wifiLevel.ToString()}";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UpdateState();
            }
        }

        private void OnDestroy()
        {
            _networkFeature?.Dispose();
        }
    }
}