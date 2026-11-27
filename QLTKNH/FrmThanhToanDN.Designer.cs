using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLTKNH
{
    partial class FrmThanhToanDN
    {
        private System.ComponentModel.IContainer components;
        private Panel pnlTop, pnlOverview, pnlSearch;
        private FlowLayoutPanel pnlBoLoc;
        private Label lblTitle, lblStatus, lblOverview, lblTu, lblDen, lblMST, lblTrangThai, lblThoiGian;
        private TextBox txtTim;
        private Button btnTim, btnBaoCao, btnHoaDon;
        private ComboBox cboTrangThai, cboThoiGian;
        private DateTimePicker dtpTu, dtpDen;
        private DataGridView dgv;
        private ContextMenuStrip cmsTaiKhoan;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.pnlBoLoc = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlSearch = new System.Windows.Forms.Panel();
            this.lblMST = new System.Windows.Forms.Label();
            this.txtTim = new System.Windows.Forms.TextBox();
            this.btnTim = new System.Windows.Forms.Button();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.cboTrangThai = new System.Windows.Forms.ComboBox();
            this.lblThoiGian = new System.Windows.Forms.Label();
            this.cboThoiGian = new System.Windows.Forms.ComboBox();
            this.lblTu = new System.Windows.Forms.Label();
            this.dtpTu = new System.Windows.Forms.DateTimePicker();
            this.lblDen = new System.Windows.Forms.Label();
            this.dtpDen = new System.Windows.Forms.DateTimePicker();
            this.btnBaoCao = new System.Windows.Forms.Button();
            this.btnHoaDon = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlOverview = new System.Windows.Forms.Panel();
            this.lblOverview = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.cmsTaiKhoan = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pnlTop.SuspendLayout();
            this.pnlBoLoc.SuspendLayout();
            this.pnlSearch.SuspendLayout();
            this.pnlOverview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.pnlTop.Controls.Add(this.pnlBoLoc);
            this.pnlTop.Controls.Add(this.lblTitle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1250, 100);
            this.pnlTop.TabIndex = 4;
            // 
            // pnlBoLoc
            // 
            this.pnlBoLoc.AutoScroll = true;
            this.pnlBoLoc.Controls.Add(this.pnlSearch);
            this.pnlBoLoc.Controls.Add(this.btnTim);
            this.pnlBoLoc.Controls.Add(this.lblTrangThai);
            this.pnlBoLoc.Controls.Add(this.cboTrangThai);
            this.pnlBoLoc.Controls.Add(this.lblThoiGian);
            this.pnlBoLoc.Controls.Add(this.cboThoiGian);
            this.pnlBoLoc.Controls.Add(this.lblTu);
            this.pnlBoLoc.Controls.Add(this.dtpTu);
            this.pnlBoLoc.Controls.Add(this.lblDen);
            this.pnlBoLoc.Controls.Add(this.dtpDen);
            this.pnlBoLoc.Controls.Add(this.btnBaoCao);
            this.pnlBoLoc.Controls.Add(this.btnHoaDon);
            this.pnlBoLoc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBoLoc.Location = new System.Drawing.Point(0, 40);
            this.pnlBoLoc.Name = "pnlBoLoc";
            this.pnlBoLoc.Padding = new System.Windows.Forms.Padding(16, 5, 8, 5);
            this.pnlBoLoc.Size = new System.Drawing.Size(1250, 60);
            this.pnlBoLoc.TabIndex = 0;
            // 
            // pnlSearch
            // 
            this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.pnlSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSearch.Controls.Add(this.lblMST);
            this.pnlSearch.Controls.Add(this.txtTim);
            this.pnlSearch.Location = new System.Drawing.Point(19, 8);
            this.pnlSearch.Name = "pnlSearch";
            this.pnlSearch.Size = new System.Drawing.Size(230, 38);
            this.pnlSearch.TabIndex = 0;
            // 
            // lblMST
            // 
            this.lblMST.AutoSize = true;
            this.lblMST.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblMST.Location = new System.Drawing.Point(6, 9);
            this.lblMST.Name = "lblMST";
            this.lblMST.Size = new System.Drawing.Size(91, 21);
            this.lblMST.TabIndex = 0;
            this.lblMST.Text = "MST/CCCD:";
            // 
            // txtTim
            // 
            this.txtTim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtTim.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTim.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtTim.ForeColor = System.Drawing.Color.White;
            this.txtTim.Location = new System.Drawing.Point(93, 8);
            this.txtTim.MaxLength = 200;
            this.txtTim.Name = "txtTim";
            this.txtTim.Size = new System.Drawing.Size(127, 23);
            this.txtTim.TabIndex = 1;
            // 
            // btnTim
            // 
            this.btnTim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTim.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTim.FlatAppearance.BorderSize = 0;
            this.btnTim.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTim.ForeColor = System.Drawing.Color.White;
            this.btnTim.Location = new System.Drawing.Point(255, 8);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(132, 38);
            this.btnTim.TabIndex = 1;
            this.btnTim.Text = "Tìm MST/CCCD";
            this.btnTim.UseVisualStyleBackColor = false;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Location = new System.Drawing.Point(398, 17);
            this.lblTrangThai.Margin = new System.Windows.Forms.Padding(8, 12, 0, 0);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(82, 21);
            this.lblTrangThai.TabIndex = 2;
            this.lblTrangThai.Text = "Trạng thái:";
            // 
            // cboTrangThai
            // 
            this.cboTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrangThai.Items.AddRange(new object[] {
            "Tất cả trạng thái",
            "Hoạt động",
            "Đang khóa",
            "Đã đóng"});
            this.cboTrangThai.Location = new System.Drawing.Point(483, 13);
            this.cboTrangThai.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Size = new System.Drawing.Size(128, 29);
            this.cboTrangThai.TabIndex = 3;
            // 
            // lblThoiGian
            // 
            this.lblThoiGian.AutoSize = true;
            this.lblThoiGian.Location = new System.Drawing.Point(622, 17);
            this.lblThoiGian.Margin = new System.Windows.Forms.Padding(8, 12, 0, 0);
            this.lblThoiGian.Name = "lblThoiGian";
            this.lblThoiGian.Size = new System.Drawing.Size(78, 21);
            this.lblThoiGian.TabIndex = 4;
            this.lblThoiGian.Text = "Thời gian:";
            // 
            // cboThoiGian
            // 
            this.cboThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboThoiGian.Items.AddRange(new object[] {
            "Tất cả ngày",
            "Hôm nay",
            "Hôm qua",
            "7 ngày",
            "30 ngày",
            "3 tháng",
            "1 năm",
            "Tùy chọn"});
            this.cboThoiGian.Location = new System.Drawing.Point(703, 13);
            this.cboThoiGian.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.cboThoiGian.Name = "cboThoiGian";
            this.cboThoiGian.Size = new System.Drawing.Size(115, 29);
            this.cboThoiGian.TabIndex = 5;
            // 
            // lblTu
            // 
            this.lblTu.AutoSize = true;
            this.lblTu.Location = new System.Drawing.Point(826, 17);
            this.lblTu.Margin = new System.Windows.Forms.Padding(5, 12, 0, 0);
            this.lblTu.Name = "lblTu";
            this.lblTu.Size = new System.Drawing.Size(30, 21);
            this.lblTu.TabIndex = 6;
            this.lblTu.Text = "Từ:";
            this.lblTu.Visible = false;
            // 
            // dtpTu
            // 
            this.dtpTu.CustomFormat = "dd/MM/yyyy";
            this.dtpTu.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTu.Location = new System.Drawing.Point(859, 13);
            this.dtpTu.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.dtpTu.Name = "dtpTu";
            this.dtpTu.Size = new System.Drawing.Size(112, 29);
            this.dtpTu.TabIndex = 7;
            this.dtpTu.Visible = false;
            // 
            // lblDen
            // 
            this.lblDen.AutoSize = true;
            this.lblDen.Location = new System.Drawing.Point(979, 17);
            this.lblDen.Margin = new System.Windows.Forms.Padding(5, 12, 0, 0);
            this.lblDen.Name = "lblDen";
            this.lblDen.Size = new System.Drawing.Size(41, 21);
            this.lblDen.TabIndex = 8;
            this.lblDen.Text = "Đến:";
            this.lblDen.Visible = false;
            // 
            // dtpDen
            // 
            this.dtpDen.CustomFormat = "dd/MM/yyyy";
            this.dtpDen.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDen.Location = new System.Drawing.Point(1023, 13);
            this.dtpDen.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            this.dtpDen.Name = "dtpDen";
            this.dtpDen.Size = new System.Drawing.Size(112, 29);
            this.dtpDen.TabIndex = 9;
            this.dtpDen.Visible = false;
            // 
            // btnBaoCao
            // 
            this.btnBaoCao.Location = new System.Drawing.Point(1141, 8);
            this.btnBaoCao.Name = "btnBaoCao";
            this.btnBaoCao.Size = new System.Drawing.Size(75, 23);
            this.btnBaoCao.TabIndex = 10;
            this.btnBaoCao.Text = "Xuất báo cáo";
            this.btnBaoCao.Visible = false;
            // 
            // btnHoaDon
            // 
            this.btnHoaDon.Location = new System.Drawing.Point(19, 52);
            this.btnHoaDon.Name = "btnHoaDon";
            this.btnHoaDon.Size = new System.Drawing.Size(75, 23);
            this.btnHoaDon.TabIndex = 11;
            this.btnHoaDon.Text = "Xuất hóa đơn";
            this.btnHoaDon.Visible = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(16, 6, 0, 0);
            this.lblTitle.Size = new System.Drawing.Size(1250, 40);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "TÀI KHOẢN THANH TOÁN DOANH NGHIỆP";
            // 
            // pnlOverview
            // 
            this.pnlOverview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.pnlOverview.Controls.Add(this.lblOverview);
            this.pnlOverview.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOverview.Location = new System.Drawing.Point(0, 100);
            this.pnlOverview.Name = "pnlOverview";
            this.pnlOverview.Size = new System.Drawing.Size(1250, 50);
            this.pnlOverview.TabIndex = 3;
            // 
            // lblOverview
            // 
            this.lblOverview.AutoSize = true;
            this.lblOverview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblOverview.ForeColor = System.Drawing.Color.White;
            this.lblOverview.Location = new System.Drawing.Point(16, 15);
            this.lblOverview.Name = "lblOverview";
            this.lblOverview.Size = new System.Drawing.Size(152, 20);
            this.lblOverview.TabIndex = 0;
            this.lblOverview.Text = "Chưa chọn tài khoản";
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 687);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(14, 7, 0, 0);
            this.lblStatus.Size = new System.Drawing.Size(1250, 33);
            this.lblStatus.TabIndex = 2;
            // 
            // dgv
            // 
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
            this.dgv.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgv.ColumnHeadersHeight = 40;
            this.dgv.ContextMenuStrip = this.cmsTaiKhoan;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.EnableHeadersVisualStyles = false;
            this.dgv.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.dgv.Location = new System.Drawing.Point(0, 150);
            this.dgv.MultiSelect = false;
            this.dgv.Name = "dgv";
            this.dgv.ReadOnly = true;
            this.dgv.RowHeadersVisible = false;
            this.dgv.RowHeadersWidth = 51;
            this.dgv.RowTemplate.Height = 32;
            this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv.Size = new System.Drawing.Size(1250, 537);
            this.dgv.TabIndex = 1;
            this.dgv.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellContentClick);
            // 
            // cmsTaiKhoan
            // 
            this.cmsTaiKhoan.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsTaiKhoan.Name = "cmsTaiKhoan";
            this.cmsTaiKhoan.Size = new System.Drawing.Size(61, 4);
            // 
            // FrmThanhToanDN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1250, 720);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlOverview);
            this.Controls.Add(this.pnlTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "FrmThanhToanDN";
            this.Text = "Quản lý tài khoản thanh toán doanh nghiệp";
            this.pnlTop.ResumeLayout(false);
            this.pnlBoLoc.ResumeLayout(false);
            this.pnlBoLoc.PerformLayout();
            this.pnlSearch.ResumeLayout(false);
            this.pnlSearch.PerformLayout();
            this.pnlOverview.ResumeLayout(false);
            this.pnlOverview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
