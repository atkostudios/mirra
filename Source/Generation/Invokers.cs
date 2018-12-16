namespace Atko.Mirra.Generation
{
    public delegate object InstanceMethodInvoker(object instance, object[] arguments);

    public delegate object InstanceGetInvoker(object instance);

    public delegate void InstanceSetInvoker(object instance, object value);

    public delegate object InstanceIndexerGetInvoker(object instance, object[] index);

    public delegate void InstanceIndexerSetInvoker(object instance, object[] index, object value);

    public delegate object StaticMethodInvoker(object[] arguments);

    public delegate object StaticGetInvoker();

    public delegate void StaticSetInvoker(object value);
}