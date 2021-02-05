using System;
using Xunit;
using ZenIoc;
using IocPerformance.Classes.Generics;
using Xunit.Abstractions;
using IocPerformance.Classes.Multiple;
using System.Collections.Generic;
using IocPerformance.Classes.Dummy;
using IocPerformance.Classes.Standard;
using IocPerformance.Classes.Complex;
using IocPerformance.Classes.Properties;
using IocPerformance.Classes.Conditions;
using IocPerformance.Classes.Child;

namespace ZenIocTests
{
    [Collection("ZenIoc")]
    public class FeatureTests
    {
        void Prepare()
        {
            IocContainer.ResetContainer();

            PrepareBasic();

            RegisterPropertyInjection();
            RegisterOpenGeneric();
            RegisterMultiple();
            RegisterConditional();
            IocContainer.Compile();
        }

        void PrepareBasic()
        {
            IocContainer.ResetContainer();
            RegisterDummies();
            RegisterStandard();
            RegisterComplexObject();
            IocContainer.Compile();
        }


        private void RegisterDummies()
        {
            IocContainer.Register<IDummyOne, DummyOne>();
            IocContainer.Register<IDummyTwo, DummyTwo>();
            IocContainer.Register<IDummyThree, DummyThree>();
            IocContainer.Register<IDummyFour, DummyFour>();
            IocContainer.Register<IDummyFive, DummyFive>();
            IocContainer.Register<IDummySix, DummySix>();
            IocContainer.Register<IDummySeven, DummySeven>();
            IocContainer.Register<IDummyEight, DummyEight>();
            IocContainer.Register<IDummyNine, DummyNine>();
            IocContainer.Register<IDummyTen, DummyTen>();
        }

        private void RegisterStandard()
        {
            IocContainer.Register<ISingleton1, Singleton1>().SingleInstance();
            IocContainer.Register<ISingleton2, Singleton2>().SingleInstance();
            IocContainer.Register<ISingleton3, Singleton3>().SingleInstance();

            IocContainer.Register<ITransient1, Transient1>();
            IocContainer.Register<ITransient2, Transient2>();
            IocContainer.Register<ITransient3, Transient3>();

            IocContainer.Register<ICombined1, Combined1>();
            IocContainer.Register<ICombined2, Combined2>();
            IocContainer.Register<ICombined3, Combined3>();
        }

        private void RegisterComplexObject()
        {
            IocContainer.Register<IFirstService, FirstService>();
            IocContainer.Register<ISecondService, SecondService>();
            IocContainer.Register<IThirdService, ThirdService>();

            IocContainer.Register<ISubObjectOne,SubObjectOne>();
            IocContainer.Register<ISubObjectTwo,SubObjectTwo>();
            IocContainer.Register<ISubObjectThree,SubObjectThree>();

            IocContainer.Register<IComplex1,Complex1>();
            IocContainer.Register<IComplex2,Complex2>();
            IocContainer.Register<IComplex3,Complex3>();
        }

        private void RegisterPropertyInjection()
        {
            IocContainer.Register<IServiceA, ServiceA>().SingleInstance();
            IocContainer.Register<IServiceB, ServiceB>().SingleInstance();
            IocContainer.Register<IServiceC, ServiceC>().SingleInstance();
            IocContainer.Register<ISubObjectA, SubObjectA>();
            IocContainer.Register<ISubObjectB, SubObjectB>();
            IocContainer.Register<ISubObjectC, SubObjectC>();
            IocContainer.Register<IComplexPropertyObject1, ComplexPropertyObject1>();
            IocContainer.Register<IComplexPropertyObject2, ComplexPropertyObject2>();
            IocContainer.Register<IComplexPropertyObject3, ComplexPropertyObject3>();

        }

        private void RegisterOpenGeneric()
        {
            IocContainer.RegisterType(typeof(GenericExport<>), typeof(IGenericInterface<>)); //todo should be multi
            IocContainer.RegisterType(typeof(ImportGeneric<>));
        }

