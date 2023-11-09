using System;
using UnityEngine;

namespace AndroidImmediateKit
{
    public class ConnectivityManager : SystemServiceBase
    {
        static class Methods
        {
            /// <summary>
            /// Deprecated API level 29
            /// android.permission.ACCESS_NETWORK_STATE
            /// </summary>
            public static readonly PermissionMethod<AndroidJavaObject> GetActiveNetworkInfo =
                new("getActiveNetworkInfo", Permissions.AccessNetworkState);
        }

        public ConnectivityManager(UnityPlayerActivity activity) : base(activity, SystemService.Connectivity)
        {
        }

        // TODO: apply fluent result
        public bool TryGetActiveNetworkInfo(out NetworkInfo info)
        {
            info = null;
            if (TryCall(Methods.GetActiveNetworkInfo, out var instance))
            {
                info = instance == null ? null : new NetworkInfo(instance);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Deprecated API level 29
    /// </summary>
    public class NetworkInfo : AndroidJavaObjectContext
    {
        static class Methods
        {
            public static readonly Method<bool> IsConnected = "isConnected";

            /// <summary>
            /// Deprecated API level 28
            /// </summary>
            public static readonly Method<int> GetConnectType = "getType";

            public static readonly Method<string> GetConnectTypeName = "getTypeName";
        }

        public bool IsConnected => Call(Methods.IsConnected);
        public int GetConnectType => Call(Methods.GetConnectType);
        public string GetConnectTypeName => Call(Methods.GetConnectTypeName);

        public NetworkInfo(AndroidJavaObject instance) : base(instance)
        {
        }
    }

    public static class NetworkType
    {
        /// <summary>
        /// Deprecated API level 28
        /// </summary>
        public const int Mobile = 0;

        /// <summary>
        /// Deprecated API level 28
        /// </summary>
        public const int Wifi = 1;
    }
}