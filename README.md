# **Mirra**

## A reflection library for .NET focused on simplicity and performance.

## **Installation**

| **.Net Core**                             | **Nuget CLI**                          |
|-------------------------------------------|----------------------------------------|
| ```dotnet package add Atko.Mirra```       | ```nuget install Atko.Mirra```         |

## **Getting Started**

To use `Mirra`, your entry point is ```TypeImage```. This is a wrapper for the built in system ```Type```. To access the ```TypeImage``` of a type you can use any of the following...

---
*Dynamic Lookup*
```csharp
var image = TypeImage.Get(typeof(Class));
```

*Static Lookup*
```csharp
var image = TypeImage.Get<Class>();
```

*Extension Method*
```csharp
var image = typeof(Class).Image()
```

*Implicit Conversion*
```csharp
var image = (TypeImage) typeof(Class)
```
---

For the following examples, we will using these definitions...

```csharp
class Class {
    int Count { get; }

    public Class(int count) {
        Count = count;
    }
}
```

```csharp
var instance = (object) new Class(0);
```

```csharp
var image = instance.GetType().Image();
```

---


## **Using Properties and Fields**

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
var accessor = image.Accessor(nameof(Class.Count));
```

#### *Getting*

*The value of **any** property or field can be retrieved.*
```csharp
Debug.Assert(((int) accessor.Get(instance)) == 0);
```

#### *Setting*

*The value of **any** settable property, auto property or field can be set. This means you can set get-only auto properties and readonly fields just like any other accessor.*
```csharp
accessor.Set(instance, 2); // Setting a get-only auto property.

Debug.Assert(((int) accessor.Get(instance)) == 2);
```
