using System.Linq;
using System.Reflection;

namespace Atko.Mirra.Generation
{
    public class ArgumentChecker
    {
        ParameterInfo[] Parameters { get; }

        public ArgumentChecker(ParameterInfo[] parameters, int argumentCount)
        {
            Parameters = parameters.Take(argumentCount).ToArray();
        }

        public void CheckArguments(object[] arguments)
        {
            if (arguments.Length != Parameters.Length)
            {
                throw new MirraInvocationArgumentCountException();
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                var parameter = Parameters[i];
                var argument = arguments[i];

                if (parameter.ParameterType.IsValueType && argument == null)
                {
                    throw new MirraInvocationArgumentException();
                }

                if (argument != null && !parameter.ParameterType.IsInstanceOfType(argument))
                {
                    throw new MirraInvocationArgumentException();
                }
            }
        }
    }
}