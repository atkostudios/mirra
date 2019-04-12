using System.Collections;
using System.Collections.Generic;
using NullGuard;

namespace Atko.Mirra
{
    class TypeTreeEnumerator : IEnumerator<TypeImage>
    {
        static Stack<Stack<TypeImage>> StackPool { get; } = new Stack<Stack<TypeImage>>();

        static Stack<TypeImage> BorrowStack()
        {
            lock (StackPool)
            {
                if (StackPool.Count == 0)
                {
                    return new Stack<TypeImage>();
                }

                var stack = StackPool.Pop();
                stack.Clear();
                return stack;
            }
        }

        static void ReturnStack(Stack<TypeImage> stack)
        {
            lock (StackPool)
            {
                stack.Clear();
                StackPool.Push(stack);
            }
        }

        [AllowNull]
        public TypeImage Current { get; private set; }

        object IEnumerator.Current => Current;

        TypeImage Root { get; }

        [AllowNull]
        Stack<TypeImage> Stack { get; set; }

        internal TypeTreeEnumerator(TypeImage root, bool skip)
        {
            Root = root;
            Current = null;

            Stack = BorrowStack();
            Stack.Push(Root);

            if (skip)
            {
                MoveNext();
            }
        }

        public bool MoveNext()
        {
            if (Stack.Count == 0)
            {
                return false;
            }

            Current = Stack.Pop();

            if (Current != null)
            {
                foreach (var type in Current.Subclasses)
                {
                    Stack.Push(type);
                }
            }

            return true;
        }

        public void Reset()
        {
            Current = null;
            Stack = BorrowStack();
        }

        public void Dispose()
        {
            ReturnStack(Stack);
            Stack = null;
        }
    }
}