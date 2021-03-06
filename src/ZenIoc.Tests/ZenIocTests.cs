using System;
using Xunit;
using ZenIoc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ZenIocTests
{
    [Collection("ZenIoc")]
    public class NewIocContainerTests
    {
        [Fact]
        public void Playground()
        {
        }

        #region CLASSES
        public abstract class DisposableBase : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
                => Dispose(true);

            protected void Dispose(bool disposing)
            {
                if (!Disposed)
                {
                    Disposed = true;

                    if (disposing)
                    {
                        DisposeExplicit();
                    }

                    DisposeImplicit();

                    GC.SuppressFinalize(this);
                }
            }

            protected virtual void DisposeExplicit() { }
            protected virtual void DisposeImplicit() { }

            ~DisposableBase()
            {
                Dispose(false);
            }
        }

        class ClassWithStringParameter
        {
            public ClassWithStringParameter(string name)
            {

            }
        }

        interface IService { }
        class MyService : IService { public string Name { get; set; } }

        interface IServiceWithTwoImplementations { }
        class ServiceTwo : IServiceWithTwoImplementations { }
        class MockServiceTwo : IServiceWithTwoImplementations { }

        interface IServiceWithNoImplementations { }

        class ConcreteOnly { }

        interface IClassWith3Ctors
        {
            string ConstructorUsed { get; }
            IService Service { get; }
            ConcreteOnly Concrete { get; }
        }
        class ClassWith3Ctors : IClassWith3Ctors
        {
            public string ConstructorUsed { get; }
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            public ClassWith3Ctors(IService service)
            { this.Service = service; ConstructorUsed = "(IService service)"; }
            public ClassWith3Ctors() { ConstructorUsed = "()"; }
            internal ClassWith3Ctors(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; ConstructorUsed = "(IService service, ConcreteOnly concrete)"; }
        }

        class ClassWithFlaggedCtor
        {
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            [ResolveUsing]
            public ClassWithFlaggedCtor(IService service)
            { this.Service = service; }

            public ClassWithFlaggedCtor(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; }
        }

        class ClassWith2FlaggedCtors
        {
            public IService Service { get; }
            public ConcreteOnly Concrete { get; }

            [ResolveUsing]
            public ClassWith2FlaggedCtors(IService service)
            { this.Service = service; }

            [ResolveUsing]
            public ClassWith2FlaggedCtors(IService service, ConcreteOnly concrete)
            { this.Service = service; this.Concrete = concrete; }
        }

        class ClassThatsResolvableWithoutRegistering
        {
            public ConcreteOnly Concrete { get; }
            public ClassThatsResolvableWithoutRegistering(ConcreteOnly concrete)
            {
                this.Concrete = concrete;
            }
        }

        class ClassWithKeyedDependency
        {
            public IService Service { get; }
            public ClassWithKeyedDependency()
            {

            }
            public ClassWithKeyedDependency([ResolveNamed("test")]IService service)
            {
                Service = service;
            }
        }

        class ClassThatsUnresolvable
        {
            public ClassThatsUnresolvable()
            {
                throw new Exception("Its not possible to instantiate this class");
            }
        }

        class ClassWithInternalConstructor
        {
            public string ConstructorUsed { get; }

            public ClassWithInternalConstructor()
            {
                ConstructorUsed = "public";
            }

            internal ClassWithInternalConstructor(ConcreteOnly concreteOnly)
            {
                ConstructorUsed = "internal";
            }
        }

        class ClassThatHasToBeRegistered
        {
            public ClassThatHasToBeRegistered(int number)
            {

            }
        }

        class ClassWithoutParamaterlessCtor
        {
            public ClassWithoutParamaterlessCtor(IService service)
            {

            }
        }

        public class ClassThatThrowsOnDisposed : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        interface IClassThatsDisposable
        {

        }
        class ClassThatsDisposable : DisposableBase, IClassThatsDisposable
        {
            private Action _onExplicitDispose;
            private Action _onImplicitDispose;

            public ClassThatsDisposable(Action onExplicitDispose, Action onImplicitDispose)
            {
                _onExplicitDispose = onExplicitDispose;
                _onImplicitDispose = onImplicitDispose;
            }

            protected override void DisposeExplicit()
                => _onExplicitDispose?.DynamicInvoke();
            protected override void DisposeImplicit()
                => _onImplicitDispose?.DynamicInvoke();
        }

        public abstract class ClassThatsAbstract { }
        public class ClassInheritsAbstract1 : ClassThatsAbstract { }
        public class ClassInheritsAbstract2 : ClassThatsAbstract { }

        public class ClassThatsGeneric<T> { }
        #endregion

        [Fact]
        public void Constructor()
        {
            Assert.IsAssignableFrom<IIocContainer>(new IocContainer());
        }

        [Fact]
        public void InitializeStaticContainer()
        {
            //Not a real test
            IocContainer.Initialize(new ContainerOptions {
                TryResolveUnregistered = false,
                ResolveShouldBubbleUpContainers = false});
        }

        [Fact]
        public void ContainerOptions_AreGlobal()
        {
            ContainerOptions options = new ContainerOptions
            {
                TryResolveUnregistered = false
            };
            using  var container = new IocContainer(options);
            using var secondContainer = new IocContainer();

            Assert.False(IocContainer.options.TryResolveUnregistered);
        }


        #region MetaObject

        [Fact]
        public void MetaObjectCtor_InstanceNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetaObject(null));
        }

        [Fact]
        public void MetaObjectCtor_InstanceDelegateNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MetaObject(typeof(ConcreteOnly), LifeCycle.Transient, instanceDelegate: null,null));
        }

        [Fact]
        public void MetaObject_UsingConstructor_TypeHasNoConstructor_Throws()
        {
            Assert.Throws<RegisterException>(() => new MetaObject(typeof(ClassThatsUnresolvable), LifeCycle.Singleton, new Type[] { typeof(MyService) }));
        }

        [Fact]
        public void MetaObject_UsingConstructor_NoMatchFound_Throws()
        {
            Assert.Throws<RegisterException>(() => new MetaObject(typeof(ConcreteOnly), LifeCycle.Singleton, new Type[] { typeof(MyService) }));
        }

