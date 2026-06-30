using System;
using System.Drawing;
using System.Windows.Forms;
using NoFences.Model;

namespace NoFences
{
    public partial class TodoDialog : Form
    {
        public string NewTitle { get; private set; }
        public string NewDescription { get; private set; }
        public DateTime NewDueDate { get; private set; }
        public int NewPriority { get; private set; }

        public TodoDialog(string title = "", string description = "", DateTime? dueDate = null, int priority = 1)
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtDescription.Text = description;
            datePicker.Value = dueDate ?? DateTime.Today;
            cmbPriority.SelectedIndex = Math.Clamp(priority, 0, 3);

            this.Text = string.IsNullOrEmpty(title) ? "Add Todo" : "Edit Todo";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a title for the todo item.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NewTitle = txtTitle.Text;
            NewDescription = txtDescription.Text;
            NewDueDate = datePicker.Value.Date;
            NewPriority = cmbPriority.SelectedIndex;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void TodoDialog_Load(object sender, EventArgs e)
        {
        }
    }
}