        private void RegisterMultiple()
        {
            IocContainer.Register<ISimpleAdapter, SimpleAdapterOne>();
            IocContainer.Register<ISimpleAdapter, SimpleAdapterTwo>("2");
            IocContainer.Register<ISimpleAdapter, SimpleAdapterThree>("3");
            IocContainer.Register<ISimpleAdapter, SimpleAdapterFour>("4");
            IocContainer.Register<ISimpleAdapter, SimpleAdapterFive>("5");
            IocContainer.Register<IEnumerable<ISimpleAdapter>>();

            IocContainer.Register<ImportMultiple1>();
            IocContainer.Register<ImportMultiple2>();
            IocContainer.Register<ImportMultiple3>();
        }

        private void RegisterConditional()
        {
            IocContainer.Register<IExportConditionInterface, ExportConditionalObject1>("ExportConditionalObject1");
            IocContainer.Register<IExportConditionInterface, ExportConditionalObject2>("ExportConditionalObject2");
            IocContainer.Register<IExportConditionInterface, ExportConditionalObject3>("ExportConditionalObject3");

            IocContainer.RegisterExplicit<ImportConditionObject1,ImportConditionObject1>(c=>new ImportConditionObject1(IocContainer.Resolve<IExportConditionInterface>("ExportConditionalObject1")));
            IocContainer.RegisterExplicit<ImportConditionObject2,ImportConditionObject2>(c => new ImportConditionObject2(IocContainer.Resolve<IExportConditionInterface>("ExportConditionalObject2")));
            IocContainer.RegisterExplicit<ImportConditionObject3,ImportConditionObject3>(c => new ImportConditionObject3(IocContainer.Resolve<IExportConditionInterface>("ExportConditionalObject3")));
        }

        [Fact]
        public void InstanceIEnumerable()
        {
            //todo test for abstract too
            var container = new IocContainer();
            container.Register<ISimpleAdapter, SimpleAdapterOne>();
            container.Register<ISimpleAdapter, SimpleAdapterTwo>("2");
            container.Register<ISimpleAdapter, SimpleAdapterThree>("3");
            container.Register<ISimpleAdapter, SimpleAdapterFour>("4");
            container.Register<ISimpleAdapter, SimpleAdapterFive>("5");
            container.Register<IEnumerable<ISimpleAdapter>>();

            container.RegisterExplicit<ImportMultiple1, ImportMultiple1>(c => new ImportMultiple1(c.Resolve<IEnumerable<ISimpleAdapter>>()));
            container.RegisterExplicit<ImportMultiple2, ImportMultiple2>(c => new ImportMultiple2(c.Resolve<IEnumerable<ISimpleAdapter>>()));
            container.RegisterExplicit<ImportMultiple3, ImportMultiple3>(c => new ImportMultiple3(c.Resolve<IEnumerable<ISimpleAdapter>>()));


            var importMultiple1 = (ImportMultiple1)container.Resolve(typeof(ImportMultiple1));
            var importMultiple2 = (ImportMultiple2)container.Resolve(typeof(ImportMultiple2));
            var importMultiple3 = (ImportMultiple3)container.Resolve(typeof(ImportMultiple3));

        }

        [Fact]
        public void RegisterGenericForReal()
        {
            IocContainer.ResetContainer();
            IocContainer.RegisterType(typeof(GenericExport<>), typeof(IGenericInterface<>));
            IocContainer.RegisterType(typeof(ImportGeneric<>));

            IocContainer.Resolve<ImportGeneric<int>>();
            Assert.IsType<ImportGeneric<int>>(IocContainer.Resolve(typeof(ImportGeneric<int>)));
            Assert.IsType<ImportGeneric<float>>(IocContainer.Resolve(typeof(ImportGeneric<float>)));
            Assert.IsType<ImportGeneric<object>>(IocContainer.Resolve(typeof(ImportGeneric<object>)));
            //try 2nd resolve
            Assert.IsType<ImportGeneric<int>>(IocContainer.Resolve(typeof(ImportGeneric<int>)));
            Assert.IsType<ImportGeneric<float>>(IocContainer.Resolve(typeof(ImportGeneric<float>)));
            Assert.IsType<ImportGeneric<object>>(IocContainer.Resolve(typeof(ImportGeneric<object>)));
            IocContainer.ResetContainer();
        }

