using System.Drawing;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace QLTKNH
{
    partial class FrmRiskCenter
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel root;
        private TableLayoutPanel tiles;
        private FlowLayoutPanel filters;
        private Label header;
        private Label subtitle;
        private Panel heading;
        private Label lblTotal;
        private Label lblHigh;
        private Label lblMedium;
        private Label lblWatch;
        private ComboBox scope;
        private ComboBox level;
        private TextBox search;
        private Button refresh;
        private Button btnIntegrity;
        private Label searchLabel;
        private DataGridView grid;
        private Label status;
        private DataGridViewTextBoxColumn c0;
        private DataGridViewTextBoxColumn c1;
        private DataGridViewTextBoxColumn c2;
        private DataGridViewTextBoxColumn c3;
        private DataGridViewTextBoxColumn c4;
        private DataGridViewTextBoxColumn c5;
        private DataGridViewTextBoxColumn c6;
        private DataGridViewTextBoxColumn c7;
        private DataGridViewTextBoxColumn c8;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.root = new System.Windows.Forms.TableLayoutPanel();
            this.heading = new System.Windows.Forms.Panel();
            this.header = new System.Windows.Forms.Label();
            this.subtitle = new System.Windows.Forms.Label();
            this.tiles = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblHigh = new System.Windows.Forms.Label();
            this.lblMedium = new System.Windows.Forms.Label();
            this.lblWatch = new System.Windows.Forms.Label();
            this.filters = new System.Windows.Forms.FlowLayoutPanel();
            this.scope = new System.Windows.Forms.ComboBox();
            this.level = new System.Windows.Forms.ComboBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.search = new System.Windows.Forms.TextBox();
            this.refresh = new System.Windows.Forms.Button();
            this.btnIntegrity = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.c0 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.c8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.Label();
            this.root.SuspendLayout();
            this.heading.SuspendLayout();
            this.tiles.SuspendLayout();
            this.filters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // root
            // 
            this.root.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.root.ColumnCount = 1;
            this.root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.Controls.Add(this.heading, 0, 0);
            this.root.Controls.Add(this.tiles, 0, 1);
            this.root.Controls.Add(this.filters, 0, 2);
            this.root.Controls.Add(this.btnIntegrity, 0, 3);
            this.root.Controls.Add(this.grid, 0, 5);
            this.root.Controls.Add(this.status, 0, 6);
            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.Location = new System.Drawing.Point(0, 0);
            this.root.Name = "root";
            this.root.Padding = new System.Windows.Forms.Padding(16);
            this.root.RowCount = 7;
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 95F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 84F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.root.Size = new System.Drawing.Size(1200, 700);
            this.root.TabIndex = 0;
            // 
            // heading
            // 
            this.heading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.heading.Controls.Add(this.header);
            this.heading.Controls.Add(this.subtitle);
            this.heading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heading.Location = new System.Drawing.Point(19, 19);
            this.heading.Name = "heading";
            this.heading.Size = new System.Drawing.Size(1162, 59);
            this.heading.TabIndex = 0;
            // 
            // header
            // 
            this.header.AutoSize = true;
            this.header.Font = new System.Drawing.Font("Segoe UI", 19F, System.Drawing.FontStyle.Bold);
            this.header.ForeColor = System.Drawing.Color.White;
            this.header.Location = new System.Drawing.Point(0, 0);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(376, 45);
            this.header.TabIndex = 0;
            this.header.Text = "RISK CONTROL CENTER";
            // 
            // subtitle
            // 
            this.subtitle.AutoSize = true;
            this.subtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.subtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.subtitle.Location = new System.Drawing.Point(0, 35);
            this.subtitle.Name = "subtitle";
            this.subtitle.Size = new System.Drawing.Size(398, 23);
            this.subtitle.TabIndex = 1;
            this.subtitle.Text = "Giám sát tín dụng, giao dịch, tài sản bảo đảm và dữ liệu • Chỉ đọc";
            // 
            // tiles
            // 
            this.tiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.tiles.ColumnCount = 4;
            this.tiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tiles.Controls.Add(this.lblTotal, 0, 0);
            this.tiles.Controls.Add(this.lblHigh, 1, 0);
            this.tiles.Controls.Add(this.lblMedium, 2, 0);
            this.tiles.Controls.Add(this.lblWatch, 3, 0);
            this.tiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tiles.Location = new System.Drawing.Point(19, 84);
            this.tiles.Name = "tiles";
            this.tiles.RowCount = 1;
            this.tiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tiles.Size = new System.Drawing.Size(1162, 89);
            this.tiles.TabIndex = 1;
            // 
            // lblTotal
            // 
            this.lblTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTotal.ForeColor = System.Drawing.Color.White;
            this.lblTotal.Location = new System.Drawing.Point(4, 4);
            this.lblTotal.Margin = new System.Windows.Forms.Padding(4);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(282, 81);
            this.lblTotal.TabIndex = 0;
            this.lblTotal.Tag = "TỔNG CẢNH BÁO";
            this.lblTotal.Text = "TỔNG CẢNH BÁO\n—";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblHigh
            // 
            this.lblHigh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblHigh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHigh.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblHigh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(113)))), ((int)(((byte)(113)))));
            this.lblHigh.Location = new System.Drawing.Point(294, 4);
            this.lblHigh.Margin = new System.Windows.Forms.Padding(4);
            this.lblHigh.Name = "lblHigh";
            this.lblHigh.Size = new System.Drawing.Size(282, 81);
            this.lblHigh.TabIndex = 1;
            this.lblHigh.Tag = "CAO";
            this.lblHigh.Text = "CAO\n—";
            this.lblHigh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMedium
            // 
            this.lblMedium.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblMedium.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMedium.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblMedium.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(146)))), ((int)(((byte)(60)))));
            this.lblMedium.Location = new System.Drawing.Point(584, 4);
            this.lblMedium.Margin = new System.Windows.Forms.Padding(4);
            this.lblMedium.Name = "lblMedium";
            this.lblMedium.Size = new System.Drawing.Size(282, 81);
            this.lblMedium.TabIndex = 2;
            this.lblMedium.Tag = "TRUNG BÌNH";
            this.lblMedium.Text = "TRUNG BÌNH\n—";
            this.lblMedium.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWatch
            // 
            this.lblWatch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblWatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWatch.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblWatch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(165)))), ((int)(((byte)(250)))));
            this.lblWatch.Location = new System.Drawing.Point(874, 4);
            this.lblWatch.Margin = new System.Windows.Forms.Padding(4);
            this.lblWatch.Name = "lblWatch";
            this.lblWatch.Size = new System.Drawing.Size(284, 81);
            this.lblWatch.TabIndex = 3;
            this.lblWatch.Tag = "THEO DÕI";
            this.lblWatch.Text = "THEO DÕI\n—";
            this.lblWatch.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // filters
            // 
            this.filters.AutoScroll = true;
            this.filters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.filters.Controls.Add(this.scope);
            this.filters.Controls.Add(this.level);
            this.filters.Controls.Add(this.searchLabel);
            this.filters.Controls.Add(this.search);
            this.filters.Controls.Add(this.refresh);
            this.filters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filters.Location = new System.Drawing.Point(19, 179);
            this.filters.Name = "filters";
            this.filters.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.filters.Size = new System.Drawing.Size(1162, 42);
            this.filters.TabIndex = 2;
            this.filters.WrapContents = false;
            // 
            // scope
            // 
            this.scope.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.scope.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scope.ForeColor = System.Drawing.Color.White;
            this.scope.Items.AddRange(new object[] {
            "Tất cả CN/DN",
            "Cá nhân",
            "Doanh nghiệp"});
            this.scope.Location = new System.Drawing.Point(3, 8);
            this.scope.Name = "scope";
            this.scope.Size = new System.Drawing.Size(155, 28);
            this.scope.TabIndex = 0;
            // 
            // level
            // 
            this.level.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.level.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.level.ForeColor = System.Drawing.Color.White;
            this.level.Items.AddRange(new object[] {
            "Mọi mức cảnh báo",
            "Cao",
            "Trung bình",
            "Theo dõi"});
            this.level.Location = new System.Drawing.Point(164, 8);
            this.level.Name = "level";
            this.level.Size = new System.Drawing.Size(155, 28);
            this.level.TabIndex = 1;
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.ForeColor = System.Drawing.Color.White;
            this.searchLabel.Location = new System.Drawing.Point(330, 12);
            this.searchLabel.Margin = new System.Windows.Forms.Padding(8, 7, 3, 0);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(169, 20);
            this.searchLabel.TabIndex = 2;
            this.searchLabel.Text = "STK / CCCD / MST / Tên:";
            // 
            // search
            // 
            this.search.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.search.ForeColor = System.Drawing.Color.White;
            this.search.Location = new System.Drawing.Point(505, 8);
            this.search.Name = "search";
            this.search.Size = new System.Drawing.Size(220, 27);
            this.search.TabIndex = 3;
            // 
            // refresh
            // 
            this.refresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refresh.ForeColor = System.Drawing.Color.White;
            this.refresh.Location = new System.Drawing.Point(731, 8);
            this.refresh.Name = "refresh";
            this.refresh.Size = new System.Drawing.Size(100, 30);
            this.refresh.TabIndex = 4;
            this.refresh.Text = "Làm mới";
            this.refresh.UseVisualStyleBackColor = false;
            // 
            // btnIntegrity
            // 
            this.btnIntegrity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnIntegrity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnIntegrity.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIntegrity.ForeColor = System.Drawing.Color.White;
            this.btnIntegrity.Location = new System.Drawing.Point(19, 228);
            this.btnIntegrity.Name = "btnIntegrity";
            this.btnIntegrity.Size = new System.Drawing.Size(225, 32);
            this.btnIntegrity.TabIndex = 3;
            this.btnIntegrity.Text = "Kiểm tra toàn vẹn dữ liệu";
            this.btnIntegrity.UseVisualStyleBackColor = false;
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.grid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.grid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.grid.ColumnHeadersHeight = 36;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.c0,
            this.c1,
            this.c2,
            this.c3,
            this.c4,
            this.c5,
            this.c6,
            this.c7,
            this.c8});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grid.DefaultCellStyle = dataGridViewCellStyle3;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.Location = new System.Drawing.Point(19, 267);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.RowHeadersWidth = 51;
            this.grid.RowTemplate.Height = 30;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(1162, 379);
            this.grid.TabIndex = 4;
            // 
            // c0
            // 
            this.c0.HeaderText = "Mức độ";
            this.c0.MinimumWidth = 6;
            this.c0.Name = "c0";
            this.c0.ReadOnly = true;
            this.c0.Width = 95;
            // 
            // c1
            // 
            this.c1.HeaderText = "Đối tượng";
            this.c1.MinimumWidth = 6;
            this.c1.Name = "c1";
            this.c1.ReadOnly = true;
            this.c1.Width = 110;
            // 
            // c2
            // 
            this.c2.HeaderText = "Loại TK";
            this.c2.MinimumWidth = 6;
            this.c2.Name = "c2";
            this.c2.ReadOnly = true;
            // 
            // c3
            // 
            this.c3.HeaderText = "Số tài khoản";
            this.c3.MinimumWidth = 6;
            this.c3.Name = "c3";
            this.c3.ReadOnly = true;
            this.c3.Width = 120;
            // 
            // c4
            // 
            this.c4.HeaderText = "CCCD / MST";
            this.c4.MinimumWidth = 6;
            this.c4.Name = "c4";
            this.c4.ReadOnly = true;
            this.c4.Width = 130;
            // 
            // c5
            // 
            this.c5.HeaderText = "Tên khách hàng";
            this.c5.MinimumWidth = 6;
            this.c5.Name = "c5";
            this.c5.ReadOnly = true;
            this.c5.Width = 180;
            // 
            // c6
            // 
            this.c6.HeaderText = "Cảnh báo";
            this.c6.MinimumWidth = 6;
            this.c6.Name = "c6";
            this.c6.ReadOnly = true;
            this.c6.Width = 320;
            // 
            // c7
            // 
            this.c7.HeaderText = "Số tiền";
            this.c7.MinimumWidth = 6;
            this.c7.Name = "c7";
            this.c7.ReadOnly = true;
            this.c7.Width = 145;
            // 
            // c8
            // 
            this.c8.HeaderText = "Đáo hạn";
            this.c8.MinimumWidth = 6;
            this.c8.Name = "c8";
            this.c8.ReadOnly = true;
            this.c8.Width = 110;
            // 
            // status
            // 
            this.status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.status.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.status.Location = new System.Drawing.Point(19, 649);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1162, 35);
            this.status.TabIndex = 5;
            this.status.Text = "Chế độ chỉ đọc — Dữ liệu được tải khi chạy chương trình.";
            // 
            // FrmRiskCenter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.root);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "FrmRiskCenter";
            this.Text = "Risk Control Center";
            this.root.ResumeLayout(false);
            this.heading.ResumeLayout(false);
            this.heading.PerformLayout();
            this.tiles.ResumeLayout(false);
            this.filters.ResumeLayout(false);
            this.filters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
