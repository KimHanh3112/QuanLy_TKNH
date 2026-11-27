namespace QLTKNH
{
	partial class FrmTietKiem
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			this.panelHeader = new System.Windows.Forms.Panel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.panelSearch = new System.Windows.Forms.Panel();
			this.lblCCCD = new System.Windows.Forms.Label();
			this.txtCCCD = new System.Windows.Forms.TextBox();
			this.btnTimCCCD = new System.Windows.Forms.Button();
			this.panelFilter = new System.Windows.Forms.Panel();
			this.lblLocKyHan = new System.Windows.Forms.Label();
			this.cboLocKyHan = new System.Windows.Forms.ComboBox();
			this.lblLocTrangThai = new System.Windows.Forms.Label();
			this.cboLocTrangThai = new System.Windows.Forms.ComboBox();
			this.lblLocThoiGian = new System.Windows.Forms.Label();
			this.cboLocThoiGian = new System.Windows.Forms.ComboBox();
			this.lblTuNgay = new System.Windows.Forms.Label();
			this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
			this.lblDenNgay = new System.Windows.Forms.Label();
			this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
			this.panelSummary = new System.Windows.Forms.Panel();
			this.lblTongTaiKhoan = new System.Windows.Forms.Label();
			this.lblTongTienGui = new System.Windows.Forms.Label();
			this.lblTongDaNhan = new System.Windows.Forms.Label();
			this.lblKhachHangChon = new System.Windows.Forms.Label();
			this.dgvTietKiem = new System.Windows.Forms.DataGridView();
			this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSDT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTongTienGui = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSoTienNhan = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colLaiSuat = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colKyHan = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNgayLam = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNgayCapNhat = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.cmsTietKiem = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiMoSo = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiTatToan = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiRutTien = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiDongTK = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiKhoaMoTK = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiXemLichSu = new System.Windows.Forms.ToolStripMenuItem();
			this.panelHeader.SuspendLayout();
			this.panelSearch.SuspendLayout();
			this.panelFilter.SuspendLayout();
			this.panelSummary.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTietKiem)).BeginInit();
			this.cmsTietKiem.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelHeader
			// 
			this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			this.panelHeader.Controls.Add(this.lblTitle);
			this.panelHeader.Controls.Add(this.panelSearch);
			this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelHeader.Location = new System.Drawing.Point(0, 0);
			this.panelHeader.Name = "panelHeader";
			this.panelHeader.Size = new System.Drawing.Size(1280, 65);
			this.panelHeader.TabIndex = 0;
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Location = new System.Drawing.Point(16, 18);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(311, 37);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "QUẢN LÝ SỔ TIẾT KIỆM";
			// 
			// panelSearch
			// 
			this.panelSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.panelSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
			this.panelSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSearch.Controls.Add(this.lblCCCD);
			this.panelSearch.Controls.Add(this.txtCCCD);
			this.panelSearch.Controls.Add(this.btnTimCCCD);
			this.panelSearch.Location = new System.Drawing.Point(780, 12);
			this.panelSearch.Name = "panelSearch";
			this.panelSearch.Size = new System.Drawing.Size(484, 40);
			this.panelSearch.TabIndex = 1;
			// 
			// lblCCCD
			// 
			this.lblCCCD.AutoSize = true;
			this.lblCCCD.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblCCCD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(174)))), ((int)(((byte)(192)))));
			this.lblCCCD.Location = new System.Drawing.Point(10, 12);
			this.lblCCCD.Name = "lblCCCD";
			this.lblCCCD.Size = new System.Drawing.Size(51, 20);
			this.lblCCCD.TabIndex = 0;
			this.lblCCCD.Text = "CCCD:";
			// 
			// txtCCCD
			// 
			this.txtCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
			this.txtCCCD.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtCCCD.Font = new System.Drawing.Font("Segoe UI", 11F);
			this.txtCCCD.ForeColor = System.Drawing.Color.White;
			this.txtCCCD.Location = new System.Drawing.Point(60, 10);
			this.txtCCCD.MaxLength = 12;
			this.txtCCCD.Name = "txtCCCD";
			this.txtCCCD.Size = new System.Drawing.Size(280, 25);
			this.txtCCCD.TabIndex = 1;
			// 
			// btnTimCCCD
			// 
			this.btnTimCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			this.btnTimCCCD.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnTimCCCD.FlatAppearance.BorderSize = 0;
			this.btnTimCCCD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTimCCCD.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnTimCCCD.ForeColor = System.Drawing.Color.White;
			this.btnTimCCCD.Location = new System.Drawing.Point(350, 4);
			this.btnTimCCCD.Name = "btnTimCCCD";
			this.btnTimCCCD.Size = new System.Drawing.Size(126, 30);
			this.btnTimCCCD.TabIndex = 2;
			this.btnTimCCCD.Text = "Tìm CCCD";
			this.btnTimCCCD.UseVisualStyleBackColor = false;
			// 
			// panelFilter
			// 
			this.panelFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
			this.panelFilter.Controls.Add(this.lblLocKyHan);
			this.panelFilter.Controls.Add(this.cboLocKyHan);
			this.panelFilter.Controls.Add(this.lblLocTrangThai);
			this.panelFilter.Controls.Add(this.cboLocTrangThai);
			this.panelFilter.Controls.Add(this.lblLocThoiGian);
			this.panelFilter.Controls.Add(this.cboLocThoiGian);
			this.panelFilter.Controls.Add(this.lblTuNgay);
			this.panelFilter.Controls.Add(this.dtpTuNgay);
			this.panelFilter.Controls.Add(this.lblDenNgay);
			this.panelFilter.Controls.Add(this.dtpDenNgay);
			this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelFilter.Location = new System.Drawing.Point(0, 65);
			this.panelFilter.Name = "panelFilter";
			this.panelFilter.Size = new System.Drawing.Size(1280, 50);
			this.panelFilter.TabIndex = 1;
			// 
			// lblLocKyHan
			// 
			this.lblLocKyHan.AutoSize = true;
			this.lblLocKyHan.ForeColor = System.Drawing.Color.White;
			this.lblLocKyHan.Location = new System.Drawing.Point(18, 17);
			this.lblLocKyHan.Name = "lblLocKyHan";
			this.lblLocKyHan.Size = new System.Drawing.Size(55, 20);
			this.lblLocKyHan.TabIndex = 0;
			this.lblLocKyHan.Text = "Kỳ hạn:";
			// 
			// cboLocKyHan
			// 
			this.cboLocKyHan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLocKyHan.FormattingEnabled = true;
			this.cboLocKyHan.Items.AddRange(new object[] {
			"Tất cả kỳ hạn",
			"3 tháng",
			"6 tháng",
			"12 tháng",
			"18 tháng",
			"24 tháng",
			"36 tháng"});
			this.cboLocKyHan.Location = new System.Drawing.Point(84, 13);
			this.cboLocKyHan.Name = "cboLocKyHan";
			this.cboLocKyHan.Size = new System.Drawing.Size(130, 28);
			this.cboLocKyHan.TabIndex = 1;
			// 
			// lblLocTrangThai
			// 
			this.lblLocTrangThai.AutoSize = true;
			this.lblLocTrangThai.ForeColor = System.Drawing.Color.White;
			this.lblLocTrangThai.Location = new System.Drawing.Point(220, 17);
			this.lblLocTrangThai.Name = "lblLocTrangThai";
			this.lblLocTrangThai.Size = new System.Drawing.Size(78, 20);
			this.lblLocTrangThai.TabIndex = 2;
			this.lblLocTrangThai.Text = "Trạng thái:";
			// 
			// cboLocTrangThai
			// 
			this.cboLocTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLocTrangThai.FormattingEnabled = true;
			this.cboLocTrangThai.Items.AddRange(new object[] {
			"Tất cả trạng thái",
			"Hoạt động",
			"Đang khóa",
			"Đã đóng"});
			this.cboLocTrangThai.Location = new System.Drawing.Point(304, 13);
			this.cboLocTrangThai.Name = "cboLocTrangThai";
			this.cboLocTrangThai.Size = new System.Drawing.Size(140, 28);
			this.cboLocTrangThai.TabIndex = 3;
			// 
			// lblLocThoiGian
			// 
			this.lblLocThoiGian.AutoSize = true;
			this.lblLocThoiGian.ForeColor = System.Drawing.Color.White;
			this.lblLocThoiGian.Location = new System.Drawing.Point(448, 17);
			this.lblLocThoiGian.Name = "lblLocThoiGian";
			this.lblLocThoiGian.Size = new System.Drawing.Size(74, 20);
			this.lblLocThoiGian.TabIndex = 4;
			this.lblLocThoiGian.Text = "Thời gian:";
			// 
			// cboLocThoiGian
			// 
			this.cboLocThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLocThoiGian.FormattingEnabled = true;
			this.cboLocThoiGian.Items.AddRange(new object[] {
			"Tất cả",
			"Hôm nay",
			"Hôm qua",
			"7 ngày",
			"1 tháng",
			"3 tháng",
			"1 năm",
			"Tùy chọn"});
			this.cboLocThoiGian.Location = new System.Drawing.Point(528, 14);
			this.cboLocThoiGian.Name = "cboLocThoiGian";
			this.cboLocThoiGian.Size = new System.Drawing.Size(120, 28);
			this.cboLocThoiGian.TabIndex = 5;
			// 
			// lblTuNgay
			// 
			this.lblTuNgay.AutoSize = true;
			this.lblTuNgay.ForeColor = System.Drawing.Color.White;
			this.lblTuNgay.Location = new System.Drawing.Point(687, 17);
			this.lblTuNgay.Name = "lblTuNgay";
			this.lblTuNgay.Size = new System.Drawing.Size(65, 20);
			this.lblTuNgay.TabIndex = 6;
			this.lblTuNgay.Text = "Từ ngày:";
			this.lblTuNgay.Visible = false;
			// 
			// dtpTuNgay
			// 
			this.dtpTuNgay.CustomFormat = "dd/MM/yyyy";
			this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpTuNgay.Location = new System.Drawing.Point(758, 14);
			this.dtpTuNgay.Name = "dtpTuNgay";
			this.dtpTuNgay.Size = new System.Drawing.Size(115, 27);
			this.dtpTuNgay.TabIndex = 7;
			this.dtpTuNgay.Visible = false;
			// 
			// lblDenNgay
			// 
			this.lblDenNgay.AutoSize = true;
			this.lblDenNgay.ForeColor = System.Drawing.Color.White;
			this.lblDenNgay.Location = new System.Drawing.Point(879, 17);
			this.lblDenNgay.Name = "lblDenNgay";
			this.lblDenNgay.Size = new System.Drawing.Size(75, 20);
			this.lblDenNgay.TabIndex = 8;
			this.lblDenNgay.Text = "Đến ngày:";
			this.lblDenNgay.Visible = false;
			// 
			// dtpDenNgay
			// 
			this.dtpDenNgay.CustomFormat = "dd/MM/yyyy";
			this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpDenNgay.Location = new System.Drawing.Point(970, 14);
			this.dtpDenNgay.Name = "dtpDenNgay";
			this.dtpDenNgay.Size = new System.Drawing.Size(115, 27);
			this.dtpDenNgay.TabIndex = 9;
			this.dtpDenNgay.Visible = false;
			// 
			// panelSummary
			// 
			this.panelSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			this.panelSummary.Controls.Add(this.lblTongTaiKhoan);
			this.panelSummary.Controls.Add(this.lblTongTienGui);
			this.panelSummary.Controls.Add(this.lblTongDaNhan);
			this.panelSummary.Controls.Add(this.lblKhachHangChon);
			this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelSummary.Location = new System.Drawing.Point(0, 115);
			this.panelSummary.Name = "panelSummary";
			this.panelSummary.Size = new System.Drawing.Size(1280, 50);
			this.panelSummary.TabIndex = 2;
			// 
			// lblTongTaiKhoan
			// 
			this.lblTongTaiKhoan.AutoSize = true;
			this.lblTongTaiKhoan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblTongTaiKhoan.ForeColor = System.Drawing.Color.White;
			this.lblTongTaiKhoan.Location = new System.Drawing.Point(16, 8);
			this.lblTongTaiKhoan.Name = "lblTongTaiKhoan";
			this.lblTongTaiKhoan.Size = new System.Drawing.Size(131, 20);
			this.lblTongTaiKhoan.TabIndex = 0;
			this.lblTongTaiKhoan.Text = "Tổng tài khoản: 0";
			// 
			// lblTongTienGui
			// 
			this.lblTongTienGui.AutoSize = true;
			this.lblTongTienGui.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblTongTienGui.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
			this.lblTongTienGui.Location = new System.Drawing.Point(220, 8);
			this.lblTongTienGui.Name = "lblTongTienGui";
			this.lblTongTienGui.Size = new System.Drawing.Size(157, 20);
			this.lblTongTienGui.TabIndex = 1;
			this.lblTongTienGui.Text = "Tổng gốc còn lại: 0 VNĐ";
			// 
			// lblTongDaNhan
			// 
			this.lblTongDaNhan.AutoSize = true;
			this.lblTongDaNhan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblTongDaNhan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(146)))), ((int)(((byte)(60)))));
			this.lblTongDaNhan.Location = new System.Drawing.Point(436, 8);
			this.lblTongDaNhan.Name = "lblTongDaNhan";
			this.lblTongDaNhan.Size = new System.Drawing.Size(159, 20);
			this.lblTongDaNhan.TabIndex = 2;
			this.lblTongDaNhan.Text = "Tổng tiền đã nhận: 0 VNĐ";
			// 
			// lblKhachHangChon
			// 
			this.lblKhachHangChon.AutoSize = true;
			this.lblKhachHangChon.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblKhachHangChon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
			this.lblKhachHangChon.Location = new System.Drawing.Point(16, 28);
			this.lblKhachHangChon.Name = "lblKhachHangChon";
			this.lblKhachHangChon.Size = new System.Drawing.Size(560, 20);
			this.lblKhachHangChon.TabIndex = 3;
			this.lblKhachHangChon.Text = "Đã chọn KH: [Chưa chọn] | CCCD: - | SĐT: - | Gốc còn lại: 0 VNĐ | Đã nhận: 0 VNĐ";
			// 
			// dgvTietKiem
			// 
			this.dgvTietKiem.AllowUserToAddRows = false;
			this.dgvTietKiem.AllowUserToDeleteRows = false;
			this.dgvTietKiem.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvTietKiem.BackgroundColor = System.Drawing.Color.White;
			this.dgvTietKiem.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvTietKiem.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
			this.dgvTietKiem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvTietKiem.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colSTT,
			this.colSTK,
			this.colCCCD,
			this.colHoTen,
			this.colSDT,
			this.colTongTienGui,
			this.colSoTienNhan,
			this.colLaiSuat,
			this.colKyHan,
			this.colNgayLam,
			this.colNgayCapNhat,
			this.colVaiTro,
			this.colTrangThai});
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
			dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvTietKiem.DefaultCellStyle = dataGridViewCellStyle5;
			this.dgvTietKiem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvTietKiem.EnableHeadersVisualStyles = false;
			this.dgvTietKiem.GridColor = System.Drawing.Color.Blue;
			this.dgvTietKiem.Location = new System.Drawing.Point(0, 165);
			this.dgvTietKiem.MultiSelect = false;
			this.dgvTietKiem.Name = "dgvTietKiem";
			this.dgvTietKiem.ReadOnly = true;
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvTietKiem.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
			this.dgvTietKiem.RowHeadersVisible = false;
			this.dgvTietKiem.RowHeadersWidth = 51;
			this.dgvTietKiem.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvTietKiem.Size = new System.Drawing.Size(1280, 555);
			this.dgvTietKiem.TabIndex = 3;
			// 
			// colSTT
			// 
			this.colSTT.FillWeight = 40F;
			this.colSTT.HeaderText = "STT";
			this.colSTT.MinimumWidth = 6;
			this.colSTT.Name = "colSTT";
			this.colSTT.ReadOnly = true;
			// 
			// colSTK
			// 
			this.colSTK.FillWeight = 85F;
			this.colSTK.HeaderText = "STK";
			this.colSTK.MinimumWidth = 6;
			this.colSTK.Name = "colSTK";
			this.colSTK.ReadOnly = true;
			this.colSTK.ToolTipText = "Nhấp đúp vào sổ, chọn chủ/đồng sở hữu trong bảng chi tiết, rồi nhấn Vay bằng sổ tiết kiệm.";
			// 
			// colCCCD
			// 
			this.colCCCD.HeaderText = "CCCD";
			this.colCCCD.MinimumWidth = 6;
			this.colCCCD.Name = "colCCCD";
			this.colCCCD.ReadOnly = true;
			// 
			// colHoTen
			// 
			this.colHoTen.FillWeight = 120F;
			this.colHoTen.HeaderText = "Họ Tên";
			this.colHoTen.MinimumWidth = 6;
			this.colHoTen.Name = "colHoTen";
			this.colHoTen.ReadOnly = true;
			// 
			// colSDT
			// 
			this.colSDT.FillWeight = 90F;
			this.colSDT.HeaderText = "SĐT";
			this.colSDT.MinimumWidth = 6;
			this.colSDT.Name = "colSDT";
			this.colSDT.ReadOnly = true;
			// 
			// colTongTienGui
			// 
			this.colTongTienGui.FillWeight = 110F;
			this.colTongTienGui.HeaderText = "Tổng gốc còn lại";
			this.colTongTienGui.MinimumWidth = 6;
			this.colTongTienGui.Name = "colTongTienGui";
			this.colTongTienGui.ReadOnly = true;
			// 
			// colSoTienNhan
			// 
			this.colSoTienNhan.FillWeight = 110F;
			this.colSoTienNhan.HeaderText = "Tổng tiền đã nhận";
			this.colSoTienNhan.MinimumWidth = 6;
			this.colSoTienNhan.Name = "colSoTienNhan";
			this.colSoTienNhan.ReadOnly = true;
			// 
			// colLaiSuat
			// 
			this.colLaiSuat.FillWeight = 75F;
			this.colLaiSuat.HeaderText = "Lãi suất";
			this.colLaiSuat.MinimumWidth = 6;
			this.colLaiSuat.Name = "colLaiSuat";
			this.colLaiSuat.ReadOnly = true;
			// 
			// colKyHan
			// 
			this.colKyHan.FillWeight = 80F;
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
			// colNgayCapNhat
			// 
			this.colNgayCapNhat.FillWeight = 110F;
			this.colNgayCapNhat.HeaderText = "Ngày cập nhật TT";
			this.colNgayCapNhat.MinimumWidth = 6;
			this.colNgayCapNhat.Name = "colNgayCapNhat";
			this.colNgayCapNhat.ReadOnly = true;
			// 
			// colVaiTro
			// 
			this.colVaiTro.FillWeight = 70F;
			this.colVaiTro.HeaderText = "Vai trò";
			this.colVaiTro.MinimumWidth = 6;
			this.colVaiTro.Name = "colVaiTro";
			this.colVaiTro.ReadOnly = true;
			// 
			// colTrangThai
			// 
			this.colTrangThai.FillWeight = 85F;
			this.colTrangThai.HeaderText = "Trạng thái";
			this.colTrangThai.MinimumWidth = 6;
			this.colTrangThai.Name = "colTrangThai";
			this.colTrangThai.ReadOnly = true;
			// 
			// cmsTietKiem
			// 
			this.cmsTietKiem.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.cmsTietKiem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsmiMoSo,
			this.tsmiTatToan,
			this.tsmiRutTien,
			this.tsmiDongTK,
			this.tsmiKhoaMoTK,
			this.tsmiXemLichSu});
			this.cmsTietKiem.Name = "cmsTietKiem";
			this.cmsTietKiem.Size = new System.Drawing.Size(288, 148);
			// 
			// tsmiMoSo
			// 
			this.tsmiMoSo.Name = "tsmiMoSo";
			this.tsmiMoSo.Size = new System.Drawing.Size(287, 24);
			this.tsmiMoSo.Text = "+ Mở sổ tiết kiệm";
			// 
			// tsmiTatToan
			// 
			this.tsmiTatToan.Name = "tsmiTatToan";
			this.tsmiTatToan.Size = new System.Drawing.Size(287, 24);
			this.tsmiTatToan.Text = "+ Tất toán";
			// 
			// tsmiRutTien
			// 
			this.tsmiRutTien.Name = "tsmiRutTien";
			this.tsmiRutTien.Size = new System.Drawing.Size(287, 24);
			this.tsmiRutTien.Text = "+ Rút tiền";
			// 
			// tsmiDongTK
			// 
			this.tsmiDongTK.Name = "tsmiDongTK";
			this.tsmiDongTK.Size = new System.Drawing.Size(287, 24);
			this.tsmiDongTK.Text = "+ Đóng tài khoản";
			// 
			// tsmiKhoaMoTK
			// 
			this.tsmiKhoaMoTK.Name = "tsmiKhoaMoTK";
			this.tsmiKhoaMoTK.Size = new System.Drawing.Size(287, 24);
			this.tsmiKhoaMoTK.Text = "+ Khóa tạm thời / Mở tài khoản";
			// 
			// tsmiXemLichSu
			// 
			this.tsmiXemLichSu.Name = "tsmiXemLichSu";
			this.tsmiXemLichSu.Size = new System.Drawing.Size(287, 24);
			this.tsmiXemLichSu.Text = "+ Xem lịch sử nhận tiền";
			// 
			// FrmTietKiem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.ClientSize = new System.Drawing.Size(1280, 720);
			this.Controls.Add(this.dgvTietKiem);
			this.Controls.Add(this.panelSummary);
			this.Controls.Add(this.panelFilter);
			this.Controls.Add(this.panelHeader);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "FrmTietKiem";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Quản Lý Sổ Tiết Kiệm";
			this.panelHeader.ResumeLayout(false);
			this.panelHeader.PerformLayout();
			this.panelSearch.ResumeLayout(false);
			this.panelSearch.PerformLayout();
			this.panelFilter.ResumeLayout(false);
			this.panelFilter.PerformLayout();
			this.panelSummary.ResumeLayout(false);
			this.panelSummary.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTietKiem)).EndInit();
			this.cmsTietKiem.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelHeader;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Panel panelSearch;
		private System.Windows.Forms.Label lblCCCD;
		private System.Windows.Forms.TextBox txtCCCD;
		private System.Windows.Forms.Button btnTimCCCD;

		private System.Windows.Forms.Panel panelFilter;
		private System.Windows.Forms.Label lblLocKyHan;
		private System.Windows.Forms.ComboBox cboLocKyHan;
		private System.Windows.Forms.Label lblLocTrangThai;
		private System.Windows.Forms.ComboBox cboLocTrangThai;
		private System.Windows.Forms.Label lblLocThoiGian;
		private System.Windows.Forms.ComboBox cboLocThoiGian;
		private System.Windows.Forms.Label lblTuNgay;
		private System.Windows.Forms.DateTimePicker dtpTuNgay;
		private System.Windows.Forms.Label lblDenNgay;
		private System.Windows.Forms.DateTimePicker dtpDenNgay;

		private System.Windows.Forms.Panel panelSummary;
		private System.Windows.Forms.Label lblTongTaiKhoan;
		private System.Windows.Forms.Label lblTongTienGui;
		private System.Windows.Forms.Label lblTongDaNhan;
		private System.Windows.Forms.Label lblKhachHangChon;

		private System.Windows.Forms.DataGridView dgvTietKiem;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCCCD;
		private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSDT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTongTienGui;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSoTienNhan;
		private System.Windows.Forms.DataGridViewTextBoxColumn colLaiSuat;
		private System.Windows.Forms.DataGridViewTextBoxColumn colKyHan;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNgayLam;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNgayCapNhat;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVaiTro;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;

		private System.Windows.Forms.ContextMenuStrip cmsTietKiem;
		private System.Windows.Forms.ToolStripMenuItem tsmiMoSo;
		private System.Windows.Forms.ToolStripMenuItem tsmiTatToan;
		private System.Windows.Forms.ToolStripMenuItem tsmiRutTien;
		private System.Windows.Forms.ToolStripMenuItem tsmiDongTK;
		private System.Windows.Forms.ToolStripMenuItem tsmiKhoaMoTK;
		private System.Windows.Forms.ToolStripMenuItem tsmiXemLichSu;
	}
}