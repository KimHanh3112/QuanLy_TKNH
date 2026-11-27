using System.Drawing;
using System.Windows.Forms;

namespace QLTKNH
{
	public partial class FrmHoSoNhanVien
	{
		private System.ComponentModel.IContainer components = null;
		private TextBox txtTenDangNhap, txtHoTen, txtEmail, txtDienThoai;
		private TextBox txtMatKhauCu, txtMatKhauMoi, txtNhapLai;
		private Label lblVaiTro, lblTrangThai;
		private Button btnLuu, btnDoiMatKhau;
		private TableLayoutPanel mainLayout;
		private FlowLayoutPanel pnlNut;
		private Label lblDong0;
		private Label lblDong1;
		private Label lblDong2;
		private Label lblDong3;
		private Label lblDong4;
		private Label lblDong5;
		private Label lblDong6;
		private Label lblDong7;
		private Label lblDong8;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null) components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblDong0 = new System.Windows.Forms.Label();
            this.txtTenDangNhap = new System.Windows.Forms.TextBox();
            this.lblDong1 = new System.Windows.Forms.Label();
            this.txtHoTen = new System.Windows.Forms.TextBox();
            this.lblDong2 = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblDong3 = new System.Windows.Forms.Label();
            this.txtDienThoai = new System.Windows.Forms.TextBox();
            this.lblDong4 = new System.Windows.Forms.Label();
            this.lblVaiTro = new System.Windows.Forms.Label();
            this.lblDong5 = new System.Windows.Forms.Label();
            this.lblTrangThai = new System.Windows.Forms.Label();
            this.lblDong6 = new System.Windows.Forms.Label();
            this.txtMatKhauCu = new System.Windows.Forms.TextBox();
            this.lblDong7 = new System.Windows.Forms.Label();
            this.txtMatKhauMoi = new System.Windows.Forms.TextBox();
            this.lblDong8 = new System.Windows.Forms.Label();
            this.txtNhapLai = new System.Windows.Forms.TextBox();
            this.pnlNut = new System.Windows.Forms.FlowLayoutPanel();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnDoiMatKhau = new System.Windows.Forms.Button();
            this.mainLayout.SuspendLayout();
            this.pnlNut.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 2;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.lblDong0, 0, 0);
            this.mainLayout.Controls.Add(this.txtTenDangNhap, 1, 0);
            this.mainLayout.Controls.Add(this.lblDong1, 0, 1);
            this.mainLayout.Controls.Add(this.txtHoTen, 1, 1);
            this.mainLayout.Controls.Add(this.lblDong2, 0, 2);
            this.mainLayout.Controls.Add(this.txtEmail, 1, 2);
            this.mainLayout.Controls.Add(this.lblDong3, 0, 3);
            this.mainLayout.Controls.Add(this.txtDienThoai, 1, 3);
            this.mainLayout.Controls.Add(this.lblDong4, 0, 4);
            this.mainLayout.Controls.Add(this.lblVaiTro, 1, 4);
            this.mainLayout.Controls.Add(this.lblDong5, 0, 5);
            this.mainLayout.Controls.Add(this.lblTrangThai, 1, 5);
            this.mainLayout.Controls.Add(this.lblDong6, 0, 6);
            this.mainLayout.Controls.Add(this.txtMatKhauCu, 1, 6);
            this.mainLayout.Controls.Add(this.lblDong7, 0, 7);
            this.mainLayout.Controls.Add(this.txtMatKhauMoi, 1, 7);
            this.mainLayout.Controls.Add(this.lblDong8, 0, 8);
            this.mainLayout.Controls.Add(this.txtNhapLai, 1, 8);
            this.mainLayout.Controls.Add(this.pnlNut, 0, 9);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(22);
            this.mainLayout.RowCount = 10;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.mainLayout.Size = new System.Drawing.Size(610, 590);
            this.mainLayout.TabIndex = 0;
            // 
            // lblDong0
            // 
            this.lblDong0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong0.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong0.Location = new System.Drawing.Point(25, 22);
            this.lblDong0.Name = "lblDong0";
            this.lblDong0.Size = new System.Drawing.Size(164, 43);
            this.lblDong0.TabIndex = 0;
            this.lblDong0.Text = "Tên đăng nhập";
            this.lblDong0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtTenDangNhap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTenDangNhap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTenDangNhap.ForeColor = System.Drawing.Color.White;
            this.txtTenDangNhap.Location = new System.Drawing.Point(195, 25);
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.ReadOnly = true;
            this.txtTenDangNhap.Size = new System.Drawing.Size(390, 30);
            this.txtTenDangNhap.TabIndex = 1;
            // 
            // lblDong1
            // 
            this.lblDong1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong1.Location = new System.Drawing.Point(25, 65);
            this.lblDong1.Name = "lblDong1";
            this.lblDong1.Size = new System.Drawing.Size(164, 43);
            this.lblDong1.TabIndex = 2;
            this.lblDong1.Text = "Họ và tên";
            this.lblDong1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHoTen
            // 
            this.txtHoTen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtHoTen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHoTen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHoTen.ForeColor = System.Drawing.Color.White;
            this.txtHoTen.Location = new System.Drawing.Point(195, 68);
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.ReadOnly = true;
            this.txtHoTen.Size = new System.Drawing.Size(390, 30);
            this.txtHoTen.TabIndex = 3;
            // 
            // lblDong2
            // 
            this.lblDong2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong2.Location = new System.Drawing.Point(25, 108);
            this.lblDong2.Name = "lblDong2";
            this.lblDong2.Size = new System.Drawing.Size(164, 43);
            this.lblDong2.TabIndex = 4;
            this.lblDong2.Text = "Email";
            this.lblDong2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtEmail
            // 
            this.txtEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEmail.ForeColor = System.Drawing.Color.White;
            this.txtEmail.Location = new System.Drawing.Point(195, 111);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.Size = new System.Drawing.Size(390, 30);
            this.txtEmail.TabIndex = 5;
            // 
            // lblDong3
            // 
            this.lblDong3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong3.Location = new System.Drawing.Point(25, 151);
            this.lblDong3.Name = "lblDong3";
            this.lblDong3.Size = new System.Drawing.Size(164, 43);
            this.lblDong3.TabIndex = 6;
            this.lblDong3.Text = "Số điện thoại";
            this.lblDong3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDienThoai
            // 
            this.txtDienThoai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtDienThoai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDienThoai.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDienThoai.ForeColor = System.Drawing.Color.White;
            this.txtDienThoai.Location = new System.Drawing.Point(195, 154);
            this.txtDienThoai.MaxLength = 11;
            this.txtDienThoai.Name = "txtDienThoai";
            this.txtDienThoai.ReadOnly = true;
            this.txtDienThoai.Size = new System.Drawing.Size(390, 30);
            this.txtDienThoai.TabIndex = 7;
            this.txtDienThoai.TextChanged += new System.EventHandler(this.TxtDienThoai_TextChanged);
            this.txtDienThoai.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtDienThoai_KeyPress);
            // 
            // lblDong4
            // 
            this.lblDong4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong4.Location = new System.Drawing.Point(25, 194);
            this.lblDong4.Name = "lblDong4";
            this.lblDong4.Size = new System.Drawing.Size(164, 43);
            this.lblDong4.TabIndex = 8;
            this.lblDong4.Text = "Vai trò (chỉ xem)";
            this.lblDong4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVaiTro
            // 
            this.lblVaiTro.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVaiTro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(147)))), ((int)(((byte)(197)))), ((int)(((byte)(253)))));
            this.lblVaiTro.Location = new System.Drawing.Point(195, 194);
            this.lblVaiTro.Name = "lblVaiTro";
            this.lblVaiTro.Size = new System.Drawing.Size(390, 43);
            this.lblVaiTro.TabIndex = 9;
            this.lblVaiTro.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDong5
            // 
            this.lblDong5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong5.Location = new System.Drawing.Point(25, 237);
            this.lblDong5.Name = "lblDong5";
            this.lblDong5.Size = new System.Drawing.Size(164, 43);
            this.lblDong5.TabIndex = 10;
            this.lblDong5.Text = "Trạng thái (chỉ xem)";
            this.lblDong5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTrangThai
            // 
            this.lblTrangThai.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTrangThai.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(239)))), ((int)(((byte)(172)))));
            this.lblTrangThai.Location = new System.Drawing.Point(195, 237);
            this.lblTrangThai.Name = "lblTrangThai";
            this.lblTrangThai.Size = new System.Drawing.Size(390, 43);
            this.lblTrangThai.TabIndex = 11;
            this.lblTrangThai.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDong6
            // 
            this.lblDong6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong6.Location = new System.Drawing.Point(25, 280);
            this.lblDong6.Name = "lblDong6";
            this.lblDong6.Size = new System.Drawing.Size(164, 1);
            this.lblDong6.TabIndex = 12;
            this.lblDong6.Text = "Mật khẩu hiện tại";
            this.lblDong6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMatKhauCu
            // 
            this.txtMatKhauCu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtMatKhauCu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMatKhauCu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMatKhauCu.ForeColor = System.Drawing.Color.White;
            this.txtMatKhauCu.Location = new System.Drawing.Point(195, 283);
            this.txtMatKhauCu.Name = "txtMatKhauCu";
            this.txtMatKhauCu.Size = new System.Drawing.Size(390, 30);
            this.txtMatKhauCu.TabIndex = 13;
            this.txtMatKhauCu.UseSystemPasswordChar = true;
            // 
            // lblDong7
            // 
            this.lblDong7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong7.Location = new System.Drawing.Point(25, 280);
            this.lblDong7.Name = "lblDong7";
            this.lblDong7.Size = new System.Drawing.Size(164, 1);
            this.lblDong7.TabIndex = 14;
            this.lblDong7.Text = "Mật khẩu mới";
            this.lblDong7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMatKhauMoi
            // 
            this.txtMatKhauMoi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtMatKhauMoi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMatKhauMoi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMatKhauMoi.ForeColor = System.Drawing.Color.White;
            this.txtMatKhauMoi.Location = new System.Drawing.Point(195, 283);
            this.txtMatKhauMoi.Name = "txtMatKhauMoi";
            this.txtMatKhauMoi.Size = new System.Drawing.Size(390, 30);
            this.txtMatKhauMoi.TabIndex = 15;
            this.txtMatKhauMoi.UseSystemPasswordChar = true;
            // 
            // lblDong8
            // 
            this.lblDong8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDong8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblDong8.Location = new System.Drawing.Point(25, 280);
            this.lblDong8.Name = "lblDong8";
            this.lblDong8.Size = new System.Drawing.Size(164, 1);
            this.lblDong8.TabIndex = 16;
            this.lblDong8.Text = "Nhập lại mật khẩu";
            this.lblDong8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtNhapLai
            // 
            this.txtNhapLai.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtNhapLai.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNhapLai.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNhapLai.ForeColor = System.Drawing.Color.White;
            this.txtNhapLai.Location = new System.Drawing.Point(195, 283);
            this.txtNhapLai.Name = "txtNhapLai";
            this.txtNhapLai.Size = new System.Drawing.Size(390, 30);
            this.txtNhapLai.TabIndex = 17;
            this.txtNhapLai.UseSystemPasswordChar = true;
            // 
            // pnlNut
            // 
            this.pnlNut.AutoSize = true;
            this.mainLayout.SetColumnSpan(this.pnlNut, 2);
            this.pnlNut.Controls.Add(this.btnLuu);
            this.pnlNut.Controls.Add(this.btnDoiMatKhau);
            this.pnlNut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNut.Location = new System.Drawing.Point(25, 283);
            this.pnlNut.Name = "pnlNut";
            this.pnlNut.Size = new System.Drawing.Size(560, 282);
            this.pnlNut.TabIndex = 18;
            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnLuu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(3, 3);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(156, 36);
            this.btnLuu.TabIndex = 0;
            this.btnLuu.Text = "Cập nhật";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.BtnLuu_Click);
            // 
            // btnDoiMatKhau
            // 
            this.btnDoiMatKhau.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnDoiMatKhau.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDoiMatKhau.ForeColor = System.Drawing.Color.White;
            this.btnDoiMatKhau.Location = new System.Drawing.Point(165, 3);
            this.btnDoiMatKhau.Name = "btnDoiMatKhau";
            this.btnDoiMatKhau.Size = new System.Drawing.Size(172, 36);
            this.btnDoiMatKhau.TabIndex = 1;
            this.btnDoiMatKhau.Text = "Đổi mật khẩu";
            this.btnDoiMatKhau.UseVisualStyleBackColor = false;
            this.btnDoiMatKhau.Click += new System.EventHandler(this.BtnDoiMatKhau_Click);
            // 
            // FrmHoSoNhanVien
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(610, 590);
            this.Controls.Add(this.mainLayout);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(610, 590);
            this.Name = "FrmHoSoNhanVien";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hồ sơ cá nhân";
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.pnlNut.ResumeLayout(false);
            this.ResumeLayout(false);

		}
	}
}
