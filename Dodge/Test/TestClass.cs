namespace Atko.Dodge.Test
{
    public class TestClass
    {
        public static int PublicStaticField;
        static int PrivateStaticField;

        public int PublicField;
        int PrivateField;

        public static int PublicStaticAutoProperty { get; set; }
        static int PrivateStaticAutoProperty { get; set; }

        public static int PublicStaticGetOnlyAutoProperty { get; }
        static int PrivateStaticGetOnlyAutoProperty { get; }

        public static int PublicStaticGetOnlyProperty => 0;
        static int PrivateStaticGetOnlyProperty => 0;

        public int PublicAutoProperty { get; set; }
        int PrivateAutoProperty { get; set; }

        public int PublicGetOnlyAutoProperty { get; }
        int PrivateGetOnlyAutoProperty { get; }

        public int PublicGetOnlyProperty => 0;
        int PrivateGetOnlyProperty => 0;
    }
}