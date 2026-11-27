namespace QLTKNH
{
	partial class FrmThongKeDN
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
			this.panelTop = new System.Windows.Forms.Panel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblMoc = new System.Windows.Forms.Label();
			this.cboMocThoiGian = new System.Windows.Forms.ComboBox();
			this.lblTu = new System.Windows.Forms.Label();
			this.dtpTuNgay = new System.Windows.Forms.DateTimePicker();
			this.lblDen = new System.Windows.Forms.Label();
			this.dtpDenNgay = new System.Windows.Forms.DateTimePicker();
			this.btnLamMoi = new System.Windows.Forms.Button();

			this.panelCards = new System.Windows.Forms.Panel();
			this.cardKH = new System.Windows.Forms.Panel();
			this.lblTongKH = new System.Windows.Forms.Label();
			this.lblKHTitle = new System.Windows.Forms.Label();
			this.cardVay = new System.Windows.Forms.Panel();
			this.lblTongTKVay = new System.Windows.Forms.Label();
			this.lblVayTitle = new System.Windows.Forms.Label();
			this.cardTK = new System.Windows.Forms.Panel();
			this.lblTongTKTK = new System.Windows.Forms.Label();
			this.lblTKTitle = new System.Windows.Forms.Label();
			this.cardTT = new System.Windows.Forms.Panel();
			this.lblTongTKTT = new System.Windows.Forms.Label();
			this.lblTTTitle = new System.Windows.Forms.Label();

			this.panelDoanhThu = new System.Windows.Forms.Panel();
			this.lblDoanhThuTitle = new System.Windows.Forms.Label();
			this.lblLaiThuVayCaption = new System.Windows.Forms.Label();
			this.lblLaiThuVay = new System.Windows.Forms.Label();
			this.lblLaiChiTKCaption = new System.Windows.Forms.Label();
			this.lblLaiChiTK = new System.Windows.Forms.Label();
			this.lblDoanhThuRongCaption = new System.Windows.Forms.Label();
			this.lblDoanhThuRong = new System.Windows.Forms.Label();

			this.panelTien = new System.Windows.Forms.Panel();
			this.lblTienTitle = new System.Windows.Forms.Label();
			this.lblTongTienVayCaption = new System.Windows.Forms.Label();
			this.lblTongTienVay = new System.Windows.Forms.Label();
			this.lblTongDaTraCaption = new System.Windows.Forms.Label();
			this.lblTongDaTra = new System.Windows.Forms.Label();
			this.lblTongDuNoCaption = new System.Windows.Forms.Label();
			this.lblTongDuNo = new System.Windows.Forms.Label();
			this.lblTongTienGuiCaption = new System.Windows.Forms.Label();
			this.lblTongTienGui = new System.Windows.Forms.Label();
			this.lblTongSoDuTTCaption = new System.Windows.Forms.Label();
			this.lblTongSoDuTT = new System.Windows.Forms.Label();

			this.panelGD = new System.Windows.Forms.Panel();
			this.lblGDTitle = new System.Windows.Forms.Label();
			this.lblSoGDVayCaption = new System.Windows.Forms.Label();
			this.lblSoGDVay = new System.Windows.Forms.Label();
			this.lblSoGDTKCaption = new System.Windows.Forms.Label();
			this.lblSoGDTK = new System.Windows.Forms.Label();
			this.lblSoGDTTCaption = new System.Windows.Forms.Label();
			this.lblSoGDTT = new System.Windows.Forms.Label();
			this.lblTongGDCaption = new System.Windows.Forms.Label();
			this.lblTongGD = new System.Windows.Forms.Label();

			this.dgvTrangThai = new System.Windows.Forms.DataGridView();
			this.dgvTopLai = new System.Windows.Forms.DataGridView();
			this.dgvKyHan = new System.Windows.Forms.DataGridView();
			this.lblTrangThaiTitle = new System.Windows.Forms.Label();
			this.lblTopLaiTitle = new System.Windows.Forms.Label();
			this.lblKyHanTitle = new System.Windows.Forms.Label();

			this.panelTop.SuspendLayout();
			this.panelCards.SuspendLayout();
			this.cardKH.SuspendLayout();
			this.cardVay.SuspendLayout();
			this.cardTK.SuspendLayout();
			this.cardTT.SuspendLayout();
			this.panelDoanhThu.SuspendLayout();
			this.panelTien.SuspendLayout();
			this.panelGD.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTrangThai)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvTopLai)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvKyHan)).BeginInit();
			this.SuspendLayout();

			// ===== panelTop =====
			this.panelTop.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.panelTop.Controls.Add(this.lblTitle);
			this.panelTop.Controls.Add(this.lblMoc);
			this.panelTop.Controls.Add(this.cboMocThoiGian);
			this.panelTop.Controls.Add(this.lblTu);
			this.panelTop.Controls.Add(this.dtpTuNgay);
			this.panelTop.Controls.Add(this.lblDen);
			this.panelTop.Controls.Add(this.dtpDenNgay);
			this.panelTop.Controls.Add(this.btnLamMoi);
			this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTop.Height = 90;
			this.panelTop.Name = "panelTop";

			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Location = new System.Drawing.Point(20, 12);
			this.lblTitle.Text = "📊 THỐNG KÊ TỔNG HỢP DOANH NGHIỆP";

			this.lblMoc.AutoSize = true;
			this.lblMoc.ForeColor = System.Drawing.Color.White;
			this.lblMoc.Location = new System.Drawing.Point(20, 52);
			this.lblMoc.Text = "Khoảng thời gian:";

			this.cboMocThoiGian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboMocThoiGian.Items.AddRange(new object[] { "Tất cả", "Hôm nay", "Tháng này", "Năm này", "Tùy chọn" });
			this.cboMocThoiGian.Location = new System.Drawing.Point(140, 48);
			this.cboMocThoiGian.Size = new System.Drawing.Size(130, 25);
			this.cboMocThoiGian.SelectedIndex = 0;

			this.lblTu.AutoSize = true;
			this.lblTu.ForeColor = System.Drawing.Color.White;
			this.lblTu.Location = new System.Drawing.Point(290, 52);
			this.lblTu.Text = "Từ:";

			this.dtpTuNgay.Enabled = false;
			this.dtpTuNgay.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpTuNgay.Location = new System.Drawing.Point(320, 48);
			this.dtpTuNgay.Size = new System.Drawing.Size(120, 25);

			this.lblDen.AutoSize = true;
			this.lblDen.ForeColor = System.Drawing.Color.White;
			this.lblDen.Location = new System.Drawing.Point(455, 52);
			this.lblDen.Text = "Đến:";

			this.dtpDenNgay.Enabled = false;
			this.dtpDenNgay.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpDenNgay.Location = new System.Drawing.Point(490, 48);
			this.dtpDenNgay.Size = new System.Drawing.Size(120, 25);

			this.btnLamMoi.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
			this.btnLamMoi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLamMoi.ForeColor = System.Drawing.Color.White;
			this.btnLamMoi.Location = new System.Drawing.Point(630, 45);
			this.btnLamMoi.Size = new System.Drawing.Size(110, 30);
			this.btnLamMoi.Text = "🔄 Làm mới";

			// ===== panelCards =====
			this.panelCards.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelCards.Height = 100;
			this.panelCards.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.panelCards.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);

			// Card KH
			this.cardKH.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.cardKH.Size = new System.Drawing.Size(200, 80);
			this.cardKH.Location = new System.Drawing.Point(20, 10);
			this.lblKHTitle.Text = "Tổng doanh nghiệp";
			this.lblKHTitle.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblKHTitle.Location = new System.Drawing.Point(15, 10);
			this.lblKHTitle.AutoSize = true;
			this.lblTongKH.Text = "0";
			this.lblTongKH.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
			this.lblTongKH.ForeColor = System.Drawing.Color.White;
			this.lblTongKH.Location = new System.Drawing.Point(15, 35);
			this.lblTongKH.AutoSize = true;
			this.cardKH.Controls.Add(this.lblKHTitle);
			this.cardKH.Controls.Add(this.lblTongKH);

			// Card Vay
			this.cardVay.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.cardVay.Size = new System.Drawing.Size(200, 80);
			this.cardVay.Location = new System.Drawing.Point(240, 10);
			this.lblVayTitle.Text = "Tài khoản Vay";
			this.lblVayTitle.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblVayTitle.Location = new System.Drawing.Point(15, 10);
			this.lblVayTitle.AutoSize = true;
			this.lblTongTKVay.Text = "0";
			this.lblTongTKVay.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
			this.lblTongTKVay.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246);
			this.lblTongTKVay.Location = new System.Drawing.Point(15, 35);
			this.lblTongTKVay.AutoSize = true;
			this.cardVay.Controls.Add(this.lblVayTitle);
			this.cardVay.Controls.Add(this.lblTongTKVay);

			// Card TK
			this.cardTK.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.cardTK.Size = new System.Drawing.Size(200, 80);
			this.cardTK.Location = new System.Drawing.Point(460, 10);
			this.lblTKTitle.Text = "Sổ Tiết kiệm";
			this.lblTKTitle.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTKTitle.Location = new System.Drawing.Point(15, 10);
			this.lblTKTitle.AutoSize = true;
			this.lblTongTKTK.Text = "0";
			this.lblTongTKTK.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
			this.lblTongTKTK.ForeColor = System.Drawing.Color.FromArgb(34, 197, 94);
			this.lblTongTKTK.Location = new System.Drawing.Point(15, 35);
			this.lblTongTKTK.AutoSize = true;
			this.cardTK.Controls.Add(this.lblTKTitle);
			this.cardTK.Controls.Add(this.lblTongTKTK);

			// Card TT
			this.cardTT.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.cardTT.Size = new System.Drawing.Size(200, 80);
			this.cardTT.Location = new System.Drawing.Point(680, 10);
			this.lblTTTitle.Text = "Tài khoản TT";
			this.lblTTTitle.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTTTitle.Location = new System.Drawing.Point(15, 10);
			this.lblTTTitle.AutoSize = true;
			this.lblTongTKTT.Text = "0";
			this.lblTongTKTT.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
			this.lblTongTKTT.ForeColor = System.Drawing.Color.FromArgb(251, 146, 60);
			this.lblTongTKTT.Location = new System.Drawing.Point(15, 35);
			this.lblTongTKTT.AutoSize = true;
			this.cardTT.Controls.Add(this.lblTTTitle);
			this.cardTT.Controls.Add(this.lblTongTKTT);

			this.panelCards.Controls.Add(this.cardKH);
			this.panelCards.Controls.Add(this.cardVay);
			this.panelCards.Controls.Add(this.cardTK);
			this.panelCards.Controls.Add(this.cardTT);

			// ===== panelDoanhThu =====
			this.panelDoanhThu.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.panelDoanhThu.Location = new System.Drawing.Point(20, 200);
			this.panelDoanhThu.Size = new System.Drawing.Size(400, 180);
			this.lblDoanhThuTitle.Text = "💰 DOANH THU NGÂN HÀNG";
			this.lblDoanhThuTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblDoanhThuTitle.ForeColor = System.Drawing.Color.White;
			this.lblDoanhThuTitle.Location = new System.Drawing.Point(15, 12);
			this.lblDoanhThuTitle.AutoSize = true;

			this.lblLaiThuVayCaption.Text = "Lãi thu từ khoản vay:";
			this.lblLaiThuVayCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblLaiThuVayCaption.Location = new System.Drawing.Point(15, 50);
			this.lblLaiThuVayCaption.AutoSize = true;
			this.lblLaiThuVay.Text = "0 VNĐ";
			this.lblLaiThuVay.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblLaiThuVay.ForeColor = System.Drawing.Color.FromArgb(34, 197, 94);
			this.lblLaiThuVay.Location = new System.Drawing.Point(200, 48);
			this.lblLaiThuVay.AutoSize = true;

			this.lblLaiChiTKCaption.Text = "Lãi chi cho tiết kiệm:";
			this.lblLaiChiTKCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblLaiChiTKCaption.Location = new System.Drawing.Point(15, 85);
			this.lblLaiChiTKCaption.AutoSize = true;
			this.lblLaiChiTK.Text = "0 VNĐ";
			this.lblLaiChiTK.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
			this.lblLaiChiTK.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68);
			this.lblLaiChiTK.Location = new System.Drawing.Point(200, 83);
			this.lblLaiChiTK.AutoSize = true;

			this.lblDoanhThuRongCaption.Text = "DOANH THU RÒNG:";
			this.lblDoanhThuRongCaption.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblDoanhThuRongCaption.ForeColor = System.Drawing.Color.White;
			this.lblDoanhThuRongCaption.Location = new System.Drawing.Point(15, 130);
			this.lblDoanhThuRongCaption.AutoSize = true;
			this.lblDoanhThuRong.Text = "0 VNĐ";
			this.lblDoanhThuRong.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
			this.lblDoanhThuRong.ForeColor = System.Drawing.Color.FromArgb(34, 197, 94);
			this.lblDoanhThuRong.Location = new System.Drawing.Point(180, 125);
			this.lblDoanhThuRong.AutoSize = true;

			this.panelDoanhThu.Controls.AddRange(new System.Windows.Forms.Control[] {
				this.lblDoanhThuTitle, this.lblLaiThuVayCaption, this.lblLaiThuVay,
				this.lblLaiChiTKCaption, this.lblLaiChiTK,
				this.lblDoanhThuRongCaption, this.lblDoanhThuRong
			});

			// ===== panelTien =====
			this.panelTien.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.panelTien.Location = new System.Drawing.Point(440, 200);
			this.panelTien.Size = new System.Drawing.Size(400, 180);
			this.lblTienTitle.Text = "💵 TỔNG TIỀN";
			this.lblTienTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblTienTitle.ForeColor = System.Drawing.Color.White;
			this.lblTienTitle.Location = new System.Drawing.Point(15, 12);
			this.lblTienTitle.AutoSize = true;

			this.lblTongTienVayCaption.Text = "Tổng tiền cho vay:";
			this.lblTongTienVayCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTongTienVayCaption.Location = new System.Drawing.Point(15, 45);
			this.lblTongTienVayCaption.AutoSize = true;
			this.lblTongTienVay.Text = "0 VNĐ";
			this.lblTongTienVay.ForeColor = System.Drawing.Color.White;
			this.lblTongTienVay.Location = new System.Drawing.Point(200, 45);
			this.lblTongTienVay.AutoSize = true;

			this.lblTongDaTraCaption.Text = "Tổng đã thu hồi:";
			this.lblTongDaTraCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTongDaTraCaption.Location = new System.Drawing.Point(15, 70);
			this.lblTongDaTraCaption.AutoSize = true;
			this.lblTongDaTra.Text = "0 VNĐ";
			this.lblTongDaTra.ForeColor = System.Drawing.Color.White;
			this.lblTongDaTra.Location = new System.Drawing.Point(200, 70);
			this.lblTongDaTra.AutoSize = true;

			this.lblTongDuNoCaption.Text = "Tổng dư nợ:";
			this.lblTongDuNoCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTongDuNoCaption.Location = new System.Drawing.Point(15, 95);
			this.lblTongDuNoCaption.AutoSize = true;
			this.lblTongDuNo.Text = "0 VNĐ";
			this.lblTongDuNo.ForeColor = System.Drawing.Color.FromArgb(251, 146, 60);
			this.lblTongDuNo.Location = new System.Drawing.Point(200, 95);
			this.lblTongDuNo.AutoSize = true;

			this.lblTongTienGuiCaption.Text = "Tiền gửi tiết kiệm:";
			this.lblTongTienGuiCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTongTienGuiCaption.Location = new System.Drawing.Point(15, 120);
			this.lblTongTienGuiCaption.AutoSize = true;
			this.lblTongTienGui.Text = "0 VNĐ";
			this.lblTongTienGui.ForeColor = System.Drawing.Color.White;
			this.lblTongTienGui.Location = new System.Drawing.Point(200, 120);
			this.lblTongTienGui.AutoSize = true;

			this.lblTongSoDuTTCaption.Text = "Số dư thanh toán:";
			this.lblTongSoDuTTCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblTongSoDuTTCaption.Location = new System.Drawing.Point(15, 145);
			this.lblTongSoDuTTCaption.AutoSize = true;
			this.lblTongSoDuTT.Text = "0 VNĐ";
			this.lblTongSoDuTT.ForeColor = System.Drawing.Color.White;
			this.lblTongSoDuTT.Location = new System.Drawing.Point(200, 145);
			this.lblTongSoDuTT.AutoSize = true;

			this.panelTien.Controls.AddRange(new System.Windows.Forms.Control[] {
				this.lblTienTitle, this.lblTongTienVayCaption, this.lblTongTienVay,
				this.lblTongDaTraCaption, this.lblTongDaTra, this.lblTongDuNoCaption, this.lblTongDuNo,
				this.lblTongTienGuiCaption, this.lblTongTienGui, this.lblTongSoDuTTCaption, this.lblTongSoDuTT
			});

			// ===== panelGD =====
			this.panelGD.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.panelGD.Location = new System.Drawing.Point(860, 200);
			this.panelGD.Size = new System.Drawing.Size(300, 180);
			this.lblGDTitle.Text = "📋 GIAO DỊCH";
			this.lblGDTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
			this.lblGDTitle.ForeColor = System.Drawing.Color.White;
			this.lblGDTitle.Location = new System.Drawing.Point(15, 12);
			this.lblGDTitle.AutoSize = true;

			this.lblSoGDVayCaption.Text = "GD Vay:";
			this.lblSoGDVayCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblSoGDVayCaption.Location = new System.Drawing.Point(15, 50);
			this.lblSoGDVayCaption.AutoSize = true;
			this.lblSoGDVay.Text = "0";
			this.lblSoGDVay.ForeColor = System.Drawing.Color.White;
			this.lblSoGDVay.Location = new System.Drawing.Point(150, 50);
			this.lblSoGDVay.AutoSize = true;

			this.lblSoGDTKCaption.Text = "GD Tiết kiệm:";
			this.lblSoGDTKCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblSoGDTKCaption.Location = new System.Drawing.Point(15, 80);
			this.lblSoGDTKCaption.AutoSize = true;
			this.lblSoGDTK.Text = "0";
			this.lblSoGDTK.ForeColor = System.Drawing.Color.White;
			this.lblSoGDTK.Location = new System.Drawing.Point(150, 80);
			this.lblSoGDTK.AutoSize = true;

			this.lblSoGDTTCaption.Text = "GD Thanh toán:";
			this.lblSoGDTTCaption.ForeColor = System.Drawing.Color.FromArgb(160, 174, 192);
			this.lblSoGDTTCaption.Location = new System.Drawing.Point(15, 110);
			this.lblSoGDTTCaption.AutoSize = true;
			this.lblSoGDTT.Text = "0";
			this.lblSoGDTT.ForeColor = System.Drawing.Color.White;
			this.lblSoGDTT.Location = new System.Drawing.Point(150, 110);
			this.lblSoGDTT.AutoSize = true;

			this.lblTongGDCaption.Text = "TỔNG GD:";
			this.lblTongGDCaption.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblTongGDCaption.ForeColor = System.Drawing.Color.White;
			this.lblTongGDCaption.Location = new System.Drawing.Point(15, 145);
			this.lblTongGDCaption.AutoSize = true;
			this.lblTongGD.Text = "0";
			this.lblTongGD.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
			this.lblTongGD.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246);
			this.lblTongGD.Location = new System.Drawing.Point(150, 142);
			this.lblTongGD.AutoSize = true;

			this.panelGD.Controls.AddRange(new System.Windows.Forms.Control[] {
				this.lblGDTitle, this.lblSoGDVayCaption, this.lblSoGDVay,
				this.lblSoGDTKCaption, this.lblSoGDTK, this.lblSoGDTTCaption, this.lblSoGDTT,
				this.lblTongGDCaption, this.lblTongGD
			});

			// ===== Grids =====
			this.lblTrangThaiTitle.Text = "Trạng thái tài khoản";
			this.lblTrangThaiTitle.ForeColor = System.Drawing.Color.White;
			this.lblTrangThaiTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblTrangThaiTitle.Location = new System.Drawing.Point(20, 400);
			this.lblTrangThaiTitle.AutoSize = true;

			this.dgvTrangThai.Location = new System.Drawing.Point(20, 425);
			this.dgvTrangThai.Size = new System.Drawing.Size(400, 150);
			this.dgvTrangThai.BackgroundColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.dgvTrangThai.ForeColor = System.Drawing.Color.White;
			this.dgvTrangThai.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.dgvTrangThai.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
			this.dgvTrangThai.EnableHeadersVisualStyles = false;
			this.dgvTrangThai.RowHeadersVisible = false;
			this.dgvTrangThai.AllowUserToAddRows = false;
			this.dgvTrangThai.ReadOnly = true;
			this.dgvTrangThai.Columns.Add("Loai", "Loại TK");
			this.dgvTrangThai.Columns.Add("HoatDong", "Hoạt động");
			this.dgvTrangThai.Columns.Add("Khoa", "Khóa");
			this.dgvTrangThai.Columns.Add("Dong", "Đóng");
			this.dgvTrangThai.Columns.Add("Tong", "Tổng");

			this.lblTopLaiTitle.Text = "Top doanh nghiệp trả lãi vay";
			this.lblTopLaiTitle.ForeColor = System.Drawing.Color.White;
			this.lblTopLaiTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblTopLaiTitle.Location = new System.Drawing.Point(440, 400);
			this.lblTopLaiTitle.AutoSize = true;

			this.dgvTopLai.Location = new System.Drawing.Point(440, 425);
			this.dgvTopLai.Size = new System.Drawing.Size(400, 150);
			this.dgvTopLai.BackgroundColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.dgvTopLai.ForeColor = System.Drawing.Color.White;
			this.dgvTopLai.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.dgvTopLai.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
			this.dgvTopLai.EnableHeadersVisualStyles = false;
			this.dgvTopLai.RowHeadersVisible = false;
			this.dgvTopLai.AllowUserToAddRows = false;
			this.dgvTopLai.ReadOnly = true;
			this.dgvTopLai.Columns.Add("STT", "STT");
			this.dgvTopLai.Columns.Add("SoTK", "Số TK Vay");
			this.dgvTopLai.Columns.Add("HoTen", "Tên doanh nghiệp");
			this.dgvTopLai.Columns.Add("Lai", "Tổng lãi thu");

			this.lblKyHanTitle.Text = "Phân bổ theo kỳ hạn vay";
			this.lblKyHanTitle.ForeColor = System.Drawing.Color.White;
			this.lblKyHanTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblKyHanTitle.Location = new System.Drawing.Point(860, 400);
			this.lblKyHanTitle.AutoSize = true;

			this.dgvKyHan.Location = new System.Drawing.Point(860, 425);
			this.dgvKyHan.Size = new System.Drawing.Size(300, 150);
			this.dgvKyHan.BackgroundColor = System.Drawing.Color.FromArgb(30, 41, 59);
			this.dgvKyHan.ForeColor = System.Drawing.Color.White;
			this.dgvKyHan.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.dgvKyHan.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
			this.dgvKyHan.EnableHeadersVisualStyles = false;
			this.dgvKyHan.RowHeadersVisible = false;
			this.dgvKyHan.AllowUserToAddRows = false;
			this.dgvKyHan.ReadOnly = true;
			this.dgvKyHan.Columns.Add("KyHan", "Kỳ hạn");
			this.dgvKyHan.Columns.Add("SoLuong", "Số lượng");
			this.dgvKyHan.Columns.Add("TongTien", "Tổng tiền vay");

			// ===== Form =====
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
			this.ClientSize = new System.Drawing.Size(1200, 700);
			this.Controls.Add(this.dgvKyHan);
			this.Controls.Add(this.lblKyHanTitle);
			this.Controls.Add(this.dgvTopLai);
			this.Controls.Add(this.lblTopLaiTitle);
			this.Controls.Add(this.dgvTrangThai);
			this.Controls.Add(this.lblTrangThaiTitle);
			this.Controls.Add(this.panelGD);
			this.Controls.Add(this.panelTien);
			this.Controls.Add(this.panelDoanhThu);
			this.Controls.Add(this.panelCards);
			this.Controls.Add(this.panelTop);
			this.Name = "FrmThongKeDN";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Thống kê doanh nghiệp & doanh thu";

			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.panelCards.ResumeLayout(false);
			this.cardKH.ResumeLayout(false);
			this.cardKH.PerformLayout();
			this.cardVay.ResumeLayout(false);
			this.cardVay.PerformLayout();
			this.cardTK.ResumeLayout(false);
			this.cardTK.PerformLayout();
			this.cardTT.ResumeLayout(false);
			this.cardTT.PerformLayout();
			this.panelDoanhThu.ResumeLayout(false);
			this.panelDoanhThu.PerformLayout();
			this.panelTien.ResumeLayout(false);
			this.panelTien.PerformLayout();
			this.panelGD.ResumeLayout(false);
			this.panelGD.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTrangThai)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvTopLai)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgvKyHan)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Panel panelTop;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblMoc;
		private System.Windows.Forms.ComboBox cboMocThoiGian;
		private System.Windows.Forms.Label lblTu;
		private System.Windows.Forms.DateTimePicker dtpTuNgay;
		private System.Windows.Forms.Label lblDen;
		private System.Windows.Forms.DateTimePicker dtpDenNgay;
		private System.Windows.Forms.Button btnLamMoi;

		private System.Windows.Forms.Panel panelCards;
		private System.Windows.Forms.Panel cardKH, cardVay, cardTK, cardTT;
		private System.Windows.Forms.Label lblTongKH, lblKHTitle;
		private System.Windows.Forms.Label lblTongTKVay, lblVayTitle;
		private System.Windows.Forms.Label lblTongTKTK, lblTKTitle;
		private System.Windows.Forms.Label lblTongTKTT, lblTTTitle;

		private System.Windows.Forms.Panel panelDoanhThu;
		private System.Windows.Forms.Label lblDoanhThuTitle;
		private System.Windows.Forms.Label lblLaiThuVayCaption, lblLaiThuVay;
		private System.Windows.Forms.Label lblLaiChiTKCaption, lblLaiChiTK;
		private System.Windows.Forms.Label lblDoanhThuRongCaption, lblDoanhThuRong;

		private System.Windows.Forms.Panel panelTien;
		private System.Windows.Forms.Label lblTienTitle;
		private System.Windows.Forms.Label lblTongTienVayCaption, lblTongTienVay;
		private System.Windows.Forms.Label lblTongDaTraCaption, lblTongDaTra;
		private System.Windows.Forms.Label lblTongDuNoCaption, lblTongDuNo;
		private System.Windows.Forms.Label lblTongTienGuiCaption, lblTongTienGui;
		private System.Windows.Forms.Label lblTongSoDuTTCaption, lblTongSoDuTT;

		private System.Windows.Forms.Panel panelGD;
		private System.Windows.Forms.Label lblGDTitle;
		private System.Windows.Forms.Label lblSoGDVayCaption, lblSoGDVay;
		private System.Windows.Forms.Label lblSoGDTKCaption, lblSoGDTK;
		private System.Windows.Forms.Label lblSoGDTTCaption, lblSoGDTT;
		private System.Windows.Forms.Label lblTongGDCaption, lblTongGD;

		private System.Windows.Forms.DataGridView dgvTrangThai, dgvTopLai, dgvKyHan;
		private System.Windows.Forms.Label lblTrangThaiTitle, lblTopLaiTitle, lblKyHanTitle;
	}
}