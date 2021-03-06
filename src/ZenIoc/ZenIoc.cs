//MIT License

//Copyright(c) 2020 Guy Antoine

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ZenIoc
{
    /// <summary>
    /// Represents a set of configurable options when creating a new instance of the container.
    /// </summary>
    public class ContainerOptions
    {
        private static readonly Lazy<ContainerOptions> DefaultOptions =
            new Lazy<ContainerOptions>(() => new ContainerOptions());

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerOptions"/> class.
        /// </summary>
        public ContainerOptions()
        {
            TryResolveUnregistered = true;
            ResolveShouldBubbleUpContainers = true;
            ShouldPickGreediestConstructor = true;
        }

        /// <summary>
        /// Gets the default <see cref="ContainerOptions"/> used across all <see cref="IocContainer"/> instances.
        /// </summary>
        public static ContainerOptions Default => DefaultOptions.Value;

        /// <summary>
        /// If <c>true</c> will attempt to resolve
        ///  the class anyway. If successful it will
        ///  add the resolved class to the container.
        /// </summary>
        /// <remarks>
        /// The default value is true.
        /// </remarks>
        public bool TryResolveUnregistered { get; set; }


        /// <summary>
        /// Applies when resolving from a child container
        ///  and the type to resolve is not registered in
        ///  the child container. If <c>true</c>, ZenIoc
        ///  will search the parent container for the type.
        ///  If nested children, will bubble up to the ultimate
        ///  ancestor.
        /// </summary>
        /// <remarks>
        /// The default value is true.
        /// </remarks>
        public bool ResolveShouldBubbleUpContainers { get; set; }

        /// <summary>
        /// If no constructor is specified and more than one 
        /// constructor is available, resolve will pick the
        /// one with the most parameters if <see cref="ShouldPickGreediestConstructor"/> is true.
        /// Otherwise the constructor with the least parameters
        /// will be selected.
        /// </summary>
        /// <remarks>Default is true</remarks>
        public bool ShouldPickGreediestConstructor { get; set; }
    }

    //todo list registrations
    /// <summary>
    /// Interface for the dependency injection container
    /// </summary>
    public interface IIocContainer : IDisposable
    {
        /// <summary>
        /// Optional name to identify the container with. Useful if using more than one container.
        /// See <see cref="GetContainer"/> method.
        /// </summary> 
        string Name { get; set; }

        /// <summary>
        /// Retrieves a container, identified by its <see cref="Name"/>
        /// </summary>
        /// <param name="name">The container's name</param>
        /// <returns></returns>
        IIocContainer GetContainer(string name);

        /// <summary>
        /// Get the current container's parent container.
        ///  Will be <c>null</c> if the current instance
        ///  is not a child container.
        /// </summary>
        IIocContainer GetParent();

        /// <summary>
        /// Get the current container's child container. 
        ///  Will be <c>null</c> if no child has been created.
        /// </summary>
        IReadOnlyList<IIocContainer> GetChildren();

        /// <summary>
        /// Registers a new child container and sets
        ///  its Parent to the current
        ///  instance.
        /// </summary>
        /// <returns>A new <see cref="IIocContainer"/> child
        ///  container</returns>
        IIocContainer NewChildContainer(string name = null);

        /// <summary>
        /// Registers a Type in the container.
        /// </summary>
        /// <param name="concreteType">Type to be instantiated</param>
        /// <param name="resolvedType">Type to be resolved if different from <paramref name="concreteType"/></param>
        /// <param name="key">Optional key which allows multiple registrations with the same <paramref name="resolvedType"/></param>
        /// <returns>Fluent API</returns>
        IRegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null);

        /// <summary>
        /// Registers a Type in the container.
        /// </summary>
        /// <param name="concreteType">Type to be instantiated</param>
        /// <param name="resolvedType">Type to be resolved if different from <paramref name="concreteType"/></param>
        /// <param name="key">Optional key which allows multiple registrations with the same <paramref name="resolvedType"/></param>
        /// <param name="constructorParameters">If provided, will specify the specific contructor to be used to instantiate the <paramref name="concreteType"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        IRegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters);

        /// <summary>
        /// Register a Type in the container
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TConcrete>()
            where TConcrete : notnull;
        /// <summary>
        /// Register a Type in the container
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TResolved, TConcrete>()
            where TConcrete : notnull, TResolved;

        /// <summary>
        /// Register a Type in the container, with a key.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TConcrete>(string key)
            where TConcrete : notnull;

        /// <summary>
        /// Register a Type in the container, with a key.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TResolved, TConcrete>(string key)
            where TConcrete : notnull, TResolved;


        /// <summary>
        /// Register a Type in the container while specifying a constructor.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull;

        /// <summary>
        /// Register a Type in the container, specifying a constructor.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TResolved, TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull, TResolved;

        /// <summary>
        /// Register a Type in the container, with a key and specifying a constructor.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull;

        /// <summary>
        /// Register a Type in the container, with a key and specifying a constructor.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        RegisterOptions Register<TResolved, TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull, TResolved;

        /// <summary>
        /// Register a lambda expression that returns an instance.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="instanceDelegate">The lambda expression</param>
        /// <returns>Fluent API</returns>
        RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IIocContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved;

        /// <summary>
        /// Register a lambda expression that returns an instance, with a key.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="instanceDelegate">The lambda expression</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IIocContainer, TConcrete>> instanceDelegate, string key)
            where TConcrete : notnull, TResolved;

        /// <summary>
        /// Register an object that has already been instantiated
        /// </summary>
        /// <param name="instance">The object instance</param>
        void RegisterInstance(object instance);

        /// <summary>
        /// Register an object that has already been instantiated
        /// </summary>
        /// <typeparam name="TResolved">Type to be resolved as</typeparam>
        /// <param name="instance">The object instance</param>
        void RegisterInstance<TResolved>(object instance)
            where TResolved : notnull;

        /// <summary>
        /// Register an object that has already been instantiated, with a key
        /// </summary>
        /// <param name="instance">The object instance</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        void RegisterInstance(object instance, string key);

        /// <summary>
        /// Register an object that has already been instantiated, with a key
        /// </summary>
        /// <typeparam name="TResolved">Type to be resolved as</typeparam>
        /// <param name="instance">The object instance</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        void RegisterInstance<TResolved>(object instance, string key)
            where TResolved : notnull;

        /// <summary>
        /// Register all implementations of an interface, or all derived classes from an abstract class.
        /// </summary>
        /// <typeparam name="TResolved">The interface / abstract class</typeparam>
        /// <param name="lifeCycle"></param>
        void RegisterTypesOf<TResolved>(LifeCycle lifeCycle = LifeCycle.Transient) where TResolved : class;

        /// <summary>
        /// Pre-compile the container to enable faster
        ///  first-time resolution
        /// </summary>
        void Compile();

        /// <summary>
        /// Resolve a Type from the container
        /// </summary>
        /// <typeparam name="T">Type to Resolve</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/></returns>
        T Resolve<T>() where T : notnull;

        /// <summary>
        /// Resolve a Type from the container that has the specified key
        /// </summary>
        /// <typeparam name="T">Type to Resolve</typeparam>
        /// <param name="key">The key with which it was registered</param>
        /// <returns>An instance of type <typeparamref name="T"/></returns>
        T Resolve<T>(string key) where T : notnull;

        /// <summary>
        /// Resolve the specified <c>Type</c>
        /// </summary>
        /// <param name="type">The Type to resolve</param>
        /// <returns>An object of the specified type</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolve the specified <c>Type</c>, with the specified key
        /// </summary>
        /// <param name="type">The Type to resolve</param>
        /// <param name="key">The key with which it was registered</param>
        /// <returns>An object of the specified type</returns>
        object Resolve(Type type, string key);

        /// <summary>
        /// De-register a Type from the container.
        ///  <c>IDisposable</c> will be called if it's
        ///  implemented.
        /// </summary>
        /// <typeparam name="T">The Type to de-register</typeparam>
        void Unregister<T>()
            where T : notnull;

        /// <summary>
        /// De-register a Type from the container, with the specified key
        ///  <c>IDisposable</c> will be called if it's
        ///  implemented.
        /// </summary>
        /// <typeparam name="T">The Type to de-register</typeparam>
        /// <param name="key">The key with which it was registered</param>
        void Unregister<T>(string key)
            where T : notnull;

        /// <summary>
        /// De-register everything in the container, calling
        ///  <c>IDisposable</c> on all objects that implement
        ///  it.
        /// </summary>
        void UnregisterAll();
    }

    /// <summary>
    /// Extensions to avoid having to cast IocContainer instance
    /// to IIocContainer when trying to access its methods.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IIocContainerExtensions
    {
        /// <summary>
        /// Retrieves a container, identified by its <see cref="IIocContainer.Name"/>
        /// </summary>
        /// <param name="name">The container's name</param>
        /// <returns></returns>
        /// <param name="diContainer"></param>
        public static IIocContainer GetContainer(this IIocContainer diContainer, string name)
            => diContainer.GetContainer(name);

        /// <summary>
        /// Get the current container's child container. 
        ///  Will be <c>null</c> if no child has been created.
        /// </summary>
        public static IReadOnlyList<IIocContainer> GetChildren(this IIocContainer diContainer)
            => diContainer.GetChildren();

        /// <summary>
        /// Registers a new child container and sets
        ///  its Parent to the current
        ///  instance.
        /// </summary>
        /// <returns>A new <see cref="IIocContainer"/> child
        ///  container</returns>
        public static IIocContainer NewChildContainer(this IIocContainer container, string name = null)
            => container.NewChildContainer(name);

        /// <summary>
        /// Registers a Type in the container.
        /// </summary>
        /// <param name="concreteType">Type to be instantiated</param>
        /// <param name="resolvedType">Type to be resolved if different from <paramref name="concreteType"/></param>
        /// <param name="key">Optional key which allows multiple registrations with the same <paramref name="resolvedType"/></param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static IRegisterOptions RegisterType(this IIocContainer diContainer, Type concreteType, Type resolvedType = null, string key = null)
            => diContainer.RegisterType(concreteType, resolvedType, key);

        /// <summary>
        /// Registers a Type in the container.
        /// </summary>
        /// <param name="concreteType">Type to be instantiated</param>
        /// <param name="resolvedType">Type to be resolved if different from <paramref name="concreteType"/></param>
        /// <param name="key">Optional key which allows multiple registrations with the same <paramref name="resolvedType"/></param>
        /// <param name="constructorParameters">If provided, will specify the specific contructor to be used to instantiate the <paramref name="concreteType"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static IRegisterOptions RegisterType(this IIocContainer diContainer, Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters)
            => diContainer.RegisterType(concreteType, resolvedType, key, constructorParameters);

        /// <summary>
        /// Register a Type in the container
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TConcrete>(this IIocContainer diContainer)
            where TConcrete : notnull
            => diContainer.Register<TConcrete>();
        /// <summary>
        /// Register a Type in the container
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TResolved, TConcrete>(this IIocContainer diContainer)
            where TConcrete : notnull, TResolved
            => diContainer.Register<TResolved, TConcrete>();

        /// <summary>
        /// Register a Type in the container, with a key.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TConcrete>(this IIocContainer diContainer, string key)
            where TConcrete : notnull
            => diContainer.Register<TConcrete>(key);

        /// <summary>
        /// Register a Type in the container, with a key.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TResolved, TConcrete>(this IIocContainer diContainer, string key)
            where TConcrete : notnull, TResolved
            => diContainer.Register<TResolved, TConcrete>(key);


        /// <summary>
        /// Register a Type in the container while specifying a constructor.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TConcrete>(this IIocContainer diContainer, params Type[] constructorParameters)
            where TConcrete : notnull
            => diContainer.Register<TConcrete>(constructorParameters);

        /// <summary>
        /// Register a Type in the container, specifying a constructor.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TResolved, TConcrete>(this IIocContainer diContainer, params Type[] constructorParameters)
            where TConcrete : notnull, TResolved
            => diContainer.Register<TResolved, TConcrete>(constructorParameters);

        /// <summary>
        /// Register a Type in the container, with a key and specifying a constructor.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TConcrete>(this IIocContainer diContainer, string key, params Type[] constructorParameters)
            where TConcrete : notnull
            => diContainer.Register<TConcrete>(key, constructorParameters);

        /// <summary>
        /// Register a Type in the container, with a key and specifying a constructor.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions Register<TResolved, TConcrete>(this IIocContainer diContainer, string key, params Type[] constructorParameters)
            where TConcrete : notnull, TResolved
            => diContainer.Register<TResolved, TConcrete>(key, constructorParameters);

        /// <summary>
        /// Register a lambda expression that returns an instance.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="instanceDelegate">The lambda expression</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions RegisterExplicit<TResolved, TConcrete>(this IIocContainer diContainer, Expression<Func<IIocContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved
            => diContainer.RegisterExplicit<TResolved, TConcrete>(instanceDelegate);

        /// <summary>
        /// Register a lambda expression that returns an instance, with a key.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="instanceDelegate">The lambda expression</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        ///  <param name="diContainer"></param>
        public static RegisterOptions RegisterExplicit<TResolved, TConcrete>(this IIocContainer diContainer, Expression<Func<IIocContainer, TConcrete>> instanceDelegate, string key)
            where TConcrete : notnull, TResolved
            => diContainer.RegisterExplicit<TResolved, TConcrete>(instanceDelegate, key);

        /// <summary>
        /// Register an object that has already been instantiated
        /// </summary>
        /// <param name="instance">The object instance</param>
        ///  <param name="diContainer"></param>
        public static void RegisterInstance(this IIocContainer diContainer, object instance)
            => diContainer.RegisterInstance(instance);

        /// <summary>
        /// Register an object that has already been instantiated
        /// </summary>
        /// <typeparam name="TResolved">Type to be resolved as</typeparam>
        /// <param name="instance">The object instance</param>
        ///  <param name="diContainer"></param>
        public static void RegisterInstance<TResolved>(this IIocContainer diContainer, object instance)
            where TResolved : notnull
            => diContainer.RegisterInstance<TResolved>(instance);

        /// <summary>
        /// Register an object that has already been instantiated, with a key
        /// </summary>
        /// <param name="instance">The object instance</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        ///  <param name="diContainer"></param>
        public static void RegisterInstance(this IIocContainer diContainer, object instance, string key)
            => diContainer.RegisterInstance(instance, key);

        /// <summary>
        /// Register an object that has already been instantiated, with a key
        /// </summary>
        /// <typeparam name="TResolved">Type to be resolved as</typeparam>
        /// <param name="instance">The object instance</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        ///  <param name="diContainer"></param>
        public static void RegisterInstance<TResolved>(this IIocContainer diContainer, object instance, string key)
            where TResolved : notnull
            => diContainer.RegisterInstance<TResolved>(instance, key);

        /// <summary>
        /// Register all implementations of an interface, or all derived classes from an abstract class.
        /// </summary>
        /// <typeparam name="TResolved">The interface / abstract class</typeparam>
        /// <param name="lifeCycle"></param>
        ///  <param name="diContainer"></param>
        public static void RegisterTypesOf<TResolved>(this IIocContainer diContainer, LifeCycle lifeCycle)
            where TResolved : notnull
            => diContainer.RegisterTypesOf<TResolved>(lifeCycle);

        /// <summary>
        /// Pre-compile the container to enable faster
        ///  first-time resolution
        /// </summary>
        ///  <param name="diContainer"></param>
        public static void Compile(this IIocContainer diContainer)
            => diContainer.Compile();

        /// <summary>
        /// Resolve a Type from the container
        /// </summary>
        /// <typeparam name="T">Type to Resolve</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/></returns>
        ///  <param name="diContainer"></param>
        public static T Resolve<T>(this IIocContainer diContainer) where T : notnull
            => diContainer.Resolve<T>();

        /// <summary>
        /// Resolve a Type from the container that has the specified key
        /// </summary>
        /// <typeparam name="T">Type to Resolve</typeparam>
        /// <param name="key">The key with which it was registered</param>
        /// <returns>An instance of type <typeparamref name="T"/></returns>
        ///  <param name="diContainer"></param>
        public static T Resolve<T>(this IIocContainer diContainer, string key) where T : notnull
            => diContainer.Resolve<T>(key);

        /// <summary>
        /// Resolve the specified <c>Type</c>
        /// </summary>
        /// <param name="type">The Type to resolve</param>
        /// <returns>An object of the specified type</returns>
        ///  <param name="diContainer"></param>
        public static object Resolve(this IIocContainer diContainer, Type type)
            => diContainer.Resolve(type);

        /// <summary>
        /// Resolve the specified <c>Type</c>, with the specified key
        /// </summary>
        /// <param name="type">The Type to resolve</param>
        /// <param name="key">The key with which it was registered</param>
        /// <returns>An object of the specified type</returns>
        ///  <param name="diContainer"></param>
        public static object Resolve(this IIocContainer diContainer, Type type, string key)
            => diContainer.Resolve(type, key);

        /// <summary>
        /// De-register a Type from the container.
        ///  <c>IDisposable</c> will be called if it's
        ///  implemented.
        /// </summary>
        /// <typeparam name="T">The Type to de-register</typeparam>
        ///  <param name="diContainer"></param>
        public static void Unregister<T>(this IIocContainer diContainer)
            where T : notnull
            => diContainer.Unregister<T>();

        /// <summary>
        /// De-register a Type from the container, with the specified key
        ///  <c>IDisposable</c> will be called if it's
        ///  implemented.
        /// </summary>
        /// <typeparam name="T">The Type to de-register</typeparam>
        /// <param name="key">The key with which it was registered</param>
        ///  <param name="diContainer"></param>
        public static void Unregister<T>(this IIocContainer diContainer, string key)
            where T : notnull
            => diContainer.Unregister<T>(key);

        /// <summary>
        /// De-register everything in the container, calling
        ///  <c>IDisposable</c> on all objects that implement
        ///  it.
        /// </summary>
        ///  <param name="diContainer"></param>
        public static void UnregisterAll(this IIocContainer diContainer)
            => diContainer.UnregisterAll();

        /// <summary>
        /// Disposes the container and all its registrations
        /// </summary>
        /// <param name="diContainer"></param>
        public static void Dispose(this IIocContainer diContainer)
            => diContainer.Dispose();
    }

    /// <summary>
    /// Dependency injection container
    /// </summary>
    public class IocContainer : IIocContainer
    {
        string name;
        ///<inheritdoc/>
        public string Name
        {
            get => name;
            set
            {
                if (name != null)
                    containers.TryRemove(name, out _);

                if (containers.TryAdd(value, this))
                    name = value;
                else throw new ArgumentException("Could not set Name. Ensure the container's name is unique. " +
                    "Less common reason could be that the operation is blocked in a multi-threaded application.");
            }
        }

        internal static readonly ConcurrentDictionary<string, IIocContainer> containers = new ConcurrentDictionary<string, IIocContainer>();

        /// <summary>
        /// Retrieves a container, identified by its <see cref="Name"/>
        /// </summary>
        /// <param name="name">The container's name</param>
        /// <returns></returns>
        public static IIocContainer GetContainer(string name)
            => (Instance as IIocContainer).GetContainer(name);
        ///<inheritdoc/>
        IIocContainer IIocContainer.GetContainer(string name)
        => containers.TryGetValue(name, out IIocContainer container)
            ? container
            : throw new KeyNotFoundException(
                "Couldn't retrieve container. Is the name correct?");


        /// <summary>
        /// Global settings applied across all containers
        /// </summary>
        internal static readonly ContainerOptions options = new ContainerOptions();

        /// <summary>
        /// Instantiate a new container
        /// </summary>
        /// <param name="name">Optional name for the container. Helpful if you plan on using many containers.</param>
        public IocContainer(string name = null)
        {
            Name = name ?? Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IocContainer"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ContainerOptions"/> instances that represents the configurable options.</param>
        /// <param name="name">Optional name for the container. Helpful if you plan on using many containers.</param>
        public IocContainer(ContainerOptions options, string name = null) : this(o =>
        {
            o.TryResolveUnregistered = options.TryResolveUnregistered;
            o.ResolveShouldBubbleUpContainers = options.ResolveShouldBubbleUpContainers;
        }, name)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="IocContainer"/> class.
        /// </summary>
        /// <param name="configureOptions">A delegate used to configure <see cref="ContainerOptions"/>.</param>
        /// <param name="name">Optional name for the container. Helpful if you plan on using many containers.</param>
        public IocContainer(Action<ContainerOptions> configureOptions, string name = null) : this(name)
        {
            configureOptions(options);
        }

        /// <summary>
        /// Constructs the container with the given dictionary
        /// </summary>
        /// <param name="container"></param>
        /// <param name="options">The <see cref="ContainerOptions"/> instances that represents the configurable options.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IocContainer(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, ContainerOptions options) : this(options)
        {
            this.container = container;
        }

        /// <summary>
        /// Sets the dictionary to the provided dictionary
        /// </summary>
        /// <param name="container"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetContainer(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container)
            => Instance.container = container;

        /// <summary>
        /// Resets the container to the default container.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ResetContainer()
        {
            IocContainer.containers.Clear();
            Instance = new IocContainer();
        }

        /// <summary>
        /// Initializes the Singleton intance of the <see cref="IocContainer"/>
        /// class with user-specified options. 
        /// </summary>
        /// <remarks>Will only apply to the singleton, and will recreate the container (erasing any registrations)</remarks>
        /// <param name="options">The <see cref="ContainerOptions"/> instances that represents the configurable options.</param>
        public static void Initialize(ContainerOptions options)
        {
            IocContainer.containers.Clear();
            Instance = new IocContainer(options);
        }

        /// <summary>
        /// Initializes the Singleton intance of the <see cref="IocContainer"/>
        /// class with user-specified options. 
        /// </summary>
        /// <remarks>Will only apply to the singleton, and will recreate the container (erasing any registrations)</remarks>
        /// <param name="configureOptions">A delegate used to configure <see cref="ContainerOptions"/>.</param>
        public static void Initialize(Action<ContainerOptions> configureOptions)
        {
            IocContainer.containers.Clear();
            Instance = new IocContainer(configureOptions);
        }

        internal static IocContainer Instance { get; private set; } = new IocContainer();

        private IIocContainer parent;
        ///<inheritdoc/>
        public IIocContainer GetParent()
            => parent;

        private readonly List<IIocContainer> children = new List<IIocContainer>();
        /// <summary>
        /// Get the current container's child container. 
        ///  Will be <c>null</c> if no child has been created.
        /// </summary>
        public static IReadOnlyList<IIocContainer> GetChildren()
            => (Instance as IIocContainer).GetChildren();
        ///<inheritdoc/>
        IReadOnlyList<IIocContainer> IIocContainer.GetChildren()
            => children;

        internal ConcurrentDictionary<Tuple<Type, string>, MetaObject> container = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
        internal ConcurrentDictionary<Tuple<Type, string>, MetaObject> parentContainer;
        IEnumerable<Type> assemblyTypesCache;

        /// <summary>
        /// Registers a new child container and sets
        ///  its Parent to the current
        ///  instance.
        /// </summary>
        /// <returns>A new <see cref="IIocContainer"/> child
        ///  container</returns>
        public static IIocContainer NewChildContainer(string name = null)
            => (Instance as IIocContainer).NewChildContainer(name);
        ///<inheritdoc/>
        IIocContainer IIocContainer.NewChildContainer(string name)
        {
            var child = new IocContainer(name)
            {
                parentContainer = this.container,
                parent = this
            };

            this.children.Add(child);

            return child;
        }


        #region Registration
        #region Register

        /// <summary>
        /// Register a Type in the container
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TConcrete>()
            where TConcrete : notnull
            => (Instance as IIocContainer).Register<TConcrete>(constructorParameters: null);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TConcrete>()
            => (this as IIocContainer).Register<TConcrete>(constructorParameters: null);


        /// <summary>
        /// Register a Type in the container while specifying a constructor.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull
            => (Instance as IIocContainer).Register<TConcrete>(constructorParameters);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TConcrete>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));


        /// <summary>
        /// Register a Type in the container
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TResolved, TConcrete>()
            where TConcrete : notnull, TResolved
            => (Instance as IIocContainer).Register<TResolved, TConcrete>(constructorParameters: null);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TResolved, TConcrete>()
            => (this as IIocContainer).Register<TResolved, TConcrete>(constructorParameters: null);

        /// <summary>
        /// Register a Type in the container, specifying a constructor.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TResolved, TConcrete>(params Type[] constructorParameters)
            where TConcrete : notnull, TResolved
            => (Instance as IIocContainer).Register<TResolved, TConcrete>(constructorParameters);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TResolved, TConcrete>(params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        #endregion
        #region Register with Key
        /// <summary>
        /// Register a Type in the container, with a key.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TConcrete>(string key)
            where TConcrete : notnull
            => (Instance as IIocContainer).Register<TConcrete>(key, constructorParameters: null);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TConcrete>(string key)
            => (this as IIocContainer).Register<TConcrete>(key, constructorParameters: null);

        /// <summary>
        /// Register a Type in the container, with a key and specifying a constructor.
        /// </summary>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull
            => (Instance as IIocContainer).Register<TConcrete>(key, constructorParameters);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TConcrete>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, null, key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        /// <summary>
        /// Register a Type in the container, with a key.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TResolved, TConcrete>(string key)
            where TConcrete : notnull, TResolved
            => (Instance as IIocContainer).Register<TResolved, TConcrete>(key, constructorParameters: null);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TResolved, TConcrete>(string key)
            => (this as IIocContainer).Register<TResolved, TConcrete>(key, constructorParameters: null);

        /// <summary>
        /// Register a Type in the container, with a key and specifying a constructor.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="constructorParameters">Will specify the specific contructor to be used to instantiate the <typeparamref name="TConcrete"/>. A constructor with matching parameters will be used.</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions Register<TResolved, TConcrete>(string key, params Type[] constructorParameters)
            where TConcrete : notnull, TResolved
            => (Instance as IIocContainer).Register<TResolved, TConcrete>(key, constructorParameters);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.Register<TResolved, TConcrete>(string key, params Type[] constructorParameters)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, constructorParameters)));



        #endregion

        #region RegisterExplicit
        ///<inheritdoc/>
        Expression<Func<TInput, object>> CastToUntypedOutput<TInput, TOutput>
                (Expression<Func<TInput, TOutput>> expression)
        {
            // Add the boxing operation, but get a weakly typed expression
            Expression converted = Expression.Convert
                 (expression.Body, typeof(object));
            // Use Expression.Lambda to get back to strong typing
            return Expression.Lambda<Func<TInput, object>>
                 (converted, expression.Parameters);
        }



        /// <summary>
        /// Register a lambda expression that returns an instance.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="instanceDelegate">The lambda expression</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IIocContainer, TConcrete>> instanceDelegate)
            where TConcrete : notnull, TResolved
            => (Instance as IIocContainer).RegisterExplicit<TResolved, TConcrete>(instanceDelegate);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.RegisterExplicit<TResolved, TConcrete>(Expression<Func<IIocContainer, TConcrete>> instanceDelegate)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), null, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate), new Tuple<Type, string>(typeof(TResolved), null))));



        /// <summary>
        /// Register a lambda expression that returns an instance, with a key.
        /// </summary>
        /// <typeparam name="TResolved">Type that will be called to resolve</typeparam>
        /// <typeparam name="TConcrete">Type to be instantiated</typeparam>
        /// <param name="instanceDelegate">The lambda expression</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        /// <returns>Fluent API</returns>
        public static RegisterOptions RegisterExplicit<TResolved, TConcrete>(Expression<Func<IIocContainer, TConcrete>>
 instanceDelegate, string key)
            where TConcrete : notnull, TResolved
                => (Instance as IIocContainer).RegisterExplicit<TResolved, TConcrete>(instanceDelegate, key);

        ///<inheritdoc/>
        RegisterOptions IIocContainer.RegisterExplicit<TResolved, TConcrete>(Expression<Func<IIocContainer, TConcrete>> instanceDelegate, string key)
            => new RegisterOptions(
                container,
                InternalRegister(container, typeof(TResolved), key, new MetaObject(typeof(TConcrete), LifeCycle.Transient, CastToUntypedOutput(instanceDelegate), new Tuple<Type, string>(typeof(TResolved), key))));

        #endregion

        #region Register Instance

        /// <summary>
        /// Register an object that has already been instantiated
        /// </summary>
        /// <param name="instance">The object instance</param>
        public static void RegisterInstance(object instance)
            => (Instance as IIocContainer).RegisterInstance(instance);

        ///<inheritdoc/>
        void IIocContainer.RegisterInstance(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, null, new MetaObject(instance)));



        /// <summary>
        /// Register an object that has already been instantiated
        /// </summary>
        /// <typeparam name="TResolved">Type to be resolved as</typeparam>
        /// <param name="instance">The object instance</param>
        public static void RegisterInstance<TResolved>(object instance)
            where TResolved : notnull
            => (Instance as IIocContainer).RegisterInstance<TResolved>(instance);
        //todo Validate TResolved : TConcrete - perhaps bring type conversion forward to here : Can return typeof(TResolved to fit into one method call)

        ///<inheritdoc/>
        void IIocContainer.RegisterInstance<TResolved>(object instance)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(TResolved), null, new MetaObject(instance)));


        /// <summary>
        /// Register an object that has already been instantiated, with a key
        /// </summary>
        /// <param name="instance">The object instance</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        public static void RegisterInstance(object instance, string key)
            => (Instance as IIocContainer).RegisterInstance(instance, key);

        ///<inheritdoc/>
        void IIocContainer.RegisterInstance(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, null, key, new MetaObject(instance)));


        /// <summary>
        /// Register an object that has already been instantiated, with a key
        /// </summary>
        /// <typeparam name="TResolved">Type to be resolved as</typeparam>
        /// <param name="instance">The object instance</param>
        /// <param name="key">Named Type which allows for multiple registrations of the same Type, identified with different keys</param>
        public static void RegisterInstance<TResolved>(object instance, string key)
            where TResolved : notnull
            => (Instance as IIocContainer).RegisterInstance<TResolved>(instance, key);

        ///<inheritdoc/>
        void IIocContainer.RegisterInstance<TResolved>(object instance, string key)
            => new RegisterOptions(
                container
                , InternalRegister(container, typeof(TResolved), key, new MetaObject(instance)));


        /// <summary>
        /// Register all implementations of an interface, or all derived classes from an abstract class.
        /// </summary>
        /// <typeparam name="TResolved">The interface / abstract class</typeparam>
        /// <param name="lifeCycle"></param>
        public static void RegisterTypesOf<TResolved>(LifeCycle lifeCycle = LifeCycle.Transient) where TResolved : class
            => (Instance as IIocContainer).RegisterTypesOf<TResolved>(lifeCycle);
        ///<inheritdoc/>
        void IIocContainer.RegisterTypesOf<TResolved>(LifeCycle lifeCycle)
            => InternalRegisterTypesOf(container, typeof(TResolved), lifeCycle);

        void InternalRegisterTypesOf(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolved, LifeCycle lifeCycle)
        {
            IRegisterOptions registerOptions;
            IEnumerable<Type> types;

            if (resolved.IsInterface)
                types = resolved.Assembly
                .GetTypes()
                .Where(t => t
                    .GetInterfaces()
                    .Contains(resolved)
                    && !t.IsAbstract);
            else if (resolved.IsAbstract)
                types = resolved.Assembly
                .GetTypes()
                .Where(t => t
                    .IsSubclassOf(resolved));
            else
                throw new RegisterException($"{nameof(RegisterTypesOf)} : {resolved} should be an interace or abstract class");

            foreach (Type type in types)
            {
                //Register each type
                registerOptions =
                    new RegisterOptions(
                        container,
                        InternalRegister(
                            container,
                            resolved,
                            type.Name,
                            new MetaObject(
                                type,
                                LifeCycle.Transient,
                                null)));
                if (lifeCycle == LifeCycle.Singleton)
                    registerOptions.SingleInstance();
            }

            //Register the IEnumerable
            registerOptions =
                new RegisterOptions(
                    container,
                    InternalRegister(
                        container,
                        null,
                        null,
                        new MetaObject(
                            Type.GetType(
                               $"System.Collections.Generic" +
                               $".IEnumerable`1[[" +
                               $"{resolved.AssemblyQualifiedName}]]"),
                            LifeCycle.Transient,
                            null)));
            if (lifeCycle == LifeCycle.Singleton)
                registerOptions.SingleInstance();
        }


        #endregion

        #region Register Type
        /// <summary>
        /// Registers a Type in the container.
        /// </summary>
        /// <param name="concreteType">Type to be instantiated</param>
        /// <param name="resolvedType">Type to be resolved if different from <paramref name="concreteType"/></param>
        /// <param name="key">Optional key which allows multiple registrations with the same <paramref name="resolvedType"/></param>
        /// <returns>Fluent API</returns>
        public static IRegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null)
            => (Instance as IIocContainer).RegisterType(concreteType, resolvedType, key);

        ///<inheritdoc/>
        IRegisterOptions IIocContainer.RegisterType(Type concreteType, Type resolvedType, string key)
            => (this as IIocContainer).RegisterType(concreteType, resolvedType, key, constructorParameters: null);



        /// <summary>
        /// Registers a Type in the container.
        /// </summary>
        /// <param name="concreteType">Type to be instantiated</param>
        /// <param name="resolvedType">Type to be resolved if different from <paramref name="concreteType"/></param>
        /// <param name="key">Optional key which allows multiple registrations with the same <paramref name="resolvedType"/></param>
        /// <param name="constructorParameters">If provided, will specify the specific contructor to be used to instantiate the <paramref name="concreteType"/>. A constructor with matching parameters will be used.</param>
        /// <returns>Fluent API</returns>
        public static IRegisterOptions RegisterType(Type concreteType, Type resolvedType = null, string key = null, params Type[] constructorParameters)
            => (Instance as IIocContainer).RegisterType(concreteType, resolvedType, key, constructorParameters);

        ///<inheritdoc/>
        IRegisterOptions IIocContainer.RegisterType(Type concreteType, Type resolvedType, string key, params Type[] constructorParameters)
        {
            return new RegisterOptions(
                container,
                InternalRegister(container, resolvedType, key,
                    new MetaObject(
                        concreteType,
                        LifeCycle.Transient,
                        constructorParameters)));
        }
        //todo validatee resolvedType:TConcrete

        #endregion

        /// <summary>
        /// Pre-compile the container to enable faster
        ///  first-time resolution
        /// </summary>
        public static void Compile()
            => (Instance as IIocContainer).Compile();

        ///<inheritdoc/>
        void IIocContainer.Compile()
        {
            foreach (var keyValuePair in container)
            {
                if (keyValuePair.Value.ActivationExpression == null)
                    MakeNewExpression(container, keyValuePair.Value);
            }
        }

        internal Tuple<Type, string> InternalRegister(
            ConcurrentDictionary<Tuple<Type, string>, MetaObject> container,
            Type resolvedType,
            string key,
            MetaObject metaObject
            )
        {
            var containerKey = new Tuple<Type, string>(
                resolvedType ?? metaObject.TConcrete, key);

            if (!container.TryAdd(containerKey, metaObject))
            {
                //todo ContainerOption to not throw if already contains

                var builder = new StringBuilder();
                builder.Append($"{containerKey.Item1} is already registered");
                if (containerKey.Item2 != null)
                    builder.Append($" with key '{nameof(containerKey.Item2)}'");
                builder.Append(".");
                throw new RegisterException(builder.ToString());
            }

            return containerKey;
        }

        #endregion

        #region Resolve

        /// <summary>
        /// Resolve a Type from the container
        /// </summary>
        /// <typeparam name="T">Type to Resolve</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/></returns>
        public static T Resolve<T>() where T : notnull
            => (Instance as IIocContainer).Resolve<T>();

        ///<inheritdoc/>
        T IIocContainer.Resolve<T>()
            => (T)InternalResolve(container, typeof(T), null);

        /// <summary>
        /// Resolve a Type from the container that has the specified key
        /// </summary>
        /// <typeparam name="T">Type to Resolve</typeparam>
        /// <param name="key">The key with which it was registered</param>
        /// <returns>An instance of type <typeparamref name="T"/></returns>
        public static T Resolve<T>(string key) where T : notnull
            => (Instance as IIocContainer).Resolve<T>(key);

        ///<inheritdoc/>
        T IIocContainer.Resolve<T>(string key)
            => (T)InternalResolve(container, typeof(T), key);

        /// <summary>
        /// Resolve the specified <c>Type</c>
        /// </summary>
        /// <param name="type">The Type to resolve</param>
        /// <returns>An object of the specified type</returns>
        public static object Resolve(Type type)
            => (Instance as IIocContainer).Resolve(type);

        ///<inheritdoc/>
        object IIocContainer.Resolve(Type type)
            => InternalResolve(container, type, null);

        /// <summary>
        /// Resolve the specified <c>Type</c>, with the specified key
        /// </summary>
        /// <param name="type">The Type to resolve</param>
        /// <param name="key">The key with which it was registered</param>
        /// <returns>An object of the specified type</returns>
        public static object Resolve(Type type, string key)
            => (Instance as IIocContainer).Resolve(type, key);

        ///<inheritdoc/>
        object IIocContainer.Resolve(Type type, string key)
            => InternalResolve(container, type, key);



        internal Expression GetExpression(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            var metaObject = GetMetaObject(container, resolvedType, key);

            if (metaObject.LifeCycle is LifeCycle.Singleton)
                return Expression.Call(
                    MetaObject.IIocContainerParameter,
                    typeof(IIocContainer)
                        .GetMethod(
                            nameof(IIocContainer.Resolve),
                            new Type[] { typeof(string) })
                        .MakeGenericMethod(resolvedType),
                    Expression.Constant(key, typeof(string)));

            if (metaObject.NewExpression != null)
                return metaObject.NewExpression;

            MakeNewExpression(container, metaObject);

            return metaObject.NewExpression;
        }

        //todo might read easier as an extension method
        internal void MakeNewExpression(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, MetaObject metaObject)
        {
            var paramsInfo = metaObject.ConstructorCache?.GetParameters();

            if (paramsInfo != null)
            {

                var argsExp = new Expression[paramsInfo.Length];

                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    var param = paramsInfo[i];
                    var namedAttribute = param.GetCustomAttribute<ResolveNamedAttribute>();

                    argsExp[i] = GetExpression(
                        container,
                        param.ParameterType,
                        namedAttribute?.Key);
                }

                metaObject.NewExpression = Expression.New(metaObject.ConstructorCache, argsExp);
            }

            else if (metaObject.TConcrete.IsGenericType
                     && metaObject.TConcrete.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                metaObject.NewExpression = GetEnumerableExpression(container, metaObject.TConcrete);

            else
                throw new Exception($"{nameof(metaObject.ConstructorCache)} should not be null");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">Needed so we can call with ParentContainer</param>
        /// <param name="resolvedType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal MetaObject GetMetaObject(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            if (container.TryGetValue(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
                return metaObject;

            if (resolvedType.IsGenericType)
            {
                if (!resolvedType.IsConstructedGenericType)
                    throw new ResolveException($"{resolvedType.Name} is not a Constructed Generic Type");
                else if (resolvedType.GetGenericTypeDefinition() != typeof(IEnumerable<>)) //if Constructed Generic
                {
                    var genericTypeDefinition = resolvedType.GetGenericTypeDefinition();

                    if (container.TryGetValue(new Tuple<Type, string>(genericTypeDefinition, key), out MetaObject genericMetaObject))
                    {
                        Type[] closedTypeArgs = resolvedType.GetGenericArguments();
                        Type resolvableType = genericTypeDefinition.MakeGenericType(closedTypeArgs);
                        Type makeableType = genericMetaObject.TConcrete.MakeGenericType(closedTypeArgs);

                        //todo - investigate if specify constructor with generic type and pass params here or constructorcache
                        var specificMetaObject = new MetaObject(makeableType, genericMetaObject.LifeCycle, constructorParams: null);
                        InternalRegister(container, resolvableType, key, specificMetaObject);

                        return specificMetaObject;
                    }
                }
            }

            if (parent != null && options.ResolveShouldBubbleUpContainers == true)
                return GetMetaObject(parentContainer, resolvedType, key);

            if (!options.TryResolveUnregistered)
                throw new ResolveException(
                    $"The type {resolvedType.Name} has not been registered. Either " +
                    $"register the class, or configure the container options differently " +
                    $"when initialising the conatiner.");

            if (resolvedType.IsGenericType
                && resolvedType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) //Register as Singleton
            {
                InternalRegisterTypesOf(container,
                    resolvedType.GetGenericArguments()[0],
                    LifeCycle.Singleton);

                return GetMetaObject(container, resolvedType, null);
            }
            else if (resolvedType.IsInterface || resolvedType.IsAbstract) //register as Singleton
            {
                IEnumerable<Type> implementations;

                if (assemblyTypesCache is null)
                    assemblyTypesCache = AppDomain
                        .CurrentDomain
                        .GetAssemblies()
                        .SelectMany(a => a.GetTypes());

                implementations = assemblyTypesCache
                    .Where(t =>
                        resolvedType.IsAssignableFrom(t)
                        && t != resolvedType);

                if (implementations.Count() != 1)
                {
                    var builder = new StringBuilder();
                    builder.Append(
                        $"Could not Resolve or Create {resolvedType.Name}" +
                        $". It is not registered in {nameof(IocContainer)}. Furthermore, " +
                        $"smart resolve couldn't create an instance. ");
                    if (implementations.Count() == 0)
                        builder.AppendLine("No implementations were found.");
                    else
                    {
                        builder.Append("Too many implementations were found: ");
                        foreach (var implementation in implementations)
                        {
                            builder.Append($"{implementation.Name}; ");
                        }
                        builder.Remove(builder.Length - 2, 2);
                    }

                    throw new ResolveException(builder.ToString());
                }

                metaObject = new MetaObject(implementations.ToArray()[0], LifeCycle.Singleton, constructorParams: null);
            }
            else
                metaObject = new MetaObject(resolvedType, LifeCycle.Transient, constructorParams: null); //Register as Transient

            if (container.TryAdd(new Tuple<Type, string>(resolvedType, null), metaObject))
                return metaObject;

            // This condition should never be hit. Here just in case.
            throw new ResolveException(
                $"The type {resolvedType.Name} has not been registered and SmartResolve didn't work.");
        }

        internal object InternalResolve(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            var metaObject = GetMetaObject(container, resolvedType, key);

            if (metaObject.ActivationExpression is null)
                MakeNewExpression(container, metaObject);

            return metaObject.GetObject(this);
        }

        internal Expression GetEnumerableExpression(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType)
        {
            var resolvableType = resolvedType.GetGenericArguments()[0];
            var addMethod = typeof(List<>).MakeGenericType(resolvableType).GetMethod("Add");

            List<ElementInit> listElements = new List<ElementInit>();
            foreach (var key in container.Keys)
            {
                if (key.Item1 == resolvableType)
                {
                    var expression = GetExpression(container, resolvableType, key.Item2);
                    listElements.Add(Expression.ElementInit(addMethod, expression));
                }
            }

            // Create a NewExpression that represents constructing
            // a new instance of Dictionary<int, string>.
            NewExpression newDictionaryExpression =
                Expression.New(typeof(List<>).MakeGenericType(resolvableType));

            // Create a ListInitExpression that represents initializing
            // a new Dictionary<> instance with two key-value pairs.
            if (listElements.Any())
                return
                    Expression.ListInit(
                        newDictionaryExpression,
                        listElements);

            throw new ResolveException($"Could not resolve {resolvedType.Name}");
        }

        #endregion
        #region Unregister

        /// <summary>
        /// De-register a Type from the container.
        ///  <c>IDisposable</c> will be called if it's
        ///  implemented.
        /// </summary>
        /// <typeparam name="T">The Type to de-register</typeparam>
        public static void Unregister<T>()
            where T : notnull
                => (Instance as IIocContainer).Unregister<T>();

        ///<inheritdoc/>
        void IIocContainer.Unregister<T>()
            => InternalUnregister(container, typeof(T), null);

        /// <summary>
        /// De-register a Type from the container, with the specified key
        ///  <c>IDisposable</c> will be called if it's
        ///  implemented.
        /// </summary>
        /// <typeparam name="T">The Type to de-register</typeparam>
        /// <param name="key">The key with which it was registered</param>
        public static void Unregister<T>(string key)
            where T : notnull
                => (Instance as IIocContainer).Unregister<T>(key);

        ///<inheritdoc/>
        void IIocContainer.Unregister<T>(string key)
            => InternalUnregister(container, typeof(T), key);


        static void InternalUnregister(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Type resolvedType, string key)
        {
            if (container.TryRemove(new Tuple<Type, string>(resolvedType, key), out MetaObject metaObject))
                TryDispose(metaObject);
            else
            {
                var builder = new StringBuilder();
                builder.Append($"Can't find {resolvedType.Name}");
                if (!string.IsNullOrEmpty(key))
                    builder.Append($" with key '{key}'");
                builder.Append(".");
                throw new ResolveException(builder.ToString());
            }
        }

        /// <summary>
        /// De-register everything in the container, calling
        ///  <c>IDisposable</c> on all objects that implement
        ///  it.
        /// </summary>
        public static void UnregisterAll()
            => (Instance as IIocContainer).UnregisterAll();

        ///<inheritdoc/>
        void IIocContainer.UnregisterAll()
            => InternalUnregisterAll(container);


        static void InternalUnregisterAll(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container)
        {
            foreach (var registration in container.ToArray())
            {
                TryDispose(registration.Value);
                container.TryRemove(registration.Key, out _);
            }
        }

        private static void TryDispose(MetaObject metaObject)
        {
            try
            {
                metaObject.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while disposing registered object of type {metaObject.TConcrete.Name}", ex);
            }
        }
        #endregion

        void IDisposable.Dispose()
        {
            InternalUnregisterAll(container);
            if (!containers.TryRemove(Name, out _))
                Console.WriteLine("ZenIoc Error: Could not remove registration of container " +
                    $"named '{Name}' when disposing");
        }
    }

    /// <summary>
    /// Fluent API
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IRegisterOptions : ILifeCycleOptions
    {
    }

    /// <summary>
    /// Fluent API
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILifeCycleOptions
    {
        /// <summary>
        /// Ensure singleton lifecycle
        /// </summary>
        void SingleInstance();
    }

    /// <summary>
    /// Fluent API
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RegisterOptions : IRegisterOptions
    {
        readonly ConcurrentDictionary<Tuple<Type, string>, MetaObject> container;
        readonly Tuple<Type, string> key;

        /// <summary>
        /// Constructs the RegisterOptions class
        /// </summary>
        /// <param name="container"></param>
        /// <param name="key"></param>
        public RegisterOptions(ConcurrentDictionary<Tuple<Type, string>, MetaObject> container, Tuple<Type, string> key)
        {
            this.container = container;
            this.key = key;
        }

        /// <summary>
        /// Ensures a singleton lifecycle
        /// </summary>
        public void SingleInstance()
        {
            container[key].LifeCycle = LifeCycle.Singleton;
        }
    }

    /// <summary>
    /// MetaObject that holds all the info needed to instantiate an object
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaObject : IDisposable
    {
        #region Constructors
        /// <summary>
        /// Construct the <see cref="MetaObject"/>
        /// </summary>
        /// <param name="instance"></param>
        public MetaObject(object instance) : this(instance?.GetType(), LifeCycle.Singleton)
        {
            Instance = instance;
            ActivationExpression = c => instance; //Never called, but ActivationExpression cant be null
        }


        /// <summary>
        /// Construct the <see cref="MetaObject"/>
        /// </summary>
        /// <param name="concreteType"></param>
        /// <param name="lifeCycle"></param>
        /// <param name="instanceDelegate"></param>
        /// <param name="key">Need to know its full dictionary key to make its uncompiled expression</param>
        public MetaObject(Type concreteType, LifeCycle lifeCycle, Expression<Func<IIocContainer, object>> instanceDelegate, Tuple<Type, string> key) : this(concreteType, lifeCycle)
        {
            if (instanceDelegate is null)
                throw new ArgumentNullException(nameof(instanceDelegate));

            var resolveMethod = typeof(IIocContainer).GetMethod(nameof(IIocContainer.Resolve), new Type[] { typeof(string) }).MakeGenericMethod(key.Item1);

            //Cant set to instanceDelegate.Body because the parameter c is out of scope
            //...figure this out and you'll have a more elegant solution
            NewExpression = Expression.Call(
                IIocContainerParameter,
                resolveMethod,
                Expression.Constant(key.Item2, typeof(string)));

            ActivationExpression = instanceDelegate.Compile();
        }

        /// <summary>
        /// Construct the <see cref="MetaObject"/>
        /// </summary>
        public MetaObject(Type concreteType, LifeCycle lifeCycle, Type[] constructorParams) : this(concreteType, lifeCycle)
        {
            if (constructorParams == Type.EmptyTypes) //specificaly select paramaterless constructor
                ConstructorCache = concreteType.GetConstructor(constructorParams);
            else
                ConstructorCache = constructorParams != null//params are specified
                        ? GetSpecificConstructor(concreteType, constructorParams)
                        : GetBestConstructor(concreteType);
        }

        private MetaObject(Type concreteType, LifeCycle lifeCycle)
        {
            TConcrete = concreteType ?? throw new ArgumentNullException(nameof(concreteType));
            LifeCycle = lifeCycle;
        }
        #endregion

        #region Properties

        internal object Instance { get; set; }

        internal static ParameterExpression IIocContainerParameter { get; } = Expression.Parameter(typeof(IIocContainer), "c");

        Expression newExpression;
        internal Expression NewExpression
        {
            get => newExpression;
            set
            {
                newExpression = value;

                ActivationExpression = Expression.Lambda(
                    newExpression,
                    IIocContainerParameter
                    ).Compile() as Func<IIocContainer, object>;
            }
        }


        internal Type TConcrete { get; }

        internal LifeCycle LifeCycle { get; set; }

        internal ConstructorInfo ConstructorCache { get; }

        internal Func<IIocContainer, object> ActivationExpression { get; private set; }

        #endregion

        #region Methods

        internal object GetObject(IIocContainer container)
        {
            if (LifeCycle is LifeCycle.Singleton)
            {
                if (Instance is null)
                    Instance = ActivationExpression(container);

                return Instance;
            }

            return ActivationExpression(container);
        }

        internal ConstructorInfo GetSpecificConstructor(Type concreteType, params Type[] args)
        {
            try
            {
                var constructor = concreteType
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        args,
                        null);

                //Rather throw error on registration
                if (constructor is null)
                    throw new Exception($"No matching constructor found.");

                return constructor;
            }
            catch (Exception ex)
            {
                throw new RegisterException($"Could not register {TConcrete.Name} with specified constructor.", ex);
            }
        }

        internal ConstructorInfo GetBestConstructor(Type concreteType)
        {
            var constructors = concreteType.
                    GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .ToList();

            if (constructors.Count == 0)
                if (concreteType.IsGenericType
                    && concreteType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return null; //ok because we won't need the constructor
                else
                    throw new RegisterException($"{concreteType.Name} won't be resolved as it has no constructors.");

            if (constructors.Count > 1)
            {
                //if flagged, shorten to only flagged constructors
                var flaggedConstructors = constructors
                    .Where(c => c.GetCustomAttribute<ResolveUsingAttribute>() != null)
                    .ToList();

                if (flaggedConstructors.Any())
                {
                    if (flaggedConstructors.Count > 1)
                        throw new ResolveException($"{concreteType.Name} may only have one [ResolveUsing] attribute");
                    constructors = flaggedConstructors;
                }

                //use aggregator to find greediest / leanest
                if (IocContainer.options.ShouldPickGreediestConstructor)
                    return constructors
                        .Aggregate((i, j)
                            => i.GetParameters().Count() > j.GetParameters().Count()
                            ? i
                            : j);
                else
                    return constructors
                        .Aggregate((i, j)
                            => i.GetParameters().Count() < j.GetParameters().Count()
                            ? i
                            : j);

            }
            //else has to be only one
            return constructors[0];
        }

        /// <summary>
        /// Implements IDisposable
        /// </summary>
        public void Dispose()
        {
            if (Instance != null
                && Instance is IDisposable disposable)
                disposable.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Lifecycle of the registered object
    /// </summary>
    public enum LifeCycle
    {
        /// <summary>
        /// Single instance only
        /// </summary>
        Singleton,
        /// <summary>
        /// Multiple instances - a new one instantiated
        /// each time <c>Resolve</c> is called
        /// </summary>
        Transient
    }

    /// <summary>
    /// Exception thrown on Registration
    /// </summary>
    public class RegisterException : Exception
    {
        //public RegistrationException() : base()
        //{ }
        /// <summary>
        /// Exception thrown on Registration
        /// </summary>
        public RegisterException(string message) : base(message)
        { }
        /// <summary>
        /// Exception thrown on Registration
        /// </summary>
        public RegisterException(string message, Exception innerException) : base(message, innerException)
        { }

    }
    /// <summary>
    /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
    /// </summary>
    public class ResolveException : Exception
    {
        /// <summary>
        /// Exception thrown when <c>Resolve</c> is called for a type that has not been registered
        /// </summary>
        public ResolveException(string message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// Will resolved the dependency associated with the named <see cref="Key"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ResolveNamedAttribute : Attribute
    {
        /// <summary>
        /// The Key
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Will resolved the dependency associated with the named <paramref name="key"/>
        /// </summary>
        public ResolveNamedAttribute(string key)
        {
            Key = key;
        }
    }
    /// <summary>
    /// Attribute to explicitly mark which constructor should be used by the <see cref="IocContainer"/>
    /// to instantiate the class
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class ResolveUsingAttribute : Attribute
    {
        /// <summary>
        /// Attribute to explicitly mark which constructor should be used by the <see cref="IocContainer"/>
        /// to instantiate the class
        /// </summary>
        public ResolveUsingAttribute()
        {
        }
    }
}
