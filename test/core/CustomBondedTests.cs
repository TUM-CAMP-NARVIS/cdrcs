// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace UnitTest
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using Cdrcs;
    using Cdrcs.Expressions;
    using Cdrcs.IO;
    using Cdrcs.IO.Unsafe;
    using Cdrcs.Protocols;
    using Cdrcs.Internal.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class CustomCdrcsedTests
    {
        #region Schemas

        [Schema]
        public class X
        {
            public X()
                : this("CustomCdrcsedTests.X", "X")
            {
            }

            protected X(string fullName, string name)
            {
                bonded_Y = CustomCdrcsed<Y>.Empty;
            }

            [Cdrcs.Id(0), Cdrcs.Type(typeof (Cdrcs.Tag.bonded<Y>))]
            public CustomCdrcsed<Y> bonded_Y { get; set; }
        }

        [Schema]
        public class Y
        {
            public Y()
                : this("CustomCdrcsedTests.Y", "Y")
            {
            }

            protected Y(string fullName, string name)
            {
                FullName = fullName;
            }

            [Cdrcs.Id(0), Cdrcs.Type(typeof (string))]
            public string FullName { get; set; }
        }

        [Schema]
        public class YDerived : Y
        {
            public YDerived()
                : this("CustomCdrcsedTests.YDerived", "YDerived")
            {
            }

            protected YDerived(string fullName, string name)
                : base(fullName, name)
            {
                Z = CustomCdrcsed<Z>.From(Cdrcs.GenericFactory.Create<Z>());
            }

            [Cdrcs.Id(1), Cdrcs.Type(typeof (Cdrcs.Tag.bonded<Z>))]
            public CustomCdrcsed<Z> Z { get; set; }
        }

        [Schema]
        public class Z
        {
            public Z()
                : this("CustomCdrcsedTests.Z", "Z")
            {
            }

            protected Z(string fullName, string name)
            {
                FullName = fullName;
            }

            [Cdrcs.Id(0)]
            public string FullName { get; set; }

            [Cdrcs.Id(1)]
            public int Value { get; set; }
        }

        #endregion

        public abstract class CustomCdrcsed<T>
        {
            public abstract T Value { get; }

            public abstract CustomCdrcsed<U> Convert<U>();

            public static CustomCdrcsed<T> Empty
            {
                get { return CustomCdrcsedPoly<T, T>.Empty; }
            }

            public static CustomCdrcsed<T> From<TActual>(TActual instance)
            {
                return new CustomCdrcsedPoly<T,TActual>(instance);
            }
        }

        internal class CustomCdrcsedPoly<T, TActual> : CustomCdrcsed<T>, ICdrcsed<T>
        {
            public new static readonly CustomCdrcsed<T> Empty = new CustomCdrcsedPoly<T, TActual>(GenericFactory.Create<TActual>());

            private readonly TActual instance;

            public CustomCdrcsedPoly(TActual instance)
            {
                this.instance = instance;
            }

            public override T Value
            {
                get { return Deserialize(); }
            }

            public T Deserialize()
            {
                return CustomTransformFactory.Instance.Cloner<TActual, T>().Clone<T>(instance);
            }

            public void Serialize<W>(W writer)
            {
                CustomTransformFactory.Instance.Serializer<W, TActual>().Serialize(instance, writer);
            }

            public U Deserialize<U>()
            {
                return CustomTransformFactory.Instance.Cloner<TActual, U>().Clone<U>(instance);
            }

            ICdrcsed<U> ICdrcsed.Convert<U>()
            {
                return this as ICdrcsed<U>;
            }

            public override CustomCdrcsed<U> Convert<U>()
            {
                return new CustomCdrcsedPoly<U, TActual>(instance);
            }
        }

        internal class CustomCdrcsed<T, R> : CustomCdrcsed<T>, ICdrcsed<T>
            where R : ICloneable<R>
        {
            private readonly R reader;
            private readonly RuntimeSchema schema;
            private readonly Lazy<T> value;

            public CustomCdrcsed(R reader)
            {
                this.reader = reader.Clone();
                this.schema = RuntimeSchema.Empty;

                this.value = new Lazy<T>(this.Deserialize<T>, LazyThreadSafetyMode.ExecutionAndPublication);
            }

            public CustomCdrcsed(R reader, RuntimeSchema schema)
            {
                this.reader = reader.Clone();
                this.schema = schema;

                this.value = new Lazy<T>(this.Deserialize<T>, LazyThreadSafetyMode.ExecutionAndPublication);
            }

            public override T Value
            {
                get { return value.Value; }
            }

            public T Deserialize()
            {
                return Deserialize<T>();
            }

            public void Serialize<W>(W writer)
            {
                CustomTransformFactory.Instance.Transcoder<R, W>(schema).Transcode(reader.Clone(), writer);
            }

            public U Deserialize<U>()
            {
                return CustomTransformFactory.Instance.Deserializer<R, U>(schema).Deserialize<U>(reader.Clone());
            }

            ICdrcsed<U> ICdrcsed.Convert<U>()
            {
                return (ICdrcsed<U>) Convert<U>();
            }

            public override CustomCdrcsed<U> Convert<U>()
            {
                return new CustomCdrcsed<U, R>(reader, schema);
            }
        }

        internal class CustomCdrcsedVoid<R> : ICdrcsed
            where R : ICloneable<R>
        {
            private readonly R reader;
            private readonly RuntimeSchema schema;

            public CustomCdrcsedVoid(R reader, RuntimeSchema schema)
            {
                this.reader = reader.Clone();
                this.schema = schema;
            }

            public void Serialize<W>(W writer)
            {
                CustomTransformFactory.Instance.Transcoder<R, W>(schema).Transcode(reader.Clone(), writer);
            }

            public U Deserialize<U>()
            {
                return CustomTransformFactory.Instance.Deserializer<R, U>(schema).Deserialize<U>(reader.Clone());
            }

            ICdrcsed<U> ICdrcsed.Convert<U>()
            {
                return new CustomCdrcsed<U, R>(reader, schema);
            }
        }

        /// <summary>
        ///     Custom ITransformFactory for making Deserializer work.
        /// </summary>
        private class CustomTransformFactory
        {
            public static readonly CustomTransformFactory Instance = new CustomTransformFactory();

            private CustomTransformFactory() { }

            public Cloner<TSource> Cloner<TSource, T>()
            {
                return new Cloner<TSource>(typeof(T), new ObjectParser(typeof(TSource), ObjectCdrcsedFactory), Factory);
            }

            public Serializer<W> Serializer<W, T>()
            {
                return new Serializer<W>(typeof(T), new ObjectParser(typeof(T), ObjectCdrcsedFactory), false);
            }

            public Deserializer<R> Deserializer<R, T>(RuntimeSchema schema)
            {
                var parser = schema.HasValue
                                 ? ParserFactory<R>.Create(schema, PayloadCdrcsedFactory)
                                 : ParserFactory<R>.Create(typeof(T), PayloadCdrcsedFactory);

                return new Deserializer<R>(typeof(T), parser, Factory, false);
            }

            private static Expression ObjectCdrcsedFactory(Type objectType, Expression value)
            {
                var method = objectType.GetMethod(typeof(CustomCdrcsed<>), "From", value.Type);

                return Expression.Call(method, value);
            }

            private static Expression PayloadCdrcsedFactory(Expression reader, Expression schema)
            {
                var ctor = typeof(CustomCdrcsedVoid<>).MakeGenericType(reader.Type).GetConstructor(reader.Type, schema.Type);
                return Expression.New(ctor, reader, schema);
            }

            public Transcoder<R, W> Transcoder<R, W>(RuntimeSchema schema)
            {
                return new Transcoder<R, W>(schema, ParserFactory<R>.Create(schema, PayloadCdrcsedFactory));
            }

            private static Expression Factory(Type type, Type schemaType, params Expression[] arguments)
            {
                if (type.IsGenericType())
                {
                    var typeDefinition = type.GetGenericTypeDefinition();
                    if (typeDefinition == typeof(CustomCdrcsed<>))
                    {
                        var arg = arguments[0]; // CustomCdrcsedVoid<R>

                        Type[] genericArgs = type.GetTypeInfo().GenericTypeArguments;
                        Assert.IsNotEmpty(genericArgs);

                        var bondedConvert = typeof(ICdrcsed).GetMethod("Convert").MakeGenericMethod(genericArgs);

                        return Expression.ConvertChecked(Expression.Call(arg, bondedConvert), type);
                    }
                }

                return null;
            }
        }

        [Test]
        public void SupportSettingCustomFactory_RoundTrip()
        {
            var y = new YDerived();
            y.Z = CustomCdrcsed<Z>.From(new Z {Value = 42});
            var x = new X();
            x.bonded_Y = CustomCdrcsed<Y>.From(y);

            var buffer = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(buffer);
            CustomTransformFactory.Instance.Serializer<CompactBinaryWriter<OutputBuffer>, X>().Serialize(x, writer);

            var inputStream = new InputBuffer(buffer.Data);
            var reader = new CompactBinaryReader<InputBuffer>(inputStream);
            var x1 = CustomTransformFactory.Instance.Deserializer<CompactBinaryReader<InputBuffer>, X>(RuntimeSchema.Empty).Deserialize<X>(reader);

            Assert.That(x1, Is.Not.Null);
            Assert.That(x1.bonded_Y, Is.InstanceOf<CustomCdrcsed<Y, CompactBinaryReader<InputBuffer>>>());
            Assert.That(x1.bonded_Y.Value.FullName, Is.EqualTo("CustomCdrcsedTests.YDerived"));
            Assert.That(x1.bonded_Y.Convert<YDerived>().Value.Z.Value.Value, Is.EqualTo(42));

        }

        [Test]
        public void SupportSettingCustomFactory_Clone()
        {
            var y = new YDerived();
            y.Z = CustomCdrcsed<Z>.From(new Z { Value = 42 });
            var x = new X();
            x.bonded_Y = CustomCdrcsed<Y>.From(y);

            var x1 = CustomTransformFactory.Instance.Cloner<X, X>().Clone<X>(x);

            Assert.That(x1, Is.Not.Null);
            Assert.That(x1.bonded_Y.Value.FullName, Is.EqualTo("CustomCdrcsedTests.YDerived"));
            Assert.That(x1.bonded_Y.Convert<YDerived>().Value.Z.Value.Value, Is.EqualTo(42));
        }
    }
}
