namespace QLTKNH
{
    partial class FrmKhachHang
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;

        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Label lblCCCD;
        private System.Windows.Forms.TextBox txtCCCD;
        private System.Windows.Forms.Button btnTimCCCD;

        // PANEL BỘ LỌC
        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.Label lblFilterLoai;
        private System.Windows.Forms.ComboBox cboLocLoaiTK;

        private System.Windows.Forms.Label lblFilterTrangThai;
        private System.Windows.Forms.ComboBox cboLocTrangThai;

        private System.Windows.Forms.Label lblFilterNhomNo;
        private System.Windows.Forms.ComboBox cboLocNhomNo;

        private System.Windows.Forms.Label lblFilterThoiGian;
        private System.Windows.Forms.ComboBox cboLocThoiGian;
        private System.Windows.Forms.Label lblTuNgay;
        private System.Windows.Forms.DateTimePicker dtpTuNgay;
        private System.Windows.Forms.Label lblDenNgay;
        private System.Windows.Forms.DateTimePicker dtpDenNgay;

        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblTrangThai;
        private System.Windows.Forms.Label lblSoKetQua;
        private System.Windows.Forms.Label lblTongTaiKhoan;

        private System.Windows.Forms.Panel panelGrid;
        private System.Windows.Forms.Label lblGridTitle;
        private System.Windows.Forms.DataGridView dgvKhachHang;

