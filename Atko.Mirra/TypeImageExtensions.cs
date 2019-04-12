using System;

namespace Atko.Mirra
{
    public static class TypeImageExtensions
    {
        public static TypeImage Image(this Type type)
        {
            return TypeImage.Get(type);
        }
    }
}