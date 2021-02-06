# ![Logo](art/icon@64x64.png) ZenIoc
**Fast Friendly Dependency Injection**

[![Coverage](https://raw.githubusercontent.com/zenmvvm/ZenIoc/develop/coverage/badge_linecoverage.svg)](https://htmlpreview.github.io/?https://raw.githubusercontent.com/zenmvvm/ZenIoc/develop/coverage/index.html) [![NuGet](https://buildstats.info/nuget/ZenIoc?includePreReleases=false)](https://www.nuget.org/packages/ZenIoc/) ![CI](https://github.com/zenmvvm/ZenIoc/workflows/CI/badge.svg?branch=main)

* **Fast**. ZenIoc was designed to be used in mobile apps. As such, quick resolving of dependencies is important.
* **Feature-Rich**. ZenIoc offers advanced features, without compromising on performance. ZenIoc supports:
  * [Constructor selection](https://github.com/zenmvvm/ZenIoc/wiki/Registration#constructor-selection): If multiple constructors, can control which one is used to build the object.
  * [Property Injection](https://github.com/zenmvvm/ZenIoc/wiki/Registration#linq-expressions): Objects which require property injection are resolved
  * [Generics](https://github.com/zenmvvm/ZenIoc/wiki/Registration#open-generic-types): Objects with a generic dependency are resolved
  * [IEnumerables](https://github.com/zenmvvm/ZenIoc/wiki/Registration#ienumerable): Several objects that implement the same interface are resolved into an Enumerable of that interface
  * Conditional / [Named dependencies](https://github.com/zenmvvm/ZenIoc/wiki/Registration#named-registrations): Objects with a conditional dependency are resolved
  * [Child Containers](https://github.com/zenmvvm/ZenIoc/wiki#child-containers): Objects can be scoped to a child container. Nesting of child containers is supported
* **Friendly**. ZenIoc offers a simple API and convenience:
  * [Smart-Resolve](https://github.com/zenmvvm/ZenIoc/wiki/Resolution#smart-resolve) will attempt to resolve an unregistered dependency
  * Singleton implementation for simple applications
  * Attribute decorators which simplify constructor selection and conditional resolution

## QuickStart

If you are new to DI, see [Why Di](https://github.com/zenmvvm/ZenIoc/wiki/Why-Di%3F) for. an introduction. If you want advanced usage, [browse the wiki](https://github.com/zenmvvm/ZenIoc/wiki).

The easiest way to **get started** is with ZenIoc's singleton implementation. This will give access to the container throughout your app by calling `IocContainer`. 

```c#
using ZenIoc;

//Register
IocContainer.Register<IBrewEquipment,FrenchPress>();

//Resolve
var frenchPress = IocContainer.Resolve<IBrewEquipment>();
```

You can even be lazy and in most use-cases leave out the `Register` step alltogether. The [Smart Resolve](https://github.com/zenmvvm/ZenIoc/wiki/Resolution#smart-resolve) feature will take care of this for you. The executable example below shows two implementations: with and without Smart Resolve.



### Executable Example

The following example instantiates five coffee-making bots. Both CoffeeBots share the same `FilteredWater`, so this is registered as a singleton instance. Each Bot uses their own`IBrewEquipment` to make coffee. In this implementation, we give all the bots a `FrenchPress` which implements `IBrewEquipment`. 

Executable code below. The `Instances` properties are just to track and report on how many of each type has been instantiated.

```c#
using System;
using ZenIoc;

public class CoffeeBot
{
    public CoffeeBot(IBrewEquipment equipment, FilteredWater water)
    {
        Console.WriteLine("Ready to brew using "
            + equipment + equipment.Instances
            +" and "
            + water + water.Instances);
    }
}

public interface IBrewEquipment { int Instances { get; } }
public class FrenchPress : IBrewEquipment
{
    static int instances;
    public int Instances => instances;
    public FrenchPress() { instances++; }
}

public class FilteredWater
{
    static int instances;
    public int Instances => instances;
    public FilteredWater() { instances++; }
}

public class Program
{
    static void Main(string[] args)
    {
        IocContainer.Register<FilteredWater>().SingleInstance(); //Singleton
        IocContainer.Register<IBrewEquipment,FrenchPress>();
        IocContainer.Register<CoffeeBot>();

        var robot1 = IocContainer.Resolve<CoffeeBot>();
        var robot2 = IocContainer.Resolve<CoffeeBot>();
        var robot3 = IocContainer.Resolve<CoffeeBot>();
        var robot4 = IocContainer.Resolve<CoffeeBot>();
        var robot5 = IocContainer.Resolve<CoffeeBot>();
    }
}

//Output:
//Ready to brew using FrenchPress1 and FilteredWater1
//Ready to brew using FrenchPress2 and FilteredWater1
//Ready to brew using FrenchPress3 and FilteredWater1
//Ready to brew using FrenchPress4 and FilteredWater1
//Ready to brew using FrenchPress5 and FilteredWater1
```

ZenIoc has a convenient [Smart Resolve](https://github.com/zenmvvm/ZenIoc/wiki/Resolution#smart-resolve) feature. This lets the user resolve instances that haven't been registered in the container. The `Main` method can be reduced to:

```c#
//SMART-RESOLVE
public class Program
{
  static void Main(string[] args)
  {
    //No Registration needed
    var robot1 = IocContainer.Resolve<CoffeeBot>();
    var robot2 = IocContainer.Resolve<CoffeeBot>();
    var robot3 = IocContainer.Resolve<CoffeeBot>();
    var robot4 = IocContainer.Resolve<CoffeeBot>();
    var robot5 = IocContainer.Resolve<CoffeeBot>();
  }
}

//Output:
//Ready to brew using FrenchPress1 and FilteredWater1
//Ready to brew using FrenchPress1 and FilteredWater2
//Ready to brew using FrenchPress1 and FilteredWater3
//Ready to brew using FrenchPress1 and FilteredWater4
//Ready to brew using FrenchPress1 and FilteredWater5
```

> :memo: Note that the ZenIoc resolves in a different lifecycle for the objects. Interfaces will be assumed to be Singletons and Types assumed to be Transient.
