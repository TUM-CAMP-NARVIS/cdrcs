namespace UnitTest
{
    using Cdrcs;
    using NUnit.Framework;

    [TestFixture]
    public class CloningTests
    {
        public void TestCloning<T, U>() 
            where T : class 
            where U : class
        {
            var source = Random.Init<T>();
            var target = Clone<U>.From(source);

            Assert.IsTrue(source.IsEqual(target));
        }

        [Test]
        public void Cloning()
        {
            TestCloning<BasicTypes, BasicTypes>();
            TestCloning<Readonly.BasicTypes, Readonly.BasicTypes>();
            TestCloning<Readonly.SimpleContainers, Readonly.SimpleContainers>();
            TestCloning<BasicTypes, BasicTypesView>();
            TestCloning<NullableBasicTypes, NullableBasicTypes>();
            TestCloning<NullableContainers, NullableContainers>();
            TestCloning<NestedContainers, NestedContainers>();
            TestCloning<Derived, DerivedView>();
            TestCloning<DerivedView, Derived>();
            TestCloning<Derived, Nested>();
            TestCloning<StructWithBlobs, StructWithBlobs>();
            TestCloning<StructWithByteLists, StructWithBlobs>();
            TestCloning<StructWithBlobs, StructWithByteLists>();
            TestCloning<Containers, Containers>();
            TestCloning<Nothing, Nothing>();
            TestCloning<NotNothingView, Nothing>();
            // TODO: cloning with type promotion
            //TestCloning<CdrcsClass<byte>, CdrcsClass<ushort>>();
            //TestCloning<CdrcsClass<float>, CdrcsClass<double>>();
            TestCloning<Tree, Tree>();
        }

        [Test]
        public void CloningCdrcsed()
        {
            var source = new StructWithCdrcsed();

            var field = Random.Init<Derived>();
            source.field = Util.MakeCdrcsedCB(field);

            var poly0 = Random.Init<EmptyBase>();
            var poly1 = Random.Init<Nested>();
            var poly2 = Random.Init<Derived>();

            source.poly.Add(Util.MakeCdrcsedCB(poly0));
            source.poly.Add(Util.MakeCdrcsedCB(poly1));
            source.poly.Add(new Cdrcsed<Derived>(poly2));

            var target = Clone<StructWithCdrcsed>.From(source);

            Assert.IsTrue(Comparer.Equal(field, target.field.Deserialize<Derived>()));
            Assert.IsTrue(Comparer.Equal(poly0, target.poly[0].Deserialize<EmptyBase>()));
            Assert.IsTrue(Comparer.Equal(poly1, target.poly[1].Deserialize()));
            Assert.IsTrue(Comparer.Equal(poly2, target.poly[2].Deserialize<Derived>()));
        }
    }
}
