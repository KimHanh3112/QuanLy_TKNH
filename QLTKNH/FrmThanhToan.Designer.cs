namespace QLTKNH
{
	partial class FrmThanhToan
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
			this.pnlTop = new System.Windows.Forms.Panel();
			this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
			this.lblDenNgay = new System.Windows.Forms.Label();
			this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
			this.lblTuNgay = new System.Windows.Forms.Label();
			this.cboLocThoiGian = new System.Windows.Forms.ComboBox();
			this.lblLocThoiGian = new System.Windows.Forms.Label();
			this.cboLocTrangThai = new System.Windows.Forms.ComboBox();
			this.lblLocTrangThai = new System.Windows.Forms.Label();
			this.btnTimCCCD = new System.Windows.Forms.Button();
			this.pnlSearch = new System.Windows.Forms.Panel();
			this.lblCCCD = new System.Windows.Forms.Label();
			this.txtCCCD = new System.Windows.Forms.TextBox();
			this.pnlOverview = new System.Windows.Forms.Panel();
			this.lblDaChonInfo = new System.Windows.Forms.Label();
			this.lblDaChonTitle = new System.Windows.Forms.Label();
			this.lblTongTaiKhoan = new System.Windows.Forms.Label();
			this.dgvThanhToan = new System.Windows.Forms.DataGridView();
			this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSDT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTongSoTien = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNgayLam = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNgayCapNhatTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.cmsThanhToan = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuMoThemTK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuDoiThongTinTK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuDangKyHanMuc = new System.Windows.Forms.ToolStripMenuItem();
			this.menuNapTien = new System.Windows.Forms.ToolStripMenuItem();
			this.menuRutTien = new System.Windows.Forms.ToolStripMenuItem();
			this.menuChuyenTien = new System.Windows.Forms.ToolStripMenuItem();
			this.menuKhoaMoKhanCap = new System.Windows.Forms.ToolStripMenuItem();
			this.menuPhongToa = new System.Windows.Forms.ToolStripMenuItem();
			this.menuDongTK = new System.Windows.Forms.ToolStripMenuItem();
			this.menuDangKyDichVu = new System.Windows.Forms.ToolStripMenuItem();
			this.menuXemLichSu = new System.Windows.Forms.ToolStripMenuItem();
			this.menuInSaoKe = new System.Windows.Forms.ToolStripMenuItem();
			this.pnlTop.SuspendLayout();
			this.pnlSearch.SuspendLayout();
			this.pnlOverview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvThanhToan)).BeginInit();
			this.cmsThanhToan.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlTop
			// 
			this.pnlTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.pnlTop.Controls.Add(this.dtpDenNgay);
			this.pnlTop.Controls.Add(this.lblDenNgay);
			this.pnlTop.Controls.Add(this.dtpTuNgay);
			this.pnlTop.Controls.Add(this.lblTuNgay);
			this.pnlTop.Controls.Add(this.cboLocThoiGian);
			this.pnlTop.Controls.Add(this.lblLocThoiGian);
			this.pnlTop.Controls.Add(this.cboLocTrangThai);
			this.pnlTop.Controls.Add(this.lblLocTrangThai);
			this.pnlTop.Controls.Add(this.btnTimCCCD);
			this.pnlTop.Controls.Add(this.pnlSearch);
			this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlTop.Location = new System.Drawing.Point(0, 0);
			this.pnlTop.Name = "pnlTop";
			this.pnlTop.Size = new System.Drawing.Size(1200, 70);
			this.pnlTop.TabIndex = 0;
			// 
			// dtpDenNgay
			// 
			this.dtpDenNgay.CustomFormat = "dd/MM/yyyy";
			this.dtpDenNgay.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpDenNgay.Location = new System.Drawing.Point(992, 20);
			this.dtpDenNgay.Name = "dtpDenNgay";
			this.dtpDenNgay.Size = new System.Drawing.Size(110, 27);
			this.dtpDenNgay.TabIndex = 5;
			this.dtpDenNgay.Visible = false;
			// 
			// lblDenNgay
			// 
			this.lblDenNgay.AutoSize = true;
			this.lblDenNgay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
			this.lblDenNgay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
			this.lblDenNgay.Location = new System.Drawing.Point(950, 24);
			this.lblDenNgay.Name = "lblDenNgay";
			this.lblDenNgay.Size = new System.Drawing.Size(39, 20);
			this.lblDenNgay.TabIndex = 6;
			this.lblDenNgay.Text = "Đến:";
			this.lblDenNgay.Visible = false;
			// 
			// dtpTuNgay
			// 
			this.dtpTuNgay.CustomFormat = "dd/MM/yyyy";
			this.dtpTuNgay.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpTuNgay.Location = new System.Drawing.Point(830, 20);
			this.dtpTuNgay.Name = "dtpTuNgay";
			this.dtpTuNgay.Size = new System.Drawing.Size(110, 27);
			this.dtpTuNgay.TabIndex = 4;
			this.dtpTuNgay.Visible = false;
			// 
			// lblTuNgay
			// 
			this.lblTuNgay.AutoSize = true;
			this.lblTuNgay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
			this.lblTuNgay.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
			this.lblTuNgay.Location = new System.Drawing.Point(795, 24);
			this.lblTuNgay.Name = "lblTuNgay";
			this.lblTuNgay.Size = new System.Drawing.Size(29, 20);
			this.lblTuNgay.TabIndex = 7;
			this.lblTuNgay.Text = "Từ:";
			this.lblTuNgay.Visible = false;
			// 
			// cboLocThoiGian
			// 
			this.cboLocThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLocThoiGian.Font = new System.Drawing.Font("Segoe UI", 9.5F);
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
			this.cboLocThoiGian.Location = new System.Drawing.Point(665, 20);
			this.cboLocThoiGian.Name = "cboLocThoiGian";
			this.cboLocThoiGian.Size = new System.Drawing.Size(115, 29);
			this.cboLocThoiGian.TabIndex = 3;
			// 
			// lblLocThoiGian
			// 
			this.lblLocThoiGian.AutoSize = true;
			this.lblLocThoiGian.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblLocThoiGian.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
			this.lblLocThoiGian.Location = new System.Drawing.Point(585, 24);
			this.lblLocThoiGian.Name = "lblLocThoiGian";
			this.lblLocThoiGian.Size = new System.Drawing.Size(74, 20);
			this.lblLocThoiGian.TabIndex = 8;
			this.lblLocThoiGian.Text = "Thời gian:";
			// 
			// cboLocTrangThai
			// 
			this.cboLocTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLocTrangThai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
			this.cboLocTrangThai.FormattingEnabled = true;
			this.cboLocTrangThai.Items.AddRange(new object[] {
			"Tất cả",
			"Hoạt động",
			"Đang khóa",
			"Phong tỏa",
			"Đã đóng"});
			this.cboLocTrangThai.Location = new System.Drawing.Point(448, 20);
			this.cboLocTrangThai.Name = "cboLocTrangThai";
			this.cboLocTrangThai.Size = new System.Drawing.Size(120, 29);
			this.cboLocTrangThai.TabIndex = 2;
			// 
			// lblLocTrangThai
			// 
			this.lblLocTrangThai.AutoSize = true;
			this.lblLocTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblLocTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
			this.lblLocTrangThai.Location = new System.Drawing.Point(365, 24);
			this.lblLocTrangThai.Name = "lblLocTrangThai";
			this.lblLocTrangThai.Size = new System.Drawing.Size(78, 20);
			this.lblLocTrangThai.TabIndex = 9;
			this.lblLocTrangThai.Text = "Trạng thái:";
			// 
			// btnTimCCCD
			// 
			this.btnTimCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			this.btnTimCCCD.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnTimCCCD.FlatAppearance.BorderSize = 0;
			this.btnTimCCCD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTimCCCD.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnTimCCCD.ForeColor = System.Drawing.Color.White;
			this.btnTimCCCD.Location = new System.Drawing.Point(245, 15);
			this.btnTimCCCD.Name = "btnTimCCCD";
			this.btnTimCCCD.Size = new System.Drawing.Size(100, 38);
			this.btnTimCCCD.TabIndex = 1;
			this.btnTimCCCD.Text = "Tìm CCCD";
			this.btnTimCCCD.UseVisualStyleBackColor = false;
			// 
			// pnlSearch
			// 
			this.pnlSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			this.pnlSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pnlSearch.Controls.Add(this.lblCCCD);
			this.pnlSearch.Controls.Add(this.txtCCCD);
			this.pnlSearch.Location = new System.Drawing.Point(16, 15);
			this.pnlSearch.Name = "pnlSearch";
			this.pnlSearch.Size = new System.Drawing.Size(220, 38);
			this.pnlSearch.TabIndex = 0;
			// 
			// lblCCCD
			// 
			this.lblCCCD.AutoSize = true;
			this.lblCCCD.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblCCCD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
			this.lblCCCD.Location = new System.Drawing.Point(8, 9);
			this.lblCCCD.Name = "lblCCCD";
			this.lblCCCD.Size = new System.Drawing.Size(50, 20);
			this.lblCCCD.TabIndex = 0;
			this.lblCCCD.Text = "CCCD:";
			// 
			// txtCCCD
			// 
			this.txtCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			this.txtCCCD.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtCCCD.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.txtCCCD.ForeColor = System.Drawing.Color.White;
			this.txtCCCD.Location = new System.Drawing.Point(57, 8);
			this.txtCCCD.MaxLength = 12;
			this.txtCCCD.Name = "txtCCCD";
			this.txtCCCD.Size = new System.Drawing.Size(150, 23);
			this.txtCCCD.TabIndex = 0;
			// 
			// pnlOverview
			// 
			this.pnlOverview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			this.pnlOverview.Controls.Add(this.lblDaChonInfo);
			this.pnlOverview.Controls.Add(this.lblDaChonTitle);
			this.pnlOverview.Controls.Add(this.lblTongTaiKhoan);
			this.pnlOverview.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlOverview.Location = new System.Drawing.Point(0, 70);
			this.pnlOverview.Name = "pnlOverview";
			this.pnlOverview.Size = new System.Drawing.Size(1200, 50);
			this.pnlOverview.TabIndex = 1;
			// 
			// lblDaChonInfo
			// 
			this.lblDaChonInfo.AutoSize = true;
			this.lblDaChonInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.lblDaChonInfo.ForeColor = System.Drawing.Color.White;
			this.lblDaChonInfo.Location = new System.Drawing.Point(170, 15);
			this.lblDaChonInfo.Name = "lblDaChonInfo";
			this.lblDaChonInfo.Size = new System.Drawing.Size(337, 20);
			this.lblDaChonInfo.TabIndex = 0;
			this.lblDaChonInfo.Text = "Chưa chọn | CCCD: - | SĐT: - | Số dư: 0 VNĐ";
			// 
			// lblDaChonTitle
			// 
			this.lblDaChonTitle.AutoSize = true;
			this.lblDaChonTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblDaChonTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(213)))), ((int)(((byte)(225)))));
			this.lblDaChonTitle.Location = new System.Drawing.Point(16, 15);
			this.lblDaChonTitle.Name = "lblDaChonTitle";
			this.lblDaChonTitle.Size = new System.Drawing.Size(146, 20);
			this.lblDaChonTitle.TabIndex = 1;
			this.lblDaChonTitle.Text = "Đã chọn khách hàng:";
			// 
			// lblTongTaiKhoan
			// 
			this.lblTongTaiKhoan.AutoSize = true;
			this.lblTongTaiKhoan.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblTongTaiKhoan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(189)))), ((int)(((byte)(248)))));
			this.lblTongTaiKhoan.Location = new System.Drawing.Point(1000, 15);
			this.lblTongTaiKhoan.Name = "lblTongTaiKhoan";
			this.lblTongTaiKhoan.Size = new System.Drawing.Size(142, 21);
			this.lblTongTaiKhoan.TabIndex = 4;
			this.lblTongTaiKhoan.Text = "Tổng tài khoản: 0";
			// 
			// dgvThanhToan
			// 
			this.dgvThanhToan.AllowUserToAddRows = false;
			this.dgvThanhToan.AllowUserToDeleteRows = false;
			this.dgvThanhToan.AllowUserToResizeRows = false;
			this.dgvThanhToan.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvThanhToan.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.dgvThanhToan.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dgvThanhToan.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dgvThanhToan.ColumnHeadersHeight = 35;
			this.dgvThanhToan.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colSTT,
			this.colSTK,
			this.colCCCD,
			this.colHoTen,
			this.colSDT,
			this.colTongSoTien,
			this.colNgayLam,
			this.colNgayCapNhatTT,
			this.colVaiTro,
			this.colTrangThai});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvThanhToan.DefaultCellStyle = dataGridViewCellStyle2;
			this.dgvThanhToan.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvThanhToan.EnableHeadersVisualStyles = false;
			this.dgvThanhToan.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
			this.dgvThanhToan.Location = new System.Drawing.Point(0, 120);
			this.dgvThanhToan.MultiSelect = false;
			this.dgvThanhToan.Name = "dgvThanhToan";
			this.dgvThanhToan.ReadOnly = true;
			this.dgvThanhToan.RowHeadersVisible = false;
			this.dgvThanhToan.RowHeadersWidth = 51;
			this.dgvThanhToan.RowTemplate.Height = 32;
			this.dgvThanhToan.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvThanhToan.Size = new System.Drawing.Size(1200, 580);
			this.dgvThanhToan.TabIndex = 2;
			this.dgvThanhToan.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvThanhToan_CellContentClick);
			// 
			// colSTT
			// 
			this.colSTT.FillWeight = 35F;
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
			// 
			// colCCCD
			// 
			this.colCCCD.FillWeight = 95F;
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
			this.colSDT.FillWeight = 85F;
			this.colSDT.HeaderText = "SĐT";
			this.colSDT.MinimumWidth = 6;
			this.colSDT.Name = "colSDT";
			this.colSDT.ReadOnly = true;
			// 
			// colTongSoTien
			// 
			this.colTongSoTien.HeaderText = "Số dư";
			this.colTongSoTien.MinimumWidth = 6;
			this.colTongSoTien.Name = "colTongSoTien";
			this.colTongSoTien.ReadOnly = true;
			// 
			// colNgayLam
			// 
			this.colNgayLam.FillWeight = 110F;
			this.colNgayLam.HeaderText = "Ngày mở";
			this.colNgayLam.MinimumWidth = 6;
			this.colNgayLam.Name = "colNgayLam";
			this.colNgayLam.ReadOnly = true;
			// 
			// colNgayCapNhatTT
			// 
			this.colNgayCapNhatTT.FillWeight = 110F;
			this.colNgayCapNhatTT.HeaderText = "Ngày cập nhật";
			this.colNgayCapNhatTT.MinimumWidth = 6;
			this.colNgayCapNhatTT.Name = "colNgayCapNhatTT";
			this.colNgayCapNhatTT.ReadOnly = true;
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
			this.colTrangThai.FillWeight = 80F;
			this.colTrangThai.HeaderText = "Trạng thái";
			this.colTrangThai.MinimumWidth = 6;
			this.colTrangThai.Name = "colTrangThai";
			this.colTrangThai.ReadOnly = true;
			// 
			// cmsThanhToan
			// 
			this.cmsThanhToan.Font = new System.Drawing.Font("Segoe UI", 9.5F);
			this.cmsThanhToan.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.cmsThanhToan.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.menuMoThemTK});
			this.cmsThanhToan.Name = "cmsThanhToan";
			this.cmsThanhToan.Size = new System.Drawing.Size(280, 320);
			// 
			// menuMoThemTK
			// 
			this.menuMoThemTK.Name = "menuMoThemTK";
			this.menuMoThemTK.Size = new System.Drawing.Size(279, 26);
			this.menuMoThemTK.Text = "+ Mở thêm tài khoản";
			// 
			// menuDoiThongTinTK
			// 
			this.menuDoiThongTinTK.Name = "menuDoiThongTinTK";
			this.menuDoiThongTinTK.Size = new System.Drawing.Size(279, 26);
			this.menuDoiThongTinTK.Text = "+ Thay đổi thông tin TK";
			// 
			// menuDangKyHanMuc
			// 
			this.menuDangKyHanMuc.Name = "menuDangKyHanMuc";
			this.menuDangKyHanMuc.Size = new System.Drawing.Size(279, 26);
			this.menuDangKyHanMuc.Text = "+ Đăng ký hạn mức GD";
			// 
			// menuNapTien
			// 
			this.menuNapTien.Name = "menuNapTien";
			this.menuNapTien.Size = new System.Drawing.Size(279, 26);
			this.menuNapTien.Text = "+ Nộp tiền mặt";
			// 
			// menuRutTien
			// 
			this.menuRutTien.Name = "menuRutTien";
			this.menuRutTien.Size = new System.Drawing.Size(279, 26);
			this.menuRutTien.Text = "+ Rút tiền mặt";
			// 
			// menuChuyenTien
			// 
			this.menuChuyenTien.Name = "menuChuyenTien";
			this.menuChuyenTien.Size = new System.Drawing.Size(279, 26);
			this.menuChuyenTien.Text = "+ Chuyển khoản";
			// 
			// menuKhoaMoKhanCap
			// 
			this.menuKhoaMoKhanCap.Name = "menuKhoaMoKhanCap";
			this.menuKhoaMoKhanCap.Size = new System.Drawing.Size(279, 26);
			this.menuKhoaMoKhanCap.Text = "+ Tạm khóa / Mở khóa";
			// 
			// menuPhongToa
			// 
			this.menuPhongToa.Name = "menuPhongToa";
			this.menuPhongToa.Size = new System.Drawing.Size(279, 26);
			this.menuPhongToa.Text = "+ Phong tỏa / Gỡ phong tỏa";
			// 
			// menuDongTK
			// 
			this.menuDongTK.Name = "menuDongTK";
			this.menuDongTK.Size = new System.Drawing.Size(279, 26);
			this.menuDongTK.Text = "+ Tất toán / Đóng tài khoản";
			// 
			// menuDangKyDichVu
			// 
			this.menuDangKyDichVu.Name = "menuDangKyDichVu";
			this.menuDangKyDichVu.Size = new System.Drawing.Size(279, 26);
			this.menuDangKyDichVu.Text = "+ Đăng ký dịch vụ đi kèm";
			// 
			// menuXemLichSu
			// 
			this.menuXemLichSu.Name = "menuXemLichSu";
			this.menuXemLichSu.Size = new System.Drawing.Size(279, 26);
			this.menuXemLichSu.Text = "+ Xem lịch sử giao dịch";
			// 
			// menuInSaoKe
			// 
			this.menuInSaoKe.Name = "menuInSaoKe";
			this.menuInSaoKe.Size = new System.Drawing.Size(279, 26);
			this.menuInSaoKe.Text = "+ In sao kê (mộc đỏ)";
			// 
			// FrmThanhToan
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.ClientSize = new System.Drawing.Size(1200, 700);
			this.Controls.Add(this.dgvThanhToan);
			this.Controls.Add(this.pnlOverview);
			this.Controls.Add(this.pnlTop);
			this.Name = "FrmThanhToan";
			this.Text = "Quản Lý Tài Khoản Thanh Toán";
			this.pnlTop.ResumeLayout(false);
			this.pnlTop.PerformLayout();
			this.pnlSearch.ResumeLayout(false);
			this.pnlSearch.PerformLayout();
			this.pnlOverview.ResumeLayout(false);
			this.pnlOverview.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvThanhToan)).EndInit();
			this.cmsThanhToan.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel pnlTop;
		private System.Windows.Forms.Panel pnlSearch;
		private System.Windows.Forms.Label lblCCCD;
		private System.Windows.Forms.TextBox txtCCCD;
		private System.Windows.Forms.Button btnTimCCCD;
		private System.Windows.Forms.Label lblLocTrangThai;
		private System.Windows.Forms.ComboBox cboLocTrangThai;
		private System.Windows.Forms.Label lblLocThoiGian;
		private System.Windows.Forms.ComboBox cboLocThoiGian;
		private System.Windows.Forms.Label lblTuNgay;
		private System.Windows.Forms.DateTimePicker dtpTuNgay;
		private System.Windows.Forms.Label lblDenNgay;
		private System.Windows.Forms.DateTimePicker dtpDenNgay;
		private System.Windows.Forms.Panel pnlOverview;
		private System.Windows.Forms.Label lblTongTaiKhoan;
		private System.Windows.Forms.Label lblDaChonTitle;
		private System.Windows.Forms.Label lblDaChonInfo;
		private System.Windows.Forms.DataGridView dgvThanhToan;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCCCD;
		private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSDT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTongSoTien;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNgayLam;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNgayCapNhatTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVaiTro;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;
		private System.Windows.Forms.ContextMenuStrip cmsThanhToan;
		private System.Windows.Forms.ToolStripMenuItem menuMoThemTK;
		private System.Windows.Forms.ToolStripMenuItem menuDoiThongTinTK;
		private System.Windows.Forms.ToolStripMenuItem menuDangKyHanMuc;
		private System.Windows.Forms.ToolStripMenuItem menuNapTien;
		private System.Windows.Forms.ToolStripMenuItem menuRutTien;
		private System.Windows.Forms.ToolStripMenuItem menuChuyenTien;
		private System.Windows.Forms.ToolStripMenuItem menuKhoaMoKhanCap;
		private System.Windows.Forms.ToolStripMenuItem menuPhongToa;
		private System.Windows.Forms.ToolStripMenuItem menuDongTK;
		private System.Windows.Forms.ToolStripMenuItem menuDangKyDichVu;
		private System.Windows.Forms.ToolStripMenuItem menuXemLichSu;
		private System.Windows.Forms.ToolStripMenuItem menuInSaoKe;
	}
}