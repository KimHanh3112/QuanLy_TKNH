using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLTKNH
{
    partial class FrmVayDN
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelTopSearch = new System.Windows.Forms.Panel();
            this.lblLocThoiGian = new System.Windows.Forms.Label();
            this.cboLocThoiGian = new System.Windows.Forms.ComboBox();
            this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
            this.lblDen = new System.Windows.Forms.Label();
            this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
            this.cboLocKyHan = new System.Windows.Forms.ComboBox();
            this.lblLocKyHan = new System.Windows.Forms.Label();
            this.cboLocNhom = new System.Windows.Forms.ComboBox();
            this.lblLocNhom = new System.Windows.Forms.Label();
            this.btnTimCCCD = new System.Windows.Forms.Button();
            this.txtCCCDSearch = new System.Windows.Forms.TextBox();
            this.lblCCCD = new System.Windows.Forms.Label();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelSummary = new System.Windows.Forms.Panel();
            this.lblSelectedCustomerInfo = new System.Windows.Forms.Label();
            this.lblTongDaTra = new System.Windows.Forms.Label();
            this.lblTongNo = new System.Windows.Forms.Label();
            this.lblTongSoTK = new System.Windows.Forms.Label();
            this.panelGridContainer = new System.Windows.Forms.Panel();
            this.dgvVay = new System.Windows.Forms.DataGridView();
            this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSDT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDuNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDaTra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLaiSuat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKyHan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNgayLam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNhomNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNgayCapNhatTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripVay = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuMoThemTK = new System.Windows.Forms.ToolStripMenuItem();
            this.menuChuyenNhomNo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuThanhToan = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDongTK = new System.Windows.Forms.ToolStripMenuItem();
            this.menuKhoaMoTK = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLichSu = new System.Windows.Forms.ToolStripMenuItem();
            this.panelTopSearch.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.panelGridContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVay)).BeginInit();
            this.contextMenuStripVay.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTopSearch
            // 
            this.panelTopSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
            this.panelTopSearch.Controls.Add(this.lblLocThoiGian);
            this.panelTopSearch.Controls.Add(this.cboLocThoiGian);
            this.panelTopSearch.Controls.Add(this.dtpTuNgay);
            this.panelTopSearch.Controls.Add(this.lblDen);
            this.panelTopSearch.Controls.Add(this.dtpDenNgay);
            this.panelTopSearch.Controls.Add(this.cboLocKyHan);
            this.panelTopSearch.Controls.Add(this.lblLocKyHan);
            this.panelTopSearch.Controls.Add(this.cboLocNhom);
            this.panelTopSearch.Controls.Add(this.lblLocNhom);
            this.panelTopSearch.Controls.Add(this.btnTimCCCD);
            this.panelTopSearch.Controls.Add(this.txtCCCDSearch);
            this.panelTopSearch.Controls.Add(this.lblCCCD);
            this.panelTopSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopSearch.Location = new System.Drawing.Point(0, 50);
            this.panelTopSearch.Name = "panelTopSearch";
            this.panelTopSearch.Size = new System.Drawing.Size(1380, 55);
            this.panelTopSearch.TabIndex = 0;
            // 
            // lblLocThoiGian
            // 
            this.lblLocThoiGian.AutoSize = true;
            this.lblLocThoiGian.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLocThoiGian.ForeColor = System.Drawing.Color.White;
            this.lblLocThoiGian.Location = new System.Drawing.Point(860, 20);
            this.lblLocThoiGian.Name = "lblLocThoiGian";
            this.lblLocThoiGian.Size = new System.Drawing.Size(78, 20);
            this.lblLocThoiGian.TabIndex = 8;
            this.lblLocThoiGian.Text = "Thời gian:";
            // 
            // cboLocThoiGian
            // 
            this.cboLocThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocThoiGian.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboLocThoiGian.FormattingEnabled = true;
            this.cboLocThoiGian.Location = new System.Drawing.Point(958, 11);
            this.cboLocThoiGian.Name = "cboLocThoiGian";
            this.cboLocThoiGian.Size = new System.Drawing.Size(95, 29);
            this.cboLocThoiGian.TabIndex = 9;
            // 
            // dtpTuNgay
            // 
            this.dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            this.dtpTuNgay.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTuNgay.Location = new System.Drawing.Point(1072, 13);
            this.dtpTuNgay.Name = "dtpTuNgay";
            this.dtpTuNgay.Size = new System.Drawing.Size(105, 27);
            this.dtpTuNgay.TabIndex = 10;
            // 
            // lblDen
            // 
            this.lblDen.AutoSize = true;
            this.lblDen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDen.ForeColor = System.Drawing.Color.White;
            this.lblDen.Location = new System.Drawing.Point(1183, 16);
            this.lblDen.Name = "lblDen";
            this.lblDen.Size = new System.Drawing.Size(15, 20);
            this.lblDen.TabIndex = 11;
            this.lblDen.Text = "-";
            // 
            // dtpDenNgay
            // 
            this.dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            this.dtpDenNgay.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDenNgay.Location = new System.Drawing.Point(1204, 13);
            this.dtpDenNgay.Name = "dtpDenNgay";
            this.dtpDenNgay.Size = new System.Drawing.Size(105, 27);
            this.dtpDenNgay.TabIndex = 12;
            // 
            // cboLocKyHan
            // 
            this.cboLocKyHan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocKyHan.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboLocKyHan.FormattingEnabled = true;
            this.cboLocKyHan.Location = new System.Drawing.Point(732, 12);
            this.cboLocKyHan.Name = "cboLocKyHan";
            this.cboLocKyHan.Size = new System.Drawing.Size(95, 29);
            this.cboLocKyHan.TabIndex = 7;
            // 
            // lblLocKyHan
            // 
            this.lblLocKyHan.AutoSize = true;
            this.lblLocKyHan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLocKyHan.ForeColor = System.Drawing.Color.White;
            this.lblLocKyHan.Location = new System.Drawing.Point(645, 20);
            this.lblLocKyHan.Name = "lblLocKyHan";
            this.lblLocKyHan.Size = new System.Drawing.Size(60, 20);
            this.lblLocKyHan.TabIndex = 6;
            this.lblLocKyHan.Text = "Kỳ hạn:";
            // 
            // cboLocNhom
            // 
            this.cboLocNhom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocNhom.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cboLocNhom.FormattingEnabled = true;
            this.cboLocNhom.Location = new System.Drawing.Point(480, 13);
            this.cboLocNhom.Name = "cboLocNhom";
            this.cboLocNhom.Size = new System.Drawing.Size(150, 29);
            this.cboLocNhom.TabIndex = 5;
            // 
            // lblLocNhom
            // 
            this.lblLocNhom.AutoSize = true;
            this.lblLocNhom.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblLocNhom.ForeColor = System.Drawing.Color.White;
            this.lblLocNhom.Location = new System.Drawing.Point(386, 20);
            this.lblLocNhom.Name = "lblLocNhom";
            this.lblLocNhom.Size = new System.Drawing.Size(79, 20);
            this.lblLocNhom.TabIndex = 4;
            this.lblLocNhom.Text = "Nhóm nợ:";
            // 
            // btnTimCCCD
            // 
            this.btnTimCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTimCCCD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTimCCCD.FlatAppearance.BorderSize = 0;
            this.btnTimCCCD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTimCCCD.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnTimCCCD.ForeColor = System.Drawing.Color.White;
            this.btnTimCCCD.Location = new System.Drawing.Point(305, 12);
            this.btnTimCCCD.Name = "btnTimCCCD";
            this.btnTimCCCD.Size = new System.Drawing.Size(75, 30);
            this.btnTimCCCD.TabIndex = 2;
            this.btnTimCCCD.Text = "🔍 Tìm";
            this.btnTimCCCD.UseVisualStyleBackColor = false;
            // 
            // txtCCCDSearch
            // 
            this.txtCCCDSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtCCCDSearch.Location = new System.Drawing.Point(118, 12);
            this.txtCCCDSearch.MaxLength = 12;
            this.txtCCCDSearch.Name = "txtCCCDSearch";
            this.txtCCCDSearch.Size = new System.Drawing.Size(168, 30);
            this.txtCCCDSearch.TabIndex = 1;
            // 
            // lblCCCD
            // 
            this.lblCCCD.AutoSize = true;
            this.lblCCCD.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblCCCD.ForeColor = System.Drawing.Color.White;
            this.lblCCCD.Location = new System.Drawing.Point(10, 16);
            this.lblCCCD.Name = "lblCCCD";
            this.lblCCCD.Size = new System.Drawing.Size(144, 21);
            this.lblCCCD.TabIndex = 0;
            this.lblCCCD.Text = "Nhập Mã số thuế:";
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(1380, 50);
            this.panelHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(18, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(355, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "QUẢN LÝ TÀI KHOẢN VAY NỢ";
            // 
            // panelSummary
            // 
            this.panelSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.panelSummary.Controls.Add(this.lblSelectedCustomerInfo);
            this.panelSummary.Controls.Add(this.lblTongDaTra);
            this.panelSummary.Controls.Add(this.lblTongNo);
            this.panelSummary.Controls.Add(this.lblTongSoTK);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSummary.Location = new System.Drawing.Point(0, 105);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1380, 65);
            this.panelSummary.TabIndex = 3;
            this.panelSummary.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSummary_Paint);
            // 
            // lblSelectedCustomerInfo
            // 
            this.lblSelectedCustomerInfo.AutoSize = true;
            this.lblSelectedCustomerInfo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSelectedCustomerInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(224)))), ((int)(((byte)(71)))));
            this.lblSelectedCustomerInfo.Location = new System.Drawing.Point(15, 36);
            this.lblSelectedCustomerInfo.Name = "lblSelectedCustomerInfo";
            this.lblSelectedCustomerInfo.Size = new System.Drawing.Size(321, 21);
            this.lblSelectedCustomerInfo.TabIndex = 3;
            this.lblSelectedCustomerInfo.Text = "👤 Đang chọn: Chưa chọn doanh nghiệp";
            this.lblSelectedCustomerInfo.Click += new System.EventHandler(this.lblSelectedCustomerInfo_Click);
            // 
            // lblTongDaTra
            // 
            this.lblTongDaTra.AutoSize = true;
            this.lblTongDaTra.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTongDaTra.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(239)))), ((int)(((byte)(172)))));
            this.lblTongDaTra.Location = new System.Drawing.Point(580, 10);
            this.lblTongDaTra.Name = "lblTongDaTra";
            this.lblTongDaTra.Size = new System.Drawing.Size(244, 21);
            this.lblTongDaTra.TabIndex = 2;
            this.lblTongDaTra.Text = "✅ Tổng đã thanh toán: 0 VNĐ";
            // 
            // lblTongNo
            // 
            this.lblTongNo.AutoSize = true;
            this.lblTongNo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTongNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(165)))), ((int)(((byte)(165)))));
            this.lblTongNo.Location = new System.Drawing.Point(230, 10);
            this.lblTongNo.Name = "lblTongNo";
            this.lblTongNo.Size = new System.Drawing.Size(198, 21);
            this.lblTongNo.TabIndex = 1;
            this.lblTongNo.Text = "💰 Tổng tiền vay: 0 VNĐ";
            // 
            // lblTongSoTK
            // 
            this.lblTongSoTK.AutoSize = true;
            this.lblTongSoTK.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTongSoTK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.lblTongSoTK.Location = new System.Drawing.Point(15, 10);
            this.lblTongSoTK.Name = "lblTongSoTK";
            this.lblTongSoTK.Size = new System.Drawing.Size(169, 21);
            this.lblTongSoTK.TabIndex = 0;
            this.lblTongSoTK.Text = "📊 Tổng tài khoản: 0";
            // 
            // panelGridContainer
            // 
            this.panelGridContainer.Controls.Add(this.dgvVay);
            this.panelGridContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGridContainer.Location = new System.Drawing.Point(0, 170);
            this.panelGridContainer.Name = "panelGridContainer";
            this.panelGridContainer.Padding = new System.Windows.Forms.Padding(15);
            this.panelGridContainer.Size = new System.Drawing.Size(1380, 580);
            this.panelGridContainer.TabIndex = 2;
            // 
            // dgvVay
            // 
            this.dgvVay.AllowUserToAddRows = false;
            this.dgvVay.AllowUserToDeleteRows = false;
            this.dgvVay.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvVay.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvVay.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvVay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVay.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSTT,
            this.colSTK,
            this.colCCCD,
            this.colHoTen,
            this.colSDT,
            this.colDuNo,
            this.colDaTra,
            this.colLaiSuat,
            this.colKyHan,
            this.colNgayLam,
            this.colNhomNo,
            this.colNgayCapNhatTT,
            this.colVaiTro,
            this.colTrangThai});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvVay.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvVay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVay.Location = new System.Drawing.Point(15, 15);
            this.dgvVay.MultiSelect = false;
            this.dgvVay.Name = "dgvVay";
            this.dgvVay.ReadOnly = true;
            this.dgvVay.RowHeadersVisible = false;
            this.dgvVay.RowHeadersWidth = 51;
            this.dgvVay.RowTemplate.Height = 28;
            this.dgvVay.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVay.Size = new System.Drawing.Size(1350, 550);
            this.dgvVay.TabIndex = 0;
            this.dgvVay.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVay_CellContentClick);
            // 
            // colSTT
            // 
            this.colSTT.FillWeight = 30F;
            this.colSTT.HeaderText = "STT";
            this.colSTT.MinimumWidth = 6;
            this.colSTT.Name = "colSTT";
            this.colSTT.ReadOnly = true;
            // 
            // colSTK
            // 
            this.colSTK.FillWeight = 65F;
            this.colSTK.HeaderText = "STK";
            this.colSTK.MinimumWidth = 6;
            this.colSTK.Name = "colSTK";
            this.colSTK.ReadOnly = true;
            // 
            // colCCCD
            // 
            this.colCCCD.FillWeight = 85F;
            this.colCCCD.HeaderText = "MST";
            this.colCCCD.MinimumWidth = 6;
            this.colCCCD.Name = "colCCCD";
            this.colCCCD.ReadOnly = true;
            // 
            // colHoTen
            // 
            this.colHoTen.HeaderText = "Tên doanh nghiệp";
            this.colHoTen.MinimumWidth = 6;
            this.colHoTen.Name = "colHoTen";
            this.colHoTen.ReadOnly = true;
            // 
            // colSDT
            // 
            this.colSDT.FillWeight = 80F;
            this.colSDT.HeaderText = "SĐT";
            this.colSDT.MinimumWidth = 6;
            this.colSDT.Name = "colSDT";
            this.colSDT.ReadOnly = true;
            // 
            // colDuNo
            // 
            this.colDuNo.FillWeight = 95F;
            this.colDuNo.HeaderText = "Tổng tiền vay";
            this.colDuNo.MinimumWidth = 6;
            this.colDuNo.Name = "colDuNo";
            this.colDuNo.ReadOnly = true;
            // 
            // colDaTra
            // 
            this.colDaTra.FillWeight = 85F;
            this.colDaTra.HeaderText = "Tổng đã trả (gốc + lãi + phí)";
            this.colDaTra.MinimumWidth = 6;
            this.colDaTra.Name = "colDaTra";
            this.colDaTra.ReadOnly = true;
            // 
            // colLaiSuat
            // 
            this.colLaiSuat.FillWeight = 65F;
            this.colLaiSuat.HeaderText = "Lãi suất";
            this.colLaiSuat.MinimumWidth = 6;
            this.colLaiSuat.Name = "colLaiSuat";
            this.colLaiSuat.ReadOnly = true;
            // 
            // colKyHan
            // 
            this.colKyHan.FillWeight = 65F;
            this.colKyHan.HeaderText = "Kỳ hạn";
            this.colKyHan.MinimumWidth = 6;
            this.colKyHan.Name = "colKyHan";
            this.colKyHan.ReadOnly = true;
            // 
            // colNgayLam
            // 
            this.colNgayLam.FillWeight = 110F;
            this.colNgayLam.HeaderText = "Ngày làm";
            this.colNgayLam.MinimumWidth = 6;
            this.colNgayLam.Name = "colNgayLam";
            this.colNgayLam.ReadOnly = true;
            // 
            // colNhomNo
            // 
            this.colNhomNo.FillWeight = 130F;
            this.colNhomNo.HeaderText = "Nhóm nợ";
            this.colNhomNo.MinimumWidth = 6;
            this.colNhomNo.Name = "colNhomNo";
            this.colNhomNo.ReadOnly = true;
            // 
            // colNgayCapNhatTT
            // 
            this.colNgayCapNhatTT.FillWeight = 110F;
            this.colNgayCapNhatTT.HeaderText = "Ngày cập nhật TT";
            this.colNgayCapNhatTT.MinimumWidth = 6;
            this.colNgayCapNhatTT.Name = "colNgayCapNhatTT";
            this.colNgayCapNhatTT.ReadOnly = true;
            // 
            // colVaiTro
            // 
            this.colVaiTro.FillWeight = 150F;
            this.colVaiTro.HeaderText = "Vai trò";
            this.colVaiTro.MinimumWidth = 150;
            this.colVaiTro.Name = "colVaiTro";
            this.colVaiTro.ReadOnly = true;
            // 
            // colTrangThai
            // 
            this.colTrangThai.FillWeight = 70F;
            this.colTrangThai.HeaderText = "Trạng thái";
            this.colTrangThai.MinimumWidth = 6;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.ReadOnly = true;
            // 
            // contextMenuStripVay
            // 
            this.contextMenuStripVay.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripVay.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuMoThemTK});
            this.contextMenuStripVay.Name = "contextMenuStripVay";
            this.contextMenuStripVay.Size = new System.Drawing.Size(255, 28);
            // 
            // menuMoThemTK
            // 
            this.menuMoThemTK.Name = "menuMoThemTK";
            this.menuMoThemTK.Size = new System.Drawing.Size(254, 24);
            this.menuMoThemTK.Text = "➕ Mở thêm tài khoản vay";
            // 
            // menuChuyenNhomNo
            // 
            this.menuChuyenNhomNo.Name = "menuChuyenNhomNo";
            this.menuChuyenNhomNo.Size = new System.Drawing.Size(290, 24);
            this.menuChuyenNhomNo.Text = "🔄 Chuyển nhóm nợ";
            // 
            // menuThanhToan
            // 
            this.menuThanhToan.Name = "menuThanhToan";
            this.menuThanhToan.Size = new System.Drawing.Size(290, 24);
            this.menuThanhToan.Text = "💵 Thanh toán tiền";
            // 
            // menuDongTK
            // 
            this.menuDongTK.Name = "menuDongTK";
            this.menuDongTK.Size = new System.Drawing.Size(290, 24);
            this.menuDongTK.Text = "❌ Đóng tài khoản";
            // 
            // menuKhoaMoTK
            // 
            this.menuKhoaMoTK.Name = "menuKhoaMoTK";
            this.menuKhoaMoTK.Size = new System.Drawing.Size(290, 24);
            this.menuKhoaMoTK.Text = "🔒 Khóa tạm thời/Mở tài khoản";
            // 
            // menuLichSu
            // 
            this.menuLichSu.Name = "menuLichSu";
            this.menuLichSu.Size = new System.Drawing.Size(290, 24);
            this.menuLichSu.Text = "📜 Xem lịch sử đóng tiền";
            // 
            // FrmVayDN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1380, 750);
            this.Controls.Add(this.panelGridContainer);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.panelTopSearch);
            this.Controls.Add(this.panelHeader);
            this.Name = "FrmVayDN";
            this.Text = "QUẢN LÝ TÀI KHOẢN VAY";
            this.panelTopSearch.ResumeLayout(false);
            this.panelTopSearch.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelSummary.ResumeLayout(false);
            this.panelSummary.PerformLayout();
            this.panelGridContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVay)).EndInit();
            this.contextMenuStripVay.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTopSearch;
        private System.Windows.Forms.Label lblCCCD;
        private System.Windows.Forms.TextBox txtCCCDSearch;
        private System.Windows.Forms.Button btnTimCCCD;
        private System.Windows.Forms.Label lblLocNhom;
        private System.Windows.Forms.ComboBox cboLocNhom;
        private System.Windows.Forms.Label lblLocKyHan;
        private System.Windows.Forms.ComboBox cboLocKyHan;
        private System.Windows.Forms.Label lblLocThoiGian;
        private System.Windows.Forms.ComboBox cboLocThoiGian;
        private System.Windows.Forms.DateTimePicker dtpTuNgay;
        private System.Windows.Forms.Label lblDen;
        private System.Windows.Forms.DateTimePicker dtpDenNgay;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelSummary;
        private System.Windows.Forms.Label lblTongSoTK;
        private System.Windows.Forms.Label lblTongNo;
        private System.Windows.Forms.Label lblTongDaTra;
        private System.Windows.Forms.Label lblSelectedCustomerInfo;
        private System.Windows.Forms.Panel panelGridContainer;
        private System.Windows.Forms.DataGridView dgvVay;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripVay;
        private System.Windows.Forms.ToolStripMenuItem menuMoThemTK;
        private System.Windows.Forms.ToolStripMenuItem menuChuyenNhomNo;
        private System.Windows.Forms.ToolStripMenuItem menuThanhToan;
        private System.Windows.Forms.ToolStripMenuItem menuDongTK;
        private System.Windows.Forms.ToolStripMenuItem menuKhoaMoTK;
        private System.Windows.Forms.ToolStripMenuItem menuLichSu;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSTK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCCCD;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSDT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDuNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDaTra;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLaiSuat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKyHan;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNgayLam;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNhomNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNgayCapNhatTT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVaiTro;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;
        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign36()
        {
            return new Button
            {
                Text = "Xuất PDF",
                Width = 135,
                Height = 35,
                Location = new Point(12, 10),
                BackColor = colorActiveBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Panel CreatePanelDesign35()
        {
            return new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = Color.White };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private DataGridView CreateDataGridViewDesign34()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign33()
        {
            return new Button { Text = "Xác nhận", Location = new Point(150, 120), Width = 100, BackColor = colorActiveBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign32()
        {
            return new TextBox { Location = new Point(20, 45), Width = 340, Height = 60, Multiline = true };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign31()
        {
            return new Button { Text = "Hủy", Location = new Point(330, 460), Size = new Size(110, 40), Font = new Font("Segoe UI", 9.5F) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign30()
        {
            return new Button { Text = "Xác nhận thanh toán", Location = new Point(140, 460), Size = new Size(180, 40), BackColor = colorActiveBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign29()
        {
            return new Label { Location = new Point(20, 365), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.DarkGreen };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign28()
        {
            return new TextBox { Location = new Point(35, 130), Width = 200, Enabled = false, Font = new Font("Segoe UI", 9F) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private RadioButton CreateRadioButtonDesign27()
        {
            return new RadioButton { Text = "Trả theo kỳ (Gốc chia đều các tháng + Lãi + Phí phạt)", Location = new Point(15, 60), AutoSize = true, Font = new Font("Segoe UI", 9F) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private GroupBox CreateGroupBoxDesign26()
        {
            return new GroupBox
            {
                Text = "Chọn Phương Thức Trả",
                Location = new Point(20, 175),
                Size = new Size(520, 180),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private GroupBox CreateGroupBoxDesign25()
        {
            return new GroupBox
            {
                Text = "Bảng Tính Lãi & Phí Phát Sinh",
                Location = new Point(20, 15),
                Size = new Size(520, 150),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign24()
        {
            return new Button { Text = "Hủy", Location = new Point(210, 110), Size = new Size(90, 35), Font = new Font("Segoe UI", 9F) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign23()
        {
            return new Button { Text = "Xác nhận", Location = new Point(100, 110), Size = new Size(100, 35), BackColor = colorActiveBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private ComboBox CreateComboBoxDesign22()
        {
            return new ComboBox { Location = new Point(20, 50), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign21()
        {
            return new Label { Text = "Số tiền tối đa có thể vay: 150,000,000 VNĐ", Location = new Point(25, 130), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign20()
        {
            return new Label { Text = "", Location = new Point(25, 197), AutoSize = true, ForeColor = Color.DarkBlue, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign19()
        {
            return new TextBox { Location = new Point(180, 157), Width = 180, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign18()
        {
            return new Label { Text = "Lãi tại ngày vay:", Location = new Point(25, 160), AutoSize = true, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign17()
        {
            return new TextBox { Location = new Point(180, 122), Width = 180, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign16()
        {
            return new Label { Text = "Gốc sổ (VNĐ):", Location = new Point(25, 125), AutoSize = true, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign15()
        {
            return new TextBox { Location = new Point(180, 87), Width = 180, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private ComboBox CreateComboBoxDesign14()
        {
            return new ComboBox { Location = new Point(180, 87), Width = 305, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private ComboBox CreateComboBoxDesign13()
        {
            return new ComboBox { Location = new Point(180, 45), Width = 305, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Panel CreatePanelDesign12()
        {
            return new Panel
            {
                Location = new Point(25, 235),
                Size = new Size(520, 380),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign11()
        {
            return new TextBox { Location = new Point(165, 197), Width = 230, ReadOnly = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.Black, BackColor = Color.White };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign10()
        {
            return new Label { Text = "SĐT", Location = new Point(340, 23), AutoSize = true };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign9()
        {
            return new Label { Text = "Tên doanh nghiệp", Location = new Point(172, 23), AutoSize = true };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Label CreateLabelDesign8()
        {
            return new Label { Text = "MST", Location = new Point(7, 23), AutoSize = true };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign7()
        {
            return new TextBox { Location = new Point(375, 20), Width = 105, MaxLength = 11 };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign6()
        {
            return new TextBox { Location = new Point(225, 20), Width = 105 };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private TextBox CreateTextBoxDesign5()
        {
            return new TextBox { Location = new Point(50, 20), Width = 115, MaxLength = 12 };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Panel CreatePanelDesign4()
        {
            return new Panel
            {
                Location = new Point(25, 160),
                Size = new Size(520, 205),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AutoScroll = true,
                Visible = false
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private NumericUpDown CreateNumericUpDownDesign3()
        {
            return new NumericUpDown
            {
                Location = new Point(410, 125),
                Width = 135,
                Minimum = 1,
                Maximum = 3,
                Value = 1,
                Visible = false
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private Button CreateButtonDesign2()
        {
            return new Button
            {
                Text = "+ Thêm người được ủy quyền",
                Location = new Point(355, 90),
                Size = new Size(190, 28),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(37, 99, 235),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 8.5F),
                Enabled = true,
                Cursor = Cursors.Hand
            };
        }

        // Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmVayDN.cs.
        private GroupBox CreateGroupBoxDesign1()
        {
            return new GroupBox
            {
                Name = "grpTimKiemTuLamMoi",
                Text = "",
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = panelTopSearch.BackColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
        }
        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Form CreateFormDynamicDesign40(string value1)
        {
            return new Form
            {
                Text = value1,
                Size = new Size(780, 450),
                StartPosition = FormStartPosition.CenterParent
            };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign39(string value1)
        {
            return new Label { Text = value1, Location = new Point(20, 20), AutoSize = true };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private RadioButton CreateRadioButtonDynamicDesign38(string value1)
        {
            return new RadioButton { Text = value1, Location = new Point(15, 95), AutoSize = true, Font = new Font("Segoe UI", 9F) };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private RadioButton CreateRadioButtonDynamicDesign37(string value1)
        {
            return new RadioButton { Text = value1, Location = new Point(15, 25), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 9F) };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign36(string value1)
        {
            return new Label { Text = value1, Location = new Point(15, 115), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.Blue };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign35(string value1, Color value2)
        {
            return new Label { Text = value1, Location = new Point(15, 85), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = value2 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign34(string value1)
        {
            return new Label { Text = value1, Location = new Point(15, 55), AutoSize = true, Font = new Font("Segoe UI", 9F) };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign33(string value1)
        {
            return new Label { Text = value1, Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9F) };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Form CreateFormDynamicDesign32(string value1)
        {
            return new Form
            {
                Text = value1,
                Size = new Size(580, 570),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign31(string value1)
        {
            return new Label { Text = value1, Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Button CreateButtonDynamicDesign30(Font value1)
        {
            return new Button
            {
                Text = "Hủy",
                Location = new Point(240, 250),
                Size = new Size(110, 35),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = value1,
                Cursor = Cursors.Hand
            };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Button CreateButtonDynamicDesign29(Font value1)
        {
            return new Button
            {
                Text = "Xác nhận mở",
                Location = new Point(110, 250),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = value1,
                Cursor = Cursors.Hand
            };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private ComboBox CreateComboBoxDynamicDesign28(Font value1)
        {
            return new ComboBox { Location = new Point(180, 197), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign27(Font value1)
        {
            return new Label { Text = "Kỳ hạn vay:", Location = new Point(25, 200), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign26(Font value1)
        {
            return new TextBox { Location = new Point(180, 162), Width = 180, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign25(Font value1)
        {
            return new Label { Text = "Số tiền muốn vay:", Location = new Point(25, 165), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private ComboBox CreateComboBoxDynamicDesign24(Font value1)
        {
            return new ComboBox { Location = new Point(180, 87), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign23(Font value1)
        {
            return new Label { Text = "Nhóm nợ hiện tại:", Location = new Point(25, 90), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign22(Font value1)
        {
            return new Label { Text = "Chọn sổ tiết kiệm:", Location = new Point(25, 90), AutoSize = true, Font = value1, Visible = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign21(Font value1)
        {
            return new Label { Text = "Loại thế chấp:", Location = new Point(25, 48), AutoSize = true, Font = value1, Visible = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign20(Font value1)
        {
            return new TextBox { Location = new Point(180, 87), Width = 180, Text = "1000000000", Font = value1, Visible = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign19(Font value1)
        {
            return new Label { Text = "Định giá TS (VNĐ):", Location = new Point(25, 90), AutoSize = true, Font = value1, Visible = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign18(Font value1)
        {
            return new TextBox { Location = new Point(180, 52), Width = 180, Text = "Bất động sản", Font = value1, Visible = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign17(Font value1)
        {
            return new Label { Text = "Tên tài sản:", Location = new Point(25, 55), AutoSize = true, Font = value1, Visible = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign16(Font value1)
        {
            return new TextBox { Location = new Point(180, 52), Width = 180, Text = "15000000", Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign15(Font value1)
        {
            return new Label { Text = "Mức lương (VNĐ):", Location = new Point(25, 55), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private RadioButton CreateRadioButtonDynamicDesign14(Font value1)
        {
            return new RadioButton { Text = "Vay Thế Chấp", Location = new Point(180, 15), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private RadioButton CreateRadioButtonDynamicDesign13(Font value1)
        {
            return new RadioButton { Text = "Vay Tín Chấp", Location = new Point(25, 15), AutoSize = true, Checked = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign12(Font value1)
        {
            return new Label { Text = "Số TK được cấp:", Location = new Point(35, 200), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private RadioButton CreateRadioButtonDynamicDesign11(Font value1)
        {
            return new RadioButton { Text = "Số đẹp (Tính phí)", Location = new Point(200, 165), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private RadioButton CreateRadioButtonDynamicDesign10(Font value1)
        {
            return new RadioButton { Text = "Số ngẫu nhiên (Free)", Location = new Point(35, 165), AutoSize = true, Checked = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private ComboBox CreateComboBoxDynamicDesign9(Font value1)
        {
            return new ComboBox { Location = new Point(165, 127), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = value1, Enabled = false };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign8(Font value1)
        {
            return new Label { Text = "Loại tài khoản:", Location = new Point(35, 130), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private GroupBox CreateGroupBoxDynamicDesign7(string value1, Point value2)
        {
            return new GroupBox { Text = value1, Location = value2, Size = new Size(490, 58) };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign6(string value1, Font value2)
        {
            return new TextBox { Location = new Point(165, 92), Width = 230, Text = value1, ReadOnly = true, Enabled = false, Font = value2 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign5(Font value1)
        {
            return new Label { Text = "Số điện thoại:", Location = new Point(35, 95), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign4(string value1, Font value2)
        {
            return new TextBox { Location = new Point(165, 57), Width = 230, Text = value1, ReadOnly = true, Enabled = false, Font = value2 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign3(Font value1)
        {
            return new Label { Text = "Họ và Tên:", Location = new Point(35, 60), AutoSize = true, Font = value1 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private TextBox CreateTextBoxDynamicDesign2(string value1, Font value2)
        {
            return new TextBox { Location = new Point(165, 22), Width = 230, Text = value1, ReadOnly = true, Enabled = false, Font = value2 };
        }

        // Bố cục giao diện; giá trị động được truyền từ phần logic.
        private Label CreateLabelDynamicDesign1(Font value1)
        {
            return new Label { Text = "CCCD (12 số):", Location = new Point(35, 25), AutoSize = true, Font = value1 };
        }
    }
}
