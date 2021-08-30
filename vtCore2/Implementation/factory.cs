using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtCore.Interfaces;
using vtCore2.Implementation;

namespace vtCore.Implementation
{
    public static class Factory
    {
        public static ISolution makeSolution()
        {
            return new Solution
            {
                name = "Test Solution",
                targetIDE = TargetIDE.vsCode,
                buildSystem = BuildSystem.makefile,
                toolchain = new Toolchain
                {
                    boardsTxtPath = "boardstxt.path",
                    compiler = new Compiler
                    {
                        description = "GCC",
                        path = "gcc path"
                    },
                    core = new Core
                    {
                        description = "Teensyduino 1.54",
                        repository = new GitRepository
                        {
                            name="Teensyduino",
                            URL="asdfasf",
                            tag = "v1.54",
                            tags = new List<string>
                            {
                                "1", "12"
                            }
                        }
                    }
                     
                },
                projects = new List<IProject>
                {
                    new Project
                    {
                        projectName = "First Project",
                        extrasFolder = "extras",
                        libFolder = "lib",
                        toolchain = new Toolchain
                        {
                            compiler = new Compiler
                            {
                                description = "GCC 10.2",
                                path = "gcc path 10.2"
                            }
                        }
                    }
                }
            };
        }



        public static IProject makeProject(string name)
        {
            return new Project();
        }

    }
}
