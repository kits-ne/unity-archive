using System;

namespace UniBloc
{
    public class StateException : Exception
    {
        public StateException(string message) : base(message)
        {
        }
    }
}