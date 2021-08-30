using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vtCore;
using vtCore.Interfaces;

namespace ViewModel
{
    public class ExampleVM
    {
        public String name { get; set; }

        public ExampleVM()
        {

        }
    }


    public class LibExamplesTabVM : BaseViewModel
    {

        public RelayCommand cmdAddExample { get;  }
        private void doAddExample(object o)
        {

        }


        public ObservableCollection<ExampleVM> examples { get; }


        public LibExamplesTabVM(IProject project, SetupData setup)
        {
            cmdAddExample = new RelayCommand(doAddExample);

            examples = new ObservableCollection<ExampleVM>();

            examples.Add(new ExampleVM { name = "first" });
            examples.Add(new ExampleVM { name = "second" });
        }


    }
}