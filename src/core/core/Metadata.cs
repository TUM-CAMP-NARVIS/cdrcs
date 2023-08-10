using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdrcs
{
    public partial class Metadata
    {
        public string name;
        public string qualified_name;
        public Dictionary<string, string> attributes;
        public Modifier modifier = Modifier.Optional;
        public Variant default_value;
    }
}
