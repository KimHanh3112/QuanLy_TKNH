namespace QLTKNH
{
    partial class FrmLoaiTaiKhoanDN
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.spcMain = new System.Windows.Forms.SplitContainer();
            this.spcTop = new System.Windows.Forms.SplitContainer();
            this.grpInput = new System.Windows.Forms.GroupBox();
            this.lblDanhMucLoai = new System.Windows.Forms.Label();
            this.cboDanhMucLoaiTK = new System.Windows.Forms.ComboBox();
            this.lblThang = new System.Windows.Forms.Label();
            this.nudThang = new System.Windows.Forms.NumericUpDown();
            this.btnTim = new System.Windows.Forms.Button();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.grpLoaiTK = new System.Windows.Forms.GroupBox();
            this.dgvLoaiTK = new System.Windows.Forms.DataGridView();
            this.colMaLoai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTenLoai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colThang = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLaiSuat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMoTa = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpChiTiet = new System.Windows.Forms.GroupBox();
            this.dgvChiTietTK = new System.Windows.Forms.DataGridView();
            this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMaTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTenKhachHang = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoDuDuNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNgayMo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.lblChiTietHeader = new System.Windows.Forms.Label();
            this.lblTongSoLuong = new System.Windows.Forms.Label();
            this.cmsLoaiTK = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiXoa = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcMain)).BeginInit();
            this.spcMain.Panel1.SuspendLayout();
            this.spcMain.Panel2.SuspendLayout();
            this.spcMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcTop)).BeginInit();
            this.spcTop.Panel1.SuspendLayout();
            this.spcTop.Panel2.SuspendLayout();
            this.spcTop.SuspendLayout();
            this.grpInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThang)).BeginInit();
            this.grpLoaiTK.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLoaiTK)).BeginInit();
            this.grpChiTiet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTietTK)).BeginInit();
            this.pnlFilterBar.SuspendLayout();
            this.cmsLoaiTK.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1250, 50);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(593, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "QUẢN LÝ DANH MỤC & CẤU HÌNH LOẠI TÀI KHOẢN";
            // 
            // spcMain
            // 
            this.spcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcMain.Location = new System.Drawing.Point(0, 50);
            this.spcMain.Name = "spcMain";
            this.spcMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcMain.Panel1
            // 
            this.spcMain.Panel1.Controls.Add(this.spcTop);
            // 
            // spcMain.Panel2
            // 
            this.spcMain.Panel2.Controls.Add(this.grpChiTiet);
            this.spcMain.Size = new System.Drawing.Size(1250, 670);
            this.spcMain.SplitterDistance = 310;
            this.spcMain.TabIndex = 1;
            // 
            // spcTop
            // 
            this.spcTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcTop.Location = new System.Drawing.Point(0, 0);
            this.spcTop.Name = "spcTop";
            // 
            // spcTop.Panel1
            // 
            this.spcTop.Panel1.Controls.Add(this.grpInput);
            // 
            // spcTop.Panel2
            // 
            this.spcTop.Panel2.Controls.Add(this.grpLoaiTK);
            this.spcTop.Size = new System.Drawing.Size(1250, 310);
            this.spcTop.SplitterDistance = 440;
            this.spcTop.TabIndex = 0;
            // 
            // grpInput
            // 
            this.grpInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.grpInput.Controls.Add(this.lblDanhMucLoai);
            this.grpInput.Controls.Add(this.cboDanhMucLoaiTK);
            this.grpInput.Controls.Add(this.lblThang);
            this.grpInput.Controls.Add(this.nudThang);
            this.grpInput.Controls.Add(this.btnTim);
            this.grpInput.Controls.Add(this.btnLamMoi);
            this.grpInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInput.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpInput.ForeColor = System.Drawing.Color.White;
            this.grpInput.Location = new System.Drawing.Point(0, 0);
            this.grpInput.Name = "grpInput";
            this.grpInput.Padding = new System.Windows.Forms.Padding(10);
            this.grpInput.Size = new System.Drawing.Size(440, 310);
            this.grpInput.TabIndex = 0;
            this.grpInput.TabStop = false;
            this.grpInput.Text = "Tìm Kiếm Cấu Hình Loại Tài Khoản";
            // 
            // lblDanhMucLoai
            // 
            this.lblDanhMucLoai.AutoSize = true;
            this.lblDanhMucLoai.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDanhMucLoai.Location = new System.Drawing.Point(15, 40);
            this.lblDanhMucLoai.Name = "lblDanhMucLoai";
            this.lblDanhMucLoai.Size = new System.Drawing.Size(108, 20);
            this.lblDanhMucLoai.TabIndex = 0;
            this.lblDanhMucLoai.Text = "Loại Tài Khoản:";
            // 
            // cboDanhMucLoaiTK
            // 
            this.cboDanhMucLoaiTK.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDanhMucLoaiTK.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboDanhMucLoaiTK.FormattingEnabled = true;
            this.cboDanhMucLoaiTK.Location = new System.Drawing.Point(115, 37);
            this.cboDanhMucLoaiTK.Name = "cboDanhMucLoaiTK";
            this.cboDanhMucLoaiTK.Size = new System.Drawing.Size(203, 28);
            this.cboDanhMucLoaiTK.TabIndex = 0;
            // 
            // lblThang
            // 
            this.lblThang.AutoSize = true;
            this.lblThang.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblThang.Location = new System.Drawing.Point(15, 80);
            this.lblThang.Name = "lblThang";
            this.lblThang.Size = new System.Drawing.Size(110, 20);
            this.lblThang.TabIndex = 1;
            this.lblThang.Text = "Kỳ hạn (Tháng):";
            // 
            // nudThang
            // 
            this.nudThang.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.nudThang.Location = new System.Drawing.Point(115, 77);
            this.nudThang.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.nudThang.Name = "nudThang";
            this.nudThang.Size = new System.Drawing.Size(120, 27);
            this.nudThang.TabIndex = 1;
            // 
            // btnTim
            // 
            this.btnTim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTim.FlatAppearance.BorderSize = 0;
            this.btnTim.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTim.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnTim.ForeColor = System.Drawing.Color.White;
            this.btnTim.Location = new System.Drawing.Point(12, 138);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(145, 36);
            this.btnTim.TabIndex = 2;
            this.btnTim.Text = "🔍 Tìm Kiếm";
            this.btnTim.UseVisualStyleBackColor = false;
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.btnLamMoi.FlatAppearance.BorderSize = 0;
            this.btnLamMoi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLamMoi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLamMoi.ForeColor = System.Drawing.Color.White;
            this.btnLamMoi.Location = new System.Drawing.Point(173, 138);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(145, 36);
            this.btnLamMoi.TabIndex = 3;
            this.btnLamMoi.Text = "🔄 Làm Mới";
            this.btnLamMoi.UseVisualStyleBackColor = false;
            // 
            // grpLoaiTK
            // 
            this.grpLoaiTK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.grpLoaiTK.Controls.Add(this.dgvLoaiTK);
            this.grpLoaiTK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLoaiTK.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpLoaiTK.ForeColor = System.Drawing.Color.White;
            this.grpLoaiTK.Location = new System.Drawing.Point(0, 0);
            this.grpLoaiTK.Name = "grpLoaiTK";
            this.grpLoaiTK.Size = new System.Drawing.Size(806, 310);
            this.grpLoaiTK.TabIndex = 0;
            this.grpLoaiTK.TabStop = false;
            this.grpLoaiTK.Text = "Danh Sách Loại Tài Khoản ";
            // 
            // dgvLoaiTK
            // 
            this.dgvLoaiTK.AllowUserToAddRows = false;
            this.dgvLoaiTK.AllowUserToDeleteRows = false;
            this.dgvLoaiTK.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLoaiTK.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
            this.dgvLoaiTK.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvLoaiTK.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLoaiTK.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMaLoai,
            this.colTenLoai,
            this.colThang,
            this.colLaiSuat,
            this.colMoTa});
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle8.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvLoaiTK.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvLoaiTK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLoaiTK.EnableHeadersVisualStyles = false;
            this.dgvLoaiTK.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.dgvLoaiTK.Location = new System.Drawing.Point(3, 25);
            this.dgvLoaiTK.MultiSelect = false;
            this.dgvLoaiTK.Name = "dgvLoaiTK";
            this.dgvLoaiTK.ReadOnly = true;
            this.dgvLoaiTK.RowHeadersWidth = 51;
            this.dgvLoaiTK.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLoaiTK.Size = new System.Drawing.Size(800, 282);
            this.dgvLoaiTK.TabIndex = 0;
            // 
            // colMaLoai
            // 
            this.colMaLoai.HeaderText = "Mã Loại TK";
            this.colMaLoai.MinimumWidth = 6;
            this.colMaLoai.Name = "colMaLoai";
            this.colMaLoai.ReadOnly = true;
            // 
            // colTenLoai
            // 
            this.colTenLoai.HeaderText = "Tên Loại Tài Khoản";
            this.colTenLoai.MinimumWidth = 6;
            this.colTenLoai.Name = "colTenLoai";
            this.colTenLoai.ReadOnly = true;
            // 
            // colThang
            // 
            this.colThang.HeaderText = "Kỳ hạn (Tháng)";
            this.colThang.MinimumWidth = 6;
            this.colThang.Name = "colThang";
            this.colThang.ReadOnly = true;
            // 
            // colLaiSuat
            // 
            this.colLaiSuat.HeaderText = "Lãi Suất (%/năm)";
            this.colLaiSuat.MinimumWidth = 6;
            this.colLaiSuat.Name = "colLaiSuat";
            this.colLaiSuat.ReadOnly = true;
            // 
            // colMoTa
            // 
            this.colMoTa.HeaderText = "Mô Tả / Ghi Chú";
            this.colMoTa.MinimumWidth = 6;
            this.colMoTa.Name = "colMoTa";
            this.colMoTa.ReadOnly = true;
            // 
            // grpChiTiet
            // 
            this.grpChiTiet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.grpChiTiet.Controls.Add(this.dgvChiTietTK);
            this.grpChiTiet.Controls.Add(this.pnlFilterBar);
            this.grpChiTiet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpChiTiet.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.grpChiTiet.ForeColor = System.Drawing.Color.White;
            this.grpChiTiet.Location = new System.Drawing.Point(0, 0);
            this.grpChiTiet.Name = "grpChiTiet";
            this.grpChiTiet.Size = new System.Drawing.Size(1250, 356);
            this.grpChiTiet.TabIndex = 0;
            this.grpChiTiet.TabStop = false;
            this.grpChiTiet.Text = "Chi Tiết Danh Sách Tài Khoản Khách đang chọn";
            // 
            // dgvChiTietTK
            // 
            this.dgvChiTietTK.AllowUserToAddRows = false;
            this.dgvChiTietTK.AllowUserToDeleteRows = false;
            this.dgvChiTietTK.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChiTietTK.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            this.dgvChiTietTK.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvChiTietTK.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvChiTietTK.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSTT,
            this.colMaTK,
            this.colTenKhachHang,
            this.colSoCCCD,
            this.colSoDuDuNo,
            this.colNgayMo,
            this.colTrangThai,
            this.colVaiTro});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(197)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvChiTietTK.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvChiTietTK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvChiTietTK.EnableHeadersVisualStyles = false;
            this.dgvChiTietTK.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.dgvChiTietTK.Location = new System.Drawing.Point(3, 63);
            this.dgvChiTietTK.MultiSelect = false;
            this.dgvChiTietTK.Name = "dgvChiTietTK";
            this.dgvChiTietTK.ReadOnly = true;
            this.dgvChiTietTK.RowHeadersWidth = 51;
            this.dgvChiTietTK.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvChiTietTK.Size = new System.Drawing.Size(1244, 290);
            this.dgvChiTietTK.TabIndex = 1;
            // 
            // colSTT
            // 
            this.colSTT.HeaderText = "STT";
            this.colSTT.MinimumWidth = 6;
            this.colSTT.Name = "colSTT";
            this.colSTT.ReadOnly = true;
            // 
            // colMaTK
            // 
            this.colMaTK.HeaderText = "Mã TK / Số TK";
            this.colMaTK.MinimumWidth = 6;
            this.colMaTK.Name = "colMaTK";
            this.colMaTK.ReadOnly = true;
            // 
            // colTenKhachHang
            // 
            this.colTenKhachHang.HeaderText = "Tên Khách Hàng";
            this.colTenKhachHang.MinimumWidth = 6;
            this.colTenKhachHang.Name = "colTenKhachHang";
            this.colTenKhachHang.ReadOnly = true;
            // 
            // colSoCCCD
            // 
            this.colSoCCCD.HeaderText = "Số Mã số thuế";
            this.colSoCCCD.MinimumWidth = 6;
            this.colSoCCCD.Name = "colSoCCCD";
            this.colSoCCCD.ReadOnly = true;
            // 
            // colSoDuDuNo
            // 
            this.colSoDuDuNo.HeaderText = "Số Dư / Dư Nợ Hiện Tại";
            this.colSoDuDuNo.MinimumWidth = 6;
            this.colSoDuDuNo.Name = "colSoDuDuNo";
            this.colSoDuDuNo.ReadOnly = true;
            // 
            // colNgayMo
            // 
            this.colNgayMo.HeaderText = "Ngày Mở";
            this.colNgayMo.MinimumWidth = 6;
            this.colNgayMo.Name = "colNgayMo";
            this.colNgayMo.ReadOnly = true;
            // 
            // colTrangThai
            // 
            this.colTrangThai.HeaderText = "Trạng Thái";
            this.colTrangThai.MinimumWidth = 6;
            this.colTrangThai.Name = "colTrangThai";
            this.colTrangThai.ReadOnly = true;
            // 
            // colVaiTro
            // 
            this.colVaiTro.FillWeight = 160F;
            this.colVaiTro.HeaderText = "Vai trò";
            this.colVaiTro.MinimumWidth = 170;
            this.colVaiTro.Name = "colVaiTro";
            this.colVaiTro.ReadOnly = true;
            // 
            // pnlFilterBar
            // 
            this.pnlFilterBar.Controls.Add(this.lblChiTietHeader);
            this.pnlFilterBar.Controls.Add(this.lblTongSoLuong);
            this.pnlFilterBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Location = new System.Drawing.Point(3, 25);
            this.pnlFilterBar.Name = "pnlFilterBar";
            this.pnlFilterBar.Size = new System.Drawing.Size(1244, 38);
            this.pnlFilterBar.TabIndex = 0;
            // 
            // lblChiTietHeader
            // 
            this.lblChiTietHeader.AutoSize = true;
            this.lblChiTietHeader.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblChiTietHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblChiTietHeader.Location = new System.Drawing.Point(15, 10);
            this.lblChiTietHeader.Name = "lblChiTietHeader";
            this.lblChiTietHeader.Size = new System.Drawing.Size(331, 21);
            this.lblChiTietHeader.TabIndex = 0;
            this.lblChiTietHeader.Text = "Danh sách tài khoản mở theo loại đã chọn";
            // 
            // lblTongSoLuong
            // 
            this.lblTongSoLuong.AutoSize = true;
            this.lblTongSoLuong.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTongSoLuong.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(211)))), ((int)(((byte)(153)))));
            this.lblTongSoLuong.Location = new System.Drawing.Point(450, 10);
            this.lblTongSoLuong.Name = "lblTongSoLuong";
            this.lblTongSoLuong.Size = new System.Drawing.Size(213, 21);
            this.lblTongSoLuong.TabIndex = 1;
            this.lblTongSoLuong.Text = "Tổng số lượng tài khoản: 0";
            // 
            // cmsLoaiTK
            // 
            this.cmsLoaiTK.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmsLoaiTK.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiXoa});
            this.cmsLoaiTK.Name = "cmsLoaiTK";
            this.cmsLoaiTK.Size = new System.Drawing.Size(132, 28);
            // 
            // tsmiXoa
            // 
            this.tsmiXoa.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsmiXoa.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.tsmiXoa.Name = "tsmiXoa";
            this.tsmiXoa.Size = new System.Drawing.Size(131, 24);
            this.tsmiXoa.Text = "🗑️ Xóa";
            // 
            // FrmLoaiTaiKhoanDN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1250, 720);
            this.Controls.Add(this.spcMain);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FrmLoaiTaiKhoanDN";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quản Lý Loại Tài Khoản";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.spcMain.Panel1.ResumeLayout(false);
            this.spcMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcMain)).EndInit();
            this.spcMain.ResumeLayout(false);
            this.spcTop.Panel1.ResumeLayout(false);
            this.spcTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcTop)).EndInit();
            this.spcTop.ResumeLayout(false);
            this.grpInput.ResumeLayout(false);
            this.grpInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThang)).EndInit();
            this.grpLoaiTK.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLoaiTK)).EndInit();
            this.grpChiTiet.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTietTK)).EndInit();
            this.pnlFilterBar.ResumeLayout(false);
            this.pnlFilterBar.PerformLayout();
            this.cmsLoaiTK.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.SplitContainer spcMain;
        private System.Windows.Forms.SplitContainer spcTop;

        private System.Windows.Forms.GroupBox grpInput;
        private System.Windows.Forms.Label lblDanhMucLoai;
        private System.Windows.Forms.ComboBox cboDanhMucLoaiTK;
        private System.Windows.Forms.Label lblThang;
        private System.Windows.Forms.NumericUpDown nudThang;
        private System.Windows.Forms.Button btnTim;
        private System.Windows.Forms.Button btnLamMoi;

        private System.Windows.Forms.GroupBox grpLoaiTK;
        private System.Windows.Forms.DataGridView dgvLoaiTK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMaLoai;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTenLoai;
        private System.Windows.Forms.DataGridViewTextBoxColumn colThang;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLaiSuat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMoTa;

        private System.Windows.Forms.ContextMenuStrip cmsLoaiTK;
        private System.Windows.Forms.ToolStripMenuItem tsmiXoa;

        private System.Windows.Forms.GroupBox grpChiTiet;
        private System.Windows.Forms.Panel pnlFilterBar;
        private System.Windows.Forms.Label lblChiTietHeader;
        private System.Windows.Forms.Label lblTongSoLuong;
        private System.Windows.Forms.DataGridView dgvChiTietTK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMaTK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTenKhachHang;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoCCCD;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoDuDuNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNgayMo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTrangThai;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVaiTro;
    }
}