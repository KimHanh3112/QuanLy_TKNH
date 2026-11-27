using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace QLTKNH
{
    public partial class FrmThanhToan : Form
    {

        // Phan cap chi o lop hien thi: khong sua Value, Tag hay quyen thao tac.
        private static void GanPhanCapCaNhan(DataGridView grid, int cotTen, int cotVaiTro, Font fontDam)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.CellFormatting += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count ||
                    (e.ColumnIndex != cotTen && e.ColumnIndex != cotVaiTro) ||
                    grid.Columns.Count <= cotVaiTro || grid.Rows[e.RowIndex].IsNewRow) return;
                string vaiTro = Convert.ToString(grid.Rows[e.RowIndex].Cells[cotVaiTro].Value).Trim();
                bool dong = vaiTro.StartsWith("Đồng sở hữu", StringComparison.OrdinalIgnoreCase);
                bool chu = vaiTro.StartsWith("Chủ", StringComparison.OrdinalIgnoreCase);
                if (!dong && !chu) return;
                e.CellStyle.ForeColor = dong ? Color.FromArgb(192, 132, 252) : Color.FromArgb(96, 165, 250);
                e.CellStyle.SelectionForeColor = dong ? Color.FromArgb(233, 213, 255) : Color.FromArgb(219, 234, 254);
                if (chu) e.CellStyle.Font = fontDam;
                if (dong && e.ColumnIndex == cotTen)
                {
                    e.Value = "  ↳ " + Convert.ToString(e.Value);
                    e.FormattingApplied = true;
                }
            };
        }

        private static void CanDeuBangSoHuuCaNhan(DataGridView grid)
        {
            if (grid.Columns.Count != 5) return;
            float[] weights = { 22F, 35F, 17F, 14F, 12F };
            int[] minWidths = { 115, 180, 105, 100, 90 };
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            for (int i = 0; i < 5; i++)
            {
                grid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.NotSet;
                grid.Columns[i].MinimumWidth = minWidths[i];
                grid.Columns[i].FillWeight = weights[i];
                grid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private readonly string _connStr =
            @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        private readonly Color colorDarkBg = Color.FromArgb(15, 23, 42);
        private readonly Color colorCardBg = Color.FromArgb(30, 41, 59);
        private readonly Color colorBlue = Color.FromArgb(37, 99, 235);
        private readonly Color colorGreen = Color.FromArgb(34, 197, 94);
        private readonly Color colorRed = Color.FromArgb(239, 68, 68);
        private readonly Color colorOrange = Color.FromArgb(249, 115, 22);

        private string cccdMoTuFrmMain;
        public event EventHandler DataChanged;
        private void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

        public FrmThanhToan() : this(null, "") { }
        public FrmThanhToan(FrmMain frmMain, string cccd) : this(cccd)
        {
            if (frmMain != null) this.Owner = frmMain;
        }

        public FrmThanhToan(string cccd)
        {
            cccdMoTuFrmMain = cccd ?? "";
            InitializeComponent();
            GanPhanCapCaNhan(dgvThanhToan, 3, 8, _fontVaiTro);
            BatVeDem(dgvThanhToan);
            dgvThanhToan.Disposed += (s, e) => _fontVaiTro.Dispose();

            this.Load += FrmThanhToan_Load;
            this.Activated += (s, e) => {
                if ((DateTime.UtcNow - _lanTaiDanhSach).TotalSeconds >= 2)
                    HenTaiDanhSachThanhToan();
            };
            _timerTaiDanhSach.Tick += (s, e) => { _timerTaiDanhSach.Stop(); HienThiDanhSach(txtCCCD.Text.Trim(), true); };
            Disposed += (s, e) => _timerTaiDanhSach.Dispose();

            txtCCCD.TextChanged += TxtCCCD_TextChanged;
            txtCCCD.KeyPress += TxtCCCD_KeyPress;
            dgvThanhToan.CellClick += DgvThanhToan_CellClick;
            dgvThanhToan.CellDoubleClick += DgvThanhToan_CellDoubleClick;
            dgvThanhToan.MouseDown += DgvThanhToan_MouseDown;
            btnTimCCCD.Click += BtnTimCCCD_Click;

            panelSearchEvents();
            DangKyDoubleClickVungTrong(this);
            dgvThanhToan.MouseDoubleClick += DgvThanhToan_MouseDoubleClick;
            // Nhấn một lần vào khoảng trắng cũng xóa tìm kiếm và khôi phục danh sách ban đầu.
            DangKyClickVungTrong(this);
            dgvThanhToan.MouseClick += DgvThanhToan_MouseClick;

            cboLocTrangThai.SelectedIndexChanged += (s, e) => HenTaiDanhSachThanhToan();
            cboLocThoiGian.SelectedIndexChanged += CboLocThoiGian_SelectedIndexChanged;
            dtpTuNgay.ValueChanged += (s, e) => { if (cboLocThoiGian.SelectedIndex == 7) HenTaiDanhSachThanhToan(); };
            dtpDenNgay.ValueChanged += (s, e) => { if (cboLocThoiGian.SelectedIndex == 7) HenTaiDanhSachThanhToan(); };

            SetupContextMenuEvents();
        }

        #region SQL Helpers

        // Quan he dong so huu moi theo SO TAI KHOAN thay vi theo CCCD cua chu.
        // Chi chuyen lien ket lich su trong lan dau tao bang, khong sua du lieu cu.
        private bool _daKhoiTaoLienKetDongSoHuu;
        private bool DamBaoLienKetDongSoHuu()
        {
            if (_daKhoiTaoLienKetDongSoHuu) return true;
            try
            {
                using (SqlConnection cn = GetConn())
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
                        SET XACT_ABORT ON;
                        BEGIN TRANSACTION;
                        IF OBJECT_ID(N'dbo.DongSoHuuTaiKhoanThanhToan', N'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.DongSoHuuTaiKhoanThanhToan
                            (
                                SoTKTT VARCHAR(20) NOT NULL,
                                CCCD VARCHAR(12) NOT NULL,
                                HoTen NVARCHAR(100) NOT NULL,
                                SoDienThoai VARCHAR(11) NOT NULL,
                                CONSTRAINT PK_DongSoHuuTaiKhoanThanhToan PRIMARY KEY (SoTKTT, CCCD),
                                CONSTRAINT FK_DongSoHuuTKTT_TaiKhoan FOREIGN KEY (SoTKTT)
                                    REFERENCES dbo.TaiKhoanThanhToan(SoTKTT)
                            );
                            -- Giu lai cach hien thi dong so huu cua tai khoan da ton tai.
                            EXEC sys.sp_executesql N'INSERT INTO dbo.DongSoHuuTaiKhoanThanhToan
                                (SoTKTT, CCCD, HoTen, SoDienThoai)
                            SELECT tk.SoTKTT, d.CCCD, MAX(d.HoTen), MAX(d.SoDienThoai)
                            FROM dbo.TaiKhoanThanhToan tk
                            INNER JOIN dbo.DongSoHuu d ON d.CCCD_ChuSoHuu = tk.CCCD
                            WHERE d.CCCD <> tk.CCCD
                            GROUP BY tk.SoTKTT, d.CCCD';
                        END;
                        COMMIT TRANSACTION;", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                _daKhoiTaoLienKetDongSoHuu = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Khong the khoi tao lien ket dong so huu theo so tai khoan:\n" + ex.Message,
                    "Loi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Kiem tra trung so tai khoan tren ca ba loai tai khoan.
        private bool SoTaiKhoanDaTonTai(string stk)
        {
            using (SqlConnection cn = GetConn())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk)
                    OR EXISTS (SELECT 1 FROM dbo.TaiKhoanVay WHERE SoTKVay=@stk)
                    OR EXISTS (SELECT 1 FROM dbo.SoTietKiem WHERE SoTKTK=@stk)
                    THEN 1 ELSE 0 END", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        private SqlConnection GetConn() => new SqlConnection(_connStr);

        private DataTable ExecuteQuery(string sql, params SqlParameter[] pars)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = GetConn())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null) cmd.Parameters.AddRange(pars);
                    conn.Open();
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi truy vấn:\n" + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        private int ExecuteNonQuery(string sql, params SqlParameter[] pars)
        {
            try
            {
                using (var conn = GetConn())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null) cmd.Parameters.AddRange(pars);
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thực thi:\n" + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        private object ExecuteScalar(string sql, params SqlParameter[] pars)
        {
            try
            {
                using (var conn = GetConn())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null) cmd.Parameters.AddRange(pars);
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy dữ liệu:\n" + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        // Chỉ dùng khi ghi giao dịch mới: lưu đúng họ tên người đang đăng nhập tại thời điểm thao tác.
        private static string LayTenNguoiThucHien()
        {
            var nhanVien = BankSecurity.Current;
            if (nhanVien == null || string.IsNullOrWhiteSpace(nhanVien.HoTen))
                throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập. Vui lòng đăng nhập lại trước khi thực hiện giao dịch.");
            return nhanVien.HoTen.Trim();
        }

        private void GhiLichSu(string stk, string noiDung, decimal? soTien = null, decimal? soDuSau = null)
        {
            if (string.IsNullOrWhiteSpace(stk)) return;
            ExecuteNonQuery(@"INSERT INTO LichSuGiaoDich (SoTKTT, NoiDung, SoTien, SoDuSau, ThoiGian, NguoiThucHien)
                              VALUES (@stk, @nd, @st, @sd, GETDATE(), @nguoi)",
                new SqlParameter("@stk", stk),
                new SqlParameter("@nd", noiDung ?? ""),
                new SqlParameter("@st", (object)soTien ?? DBNull.Value),
                new SqlParameter("@sd", (object)soDuSau ?? DBNull.Value),
                new SqlParameter("@nguoi", SqlDbType.NVarChar, 100) { Value = LayTenNguoiThucHien() });
        }

        private string MapTrangThai(int tt) =>
     tt == 0 ? "Hoạt động" :
     tt == 1 ? "Đang khóa" :
     tt == 2 ? "Đã đóng" : "Không rõ";
        #endregion

        #region UI Init

        // Nhấp một hoặc hai lần vào nền trống để khôi phục bảng ban đầu.
        // Không ảnh hưởng thao tác trên nút, ô nhập hay dòng tài khoản.
        private void panelSearchEvents()
        {
            // Các vùng nền được đăng ký bởi DangKyDoubleClickVungTrong.
        }

        private bool _dangKhoiPhucDanhSach;
        private bool _dangKhoiTaoBoLoc;
        private readonly Timer _timerTaiDanhSach = new Timer { Interval = 300 };
        private DateTime _lanTaiDanhSach = DateTime.MinValue;
        private long _phienTaiDanhSach;
        private void HenTaiDanhSachThanhToan()
        {
            if (_dangKhoiPhucDanhSach || _dangKhoiTaoBoLoc || IsDisposed || Disposing) return;
            ++_phienTaiDanhSach;
            _timerTaiDanhSach.Stop();
            _timerTaiDanhSach.Start();
        }
        private DataTable DocDanhSachThanhToan(string sql, SqlParameter[] pars)
        {
            using (var cn = GetConn())
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddRange(pars);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
        private readonly Font _fontVaiTro = new Font("Segoe UI", 9F, FontStyle.Bold);
        private static void BatVeDem(DataGridView grid)
        {
            typeof(DataGridView).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(grid, true, null);
        }

        // Khôi phục toàn bộ bộ lọc về như lúc mới mở form, kể cả khi ô CCCD trống.
        private void XoaTimKiemCCCD()
        {
            if (_dangKhoiPhucDanhSach || txtCCCD.IsDisposed) return;
            _dangKhoiPhucDanhSach = true;
            try
            {
                // Bỏ lọc CCCD, trạng thái và thời gian để bảng hiện lại tất cả tài khoản.
                txtCCCD.Clear();
                if (cboLocTrangThai.Items.Count > 0) cboLocTrangThai.SelectedIndex = 0;
                if (cboLocThoiGian.Items.Count > 0) cboLocThoiGian.SelectedIndex = 0;

                // Cập nhật vùng chọn ngày khi thoát chế độ lọc tùy chọn.
                lblTuNgay.Visible = false;
                dtpTuNgay.Visible = false;
                lblDenNgay.Visible = false;
                dtpDenNgay.Visible = false;

                // Lấy lại tất cả tài khoản từ SQL và cập nhật bảng cùng thống kê.
            }
            finally
            {
                _dangKhoiPhucDanhSach = false;
            }
            HienThiDanhSach("");
        }

        // Chỉ đăng ký sự kiện trên các vùng nền, không can thiệp nút, ô nhập
        // hay các hàng tài khoản có thao tác chọn / mở chi tiết.
        private void DangKyClickVungTrong(Control cha)
        {
            if (cha is Form || cha is Panel || cha is Label || cha is GroupBox || cha is UserControl)
                cha.MouseClick += VungTrong_MouseClick;

            foreach (Control con in cha.Controls)
            {
                if (con is DataGridView) continue;
                DangKyClickVungTrong(con);
            }
        }

        private void VungTrong_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                XoaTimKiemCCCD();
        }

        private void DgvThanhToan_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            DataGridView.HitTestInfo hit = dgvThanhToan.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.None)
                XoaTimKiemCCCD();
        }

        private void DangKyDoubleClickVungTrong(Control cha)
        {
            // Chỉ nhận nhấp đúp trên nền form, panel và nhãn, không gắn vào
            // textbox, combobox, button, bảng hay các vùng có tương tác khác.
            if (cha is Form || cha is Panel || cha is Label || cha is GroupBox || cha is UserControl)
                cha.MouseDoubleClick += VungTrong_MouseDoubleClick;

            foreach (Control con in cha.Controls)
            {
                if (con is DataGridView) continue; // Xử lý riêng vùng trống của bảng.
                DangKyDoubleClickVungTrong(con);
            }
        }

        private void VungTrong_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                XoaTimKiemCCCD();
        }

        private void DgvThanhToan_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            DataGridView.HitTestInfo hit = dgvThanhToan.HitTest(e.X, e.Y);
            // Chỉ xóa ở khoảng trống của bảng, không xóa khi nhấp đúp vào dòng
            // (vì nhấp đúp vào dòng được dùng để mở chi tiết tài khoản).
            if (hit.Type == DataGridViewHitTestType.None)
                XoaTimKiemCCCD();
        }

        private void FrmThanhToan_Load(object sender, EventArgs e)
        {
            _dangKhoiTaoBoLoc = true;
            try
            {
                cboLocTrangThai.SelectedIndex = 0;
                cboLocThoiGian.SelectedIndex = 0;
                txtCCCD.Focus();
                if (!string.IsNullOrWhiteSpace(cccdMoTuFrmMain))
                    txtCCCD.Text = cccdMoTuFrmMain;
            }
            finally { _dangKhoiTaoBoLoc = false; }
            HenTaiDanhSachThanhToan();
        }

        private void CboLocThoiGian_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool laTuyChon = cboLocThoiGian.SelectedIndex == 7;
            lblTuNgay.Visible = laTuyChon;
            dtpTuNgay.Visible = laTuyChon;
            lblDenNgay.Visible = laTuyChon;
            dtpDenNgay.Visible = laTuyChon;
            HenTaiDanhSachThanhToan();
        }

        private void TxtCCCD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void TxtCCCD_TextChanged(object sender, EventArgs e)
        {
            string clean = new string((txtCCCD.Text ?? "").Where(char.IsDigit).ToArray());
            if (clean.Length > 12) clean = clean.Substring(0, 12);
            if (txtCCCD.Text != clean)
            {
                int cursor = txtCCCD.SelectionStart;
                txtCCCD.Text = clean;
                txtCCCD.SelectionStart = Math.Min(cursor, clean.Length);
                return; // TextChanged lồng đã xử lý giá trị sạch.
            }
            HenTaiDanhSachThanhToan();
        }

        private void BtnTimCCCD_Click(object sender, EventArgs e)
        {
            string cccd = txtCCCD.Text.Trim();
            _timerTaiDanhSach.Stop();
            if (string.IsNullOrWhiteSpace(cccd))
            {
                MessageBox.Show("Vui lòng nhập CCCD cần tìm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!DamBaoLienKetDongSoHuu()) return;
            HienThiDanhSach(cccd);
            // Chỉ hỏi đăng ký khi người dùng tìm CCCD đầy đủ, không tự mở khi đang gõ.
            if (cccd.Length != 12 || !cccd.All(c => c >= '0' && c <= '9')) return;
            try
            {
                var kq = FrmMain.TraCuuTimMoMoiCaNhan(_connStr, cccd, "Thanh toán");
                if (kq.CoTaiKhoan) return; // Có tài khoản nhưng có thể đang bị bộ lọc ẩn.
                string noiDung = kq.CoDanhTinh
                    ? "Khách hàng chưa có tài khoản loại này. Bạn có muốn mở mới?"
                    : "CCCD chưa tồn tại. Bạn có muốn đăng ký khách hàng và mở tài khoản mới?";
                if (MessageBox.Show(this, noiDung, "Không tìm thấy tài khoản",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    MoTaiKhoanThanhToanCaNhan(cccd, kq);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kiểm tra hoặc mở tài khoản: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Hiển thị danh sách + Double-click

        private async void HienThiDanhSach(string filterCCCD, bool batDongBo = false)
        {
            if (_dangKhoiPhucDanhSach || _dangKhoiTaoBoLoc || IsDisposed || Disposing) return;
            _timerTaiDanhSach.Stop();
            long phien = ++_phienTaiDanhSach;
            if (!DamBaoLienKetDongSoHuu()) return;

            string sql = @"
SELECT 
    tk.SoTKTT, tk.SoDu, tk.TrangThai, tk.NgayMo, tk.NgayCapNhatTT,
    kh.CCCD AS CCCDChu,
    kh.HoTen AS HoTenChu,
    kh.SoDienThoai AS SDTChu,
    dsh.CCCD AS CCCDDSH,
    COALESCE(khd.HoTen, dsh.HoTen) AS HoTenDSH,
    COALESCE(khd.SoDienThoai, dsh.SoDienThoai) AS SDTDSH
FROM TaiKhoanThanhToan tk
INNER JOIN KhachHang kh ON tk.CCCD = kh.CCCD
LEFT JOIN dbo.DongSoHuuTaiKhoanThanhToan dsh ON dsh.SoTKTT = tk.SoTKTT
LEFT JOIN dbo.KhachHang khd ON khd.CCCD = dsh.CCCD
WHERE 1=1 ";

            var pars = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(filterCCCD))
            {
                sql += " AND (kh.CCCD LIKE @cccd OR dsh.CCCD LIKE @cccd) ";
                pars.Add(new SqlParameter("@cccd", "%" + filterCCCD + "%"));
            }

            if (cboLocTrangThai.SelectedIndex > 0)
            {
                sql += " AND tk.TrangThai = @tt ";
                pars.Add(new SqlParameter("@tt", cboLocTrangThai.SelectedIndex - 1));
            }

            if (cboLocThoiGian.SelectedIndex > 0)
            {
                DateTime today = DateTime.Today;
                DateTime from = today, to = today;
                switch (cboLocThoiGian.SelectedIndex)
                {
                    case 1: from = to = today; break;
                    case 2: from = to = today.AddDays(-1); break;
                    case 3: from = today.AddDays(-7); break;
                    case 4: from = today.AddDays(-30); break;
                    case 5: from = today.AddMonths(-3); break;
                    case 6: from = today.AddYears(-1); break;
                    case 7: from = dtpTuNgay.Value.Date; to = dtpDenNgay.Value.Date; break;
                }
                sql += " AND tk.NgayMo >= @from AND tk.NgayMo < DATEADD(day, 1, @to) ";
                pars.Add(new SqlParameter("@from", from));
                pars.Add(new SqlParameter("@to", to));
            }

            sql += " ORDER BY tk.NgayMo DESC, tk.SoTKTT";

            DataTable dt;
            try
            {
                var parameters = pars.ToArray();
                dt = batDongBo ? await System.Threading.Tasks.Task.Run(() => DocDanhSachThanhToan(sql, parameters)) : DocDanhSachThanhToan(sql, parameters);
                if (IsDisposed || Disposing || phien != _phienTaiDanhSach) return;
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing && phien == _phienTaiDanhSach)
                    MessageBox.Show("Lỗi tải danh sách: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _lanTaiDanhSach = DateTime.UtcNow;
            dgvThanhToan.SuspendLayout();
            try
            {
                dgvThanhToan.Rows.Clear();
                int stt = 1;
                int tongTK = 0;
                var seen = new HashSet<string>();

                foreach (DataRow r in dt.Rows)
                {
                    string stk = r["SoTKTT"].ToString();
                    string trangThai = MapTrangThai(Convert.ToInt32(r["TrangThai"]));
                    string ngayMo = r["NgayMo"] == DBNull.Value ? "-" : Convert.ToDateTime(r["NgayMo"]).ToString("dd/MM/yyyy HH:mm");
                    string ngayCN = r["NgayCapNhatTT"] == DBNull.Value ? "-" : Convert.ToDateTime(r["NgayCapNhatTT"]).ToString("dd/MM/yyyy HH:mm");
                    string soDuStr = $"{Convert.ToDecimal(r["SoDu"]):N0} VNĐ";

                    if (!seen.Contains(stk))
                    {
                        seen.Add(stk);
                        tongTK++;
                        AddRow(stt++, stk, r["CCCDChu"].ToString(), r["HoTenChu"].ToString(), r["SDTChu"].ToString(),
                            soDuStr, ngayMo, ngayCN, "Chủ", trangThai);
                    }

                    if (r["CCCDDSH"] != DBNull.Value && !string.IsNullOrEmpty(r["CCCDDSH"].ToString()))
                    {
                        AddRow(stt++, stk, r["CCCDDSH"].ToString(), r["HoTenDSH"].ToString(), r["SDTDSH"].ToString(),
                            soDuStr, ngayMo, ngayCN, "Đồng sở hữu", trangThai);
                    }
                }

                lblTongTaiKhoan.Text = $"Tổng tài khoản: {tongTK:N0}";

                if (dgvThanhToan.Rows.Count > 0)
                {
                    dgvThanhToan.ClearSelection();
                    dgvThanhToan.Rows[0].Selected = true;
                    HienThiKhachHangDangChon(0);
                }
                else
                    lblDaChonInfo.Text = "Chưa chọn | CCCD: - | SĐT: - | Số dư: 0 VNĐ";
            }
            finally { dgvThanhToan.ResumeLayout(); }
        }

        private void AddRow(int stt, string stk, string cccd, string hoTen, string sdt,
            string soDu, string ngayMo, string ngayCN, string vaiTro, string trangThai)
        {
            int idx = dgvThanhToan.Rows.Add(stt, stk, cccd, hoTen, sdt, soDu, ngayMo, ngayCN, vaiTro, trangThai);
            var row = dgvThanhToan.Rows[idx];

            if (vaiTro == "Chủ")
            {
                row.Cells["colVaiTro"].Style.ForeColor = Color.FromArgb(96, 165, 250);
                row.Cells["colVaiTro"].Style.Font = _fontVaiTro;
            }
            else
            {
                row.Cells["colVaiTro"].Style.ForeColor = Color.FromArgb(192, 132, 252);
                row.Cells["colVaiTro"].Style.Font = _fontVaiTro;
            }

            ToMauTrangThaiThanhToan(row.Cells["colTrangThai"], trangThai);
        }

        // Chỉ đổi màu hiển thị, không thay đổi trạng thái hoặc nghiệp vụ SQL.
        private Color MauTrangThaiThanhToan(string trangThai)
        {
            return trangThai == "Hoạt động" ? colorGreen :
                   trangThai == "Đang khóa" ? colorOrange : colorRed;
        }

        private void ToMauTrangThaiThanhToan(DataGridViewCell cell, string trangThai)
        {
            Color mau = MauTrangThaiThanhToan(trangThai);
            cell.Style.ForeColor = mau;
            cell.Style.SelectionForeColor = mau;
        }

        private void DgvThanhToan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) HienThiKhachHangDangChon(e.RowIndex);
        }

        private void DgvThanhToan_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string stk = dgvThanhToan.Rows[e.RowIndex].Cells["colSTK"].Value?.ToString();
            if (!string.IsNullOrEmpty(stk))
            {
                dgvThanhToan.CurrentCell = dgvThanhToan.Rows[e.RowIndex].Cells[0];
                HienThiChiTietVaChucNangThanhToan(stk);
            }
        }

        private void HienThiChiTietTaiKhoan(string stk)
        {
            DataTable dtTK = ExecuteQuery(@"
                SELECT tk.SoTKTT, tk.SoDu, tk.TrangThai, tk.NgayMo, tk.LoaiTienTe, tk.LoaiTaiKhoan,
                       kh.CCCD, kh.HoTen, kh.SoDienThoai
                FROM TaiKhoanThanhToan tk
                INNER JOIN KhachHang kh ON tk.CCCD = kh.CCCD
                WHERE tk.SoTKTT = @stk", new SqlParameter("@stk", stk));

            if (dtTK.Rows.Count == 0) { MessageBox.Show("Không tìm thấy tài khoản!"); return; }

            DataRow tk = dtTK.Rows[0];
            string cccdChu = tk["CCCD"].ToString();
            string trangThai = MapTrangThai(Convert.ToInt32(tk["TrangThai"]));

            if (!DamBaoLienKetDongSoHuu()) return;
            DataTable dtDSH = ExecuteQuery(
                "SELECT CCCD, HoTen, SoDienThoai FROM dbo.DongSoHuuTaiKhoanThanhToan WHERE SoTKTT = @stk",
                new SqlParameter("@stk", stk));

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = $"Chi tiết tài khoản - {stk}";
                dlg.Size = new Size(720, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.BackColor = colorDarkBg;
                dlg.ForeColor = Color.White;

                var lblHeader = new Label
                {
                    Text = $"THÔNG TIN TÀI KHOẢN: {stk}",
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    ForeColor = colorBlue,
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                var lblInfo = new Label
                {
                    Text = $"Số dư        : {Convert.ToDecimal(tk["SoDu"]):N0} VNĐ\n" +
                          $"Trạng thái   : {trangThai}\n" +
                          $"Loại tiền tệ : {tk["LoaiTienTe"]}\n" +
                          $"Loại TK      : {tk["LoaiTaiKhoan"]}\n" +
                          $"Ngày mở      : {Convert.ToDateTime(tk["NgayMo"]):dd/MM/yyyy HH:mm:ss}",
                    Font = new Font("Segoe UI", 10F),
                    Location = new Point(20, 55),
                    AutoSize = true
                };

                var lblSoHuu = new Label
                {
                    Text = "DANH SÁCH SỞ HỮU / ĐỒNG SỞ HỮU",
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = colorGreen,
                    Location = new Point(20, 160),
                    AutoSize = true
                };

                var dgv = new DataGridView
                {
                    Location = new Point(20, 195),
                    Size = new Size(660, 240),
                    BackgroundColor = colorCardBg,
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    RowHeadersVisible = false,
                    EnableHeadersVisualStyles = false
                };
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                dgv.DefaultCellStyle.BackColor = colorCardBg;
                dgv.DefaultCellStyle.ForeColor = Color.White;
                dgv.DefaultCellStyle.SelectionBackColor = colorBlue;

                dgv.Columns.Add("colVaiTro", "Vai trò");
                dgv.Columns.Add("colHoTen", "Họ và tên");
                dgv.Columns.Add("colCCCD", "CCCD");
                dgv.Columns.Add("colSDT", "Số điện thoại");
                dgv.Columns.Add("colTT", "Trạng thái TK");

                int idxChu = dgv.Rows.Add("Chủ tài khoản", tk["HoTen"], tk["CCCD"], tk["SoDienThoai"], trangThai);
                dgv.Rows[idxChu].Cells[0].Style.ForeColor = Color.FromArgb(96, 165, 250);
                dgv.Rows[idxChu].Cells[0].Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

                foreach (DataRow d in dtDSH.Rows)
                {
                    int idx = dgv.Rows.Add("Đồng sở hữu", d["HoTen"], d["CCCD"], d["SoDienThoai"], trangThai);
                    dgv.Rows[idx].Cells[0].Style.ForeColor = Color.FromArgb(192, 132, 252);
                    dgv.Rows[idx].Cells[0].Style.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                }

                foreach (DataGridViewRow dong in dgv.Rows)
                    ToMauTrangThaiThanhToan(dong.Cells["colTT"], trangThai);

                var btnDong = new Button
                {
                    Text = "Đóng",
                    Size = new Size(120, 36),
                    Location = new Point(290, 450),
                    BackColor = colorBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnDong.Click += (s, e) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { lblHeader, lblInfo, lblSoHuu, dgv, btnDong });
                dlg.ShowDialog(this);
            }
        }


        // Modern Banking: chỉ thay bố cục hiển thị, giữ nguyên SQL và callback nghiệp vụ.
        private void HienThiChiTietVaChucNangThanhToan(string stk)
        {
            DataTable dt = ExecuteQuery(@"SELECT tk.SoTKTT, tk.SoDu, tk.TrangThai, tk.NgayMo,
                    tk.LoaiTienTe, tk.LoaiTaiKhoan, tk.HanMucNgay, tk.HanMucLan,
                    tk.TenHienThi, kh.CCCD, kh.HoTen, kh.SoDienThoai
                FROM dbo.TaiKhoanThanhToan tk INNER JOIN dbo.KhachHang kh ON kh.CCCD=tk.CCCD
                WHERE tk.SoTKTT=@stk", new SqlParameter("@stk", stk));
            if (dt.Rows.Count == 0) { MessageBox.Show("Không tìm thấy tài khoản!"); return; }
            if (!DamBaoLienKetDongSoHuu()) return;
            DataTable coOwners = ExecuteQuery(@"SELECT CCCD, HoTen, SoDienThoai
                FROM dbo.DongSoHuuTaiKhoanThanhToan WHERE SoTKTT=@stk", new SqlParameter("@stk", stk));
            DataRow tk = dt.Rows[0];
            string status = MapTrangThai(Convert.ToInt32(tk["TrangThai"]));
            bool closed = Convert.ToInt32(tk["TrangThai"]) == 2;
            bool locked = Convert.ToInt32(tk["TrangThai"]) == 1;
            bool canUse = !closed && !locked;

            Color bg = Color.FromArgb(15, 23, 42);
            Color card = Color.FromArgb(25, 36, 57);
            Color border = Color.FromArgb(52, 67, 92);
            Color muted = Color.FromArgb(160, 177, 202);
            Color white = Color.FromArgb(241, 245, 249);
            using (Form dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = "Chi tiết & chức năng tài khoản thanh toán - " + stk;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.MinimumSize = new Size(1120, 690);
                dlg.BackColor = bg;
                dlg.ForeColor = white;
                dlg.Font = new Font("Segoe UI", 10F);

                TableLayoutPanel root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = bg,
                    Padding = new Padding(24, 14, 24, 14),
                    ColumnCount = 1,
                    RowCount = 7,
                    Margin = Padding.Empty
                };
                root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 148));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 195));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
                dlg.Controls.Add(root);

                Panel header = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
                Label title = new Label
                {
                    Text = "TÀI KHOẢN THANH TOÁN",
                    Dock = DockStyle.Top,
                    Height = 34,
                    ForeColor = white,
                    Font = new Font("Segoe UI", 19F, FontStyle.Bold)
                };
                Label subtitle = new Label
                {
                    Text = "Số tài khoản  " + stk + "     •     " + status,
                    Dock = DockStyle.Bottom,
                    Height = 21,
                    ForeColor = MauTrangThaiThanhToan(status),
                    Font = new Font("Segoe UI", 10.5F)
                };
                header.Controls.Add(title);
                header.Controls.Add(subtitle);
                root.Controls.Add(header, 0, 0);

                TableLayoutPanel infoCards = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1,
                    Margin = new Padding(0, 6, 0, 5)
                };
                infoCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
                infoCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
                infoCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
                infoCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.Controls.Add(infoCards, 0, 1);

                Panel balanceCard = TaoTheThongTinThanhToan("SỐ DƯ HIỆN TẠI", card, muted);
                Label balance = new Label
                {
                    Text = Convert.ToDecimal(tk["SoDu"]).ToString("N0") + " VNĐ",
                    Location = new Point(16, 42),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(74, 222, 128)
                };
                Label state = new Label
                {
                    Text = "Trạng thái: " + status + "     |     " + tk["LoaiTienTe"],
                    Location = new Point(18, 99),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = MauTrangThaiThanhToan(status)
                };
                balanceCard.Controls.Add(balance);
                balanceCard.Controls.Add(state);
                infoCards.Controls.Add(balanceCard, 0, 0);

                Panel accountCard = TaoTheThongTinThanhToan("THÔNG TIN TÀI KHOẢN", card, muted);
                Label accountInfo = new Label
                {
                    Text = "Loại TK: " + tk["LoaiTaiKhoan"] + "\n" +
                           "Ngày mở: " + Convert.ToDateTime(tk["NgayMo"]).ToString("dd/MM/yyyy HH:mm:ss"),
                    Location = new Point(17, 45),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 13F),
                    ForeColor = white
                };
                accountCard.Controls.Add(accountInfo);
                infoCards.Controls.Add(accountCard, 1, 0);

                Panel limitsCard = TaoTheThongTinThanhToan("HẠN MỨC GIAO DỊCH", card, muted);
                Label limits = new Label
                {
                    Text = "Theo ngày: " + Convert.ToDecimal(tk["HanMucNgay"]).ToString("N0") + " VNĐ\n" +
                           "Mỗi lần: " + Convert.ToDecimal(tk["HanMucLan"]).ToString("N0") + " VNĐ",
                    Location = new Point(17, 45),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 13F),
                    ForeColor = white
                };
                limitsCard.Controls.Add(limits);
                infoCards.Controls.Add(limitsCard, 2, 0);

                Label ownerHeading = new Label
                {
                    Text = "CHỦ SỞ HỮU VÀ ĐỒNG SỞ HỮU",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = white,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold)
                };
                root.Controls.Add(ownerHeading, 0, 2);

                DataGridView owners = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 0, 0, 2),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = card,
                    BorderStyle = BorderStyle.None,
                    EnableHeadersVisualStyles = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    ScrollBars = ScrollBars.None,
                    Font = new Font("Segoe UI", 12F),
                    GridColor = border
                };
                owners.Columns.Add("role", "Vai trò"); owners.Columns.Add("name", "Họ và tên");
                owners.Columns.Add("cccd", "CCCD"); owners.Columns.Add("phone", "Số điện thoại");
                owners.Columns.Add("status", "Trạng thái TK");
                GanPhanCapCaNhan(owners, 1, 0, _fontVaiTro);
                CanDeuBangSoHuuCaNhan(owners);
                owners.ColumnHeadersHeight = 42;
                owners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                owners.RowTemplate.Height = 40;
                owners.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 56, 80);
                owners.ColumnHeadersDefaultCellStyle.ForeColor = white;
                owners.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                owners.DefaultCellStyle.BackColor = card;
                owners.DefaultCellStyle.ForeColor = white;
                owners.DefaultCellStyle.SelectionBackColor = Color.FromArgb(39, 72, 120);
                owners.DefaultCellStyle.SelectionForeColor = white;
                owners.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(31, 44, 66);
                owners.Columns["role"].FillWeight = 20;
                owners.Columns["name"].FillWeight = 27;
                owners.Columns["cccd"].FillWeight = 20;
                owners.Columns["phone"].FillWeight = 17;
                owners.Columns["status"].FillWeight = 16;
                string tenChuHienThi = Convert.ToString(tk["HoTen"]);
                owners.Rows.Add("Chủ tài khoản", tenChuHienThi, tk["CCCD"], tk["SoDienThoai"], status);
                foreach (DataRow item in coOwners.Rows)
                    owners.Rows.Add("Đồng sở hữu", item["HoTen"], item["CCCD"], item["SoDienThoai"], status);
                foreach (DataGridViewRow row in owners.Rows)
                    row.Cells[0].Style.ForeColor = row.Index == 0 ? Color.FromArgb(96, 165, 250) : Color.FromArgb(192, 132, 252);
                foreach (DataGridViewRow dong in owners.Rows)
                    ToMauTrangThaiThanhToan(dong.Cells["status"], status);
                root.Controls.Add(owners, 0, 3);

                Label functionHeading = new Label
                {
                    Text = "CHỨC NĂNG TÀI KHOẢN",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = white,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold)
                };
                root.Controls.Add(functionHeading, 0, 4);

                TableLayoutPanel groups = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 1,
                    Margin = Padding.Empty
                };
                for (int i = 0; i < 4; i++)
                    groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                groups.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                root.Controls.Add(groups, 0, 5);

                // Đọc lại dữ liệu thật từ SQL sau khi thay đổi thông tin hoặc giao dịch.
                // Giữ nguyên bố cục các thẻ thông tin và các callback nghiệp vụ.
                Action<string> capNhatNutTheoTrangThai = null;
                Action lamMoiSoDuChiTiet = () =>
                {
                    if (dlg.IsDisposed) return;
                    DataTable thongTinMoi = ExecuteQuery(@"
                        SELECT tk.SoDu, tk.TrangThai, tk.LoaiTienTe, tk.LoaiTaiKhoan,
                               tk.NgayMo, tk.HanMucNgay, tk.HanMucLan, tk.TenHienThi, tk.DiaChiBaoCao,
                               kh.HoTen AS TenChu, kh.SoDienThoai AS SDTChu
                        FROM dbo.TaiKhoanThanhToan tk INNER JOIN dbo.KhachHang kh ON kh.CCCD=tk.CCCD WHERE tk.SoTKTT=@stk",
                        new SqlParameter("@stk", SqlDbType.VarChar, 20) { Value = stk });
                    if (thongTinMoi.Rows.Count == 0) return;
                    DataRow moi = thongTinMoi.Rows[0];
                    balance.Text = Convert.ToDecimal(moi["SoDu"]).ToString("N0") + " VNĐ";
                    string trangThaiMoi = MapTrangThai(Convert.ToInt32(moi["TrangThai"]));
                    state.Text = "Trạng thái: " + trangThaiMoi + "     |     " + moi["LoaiTienTe"];
                    subtitle.Text = "Số tài khoản  " + stk + "     •     " + trangThaiMoi;
                    state.ForeColor = MauTrangThaiThanhToan(trangThaiMoi);
                    subtitle.ForeColor = MauTrangThaiThanhToan(trangThaiMoi);
                    capNhatNutTheoTrangThai?.Invoke(trangThaiMoi);
                    string tenHienThi = Convert.ToString(moi["TenHienThi"]);
                    accountInfo.Text = "Loại TK: " + moi["LoaiTaiKhoan"] + "\n" +
                        "Ngày mở: " + Convert.ToDateTime(moi["NgayMo"]).ToString("dd/MM/yyyy HH:mm:ss") +
                        (string.IsNullOrWhiteSpace(tenHienThi) ? "" : "\nTên hiển thị: " + tenHienThi);
                    limits.Text = "Theo ngày: " + Convert.ToDecimal(moi["HanMucNgay"]).ToString("N0") + " VNĐ\n" +
                                  "Mỗi lần: " + Convert.ToDecimal(moi["HanMucLan"]).ToString("N0") + " VNĐ";
                    foreach (DataGridViewRow dong in owners.Rows)
                        dong.Cells["status"].Value = trangThaiMoi;
                    string tenMoi = Convert.ToString(moi["TenHienThi"]);
                    if (string.IsNullOrWhiteSpace(tenMoi)) tenMoi = Convert.ToString(moi["TenChu"]);
                    if (owners.Rows.Count > 0)
                    {
                        owners.Rows[0].Cells["name"].Value = tenMoi;
                        owners.Rows[0].Cells["phone"].Value = Convert.ToString(moi["SDTChu"]);
                    }
                    DataTable dongChuMoi = ExecuteQuery(@"SELECT d.CCCD,
                        COALESCE(kh.HoTen,d.HoTen) AS HoTen,
                        COALESCE(kh.SoDienThoai,d.SoDienThoai) AS SoDienThoai
                        FROM dbo.DongSoHuuTaiKhoanThanhToan d
                        LEFT JOIN dbo.KhachHang kh ON kh.CCCD=d.CCCD
                        WHERE d.SoTKTT=@stk ORDER BY d.CCCD", new SqlParameter("@stk", stk));
                    while (owners.Rows.Count > 1) owners.Rows.RemoveAt(1);
                    foreach (DataRow dongChu in dongChuMoi.Rows)
                        owners.Rows.Add("Đồng sở hữu", dongChu["HoTen"], dongChu["CCCD"],
                            dongChu["SoDienThoai"], trangThaiMoi);
                    foreach (DataGridViewRow dong in owners.Rows)
                        ToMauTrangThaiThanhToan(dong.Cells["status"], trangThaiMoi);
                    HenTaiDanhSachThanhToan();
                };
                // Khi trở lại cửa sổ chi tiết, đồng bộ dữ liệu đã được sửa.
                bool daKichHoatChiTiet = false;
                dlg.Activated += (s, e) => {
                    if (!daKichHoatChiTiet) { daKichHoatChiTiet = true; return; }
                    lamMoiSoDuChiTiet();
                };

                AddThanhToanGroup(groups, "Giao dịch", 0, new[] {
                    new Tuple<string, Action, bool>("Nạp tiền", () => MenuNapTien_Click(null, EventArgs.Empty), canUse),
                    new Tuple<string, Action, bool>("Rút tiền", () => MenuRutTien_Click(null, EventArgs.Empty), canUse),
                    new Tuple<string, Action, bool>("Chuyển tiền", () => MenuChuyenTienTheoSTK(stk), canUse)
                }, lamMoiSoDuChiTiet, stk);
                AddThanhToanGroup(groups, "Trạng thái tài khoản", 1, new[] {
                    new Tuple<string, Action, bool>(locked ? "Mở khóa" : "Tạm khóa", () => MenuKhoaMoKhanCap_Click(null, EventArgs.Empty), !closed),
                    new Tuple<string, Action, bool>("Đóng tài khoản", () => MenuDongTK_Click(null, EventArgs.Empty), !closed)
                }, lamMoiSoDuChiTiet, stk);
                AddThanhToanGroup(groups, "Quản lý tài khoản", 2, new[] {
                    new Tuple<string, Action, bool>("Thay đổi thông tin", () => MenuDoiThongTinTK_Click(null, EventArgs.Empty), canUse),
                    new Tuple<string, Action, bool>("Đăng ký hạn mức", () => MenuDangKyHanMuc_Click(null, EventArgs.Empty), canUse),
                    new Tuple<string, Action, bool>("Đăng ký dịch vụ", () => MenuDangKyDichVu_Click(null, EventArgs.Empty), canUse)
                }, lamMoiSoDuChiTiet, stk);
                AddThanhToanGroup(groups, "Lịch sử & báo cáo", 3, new[] {
                    new Tuple<string, Action, bool>("Xem lịch sử giao dịch", () => MoLichSuTuChiTiet(stk, true), true),
                    new Tuple<string, Action, bool>("Xem lịch sử hoạt động", () => MoLichSuTuChiTiet(stk, false), true)
                }, lamMoiSoDuChiTiet, stk);

                Panel footer = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
                Button close = new Button
                {
                    Text = "Đóng cửa sổ",
                    Dock = DockStyle.Right,
                    Width = 160,
                    Height = 38,
                    Margin = new Padding(0, 6, 0, 4),
                    BackColor = Color.FromArgb(42, 58, 82),
                    ForeColor = white,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                close.FlatAppearance.BorderSize = 0;
                close.Click += (s, e) => dlg.Close();
                footer.Controls.Add(close);
                // Hai nút QR nằm ở thanh dưới, không phụ thuộc chiều cao nhóm chức năng.
                TableLayoutPanel qrBar = new TableLayoutPanel
                {
                    Dock = DockStyle.Left,
                    Width = 440,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                    ColumnCount = 2,
                    RowCount = 1
                };
                qrBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                qrBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                qrBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                Button btnQRNhanTien = new Button
                {
                    Name = "btnQRNhanTien",
                    Text = "QR nhận tiền",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 0, 6, 0),
                    BackColor = colorBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Enabled = canUse
                };
                btnQRNhanTien.FlatAppearance.BorderSize = 0;
                btnQRNhanTien.Click += (senderQR, argsQR) => HienThiQrNhanTien(stk);
                qrBar.Controls.Add(btnQRNhanTien, 0, 0);
                Button btnQRChuyenTien = new Button
                {
                    Name = "btnQRChuyenTien",
                    Text = "Chuyển tiền qua QR",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(6, 0, 0, 0),
                    Font = btnQRNhanTien.Font,
                    Cursor = Cursors.Hand,
                    Enabled = canUse
                };
                foreach (Button qr in new[] { btnQRNhanTien, btnQRChuyenTien })
                {
                    qr.UseVisualStyleBackColor = false;
                    qr.FlatStyle = FlatStyle.Flat;
                    qr.BackColor = colorBlue;
                    qr.ForeColor = Color.White;
                    qr.FlatAppearance.BorderSize = 0;
                    qr.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
                    qr.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);
                }
                btnQRChuyenTien.Click += (senderQR, argsQR) => MenuChuyenTienTheoSTK(stk, true);
                qrBar.Controls.Add(btnQRChuyenTien, 1, 0);
                footer.Controls.Add(qrBar);
                root.Controls.Add(footer, 0, 6);
                capNhatNutTheoTrangThai = trangThaiMoi =>
                {
                    bool hoatDong = trangThaiMoi == "Hoạt động";
                    bool dangKhoa = trangThaiMoi == "Đang khóa";
                    foreach (Panel nhom in groups.Controls.OfType<Panel>())
                    {
                        foreach (Button nut in nhom.Controls.OfType<Button>())
                        {
                            string ten = nut.Tag as string;
                            if (string.IsNullOrEmpty(ten)) continue;
                            bool lichSu = ten.StartsWith("Xem lịch sử", StringComparison.Ordinal);
                            bool khoaMo = ten == "Mở khóa" || ten == "Tạm khóa";
                            bool dong = ten == "Đóng tài khoản";
                            bool choPhep = lichSu || ((khoaMo || dong) ? hoatDong || dangKhoa : hoatDong);
                            string nhan = khoaMo ? (dangKhoa ? "Mở khóa" : "Tạm khóa") : ten;
                            nut.Enabled = choPhep;
                            nut.Text = nhan + (choPhep ? "" : " (Không khả dụng)");
                            nut.ForeColor = choPhep ? white : Color.FromArgb(248, 113, 113);
                            nut.Cursor = choPhep ? Cursors.Hand : Cursors.Default;
                        }
                    }
                    btnQRNhanTien.Enabled = hoatDong;
                    btnQRChuyenTien.Enabled = hoatDong;
                };
                dlg.ShowDialog(this);
            }
        }

        // Điều hướng đến danh mục Lịch sử của FrmMain, theo tài khoản đang mở chi tiết.
        private void MoLichSuTuChiTiet(string stk, bool giaoDich)
        {
            if (string.IsNullOrWhiteSpace(stk)) return;
            FrmMain main = null;
            foreach (Form f in Application.OpenForms)
            {
                main = f as FrmMain;
                if (main != null && !main.IsDisposed) break;
                main = null;
            }
            if (main == null)
            {
                MessageBox.Show("Không tìm thấy FrmMain để mở danh mục Lịch sử.",
                    "Điều hướng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            main.MoLichSuTuThanhToan(this, false, stk, giaoDich, _connStr);
        }

        private Panel TaoTheThongTinThanhToan(string title, Color backColor, Color labelColor)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backColor,
                Margin = new Padding(0, 0, 12, 0),
                Padding = new Padding(0)
            };
            Label heading = new Label
            {
                Text = title,
                Location = new Point(17, 12),
                AutoSize = true,
                ForeColor = labelColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panel.Controls.Add(heading);
            return panel;
        }

        private void AddThanhToanGroup(TableLayoutPanel parent, string title, int column,
            Tuple<string, Action, bool>[] items, Action sauThaoTac = null, string stkChiTiet = null)
        {
            Color card = Color.FromArgb(25, 36, 57);
            Color white = Color.FromArgb(241, 245, 249);
            Panel group = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = card,
                Margin = new Padding(column == 0 ? 0 : 5, 0, column == 3 ? 0 : 5, 0)
            };
            Label heading = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(147, 197, 253),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            group.Controls.Add(heading);
            for (int i = 0; i < items.Length; i++)
            {
                Tuple<string, Action, bool> spec = items[i];
                Button b = new Button
                {
                    Text = spec.Item1 + (spec.Item3 ? "" : " (Không khả dụng)"),
                    BackColor = Color.FromArgb(40, 56, 80),
                    ForeColor = spec.Item3 ? white : Color.FromArgb(248, 113, 113),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F),
                    Enabled = spec.Item3,
                    Cursor = spec.Item3 ? Cursors.Hand : Cursors.Default,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    Height = 39,
                    Left = 10,
                    Top = 39 + i * 47
                };
                b.Tag = spec.Item1;
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s, e) =>
                {
                    // Mọi chức năng trong cửa sổ chi tiết dùng STK cố định của cửa sổ.
                    // Không phụ thuộc dòng đang được chọn trên bảng chính.
                    string stkTruoc = _stkDangThaoTacChiTiet;
                    string cccdTruoc = _cccdDangSuaChiTiet;
                    _stkDangThaoTacChiTiet = stkChiTiet;
                    _cccdDangSuaChiTiet = null;
                    if (spec.Item1 == "Thay đổi thông tin" && stkChiTiet != null)
                    {
                        // Chọn đúng người trong bảng sở hữu của cửa sổ chi tiết.
                        Form chiTiet = parent.FindForm();
                        DataGridView bangSoHuu = chiTiet == null ? null :
                            TimBangSoHuu(chiTiet);
                        if (bangSoHuu != null && bangSoHuu.CurrentRow != null)
                            _cccdDangSuaChiTiet = Convert.ToString(
                                bangSoHuu.CurrentRow.Cells["cccd"].Value);
                    }
                    try
                    {
                        spec.Item2();
                        if (sauThaoTac != null && !spec.Item1.StartsWith("Xem lịch sử", StringComparison.Ordinal)) sauThaoTac();
                    }
                    finally
                    {
                        _stkDangThaoTacChiTiet = stkTruoc;
                        _cccdDangSuaChiTiet = cccdTruoc;
                    }
                };
                group.Controls.Add(b);
                // Giữ nút vừa chiều rộng nhóm kể cả khi phóng to hoặc thu nhỏ.
                Button current = b;
                group.Resize += (s, e) => current.Width = Math.Max(80, group.ClientSize.Width - 20);
            }
            parent.Controls.Add(group, column, 0);
        }

        private void HienThiKhachHangDangChon(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvThanhToan.Rows.Count) return;
            var row = dgvThanhToan.Rows[rowIndex];
            lblDaChonInfo.Text = $"{row.Cells["colHoTen"].Value}  |  CCCD: {row.Cells["colCCCD"].Value}  |  SĐT: {row.Cells["colSDT"].Value}  |  Số dư: {row.Cells["colTongSoTien"].Value}";
        }

        #endregion

        #region Context Menu Setup (GIỮ NGUYÊN)

        private void SetupContextMenuEvents()
        {
            menuMoThemTK.Click += MenuMoThemTK_Click;
            menuDoiThongTinTK.Click += MenuDoiThongTinTK_Click;
            menuDangKyHanMuc.Click += MenuDangKyHanMuc_Click;
            menuNapTien.Click += MenuNapTien_Click;
            menuRutTien.Click += MenuRutTien_Click;
            menuChuyenTien.Click += MenuChuyenTien_Click;
            menuKhoaMoKhanCap.Click += MenuKhoaMoKhanCap_Click;
            menuPhongToa.Visible = false; // Đã gộp vào Tạm khóa / Mở khóa
            menuDongTK.Click += MenuDongTK_Click;
            menuKhoaMoKhanCap.Text = "Tạm khóa / Mở khóa";
            menuDongTK.Text = "Đóng tài khoản";
            menuDangKyDichVu.Click += MenuDangKyDichVu_Click;
            menuXemLichSu.Click += MenuXemLichSu_Click;
            menuInSaoKe.Click += MenuInSaoKe_Click;
        }

        private void DgvThanhToan_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var hit = dgvThanhToan.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0) return;

            dgvThanhToan.ClearSelection();
            dgvThanhToan.Rows[hit.RowIndex].Selected = true;
            // CurrentRow phai trung voi dong vua nhan chuot phai (ke ca dong dong so huu).
            if (hit.ColumnIndex >= 0)
                dgvThanhToan.CurrentCell = dgvThanhToan.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
            else
                dgvThanhToan.CurrentCell = dgvThanhToan.Rows[hit.RowIndex].Cells[0];
            HienThiKhachHangDangChon(hit.RowIndex);

            string trangThai = dgvThanhToan.Rows[hit.RowIndex].Cells["colTrangThai"].Value?.ToString() ?? "Hoạt động";
            bool isClosed = trangThai == "Đã đóng";
            bool isLocked = trangThai == "Đang khóa";

            menuNapTien.Enabled = !isClosed && !isLocked;
            menuRutTien.Enabled = !isClosed && !isLocked;
            menuChuyenTien.Enabled = !isClosed && !isLocked;
            menuDongTK.Enabled = !isClosed && !isLocked;
            menuKhoaMoKhanCap.Enabled = !isClosed;
            menuPhongToa.Visible = false;
            menuDoiThongTinTK.Enabled = !isClosed && !isLocked;
            menuDangKyHanMuc.Enabled = !isClosed && !isLocked;
            menuDangKyDichVu.Enabled = !isClosed && !isLocked;
            menuMoThemTK.Enabled = !isClosed && !isLocked;
            menuXemLichSu.Enabled = true;
            menuInSaoKe.Enabled = true;
            menuKhoaMoKhanCap.Text = isLocked ? "Mở khóa" : "Tạm khóa";

            cmsThanhToan.Show(dgvThanhToan, e.Location);
        }

        // Tất cả nghiệp vụ thay đổi tiền phải kiểm tra trạng thái thật trong SQL.
        private bool KiemTraTaiKhoanHoatDong(string stk)
        {
            object value = ExecuteScalar("SELECT TrangThai FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk", new SqlParameter("@stk", stk));
            if (value == null || value == DBNull.Value) { MessageBox.Show("Không tìm thấy tài khoản."); return false; }
            int state = Convert.ToInt32(value);
            if (state != 0)
            {
                MessageBox.Show(state == 2 ? "Tài khoản đã đóng vĩnh viễn, không thể thực hiện thao tác này."
                    : "Tài khoản đang tạm khóa số dư. Hãy mở khóa trước.",
                    "Không thể thao tác", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Chỉ dùng trong thời gian thực thi nút của cửa sổ chi tiết.
        // Menu chuột phải ngoài bảng vẫn sử dụng CurrentRow như ban đầu.
        private string _stkDangThaoTacChiTiet;
        private string _cccdDangSuaChiTiet;

        private DataGridView TimBangSoHuu(Control cha)
        {
            foreach (Control con in cha.Controls)
            {
                DataGridView dgv = con as DataGridView;
                if (dgv != null && dgv.Columns.Contains("cccd") && dgv.Columns.Contains("role"))
                    return dgv;
                DataGridView found = TimBangSoHuu(con);
                if (found != null) return found;
            }
            return null;
        }

        private string GetSelectedSTK() =>
            !string.IsNullOrWhiteSpace(_stkDangThaoTacChiTiet)
                ? _stkDangThaoTacChiTiet
                : dgvThanhToan.CurrentRow?.Cells["colSTK"].Value?.ToString();

        private string GetSelectedHoTen()
        {
            if (!string.IsNullOrWhiteSpace(_stkDangThaoTacChiTiet))
            {
                object ten = ExecuteScalar(@"SELECT kh.HoTen
                    FROM dbo.TaiKhoanThanhToan tk
                    INNER JOIN dbo.KhachHang kh ON kh.CCCD = tk.CCCD
                    WHERE tk.SoTKTT = @stk",
                    new SqlParameter("@stk", _stkDangThaoTacChiTiet));
                return ten == null || ten == DBNull.Value ? "" : Convert.ToString(ten);
            }
            return dgvThanhToan.CurrentRow?.Cells["colHoTen"].Value?.ToString() ?? "";
        }
        private decimal GetSoDu(string stk)
        {
            object val = ExecuteScalar("SELECT SoDu FROM TaiKhoanThanhToan WHERE SoTKTT = @stk", new SqlParameter("@stk", stk));
            return val == null || val == DBNull.Value ? 0 : Convert.ToDecimal(val);
        }
        private string GetTrangThai(string stk)
        {
            object val = ExecuteScalar("SELECT TrangThai FROM TaiKhoanThanhToan WHERE SoTKTT = @stk", new SqlParameter("@stk", stk));
            return val == null ? "Hoạt động" : MapTrangThai(Convert.ToInt32(val));
        }

        #endregion

        #region 1. Mở & Quản lý tài khoản

        // Giao diện mở tài khoản được đồng bộ với FrmVay, nội dung dành cho Thanh toán.
        // Chủ sở hữu và đồng sở hữu đều được phép đứng tên chủ tài khoản mới.
        private void MenuMoThemTK_Click(object sender, EventArgs e)
        {
            MoTaiKhoanThanhToanCaNhan();
        }

        private void MoTaiKhoanThanhToanCaNhan(string cccdTim = null, FrmMain.KetQuaTimMoMoiCaNhan kq = null)
        {
            if (!DamBaoLienKetDongSoHuu()) return;
            bool tuTimKiem = kq != null;
            bool khachMoi = tuTimKiem && !kq.CoDanhTinh;
            DataGridViewRow selected = dgvThanhToan.CurrentRow;
            if (!tuTimKiem && selected != null && GetTrangThai(GetSelectedSTK()) == "Đã đóng")
            { MessageBox.Show("Tài khoản đã đóng vĩnh viễn."); return; }
            if (!tuTimKiem && (selected == null || selected.IsNewRow)) return;

            string cccd = tuTimKiem ? cccdTim : Convert.ToString(selected.Cells["colCCCD"].Value)?.Trim();
            if (string.IsNullOrWhiteSpace(cccd)) return;

            DataTable khTable = tuTimKiem ? null : ExecuteQuery(@"
                SELECT TOP 1 HoTen, SoDienThoai FROM
                (
                    SELECT HoTen, SoDienThoai, 0 AS UuTien FROM dbo.KhachHang WHERE CCCD=@cccd
                    UNION ALL
                    SELECT HoTen, SoDienThoai, 1 AS UuTien FROM dbo.DongSoHuu WHERE CCCD=@cccd
                ) x ORDER BY UuTien", new SqlParameter("@cccd", cccd));
            if (!tuTimKiem && khTable.Rows.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thông tin khách hàng được chọn.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hoTen = tuTimKiem ? kq.HoTen : Convert.ToString(khTable.Rows[0]["HoTen"]);
            string sdt = tuTimKiem ? kq.SoDienThoai : Convert.ToString(khTable.Rows[0]["SoDienThoai"]);
            using (Form dlg = new Form())
            {
                if (!tuTimKiem) dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = "Thêm Tài Khoản Cho Khách Hàng - Thanh toán";
                dlg.Size = new Size(580, 680);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.FromArgb(245, 245, 247);
                dlg.AutoScroll = true;

                Font fontNormal = new Font("Segoe UI", 9F, FontStyle.Regular);
                Font fontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
                Label lblCCCD = new Label { Text = "CCCD (12 số):", Location = new Point(35, 25), AutoSize = true, Font = fontNormal };
                TextBox txtCCCD = new TextBox { Location = new Point(165, 22), Width = 230, Text = cccd, ReadOnly = true, Enabled = false, Font = fontNormal };
                Label lblHoTen = new Label { Text = "Họ và Tên:", Location = new Point(35, 60), AutoSize = true, Font = fontNormal };
                TextBox txtHoTen = new TextBox { Location = new Point(165, 57), Width = 230, Text = hoTen, ReadOnly = !khachMoi, Enabled = khachMoi, MaxLength = 100, Font = fontNormal };
                Label lblSDT = new Label { Text = "Số điện thoại:", Location = new Point(35, 95), AutoSize = true, Font = fontNormal };
                TextBox txtSDT = new TextBox { Location = new Point(165, 92), Width = 230, Text = sdt, ReadOnly = !khachMoi, Enabled = khachMoi, MaxLength = 11, Font = fontNormal };

                Button btnDongSoHuu = new Button
                {
                    Text = "+ Thêm đồng sở hữu",
                    Location = new Point(410, 90),
                    Size = new Size(135, 28),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = colorBlue,
                    BackColor = Color.White,
                    Font = new Font("Segoe UI", 8.5F),
                    Cursor = Cursors.Hand
                };
                btnDongSoHuu.FlatAppearance.BorderColor = colorBlue;
                NumericUpDown nudDongSoHuu = new NumericUpDown
                {
                    Location = new Point(410, 125),
                    Width = 135,
                    Minimum = 1,
                    Maximum = 2,
                    Value = 1,
                    Visible = false
                };
                Panel pnlDongSoHuu = new Panel
                {
                    Location = new Point(25, 160),
                    Size = new Size(520, 205),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White,
                    AutoScroll = true,
                    Visible = false
                };
                var dongSoHuuInputs = new List<Tuple<TextBox, TextBox, TextBox>>();
                Action renderDongSoHuu = () =>
                {
                    var previous = dongSoHuuInputs.Select(x => new[] { x.Item1.Text, x.Item2.Text, x.Item3.Text }).ToList();
                    pnlDongSoHuu.Controls.Clear();
                    dongSoHuuInputs.Clear();
                    for (int i = 0; i < (int)nudDongSoHuu.Value; ++i)
                    {
                        GroupBox group = new GroupBox { Text = "Đồng sở hữu " + (i + 1), Location = new Point(8, 8 + i * 64), Size = new Size(490, 58) };
                        TextBox c = new TextBox { Location = new Point(50, 20), Width = 115, MaxLength = 12 };
                        TextBox n = new TextBox { Location = new Point(225, 20), Width = 105 };
                        TextBox t = new TextBox { Location = new Point(375, 20), Width = 105, MaxLength = 11 };
                        c.KeyPress += (ss, ee) => { if (!char.IsControl(ee.KeyChar) && !char.IsDigit(ee.KeyChar)) ee.Handled = true; };
                        t.KeyPress += (ss, ee) => { if (!char.IsControl(ee.KeyChar) && !char.IsDigit(ee.KeyChar)) ee.Handled = true; };
                        if (i < previous.Count) { c.Text = previous[i][0]; n.Text = previous[i][1]; t.Text = previous[i][2]; }
                        group.Controls.AddRange(new Control[] {
                            new Label { Text = "CCCD", Location = new Point(7, 23), AutoSize = true }, c,
                            new Label { Text = "Họ tên", Location = new Point(172, 23), AutoSize = true }, n,
                            new Label { Text = "SĐT", Location = new Point(340, 23), AutoSize = true }, t
                        });
                        pnlDongSoHuu.Controls.Add(group);
                        dongSoHuuInputs.Add(Tuple.Create(c, n, t));
                    }
                };
                nudDongSoHuu.ValueChanged += (ss, ee) => renderDongSoHuu();

                // Hiển thị đủ ba loại tài khoản của hệ thống nhưng cố định Thanh toán trên form này.
                Label lblLoaiTaiKhoan = new Label { Text = "Loại tài khoản:", Location = new Point(35, 130), AutoSize = true, Font = fontNormal };
                ComboBox cboLoaiTaiKhoan = new ComboBox
                {
                    Location = new Point(165, 127),
                    Width = 230,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = fontNormal,
                    Enabled = false
                };
                cboLoaiTaiKhoan.Items.AddRange(new object[] { "Thanh toán", "Tiết kiệm", "Vay" });
                cboLoaiTaiKhoan.SelectedIndex = 0; // Luôn cố định ở Thanh toán; Enabled = false.

                // Tách hình thức sở hữu thành trường riêng để giữ chức năng đồng sở hữu.
                Label lblLoaiTK = new Label { Text = "Hình thức sở hữu:", Location = new Point(35, 165), AutoSize = true, Font = fontNormal };
                ComboBox cboLoaiTK = new ComboBox { Location = new Point(165, 162), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = fontNormal };
                cboLoaiTK.Items.AddRange(new object[] { "Đơn danh", "Đồng sở hữu" });
                cboLoaiTK.SelectedIndex = 0;

                RadioButton rdoNgauNhien = new RadioButton { Text = "Số ngẫu nhiên (Free)", Location = new Point(35, 200), AutoSize = true, Checked = true, Font = fontNormal };
                RadioButton rdoSoDep = new RadioButton { Text = "Số đẹp (Tính phí)", Location = new Point(200, 200), AutoSize = true, Font = fontNormal };
                Label lblSTK = new Label { Text = "Số TK được cấp:", Location = new Point(35, 235), AutoSize = true, Font = fontNormal };
                TextBox txtSTK = new TextBox { Location = new Point(165, 232), Width = 230, ReadOnly = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), BackColor = Color.White };
                Random rnd = new Random();
                Action generateSTK = () =>
                {
                    try
                    {
                        // Sinh lai neu trung bat ky STK thanh toan, vay, tiet kiem.
                        for (int i = 0; i < 100; i++)
                        {
                            string candidate = rdoSoDep.Checked
                                ? "8888" + rnd.Next(1000, 10000).ToString("D4")
                                : "106" + rnd.Next(1000000, 10000000).ToString("D7");
                            if (!SoTaiKhoanDaTonTai(candidate))
                            {
                                txtSTK.Text = candidate;
                                return;
                            }
                        }
                        txtSTK.Clear();
                        MessageBox.Show("Chua tao duoc so tai khoan khong trung. Vui long thu lai.");
                    }
                    catch (Exception ex)
                    {
                        txtSTK.Clear();
                        MessageBox.Show("Khong the kiem tra so tai khoan tren SQL: " + ex.Message);
                    }
                };
                rdoNgauNhien.CheckedChanged += (ss, ee) => { if (rdoNgauNhien.Checked) generateSTK(); };
                rdoSoDep.CheckedChanged += (ss, ee) => { if (rdoSoDep.Checked) generateSTK(); };
                generateSTK();

                Panel pnlContainer = new Panel
                {
                    Location = new Point(25, 270),
                    Size = new Size(520, 350),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };
                Label lblHeader = new Label { Text = "THÔNG TIN TÀI KHOẢN THANH TOÁN", Location = new Point(25, 17), AutoSize = true, Font = fontBold, ForeColor = colorBlue };
                Label lblTienTe = new Label { Text = "Loại tiền tệ:", Location = new Point(25, 60), AutoSize = true, Font = fontNormal };
                ComboBox cboTienTe = new ComboBox { Location = new Point(180, 57), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = fontNormal };
                cboTienTe.Items.AddRange(new object[] { "VND", "USD", "EUR", "JPY", "CNY" });
                cboTienTe.SelectedIndex = 0;
                Label lblHanMucNgay = new Label { Text = "Hạn mức / ngày:", Location = new Point(25, 105), AutoSize = true, Font = fontNormal };
                TextBox txtHanMucNgay = new TextBox { Location = new Point(180, 102), Width = 220, Text = "50000000", Font = fontNormal };
                Label lblHanMucLan = new Label { Text = "Hạn mức / lần:", Location = new Point(25, 150), AutoSize = true, Font = fontNormal };
                TextBox txtHanMucLan = new TextBox { Location = new Point(180, 147), Width = 220, Text = "20000000", Font = fontNormal };
                Label lblThongTin = new Label
                {
                    Text = "Số dư ban đầu: 0 VNĐ  |  Trạng thái: Hoạt động",
                    Location = new Point(25, 199),
                    AutoSize = true,
                    Font = fontNormal,
                    ForeColor = Color.FromArgb(70, 85, 110)
                };
                Button btnXacNhan = new Button
                {
                    Text = "Xác nhận mở",
                    Location = new Point(110, 262),
                    Size = new Size(120, 35),
                    BackColor = colorBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = fontBold
                };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(240, 262), Size = new Size(110, 35), Font = fontNormal };
                btnXacNhan.FlatAppearance.BorderSize = 0;

                Action updateLayout = () =>
                {
                    int offset = pnlDongSoHuu.Visible ? 220 : 0;
                    lblLoaiTaiKhoan.Top = 130 + offset; cboLoaiTaiKhoan.Top = 127 + offset;
                    lblLoaiTK.Top = 165 + offset; cboLoaiTK.Top = 162 + offset;
                    rdoNgauNhien.Top = 200 + offset; rdoSoDep.Top = 200 + offset;
                    lblSTK.Top = 235 + offset; txtSTK.Top = 232 + offset;
                    pnlContainer.Top = 270 + offset;
                    dlg.AutoScrollMinSize = new Size(0, pnlContainer.Bottom + 45);
                };
                btnDongSoHuu.Click += (ss, ee) =>
                {
                    if (!pnlDongSoHuu.Visible)
                    {
                        nudDongSoHuu.Visible = true;
                        pnlDongSoHuu.Visible = true;
                        cboLoaiTK.SelectedItem = "Đồng sở hữu";
                        if (dongSoHuuInputs.Count == 0) renderDongSoHuu();
                    }
                    else
                    {
                        nudDongSoHuu.Visible = false;
                        pnlDongSoHuu.Visible = false;
                        cboLoaiTK.SelectedItem = "Đơn danh";
                    }
                    updateLayout();
                };
                cboLoaiTK.SelectedIndexChanged += (ss, ee) =>
                {
                    if (cboLoaiTK.SelectedItem?.ToString() == "Đơn danh" && pnlDongSoHuu.Visible)
                    {
                        pnlDongSoHuu.Visible = false;
                        nudDongSoHuu.Visible = false;
                    }
                    updateLayout();
                };
                btnHuy.Click += (ss, ee) => dlg.Close();
                btnXacNhan.Click += (ss, ee) =>
                {
                    if (khachMoi)
                    {
                        hoTen = txtHoTen.Text.Trim(); sdt = txtSDT.Text.Trim();
                        if (!FrmMain.KiemTraThongTinTimMoMoiCaNhan(cccd, hoTen, sdt)) return;
                    }
                    string stk = txtSTK.Text.Trim();
                    if (stk.Length < 8 || stk.Length > 20 || !stk.All(char.IsDigit))
                    {
                        MessageBox.Show("Số tài khoản phải gồm 8 đến 20 chữ số.");
                        return;
                    }
                    decimal hmNgay, hmLan;
                    string ngayDigits = new string(txtHanMucNgay.Text.Where(char.IsDigit).ToArray());
                    string lanDigits = new string(txtHanMucLan.Text.Where(char.IsDigit).ToArray());
                    if (!decimal.TryParse(ngayDigits, out hmNgay) || hmNgay <= 0 ||
                        !decimal.TryParse(lanDigits, out hmLan) || hmLan <= 0 || hmLan > hmNgay)
                    {
                        MessageBox.Show("Hạn mức không hợp lệ. Hạn mức mỗi lần phải nhỏ hơn hoặc bằng hạn mức ngày.");
                        return;
                    }
                    bool laDongSoHuu = cboLoaiTK.SelectedItem?.ToString() == "Đồng sở hữu";
                    if (laDongSoHuu && (!pnlDongSoHuu.Visible || dongSoHuuInputs.Count == 0))
                    {
                        MessageBox.Show("Vui lòng nhấn '+ Thêm đồng sở hữu' và nhập thông tin người đồng sở hữu.");
                        return;
                    }
                    var dsMoi = new List<Tuple<string, string, string>>();
                    var cccdDaNhap = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { cccd };
                    var sdtDaNhap = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { sdt };
                    if (laDongSoHuu)
                    {
                        foreach (var fields in dongSoHuuInputs)
                        {
                            string c = fields.Item1.Text.Trim(), n = fields.Item2.Text.Trim(), t = fields.Item3.Text.Trim();
                            if (c.Length != 12 || !c.All(char.IsDigit) || string.IsNullOrWhiteSpace(n) ||
                                (t.Length != 10 && t.Length != 11) || !t.All(char.IsDigit) ||
                                !cccdDaNhap.Add(c) || !sdtDaNhap.Add(t))
                            {
                                MessageBox.Show("Đồng sở hữu phải có CCCD 12 số, họ tên, SĐT 10-11 số và không trùng thông tin.");
                                return;
                            }
                            dsMoi.Add(Tuple.Create(c, n, t));
                        }
                    }

                    // Các thao tác đều nằm trong cùng transaction: thất bại sẽ rollback toàn bộ.
                    try
                    {
                        using (SqlConnection conn = GetConn())
                        {
                            conn.Open();
                            using (SqlTransaction tran = conn.BeginTransaction())
                            {
                                try
                                {
                                    // Xac minh lai so STK ngay truoc INSERT, trong transaction.
                                    using (SqlCommand checkStk = new SqlCommand(@"
                                        IF EXISTS (SELECT 1 FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTT=@stk)
                                           OR EXISTS (SELECT 1 FROM dbo.TaiKhoanVay WITH (UPDLOCK, HOLDLOCK) WHERE SoTKVay=@stk)
                                           OR EXISTS (SELECT 1 FROM dbo.SoTietKiem WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTK=@stk)
                                            THROW 50002, N'So tai khoan da ton tai trong he thong.', 1;", conn, tran))
                                    {
                                        checkStk.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        checkStk.ExecuteNonQuery();
                                    }
                                    if (khachMoi) FrmMain.KiemTraTrungKhachHangTimMoMoiCaNhan(conn, tran, cccd, sdt);
                                    if (tuTimKiem)
                                    {
                                        using (var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.KhachHang WITH (UPDLOCK,HOLDLOCK) WHERE CCCD=@cccd)
                                            INSERT INTO dbo.KhachHang(CCCD,HoTen,SoDienThoai) VALUES(@cccd,@ten,@sdt)", conn, tran))
                                        {
                                            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccd;
                                            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = hoTen;
                                            cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 11).Value = sdt;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    else using (SqlCommand cmd = new SqlCommand(@"
                                        IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WITH (UPDLOCK, HOLDLOCK) WHERE CCCD=@cccd)
                                        BEGIN
                                            INSERT INTO dbo.KhachHang(CCCD, HoTen, SoDienThoai)
                                            SELECT TOP 1 CCCD, HoTen, SoDienThoai FROM dbo.DongSoHuu
                                            WHERE CCCD=@cccd ORDER BY Id;
                                        END
                                        IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD=@cccd)
                                            THROW 50001, N'Không tìm thấy chủ sở hữu.', 1;", conn, tran))
                                        {
                                            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccd;
                                            cmd.ExecuteNonQuery();
                                        }
                                    foreach (var d in dsMoi)
                                    {
                                        // Nguoi dong so huu moi phai co CCCD va SDT chua ton tai.
                                        using (SqlCommand cmd = new SqlCommand(@"
                                            SELECT COUNT(*) FROM
                                            (SELECT CCCD, SoDienThoai FROM dbo.KhachHang
                                             UNION ALL SELECT CCCD, SoDienThoai FROM dbo.DongSoHuu) x
                                            WHERE CCCD=@c OR SoDienThoai=@s", conn, tran))
                                        {
                                            cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = d.Item1;
                                            cmd.Parameters.Add("@s", SqlDbType.VarChar, 11).Value = d.Item3;
                                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                                                throw new InvalidOperationException("CCCD hoặc SĐT đã được đăng ký: " + d.Item1);
                                        }
                                    }
                                    using (SqlCommand cmd = new SqlCommand(@"
                                        INSERT INTO dbo.TaiKhoanThanhToan
                                        (SoTKTT, CCCD, SoDu, TrangThai, NgayMo, NgayCapNhatTT,
                                         LoaiTienTe, LoaiTaiKhoan, HanMucNgay, HanMucLan)
                                        VALUES (@stk, @cccd, 0, 0, GETDATE(), GETDATE(), @tienTe, @loai, @ngay, @lan);", conn, tran))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccd;
                                        cmd.Parameters.Add("@tienTe", SqlDbType.NVarChar, 10).Value = cboTienTe.SelectedItem.ToString();
                                        cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 30).Value = cboLoaiTK.SelectedItem.ToString();
                                        SqlParameter pNgay = cmd.Parameters.Add("@ngay", SqlDbType.Decimal);
                                        pNgay.Precision = 18; pNgay.Scale = 0; pNgay.Value = hmNgay;
                                        SqlParameter pLan = cmd.Parameters.Add("@lan", SqlDbType.Decimal);
                                        pLan.Precision = 18; pLan.Scale = 0; pLan.Value = hmLan;
                                        cmd.ExecuteNonQuery();
                                    }
                                    foreach (var d in dsMoi)
                                    {
                                        using (SqlCommand cmd = new SqlCommand(@"
                                            INSERT INTO dbo.DongSoHuuTaiKhoanThanhToan(SoTKTT, CCCD, HoTen, SoDienThoai)
                                            VALUES (@stk, @cccd, @ten, @sdt);", conn, tran))
                                        {
                                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = d.Item1;
                                            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = d.Item2;
                                            cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 11).Value = d.Item3;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    using (SqlCommand cmd = new SqlCommand(@"
                                        INSERT INTO dbo.LichSuGiaoDich (SoTKTT, NoiDung, SoTien, SoDuSau, ThoiGian, NguoiThucHien)
                                         VALUES (@stk, @noidung, NULL, 0, GETDATE(), @nguoi);", conn, tran))
                                    {
                                        cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        cmd.Parameters.Add("@noidung", SqlDbType.NVarChar, 500).Value =
                                            "Mở tài khoản mới. Loại: " + cboLoaiTK.SelectedItem + ", Tiền tệ: " + cboTienTe.SelectedItem;
                                        cmd.ExecuteNonQuery();
                                    }
                                    tran.Commit();
                                }
                                catch { tran.Rollback(); throw; }
                            }
                        }
                        MessageBox.Show("Mở tài khoản thanh toán thành công!\n\nChủ sở hữu: " + hoTen +
                            "\nCCCD: " + cccd + "\nSố tài khoản: " + stk +
                            "\nLoại tài khoản: " + cboLoaiTK.SelectedItem,
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(ex.Number == 2601 || ex.Number == 2627
                                ? "Số tài khoản hoặc thông tin khách hàng đã tồn tại. Vui lòng kiểm tra lại."
                                : "Không thể mở tài khoản: " + ex.Message,
                            "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể mở tài khoản: " + ex.Message,
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                pnlContainer.Controls.AddRange(new Control[] {
                    lblHeader, lblTienTe, cboTienTe, lblHanMucNgay, txtHanMucNgay,
                    lblHanMucLan, txtHanMucLan, lblThongTin, btnXacNhan, btnHuy
                });
                dlg.Controls.AddRange(new Control[] {
                    lblCCCD, txtCCCD, lblHoTen, txtHoTen, lblSDT, txtSDT,
                    btnDongSoHuu, nudDongSoHuu, pnlDongSoHuu,
                    lblLoaiTaiKhoan, cboLoaiTaiKhoan,
                    lblLoaiTK, cboLoaiTK, rdoNgauNhien, rdoSoDep,
                    lblSTK, txtSTK, pnlContainer
                });
                updateLayout();
                if (tuTimKiem) FrmMain.DongBoMauKhungMoTaiKhoan(dlg);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    HienThiDanhSach(txtCCCD.Text.Trim());
                    OnDataChanged();
                }
            }
        }

        private void MenuDoiThongTinTK_Click(object sender, EventArgs e)
        {
            string stk = GetSelectedSTK();
            if (string.IsNullOrEmpty(stk) || !KiemTraTaiKhoanHoatDong(stk)) return;

            // Ưu tiên người được chọn trong bảng chi tiết, nếu mở bằng chuột phải
            // thì lấy CCCD của chính hàng đang chọn trên bảng chính.
            string cccdChon = _cccdDangSuaChiTiet;
            if (string.IsNullOrWhiteSpace(cccdChon))
                cccdChon = Convert.ToString(dgvThanhToan.CurrentRow?.Cells["colCCCD"].Value);
            if (string.IsNullOrWhiteSpace(cccdChon)) return;

            DataTable dt = ExecuteQuery(@"
                SELECT tk.TenHienThi, tk.DiaChiBaoCao, tk.LoaiTienTe, tk.LoaiTaiKhoan, 
                       tk.HanMucNgay, tk.HanMucLan,
                       COALESCE(kh.HoTen, d.HoTen) AS HoTen,
                       @cccd AS CCCD,
                       CASE WHEN tk.CCCD=@cccd THEN 1 ELSE 0 END AS LaChu
                FROM TaiKhoanThanhToan tk
                LEFT JOIN dbo.DongSoHuuTaiKhoanThanhToan d
                    ON d.SoTKTT=tk.SoTKTT AND d.CCCD=@cccd
                LEFT JOIN dbo.KhachHang kh ON kh.CCCD=@cccd
                WHERE tk.SoTKTT=@stk
                  AND (tk.CCCD=@cccd OR d.CCCD IS NOT NULL)",
                new SqlParameter("@stk", stk),
                new SqlParameter("@cccd", cccdChon));

            if (dt.Rows.Count == 0) return;
            var r = dt.Rows[0];
            bool laChu = Convert.ToInt32(r["LaChu"]) == 1;

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = $"Thay đổi thông tin TK {stk}";
                dlg.Size = new Size(520, 355);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                int y = 15;
                var lblInfo = new Label { Text = $"STK: {stk} | {(laChu ? "Chủ sở hữu" : "Đồng sở hữu")}: {r["HoTen"]} | CCCD: {r["CCCD"]}", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                y += 40;

                var lblTen = new Label { Text = "Tên hiển thị:", Location = new Point(20, y), AutoSize = true };
                var txtTen = new TextBox { Location = new Point(160, y - 3), Width = 260, Text = Convert.ToString(r["HoTen"]) };
                y += 35;

                var lblDC = new Label { Text = "Địa chỉ nhận BC:", Location = new Point(20, y), AutoSize = true };
                var txtDC = new TextBox { Location = new Point(160, y - 3), Width = 260, Height = 60, Multiline = true, Text = r["DiaChiBaoCao"]?.ToString() ?? "" };
                y += 75;

                var lblTienTe = new Label { Text = "Loại tiền tệ:", Location = new Point(20, y), AutoSize = true };
                var cboTienTe = new ComboBox { Location = new Point(160, y - 3), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
                cboTienTe.Items.AddRange(new[] { "VND", "USD", "EUR", "JPY", "CNY" });
                cboTienTe.SelectedItem = r["LoaiTienTe"]?.ToString() ?? "VND";
                y += 45;

                var btnOk = new Button { Text = "Lưu thay đổi", Location = new Point(160, y), Size = new Size(120, 35), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnHuy = new Button { Text = "Hủy", Location = new Point(300, y), Size = new Size(100, 35) };

                btnOk.Click += (s, ev) =>
                {
                    string tenMoi = txtTen.Text.Trim();
                    if (string.IsNullOrWhiteSpace(tenMoi))
                    {
                        MessageBox.Show("Vui lòng nhập tên khách hàng.");
                        return;
                    }

                    try
                    {
                        using (SqlConnection cn = GetConn())
                        {
                            cn.Open();
                            using (SqlTransaction tran = cn.BeginTransaction())
                            {
                                try
                                {
                                    // Đồng bộ tên theo CCCD chủ sở hữu, không theo một STK.
                                    // Các trường địa chỉ báo cáo / tiền tệ vẫn riêng từng tài khoản.
                                    string sql = @"
                                        IF NOT EXISTS (
                                            SELECT 1 FROM dbo.TaiKhoanThanhToan tk
                                            LEFT JOIN dbo.DongSoHuuTaiKhoanThanhToan d
                                                ON d.SoTKTT=tk.SoTKTT AND d.CCCD=@cccd
                                            WHERE tk.SoTKTT=@stk
                                              AND (tk.CCCD=@cccd OR d.CCCD IS NOT NULL))
                                            THROW 50001, N'Người được chọn không thuộc tài khoản này.', 1;

                                        UPDATE dbo.KhachHang SET HoTen=@ten WHERE CCCD=@cccd;
                                        UPDATE dbo.DongSoHuu SET HoTen=@ten WHERE CCCD=@cccd;
                                        UPDATE dbo.DongSoHuuTaiKhoanThanhToan
                                        SET HoTen=@ten WHERE CCCD=@cccd;

                                        -- Chỉ cập nhật tên hiển thị của tài khoản do người này làm chủ.
                                        UPDATE dbo.TaiKhoanThanhToan
                                        SET TenHienThi=@ten, NgayCapNhatTT=GETDATE()
                                        WHERE CCCD=@cccd;

                                        -- Các thiết lập này chỉ áp dụng cho STK đang sửa.
                                        UPDATE dbo.TaiKhoanThanhToan
                                        SET DiaChiBaoCao=@dc, LoaiTienTe=@tt,
                                            NgayCapNhatTT=GETDATE()
                                        WHERE SoTKTT=@stk;";
                                    using (SqlCommand cmd = new SqlCommand(sql, cn, tran))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccdChon;
                                        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = tenMoi;
                                        cmd.Parameters.Add("@dc", SqlDbType.NVarChar, -1).Value = txtDC.Text.Trim();
                                        cmd.Parameters.Add("@tt", SqlDbType.VarChar, 10).Value = cboTienTe.SelectedItem.ToString();
                                        cmd.ExecuteNonQuery();
                                    }
                                    tran.Commit();
                                }
                                catch
                                {
                                    tran.Rollback();
                                    throw;
                                }
                            }
                        }
                        GhiLichSu(stk, "Cập nhật thông tin " + (laChu ? "chủ sở hữu" : "đồng sở hữu") + " CCCD " + cccdChon);
                        MessageBox.Show("Đã cập nhật thông tin người được chọn và đồng bộ theo CCCD!");
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể cập nhật thông tin:\n" + ex.Message,
                            "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lblInfo, lblTen, txtTen, lblDC, txtDC, lblTienTe, cboTienTe, btnOk, btnHuy });

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    HienThiDanhSach(txtCCCD.Text.Trim());
                    OnDataChanged();
                }
            }
        }

        private void MenuDangKyHanMuc_Click(object sender, EventArgs e)
        {
            string stk = GetSelectedSTK();
            if (string.IsNullOrEmpty(stk) || !KiemTraTaiKhoanHoatDong(stk)) return;

            DataTable dt = ExecuteQuery("SELECT HanMucNgay, HanMucLan, SoDu, TrangThai FROM TaiKhoanThanhToan WHERE SoTKTT = @stk", new SqlParameter("@stk", stk));
            if (dt.Rows.Count == 0) return;
            var r = dt.Rows[0];

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = $"Đăng ký hạn mức - {stk}";
                dlg.Size = new Size(420, 250);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                int y = 15;
                var lblInfo = new Label { Text = $"Số dư: {Convert.ToDecimal(r["SoDu"]):N0} VNĐ | Trạng thái: {MapTrangThai(Convert.ToInt32(r["TrangThai"]))}", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                y += 40;

                var lbl1 = new Label { Text = "Hạn mức / ngày (VNĐ):", Location = new Point(20, y), AutoSize = true };
                var txt1 = new TextBox { Location = new Point(180, y - 3), Width = 180, Text = Convert.ToDecimal(r["HanMucNgay"]).ToString("N0") };
                y += 40;

                var lbl2 = new Label { Text = "Hạn mức / lần (VNĐ):", Location = new Point(20, y), AutoSize = true };
                var txt2 = new TextBox { Location = new Point(180, y - 3), Width = 180, Text = Convert.ToDecimal(r["HanMucLan"]).ToString("N0") };
                y += 50;

                var btnOk = new Button { Text = "Cập nhật hạn mức", Location = new Point(140, y), Size = new Size(140, 35), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnHuy = new Button { Text = "Hủy", Location = new Point(300, y), Size = new Size(80, 35) };

                btnOk.Click += (s, ev) =>
                {
                    if (!decimal.TryParse(txt1.Text.Replace(",", ""), out decimal hmNgay) ||
                        !decimal.TryParse(txt2.Text.Replace(",", ""), out decimal hmLan))
                    { MessageBox.Show("Giá trị không hợp lệ!"); return; }

                    // 0 vẫn giữ ý nghĩa không giới hạn như logic chuyển khoản hiện tại.
                    if (hmNgay < 0 || hmLan < 0 || hmNgay != decimal.Truncate(hmNgay) ||
                        hmLan != decimal.Truncate(hmLan) || hmNgay > 999999999999999999m ||
                        hmLan > 999999999999999999m)
                    { MessageBox.Show("Hạn mức phải là số nguyên không âm và phù hợp kiểu dữ liệu SQL."); return; }
                    try
                    {
                        string nguoi = LayTenNguoiThucHien();
                        using (var cn = GetConn())
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction())
                            {
                                using (var cmd = new SqlCommand(@"UPDATE dbo.TaiKhoanThanhToan
                                    SET HanMucNgay=@n, HanMucLan=@l, NgayCapNhatTT=GETDATE()
                                    WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@n", hmNgay);
                                    cmd.Parameters.AddWithValue("@l", hmLan);
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                    if (cmd.ExecuteNonQuery() != 1)
                                        throw new InvalidOperationException("Tài khoản không còn hoạt động.");
                                }
                                using (var cmd = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDich
                                    (SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
                                    VALUES(@stk,@nd,NULL,NULL,GETDATE(),@nguoi)", cn, tx))
                                {
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                    cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value =
                                        $"Cập nhật hạn mức: Ngày={hmNgay:N0}, Lần={hmLan:N0}";
                                    cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = nguoi;
                                    cmd.ExecuteNonQuery();
                                }
                                tx.Commit();
                            }
                        }
                        MessageBox.Show("Đã cập nhật hạn mức!");
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    { MessageBox.Show("Không thể cập nhật hạn mức: " + ex.Message); }
                };

                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lblInfo, lbl1, txt1, lbl2, txt2, btnOk, btnHuy });
                if (dlg.ShowDialog() == DialogResult.OK) OnDataChanged();
            }
        }

        #endregion

        #region 2. Giao dịch

        private void MenuNapTien_Click(object sender, EventArgs e)
        {
            string stk = GetSelectedSTK();
            string hoTen = GetSelectedHoTen();
            if (string.IsNullOrEmpty(stk) || !KiemTraTaiKhoanHoatDong(stk)) return;
            decimal soDuHienTai = GetSoDu(stk);

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = "Nộp tiền mặt vào tài khoản";
                dlg.Size = new Size(510, 640);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.FromArgb(245, 245, 247);

                var lblInfo = new Label
                {
                    Text = $"TK: {stk} - {hoTen}\nSố dư hiện tại: {soDuHienTai:N0} VNĐ",
                    Location = new Point(20, 15),
                    Size = new Size(455, 48),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
                };
                var lblMG = new Label
                {
                    Text = "Chọn số lượng tờ tiền theo từng mệnh giá:",
                    Location = new Point(20, 77),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
                };

                int[] menhGia = { 500000, 200000, 100000, 50000, 20000, 10000, 5000, 2000, 1000 };
                var danhSachSoTo = new List<NumericUpDown>();
                var pnlMenhGia = new Panel
                {
                    Location = new Point(20, 105),
                    Size = new Size(450, 340),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoScroll = true
                };
                var lblTong = new Label
                {
                    Text = "TỔNG TIỀN NỘP: 0 VNĐ",
                    Location = new Point(20, 463),
                    Size = new Size(450, 34),
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                    ForeColor = colorGreen
                };
                Action tinhTong = () =>
                {
                    decimal tong = 0m;
                    for (int i = 0; i < menhGia.Length; i++)
                        tong += menhGia[i] * danhSachSoTo[i].Value;
                    lblTong.Text = "TỔNG TIỀN NỘP: " + tong.ToString("N0") + " VNĐ";
                };

                for (int i = 0; i < menhGia.Length; i++)
                {
                    int giaTri = menhGia[i];
                    int y = 12 + i * 35;
                    var lbl = new Label
                    {
                        Text = giaTri.ToString("N0") + " VNĐ",
                        Location = new Point(20, y + 4),
                        Size = new Size(155, 24),
                        Font = new Font("Segoe UI", 10F)
                    };
                    var nud = new NumericUpDown
                    {
                        Location = new Point(185, y),
                        Size = new Size(125, 26),
                        Minimum = 0,
                        Maximum = 100000,
                        Value = 0,
                        DecimalPlaces = 0,
                        Increment = 1,
                        ThousandsSeparator = true,
                        Font = new Font("Segoe UI", 10F)
                    };
                    var lblDonVi = new Label
                    {
                        Text = "tờ",
                        Location = new Point(322, y + 4),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10F)
                    };
                    danhSachSoTo.Add(nud);
                    nud.ValueChanged += (s, ev) => tinhTong();
                    pnlMenhGia.Controls.AddRange(new Control[] { lbl, nud, lblDonVi });
                }

                var btnOk = new Button
                {
                    Text = "Xác nhận nộp",
                    Location = new Point(190, 525),
                    Size = new Size(130, 38),
                    BackColor = colorBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(335, 525),
                    Size = new Size(100, 38)
                };
                btnOk.Click += (s, ev) =>
                {
                    decimal soTien = 0m;
                    var chiTietMenhGia = new List<string>();
                    for (int i = 0; i < menhGia.Length; i++)
                    {
                        decimal soTo = danhSachSoTo[i].Value;
                        if (soTo <= 0) continue;
                        soTien += menhGia[i] * soTo;
                        chiTietMenhGia.Add(menhGia[i].ToString("N0") + " x " + soTo.ToString("N0"));
                    }
                    if (soTien <= 0)
                    {
                        MessageBox.Show("Vui lòng chọn ít nhất một tờ tiền để nộp!");
                        return;
                    }
                    if (!KiemTraTaiKhoanHoatDong(stk)) return;
                    string chiTiet = " | Mệnh giá: " + string.Join("; ", chiTietMenhGia);
                    decimal soDuMoi;
                    try
                    {
                        string nguoi = LayTenNguoiThucHien();
                        using (var cn = GetConn())
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                using (var cmd = new SqlCommand(@"UPDATE dbo.TaiKhoanThanhToan
                                    SET SoDu=SoDu+@tien, NgayCapNhatTT=GETDATE()
                                    OUTPUT INSERTED.SoDu
                                    WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                                {
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                    cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = soTien;
                                    object du = cmd.ExecuteScalar();
                                    if (du == null || du == DBNull.Value)
                                        throw new InvalidOperationException("Tài khoản không còn hoạt động.");
                                    soDuMoi = Convert.ToDecimal(du);
                                }
                                using (var cmd = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDich
                                    (SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
                                    VALUES(@stk,@nd,@tien,@du,GETDATE(),@nguoi)", cn, tx))
                                {
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                    cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value = $"Nộp tiền mặt: +{soTien:N0} VNĐ{chiTiet}";
                                    cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = soTien;
                                    cmd.Parameters.Add("@du", SqlDbType.Decimal).Value = soDuMoi;
                                    cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = nguoi;
                                    cmd.ExecuteNonQuery();
                                }
                                tx.Commit();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể nộp tiền: " + ex.Message, "Lỗi SQL",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    dlg.DialogResult = DialogResult.OK;
                    try
                    {
                        XuatHoaDon("HÓA ĐƠN NỘP TIỀN MẶT", stk, hoTen, "Nộp tiền mặt" + chiTiet, soTien, soDuMoi);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Đã nộp tiền thành công, nhưng không thể xuất hóa đơn: " + ex.Message);
                    }
                };
                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lblInfo, lblMG, pnlMenhGia, lblTong, btnOk, btnHuy });
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    HienThiDanhSach(txtCCCD.Text.Trim());
                    OnDataChanged();
                }
            }
        }

        private void MenuRutTien_Click(object sender, EventArgs e)
        {
            MoFormRutTienMat(false);
        }

        // Rút tiền vẫn có thể đưa số dư về 0 và giữ tài khoản hoạt động.
        // Chỉ khi chọn Đóng tài khoản, tài khoản mới chuyển trạng thái 2 vĩnh viễn.
        private void MoFormRutTienMat(bool uuTienDongTaiKhoan)
        {
            string stk = GetSelectedSTK();
            string hoTen = GetSelectedHoTen();
            if (string.IsNullOrWhiteSpace(stk) || !KiemTraTaiKhoanHoatDong(stk)) return;
            decimal soDu = GetSoDu(stk);
            if (soDu < 0) { MessageBox.Show("Số dư không hợp lệ."); return; }

            using (Form dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = uuTienDongTaiKhoan ? "Rút toàn bộ và đóng tài khoản" : "Rút tiền mặt";
                dlg.Size = new Size(510, 345);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.FromArgb(245, 245, 247);

                Label lblInfo = new Label
                {
                    Text = $"TK: {stk} - {hoTen}\nSố dư khả dụng: {soDu:N0} VNĐ",
                    Location = new Point(22, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                Label lblTien = new Label { Text = "Số tiền rút:", Location = new Point(22, 88), AutoSize = true };
                TextBox txtTien = new TextBox { Location = new Point(145, 84), Width = 285, Text = soDu.ToString("0") };
                Button btnRutHet = new Button { Text = "Rút hết số dư", Location = new Point(145, 119), Size = new Size(140, 32) };
                Label lblChuThich = new Label
                {
                    Text = "Rút hết mà không đóng: số dư = 0, tài khoản vẫn hoạt động.",
                    Location = new Point(22, 165),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(70, 85, 110)
                };
                Button btnRut = new Button { Text = "Xác nhận rút", Location = new Point(22, 209), Size = new Size(125, 38), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnDongTK = new Button { Text = "Đóng tài khoản", Location = new Point(160, 209), Size = new Size(145, 38), BackColor = colorRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(318, 209), Size = new Size(100, 38) };
                btnRutHet.Click += (s, e) => txtTien.Text = GetSoDu(stk).ToString("0");
                if (uuTienDongTaiKhoan) { txtTien.Text = soDu.ToString("0"); btnDongTK.Focus(); }

                Action<bool> xacNhan = dongTK =>
                {
                    decimal tienRut;
                    string digits = txtTien.Text.Trim().Replace(",", "").Replace(".", "").Replace(" ", "");
                    if (!decimal.TryParse(digits, out tienRut) || tienRut < 0 || (!dongTK && tienRut == 0))
                    { MessageBox.Show("Số tiền rút không hợp lệ."); return; }
                    if (dongTK && MessageBox.Show("Đóng tài khoản là vĩnh viễn, không thể mở lại. Hệ thống sẽ rút toàn bộ số dư còn lại. Tiếp tục?",
                        "Xác nhận đóng vĩnh viễn", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                    decimal soDuMoi;
                    decimal tienThucRut;
                    try
                    {
                        using (SqlConnection conn = GetConn())
                        {
                            conn.Open();
                            using (SqlTransaction tran = conn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                try
                                {
                                    decimal soDuThuc;
                                    int trangThai;
                                    decimal phongToa;
                                    using (SqlCommand cmd = new SqlCommand(@"SELECT SoDu, TrangThai, ISNULL(SoTienPhongToa,0)
                                        FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTT=@stk", conn, tran))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (!reader.Read()) throw new InvalidOperationException("Không tìm thấy tài khoản.");
                                            soDuThuc = Convert.ToDecimal(reader.GetValue(0));
                                            trangThai = Convert.ToInt32(reader.GetValue(1));
                                            phongToa = Convert.ToDecimal(reader.GetValue(2));
                                        }
                                    }
                                    if (trangThai != 0) throw new InvalidOperationException("Tài khoản đang khóa hoặc đã đóng, không thể giao dịch.");
                                    if (phongToa > 0) throw new InvalidOperationException("Số dư đang phong tỏa, cần gỡ phong tỏa trước.");
                                    tienThucRut = dongTK ? soDuThuc : tienRut;
                                    if (tienThucRut > soDuThuc) throw new InvalidOperationException("Số dư hiện tại không đủ.");
                                    if (!dongTK && tienThucRut <= 0) throw new InvalidOperationException("Số tiền rút phải lớn hơn 0.");
                                    soDuMoi = soDuThuc - tienThucRut;
                                    using (SqlCommand cmd = new SqlCommand(@"UPDATE dbo.TaiKhoanThanhToan
                                        SET SoDu=@du, TrangThai=@tt, SoTienPhongToa=0, NgayCapNhatTT=GETDATE()
                                        WHERE SoTKTT=@stk AND TrangThai=0", conn, tran))
                                    {
                                        cmd.Parameters.Add("@du", SqlDbType.Decimal).Value = soDuMoi;
                                        cmd.Parameters.Add("@tt", SqlDbType.Int).Value = dongTK ? 2 : 0;
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Cập nhật tài khoản thất bại.");
                                    }
                                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDich
                                        (SoTKTT, NoiDung, SoTien, SoDuSau, ThoiGian, NguoiThucHien)
                                         VALUES (@stk, @nd, @tien, @du, GETDATE(), @nguoi)", conn, tran))
                                    {
                                        cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value = dongTK
                                            ? "Rút toàn bộ tiền mặt và đóng tài khoản vĩnh viễn" : "Rút tiền mặt";
                                        cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = tienThucRut;
                                        cmd.Parameters.Add("@du", SqlDbType.Decimal).Value = soDuMoi;
                                        cmd.ExecuteNonQuery();
                                    }
                                    tran.Commit();
                                }
                                catch { tran.Rollback(); throw; }
                            }
                        }
                        MessageBox.Show(dongTK
                            ? $"Đã rút {tienThucRut:N0} VNĐ và ĐÓNG VĨNH VIỄN tài khoản {stk}."
                            : $"Đã rút {tienThucRut:N0} VNĐ. Số dư còn lại: {soDuMoi:N0} VNĐ. Tài khoản vẫn hoạt động.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if (tienThucRut > 0)
                            XuatHoaDon("HÓA ĐƠN RÚT TIỀN MẶT", stk, hoTen,
                                dongTK ? "Rút hết tiền và đóng tài khoản" : "Rút tiền mặt", tienThucRut, soDuMoi);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    { MessageBox.Show("Không thể xử lý giao dịch: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                };
                btnRut.Click += (s, e) => xacNhan(false);
                btnDongTK.Click += (s, e) => xacNhan(true);
                btnHuy.Click += (s, e) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lblInfo, lblTien, txtTien, btnRutHet, lblChuThich, btnRut, btnDongTK, btnHuy });
                if (dlg.ShowDialog(this) == DialogResult.OK)
                { HienThiDanhSach(txtCCCD.Text.Trim()); OnDataChanged(); }
            }
        }

        private void MenuChuyenTien_Click(object sender, EventArgs e)
        {
            // Menu chuột phải vẫn dùng tài khoản đang chọn trên bảng.
            MenuChuyenTienTheoSTK(GetSelectedSTK());
        }

        // QR nội bộ QLTKNH, không phải VietQR hoặc lệnh chuyển tiền.
        private void HienThiQrNhanTien(string stk)
        {
            if (!KiemTraTaiKhoanHoatDong(stk)) return;
            DataTable dt = ExecuteQuery(@"SELECT kh.HoTen FROM dbo.TaiKhoanThanhToan tk
                JOIN dbo.KhachHang kh ON kh.CCCD=tk.CCCD WHERE tk.SoTKTT=@stk AND tk.TrangThai=0",
                new SqlParameter("@stk", SqlDbType.VarChar, 20) { Value = stk });
            if (dt.Rows.Count == 0) return;
            using (var dlg = new Form
            {
                Text = "QR nhận tiền - QLTKNH",
                ClientSize = new Size(440, 520),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = colorDarkBg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            })
            {
                var title = new Label
                {
                    Text = "QR NHẬN TIỀN NỘI BỘ",
                    Bounds = new Rectangle(20, 12, 400, 35),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(147, 197, 253),
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold)
                };
                var pic = new PictureBox { Bounds = new Rectangle(100, 52, 240, 240), BackColor = Color.White, SizeMode = PictureBoxSizeMode.Zoom };
                var info = new Label
                {
                    Text = Convert.ToString(dt.Rows[0]["HoTen"]) + "\n" + stk,
                    Bounds = new Rectangle(20, 300, 400, 48),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                var label = new Label { Text = "Số tiền (VNĐ, có thể để trống)", Bounds = new Rectangle(25, 353, 390, 24) };
                var amount = new TextBox { Bounds = new Rectangle(25, 380, 390, 28), BackColor = colorCardBg, ForeColor = Color.White };
                var note = new Label { Text = "Chỉ sử dụng trong QLTKNH, không chuyển tiền thật.", Bounds = new Rectangle(25, 475, 390, 35), ForeColor = Color.FromArgb(148, 163, 184) };
                var create = new Button { Text = "Tạo QR", Bounds = new Rectangle(25, 425, 185, 40), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var save = new Button { Text = "Lưu ảnh PNG", Bounds = new Rectangle(230, 425, 185, 40), BackColor = colorCardBg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Action render = () =>
                {
                    try
                    {
                        string raw = amount.Text.Trim();
                        decimal value;
                        if (raw.Length > 0 && (!decimal.TryParse(raw, System.Globalization.NumberStyles.None,
                            System.Globalization.CultureInfo.InvariantCulture, out value) || value <= 0))
                        { MessageBox.Show(dlg, "Nhập số nguyên VNĐ dương, không dấu phân cách."); return; }
                        string payload = "QLTKNH:TT:CN:1|" + stk + "|" + raw;
                        var writer = new ZXing.BarcodeWriter
                        {
                            Format = ZXing.BarcodeFormat.QR_CODE,
                            Options = new ZXing.Common.EncodingOptions { Width = 360, Height = 360, Margin = 4 }
                        };
                        Bitmap image = writer.Write(payload);
                        Image old = pic.Image; pic.Image = image; if (old != null) old.Dispose();
                        save.Enabled = true;
                    }
                    catch (Exception ex) { MessageBox.Show(dlg, "Không tạo được QR: " + ex.Message); }
                };
                amount.TextChanged += (senderQR, argsQR) => { save.Enabled = false; Image old = pic.Image; pic.Image = null; if (old != null) old.Dispose(); };
                create.Click += (senderQR, argsQR) => render();
                save.Click += (senderQR, argsQR) =>
                {
                    if (pic.Image == null) return;
                    using (var file = new SaveFileDialog { Filter = "PNG (*.png)|*.png", FileName = "QR_" + stk + ".png" })
                        if (file.ShowDialog(dlg) == DialogResult.OK)
                            try { pic.Image.Save(file.FileName, System.Drawing.Imaging.ImageFormat.Png); }
                            catch (Exception ex) { MessageBox.Show(dlg, "Không lưu được ảnh: " + ex.Message); }
                };
                dlg.Controls.AddRange(new Control[] { title, pic, info, label, amount, create, save, note });
                try { render(); dlg.ShowDialog(this); }
                finally { if (pic.Image != null) { pic.Image.Dispose(); pic.Image = null; } }
            }
        }

        private bool DocQrNhanTien(IWin32Window owner, out string stk, out decimal? tien)
        {
            stk = null; tien = null;
            using (var file = new OpenFileDialog { Filter = "Ảnh QR (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp" })
            {
                if (file.ShowDialog(owner) != DialogResult.OK) return false;
                using (var bitmap = new Bitmap(file.FileName))
                {
                    var reader = new ZXing.BarcodeReader();
                    reader.Options.PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE };
                    var result = reader.Decode(bitmap);
                    string[] parts = result == null ? new string[0] : result.Text.Split('|');
                    if (parts.Length != 3 || parts[0] != "QLTKNH:TT:CN:1" || parts[1].Length == 0 || parts[1].Length > 20 ||
                        !parts[1].All(c => c >= '0' && c <= '9'))
                    { MessageBox.Show(owner, "QR không đúng định dạng thanh toán cá nhân QLTKNH."); return false; }
                    decimal value;
                    if (parts[2].Length > 0)
                    {
                        if (!decimal.TryParse(parts[2], System.Globalization.NumberStyles.None,
                            System.Globalization.CultureInfo.InvariantCulture, out value) || value <= 0)
                        { MessageBox.Show(owner, "Số tiền trong QR không hợp lệ."); return false; }
                        tien = value;
                    }
                    stk = parts[1]; return true;
                }
            }
        }

        private bool XacNhanChuyenTienCN(IWin32Window owner, string nguon, string tenNguon, string dich, string tenDich, decimal tien, string loaiChuyen, string nganHang)
        {
            Color navy = Color.FromArgb(15, 23, 42), card = Color.FromArgb(30, 41, 59);
            Color ink = Color.FromArgb(241, 245, 249), muted = Color.FromArgb(148, 163, 184), blue = Color.FromArgb(147, 197, 253);
            using (var f = new Form
            {
                Text = "Xác nhận chuyển tiền cá nhân",
                ClientSize = new Size(600, 590),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = navy,
                ForeColor = ink,
                AutoScaleMode = AutoScaleMode.Dpi
            })
            using (var normal = new Font("Segoe UI", 10F))
            using (var bold = new Font("Segoe UI", 11F, FontStyle.Bold))
            using (var large = new Font("Segoe UI", 23F, FontStyle.Bold))
            using (var heading = new Font("Segoe UI", 16F, FontStyle.Bold))
            {
                f.Font = normal;
                Action<Control, string, int, int, int, int, Color, Font> label = (parent, text, x, y, w, h, color, font) =>
                    parent.Controls.Add(new Label
                    {
                        Text = text,
                        Bounds = new Rectangle(x, y, w, h),
                        ForeColor = color,
                        Font = font,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoEllipsis = true
                    });
                f.Controls.Add(new Panel { Bounds = new Rectangle(0, 0, 600, 5), BackColor = Color.FromArgb(37, 99, 235) });
                label(f, "XÁC NHẬN CHUYỂN TIỀN", 24, 18, 552, 36, ink, heading);
                label(f, loaiChuyen + " / " + nganHang, 24, 55, 552, 25, blue, normal);
                var money = new Panel { Bounds = new Rectangle(24, 92, 552, 88), BackColor = card };
                label(money, "SỐ TIỀN CHUYỂN", 16, 8, 520, 23, muted, normal);
                label(money, tien.ToString("#,0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ", 16, 31, 520, 48, blue, large);
                f.Controls.Add(money);
                var from = new Panel { Bounds = new Rectangle(24, 192, 552, 108), BackColor = card };
                label(from, "01  /  TÀI KHOẢN NGUỒN", 16, 8, 520, 22, muted, normal);
                label(from, nguon, 16, 30, 520, 26, blue, bold);
                label(from, tenNguon, 16, 57, 520, 44, ink, normal);
                f.Controls.Add(from);
                label(f, "↓   CHUYỂN ĐẾN", 40, 302, 520, 30, blue, bold);
                var to = new Panel { Bounds = new Rectangle(24, 334, 552, 108), BackColor = card };
                label(to, "02  /  TÀI KHOẢN NHẬN", 16, 8, 520, 22, muted, normal);
                label(to, dich, 16, 30, 520, 26, blue, bold);
                label(to, tenDich, 16, 57, 520, 44, ink, normal);
                f.Controls.Add(to);
                label(f, "Kiểm tra kỹ người nhận và số tiền trước khi xác nhận.\nSố dư và hạn mức sẽ được kiểm tra khi thực hiện giao dịch.", 24, 452, 552, 56, muted, normal);
                var cancel = new Button
                {
                    Text = "Hủy giao dịch",
                    Bounds = new Rectangle(24, 526, 184, 44),
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(51, 65, 85),
                    ForeColor = ink,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                var ok = new Button
                {
                    Text = "XÁC NHẬN CHUYỂN TIỀN  →",
                    Bounds = new Rectangle(220, 526, 356, 44),
                    DialogResult = DialogResult.OK,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    Font = bold,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                cancel.FlatAppearance.BorderSize = 0; ok.FlatAppearance.BorderSize = 0;
                f.Controls.AddRange(new Control[] { cancel, ok }); f.CancelButton = cancel;
                f.Shown += (senderForm, argsForm) => cancel.Focus();
                return f.ShowDialog(owner) == DialogResult.OK;
            }
        }

        private void MenuChuyenTienTheoSTK(string stkNguon)
        {
            MenuChuyenTienTheoSTK(stkNguon, false);
        }

        private void MenuChuyenTienTheoSTK(string stkNguon, bool moQRNgay)
        {
            // Nút trong cửa sổ chi tiết phải dùng STK của chính cửa sổ đó,
            // không lấy CurrentRow (có thể đang chọn tài khoản khác).
            if (string.IsNullOrWhiteSpace(stkNguon) || !KiemTraTaiKhoanHoatDong(stkNguon)) return;
            DataTable thongTinNguon = ExecuteQuery(@"SELECT tk.SoDu, kh.HoTen
                FROM dbo.TaiKhoanThanhToan tk
                INNER JOIN dbo.KhachHang kh ON kh.CCCD = tk.CCCD
                WHERE tk.SoTKTT = @stk", new SqlParameter("@stk", stkNguon));
            if (thongTinNguon.Rows.Count == 0) { MessageBox.Show("Không tìm thấy tài khoản nguồn."); return; }
            string hoTenNguon = Convert.ToString(thongTinNguon.Rows[0]["HoTen"]);
            decimal soDuNguon = Convert.ToDecimal(thongTinNguon.Rows[0]["SoDu"]);

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = "Chuyển tiền";
                dlg.Size = new Size(460, 400);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                int y = 15;
                var l1 = new Label { Text = $"TK nguồn: {stkNguon} ({hoTenNguon})\nSố dư: {soDuNguon:N0} VNĐ", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                y += 50;

                var lblLoai = new Label { Text = "Loại chuyển:", Location = new Point(20, y), AutoSize = true };
                var cboLoai = new ComboBox { Location = new Point(150, y - 3), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
                cboLoai.Items.AddRange(new[] { "Trong hệ thống", "Ngoài hệ thống (CITAD/NAPAS 247)" });
                cboLoai.SelectedIndex = 0;
                y += 35;

                var lblNH = new Label { Text = "Ngân hàng nhận:", Location = new Point(20, y), AutoSize = true };
                var cboNH = new ComboBox { Location = new Point(150, y - 3), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
                cboNH.Items.AddRange(new object[] { "Softech Bank (Nội bộ)", "Vietcombank", "MBBank", "Techcombank", "VietinBank", "Agribank", "BIDV", "VPBank", "ACB" });
                cboNH.SelectedIndex = 0;
                y += 35;

                var lblSTK = new Label { Text = "STK nhận:", Location = new Point(20, y), AutoSize = true };
                var txtSTKDich = new TextBox { Location = new Point(150, y - 3), Width = 174 };
                var btnDocQR = new Button { Text = "Đọc QR", Location = new Point(330, y - 5), Size = new Size(80, 28), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                y += 35;

                var lblTen = new Label { Text = "Họ tên người nhận:", Location = new Point(20, y), AutoSize = true };
                var txtTen = new TextBox { Location = new Point(150, y - 3), Width = 260, TextAlign = HorizontalAlignment.Left, RightToLeft = RightToLeft.No };
                y += 35;

                var lblTien = new Label { Text = "Số tiền chuyển:", Location = new Point(20, y), AutoSize = true };
                var txtTien = new TextBox { Location = new Point(150, y - 3), Width = 260, Text = "500000" };
                y += 50;

                if (moQRNgay)
                    dlg.Shown += (senderQR, argsQR) => dlg.BeginInvoke(new Action(() => btnDocQR.PerformClick()));
                btnDocQR.Click += (senderQR, argsQR) =>
                {
                    try
                    {
                        string stkQR; decimal? tienQR;
                        if (!DocQrNhanTien(dlg, out stkQR, out tienQR)) return;
                        if (stkQR == stkNguon) { MessageBox.Show(dlg, "Không thể chuyển cho chính mình!"); return; }
                        DataTable dich = ExecuteQuery(@"SELECT kh.HoTen FROM dbo.TaiKhoanThanhToan tk
                            JOIN dbo.KhachHang kh ON tk.CCCD=kh.CCCD WHERE tk.SoTKTT=@stk AND tk.TrangThai=0",
                            new SqlParameter("@stk", SqlDbType.VarChar, 20) { Value = stkQR });
                        if (dich.Rows.Count == 0) { MessageBox.Show(dlg, "Tài khoản QR không tồn tại hoặc không hoạt động."); return; }
                        cboLoai.SelectedIndex = 0;
                        cboNH.SelectedIndex = 0;
                        txtSTKDich.Text = stkQR;
                        txtTen.Text = Convert.ToString(dich.Rows[0]["HoTen"]);
                        txtTen.Select(0, 0);
                        txtTien.Text = tienQR.HasValue ? tienQR.Value.ToString("0", System.Globalization.CultureInfo.InvariantCulture) : "";
                        txtTien.Focus();
                        MessageBox.Show(dlg, "Đã đọc QR. Kiểm tra người nhận và số tiền trước khi nhấn Chuyển tiền.");
                    }
                    catch (Exception ex) { MessageBox.Show(dlg, "Không thể đọc QR: " + ex.Message); }
                };

                txtSTKDich.Leave += (s, ev) =>
                {
                    string stk = txtSTKDich.Text.Trim();
                    if (string.IsNullOrEmpty(stk)) return;
                    object ten = ExecuteScalar(@"SELECT kh.HoTen FROM TaiKhoanThanhToan tk JOIN KhachHang kh ON tk.CCCD = kh.CCCD WHERE tk.SoTKTT = @stk", new SqlParameter("@stk", stk));
                    if (ten != null) { cboLoai.SelectedIndex = 0; cboNH.SelectedIndex = 0; txtTen.Text = ten.ToString(); txtTen.Select(0, 0); }
                };

                var btnOk = new Button { Text = "Chuyển tiền", Location = new Point(150, y), Size = new Size(120, 36), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnHuy = new Button { Text = "Hủy", Location = new Point(290, y), Size = new Size(100, 36) };

                btnOk.Click += (s, ev) =>
                {
                    string stkDich = txtSTKDich.Text.Trim();
                    string hoTenDich = txtTen.Text.Trim();
                    string nganHang = cboNH.SelectedItem?.ToString() ?? "";
                    bool noiBo = cboLoai.SelectedIndex == 0;

                    if (string.IsNullOrEmpty(stkDich) || string.IsNullOrEmpty(hoTenDich)) { MessageBox.Show("Vui lòng nhập đầy đủ thông tin!"); return; }
                    if (stkDich == stkNguon && noiBo) { MessageBox.Show("Không thể chuyển cho chính mình!"); return; }
                    if (!decimal.TryParse(txtTien.Text.Replace(",", ""), out decimal soTien) || soTien <= 0) { MessageBox.Show("Số tiền không hợp lệ!"); return; }
                    // Kiểm tra trên SQL trong transaction ở dưới, không dùng số dư cũ
                    // đọc lúc mở hộp thoại để từ chối giao dịch.

                    if (noiBo)
                    {
                        DataTable nguoiNhan = ExecuteQuery(@"SELECT kh.HoTen FROM dbo.TaiKhoanThanhToan tk
                            JOIN dbo.KhachHang kh ON tk.CCCD=kh.CCCD WHERE tk.SoTKTT=@stk AND tk.TrangThai=0",
                            new SqlParameter("@stk", SqlDbType.VarChar, 20) { Value = stkDich });
                        if (nguoiNhan.Rows.Count == 0) { MessageBox.Show(dlg, "Tài khoản nhận không tồn tại hoặc không hoạt động."); return; }
                        hoTenDich = Convert.ToString(nguoiNhan.Rows[0]["HoTen"]);
                        txtTen.Text = hoTenDich;
                        txtTen.Select(0, 0);
                    }
                    decimal soDuMoiNguon;
                    string loaiGD = noiBo ? "Chuyển nội bộ" : "Chuyển ngoài (CITAD/NAPAS)";
                    if (!XacNhanChuyenTienCN(dlg, stkNguon, hoTenNguon, stkDich, hoTenDich, soTien, loaiGD, nganHang)) return;
                    try
                    {
                        using (SqlConnection cn = GetConn())
                        {
                            cn.Open();
                            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                try
                                {
                                    decimal duNguon;
                                    using (SqlCommand cmd = new SqlCommand(@"SELECT SoDu FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK,HOLDLOCK)
                                        WHERE SoTKTT=@stk AND TrangThai=0 AND ISNULL(SoTienPhongToa,0)=0", cn, tx))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkNguon;
                                        object v = cmd.ExecuteScalar();
                                        if (v == null) throw new InvalidOperationException("Tài khoản nguồn đã khóa hoặc đóng.");
                                        duNguon = Convert.ToDecimal(v);
                                    }
                                    if (duNguon < soTien)
                                        throw new InvalidOperationException($"Số dư tài khoản nguồn không đủ. Hiện có: {duNguon:N0} VNĐ.");

                                    // Hạn mức được lấy từ tài khoản NGUỒN, không phải dòng đang chọn.
                                    // Chỉ tính các giao dịch chuyển tiền đi đã ghi nhận trong ngày.
                                    decimal hanMucLan, hanMucNgay;
                                    using (SqlCommand cmd = new SqlCommand(@"SELECT HanMucLan, HanMucNgay
                                        FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk", cn, tx))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkNguon;
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            if (!reader.Read()) throw new InvalidOperationException("Không tìm thấy hạn mức tài khoản nguồn.");
                                            hanMucLan = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                                            hanMucNgay = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                                        }
                                    }
                                    if (hanMucLan > 0 && soTien > hanMucLan)
                                        throw new InvalidOperationException($"Vượt hạn mức mỗi lần ({hanMucLan:N0} VNĐ).");
                                    decimal daChuyenHomNay;
                                    using (SqlCommand cmd = new SqlCommand(@"SELECT ISNULL(SUM(ABS(SoTien)),0)
                                        FROM dbo.LichSuGiaoDich WITH (UPDLOCK,HOLDLOCK)
                                        WHERE SoTKTT=@stk AND ThoiGian >= CONVERT(date,GETDATE())
                                          AND ThoiGian < DATEADD(day,1,CONVERT(date,GETDATE()))
                                          AND (NoiDung LIKE N'Chuyển nội bộ đến %'
                                               OR NoiDung LIKE N'Chuyển ngoài (CITAD/NAPAS) đến %')", cn, tx))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkNguon;
                                        daChuyenHomNay = Convert.ToDecimal(cmd.ExecuteScalar());
                                    }
                                    if (hanMucNgay > 0 && daChuyenHomNay + soTien > hanMucNgay)
                                        throw new InvalidOperationException($"Vượt hạn mức chuyển tiền trong ngày ({hanMucNgay:N0} VNĐ). Đã chuyển: {daChuyenHomNay:N0} VNĐ; còn: {Math.Max(0, hanMucNgay - daChuyenHomNay):N0} VNĐ.");
                                    soDuMoiNguon = duNguon - soTien;
                                    using (SqlCommand cmd = new SqlCommand(@"UPDATE dbo.TaiKhoanThanhToan
                                        SET SoDu=SoDu-@t, NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                                    {
                                        cmd.Parameters.Add("@t", SqlDbType.Decimal).Value = soTien;
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkNguon;
                                        if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không thể trừ tiền tài khoản nguồn.");
                                    }
                                    if (noiBo)
                                    {
                                        decimal duDich;
                                        using (SqlCommand cmd = new SqlCommand(@"SELECT SoDu FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK,HOLDLOCK)
                                            WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                                        {
                                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkDich;
                                            object v = cmd.ExecuteScalar();
                                            if (v == null) throw new InvalidOperationException("Tài khoản nhận không tồn tại, đang khóa hoặc đã đóng.");
                                            duDich = Convert.ToDecimal(v);
                                        }
                                        // Cộng tiền trực tiếp vào tài khoản nhận và lấy số dư
                                        // sau cập nhật từ SQL, không dùng số dư lưu tạm trên form.
                                        using (SqlCommand cmd = new SqlCommand(@"UPDATE dbo.TaiKhoanThanhToan
                                            SET SoDu=SoDu+@t, NgayCapNhatTT=GETDATE()
                                            OUTPUT INSERTED.SoDu
                                            WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                                        {
                                            cmd.Parameters.Add("@t", SqlDbType.Decimal).Value = soTien;
                                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkDich;
                                            object soDuDichSau = cmd.ExecuteScalar();
                                            if (soDuDichSau == null || soDuDichSau == DBNull.Value)
                                                throw new InvalidOperationException("Không thể cộng tiền tài khoản nhận.");
                                            duDich = Convert.ToDecimal(soDuDichSau) - soTien;
                                        }
                                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDich(SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
                                             VALUES(@stk,@nd,@t,@du,GETDATE(),@nguoi)", cn, tx))
                                        {
                                            cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkDich;
                                            cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value = $"Nhận chuyển khoản từ {stkNguon}";
                                            cmd.Parameters.Add("@t", SqlDbType.Decimal).Value = soTien;
                                            cmd.Parameters.Add("@du", SqlDbType.Decimal).Value = duDich + soTien;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDich(SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
                                             VALUES(@stk,@nd,@t,@du,GETDATE(),@nguoi)", cn, tx))
                                    {
                                        cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkNguon;
                                        cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value = $"{loaiGD} đến {hoTenDich} ({stkDich} - {nganHang})";
                                        cmd.Parameters.Add("@t", SqlDbType.Decimal).Value = soTien;
                                        cmd.Parameters.Add("@du", SqlDbType.Decimal).Value = soDuMoiNguon;
                                        cmd.ExecuteNonQuery();
                                    }
                                    tx.Commit();
                                }
                                catch { tx.Rollback(); throw; }
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Chuyển tiền thất bại: " + ex.Message); return; }
                    XuatHoaDon("HÓA ĐƠN CHUYỂN TIỀN", stkNguon, hoTenNguon, $"{loaiGD}\r\nNgười nhận: {hoTenDich}\r\nSTK: {stkDich}\r\nNH: {nganHang}", soTien, soDuMoiNguon);
                    dlg.DialogResult = DialogResult.OK;
                };

                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { l1, lblLoai, cboLoai, lblNH, cboNH, lblSTK, txtSTKDich, btnDocQR, lblTen, txtTen, lblTien, txtTien, btnOk, btnHuy });
                if (dlg.ShowDialog() == DialogResult.OK) { HienThiDanhSach(txtCCCD.Text.Trim()); OnDataChanged(); }
            }
        }

        #endregion

        #region 3. Kiểm soát trạng thái

        private void MenuKhoaMoKhanCap_Click(object sender, EventArgs e)
        {
            string stk = GetSelectedSTK();
            if (string.IsNullOrWhiteSpace(stk)) return;
            DataTable dt = ExecuteQuery("SELECT SoDu, TrangThai, ISNULL(SoTienPhongToa,0) AS PhongToa FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk",
                new SqlParameter("@stk", stk));
            if (dt.Rows.Count == 0) return;
            int trangThai = Convert.ToInt32(dt.Rows[0]["TrangThai"]);
            decimal soDu = Convert.ToDecimal(dt.Rows[0]["SoDu"]);
            if (trangThai == 2)
            { MessageBox.Show("Tài khoản đã đóng vĩnh viễn, không thể mở lại."); return; }
            if (trangThai == 1)
            {
                if (MessageBox.Show($"Mở khóa tài khoản {stk} và gỡ phong tỏa toàn bộ số dư?",
                    "Mở khóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                if (ThayDoiKhoaVaPhongToa(stk, false, ""))
                { MessageBox.Show("Đã mở khóa và gỡ phong tỏa."); HienThiDanhSach(txtCCCD.Text.Trim()); OnDataChanged(); }
                return;
            }
            using (Form dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = "Tạm khóa / Phong tỏa số dư - " + stk;
                dlg.Size = new Size(455, 265);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                Label lbl = new Label
                {
                    Text = $"Số dư hiện tại: {soDu:N0} VNĐ\nKhóa tài khoản sẽ phong tỏa toàn bộ số dư và dừng giao dịch.",
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                Label lblLyDo = new Label { Text = "Lý do tạm khóa:", Location = new Point(20, 88), AutoSize = true };
                TextBox txtLyDo = new TextBox { Location = new Point(20, 112), Size = new Size(390, 44), Multiline = true };
                Button btnOk = new Button { Text = "Tạm khóa tài khoản", Location = new Point(90, 175), Size = new Size(155, 34), BackColor = colorOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(260, 175), Size = new Size(95, 34) };
                btnOk.Click += (senderKhoa, argsKhoa) =>
                {
                    if (string.IsNullOrWhiteSpace(txtLyDo.Text)) { MessageBox.Show("Vui lòng nhập lý do."); return; }
                    if (ThayDoiKhoaVaPhongToa(stk, true, txtLyDo.Text.Trim())) dlg.DialogResult = DialogResult.OK;
                };
                btnHuy.Click += (senderHuy, argsHuy) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lbl, lblLyDo, txtLyDo, btnOk, btnHuy });
                if (dlg.ShowDialog(this) == DialogResult.OK)
                { HienThiDanhSach(txtCCCD.Text.Trim()); OnDataChanged(); }
            }
        }

        private bool ThayDoiKhoaVaPhongToa(string stk, bool khoa, string lyDo)
        {
            try
            {
                using (SqlConnection cn = GetConn())
                {
                    cn.Open();
                    using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(@"UPDATE dbo.TaiKhoanThanhToan
                                SET TrangThai=@moi, SoTienPhongToa=CASE WHEN @moi=1 THEN SoDu ELSE 0 END,
                                    NgayCapNhatTT=GETDATE()
                                WHERE SoTKTT=@stk AND TrangThai=@cu", cn, tx))
                            {
                                cmd.Parameters.Add("@moi", SqlDbType.Int).Value = khoa ? 1 : 0;
                                cmd.Parameters.Add("@cu", SqlDbType.Int).Value = khoa ? 0 : 1;
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Trạng thái tài khoản đã thay đổi hoặc tài khoản đã đóng.");
                            }
                            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDich
                                (SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
                                 SELECT SoTKTT,@nd,NULL,SoDu,GETDATE(),@nguoi FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk", cn, tx))
                            {
                                cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value = khoa
                                    ? "Tạm khóa và phong tỏa toàn bộ số dư. Lý do: " + lyDo : "Mở khóa và gỡ phong tỏa toàn bộ số dư";
                                cmd.ExecuteNonQuery();
                            }
                            tx.Commit();
                        }
                        catch { tx.Rollback(); throw; }
                    }
                }
                return true;
            }
            catch (Exception ex)
            { MessageBox.Show("Không thể thay đổi trạng thái: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
        }

        // Giữ tên hàm cũ để tương thích với sự kiện Designer hoặc code khác.
        private void MenuPhongToa_Click(object sender, EventArgs e) => MenuKhoaMoKhanCap_Click(sender, e);

        // Khi số dư bằng 0, đóng trực tiếp (sau khi xác nhận).
        // Khi còn tiền, mở form rút toàn bộ số dư và đóng như trước.
        private void MenuDongTK_Click(object sender, EventArgs e)
        {
            string stk = GetSelectedSTK();
            if (string.IsNullOrWhiteSpace(stk) || !KiemTraTaiKhoanHoatDong(stk)) return;

            decimal soDu = GetSoDu(stk);
            if (soDu < 0)
            {
                MessageBox.Show("Số dư tài khoản không hợp lệ.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (soDu > 0)
            {
                // Giữ nguyên form rút tiền có nút Đóng tài khoản.
                MoFormRutTienMat(true);
                return;
            }

            if (MessageBox.Show(
                "Tài khoản " + stk + " hiện có số dư 0 VNĐ.\n" +
                "Bạn có chắc muốn ĐÓNG VĨNH VIỄN tài khoản này không?\n" +
                "Sau khi đóng, tài khoản không thể mở lại hay thực hiện giao dịch.",
                "Xác nhận đóng tài khoản", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                using (SqlConnection cn = GetConn())
                {
                    cn.Open();
                    using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            // Kiểm tra lại ngay trong transaction để không đóng nhầm
                            // nếu tài khoản vừa phát sinh số dư hoặc bị khóa.
                            decimal soDuHienTai;
                            decimal phongToa;
                            int trangThai;
                            using (SqlCommand cmd = new SqlCommand(@"
                                SELECT SoDu, ISNULL(SoTienPhongToa, 0), TrangThai
                                FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK, HOLDLOCK)
                                WHERE SoTKTT=@stk", cn, tx))
                            {
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                        throw new InvalidOperationException("Không tìm thấy tài khoản.");
                                    soDuHienTai = Convert.ToDecimal(reader.GetValue(0));
                                    phongToa = Convert.ToDecimal(reader.GetValue(1));
                                    trangThai = Convert.ToInt32(reader.GetValue(2));
                                }
                            }

                            if (trangThai != 0)
                                throw new InvalidOperationException("Tài khoản đang khóa hoặc đã đóng.");
                            if (soDuHienTai != 0 || phongToa != 0)
                                throw new InvalidOperationException(
                                    "Số dư hoặc tiền phong tỏa đã thay đổi. Vui lòng kiểm tra lại trước khi đóng.");

                            using (SqlCommand cmd = new SqlCommand(@"
                                UPDATE dbo.TaiKhoanThanhToan
                                SET TrangThai=2, SoTienPhongToa=0, NgayCapNhatTT=GETDATE()
                                WHERE SoTKTT=@stk AND TrangThai=0 AND SoDu=0
                                  AND ISNULL(SoTienPhongToa,0)=0", cn, tx))
                            {
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                if (cmd.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("Không thể đóng tài khoản.");
                            }

                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO dbo.LichSuGiaoDich
                                    (SoTKTT, NoiDung, SoTien, SoDuSau, ThoiGian, NguoiThucHien)
                                VALUES (@stk, @nd, @tien, 0, GETDATE(), @nguoi)", cn, tx))
                            {
                                cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                cmd.Parameters.Add("@nd", SqlDbType.NVarChar, 500).Value =
                                    "Đóng tài khoản vĩnh viễn (số dư trước khi đóng: 0 VNĐ)";
                                cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = 0m;
                                cmd.ExecuteNonQuery();
                            }
                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                MessageBox.Show("Đã đóng vĩnh viễn tài khoản " + stk +
                    ".\nSố dư: 0 VNĐ. Tài khoản được giữ lại để xem lịch sử.",
                    "Đóng tài khoản thành công", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                HienThiDanhSach(txtCCCD.Text.Trim());
                OnDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đóng tài khoản: " + ex.Message,
                    "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        #region 4. Dịch vụ

        private void MenuDangKyDichVu_Click(object sender, EventArgs e)
        {
            string stk = GetSelectedSTK();
            string hoTen = GetSelectedHoTen();
            if (string.IsNullOrEmpty(stk) || !KiemTraTaiKhoanHoatDong(stk)) return;

            DataTable dtDV = ExecuteQuery("SELECT TenDichVu, TrangThai FROM DichVuDangKy WHERE SoTKTT = @stk", new SqlParameter("@stk", stk));

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = $"Đăng ký dịch vụ - {stk} ({hoTen})";
                dlg.Size = new Size(500, 340);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                var lblInfo = new Label { Text = $"Tài khoản: {stk} | Chủ: {hoTen}", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                var chkSMS = new CheckBox { Text = "SMS Banking", Location = new Point(30, 55), AutoSize = true };
                var chkDigital = new CheckBox { Text = "Digital / Mobile Banking", Location = new Point(30, 90), AutoSize = true };
                var chkThe = new CheckBox { Text = "Phát hành Thẻ ghi nợ (Debit Card)", Location = new Point(30, 125), AutoSize = true };
                var chkAutoDebit = new CheckBox { Text = "Trích nợ tự động (Auto-debit)", Location = new Point(30, 160), AutoSize = true };

                foreach (DataRow r in dtDV.Rows)
                {
                    string ten = r["TenDichVu"].ToString();
                    bool active = r["TrangThai"].ToString() == "Đang hoạt động";
                    if (ten.Contains("SMS")) chkSMS.Checked = active;
                    if (ten.Contains("Digital") || ten.Contains("Mobile")) chkDigital.Checked = active;
                    if (ten.Contains("Thẻ") || ten.Contains("Debit")) chkThe.Checked = active;
                    if (ten.Contains("Auto") || ten.Contains("trích nợ")) chkAutoDebit.Checked = active;
                }

                var btnOk = new Button { Text = "Lưu đăng ký", Location = new Point(150, 220), Size = new Size(130, 36), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnHuy = new Button { Text = "Hủy", Location = new Point(300, 220), Size = new Size(100, 36) };

                btnOk.Click += (s, ev) =>
                {
                    ExecuteNonQuery("DELETE FROM DichVuDangKy WHERE SoTKTT = @stk", new SqlParameter("@stk", stk));
                    void ThemDV(string ten, bool checked_)
                    {
                        if (!checked_) return;
                        ExecuteNonQuery("INSERT INTO DichVuDangKy (SoTKTT, TenDichVu, TrangThai, NgayDangKy) VALUES (@stk, @ten, N'Đang hoạt động', GETDATE())",
                            new SqlParameter("@stk", stk), new SqlParameter("@ten", ten));
                    }
                    ThemDV("SMS Banking", chkSMS.Checked);
                    ThemDV("Digital/Mobile Banking", chkDigital.Checked);
                    ThemDV("Thẻ ghi nợ Debit Card", chkThe.Checked);
                    ThemDV("Trích nợ tự động Auto-debit", chkAutoDebit.Checked);
                    GhiLichSu(stk, "Cập nhật đăng ký dịch vụ đi kèm");
                    MessageBox.Show("Đã lưu đăng ký dịch vụ!");
                    dlg.DialogResult = DialogResult.OK;
                };
                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lblInfo, chkSMS, chkDigital, chkThe, chkAutoDebit, btnOk, btnHuy });
                if (dlg.ShowDialog() == DialogResult.OK) OnDataChanged();
            }
        }

        #endregion

        #region 5. Lịch sử & Sao kê

        // Menu chuột phải: vẫn sử dụng tài khoản của dòng đang chọn.
        private void MenuXemLichSu_Click(object sender, EventArgs e)
        {
            XemLichSuTheoSTK(GetSelectedSTK());
        }

        // Cửa sổ chi tiết: dùng STK cố định của cửa sổ, không phụ thuộc CurrentRow.
        private void XemLichSuTheoSTK(string stk)
        {
            if (string.IsNullOrWhiteSpace(stk)) return;
            object tenChu = ExecuteScalar(@"SELECT kh.HoTen
                FROM dbo.TaiKhoanThanhToan tk
                INNER JOIN dbo.KhachHang kh ON kh.CCCD = tk.CCCD
                WHERE tk.SoTKTT = @stk", new SqlParameter("@stk", stk));
            if (tenChu == null || tenChu == DBNull.Value)
            {
                MessageBox.Show("Không tìm thấy tài khoản " + stk + " trong SQL.");
                return;
            }
            string hoTen = Convert.ToString(tenChu);

            DataTable dt = ExecuteQuery(@"SELECT ThoiGian, NoiDung, SoTien, SoDuSau FROM LichSuGiaoDich WHERE SoTKTT = @stk ORDER BY ThoiGian DESC", new SqlParameter("@stk", stk));

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = $"Lịch sử giao dịch - {stk} ({hoTen})";
                dlg.Size = new Size(900, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;

                var dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = colorDarkBg,
                    ForeColor = Color.White,
                    ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = colorCardBg, ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold) },
                    DefaultCellStyle = new DataGridViewCellStyle { BackColor = colorDarkBg, ForeColor = Color.White, SelectionBackColor = colorBlue },
                    RowHeadersVisible = false
                };
                dgv.EnableHeadersVisualStyles = false;
                dgv.DataSource = dt;

                if (dgv.Columns.Count >= 4)
                {
                    dgv.Columns["ThoiGian"].HeaderText = "Thời gian";
                    dgv.Columns["NoiDung"].HeaderText = "Nội dung giao dịch";
                    dgv.Columns["SoTien"].HeaderText = "Số tiền";
                    dgv.Columns["SoDuSau"].HeaderText = "Số dư sau";
                    dgv.Columns["ThoiGian"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    dgv.Columns["SoTien"].DefaultCellStyle.Format = "N0";
                    dgv.Columns["SoDuSau"].DefaultCellStyle.Format = "N0";
                }

                if (dt.Rows.Count == 0) MessageBox.Show($"Tài khoản {stk} chưa có lịch sử giao dịch.");
                dlg.Controls.Add(dgv);
                dlg.ShowDialog();
            }
        }

        // Menu chuột phải: giữ nguyên cách chọn tài khoản từ bảng.
        private void MenuInSaoKe_Click(object sender, EventArgs e)
        {
            InSaoKeTheoSTK(GetSelectedSTK());
        }

        // Cửa sổ chi tiết: mọi truy vấn sao kê và xuất PDF đều dùng STK của cửa sổ.
        private void InSaoKeTheoSTK(string stk)
        {
            if (string.IsNullOrWhiteSpace(stk)) return;
            object tenChu = ExecuteScalar(@"SELECT kh.HoTen
                FROM dbo.TaiKhoanThanhToan tk
                INNER JOIN dbo.KhachHang kh ON kh.CCCD = tk.CCCD
                WHERE tk.SoTKTT = @stk", new SqlParameter("@stk", stk));
            if (tenChu == null || tenChu == DBNull.Value)
            {
                MessageBox.Show("Không tìm thấy tài khoản " + stk + " trong SQL.");
                return;
            }
            string hoTen = Convert.ToString(tenChu);

            using (var dlg = new Form())
            {
                dlg.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(dlg);
                dlg.Text = "In sao kê tài khoản";
                dlg.Size = new Size(500, 300);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                var lblTu = new Label { Text = "Từ ngày:", Location = new Point(30, 30), AutoSize = true };
                var dtpTu = new DateTimePicker { Location = new Point(120, 27), Width = 150, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
                dtpTu.Value = DateTime.Today.AddMonths(-1);
                var lblDen = new Label { Text = "Đến ngày:", Location = new Point(30, 70), AutoSize = true };
                var dtpDen = new DateTimePicker { Location = new Point(120, 67), Width = 150, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };

                var btnIn = new Button { Text = "Xem / In sao kê", Location = new Point(120, 170), Size = new Size(140, 36), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnHuy = new Button { Text = "Hủy", Location = new Point(280, 170), Size = new Size(100, 36) };
                var btnPdf = new Button { Text = "Xuất PDF", Location = new Point(120, 215), Size = new Size(140, 32), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnPdf.Click += (s, ev) => XuatPhieuThanhToanHtml(stk, dtpTu.Value.Date, dtpDen.Value.Date);

                btnIn.Click += (s, ev) =>
                {
                    DataTable dt = ExecuteQuery(@"SELECT ThoiGian, NoiDung, SoTien, SoDuSau FROM LichSuGiaoDich 
                        WHERE SoTKTT = @stk AND CAST(ThoiGian AS DATE) BETWEEN @tu AND @den ORDER BY ThoiGian",
                        new SqlParameter("@stk", stk), new SqlParameter("@tu", dtpTu.Value.Date), new SqlParameter("@den", dtpDen.Value.Date));

                    using (var frmSK = new Form())
                    {
                        frmSK.Shown += (senderMau, argsMau) => DongBoMauFormPhuThanhToan(frmSK);
                        frmSK.Text = $"Sao kê tài khoản {stk}";
                        frmSK.Size = new Size(700, 520);
                        frmSK.StartPosition = FormStartPosition.CenterParent;

                        var txt = new TextBox { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5F), ScrollBars = ScrollBars.Both };
                        var sb = new StringBuilder();
                        sb.AppendLine("============================================================");
                        sb.AppendLine("           NGÂN HÀNG SOFTECH BANK - SAO KÊ TÀI KHOẢN");
                        sb.AppendLine("============================================================");
                        sb.AppendLine($"Chủ tài khoản : {hoTen}");
                        sb.AppendLine($"Số tài khoản  : {stk}");
                        sb.AppendLine($"Kỳ sao kê     : {dtpTu.Value:dd/MM/yyyy} → {dtpDen.Value:dd/MM/yyyy}");
                        sb.AppendLine($"Ngày in       : {DateTime.Now:dd/MM/yyyy HH:mm}");
                        sb.AppendLine("------------------------------------------------------------");
                        sb.AppendLine($"{"Thời gian",-20} {"Nội dung",-35} {"Số tiền",15} {"Số dư sau",15}");
                        sb.AppendLine("------------------------------------------------------------");
                        foreach (DataRow r in dt.Rows)
                        {
                            string tg = Convert.ToDateTime(r["ThoiGian"]).ToString("dd/MM/yyyy HH:mm");
                            string nd = r["NoiDung"].ToString();
                            if (nd.Length > 33) nd = nd.Substring(0, 30) + "...";
                            string st = r["SoTien"] == DBNull.Value ? "" : Convert.ToDecimal(r["SoTien"]).ToString("N0");
                            string sd = r["SoDuSau"] == DBNull.Value ? "" : Convert.ToDecimal(r["SoDuSau"]).ToString("N0");
                            sb.AppendLine($"{tg,-20} {nd,-35} {st,15} {sd,15}");
                        }
                        sb.AppendLine("------------------------------------------------------------");
                        sb.AppendLine("        Cảm ơn quý khách đã sử dụng dịch vụ Softech Bank!");
                        sb.AppendLine("============================================================");
                        txt.Text = sb.ToString();
                        frmSK.Controls.Add(txt);
                        frmSK.ShowDialog();
                    }
                    dlg.DialogResult = DialogResult.OK;
                };
                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { lblTu, dtpTu, lblDen, dtpDen, btnIn, btnHuy, btnPdf });
                dlg.ShowDialog();
            }
        }

        #endregion

        // Xuất HTML theo mẫu phiếu tiết kiệm; mở trình duyệt và chọn In > Save as PDF.
        // Không thay đổi giao diện Designer hay dữ liệu SQL.
        private static string HtmlSafe(object value) => WebUtility.HtmlEncode(Convert.ToString(value) ?? "");

        private void XuatPhieuThanhToanHtml(string stk, DateTime tuNgay, DateTime denNgay)
        {
            if (tuNgay.Date > denNgay.Date)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
                return;
            }

            // Truy vấn riêng để lấy đúng chủ tài khoản, kể cả khi chọn dòng đồng sở hữu.
            DataTable kh = ExecuteQuery(@"SELECT TOP 1 kh.HoTen, kh.CCCD, kh.SoDienThoai,
                       tk.SoTKTT, tk.SoDu, tk.NgayMo, tk.TrangThai, tk.LoaiTienTe
                FROM TaiKhoanThanhToan tk INNER JOIN KhachHang kh ON kh.CCCD = tk.CCCD
                WHERE tk.SoTKTT = @stk", new SqlParameter("@stk", stk));
            if (kh.Rows.Count == 0)
            {
                MessageBox.Show("Không tìm thấy thông tin tài khoản trong SQL.");
                return;
            }
            DataRow k = kh.Rows[0];
            DataTable history = ExecuteQuery(@"SELECT ThoiGian, NoiDung, SoTien, SoDuSau
                FROM LichSuGiaoDich WHERE SoTKTT = @stk AND ThoiGian >= @tu AND ThoiGian < @den
                ORDER BY ThoiGian ASC",
                new SqlParameter("@stk", stk),
                new SqlParameter("@tu", tuNgay.Date),
                new SqlParameter("@den", denNgay.Date.AddDays(1)));

            var rows = new StringBuilder();
            foreach (DataRow r in history.Rows)
            {
                string amount = r["SoTien"] == DBNull.Value ? "-" : Convert.ToDecimal(r["SoTien"]).ToString("N0") + " VNĐ";
                string balance = r["SoDuSau"] == DBNull.Value ? "-" : Convert.ToDecimal(r["SoDuSau"]).ToString("N0") + " VNĐ";
                rows.Append("<tr><td>").Append(Convert.ToDateTime(r["ThoiGian"]).ToString("dd/MM/yyyy HH:mm"))
                    .Append("</td><td>").Append(HtmlSafe(r["NoiDung"]))
                    .Append("</td><td class='right'>").Append(amount)
                    .Append("</td><td class='right'>").Append(balance).Append("</td></tr>");
            }
            if (history.Rows.Count == 0) rows.Append("<tr><td colspan='4' class='empty'>Không có giao dịch trong khoảng thời gian này.</td></tr>");

            string status = Convert.ToString(k["TrangThai"]);
            if (int.TryParse(status, out int code)) status = MapTrangThai(code);
            string html = @"<!doctype html><html lang='vi'><head><meta charset='utf-8'><title>Sao kê thanh toán</title>
<style>
*{box-sizing:border-box;-webkit-print-color-adjust:exact;print-color-adjust:exact}
body{margin:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;color:#1e293b}
.page{max-width:790px;margin:30px auto;background:white;padding:40px;border:1px solid #e2e8f0;border-radius:12px}
.head{display:flex;justify-content:space-between;gap:20px;border-bottom:2px solid #2563eb;padding-bottom:20px}
.brand{font-size:19px;font-weight:700;color:#0f172a}.sub,.meta,.muted{font-size:12px;color:#64748b}
h2{text-align:center;font-size:20px;margin:30px 0 5px}.subtitle{text-align:center;color:#64748b;font-size:13px;margin-bottom:25px}
.grid{display:grid;grid-template-columns:1fr 1fr;gap:18px}.card{background:#f8fafc;border:1px solid #e2e8f0;padding:17px;border-radius:8px}
.card h3,.section{color:#2563eb;font-size:13px;border-bottom:1px solid #cbd5e1;padding-bottom:9px;text-transform:uppercase}
.info{display:flex;justify-content:space-between;gap:12px;font-size:12px;margin:9px 0}.info b{text-align:right;overflow-wrap:anywhere}
table{width:100%;border-collapse:collapse;font-size:12px;margin-top:12px;table-layout:fixed}th{background:#f1f5f9;text-align:left;border-bottom:2px solid #cbd5e1}td,th{padding:10px 8px;border-bottom:1px solid #e2e8f0;overflow-wrap:anywhere}.right{text-align:right}.empty{text-align:center;color:#64748b}
.sign{display:grid;grid-template-columns:1fr 1fr;text-align:center;margin-top:45px;font-size:13px}.space{height:80px}
@page{size:A4;margin:14mm}@media print{body{background:white}.page{margin:0;padding:0;max-width:none;border:0;border-radius:0}.card,tr{break-inside:avoid}}
</style></head><body><main class='page'><header class='head'><div><div class='brand'>NGÂN HÀNG THƯƠNG MẠI QLTKNH</div><div class='sub'>Hệ thống quản lý tài khoản thanh toán</div></div><div class='meta'>Mã phiếu: <b>{{MA}}</b><br>Ngày xuất: {{NGAY}}</div></header>
<h2>SAO KÊ TÀI KHOẢN THANH TOÁN</h2><div class='subtitle'>Chi tiết tài khoản và lịch sử giao dịch</div>
<div class='grid'><section class='card'><h3>Thông tin khách hàng</h3><div class='info'><span>Họ tên</span><b>{{TEN}}</b></div><div class='info'><span>CCCD</span><b>{{CCCD}}</b></div><div class='info'><span>Số điện thoại</span><b>{{SDT}}</b></div></section>
<section class='card'><h3>Thông tin tài khoản</h3><div class='info'><span>Số tài khoản</span><b>{{STK}}</b></div><div class='info'><span>Số dư hiện tại</span><b>{{SODU}}</b></div><div class='info'><span>Trạng thái</span><b>{{TT}}</b></div><div class='info'><span>Ngày mở</span><b>{{MO}}</b></div><div class='info'><span>Tiền tệ</span><b>{{TIENTE}}</b></div></section></div>
<h3 class='section' style='margin-top:28px'>Lịch sử giao dịch: {{TU}} - {{DEN}}</h3><table><thead><tr><th style='width:24%'>Thời gian</th><th>Nội dung</th><th style='width:20%' class='right'>Số tiền</th><th style='width:22%' class='right'>Số dư sau</th></tr></thead><tbody>{{ROWS}}</tbody></table>
<div class='sign'><div><b>Khách hàng</b><p class='muted'>(Ký và ghi rõ họ tên)</p><div class='space'></div><b>{{TEN}}</b></div><div><b>Giao dịch viên xác nhận</b><p class='muted'>(Ký và đóng dấu khi xác nhận)</p><div class='space'></div><b>Hệ thống QLTKNH</b></div></div></main></body></html>";
            html = html.Replace("{{MA}}", "SK-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                .Replace("{{NGAY}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                .Replace("{{TEN}}", HtmlSafe(k["HoTen"]))
                .Replace("{{CCCD}}", HtmlSafe(k["CCCD"]))
                .Replace("{{SDT}}", HtmlSafe(k["SoDienThoai"]))
                .Replace("{{STK}}", HtmlSafe(stk))
                .Replace("{{SODU}}", Convert.ToDecimal(k["SoDu"]).ToString("N0") + " VNĐ")
                .Replace("{{TT}}", HtmlSafe(status))
                .Replace("{{MO}}", Convert.ToDateTime(k["NgayMo"]).ToString("dd/MM/yyyy"))
                .Replace("{{TIENTE}}", HtmlSafe(k["LoaiTienTe"]))
                .Replace("{{TU}}", tuNgay.ToString("dd/MM/yyyy"))
                .Replace("{{DEN}}", denNgay.ToString("dd/MM/yyyy"))
                .Replace("{{ROWS}}", rows.ToString());

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Trang in PDF (*.html)|*.html";
                sfd.Title = "Xuất sao kê để in / lưu PDF";
                sfd.FileName = "SaoKe_" + stk + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".html";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllText(sfd.FileName, html, new UTF8Encoding(false));
                    if (MessageBox.Show("Đã lưu mẫu sao kê. Mở trong trình duyệt để chọn Ctrl+P → Save as PDF?",
                        "Xuất sao kê", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xuất sao kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Hóa đơn

        // Xuất hóa đơn riêng cho giao dịch vừa thành công; không thay đổi xử lý tiền/SQL.
        // Tương tự mẫu sao kê hiện có: mở HTML trong trình duyệt để In -> Save as PDF.
        private void XuatHoaDon(string loaiHoaDon, string stk, string hoTen,
            string noiDung, decimal soTien, decimal soDuConLai)
        {
            if (MessageBox.Show("Giao dịch thành công. Bạn có muốn in hóa đơn PDF cho giao dịch này?",
                "In hóa đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DateTime ngayGiaoDich = DateTime.Now;
            string maHoaDon = "HD-" + ngayGiaoDich.ToString("yyyyMMddHHmmssfff");
            string html = @"<!doctype html><html lang='vi'><head><meta charset='utf-8'>
<title>Hóa đơn giao dịch</title><style>
*{box-sizing:border-box;-webkit-print-color-adjust:exact;print-color-adjust:exact}
body{background:#f1f5f9;color:#1e293b;font-family:'Segoe UI',Arial,sans-serif;margin:0}
.page{max-width:790px;margin:30px auto;background:white;padding:42px;border:1px solid #e2e8f0;border-radius:12px}
.head{display:flex;justify-content:space-between;border-bottom:2px solid #2563eb;padding-bottom:20px;gap:20px}
.brand{font-size:20px;font-weight:700;color:#0f172a}.muted{color:#64748b;font-size:12px}
h2{text-align:center;margin:32px 0 8px;font-size:21px}
.subtitle{text-align:center;color:#64748b;font-size:13px;margin-bottom:28px}
.card{background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:20px}
.line{display:flex;justify-content:space-between;gap:18px;margin:13px 0;font-size:14px}
.line b{text-align:right;overflow-wrap:anywhere;white-space:pre-line}
.total{border-top:2px solid #cbd5e1;padding-top:18px;margin-top:18px;color:#2563eb;font-size:17px}
.sign{display:grid;grid-template-columns:1fr 1fr;text-align:center;margin-top:55px;font-size:13px}
.space{height:75px}
@page{size:A4;margin:14mm}
@media print{body{background:white}.page{margin:0;padding:0;max-width:none;border:0;border-radius:0}}
</style></head><body><main class='page'>
<header class='head'><div><div class='brand'>NGÂN HÀNG THƯƠNG MẠI QLTKNH</div>
<div class='muted'>Hệ thống quản lý tài khoản thanh toán</div></div>
<div class='muted'>Mã hóa đơn: <b>{{MA}}</b><br>Ngày giao dịch: {{NGAY}}</div></header>
<h2>{{LOAI}}</h2><div class='subtitle'>Chứng từ cho một giao dịch đã thực hiện</div>
<section class='card'>
<div class='line'><span>Chủ tài khoản</span><b>{{TEN}}</b></div>
<div class='line'><span>Số tài khoản</span><b>{{STK}}</b></div>
<div class='line'><span>Nội dung giao dịch</span><b>{{NOIDUNG}}</b></div>
<div class='line total'><span>Số tiền giao dịch</span><b>{{SOTIEN}}</b></div>
<div class='line'><span>Số dư sau giao dịch</span><b>{{SODU}}</b></div>
<div class='line'><span>Trạng thái</span><b>Thành công</b></div>
</section>
<div class='sign'><div><b>Khách hàng</b><p class='muted'>(Ký, ghi rõ họ tên)</p><div class='space'></div><b>{{TEN}}</b></div>
<div><b>Giao dịch viên</b><p class='muted'>(Ký xác nhận)</p><div class='space'></div><b>Hệ thống QLTKNH</b></div></div>
</main></body></html>";
            html = html.Replace("{{MA}}", HtmlSafe(maHoaDon))
                .Replace("{{NGAY}}", ngayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"))
                .Replace("{{LOAI}}", HtmlSafe(loaiHoaDon))
                .Replace("{{TEN}}", HtmlSafe(hoTen))
                .Replace("{{STK}}", HtmlSafe(stk))
                .Replace("{{NOIDUNG}}", HtmlSafe(noiDung))
                .Replace("{{SOTIEN}}", soTien.ToString("N0") + " VNĐ")
                .Replace("{{SODU}}", soDuConLai.ToString("N0") + " VNĐ");

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Lưu hóa đơn để in / xuất PDF";
                sfd.Filter = "Trang hóa đơn (*.html)|*.html";
                sfd.FileName = "HoaDon_" + stk + "_" + ngayGiaoDich.ToString("yyyyMMdd_HHmmss") + ".html";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllText(sfd.FileName, html, new UTF8Encoding(false));
                    if (MessageBox.Show(
                        "Đã lưu hóa đơn. Mở trong trình duyệt và nhấn Ctrl+P, chọn Save as PDF để lưu PDF?",
                        "In hóa đơn PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xuất hóa đơn: " + ex.Message,
                        "Lỗi xuất hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion


        // Chỉ trang trí giao diện các cửa sổ phụ. Không thay đổi sự kiện, dữ liệu,
        // trạng thái Enabled/ReadOnly, kích thước, vị trí hay nghiệp vụ SQL.
        private void DongBoMauFormPhuThanhToan(Form cuaSo)
        {
            if (cuaSo == null || cuaSo.IsDisposed || cuaSo.WindowState == FormWindowState.Maximized)
                return;
            Color nen = Color.FromArgb(15, 23, 42);
            Color the = Color.FromArgb(30, 41, 59);
            Color oNhap = Color.FromArgb(39, 53, 75);
            Color chu = Color.FromArgb(241, 245, 249);
            Color phu = Color.FromArgb(148, 163, 184);
            Color xanh = Color.FromArgb(37, 99, 235);
            Color xanhLa = Color.FromArgb(34, 197, 94);
            Color doNhat = Color.FromArgb(248, 113, 113);
            cuaSo.BackColor = nen;
            cuaSo.ForeColor = chu;
            cuaSo.Font = new Font("Segoe UI", 9.5F);
            DongBoMauDieuKhienThanhToan(cuaSo, nen, the, oNhap, chu, phu, xanh, xanhLa, doNhat);
        }

        private void DongBoMauDieuKhienThanhToan(Control cha, Color nen, Color the,
            Color oNhap, Color chu, Color phu, Color xanh, Color xanhLa, Color doNhat)
        {
            foreach (Control c in cha.Controls)
            {
                if (c is Button)
                {
                    Button b = (Button)c;
                    string ten = (b.Text ?? "").ToLowerInvariant();
                    bool huy = ten.Contains("hủy") || ten.Contains("huỷ") || ten.Contains("đóng") || ten.Contains("thoát");
                    bool nguyHiem = ten.Contains("xóa") || ten.Contains("khoá") || ten.Contains("khóa") || ten.Contains("tất toán");
                    bool thanhCong = ten.Contains("nạp") || ten.Contains("lưu") || ten.Contains("xác nhận") || ten.Contains("mở khóa");
                    b.UseVisualStyleBackColor = false;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    b.BackColor = huy ? Color.FromArgb(71, 85, 105) : nguyHiem ? Color.FromArgb(185, 28, 28) : thanhCong ? Color.FromArgb(21, 128, 61) : xanh;
                    b.ForeColor = Color.White;
                    b.Font = new Font("Segoe UI", b.Font.Size, FontStyle.Bold);
                    b.Cursor = b.Enabled ? Cursors.Hand : Cursors.Default;
                }
                else if (c is TextBox)
                {
                    TextBox t = (TextBox)c;
                    t.BackColor = t.ReadOnly ? the : oNhap;
                    t.ForeColor = t.ReadOnly ? Color.FromArgb(147, 197, 253) : chu;
                    t.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox)
                {
                    ComboBox cb = (ComboBox)c;
                    cb.BackColor = oNhap;
                    cb.ForeColor = chu;
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is NumericUpDown)
                {
                    c.BackColor = oNhap;
                    c.ForeColor = chu;
                }
                else if (c is DateTimePicker)
                {
                    c.BackColor = oNhap;
                    c.ForeColor = chu;
                }
                else if (c is DataGridView)
                {
                    DataGridView grid = (DataGridView)c;
                    grid.BackgroundColor = the;
                    grid.GridColor = Color.FromArgb(51, 65, 85);
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 99, 235);
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    grid.DefaultCellStyle.BackColor = the;
                    grid.DefaultCellStyle.ForeColor = chu;
                    grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 64, 175);
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(37, 50, 71);
                    grid.AlternatingRowsDefaultCellStyle.ForeColor = chu;
                }
                else if (c is GroupBox)
                {
                    c.BackColor = the;
                    c.ForeColor = Color.FromArgb(147, 197, 253);
                }
                else if (c is Panel || c is TableLayoutPanel || c is FlowLayoutPanel)
                {
                    c.BackColor = the;
                    c.ForeColor = chu;
                }
                else if (c is Label)
                {
                    Label l = (Label)c;
                    l.ForeColor = l.Font.Bold ? Color.FromArgb(147, 197, 253) : chu;
                    if (l.Text != null && (l.Text.Contains("VNĐ") || l.Text.Contains("Số dư")))
                        l.ForeColor = xanhLa;
                }
                else if (c is RadioButton || c is CheckBox)
                {
                    c.ForeColor = chu;
                    c.BackColor = c.Parent != null ? c.Parent.BackColor : nen;
                }
                if (c.HasChildren)
                    DongBoMauDieuKhienThanhToan(c, nen, the, oNhap, chu, phu, xanh, xanhLa, doNhat);
            }
        }

        private void dgvThanhToan_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}