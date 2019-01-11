using System.Reflection;
using Atko.Mirra.Generation;

namespace Atko.Mirra.Images
{
    public class ConstructorImage : CallableImage
    {
        public ConstructorInfo Constructor => (ConstructorInfo)Member;

        internal ConstructorImage(ConstructorInfo constructor) :
            base(constructor, null, (argumentCount) => CodeGenerator.Instance.Constructor(constructor, argumentCount))
        { }

        /// <summary>
        /// Invoke the constructor with the provided arguments and return the resulting instance.
        /// </summary>
        /// <param name="arguments">The arguments to provide to the constructor.</param>
        /// <returns>The constructed instance.</returns>
        public object Call(params object[] arguments)
        {
            return CallInternal(null, arguments);
        }
    }
}