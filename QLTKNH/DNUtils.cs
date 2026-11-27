
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QLTKNH
{
    internal static class DNUtils
    {
        // Kết nối SQL Server của project
        public const string Conn =
			@"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        // =========================================
        // 1. THAM SỐ SQL
        // =========================================

        public static SqlParameter P(
            string name, object value)
        {
            return new SqlParameter(
                name,
                value ?? DBNull.Value
            );
        }

        // =========================================
        // 2. ĐỌC DỮ LIỆU SQL
        // =========================================

        public static DataTable Query(
            string sql,
            params SqlParameter[] parameters)
        {
            using (SqlConnection cn =
                new SqlConnection(Conn))
            using (SqlCommand cmd =
                new SqlCommand(sql, cn))
            {
                if (parameters != null &&
                    parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                DataTable dt = new DataTable();

                using (SqlDataAdapter da =
                    new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                return dt;
            }
        }

        // =========================================
        // 3. THỰC THI INSERT / UPDATE
        // =========================================

        public static int Execute(
            string sql,
            params SqlParameter[] parameters)
        {
            using (SqlConnection cn =
                new SqlConnection(Conn))
            using (SqlCommand cmd =
                new SqlCommand(sql, cn))
            {
                if (parameters != null &&
                    parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                cn.Open();

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================================
        // 4. NHÂN VIÊN ĐANG ĐĂNG NHẬP
        // =========================================

        public static string NhanVien()
        {
            // Đọc phiên BankSecurity khi lớp này có
            // trong project, không tạo tài khoản giả.

            Type securityType = null;

            foreach (Assembly assembly
                in AppDomain.CurrentDomain.GetAssemblies())
            {
                securityType =
                    assembly.GetType("QLTKNH.BankSecurity");

                if (securityType != null)
                    break;
            }

            if (securityType == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy lớp BankSecurity. " +
                    "Hãy kiểm tra file đăng nhập của project.");
            }

            BindingFlags flags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static;

            PropertyInfo currentProperty =
                securityType.GetProperty(
                    "Current", flags);

            if (currentProperty == null)
            {
                throw new InvalidOperationException(
                    "BankSecurity chưa có thuộc tính Current.");
            }

            object current =
                currentProperty.GetValue(null, null);

            if (current == null)
            {
                throw new InvalidOperationException(
                    "Chưa đăng nhập tài khoản nhân viên.");
            }

            Type sessionType = current.GetType();

            PropertyInfo hoTenProperty =
                sessionType.GetProperty(
                    "HoTen",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

            string hoTen = hoTenProperty == null
                ? ""
                : Convert.ToString(
                    hoTenProperty.GetValue(current, null));

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                throw new InvalidOperationException(
                    "Không xác định được họ tên nhân viên " +
                    "đang đăng nhập.");
            }

            return hoTen.Trim();
        }

        // =========================================
        // 5. KIỂM TRA SỐ TIỀN
        // =========================================

        public static decimal Money(
            string value,
            bool positive = true)
        {
            decimal result;

            if (!decimal.TryParse(value, out result))
            {
                throw new InvalidOperationException(
                    "Số tiền không hợp lệ.");
            }

            if (positive && result <= 0)
            {
                throw new InvalidOperationException(
                    "Số tiền phải lớn hơn 0.");
            }

            if (!positive && result < 0)
            {
                throw new InvalidOperationException(
                    "Số tiền không được âm.");
            }

            return result;
        }

        // =========================================
        // 6. GIAO DIỆN DATAGRIDVIEW
        // =========================================

        public static void Style(DataGridView dgv)
        {
            if (dgv == null)
                return;

            Color nen =
                Color.FromArgb(15, 23, 42);

            Color nenBang =
                Color.FromArgb(30, 41, 59);

            Color xanh =
                Color.FromArgb(37, 99, 235);

            dgv.BackgroundColor = nen;
            dgv.GridColor =
                Color.FromArgb(51, 65, 85);

            dgv.BorderStyle =
                BorderStyle.None;

            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.MultiSelect = false;

            dgv.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersHeight = 38;
            dgv.RowTemplate.Height = 34;

            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                nenBang;

            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgv.DefaultCellStyle.BackColor =
                nen;

            dgv.DefaultCellStyle.ForeColor =
                Color.White;

            dgv.DefaultCellStyle.SelectionBackColor =
                xanh;

            dgv.DefaultCellStyle.SelectionForeColor =
                Color.White;

            dgv.AlternatingRowsDefaultCellStyle.BackColor =
                nenBang;

            dgv.AlternatingRowsDefaultCellStyle.ForeColor =
                Color.White;
        }

        // =========================================
        // 7. HỘP THOẠI NHẬP THÔNG TIN
        // =========================================

        public static string[] Input(
            Form owner,
            string title,
            params string[] labels)
        {
            using (Form frm = new Form())
            {
                frm.Text = title;

                frm.StartPosition =
                    FormStartPosition.CenterParent;

                frm.ClientSize = new Size(
                    650,
                    100 + labels.Length * 45);

                frm.FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                frm.MaximizeBox = false;
                frm.MinimizeBox = false;

                frm.BackColor =
                    Color.FromArgb(15, 23, 42);

                frm.ForeColor = Color.White;
                frm.Font =
                    new Font("Segoe UI", 10F);

                TextBox[] inputs =
                    new TextBox[labels.Length];

                for (int i = 0; i < labels.Length; i++)
                {
                    Label lbl = new Label
                    {
                        Text = labels[i],
                        Location =
                            new Point(15, 20 + i * 45),
                        Size = new Size(220, 30),
                        ForeColor = Color.White
                    };

                    TextBox txt = new TextBox
                    {
                        Location =
                            new Point(240, 17 + i * 45),
                        Size = new Size(380, 30),
                        MaxLength = 200
                    };

                    inputs[i] = txt;

                    frm.Controls.Add(lbl);
                    frm.Controls.Add(txt);
                }

                Button btnOK = new Button
                {
                    Text = "Xác nhận",
                    Size = new Size(125, 35),
                    Location = new Point(
                        350,
                        25 + labels.Length * 45),
                    DialogResult = DialogResult.OK
                };

                Button btnCancel = new Button
                {
                    Text = "Hủy",
                    Size = new Size(125, 35),
                    Location = new Point(
                        495,
                        25 + labels.Length * 45),
                    DialogResult = DialogResult.Cancel
                };

                frm.Controls.Add(btnOK);
                frm.Controls.Add(btnCancel);

                frm.AcceptButton = btnOK;
                frm.CancelButton = btnCancel;

                if (frm.ShowDialog(owner) != DialogResult.OK)
                    return null;

                return inputs
                    .Select(x => x.Text.Trim())
                    .ToArray();
            }
        }

        // =========================================
        // 8. TÌM DOANH NGHIỆP THEO MST
        // =========================================

        public static int Company(string mst)
        {
            DataTable dt = Query(@"
SELECT MaDN
FROM dbo.DN_DoanhNghiep
WHERE MaSoThue = @mst
AND TrangThai = N'Hoạt động'",
                P("@mst", mst));

            if (dt.Rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy doanh nghiệp " +
                    "đang hoạt động với mã số thuế này.");
            }

            return Convert.ToInt32(dt.Rows[0]["MaDN"]);
        }

        // =========================================
        // 9. TÌM NGƯỜI PHÁP LÝ
        // =========================================

        public static int LegalPerson(
            int maDN,
            string cccd)
        {
            DataTable dt = Query(@"
SELECT MaNguoi
FROM dbo.DN_NguoiPhapLy
WHERE MaDN = @dn
AND CCCD = @cccd
AND DangHieuLuc = 1",
                P("@dn", maDN),
                P("@cccd", cccd));

            if (dt.Rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "CCCD người pháp lý không thuộc " +
                    "doanh nghiệp hoặc đã hết hiệu lực.");
            }

            return Convert.ToInt32(
                dt.Rows[0]["MaNguoi"]);
        }

        // =========================================
        // 10. MỞ TÀI KHOẢN DOANH NGHIỆP
        // =========================================

        public static void OpenAccount(
            string mst,
            string stk,
            string loai,
            string cccd1,
            string cccd2,
            decimal tien,
            decimal lai,
            int kyHan)
        {
            if (string.IsNullOrWhiteSpace(stk) ||
                stk.Length > 20)
            {
                throw new InvalidOperationException(
                    "Số tài khoản phải từ 1 đến 20 ký tự.");
            }

            if (cccd1 == cccd2)
            {
                throw new InvalidOperationException(
                    "Hai người chịu trách nhiệm pháp lý " +
                    "phải khác nhau.");
            }

            int maDN = Company(mst);

            int nguoi1 =
                LegalPerson(maDN, cccd1);

            int nguoi2 =
                LegalPerson(maDN, cccd2);

            // Thủ tục SQL phải kiểm tra lại tính hợp lệ
            // trong transaction để tránh dữ liệu bị thay đổi
            // giữa lúc kiểm tra và lúc lưu tài khoản.

            using (SqlConnection cn =
                new SqlConnection(Conn))
            using (SqlCommand cmd =
                new SqlCommand(
                    "dbo.DN_MoTaiKhoan", cn))
            {
                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddRange(new[]
                {
                    P("@SoTK", stk),
                    P("@MaDN", maDN),
                    P("@LoaiTK", loai),
                    P("@Nguoi1", nguoi1),
                    P("@Nguoi2", nguoi2),
                    P("@SoTien", tien),
                    P("@LaiSuat", lai),
                    P("@KyHan", kyHan),
                    P("@NguoiThucHien", NhanVien())
                });

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // =========================================
        // 11. XEM NGƯỜI CHỊU TRÁCH NHIỆM PHÁP LÝ
        // =========================================

        public static void ShowLegal(
            Form owner,
            string stk)
        {
            DataTable dt = Query(@"
SELECT
    n.CCCD,
    n.HoTen AS [Họ tên],
    n.ChucDanh AS [Chức danh],
    n.SoDienThoai AS [Điện thoại],
    r.VaiTro AS [Vai trò]
FROM dbo.DN_TrachNhiemTaiKhoan r
JOIN dbo.DN_NguoiPhapLy n
    ON n.MaNguoi = r.MaNguoi
WHERE r.SoTK = @stk",
                P("@stk", stk));

            using (Form frm = new Form())
            {
                frm.Text =
                    "Người chịu trách nhiệm - " + stk;

                frm.Size = new Size(950, 450);

                frm.StartPosition =
                    FormStartPosition.CenterParent;

                frm.BackColor =
                    Color.FromArgb(15, 23, 42);

                DataGridView dgv =
                    new DataGridView
                    {
                        Dock = DockStyle.Fill
                    };

                Style(dgv);
                dgv.DataSource = dt;

                frm.Controls.Add(dgv);
                frm.ShowDialog(owner);
            }
        }

        // =========================================
        // 12. THÔNG BÁO LỖI
        // =========================================

        public static void Error(Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Quản lý doanh nghiệp",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        // =========================================
        // 13. XÁC NHẬN THAO TÁC
        // =========================================

        public static bool Confirm(string message)
        {
            return MessageBox.Show(
                message,
                "Xác nhận thao tác",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) == DialogResult.Yes;
        }


        // === BỔ SUNG NGHIỆP VỤ DN (đồng bộ từ DN_NghiepVu, không sửa hàm sẵn có) ===
        private static string[] InputDialog(IWin32Window owner, string title, string[] labels, string[] defaults = null)
        {
            using (var dlg = new Form())
            {
                dlg.Text = title; dlg.ClientSize = new Size(610, 93 + labels.Length * 46);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog; dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false; dlg.BackColor = Color.FromArgb(15, 23, 42);
                dlg.ForeColor = Color.White; dlg.Font = new Font("Segoe UI", 10);
                TextBox[] texts = new TextBox[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    dlg.Controls.Add(new Label { Text = labels[i], Location = new Point(15, 18 + i * 46), Width = 210, Height = 29 });
                    texts[i] = new TextBox
                    {
                        Location = new Point(226, 15 + i * 46),
                        Width = 365,
                        MaxLength = 200,
                        Text = defaults != null && i < defaults.Length ? defaults[i] ?? "" : ""
                    };
                    dlg.Controls.Add(texts[i]);
                }
                var ok = new Button { Text = "Xác nhận", Location = new Point(320, 30 + labels.Length * 46), Size = new Size(125, 34), DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Hủy", Location = new Point(458, 30 + labels.Length * 46), Size = new Size(125, 34), DialogResult = DialogResult.Cancel };
                dlg.Controls.Add(ok); dlg.Controls.Add(cancel); dlg.AcceptButton = ok; dlg.CancelButton = cancel;
                if (dlg.ShowDialog(owner) != DialogResult.OK) return null;
                return texts.Select(x => x.Text.Trim()).ToArray();
            }
        }
        public static void OpenAccount(IWin32Window owner, string accountType, string prefillMst)
        {
            string[] a = InputDialog(owner, "Mở tài khoản " + accountType + " - Doanh nghiệp",
                new[]{"Mã số thuế công ty","Số tài khoản mới","CCCD người pháp lý 1","CCCD người pháp lý 2",
                      "Số tiền gốc / số dư ban đầu","Lãi suất (%/năm)","Kỳ hạn (tháng)"},
                new[] { prefillMst, "", "", "", "0", "0", "0" });
            if (a == null) return;
            if (string.IsNullOrWhiteSpace(a[0]) || string.IsNullOrWhiteSpace(a[1]) || a[1].Length > 20)
                throw new InvalidOperationException("Mã số thuế và số tài khoản bắt buộc, STK tối đa 20 ký tự.");
            if (a[2].Length != 12 || a[3].Length != 12 || !a[2].All(char.IsDigit) || !a[3].All(char.IsDigit) || a[2] == a[3])
                throw new InvalidOperationException("Cần hai CCCD 12 số khác nhau của người pháp lý đang hiệu lực.");
            decimal soTien = Money(a[4], false), lai = Money(a[5], false);
            if (soTien > 9999999999999999m || lai > 99999m) throw new InvalidOperationException("Giá trị vượt giới hạn cột SQL.");
            int months;
            if (!int.TryParse(a[6], out months) || months < 0 || months > 600)
                throw new InvalidOperationException("Kỳ hạn phải từ 0 đến 600 tháng.");
            if (accountType == "Thanh toán" && (lai != 0 || months != 0))
                throw new InvalidOperationException("Tài khoản thanh toán không áp dụng kỳ hạn hoặc lãi suất tại form này.");
            if (accountType != "Thanh toán" && (soTien <= 0 || months <= 0))
                throw new InvalidOperationException("Vay/Tiết kiệm yêu cầu số tiền gốc và kỳ hạn lớn hơn 0.");
            DataTable d = Query("SELECT MaDN FROM dbo.DN_DoanhNghiep WHERE MaSoThue=@mst AND TrangThai=N'Hoạt động'", P("@mst", a[0]));
            if (d.Rows.Count != 1) throw new InvalidOperationException("Mã số thuế không thuộc doanh nghiệp đang hoạt động.");
            int maDN = Convert.ToInt32(d.Rows[0][0]);
            DataTable people = Query("SELECT MaNguoi,CCCD FROM dbo.DN_NguoiPhapLy WHERE MaDN=@dn AND DangHieuLuc=1 AND CCCD IN (@a,@b)", P("@dn", maDN), P("@a", a[2]), P("@b", a[3]));
            if (people.Rows.Count != 2) throw new InvalidOperationException("Hai người pháp lý phải cùng thuộc công ty và đang hiệu lực.");
            int p1 = 0, p2 = 0;
            foreach (DataRow row in people.Rows) { if (Convert.ToString(row["CCCD"]) == a[2]) p1 = Convert.ToInt32(row["MaNguoi"]); else p2 = Convert.ToInt32(row["MaNguoi"]); }
            string operatorName = NhanVien();
            if (!Confirm("Xác nhận mở tài khoản " + a[1] + " cho doanh nghiệp " + a[0] + "?")) return;
            using (var cn = new SqlConnection(Conn)) using (var cmd = new SqlCommand("dbo.DN_MoTaiKhoan", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(new[]{P("@SoTK",a[1]),P("@MaDN",maDN),P("@LoaiTK",accountType),P("@Nguoi1",p1),P("@Nguoi2",p2),
                    P("@SoTien",soTien),P("@LaiSuat",lai),P("@KyHan",months),P("@NguoiThucHien",operatorName)});
                cn.Open(); cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Đã mở tài khoản doanh nghiệp " + a[1] + ".", "Thành công");
        }
		public static void OpenHistory(IWin32Window owner, string mst)
		{
			using (var f = new FrmLichSuDN())
			{
				f.ShowDialog(owner);
			}
		}
		public static int CountActiveLegal(SqlConnection cn, SqlTransaction tr, string stk)
        {
            using (var c = new SqlCommand(@"SELECT COUNT(*) FROM dbo.DN_TrachNhiemTaiKhoan r
                JOIN dbo.DN_NguoiPhapLy p ON p.MaNguoi=r.MaNguoi AND p.MaDN=r.MaDN
                WHERE r.SoTK=@stk AND p.DangHieuLuc=1", cn, tr))
            { c.Parameters.Add(P("@stk", stk)); return Convert.ToInt32(c.ExecuteScalar()); }
        }
        public static void ChangeStatus(string stk, string loai, string status, string note)
        {
            string employee = NhanVien();
            using (var cn = new SqlConnection(Conn))
            {
                cn.Open(); using (var tr = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"SELECT TrangThai FROM dbo.DN_TaiKhoan WITH(UPDLOCK,HOLDLOCK)
                            WHERE SoTK=@stk AND LoaiTK=@loai", cn, tr))
                        {
                            cmd.Parameters.AddRange(new[] { P("@stk", stk), P("@loai", loai) });
                            object old = cmd.ExecuteScalar();
                            if (old == null) throw new InvalidOperationException("Không tìm thấy tài khoản doanh nghiệp.");
                            string prev = Convert.ToString(old);
                            if (prev == "Đã đóng") throw new InvalidOperationException("Tài khoản đã đóng không thể hoạt động trở lại.");
                            if (prev == status) throw new InvalidOperationException("Trạng thái tài khoản không thay đổi.");
                            if (status == "Đã đóng")
                            {
                                using (var amountCheck = new SqlCommand(@"SELECT CASE WHEN LoaiTK=N'Vay' THEN SoTienGoc-SoDu ELSE SoDu END
                                    FROM dbo.DN_TaiKhoan WHERE SoTK=@stk", cn, tr))
                                {
                                    amountCheck.Parameters.Add(P("@stk", stk));
                                    decimal remaining = Convert.ToDecimal(amountCheck.ExecuteScalar());
                                    if (remaining != 0) throw new InvalidOperationException("Không thể đóng tài khoản khi vẫn còn số dư hoặc dư nợ gốc.");
                                }
                            }
                            if (status == "Hoạt động" && CountActiveLegal(cn, tr, stk) < 2)
                                throw new InvalidOperationException("Cần ít nhất 2 người pháp lý hợp lệ để mở khóa.");
                            using (var up = new SqlCommand("UPDATE dbo.DN_TaiKhoan SET TrangThai=@s WHERE SoTK=@stk", cn, tr))
                            { up.Parameters.AddRange(new[] { P("@s", status), P("@stk", stk) }); up.ExecuteNonQuery(); }
                        }
                        using (var log = new SqlCommand(@"INSERT INTO dbo.DN_LichSu(SoTK,LoaiGiaoDich,SoTien,SoDuSau,NguoiThucHien,GhiChu)
                            SELECT SoTK,@act,0,SoDu,@emp,@note FROM dbo.DN_TaiKhoan WHERE SoTK=@stk", cn, tr))
                        { log.Parameters.AddRange(new[] { P("@stk", stk), P("@act", status), P("@emp", employee), P("@note", note) }); log.ExecuteNonQuery(); }
                        tr.Commit();
                    }
                    catch { tr.Rollback(); throw; }
                }
            }
        }
        public static void ChangeBalance(string stk, string loai, string action, decimal amount, bool increase, string note)
        {
            string employee = NhanVien();
            using (var cn = new SqlConnection(Conn))
            {
                cn.Open(); using (var tr = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (var cmd = new SqlCommand(@"UPDATE dbo.DN_TaiKhoan WITH(UPDLOCK)
                            SET SoDu=SoDu+@delta
                            WHERE SoTK=@stk AND LoaiTK=@loai AND TrangThai=N'Hoạt động'
                              AND SoDu+@delta>=0 AND SoDu+@delta<=SoTienGoc
                              AND (SELECT COUNT(*) FROM dbo.DN_TrachNhiemTaiKhoan r
                                JOIN dbo.DN_NguoiPhapLy p ON p.MaNguoi=r.MaNguoi AND p.MaDN=r.MaDN
                                WHERE r.SoTK=@stk AND p.DangHieuLuc=1)>=2", cn, tr))
                        {
                            // Vay: SoDu là gốc đã trả; tiết kiệm: SoDu là gốc còn lại.
                            cmd.Parameters.AddRange(new[] { P("@delta", increase ? amount : -amount), P("@stk", stk), P("@loai", loai) });
                            if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Tài khoản bị khóa, thiếu hai người pháp lý hoặc số tiền vượt số dư/gốc hợp lệ.");
                        }
                        using (var log = new SqlCommand(@"INSERT INTO dbo.DN_LichSu(SoTK,LoaiGiaoDich,SoTien,SoDuSau,NguoiThucHien,GhiChu)
                            SELECT SoTK,@act,@money,SoDu,@emp,@note FROM dbo.DN_TaiKhoan WHERE SoTK=@stk", cn, tr))
                        { log.Parameters.AddRange(new[] { P("@stk", stk), P("@act", action), P("@money", amount), P("@emp", employee), P("@note", note) }); log.ExecuteNonQuery(); }
                        tr.Commit();
                    }
                    catch { tr.Rollback(); throw; }
                }
            }
        }
    }
}
