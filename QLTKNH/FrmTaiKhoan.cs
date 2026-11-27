using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace QLTKNH
{
	public partial class FrmTaiKhoan : Form
	{
		private readonly string _connStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";
		private const int VongLap = 210000;
		private int _maDangChon = 0;
		private int _maChucVuCu = 0;

		public FrmTaiKhoan()
		{
			InitializeComponent();
			Load += (s, e) =>
			{
				if (!BankSecurity.IsLoggedIn || !BankSecurity.CanManageEmployees || !QuyenHoSo.LaGiamDoc())
				{
					MessageBox.Show("Bạn không có quyền quản lý tài khoản.", "Từ chối truy cập",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					Close();
					return;
				}
				TaiChucVu();
				TaiDanhSach();
			};
		}

		private SqlConnection KetNoi() { return new SqlConnection(_connStr); }

		private void TaiChucVu()
		{
			using (var cn = KetNoi())
			using (var da = new SqlDataAdapter(
				"SELECT MaChucVu,TenChucVu FROM dbo.ChucVuDangNhap ORDER BY MaChucVu", cn))
			{
				var dt = new DataTable();
				da.Fill(dt);
				cboChucVu.DataSource = dt;
				cboChucVu.DisplayMember = "TenChucVu";
				cboChucVu.ValueMember = "MaChucVu";
			}
		}

		private void TaiDanhSach()
		{
			try
			{
				using (var cn = KetNoi())
				using (var cmd = new SqlCommand(@"
SELECT t.MaTaiKhoan, t.TenDangNhap, t.HoTen, t.MaChucVu,
       c.TenChucVu, t.DangHoatDong, t.NgayTao, t.LanDangNhapCuoi,
       t.SoLanSai, t.KhoaDen
FROM dbo.TaiKhoanDangNhap t
JOIN dbo.ChucVuDangNhap c ON c.MaChucVu=t.MaChucVu
WHERE (@tim=N'' OR t.TenDangNhap LIKE N'%'+@tim+N'%' OR t.HoTen LIKE N'%'+@tim+N'%')
ORDER BY t.MaTaiKhoan", cn))
				{
					cmd.Parameters.Add("@tim", SqlDbType.NVarChar, 120).Value = txtTimKiem.Text.Trim();
					var dt = new DataTable();
					new SqlDataAdapter(cmd).Fill(dt);
					dgvTaiKhoan.DataSource = dt;
					foreach (string col in new[] { "MaTaiKhoan", "MaChucVu", "SoLanSai", "KhoaDen" })
						if (dgvTaiKhoan.Columns.Contains(col)) dgvTaiKhoan.Columns[col].Visible = false;
					dgvTaiKhoan.ClearSelection();
				}
			}
			catch (Exception ex) { MessageBox.Show("Không tải được tài khoản: " + ex.Message); }
		}

		private void dgvTaiKhoan_SelectionChanged(object sender, EventArgs e)
		{
			if (dgvTaiKhoan.SelectedRows.Count == 0) return;
			var r = dgvTaiKhoan.SelectedRows[0];
			if (r.DataBoundItem == null) return;
			_maDangChon = Convert.ToInt32(r.Cells["MaTaiKhoan"].Value);
			_maChucVuCu = Convert.ToInt32(r.Cells["MaChucVu"].Value);
			txtTenDangNhap.Text = Convert.ToString(r.Cells["TenDangNhap"].Value);
			txtHoTen.Text = Convert.ToString(r.Cells["HoTen"].Value);
			cboChucVu.SelectedValue = _maChucVuCu;
			chkHoatDong.Checked = Convert.ToBoolean(r.Cells["DangHoatDong"].Value);
			txtMatKhau.Clear();
			txtNhapLai.Clear();
		}

		private void LamMoi(object sender, EventArgs e)
		{
			_maDangChon = 0;
			_maChucVuCu = 0;
			dgvTaiKhoan.ClearSelection();
			txtTenDangNhap.Clear();
			txtHoTen.Clear();
			txtMatKhau.Clear();
			txtNhapLai.Clear();
			chkHoatDong.Checked = true;
			if (cboChucVu.Items.Count > 0) cboChucVu.SelectedValue = (byte)3;
			txtTenDangNhap.Focus();
		}

		private static void TaoHash(string password, out byte[] salt, out byte[] hash)
		{
			salt = new byte[16];
			using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
			using (var pbkdf = new Rfc2898DeriveBytes(password, salt, VongLap, HashAlgorithmName.SHA256))
				hash = pbkdf.GetBytes(32);
		}

		private bool KiemTraNhap(bool canMatKhau)
		{
			if (txtTenDangNhap.Text.Trim().Length == 0 || txtHoTen.Text.Trim().Length == 0)
			{
				MessageBox.Show("Nhập đầy đủ tên đăng nhập và họ tên."); return false;
			}
			if (cboChucVu.SelectedValue == null || cboChucVu.SelectedValue is DataRowView)
			{
				MessageBox.Show("Chọn chức vụ."); return false;
			}
			if (canMatKhau || txtMatKhau.Text.Length > 0 || txtNhapLai.Text.Length > 0)
			{
				if (txtMatKhau.Text.Length < 12 || txtMatKhau.Text != txtNhapLai.Text)
				{
					MessageBox.Show("Mật khẩu cần ít nhất 12 ký tự và hai lần nhập phải trùng nhau.");
					return false;
				}
			}
			return true;
		}

		private bool CoGiamDocKhac(SqlConnection cn, SqlTransaction tx, int ngoaiTru)
		{
			using (var cmd = new SqlCommand(
				"SELECT COUNT(*) FROM dbo.TaiKhoanDangNhap WITH (UPDLOCK,HOLDLOCK) WHERE MaChucVu=1 AND DangHoatDong=1 AND MaTaiKhoan<>@id", cn, tx))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = ngoaiTru;
				return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
			}
		}

		private bool DaCoGiamDoc(SqlConnection cn, SqlTransaction tx, int ngoaiTru)
		{
			using (var cmd = new SqlCommand(
				"SELECT COUNT(*) FROM dbo.TaiKhoanDangNhap WITH (UPDLOCK,HOLDLOCK) WHERE MaChucVu=1 AND MaTaiKhoan<>@id", cn, tx))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = ngoaiTru;
				return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
			}
		}

		private void btnThem_Click(object sender, EventArgs e)
		{
			if (!QuyenHoSo.KiemTraQuyen(this)) return;
			if (!KiemTraNhap(true)) return;
			byte role = Convert.ToByte(cboChucVu.SelectedValue);
			byte[] salt, hash;
			TaoHash(txtMatKhau.Text, out salt, out hash);
			try
			{
				using (var cn = KetNoi())
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							if (role == 1 && DaCoGiamDoc(cn, tx, 0))
								throw new InvalidOperationException("Hệ thống chỉ cho phép một tài khoản Giám đốc.");
							using (var cmd = new SqlCommand(@"
INSERT INTO dbo.TaiKhoanDangNhap
(TenDangNhap,HoTen,MaChucVu,MatKhauHash,MatKhauSalt,SoLanLap,DangHoatDong)
VALUES (@user,@name,@role,@hash,@salt,@rounds,@active)", cn, tx))
							{
								cmd.Parameters.Add("@user", SqlDbType.NVarChar, 60).Value = txtTenDangNhap.Text.Trim();
								cmd.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = txtHoTen.Text.Trim();
								cmd.Parameters.Add("@role", SqlDbType.TinyInt).Value = role;
								cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
								cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
								cmd.Parameters.Add("@rounds", SqlDbType.Int).Value = VongLap;
								cmd.Parameters.Add("@active", SqlDbType.Bit).Value = chkHoatDong.Checked;
								cmd.ExecuteNonQuery();
							}
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				MessageBox.Show("Thêm tài khoản thành công.");
				TaiDanhSach(); LamMoi(null, EventArgs.Empty);
			}
			catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
			{ MessageBox.Show("Tên đăng nhập đã tồn tại."); }
			catch (Exception ex) { MessageBox.Show("Không thể thêm: " + ex.Message); }
		}

		private void btnSua_Click(object sender, EventArgs e)
		{
			if (!QuyenHoSo.KiemTraQuyen(this)) return;
			if (_maDangChon <= 0) { MessageBox.Show("Hãy chọn tài khoản cần sửa."); return; }
			if (!KiemTraNhap(false)) return;
			byte role = Convert.ToByte(cboChucVu.SelectedValue);
			if (_maDangChon == BankSecurity.Current.Id &&
				(role != _maChucVuCu || !chkHoatDong.Checked))
			{
				MessageBox.Show("Không được tự thay đổi chức vụ hoặc khóa tài khoản đang đăng nhập.");
				return;
			}
			try
			{
				using (var cn = KetNoi())
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							byte oldRole;
							bool oldActive;
							using (var check = new SqlCommand(
								"SELECT MaChucVu,DangHoatDong FROM dbo.TaiKhoanDangNhap WITH (UPDLOCK,HOLDLOCK) WHERE MaTaiKhoan=@id", cn, tx))
							{
								check.Parameters.Add("@id", SqlDbType.Int).Value = _maDangChon;
								using (var rd = check.ExecuteReader())
								{
									if (!rd.Read()) throw new InvalidOperationException("Tài khoản không còn tồn tại.");
									oldRole = rd.GetByte(0);
									oldActive = rd.GetBoolean(1);
								}
							}
							if (role == 1 && oldRole != 1 && DaCoGiamDoc(cn, tx, _maDangChon))
								throw new InvalidOperationException("Chỉ được có một Giám đốc.");
							if (oldRole == 1 && oldActive && (role != 1 || !chkHoatDong.Checked) &&
								!CoGiamDocKhac(cn, tx, _maDangChon))
								throw new InvalidOperationException("Không được khóa hoặc hạ quyền Giám đốc duy nhất.");
							using (var cmd = new SqlCommand(@"
UPDATE dbo.TaiKhoanDangNhap
SET TenDangNhap=@user, HoTen=@name, MaChucVu=@role, DangHoatDong=@active
WHERE MaTaiKhoan=@id", cn, tx))
							{
								cmd.Parameters.Add("@id", SqlDbType.Int).Value = _maDangChon;
								cmd.Parameters.Add("@user", SqlDbType.NVarChar, 60).Value = txtTenDangNhap.Text.Trim();
								cmd.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = txtHoTen.Text.Trim();
								cmd.Parameters.Add("@role", SqlDbType.TinyInt).Value = role;
								cmd.Parameters.Add("@active", SqlDbType.Bit).Value = chkHoatDong.Checked;
								cmd.ExecuteNonQuery();
							}
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				MessageBox.Show("Cập nhật tài khoản thành công.");
				TaiDanhSach(); LamMoi(null, EventArgs.Empty);
			}
			catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
			{ MessageBox.Show("Tên đăng nhập đã tồn tại."); }
			catch (Exception ex) { MessageBox.Show("Không thể cập nhật: " + ex.Message); }
		}

		private void btnDoiMatKhau_Click(object sender, EventArgs e)
		{
			if (!QuyenHoSo.KiemTraQuyen(this)) return;
			if (_maDangChon <= 0) { MessageBox.Show("Chọn tài khoản cần đặt lại mật khẩu."); return; }
			if (txtMatKhau.Text.Length < 12 || txtMatKhau.Text != txtNhapLai.Text)
			{
				MessageBox.Show("Nhập mật khẩu mới tối thiểu 12 ký tự và xác nhận chính xác.");
				return;
			}
			if (MessageBox.Show("Đặt lại mật khẩu cho tài khoản đang chọn?", "Xác nhận",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
			byte[] salt, hash;
			TaoHash(txtMatKhau.Text, out salt, out hash);
			try
			{
				using (var cn = KetNoi())
				using (var cmd = new SqlCommand(@"
UPDATE dbo.TaiKhoanDangNhap
SET MatKhauHash=@hash,MatKhauSalt=@salt,SoLanLap=@rounds,SoLanSai=0,KhoaDen=NULL
WHERE MaTaiKhoan=@id", cn))
				{
					cmd.Parameters.Add("@id", SqlDbType.Int).Value = _maDangChon;
					cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
					cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
					cmd.Parameters.Add("@rounds", SqlDbType.Int).Value = VongLap;
					cn.Open();
					if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Tài khoản không tồn tại.");
				}
				txtMatKhau.Clear(); txtNhapLai.Clear();
				MessageBox.Show("Đặt lại mật khẩu thành công.");
				TaiDanhSach();
			}
			catch (Exception ex) { MessageBox.Show("Không thể đổi mật khẩu: " + ex.Message); }
		}

		private void btnKhoa_Click(object sender, EventArgs e)
		{
			if (!QuyenHoSo.KiemTraQuyen(this)) return;
			if (_maDangChon <= 0) { MessageBox.Show("Chọn tài khoản cần khóa/mở khóa."); return; }
			if (_maDangChon == BankSecurity.Current.Id)
			{
				MessageBox.Show("Không được tự khóa tài khoản đang đăng nhập."); return;
			}
			try
			{
				using (var cn = KetNoi())
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							byte role;
							bool active;
							using (var cmd = new SqlCommand(
								"SELECT MaChucVu,DangHoatDong FROM dbo.TaiKhoanDangNhap WITH (UPDLOCK,HOLDLOCK) WHERE MaTaiKhoan=@id", cn, tx))
							{
								cmd.Parameters.Add("@id", SqlDbType.Int).Value = _maDangChon;
								using (var rd = cmd.ExecuteReader())
								{
									if (!rd.Read()) throw new InvalidOperationException("Tài khoản không tồn tại.");
									role = rd.GetByte(0);
									active = rd.GetBoolean(1);
								}
							}
							if (active && role == 1 && !CoGiamDocKhac(cn, tx, _maDangChon))
								throw new InvalidOperationException("Không được khóa Giám đốc duy nhất.");
							using (var cmd = new SqlCommand(@"
UPDATE dbo.TaiKhoanDangNhap
SET DangHoatDong=@active,SoLanSai=0,KhoaDen=NULL
WHERE MaTaiKhoan=@id", cn, tx))
							{
								cmd.Parameters.Add("@id", SqlDbType.Int).Value = _maDangChon;
								cmd.Parameters.Add("@active", SqlDbType.Bit).Value = !active;
								cmd.ExecuteNonQuery();
							}
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				MessageBox.Show("Đã thay đổi trạng thái tài khoản.");
				TaiDanhSach(); LamMoi(null, EventArgs.Empty);
			}
			catch (Exception ex) { MessageBox.Show("Không thể đổi trạng thái: " + ex.Message); }
		}

		private void btnLamMoi_Click(object sender, EventArgs e)
		{
			txtTimKiem.Clear();
			TaiDanhSach();
			LamMoi(null, EventArgs.Empty);
		}
		private void txtTimKiem_TextChanged(object sender, EventArgs e) { TaiDanhSach(); }
	}
}
