using System.Linq;
using System.Reflection;

namespace Atko.Mirra.Generation
{
    public class ArgumentChecker
    {
        ParameterInfo[] Parameters { get; }

        int MinArgumentCount { get; }
        int MaxArgumentCount => Parameters.Length;

        public ArgumentChecker(ParameterInfo[] parameters)
        {
            Parameters = parameters;
            MinArgumentCount = parameters.TakeWhile((current) => !current.IsOptional).Count();
        }

        public void Check(object[] arguments)
        {
            if (arguments.Length < MinArgumentCount || arguments.Length > MaxArgumentCount)
            {
                throw new MirraInvocationArgumentCountException();
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                var parameter = Parameters[i];
                var argument = arguments[i];

                if (parameter.ParameterType.IsValueType && argument == null)
                {
                    throw new MirraInvocationArgumentTypeException();
                }

                if (argument != null && !parameter.ParameterType.IsInstanceOfType(argument))
                {
                    throw new MirraInvocationArgumentTypeException();
                }
            }
        }
    }
}