namespace UnitTest
{
    using System.Collections.Generic;
    using System.Reflection;
    using Cdrcs;

    [Schema, Attribute("xmlns", "urn:UnitTest.CdrcsClass")]
    class CdrcsClass<T>
    {
        [Cdrcs.Id(2), Required]
        public T field = GenericFactory.Create<T>();

        public static CdrcsDataType TypeId 
        { 
            get { return Schema<CdrcsClass<T>>.RuntimeSchema.StructDef.fields[0].type.id; }
        }

        public override bool Equals(object that)
        {
            var thatField = that.GetType().GetTypeInfo().GetDeclaredField("field");
            return field.IsEqual<object, object>(thatField.GetValue(that));
        }
            
        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(field);
        }
    }

    [Schema, Attribute("xmlns", "urn:UnitTest.CdrcsClass")]
    internal class CdrcsClass<T1, T2>
    {
        [Cdrcs.Id(1), Required]
        public T1 extra = GenericFactory.Create<T1>();

        [Cdrcs.Id(2), Required]
        public T2 field = GenericFactory.Create<T2>();

        public override bool Equals(object that)
        {
            if (that is CdrcsClass<T2>)
            {
                var thatField = that.GetType().GetTypeInfo().GetDeclaredField("field");
                return field.IsEqual<object, object>(thatField.GetValue(that));
            }

            return false;
        }
        public override int GetHashCode()
        {
            return EqualityComparer<T2>.Default.GetHashCode(field);
        }
    }
}
