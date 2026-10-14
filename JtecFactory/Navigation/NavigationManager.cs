using System.Windows.Forms;

namespace JtecFactory.Navigation
{
    public sealed class NavigationManager
    {
        public MenuItem CurrentParentMenu { get; private set; }
        public MenuItem CurrentChildMenu { get; private set; }
        public Form CurrentFunctionForm { get; private set; }

        public void SelectParent(MenuItem parentMenu)
        {
            CurrentParentMenu = parentMenu;
            CurrentChildMenu = null;
            CurrentFunctionForm = null;
        }

        public void SelectChild(MenuItem childMenu)
        {
            CurrentChildMenu = childMenu;
            CurrentFunctionForm = null;
        }

        public void SetCurrentFunctionForm(Form form)
        {
            CurrentFunctionForm = form;
        }

        public void ReturnToChildMenu()
        {
            CurrentFunctionForm = null;
        }
    }
}