using System;
using System.Reflection;

namespace Atko.Mirra.Generation
{
    static partial class Generate
    {
        static bool IsAccessor(MemberInfo member)
        {
            return (member.MemberType & (MemberTypes.Property | MemberTypes.Field)) != 0;
        }

        static void CheckInstance(Type type, object argument)
        {
            if (IsInvalid(type, argument))
            {
                throw new MirraInvocationInstanceTypeException();
            }
        }

        static void CheckArgument(Type type, object argument)
        {
            if (IsInvalid(type, argument))
            {
                throw new MirraInvocationArgumentTypeException();
            }
        }

        static bool IsInvalid(Type type, object argument)
        {
            if (type.IsValueType && argument == null)
            {
                return true;
            }

            if (!type.IsInstanceOfType(argument))
            {
                return true;
            }

            return false;
        }
    }
}