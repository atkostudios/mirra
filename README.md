# **Mirra**

## Mirra is a reflection library for .NET focused on simplicity and performance.

## **Installation**

| **.Net Core**                             | **Nuget CLI**                          |
|-------------------------------------------|----------------------------------------|
| ```dotnet package add Atko.Mirra```       | ```nuget install Atko.Mirra```         |


# **Features**

* Simplified interface based on the original .NET reflection API
* Usable on all platforms
* Performance via by lazy byte-code generation if available.
* Fast, cached access to properties, fields, methods, indexers and constructors
* Unified usage of fields and properties
* 100% thread safe
* Designed to work well with generic types
* Easy iteration and query of inheritance trees

# **Usage**

To use `Mirra`, your entry point is ```TypeImage```. This is a wrapper for the built in system ```Type```. To access the ```TypeImage``` of a type you can use any of the following...

---
*Dynamic Lookup*
```csharp
var type = TypeImage.Get(typeof(Class));
```

*Static Lookup*
```csharp
var type = TypeImage.Get<Class>();
```

*Extension Method*
```csharp
var type = typeof(Class).Image()
```

*Implicit Conversion*
```csharp
var type = (TypeImage) typeof(Class)
```
---

For the following examples, we will using these definitions...

```csharp
class Class {
    public int Count { get; }

    public Class() { }

    public Class(int count) {
        Count = count;
    }

    public Class Add(int number) {
        return new Class(Count + number);
    }
}
```

```csharp
var instance = new Class(0);
```

```csharp
var type = instance.GetType().Image();
```

---

## **Properties and Fields**

* *To a property, use `TypeImage.Property(string name)`;*
* *To access a field, use `TypeImage.Field(string name)`;*
* *To access a property or field, use `TypeImage.Accessor(string name)`;*

Both `PropertyImage` and `FieldImage` inherit from the base class `AccessorImage`.

Because of this, we don't have to worry if we are using a property or field if we don't actually care. In the following code, `Count` could be either one and it would work the same.

---
*Mirra can access any member, regardless of visibility.*

*Mirra will resolve members nearest to the most derived class. This means that if a superclass has a private member with the same name as a member in a derived class, the member from the derived class will be returned by a lookup for that name. If we **do** want the member in the superclass, we can simply use the `TypeImage` of the superclass to retrieve it.*

---

#### *Accessor*
```csharp
var accessor = type.Accessor(nameof(Class.Count));
```

#### *Getting*

*The value of **any** property or field can be retrieved.*
```csharp
Debug.Assert(((int) type.Get(instance)) == 0);
```

#### *Setting*

*The value of **any** settable property, auto property or field can be set. This means you can set get-only auto properties and readonly fields just like any other accessor.*
```csharp
accessor.Set(instance, 2); // Setting a get-only auto property.

Debug.Assert(instance.Count == 2);
```

## **Methods**

```csharp
var method = type.Method(nameof(Class.Increment), typeof(int));
```

#### *Calling*

```csharp
Debug.Assert((int) accessor.Get(instance)) == 0);

var result = method.Call(1);

Debug.Assert((int) accessor.Get(result)) == 1);
```

## **Constructors**

```csharp
var constructor = type.Constructor(typeof(int));
```

#### *Calling*

*This is much faster than using `Activator`.*

```csharp
var constructed = (Class) constructor.Call(1);

Debug.Assert(constructed.Count == 1);
```
