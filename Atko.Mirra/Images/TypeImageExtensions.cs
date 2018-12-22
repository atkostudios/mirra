using System;

namespace Atko.Mirra.Images
{
    public static class TypeImageExtensions
    {
        public static TypeImage Image(this Type type)
        {
            return TypeImage.Get(type);
        }
    }
}