        [Fact]
        public void PrepareAndRegister()
        {
            PrepareBasic();
            IocContainer.ResetContainer(); //was dipose

        }

        [Fact]
        public void Combined()
        {
            RegisterStandard();

            for (int i = 0; i < 2; i++)
            {
                var combined1 = (ICombined1)IocContainer.Resolve(typeof(ICombined1));
                var combined2 = (ICombined2)IocContainer.Resolve(typeof(ICombined2));
                var combined3 = (ICombined3)IocContainer.Resolve(typeof(ICombined3));
            }

            Assert.Equal(1, Singleton1.Instances);
            Assert.Equal(1, Singleton2.Instances);
            Assert.Equal(1, Singleton3.Instances);

            Assert.Equal(2, Combined1.Instances);
            Assert.Equal(2, Combined2.Instances);
            Assert.Equal(2, Combined3.Instances);

            Assert.Equal(2, Transient1.Instances);
            Assert.Equal(2, Transient2.Instances);
            Assert.Equal(2, Transient3.Instances);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void Transient()
        {
            ResetInstanceCounters();
            RegisterStandard();

            for (int i = 0; i < 2; i++)
            {
                var transient1 = (ITransient1)IocContainer.Resolve(typeof(ITransient1));
                var transient2 = (ITransient2)IocContainer.Resolve(typeof(ITransient2));
                var transient3 = (ITransient3)IocContainer.Resolve(typeof(ITransient3));
            }

            Assert.Equal(2, Transient1.Instances);
            Assert.Equal(2, Transient2.Instances);
            Assert.Equal(2, Transient3.Instances);

            IocContainer.ResetContainer();
        }

        [Fact]
        public void PrepareAndRegisterAndSimpleResolve()
        {
            ResetInstanceCounters();
            PrepareBasic();
            IocContainer.Resolve(typeof(IDummyOne));
            IocContainer.Resolve(typeof(ISingleton1));

            Assert.Equal(1, Singleton1.Instances);
            IocContainer.ResetContainer();

        }

        [Fact]
        public void Singleton()
        {
            ResetInstanceCounters();
            RegisterStandard();

            for (int i = 0; i < 2; i++)
            {

                var singleton1 = (ISingleton1)IocContainer.Resolve(typeof(ISingleton1));
                var singleton2 = (ISingleton2)IocContainer.Resolve(typeof(ISingleton2));
                var singleton3 = (ISingleton3)IocContainer.Resolve(typeof(ISingleton3));
            }

            Assert.Equal(1, Singleton1.Instances);
            Assert.Equal(1, Singleton2.Instances);
            Assert.Equal(1, Singleton3.Instances);

            IocContainer.ResetContainer();

        }

        [Fact]
        public void Complex()
        {
            ResetInstanceCounters();
            RegisterComplexObject();

            for (int i = 0; i < 2; i++)
            {
                var complex1 = (IComplex1)IocContainer.Resolve(typeof(IComplex1));
                var complex2 = (IComplex2)IocContainer.Resolve(typeof(IComplex2));
                var complex3 = (IComplex3)IocContainer.Resolve(typeof(IComplex3));
            }

            Assert.Equal(2, Complex1.Instances);
            Assert.Equal(2, Complex2.Instances);
            Assert.Equal(2, Complex3.Instances);

            IocContainer.ResetContainer();

        }

        [Fact]
        public void Property()
        {
            ResetInstanceCounters();
            RegisterPropertyInjection();

            for (int i = 0; i < 2; i++)
            {
                var complex1 = (IComplexPropertyObject1)IocContainer.Resolve(typeof(IComplexPropertyObject1));
                var complex2 = (IComplexPropertyObject2)IocContainer.Resolve(typeof(IComplexPropertyObject2));
                var complex3 = (IComplexPropertyObject3)IocContainer.Resolve(typeof(IComplexPropertyObject3));
            }

            Assert.Equal(2, ComplexPropertyObject1.Instances);
            Assert.Equal(2, ComplexPropertyObject2.Instances);
            Assert.Equal(2, ComplexPropertyObject3.Instances);

            IocContainer.ResetContainer();

        }

        [Fact]
        public void Generics()
        {
            ResetInstanceCounters();
            RegisterOpenGeneric();

            for (int i = 0; i < 2; i++)
            {
                var generic1 = (ImportGeneric<int>)IocContainer.Resolve(typeof(ImportGeneric<int>));
                var generic2 = (ImportGeneric<float>)IocContainer.Resolve(typeof(ImportGeneric<float>));
                var generic3 = (ImportGeneric<object>)IocContainer.Resolve(typeof(ImportGeneric<object>));
            }

            Assert.Equal(2, ImportGeneric<int>.Instances);
            Assert.Equal(2, ImportGeneric<float>.Instances);
            Assert.Equal(2, ImportGeneric<object>.Instances);

            IocContainer.ResetContainer();

        }

        [Fact]
        public void IEnumerable()
        {
            ResetInstanceCounters();
            RegisterMultiple();

            for (int i = 0; i < 2; i++)
            {
                var importMultiple1 = (ImportMultiple1)IocContainer.Resolve(typeof(ImportMultiple1));
                var importMultiple2 = (ImportMultiple2)IocContainer.Resolve(typeof(ImportMultiple2));
                var importMultiple3 = (ImportMultiple3)IocContainer.Resolve(typeof(ImportMultiple3));
            }

            Assert.Equal(2, ImportMultiple1.Instances);
            Assert.Equal(2, ImportMultiple2.Instances);
            Assert.Equal(2, ImportMultiple3.Instances);

            IocContainer.ResetContainer();

        }

        [Fact]
        public void Conditional()
        {
            ResetInstanceCounters();
            RegisterConditional();

            for (int i = 0; i < 2; i++)
            {
                var importConditionObject1 = (ImportConditionObject1)IocContainer.Resolve(typeof(ImportConditionObject1));
                var importConditionObject2 = (ImportConditionObject2)IocContainer.Resolve(typeof(ImportConditionObject2));
                var importConditionObject3 = (ImportConditionObject3)IocContainer.Resolve(typeof(ImportConditionObject3));
            }

            Assert.Equal(2, ImportConditionObject1.Instances);
            Assert.Equal(2, ImportConditionObject2.Instances);
            Assert.Equal(2, ImportConditionObject3.Instances);

            IocContainer.ResetContainer();

        }

        [Fact]
        public void ChildContainer()
        {
            ResetInstanceCounters();

            var container = new IocContainer();
            container.Register<ISingleton1, Singleton1>().SingleInstance();
            container.Register<ISingleton2, Singleton2>().SingleInstance();
            container.Register<ISingleton3, Singleton3>().SingleInstance();
            
            var child = container.NewChildContainer();
            child.Register<ITransient1, ScopedTransient>();
            child.Register<ICombined1, ScopedCombined1>();
            child.Register<ICombined2, ScopedCombined2>();
            child.Register<ICombined3, ScopedCombined3>();

            for (int i = 0; i < 2; i++)
            {
                var scopedCombined1 = (ICombined1)child.Resolve(typeof(ICombined1));
                var scopedCombined2 = (ICombined2)child.Resolve(typeof(ICombined2));
                var scopedCombined3 = (ICombined3)child.Resolve(typeof(ICombined3));
            }

            Assert.Equal(2, ScopedCombined1.Instances);
            Assert.Equal(2, ScopedCombined2.Instances);
            Assert.Equal(2, ScopedCombined3.Instances);

        }

        private void ResetInstanceCounters()
        {
            Transient1.Instances = Transient2.Instances = Transient3.Instances = 0;
            Singleton1.Instances = Singleton2.Instances = Singleton3.Instances = 0;
            Complex1.Instances = Complex2.Instances = Complex3.Instances = 0;
            ImportMultiple1.Instances = ImportMultiple2.Instances = ImportMultiple3.Instances = 0;
        }
    }
}
