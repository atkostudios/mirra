using System.Collections;
using System.Collections.Generic;

namespace Atko.Mirra.Images
{
    class TypeTreeEnumerable : IEnumerable<TypeImage>
    {
        TypeImage Root { get; }
        bool Skip { get; }

        public TypeTreeEnumerable(TypeImage root, bool skip)
        {
            Root = root;
            Skip = skip;
        }

        TypeTreeEnumerator GetEnumerator()
        {
            return new TypeTreeEnumerator(Root, Skip);
        }

        IEnumerator<TypeImage> IEnumerable<TypeImage>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}