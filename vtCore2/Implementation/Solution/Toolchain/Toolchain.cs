using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtCore.Implementation;
using vtCore.Interfaces;

namespace vtCore.Implementation
{
    class Toolchain : IToolchain
    {
        public string name { get; set; }
        public ICompiler compiler { get; set; }
        public ICore core { get; set; }
        public List<IUploader> uploaders { get; } 
        public string boardsTxtPath { get; set; }
               
        
        private List<IUploader> _uploaders = new();

        public override string ToString() => name;
    }
}
