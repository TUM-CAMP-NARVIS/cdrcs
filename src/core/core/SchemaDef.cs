using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdrcs
{
    public partial class SchemaDef
    {
        public List<StructDef> structs { get; set; }
        public TypeDef root { get; set; }

        public SchemaDef() { 
            structs = new List<StructDef>();
            root = new TypeDef();
        }
    }
}
