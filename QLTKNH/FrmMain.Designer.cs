using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLTKNH
{
	partial class FrmMain
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
			System.Windows.Forms.DataGridViewCellStyle dgvKhachHangCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
			this.panelSidebar = new System.Windows.Forms.Panel();
			this.panelMenu = new System.Windows.Forms.Panel();
			this.tableTabSwitcher = new System.Windows.Forms.TableLayoutPanel();
			this.btnCaNhan = new System.Windows.Forms.Button();
			this.btnDoanhNghiep = new System.Windows.Forms.Button();
			this.lblTitle = new System.Windows.Forms.Label();
			this.panelContent = new System.Windows.Forms.Panel();
			this.panelMainLayout = new System.Windows.Forms.Panel();
			this.panelRightContainer = new System.Windows.Forms.Panel();
			this.pnlThanhToanGroup = new System.Windows.Forms.Panel();
			this.dgvThanhToan = new System.Windows.Forms.DataGridView();
			this.colTTSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTTSoTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTTSoDu = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTTTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTTVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblThanhToanTitle = new System.Windows.Forms.Label();
			this.lblTotalTTHeader = new System.Windows.Forms.Label();
			this.pnlTietKiemGroup = new System.Windows.Forms.Panel();
			this.dgvTietKiem = new System.Windows.Forms.DataGridView();
			this.colTKSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKSoTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKSoTienGui = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKLaiSuat = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKSoTienDaRut = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKKyHan = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTKVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblTietKiemTitle = new System.Windows.Forms.Label();
			this.lblTotalTKHeader = new System.Windows.Forms.Label();
			this.pnlVayGroup = new System.Windows.Forms.Panel();
			this.dgvVay = new System.Windows.Forms.DataGridView();
			this.colVaySTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVaySoTK = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVaySoTien = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVaySoTienDaTra = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVayLaiSuat = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVayKyHan = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVayTrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colVayVaiTro = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblVayTitle = new System.Windows.Forms.Label();
			this.lblTotalVayHeader = new System.Windows.Forms.Label();
			this.lblInfoSelectedCustomer = new System.Windows.Forms.Label();
			this.panelLeftContainer = new System.Windows.Forms.Panel();
			this.panelLeftTop = new System.Windows.Forms.Panel();
			this.dgvKhachHang = new System.Windows.Forms.DataGridView();
			this.colSTT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colCCCD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colHoTen = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colSDT = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.lblHeaderDSKH = new System.Windows.Forms.Label();
			this.panelTopSearch = new System.Windows.Forms.Panel();
			this.btnTimCCCD = new System.Windows.Forms.Button();
			this.txtCCCDSearch = new System.Windows.Forms.TextBox();
			this.lblCCCDLabel = new System.Windows.Forms.Label();
			this.panelSidebar.SuspendLayout();
			this.tableTabSwitcher.SuspendLayout();
			this.panelContent.SuspendLayout();
			this.panelMainLayout.SuspendLayout();
			this.panelRightContainer.SuspendLayout();
			this.pnlThanhToanGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvThanhToan)).BeginInit();
			this.pnlTietKiemGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvTietKiem)).BeginInit();
			this.pnlVayGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvVay)).BeginInit();
			this.panelLeftContainer.SuspendLayout();
			this.panelLeftTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvKhachHang)).BeginInit();
			this.panelTopSearch.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelSidebar
			// 
			this.panelSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(19)))), ((int)(((byte)(43)))));
			this.panelSidebar.Controls.Add(this.panelMenu);
			this.panelSidebar.Controls.Add(this.tableTabSwitcher);
			this.panelSidebar.Controls.Add(this.lblTitle);
			this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelSidebar.Location = new System.Drawing.Point(0, 0);
			this.panelSidebar.Name = "panelSidebar";
			this.panelSidebar.Padding = new System.Windows.Forms.Padding(12);
			this.panelSidebar.Size = new System.Drawing.Size(320, 750);
			this.panelSidebar.TabIndex = 0;
			// 
			// panelMenu
			// 
			this.panelMenu.AutoScroll = true;
			this.panelMenu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMenu.Location = new System.Drawing.Point(12, 107);
			this.panelMenu.Name = "panelMenu";
			this.panelMenu.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.panelMenu.Size = new System.Drawing.Size(296, 631);
			this.panelMenu.TabIndex = 2;
			// 
			// tableTabSwitcher
			// 
			this.tableTabSwitcher.ColumnCount = 2;
			this.tableTabSwitcher.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableTabSwitcher.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableTabSwitcher.Controls.Add(this.btnCaNhan, 0, 0);
			this.tableTabSwitcher.Controls.Add(this.btnDoanhNghiep, 1, 0);
			this.tableTabSwitcher.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableTabSwitcher.Location = new System.Drawing.Point(12, 62);
			this.tableTabSwitcher.Name = "tableTabSwitcher";
			this.tableTabSwitcher.RowCount = 1;
			this.tableTabSwitcher.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableTabSwitcher.Size = new System.Drawing.Size(296, 45);
			this.tableTabSwitcher.TabIndex = 1;
			// 
			// btnCaNhan
			// 
			this.btnCaNhan.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnCaNhan.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnCaNhan.FlatAppearance.BorderSize = 0;
			this.btnCaNhan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCaNhan.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
			this.btnCaNhan.ForeColor = System.Drawing.Color.White;
			this.btnCaNhan.Location = new System.Drawing.Point(0, 0);
			this.btnCaNhan.Margin = new System.Windows.Forms.Padding(0);
			this.btnCaNhan.Name = "btnCaNhan";
			this.btnCaNhan.Size = new System.Drawing.Size(148, 45);
			this.btnCaNhan.TabIndex = 0;
			this.btnCaNhan.Text = "👤 Cá Nhân";
			this.btnCaNhan.UseVisualStyleBackColor = true;
			this.btnCaNhan.Click += new System.EventHandler(this.btnCaNhan_Click);
			// 
			// btnDoanhNghiep
			// 
			this.btnDoanhNghiep.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnDoanhNghiep.Dock = System.Windows.Forms.DockStyle.Fill;
			this.btnDoanhNghiep.FlatAppearance.BorderSize = 0;
			this.btnDoanhNghiep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDoanhNghiep.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
			this.btnDoanhNghiep.ForeColor = System.Drawing.Color.White;
			this.btnDoanhNghiep.Location = new System.Drawing.Point(148, 0);
			this.btnDoanhNghiep.Margin = new System.Windows.Forms.Padding(0);
			this.btnDoanhNghiep.Name = "btnDoanhNghiep";
			this.btnDoanhNghiep.Size = new System.Drawing.Size(148, 45);
			this.btnDoanhNghiep.TabIndex = 1;
			this.btnDoanhNghiep.Text = "🏢 Doanh Nghiệp";
			this.btnDoanhNghiep.UseVisualStyleBackColor = true;
			this.btnDoanhNghiep.Click += new System.EventHandler(this.btnDoanhNghiep_Click);
			// 
			// lblTitle
			// 
			this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Location = new System.Drawing.Point(12, 12);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(296, 50);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "BANKING SYSTEM";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelContent
			// 
			this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.panelContent.Controls.Add(this.panelMainLayout);
			this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelContent.Location = new System.Drawing.Point(320, 0);
			this.panelContent.Name = "panelContent";
			this.panelContent.Size = new System.Drawing.Size(1262, 750);
			this.panelContent.TabIndex = 1;
			// 
			// panelMainLayout
			// 
			this.panelMainLayout.Controls.Add(this.panelRightContainer);
			this.panelMainLayout.Controls.Add(this.panelLeftContainer);
			this.panelMainLayout.Controls.Add(this.panelTopSearch);
			this.panelMainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMainLayout.Location = new System.Drawing.Point(0, 0);
			this.panelMainLayout.Name = "panelMainLayout";
			this.panelMainLayout.Size = new System.Drawing.Size(1262, 750);
			this.panelMainLayout.TabIndex = 0;
			// 
			// panelRightContainer
			// 
			this.panelRightContainer.AutoScroll = true;
			this.panelRightContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
			this.panelRightContainer.Controls.Add(this.pnlThanhToanGroup);
			this.panelRightContainer.Controls.Add(this.pnlTietKiemGroup);
			this.panelRightContainer.Controls.Add(this.pnlVayGroup);
			this.panelRightContainer.Controls.Add(this.lblInfoSelectedCustomer);
			this.panelRightContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelRightContainer.Location = new System.Drawing.Point(480, 50);
			this.panelRightContainer.Name = "panelRightContainer";
			this.panelRightContainer.Padding = new System.Windows.Forms.Padding(15);
			this.panelRightContainer.Size = new System.Drawing.Size(782, 700);
			this.panelRightContainer.TabIndex = 2;
			// 
			// pnlThanhToanGroup
			// 
			this.pnlThanhToanGroup.Controls.Add(this.dgvThanhToan);
			this.pnlThanhToanGroup.Controls.Add(this.lblThanhToanTitle);
			this.pnlThanhToanGroup.Controls.Add(this.lblTotalTTHeader);
			this.pnlThanhToanGroup.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlThanhToanGroup.Location = new System.Drawing.Point(15, 425);
			this.pnlThanhToanGroup.Name = "pnlThanhToanGroup";
			this.pnlThanhToanGroup.Padding = new System.Windows.Forms.Padding(0, 0, 0, 15);
			this.pnlThanhToanGroup.Size = new System.Drawing.Size(752, 240);
			this.pnlThanhToanGroup.TabIndex = 3;
			// 
			// dgvThanhToan
			// 
			this.dgvThanhToan.AllowUserToAddRows = false;
			this.dgvThanhToan.AllowUserToDeleteRows = false;
			this.dgvThanhToan.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvThanhToan.BackgroundColor = System.Drawing.Color.White;
			this.dgvThanhToan.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvThanhToan.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colTTSTT,
			this.colTTSoTK,
			this.colTTSoDu,
			this.colTTTrangThai,
			this.colTTVaiTro});
			this.dgvThanhToan.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvThanhToan.Location = new System.Drawing.Point(0, 30);
			this.dgvThanhToan.MultiSelect = true;
			this.dgvThanhToan.Name = "dgvThanhToan";
			this.dgvThanhToan.ReadOnly = true;
			this.dgvThanhToan.RowHeadersVisible = false;
			this.dgvThanhToan.RowHeadersWidth = 51;
			this.dgvThanhToan.RowTemplate.Height = 24;
			this.dgvThanhToan.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvThanhToan.Size = new System.Drawing.Size(752, 195);
			this.dgvThanhToan.TabIndex = 1;
			// 
			// colTTSTT
			// 
			this.colTTSTT.FillWeight = 28F;
			this.colTTSTT.HeaderText = "STT";
			this.colTTSTT.MinimumWidth = 6;
			this.colTTSTT.Name = "colTTSTT";
			this.colTTSTT.ReadOnly = true;
			// 
			// colTTSoTK
			// 
			this.colTTSoTK.FillWeight = 85F;
			this.colTTSoTK.HeaderText = "Số TK TT";
			this.colTTSoTK.MinimumWidth = 6;
			this.colTTSoTK.Name = "colTTSoTK";
			this.colTTSoTK.ReadOnly = true;
			// 
			// colTTSoDu
			// 
			this.colTTSoDu.FillWeight = 110F;
			this.colTTSoDu.HeaderText = "Số tiền dùng có (Số dư)";
			this.colTTSoDu.MinimumWidth = 6;
			this.colTTSoDu.Name = "colTTSoDu";
			this.colTTSoDu.ReadOnly = true;
			// 
			// colTTTrangThai
			// 
			this.colTTTrangThai.FillWeight = 75F;
			this.colTTTrangThai.HeaderText = "Trạng thái thẻ";
			this.colTTTrangThai.MinimumWidth = 6;
			this.colTTTrangThai.Name = "colTTTrangThai";
			this.colTTTrangThai.ReadOnly = true;
			// 
			// colTTVaiTro
			// 
			this.colTTVaiTro.FillWeight = 185F;
			this.colTTVaiTro.HeaderText = "Vai trò";
			this.colTTVaiTro.MinimumWidth = 6;
			this.colTTVaiTro.Name = "colTTVaiTro";
			this.colTTVaiTro.ReadOnly = true;
			// 
			// lblThanhToanTitle
			// 
			this.lblThanhToanTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblThanhToanTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblThanhToanTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.lblThanhToanTitle.Location = new System.Drawing.Point(0, 0);
			this.lblThanhToanTitle.Name = "lblThanhToanTitle";
			this.lblThanhToanTitle.Size = new System.Drawing.Size(752, 30);
			this.lblThanhToanTitle.TabIndex = 0;
			this.lblThanhToanTitle.Text = "3. TÀI KHOẢN THANH TOÁN";
			this.lblThanhToanTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblTotalTTHeader
			// 
			this.lblTotalTTHeader.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblTotalTTHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			this.lblTotalTTHeader.Location = new System.Drawing.Point(260, 0);
			this.lblTotalTTHeader.Name = "lblTotalTTHeader";
			this.lblTotalTTHeader.Size = new System.Drawing.Size(490, 30);
			this.lblTotalTTHeader.TabIndex = 2;
			this.lblTotalTTHeader.Text = "Số TK Thanh toán: 0 | Tổng số tiền dùng có: 0 VNĐ";
			this.lblTotalTTHeader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pnlTietKiemGroup
			// 
			this.pnlTietKiemGroup.Controls.Add(this.dgvTietKiem);
			this.pnlTietKiemGroup.Controls.Add(this.lblTietKiemTitle);
			this.pnlTietKiemGroup.Controls.Add(this.lblTotalTKHeader);
			this.pnlTietKiemGroup.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlTietKiemGroup.Location = new System.Drawing.Point(15, 245);
			this.pnlTietKiemGroup.Name = "pnlTietKiemGroup";
			this.pnlTietKiemGroup.Padding = new System.Windows.Forms.Padding(0, 0, 0, 15);
			this.pnlTietKiemGroup.Size = new System.Drawing.Size(752, 180);
			this.pnlTietKiemGroup.TabIndex = 2;
			// 
			// dgvTietKiem
			// 
			this.dgvTietKiem.AllowUserToAddRows = false;
			this.dgvTietKiem.AllowUserToDeleteRows = false;
			this.dgvTietKiem.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvTietKiem.BackgroundColor = System.Drawing.Color.White;
			this.dgvTietKiem.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvTietKiem.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colTKSTT,
			this.colTKSoTK,
			this.colTKSoTienGui,
			this.colTKLaiSuat,
			this.colTKSoTienDaRut,
			this.colTKKyHan,
			this.colTKTrangThai,
			this.colTKVaiTro});
			this.dgvTietKiem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvTietKiem.Location = new System.Drawing.Point(0, 30);
			this.dgvTietKiem.MultiSelect = true;
			this.dgvTietKiem.Name = "dgvTietKiem";
			this.dgvTietKiem.ReadOnly = true;
			this.dgvTietKiem.RowHeadersVisible = false;
			this.dgvTietKiem.RowHeadersWidth = 51;
			this.dgvTietKiem.RowTemplate.Height = 24;
			this.dgvTietKiem.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvTietKiem.Size = new System.Drawing.Size(752, 135);
			this.dgvTietKiem.TabIndex = 1;
			this.dgvTietKiem.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTietKiem_CellContentClick);
			// 
			// colTKSTT
			// 
			this.colTKSTT.FillWeight = 28F;
			this.colTKSTT.HeaderText = "STT";
			this.colTKSTT.MinimumWidth = 6;
			this.colTKSTT.Name = "colTKSTT";
			this.colTKSTT.ReadOnly = true;
			// 
			// colTKSoTK
			// 
			this.colTKSoTK.FillWeight = 72F;
			this.colTKSoTK.HeaderText = "Số TK TK";
			this.colTKSoTK.MinimumWidth = 6;
			this.colTKSoTK.Name = "colTKSoTK";
			this.colTKSoTK.ReadOnly = true;
			// 
			// colTKSoTienGui
			// 
			this.colTKSoTienGui.FillWeight = 85F;
			this.colTKSoTienGui.HeaderText = "Số tiền gửi";
			this.colTKSoTienGui.MinimumWidth = 6;
			this.colTKSoTienGui.Name = "colTKSoTienGui";
			this.colTKSoTienGui.ReadOnly = true;
			// 
			// colTKLaiSuat
			// 
			this.colTKLaiSuat.FillWeight = 58F;
			this.colTKLaiSuat.HeaderText = "Lãi suất";
			this.colTKLaiSuat.MinimumWidth = 6;
			this.colTKLaiSuat.Name = "colTKLaiSuat";
			this.colTKLaiSuat.ReadOnly = true;
			// 
			// colTKSoTienDaRut
			// 
			this.colTKSoTienDaRut.FillWeight = 80F;
			this.colTKSoTienDaRut.HeaderText = "Số tiền đã rút";
			this.colTKSoTienDaRut.MinimumWidth = 6;
			this.colTKSoTienDaRut.Name = "colTKSoTienDaRut";
			this.colTKSoTienDaRut.ReadOnly = true;
			// 
			// colTKKyHan
			// 
			this.colTKKyHan.FillWeight = 58F;
			this.colTKKyHan.HeaderText = "Kỳ hạn";
			this.colTKKyHan.MinimumWidth = 6;
			this.colTKKyHan.Name = "colTKKyHan";
			this.colTKKyHan.ReadOnly = true;
			// 
			// colTKTrangThai
			// 
			this.colTKTrangThai.FillWeight = 68F;
			this.colTKTrangThai.HeaderText = "Trạng thái";
			this.colTKTrangThai.MinimumWidth = 6;
			this.colTKTrangThai.Name = "colTKTrangThai";
			this.colTKTrangThai.ReadOnly = true;
			// 
			// colTKVaiTro
			// 
			this.colTKVaiTro.FillWeight = 175F;
			this.colTKVaiTro.HeaderText = "Vai trò";
			this.colTKVaiTro.MinimumWidth = 6;
			this.colTKVaiTro.Name = "colTKVaiTro";
			this.colTKVaiTro.ReadOnly = true;
			// 
			// lblTietKiemTitle
			// 
			this.lblTietKiemTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTietKiemTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblTietKiemTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.lblTietKiemTitle.Location = new System.Drawing.Point(0, 0);
			this.lblTietKiemTitle.Name = "lblTietKiemTitle";
			this.lblTietKiemTitle.Size = new System.Drawing.Size(752, 30);
			this.lblTietKiemTitle.TabIndex = 0;
			this.lblTietKiemTitle.Text = "2. SỔ TIẾT KIỆM";
			this.lblTietKiemTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblTotalTKHeader
			// 
			this.lblTotalTKHeader.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblTotalTKHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			this.lblTotalTKHeader.Location = new System.Drawing.Point(260, 0);
			this.lblTotalTKHeader.Name = "lblTotalTKHeader";
			this.lblTotalTKHeader.Size = new System.Drawing.Size(490, 30);
			this.lblTotalTKHeader.TabIndex = 2;
			this.lblTotalTKHeader.Text = "Số TK Tiết kiệm: 0 | Tổng số tiền TK: 0 VNĐ";
			this.lblTotalTKHeader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// pnlVayGroup
			// 
			this.pnlVayGroup.Controls.Add(this.dgvVay);
			this.pnlVayGroup.Controls.Add(this.lblVayTitle);
			this.pnlVayGroup.Controls.Add(this.lblTotalVayHeader);
			this.pnlVayGroup.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlVayGroup.Location = new System.Drawing.Point(15, 75);
			this.pnlVayGroup.Name = "pnlVayGroup";
			this.pnlVayGroup.Padding = new System.Windows.Forms.Padding(0, 0, 0, 15);
			this.pnlVayGroup.Size = new System.Drawing.Size(752, 170);
			this.pnlVayGroup.TabIndex = 1;
			// 
			// dgvVay
			// 
			this.dgvVay.AllowUserToAddRows = false;
			this.dgvVay.AllowUserToDeleteRows = false;
			this.dgvVay.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvVay.BackgroundColor = System.Drawing.Color.White;
			this.dgvVay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvVay.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colVaySTT,
			this.colVaySoTK,
			this.colVaySoTien,
			this.colVaySoTienDaTra,
			this.colVayLaiSuat,
			this.colVayKyHan,
			this.colVayTrangThai,
			this.colVayVaiTro});
			this.dgvVay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvVay.Location = new System.Drawing.Point(0, 30);
			this.dgvVay.MultiSelect = true;
			this.dgvVay.Name = "dgvVay";
			this.dgvVay.ReadOnly = true;
			this.dgvVay.RowHeadersVisible = false;
			this.dgvVay.RowHeadersWidth = 51;
			this.dgvVay.RowTemplate.Height = 24;
			this.dgvVay.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvVay.Size = new System.Drawing.Size(752, 125);
			this.dgvVay.TabIndex = 1;
			// 
			// colVaySTT
			// 
			this.colVaySTT.FillWeight = 28F;
			this.colVaySTT.HeaderText = "STT";
			this.colVaySTT.MinimumWidth = 6;
			this.colVaySTT.Name = "colVaySTT";
			this.colVaySTT.ReadOnly = true;
			// 
			// colVaySoTK
			// 
			this.colVaySoTK.FillWeight = 72F;
			this.colVaySoTK.HeaderText = "Số TK Vay";
			this.colVaySoTK.MinimumWidth = 6;
			this.colVaySoTK.Name = "colVaySoTK";
			this.colVaySoTK.ReadOnly = true;
			// 
			// colVaySoTien
			// 
			this.colVaySoTien.FillWeight = 85F;
			this.colVaySoTien.HeaderText = "Số tiền vay";
			this.colVaySoTien.MinimumWidth = 6;
			this.colVaySoTien.Name = "colVaySoTien";
			this.colVaySoTien.ReadOnly = true;
			// 
			// colVaySoTienDaTra
			// 
			this.colVaySoTienDaTra.FillWeight = 85F;
			this.colVaySoTienDaTra.HeaderText = "Số tiền đã trả";
			this.colVaySoTienDaTra.MinimumWidth = 6;
			this.colVaySoTienDaTra.Name = "colVaySoTienDaTra";
			this.colVaySoTienDaTra.ReadOnly = true;
			// 
			// colVayLaiSuat
			// 
			this.colVayLaiSuat.FillWeight = 58F;
			this.colVayLaiSuat.HeaderText = "Lãi suất";
			this.colVayLaiSuat.MinimumWidth = 6;
			this.colVayLaiSuat.Name = "colVayLaiSuat";
			this.colVayLaiSuat.ReadOnly = true;
			// 
			// colVayKyHan
			// 
			this.colVayKyHan.FillWeight = 58F;
			this.colVayKyHan.HeaderText = "Kỳ hạn";
			this.colVayKyHan.MinimumWidth = 6;
			this.colVayKyHan.Name = "colVayKyHan";
			this.colVayKyHan.ReadOnly = true;
			// 
			// colVayTrangThai
			// 
			this.colVayTrangThai.FillWeight = 68F;
			this.colVayTrangThai.HeaderText = "Trạng thái";
			this.colVayTrangThai.MinimumWidth = 6;
			this.colVayTrangThai.Name = "colVayTrangThai";
			this.colVayTrangThai.ReadOnly = true;
			// 
			// colVayVaiTro
			// 
			this.colVayVaiTro.FillWeight = 175F;
			this.colVayVaiTro.HeaderText = "Vai trò";
			this.colVayVaiTro.MinimumWidth = 6;
			this.colVayVaiTro.Name = "colVayVaiTro";
			this.colVayVaiTro.ReadOnly = true;
			// 
			// lblVayTitle
			// 
			this.lblVayTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblVayTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblVayTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.lblVayTitle.Location = new System.Drawing.Point(0, 0);
			this.lblVayTitle.Name = "lblVayTitle";
			this.lblVayTitle.Size = new System.Drawing.Size(752, 30);
			this.lblVayTitle.TabIndex = 0;
			this.lblVayTitle.Text = "1. TÀI KHOẢN VAY";
			this.lblVayTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblTotalVayHeader
			// 
			this.lblTotalVayHeader.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.lblTotalVayHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			this.lblTotalVayHeader.Location = new System.Drawing.Point(260, 0);
			this.lblTotalVayHeader.Name = "lblTotalVayHeader";
			this.lblTotalVayHeader.Size = new System.Drawing.Size(490, 30);
			this.lblTotalVayHeader.TabIndex = 2;
			this.lblTotalVayHeader.Text = "Số TK Vay: 0 | Tổng số tiền vay: 0 VNĐ";
			this.lblTotalVayHeader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblInfoSelectedCustomer
			// 
			this.lblInfoSelectedCustomer.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblInfoSelectedCustomer.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblInfoSelectedCustomer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.lblInfoSelectedCustomer.Location = new System.Drawing.Point(15, 15);
			this.lblInfoSelectedCustomer.Name = "lblInfoSelectedCustomer";
			this.lblInfoSelectedCustomer.Size = new System.Drawing.Size(752, 60);
			this.lblInfoSelectedCustomer.TabIndex = 0;
			this.lblInfoSelectedCustomer.Text = "THÔNG TIN KHÁCH HÀNG ĐANG CHỌN:";
			// 
			// panelLeftContainer
			// 
			this.panelLeftContainer.Controls.Add(this.panelLeftTop);
			this.panelLeftContainer.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelLeftContainer.Location = new System.Drawing.Point(0, 50);
			this.panelLeftContainer.Name = "panelLeftContainer";
			this.panelLeftContainer.Padding = new System.Windows.Forms.Padding(10);
			this.panelLeftContainer.Size = new System.Drawing.Size(480, 700);
			this.panelLeftContainer.TabIndex = 1;
			// 
			// panelLeftTop
			// 
			this.panelLeftTop.Controls.Add(this.dgvKhachHang);
			this.panelLeftTop.Controls.Add(this.lblHeaderDSKH);
			this.panelLeftTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelLeftTop.Location = new System.Drawing.Point(10, 10);
			this.panelLeftTop.Name = "panelLeftTop";
			this.panelLeftTop.Size = new System.Drawing.Size(460, 680);
			this.panelLeftTop.TabIndex = 0;
			// 
			// dgvKhachHang
			// 
			this.dgvKhachHang.AllowUserToAddRows = false;
			this.dgvKhachHang.AllowUserToDeleteRows = false;
			this.dgvKhachHang.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.dgvKhachHang.BackgroundColor = System.Drawing.Color.White;
			this.dgvKhachHang.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvKhachHang.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.colSTT,
			this.colCCCD,
			this.colHoTen,
			this.colSDT});
			dgvKhachHangCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dgvKhachHangCellStyle.BackColor = System.Drawing.Color.White;
			dgvKhachHangCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
			dgvKhachHangCellStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			dgvKhachHangCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			dgvKhachHangCellStyle.SelectionForeColor = System.Drawing.Color.White;
			dgvKhachHangCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dgvKhachHang.DefaultCellStyle = dgvKhachHangCellStyle;
			this.dgvKhachHang.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvKhachHang.Location = new System.Drawing.Point(0, 30);
			this.dgvKhachHang.MultiSelect = false;
			this.dgvKhachHang.Name = "dgvKhachHang";
			this.dgvKhachHang.ReadOnly = true;
			this.dgvKhachHang.RowHeadersVisible = false;
			this.dgvKhachHang.RowHeadersWidth = 51;
			this.dgvKhachHang.RowTemplate.Height = 24;
			this.dgvKhachHang.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvKhachHang.Size = new System.Drawing.Size(460, 650);
			this.dgvKhachHang.TabIndex = 1;
			// 
			// colSTT
			// 
			this.colSTT.FillWeight = 25F;
			this.colSTT.HeaderText = "STT";
			this.colSTT.MinimumWidth = 6;
			this.colSTT.Name = "colSTT";
			this.colSTT.ReadOnly = true;
			// 
			// colCCCD
			// 
			this.colCCCD.FillWeight = 80F;
			this.colCCCD.HeaderText = "CCCD";
			this.colCCCD.MinimumWidth = 6;
			this.colCCCD.Name = "colCCCD";
			this.colCCCD.ReadOnly = true;
			// 
			// colHoTen
			// 
			this.colHoTen.FillWeight = 90F;
			this.colHoTen.HeaderText = "Họ và Tên";
			this.colHoTen.MinimumWidth = 6;
			this.colHoTen.Name = "colHoTen";
			this.colHoTen.ReadOnly = true;
			// 
			// colSDT
			// 
			this.colSDT.FillWeight = 70F;
			this.colSDT.HeaderText = "Số điện thoại";
			this.colSDT.MinimumWidth = 6;
			this.colSDT.Name = "colSDT";
			this.colSDT.ReadOnly = true;
			// 
			// lblHeaderDSKH
			// 
			this.lblHeaderDSKH.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblHeaderDSKH.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
			this.lblHeaderDSKH.ForeColor = System.Drawing.Color.White;
			this.lblHeaderDSKH.Location = new System.Drawing.Point(0, 0);
			this.lblHeaderDSKH.Name = "lblHeaderDSKH";
			this.lblHeaderDSKH.Size = new System.Drawing.Size(460, 30);
			this.lblHeaderDSKH.TabIndex = 0;
			this.lblHeaderDSKH.Text = "DANH SÁCH KHÁCH HÀNG";
			this.lblHeaderDSKH.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelTopSearch
			// 
			this.panelTopSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(33)))), ((int)(((byte)(60)))));
			this.panelTopSearch.Controls.Add(this.btnTimCCCD);
			this.panelTopSearch.Controls.Add(this.txtCCCDSearch);
			this.panelTopSearch.Controls.Add(this.lblCCCDLabel);
			this.panelTopSearch.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTopSearch.Location = new System.Drawing.Point(0, 0);
			this.panelTopSearch.Name = "panelTopSearch";
			this.panelTopSearch.Size = new System.Drawing.Size(1262, 50);
			this.panelTopSearch.TabIndex = 0;
			this.panelTopSearch.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTopSearch_Paint);
			// 
			// btnTimCCCD
			// 
			this.btnTimCCCD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
			this.btnTimCCCD.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnTimCCCD.FlatAppearance.BorderSize = 0;
			this.btnTimCCCD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnTimCCCD.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.btnTimCCCD.ForeColor = System.Drawing.Color.White;
			this.btnTimCCCD.Location = new System.Drawing.Point(400, 10);
			this.btnTimCCCD.Name = "btnTimCCCD";
			this.btnTimCCCD.Size = new System.Drawing.Size(100, 30);
			this.btnTimCCCD.TabIndex = 2;
			this.btnTimCCCD.Text = "🔍 Tìm";
			this.btnTimCCCD.UseVisualStyleBackColor = false;
			// 
			// txtCCCDSearch
			// 
			this.txtCCCDSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.txtCCCDSearch.Location = new System.Drawing.Point(140, 10);
			this.txtCCCDSearch.MaxLength = 12;
			this.txtCCCDSearch.Name = "txtCCCDSearch";
			this.txtCCCDSearch.Size = new System.Drawing.Size(250, 30);
			this.txtCCCDSearch.TabIndex = 1;
			// 
			// lblCCCDLabel
			// 
			this.lblCCCDLabel.AutoSize = true;
			this.lblCCCDLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			this.lblCCCDLabel.ForeColor = System.Drawing.Color.White;
			this.lblCCCDLabel.Location = new System.Drawing.Point(15, 13);
			this.lblCCCDLabel.Name = "lblCCCDLabel";
			this.lblCCCDLabel.Size = new System.Drawing.Size(107, 23);
			this.lblCCCDLabel.TabIndex = 0;
			this.lblCCCDLabel.Text = "Nhập CCCD:";
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
			this.ClientSize = new System.Drawing.Size(1582, 750);
			this.Controls.Add(this.panelContent);
			this.Controls.Add(this.panelSidebar);
			this.Name = "FrmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "HỆ THỐNG QUẢN LÝ NGÂN HÀNG & GIAO DỊCH";
			this.panelSidebar.ResumeLayout(false);
			this.tableTabSwitcher.ResumeLayout(false);
			this.panelContent.ResumeLayout(false);
			this.panelMainLayout.ResumeLayout(false);
			this.panelRightContainer.ResumeLayout(false);
			this.pnlThanhToanGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvThanhToan)).EndInit();
			this.pnlTietKiemGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvTietKiem)).EndInit();
			this.pnlVayGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvVay)).EndInit();
			this.panelLeftContainer.ResumeLayout(false);
			this.panelLeftTop.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvKhachHang)).EndInit();
			this.panelTopSearch.ResumeLayout(false);
			this.panelTopSearch.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel panelSidebar;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.TableLayoutPanel tableTabSwitcher;
		private System.Windows.Forms.Button btnCaNhan;
		private System.Windows.Forms.Button btnDoanhNghiep;
		private System.Windows.Forms.Panel panelMenu;
		private System.Windows.Forms.Panel panelContent;
		private System.Windows.Forms.Panel panelMainLayout;
		private System.Windows.Forms.Panel panelTopSearch;
		private System.Windows.Forms.Label lblCCCDLabel;
		private System.Windows.Forms.TextBox txtCCCDSearch;
		private System.Windows.Forms.Button btnTimCCCD;
		private System.Windows.Forms.Panel panelLeftContainer;
		private System.Windows.Forms.Panel panelLeftTop;
		private System.Windows.Forms.Label lblHeaderDSKH;
		private System.Windows.Forms.DataGridView dgvKhachHang;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colCCCD;
		private System.Windows.Forms.DataGridViewTextBoxColumn colHoTen;
		private System.Windows.Forms.DataGridViewTextBoxColumn colSDT;
		private System.Windows.Forms.Panel panelRightContainer;
		private System.Windows.Forms.Label lblInfoSelectedCustomer;
		private System.Windows.Forms.Panel pnlVayGroup;
		private System.Windows.Forms.Label lblVayTitle;
		private System.Windows.Forms.Label lblTotalVayHeader;
		private System.Windows.Forms.DataGridView dgvVay;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVaySTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVaySoTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVaySoTien;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVaySoTienDaTra;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVayLaiSuat;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVayKyHan;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVayTrangThai;
		private System.Windows.Forms.DataGridViewTextBoxColumn colVayVaiTro;
		private System.Windows.Forms.Panel pnlTietKiemGroup;
		private System.Windows.Forms.Label lblTietKiemTitle;
		private System.Windows.Forms.Label lblTotalTKHeader;
		private System.Windows.Forms.DataGridView dgvTietKiem;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKSTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKSoTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKSoTienGui;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKLaiSuat;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKSoTienDaRut;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKKyHan;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKTrangThai;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTKVaiTro;
		private System.Windows.Forms.Panel pnlThanhToanGroup;
		private System.Windows.Forms.Label lblThanhToanTitle;
		private System.Windows.Forms.Label lblTotalTTHeader;
		private System.Windows.Forms.DataGridView dgvThanhToan;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTTSTT;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTTSoTK;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTTSoDu;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTTTrangThai;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTTVaiTro;
		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Button CreateButtonDesign51()
		{
			return new Button { Text = "Hủy", Location = new Point(240, 160), Size = new Size(120, 35) };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Button CreateButtonDesign50()
		{
			return new Button { Text = "Xác nhận mở", Location = new Point(110, 160), Size = new Size(120, 35), BackColor = colorActiveBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign49()
		{
			return new Label { Text = "* Kỳ hạn dành cho rút trước hạn là 0,1%/năm dành cho tất cả các kỳ hạn.", Location = new Point(15, 100), AutoSize = true, ForeColor = Color.DarkGray };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private ComboBox CreateComboBoxDesign48()
		{
			return new ComboBox { Location = new Point(200, 58), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign47()
		{
			return new Label { Text = "Chọn kỳ hạn gửi (Bắt buộc):", Location = new Point(15, 60), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign46()
		{
			return new TextBox { Location = new Point(200, 18), Width = 200 };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign45()
		{
			return new Label { Text = "Số tiền gửi tiết kiệm (VND):", Location = new Point(15, 20), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign44()
		{
			return new Label { Text = "Nhóm nợ hiện tại:", Location = new Point(10, 166), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign43()
		{
			return new Label { Text = "Định giá TS (VNĐ):", Location = new Point(10, 74), AutoSize = true, Name = "lblDinhGiaTS" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign42()
		{
			return new Label { Text = "Tên tài sản:", Location = new Point(10, 42), AutoSize = true, Name = "lblTenTS" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign41()
		{
			return new Label { Text = "Loại tài sản:", Location = new Point(10, 8), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign40()
		{
			return new Label { Text = "Nhóm nợ hiện tại:", Location = new Point(10, 44), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign39()
		{
			return new Label { Text = "Mức lương (VNĐ):", Location = new Point(10, 12), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign38()
		{
			return new TextBox { Location = new Point(180, 42), Width = 160, Text = "1000000000" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign37()
		{
			return new TextBox { Location = new Point(180, 10), Width = 160, Text = "Bất động sản" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign36()
		{
			return new TextBox { Location = new Point(180, 10), Width = 160, Text = "15000000" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign35()
		{
			return new Label { Location = new Point(10, 105), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.Red };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign34()
		{
			return new Label { Text = "", Location = new Point(10, 133), AutoSize = true, ForeColor = Color.DarkBlue };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign33()
		{
			return new Label { Text = "Lãi đến ngày vay:", Location = new Point(10, 102), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign32()
		{
			return new Label { Text = "Gốc sổ (VNĐ):", Location = new Point(10, 70), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign31()
		{
			return new TextBox { Location = new Point(180, 100), Width = 160, Text = "" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign30()
		{
			return new TextBox { Location = new Point(180, 68), Width = 160, Text = "" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign29()
		{
			return new TextBox { Location = new Point(180, 36), Width = 160, Text = "" };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign28()
		{
			return new Label { Text = "Chọn sổ tiết kiệm:", Location = new Point(10, 39), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private ComboBox CreateComboBoxDesign27()
		{
			return new ComboBox { Location = new Point(180, 36), Width = 325, DropDownStyle = ComboBoxStyle.DropDownList };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private ComboBox CreateComboBoxDesign26()
		{
			return new ComboBox { Location = new Point(180, 5), Width = 325, DropDownStyle = ComboBoxStyle.DropDownList };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Panel CreatePanelDesign25()
		{
			return new Panel { Location = new Point(10, 40), Size = new Size(535, 295), AutoScroll = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private RadioButton CreateRadioButtonDesign24()
		{
			return new RadioButton { Text = "Vay Thế Chấp", Location = new Point(150, 10), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private RadioButton CreateRadioButtonDesign23()
		{
			return new RadioButton { Text = "Vay Tín Chấp", Location = new Point(15, 10), Checked = true, AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Button CreateButtonDesign22()
		{
			return new Button { Text = "Xác nhận mở tài khoản thanh toán", Location = new Point(130, 130), Size = new Size(280, 45), BackColor = colorActiveBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Panel CreatePanelDesign21()
		{
			return new Panel { Location = new Point(20, 265), Size = new Size(560, 350), BorderStyle = BorderStyle.FixedSingle };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign20()
		{
			return new TextBox { Location = new Point(150, 228), Width = 220, ReadOnly = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign19()
		{
			return new Label { Text = "Số TK được cấp:", Location = new Point(20, 230), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign18()
		{
			return new Label { Text = "* Phí mở số đẹp: 500,000 VNĐ", Location = new Point(340, 197), AutoSize = true, ForeColor = Color.Red, Visible = false };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private RadioButton CreateRadioButtonDesign17()
		{
			return new RadioButton { Text = "Số đẹp (Tính phí)", Location = new Point(180, 195), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private RadioButton CreateRadioButtonDesign16()
		{
			return new RadioButton { Text = "Số ngẫu nhiên (Free)", Location = new Point(20, 195), Checked = true, AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private ComboBox CreateComboBoxDesign15()
		{
			return new ComboBox { Location = new Point(150, 158), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign14()
		{
			return new Label { Text = "Loại tài khoản:", Location = new Point(20, 160), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign13()
		{
			return new TextBox { Location = new Point(407, 19), Width = 105, MaxLength = 11 };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign12()
		{
			return new Label { Text = "SĐT:", Location = new Point(370, 22), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign11()
		{
			return new TextBox { Location = new Point(242, 19), Width = 120 };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign10()
		{
			return new Label { Text = "Họ tên:", Location = new Point(190, 22), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private TextBox CreateTextBoxDesign9()
		{
			return new TextBox { Location = new Point(55, 19), Width = 125, MaxLength = 12 };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign8()
		{
			return new Label { Text = "CCCD:", Location = new Point(8, 22), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Panel CreatePanelDesign7()
		{
			return new Panel { Location = new Point(20, 160), Size = new Size(560, 205), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Visible = false, AutoScroll = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private NumericUpDown CreateNumericUpDownDesign6()
		{
			return new NumericUpDown { Location = new Point(390, 120), Width = 155, Minimum = 1, Maximum = 3, Value = 1, Visible = false };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Button CreateButtonDesign5()
		{
			return new Button { Text = "+ Thêm đồng sở hữu", Location = new Point(390, 86), Size = new Size(155, 30), BackColor = Color.White, ForeColor = colorActiveBlue, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign4()
		{
			return new Label { Text = "Số điện thoại:", Location = new Point(20, 90), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign3()
		{
			return new Label { Text = "Họ và Tên:", Location = new Point(20, 55), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
		private Label CreateLabelDesign2()
		{
			return new Label { Text = "CCCD (12 số):", Location = new Point(20, 20), AutoSize = true };
		}

		// Chỉ khởi tạo thuộc tính giao diện; logic và sự kiện được giữ trong FrmMain.cs.
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
		private Button CreateButtonDynamicDesign12(string value1)
		{
			return new Button
			{
				Text = value1,
				Height = 46,
				Dock = DockStyle.Top,
				FlatStyle = FlatStyle.Flat,
				ForeColor = colorTextMuted,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", 10F, FontStyle.Regular),
				TextAlign = ContentAlignment.MiddleLeft,
				Cursor = Cursors.Hand
			};
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private Button CreateButtonDynamicDesign11(Point value1)
		{
			return new Button { Text = "Hủy", Location = value1, Size = new Size(110, 32) };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private Button CreateButtonDynamicDesign10(Point value1)
		{
			return new Button { Text = "Xác nhận mở", Location = value1, Size = new Size(110, 32), BackColor = colorActiveBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private ComboBox CreateComboBoxDynamicDesign9(Point value1)
		{
			return new ComboBox { Location = value1, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private Label CreateLabelDynamicDesign8(Point value1)
		{
			return new Label { Text = "Kỳ hạn vay:", Location = value1, AutoSize = true };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private TextBox CreateTextBoxDynamicDesign7(Point value1)
		{
			return new TextBox { Location = value1, Width = 160 };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private Label CreateLabelDynamicDesign6(Point value1)
		{
			return new Label { Text = "Số tiền muốn vay:", Location = value1, AutoSize = true };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private ComboBox CreateComboBoxDynamicDesign5(Point value1)
		{
			return new ComboBox { Location = value1, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private GroupBox CreateGroupBoxDynamicDesign4(string value1, Point value2)
		{
			return new GroupBox { Text = value1, Location = value2, Size = new Size(525, 58) };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private TextBox CreateTextBoxDynamicDesign3(string value1, bool value2)
		{
			return new TextBox { Location = new Point(150, 88), Width = 220, MaxLength = 11, Text = value1, ReadOnly = value2 };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private TextBox CreateTextBoxDynamicDesign2(string value1, bool value2)
		{
			return new TextBox { Location = new Point(150, 53), Width = 220, Text = value1, ReadOnly = value2 };
		}

		// Bố cục giao diện; giá trị động được truyền từ phần logic.
		private TextBox CreateTextBoxDynamicDesign1(string value1, bool value2)
		{
			return new TextBox { Location = new Point(150, 18), Width = 220, MaxLength = 12, Text = value1, ReadOnly = value2 };
		}
	}
}
