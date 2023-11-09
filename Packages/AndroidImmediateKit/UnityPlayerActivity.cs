using System;
using UnityEngine;

namespace AndroidImmediateKit
{
    public static class AndroidImmediateManager
    {
    }

    public class UnityPlayerActivity : AndroidJavaObjectContext
    {
        static class Methods
        {
            public static readonly Method<string, AndroidJavaObject> GetSystemService = "getSystemService";
        }

        private const string ClassName = "com.unity3d.player.UnityPlayer";
        private const string CurrentActivityName = "currentActivity";

        private static AndroidJavaObject GetCurrentActivity()
        {
            return new AndroidJavaClass(ClassName)
                .GetStatic<AndroidJavaObject>(CurrentActivityName);
        }

        public UnityPlayerActivity() : base(GetCurrentActivity())
        {
        }

        public AndroidJavaObject GetSystemService(string name)
        {
            return Call(Methods.GetSystemService, name);
        }
    }

    public static class UnityPlayerActivityExtensions
    {
        public static ConnectivityManager GetConnectivityManager(this UnityPlayerActivity activity) => new(activity);
        public static WifiManager GetWifiManager(this UnityPlayerActivity activity) => new(activity);
    }
}