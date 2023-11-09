using System;
using UnityEngine;
using UnityEngine.Android;

namespace AndroidImmediateKit
{
    public abstract partial class AndroidJavaObjectContext : IDisposable
    {
        private readonly AndroidJavaObject _instance;

        protected AndroidJavaObjectContext(AndroidJavaObject instance)
        {
            _instance = instance;
        }

        protected TResult Call<TResult>(in Method<TResult> method) => method.Call(_instance);

        // ReSharper disable Unity.PerformanceAnalysis
        protected bool TryCall<TResult>(in PermissionMethod<TResult> method, out TResult result)
        {
            result = default;
            foreach (var permission in method.Permissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Debug.LogError($"require permission: {permission}");
                    Permission.RequestUserPermission(permission);
                    return false;
                }
            }

            result = method.Call(_instance);
            return true;
        }


        public void Dispose()
        {
            _instance?.Dispose();
        }

        public static explicit operator AndroidJavaObject(AndroidJavaObjectContext context) => context._instance;
    }
}