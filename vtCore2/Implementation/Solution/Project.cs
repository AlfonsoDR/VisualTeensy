using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtCore.Interfaces;

namespace vtCore.Implementation
{
    class Project : IProject
    {
        public string projectName { get; set; }
        public string projectFolder { get;  set; }
        public string sourceFolder { get; set; }
        public string libFolder { get; set; }
        public string extrasFolder { get; set; }
        public IToolchain toolchain { get; set; }

        public override string ToString() => projectName;
    }
}
