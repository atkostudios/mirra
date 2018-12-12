using System;

namespace Atko.Dodge.Images
{
    public static class TypeImageExtensions
    {
        public static TypeImage Image(this Type type)
        {
            return TypeImage.Get(type);
        }
    }
}