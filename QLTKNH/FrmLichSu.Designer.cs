namespace QLTKNH
{
	partial class FrmLichSu
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.panelFilter = new System.Windows.Forms.Panel();
			this.lblCCCD = new System.Windows.Forms.Label();
			this.txtCCCD = new System.Windows.Forms.TextBox();
			this.lblMoc = new System.Windows.Forms.Label();
			this.cboMocThoiGian = new System.Windows.Forms.ComboBox();
			this.lblTu = new System.Windows.Forms.Label();
			this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
			this.lblDen = new System.Windows.Forms.Label();
			this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
			this.lblLoai = new System.Windows.Forms.Label();
			this.cboLoaiTK = new System.Windows.Forms.ComboBox();
			this.btnLamMoi = new System.Windows.Forms.Button();
			this.lblThongKe = new System.Windows.Forms.Label();
			this.dgvLichSu = new System.Windows.Forms.DataGridView();
			this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNgayGD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colLoaiTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSoTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colLoaiGD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSoTien = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSoDuSau = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colNguoiTH = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colGhiChu = new System.Windows.Forms.DataGridViewTextBoxColumn();

			this.panelFilter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvLichSu)).BeginInit();
			this.SuspendLayout();

			// panelFilter
			this.panelFilter.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.panelFilter.Controls.Add(this.lblCCCD);
			this.panelFilter.Controls.Add(this.txtCCCD);
			this.panelFilter.Controls.Add(this.lblMoc);
			this.panelFilter.Controls.Add(this.cboMocThoiGian);
			this.panelFilter.Controls.Add(this.lblTu);
			this.panelFilter.Controls.Add(this.dtpTuNgay);
			this.panelFilter.Controls.Add(this.lblDen);
			this.panelFilter.Controls.Add(this.dtpDenNgay);
			this.panelFilter.Controls.Add(this.lblLoai);
			this.panelFilter.Controls.Add(this.cboLoaiTK);
			this.panelFilter.Controls.Add(this.btnLamMoi);
			this.panelFilter.Controls.Add(this.lblThongKe);
			this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelFilter.Location = new System.Drawing.Point(0, 0);
			this.panelFilter.Name = "panelFilter";
			this.panelFilter.Size = new System.Drawing.Size(1280, 100);
			this.panelFilter.TabIndex = 0;

			// lblCCCD
			this.lblCCCD.AutoSize = true;
			this.lblCCCD.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblCCCD.ForeColor = System.Drawing.Color.White;
			this.lblCCCD.Location = new System.Drawing.Point(20, 20);
			this.lblCCCD.Name = "lblCCCD";
			this.lblCCCD.Size = new System.Drawing.Size(45, 17);
			this.lblCCCD.Text = "CCCD:";

			// txtCCCD
			this.txtCCCD.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.txtCCCD.Location = new System.Drawing.Point(80, 17);
			this.txtCCCD.MaxLength = 12;
			this.txtCCCD.Name = "txtCCCD";
			this.txtCCCD.Size = new System.Drawing.Size(150, 25);
			this.txtCCCD.TabIndex = 1;
			this.txtCCCD.TextChanged += new System.EventHandler(this.txtCCCD_TextChanged);

			// lblMoc
			this.lblMoc.AutoSize = true;
			this.lblMoc.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblMoc.ForeColor = System.Drawing.Color.White;
			this.lblMoc.Location = new System.Drawing.Point(250, 20);
			this.lblMoc.Name = "lblMoc";
			this.lblMoc.Size = new System.Drawing.Size(100, 17);
			this.lblMoc.Text = "Mốc thời gian:";

			// cboMocThoiGian
			this.cboMocThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboMocThoiGian.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.cboMocThoiGian.FormattingEnabled = true;
			this.cboMocThoiGian.Items.AddRange(new object[] {
				"Tất cả", "Hôm nay", "Tháng này", "Năm này", "Tùy chọn"});
			this.cboMocThoiGian.Location = new System.Drawing.Point(360, 17);
			this.cboMocThoiGian.Name = "cboMocThoiGian";
			this.cboMocThoiGian.Size = new System.Drawing.Size(140, 25);
			this.cboMocThoiGian.TabIndex = 2;
			this.cboMocThoiGian.SelectedIndex = 0;
			this.cboMocThoiGian.SelectedIndexChanged += new System.EventHandler(this.cboMocThoiGian_SelectedIndexChanged);

			// lblTu
			this.lblTu.AutoSize = true;
			this.lblTu.ForeColor = System.Drawing.Color.White;
			this.lblTu.Location = new System.Drawing.Point(520, 20);
			this.lblTu.Name = "lblTu";
			this.lblTu.Size = new System.Drawing.Size(55, 15);
			this.lblTu.Text = "Từ ngày:";

			// dtpTuNgay
			this.dtpTuNgay.Enabled = false;
			this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpTuNgay.Location = new System.Drawing.Point(585, 17);
			this.dtpTuNgay.Name = "dtpTuNgay";
			this.dtpTuNgay.Size = new System.Drawing.Size(120, 23);
			this.dtpTuNgay.TabIndex = 3;
			this.dtpTuNgay.ValueChanged += new System.EventHandler(this.dtpTuNgay_ValueChanged);

			// lblDen
			this.lblDen.AutoSize = true;
			this.lblDen.ForeColor = System.Drawing.Color.White;
			this.lblDen.Location = new System.Drawing.Point(720, 20);
			this.lblDen.Name = "lblDen";
			this.lblDen.Size = new System.Drawing.Size(60, 15);
			this.lblDen.Text = "Đến ngày:";

			// dtpDenNgay
			this.dtpDenNgay.Enabled = false;
			this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpDenNgay.Location = new System.Drawing.Point(790, 17);
			this.dtpDenNgay.Name = "dtpDenNgay";
			this.dtpDenNgay.Size = new System.Drawing.Size(120, 23);
			this.dtpDenNgay.TabIndex = 4;
			this.dtpDenNgay.ValueChanged += new System.EventHandler(this.dtpDenNgay_ValueChanged);

			// lblLoai
			this.lblLoai.AutoSize = true;
			this.lblLoai.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblLoai.ForeColor = System.Drawing.Color.White;
			this.lblLoai.Location = new System.Drawing.Point(20, 60);
			this.lblLoai.Name = "lblLoai";
			this.lblLoai.Size = new System.Drawing.Size(55, 17);
			this.lblLoai.Text = "Loại TK:";

			// cboLoaiTK
			this.cboLoaiTK.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboLoaiTK.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.cboLoaiTK.FormattingEnabled = true;
			this.cboLoaiTK.Items.AddRange(new object[] {
				"Tất cả", "Vay", "Tiết kiệm", "Thanh toán"});
			this.cboLoaiTK.Location = new System.Drawing.Point(80, 57);
			this.cboLoaiTK.Name = "cboLoaiTK";
			this.cboLoaiTK.Size = new System.Drawing.Size(150, 25);
			this.cboLoaiTK.TabIndex = 5;
			this.cboLoaiTK.SelectedIndex = 0;
			this.cboLoaiTK.SelectedIndexChanged += new System.EventHandler(this.cboLoaiTK_SelectedIndexChanged);

			// btnLamMoi
			this.btnLamMoi.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
			this.btnLamMoi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLamMoi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnLamMoi.ForeColor = System.Drawing.Color.White;
			this.btnLamMoi.Location = new System.Drawing.Point(250, 55);
			this.btnLamMoi.Name = "btnLamMoi";
			this.btnLamMoi.Size = new System.Drawing.Size(110, 28);
			this.btnLamMoi.TabIndex = 6;
			this.btnLamMoi.Text = "🔄 Làm mới";
			this.btnLamMoi.UseVisualStyleBackColor = false;
			this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);

			// lblThongKe
			this.lblThongKe.AutoSize = true;
			this.lblThongKe.Font = new System.Drawing.Font("Segoe UI", 9.5F);
			this.lblThongKe.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblThongKe.Location = new System.Drawing.Point(380, 60);
			this.lblThongKe.Name = "lblThongKe";
			this.lblThongKe.Size = new System.Drawing.Size(140, 17);
			this.lblThongKe.Text = "Tổng số giao dịch: 0";

			// dgvLichSu
			this.dgvLichSu.AllowUserToAddRows = false;
			this.dgvLichSu.BackgroundColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.dgvLichSu.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvLichSu.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
				this.colSTT, this.colNgayGD, this.colLoaiTK, this.colSoTK, this.colCCCD,
				this.colHoTen, this.colLoaiGD, this.colSoTien, this.colSoDuSau, this.colNguoiTH, this.colGhiChu});
			this.dgvLichSu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvLichSu.Location = new System.Drawing.Point(0, 100);
			this.dgvLichSu.MultiSelect = false;
			this.dgvLichSu.Name = "dgvLichSu";
			this.dgvLichSu.ReadOnly = true;
			this.dgvLichSu.RowHeadersVisible = false;
			this.dgvLichSu.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvLichSu.Size = new System.Drawing.Size(1280, 620);
			this.dgvLichSu.TabIndex = 1;


			// Columns
			this.colSTT.HeaderText = "STT";
			this.colSTT.Name = "colSTT";
			this.colSTT.Width = 50;

			this.colNgayGD.HeaderText = "Ngày giao dịch";
			this.colNgayGD.Name = "colNgayGD";
			this.colNgayGD.Width = 140;

			this.colLoaiTK.HeaderText = "Loại TK";
			this.colLoaiTK.Name = "colLoaiTK";
			this.colLoaiTK.Width = 90;

			this.colSoTK.HeaderText = "Số tài khoản";
			this.colSoTK.Name = "colSoTK";
			this.colSoTK.Width = 100;

			this.colCCCD.HeaderText = "CCCD";
			this.colCCCD.Name = "colCCCD";
			this.colCCCD.Width = 120;

			this.colHoTen.HeaderText = "Họ tên";
			this.colHoTen.Name = "colHoTen";
			this.colHoTen.Width = 140;

			this.colLoaiGD.HeaderText = "Loại giao dịch";
			this.colLoaiGD.Name = "colLoaiGD";
			this.colLoaiGD.Width = 150;

			this.colSoTien.HeaderText = "Số tiền";
			this.colSoTien.Name = "colSoTien";
			this.colSoTien.Width = 120;

			this.colSoDuSau.HeaderText = "Số dư sau";
			this.colSoDuSau.Name = "colSoDuSau";
			this.colSoDuSau.Width = 120;

			this.colNguoiTH.HeaderText = "Người thực hiện";
			this.colNguoiTH.Name = "colNguoiTH";
			this.colNguoiTH.Width = 150;

			this.colGhiChu.HeaderText = "Ghi chú";
			this.colGhiChu.Name = "colGhiChu";
			this.colGhiChu.Width = 200;

			// FrmLichSuTongHop
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.ClientSize = new System.Drawing.Size(1280, 720);
			this.Controls.Add(this.dgvLichSu);
			this.Controls.Add(this.panelFilter);
			this.Name = "FrmLichSuTongHop";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Lịch sử giao dịch tổng hợp - Tất cả loại tài khoản";
			this.Load += new System.EventHandler(this.FrmLichSuTongHop_Load);

			this.panelFilter.ResumeLayout(false);
			this.panelFilter.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvLichSu)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel panelFilter;
		private System.Windows.Forms.Label lblCCCD;
		private System.Windows.Forms.TextBox txtCCCD;
		private System.Windows.Forms.Label lblMoc;
		private System.Windows.Forms.ComboBox cboMocThoiGian;
		private System.Windows.Forms.Label lblTu;
		private System.Windows.Forms.DateTimePicker dtpTuNgay;
		private System.Windows.Forms.Label lblDen;
		private System.Windows.Forms.DateTimePicker dtpDenNgay;
		private System.Windows.Forms.Label lblLoai;
		private System.Windows.Forms.ComboBox cboLoaiTK;
		private System.Windows.Forms.Button btnLamMoi;
		private System.Windows.Forms.Label lblThongKe;
		private System.Windows.Forms.DataGridView dgvLichSu;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNgayGD;
		private System.Windows.Forms.DataGridViewTextBoxColumn colLoaiTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSoTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCCCD;
		private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
		private System.Windows.Forms.DataGridViewTextBoxColumn colLoaiGD;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSoTien;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSoDuSau;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNguoiTH;
		private System.Windows.Forms.DataGridViewTextBoxColumn colGhiChu;
	}
}