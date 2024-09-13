using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowUI
{
    public class RootControlViewModel
    {
        public RootControlViewModel(
            TestBrowserViewModel testBrowserViewModel, 
            CodeEditorViewModel codeEditorViewModel)
        {
            TestBrowserViewModel = testBrowserViewModel;
            CodeEditorViewModel = codeEditorViewModel;
        }

        public TestBrowserViewModel TestBrowserViewModel { get; }
        public CodeEditorViewModel CodeEditorViewModel { get; }
    }
}
