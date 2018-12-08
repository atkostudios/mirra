using System;

namespace Atko.Dodge.Models
{
    public static class TypeModelExtensions
    {
        public static TypeModel Model(this object obj)
        {
            return TypeModel.Get(obj.GetType());
        }

        public static TypeModel Model<T>(this T obj)
        {
            var type = obj.GetType();
            if (type == typeof(T))
            {
                return TypeModel.Get<T>();
            }

            return TypeModel.Get(type);
        }

        public static TypeModel Model(this Type type)
        {
            return TypeModel.Get(type);
        }
    }
}