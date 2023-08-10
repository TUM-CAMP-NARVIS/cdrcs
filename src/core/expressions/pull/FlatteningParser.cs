// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cdrcs.Expressions.Pull
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal sealed class FlatteningParser : IParser
    {
        RuntimeSchema schema;
        readonly List<TransformSchemaPair> pairs = new List<TransformSchemaPair>();
        
        public FlatteningParser(RuntimeSchema rootSchema)
        {
            schema = rootSchema;
        }

        public IEnumerable<TransformSchemaPair> Transforms
        {
            get { return pairs; }
        }

        public Expression Apply(ITransform transform)
        {
            pairs.Add(new TransformSchemaPair(transform, schema));

            if (schema.HasBase)
            {
                schema = schema.GetBaseSchema();
                transform.Base(this);
            }

            return Expression.Empty();
        }

        public Expression Container(CdrcsDataType? expectedType, ContainerHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public Expression Map(CdrcsDataType? expectedKeyType, CdrcsDataType? expectedValueType, MapHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public Expression Blob(Expression count)
        {
            throw new System.NotImplementedException();
        }

        public Expression Scalar(Expression valueType, CdrcsDataType expectedType, ValueHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public Expression Cdrcsed(ValueHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public Expression Skip(Expression valueType)
        {
            throw new System.NotImplementedException();
        }

        public ParameterExpression ReaderParam
        {
            get { throw new System.NotImplementedException(); }
        }

        public Expression ReaderValue
        {
            get { throw new System.NotImplementedException(); }
        }

        public int HierarchyDepth
        {
            get { return 0; }
        }

        public bool IsCdrcsed
        {
            get { throw new System.NotImplementedException(); }
        }
    }

    public sealed class TransformSchemaPair
    {
        public TransformSchemaPair(ITransform transform, RuntimeSchema schema)
        {
            Transform = transform;
            Schema = schema;
        }

        public ITransform Transform { get; private set; }
        public RuntimeSchema Schema { get; private set; }
    }
}
