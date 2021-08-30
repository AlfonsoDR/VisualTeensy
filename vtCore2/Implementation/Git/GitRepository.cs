using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using vtCore.Interfaces;

namespace vtCore2.Implementation
{
    class GitRepository : IGitRepository
    {
        public GitRepository()
        {
            name = "noname";
        }

        public string name { get; set; }
        public string URL { get; set; }
        public string targetFolder { get; set; }
        public string tag { get; set; }

        [JsonIgnore]
        public List<string> tags { get; set; }

        public bool checkout()
        {
            throw new NotImplementedException();
        }

        public string clone()
        {
            throw new NotImplementedException();
        }

        public string pull()
        {
            throw new NotImplementedException();
        }
    }
}
