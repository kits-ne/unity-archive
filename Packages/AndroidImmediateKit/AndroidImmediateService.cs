using System;

namespace AndroidImmediateKit
{
    public static class Features
    {
        /// <summary>
        /// android.permission.ACCESS_NETWORK_STATE
        /// android.permission.ACCESS_WIFI_STATE
        /// </summary>
        public static INetworkFeature GetNetwork()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidNetworkFeature();
#endif
            return new EditorNetworkFeature();
        }
    }

    public interface INetworkFeature : IDisposable
    {
        (bool isConnected, int networkType, int wifiLevel) GetInfo(int wifiNumLevels);
    }

    public class AndroidNetworkFeature : INetworkFeature
    {
        private readonly UnityPlayerActivity _unityPlayer;
        private readonly ConnectivityManager _connectivity;
        private readonly WifiManager _wifi;

        public AndroidNetworkFeature()
        {
            _unityPlayer = new UnityPlayerActivity();
            _connectivity = _unityPlayer.GetConnectivityManager();
            _wifi = _unityPlayer.GetWifiManager();
        }

        public (bool isConnected, int networkType, int wifiLevel) GetInfo(int wifiNumLevels)
        {
            using var infoScope = new InfoScope(_connectivity, _wifi);
            return (infoScope.IsConnected, infoScope.NetworkType, infoScope.GetWifiLevel(wifiNumLevels));
        }

        public void Dispose()
        {
            _wifi.Dispose();
            _connectivity.Dispose();
            _unityPlayer.Dispose();
        }

        readonly struct InfoScope : IDisposable
        {
            public bool IsConnected
            {
                get
                {
                    if (_wifiInfo == null) return false;
                    return _networkInfo?.IsConnected ?? false;
                }
            }

            public int NetworkType => _networkInfo?.GetConnectType ?? -1;

            public int GetWifiLevel(int numLevels)
            {
                if (_wifiInfo == null) return 0;
                var rssi = _wifiInfo.GetRssi;
                return _wifiManager.CalculateSignalLevel(rssi, numLevels);
            }

            private readonly NetworkInfo _networkInfo;
            private readonly WifiInfo _wifiInfo;
            private readonly WifiManager _wifiManager;

            public InfoScope(
                ConnectivityManager connectivity,
                WifiManager wifi)
            {
                connectivity.TryGetActiveNetworkInfo(out _networkInfo);

                _wifiManager = wifi;
                _wifiManager.TryGetConnectionInfo(out _wifiInfo);
            }

            public void Dispose()
            {
                _networkInfo?.Dispose();
                _wifiInfo?.Dispose();
            }
        }
    }

    public class EditorNetworkFeature : INetworkFeature
    {
        public (bool isConnected, int networkType, int wifiLevel) GetInfo(int wifiNumLevels)
        {
            return (true, NetworkType.Wifi, wifiNumLevels);
        }

        public void Dispose()
        {
        }
    }
}