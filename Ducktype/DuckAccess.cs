using System;
using System.Collections.Generic;
using Utility;

namespace Ducktype
{
    public static class DuckAccess
    {
        public static Type GetImplementation(Type type, Type ancestor)
        {
            return TypeProcessor.GetImplementation(type, ancestor);
        }
    }

}