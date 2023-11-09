using AndroidImmediateKit;
using TMPro;
using UnityEngine;

namespace NativeInspector
{
    public class ConnectivityInspector : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public TextMeshProUGUI text2;

        private UnityPlayerActivity _unityPlayerActivity;
        private ConnectivityManager _connectivityManager;
        private WifiManager _wifiManager;

        private void Awake()
        {
            _unityPlayerActivity = new UnityPlayerActivity();
        }

        public void UpdateNetworkState()
        {
            _connectivityManager ??= _unityPlayerActivity.GetConnectivityManager();

            if (!_connectivityManager.TryGetActiveNetworkInfo(out var networkInfo))
            {
                Debug.Log("get network failed");
                return;
            }

            // reason: airplane mode,...
            if (networkInfo == null)
            {
                text.text = "network info is null";
                return;
            }

            using (networkInfo)
            {
                var isConnected = networkInfo.IsConnected;

                var connectedType = networkInfo.GetConnectType;
                var connectedTypeName = networkInfo.GetConnectTypeName;

                text.text = $"{isConnected}, {connectedType}, {connectedTypeName}";
            }
        }


        public void UpdateWifiLevel()
        {
            _wifiManager ??= _unityPlayerActivity.GetWifiManager();
            if (!_wifiManager.TryGetConnectionInfo(out var wifiInfo))
            {
                Debug.Log("connection info failed");
                return;
            }

            if (wifiInfo == null)
            {
                Debug.Log("wifi info is null");
                return;
            }

            using (wifiInfo)
            {
                var rssi = wifiInfo.GetRssi;

                var numOfLevels = 3;
                var level = _wifiManager.CalculateSignalLevel(rssi, numOfLevels);
                text2.text = $"{rssi}, {level}";
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UpdateNetworkState();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UpdateWifiLevel();
            }
        }

        private void OnDestroy()
        {
            _wifiManager?.Dispose();
            _connectivityManager?.Dispose();

            _unityPlayerActivity?.Dispose();
            _connectivityManager?.Dispose();
        }
    }
}