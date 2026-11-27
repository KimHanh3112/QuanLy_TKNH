using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace QLTKNH
{
	public partial class FrmHoSoNhanVien : Form
	{
		private bool dangDoiMatKhau;
		private bool dangSuaHoSo;
		private string tenBanDau, emailBanDau, dienThoaiBanDau;

		// Cho phép cửa sổ chính cập nhật tên ngay khi hồ sơ được lưu thành công.
		public event EventHandler HoSoDaCapNhat;

		public FrmHoSoNhanVien()
		{
			InitializeComponent();
			Load += (s, e) => { ChuyenCheDoDoiMatKhau(false); TaiHoSo(); DatCheDoSua(false); };
			FormClosing += FrmHoSoNhanVien_FormClosing;
		}
		// Tương thích lời gọi trong menu cũ, không sử dụng chuỗi kết nối khác.
		public FrmHoSoNhanVien(string ignoredConnectionString) : this() { }

		// Chặn ký tự không phải số khi gõ; vẫn cho phép Backspace, Delete,
		// Ctrl+C, Ctrl+V, Ctrl+X và các phím điều khiển bình thường.
		private void TxtDienThoai_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && (e.KeyChar < '0' || e.KeyChar > '9'))
				e.Handled = true;
		}

		// Bảo đảm cả nội dung dán (Ctrl+V, menu chuột phải) cũng chỉ có chữ số.
		private void TxtDienThoai_TextChanged(object sender, EventArgs e)
		{
			string oldText = txtDienThoai.Text;
			string filtered = new string(System.Linq.Enumerable.ToArray(
				System.Linq.Enumerable.Where(oldText, c => c >= '0' && c <= '9')));
			if (filtered.Length > 11) filtered = filtered.Substring(0, 11);
			if (oldText == filtered) return;

			int oldCaret = txtDienThoai.SelectionStart;
			int newCaret = 0;
			for (int i = 0; i < Math.Min(oldCaret, oldText.Length); i++)
				if (oldText[i] >= '0' && oldText[i] <= '9' && newCaret < 11) newCaret++;
			txtDienThoai.Text = filtered;
			txtDienThoai.SelectionStart = Math.Min(newCaret, filtered.Length);
		}

		private void TaiHoSo()
		{
			try
			{
				using (var cn = new SqlConnection(QuyenHoSo.Conn))
				using (var cmd = new SqlCommand(@"SELECT TenDangNhap,HoTen,Email,SoDienThoai,ChucVu,TrangThai,BiKhoa
                    FROM dbo.NhanVienNganHang WHERE Id=@id", cn))
				{
					cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuyenHoSo.Id;
					cn.Open();
					using (var rd = cmd.ExecuteReader())
					{
						if (!rd.Read()) throw new InvalidOperationException("Không tìm thấy tài khoản đăng nhập.");
						txtTenDangNhap.Text = Convert.ToString(rd["TenDangNhap"]);
						txtHoTen.Text = Convert.ToString(rd["HoTen"]);
						txtEmail.Text = rd["Email"] == DBNull.Value ? "" : Convert.ToString(rd["Email"]);
						txtDienThoai.Text = Convert.ToString(rd["SoDienThoai"]);
						lblVaiTro.Text = Convert.ToString(rd["ChucVu"]);
						lblTrangThai.Text = Convert.ToBoolean(rd["BiKhoa"]) ? "Đã khóa" : Convert.ToString(rd["TrangThai"]);
						tenBanDau = txtHoTen.Text;
						emailBanDau = txtEmail.Text;
						dienThoaiBanDau = txtDienThoai.Text;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Không tải được hồ sơ: " + ex.Message, "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				Close();
			}
		}
		private bool KiemTra()
		{
			if (string.IsNullOrWhiteSpace(txtHoTen.Text) || txtHoTen.Text.Trim().Length > 100)
			{ MessageBox.Show("Họ tên phải có từ 1 đến 100 ký tự."); return false; }
			string email = txtEmail.Text.Trim();
			if (email.Length > 150) { MessageBox.Show("Email tối đa 150 ký tự."); return false; }
			if (email.Length > 0)
			{
				try { if (new MailAddress(email).Address != email) throw new FormatException(); }
				catch { MessageBox.Show("Địa chỉ email không hợp lệ."); return false; }
			}
			string phone = txtDienThoai.Text.Trim();
			if (phone.Length < 10 || phone.Length > 11 || !System.Linq.Enumerable.All(phone, c => c >= '0' && c <= '9'))
			{ MessageBox.Show("Số điện thoại phải gồm 10 đến 11 chữ số."); return false; }
			return true;
		}
		private void DatCheDoSua(bool bat)
		{
			dangSuaHoSo = bat;
			txtTenDangNhap.ReadOnly = true;
			txtHoTen.ReadOnly = !bat;
			txtEmail.ReadOnly = !bat;
			txtDienThoai.ReadOnly = !bat;
			btnLuu.Text = dangDoiMatKhau ? "Lưu mật khẩu" : bat ? "Lưu cập nhật" : "Cập nhật";
		}

		private bool CoThayDoi()
		{
			return txtHoTen.Text.Trim() != (tenBanDau ?? "") ||
				txtEmail.Text.Trim() != (emailBanDau ?? "") ||
				txtDienThoai.Text.Trim() != (dienThoaiBanDau ?? "");
		}

		private bool LuuHoSo(bool tuDong)
		{
			if (!CoThayDoi()) { DatCheDoSua(false); return true; }
			if (!KiemTra()) return false;
			try
			{
				using (var cn = new SqlConnection(QuyenHoSo.Conn))
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							using (var cmd = new SqlCommand(@"UPDATE dbo.NhanVienNganHang
                                SET HoTen=@name,Email=@email,SoDienThoai=@phone
                                WHERE Id=@id AND TrangThai<>N'Nghỉ việc' AND BiKhoa=0", cn, tx))
							{
								cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = txtHoTen.Text.Trim();
								cmd.Parameters.Add("@email", SqlDbType.NVarChar, 150).Value = txtEmail.Text.Trim();
								cmd.Parameters.Add("@phone", SqlDbType.VarChar, 11).Value = txtDienThoai.Text.Trim();
								cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuyenHoSo.Id;
								if (cmd.ExecuteNonQuery() != 1)
									throw new UnauthorizedAccessException("Tài khoản không còn hoạt động hoặc đã bị khóa.");
							}
							QuyenHoSo.GhiNhatKy(cn, tx, tuDong ? "Tự lưu hồ sơ khi rời trang" : "Cập nhật hồ sơ cá nhân",
								"Cập nhật họ tên, email, số điện thoại", QuyenHoSo.Id);
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				tenBanDau = txtHoTen.Text.Trim();
				emailBanDau = txtEmail.Text.Trim();
				dienThoaiBanDau = txtDienThoai.Text.Trim();
				BankSecurity.Current.HoTen = tenBanDau;
				var handler = HoSoDaCapNhat;
				if (handler != null) handler(this, EventArgs.Empty);
				DatCheDoSua(false);
				if (!tuDong) MessageBox.Show(this, "Đã cập nhật hồ sơ.", "Thành công");
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Không lưu được hồ sơ: " + ex.Message, "Lỗi cập nhật");
				return false;
			}
		}

		private void BtnLuu_Click(object sender, EventArgs e)
		{
			if (dangDoiMatKhau) { LuuMatKhauMoi(); return; }
			if (!dangSuaHoSo)
			{
				DatCheDoSua(true);
				txtHoTen.Focus();
				return;
			}
			LuuHoSo(false);
		}

		private void FrmHoSoNhanVien_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Chỉ tự lưu thay đổi hồ sơ sau khi đã bật chế độ Cập nhật.
			// Mật khẩu đang nhập dở không bao giờ tự lưu.
			if (dangSuaHoSo && CoThayDoi() && !LuuHoSo(true)) e.Cancel = true;
		}

		private void ChuyenCheDoDoiMatKhau(bool bat)
		{
			dangDoiMatKhau = bat;
			for (int i = 6; i <= 8; i++)
			{
				mainLayout.RowStyles[i].Height = bat ? 43F : 0F;
				Control nhan = mainLayout.GetControlFromPosition(0, i);
				Control oNhap = mainLayout.GetControlFromPosition(1, i);
				if (nhan != null) nhan.Visible = bat;
				if (oNhap != null) oNhap.Visible = bat;
			}
			if (!bat)
			{
				txtMatKhauCu.Clear();
				txtMatKhauMoi.Clear();
				txtNhapLai.Clear();
			}
			btnLuu.Text = bat ? "Lưu mật khẩu" : (dangSuaHoSo ? "Lưu cập nhật" : "Cập nhật");
			btnDoiMatKhau.Text = bat ? "Hủy đổi mật khẩu" : "Đổi mật khẩu";
			// Khi đổi mật khẩu, khóa phần sửa hồ sơ để tránh nhầm nút lưu.
			txtHoTen.ReadOnly = bat || !dangSuaHoSo;
			txtEmail.ReadOnly = bat || !dangSuaHoSo;
			txtDienThoai.ReadOnly = bat || !dangSuaHoSo;
			if (bat) txtMatKhauCu.Focus();
			mainLayout.PerformLayout();
		}

		private void BtnDoiMatKhau_Click(object sender, EventArgs e)
		{
			if (!dangDoiMatKhau && dangSuaHoSo && !LuuHoSo(true)) return;
			ChuyenCheDoDoiMatKhau(!dangDoiMatKhau);
		}

		private void LuuMatKhauMoi()
		{
			if (txtMatKhauCu.Text.Length == 0 || txtMatKhauMoi.Text.Length < 12 || txtMatKhauMoi.Text != txtNhapLai.Text)
			{ MessageBox.Show("Nhập mật khẩu hiện tại và mật khẩu mới từ 12 ký tự; xác nhận phải trùng nhau."); return; }
			if (txtMatKhauCu.Text == txtMatKhauMoi.Text)
			{ MessageBox.Show("Mật khẩu mới phải khác mật khẩu hiện tại."); return; }
			try
			{
				using (var cn = new SqlConnection(QuyenHoSo.Conn))
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							byte[] salt, hash;
							using (var cmd = new SqlCommand(@"SELECT MatKhauSalt,MatKhauHash FROM dbo.NhanVienNganHang WITH(UPDLOCK,HOLDLOCK)
                                WHERE Id=@id AND TrangThai<>N'Nghỉ việc' AND BiKhoa=0", cn, tx))
							{
								cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuyenHoSo.Id;
								using (var rd = cmd.ExecuteReader())
								{
									if (!rd.Read()) throw new UnauthorizedAccessException("Không tìm thấy tài khoản đang hoạt động.");
									salt = (byte[])rd[0]; hash = (byte[])rd[1];
								}
							}
							if (!QuyenHoSo.TrungHash(QuyenHoSo.Bam(txtMatKhauCu.Text, salt), hash))
								throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
							byte[] newSalt = new byte[16];
							using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(newSalt);
							byte[] newHash = QuyenHoSo.Bam(txtMatKhauMoi.Text, newSalt);
							using (var cmd = new SqlCommand(@"UPDATE dbo.NhanVienNganHang
                                 SET MatKhauSalt=@salt,MatKhauHash=@hash WHERE Id=@id AND BiKhoa=0", cn, tx))
							{
								cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = newSalt;
								cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = newHash;
								cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuyenHoSo.Id;
								if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không thể đổi mật khẩu.");
							}
							QuyenHoSo.GhiNhatKy(cn, tx, "Đổi mật khẩu cá nhân", "Nhân viên đổi mật khẩu của chính mình", QuyenHoSo.Id);
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				ChuyenCheDoDoiMatKhau(false);
				MessageBox.Show(this, "Đổi mật khẩu thành công. Dùng mật khẩu mới ở lần đăng nhập sau.");
			}
			catch (Exception ex) { MessageBox.Show(this, "Không đổi được mật khẩu: " + ex.Message); }
		}
	}
}
