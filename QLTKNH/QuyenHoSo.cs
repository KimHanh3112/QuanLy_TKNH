using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace QLTKNH
{
	internal static class QuyenHoSo
	{
		// Luôn sử dụng cùng SQL Server với lớp đăng nhập.
		internal static string Conn { get { return BankSecurity.ConnStr; } }
		internal static int Id
		{
			get
			{
				if (!BankSecurity.IsLoggedIn || BankSecurity.Current == null)
					throw new UnauthorizedAccessException("Vui lòng đăng nhập lại.");
				return BankSecurity.Current.Id;
			}
		}
		internal static bool LaGiamDoc()
		{
			if (!BankSecurity.IsLoggedIn) return false;
			using (var cn = new SqlConnection(Conn))
			using (var cmd = new SqlCommand(@"SELECT COUNT(1) FROM dbo.NhanVienNganHang
                 WHERE Id=@id AND ChucVu=N'Giám đốc' AND TrangThai<>N'Nghỉ việc'
                   AND ISNULL(BiKhoa,0)=0", cn))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = Id;
				cn.Open();
				return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
			}
		}
		internal static void YeuCauGiamDoc()
		{
			if (!LaGiamDoc()) throw new UnauthorizedAccessException("Chỉ Giám đốc có quyền quản lý nhân viên.");
		}
		internal static bool KiemTraQuyen(Form owner)
		{
			try { YeuCauGiamDoc(); return true; }
			catch (Exception ex)
			{
				MessageBox.Show(owner, ex.Message, "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
		}
		// Tương thích chính xác với HashPassword trong FrmDangNhap.cs hiện tại:
		// Rfc2898DeriveBytes(password, salt, 150000), sử dụng PRF mặc định SHA1.
		internal static byte[] Bam(string password, byte[] salt)
		{
			using (var kdf = new Rfc2898DeriveBytes(password, salt, 150000))
				return kdf.GetBytes(32);
		}
		internal static bool TrungHash(byte[] a, byte[] b)
		{
			if (a == null || b == null) return false;
			int diff = a.Length ^ b.Length;
			for (int i = 0; i < Math.Min(a.Length, b.Length); i++) diff |= a[i] ^ b[i];
			return diff == 0;
		}
		internal static void GhiNhatKy(SqlConnection cn, SqlTransaction tx, string hanhDong, string chiTiet, int? idTacDong = null)
		{
			using (var cmd = new SqlCommand(@"INSERT dbo.NhatKyHoSo
              (NguoiThucHienId,TenNguoiThucHien,HanhDong,ChiTiet,TaiKhoanTacDongId)
              VALUES(@id,@name,@action,@detail,@target)", cn, tx))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = Id;
				cmd.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = BankSecurity.Current.HoTen ?? "";
				cmd.Parameters.Add("@action", SqlDbType.NVarChar, 100).Value = hanhDong;
				cmd.Parameters.Add("@detail", SqlDbType.NVarChar, 1000).Value = chiTiet ?? "";
				cmd.Parameters.Add("@target", SqlDbType.Int).Value = (object)idTacDong ?? DBNull.Value;
				cmd.ExecuteNonQuery();
			}
			using (var cmd = new SqlCommand(@"INSERT dbo.NhatKyNhanVien
                (NhanVienId,TenDangNhap,HanhDong,ChiTiet,ThoiGian)
                VALUES(@id,@login,@action,@detail,GETDATE())", cn, tx))
			{
				cmd.Parameters.Add("@id", SqlDbType.Int).Value = Id;
				cmd.Parameters.Add("@login", SqlDbType.VarChar, 60).Value = BankSecurity.Current.TenDangNhap;
				cmd.Parameters.Add("@action", SqlDbType.NVarChar, 100).Value = hanhDong;
				cmd.Parameters.Add("@detail", SqlDbType.NVarChar, 1000).Value = chiTiet ?? "";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
