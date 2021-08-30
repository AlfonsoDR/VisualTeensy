using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using vtCore.Interfaces;

namespace vtCore.Implementation
{
    class Solution : ISolution
    {
        public string name { get; set; }
        public string path { get; set; }
        public TargetIDE targetIDE { get; set; }
        public BuildSystem buildSystem { get; set; }
        public List<IProject> projects { get; set; }
        public IToolchain toolchain { get; set; }

        public void addProject(IProject p)
        {
            throw new NotImplementedException();
        }

        public void removeProject(IProject p)
        {
            throw new NotImplementedException();
        }
    }
}
