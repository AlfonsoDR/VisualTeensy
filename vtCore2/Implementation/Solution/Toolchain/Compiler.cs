using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtCore.Interfaces;

namespace vtCore.Implementation
{
    class Compiler : ICompiler
    {
        public string description { get; set; }
        public string URL { get; set; }
        public string path { get; set; }

        public bool download()
        {
            throw new NotImplementedException();
        }

        public override string ToString() =>description;
    }
}
