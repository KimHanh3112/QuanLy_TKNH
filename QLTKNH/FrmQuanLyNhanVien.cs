using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace QLTKNH
{
	public partial class FrmQuanLyNhanVien : Form
	{
		private int _idChon;
		private bool _dangTai;
		private bool _cheDoThem;
		private bool _dangLocDienThoai;

		public FrmQuanLyNhanVien()
		{
			InitializeComponent();
			Load += (s, e) =>
			{
				if (!QuyenHoSo.KiemTraQuyen(this)) { Close(); return; }
				TaiDanhSach("");
			};
		}

		private void TaiDanhSach(string tuKhoa)
		{
			QuyenHoSo.YeuCauGiamDoc();
			using (var cn = new SqlConnection(QuyenHoSo.Conn))
			using (var cmd = new SqlCommand(@"
SELECT Id, TenDangNhap, HoTen, Email, SoDienThoai, ChucVu, TrangThai, BiKhoa
FROM dbo.NhanVienNganHang
WHERE (@tu=N'' OR TenDangNhap LIKE N'%'+@tu+N'%' OR SoDienThoai LIKE @tu+'%')
ORDER BY CASE WHEN ChucVu=N'Giám đốc' THEN 0 ELSE 1 END, TenDangNhap", cn))
			{
				cmd.Parameters.Add("@tu", SqlDbType.NVarChar, 120).Value = tuKhoa ?? "";
				var table = new DataTable();
				using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(table);
				_dangTai = true;
				try
				{
					dgvNhanVien.DataSource = table;
					VietHoaCot();
					dgvNhanVien.ClearSelection();
					dgvNhanVien.CurrentCell = null;
					_idChon = 0;
					_cheDoThem = false;
					LamTrongThongTin();
					btnLuu.Enabled = false;
					lblThongBao.Text = "Danh sách: " + table.Rows.Count + " nhân viên.";
				}
				finally { _dangTai = false; }
			}
		}

		private void VietHoaCot()
		{
			string[,] columns = {
				{"TenDangNhap", "Tên đăng nhập"}, {"HoTen", "Họ tên"},
				{"Email", "Gmail / Email"}, {"SoDienThoai", "Số điện thoại"},
				{"ChucVu", "Vai trò"}, {"TrangThai", "Trạng thái"},
				{"BiKhoa", "Bị khóa"}
			};
			if (dgvNhanVien.Columns.Contains("Id")) dgvNhanVien.Columns["Id"].Visible = false;
			foreach (var i in Enumerable.Range(0, columns.GetLength(0)))
				if (dgvNhanVien.Columns.Contains(columns[i, 0]))
					dgvNhanVien.Columns[columns[i, 0]].HeaderText = columns[i, 1];
			if (dgvNhanVien.Columns.Contains("BiKhoa"))
				dgvNhanVien.Columns["BiKhoa"].FillWeight = 55;
		}

		private void BtnTim_Click(object sender, EventArgs e)
		{
			try
			{
				string term = txtTim.Text.Trim();
				if (term.Length == 0) { TaiDanhSach(""); return; }
				TaiDanhSach(term);
				var dt = dgvNhanVien.DataSource as DataTable;
				if (dt != null && dt.Rows.Count == 0)
				{
					ChuyenSangThemMoi(term);
					lblThongBao.Text = "Không tìm thấy. Có thể nhập thông tin dưới đây để thêm nhân viên mới.";
					MessageBox.Show(this, "Không tìm thấy nhân viên với tên đăng nhập hoặc số điện thoại đã nhập.\nBạn có thể thêm nhân viên ở phần thông tin bên dưới.",
						"Không có kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else lblThongBao.Text = "Tìm thấy " + (dt == null ? 0 : dt.Rows.Count) + " kết quả. Nhấp vào dòng để sửa.";
			}
			catch (Exception ex) { MessageBox.Show(this, ex.Message, "Lỗi tìm kiếm"); }
		}

		private void ChuyenSangThemMoi(string term)
		{
			_idChon = 0;
			_cheDoThem = true;
			_dangTai = true;
			try
			{
				LamTrongThongTin();
				if (term.All(char.IsDigit)) txtDienThoai.Text = term;
				else txtTenDangNhap.Text = term;
				txtTenDangNhap.ReadOnly = false;
				txtMatKhauMoi.Visible = true;
				lblMatKhauMoi.Visible = true;
				btnLuu.Enabled = true;
				btnLuu.Text = "Thêm nhân viên";
				lblVaiTro.Text = "Giao dịch viên";
				lblTrangThai.Text = "Chưa tạo";
				txtHoTen.Focus();
			}
			finally { _dangTai = false; }
		}

		private void LamTrongThongTin()
		{
			txtTenDangNhap.Text = "";
			txtHoTen.Text = "";
			txtEmail.Text = "";
			txtDienThoai.Text = "";
			txtMatKhauMoi.Text = "";
			lblVaiTro.Text = "—";
			lblTrangThai.Text = "—";
			txtTenDangNhap.ReadOnly = true;
			txtMatKhauMoi.Visible = false;
			lblMatKhauMoi.Visible = false;
			btnLuu.Text = "Lưu hồ sơ";
		}

		private void DgvNhanVien_SelectionChanged(object sender, EventArgs e)
		{
			if (_dangTai || dgvNhanVien.SelectedRows.Count == 0) return;
			var r = dgvNhanVien.SelectedRows[0].DataBoundItem as DataRowView;
			if (r == null) return;
			_idChon = Convert.ToInt32(r["Id"]);
			_cheDoThem = false;
			_dangTai = true;
			try
			{
				LamTrongThongTin();
				txtTenDangNhap.Text = Convert.ToString(r["TenDangNhap"]);
				txtHoTen.Text = Convert.ToString(r["HoTen"]);
				txtEmail.Text = Convert.ToString(r["Email"]);
				txtDienThoai.Text = Convert.ToString(r["SoDienThoai"]);
				lblVaiTro.Text = Convert.ToString(r["ChucVu"]);
				lblTrangThai.Text = Convert.ToBoolean(r["BiKhoa"]) ? "Đã khóa" : Convert.ToString(r["TrangThai"]);
				btnLuu.Enabled = _idChon != QuyenHoSo.Id && lblVaiTro.Text != "Giám đốc";
				lblThongBao.Text = "Đang xem: " + txtTenDangNhap.Text;
			}
			finally { _dangTai = false; }
		}

		private void TimKiemReset_Click(object sender, EventArgs e)
		{
			txtTim.Clear();
			try { TaiDanhSach(""); } catch (Exception ex) { MessageBox.Show(this, ex.Message); }
		}

		private void TxtDienThoai_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
		}
		private void TxtDienThoai_TextChanged(object sender, EventArgs e)
		{
			if (_dangLocDienThoai) return;
			string old = txtDienThoai.Text;
			string digits = new string(old.Where(c => c >= '0' && c <= '9').Take(11).ToArray());
			if (digits == old) return;
			_dangLocDienThoai = true;
			txtDienThoai.Text = digits;
			txtDienThoai.SelectionStart = digits.Length;
			_dangLocDienThoai = false;
		}

		private bool KiemTraThongTin()
		{
			string user = txtTenDangNhap.Text.Trim();
			string name = txtHoTen.Text.Trim();
			string phone = txtDienThoai.Text.Trim();
			string email = txtEmail.Text.Trim();
			if (name.Length == 0 || name.Length > 100) { MessageBox.Show("Họ tên phải có từ 1 đến 100 ký tự."); return false; }
			if (phone.Length < 10 || phone.Length > 11 || !phone.All(c => c >= '0' && c <= '9'))
			{ MessageBox.Show("Số điện thoại phải gồm 10 đến 11 chữ số."); return false; }
			if (email.Length > 150) { MessageBox.Show("Email tối đa 150 ký tự."); return false; }
			if (email.Length > 0)
			{
				try { if (new MailAddress(email).Address != email) throw new FormatException(); }
				catch { MessageBox.Show("Địa chỉ Gmail/Email không hợp lệ."); return false; }
			}
			if (_cheDoThem)
			{
				if (user.Length == 0 || user.Length > 60 || !user.All(c => char.IsLetterOrDigit(c) || c == '_'))
				{ MessageBox.Show("Tên đăng nhập phải gồm chữ, số hoặc dấu gạch dưới, tối đa 60 ký tự."); return false; }
				if (txtMatKhauMoi.Text.Length < 12)
				{ MessageBox.Show("Mật khẩu nhân viên mới phải có ít nhất 12 ký tự."); return false; }
			}
			return true;
		}

		private static void KiemTraQuyenTrongGiaoDich(SqlConnection cn, SqlTransaction tx)
		{
			using (var cmd = new SqlCommand(@"IF NOT EXISTS
(SELECT 1 FROM dbo.NhanVienNganHang WITH(UPDLOCK,HOLDLOCK)
 WHERE Id=@id AND ChucVu=N'Giám đốc' AND BiKhoa=0 AND TrangThai<>N'Nghỉ việc')
 THROW 52001,N'Chỉ Giám đốc đang hoạt động mới được thay đổi nhân viên.',1;", cn, tx))
			{ cmd.Parameters.Add("@id", SqlDbType.Int).Value = QuyenHoSo.Id; cmd.ExecuteNonQuery(); }
		}

		private void BtnLuu_Click(object sender, EventArgs e)
		{
			if (!KiemTraThongTin()) return;
			try
			{
				QuyenHoSo.YeuCauGiamDoc();
				using (var cn = new SqlConnection(QuyenHoSo.Conn))
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							KiemTraQuyenTrongGiaoDich(cn, tx);
							if (_cheDoThem)
							{
								byte[] salt = new byte[16];
								using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
								byte[] hash = QuyenHoSo.Bam(txtMatKhauMoi.Text, salt);
								int newId;
								using (var cmd = new SqlCommand(@"INSERT dbo.NhanVienNganHang
(TenDangNhap,MatKhauHash,MatKhauSalt,HoTen,SoDienThoai,Email,
 ChucVu,CaLam,TrangThai,BiKhoa,DangHoatDong)
OUTPUT INSERTED.Id
VALUES(@login,@hash,@salt,@name,@phone,@mail,N'Giao dịch viên',N'Cả hai',N'Offline',0,0);", cn, tx))
								{
									cmd.Parameters.Add("@login", SqlDbType.VarChar, 60).Value = txtTenDangNhap.Text.Trim();
									cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = hash;
									cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
									cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = txtHoTen.Text.Trim();
									cmd.Parameters.Add("@phone", SqlDbType.VarChar, 11).Value = txtDienThoai.Text.Trim();
									cmd.Parameters.Add("@mail", SqlDbType.NVarChar, 150).Value = (object)txtEmail.Text.Trim() ?? DBNull.Value;
									newId = Convert.ToInt32(cmd.ExecuteScalar());
								}
								QuyenHoSo.GhiNhatKy(cn, tx, "Thêm nhân viên", "Tạo tài khoản " + txtTenDangNhap.Text.Trim(), newId);
							}
							else
							{
								if (_idChon <= 0 || _idChon == QuyenHoSo.Id)
									throw new UnauthorizedAccessException("Không được tự chỉnh sửa tại màn hình quản lý nhân viên.");
								using (var cmd = new SqlCommand(@"UPDATE dbo.NhanVienNganHang
 SET HoTen=@name,Email=@mail,SoDienThoai=@phone
 WHERE Id=@target AND Id<>@actor AND ChucVu<>N'Giám đốc' AND TrangThai<>N'Nghỉ việc'", cn, tx))
								{
									cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = txtHoTen.Text.Trim();
									cmd.Parameters.Add("@mail", SqlDbType.NVarChar, 150).Value = (object)txtEmail.Text.Trim() ?? DBNull.Value;
									cmd.Parameters.Add("@phone", SqlDbType.VarChar, 11).Value = txtDienThoai.Text.Trim();
									cmd.Parameters.Add("@target", SqlDbType.Int).Value = _idChon;
									cmd.Parameters.Add("@actor", SqlDbType.Int).Value = QuyenHoSo.Id;
									if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không thể cập nhật tài khoản được chọn.");
								}
								QuyenHoSo.GhiNhatKy(cn, tx, "Sửa nhân viên", "Sửa thông tin nhân viên ID=" + _idChon, _idChon);
							}
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				MessageBox.Show(this, _cheDoThem ? "Đã tạo tài khoản nhân viên mới." : "Đã lưu thông tin nhân viên.", "Thành công");
				txtTim.Clear();
				TaiDanhSach("");
			}
			catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
			{ MessageBox.Show(this, "Tên đăng nhập đã tồn tại. Vui lòng kiểm tra lại."); }
			catch (Exception ex) { MessageBox.Show(this, "Không lưu được: " + ex.Message); }
		}

		private void MenuNhanVien_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bool can = _idChon > 0 && _idChon != QuyenHoSo.Id && lblVaiTro.Text != "Giám đốc" && lblTrangThai.Text != "Nghỉ việc";
			miKhoaMo.Enabled = can && QuyenHoSo.LaGiamDoc();
			miKhoaMo.Text = lblTrangThai.Text == "Đã khóa" ? "Mở khóa tài khoản" : "Khóa tài khoản";
		}
		private void DgvNhanVien_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
			dgvNhanVien.ClearSelection();
			dgvNhanVien.Rows[e.RowIndex].Selected = true;
			int col = e.ColumnIndex < 0 ? 0 : e.ColumnIndex;
			dgvNhanVien.CurrentCell = dgvNhanVien.Rows[e.RowIndex].Cells[col];
		}
		private void MiKhoaMo_Click(object sender, EventArgs e)
		{
			if (_idChon <= 0 || _idChon == QuyenHoSo.Id) return;
			try
			{
				QuyenHoSo.YeuCauGiamDoc();
				bool willLock = lblTrangThai.Text != "Đã khóa";
				if (MessageBox.Show(this, "Xác nhận " + (willLock ? "khóa" : "mở khóa") + " tài khoản " + txtTenDangNhap.Text + "?",
					"Quản lý nhân viên", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
				using (var cn = new SqlConnection(QuyenHoSo.Conn))
				{
					cn.Open();
					using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
					{
						try
						{
							KiemTraQuyenTrongGiaoDich(cn, tx);
							using (var cmd = new SqlCommand(@"UPDATE dbo.NhanVienNganHang
 SET BiKhoa=@lock, DangHoatDong=CASE WHEN @lock=1 THEN 0 ELSE DangHoatDong END,
 TrangThai=CASE WHEN @lock=1 THEN N'Offline' ELSE TrangThai END
 WHERE Id=@target AND Id<>@actor AND ChucVu<>N'Giám đốc' AND TrangThai<>N'Nghỉ việc'", cn, tx))
							{
								cmd.Parameters.Add("@lock", SqlDbType.Bit).Value = willLock;
								cmd.Parameters.Add("@target", SqlDbType.Int).Value = _idChon;
								cmd.Parameters.Add("@actor", SqlDbType.Int).Value = QuyenHoSo.Id;
								if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không có quyền thay đổi trạng thái tài khoản này.");
							}
							QuyenHoSo.GhiNhatKy(cn, tx, willLock ? "Khóa nhân viên" : "Mở khóa nhân viên",
								"Tài khoản " + txtTenDangNhap.Text + (willLock ? " bị khóa bởi Giám đốc" : " được mở khóa bởi Giám đốc"), _idChon);
							tx.Commit();
						}
						catch { tx.Rollback(); throw; }
					}
				}
				MessageBox.Show(this, willLock ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.");
				TaiDanhSach("");
			}
			catch (Exception ex) { MessageBox.Show(this, "Không thể cập nhật trạng thái: " + ex.Message); }
		}
	}
}