//#if DEBUG
//        [Fact]
//        public void MetaObject_RegisteredTransient_SetInstance_Throws()
//        {
//            var metaObject = new MetaObject(typeof(ConcreteOnly), LifeCycle.Transient);
//            Assert.Throws<Exception>(() => metaObject.Instance = new ConcreteOnly());

//        }
//#endif

        #endregion

        #region Registration
        #region internal

        [Fact]
        public void InnerRegister_ConcreteTypeIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            IocContainer.Instance.InternalRegister(
                new ConcurrentDictionary<Tuple<Type, string>, MetaObject>(),
                typeof(IService),
                null,
                new MetaObject(concreteType: null, //We need this at a minimum to justify registration 
                               LifeCycle.Transient, null)
                ));
        }

        #endregion
        #region Register<ConcreteType>()
        [Fact]
        public void StaticRegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }

        #endregion
        #region Register<ConcreteType,ResolvedType>()
        [Fact]
        public void StaticRegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<IService, MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<IService, MyService>();

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterResolvedType_Default_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_Default_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }

        #endregion
        #region Register<ConcreteType>(string key)
        [Fact]
        public void StaticRegisterConcreteTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticRegisterWithCtorParametersWithKey_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<ClassWith3Ctors>("test",typeof(IService));

            Assert.Equal("(IService service)", IocContainer.Resolve<ClassWith3Ctors>("test").ConstructorUsed);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterWithCtorParametersWithKey_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using  var container = new IocContainer(mock, ContainerOptions.Default);
            container.Register<ClassWith3Ctors>("test", typeof(IService));

            Assert.Equal("(IService service)", container.Resolve<ClassWith3Ctors>("test").ConstructorUsed);
        }

        [Fact]
        public void RegisterConcreteTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        #endregion
        #region Register<ConcreteType,ResolvedType>(string key)
        [Fact]
        public void StaticRegisterResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<IService, MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<IService, MyService>("test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        [Fact]
        public void StaticRegisterResolvedWithCtorParameters_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<IClassWith3Ctors, ClassWith3Ctors>(typeof(IService));

            Assert.Equal("(IService service)", IocContainer.Resolve<IClassWith3Ctors>().ConstructorUsed);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedWithCtorParameters_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();


            using  var container = new IocContainer(mock, ContainerOptions.Default);
            container.Register<IClassWith3Ctors, ClassWith3Ctors>(typeof(IService));

            Assert.Equal("(IService service)", container.Resolve<IClassWith3Ctors>().ConstructorUsed);
        }

        [Fact]
        public void StaticRegisterResolvedWithCtorParametersWithKey_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<IClassWith3Ctors, ClassWith3Ctors>("test",typeof(IService));

            Assert.Equal("(IService service)", IocContainer.Resolve<IClassWith3Ctors>("test").ConstructorUsed);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedWithCtorParametersWithKey_ResolvesWithSelectedCtor()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<IClassWith3Ctors, ClassWith3Ctors>("test", typeof(IService));

            Assert.Equal("(IService service)", container.Resolve<IClassWith3Ctors>("test").ConstructorUsed);
        }

        #endregion

        #region RegisterInstance
        [Fact]
        public void StaticRegisterInstance_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterInstance(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstance_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterInstance(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterInstanceWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterInstance<IService>(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterInstance<IService>(new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterInstanceWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterInstance(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterInstance(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        [Fact]
        public void StaticRegisterInstanceWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterInstance<IService>(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterInstanceWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterInstance<IService>(new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }


        #endregion

        #region RegisterExpression

        [Fact]
        public void StaticRegisterExpression_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterExplicit<MyService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpression_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<MyService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterExpressionWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterExplicit<IService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpressionWithResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<IService,MyService>(c => new MyService());

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterExpressionWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterExplicit<MyService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpressionWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<MyService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), "test")));
        }

        [Fact]
        public void StaticRegisterExpressionWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterExplicit<IService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterExpressionWithResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterExplicit<IService,MyService>(c => new MyService(), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        #endregion

        #region RegisterType
        [Fact]
        public void StaticRegisterType_Defaults_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterType(typeof(MyService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_Defaults_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(MyService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(MyService), null)));
        }

        [Fact]
        public void StaticRegisterType_ResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterType(typeof(MyService), typeof(IService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_ResolvedType_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(MyService), typeof(IService));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), null)));
        }

        [Fact]
        public void StaticRegisterType_ResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterType(typeof(MyService), typeof(IService), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_ResolvedTypeWithKey_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(MyService), typeof(IService), "test");

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(IService), "test")));
        }

        [Fact]
        public void StaticRegisterType_WithConstructorParams_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.RegisterType(typeof(ClassWith3Ctors), null, null, typeof(IService), typeof(ConcreteOnly));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(ClassWith3Ctors), null)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterType_WithConstructorParams_RegistersWithExpectedKey()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.RegisterType(typeof(ClassWith3Ctors), null, null, typeof(IService), typeof(ConcreteOnly));

            Assert.True(mock.ContainsKey(new Tuple<Type, string>(typeof(ClassWith3Ctors), null)));
        }
        #endregion

        #region RegisterTypesOf
        [Fact]
        public void RegisterTypesOf_NotInterfaceAbstract_Throws()
        {
            Assert.Throws<RegisterException>(() => IocContainer.RegisterTypesOf<MyService>());

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterTypesOf_Abstract_Singleton()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);
          IocContainer.RegisterTypesOf<ClassThatsAbstract>(LifeCycle.Singleton);

            Assert.Equal(3, mock.Count);

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(ClassThatsAbstract), nameof(ClassInheritsAbstract1)))
                && r.Value.TConcrete.Equals(typeof(ClassInheritsAbstract1))
                && r.Value.LifeCycle.Equals(LifeCycle.Singleton)
                );

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(ClassThatsAbstract), nameof(ClassInheritsAbstract2)))
                && r.Value.TConcrete.Equals(typeof(ClassInheritsAbstract2))
                && r.Value.LifeCycle.Equals(LifeCycle.Singleton)
                );

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IEnumerable<ClassThatsAbstract>), null))
                && r.Value.TConcrete.Equals(typeof(IEnumerable<ClassThatsAbstract>))
                && r.Value.LifeCycle.Equals(LifeCycle.Singleton)
                );

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterTypesOf_Interface_Singleton()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);
            IocContainer.RegisterTypesOf<IServiceWithTwoImplementations>(LifeCycle.Singleton);

            Assert.Equal(3, mock.Count);

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IServiceWithTwoImplementations), nameof(MockServiceTwo)))
                && r.Value.TConcrete.Equals(typeof(MockServiceTwo))
                && r.Value.LifeCycle.Equals(LifeCycle.Singleton)
                );

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IServiceWithTwoImplementations), nameof(ServiceTwo)))
                && r.Value.TConcrete.Equals(typeof(ServiceTwo))
                && r.Value.LifeCycle.Equals(LifeCycle.Singleton)
                );

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IEnumerable<IServiceWithTwoImplementations>), null))
                && r.Value.TConcrete.Equals(typeof(IEnumerable<IServiceWithTwoImplementations>))
                && r.Value.LifeCycle.Equals(LifeCycle.Singleton)
                );

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterTypesOf_Interface()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);
            IocContainer.RegisterTypesOf<IServiceWithTwoImplementations>();

            Assert.Equal(3, mock.Count);

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IServiceWithTwoImplementations), nameof(MockServiceTwo)))
                && r.Value.TConcrete.Equals(typeof(MockServiceTwo))
                && r.Value.LifeCycle.Equals(LifeCycle.Transient)
                );

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IServiceWithTwoImplementations), nameof(ServiceTwo)))
                && r.Value.TConcrete.Equals(typeof(ServiceTwo))
                && r.Value.LifeCycle.Equals(LifeCycle.Transient)
                );

            Assert.Contains(mock, r =>
                r.Key.Equals(new Tuple<Type, string>(typeof(IEnumerable<IServiceWithTwoImplementations>), null))
                && r.Value.TConcrete.Equals(typeof(IEnumerable<IServiceWithTwoImplementations>))
                && r.Value.LifeCycle.Equals(LifeCycle.Transient)
                );

            IocContainer.ResetContainer();
        }

        [Fact]
        public void ResolveIEnumerableInterface_NotRegistered()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);
            
            var enumerable = IocContainer.Resolve<IEnumerable<IServiceWithTwoImplementations>>();

            Assert.Equal(2, enumerable.Count());
            Assert.Contains(enumerable, t => t.GetType().Equals(typeof(ServiceTwo)));
            Assert.Contains(enumerable, t => t.GetType().Equals(typeof(MockServiceTwo)));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void ResolveIEnumerableInterface_NotRegistered_SomeMembersRegistered()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);
            IocContainer.Register<IServiceWithTwoImplementations, MockServiceTwo>(nameof(MockServiceTwo)).SingleInstance();

            Assert.Throws<RegisterException>(()=>
                IocContainer.Resolve<IEnumerable<IServiceWithTwoImplementations>>());

            IocContainer.ResetContainer();
        }

        [Fact]
        public void ResolveIEnumerableAbstract_NotRegistered()
        {
            IocContainer.Initialize(o => {o.TryResolveUnregistered = true; });

            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            var enumerable = IocContainer.Resolve<IEnumerable<ClassThatsAbstract>>();

            Assert.Equal(2, enumerable.Count());
            Assert.Contains(enumerable, t => t.GetType().Equals(typeof(ClassInheritsAbstract1)));
            Assert.Contains(enumerable, t => t.GetType().Equals(typeof(ClassInheritsAbstract2)));

            IocContainer.ResetContainer();
        }


        #endregion

        #region RegisterOptions
        [Fact]
        public void StaticRegisterConcreteType_OptionsSingleInstance_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer
                .Register<MyService>()
                .SingleInstance();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType_OptionsSingleInstance_RegistersAsSingleInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container
                .Register<MyService>()
                .SingleInstance();

            Assert.Equal(LifeCycle.Singleton, mock[new Tuple<Type, string>(typeof(MyService), null)].LifeCycle);
        }

        [Fact]
        public void StaticRegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer
                .Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterResolvedType_RegistersAsMultiInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container
                .Register<IService, MyService>();

            Assert.Equal(LifeCycle.Transient, mock[new Tuple<Type, string>(typeof(IService), null)].LifeCycle);
        }

        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_ResolvesSpecifiedConstructor()
        {
            IocContainer.Register<IService, MyService>();

            IocContainer
                .Register<ClassWith3Ctors>(typeof(IService));

            var resolved = IocContainer.Resolve<ClassWith3Ctors>();
            Assert.Equal("(IService service)", resolved.ConstructorUsed);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void RegisterConcreteType__UsingConstructorOption_ResolvesSpecifiedConstructor()
        {
            using var container = new IocContainer();
            container.Register<IService, MyService>();

            container
                .Register<ClassWith3Ctors>(typeof(IService));

            var resolved = container.Resolve<ClassWith3Ctors>();
            Assert.Equal("(IService service)", resolved.ConstructorUsed);
        }

        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_WrongCtor_ThrowsRegistrationException()
        {
            Assert.Throws<RegisterException>(() =>
                IocContainer
                    .Register<ClassWith3Ctors>(typeof(ConcreteOnly)));

            IocContainer.ResetContainer();
        }


        [Fact]
        public void StaticRegisterConcreteType_UsingConstructorOption_ResolvesInternalConstructor()
        {
            IocContainer
                .Register<ClassWithInternalConstructor>(typeof(ConcreteOnly));

            var resolved = IocContainer.Resolve<ClassWithInternalConstructor>();
            Assert.Equal("internal", resolved.ConstructorUsed);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Compile_SetsActivationExpression()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using  var container = new IocContainer(mock, ContainerOptions.Default);
            container.Register<ClassWith3Ctors>("test", typeof(IService));

            mock.TryGetValue(new Tuple<Type, string>(typeof(ClassWith3Ctors), "test"), out MetaObject metaObject);
            Assert.Null(metaObject.ActivationExpression);

            container.Compile();
            Assert.NotNull(metaObject.ActivationExpression);
        }


        #endregion
        #endregion

        #region Resolve
        #region Internal

        [Fact]
        public void GetConstructorParams_gt1Constructor_ReturnsFromGreediestPublic()
        {
            //ClassWith3Ctors has 3 constructors, two public () & (IService)
            //and one internal (IService, ConcreteOnly)

            //Expect to ignore internal, but take greediest public ctor
            var exepectedParamters
                = typeof(ClassWith3Ctors)
                    .GetConstructor(new Type[] { typeof(IService) });//.GetParameters();

            var metaObj = new MetaObject(typeof(object), LifeCycle.Singleton, null);

            var parameters
                = metaObj.
                    GetBestConstructor(typeof(ClassWith3Ctors));

            Assert.Equal(exepectedParamters, parameters);
        }

        [Fact]
        public void GetConstructorParams_gt1Ctor_FlaggedCtor_ReturnsFlaggedNotGreediest()
        {
            //ClassWithFlaggedCtor has 2 public constructors
            //(IService) - Flagged
            //(IService, ConcreteOnly)

            //Expect to pick flagged ctor
            var exepectedParamters
                = typeof(ClassWithFlaggedCtor)
                    .GetConstructor(new Type[] { typeof(IService) });//.GetParameters();

            var metaObj = new MetaObject(typeof(object), LifeCycle.Singleton, null);

            var parameters
                = metaObj.
                    GetBestConstructor(typeof(ClassWithFlaggedCtor));

            Assert.Equal(exepectedParamters, parameters);
        }

        [Fact]
        public void GetConstructorParams_gt1Ctor_2FlaggedCtors_ThrowsResolveException()
        {
            var exepectedParamters
                = typeof(ClassWith2FlaggedCtors)
                    .GetConstructor(new Type[] { typeof(IService) }).GetParameters();

            var metaObj = new MetaObject(typeof(object), LifeCycle.Singleton, null);

            Assert.Throws<ResolveException>(() => metaObj.
                                GetBestConstructor(typeof(ClassWith2FlaggedCtors)));
        }

        [Fact]
        public void GetMetaObject_IsGenericType_NotConstructedGeneric_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer();
            Assert.Throws<ResolveException>(()=>container.GetMetaObject(mock, typeof(ClassThatsGeneric<>), null));
        }

        [Fact]
        public void Resolve_IsConstructedGeneric_NotRegistered()
        {
            using var container = new IocContainer();

            Assert.IsType<ClassThatsGeneric<MyService>>(container.Resolve<ClassThatsGeneric<MyService>>());
        }


        [Fact]
        public void GetMetaObject_UnregisteredInterface_Gt1Implementations_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer();
            Assert.Throws<ResolveException>(() => container.GetMetaObject(mock, typeof(IServiceWithTwoImplementations), null));
        }

        [Fact]
        public void GetMetaObject_UnregisteredInterface_NoImplementations_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer();
            Assert.Throws<ResolveException>(() => container.GetMetaObject(mock, typeof(IServiceWithNoImplementations), null));
        }

        [Fact]
        public void GetEnumerableException_Nothing_Registered_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer();
            Assert.Throws<ResolveException>(()=>container.GetEnumerableExpression(mock, typeof(IEnumerable<IService>)));
            
        }

        [Fact]
        public void MakeNewExpression_ConstructorCacheNull_Throws()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer();
            var metaObject = new MetaObject(new MyService());
            Assert.Null(metaObject.ConstructorCache);
            Assert.Throws<Exception>(() => container.MakeNewExpression(mock, metaObject));
        }

        #endregion

        [Fact]
        public void StaticResolve_Unregistered_Works()
        {
            var obj = IocContainer.Resolve<ClassThatsResolvableWithoutRegistering>();

            Assert.IsType<ClassThatsResolvableWithoutRegistering>(obj);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_Unregistered_Works()
        {
            using var container = new IocContainer(ContainerOptions.Default);
            var obj = container.Resolve<ClassThatsResolvableWithoutRegistering>();

            Assert.IsType<ClassThatsResolvableWithoutRegistering>(obj);

        }

        [Fact]
        public void StaticResolve_IsStrictModeTrue_Unregistered_Throws()
        {
            IocContainer.Initialize(o => o.TryResolveUnregistered = false);
            Assert.Throws<ResolveException>(
                () => IocContainer.Resolve<ClassThatsResolvableWithoutRegistering>());
        }

        [Fact]
        public void StaticResolve_KeyedDependency_Works()
        {
            IocContainer.Register<IService, MyService>("test");
            IocContainer.Register<ClassWithKeyedDependency>();
            var obj = IocContainer.Resolve<ClassWithKeyedDependency>();

            Assert.IsType<ClassWithKeyedDependency>(obj);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            IocContainer.RegisterInstance(instance);
            var resolved = IocContainer.Resolve<MyService>();
            Assert.Equal(instance, resolved); //exactly same object returned

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolve_InterfaceThatsNotRegistered_Works()
        {
            Assert.IsAssignableFrom<IService>(IocContainer.Resolve<IService>());

            IocContainer.ResetContainer();
        }


        //Silly test - just a throw in the constructor
        //[Fact]
        //public void StaticResolve_UnregisteredUnresolvable2_Throws()
        //{
        //    Assert.Throws<ResolveException>(
        //        () => IocContainer.Resolve<ClassThatsUnresolvable>());
        //}

        //Now have explicit child container
        //[Fact]
        //public void ResolveFromContainerInstance_RegisteredInStaticContainer_ResolvesFromStaticContainer()
        //{
        //    var registeredObject = new ClassThatHasToBeRegistered(3);
        //    IocContainer.RegisterInstance(registeredObject);

        //    using var container = new IocContainer();
        //    var resolved = container.Resolve<ClassThatHasToBeRegistered>();

        //    Assert.Equal(registeredObject, resolved);

        //    IocContainer.ResetContainer();
        //}


        [Fact]
        public void StaticResolve_Interface_ThrowsResolutionException()
        {
            Assert.Throws<RegisterException>(() => IocContainer.Register<IService>());
            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolveWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            IocContainer.RegisterInstance(instance, "test");
            var resolved = IocContainer.Resolve<MyService>("test");
            Assert.Equal(instance, resolved); //exactly same object returned

            IocContainer.ResetContainer();
        }

        [Fact]
        public void ResolveWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            using var container = new IocContainer();
            container.RegisterInstance(instance, "test");
            var resolved = container.Resolve<MyService>("test");
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        [Fact]
        public void StaticResolveofSingleInstance_SecondResolve_ReturnsSavedInstance()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<IService, MyService>().SingleInstance();

            Assert.Null(mock[new Tuple<Type, string>(typeof(IService), null)].Instance);

            //first resolve
            var first = IocContainer.Resolve<IService>();

            Assert.NotNull(mock[new Tuple<Type, string>(typeof(IService), null)].Instance);

            //second resolve
            var second = IocContainer.Resolve<IService>();

            Assert.Equal(first, second);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticResolveofSingleInstance_SecondResolve_IsSameObject()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            var singleton = new MyService();
            IocContainer.RegisterInstance<IService>(singleton);

            //first resolve
            Assert.Equal(singleton, IocContainer.Resolve<IService>());

            //second resolve
            //first resolve
            Assert.Equal(singleton, IocContainer.Resolve<IService>());

            IocContainer.ResetContainer();
        }

        #region Resolve from RegisterExpression
        [Fact]
        public void StaticResolve_Expression_ReturnsInstance()
        {
            IocContainer.Register<IService, MyService>();
            IocContainer.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(IocContainer.Resolve<IService>()));

            var resolved = IocContainer.Resolve<ClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_Expression_ReturnsInstance()
        {
            using var container = new IocContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()));

            var resolved = container.Resolve<ClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);
        }

        [Fact]
        public void StaticResolve_ExpressionWithResolveType_ReturnsInstance()
        {
            IocContainer.Register<IService, MyService>();
            IocContainer.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(IocContainer.Resolve<IService>()));

            var resolved = IocContainer.Resolve<IClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_ExpressionWithResolveType_ReturnsInstance()
        {
            using var container = new IocContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()));

            var resolved = container.Resolve<IClassWith3Ctors>();
            Assert.IsType<ClassWith3Ctors>(resolved);
        }


        [Fact]
        public void StaticResolve_ExpressionWithKey_ReturnsInstance()
        {
            IocContainer.Register<IService, MyService>();
            IocContainer.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(IocContainer.Resolve<IService>()), "test");

            var resolved = IocContainer.Resolve<ClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_ExpressionWithKey_ReturnsInstance()
        {
            using var container = new IocContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<ClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()), "test");

            var resolved = container.Resolve<ClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);
        }

        [Fact]
        public void StaticResolve_ExpressionWithResolveTypeWithKey_ReturnsInstance()
        {
            IocContainer.Register<IService, MyService>();
            IocContainer.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(IocContainer.Resolve<IService>()), "test");

            var resolved = IocContainer.Resolve<IClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Resolve_ExpressionWithResolveTypeWithKey_ReturnsInstance()
        {
            using var container = new IocContainer();
            container.Register<IService, MyService>();
            container.RegisterExplicit<IClassWith3Ctors,ClassWith3Ctors>(c => new ClassWith3Ctors(c.Resolve<IService>()), "test");

            var resolved = container.Resolve<IClassWith3Ctors>("test");
            Assert.IsType<ClassWith3Ctors>(resolved);
        }

        #endregion

        #region Resolve(Type) overloads
        [Fact]
        public void StaticResolveType_InstanceRegistered_ReturnsInstance()
        {
            IocContainer.ResetContainer();

            var instance = new MyService();
            IocContainer.RegisterInstance(instance);
            var resolved = IocContainer.Resolve(typeof(MyService));
            Assert.Equal(instance, resolved); //exactly same object returned

            IocContainer.ResetContainer();
        }

        [Fact]
        public void ResolveType_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            using var container = new IocContainer();
            container.RegisterInstance(instance);
            var resolved = container.Resolve(typeof(MyService));
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        [Fact]
        public void StaticResolveTypeWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            IocContainer.RegisterInstance(instance, "test");
            var resolved = IocContainer.Resolve(typeof(MyService), "test");
            Assert.Equal(instance, resolved); //exactly same object returned

            IocContainer.ResetContainer();
        }

        [Fact]
        public void ResolveTypeWithKey_InstanceRegistered_ReturnsInstance()
        {
            var instance = new MyService();
            using var container = new IocContainer();
            container.RegisterInstance(instance, "test");
            var resolved = container.Resolve(typeof(MyService), "test");
            Assert.Equal(instance, resolved); //exactly same object returned
        }

        #endregion

        #endregion

        #region Unregister
        [Fact]
        public void StaticUnregister_NotRegistered_ThrowsResolutionExceptoin()
        {
            Assert.Throws<ResolveException>(() => IocContainer.Unregister<ConcreteOnly>());
        }

        [Fact]
        public void StaticUnregister_ExceptionWhileDisposing_ThrowsException()
        {
            //Ensure MetaObject.Instance is set
            IocContainer.Register<ClassThatThrowsOnDisposed>().SingleInstance();

            IocContainer.Resolve<ClassThatThrowsOnDisposed>();

            Assert.Throws<Exception>(() => IocContainer.Unregister<ClassThatThrowsOnDisposed>());

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregister_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<ConcreteOnly>();

            Assert.NotEmpty(mock);

            IocContainer.Unregister<ConcreteOnly>();

            Assert.Empty(mock);

            IocContainer.ResetContainer();
        }
        [Fact]
        public void Unregister_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<ConcreteOnly>();

            Assert.NotEmpty(mock);

            container.Unregister<ConcreteOnly>();

            Assert.Empty(mock);
        }
        [Fact]
        public void StaticUnregisterWithKey_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            IocContainer.Unregister<ConcreteOnly>("test");

            Assert.Empty(mock);

            IocContainer.ResetContainer();
        }
        [Fact]
        public void UnregisterWithKey_Registered_IsRemoved()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            container.Unregister<ConcreteOnly>("test");

            Assert.Empty(mock);
        }
        [Fact]
        public void StaticUnregisterWithKey_NotRegistered_ThrowsResolutionException()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<ConcreteOnly>("test");

            Assert.NotEmpty(mock);

            Assert.Throws<ResolveException>(
                () => IocContainer.Unregister<ConcreteOnly>("wrongKey"));

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregister_Registered_DisposeCalled()
        {
            bool wasDisposedExplicitly = false;
            var disposable = new ClassThatsDisposable(() => wasDisposedExplicitly = true, () => { });
            IocContainer.RegisterInstance<IClassThatsDisposable>(disposable);
            IocContainer.Resolve<IClassThatsDisposable>();

            IocContainer.Unregister<IClassThatsDisposable>();

            Assert.True(wasDisposedExplicitly);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void StaticUnregisterAll()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            IocContainer.SetContainer(mock);

            IocContainer.Register<MyService>();
            IocContainer.Register<ConcreteOnly>();
            IocContainer.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            IocContainer.UnregisterAll();

            Assert.Empty(mock);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void UnregisterAll()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();
            container.Register<ConcreteOnly>();
            container.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            container.UnregisterAll();

            Assert.Empty(mock);
        }

        [Fact]
        public void Dispose()
        {
            var mock = new ConcurrentDictionary<Tuple<Type, string>, MetaObject>();
            using var container = new IocContainer(mock, ContainerOptions.Default);

            container.Register<MyService>();
            container.Register<ConcreteOnly>();
            container.Register<ClassWith3Ctors>();

            Assert.Equal(3, mock.Count);

            container.Dispose();

            Assert.Empty(mock);
        }

        #endregion

        #region ChildContainers

        [Fact]
        public void NewChildContainer_CreatesNewContainer()
        {
            using  var container = new IocContainer();
            var child = container.NewChildContainer();

            Assert.IsAssignableFrom<IIocContainer>(child);

            //Repeat for Static implementation
            child = IocContainer.NewChildContainer();

            Assert.IsAssignableFrom<IIocContainer>(child);
        }

        [Fact]
        public void NewChildContainer_SetsNewContainersParent()
        {
            using  var container = new IocContainer();
            var child = container.NewChildContainer();

            Assert.Equal(container, child.GetParent());

            //Repeat for Static implementation
            child = IocContainer.NewChildContainer();
            Assert.Equal(IocContainer.Instance, child.GetParent());
        }


        [Fact]
        public void Parent_ReturnsParentContainer()
        {
            using  var container = new IocContainer();

            //Cast in order to test IDicontainerExtensions
            IocContainer child = container.NewChildContainer() as IocContainer;

            Assert.Equal(container, child.GetParent());
        }

        [Fact]
        public void NewChildContainer_SetsChild()
        {
            using  var container = new IocContainer();
            var child = container.NewChildContainer();

            Assert.Equal(child, container.GetChildren()[0]);
            
            //Repeat for Static implementation
            child = IocContainer.NewChildContainer();
            Assert.Equal(child, IocContainer.GetChildren()[0]);

            IocContainer.Initialize(ContainerOptions.Default); //Reset options to default
        }

        [Fact]
        public void NewChildContainer_ContainerOptions_AreGlobal()
        {
            ContainerOptions options = new ContainerOptions
            {
                TryResolveUnregistered = false
            };
            using  var container = new IocContainer(options);
            var child = container.NewChildContainer();

            Assert.False(IocContainer.options.TryResolveUnregistered);
        }


        #endregion

        #region Named Containers
        [Fact]
        public void GetContainer_ReturnsNamedContainer()
        {
            using var mainContainer = new IocContainer();
            using var namedContainer = new IocContainer("namedContainer");

            Assert.Equal(namedContainer, mainContainer.GetContainer("namedContainer"));

            //Static implementation
            Assert.Equal(namedContainer, IocContainer.GetContainer("namedContainer"));
        }

        [Fact]
        public void GetContainer_NameWrong_Throws()
        {
            using var mainContainer = new IocContainer();
            using var namedContainer = new IocContainer("namedContainer");

            Assert.Throws<KeyNotFoundException>(()=> mainContainer.GetContainer("wrongName"));
        }


        [Fact]
        public void SetName_RemovesOldNameFromRegister()
        {
            using  var container = new IocContainer("OldName")
            {
                Name = "NewName"
            };
            
            Assert.Equal(2, IocContainer.containers.Count);
            Assert.False(IocContainer.containers.TryGetValue("OldName", out _));
        }

        [Fact]
        public void SetName_AlreadyExists_Throws()
        {
            using var container = new IocContainer("namedContainer");
            using var container2 = new IocContainer();
            Assert.Throws<ArgumentException>(()=>container2.Name = "namedContainer");
        }

        #endregion

        #region Exceptions
        [Fact]
        public void StaticRegisterConcreteType_Duplicate_ThrowsRegistrationException()
        {
            IocContainer.Register<MyService>();
            Assert.Throws<RegisterException>(() => IocContainer.Register<MyService>());
        }

        [Fact]
        public void StaticRegisterConcreteTypeWithKey_Duplicate_ThrowsRegistrationException()
        {
            IocContainer.Register<MyService>("test");
            Assert.Throws<RegisterException>(() => IocContainer.Register<MyService>("test"));
        }

        [Fact]
        public void StaticResolveClassWithInternalConstructor_ResolvesPublicEvenThoughInternalIsGreediest()
        {
            IocContainer
                .Register<ClassWithInternalConstructor>();

            var resolved = IocContainer.Resolve<ClassWithInternalConstructor>();
            Assert.Equal("public", resolved.ConstructorUsed);

            IocContainer.ResetContainer();
        }



        #endregion

    }
}
