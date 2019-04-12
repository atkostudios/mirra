namespace Atko.Mirra
{
    internal delegate object InstanceMethodInvoker(object instance, object[] arguments);

    internal delegate object InstanceGetInvoker(object instance);

    internal delegate void InstanceSetInvoker(object instance, object value);

    internal delegate object InstanceIndexerGetInvoker(object instance, object[] index);

    internal delegate void InstanceIndexerSetInvoker(object instance, object[] index, object value);

    internal delegate object StaticMethodInvoker(object[] arguments);

    internal delegate object StaticGetInvoker();

    internal delegate void StaticSetInvoker(object value);
}