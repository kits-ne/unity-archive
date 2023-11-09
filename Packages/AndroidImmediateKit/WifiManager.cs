using UnityEngine;

namespace AndroidImmediateKit
{
    public class WifiManager : SystemServiceBase
    {
        static class Methods
        {
            /// <summary>
            /// Deprecated API level 31
            /// android.permission.ACCESS_WIFI_STATE
            /// </summary>
            public static readonly PermissionMethod<AndroidJavaObject> GetConnectionInfo =
                new("getConnectionInfo", Permissions.AccessWifiState);

            /// <summary>
            /// Deprecated API level 30
            /// use calculateSignalLevel(int)
            /// </summary>
            public static readonly StaticMethod<int, int, int> CalculateSignalLevel = "calculateSignalLevel";
        }


        public WifiManager(UnityPlayerActivity activity) : base(activity, SystemService.Wifi)
        {
        }

        public bool TryGetConnectionInfo(out WifiInfo wifiInfo)
        {
            wifiInfo = null;
            if (TryCall(Methods.GetConnectionInfo, out var instance))
            {
                wifiInfo = new WifiInfo(instance);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 0 ~ numLevels - 1
        /// </summary>
        public int CalculateSignalLevel(int rssi, int numLevels)
        {
            AndroidJavaObject instance = (AndroidJavaObject) this;
            // return instance.CallStatic<int, int>(Methods.CalculateSignalLevel.Name, new int[] {rssi, numLevels});
            // return instance.CallStatic<int>(Methods.CalculateSignalLevel.Name, rssi, numLevels);
            return Call(Methods.CalculateSignalLevel, rssi, numLevels);
        }
    }

    public class WifiInfo : AndroidJavaObjectContext
    {
        static class Methods
        {
            /// <summary>
            /// -55 ~ -90
            /// </summary>
            public static readonly Method<int> GetRssi = "getRssi";
        }

        public WifiInfo(AndroidJavaObject instance) : base(instance)
        {
        }

        public int GetRssi => Call(Methods.GetRssi);
    }

    
}