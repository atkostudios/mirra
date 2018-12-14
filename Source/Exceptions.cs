using System;

namespace Atko.Mirra
{
    public class MirraException : Exception
    {
        public MirraException(string message = null, Exception inner = null) : base(message, inner)
        { }
    }

    public class MirraInvocationException : MirraException
    {
        public MirraInvocationException(string message = null, Exception inner = null) :
            base(message ?? "An invocation failed.", inner)
        { }
    }

    public class MirraInvocationStructArgumentNullException : MirraInvocationException
    {
        public MirraInvocationStructArgumentNullException(Exception inner = null) :
            base("A struct argument cannot be null.", inner)
        { }
    }

    public class MirraInvocationArgumentCountException : MirraException
    {
        public MirraInvocationArgumentCountException(Exception inner = null) :
            base("Invalid number of arguments for invocation.", inner)
        { }
    }

    public class MirraInvocationArgumentTypeException : MirraException
    {
        public MirraInvocationArgumentTypeException(Exception inner = null) :
            base("Invalid argument types for invocation.", inner)
        { }
    }

    public class MirraInvocationInstanceTypeException : MirraException
    {
        public MirraInvocationInstanceTypeException(Exception inner = null) :
            base("Invalid instance type for invocation.", inner)
        { }
    }

    public class MirraMissingMemberException : MirraException
    {
        public MirraMissingMemberException(string message = null, Exception inner = null) :
            base(message ?? "Member does not exist on the target type.", inner)
        { }
    }
}