        private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSTK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCCCD;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSDT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLoaiTaiKhoan;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoDuDuNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDaTraDaNhan;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKyHan;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLaiSuat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNhomNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNgayLam;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVaiTro;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblKhachHangDangChon;
        private System.Windows.Forms.Label lblChiTiet;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelSearch = new System.Windows.Forms.Panel();
            this.btnTimCCCD = new System.Windows.Forms.Button();
            this.txtCCCD = new System.Windows.Forms.TextBox();
            this.lblCCCD = new System.Windows.Forms.Label();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
            this.lblDenNgay = new System.Windows.Forms.Label();
            this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
            this.lblTuNgay = new System.Windows.Forms.Label();
            this.cboLocThoiGian = new System.Windows.Forms.ComboBox();
            this.lblFilterThoiGian = new System.Windows.Forms.Label();
            this.cboLocNhomNo = new System.Windows.Forms.ComboBox();
            this.lblFilterNhomNo = new System.Windows.Forms.Label();
            this.cboLocTrangThai = new System.Windows.Forms.ComboBox();
            this.lblFilterTrangThai = new System.Windows.Forms.Label();
            this.cboLocLoaiTK = new System.Windows.Forms.ComboBox();
            this.lblFilterLoai = new System.Windows.Forms.Label();
            this.panelInfo = new System.Windows.Forms.Panel();
            this.lblTongTaiKhoan = new System.Windows.Forms.Label();
            this.lblSoKetQua = new System.Windows.Forms.Label();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.dgvKhachHang = new System.Windows.Forms.DataGridView();
            this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSDT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLoaiTaiKhoan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoDuDuNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDaTraDaNhan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKyHan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLaiSuat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNhomNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNgayLam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblGridTitle = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblChiTiet = new System.Windows.Forms.Label();
            this.lblKhachHangDangChon = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.panelSearch.SuspendLayout();
            this.panelFilter.SuspendLayout();
            this.panelInfo.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKhachHang)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.White;
            this.panelHeader.Controls.Add(this.lblSubTitle);
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Padding = new System.Windows.Forms.Padding(27, 15, 27, 11);
            this.panelHeader.Size = new System.Drawing.Size(1442, 83);
            this.panelHeader.TabIndex = 0;
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblSubTitle.Location = new System.Drawing.Point(31, 50);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(412, 20);
            this.lblSubTitle.TabIndex = 0;
            this.lblSubTitle.Text = "Tra cứu thông tin khách hàng và các tài khoản đang đứng tên";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblTitle.Location = new System.Drawing.Point(27, 13);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(408, 41);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "👤 QUẢN LÝ KHÁCH HÀNG";
            // 
            // panelSearch
            // 
            this.panelSearch.BackColor = System.Drawing.Color.White;
            this.panelSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSearch.Controls.Add(this.btnTimCCCD);
            this.panelSearch.Controls.Add(this.txtCCCD);
            this.panelSearch.Controls.Add(this.lblCCCD);
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearch.Location = new System.Drawing.Point(0, 83);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Padding = new System.Windows.Forms.Padding(27, 14, 27, 14);
            this.panelSearch.Size = new System.Drawing.Size(1442, 69);
            this.panelSearch.TabIndex = 1;
            // 
            // btnTimCCCD
            // 
            this.btnTimCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTimCCCD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTimCCCD.FlatAppearance.BorderSize = 0;
            this.btnTimCCCD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTimCCCD.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnTimCCCD.ForeColor = System.Drawing.Color.White;
            this.btnTimCCCD.Location = new System.Drawing.Point(589, 16);
            this.btnTimCCCD.Name = "btnTimCCCD";
            this.btnTimCCCD.Size = new System.Drawing.Size(109, 31);
            this.btnTimCCCD.TabIndex = 2;
            this.btnTimCCCD.Text = "🔍 Tìm";
            this.btnTimCCCD.UseVisualStyleBackColor = false;
            // 
            // txtCCCD
            // 
            this.txtCCCD.BackColor = System.Drawing.Color.White;
            this.txtCCCD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCCCD.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtCCCD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtCCCD.Location = new System.Drawing.Point(200, 17);
            this.txtCCCD.MaxLength = 12;
            this.txtCCCD.Name = "txtCCCD";
            this.txtCCCD.Size = new System.Drawing.Size(377, 32);
            this.txtCCCD.TabIndex = 1;
            // 
            // lblCCCD
            // 
            this.lblCCCD.AutoSize = true;
            this.lblCCCD.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCCCD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblCCCD.Location = new System.Drawing.Point(27, 21);
            this.lblCCCD.Name = "lblCCCD";
            this.lblCCCD.Size = new System.Drawing.Size(165, 23);
            this.lblCCCD.TabIndex = 3;
            this.lblCCCD.Text = "🔍 Tìm theo CCCD:";
            // 
            // panelFilter
            // 
            this.panelFilter.BackColor = System.Drawing.Color.White;
            this.panelFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFilter.Controls.Add(this.dtpDenNgay);
            this.panelFilter.Controls.Add(this.lblDenNgay);
            this.panelFilter.Controls.Add(this.dtpTuNgay);
            this.panelFilter.Controls.Add(this.cboLocThoiGian);
            this.panelFilter.Controls.Add(this.lblTuNgay);
            this.panelFilter.Controls.Add(this.lblFilterThoiGian);
            this.panelFilter.Controls.Add(this.cboLocNhomNo);
            this.panelFilter.Controls.Add(this.lblFilterNhomNo);
            this.panelFilter.Controls.Add(this.cboLocTrangThai);
            this.panelFilter.Controls.Add(this.lblFilterTrangThai);
            this.panelFilter.Controls.Add(this.cboLocLoaiTK);
            this.panelFilter.Controls.Add(this.lblFilterLoai);
            this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilter.Location = new System.Drawing.Point(0, 152);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Size = new System.Drawing.Size(1442, 55);
            this.panelFilter.TabIndex = 2;
            // 
            // dtpDenNgay
            // 
            this.dtpDenNgay.CustomFormat = "dd/MM/yyyy";
            this.dtpDenNgay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpDenNgay.Location = new System.Drawing.Point(1184, 13);
            this.dtpDenNgay.Name = "dtpDenNgay";
            this.dtpDenNgay.Size = new System.Drawing.Size(108, 26);
            this.dtpDenNgay.TabIndex = 0;
            this.dtpDenNgay.Visible = false;
            // 
            // lblDenNgay
            // 
            this.lblDenNgay.AutoSize = true;
            this.lblDenNgay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblDenNgay.Location = new System.Drawing.Point(1152, 15);
            this.lblDenNgay.Name = "lblDenNgay";
            this.lblDenNgay.Size = new System.Drawing.Size(15, 20);
            this.lblDenNgay.TabIndex = 1;
            this.lblDenNgay.Text = "-";
            this.lblDenNgay.Visible = false;
            // 
            // dtpTuNgay
            // 
            this.dtpTuNgay.CustomFormat = "dd/MM/yyyy";
            this.dtpTuNgay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpTuNgay.Location = new System.Drawing.Point(1027, 13);
            this.dtpTuNgay.Name = "dtpTuNgay";
            this.dtpTuNgay.Size = new System.Drawing.Size(108, 26);
            this.dtpTuNgay.TabIndex = 2;
            this.dtpTuNgay.Visible = false;
            // 
            // lblTuNgay
            // 
            this.lblTuNgay.AutoSize = true;
            this.lblTuNgay.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTuNgay.Location = new System.Drawing.Point(903, 18);
            this.lblTuNgay.Name = "lblTuNgay";
            this.lblTuNgay.Size = new System.Drawing.Size(29, 20);
            this.lblTuNgay.TabIndex = 3;
            this.lblTuNgay.Text = "Từ:";
            this.lblTuNgay.Visible = false;
            // 
            // cboLocThoiGian
            // 
            this.cboLocThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocThoiGian.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboLocThoiGian.FormattingEnabled = true;
            this.cboLocThoiGian.Items.AddRange(new object[] {
            "Tất cả",
            "Hôm nay",
            "Hôm qua",
            "7 ngày qua",
            "30 ngày",
            "3 tháng",
            "1 năm",
            "Tùy chọn..."});
            this.cboLocThoiGian.Location = new System.Drawing.Point(839, 12);
            this.cboLocThoiGian.Name = "cboLocThoiGian";
            this.cboLocThoiGian.Size = new System.Drawing.Size(125, 28);
            this.cboLocThoiGian.TabIndex = 4;
            // 
            // lblFilterThoiGian
            // 
            this.lblFilterThoiGian.AutoSize = true;
            this.lblFilterThoiGian.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFilterThoiGian.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblFilterThoiGian.Location = new System.Drawing.Point(755, 15);
            this.lblFilterThoiGian.Name = "lblFilterThoiGian";
            this.lblFilterThoiGian.Size = new System.Drawing.Size(78, 20);
            this.lblFilterThoiGian.TabIndex = 5;
            this.lblFilterThoiGian.Text = "Thời gian:";
            // 
            // cboLocNhomNo
            // 
            this.cboLocNhomNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocNhomNo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboLocNhomNo.FormattingEnabled = true;
            this.cboLocNhomNo.Items.AddRange(new object[] {
            "Tất cả",
            "Nhóm 1 (Đủ tiêu chuẩn)",
            "Nhóm 2 (Cần chú ý)",
            "Nhóm 3 (Dưới tiêu chuẩn)",
            "Nhóm 4 (Nghi ngờ)",
            "Nhóm 5 (Mất vốn)"});
            this.cboLocNhomNo.Location = new System.Drawing.Point(564, 12);
            this.cboLocNhomNo.Name = "cboLocNhomNo";
            this.cboLocNhomNo.Size = new System.Drawing.Size(171, 28);
            this.cboLocNhomNo.TabIndex = 6;
            // 
            // lblFilterNhomNo
            // 
            this.lblFilterNhomNo.AutoSize = true;
            this.lblFilterNhomNo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFilterNhomNo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblFilterNhomNo.Location = new System.Drawing.Point(479, 17);
            this.lblFilterNhomNo.Name = "lblFilterNhomNo";
            this.lblFilterNhomNo.Size = new System.Drawing.Size(79, 20);
            this.lblFilterNhomNo.TabIndex = 7;
            this.lblFilterNhomNo.Text = "Nhóm nợ:";
            // 
            // cboLocTrangThai
            // 
            this.cboLocTrangThai.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboLocTrangThai.FormattingEnabled = true;
            this.cboLocTrangThai.Items.AddRange(new object[] {
            "Tất cả",
            "Hoạt động",
            "Đang khóa",
            "Đã đóng"});
            this.cboLocTrangThai.Location = new System.Drawing.Point(326, 12);
            this.cboLocTrangThai.Name = "cboLocTrangThai";
            this.cboLocTrangThai.Size = new System.Drawing.Size(131, 28);
            this.cboLocTrangThai.TabIndex = 8;
            // 
            // lblFilterTrangThai
            // 
            this.lblFilterTrangThai.AutoSize = true;
            this.lblFilterTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFilterTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblFilterTrangThai.Location = new System.Drawing.Point(236, 18);
            this.lblFilterTrangThai.Name = "lblFilterTrangThai";
            this.lblFilterTrangThai.Size = new System.Drawing.Size(84, 20);
            this.lblFilterTrangThai.TabIndex = 9;
            this.lblFilterTrangThai.Text = "Trạng thái:";
            // 
            // cboLocLoaiTK
            // 
            this.cboLocLoaiTK.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLocLoaiTK.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboLocLoaiTK.FormattingEnabled = true;
            this.cboLocLoaiTK.Items.AddRange(new object[] {
            "Tất cả",
            "Vay",
            "Tiết kiệm",
            "Thanh toán"});
            this.cboLocLoaiTK.Location = new System.Drawing.Point(94, 14);
            this.cboLocLoaiTK.Name = "cboLocLoaiTK";
            this.cboLocLoaiTK.Size = new System.Drawing.Size(119, 28);
            this.cboLocLoaiTK.TabIndex = 1;
            // 
            // lblFilterLoai
            // 
            this.lblFilterLoai.AutoSize = true;
            this.lblFilterLoai.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFilterLoai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblFilterLoai.Location = new System.Drawing.Point(23, 18);
            this.lblFilterLoai.Name = "lblFilterLoai";
            this.lblFilterLoai.Size = new System.Drawing.Size(65, 20);
            this.lblFilterLoai.TabIndex = 10;
            this.lblFilterLoai.Text = "Loại TK:";
            // 
            // panelInfo
            // 
            this.panelInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.panelInfo.Controls.Add(this.lblTongTaiKhoan);
            this.panelInfo.Controls.Add(this.lblSoKetQua);
            this.panelInfo.Controls.Add(this.lblTrangThai);
            this.panelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInfo.Location = new System.Drawing.Point(0, 207);
            this.panelInfo.Name = "panelInfo";
            this.panelInfo.Size = new System.Drawing.Size(1442, 37);
            this.panelInfo.TabIndex = 3;
            // 
            // lblTongTaiKhoan
            // 
            this.lblTongTaiKhoan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTongTaiKhoan.AutoSize = true;
            this.lblTongTaiKhoan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTongTaiKhoan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblTongTaiKhoan.Location = new System.Drawing.Point(1200, 10);
            this.lblTongTaiKhoan.Name = "lblTongTaiKhoan";
            this.lblTongTaiKhoan.Size = new System.Drawing.Size(131, 20);
            this.lblTongTaiKhoan.TabIndex = 0;
            this.lblTongTaiKhoan.Text = "Tổng tài khoản: 0";
            // 
            // lblSoKetQua
            // 
            this.lblSoKetQua.AutoSize = true;
            this.lblSoKetQua.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSoKetQua.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.lblSoKetQua.Location = new System.Drawing.Point(446, 10);
            this.lblSoKetQua.Name = "lblSoKetQua";
            this.lblSoKetQua.Size = new System.Drawing.Size(0, 20);
            this.lblSoKetQua.TabIndex = 1;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.AutoSize = true;
            this.lblTrangThai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.lblTrangThai.Location = new System.Drawing.Point(27, 10);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(0, 20);
            this.lblTrangThai.TabIndex = 2;
            // 
            // panelGrid
            // 
            this.panelGrid.BackColor = System.Drawing.Color.White;
            this.panelGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGrid.Controls.Add(this.dgvKhachHang);
            this.panelGrid.Controls.Add(this.lblGridTitle);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 244);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(16, 0, 16, 13);
            this.panelGrid.Size = new System.Drawing.Size(1442, 513);
            this.panelGrid.TabIndex = 4;
            // 
            // dgvKhachHang
            // 
            this.dgvKhachHang.AllowUserToAddRows = false;
            this.dgvKhachHang.AllowUserToDeleteRows = false;
            this.dgvKhachHang.AllowUserToResizeRows = false;
            this.dgvKhachHang.BackgroundColor = System.Drawing.Color.White;
            this.dgvKhachHang.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvKhachHang.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvKhachHang.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvKhachHang.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvKhachHang.ColumnHeadersHeight = 48;
            this.dgvKhachHang.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvKhachHang.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSTT,
            this.colSTK,
            this.colCCCD,
            this.colHoTen,
            this.colSDT,
            this.colLoaiTaiKhoan,
            this.colSoDuDuNo,
            this.colDaTraDaNhan,
            this.colKyHan,
            this.colLaiSuat,
            this.colNhomNo,
            this.colNgayLam,
            this.colVaiTro,
            this.colTrangThai});
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvKhachHang.DefaultCellStyle = dataGridViewCellStyle10;
            this.dgvKhachHang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKhachHang.EnableHeadersVisualStyles = false;
            this.dgvKhachHang.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.dgvKhachHang.Location = new System.Drawing.Point(16, 43);
            this.dgvKhachHang.MultiSelect = false;
            this.dgvKhachHang.Name = "dgvKhachHang";
            this.dgvKhachHang.ReadOnly = true;
            this.dgvKhachHang.RowHeadersVisible = false;
            this.dgvKhachHang.RowHeadersWidth = 51;
            this.dgvKhachHang.RowTemplate.Height = 38;
            this.dgvKhachHang.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKhachHang.Size = new System.Drawing.Size(1408, 455);
            this.dgvKhachHang.TabIndex = 1;
            // 
            // colSTT
            // 
            this.colSTT.HeaderText = "STT";
            this.colSTT.MinimumWidth = 6;
            this.colSTT.Name = "colSTT";
            this.colSTT.ReadOnly = true;
            this.colSTT.Width = 45;
            // 
            // colSTK
            // 
            this.colSTK.HeaderText = "STK";
            this.colSTK.MinimumWidth = 6;
            this.colSTK.Name = "colSTK";
            this.colSTK.ReadOnly = true;
            this.colSTK.Width = 85;
            // 
            // colCCCD
            // 
            this.colCCCD.HeaderText = "CCCD";
            this.colCCCD.MinimumWidth = 6;
            this.colCCCD.Name = "colCCCD";
            this.colCCCD.ReadOnly = true;
            this.colCCCD.Width = 110;
            // 
            // colHoTen
            // 
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.colHoTen.DefaultCellStyle = dataGridViewCellStyle7;
            this.colHoTen.HeaderText = "Họ tên";
            this.colHoTen.MinimumWidth = 6;
            this.colHoTen.Name = "colHoTen";
            this.colHoTen.ReadOnly = true;
            this.colHoTen.Width = 135;
            // 
            // colSDT
            // 
            this.colSDT.HeaderText = "SĐT";
            this.colSDT.MinimumWidth = 6;
            this.colSDT.Name = "colSDT";
            this.colSDT.ReadOnly = true;
            // 
            // colLoaiTaiKhoan
            // 
            this.colLoaiTaiKhoan.HeaderText = "Loại tài khoản";
            this.colLoaiTaiKhoan.MinimumWidth = 6;
            this.colLoaiTaiKhoan.Name = "colLoaiTaiKhoan";
            this.colLoaiTaiKhoan.ReadOnly = true;
            this.colLoaiTaiKhoan.Width = 110;
            // 
            // colSoDuDuNo
            // 
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.colSoDuDuNo.DefaultCellStyle = dataGridViewCellStyle8;
            this.colSoDuDuNo.HeaderText = "Tổng tiền vay/Tiết kiệm/TT";
            this.colSoDuDuNo.MinimumWidth = 6;
            this.colSoDuDuNo.Name = "colSoDuDuNo";
            this.colSoDuDuNo.ReadOnly = true;
            this.colSoDuDuNo.Width = 180;
            // 
            // colDaTraDaNhan
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.colDaTraDaNhan.DefaultCellStyle = dataGridViewCellStyle9;
            this.colDaTraDaNhan.HeaderText = "Đã trả / Đã nhận";
            this.colDaTraDaNhan.MinimumWidth = 6;
            this.colDaTraDaNhan.Name = "colDaTraDaNhan";
            this.colDaTraDaNhan.ReadOnly = true;
            this.colDaTraDaNhan.Width = 120;
            // 
            // colKyHan
            // 
            this.colKyHan.HeaderText = "Kỳ hạn";
            this.colKyHan.MinimumWidth = 6;
            this.colKyHan.Name = "colKyHan";
            this.colKyHan.ReadOnly = true;
            this.colKyHan.Width = 80;
            // 
            // colLaiSuat
            // 
            this.colLaiSuat.HeaderText = "Lãi suất";
            this.colLaiSuat.MinimumWidth = 6;
            this.colLaiSuat.Name = "colLaiSuat";
            this.colLaiSuat.ReadOnly = true;
            this.colLaiSuat.Width = 85;
            // 
            // colNhomNo
            // 
            this.colNhomNo.HeaderText = "Nhóm nợ";
            this.colNhomNo.MinimumWidth = 6;
            this.colNhomNo.Name = "colNhomNo";
            this.colNhomNo.ReadOnly = true;
            this.colNhomNo.Width = 140;
            // 
            // colNgayLam
            // 
            this.colNgayLam.HeaderText = "Ngày làm";
            this.colNgayLam.MinimumWidth = 6;
            this.colNgayLam.Name = "colNgayLam";
            this.colNgayLam.ReadOnly = true;
            this.colNgayLam.Width = 145;
            // 
            // colVaiTro
            // 
            this.colVaiTro.HeaderText = "Vai trò";
            this.colVaiTro.MinimumWidth = 6;
            this.colVaiTro.Name = "colVaiTro";
            this.colVaiTro.ReadOnly = true;
            this.colVaiTro.Width = 95;
            // 
            // colTrangThai
            // 
            this.colTrangThai.HeaderText = "Trạng thái";
            this.colTrangThai.MinimumWidth = 6;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.ReadOnly = true;
            this.colTrangThai.Width = 95;
            // 
            // lblGridTitle
            // 
            this.lblGridTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblGridTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblGridTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblGridTitle.Location = new System.Drawing.Point(16, 0);
            this.lblGridTitle.Name = "lblGridTitle";
            this.lblGridTitle.Size = new System.Drawing.Size(1408, 43);
            this.lblGridTitle.TabIndex = 2;
            this.lblGridTitle.Text = "📋 DANH SÁCH TÀI KHOẢN CỦA KHÁCH HÀNG";
            this.lblGridTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.White;
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBottom.Controls.Add(this.lblChiTiet);
            this.panelBottom.Controls.Add(this.lblKhachHangDangChon);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 757);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(27, 9, 27, 9);
            this.panelBottom.Size = new System.Drawing.Size(1442, 75);
            this.panelBottom.TabIndex = 5;
            // 
            // lblChiTiet
            // 
            this.lblChiTiet.AutoSize = true;
            this.lblChiTiet.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblChiTiet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblChiTiet.Location = new System.Drawing.Point(27, 36);
            this.lblChiTiet.Name = "lblChiTiet";
            this.lblChiTiet.Size = new System.Drawing.Size(230, 20);
            this.lblChiTiet.TabIndex = 0;
            this.lblChiTiet.Text = "Chọn một dòng để xem thông tin";
            // 
            // lblKhachHangDangChon
            // 
            this.lblKhachHangDangChon.AutoSize = true;
            this.lblKhachHangDangChon.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblKhachHangDangChon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblKhachHangDangChon.Location = new System.Drawing.Point(27, 9);
            this.lblKhachHangDangChon.Name = "lblKhachHangDangChon";
            this.lblKhachHangDangChon.Size = new System.Drawing.Size(168, 21);
            this.lblKhachHangDangChon.TabIndex = 1;
            this.lblKhachHangDangChon.Text = "Chưa chọn tài khoản";
            // 
            // FrmKhachHang
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1442, 832);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelInfo);
            this.Controls.Add(this.panelFilter);
            this.Controls.Add(this.panelSearch);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.panelBottom);
            this.Name = "FrmKhachHang";
            this.Text = "Quản lý khách hàng";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            this.panelInfo.ResumeLayout(false);
            this.panelInfo.PerformLayout();
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKhachHang)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}