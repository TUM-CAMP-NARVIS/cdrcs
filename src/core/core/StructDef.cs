using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdrcs
{
    public partial class StructDef
    {
        public Metadata metadata;
        public TypeDef base_def;
        public List<FieldDef> fields;

        public StructDef()
        {
            metadata = new Metadata();
            fields = new List<FieldDef>();
            base_def = new TypeDef();
        }
    }
}
