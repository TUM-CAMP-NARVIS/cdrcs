using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdrcs
{
    public class TypeDef
    {
        public CdrcsDataType id = CdrcsDataType.BT_STRUCT;
        public UInt16 struct_def = 0;
        public TypeDef element;
        public TypeDef key;
        public bool bonded_type;
    }
}
