using System;
using System.Windows.Forms;
using JtecFactory.Navigation;

namespace JtecFactory.Forms
{
    public partial class BaseFunctionForm : Form
    {
        private readonly IPermissionService permissionService;
        private readonly int menuId;
        private readonly Action backAction;
        protected Panel ContentPanel { get { return contentPanel; } }
        protected FlowLayoutPanel ActionPanel { get { return actionPanel; } }
        protected IPermissionService PermissionService { get { return permissionService; } }
        protected int MenuId { get { return menuId; } }

        public BaseFunctionForm()
            : this("Function", "", 0, null, null)
        {
        }

        public BaseFunctionForm(string title, string description, int menuId, IPermissionService permissionService, Action backAction)
        {
            this.permissionService = permissionService;
            this.menuId = menuId;
            this.backAction = backAction;

            if (permissionService != null && !permissionService.CanView(menuId))
                throw new UnauthorizedAccessException("The current role cannot view this function.");

            InitializeComponent();
            titleLabel.Text = title;
            descriptionLabel.Text = description;
            backButton.Click += BackButtonClick;
        }

        protected void ApplyPermissions(Button addButton, Button editButton, Button deleteButton, Button exportButton)
        {
            if (permissionService == null)
                return;

            SetActionVisibility(addButton, permissionService.CanAdd(menuId));
            SetActionVisibility(editButton, permissionService.CanEdit(menuId));
            SetActionVisibility(deleteButton, permissionService.CanDelete(menuId));
            SetActionVisibility(exportButton, permissionService.CanExport(menuId));
        }

        private static void SetActionVisibility(Control control, bool allowed)
        {
            if (control != null)
                control.Visible = allowed;
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            if (backAction != null)
                backAction();
            else
                Close();
        }
    }
}