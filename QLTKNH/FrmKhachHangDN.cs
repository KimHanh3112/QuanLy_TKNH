using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace QLTKNH
{
    public partial class FrmKhachHangDN : Form
    {
        private readonly Color colorBg = Color.FromArgb(248, 250, 252);
        private readonly Color colorDark = Color.FromArgb(15, 23, 42);
        private readonly Color colorBlue = Color.FromArgb(37, 99, 235);
        private readonly Color colorBlueLight = Color.FromArgb(239, 246, 255);
        private readonly Color colorBorder = Color.FromArgb(226, 232, 240);
        private readonly Color colorText = Color.FromArgb(30, 41, 59);
        private readonly Color colorMuted = Color.FromArgb(100, 116, 139);
        private readonly Color colorGreen = Color.FromArgb(22, 163, 74);
        private readonly Color colorRed = Color.FromArgb(220, 38, 38);

        private readonly string _connStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";
        private IList danhSachKhachHang;
        private readonly Dictionary<string, int> _nhomNoSql = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // Liên kết người được ủy quyền theo số tài khoản, tránh dùng chung mọi người được ủy quyền của chủ.
        private readonly Dictionary<string, List<FrmMainDN.DongSoHuuData>> _dongSoHuuTheoSoTK =
            new Dictionary<string, List<FrmMainDN.DongSoHuuData>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _loaiCoBangDongSoHuuRieng =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly List<Tuple<int, double>> _kyHanVay = new List<Tuple<int, double>>();
        private readonly List<Tuple<int, double>> _kyHanTietKiem = new List<Tuple<int, double>>();
        private object khachHangDangChon;
        private string cccdMoTuFrmMain;
        private bool _taiDuLieuThanhCong = false;
        private bool _dangThaoTacMenu = false;
        private bool _dangKiemTraLuaChon = false;
        private bool _daHenKiemTraLuaChon = false;
        private bool _dangKhoiTaoDanhSach = true;
        private bool _boQuaKichHoatDau = true;
        private readonly System.Windows.Forms.Timer _timerLocNhanh = new System.Windows.Forms.Timer { Interval = 300 };
        private readonly Font _fontVaiTroDungChung = new Font("Segoe UI", 9F, FontStyle.Bold);
        private readonly Dictionary<Tuple<Type, string>, PropertyInfo> _cacheThuocTinh = new Dictionary<Tuple<Type, string>, PropertyInfo>();
        public event EventHandler DataChanged;

        public FrmKhachHangDN() : this(null, "") { }

        public FrmKhachHangDN(FrmMainDN frmMain, string cccd) : this(cccd)
        {
            if (frmMain != null) this.Owner = frmMain;
        }

        public FrmKhachHangDN(string cccd)
        {
            cccdMoTuFrmMain = cccd ?? "";
            InitializeComponent();
            if (dgvKhachHang.Columns.Contains("colCCCD"))
                dgvKhachHang.Columns["colCCCD"].HeaderText = "CCCD / MST";
            lblCCCD.Text = "CCCD / MST:";
            dgvKhachHang.Columns["colVaiTro"].MinimumWidth = 150;
            // Mỗi loại tài khoản có ý nghĩa thống kê khác nhau.
            colDaTraDaNhan.HeaderText = "Tổng đã trả / Tổng đã nhận";
            colSoDuDuNo.HeaderText = "Số dư / Dư nợ / Gốc còn lại";
            ApDungMauDanhSachTietKiem();
            ApDungMauToanForm();
            try { typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dgvKhachHang, true, null); }
            catch (Exception) { }
            _timerLocNhanh.Tick += (senderLoc, argsLoc) => { _timerLocNhanh.Stop(); if (!IsDisposed && !Disposing) HienThiDanhSach(txtCCCD.Text.Trim()); };
            Disposed += (senderLoc, argsLoc) => { _timerLocNhanh.Dispose(); _fontVaiTroDungChung.Dispose(); };

            this.Load += FrmKhachHang_Load;
            // Tự động tải lại và hiển thị khi chuyển tab/form quay lại FrmKhachHangDN
            this.Activated += (s, e) =>
            {
                if (_dangThaoTacMenu || _dangKhoiTaoDanhSach) return;
                if (_boQuaKichHoatDau) { _boQuaKichHoatDau = false; return; }
                LayDanhSachKhachHangTuFrmMain();
                if (_taiDuLieuThanhCong) HienThiDanhSach(txtCCCD.Text.Trim());
            };

            txtCCCD.TextChanged += TxtCCCD_TextChanged;
            txtCCCD.KeyPress += TxtCCCD_KeyPress;

            // Giữ nguyên thứ tự nhóm: chủ tài khoản trước, người được ủy quyền sau.
            foreach (DataGridViewColumn col in dgvKhachHang.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            colHoTen.HeaderText = "Doanh nghiệp / Họ tên";
            dgvKhachHang.CellFormatting += (senderGrid, argsGrid) =>
            {
                if (argsGrid.RowIndex < 0 || argsGrid.ColumnIndex != colHoTen.Index) return;
                var cell = dgvKhachHang.Rows[argsGrid.RowIndex].Cells[colHoTen.Index];
                if (cell.Tag is string prefix)
                {
                    argsGrid.Value = prefix + Convert.ToString(argsGrid.Value);
                    argsGrid.FormattingApplied = true;
                }
            };
            dgvKhachHang.CellClick += DgvKhachHang_CellClick;
            dgvKhachHang.CellDoubleClick += DgvKhachHang_CellDoubleClick;
            dgvKhachHang.MouseDown += DgvKhachHang_MouseDown;
            dgvKhachHang.SelectionChanged += DgvKhachHang_KiemTraLuaChon;
            dgvKhachHang.CurrentCellChanged += DgvKhachHang_KiemTraLuaChon;

            btnTimCCCD.Click += BtnTimCCCD_Click;
            panelSearch.MouseDown += PanelSearch_MouseDown;
            lblCCCD.Click += (s, e) => { txtCCCD.Clear(); txtCCCD.Focus(); };

            cboLocLoaiTK.SelectedIndexChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
            cboLocTrangThai.SelectedIndexChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
            cboLocNhomNo.SelectedIndexChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
            cboLocThoiGian.SelectedIndexChanged += CboLocThoiGian_SelectedIndexChanged;

            dtpTuNgay.ValueChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
            dtpDenNgay.ValueChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
        }

        // Chỉ đồng bộ màu DataGridView theo FrmTietKiemDN, không thay đổi bố cục hay xử lý.
        private void ApDungMauDanhSachTietKiem()
        {
            Color nen = Color.FromArgb(15, 23, 42);
            Color nenXenKe = Color.FromArgb(23, 33, 60);
            Color tieuDe = Color.FromArgb(30, 41, 59);
            Color xanhDuong = Color.FromArgb(37, 99, 235);
            Color chu = Color.FromArgb(241, 245, 249);

            dgvKhachHang.BackgroundColor = nen;
            dgvKhachHang.GridColor = xanhDuong;
            dgvKhachHang.EnableHeadersVisualStyles = false;
            dgvKhachHang.ColumnHeadersDefaultCellStyle.BackColor = tieuDe;
            dgvKhachHang.ColumnHeadersDefaultCellStyle.ForeColor = chu;
            dgvKhachHang.ColumnHeadersDefaultCellStyle.SelectionBackColor = tieuDe;
            dgvKhachHang.ColumnHeadersDefaultCellStyle.SelectionForeColor = chu;
            dgvKhachHang.RowHeadersDefaultCellStyle.BackColor = tieuDe;
            dgvKhachHang.RowHeadersDefaultCellStyle.ForeColor = chu;
            dgvKhachHang.DefaultCellStyle.BackColor = nen;
            dgvKhachHang.DefaultCellStyle.ForeColor = chu;
            dgvKhachHang.DefaultCellStyle.SelectionBackColor = xanhDuong;
            dgvKhachHang.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvKhachHang.AlternatingRowsDefaultCellStyle.BackColor = nenXenKe;
            dgvKhachHang.AlternatingRowsDefaultCellStyle.ForeColor = chu;
            dgvKhachHang.AlternatingRowsDefaultCellStyle.SelectionBackColor = xanhDuong;
            dgvKhachHang.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
        }

        // Chỉ thay đổi thuộc tính màu của giao diện hiện hữu; không thay đổi bố cục hoặc sự kiện.
        private void ApDungMauToanForm()
        {
            Color nen = Color.FromArgb(15, 23, 42);
            Color nenPanel = Color.FromArgb(30, 41, 59);
            Color nenPhu = Color.FromArgb(23, 33, 60);
            Color chu = Color.FromArgb(241, 245, 249);
            Color chuPhu = Color.FromArgb(160, 174, 192);
            Color xanh = Color.FromArgb(37, 99, 235);

            this.BackColor = nen;
            panelHeader.BackColor = nenPanel;
            panelSearch.BackColor = nenPhu;
            panelFilter.BackColor = nenPhu;
            panelInfo.BackColor = nenPanel;
            panelGrid.BackColor = nen;
            panelBottom.BackColor = nenPanel;

            lblTitle.ForeColor = chu;
            lblSubTitle.ForeColor = chuPhu;
            lblCCCD.ForeColor = chu;
            lblFilterLoai.ForeColor = chu;
            lblFilterTrangThai.ForeColor = chu;
            lblFilterNhomNo.ForeColor = chu;
            lblFilterThoiGian.ForeColor = chu;
            lblTuNgay.ForeColor = chu;
            lblDenNgay.ForeColor = chu;
            lblGridTitle.ForeColor = chu;
            lblTongTaiKhoan.ForeColor = chu;
            lblSoKetQua.ForeColor = Color.FromArgb(96, 165, 250);
            lblTrangThai.ForeColor = Color.FromArgb(34, 197, 94);
            lblKhachHangDangChon.ForeColor = chu;
            lblChiTiet.ForeColor = chuPhu;

            txtCCCD.BackColor = nenPanel;
            txtCCCD.ForeColor = chu;
            btnTimCCCD.BackColor = xanh;
            btnTimCCCD.ForeColor = Color.White;

            foreach (ComboBox cbo in new[] { cboLocLoaiTK, cboLocTrangThai, cboLocNhomNo, cboLocThoiGian })
            {
                cbo.BackColor = nenPanel;
                cbo.ForeColor = chu;
            }
            foreach (DateTimePicker dtp in new[] { dtpTuNgay, dtpDenNgay })
            {
                dtp.CalendarMonthBackground = nenPanel;
                dtp.CalendarForeColor = chu;
                dtp.CalendarTitleBackColor = xanh;
                dtp.CalendarTitleForeColor = Color.White;
            }
        }

        private void CboLocThoiGian_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool laTuyChon = cboLocThoiGian.SelectedIndex == 7;
            lblTuNgay.Visible = laTuyChon;
            dtpTuNgay.Visible = laTuyChon;
            lblDenNgay.Visible = laTuyChon;
            dtpDenNgay.Visible = laTuyChon;

            HienThiDanhSach(txtCCCD.Text.Trim());
        }

        private bool KiemTraTrangThaiFilter(object taiKhoan)
        {
            if (cboLocTrangThai.SelectedIndex <= 0) return true;
            string tt = LayTrangThai(taiKhoan);
            int sel = cboLocTrangThai.SelectedIndex;

            if (sel == 1) return tt == "Hoạt động";
            if (sel == 2) return tt == "Đang khóa" || tt == "Khóa khẩn cấp" || tt == "Khoa";
            if (sel == 3) return tt == "Đã đóng" || tt == "Đóng" || tt == "Dong";

            return true;
        }

        private bool KiemTraNhomNoFilter(object taiKhoan, string cccd)
        {
            if (cboLocNhomNo.SelectedIndex <= 0) return true;

            string nhomNo = LayNhomNo(taiKhoan, cccd);
            int sel = cboLocNhomNo.SelectedIndex;

            if (sel == 1) return nhomNo.Contains("1") || nhomNo.Contains("Đủ tiêu chuẩn");
            if (sel == 2) return nhomNo.Contains("2") || nhomNo.Contains("Cần chú ý");
            if (sel == 3) return nhomNo.Contains("3") || nhomNo.Contains("Dưới tiêu chuẩn");
            if (sel == 4) return nhomNo.Contains("4") || nhomNo.Contains("Nghi ngờ");
            if (sel == 5) return nhomNo.Contains("5") || nhomNo.Contains("Mất vốn");

            return true;
        }

        private DateTime? LayNgayLamDateTime(object taiKhoan)
        {
            object value = LayGiaTri(taiKhoan, "NgayLam") ?? LayGiaTri(taiKhoan, "NgayCapNhatTT") ?? LayGiaTri(taiKhoan, "NgayCapNhatTrangThai") ?? LayGiaTri(taiKhoan, "NgayMo");
            if (value is DateTime dt) return dt;
            if (value != null && DateTime.TryParse(value.ToString(), out DateTime parsed)) return parsed;
            return null;
        }

        private bool KiemTraThoiGianFilter(object taiKhoan)
        {
            if (cboLocThoiGian.SelectedIndex <= 0) return true;
            DateTime? dtObj = LayNgayLamDateTime(taiKhoan);
            if (!dtObj.HasValue) return false;

            DateTime dt = dtObj.Value.Date;
            DateTime today = DateTime.Today;

            switch (cboLocThoiGian.SelectedIndex)
            {
                case 1: return dt == today;
                case 2: return dt == today.AddDays(-1);
                case 3: return dt >= today.AddDays(-7) && dt <= today;
                case 4: return dt >= today.AddDays(-30) && dt <= today;
                case 5: return dt >= today.AddMonths(-3) && dt <= today;
                case 6: return dt >= today.AddYears(-1) && dt <= today;
                case 7: return dt >= dtpTuNgay.Value.Date && dt <= dtpDenNgay.Value.Date;
                default: return true;
            }
        }

        private void PanelSearch_MouseDown(object sender, MouseEventArgs e)
        {
            Control child = panelSearch.GetChildAtPoint(e.Location);
            if (child == null || child == lblCCCD)
            {
                txtCCCD.Clear();
                txtCCCD.Focus();
            }
        }

        private void FrmKhachHang_Load(object sender, EventArgs e)
        {
            _dangKhoiTaoDanhSach = true;
            try
            {
                cboLocLoaiTK.SelectedIndex = 0;
                cboLocTrangThai.SelectedIndex = 0;
                cboLocNhomNo.SelectedIndex = 0;
                cboLocThoiGian.SelectedIndex = 0;
                LayDanhSachKhachHangTuFrmMain();
                txtCCCD.Focus();
                if (!string.IsNullOrWhiteSpace(cccdMoTuFrmMain)) txtCCCD.Text = cccdMoTuFrmMain;
            }
            finally { _dangKhoiTaoDanhSach = false; }
            if (_taiDuLieuThanhCong) HienThiDanhSach(txtCCCD.Text.Trim());
            CapNhatThongTinTongQuan();
        }

        // Chỉ lấy dữ liệu thực tế từ SQL, không sử dụng listKhachHang của FrmMainDN.
        private void LayDanhSachKhachHangTuFrmMain()
        {
            _taiDuLieuThanhCong = false;
            var ds = new List<FrmMainDN.KhachHangData>();
            var index = new Dictionary<string, FrmMainDN.KhachHangData>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT CCCD, HoTen, SoDienThoai, NoTruocDo, NgaySinh FROM dbo.DN_KhachHang", cn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            var kh = new FrmMainDN.KhachHangData
                            {
                                CCCD = r.GetString(0),
                                HoTen = r.GetString(1),
                                SoDienThoai = r.GetString(2),
                                NoTruocDo = r.GetDecimal(3),
                                NgaySinh = r.IsDBNull(4) ? DateTime.MinValue : r.GetDateTime(4)
                            };
                            ds.Add(kh); index.Add(kh.CCCD, kh);
                        }
                    using (var cmd = new SqlCommand("SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai FROM (SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai, ROW_NUMBER() OVER (PARTITION BY CCCD_ChuSoHuu, CCCD ORDER BY Id DESC) AS rn FROM dbo.DN_DongSoHuu WHERE CCCD<>CCCD_ChuSoHuu) d WHERE rn=1", cn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            if (index.TryGetValue(r.GetString(0), out var kh))
                                kh.DanhSachDongSoHuu.Add(new FrmMainDN.DongSoHuuData
                                {
                                    CCCD = r.GetString(1),
                                    HoTen = r.GetString(2),
                                    SoDienThoai = r.GetString(3)
                                });
                    using (var cmd = new SqlCommand(@"SELECT v.SoTKVay, v.CCCD, v.SoTienVay, v.SoTienDaTra, v.LaiSuat, v.KyHan, v.TrangThai, v.NgayMo, v.NgayCapNhatTrangThai,
                       v.SoTienDaTra + ISNULL(h.LaiVaPhi,0) AS TongTienDaThanhToan
                       FROM dbo.DN_TaiKhoanVay v
                       LEFT JOIN (SELECT SoTKVay, SUM(ISNULL(SoTienLai,0)+ISNULL(PhiPhat,0)) AS LaiVaPhi
                                  FROM dbo.DN_LichSuGiaoDichVay WHERE LoaiGiaoDich <> N'Giải ngân' GROUP BY SoTKVay) h
                       ON h.SoTKVay=v.SoTKVay", cn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            if (index.TryGetValue(r.GetString(1), out var kh))
                                kh.DanhSachVay.Add(new FrmMainDN.TaiKhoanVayItem
                                {
                                    SoTKVay = r.GetString(0),
                                    SoTienVay = r.GetDecimal(2),
                                    SoTienDaTra = r.GetDecimal(3),
                                    TongTienDaThanhToan = Convert.ToDecimal(r["TongTienDaThanhToan"]),
                                    LaiSuat = r.GetDouble(4),
                                    KyHan = r.GetString(5),
                                    TrangThai = (FrmMainDN.TrangThaiTK)Convert.ToInt32(r.GetValue(6)),
                                    NgayMo = r.GetDateTime(7),
                                    NgayCapNhatTrangThai = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8)
                                });
                    using (var cmd = new SqlCommand("SELECT SoTKTK, CCCD, SoTienGui, SoTienDaRut, LaiSuat, KyHan, TrangThai, NgayMo, NgayCapNhatTT, ISNULL(TongTienDaNhan,0) AS TongTienDaNhan FROM dbo.DN_SoTietKiem", cn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            if (index.TryGetValue(r.GetString(1), out var kh))
                                kh.DanhSachTietKiem.Add(new FrmMainDN.SoTietKiemItem
                                {
                                    SoTKTK = r.GetString(0),
                                    SoTienGui = r.GetDecimal(2),
                                    SoTienDaRut = r.GetDecimal(3),
                                    TongTienDaNhan = Convert.ToDecimal(r["TongTienDaNhan"]),
                                    LaiSuat = r.GetDouble(4),
                                    KyHan = r.GetString(5),
                                    TrangThai = (FrmMainDN.TrangThaiTK)Convert.ToInt32(r.GetValue(6)),
                                    NgayMo = r.GetDateTime(7),
                                    NgayCapNhatTT = r.IsDBNull(8) ? (DateTime?)null : r.GetDateTime(8)
                                });
                    using (var cmd = new SqlCommand("SELECT SoTKTT, CCCD, SoDu, TrangThai, NgayMo FROM dbo.DN_TaiKhoanThanhToan", cn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            if (index.TryGetValue(r.GetString(1), out var kh))
                                kh.DanhSachThanhToan.Add(new FrmMainDN.TaiKhoanThanhToanItem
                                {
                                    SoTKTT = r.GetString(0),
                                    SoDu = r.GetDecimal(2),
                                    TrangThai = (FrmMainDN.TrangThaiTK)Convert.ToInt32(r.GetValue(3)),
                                    NgayMo = r.GetDateTime(4)
                                });
                    // Các bảng liên kết theo STK có thể chỉ tồn tại ở một số phiên bản CSDL.
                    // Có bảng riêng => chỉ lấy người được ủy quyền gắn với đúng STK.
                    _dongSoHuuTheoSoTK.Clear();
                    _loaiCoBangDongSoHuuRieng.Clear();
                    DocDongSoHuuTheoSTK(cn, "Tiết kiệm", "dbo.DN_DongSoHuuSoTietKiem", "SoTKTK");
                    DocDongSoHuuTheoSTK(cn, "Thanh toán", "dbo.DN_DongSoHuuTaiKhoanThanhToan", "SoTKTT");
                    DocDongSoHuuTheoSTK(cn, "Khoản vay", "dbo.DN_DongSoHuuTaiKhoanVay", "SoTKVay");

                    _nhomNoSql.Clear();
                    using (var cmd = new SqlCommand("IF OBJECT_ID('dbo.DN_NhomNoTaiKhoan','U') IS NOT NULL SELECT SoTaiKhoan, NhomNo FROM dbo.DN_NhomNoTaiKhoan", cn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) _nhomNoSql[r.GetString(0)] = r.GetInt32(1);
                }
                danhSachKhachHang = ds;
                _taiDuLieuThanhCong = true;
            }
            catch (Exception ex)
            {
                // Không xóa dữ liệu đang hiển thị khi SQL gặp lỗi.
                MessageBox.Show("Không thể tải khách hàng từ SQL Server: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DocDongSoHuuTheoSTK(SqlConnection cn, string loai,
            string tenBang, string tenCotSo)
        {
            // Tên bảng/cột cố định trong code, không nhận từ ô nhập người dùng.
            using (var exists = new SqlCommand("SELECT OBJECT_ID(@tenBang, 'U')", cn))
            {
                exists.Parameters.AddWithValue("@tenBang", tenBang);
                if (exists.ExecuteScalar() == DBNull.Value) return;
            }
            _loaiCoBangDongSoHuuRieng.Add(loai);
            using (var cmd = new SqlCommand(
                "SELECT " + tenCotSo + ", CCCD, HoTen, SoDienThoai FROM " + tenBang, cn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    string key = loai + "|" + Convert.ToString(r[0]);
                    if (!_dongSoHuuTheoSoTK.TryGetValue(key, out var ds))
                    {
                        ds = new List<FrmMainDN.DongSoHuuData>();
                        _dongSoHuuTheoSoTK[key] = ds;
                    }
                    ds.Add(new FrmMainDN.DongSoHuuData
                    {
                        CCCD = Convert.ToString(r[1]),
                        HoTen = Convert.ToString(r[2]),
                        SoDienThoai = Convert.ToString(r[3])
                    });
                }
            }
        }

        private IEnumerable<FrmMainDN.DongSoHuuData> LayDongSoHuuCuaSo(
            string loai, string stk, IList danhSachCu)
        {
            if (_loaiCoBangDongSoHuuRieng.Contains(loai))
            {
                if (_dongSoHuuTheoSoTK.TryGetValue(loai + "|" + stk, out var ds))
                    return ds;
                return Enumerable.Empty<FrmMainDN.DongSoHuuData>();
            }
            // CSDL cũ chỉ có liên kết CCCD_ChuSoHuu, không thể phân biệt theo STK.
            return danhSachCu.Cast<object>().Select(d => new FrmMainDN.DongSoHuuData
            {
                CCCD = LayString(d, "CCCD"),
                HoTen = LayString(d, "HoTen"),
                SoDienThoai = LayString(d, "SoDienThoai")
            });
        }

        private void ThucThiSql(string sql, params SqlParameter[] parameters)
        {
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parameters);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private static SqlParameter P(string key, object value) => new SqlParameter(key, value ?? DBNull.Value);

        private bool TonTaiTaiKhoanSQL(string stk)
        {
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk)
                OR EXISTS(SELECT 1 FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk)
                OR EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk) THEN 1 ELSE 0 END", cn))
            { cmd.Parameters.AddWithValue("@stk", stk); cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()) == 1; }
        }

        private List<Tuple<int, double>> LayKyHanSQL(string maLoai)
        {
            var result = new List<Tuple<int, double>>();
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand("SELECT Thang, LaiSuat FROM dbo.DN_CauHinhLoaiTaiKhoan WHERE MaLoaiTK=@ma AND Thang>0 ORDER BY Thang", cn))
            {
                cmd.Parameters.AddWithValue("@ma", maLoai); cn.Open();
                using (var r = cmd.ExecuteReader()) while (r.Read()) result.Add(Tuple.Create(r.GetInt32(0), Convert.ToDouble(r.GetValue(1))));
            }
            return result;
        }

        private void LuuKhachHangVaTaiKhoan(FrmMainDN.KhachHangData kh)
        {
            using (var cn = new SqlConnection(_connStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        Action<string, SqlParameter[]> run = (sql, ps) => {
                            using (var cmd = new SqlCommand(sql, cn, tx)) { cmd.Parameters.AddRange(ps); cmd.ExecuteNonQuery(); }
                        };
                        var nv = BankSecurity.Current;
                        if (nv == null || string.IsNullOrWhiteSpace(nv.HoTen)) throw new InvalidOperationException("Vui lòng đăng nhập lại.");
                        string tenNV = nv.HoTen;
                        Action<string, string, string> ganDongSoHuu = (bang, cot, stk) =>
                        {
                            using (var check = new SqlCommand("SELECT OBJECT_ID(@b,'U')", cn, tx))
                            {
                                check.Parameters.AddWithValue("@b", "dbo." + bang);
                                if (check.ExecuteScalar() == DBNull.Value) return;
                            }
                            foreach (var d in kh.DanhSachDongSoHuu)
                                run("INSERT INTO dbo." + bang + "(" + cot + ",CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p)",
                                    new[] { P("@stk", stk), P("@c", d.CCCD), P("@n", d.HoTen), P("@p", d.SoDienThoai) });
                        };
                        run("INSERT INTO dbo.DN_KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo,MaSoThue) VALUES(@c,@n,@p,0,@c)",
                            new[] { P("@c", kh.CCCD), P("@n", kh.HoTen), P("@p", kh.SoDienThoai) });
                        foreach (var ds in kh.DanhSachDongSoHuu)
                        {
                            FrmMainDN.KiemTraNguoiUyQuyenKhiThemDN(cn, tx, ds.CCCD, ds.HoTen, ds.SoDienThoai);
                            run("IF NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@chu AND CCCD=@c) INSERT INTO dbo.DN_DongSoHuu(CCCD_ChuSoHuu,CCCD,HoTen,SoDienThoai) VALUES(@chu,@c,@n,@p)",
                                new[] { P("@chu", kh.CCCD), P("@c", ds.CCCD), P("@n", ds.HoTen), P("@p", ds.SoDienThoai) });
                        }
                        foreach (var tt in kh.DanhSachThanhToan)
                        {
                            run("INSERT INTO dbo.DN_TaiKhoanThanhToan(SoTKTT,CCCD,SoDu,TrangThai,NgayMo,NgayCapNhatTT,LoaiTienTe,LoaiTaiKhoan,HanMucNgay,HanMucLan,TenHienThi,SoTienPhongToa) VALUES(@stk,@c,@t,0,GETDATE(),GETDATE(),N'VND',N'Doanh nghiệp',1000000000,200000000,@n,0)",
                                new[] { P("@stk", tt.SoTKTT), P("@c", kh.CCCD), P("@t", tt.SoDu), P("@n", kh.HoTen) });
                            ganDongSoHuu("DN_DongSoHuuTaiKhoanThanhToan", "SoTKTT", tt.SoTKTT);
                            run("INSERT INTO dbo.DN_LichSuGiaoDich(SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien) VALUES(@stk,N'Mở tài khoản',0,@t,GETDATE(),@nv)",
                                new[] { P("@stk", tt.SoTKTT), P("@t", tt.SoDu), P("@nv", tenNV) });
                        }
                        foreach (var vay in kh.DanhSachVay)
                        {
                            FrmMain.KiemTraNhomNoTruocMoVay(cn, tx, true,
                                new[] { kh.CCCD }.Concat(kh.DanhSachDongSoHuu.Select(d => d.CCCD)).ToArray());
                            run("INSERT INTO dbo.DN_TaiKhoanVay(SoTKVay,CCCD,SoTienVay,SoTienDaTra,LaiSuat,KyHan,TrangThai,NgayMo) VALUES(@stk,@c,@t,0,@l,@k,0,GETDATE())",
                                new[] { P("@stk", vay.SoTKVay), P("@c", kh.CCCD), P("@t", vay.SoTienVay), P("@l", vay.LaiSuat), P("@k", vay.KyHan) });
                            ganDongSoHuu("DN_DongSoHuuTaiKhoanVay", "SoTKVay", vay.SoTKVay);
                            run("INSERT INTO dbo.DN_LichSuGiaoDichVay(SoTKVay,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,PhiPhat,TongTien,SoDuConLai,GhiChu,NguoiThucHien) VALUES(@stk,GETDATE(),N'Giải ngân',@t,0,0,@t,@t,N'Mở khoản vay',@nv)",
                                new[] { P("@stk", vay.SoTKVay), P("@t", vay.SoTienVay), P("@nv", tenNV) });
                        }
                        foreach (var tk in kh.DanhSachTietKiem)
                        {
                            run("INSERT INTO dbo.DN_SoTietKiem(SoTKTK,CCCD,SoTienGui,SoTienDaRut,LaiSuat,KyHan,TrangThai,NgayMo,SoTienGocBanDau,TongTienDaNhan) VALUES(@stk,@c,@t,0,@l,@k,0,GETDATE(),@t,0)",
                                new[] { P("@stk", tk.SoTKTK), P("@c", kh.CCCD), P("@t", tk.SoTienGui), P("@l", tk.LaiSuat), P("@k", tk.KyHan) });
                            ganDongSoHuu("DN_DongSoHuuSoTietKiem", "SoTKTK", tk.SoTKTK);
                            run("INSERT INTO dbo.DN_LichSuGiaoDichTietKiem(SoTKTK,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,TongTienNhan,GhiChu,NguoiThucHien) VALUES(@stk,GETDATE(),N'Mở sổ tiết kiệm',@t,0,0,N'Mở sổ mới',@nv)",
                                new[] { P("@stk", tk.SoTKTK), P("@t", tk.SoTienGui), P("@nv", tenNV) });
                        }
                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        // Tim form chua giao dien hien tai. Khi form DN duoc nhung trong FrmMain,
        // FrmMain (khong phai FrmMainDN) la noi dieu huong cac form con.
        private Form TimFrmMainDangChuaForm()
        {
            Control current = this.Parent;
            while (current != null)
            {
                if (current is FrmMain || current is FrmMainDN)
                    return (Form)current;
                current = current.Parent;
            }

            Form owner = this.Owner;
            while (owner != null)
            {
                if (owner is FrmMain || owner is FrmMainDN)
                    return owner;
                owner = owner.Owner;
            }

            foreach (Form f in Application.OpenForms)
                if (!f.IsDisposed && (f is FrmMain || f is FrmMainDN))
                    return f;
            return null;
        }

        private static void MoFormNghiepVuTrongMain(Form main, Form formDich)
        {
            if (main is FrmMain mainCaNhan)
                mainCaNhan.OpenChildForm(formDich);
            else if (main is FrmMainDN mainDoanhNghiep)
                mainDoanhNghiep.OpenChildForm(formDich);
            else
                throw new InvalidOperationException("Khong tim thay cua so chinh de mo form nghiep vu.");
        }

        private void TxtCCCD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void TxtCCCD_TextChanged(object sender, EventArgs e)
        {
            string raw = txtCCCD.Text ?? "";
            string clean = new string(raw.Where(c => c >= '0' && c <= '9').Take(12).ToArray());
            if (raw != clean)
            {
                int pos = txtCCCD.SelectionStart;
                txtCCCD.Text = clean;
                txtCCCD.SelectionStart = Math.Min(pos, clean.Length);
                return;
            }
            if (!_dangKhoiTaoDanhSach) { _timerLocNhanh.Stop(); _timerLocNhanh.Start(); }
        }

        private void BtnTimCCCD_Click(object sender, EventArgs e)
        {
            string cccd = txtCCCD.Text.Trim();
            if (cccd.Length != 12 || !cccd.All(c => c >= '0' && c <= '9'))
            {
                MessageBox.Show("CCCD / MST phải gồm đúng 12 chữ số theo cấu hình hiện tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCCCD.Focus();
                return;
            }
            LayDanhSachKhachHangTuFrmMain();
            if (!_taiDuLieuThanhCong) return;
            bool found = danhSachKhachHang != null && danhSachKhachHang.Cast<object>().Any(kh =>
                LayString(kh, "CCCD").Trim() == cccd ||
                LayDanhSach(kh, "DanhSachDongSoHuu").Cast<object>().Any(d => LayString(d, "CCCD").Trim() == cccd))
                || _dongSoHuuTheoSoTK.Values.Any(ds => ds.Any(d => d.CCCD == cccd));
            HienThiDanhSach(cccd);
            if (!found)
            {
                if (MessageBox.Show("CCCD / MST chưa tồn tại. Bạn có muốn đăng ký doanh nghiệp mới?",
                    "Không tìm thấy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    MoFormDangKyKhachHangMoi(cccd);
            }
            else if (dgvKhachHang.Rows.Count == 0)
                MessageBox.Show("Có khách hàng nhưng không có tài khoản phù hợp bộ lọc hiện tại.", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool KiemTraSTKDaTonTai(string stk) => TonTaiTaiKhoanSQL(stk);

        private bool KiemTraKhachHangCoNhomNoXau(string cccd)
        {
            if (string.IsNullOrWhiteSpace(cccd) || danhSachKhachHang == null) return false;
            int maxCode = LayCodeNhomNoCaoNhatTheoCCCD(cccd);
            return maxCode >= 3;
        }

        private static void DongBoMauKhungThemKhachHangDN(Form dlg)
        {
            Color nen = Color.FromArgb(15, 23, 42);
            Color khung = Color.FromArgb(30, 41, 59);
            Color input = Color.FromArgb(39, 53, 75);
            Color chu = Color.FromArgb(241, 245, 249);
            Color xanh = Color.FromArgb(37, 99, 235);
            Color xanhNhat = Color.FromArgb(147, 197, 253);
            Color xam = Color.FromArgb(148, 163, 184);
            Action<Control> ganMau = null;
            ganMau = c =>
            {
                if (c is DataGridView grid)
                {
                    grid.BackgroundColor = khung;
                    grid.GridColor = Color.FromArgb(51, 65, 85);
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = khung;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = chu;
                    grid.DefaultCellStyle.BackColor = input;
                    grid.DefaultCellStyle.ForeColor = chu;
                    grid.DefaultCellStyle.SelectionBackColor = xanh;
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = nen;
                    grid.AlternatingRowsDefaultCellStyle.ForeColor = chu;
                    return;
                }
                if (c is Button b)
                {
                    string text = (b.Text ?? "").ToLowerInvariant();
                    bool huy = b.DialogResult == DialogResult.Cancel || text.Contains("hủy") || text.Contains("huỷ") || text == "đóng";
                    b.UseVisualStyleBackColor = false;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = huy ? Color.FromArgb(100, 116, 139) : Color.FromArgb(96, 165, 250);
                    b.BackColor = huy ? Color.FromArgb(51, 65, 85) : xanh;
                    b.ForeColor = Color.White;
                }
                else if (c is TextBoxBase t)
                {
                    Action capNhatMau = () => { t.BackColor = t.ReadOnly ? khung : input; t.ForeColor = t.ReadOnly ? xanhNhat : chu; };
                    capNhatMau();
                    t.ReadOnlyChanged += (sender, args) => capNhatMau();
                    t.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = input;
                    cb.ForeColor = chu;
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is NumericUpDown)
                {
                    c.BackColor = input;
                    c.ForeColor = chu;
                    return; // Không can thiệp các control nội bộ của NumericUpDown.
                }
                else if (c is Label label)
                {
                    // Giữ ý nghĩa các màu cảnh báo/hạn mức, làm sáng để đọc trên nền tối.
                    Action capNhatMau = () =>
                    {
                        Color old = label.ForeColor;
                        Color next = old;
                        if (old.R > old.G + 45 && old.R > old.B + 45) next = Color.FromArgb(248, 113, 113);
                        else if (old.B > old.R + 35 && old.B > old.G + 20) next = xanhNhat;
                        else if (old.G > old.R + 35 && old.G > old.B + 20) next = Color.FromArgb(74, 222, 128);
                        else if (old.R > old.B + 50 && old.G > old.B + 20) next = Color.FromArgb(251, 191, 36);
                        else if (old == Color.Gray || old == Color.DimGray) next = xam;
                        else if (old.GetBrightness() < 0.55f) next = chu;
                        if (old.ToArgb() != next.ToArgb()) label.ForeColor = next;
                    };
                    capNhatMau();
                    label.ForeColorChanged += (sender, args) => capNhatMau();
                    label.BackColor = Color.Transparent;
                }
                else if (c is RadioButton || c is CheckBox)
                {
                    c.BackColor = Color.Transparent;
                    c.ForeColor = chu;
                }
                else if (c is GroupBox)
                {
                    c.BackColor = khung;
                    c.ForeColor = xanhNhat;
                }
                else if (c is Panel)
                {
                    c.BackColor = khung;
                    c.ForeColor = chu;
                }
                else if (c is Form)
                {
                    c.BackColor = nen;
                    c.ForeColor = chu;
                }
                // Các nhóm ủy quyền và khung vay có thể được dựng lại sau khi mở dialog.
                c.ControlAdded += (sender, args) => ganMau(args.Control);
                foreach (Control child in c.Controls) ganMau(child);
            };
            ganMau(dlg);
        }

        private void MoFormDangKyKhachHangMoi(string initialCCCD)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Đăng Ký Khách Hàng & Mở Tài Khoản Mới";
                dlg.Size = new Size(620, 900);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label lblCCCD = new Label { Text = "MST (12 số):", Location = new Point(20, 20), AutoSize = true };
                TextBox txtCCCD = new TextBox { Location = new Point(150, 18), Width = 220, MaxLength = 12, Text = initialCCCD };
                txtCCCD.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                Label lblHoTen = new Label { Text = "Họ và Tên:", Location = new Point(20, 55), AutoSize = true };
                TextBox txtHoTen = new TextBox { Location = new Point(150, 53), Width = 220 };
                Label lblSĐT = new Label { Text = "Số điện thoại:", Location = new Point(20, 90), AutoSize = true };
                TextBox txtSĐT = new TextBox { Location = new Point(150, 88), Width = 220, MaxLength = 11 };
                txtSĐT.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);

                Button btnThemDongSoHuu = new Button { Text = "+ Thêm người được ủy quyền", Location = new Point(390, 86), Size = new Size(190, 30), BackColor = Color.White, ForeColor = colorBlue, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                btnThemDongSoHuu.FlatAppearance.BorderColor = colorBlue;
                NumericUpDown nudSoLuongDongSoHuu = new NumericUpDown { Location = new Point(390, 120), Width = 155, Minimum = 1, Maximum = 3, Value = 1, Visible = false };
                Panel pnlDongSoHuu = new Panel { Location = new Point(20, 160), Size = new Size(560, 205), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Visible = false, AutoScroll = true };
                List<TextBox> dsCCCDDongSoHuu = new List<TextBox>();
                List<TextBox> dsHoTenDongSoHuu = new List<TextBox>();
                List<TextBox> dsSDTDongSoHuu = new List<TextBox>();

                Action RenderDongSoHuu = () =>
                {
                    pnlDongSoHuu.Controls.Clear(); dsCCCDDongSoHuu.Clear(); dsHoTenDongSoHuu.Clear(); dsSDTDongSoHuu.Clear();
                    for (int i = 0; i < (int)nudSoLuongDongSoHuu.Value; i++)
                    {
                        GroupBox group = new GroupBox { Text = $"Người được ủy quyền {i + 1}", Location = new Point(8, 8 + i * 64), Size = new Size(525, 58) };
                        Label l1 = new Label { Text = "CCCD:", Location = new Point(8, 22), AutoSize = true };
                        TextBox t1 = new TextBox { Location = new Point(55, 19), Width = 125, MaxLength = 12 }; t1.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                        Label l2 = new Label { Text = "Họ tên:", Location = new Point(190, 22), AutoSize = true };
                        TextBox t2 = new TextBox { Location = new Point(242, 19), Width = 120 };
                        Label l3 = new Label { Text = "SĐT:", Location = new Point(370, 22), AutoSize = true };
                        TextBox t3 = new TextBox { Location = new Point(407, 19), Width = 105, MaxLength = 11 }; t3.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                        group.Controls.AddRange(new Control[] { l1, t1, l2, t2, l3, t3 }); pnlDongSoHuu.Controls.Add(group);
                        dsCCCDDongSoHuu.Add(t1); dsHoTenDongSoHuu.Add(t2); dsSDTDongSoHuu.Add(t3);
                    }
                };
                btnThemDongSoHuu.Click += (s, ev) => { nudSoLuongDongSoHuu.Visible = true; pnlDongSoHuu.Visible = true; RenderDongSoHuu(); };
                nudSoLuongDongSoHuu.ValueChanged += (s, ev) => { pnlDongSoHuu.Visible = true; RenderDongSoHuu(); };

                Label lblLoaiTK = new Label { Text = "Loại tài khoản:", Location = new Point(20, 160), AutoSize = true };
                ComboBox cboLoaiTK = new ComboBox { Location = new Point(150, 158), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList }; cboLoaiTK.Items.AddRange(new object[] { "Thanh toán", "Vay", "Tiết kiệm" }); cboLoaiTK.SelectedIndex = 0;
                RadioButton rdoNgauNhien = new RadioButton { Text = "Số ngẫu nhiên (Free)", Location = new Point(20, 195), Checked = true, AutoSize = true };
                RadioButton rdoSoDep = new RadioButton { Text = "Số đẹp (Tính phí)", Location = new Point(180, 195), AutoSize = true };
                Label lblPhiSoDep = new Label { Text = "* Số đẹp: kiểm tra phí theo chính sách ngân hàng", Location = new Point(340, 197), AutoSize = true, ForeColor = Color.Red, Visible = false };
                Label lblSTK = new Label { Text = "Số TK được cấp:", Location = new Point(20, 230), AutoSize = true };
                TextBox txtSTK = new TextBox { Location = new Point(150, 228), Width = 220, ReadOnly = true };
                Panel pnlSubForm = new Panel { Location = new Point(20, 265), Size = new Size(560, 350), BorderStyle = BorderStyle.FixedSingle };

                Action genSTK = () => {
                    Random r = new Random();
                    string stkGenerated = "";
                    do
                    {
                        stkGenerated = rdoSoDep.Checked ? "88889999" + r.Next(10, 99) : "10" + r.Next(10000000, 99999999);
                    } while (KiemTraSTKDaTonTai(stkGenerated));
                    txtSTK.Text = stkGenerated;
                };

                rdoSoDep.CheckedChanged += (s, ev) => { lblPhiSoDep.Visible = rdoSoDep.Checked; genSTK(); }; rdoNgauNhien.CheckedChanged += (s, ev) => genSTK(); genSTK();

                Action CapNhatViTri = () =>
                {
                    int offset = pnlDongSoHuu.Visible ? 220 : 0;
                    lblLoaiTK.Location = new Point(20, 160 + offset); cboLoaiTK.Location = new Point(150, 158 + offset); rdoNgauNhien.Location = new Point(20, 195 + offset); rdoSoDep.Location = new Point(180, 195 + offset); lblPhiSoDep.Location = new Point(340, 197 + offset); lblSTK.Location = new Point(20, 230 + offset); txtSTK.Location = new Point(150, 228 + offset); pnlSubForm.Location = new Point(20, 265 + offset);
                };
                pnlDongSoHuu.VisibleChanged += (s, ev) => CapNhatViTri();

                FrmMainDN.KhachHangData newKH = null;
                Func<bool> KiemTra = () =>
                {
                    // Dong bo dieu kien mo tai khoan DN: thanh toan >= 2, tiet kiem/vay >= 1.
                    int soNguoiUyQuyenToiThieu = Convert.ToString(cboLoaiTK.SelectedItem) == "Thanh toán" ? 2 : 1;
                    if (dsCCCDDongSoHuu.Count < soNguoiUyQuyenToiThieu)
                    {
                        MessageBox.Show(dlg, $"Vui lòng thêm ít nhất {soNguoiUyQuyenToiThieu} người được ủy quyền trước khi mở tài khoản.",
                            "Thiếu người được ủy quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        if (dsCCCDDongSoHuu.Count == 0)
                        {
                            nudSoLuongDongSoHuu.Value = Math.Max(nudSoLuongDongSoHuu.Minimum, soNguoiUyQuyenToiThieu);
                            nudSoLuongDongSoHuu.Visible = true;
                            pnlDongSoHuu.Visible = true;
                            RenderDongSoHuu();
                            CapNhatViTri();
                        }
                        if (dsCCCDDongSoHuu.Count > 0) dsCCCDDongSoHuu[0].Focus();
                        return false;
                    }
                    if (txtCCCD.Text.Length != 12 || string.IsNullOrWhiteSpace(txtHoTen.Text)) { MessageBox.Show("Vui lòng nhập đủ 12 số MST và Họ tên hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    if (txtSĐT.Text.Length < 10 || txtSĐT.Text.Length > 11) { MessageBox.Show("Số điện thoại phải bao gồm từ 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    HashSet<string> cccd = new HashSet<string>(StringComparer.OrdinalIgnoreCase), sdt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (object kh in danhSachKhachHang)
                    {
                        cccd.Add(LayString(kh, "CCCD")); sdt.Add(LayString(kh, "SoDienThoai"));
                        foreach (object ds in LayDanhSach(kh, "DanhSachDongSoHuu")) { cccd.Add(LayString(ds, "CCCD")); sdt.Add(LayString(ds, "SoDienThoai")); }
                    }
                    if (cccd.Contains(txtCCCD.Text.Trim())) { MessageBox.Show("MST của doanh nghiệp đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    if (sdt.Contains(txtSĐT.Text.Trim())) { MessageBox.Show("Số điện thoại của khách hàng đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    cccd.Clear(); sdt.Clear();
                    for (int i = 0; i < dsCCCDDongSoHuu.Count; i++)
                    {
                        string c = dsCCCDDongSoHuu[i].Text.Trim(), n = dsHoTenDongSoHuu[i].Text.Trim(), p = dsSDTDongSoHuu[i].Text.Trim();
                        if (c.Length != 12 || string.IsNullOrWhiteSpace(n)) { MessageBox.Show($"Vui lòng nhập đúng CCCD 12 số và Họ tên cho Người được ủy quyền {i + 1}!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (p.Length < 10 || p.Length > 11) { MessageBox.Show($"Số điện thoại của Người được ủy quyền {i + 1} phải gồm 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (c == txtCCCD.Text.Trim()) { MessageBox.Show($"CCCD của Người được ủy quyền {i + 1} trùng với MST của chủ tài khoản!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (p == txtSĐT.Text.Trim()) { MessageBox.Show($"Số điện thoại của Người được ủy quyền {i + 1} trùng với SĐT của chủ tài khoản!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (!cccd.Add(c)) { MessageBox.Show($"CCCD của Người được ủy quyền {i + 1} đã tồn tại hoặc bị trùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (!sdt.Add(p)) { MessageBox.Show($"Số điện thoại của Người được ủy quyền {i + 1} đã tồn tại hoặc bị trùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    }
                    return true;
                };
                Action TaoKhachHang = () =>
                {
                    newKH = new FrmMainDN.KhachHangData { CCCD = txtCCCD.Text.Trim(), HoTen = txtHoTen.Text.Trim(), SoDienThoai = txtSĐT.Text.Trim() };
                    for (int i = 0; i < dsCCCDDongSoHuu.Count; i++) newKH.DanhSachDongSoHuu.Add(new FrmMainDN.DongSoHuuData { CCCD = dsCCCDDongSoHuu[i].Text.Trim(), HoTen = dsHoTenDongSoHuu[i].Text.Trim(), SoDienThoai = dsSDTDongSoHuu[i].Text.Trim() });
                };

                Action renderSubForm = () =>
                {
                    pnlSubForm.Controls.Clear(); string selectedType = cboLoaiTK.SelectedItem.ToString();
                    if (selectedType == "Thanh toán")
                    {
                        Button btnSubmit = new Button { Text = "Xác nhận mở tài khoản thanh toán", Location = new Point(130, 130), Size = new Size(280, 45), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                        btnSubmit.Click += (s, ev) => {
                            if (!KiemTra()) return;
                            TaoKhachHang();
                            var ttItem = new FrmMainDN.TaiKhoanThanhToanItem { SoTKTT = txtSTK.Text, SoDu = 0 };
                            PropertyInfo pNhomNo = ttItem.GetType().GetProperty("NhomNo");
                            if (pNhomNo != null) pNhomNo.SetValue(ttItem, "Nhóm 1 (Đủ tiêu chuẩn)", null);
                            PropertyInfo pNgayLam = ttItem.GetType().GetProperty("NgayLam") ?? ttItem.GetType().GetProperty("NgayCapNhatTT");
                            if (pNgayLam != null) pNgayLam.SetValue(ttItem, DateTime.Now, null);
                            newKH.DanhSachThanhToan.Add(ttItem);
                            try { LuuKhachHangVaTaiKhoan(newKH); } catch (Exception ex) { MessageBox.Show("Lỗi lưu SQL: " + ex.Message); return; }
                            MessageBox.Show($"Mở tài khoản thanh toán thành công với STK: {txtSTK.Text}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            dlg.DialogResult = DialogResult.OK;
                        };
                        pnlSubForm.Controls.Add(btnSubmit);
                    }
                    else if (selectedType == "Vay")
                    {
                        RadioButton rdoTinChap = new RadioButton { Text = "Vay Tín Chấp", Location = new Point(15, 10), Checked = true, AutoSize = true }; RadioButton rdoTheChap = new RadioButton { Text = "Vay Thế Chấp", Location = new Point(150, 10), AutoSize = true }; Panel pnlVayBody = new Panel { Location = new Point(10, 40), Size = new Size(535, 295) };
                        Action loadVayBody = () =>
                        {
                            pnlVayBody.Controls.Clear(); bool isTinChap = rdoTinChap.Checked; decimal maxLoan = 0; Label lblMax = new Label { Location = new Point(10, 105), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.Blue }; TextBox txtLuong = new TextBox { Location = new Point(180, 10), Width = 160, Text = "" }; TextBox txtTS = new TextBox { Location = new Point(180, 10), Width = 160, Text = "" }; TextBox txtDG = new TextBox { Location = new Point(180, 42), Width = 160, Text = "" }; ComboBox cboNhomNo = new ComboBox { Location = new Point(180, isTinChap ? 42 : 74), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
                            cboNhomNo.Items.AddRange(new object[] { "Nhóm 1 (Đủ tiêu chuẩn)", "Nhóm 2 (Cần chú ý)", "Nhóm 3 (Dưới tiêu chuẩn)", "Nhóm 4 (Nghi ngờ)", "Nhóm 5 (Mất vốn)" }); cboNhomNo.SelectedIndex = 0;
                            Action calcMax = () => { double h = cboNhomNo.SelectedIndex == 0 ? 1.0 : cboNhomNo.SelectedIndex == 1 ? 0.8 : 0.0; decimal noTruocDo = 0; if (isTinChap) { decimal.TryParse(txtLuong.Text, out decimal luong); maxLoan = Math.Max(0, luong * 10 * (decimal)h - noTruocDo); } else { decimal.TryParse(txtDG.Text, out decimal dg); maxLoan = Math.Max(0, dg * 0.7m * (decimal)h - noTruocDo); } lblMax.Text = $"Số tiền tối đa có thể vay: {maxLoan:N0} VNĐ"; };
                            cboNhomNo.SelectedIndexChanged += (s, ev) => calcMax(); txtLuong.TextChanged += (s, ev) => calcMax(); txtDG.TextChanged += (s, ev) => calcMax();
                            if (isTinChap) { pnlVayBody.Controls.Add(new Label { Text = "Mức lương (VNĐ):", Location = new Point(10, 12), AutoSize = true }); pnlVayBody.Controls.Add(txtLuong); pnlVayBody.Controls.Add(new Label { Text = "Nhóm nợ hiện tại:", Location = new Point(10, 44), AutoSize = true }); pnlVayBody.Controls.Add(cboNhomNo); }
                            else { pnlVayBody.Controls.Add(new Label { Text = "Tên tài sản:", Location = new Point(10, 12), AutoSize = true }); pnlVayBody.Controls.Add(txtTS); pnlVayBody.Controls.Add(new Label { Text = "Định giá TS (VNĐ):", Location = new Point(10, 44), AutoSize = true }); pnlVayBody.Controls.Add(txtDG); pnlVayBody.Controls.Add(new Label { Text = "Nhóm nợ hiện tại:", Location = new Point(10, 76), AutoSize = true }); pnlVayBody.Controls.Add(cboNhomNo); }
                            pnlVayBody.Controls.Add(lblMax); int curY = 135; pnlVayBody.Controls.Add(new Label { Text = "Số tiền muốn vay:", Location = new Point(10, curY), AutoSize = true }); TextBox txtSoTienVay = new TextBox { Location = new Point(180, curY - 2), Width = 160 }; pnlVayBody.Controls.Add(txtSoTienVay); pnlVayBody.Controls.Add(new Label { Text = "Kỳ hạn vay:", Location = new Point(10, curY + 32), AutoSize = true }); ComboBox cboKH = new ComboBox { Location = new Point(180, curY + 30), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
                            var dsKyHan = LayKyHanSQL("TK_VAY");
                            foreach (var ky in dsKyHan) cboKH.Items.Add(string.Format("{0} tháng: {1:0.##}%/năm", ky.Item1, ky.Item2));
                            if (cboKH.Items.Count == 0) { MessageBox.Show("Chưa cấu hình kỳ hạn vay trong SQL!"); return; }
                            cboKH.SelectedIndex = 0; pnlVayBody.Controls.Add(cboKH);
                            Button btnOk = new Button { Text = "Xác nhận mở", Location = new Point(110, curY + 75), Size = new Size(110, 32), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; Button btnCancel = new Button { Text = "Hủy", Location = new Point(230, curY + 75), Size = new Size(110, 32) };
                            btnOk.Click += (s, ev) => {
                                if (cboNhomNo.SelectedIndex >= 2)
                                {
                                    MessageBox.Show("Khách hàng thuộc Nhóm 3 - Nhóm 5 (Nợ xấu) không được phép mở khoản vay mới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                if (KiemTraKhachHangCoNhomNoXau(txtCCCD.Text.Trim()))
                                {
                                    MessageBox.Show("Khách hàng đang có tài khoản/khoản vay thuộc Nhóm 3 - Nhóm 5 (Nợ xấu), không được phép mở thêm khoản vay!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                for (int i = 0; i < dsCCCDDongSoHuu.Count; i++)
                                {
                                    string cDSH = dsCCCDDongSoHuu[i].Text.Trim();
                                    if (KiemTraKhachHangCoNhomNoXau(cDSH))
                                    {
                                        MessageBox.Show($"Người được ủy quyền {i + 1} (CCCD: {cDSH}) đang có tài khoản/khoản vay thuộc Nhóm 3 - Nhóm 5 (Nợ xấu), không được phép mở khoản vay!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                                }
                                if (!decimal.TryParse(txtSoTienVay.Text, out decimal valVay) || valVay <= 0 || valVay > maxLoan) { MessageBox.Show("Số tiền vay không hợp lệ hoặc vượt quá hạn mức tối đa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                                if (!KiemTra()) return; TaoKhachHang(); var kyDaChon = dsKyHan[cboKH.SelectedIndex]; double rate = kyDaChon.Item2; string khStr = kyDaChon.Item1 + " tháng";
                                var vayItem = new FrmMainDN.TaiKhoanVayItem { SoTKVay = txtSTK.Text, SoTienVay = valVay, SoTienDaTra = 0, LaiSuat = rate, KyHan = khStr };
                                PropertyInfo pNhomNo = vayItem.GetType().GetProperty("NhomNo");
                                if (pNhomNo != null) pNhomNo.SetValue(vayItem, cboNhomNo.SelectedItem?.ToString(), null);
                                PropertyInfo pTrangThai = vayItem.GetType().GetProperty("TrangThai");
                                if (pTrangThai != null) pTrangThai.SetValue(vayItem, Enum.Parse(typeof(FrmMainDN.TrangThaiTK), "HoatDong"), null);
                                PropertyInfo pNgayLam = vayItem.GetType().GetProperty("NgayLam") ?? vayItem.GetType().GetProperty("NgayCapNhatTT");
                                if (pNgayLam != null) pNgayLam.SetValue(vayItem, DateTime.Now, null);
                                newKH.DanhSachVay.Add(vayItem);
                                try { LuuKhachHangVaTaiKhoan(newKH); } catch (Exception ex) { MessageBox.Show("Lỗi lưu SQL: " + ex.Message); return; }
                                MessageBox.Show("Đã mở tài khoản vay thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); dlg.DialogResult = DialogResult.OK;
                            }; btnCancel.Click += (s, ev) => dlg.Close(); pnlVayBody.Controls.AddRange(new Control[] { btnOk, btnCancel }); calcMax();
                        };
                        rdoTinChap.CheckedChanged += (s, ev) => loadVayBody(); rdoTheChap.CheckedChanged += (s, ev) => loadVayBody(); pnlSubForm.Controls.AddRange(new Control[] { rdoTinChap, rdoTheChap, pnlVayBody }); loadVayBody();
                    }
                    else
                    {
                        pnlSubForm.Controls.Add(new Label { Text = "Số tiền gửi tiết kiệm (VND):", Location = new Point(15, 20), AutoSize = true }); TextBox txtGui = new TextBox { Location = new Point(200, 18), Width = 200 }; pnlSubForm.Controls.Add(txtGui); pnlSubForm.Controls.Add(new Label { Text = "Chọn kỳ hạn gửi (Bắt buộc):", Location = new Point(15, 60), AutoSize = true }); ComboBox cboKH = new ComboBox { Location = new Point(200, 58), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList }; var dsKyHan = LayKyHanSQL("TK_TIETKIEM"); foreach (var ky in dsKyHan) cboKH.Items.Add(string.Format("Kỳ hạn {0} tháng : {1:0.##}%/năm", ky.Item1, ky.Item2)); if (cboKH.Items.Count == 0) { MessageBox.Show("Chưa cấu hình kỳ hạn tiết kiệm trong SQL!"); return; }
                        cboKH.SelectedIndex = 0; pnlSubForm.Controls.Add(cboKH); pnlSubForm.Controls.Add(new Label { Text = "* Lãi rút trước hạn áp dụng theo chính sách ngân hàng.", Location = new Point(15, 100), AutoSize = true, ForeColor = Color.DarkGray });
                        Button btnOk = new Button { Text = "Xác nhận mở", Location = new Point(110, 160), Size = new Size(120, 35), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; Button btnCancel = new Button { Text = "Hủy", Location = new Point(240, 160), Size = new Size(120, 35) };
                        btnOk.Click += (s, ev) => {
                            if (!decimal.TryParse(txtGui.Text, out decimal valGui) || valGui <= 0) { MessageBox.Show("Số tiền gửi không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                            if (!KiemTra()) return;
                            TaoKhachHang();
                            var kyDaChon = dsKyHan[cboKH.SelectedIndex];
                            var tkItem = new FrmMainDN.SoTietKiemItem { SoTKTK = txtSTK.Text, SoTienGui = valGui, LaiSuat = kyDaChon.Item2, SoTienDaRut = 0, KyHan = kyDaChon.Item1 + " tháng" };
                            PropertyInfo pNhomNo = tkItem.GetType().GetProperty("NhomNo");
                            if (pNhomNo != null) pNhomNo.SetValue(tkItem, "Nhóm 1 (Đủ tiêu chuẩn)", null);
                            PropertyInfo pTrangThai = tkItem.GetType().GetProperty("TrangThai");
                            if (pTrangThai != null) pTrangThai.SetValue(tkItem, Enum.Parse(typeof(FrmMainDN.TrangThaiTK), "HoatDong"), null);
                            PropertyInfo pNgayLam = tkItem.GetType().GetProperty("NgayLam") ?? tkItem.GetType().GetProperty("NgayCapNhatTT");
                            if (pNgayLam != null) pNgayLam.SetValue(tkItem, DateTime.Now, null);
                            newKH.DanhSachTietKiem.Add(tkItem);
                            try { LuuKhachHangVaTaiKhoan(newKH); } catch (Exception ex) { MessageBox.Show("Lỗi lưu SQL: " + ex.Message); return; }
                            MessageBox.Show("Đã mở tài khoản tiết kiệm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            dlg.DialogResult = DialogResult.OK;
                        };
                        btnCancel.Click += (s, ev) => dlg.Close(); pnlSubForm.Controls.AddRange(new Control[] { btnOk, btnCancel });
                    }
                };

                cboLoaiTK.SelectedIndexChanged += (s, ev) => renderSubForm();
                dlg.Controls.AddRange(new Control[] { lblCCCD, txtCCCD, lblHoTen, txtHoTen, lblSĐT, txtSĐT, btnThemDongSoHuu, nudSoLuongDongSoHuu, pnlDongSoHuu, lblLoaiTK, cboLoaiTK, rdoNgauNhien, rdoSoDep, lblPhiSoDep, lblSTK, txtSTK, pnlSubForm });
                renderSubForm(); CapNhatViTri();
                DongBoMauKhungThemKhachHangDN(dlg);
                if (dlg.ShowDialog() == DialogResult.OK && newKH != null) { LayDanhSachKhachHangTuFrmMain(); txtCCCD.Text = newKH.CCCD; HienThiDanhSach(newKH.CCCD); }
            }
        }

        private void HienThiDanhSach(string cccd)
        {
            if (_dangKhoiTaoDanhSach || !_taiDuLieuThanhCong || IsDisposed || Disposing) return;
            _timerLocNhanh.Stop();
            dgvKhachHang.SuspendLayout();
            try { HienThiDanhSachNoiBo(cccd); }
            finally { dgvKhachHang.ResumeLayout(); }
            DgvKhachHang_KiemTraLuaChon(dgvKhachHang, EventArgs.Empty);
        }

        private void HienThiDanhSachNoiBo(string cccd)
        {
            // Chỉ số liệu của cột này thay đổi theo loại tài khoản:
            // Vay: tổng trả gốc+lãi+phí; tiết kiệm: tổng gốc+lãi đã nhận.
            if (dgvKhachHang.Columns.Count > 7)
                dgvKhachHang.Columns[7].HeaderText = "Tổng đã trả / đã nhận";
            dgvKhachHang.Rows.Clear();
            if (danhSachKhachHang == null) return;

            int stt = 1;
            int loaiIndex = cboLocLoaiTK.SelectedIndex;

            foreach (object kh in danhSachKhachHang)
            {
                string maCCCD = LayString(kh, "CCCD");
                IList dsDongSoHuu = LayDanhSach(kh, "DanhSachDongSoHuu");

                bool khopChu = string.IsNullOrWhiteSpace(cccd) || maCCCD.Contains(cccd);
                string hoTen = LayString(kh, "HoTen");
                string sdt = LayString(kh, "SoDienThoai");

                // TÀI KHOẢN VAY
                if (loaiIndex <= 0 || loaiIndex == 1)
                {
                    IList danhSachVay = LayDanhSach(kh, "DanhSachVay");
                    foreach (object vay in danhSachVay)
                    {
                        if (!KiemTraTrangThaiFilter(vay) || !KiemTraThoiGianFilter(vay) || !KiemTraNhomNoFilter(vay, maCCCD)) continue;

                        string nhomNo = LayNhomNo(vay, maCCCD);
                        decimal soTienVay = LayDecimal(vay, "SoTienVay");
                        decimal soTienDaTra = LayDecimal(vay, "SoTienDaTra");
                        decimal tongTienDaThanhToan = LayDecimal(vay, "TongTienDaThanhToan");
                        string stk = LayString(vay, "SoTKVay");
                        decimal gocDaTra = soTienDaTra; // SQL la nguon du lieu goc da tra duy nhat
                        decimal duNoConLai = Math.Max(0, soTienVay - gocDaTra);
                        string trangThai = LayTrangThai(vay);

                        var nguoiUyQuyen = LayDongSoHuuCuaSo("Khoản vay", stk, dsDongSoHuu)
                            .Where(d => khopChu || LayString(d, "CCCD").Contains(cccd)).ToList();
                        if (!khopChu && nguoiUyQuyen.Count == 0) continue;
                        AddRow(
                            stt++,
                            stk,
                            maCCCD,
                            hoTen,
                            sdt,
                            "Khoản vay",
                            LayTien(duNoConLai),
                            LayTien(tongTienDaThanhToan),
                            LayString(vay, "KyHan"),
                            LayLaiSuat(vay),
                            nhomNo,
                            LayNgayLam(vay),
                            "Chủ",
                            trangThai);

                        for (int i = 0; i < nguoiUyQuyen.Count; i++)
                        {
                            var ds = nguoiUyQuyen[i];
                            AddRow(stt++, stk, LayString(ds, "CCCD"), LayString(ds, "HoTen"), LayString(ds, "SoDienThoai"), "Khoản vay", LayTien(duNoConLai), LayTien(tongTienDaThanhToan), LayString(vay, "KyHan"), LayLaiSuat(vay), nhomNo, LayNgayLam(vay), "Người được ủy quyền", trangThai, hoTen, maCCCD, i == nguoiUyQuyen.Count - 1);
                        }
                    }
                }

                // SỔ TIẾT KIỆM
                if (loaiIndex <= 0 || loaiIndex == 2)
                {
                    IList danhSachTietKiem = LayDanhSach(kh, "DanhSachTietKiem");
                    foreach (object tietKiem in danhSachTietKiem)
                    {
                        if (!KiemTraTrangThaiFilter(tietKiem) || !KiemTraThoiGianFilter(tietKiem) || !KiemTraNhomNoFilter(tietKiem, maCCCD)) continue;

                        string stkTK = LayString(tietKiem, "SoTKTK");
                        string nhomNoTK = LayNhomNo(tietKiem, maCCCD);
                        string trangThaiTK = LayTrangThai(tietKiem);

                        var nguoiUyQuyen = LayDongSoHuuCuaSo("Tiết kiệm", stkTK, dsDongSoHuu)
                            .Where(d => khopChu || LayString(d, "CCCD").Contains(cccd)).ToList();
                        if (!khopChu && nguoiUyQuyen.Count == 0) continue;
                        AddRow(
                            stt++,
                            LayString(tietKiem, "SoTKTK"),
                            maCCCD,
                            hoTen,
                            sdt,
                            "Tiết kiệm",
                            LayTien(LayDecimal(tietKiem, "SoTienGui")),
                            LayTien(LayDecimal(tietKiem, "TongTienDaNhan")),
                            LayString(tietKiem, "KyHan"),
                            LayLaiSuat(tietKiem),
                            nhomNoTK,
                            LayNgayLam(tietKiem),
                            "Chủ",
                            trangThaiTK);

                        for (int i = 0; i < nguoiUyQuyen.Count; i++)
                        {
                            var ds = nguoiUyQuyen[i];
                            AddRow(stt++, LayString(tietKiem, "SoTKTK"), LayString(ds, "CCCD"), LayString(ds, "HoTen"), LayString(ds, "SoDienThoai"), "Tiết kiệm", LayTien(LayDecimal(tietKiem, "SoTienGui")), LayTien(LayDecimal(tietKiem, "TongTienDaNhan")), LayString(tietKiem, "KyHan"), LayLaiSuat(tietKiem), nhomNoTK, LayNgayLam(tietKiem), "Người được ủy quyền", trangThaiTK, hoTen, maCCCD, i == nguoiUyQuyen.Count - 1);
                        }
                    }
                }

                // TÀI KHOẢN THANH TOÁN
                if (loaiIndex <= 0 || loaiIndex == 3)
                {
                    IList danhSachThanhToan = LayDanhSach(kh, "DanhSachThanhToan");
                    foreach (object thanhToan in danhSachThanhToan)
                    {
                        if (!KiemTraTrangThaiFilter(thanhToan) || !KiemTraThoiGianFilter(thanhToan) || !KiemTraNhomNoFilter(thanhToan, maCCCD)) continue;

                        string stkTT = LayString(thanhToan, "SoTKTT");
                        string nhomNoTT = LayNhomNo(thanhToan, maCCCD);
                        string trangThaiTT = LayTrangThai(thanhToan);

                        var nguoiUyQuyen = LayDongSoHuuCuaSo("Thanh toán", stkTT, dsDongSoHuu)
                            .Where(d => khopChu || LayString(d, "CCCD").Contains(cccd)).ToList();
                        if (!khopChu && nguoiUyQuyen.Count == 0) continue;
                        AddRow(
                            stt++,
                            LayString(thanhToan, "SoTKTT"),
                            maCCCD,
                            hoTen,
                            sdt,
                            "Thanh toán",
                            LayTien(LayDecimal(thanhToan, "SoDu")),
                            LayTien(0),
                            "Không",
                            "0%",
                            nhomNoTT,
                            LayNgayLam(thanhToan),
                            "Chủ",
                            trangThaiTT);

                        for (int i = 0; i < nguoiUyQuyen.Count; i++)
                        {
                            var ds = nguoiUyQuyen[i];
                            AddRow(stt++, LayString(thanhToan, "SoTKTT"), LayString(ds, "CCCD"), LayString(ds, "HoTen"), LayString(ds, "SoDienThoai"), "Thanh toán", LayTien(LayDecimal(thanhToan, "SoDu")), LayTien(0), "Không", "0%", nhomNoTT, LayNgayLam(thanhToan), "Người được ủy quyền", trangThaiTT, hoTen, maCCCD, i == nguoiUyQuyen.Count - 1);
                        }
                    }
                }
            }

            lblSoKetQua.Text = $"Hiển thị: {dgvKhachHang.Rows.Count:N0} dòng";
            lblTrangThai.Text = string.IsNullOrWhiteSpace(cccd) ? "Danh sách tất cả tài khoản" : $"Lọc theo CCCD / MST: {cccd}";
            CapNhatThongTinTongQuan();
        }

        private void AddRow(int stt, string stk, string cccd, string hoTen, string sdt, string loaiTaiKhoan, string soDuDuNo, string daTraDaNhan, string kyHan, string laiSuat, string nhomNo, string ngayLam, string vaiTro, string trangThai, string tenChu = null, string mstChu = null, bool laDongCuoi = false)
        {
            int rowIndex = dgvKhachHang.Rows.Add(stt, stk, cccd, hoTen, sdt, loaiTaiKhoan, soDuDuNo, daTraDaNhan, kyHan, laiSuat, nhomNo, ngayLam, vaiTro, trangThai);
            DataGridViewRow row = dgvKhachHang.Rows[rowIndex];

            if (loaiTaiKhoan == "Khoản vay") row.Cells["colLoaiTaiKhoan"].Style.ForeColor = Color.FromArgb(248, 113, 113);
            else if (loaiTaiKhoan == "Tiết kiệm") row.Cells["colLoaiTaiKhoan"].Style.ForeColor = Color.FromArgb(34, 197, 94);
            else if (loaiTaiKhoan == "Thanh toán") row.Cells["colLoaiTaiKhoan"].Style.ForeColor = Color.FromArgb(96, 165, 250);

            if (nhomNo != "-" && !string.IsNullOrEmpty(nhomNo))
            {
                if (nhomNo.Contains("1")) row.Cells["colNhomNo"].Style.ForeColor = Color.FromArgb(34, 197, 94);
                else if (nhomNo.Contains("2")) row.Cells["colNhomNo"].Style.ForeColor = Color.FromArgb(250, 204, 21);
                else row.Cells["colNhomNo"].Style.ForeColor = Color.FromArgb(248, 113, 113);
            }

            if (vaiTro == "Chủ")
            {
                row.Cells["colHoTen"].Style.Font = _fontVaiTroDungChung;
                row.Cells["colHoTen"].Style.ForeColor = Color.FromArgb(96, 165, 250);
                row.Cells["colHoTen"].ToolTipText = hoTen + " | MST: " + cccd;
                row.Cells["colVaiTro"].Style.ForeColor = Color.FromArgb(96, 165, 250);
                row.Cells["colVaiTro"].Style.Font = _fontVaiTroDungChung;
            }
            else if (vaiTro == "Người được ủy quyền")
            {
                row.ReadOnly = true;
                // Chỉ định dạng hiển thị, không thay đổi giá trị họ tên gốc.
                row.Cells["colHoTen"].Tag = laDongCuoi ? "    └─ " : "    ├─ ";
                row.Cells["colHoTen"].ToolTipText = hoTen + "\nThuộc doanh nghiệp: " + tenChu + "\nMST: " + mstChu + " | STK: " + stk;
                row.Cells["colSTK"].ToolTipText = "Thuộc doanh nghiệp: " + tenChu + " | MST: " + mstChu;
                row.Cells["colVaiTro"].ToolTipText = "Chỉ xem trên danh sách. Chọn dòng chủ sở hữu để mở chi tiết.";
                row.Cells["colVaiTro"].Style.ForeColor = Color.FromArgb(192, 132, 252);
                row.Cells["colVaiTro"].Style.Font = _fontVaiTroDungChung;
            }

            if (trangThai == "Hoạt động") row.Cells["colTrangThai"].Style.ForeColor = Color.FromArgb(34, 197, 94);
            else if (trangThai == "Đang khóa" || trangThai == "Khóa khẩn cấp") row.Cells["colTrangThai"].Style.ForeColor = Color.FromArgb(251, 146, 60);
            else row.Cells["colTrangThai"].Style.ForeColor = Color.FromArgb(248, 113, 113);
        }

        // Chỉ dòng chủ sở hữu được chọn để thao tác trong form này.
        private bool LaDongChuSoHuu(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvKhachHang.Rows.Count) return false;
            var row = dgvKhachHang.Rows[rowIndex];
            if (row.IsNewRow) return false;
            return NhanDinhDanhTheoVaiTro(Convert.ToString(row.Cells["colVaiTro"].Value).Trim()) == "MST";
        }

        private void BoChonDongUyQuyen()
        {
            if (_dangKiemTraLuaChon || IsDisposed || Disposing) return;
            _dangKiemTraLuaChon = true;
            try
            {
                dgvKhachHang.ClearSelection();
                lblKhachHangDangChon.Text = "Chưa chọn tài khoản";
                lblChiTiet.Text = "Chỉ chọn chủ sở hữu để xem chi tiết tài khoản.";
            }
            finally { _dangKiemTraLuaChon = false; }
            DgvKhachHang_KiemTraLuaChon(dgvKhachHang, EventArgs.Empty);
        }

        private void DgvKhachHang_KiemTraLuaChon(object sender, EventArgs e)
        {
            if (_dangKiemTraLuaChon || _daHenKiemTraLuaChon || IsDisposed || Disposing || !IsHandleCreated) return;
            _daHenKiemTraLuaChon = true;
            // Đợi DataGridView hoàn tất đổi CurrentCell để tránh lỗi tái nhập sự kiện.
            BeginInvoke(new Action(() =>
            {
                _daHenKiemTraLuaChon = false;
                if (IsDisposed || Disposing || dgvKhachHang.IsDisposed) return;
                _dangKiemTraLuaChon = true;
                try
                {
                    var current = dgvKhachHang.CurrentRow;
                    if (current != null && !LaDongChuSoHuu(current.Index))
                    {
                        dgvKhachHang.CurrentCell = null;
                        dgvKhachHang.ClearSelection();
                        lblKhachHangDangChon.Text = "Chưa chọn tài khoản";
                        lblChiTiet.Text = "Chỉ chọn chủ sở hữu để xem chi tiết tài khoản.";
                    }
                    foreach (DataGridViewRow row in dgvKhachHang.SelectedRows.Cast<DataGridViewRow>().ToArray())
                        if (!LaDongChuSoHuu(row.Index)) row.Selected = false;
                }
                finally { _dangKiemTraLuaChon = false; }
            }));
        }

        private void DgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!LaDongChuSoHuu(e.RowIndex)) { BoChonDongUyQuyen(); return; }
            HienThiThongTinDong(e.RowIndex);
        }

        // Nhấp đúp chỉ CHUYỂN DANH MỤC trong FrmMainDN, không mở cửa sổ nổi
        // và không kích hoạt bảng thông tin/chức năng của form đích.
        private void DgvKhachHang_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvKhachHang.Rows.Count) return;
            DataGridViewRow row = dgvKhachHang.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            if (!LaDongChuSoHuu(e.RowIndex)) { BoChonDongUyQuyen(); return; }

            HienThiThongTinDong(e.RowIndex);
            string stk = Convert.ToString(row.Cells["colSTK"].Value)?.Trim();
            string loai = Convert.ToString(row.Cells["colLoaiTaiKhoan"].Value)?.Trim();
            string cccdDong = Convert.ToString(row.Cells["colCCCD"].Value)?.Trim();
            if (string.IsNullOrWhiteSpace(stk)) return;

            // Dò đúng bảng bằng loại tài khoản của dòng được nhấp.
            string bang, cotSo, tenGrid;
            Func<string, Form> taoForm;
            switch (loai)
            {
                case "Khoản vay":
                case "Vay":
                    bang = "dbo.DN_TaiKhoanVay"; cotSo = "SoTKVay";
                    tenGrid = "dgvVay";
                    taoForm = chu => new FrmVayDN(chu, stk);
                    break;
                case "Tiết kiệm":
                    bang = "dbo.DN_SoTietKiem"; cotSo = "SoTKTK";
                    tenGrid = "dgvTietKiem";
                    taoForm = chu => new FrmTietKiemDN(chu);
                    break;
                case "Thanh toán":
                    bang = "dbo.DN_TaiKhoanThanhToan"; cotSo = "SoTKTT";
                    tenGrid = "dgvThanhToan";
                    taoForm = chu => new FrmThanhToanDN(chu);
                    break;
                default:
                    MessageBox.Show("Loại tài khoản không được hỗ trợ: " + loai,
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }

            try
            {
                string cccdChu;
                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(
                    "SELECT CCCD FROM " + bang + " WHERE " + cotSo + " = @stk", cn))
                {
                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                    cn.Open();
                    cccdChu = Convert.ToString(cmd.ExecuteScalar())?.Trim();
                }
                if (string.IsNullOrWhiteSpace(cccdChu))
                {
                    MessageBox.Show("Không tìm thấy số tài khoản " + stk + " trong SQL.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Form main = TimFrmMainDangChuaForm();
                if (main == null || main.IsDisposed)
                {
                    MessageBox.Show("Không tìm thấy cửa sổ chính để chuyển danh mục.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Đẩy việc chuyển form về hàng đợi giao diện: FrmMainDN có thể đóng
                // FrmKhachHangDN hiện tại trong OpenChildForm, nên không thao tác trên
                // FrmKhachHangDN nữa sau khi chuyển.
                main.BeginInvoke(new Action(() =>
                {
                    if (main.IsDisposed) return;
                    Form formDich = null;
                    try
                    {
                        formDich = taoForm(cccdChu);
                        MoFormNghiepVuTrongMain(main, formDich);
                        // Chỉ chọn/di chuyển đến STK, KHÔNG gọi CellDoubleClick.
                        ChonDongTaiKhoanKhongMoChiTiet(formDich, tenGrid, stk, cccdDong);
                    }
                    catch (Exception ex)
                    {
                        if (formDich != null && !formDich.IsDisposed && !formDich.Visible)
                            formDich.Dispose();
                        MessageBox.Show("Không thể chuyển đến danh mục tài khoản:\n" + ex.Message,
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xác định tài khoản " + stk + ":\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Đánh dấu đúng tài khoản sau khi chuyển danh mục. Không mở cửa sổ chức năng.
        private static void ChonDongTaiKhoanKhongMoChiTiet(
            Form form, string tenGrid, string stk, string cccdDong)
        {
            if (form == null || form.IsDisposed) return;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var field = form.GetType().GetField(tenGrid, flags);
            var grid = field?.GetValue(form) as DataGridView;
            if (grid == null || grid.IsDisposed || !grid.Columns.Contains("colSTK")) return;

            int viTri = -1;
            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var dong = grid.Rows[i];
                if (dong.IsNewRow) continue;
                if (!string.Equals(Convert.ToString(dong.Cells["colSTK"].Value)?.Trim(),
                    stk, StringComparison.OrdinalIgnoreCase)) continue;
                if (viTri < 0) viTri = i;
                if (grid.Columns.Contains("colCCCD") &&
                    string.Equals(Convert.ToString(dong.Cells["colCCCD"].Value)?.Trim(),
                        cccdDong, StringComparison.OrdinalIgnoreCase))
                {
                    viTri = i;
                    break;
                }
            }
            if (viTri < 0) return;

            grid.ClearSelection();
            int cot = grid.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible).Select(c => c.Index).DefaultIfEmpty(-1).First();
            if (cot >= 0)
            {
                grid.CurrentCell = grid.Rows[viTri].Cells[cot];
                grid.Rows[viTri].Selected = true;
            }
            try { grid.FirstDisplayedScrollingRowIndex = viTri; }
            catch (InvalidOperationException) { }
        }

        private static string NhanDinhDanhTheoVaiTro(string vaiTro)
        {
            return vaiTro == "Chủ" || vaiTro == "Chủ tài khoản" || vaiTro == "Chủ sở hữu" ? "MST" : "CCCD";
        }

        private void HienThiThongTinDong(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvKhachHang.Rows.Count) return;
            if (!LaDongChuSoHuu(rowIndex)) { BoChonDongUyQuyen(); return; }
            DataGridViewRow row = dgvKhachHang.Rows[rowIndex];

            string stk = row.Cells["colSTK"].Value?.ToString() ?? "";
            string cccd = row.Cells["colCCCD"].Value?.ToString() ?? "";
            string hoTen = row.Cells["colHoTen"].Value?.ToString() ?? "";
            string loai = row.Cells["colLoaiTaiKhoan"].Value?.ToString() ?? "";
            string vaiTro = row.Cells["colVaiTro"].Value?.ToString() ?? "";

            lblKhachHangDangChon.Text = $"Khách hàng: {hoTen}";
            lblChiTiet.Text = $"{NhanDinhDanhTheoVaiTro(vaiTro)}: {cccd}     |     STK: {stk}     |     Loại: {loai}     |     Vai trò: {vaiTro}";
        }

        private void DgvKhachHang_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            DataGridView.HitTestInfo hit = dgvKhachHang.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0 || dgvKhachHang.Rows[hit.RowIndex].IsNewRow) return;
            if (!LaDongChuSoHuu(hit.RowIndex)) { BoChonDongUyQuyen(); return; }
            if (hit.ColumnIndex >= 0)
                dgvKhachHang.CurrentCell = dgvKhachHang.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
            dgvKhachHang.ClearSelection();
            dgvKhachHang.Rows[hit.RowIndex].Selected = true;
            HienThiThongTinDong(hit.RowIndex);
            string stk = Convert.ToString(dgvKhachHang.Rows[hit.RowIndex].Cells["colSTK"].Value).Trim();
            string loai = Convert.ToString(dgvKhachHang.Rows[hit.RowIndex].Cells["colLoaiTaiKhoan"].Value).Trim();
            string trangThai = Convert.ToString(dgvKhachHang.Rows[hit.RowIndex].Cells["colTrangThai"].Value).Trim();
            int rowIndex = hit.RowIndex;
            string cccdDong = Convert.ToString(dgvKhachHang.Rows[rowIndex].Cells["colCCCD"].Value).Trim();
            string vaiTroDong = Convert.ToString(dgvKhachHang.Rows[rowIndex].Cells["colVaiTro"].Value).Trim();
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem sua = new ToolStripMenuItem("Sửa thông tin");
            sua.Click += (s, ev) => ChaySauKhiDongMenu(() => ThaoTacMenuTheoTaiKhoan(stk, loai, cccdDong, vaiTroDong, true));
            ToolStripMenuItem chuyenNhom = new ToolStripMenuItem("Chuyển nhóm nợ");
            chuyenNhom.Click += (s, ev) => ChaySauKhiDongMenu(() => ThaoTacMenuTheoTaiKhoan(stk, loai, cccdDong, vaiTroDong, false));
            menu.Items.Add(sua);
            menu.Items.Add(chuyenNhom);
            menu.Items.Add(new ToolStripSeparator());
            bool dangKhoa = trangThai == "Đang khóa" || trangThai == "Khóa khẩn cấp";
            bool daDong = trangThai == "Đã đóng";
            ToolStripMenuItem khoaMo = new ToolStripMenuItem(dangKhoa ? "Mở khóa tạm thời" : "Khóa tạm thời");
            khoaMo.Enabled = !daDong;
            khoaMo.Click += (s, ev) => ChaySauKhiDongMenu(() => CapNhatTrangThaiTaiKhoan(stk, loai, dangKhoa ? "HoatDong" : "Khoa"));
            menu.Items.Add(khoaMo);
            ToolStripMenuItem dong = new ToolStripMenuItem("Đóng luôn tài khoản");
            dong.Enabled = !daDong;
            dong.Click += (s, ev) => ChaySauKhiDongMenu(() => CapNhatTrangThaiTaiKhoan(stk, loai, "Dong"));
            menu.Items.Add(dong);
            // Không Dispose ngay trong Closed: WinForms vẫn có thể đang xử lý sự kiện Click.
            menu.Closed += (s, ev) =>
            {
                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke(new Action(() => { if (!menu.IsDisposed) menu.Dispose(); }));
            };
            menu.Show(dgvKhachHang, e.Location);
        }

        // Trì hoãn thao tác cho đến khi WinForms kết thúc xử lý click menu.
        private void ChaySauKhiDongMenu(Action thaoTac)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || Disposing) return;
                thaoTac();
            }));
        }

        // Tìm lại dòng theo khóa ổn định: dữ liệu có thể đã được tải lại khi menu mở.
        private void ThaoTacMenuTheoTaiKhoan(string stk, string loai, string cccd, string vaiTro, bool sua)
        {
            _dangThaoTacMenu = true;
            try
            {
                int viTri = -1;
                for (int i = 0; i < dgvKhachHang.Rows.Count; i++)
                {
                    DataGridViewRow r = dgvKhachHang.Rows[i];
                    if (Convert.ToString(r.Cells["colSTK"].Value).Trim() == stk &&
                        Convert.ToString(r.Cells["colLoaiTaiKhoan"].Value).Trim() == loai &&
                        Convert.ToString(r.Cells["colCCCD"].Value).Trim() == cccd &&
                        Convert.ToString(r.Cells["colVaiTro"].Value).Trim() == vaiTro)
                    { viTri = i; break; }
                }
                if (viTri < 0)
                {
                    MessageBox.Show("Dòng được chọn không còn tồn tại. Vui lòng tải lại danh sách.");
                    return;
                }
                if (!LaDongChuSoHuu(viTri)) return;
                if (sua) SuaThongTinKhachHangTheoDong(viTri);
                else ChuyenNhomNoTheoDong(viTri);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thực hiện thao tác: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _dangThaoTacMenu = false; }
        }

        private void CapNhatTrangThaiTaiKhoan(string stk, string loai, string tenEnumMoi)
        {
            string bang, cot;
            switch (loai)
            {
                case "Khoản vay": bang = "dbo.DN_TaiKhoanVay"; cot = "SoTKVay"; break;
                case "Tiết kiệm": bang = "dbo.DN_SoTietKiem"; cot = "SoTKTK"; break;
                case "Thanh toán": bang = "dbo.DN_TaiKhoanThanhToan"; cot = "SoTKTT"; break;
                default: MessageBox.Show("Loại tài khoản không hợp lệ."); return;
            }
            // Đọc giá trị từ enum của FrmMainDN, không tự giả định thứ tự 0/1/2.
            int moi, dong, khoa, hoatDong;
            try
            {
                moi = Convert.ToInt32(Enum.Parse(typeof(FrmMainDN.TrangThaiTK), tenEnumMoi));
                dong = Convert.ToInt32(Enum.Parse(typeof(FrmMainDN.TrangThaiTK), "Dong"));
                khoa = Convert.ToInt32(Enum.Parse(typeof(FrmMainDN.TrangThaiTK), "Khoa"));
                hoatDong = Convert.ToInt32(Enum.Parse(typeof(FrmMainDN.TrangThaiTK), "HoatDong"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xác định được enum TrangThaiTK trong FrmMainDN: " + ex.Message);
                return;
            }
            string nhan = tenEnumMoi == "Dong" ? "Đã đóng" : tenEnumMoi == "Khoa" ? "Đang khóa" : "Hoạt động";
            if (MessageBox.Show("Xác nhận chuyển tài khoản " + stk + " sang '" + nhan + "'?" +
                (tenEnumMoi == "Dong" ? "\nTài khoản đã đóng không thể mở lại từ menu này." : ""),
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                var nv = BankSecurity.Current;
                if (nv == null || string.IsNullOrWhiteSpace(nv.HoTen))
                    throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập.");
                string nguoi = nv.HoTen.Trim();
                string noiDung = tenEnumMoi == "Dong" ? "Đóng tài khoản" : tenEnumMoi == "Khoa" ? "Khóa tài khoản" : "Mở khóa tài khoản";
                using (SqlConnection cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var tx = cn.BeginTransaction())
                    {
                        int trangThaiCu = tenEnumMoi == "HoatDong" ? khoa : -1;
                        string cotNgay = loai == "Khoản vay" ? "NgayCapNhatTrangThai" : "NgayCapNhatTT";
                        string dieuKienDong = loai == "Khoản vay" ? "SoTienVay<=SoTienDaTra" :
                            loai == "Tiết kiệm" ? "SoTienGui=0" : "SoDu=0 AND SoTienPhongToa=0";
                        string sql = "UPDATE " + bang + " SET TrangThai=@moi, " + cotNgay + "=GETDATE() WHERE " + cot +
                            "=@stk AND TrangThai<>@dong AND (@cu=-1 OR TrangThai=@cu) AND TrangThai<>@moi" +
                            (tenEnumMoi == "Dong" ? " AND (" + dieuKienDong + ")" : "");
                        using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
                        {
                            cmd.Parameters.Add("@moi", SqlDbType.Int).Value = moi;
                            cmd.Parameters.Add("@dong", SqlDbType.Int).Value = dong;
                            cmd.Parameters.Add("@cu", SqlDbType.Int).Value = trangThaiCu;
                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 50).Value = stk;
                            if (cmd.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("Tài khoản không tồn tại, trạng thái đã thay đổi hoặc còn số dư, phong tỏa hay dư nợ chưa thanh toán.");
                        }
                        string sqlLS;
                        if (loai == "Khoản vay")
                            sqlLS = @"INSERT INTO dbo.DN_LichSuGiaoDichVay
                                (SoTKVay,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,PhiPhat,TongTien,SoDuConLai,GhiChu,NguoiThucHien)
                                SELECT SoTKVay,GETDATE(),@nd,0,0,0,0,CASE WHEN SoTienVay>SoTienDaTra THEN SoTienVay-SoTienDaTra ELSE 0 END,@nd,@nguoi
                                FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk";
                        else if (loai == "Tiết kiệm")
                            sqlLS = @"INSERT INTO dbo.DN_LichSuGiaoDichTietKiem
                                (SoTKTK,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,TongTienNhan,GhiChu,NguoiThucHien)
                                SELECT SoTKTK,GETDATE(),@nd,0,0,0,@nd,@nguoi FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk";
                        else
                            sqlLS = @"INSERT INTO dbo.DN_LichSuGiaoDich
                                (SoTKTT,ThoiGian,NoiDung,SoTien,SoDuSau,NguoiThucHien)
                                SELECT SoTKTT,GETDATE(),@nd,0,SoDu,@nguoi FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk";
                        using (var cmd = new SqlCommand(sqlLS, cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@stk", stk);
                            cmd.Parameters.AddWithValue("@nd", noiDung);
                            cmd.Parameters.AddWithValue("@nguoi", nguoi);
                            if (cmd.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("Không thể ghi lịch sử hoạt động.");
                        }
                        tx.Commit();
                    }
                }
                DataChanged?.Invoke(this, EventArgs.Empty);
                LayDanhSachKhachHangTuFrmMain();
                if (_taiDuLieuThanhCong) HienThiDanhSach(txtCCCD.Text.Trim());
                MessageBox.Show("Đã cập nhật tài khoản " + stk + " sang trạng thái " + nhan + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật SQL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChuyenNhomNoTheoDong(int rowIndex)
        {
            if (!LaDongChuSoHuu(rowIndex)) return;

            DataGridViewRow row = dgvKhachHang.Rows[rowIndex];
            string cccd = row.Cells["colCCCD"].Value?.ToString() ?? "";
            string stk = row.Cells["colSTK"].Value?.ToString() ?? "";
            string hoTen = row.Cells["colHoTen"].Value?.ToString() ?? "";

            if (string.IsNullOrEmpty(cccd)) return;

            using (Form dlgChuyenNhom = new Form())
            {
                string nhanDinhDanh = NhanDinhDanhTheoVaiTro(Convert.ToString(row.Cells["colVaiTro"].Value));
                dlgChuyenNhom.Text = $"Chuyển nhóm nợ theo {nhanDinhDanh}: {cccd}";
                dlgChuyenNhom.Size = new Size(420, 220);
                dlgChuyenNhom.StartPosition = FormStartPosition.CenterParent;
                dlgChuyenNhom.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlgChuyenNhom.MaximizeBox = false;
                dlgChuyenNhom.MinimizeBox = false;

                Label lblPrompt = new Label { Text = $"Chọn nhóm nợ mới cho tất cả TK của {nhanDinhDanh} {cccd} ({hoTen}):", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                ComboBox cboNhomMoi = new ComboBox { Location = new Point(20, 50), Width = 360, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5F) };

                cboNhomMoi.Items.Add("Nhóm 1 (Đủ tiêu chuẩn)");
                cboNhomMoi.Items.Add("Nhóm 2 (Cần chú ý)");
                cboNhomMoi.Items.Add("Nhóm 3 (Dưới tiêu chuẩn)");
                cboNhomMoi.Items.Add("Nhóm 4 (Nghi ngờ)");
                cboNhomMoi.Items.Add("Nhóm 5 (Mất vốn)");

                int currentCode = LayCodeNhomNoCaoNhatTheoCCCD(cccd);
                if (currentCode >= 1 && currentCode <= 5) cboNhomMoi.SelectedIndex = currentCode - 1;
                else cboNhomMoi.SelectedIndex = 0;

                Button btnXacNhan = new Button { Text = "Xác nhận", Location = new Point(100, 110), Size = new Size(100, 35), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(210, 110), Size = new Size(90, 35), Font = new Font("Segoe UI", 9F) };

                btnXacNhan.Click += (s, ev) =>
                {
                    int newCode = cboNhomMoi.SelectedIndex + 1;
                    try { LuuNhomNoTheoCCCD(cccd, newCode); LayDanhSachKhachHangTuFrmMain(); }
                    catch (Exception ex) { MessageBox.Show("Lỗi lưu nhóm nợ SQL: " + ex.Message); return; }

                    MessageBox.Show($"Đã chuyển tất cả tài khoản thuộc {nhanDinhDanh} {cccd} sang {cboNhomMoi.SelectedItem}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlgChuyenNhom.DialogResult = DialogResult.OK;
                };

                btnHuy.Click += (s, ev) => dlgChuyenNhom.Close();

                dlgChuyenNhom.Controls.AddRange(new Control[] { lblPrompt, cboNhomMoi, btnXacNhan, btnHuy });
                if (dlgChuyenNhom.ShowDialog() == DialogResult.OK)
                {
                    HienThiDanhSach(txtCCCD.Text.Trim());
                }
            }
        }

        private void LuuNhomNoTheoCCCD(string cccd, int nhom)
        {
            using (var cn = new SqlConnection(_connStr))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@";WITH Chu AS (
                    SELECT CCCD FROM dbo.DN_KhachHang WHERE CCCD=@cccd
                    UNION SELECT CCCD_ChuSoHuu FROM dbo.DN_DongSoHuu WHERE CCCD=@cccd
                ), TK AS (
                    SELECT SoTKVay AS STK FROM dbo.DN_TaiKhoanVay WHERE CCCD IN (SELECT CCCD FROM Chu)
                    UNION SELECT SoTKTK FROM dbo.DN_SoTietKiem WHERE CCCD IN (SELECT CCCD FROM Chu)
                    UNION SELECT SoTKTT FROM dbo.DN_TaiKhoanThanhToan WHERE CCCD IN (SELECT CCCD FROM Chu)
                )
                MERGE dbo.DN_NhomNoTaiKhoan AS dest USING TK AS src ON dest.SoTaiKhoan=src.STK
                WHEN MATCHED THEN UPDATE SET NhomNo=@nhom, NgayCapNhat=GETDATE()
                WHEN NOT MATCHED THEN INSERT(SoTaiKhoan,NhomNo) VALUES(src.STK,@nhom);", cn))
                { cmd.Parameters.AddWithValue("@cccd", cccd); cmd.Parameters.AddWithValue("@nhom", nhom); cmd.ExecuteNonQuery(); }
            }
        }

        private object TimNguoiTheoDong(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvKhachHang.Rows.Count || danhSachKhachHang == null) return null;
            var row = dgvKhachHang.Rows[rowIndex];
            string cccd = Convert.ToString(row.Cells["colCCCD"].Value);
            string stk = Convert.ToString(row.Cells["colSTK"].Value);
            string loai = Convert.ToString(row.Cells["colLoaiTaiKhoan"].Value);
            bool laChu = Convert.ToString(row.Cells["colVaiTro"].Value) != "Người được ủy quyền";
            if (laChu) return danhSachKhachHang.Cast<object>().FirstOrDefault(k => LayString(k, "CCCD") == cccd);
            if (_loaiCoBangDongSoHuuRieng.Contains(loai))
                return _dongSoHuuTheoSoTK.TryGetValue(loai + "|" + stk, out var ds) ? ds.FirstOrDefault(d => d.CCCD == cccd) : null;
            foreach (object kh in danhSachKhachHang)
                foreach (object d in LayDanhSach(kh, "DanhSachDongSoHuu"))
                    if (LayString(d, "CCCD") == cccd) return d;
            return null;
        }

        private void SuaThongTinKhachHangTheoDong(int rowIndex)
        {
            if (!LaDongChuSoHuu(rowIndex)) return;
            object nguoi = TimNguoiTheoDong(rowIndex);
            if (nguoi == null) return;

            bool laChu = NhanDinhDanhTheoVaiTro(Convert.ToString(dgvKhachHang.Rows[rowIndex].Cells["colVaiTro"].Value)) == "MST";
            string nhanDinhDanh = laChu ? "MST" : "CCCD";
            string cccdCu = LayString(nguoi, "CCCD");
            string hoTenCu = LayString(nguoi, "HoTen");
            string sdtCu = LayString(nguoi, "SoDienThoai");

            using (Form dlg = new Form())
            {
                dlg.Text = laChu ? "Sửa thông tin chủ sở hữu" : "Sửa thông tin người được ủy quyền";
                dlg.Size = new Size(390, 255);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                TextBox txtC = new TextBox { Text = cccdCu, Location = new Point(135, 20), Width = 210, MaxLength = 12, ReadOnly = true };
                TextBox txtN = new TextBox { Text = hoTenCu, Location = new Point(135, 60), Width = 210 };
                TextBox txtP = new TextBox { Text = sdtCu, Location = new Point(135, 100), Width = 210, MaxLength = 11 };
                txtC.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                txtP.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);

                Button btnLuu = new Button { Text = "Lưu", Location = new Point(135, 145), Width = 95, BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(245, 145), Width = 95 };
                dlg.Controls.AddRange(new Control[] {
                    new Label { Text = nhanDinhDanh + " (12 số):", Location = new Point(25, 22), AutoSize = true }, txtC,
                    new Label { Text = "Họ và Tên:", Location = new Point(25, 62), AutoSize = true }, txtN,
                    new Label { Text = "Số điện thoại:", Location = new Point(25, 102), AutoSize = true }, txtP,
                    btnLuu, btnHuy
                });

                btnLuu.Click += (s, ev) =>
                {
                    string c = txtC.Text.Trim(), n = txtN.Text.Trim(), p = txtP.Text.Trim();
                    if (c.Length != 12 || string.IsNullOrWhiteSpace(n)) { MessageBox.Show(nhanDinhDanh + " phải đủ 12 số và Họ tên không được để trống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                    if (p.Length < 10 || p.Length > 11 || !p.All(ch => ch >= '0' && ch <= '9')) { MessageBox.Show("Số điện thoại phải gồm 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                    try
                    {
                        using (var cn = new SqlConnection(_connStr))
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                string[] tables = { "DN_KhachHang", "DN_DongSoHuu", "DN_DongSoHuuTaiKhoanThanhToan", "DN_DongSoHuuSoTietKiem", "DN_DongSoHuuTaiKhoanVay" };
                                int changed = 0;
                                foreach (string table in tables)
                                {
                                    using (var exists = new SqlCommand("SELECT OBJECT_ID(@b,'U')", cn, tx))
                                    {
                                        exists.Parameters.AddWithValue("@b", "dbo." + table);
                                        if (exists.ExecuteScalar() == DBNull.Value) continue;
                                    }
                                    using (var check = new SqlCommand("SELECT COUNT(*) FROM dbo." + table + " WHERE SoDienThoai=@p AND CCCD<>@c", cn, tx))
                                    {
                                        check.Parameters.AddRange(new[] { P("@p", p), P("@c", cccdCu) });
                                        if (Convert.ToInt32(check.ExecuteScalar()) > 0) throw new InvalidOperationException("Số điện thoại đã tồn tại trong hệ thống!");
                                    }
                                    // DN: MST của chủ và CCCD của đại diện là hai loại định danh khác nhau.
                                    if (true && ((laChu && table != "DN_KhachHang") || (!laChu && table == "DN_KhachHang"))) continue;
                                    using (var cmd = new SqlCommand("UPDATE dbo." + table + " SET HoTen=@n,SoDienThoai=@p WHERE CCCD=@c", cn, tx))
                                    {
                                        cmd.Parameters.AddRange(new[] { P("@n", n), P("@p", p), P("@c", cccdCu) });
                                        changed += cmd.ExecuteNonQuery();
                                    }
                                }
                                if (changed == 0) throw new InvalidOperationException("Không tìm thấy người cần cập nhật.");
                                tx.Commit();
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi cập nhật SQL: " + ex.Message); return; }
                    PropertyInfo pc = nguoi.GetType().GetProperty("CCCD");
                    PropertyInfo pn = nguoi.GetType().GetProperty("HoTen");
                    PropertyInfo pp = nguoi.GetType().GetProperty("SoDienThoai");
                    if (pc != null) pc.SetValue(nguoi, c, null);
                    if (pn != null) pn.SetValue(nguoi, n, null);
                    if (pp != null) pp.SetValue(nguoi, p, null);

                    if (laChu && khachHangDangChon == nguoi) khachHangDangChon = nguoi;
                    LayDanhSachKhachHangTuFrmMain();
                    if (_taiDuLieuThanhCong)
                    {
                        txtCCCD.Text = c;
                        HienThiDanhSach(c);
                        CapNhatThongTinTongQuan();
                    }
                    dlg.DialogResult = DialogResult.OK;
                };
                btnHuy.Click += (s, ev) => dlg.Close();
                dlg.ShowDialog(this);
            }
        }

        private void CapNhatThongTinTongQuan()
        {
            if (dgvKhachHang == null) return;
            int soDong = dgvKhachHang.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow)
                .Select(r => Tuple.Create(Convert.ToString(r.Cells["colLoaiTaiKhoan"].Value), Convert.ToString(r.Cells["colSTK"].Value))).Distinct().Count();
            lblTongTaiKhoan.Text = $"Tổng tài khoản: {soDong:N0}";
            if (string.IsNullOrWhiteSpace(txtCCCD.Text)) lblKhachHangDangChon.Text = "Tất cả khách hàng";
        }

        private object LayGiaTri(object obj, string propertyName)
        {
            if (obj == null) return null;
            var key = Tuple.Create(obj.GetType(), propertyName);
            PropertyInfo property;
            if (!_cacheThuocTinh.TryGetValue(key, out property))
            {
                property = key.Item1.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                _cacheThuocTinh[key] = property;
            }
            if (property == null) return null;
            try { return property.GetValue(obj, null); } catch { return null; }
        }

        private string LayString(object obj, string propertyName)
        {
            object value = LayGiaTri(obj, propertyName);
            return value?.ToString() ?? "";
        }

        private decimal LayDecimal(object obj, string propertyName)
        {
            object value = LayGiaTri(obj, propertyName);
            if (value == null) return 0;
            try { return Convert.ToDecimal(value); } catch { return 0; }
        }

        private double LayDouble(object obj, string propertyName)
        {
            object value = LayGiaTri(obj, propertyName);
            if (value == null) return 0;
            try { return Convert.ToDouble(value); } catch { return 0; }
        }

        private IList LayDanhSach(object obj, string propertyName)
        {
            object value = LayGiaTri(obj, propertyName);
            if (value is IList list) return list;
            return new ArrayList();
        }

        private string LayTien(decimal tien) => $"{tien:N0} VNĐ";

        private string LayLaiSuat(object taiKhoan)
        {
            double lai = LayDouble(taiKhoan, "LaiSuat");
            return $"{lai:0.##}% / năm";
        }

        private int LayCodeNhomNoGoc(object taiKhoan)
        {
            if (taiKhoan == null) return 1;

            string stk = LayString(taiKhoan, "SoTKVay");
            if (string.IsNullOrEmpty(stk)) stk = LayString(taiKhoan, "SoTKTK");
            if (string.IsNullOrEmpty(stk)) stk = LayString(taiKhoan, "SoTKTT");

            if (!string.IsNullOrEmpty(stk) && _nhomNoSql.TryGetValue(stk, out int code))
            {
                return code;
            }

            decimal soTienVay = LayDecimal(taiKhoan, "SoTienVay");
            if (soTienVay > 0)
            {
                decimal soTienDaTra = LayDecimal(taiKhoan, "SoTienDaTra");
                decimal gocDaTra = soTienDaTra; // SQL la nguon du lieu goc da tra duy nhat
                decimal duNo = Math.Max(0, soTienVay - gocDaTra);
                if (duNo <= 0) return 1;

                object ngayMoVal = LayGiaTri(taiKhoan, "NgayMo");
                if (ngayMoVal is DateTime ngayMo)
                {
                    string kyHan = LayString(taiKhoan, "KyHan");
                    int months = ParseKyHanToMonths(kyHan);
                    DateTime ngayDaoHan = ngayMo.AddMonths(months);

                    if (DateTime.Now > ngayDaoHan)
                    {
                        int soNgayQuaHan = (int)(DateTime.Now - ngayDaoHan).TotalDays;
                        if (soNgayQuaHan >= 10 && soNgayQuaHan <= 90) return 2;
                        if (soNgayQuaHan > 90 && soNgayQuaHan <= 180) return 3;
                        if (soNgayQuaHan > 180 && soNgayQuaHan <= 360) return 4;
                        if (soNgayQuaHan > 360) return 5;
                    }
                }
            }

            object value = LayGiaTri(taiKhoan, "NhomNo") ?? LayGiaTri(taiKhoan, "TenNhomNo") ?? LayGiaTri(taiKhoan, "NhomNoStr");
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string str = value.ToString();
                if (str.Contains("5") || str.Contains("Mất vốn")) return 5;
                if (str.Contains("4") || str.Contains("Nghi ngờ")) return 4;
                if (str.Contains("3") || str.Contains("Dưới tiêu chuẩn")) return 3;
                if (str.Contains("2") || str.Contains("Cần chú ý")) return 2;
            }
            return 1;
        }

        private int LayCodeNhomNoCaoNhatTheoCCCD(string targetCCCD)
        {
            if (string.IsNullOrWhiteSpace(targetCCCD) || danhSachKhachHang == null) return 1;

            int maxCode = 1;
            HashSet<string> dsCCCDLienQuan = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { targetCCCD };

            foreach (object kh in danhSachKhachHang)
            {
                string cccdKh = LayString(kh, "CCCD");
                IList dsDSH = LayDanhSach(kh, "DanhSachDongSoHuu");

                bool coLienQuan = cccdKh.Equals(targetCCCD, StringComparison.OrdinalIgnoreCase);
                if (!coLienQuan)
                {
                    foreach (object dsh in dsDSH)
                    {
                        if (LayString(dsh, "CCCD").Equals(targetCCCD, StringComparison.OrdinalIgnoreCase))
                        {
                            coLienQuan = true;
                            break;
                        }
                    }
                }

                if (coLienQuan)
                {
                    if (!string.IsNullOrEmpty(cccdKh)) dsCCCDLienQuan.Add(cccdKh);
                    foreach (object dsh in dsDSH)
                    {
                        string cccdDsh = LayString(dsh, "CCCD");
                        if (!string.IsNullOrEmpty(cccdDsh)) dsCCCDLienQuan.Add(cccdDsh);
                    }
                }
            }

            foreach (object kh in danhSachKhachHang)
            {
                string cccdKh = LayString(kh, "CCCD");
                IList dsDSH = LayDanhSach(kh, "DanhSachDongSoHuu");

                bool thuocNhom = dsCCCDLienQuan.Contains(cccdKh);
                if (!thuocNhom)
                {
                    foreach (object dsh in dsDSH)
                    {
                        if (dsCCCDLienQuan.Contains(LayString(dsh, "CCCD")))
                        {
                            thuocNhom = true;
                            break;
                        }
                    }
                }

                if (thuocNhom)
                {
                    var listLists = new List<IList> {
                        LayDanhSach(kh, "DanhSachVay"),
                        LayDanhSach(kh, "DanhSachTietKiem"),
                        LayDanhSach(kh, "DanhSachThanhToan")
                    };

                    foreach (var list in listLists)
                    {
                        foreach (object item in list)
                        {
                            int code = LayCodeNhomNoGoc(item);
                            if (code > maxCode) maxCode = code;
                        }
                    }
                }
            }

            return maxCode;
        }

        private string LayNhomNo(object taiKhoan, string cccd = "")
        {
            if (taiKhoan == null) return "Nhóm 1 (Đủ tiêu chuẩn)";

            if (!string.IsNullOrEmpty(cccd))
            {
                int maxCode = LayCodeNhomNoCaoNhatTheoCCCD(cccd);
                return GetTenNhomNoStandard(maxCode);
            }

            return GetTenNhomNoStandard(LayCodeNhomNoGoc(taiKhoan));
        }

        private static string GetTenNhomNoStandard(int code)
        {
            switch (code)
            {
                case 1: return "Nhóm 1 (Đủ tiêu chuẩn)";
                case 2: return "Nhóm 2 (Cần chú ý)";
                case 3: return "Nhóm 3 (Dưới tiêu chuẩn)";
                case 4: return "Nhóm 4 (Nghi ngờ)";
                case 5: return "Nhóm 5 (Mất vốn)";
                default: return "Nhóm 1 (Đủ tiêu chuẩn)";
            }
        }

        private static string ChuonHoaFormatNhomNo(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Nhóm 1 (Đủ tiêu chuẩn)";
            if (input.Contains("1")) return "Nhóm 1 (Đủ tiêu chuẩn)";
            if (input.Contains("2")) return "Nhóm 2 (Cần chú ý)";
            if (input.Contains("3")) return "Nhóm 3 (Dưới tiêu chuẩn)";
            if (input.Contains("4")) return "Nhóm 4 (Nghi ngờ)";
            if (input.Contains("5")) return "Nhóm 5 (Mất vốn)";
            return input;
        }

        private static int ParseKyHanToMonths(string kyHanStr)
        {
            if (string.IsNullOrEmpty(kyHanStr)) return 12;
            string clean = new string(kyHanStr.Where(char.IsDigit).ToArray());
            if (int.TryParse(clean, out int months)) return months;
            return 12;
        }

        private string LayNgayLam(object taiKhoan)
        {
            object value = LayGiaTri(taiKhoan, "NgayLam") ?? LayGiaTri(taiKhoan, "NgayCapNhatTT") ?? LayGiaTri(taiKhoan, "NgayCapNhatTrangThai") ?? LayGiaTri(taiKhoan, "NgayMo");
            if (value is DateTime date) return date.ToString("dd/MM/yyyy HH:mm:ss");
            if (value != null && DateTime.TryParse(value.ToString(), out DateTime parsed)) return parsed.ToString("dd/MM/yyyy HH:mm:ss");
            return "-";
        }

        private string LayTrangThai(object taiKhoan)
        {
            if (taiKhoan == null) return "Hoạt động";

            object value = LayGiaTri(taiKhoan, "TrangThai");
            if (value == null) return "Hoạt động";
            string text = value.ToString();
            if (text == "HoatDong" || text == "Hoạt động") return "Hoạt động";
            if (text == "Khoa" || text == "Đang khóa" || text == "Khóa khẩn cấp") return "Đang khóa";
            if (text == "Dong" || text == "Đã đóng" || text == "Đóng") return "Đã đóng";
            return text;
        }
    }
}