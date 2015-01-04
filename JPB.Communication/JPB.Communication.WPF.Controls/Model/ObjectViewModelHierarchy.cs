using System.Collections.ObjectModel;

namespace JPB.Communication.WPF.Controls.Model
{
    public class ObjectViewModelHierarchy
    {
        private readonly ReadOnlyCollection<ObjectViewModel> _firstGeneration;
        private readonly ObjectViewModel _rootObject;

        public ObjectViewModelHierarchy(object rootObject)
        {
            _rootObject = new ObjectViewModel(rootObject);
            _firstGeneration = new ReadOnlyCollection<ObjectViewModel>(new[] {_rootObject});
        }

        public ReadOnlyCollection<ObjectViewModel> FirstGeneration
        {
            get { return _firstGeneration; }
        }
    }
}