namespace NoFences
{
    partial class FenceWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FenceWindow));
            this.appContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lockedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lockedLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.titleSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.sortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortByNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortByTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortByDateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.alignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableSnappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableEdgeAlignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableGridAlignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alignmentSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.addComponentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCalendarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTodoListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.newFenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appContextMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // appContextMenu
            //
            resources.ApplyResources(this.appContextMenu, "appContextMenu");
            this.appContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteItemToolStripMenuItem,
            this.lockedToolStripMenuItem,
            this.lockedLayoutToolStripMenuItem,
            this.minifyToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.titleSizeToolStripMenuItem,
            this.toolStripSeparator1,
            this.sortToolStripMenuItem,
            this.toolStripSeparator2,
            this.alignmentToolStripMenuItem,
            this.toolStripSeparator3,
            this.addComponentToolStripMenuItem,
            this.toolStripSeparator4,
            this.newFenceToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.appContextMenu.Name = "contextMenuStrip1";
            this.appContextMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.appContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            //
            // deleteItemToolStripMenuItem
            //
            resources.ApplyResources(this.deleteItemToolStripMenuItem, "deleteItemToolStripMenuItem");
            this.deleteItemToolStripMenuItem.Name = "deleteItemToolStripMenuItem";
            this.deleteItemToolStripMenuItem.Click += new System.EventHandler(this.deleteItemToolStripMenuItem_Click);
            //
            // lockedToolStripMenuItem
            //
            resources.ApplyResources(this.lockedToolStripMenuItem, "lockedToolStripMenuItem");
            this.lockedToolStripMenuItem.CheckOnClick = true;
            this.lockedToolStripMenuItem.Name = "lockedToolStripMenuItem";
            this.lockedToolStripMenuItem.Click += new System.EventHandler(this.lockedToolStripMenuItem_Click);
            //
            // lockedLayoutToolStripMenuItem
            //
            resources.ApplyResources(this.lockedLayoutToolStripMenuItem, "lockedLayoutToolStripMenuItem");
            this.lockedLayoutToolStripMenuItem.CheckOnClick = true;
            this.lockedLayoutToolStripMenuItem.Name = "lockedLayoutToolStripMenuItem";
            this.lockedLayoutToolStripMenuItem.Click += new System.EventHandler(this.lockedLayoutToolStripMenuItem_Click);
            //
            // minifyToolStripMenuItem
            //
            resources.ApplyResources(this.minifyToolStripMenuItem, "minifyToolStripMenuItem");
            this.minifyToolStripMenuItem.CheckOnClick = true;
            this.minifyToolStripMenuItem.Name = "minifyToolStripMenuItem";
            this.minifyToolStripMenuItem.Click += new System.EventHandler(this.minifyToolStripMenuItem_Click);
            //
            // renameToolStripMenuItem
            //
            resources.ApplyResources(this.renameToolStripMenuItem, "renameToolStripMenuItem");
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            //
            // titleSizeToolStripMenuItem
            //
            resources.ApplyResources(this.titleSizeToolStripMenuItem, "titleSizeToolStripMenuItem");
            this.titleSizeToolStripMenuItem.Name = "titleSizeToolStripMenuItem";
            this.titleSizeToolStripMenuItem.Click += new System.EventHandler(this.titleSizeToolStripMenuItem_Click);
            //
            // toolStripSeparator1
            //
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // sortToolStripMenuItem
            //
            resources.ApplyResources(this.sortToolStripMenuItem, "sortToolStripMenuItem");
            this.sortToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortByNameToolStripMenuItem,
            this.sortByTypeToolStripMenuItem,
            this.sortByDateToolStripMenuItem,
            this.sortClearToolStripMenuItem});
            this.sortToolStripMenuItem.Name = "sortToolStripMenuItem";
            //
            // sortByNameToolStripMenuItem
            //
            resources.ApplyResources(this.sortByNameToolStripMenuItem, "sortByNameToolStripMenuItem");
            this.sortByNameToolStripMenuItem.Name = "sortByNameToolStripMenuItem";
            this.sortByNameToolStripMenuItem.Click += new System.EventHandler(this.sortByNameToolStripMenuItem_Click);
            //
            // sortByTypeToolStripMenuItem
            //
            resources.ApplyResources(this.sortByTypeToolStripMenuItem, "sortByTypeToolStripMenuItem");
            this.sortByTypeToolStripMenuItem.Name = "sortByTypeToolStripMenuItem";
            this.sortByTypeToolStripMenuItem.Click += new System.EventHandler(this.sortByTypeToolStripMenuItem_Click);
            //
            // sortByDateToolStripMenuItem
            //
            resources.ApplyResources(this.sortByDateToolStripMenuItem, "sortByDateToolStripMenuItem");
            this.sortByDateToolStripMenuItem.Name = "sortByDateToolStripMenuItem";
            this.sortByDateToolStripMenuItem.Click += new System.EventHandler(this.sortByDateToolStripMenuItem_Click);
            //
            // sortClearToolStripMenuItem
            //
            resources.ApplyResources(this.sortClearToolStripMenuItem, "sortClearToolStripMenuItem");
            this.sortClearToolStripMenuItem.Name = "sortClearToolStripMenuItem";
            this.sortClearToolStripMenuItem.Click += new System.EventHandler(this.sortClearToolStripMenuItem_Click);
            //
            // toolStripSeparator2
            //
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // alignmentToolStripMenuItem
            //
            resources.ApplyResources(this.alignmentToolStripMenuItem, "alignmentToolStripMenuItem");
            this.alignmentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableSnappingToolStripMenuItem,
            this.enableEdgeAlignmentToolStripMenuItem,
            this.enableGridAlignmentToolStripMenuItem,
            this.alignmentSettingsToolStripMenuItem});
            this.alignmentToolStripMenuItem.Name = "alignmentToolStripMenuItem";
            //
            // enableSnappingToolStripMenuItem
            //
            resources.ApplyResources(this.enableSnappingToolStripMenuItem, "enableSnappingToolStripMenuItem");
            this.enableSnappingToolStripMenuItem.CheckOnClick = true;
            this.enableSnappingToolStripMenuItem.Name = "enableSnappingToolStripMenuItem";
            this.enableSnappingToolStripMenuItem.Click += new System.EventHandler(this.enableSnappingToolStripMenuItem_Click);
            //
            // enableEdgeAlignmentToolStripMenuItem
            //
            resources.ApplyResources(this.enableEdgeAlignmentToolStripMenuItem, "enableEdgeAlignmentToolStripMenuItem");
            this.enableEdgeAlignmentToolStripMenuItem.CheckOnClick = true;
            this.enableEdgeAlignmentToolStripMenuItem.Name = "enableEdgeAlignmentToolStripMenuItem";
            this.enableEdgeAlignmentToolStripMenuItem.Click += new System.EventHandler(this.enableEdgeAlignmentToolStripMenuItem_Click);
            //
            // enableGridAlignmentToolStripMenuItem
            //
            resources.ApplyResources(this.enableGridAlignmentToolStripMenuItem, "enableGridAlignmentToolStripMenuItem");
            this.enableGridAlignmentToolStripMenuItem.CheckOnClick = true;
            this.enableGridAlignmentToolStripMenuItem.Name = "enableGridAlignmentToolStripMenuItem";
            this.enableGridAlignmentToolStripMenuItem.Click += new System.EventHandler(this.enableGridAlignmentToolStripMenuItem_Click);
            //
            // alignmentSettingsToolStripMenuItem
            //
            resources.ApplyResources(this.alignmentSettingsToolStripMenuItem, "alignmentSettingsToolStripMenuItem");
            this.alignmentSettingsToolStripMenuItem.Name = "alignmentSettingsToolStripMenuItem";
            this.alignmentSettingsToolStripMenuItem.Click += new System.EventHandler(this.alignmentSettingsToolStripMenuItem_Click);
            //
            // toolStripSeparator3
            //
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // addComponentToolStripMenuItem
            //
            resources.ApplyResources(this.addComponentToolStripMenuItem, "addComponentToolStripMenuItem");
            this.addComponentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCalendarToolStripMenuItem,
            this.addTodoListToolStripMenuItem});
            this.addComponentToolStripMenuItem.Name = "addComponentToolStripMenuItem";
            //
            // addCalendarToolStripMenuItem
            //
            resources.ApplyResources(this.addCalendarToolStripMenuItem, "addCalendarToolStripMenuItem");
            this.addCalendarToolStripMenuItem.Name = "addCalendarToolStripMenuItem";
            this.addCalendarToolStripMenuItem.Click += new System.EventHandler(this.addCalendarToolStripMenuItem_Click);
            //
            // addTodoListToolStripMenuItem
            //
            resources.ApplyResources(this.addTodoListToolStripMenuItem, "addTodoListToolStripMenuItem");
            this.addTodoListToolStripMenuItem.Name = "addTodoListToolStripMenuItem";
            this.addTodoListToolStripMenuItem.Click += new System.EventHandler(this.addTodoListToolStripMenuItem_Click);
            //
            // toolStripSeparator4
            //
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            //
            // newFenceToolStripMenuItem
            //
            resources.ApplyResources(this.newFenceToolStripMenuItem, "newFenceToolStripMenuItem");
            this.newFenceToolStripMenuItem.Name = "newFenceToolStripMenuItem";
            this.newFenceToolStripMenuItem.Click += new System.EventHandler(this.newFenceToolStripMenuItem_Click);
            //
            // exitToolStripMenuItem
            //
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            //
            // FenceWindow
            //
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimizeBox = false;
            this.Name = "FenceWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FenceWindow_FormClosed);
            this.Load += new System.EventHandler(this.FenceWindow_Load);
            this.LocationChanged += new System.EventHandler(this.FenceWindow_LocationChanged);
            this.Click += new System.EventHandler(this.FenceWindow_Click);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FenceWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FenceWindow_DragEnter);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FenceWindow_Paint);
            this.DoubleClick += new System.EventHandler(this.FenceWindow_DoubleClick);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FenceWindow_MouseClick);
            this.MouseEnter += new System.EventHandler(this.FenceWindow_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.FenceWindow_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FenceWindow_MouseMove);
            this.Resize += new System.EventHandler(this.FenceWindow_Resize);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FenceWindow_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FenceWindow_MouseUp);
            this.appContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip appContextMenu;
        private System.Windows.Forms.ToolStripMenuItem lockedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lockedLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem sortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortByNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortByTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortByDateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortClearToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem alignmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableSnappingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableEdgeAlignmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableGridAlignmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alignmentSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem addComponentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCalendarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTodoListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minifyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteItemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newFenceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem titleSizeToolStripMenuItem;
    }
}

