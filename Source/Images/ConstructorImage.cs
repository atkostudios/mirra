using System;
using System.Reflection;
using Atko.Mirra.Generation;

namespace Atko.Mirra.Images
{
    public class ConstructorImage : CallableImage
    {
        public ConstructorInfo Constructor => (ConstructorInfo)Member;

        internal ConstructorImage(Type owner, ConstructorInfo constructor) :
            base(owner, constructor, null,
                (argumentCount) => Generate.Constructor(constructor, argumentCount))
        { }

        public object Call(params object[] arguments)
        {
            return CallInternal(null, arguments);
        }
    }
}