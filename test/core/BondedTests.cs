namespace UnitTest
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using Cdrcs;
    using Cdrcs.Protocols;
    using Cdrcs.IO;
    using Cdrcs.IO.Unsafe;
    using Cdrcs.Internal.Reflection;

    [TestFixture]
    public class CdrcsedTests
    {
        // Deserialize T from ICdrcsed<T> containing an instance or payload of derived class.
        void CdrcsedDeserialize<T, D>() 
            where T : class
            where D : class, T, new()
        {
            var from = Random.Init<D>();

            ICdrcsed<T> bondedInstance = new Cdrcsed<D>(from);
            ICdrcsed<T> bondedPayloadCB = Util.MakeCdrcsedCB(from);
            ICdrcsed<T> bondedPayloadCB2 = Util.MakeCdrcsedCB2(from);
            ICdrcsed<T> bondedPayloadSP = Util.MakeCdrcsedSP(from);
            ICdrcsed<CdrcsClass<ICdrcsed<T>>> nestedCdrcsed =
                new Cdrcsed<CdrcsClass<ICdrcsed<T>>>(new CdrcsClass<ICdrcsed<T>> { field = bondedInstance });

            for (var i = 2; --i != 0;)
            {
                var to1 = bondedInstance.Deserialize();
                var to2 = bondedPayloadCB.Deserialize();
                var toCb2 = bondedPayloadCB2.Deserialize();
                var to3 = bondedPayloadSP.Deserialize();

                Assert.IsTrue(to1.IsEqual<T>(from));
                Assert.IsTrue(to2.IsEqual<T>(from));
                Assert.IsTrue(toCb2.IsEqual<T>(from));
                Assert.IsTrue(to3.IsEqual<T>(from));
                Assert.IsTrue(nestedCdrcsed.Deserialize().field.Deserialize().IsEqual(from));
            }
        }

        // Deserialize derived class from ICdrcsed<T> containing an instance or payload of derived class.
        void CdrcsedDowncastDeserialize<T, D>()
            where T : class
            where D : class, T, new()
        {
            var from = Random.Init<D>();

            ICdrcsed<T> bondedInstance = new Cdrcsed<D>(from);
            ICdrcsed<T> bondedPayloadCB = Util.MakeCdrcsedCB(from);
            ICdrcsed<T> bondedPayloadCB2 = Util.MakeCdrcsedCB2(from);
            ICdrcsed<T> bondedPayloadSP = Util.MakeCdrcsedSP(from);

            for (var i = 2; --i != 0;)
            {
                var to1 = bondedInstance.Deserialize<D>();
                var to2 = bondedPayloadCB.Deserialize<D>();
                var toCb2 = bondedPayloadCB2.Deserialize<D>();
                var to3 = bondedPayloadSP.Deserialize<D>();

                Assert.IsTrue(to1.IsEqual<D>(from));
                Assert.IsTrue(to2.IsEqual<D>(from));
                Assert.IsTrue(toCb2.IsEqual<D>(from));
                Assert.IsTrue(to3.IsEqual<D>(from));
            }
        }

        // Serialize ICdrcsed<T> (transcoding to different protocol) 
        void CdrcsedSerialize<T, D>(D from, ICdrcsed<T> bonded)
            where T : class
            where D : class, T, new()
        {
            Util.RoundtripStream<ICdrcsed<T>, D> streamRoundtrip = (serialize, deserialize) =>
            {
                var stream = new MemoryStream();
                serialize(bonded, stream);
                stream.Position = 0;
                var to = deserialize(stream);

                Assert.IsTrue(to.IsEqual<D>(from));
            };

            streamRoundtrip(Util.SerializeCB, Util.DeserializeCB<D>);
            streamRoundtrip(Util.SerializeCB2, Util.DeserializeCB2<D>);
            streamRoundtrip(Util.SerializeFB, Util.DeserializeFB<D>);
            streamRoundtrip(Util.SerializeSP, Util.DeserializeSP<D, D>);
            streamRoundtrip(Util.SerializeXml, Util.DeserializeXml<D>);
        }

        void CdrcsedSerialize<D>(D from, ICdrcsed bonded)
            where D : class, new()
        {
            Util.RoundtripStream<ICdrcsed, D> streamRoundtrip = (serialize, deserialize) =>
            {
                var stream = new MemoryStream();
                serialize(bonded, stream);
                stream.Position = 0;
                var to = deserialize(stream);

                Assert.IsTrue(to.IsEqual<D>(from));
            };

            streamRoundtrip(Util.SerializeCB, Util.DeserializeCB<D>);
            streamRoundtrip(Util.SerializeCB2, Util.DeserializeCB2<D>);
            streamRoundtrip(Util.SerializeFB, Util.DeserializeFB<D>);
        }

        void CdrcsedSerialize<T, D>()
            where T : class
            where D : class, T, new()
        {
            var from = Random.Init<D>();

            ICdrcsed<T> bondedInstance = new Cdrcsed<D>(from);
            ICdrcsed<T> bondedPayloadCB = Util.MakeCdrcsedCB(from);
            ICdrcsed<T> bondedPayloadCB2 = Util.MakeCdrcsedCB2(from);
            ICdrcsed<T> bondedPayloadSP = Util.MakeCdrcsedSP(from);

            CdrcsedSerialize(from, bondedInstance);
            CdrcsedSerialize(from, bondedPayloadCB);
            CdrcsedSerialize(from, bondedPayloadCB2);
            CdrcsedSerialize(from, bondedPayloadSP);

            CdrcsedSerialize(from, (ICdrcsed)bondedPayloadCB);
            CdrcsedSerialize(from, (ICdrcsed)bondedPayloadCB2);
        }

        // Serialize a class with ICdrcsed<From> field an deserialize into class with non-lazy struct To field.
        void NonLazyDeserialization<From, To>(
            Action<CdrcsClass<ICdrcsed<From>>, Stream> serialize,
            Func<Stream, CdrcsClass<To>> deserialize)
            where From : class, new()
            where To : class
        {
            var field = Random.Init<From>();
            var from = new CdrcsClass<ICdrcsed<From>>();
            from.field = new Cdrcsed<From>(field);

            var stream = new MemoryStream();
            serialize(from, stream);
            stream.Position = 0;
            var to = deserialize(stream);

            Assert.IsTrue(field.IsEqual(to.field));
        }

        void NonLazyDeserializationAll<From, To>()
            where From : class, new()
            where To : class
        {
            NonLazyDeserialization<From, To>(
                Util.SerializeCB, Util.DeserializeCB<CdrcsClass<To>>);

            NonLazyDeserialization<From, To>(
                Util.SerializeCB2, Util.DeserializeCB2<CdrcsClass<To>>);

            NonLazyDeserialization<From, To>(
                Util.SerializeSP, Util.DeserializeSP<CdrcsClass<ICdrcsed<From>>, CdrcsClass<To>>);

            NonLazyDeserialization<From, To>(
                Util.SerializeXmlWithNamespaces, Util.DeserializeXml<CdrcsClass<To>>);
        }


        // Serialize class with struct From field and deserialize as class with ICdrcsed<To> field.
        void LazyDeserialization<From, To, R>(
            Action<CdrcsClass<From>, Stream> serialize,
            Func<Stream, CdrcsClass<ICdrcsed<To>>> deserialize)
            where From : class, new()
            where To : class 
            where R : ICloneable<R>
        {
            var from = Random.Init<CdrcsClass<From>>();
            var stream = new MemoryStream();
            
            serialize(from, stream);
            stream.Position = 0;
            var to = deserialize(stream);

            Assert.IsTrue(from.field.IsEqual(to.field.Deserialize()));

            var deserializer = new Deserializer<R>(typeof (To), Schema<From>.RuntimeSchema);
            Assert.IsTrue(from.field.IsEqual(deserializer.Deserialize(to.field)));

            Assert.IsTrue(from.field.IsEqual<From>(to.field.Deserialize<From>()));
            Assert.IsTrue(from.field.IsEqual<From>(to.field.Convert<From>().Deserialize()));
        }

        void LazyDeserializationAll<From, To>()
            where From : class, new()
            where To : class
        {
            LazyDeserialization<From, To, CompactBinaryReader<InputStream>>(
                Util.SerializeCB, Util.DeserializeCB<CdrcsClass<ICdrcsed<To>>>);

            LazyDeserialization<From, To, CompactBinaryReader<InputStream>>(
                Util.SerializeCB2, Util.DeserializeCB2<CdrcsClass<ICdrcsed<To>>>);

            LazyDeserialization<From, To, SimpleBinaryReader<InputStream>>(
                Util.SerializeSP, Util.DeserializeSP<CdrcsClass<From>, CdrcsClass<ICdrcsed<To>>>);
        }

        // Serialize class with a ICdrcsed<Through> field containing value From.
        // Deserialize and then deserialize object Through and To from the bonded<Through> field.
        void PolymorphicDeserialization<From, Through, To, R>(
            Action<CdrcsClass<ICdrcsed<Through>>, Stream> serialize,
            Func<Stream, CdrcsClass<ICdrcsed<Through>>> deserialize)
            where From : class, Through, new() 
            where Through : class, new() 
            where To : class, new() 
            where R : ICloneable<R>
        {
            var field = Random.Init<From>();
            var from = new CdrcsClass<ICdrcsed<Through>>();
            from.field = new Cdrcsed<From>(field);

            var stream = new MemoryStream();
            
            serialize(from, stream);
            stream.Position = 0;
            var to = deserialize(stream);

            Assert.IsTrue(field.IsEqual(to.field.Deserialize()));
            Assert.IsTrue(field.IsEqual(to.field.Deserialize<To>()));
            Assert.IsTrue(field.IsEqual(to.field.Convert<To>().Deserialize()));

            // bonded<T> for untagged protocol in not serialized using the protocol R
            // but instead is marshaled using a tagged protocol.
            if (!typeof(IUntaggedProtocolReader).IsAssignableFrom(typeof(R)))
            {
                var deserializer = new Deserializer<R>(typeof(To), Schema<From>.RuntimeSchema);
                Assert.IsTrue(field.IsEqual(deserializer.Deserialize(to.field.Convert<To>())));
            }
        }

        void PolymorphicDeserializationAll<From, Through, To>()
            where From : class, Through, new() 
            where Through : class, new()
            where To : class, new()
        {
            PolymorphicDeserialization<From, Through, To, CompactBinaryReader<InputStream>>(
                Util.SerializeCB, Util.DeserializeCB<CdrcsClass<ICdrcsed<Through>>>);

            PolymorphicDeserialization<From, Through, To, CompactBinaryReader<InputStream>>(
                Util.SerializeCB2, Util.DeserializeCB2<CdrcsClass<ICdrcsed<Through>>>);

            PolymorphicDeserialization<From, Through, To, SimpleBinaryReader<InputStream>>(
                Util.SerializeSP, Util.DeserializeSP<CdrcsClass<ICdrcsed<Through>>, CdrcsClass<ICdrcsed<Through>>>);
        }

        // Serialize class with a From field, deserialize as class with ICdrcsed<Through> field
        // which then is serialized (potentially to another protocol) and finally deserialized
        // as class with To field.
        void Passthrough<From, Through, To>(
            Action<CdrcsClass<From, double>, Stream> serialize,
            Func<Stream, CdrcsClass<ICdrcsed<Through>, double>> deserializeThrough,
            Action<CdrcsClass<ICdrcsed<Through>, double>, Stream> serializeThrough,
            Func<Stream, CdrcsClass<To, double>> deserialize)
            where From : class, new()
            where Through : class
            where To : class
        {
            var from = Random.Init<CdrcsClass<From, double>>();

            var stream = new MemoryStream();
            
            serialize(from, stream);
            stream.Position = 0;
            var through = deserializeThrough(stream);

            using (var stream2 = new MemoryStream())
            {
                serializeThrough(through, stream2);
                stream2.Position = 0;

                var to = deserialize(stream2);

                Assert.IsTrue(from.extra.IsEqual(to.extra));
                Assert.AreEqual(from.field, to.field);
            }
        }


        void PassthroughAll<From, Through, To>()
            where From : class, new()
            where Through : class
            where To : class
        {
            Passthrough<From, Through, To>(
                Util.SerializeCB,
                Util.DeserializeCB<CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeCB,
                Util.DeserializeCB<CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeCB2,
                Util.DeserializeCB2<CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeCB2,
                Util.DeserializeCB2<CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeCB,
                Util.DeserializeCB<CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<ICdrcsed<Through>, double>, CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeCB2,
                Util.DeserializeCB2<CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<ICdrcsed<Through>, double>, CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<From, double>, CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeCB,
                Util.DeserializeCB<CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<From, double>, CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeCB2,
                Util.DeserializeCB2<CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<From, double>, CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<ICdrcsed<Through>, double>, CdrcsClass<To, double>>);

            Passthrough<From, Through, To>(
                Util.SerializeSP,
                Util.DeserializeSP<CdrcsClass<From, double>, CdrcsClass<ICdrcsed<Through>, double>>,
                Util.SerializeXmlWithNamespaces,
                Util.DeserializeXml<CdrcsClass<To, double>>);
        }

        [Test]
        public void CdrcsedInterface()
        {
            CdrcsedDeserialize<Nested, Derived>();
            CdrcsedDowncastDeserialize<Nested, Derived>();
            CdrcsedSerialize<Nested, Derived>();
        }

        [Test]
        public void LazyDeserialize()
        {
            LazyDeserializationAll<BasicTypes, BasicTypes>();
            LazyDeserializationAll<BasicTypes, BasicTypesView>();
            LazyDeserializationAll<Derived, Derived>();
            LazyDeserializationAll<Derived, DerivedView>();
        }

        [Test]
        public void NonLazyDeserialize()
        {
            NonLazyDeserializationAll<BasicTypes, BasicTypes>();
            NonLazyDeserializationAll<BasicTypes, BasicTypesView>();
            NonLazyDeserializationAll<Derived, Derived>();
            NonLazyDeserializationAll<Derived, DerivedView>();
        }

        [Test]
        public void PolymorphicDeserialize()
        {
            PolymorphicDeserializationAll<Derived, Nested, Derived>();
            PolymorphicDeserializationAll<Derived, Nested, DerivedView>();
            PolymorphicDeserializationAll<DerivedView, Nested, Derived>();
            PolymorphicDeserializationAll<Derived, EmptyBase, Derived>();
            PolymorphicDeserializationAll<Derived, EmptyBase, Nested>();
            PolymorphicDeserializationAll<DerivedView, EmptyBase, Nested>();
        }

        [Test]
        public void Passthrough()
        {
            PassthroughAll<BasicTypes, BasicTypes, BasicTypes>();
            PassthroughAll<BasicTypes, BasicTypesView, BasicTypes>();
            PassthroughAll<Derived, Nested, Derived>();
            PassthroughAll<Derived, EmptyBase, Derived>();
            PassthroughAll<Derived, EmptyBase, Nested>();
        }

        [Test]
        public void CdrcsedEquals()
        {
            var obj1 = new StructWithCdrcsed();
            var obj2 = new StructWithCdrcsed();
            Assert.IsTrue(Comparer.Equal(obj1, obj2));

            obj1.poly.Add(Cdrcsed<Derived>.Empty);
            Assert.IsFalse(Comparer.Equal(obj1, obj2));

            obj2.poly.Add(Cdrcsed<Derived>.Empty);
            Assert.IsTrue(Comparer.Equal(obj1, obj2));

            obj2.field = new Cdrcsed<Derived>(new Derived());
            Assert.IsFalse(Comparer.Equal(obj1, obj2));
        }

        [Test]
        public void SerializeCdrcsed()
        {
            var obj = new StructWithCdrcsed();

            var field = Random.Init<Derived>();
            obj.field = new Cdrcsed<Derived>(field);

            var poly0 = Random.Init<EmptyBase>();
            var poly1 = Random.Init<Nested>();
            var poly2 = Random.Init<Derived>();
            
            obj.poly.Add(new Cdrcsed<EmptyBase>(poly0));
            obj.poly.Add(new Cdrcsed<Nested>(poly1));
            obj.poly.Add(new Cdrcsed<Derived>(poly2));

            var stream = new MemoryStream();
            
            Util.SerializeCB(obj, stream);
            stream.Position = 0;
            obj = Util.DeserializeCB<StructWithCdrcsed>(stream);

            Assert.IsTrue(Comparer.Equal(field, obj.field.Deserialize<Derived>()));
            Assert.IsTrue(Comparer.Equal(poly0, obj.poly[0].Deserialize<EmptyBase>()));
            Assert.IsTrue(Comparer.Equal(poly1, obj.poly[1].Deserialize()));
            Assert.IsTrue(Comparer.Equal(poly2, obj.poly[2].Deserialize<Derived>()));

            stream.SetLength(0);
            Util.SerializeCB(obj, stream);
            stream.Position = 0;
            obj = Util.DeserializeCB<StructWithCdrcsed>(stream);

            Assert.IsTrue(Comparer.Equal(field, obj.field.Deserialize<Derived>()));
            Assert.IsTrue(Comparer.Equal(poly0, obj.poly[0].Deserialize<EmptyBase>()));
            Assert.IsTrue(Comparer.Equal(poly1, obj.poly[1].Deserialize()));
            Assert.IsTrue(Comparer.Equal(poly2, obj.poly[2].Deserialize<Derived>()));
        }

        [Test]
        public void SerializeCdrcsedCB2Multiple()
        {
            var obj = new StructWithCdrcsed();

            var stream = new MemoryStream();
            stream.SetLength(0);
            Util.SerializeCB2Multiple(obj, stream, 2);
        }
    }
}
