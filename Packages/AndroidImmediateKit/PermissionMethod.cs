using UnityEngine;

namespace AndroidImmediateKit
{
    public readonly struct PermissionMethod<TResult>
    {
        private readonly Method<TResult> _method;
        public string[] Permissions { get; }

        public PermissionMethod(Method<TResult> method, params string[] permissions)
        {
            _method = method;
            Permissions = permissions;
        }

        public TResult Call(AndroidJavaObject instance) => _method.Call(instance);
    }
}