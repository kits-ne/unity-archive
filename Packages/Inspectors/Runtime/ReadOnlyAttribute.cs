using UnityEngine;
using System;

namespace Inspectors
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public bool onlyInPlaymode = false;
    }
}