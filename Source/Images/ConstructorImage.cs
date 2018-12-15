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

        public object Call(params object[] arguments)
        {
            return CallInternal(null, arguments);
        }
    }
}