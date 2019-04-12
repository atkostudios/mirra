using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NullGuard;

namespace Atko.Mirra
{
    public class InvokerDispatch<T> where T : class
    {
        public int MinArgumentCount { get; }
        public int MaxArgumentCount { get; }

        Lazy<T>[] Invokers { get; }

        internal InvokerDispatch(IReadOnlyCollection<ParameterInfo> parameters, Func<int, T> factory)
        {
            MinArgumentCount = parameters.TakeWhile((current) => !current.IsOptional).Count();
            MaxArgumentCount = parameters.Count;
            Invokers = new Lazy<T>[MaxArgumentCount - MinArgumentCount + 1];
            for (var i = 0; i < Invokers.Length; i++)
            {
                var count = i + MinArgumentCount;
                Invokers[i] = new Lazy<T>(() => factory(count));
            }
        }

        [return: AllowNull]
        public T Get(int argumentCount)
        {
            if (argumentCount < MinArgumentCount || argumentCount > MaxArgumentCount)
            {
                return null;
            }

            return Invokers[argumentCount - MinArgumentCount].Value;
        }
    }
}