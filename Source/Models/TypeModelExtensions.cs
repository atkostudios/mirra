using System;

namespace Atko.Dodge.Models
{
    public static class TypeModelExtensions
    {
        public static TypeModel Model(this Type type)
        {
            return TypeModel.Get(type);
        }
    }
}