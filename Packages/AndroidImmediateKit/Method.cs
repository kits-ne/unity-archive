using UnityEngine;

namespace AndroidImmediateKit
{
    #region <TResult>()

    public readonly struct Method<TResult>
    {
        private readonly string _name;

        public Method(string name)
        {
            _name = name;
        }

        public TResult Call(AndroidJavaObject instance) => instance.Call<TResult>(_name);

        public static implicit operator Method<TResult>(string name) => new(name);
    }

    #endregion

    #region <TResult>(T1)

    public readonly struct Method<T1, TResult>
    {
        private readonly string _name;

        public Method(string name)
        {
            _name = name;
        }

        public TResult Call(AndroidJavaObject instance, T1 arg)
        {
            return instance.Call<TResult>(_name, arg);
        }

        public static implicit operator Method<T1, TResult>(string name) => new(name);
    }

    public partial class AndroidJavaObjectContext
    {
        protected TResult Call<T1, TResult>(in Method<T1, TResult> method, T1 arg) =>
            method.Call(_instance, arg);
    }

    #endregion


    #region static <TResult>(T1,T2)

    public readonly struct StaticMethod<T1, T2, TResult>
    {
        private static readonly object[] Args = new object[2];
        public string Name { get; }

        public StaticMethod(string name)
        {
            Name = name;
        }

        public TResult Call(AndroidJavaObject instance, T1 arg1, T2 arg2)
        {
            Args[0] = arg1;
            Args[1] = arg2;
            return instance.CallStatic<TResult>(Name, Args);
        }

        public static implicit operator StaticMethod<T1, T2, TResult>(string name) => new(name);
    }


    public partial class AndroidJavaObjectContext
    {
        protected TResult Call<T1, T2, TResult>(in StaticMethod<T1, T2, TResult> method, T1 arg1, T2 arg2) =>
            method.Call(_instance, arg1, arg2);
    }

    #endregion
}