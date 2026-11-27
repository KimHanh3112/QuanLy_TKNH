using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLTKNH
{
	public partial class FrmQuanLyNhanVien
	{
		private System.ComponentModel.IContainer components = null;
		private DataGridView dgvNhanVien;
		private GroupBox grpTim;
		private TextBox txtTim, txtTenDangNhap, txtHoTen, txtEmail, txtDienThoai, txtMatKhauMoi;
		private Label lblVaiTro, lblTrangThai, lblMatKhauMoi, lblThongBao;
		private Button btnTim, btnLuu;
		private ContextMenuStrip menuNhanVien;
		private ToolStripMenuItem miKhoaMo;
		private TableLayoutPanel rootLayout, searchLayout, infoLayout, actionLayout;
		private Panel infoPanel;
		private Label lblTimKiem, lblTenDangNhapTitle, lblHoTenTitle, lblEmailTitle, lblDienThoaiTitle, lblVaiTroTitle, lblTrangThaiTitle;

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
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpTim = new System.Windows.Forms.GroupBox();
            this.searchLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblTimKiem = new System.Windows.Forms.Label();
            this.txtTim = new System.Windows.Forms.TextBox();
            this.btnTim = new System.Windows.Forms.Button();
            this.dgvNhanVien = new System.Windows.Forms.DataGridView();
            this.menuNhanVien = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miKhoaMo = new System.Windows.Forms.ToolStripMenuItem();
            this.infoPanel = new System.Windows.Forms.Panel();
            this.infoLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblTenDangNhapTitle = new System.Windows.Forms.Label();
            this.txtTenDangNhap = new System.Windows.Forms.TextBox();
            this.lblHoTenTitle = new System.Windows.Forms.Label();
            this.txtHoTen = new System.Windows.Forms.TextBox();
            this.lblEmailTitle = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblDienThoaiTitle = new System.Windows.Forms.Label();
            this.txtDienThoai = new System.Windows.Forms.TextBox();
            this.lblVaiTroTitle = new System.Windows.Forms.Label();
            this.lblVaiTro = new System.Windows.Forms.Label();
            this.lblTrangThaiTitle = new System.Windows.Forms.Label();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.lblMatKhauMoi = new System.Windows.Forms.Label();
            this.txtMatKhauMoi = new System.Windows.Forms.TextBox();
            this.actionLayout = new System.Windows.Forms.TableLayoutPanel();
            this.btnLuu = new System.Windows.Forms.Button();
            this.lblThongBao = new System.Windows.Forms.Label();
            this.rootLayout.SuspendLayout();
            this.grpTim.SuspendLayout();
            this.searchLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNhanVien)).BeginInit();
            this.menuNhanVien.SuspendLayout();
            this.infoPanel.SuspendLayout();
            this.infoLayout.SuspendLayout();
            this.actionLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.grpTim, 0, 0);
            this.rootLayout.Controls.Add(this.dgvNhanVien, 0, 1);
            this.rootLayout.Controls.Add(this.infoPanel, 0, 2);
            this.rootLayout.Controls.Add(this.actionLayout, 0, 3);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(15);
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 51F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.rootLayout.Size = new System.Drawing.Size(1110, 760);
            this.rootLayout.TabIndex = 1;
            // 
            // grpTim
            // 
            this.grpTim.Controls.Add(this.searchLayout);
            this.grpTim.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTim.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpTim.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.grpTim.Location = new System.Drawing.Point(18, 18);
            this.grpTim.Name = "grpTim";
            this.grpTim.Size = new System.Drawing.Size(1074, 81);
            this.grpTim.TabIndex = 0;
            this.grpTim.TabStop = false;
            this.grpTim.Text = "TÌM KIẾM NHÂN VIÊN   (nhấn vùng trống của khung để tải lại)";
            this.grpTim.Click += new System.EventHandler(this.TimKiemReset_Click);
            // 
            // searchLayout
            // 
            this.searchLayout.ColumnCount = 3;
            this.searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54F));
            this.searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16F));
            this.searchLayout.Controls.Add(this.lblTimKiem, 0, 0);
            this.searchLayout.Controls.Add(this.txtTim, 1, 0);
            this.searchLayout.Controls.Add(this.btnTim, 2, 0);
            this.searchLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchLayout.Location = new System.Drawing.Point(3, 26);
            this.searchLayout.Name = "searchLayout";
            this.searchLayout.Padding = new System.Windows.Forms.Padding(10, 6, 10, 7);
            this.searchLayout.RowCount = 1;
            this.searchLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.searchLayout.Size = new System.Drawing.Size(1068, 52);
            this.searchLayout.TabIndex = 0;
            this.searchLayout.Click += new System.EventHandler(this.TimKiemReset_Click);
            // 
            // lblTimKiem
            // 
            this.lblTimKiem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTimKiem.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTimKiem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblTimKiem.Location = new System.Drawing.Point(13, 6);
            this.lblTimKiem.Name = "lblTimKiem";
            this.lblTimKiem.Size = new System.Drawing.Size(308, 39);
            this.lblTimKiem.TabIndex = 0;
            this.lblTimKiem.Text = "Tên đăng nhập / Số điện thoại:";
            this.lblTimKiem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTimKiem.Click += new System.EventHandler(this.TimKiemReset_Click);
            // 
            // txtTim
            // 
            this.txtTim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtTim.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTim.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTim.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTim.ForeColor = System.Drawing.Color.White;
            this.txtTim.Location = new System.Drawing.Point(327, 9);
            this.txtTim.MaxLength = 120;
            this.txtTim.Name = "txtTim";
            this.txtTim.Size = new System.Drawing.Size(559, 30);
            this.txtTim.TabIndex = 1;
            this.txtTim.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtTim_Enter_KeyDown);
            // 
            // btnTim
            // 
            this.btnTim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnTim.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTim.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTim.ForeColor = System.Drawing.Color.White;
            this.btnTim.Location = new System.Drawing.Point(892, 9);
            this.btnTim.Name = "btnTim";
            this.btnTim.Size = new System.Drawing.Size(163, 33);
            this.btnTim.TabIndex = 2;
            this.btnTim.Text = "Tìm";
            this.btnTim.UseVisualStyleBackColor = false;
            this.btnTim.Click += new System.EventHandler(this.BtnTim_Click);
            // 
            // dgvNhanVien
            // 
            this.dgvNhanVien.AllowUserToAddRows = false;
            this.dgvNhanVien.AllowUserToDeleteRows = false;
            this.dgvNhanVien.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNhanVien.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvNhanVien.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvNhanVien.ColumnHeadersHeight = 29;
            this.dgvNhanVien.ContextMenuStrip = this.menuNhanVien;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvNhanVien.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvNhanVien.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNhanVien.EnableHeadersVisualStyles = false;
            this.dgvNhanVien.Location = new System.Drawing.Point(18, 105);
            this.dgvNhanVien.MultiSelect = false;
            this.dgvNhanVien.Name = "dgvNhanVien";
            this.dgvNhanVien.ReadOnly = true;
            this.dgvNhanVien.RowHeadersVisible = false;
            this.dgvNhanVien.RowHeadersWidth = 51;
            this.dgvNhanVien.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNhanVien.Size = new System.Drawing.Size(1074, 285);
            this.dgvNhanVien.TabIndex = 1;
            this.dgvNhanVien.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DgvNhanVien_CellMouseDown);
            this.dgvNhanVien.SelectionChanged += new System.EventHandler(this.DgvNhanVien_SelectionChanged);
            // 
            // menuNhanVien
            // 
            this.menuNhanVien.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuNhanVien.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miKhoaMo});
            this.menuNhanVien.Name = "menuNhanVien";
            this.menuNhanVien.Size = new System.Drawing.Size(178, 28);
            this.menuNhanVien.Opening += new System.ComponentModel.CancelEventHandler(this.MenuNhanVien_Opening);
            // 
            // miKhoaMo
            // 
            this.miKhoaMo.Name = "miKhoaMo";
            this.miKhoaMo.Size = new System.Drawing.Size(177, 24);
            this.miKhoaMo.Text = "Khóa tài khoản";
            this.miKhoaMo.Click += new System.EventHandler(this.MiKhoaMo_Click);
            // 
            // infoPanel
            // 
            this.infoPanel.AutoScroll = true;
            this.infoPanel.Controls.Add(this.infoLayout);
            this.infoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPanel.Location = new System.Drawing.Point(18, 396);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Size = new System.Drawing.Size(1074, 297);
            this.infoPanel.TabIndex = 2;
            // 
            // infoLayout
            // 
            this.infoLayout.AutoSize = true;
            this.infoLayout.ColumnCount = 2;
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.infoLayout.Controls.Add(this.lblTenDangNhapTitle, 0, 0);
            this.infoLayout.Controls.Add(this.txtTenDangNhap, 1, 0);
            this.infoLayout.Controls.Add(this.lblHoTenTitle, 0, 1);
            this.infoLayout.Controls.Add(this.txtHoTen, 1, 1);
            this.infoLayout.Controls.Add(this.lblEmailTitle, 0, 2);
            this.infoLayout.Controls.Add(this.txtEmail, 1, 2);
            this.infoLayout.Controls.Add(this.lblDienThoaiTitle, 0, 3);
            this.infoLayout.Controls.Add(this.txtDienThoai, 1, 3);
            this.infoLayout.Controls.Add(this.lblVaiTroTitle, 0, 4);
            this.infoLayout.Controls.Add(this.lblVaiTro, 1, 4);
            this.infoLayout.Controls.Add(this.lblTrangThaiTitle, 0, 5);
            this.infoLayout.Controls.Add(this.lblTrangThai, 1, 5);
            this.infoLayout.Controls.Add(this.lblMatKhauMoi, 0, 6);
            this.infoLayout.Controls.Add(this.txtMatKhauMoi, 1, 6);
            this.infoLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.infoLayout.Location = new System.Drawing.Point(0, 0);
            this.infoLayout.Name = "infoLayout";
            this.infoLayout.Padding = new System.Windows.Forms.Padding(7, 9, 7, 4);
            this.infoLayout.RowCount = 7;
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.infoLayout.Size = new System.Drawing.Size(1074, 265);
            this.infoLayout.TabIndex = 0;
            // 
            // lblTenDangNhapTitle
            // 
            this.lblTenDangNhapTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTenDangNhapTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTenDangNhapTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblTenDangNhapTitle.Location = new System.Drawing.Point(10, 9);
            this.lblTenDangNhapTitle.Name = "lblTenDangNhapTitle";
            this.lblTenDangNhapTitle.Size = new System.Drawing.Size(184, 36);
            this.lblTenDangNhapTitle.TabIndex = 0;
            this.lblTenDangNhapTitle.Text = "Tên đăng nhập";
            this.lblTenDangNhapTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtTenDangNhap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTenDangNhap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTenDangNhap.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.txtTenDangNhap.Location = new System.Drawing.Point(200, 12);
            this.txtTenDangNhap.MaxLength = 60;
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.ReadOnly = true;
            this.txtTenDangNhap.Size = new System.Drawing.Size(864, 30);
            this.txtTenDangNhap.TabIndex = 1;
            // 
            // lblHoTenTitle
            // 
            this.lblHoTenTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHoTenTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblHoTenTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblHoTenTitle.Location = new System.Drawing.Point(10, 45);
            this.lblHoTenTitle.Name = "lblHoTenTitle";
            this.lblHoTenTitle.Size = new System.Drawing.Size(184, 36);
            this.lblHoTenTitle.TabIndex = 2;
            this.lblHoTenTitle.Text = "Họ tên";
            this.lblHoTenTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHoTen
            // 
            this.txtHoTen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtHoTen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHoTen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHoTen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtHoTen.ForeColor = System.Drawing.Color.White;
            this.txtHoTen.Location = new System.Drawing.Point(200, 48);
            this.txtHoTen.MaxLength = 100;
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.Size = new System.Drawing.Size(864, 30);
            this.txtHoTen.TabIndex = 3;
            // 
            // lblEmailTitle
            // 
            this.lblEmailTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEmailTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblEmailTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblEmailTitle.Location = new System.Drawing.Point(10, 81);
            this.lblEmailTitle.Name = "lblEmailTitle";
            this.lblEmailTitle.Size = new System.Drawing.Size(184, 36);
            this.lblEmailTitle.TabIndex = 4;
            this.lblEmailTitle.Text = "Gmail / Email";
            this.lblEmailTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtEmail.ForeColor = System.Drawing.Color.White;
            this.txtEmail.Location = new System.Drawing.Point(200, 84);
            this.txtEmail.MaxLength = 150;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(864, 30);
            this.txtEmail.TabIndex = 5;
            // 
            // lblDienThoaiTitle
            // 
            this.lblDienThoaiTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDienThoaiTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDienThoaiTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDienThoaiTitle.Location = new System.Drawing.Point(10, 117);
            this.lblDienThoaiTitle.Name = "lblDienThoaiTitle";
            this.lblDienThoaiTitle.Size = new System.Drawing.Size(184, 36);
            this.lblDienThoaiTitle.TabIndex = 6;
            this.lblDienThoaiTitle.Text = "Số điện thoại";
            this.lblDienThoaiTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDienThoai
            // 
            this.txtDienThoai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtDienThoai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDienThoai.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDienThoai.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDienThoai.ForeColor = System.Drawing.Color.White;
            this.txtDienThoai.Location = new System.Drawing.Point(200, 120);
            this.txtDienThoai.MaxLength = 11;
            this.txtDienThoai.Name = "txtDienThoai";
            this.txtDienThoai.Size = new System.Drawing.Size(864, 30);
            this.txtDienThoai.TabIndex = 7;
            this.txtDienThoai.TextChanged += new System.EventHandler(this.TxtDienThoai_TextChanged);
            this.txtDienThoai.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtDienThoai_KeyPress);
            // 
            // lblVaiTroTitle
            // 
            this.lblVaiTroTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVaiTroTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblVaiTroTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblVaiTroTitle.Location = new System.Drawing.Point(10, 153);
            this.lblVaiTroTitle.Name = "lblVaiTroTitle";
            this.lblVaiTroTitle.Size = new System.Drawing.Size(184, 36);
            this.lblVaiTroTitle.TabIndex = 8;
            this.lblVaiTroTitle.Text = "Vai trò (chỉ xem)";
            this.lblVaiTroTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVaiTro
            // 
            this.lblVaiTro.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVaiTro.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblVaiTro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblVaiTro.Location = new System.Drawing.Point(200, 153);
            this.lblVaiTro.Name = "lblVaiTro";
            this.lblVaiTro.Size = new System.Drawing.Size(864, 36);
            this.lblVaiTro.TabIndex = 9;
            this.lblVaiTro.Text = "—";
            this.lblVaiTro.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTrangThaiTitle
            // 
            this.lblTrangThaiTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTrangThaiTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTrangThaiTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblTrangThaiTitle.Location = new System.Drawing.Point(10, 189);
            this.lblTrangThaiTitle.Name = "lblTrangThaiTitle";
            this.lblTrangThaiTitle.Size = new System.Drawing.Size(184, 36);
            this.lblTrangThaiTitle.TabIndex = 10;
            this.lblTrangThaiTitle.Text = "Trạng thái (chỉ xem)";
            this.lblTrangThaiTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTrangThai.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblTrangThai.Location = new System.Drawing.Point(200, 189);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(864, 36);
            this.lblTrangThai.TabIndex = 11;
            this.lblTrangThai.Text = "—";
            this.lblTrangThai.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMatKhauMoi
            // 
            this.lblMatKhauMoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMatKhauMoi.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblMatKhauMoi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblMatKhauMoi.Location = new System.Drawing.Point(10, 225);
            this.lblMatKhauMoi.Name = "lblMatKhauMoi";
            this.lblMatKhauMoi.Size = new System.Drawing.Size(184, 36);
            this.lblMatKhauMoi.TabIndex = 12;
            this.lblMatKhauMoi.Text = "Mật khẩu ban đầu";
            this.lblMatKhauMoi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMatKhauMoi
            // 
            this.txtMatKhauMoi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtMatKhauMoi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMatKhauMoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMatKhauMoi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMatKhauMoi.ForeColor = System.Drawing.Color.White;
            this.txtMatKhauMoi.Location = new System.Drawing.Point(200, 228);
            this.txtMatKhauMoi.MaxLength = 128;
            this.txtMatKhauMoi.Name = "txtMatKhauMoi";
            this.txtMatKhauMoi.Size = new System.Drawing.Size(864, 30);
            this.txtMatKhauMoi.TabIndex = 13;
            this.txtMatKhauMoi.UseSystemPasswordChar = true;
            // 
            // actionLayout
            // 
            this.actionLayout.ColumnCount = 2;
            this.actionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.actionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionLayout.Controls.Add(this.btnLuu, 0, 0);
            this.actionLayout.Controls.Add(this.lblThongBao, 1, 0);
            this.actionLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionLayout.Location = new System.Drawing.Point(18, 699);
            this.actionLayout.Name = "actionLayout";
            this.actionLayout.RowCount = 1;
            this.actionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionLayout.Size = new System.Drawing.Size(1074, 43);
            this.actionLayout.TabIndex = 3;
            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnLuu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLuu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(3, 3);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(184, 37);
            this.btnLuu.TabIndex = 0;
            this.btnLuu.Text = "Lưu hồ sơ";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.BtnLuu_Click);
            // 
            // lblThongBao
            // 
            this.lblThongBao.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblThongBao.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblThongBao.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblThongBao.Location = new System.Drawing.Point(193, 0);
            this.lblThongBao.Name = "lblThongBao";
            this.lblThongBao.Size = new System.Drawing.Size(878, 43);
            this.lblThongBao.TabIndex = 1;
            this.lblThongBao.Text = "Chọn nhân viên để chỉnh sửa.";
            this.lblThongBao.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FrmQuanLyNhanVien
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1110, 760);
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ForeColor = System.Drawing.Color.White;
            this.MinimumSize = new System.Drawing.Size(830, 600);
            this.Name = "FrmQuanLyNhanVien";
            this.Text = "Quản lý hồ sơ nhân viên";
            this.rootLayout.ResumeLayout(false);
            this.grpTim.ResumeLayout(false);
            this.searchLayout.ResumeLayout(false);
            this.searchLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNhanVien)).EndInit();
            this.menuNhanVien.ResumeLayout(false);
            this.infoPanel.ResumeLayout(false);
            this.infoPanel.PerformLayout();
            this.infoLayout.ResumeLayout(false);
            this.infoLayout.PerformLayout();
            this.actionLayout.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		// Chỉ thay cho lambda cũ trong InitializeComponent; giữ nguyên hành vi nhấn Enter để tìm kiếm.
		private void TxtTim_Enter_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				BtnTim_Click(sender, EventArgs.Empty);
				e.SuppressKeyPress = true;
			}
		}
	}
}
