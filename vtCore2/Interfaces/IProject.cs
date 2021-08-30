using System.Collections.Generic;

namespace vtCore.Interfaces
{
    public interface ICompiler
    {
        string description { get; }
        string URL { get; }
        string path { get; }

        bool download();
    }

    public interface IUploader
    {
        string description { get; }
        string URL { get; }
        string path { get; }
    }

    public interface IGitRepository
    {
        string name { get; set; }
        string URL { get; set; }
        string targetFolder { get; set; }
        string clone();
        string pull();
        List<string> tags { get; }
        string tag { get; set; }
        bool checkout();
    }

    public interface ICore
    {
        string description { get; }
        IGitRepository repository { get; }
    }


    public interface IToolchain
    {
        string name { get; }
        ICompiler compiler { get; }
        ICore core { get; }
            
        public List<IUploader> uploaders { get; }

        public string boardsTxtPath { get; }

       
    }

    


    public enum TargetIDE
    {
        vsCode,
        sublimeText,
        atom,
        vsFolder
    }
    public enum BuildSystem
    {
        makefile,
        builder
    }
   
    public interface IProject 
    {
        string projectName { get; }
        string projectFolder { get; set; }        
        string sourceFolder { get; }
        string libFolder { get; }       
        string extrasFolder { get; }

        IToolchain toolchain { get; }
    }


    public interface ISolution 
    {
        string name { get; set; }
        TargetIDE targetIDE { get; set; }
        BuildSystem buildSystem { get; set; }
        IToolchain toolchain { get; set; }
        List<IProject> projects { get; set; }

        void addProject(IProject p);
        void removeProject(IProject p);
    }
}
