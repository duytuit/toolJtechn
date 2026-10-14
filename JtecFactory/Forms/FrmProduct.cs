using System;
using System.Windows.Forms;
using JtecFactory.Navigation;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    public sealed partial class FrmProduct : Form
    {
        private readonly IPermissionService permissionService;
        private readonly int menuId;
        private readonly Action backAction;

        public FrmProduct()
        {
            InitializeComponent();
            backButton.Click += BackButtonClick;
        }

        internal FrmProduct(MenuItem menu, IPermissionService permissionService, Action backAction)
        {
            if (menu == null)
                throw new ArgumentNullException("menu");
            if (permissionService != null && !permissionService.CanView(menu.Id))
                throw new UnauthorizedAccessException("The current role cannot view this function.");

            this.permissionService = permissionService;
            this.menuId = menu.Id;
            this.backAction = backAction;

            InitializeComponent();
            titleLabel.Text = menu.Name;
            subtitleLabel.Text = menu.Description;
            backButton.Click += BackButtonClick;
            ApplyPermissions();
        }

        private void ApplyPermissions()
        {
            if (permissionService != null)
                addProductButton.Visible = permissionService.CanAdd(menuId);
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