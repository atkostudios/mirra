using System.Reflection;
using Atko.Mirra.Utility;

namespace Atko.Mirra.Images
{
    public class FieldImage : AccessorImage
    {
        public override bool IsPublic => Field.IsPublic;
        public override bool IsStatic => Field.IsStatic;

        public override bool CanGet => true;
        public override bool CanSet => true;

        public bool IsBacking { get; }

        public FieldInfo Field => (FieldInfo)Member;

        internal FieldImage(FieldInfo member) : base(member)
        {
            IsBacking = TypeUtility.IsBackingField(Field);
        }
    }
}