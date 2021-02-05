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
            DiContainer.ResetContainer();

            PrepareBasic();

            RegisterPropertyInjection();
            RegisterOpenGeneric();
            RegisterMultiple();
            RegisterConditional();
            DiContainer.Compile();
        }

        void PrepareBasic()
        {
            DiContainer.ResetContainer();
            RegisterDummies();
            RegisterStandard();
            RegisterComplexObject();
            DiContainer.Compile();
        }


        private void RegisterDummies()
        {
            DiContainer.Register<IDummyOne, DummyOne>();
            DiContainer.Register<IDummyTwo, DummyTwo>();
            DiContainer.Register<IDummyThree, DummyThree>();
            DiContainer.Register<IDummyFour, DummyFour>();
            DiContainer.Register<IDummyFive, DummyFive>();
            DiContainer.Register<IDummySix, DummySix>();
            DiContainer.Register<IDummySeven, DummySeven>();
            DiContainer.Register<IDummyEight, DummyEight>();
            DiContainer.Register<IDummyNine, DummyNine>();
            DiContainer.Register<IDummyTen, DummyTen>();
        }

        private void RegisterStandard()
        {
            DiContainer.Register<ISingleton1, Singleton1>().SingleInstance();
            DiContainer.Register<ISingleton2, Singleton2>().SingleInstance();
            DiContainer.Register<ISingleton3, Singleton3>().SingleInstance();

            DiContainer.Register<ITransient1, Transient1>();
            DiContainer.Register<ITransient2, Transient2>();
            DiContainer.Register<ITransient3, Transient3>();

            DiContainer.Register<ICombined1, Combined1>();
            DiContainer.Register<ICombined2, Combined2>();
            DiContainer.Register<ICombined3, Combined3>();
        }

        private void RegisterComplexObject()
        {
            DiContainer.Register<IFirstService, FirstService>();
            DiContainer.Register<ISecondService, SecondService>();
            DiContainer.Register<IThirdService, ThirdService>();

            DiContainer.Register<ISubObjectOne,SubObjectOne>();
            DiContainer.Register<ISubObjectTwo,SubObjectTwo>();
            DiContainer.Register<ISubObjectThree,SubObjectThree>();

            DiContainer.Register<IComplex1,Complex1>();
            DiContainer.Register<IComplex2,Complex2>();
            DiContainer.Register<IComplex3,Complex3>();
        }

        private void RegisterPropertyInjection()
        {
            DiContainer.Register<IServiceA, ServiceA>().SingleInstance();
            DiContainer.Register<IServiceB, ServiceB>().SingleInstance();
            DiContainer.Register<IServiceC, ServiceC>().SingleInstance();
            DiContainer.Register<ISubObjectA, SubObjectA>();
            DiContainer.Register<ISubObjectB, SubObjectB>();
            DiContainer.Register<ISubObjectC, SubObjectC>();
            DiContainer.Register<IComplexPropertyObject1, ComplexPropertyObject1>();
            DiContainer.Register<IComplexPropertyObject2, ComplexPropertyObject2>();
            DiContainer.Register<IComplexPropertyObject3, ComplexPropertyObject3>();

        }

        private void RegisterOpenGeneric()
        {
            DiContainer.RegisterType(typeof(GenericExport<>), typeof(IGenericInterface<>)); //todo should be multi
            DiContainer.RegisterType(typeof(ImportGeneric<>));
        }

        private void RegisterMultiple()
        {
            DiContainer.Register<ISimpleAdapter, SimpleAdapterOne>();
            DiContainer.Register<ISimpleAdapter, SimpleAdapterTwo>("2");
            DiContainer.Register<ISimpleAdapter, SimpleAdapterThree>("3");
            DiContainer.Register<ISimpleAdapter, SimpleAdapterFour>("4");
            DiContainer.Register<ISimpleAdapter, SimpleAdapterFive>("5");
            DiContainer.Register<IEnumerable<ISimpleAdapter>>();

            DiContainer.Register<ImportMultiple1>();
            DiContainer.Register<ImportMultiple2>();
            DiContainer.Register<ImportMultiple3>();
        }

        private void RegisterConditional()
        {
            DiContainer.Register<IExportConditionInterface, ExportConditionalObject1>("ExportConditionalObject1");
            DiContainer.Register<IExportConditionInterface, ExportConditionalObject2>("ExportConditionalObject2");
            DiContainer.Register<IExportConditionInterface, ExportConditionalObject3>("ExportConditionalObject3");

            DiContainer.RegisterExplicit<ImportConditionObject1,ImportConditionObject1>(c=>new ImportConditionObject1(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject1")));
            DiContainer.RegisterExplicit<ImportConditionObject2,ImportConditionObject2>(c => new ImportConditionObject2(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject2")));
            DiContainer.RegisterExplicit<ImportConditionObject3,ImportConditionObject3>(c => new ImportConditionObject3(DiContainer.Resolve<IExportConditionInterface>("ExportConditionalObject3")));
        }

        [Fact]
        public void InstanceIEnumerable()
        {
            //todo test for abstract too
            var container = new DiContainer();
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
            DiContainer.ResetContainer();
            DiContainer.RegisterType(typeof(GenericExport<>), typeof(IGenericInterface<>));
            DiContainer.RegisterType(typeof(ImportGeneric<>));

            DiContainer.Resolve<ImportGeneric<int>>();
            Assert.IsType<ImportGeneric<int>>(DiContainer.Resolve(typeof(ImportGeneric<int>)));
            Assert.IsType<ImportGeneric<float>>(DiContainer.Resolve(typeof(ImportGeneric<float>)));
            Assert.IsType<ImportGeneric<object>>(DiContainer.Resolve(typeof(ImportGeneric<object>)));
            //try 2nd resolve
            Assert.IsType<ImportGeneric<int>>(DiContainer.Resolve(typeof(ImportGeneric<int>)));
            Assert.IsType<ImportGeneric<float>>(DiContainer.Resolve(typeof(ImportGeneric<float>)));
            Assert.IsType<ImportGeneric<object>>(DiContainer.Resolve(typeof(ImportGeneric<object>)));
            DiContainer.ResetContainer();
        }

        [Fact]
        public void PrepareAndRegister()
        {
            PrepareBasic();
            DiContainer.ResetContainer(); //was dipose

        }

        [Fact]
        public void Combined()
        {
            RegisterStandard();

            for (int i = 0; i < 2; i++)
            {
                var combined1 = (ICombined1)DiContainer.Resolve(typeof(ICombined1));
                var combined2 = (ICombined2)DiContainer.Resolve(typeof(ICombined2));
                var combined3 = (ICombined3)DiContainer.Resolve(typeof(ICombined3));
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

            DiContainer.ResetContainer();
        }

        [Fact]
        public void Transient()
        {
            ResetInstanceCounters();
            RegisterStandard();

            for (int i = 0; i < 2; i++)
            {
                var transient1 = (ITransient1)DiContainer.Resolve(typeof(ITransient1));
                var transient2 = (ITransient2)DiContainer.Resolve(typeof(ITransient2));
                var transient3 = (ITransient3)DiContainer.Resolve(typeof(ITransient3));
            }

            Assert.Equal(2, Transient1.Instances);
            Assert.Equal(2, Transient2.Instances);
            Assert.Equal(2, Transient3.Instances);

            DiContainer.ResetContainer();
        }

        [Fact]
        public void PrepareAndRegisterAndSimpleResolve()
        {
            ResetInstanceCounters();
            PrepareBasic();
            DiContainer.Resolve(typeof(IDummyOne));
            DiContainer.Resolve(typeof(ISingleton1));

            Assert.Equal(1, Singleton1.Instances);
            DiContainer.ResetContainer();

        }

        [Fact]
        public void Singleton()
        {
            ResetInstanceCounters();
            RegisterStandard();

            for (int i = 0; i < 2; i++)
            {

                var singleton1 = (ISingleton1)DiContainer.Resolve(typeof(ISingleton1));
                var singleton2 = (ISingleton2)DiContainer.Resolve(typeof(ISingleton2));
                var singleton3 = (ISingleton3)DiContainer.Resolve(typeof(ISingleton3));
            }

            Assert.Equal(1, Singleton1.Instances);
            Assert.Equal(1, Singleton2.Instances);
            Assert.Equal(1, Singleton3.Instances);

            DiContainer.ResetContainer();

        }

        [Fact]
        public void Complex()
        {
            ResetInstanceCounters();
            RegisterComplexObject();

            for (int i = 0; i < 2; i++)
            {
                var complex1 = (IComplex1)DiContainer.Resolve(typeof(IComplex1));
                var complex2 = (IComplex2)DiContainer.Resolve(typeof(IComplex2));
                var complex3 = (IComplex3)DiContainer.Resolve(typeof(IComplex3));
            }

            Assert.Equal(2, Complex1.Instances);
            Assert.Equal(2, Complex2.Instances);
            Assert.Equal(2, Complex3.Instances);

            DiContainer.ResetContainer();

        }

        [Fact]
        public void Property()
        {
            ResetInstanceCounters();
            RegisterPropertyInjection();

            for (int i = 0; i < 2; i++)
            {
                var complex1 = (IComplexPropertyObject1)DiContainer.Resolve(typeof(IComplexPropertyObject1));
                var complex2 = (IComplexPropertyObject2)DiContainer.Resolve(typeof(IComplexPropertyObject2));
                var complex3 = (IComplexPropertyObject3)DiContainer.Resolve(typeof(IComplexPropertyObject3));
            }

            Assert.Equal(2, ComplexPropertyObject1.Instances);
            Assert.Equal(2, ComplexPropertyObject2.Instances);
            Assert.Equal(2, ComplexPropertyObject3.Instances);

            DiContainer.ResetContainer();

        }

        [Fact]
        public void Generics()
        {
            ResetInstanceCounters();
            RegisterOpenGeneric();

            for (int i = 0; i < 2; i++)
            {
                var generic1 = (ImportGeneric<int>)DiContainer.Resolve(typeof(ImportGeneric<int>));
                var generic2 = (ImportGeneric<float>)DiContainer.Resolve(typeof(ImportGeneric<float>));
                var generic3 = (ImportGeneric<object>)DiContainer.Resolve(typeof(ImportGeneric<object>));
            }

            Assert.Equal(2, ImportGeneric<int>.Instances);
            Assert.Equal(2, ImportGeneric<float>.Instances);
            Assert.Equal(2, ImportGeneric<object>.Instances);

            DiContainer.ResetContainer();

        }

        [Fact]
        public void IEnumerable()
        {
            ResetInstanceCounters();
            RegisterMultiple();

            for (int i = 0; i < 2; i++)
            {
                var importMultiple1 = (ImportMultiple1)DiContainer.Resolve(typeof(ImportMultiple1));
                var importMultiple2 = (ImportMultiple2)DiContainer.Resolve(typeof(ImportMultiple2));
                var importMultiple3 = (ImportMultiple3)DiContainer.Resolve(typeof(ImportMultiple3));
            }

            Assert.Equal(2, ImportMultiple1.Instances);
            Assert.Equal(2, ImportMultiple2.Instances);
            Assert.Equal(2, ImportMultiple3.Instances);

            DiContainer.ResetContainer();

        }

        [Fact]
        public void Conditional()
        {
            ResetInstanceCounters();
            RegisterConditional();

            for (int i = 0; i < 2; i++)
            {
                var importConditionObject1 = (ImportConditionObject1)DiContainer.Resolve(typeof(ImportConditionObject1));
                var importConditionObject2 = (ImportConditionObject2)DiContainer.Resolve(typeof(ImportConditionObject2));
                var importConditionObject3 = (ImportConditionObject3)DiContainer.Resolve(typeof(ImportConditionObject3));
            }

            Assert.Equal(2, ImportConditionObject1.Instances);
            Assert.Equal(2, ImportConditionObject2.Instances);
            Assert.Equal(2, ImportConditionObject3.Instances);

            DiContainer.ResetContainer();

        }

        [Fact]
        public void ChildContainer()
        {
            ResetInstanceCounters();

            var container = new DiContainer();
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
