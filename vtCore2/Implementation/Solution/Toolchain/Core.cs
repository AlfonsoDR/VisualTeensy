using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtCore.Interfaces;

namespace vtCore.Implementation
{
    class Core : ICore
    {
        public string description { get; set; }
        public IGitRepository repository { get; set; }
    }
    
}
