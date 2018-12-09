namespace Atko.Dodge.Dynamic
{
    public delegate object InstanceMethodInvoker(object instance, object[] arguments);

    public delegate object InstanceGetInvoker(object instance);

    public delegate void InstanceSetInvoker(object instance, object value);

    public delegate object IndexerGetInvoker(object instance, object[] index);

    public delegate void IndexerSetInvoker(object instance, object[] index, object value);

    public delegate object StaticMethodInvoker(object[] arguments);

    public delegate object StaticGetInvoker();

    public delegate void StaticSetInvoker(object value);
}