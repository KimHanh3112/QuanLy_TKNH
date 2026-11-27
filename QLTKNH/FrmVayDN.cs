using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QLTKNH
{
    public partial class FrmVayDN : Form
    {
        // Sự kiện thông báo khi dữ liệu tài khoản vay có thay đổi
        public event EventHandler DataChanged;

        // Chỉ dùng khi ghi giao dịch mới: lưu đúng họ tên người đang đăng nhập tại thời điểm thao tác.
        private static string LayTenNguoiThucHien()
        {
            var nhanVien = BankSecurity.Current;
            if (nhanVien == null || string.IsNullOrWhiteSpace(nhanVien.HoTen))
                throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập. Vui lòng đăng nhập lại trước khi thực hiện giao dịch.");
            return nhanVien.HoTen.Trim();
        }


        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        // Màu sắc đồng bộ giao diện
        private readonly Color colorContentBg = Color.FromArgb(15, 23, 42);
        private readonly Color colorCardBg = Color.FromArgb(30, 41, 59);
        private readonly Color colorActiveBlue = Color.FromArgb(37, 99, 235);
        private readonly Color colorGreenButton = Color.FromArgb(22, 163, 74);
        private readonly Color colorTextMuted = Color.FromArgb(160, 174, 192);

        private Dictionary<string, List<FrmMainDN.DongSoHuuData>> _uyQuyenTheoKhoanVayDN =
            new Dictionary<string, List<FrmMainDN.DongSoHuuData>>(StringComparer.OrdinalIgnoreCase);
        private IEnumerable<FrmMainDN.DongSoHuuData> LayUyQuyenKhoanVayDN(string stk)
        {
            List<FrmMainDN.DongSoHuuData> ds;
            return _uyQuyenTheoKhoanVayDN.TryGetValue(stk, out ds) ? ds : Enumerable.Empty<FrmMainDN.DongSoHuuData>();
        }

        // Danh sách khách hàng dùng chung
        private List<FrmMainDN.KhachHangData> listKhachHang = new List<FrmMainDN.KhachHangData>();

        // Thuộc tính phục vụ lọc từ FrmMainDN truyền sang
        private string _selectedCCCD = "";
        public string SelectedCCCD
        {
            get => _selectedCCCD;
            set
            {
                string val = value ?? "";
                ResolveCCCDAndSoTK(val);
            }
        }
        public string CCCD
        {
            get => SelectedCCCD;
            set => SelectedCCCD = value;
        }

        public List<string> SelectedSoTKList { get; set; } = new List<string>();

        private string _selectedSoTK = "";
        public string SelectedSoTK
        {
            get => _selectedSoTK;
            set
            {
                _selectedSoTK = value ?? "";
                if (!string.IsNullOrEmpty(_selectedSoTK))
                {
                    if (SelectedSoTKList == null) SelectedSoTKList = new List<string>();
                    if (!SelectedSoTKList.Contains(_selectedSoTK))
                    {
                        SelectedSoTKList.Add(_selectedSoTK);
                    }
                }
            }
        }
        public string SoTK
        {
            get => SelectedSoTK;
            set => SelectedSoTK = value;
        }

        // Hàm tự động phân giải khi truyền nhầm Số Tài Khoản vào tham số CCCD
        private void ResolveCCCDAndSoTK(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            // Kiểm tra xem input là CCCD hay Số Tài Khoản Vay
            var khMatch = listKhachHang.FirstOrDefault(k => k.DanhSachVay.Any(v => v.SoTKVay.Equals(input, StringComparison.OrdinalIgnoreCase)));
            if (khMatch != null)
            {
                // Nếu input là Số TK -> Lấy đúng CCCD của khách hàng đó
                _selectedCCCD = khMatch.CCCD;
                SelectedSoTK = input;
            }
            else
            {
                _selectedCCCD = input;
            }

            if (txtCCCDSearch != null && !string.IsNullOrEmpty(_selectedCCCD))
            {
                txtCCCDSearch.Text = _selectedCCCD;
            }
        }

        // Lưu trữ lịch sử thanh toán
        private static Dictionary<string, List<LichSuGiaoDichVay>> dictLichSuGiaoDich = new Dictionary<string, List<LichSuGiaoDichVay>>();

        // Lưu trữ Nhóm nợ tùy chỉnh khi thực hiện Chuyển nhóm nợ
        public static Dictionary<string, int> dictCustomNhomNo = new Dictionary<string, int>();

        // Quản lý tổng tiền gốc đã trả riêng biệt để tính dư nợ gốc còn lại cho lãi suất
        public static Dictionary<string, decimal> dictSoTienGocDaTra = new Dictionary<string, decimal>();

        public static decimal GetSoTienGocDaTra(string stk, decimal fallback)
        {
            if (!string.IsNullOrEmpty(stk) && dictSoTienGocDaTra.TryGetValue(stk, out decimal val))
                return val;
            return fallback;
        }

        // Lớp lưu trữ thông tin dòng hiển thị trên DataGridView
        private class VayGridRowModel
        {
            public string SoTKVay { get; set; }
            public string CCCD { get; set; }
            public string HoTen { get; set; }
            public string SoDienThoai { get; set; }
            public decimal SoTienVay { get; set; }
            public decimal SoTienDaTra { get; set; }
            public decimal SoTienGocDaTra { get; set; }
            public decimal TongTienDaThanhToan { get; set; }
            public decimal DuNo => Math.Max(0, SoTienVay - SoTienGocDaTra);
            public double LaiSuat { get; set; }
            public string KyHan { get; set; }
            public DateTime NgayMo { get; set; }
            public DateTime? NgayCapNhatTrangThai { get; set; }
            public string VaiTro { get; set; }
            public FrmMainDN.TrangThaiTK TrangThai { get; set; }
            public FrmMainDN.TaiKhoanVayItem RawVayItem { get; set; }
            public FrmMainDN.KhachHangData RawKhachHang { get; set; }

            public int NhomNoCode { get; set; }
            public string NhomNoTen { get; set; }
        }

        public class LichSuGiaoDichVay
        {
            public DateTime NgayGiaoDich { get; set; }
            public string SoTKVay { get; set; }
            public string PhuongThuc { get; set; }
            public decimal TienGocTra { get; set; }
            public decimal TienLai { get; set; }
            public decimal PhiPhat { get; set; }
            public decimal TongThanhToan { get; set; }
            public string GhiChu { get; set; }
        }


        // Chỉ định dạng giao diện các hộp thoại phụ; không tác động dữ liệu và sự kiện.
        private void DongBoMauFormPhu(Form dialog)
        {
            dialog.BackColor = colorContentBg;
            dialog.ForeColor = Color.White;
            dialog.Font = new Font("Segoe UI", 9.5F);
            DinhDangControlFormPhu(dialog);
        }

        private void DinhDangControlFormPhu(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is DataGridView grid)
                {
                    grid.BackgroundColor = colorContentBg;
                    grid.GridColor = Color.FromArgb(51, 65, 85);
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = colorActiveBlue;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = colorActiveBlue;
                    grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
                    grid.DefaultCellStyle.BackColor = colorContentBg;
                    grid.DefaultCellStyle.ForeColor = Color.White;
                    grid.DefaultCellStyle.SelectionBackColor = colorActiveBlue;
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(22, 31, 52);
                    grid.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
                    grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = colorActiveBlue;
                    grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
                }
                else if (c is Button button)
                {
                    string name = (button.Name + " " + button.Text).ToLowerInvariant();
                    bool huy = name.Contains("hủy") || name.Contains("huỷ") || name.Contains("đóng") || name.Contains("thoát");
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = huy ? Color.FromArgb(100, 116, 139) : Color.FromArgb(96, 165, 250);
                    button.BackColor = huy ? Color.FromArgb(51, 65, 85) : colorActiveBlue;
                    button.ForeColor = Color.White;
                    button.Font = new Font("Segoe UI", button.Font.Size, FontStyle.Bold);
                    button.Cursor = Cursors.Hand;
                }
                else if (c is TextBox || c is ComboBox || c is NumericUpDown)
                {
                    c.BackColor = colorCardBg;
                    c.ForeColor = Color.White;
                }
                else if (c is GroupBox)
                {
                    c.BackColor = colorCardBg;
                    c.ForeColor = Color.FromArgb(147, 197, 253);
                    c.Font = new Font("Segoe UI", c.Font.Size, FontStyle.Bold);
                }
                else if (c is Panel || c is TableLayoutPanel || c is FlowLayoutPanel)
                {
                    c.BackColor = colorCardBg;
                    c.ForeColor = Color.White;
                }
                else if (c is Label label)
                {
                    label.ForeColor = Color.White;
                    string value = label.Text.ToLowerInvariant();
                    if (value.Contains("tổng") || value.Contains("tối đa") || value.Contains("dư nợ"))
                    {
                        label.ForeColor = Color.FromArgb(125, 211, 252);
                        label.Font = new Font("Segoe UI", label.Font.Size, FontStyle.Bold);
                    }
                    if (value.Contains("phí phạt") || value.Contains("quá hạn"))
                        label.ForeColor = Color.FromArgb(248, 113, 113);
                }
                else if (c is RadioButton || c is CheckBox)
                {
                    c.ForeColor = Color.White;
                    c.BackColor = c.Parent is GroupBox ? colorCardBg : colorContentBg;
                }
                if (c.HasChildren && !(c is DataGridView)) DinhDangControlFormPhu(c);
            }
        }

        #region Overloaded Constructors


        // GroupBox bao phủ toàn bộ thanh tìm kiếm; chỉ vùng trống / nhãn mới làm mới.
        private GroupBox _searchGroup;
        private bool _searchRefreshing;
        private bool _dangDungBangVay;
        private readonly Timer _timerLocVay = new Timer { Interval = 250 };
        private HashSet<string> _cacheVayBangSo;
        private Dictionary<string, decimal> _cacheLaiVaPhi;
        private void LocVayCham()
        {
            if (_searchRefreshing || _dangDungBangVay || IsDisposed || Disposing) return;
            _timerLocVay.Stop();
            if (IsHandleCreated) _timerLocVay.Start();
        }
        private Font _fontNhomNoVay;

        private void SetupSearchGroup()
        {
            if (_searchGroup != null) return;
            _searchGroup = CreateGroupBoxDesign1();
            // Giữ nguyên tọa độ, kích thước và sự kiện của tất cả control trong Designer.
            Control[] children = panelTopSearch.Controls.Cast<Control>().ToArray();
            panelTopSearch.SuspendLayout();
            foreach (Control child in children)
            {
                panelTopSearch.Controls.Remove(child);
                _searchGroup.Controls.Add(child);
                if (child is Label)
                {
                    child.Cursor = Cursors.Hand;
                    child.Click += (s, e) => RefreshFromSearchGroup();
                }
            }
            panelTopSearch.Controls.Add(_searchGroup);
            panelTopSearch.ResumeLayout(true);
            _searchGroup.Click += (s, e) => RefreshFromSearchGroup();
            panelTopSearch.Click += (s, e) => RefreshFromSearchGroup();
        }

        private void RefreshFromSearchGroup()
        {
            if (_searchRefreshing) return;
            _searchRefreshing = true;
            try
            {
                txtCCCDSearch.Clear();
                _selectedCCCD = "";
                _selectedSoTK = "";
                if (SelectedSoTKList == null) SelectedSoTKList = new List<string>();
                SelectedSoTKList.Clear();
                cboLocNhom.SelectedIndex = 0;
                cboLocKyHan.SelectedIndex = 0;
                cboLocThoiGian.SelectedIndex = 0;
                dtpTuNgay.Value = DateTime.Today;
                dtpDenNgay.Value = DateTime.Today;
                RefreshSqlData();
                dgvVay.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể làm mới dữ liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _searchRefreshing = false; }
        }

        public FrmVayDN()
        {
            InitializeComponent();
            DongBoMauDanhSachVay();
            // Giảm nhấp nháy khi dựng lại bảng, không thay đổi giao diện.
            typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dgvVay, true, null);
            _fontNhomNoVay = new Font(dgvVay.Font, FontStyle.Bold);
            dgvVay.CellFormatting += ToMauVaiTroTrangThaiVay;
            foreach (DataGridViewColumn col in dgvVay.Columns) col.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvVay.CellFormatting += (sender, args) =>
            {
                if (args.RowIndex < 0 || args.ColumnIndex < 0 || dgvVay.Columns[args.ColumnIndex].Name != "colHoTen") return;
                if (Convert.ToString(dgvVay.Rows[args.RowIndex].Cells["colVaiTro"].Value) == "Người được ủy quyền")
                    args.Value = "↳ " + Convert.ToString(args.Value);
            };
            this.Disposed += (s, e) => _fontNhomNoVay.Dispose();
            LoadDataFromSql();
            SetupControls();
            SetupEvents();
            _timerLocVay.Tick += (s, e) => { _timerLocVay.Stop(); HienThiDanhSachVay(false); };
            Disposed += (s, e) => _timerLocVay.Dispose();
            SetupSearchGroup();
            this.Load += FrmVay_Load;
        }

        // Đồng bộ màu bảng danh sách với FrmTietKiemDN, không đổi bố cục hay logic.
        private void DongBoMauDanhSachVay()
        {
            dgvVay.BackgroundColor = Color.FromArgb(15, 23, 42);
            dgvVay.GridColor = Color.FromArgb(30, 64, 220);
            dgvVay.EnableHeadersVisualStyles = false;

            dgvVay.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 41, 59);
            dgvVay.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvVay.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(31, 41, 59);
            dgvVay.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            dgvVay.DefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvVay.DefaultCellStyle.ForeColor = Color.White;
            dgvVay.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
            dgvVay.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvVay.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(22, 31, 52);
            dgvVay.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
            dgvVay.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
            dgvVay.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            dgvVay.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dgvVay.RowHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        // Chỉ định dạng hai cột; không thay đổi giá trị hoặc nghiệp vụ.
        private void ToMauVaiTroTrangThaiVay(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null) return;
            bool laBangChinh = ReferenceEquals(grid, dgvVay);
            string cot = grid.Columns[e.ColumnIndex].Name;
            bool vaiTro = laBangChinh ? e.ColumnIndex == 12 : cot == "role";
            bool trangThai = laBangChinh ? e.ColumnIndex == 13 : cot == "status";
            string value = Convert.ToString(e.Value).Trim();
            Color mau;
            if (vaiTro)
            {
                if (value == "Người được ủy quyền") mau = Color.FromArgb(192, 132, 252);
                else if (value == "Chủ" || value == "Chủ sở hữu" || value == "Chủ tài khoản")
                    mau = Color.FromArgb(96, 165, 250);
                else return;
            }
            else if (trangThai)
            {
                if (value == "Hoạt động") mau = Color.FromArgb(34, 197, 94);
                else if (value == "Đang khóa") mau = Color.FromArgb(249, 115, 22);
                else if (value == "Đã đóng") mau = Color.FromArgb(239, 68, 68);
                else return;
            }
            else return;
            e.CellStyle.ForeColor = mau;
            // Trên nền chọn xanh, dùng màu sáng hơn để chữ Chủ vẫn đọc được.
            e.CellStyle.SelectionForeColor = vaiTro
                ? (value == "Người được ủy quyền" ? Color.FromArgb(243, 232, 255) : Color.FromArgb(219, 234, 254))
                : mau;
            if (vaiTro) e.CellStyle.Font = _fontNhomNoVay;
        }

        public FrmVayDN(List<FrmMainDN.KhachHangData> sharedList) : this()
        {
            // Luôn đọc dữ liệu thật từ SQL, không thay bằng danh sách bộ nhớ.
        }

        public FrmVayDN(List<FrmMainDN.KhachHangData> sharedList, string cccd) : this(sharedList)
        {
            this.SelectedCCCD = cccd;
        }

        public FrmVayDN(List<FrmMainDN.KhachHangData> sharedList, string cccd, List<string> listSoTK) : this(sharedList, cccd)
        {
            if (listSoTK != null) this.SelectedSoTKList = new List<string>(listSoTK);
        }

        public FrmVayDN(List<FrmMainDN.KhachHangData> sharedList, string cccd, string soTK) : this(sharedList, cccd)
        {
            this.SelectedSoTK = soTK;
        }

        public FrmVayDN(Form mainForm, string cccd) : this()
        {
            this.SelectedCCCD = cccd;
        }

        public FrmVayDN(Form mainForm, string cccd, List<string> listSoTK) : this(mainForm, cccd)
        {
            if (listSoTK != null) this.SelectedSoTKList = new List<string>(listSoTK);
        }

        public FrmVayDN(Form mainForm, string cccd, string soTK) : this(mainForm, cccd)
        {
            this.SelectedSoTK = soTK;
        }

        public FrmVayDN(string cccd) : this()
        {
            this.SelectedCCCD = cccd;
        }

        public FrmVayDN(string cccd, string soTK) : this(cccd)
        {
            this.SelectedSoTK = soTK;
        }
        #endregion

        private void FrmVay_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedCCCD))
            {
                if (txtCCCDSearch.Text != SelectedCCCD)
                {
                    txtCCCDSearch.Text = SelectedCCCD;
                    // Tải ngay lần đầu, không chờ timer.
                }
            }
            HienThiDanhSachVay();
        }

        #region Phương thức lọc công khai
        public void FilterByListSoTK(List<string> listSoTK)
        {
            if (listSoTK != null && listSoTK.Count > 0)
            {
                this.SelectedSoTKList = new List<string>(listSoTK);
                HienThiDanhSachVay();
            }
        }

        public void FilterBySoTK(string soTK)
        {
            if (!string.IsNullOrEmpty(soTK))
            {
                this.SelectedSoTK = soTK;
                HienThiDanhSachVay();
            }
        }

        public void SetSelectedAccounts(List<string> listSoTK)
        {
            FilterByListSoTK(listSoTK);
        }

        public void LoadDanhSachSoTK(string cccd, List<string> listSoTK)
        {
            if (!string.IsNullOrEmpty(cccd)) this.SelectedCCCD = cccd;
            FilterByListSoTK(listSoTK);
        }
        #endregion

        #region Khởi tạo Dữ liệu & Cấu hình Controls
        private readonly string _connStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        private void LoadDataFromSql()
        {
            var customers = new List<FrmMainDN.KhachHangData>();
            var byCCCD = new Dictionary<string, FrmMainDN.KhachHangData>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT CCCD, HoTen, NgaySinh, SoDienThoai, NoTruocDo FROM dbo.DN_KhachHang", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var kh = new FrmMainDN.KhachHangData
                            {
                                CCCD = Convert.ToString(r["CCCD"]),
                                HoTen = Convert.ToString(r["HoTen"]),
                                SoDienThoai = Convert.ToString(r["SoDienThoai"])
                            };
                            customers.Add(kh);
                            byCCCD[kh.CCCD] = kh;
                        }
                    }
                    using (var cmd = new SqlCommand("SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai FROM (SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai, ROW_NUMBER() OVER (PARTITION BY CCCD_ChuSoHuu, CCCD ORDER BY Id DESC) AS rn FROM dbo.DN_DongSoHuu WHERE CCCD <> CCCD_ChuSoHuu) d WHERE rn=1", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            FrmMainDN.KhachHangData kh;
                            if (!byCCCD.TryGetValue(Convert.ToString(r["CCCD_ChuSoHuu"]), out kh)) continue;
                            kh.DanhSachDongSoHuu.Add(new FrmMainDN.DongSoHuuData
                            {
                                CCCD = Convert.ToString(r["CCCD"]),
                                HoTen = Convert.ToString(r["HoTen"]),
                                SoDienThoai = Convert.ToString(r["SoDienThoai"])
                            });
                        }
                    }
                    var linksDN = new Dictionary<string, List<FrmMainDN.DongSoHuuData>>(StringComparer.OrdinalIgnoreCase);
                    using (var cmd = new SqlCommand(@"SELECT d.SoTKVay,d.CCCD,d.HoTen,d.SoDienThoai FROM dbo.DN_DongSoHuuTaiKhoanVay d JOIN dbo.DN_TaiKhoanVay v ON v.SoTKVay=d.SoTKVay WHERE d.CCCD<>v.CCCD", conn))
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            string stk = Convert.ToString(r[0]);
                            List<FrmMainDN.DongSoHuuData> ds;
                            if (!linksDN.TryGetValue(stk, out ds)) linksDN[stk] = ds = new List<FrmMainDN.DongSoHuuData>();
                            ds.Add(new FrmMainDN.DongSoHuuData { CCCD = Convert.ToString(r[1]), HoTen = Convert.ToString(r[2]), SoDienThoai = Convert.ToString(r[3]) });
                        }
                    _uyQuyenTheoKhoanVayDN = linksDN;
                    using (var cmd = new SqlCommand("SELECT SoTKVay, CCCD, SoTienVay, SoTienDaTra, LaiSuat, KyHan, TrangThai, NgayMo, NgayCapNhatTrangThai FROM dbo.DN_TaiKhoanVay", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            FrmMainDN.KhachHangData kh;
                            if (!byCCCD.TryGetValue(Convert.ToString(r["CCCD"]), out kh)) continue;
                            kh.DanhSachVay.Add(new FrmMainDN.TaiKhoanVayItem
                            {
                                SoTKVay = Convert.ToString(r["SoTKVay"]),
                                SoTienVay = Convert.ToDecimal(r["SoTienVay"]),
                                SoTienDaTra = Convert.ToDecimal(r["SoTienDaTra"]),
                                LaiSuat = Convert.ToDouble(r["LaiSuat"]),
                                KyHan = Convert.ToString(r["KyHan"]),
                                TrangThai = (FrmMainDN.TrangThaiTK)Convert.ToInt32(r["TrangThai"]),
                                NgayMo = Convert.ToDateTime(r["NgayMo"]),
                                NgayCapNhatTrangThai = r["NgayCapNhatTrangThai"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["NgayCapNhatTrangThai"])
                            });
                        }
                    }
                    // Đọc nhóm nợ đã được lưu từ SQL khi khởi động/làm mới form.
                    // Xóa giá trị bộ nhớ cũ của các tài khoản vay để SQL là nguồn dữ liệu chuẩn.
                    var loanKeys = new HashSet<string>(customers.SelectMany(k => k.DanhSachVay)
                        .Select(v => v.SoTKVay), StringComparer.OrdinalIgnoreCase);
                    foreach (string key in loanKeys) dictCustomNhomNo.Remove(key);
                    using (var cmd = new SqlCommand(@"SELECT n.SoTaiKhoan, n.NhomNo
                        FROM dbo.DN_NhomNoTaiKhoan n
                        INNER JOIN dbo.DN_TaiKhoanVay v ON v.SoTKVay = n.SoTaiKhoan
                        WHERE n.NhomNo BETWEEN 1 AND 5", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            dictCustomNhomNo[Convert.ToString(r["SoTaiKhoan"])] = Convert.ToInt32(r["NhomNo"]);
                    }
                }
                listKhachHang = customers;
                dictSoTienGocDaTra.Clear();
                foreach (var kh in listKhachHang)
                    foreach (var vay in kh.DanhSachVay)
                        dictSoTienGocDaTra[vay.SoTKVay] = vay.SoTienDaTra;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Không thể tải dữ liệu vay từ SQL Server:\n" + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshSqlData()
        {
            string stkDangChon = GetSelectedRowModel()?.SoTKVay;
            LoadDataFromSql();
            HienThiDanhSachVay();
            if (!string.IsNullOrEmpty(stkDangChon)) ChonLaiTaiKhoanVay(stkDangChon);
        }

        private VayGridRowModel ChonLaiTaiKhoanVay(string stk)
        {
            if (string.IsNullOrWhiteSpace(stk)) return null;
            foreach (DataGridViewRow row in dgvVay.Rows)
            {
                var item = row.Tag as VayGridRowModel;
                if (item == null || !string.Equals(item.SoTKVay, stk, StringComparison.OrdinalIgnoreCase)) continue;
                dgvVay.ClearSelection();
                row.Selected = true;
                if (row.Cells.Count > 0) dgvVay.CurrentCell = row.Cells[0];
                return item;
            }
            return null;
        }

        private void ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Không tìm thấy bản ghi để cập nhật.");
            }
        }

        private bool TrySqlAction(Action action)
        {
            try { action(); return true; }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu dữ liệu vào SQL Server:\n" + ex.Message, "Lỗi lưu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SetupControls()
        {
            cboLocNhom.Items.Clear();
            cboLocNhom.Items.Add("Tất cả nhóm nợ");
            cboLocNhom.Items.Add("Nhóm 1 (Đủ tiêu chuẩn)");
            cboLocNhom.Items.Add("Nhóm 2 (Cần chú ý)");
            cboLocNhom.Items.Add("Nhóm 3 (Dưới tiêu chuẩn)");
            cboLocNhom.Items.Add("Nhóm 4 (Nghi ngờ)");
            cboLocNhom.Items.Add("Nhóm 5 (Mất vốn)");
            cboLocNhom.SelectedIndex = 0;

            cboLocKyHan.Items.Clear();
            cboLocKyHan.Items.Add("Tất cả kỳ hạn");
            cboLocKyHan.Items.Add("6 tháng");
            cboLocKyHan.Items.Add("12 tháng");
            cboLocKyHan.Items.Add("24 tháng");
            cboLocKyHan.Items.Add("36 tháng");
            cboLocKyHan.Items.Add("60 tháng");
            cboLocKyHan.SelectedIndex = 0;

            cboLocThoiGian.Items.Clear();
            cboLocThoiGian.Items.Add("Tất cả");
            cboLocThoiGian.Items.Add("Hôm nay");
            cboLocThoiGian.Items.Add("Hôm qua");
            cboLocThoiGian.Items.Add("7 ngày");
            cboLocThoiGian.Items.Add("1 tháng");
            cboLocThoiGian.Items.Add("3 tháng");
            cboLocThoiGian.Items.Add("1 năm");
            cboLocThoiGian.Items.Add("Tùy chọn");
            cboLocThoiGian.SelectedIndex = 0;

            dtpTuNgay.Value = DateTime.Today;
            dtpDenNgay.Value = DateTime.Today;
            dtpTuNgay.Enabled = false;
            dtpDenNgay.Enabled = false;
        }

        private void SetupEvents()
        {
            txtCCCDSearch.TextChanged += TxtCCCDSearch_TextChanged;
            txtCCCDSearch.KeyPress += TxtCCCDSearch_KeyPress;
            btnTimCCCD.Click += BtnTimCCCD_Click;

            cboLocNhom.SelectedIndexChanged += (s, e) => { if (!_searchRefreshing) LocVayCham(); };
            cboLocKyHan.SelectedIndexChanged += (s, e) => { if (!_searchRefreshing) LocVayCham(); };

            cboLocThoiGian.SelectedIndexChanged += (s, e) =>
            {
                bool isCustom = cboLocThoiGian.SelectedItem?.ToString() == "Tùy chọn";
                dtpTuNgay.Enabled = isCustom;
                dtpDenNgay.Enabled = isCustom;
                if (!_searchRefreshing) LocVayCham();
            };

            dtpTuNgay.ValueChanged += (s, e) => { if (!_searchRefreshing && dtpTuNgay.Enabled) LocVayCham(); };
            dtpDenNgay.ValueChanged += (s, e) => { if (!_searchRefreshing && dtpDenNgay.Enabled) LocVayCham(); };

            dgvVay.MouseDown += DgvVay_MouseDown;
            dgvVay.CellDoubleClick += DgvVay_CellDoubleClick;
            dgvVay.SelectionChanged += DgvVay_SelectionChanged;

            menuMoThemTK.Click += MenuMoThemTK_Click;
            menuChuyenNhomNo.Click += MenuChuyenNhomNo_Click;
            menuThanhToan.Click += MenuThanhToan_Click;
            menuDongTK.Click += MenuDongTK_Click;
            menuKhoaMoTK.Click += MenuKhoaMoTK_Click;
            menuLichSu.Click += MenuLichSu_Click;
        }

        private void TxtCCCDSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TxtCCCDSearch_TextChanged(object sender, EventArgs e)
        {
            string cleanText = new string(txtCCCDSearch.Text.Where(char.IsDigit).ToArray());
            if (cleanText.Length > 12) cleanText = cleanText.Substring(0, 12);

            if (txtCCCDSearch.Text != cleanText)
            {
                int cursorPos = txtCCCDSearch.SelectionStart;
                txtCCCDSearch.Text = cleanText;
                txtCCCDSearch.SelectionStart = Math.Min(cursorPos, txtCCCDSearch.Text.Length);
                return;
            }

            // Khi người dùng gõ tìm kiếm mới, hủy giới hạn lọc số tài khoản cũ
            if (txtCCCDSearch.Focused)
            {
                SelectedSoTKList.Clear();
                _selectedSoTK = "";
            }

            if (!_searchRefreshing && !_dangDungBangVay) LocVayCham();
        }

        private bool _dangMoKhungVayTuTimKiem;

        private void BtnTimCCCD_Click(object sender, EventArgs e)
        {
            string ma = txtCCCDSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(ma))
            {
                HienThiDanhSachVay();
                return;
            }
            if (_dangMoKhungVayTuTimKiem || _dangDungBangVay) return;
            _timerLocVay.Stop();
            _dangMoKhungVayTuTimKiem = true;
            try
            {
                bool coVay;
                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay v
                    WHERE v.CCCD LIKE '%'+@c+'%' OR EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu d
                    WHERE d.CCCD_ChuSoHuu=v.CCCD AND d.CCCD LIKE '%'+@c+'%') OR EXISTS(
                    SELECT 1 FROM dbo.DN_DongSoHuuTaiKhoanVay d WHERE d.SoTKVay=v.SoTKVay AND d.CCCD LIKE '%'+@c+'%'))
                    THEN 1 ELSE 0 END", cn))
                {
                    cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = ma;
                    cn.Open(); coVay = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
                HienThiDanhSachVay();
                if (coVay)
                {
                    if (!dgvVay.Rows.Cast<DataGridViewRow>().Any(r => !r.IsNewRow))
                        MessageBox.Show("Có tài khoản vay nhưng không phù hợp bộ lọc hiện tại.", "Kết quả tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (ma.Length < 10 || ma.Length > 12 || !ma.All(c => c >= '0' && c <= '9'))
                {
                    MessageBox.Show("Không tìm thấy khoản vay. Vui lòng nhập MST hợp lệ từ 10 đến 12 chữ số để mở tài khoản mới."); return;
                }
                if (MessageBox.Show(this, "Không tìm thấy tài khoản vay phù hợp với: " + ma + "\nBạn có muốn mở tài khoản vay mới không?",
                    "Không tìm thấy tài khoản", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                MoKhungThemVayTuMainDN(ma);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể tìm kiếm hoặc mở tài khoản vay: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { _dangMoKhungVayTuTimKiem = false; }
        }

        private void MoKhungThemVayTuMainDN(string ma)
        {
            if (!FrmMain.ChoPhepMoVayTheoNhomNo(_connStr, true, ma)) return;
            string maDaMo = null;
            var rnd = new Random();
            Func<bool, string> taoSo = dep => dep ? "8888" + rnd.Next(1000, 9999).ToString("D4")
                : "105" + rnd.Next(1000000, 9999999).ToString("D7");
            FrmMainDN.MoKhungThemMoiTuTimKiemDN(ma, this, "Vay", taoSo,
                khung => GanNoiDungMoVayTuMainDN(khung, c => maDaMo = c));
            if (string.IsNullOrEmpty(maDaMo)) return;
            _searchRefreshing = true;
            try
            {
                _selectedCCCD = maDaMo; _selectedSoTK = "";
                if (SelectedSoTKList != null) SelectedSoTKList.Clear();
                txtCCCDSearch.Text = maDaMo;
                cboLocNhom.SelectedIndex = 0; cboLocKyHan.SelectedIndex = 0; cboLocThoiGian.SelectedIndex = 0;
                dtpTuNgay.Enabled = dtpDenNgay.Enabled = false;
            }
            finally { _searchRefreshing = false; }
            RefreshSqlData();
            OnDataChanged();
        }
        #endregion

        private void GanNoiDungMoVayTuMainDN(FrmMainDN.KhungMoTaiKhoanTuTimKiemDN khung, Action<string> daMo)
        {
            var dlgMoTK = khung.HopThoai;
            var pnlContainer = khung.NoiDung;
            var txtSTK = khung.SoTaiKhoan;
            var rdoSoDep = khung.SoDep;
            // MST cố định theo mã đã tìm; không chuyển người được ủy quyền thành chủ tài khoản.
            string soTietKiemMacDinh = null;
            var model = new VayGridRowModel
            {
                CCCD = khung.MaBanDau,
                VaiTro = "Chủ sở hữu",
                RawKhachHang = new FrmMainDN.KhachHangData { CCCD = khung.MaBanDau }
            };
            Font fontNormal = new Font("Segoe UI", 9F, FontStyle.Regular);
            Font fontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
            dlgMoTK.Disposed += (sender, args) => { fontNormal.Dispose(); fontBold.Dispose(); };
            RadioButton rdoTinChap = CreateRadioButtonDynamicDesign13(fontBold);
            RadioButton rdoTheChap = CreateRadioButtonDynamicDesign14(fontBold);

            Label lblMucLuong = CreateLabelDynamicDesign15(fontNormal);
            TextBox txtMucLuong = CreateTextBoxDynamicDesign16(fontNormal);

            Label lblTenTaiSan = CreateLabelDynamicDesign17(fontNormal);
            TextBox txtTenTaiSan = CreateTextBoxDynamicDesign18(fontNormal);

            Label lblDinhGia = CreateLabelDynamicDesign19(fontNormal);
            TextBox txtDinhGia = CreateTextBoxDynamicDesign20(fontNormal);

            Label lblLoaiTaiSan = CreateLabelDynamicDesign21(fontNormal);
            ComboBox cboLoaiTaiSan = CreateComboBoxDesign13();
            cboLoaiTaiSan.Items.AddRange(new object[] { "Tài sản cố định", "Sổ tiết kiệm trong ngân hàng", "Sổ tiết kiệm ngoài ngân hàng" });
            cboLoaiTaiSan.SelectedIndex = 0;
            Label lblSoCamCo = CreateLabelDynamicDesign22(fontNormal);
            ComboBox cboSoCamCo = CreateComboBoxDesign14();
            TextBox txtSoNgoai = CreateTextBoxDesign15();
            Label lblGocNgoai = CreateLabelDesign16();
            TextBox txtGocNgoai = CreateTextBoxDesign17();
            Label lblLaiNgoai = CreateLabelDesign18();
            TextBox txtLaiNgoai = CreateTextBoxDesign19();
            Label lblGiaTriCamCo = CreateLabelDesign20();
            string cccdLaySoTheChap = model.CCCD;
            if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
            {
                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(
                    "SELECT CCCD FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk", cn))
                {
                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soTietKiemMacDinh;
                    cn.Open();
                    cccdLaySoTheChap = Convert.ToString(cmd.ExecuteScalar());
                }
            }
            try { cboSoCamCo.Items.AddRange(FrmMainDN.LaySoTietKiemTheChap(_connStr, cccdLaySoTheChap).Cast<object>().ToArray()); }
            catch (Exception ex) { MessageBox.Show("Không đọc được sổ tiết kiệm: " + ex.Message); }
            if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
            {
                for (int i = cboSoCamCo.Items.Count - 1; i >= 0; i--)
                {
                    var so = cboSoCamCo.Items[i] as FrmMainDN.SoTietKiemTheChap;
                    if (so == null || !string.Equals(so.SoTKTK, soTietKiemMacDinh,
                        StringComparison.OrdinalIgnoreCase)) cboSoCamCo.Items.RemoveAt(i);
                }
            }
            if (cboSoCamCo.Items.Count > 0) cboSoCamCo.SelectedIndex = 0;
            if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
            {
                for (int i = 0; i < cboSoCamCo.Items.Count; i++)
                {
                    var so = cboSoCamCo.Items[i] as FrmMainDN.SoTietKiemTheChap;
                    if (so != null && string.Equals(so.SoTKTK, soTietKiemMacDinh,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        cboSoCamCo.SelectedIndex = i;
                        break;
                    }
                }
            }

            Label lblNhomNo = CreateLabelDynamicDesign23(fontNormal);
            ComboBox cboNhomNo = CreateComboBoxDynamicDesign24(fontNormal);
            cboNhomNo.Items.AddRange(new object[] {
                "Nhóm 1 (Đủ tiêu chuẩn)",
                "Nhóm 2 (Cần chú ý)",
                "Nhóm 3 (Dưới tiêu chuẩn)",
                "Nhóm 4 (Nghi ngờ)",
                "Nhóm 5 (Mất vốn)"
            });
            int initNhomIndex = Math.Max(0, FrmMainDN.LayNhomNoHienTai(_connStr, model.CCCD) - 1);
            if (initNhomIndex >= cboNhomNo.Items.Count) initNhomIndex = 0;
            cboNhomNo.SelectedIndex = initNhomIndex;
            // Nhóm nợ hiện tại chỉ để xem, không cho thay đổi khi mở tài khoản mới.
            // Việc chuyển nhóm nợ chỉ thực hiện tại chức năng Chuyển nhóm nợ.
            cboNhomNo.Enabled = false;

            Label lblSoTienToiDa = CreateLabelDesign21();

            Label lblSoTienVay = CreateLabelDynamicDesign25(fontNormal);
            TextBox txtSoTienVay = CreateTextBoxDynamicDesign26(fontNormal);

            Label lblKyHan = CreateLabelDynamicDesign27(fontNormal);
            ComboBox cboKyHan = CreateComboBoxDynamicDesign28(fontNormal);

            Button btnXacNhan = CreateButtonDynamicDesign29(fontBold);
            btnXacNhan.FlatAppearance.BorderSize = 0;

            Button btnHuy = CreateButtonDynamicDesign30(fontNormal);
            btnHuy.FlatAppearance.BorderColor = Color.LightGray;

            decimal maxLoanAmount = 0;

            Action updateLayoutAndMaxLoan = () =>
            {
                bool tin = rdoTinChap.Checked;
                int loai = cboLoaiTaiSan.SelectedIndex;
                bool batDongSan = !tin && loai == 0;
                bool soNoi = !tin && loai == 1;
                bool soNgoai = !tin && loai == 2;
                lblMucLuong.Visible = txtMucLuong.Visible = tin;
                lblLoaiTaiSan.Visible = cboLoaiTaiSan.Visible = !tin;
                lblTenTaiSan.Visible = txtTenTaiSan.Visible = batDongSan;
                lblDinhGia.Visible = txtDinhGia.Visible = batDongSan;
                lblSoCamCo.Visible = soNoi || soNgoai;
                lblSoCamCo.Text = soNoi ? "Chọn sổ tiết kiệm:" : "Số sổ bên ngoài:";
                cboSoCamCo.Visible = soNoi;
                txtSoNgoai.Visible = soNgoai;
                lblGocNgoai.Visible = txtGocNgoai.Visible = soNgoai;
                lblLaiNgoai.Visible = txtLaiNgoai.Visible = soNgoai;
                lblGiaTriCamCo.Visible = soNoi;
                int yNhom = tin ? 90 : batDongSan ? 160 : 225;
                foreach (Control input in new Control[] { txtMucLuong, txtTenTaiSan, txtDinhGia, cboLoaiTaiSan, cboSoCamCo, txtSoNgoai, txtGocNgoai, txtLaiNgoai, cboNhomNo, txtSoTienVay, cboKyHan }) input.Width = 460;
                lblTenTaiSan.Location = new Point(25, 90); txtTenTaiSan.Location = new Point(180, 87);
                lblDinhGia.Location = new Point(25, 125); txtDinhGia.Location = new Point(180, 122);
                lblNhomNo.Location = new Point(25, yNhom);
                cboNhomNo.Location = new Point(180, yNhom - 3);
                lblSoTienToiDa.Location = new Point(25, yNhom + 40);
                lblSoTienVay.Location = new Point(25, yNhom + 78);
                txtSoTienVay.Location = new Point(180, yNhom + 75);
                lblKyHan.Location = new Point(25, yNhom + 113);
                cboKyHan.Location = new Point(180, yNhom + 110);
                btnXacNhan.SetBounds(180, yNhom + 160, 180, 36);
                btnHuy.SetBounds(375, yNhom + 160, 150, 36);
                pnlContainer.Height = yNhom + 215;
                cboKyHan.Items.Clear();
                if (tin) cboKyHan.Items.AddRange(new object[] { "6 tháng: 10.5%", "12 tháng: 11.0%", "24 tháng: 12.0%", "36 tháng: 12.5%" });
                else if (soNoi) cboKyHan.Items.AddRange(new object[] { "6 tháng: 7.0%", "12 tháng: 7.0%", "24 tháng: 7.0%", "36 tháng: 7.0%" });
                else if (soNgoai) cboKyHan.Items.AddRange(new object[] { "6 tháng: 8.5%", "12 tháng: 8.5%", "24 tháng: 8.5%", "36 tháng: 8.5%" });
                else cboKyHan.Items.AddRange(new object[] { "6 tháng cố định: Từ 5.0%", "12 tháng cố định: Từ 6.5%", "24 tháng cố định: Từ 7.5%", "36 tháng cố định: Từ 8.5%", "60 tháng cố định: Từ 9.5%" });
                cboKyHan.SelectedIndex = 0;
                decimal parse(TextBox t) { decimal.TryParse(new string(t.Text.Where(char.IsDigit).ToArray()), out decimal n); return n; }
                if (tin) maxLoanAmount = parse(txtMucLuong) * 10m;
                else if (batDongSan) maxLoanAmount = parse(txtDinhGia) * .70m;
                else if (soNoi)
                {
                    var so = cboSoCamCo.SelectedItem as FrmMainDN.SoTietKiemTheChap;
                    maxLoanAmount = so == null ? 0 : Math.Floor(so.TongGiaTri * .95m);
                    lblGiaTriCamCo.Text = so == null ? "Không có sổ hợp lệ" : $"Gốc: {so.GocConLai:N0} | Lãi: {so.LaiTamTinh:N0} | Tổng: {so.TongGiaTri:N0}";
                }
                else maxLoanAmount = Math.Floor((parse(txtGocNgoai) + parse(txtLaiNgoai)) * .85m);
                if (cboNhomNo.SelectedIndex >= 2) maxLoanAmount = 0;
                lblSoTienToiDa.Text = $"Số tiền tối đa có thể vay: {maxLoanAmount:N0} VNĐ";
                dlgMoTK.AutoScrollMinSize = new Size(0, pnlContainer.Bottom + 45);
            };
            rdoTinChap.CheckedChanged += (s, ev) => updateLayoutAndMaxLoan();
            rdoTheChap.CheckedChanged += (s, ev) => updateLayoutAndMaxLoan();
            // Chỉ khởi tạo trạng thái sau khi RadioButton đã cùng thuộc một panel.
            // Không chọn ở đây vì rdoTinChap còn Checked=true từ Designer.
            cboLoaiTaiSan.SelectedIndexChanged += (s, ev) => updateLayoutAndMaxLoan();
            cboSoCamCo.SelectedIndexChanged += (s, ev) => updateLayoutAndMaxLoan();
            txtGocNgoai.TextChanged += (s, ev) => updateLayoutAndMaxLoan();
            txtLaiNgoai.TextChanged += (s, ev) => updateLayoutAndMaxLoan();
            txtMucLuong.TextChanged += (s, ev) => updateLayoutAndMaxLoan();
            txtDinhGia.TextChanged += (s, ev) => updateLayoutAndMaxLoan();

            txtSoTienVay.KeyPress += (s, ev) => { if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true; };
            txtMucLuong.KeyPress += (s, ev) => { if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true; };
            txtDinhGia.KeyPress += (s, ev) => { if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true; };

            txtSoTienVay.TextChanged += (s, ev) =>
            {
                string digits = new string(txtSoTienVay.Text.Where(char.IsDigit).ToArray());
                if (decimal.TryParse(digits, out decimal val))
                {
                    txtSoTienVay.Text = $"{val:N0}";
                    txtSoTienVay.SelectionStart = txtSoTienVay.Text.Length;
                }
            };

            txtMucLuong.TextChanged += (s, ev) =>
            {
                string digits = new string(txtMucLuong.Text.Where(char.IsDigit).ToArray());
                if (decimal.TryParse(digits, out decimal val))
                {
                    txtMucLuong.Text = $"{val:N0}";
                    txtMucLuong.SelectionStart = txtMucLuong.Text.Length;
                }
            };

            txtDinhGia.TextChanged += (s, ev) =>
            {
                string digits = new string(txtDinhGia.Text.Where(char.IsDigit).ToArray());
                if (decimal.TryParse(digits, out decimal val))
                {
                    txtDinhGia.Text = $"{val:N0}";
                    txtDinhGia.SelectionStart = txtDinhGia.Text.Length;
                }
            };

            updateLayoutAndMaxLoan();

            btnHuy.Click += (s, ev) => dlgMoTK.Close();

            btnXacNhan.Click += (s, ev) =>
            {
                var hoSo = khung.LayKhachHang();
                if (hoSo == null) return;
                model.HoTen = hoSo.HoTen; model.SoDienThoai = hoSo.SoDienThoai; model.RawKhachHang = hoSo;
                if (!KiemTraQuyenMoVayDN(model)) return;
                string digitsMoney = new string(txtSoTienVay.Text.Where(char.IsDigit).ToArray());
                if (!decimal.TryParse(digitsMoney, out decimal valSoTienVay) || valSoTienVay <= 0)
                {
                    MessageBox.Show("Vui lòng nhập số tiền vay hợp lệ (lớn hơn 0)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoTienVay.Focus();
                    return;
                }

                if (valSoTienVay > maxLoanAmount)
                {
                    MessageBox.Show($"Số tiền muốn vay ({valSoTienVay:N0} VNĐ) vượt quá số tiền tối đa có thể vay ({maxLoanAmount:N0} VNĐ)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSoTienVay.Focus();
                    return;
                }

                string stkMoi = txtSTK.Text.Trim();
                if (string.IsNullOrEmpty(stkMoi))
                {
                    MessageBox.Show("Chưa tạo được số tài khoản vay!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string kyHanSel = cboKyHan.SelectedItem.ToString();
                string kyHanTen = "12 tháng";
                double laiSuatVal = 8.5;

                if (kyHanSel.Contains("6 tháng")) kyHanTen = "6 tháng";
                else if (kyHanSel.Contains("12 tháng")) kyHanTen = "12 tháng";
                else if (kyHanSel.Contains("24 tháng")) kyHanTen = "24 tháng";
                else if (kyHanSel.Contains("36 tháng")) kyHanTen = "36 tháng";
                else if (kyHanSel.Contains("60 tháng")) kyHanTen = "60 tháng";

                int idxPercent = kyHanSel.IndexOf('%');
                if (idxPercent > 0)
                {
                    int idxStart = idxPercent - 1;
                    while (idxStart >= 0 && (char.IsDigit(kyHanSel[idxStart]) || kyHanSel[idxStart] == '.' || kyHanSel[idxStart] == ','))
                    {
                        idxStart--;
                    }
                    string numStr = kyHanSel.Substring(idxStart + 1, idxPercent - idxStart - 1).Replace(',', '.').Trim();
                    double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out laiSuatVal);
                }

                if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh) &&
                    (rdoTinChap.Checked || !rdoTheChap.Checked || cboLoaiTaiSan.SelectedIndex != 1 ||
                     !(cboSoCamCo.SelectedItem is FrmMainDN.SoTietKiemTheChap) ||
                     !string.Equals(((FrmMainDN.SoTietKiemTheChap)cboSoCamCo.SelectedItem).SoTKTK,
                                    soTietKiemMacDinh, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Khoản vay phải thế chấp bằng đúng sổ tiết kiệm đã chọn.",
                        "Sổ thế chấp không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 1 && cboSoCamCo.SelectedItem == null)
                { MessageBox.Show("Chưa có sổ tiết kiệm nội bộ hợp lệ để thế chấp."); return; }
                if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 2 && string.IsNullOrWhiteSpace(txtSoNgoai.Text))
                { MessageBox.Show("Nhập số sổ tiết kiệm ngoài ngân hàng."); return; }
                if (cboNhomNo.SelectedIndex >= 2)
                { MessageBox.Show("Nhóm nợ hiện tại không đủ điều kiện mở khoản vay."); return; }
                if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 1) laiSuatVal = 7.0;
                if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 2) laiSuatVal = 8.5;
                int selectedNhomCode = cboNhomNo.SelectedIndex + 1;
                string selectedNhomTen = cboNhomNo.SelectedItem.ToString();

                // Kiểm tra trùng CCCD/SĐT trong biểu mẫu và dữ liệu SQL.
                var dongSoHuuMoi = new List<Tuple<string, string, string>>();
                var cccdMoi = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD) };
                var sdtMoi = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { hoSo.SoDienThoai };
                foreach (var fields in hoSo.DanhSachDongSoHuu)
                {
                    string c = fields.CCCD.Trim(), n = fields.HoTen.Trim(), t = fields.SoDienThoai.Trim();
                    if (c.Length != 12 || !c.All(char.IsDigit) || string.IsNullOrWhiteSpace(n) ||
                        t.Length < 10 || t.Length > 11 || !t.All(char.IsDigit) ||
                        !cccdMoi.Add(c) || !sdtMoi.Add(t))
                    {
                        MessageBox.Show("Thông tin người được ủy quyền không hợp lệ hoặc bị trùng CCCD/SĐT!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    dongSoHuuMoi.Add(Tuple.Create(c, n, t));
                }
                if (dongSoHuuMoi.Count < 1)
                {
                    MessageBox.Show("Vui lòng thêm ít nhất 1 người được ủy quyền trước khi mở tài khoản.",
                        "Thiếu người được ủy quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!TrySqlAction(() =>
                {
                    using (var conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable))
                        {
                            try
                            {
                                if (stkMoi.Length > 20 || valSoTienVay > 999999999999999999m)
                                    throw new InvalidOperationException("Số tài khoản hoặc số tiền vay vượt giới hạn dữ liệu.");
                                using (var checkSTK = new SqlCommand(@"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk)
                                    OR EXISTS(SELECT 1 FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk)
                                    OR EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk) THEN 1 ELSE 0 END", conn, transaction))
                                {
                                    checkSTK.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkMoi;
                                    if (Convert.ToInt32(checkSTK.ExecuteScalar()) != 0)
                                        throw new InvalidOperationException("Số tài khoản đã tồn tại. Vui lòng tạo số khác.");
                                }
                                foreach (var d in dongSoHuuMoi)
                                    FrmMainDN.KiemTraNguoiUyQuyenKhiThemDN(conn, transaction, d.Item1, d.Item2, d.Item3);
                                // Tạo hồ sơ mới chỉ sau khi kiểm tra quyền, trong cùng transaction với khoản vay.
                                FrmMainDN.DamBaoHoSoMoMoiTuTimKiemDN(conn, transaction, hoSo);
                                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.DN_KhachHang WHERE CCCD=@cccd", conn, transaction))
                                {
                                    cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = model.CCCD;
                                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 1)
                                        throw new InvalidOperationException("Không tìm thấy hồ sơ chủ tài khoản doanh nghiệp.");
                                }
                                FrmMain.KiemTraNhomNoTruocMoVay(conn, transaction, true, model.CCCD, (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD));
                                if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex > 0)
                                    FrmMain.KiemTraSoTietKiemChuaVay(conn, transaction, true, cboLoaiTaiSan.SelectedIndex == 2,
                                        cboLoaiTaiSan.SelectedIndex == 1 ? ((FrmMainDN.SoTietKiemTheChap)cboSoCamCo.SelectedItem).SoTKTK : txtSoNgoai.Text.Trim());
                                using (var cmd = new SqlCommand(@"INSERT INTO dbo.DN_TaiKhoanVay
                                    (SoTKVay, CCCD, SoTienVay, SoTienDaTra, LaiSuat, KyHan, TrangThai, NgayMo)
                                    VALUES (@stk,@cccd,@tien,0,@lai,@kyhan,0,GETDATE())", conn, transaction))
                                {
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 30).Value = stkMoi;
                                    cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD);
                                    cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = valSoTienVay;
                                    cmd.Parameters.Add("@lai", SqlDbType.Float).Value = laiSuatVal;
                                    cmd.Parameters.Add("@kyhan", SqlDbType.NVarChar, 30).Value = kyHanTen;
                                    cmd.ExecuteNonQuery();
                                }
                                foreach (var d in dongSoHuuMoi)
                                {
                                    using (var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@chu AND CCCD=@cccd)
                                            INSERT INTO dbo.DN_DongSoHuu(CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai) VALUES (@chu,@cccd,@ten,@sdt);
                                            INSERT INTO dbo.DN_DongSoHuuTaiKhoanVay(SoTKVay,CCCD,HoTen,SoDienThoai) VALUES(@stk,@cccd,@ten,@sdt);", conn, transaction))
                                    {
                                        cmd.Parameters.Add("@chu", SqlDbType.VarChar, 12).Value = (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD);
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkMoi;
                                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = d.Item1;
                                        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 200).Value = d.Item2;
                                        cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 11).Value = d.Item3;
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                {
                                    string ghiChu = rdoTinChap.Checked ? "Vay tín chấp" : "Vay thế chấp tài sản cố định";
                                    if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex > 0)
                                    {
                                        string loaiCamCo = cboLoaiTaiSan.SelectedIndex == 1 ? "Trong ngân hàng" : "Ngoài ngân hàng";
                                        string soCamCo = cboLoaiTaiSan.SelectedIndex == 1
                                            ? ((FrmMainDN.SoTietKiemTheChap)cboSoCamCo.SelectedItem).SoTKTK : txtSoNgoai.Text.Trim();
                                        ghiChu = $"Vay thế chấp sổ tiết kiệm {loaiCamCo}; Số sổ: {soCamCo}; Hạn mức: {maxLoanAmount:N0} VND; Lãi suất vay: {laiSuatVal}%/năm";
                                    }
                                    using (var history = new SqlCommand(@"INSERT INTO dbo.DN_LichSuGiaoDichVay
                                        (SoTKVay, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, TongTien, SoDuConLai, GhiChu, NguoiThucHien)
                                        VALUES (@stk, GETDATE(), N'Giải ngân', @tien, 0, @tien, @tien, @note, @nguoi)", conn, transaction))
                                    {
                                        history.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                        history.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkMoi;
                                        history.Parameters.Add("@tien", SqlDbType.Decimal).Value = valSoTienVay;
                                        history.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value = ghiChu;
                                        history.ExecuteNonQuery();
                                    }
                                }
                                transaction.Commit();
                            }
                            catch { transaction.Rollback(); throw; }
                        }
                    }
                })) return;
                string loaiVayStr = rdoTinChap.Checked ? "Vay Tín Chấp" : "Vay Thế Chấp";
                string phiText = rdoSoDep.Checked ? "\n(Phí mở số đẹp: 500,000 VNĐ)" : "";
                MessageBox.Show($"Mở thành công tài khoản vay mới!\n\n• Loại vay: {loaiVayStr}\n• Chủ sở hữu: {model.HoTen}\n• Số TK Vay: {stkMoi}\n• Số tiền vay: {valSoTienVay:N0} VNĐ\n• Lãi suất: {laiSuatVal}%\n• Kỳ hạn: {kyHanTen}\n• Nhóm nợ: {selectedNhomTen}{phiText}", "Thông báo thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                daMo(model.CCCD);
                dlgMoTK.DialogResult = DialogResult.OK;
            };

            pnlContainer.Controls.AddRange(new Control[] {
                rdoTinChap, rdoTheChap,
                lblMucLuong, txtMucLuong,
                lblTenTaiSan, txtTenTaiSan, lblDinhGia, txtDinhGia,
                lblLoaiTaiSan, cboLoaiTaiSan, lblSoCamCo, cboSoCamCo, txtSoNgoai,
                lblGocNgoai, txtGocNgoai, lblLaiNgoai, txtLaiNgoai, lblGiaTriCamCo,
                lblNhomNo, cboNhomNo,
                lblSoTienToiDa,
                lblSoTienVay, txtSoTienVay,
                lblKyHan, cboKyHan,
                btnXacNhan, btnHuy
            });
            updateLayoutAndMaxLoan();
        }

        #region Mở Thêm Tài Khoản Vay
        private void MenuMoThemTK_Click(object sender, EventArgs e)
        {
            var model = GetSelectedRowModel();
            if (model != null)
            {
                ThucHienMoThemTaiKhoanVay(model);
            }
        }

        private static bool LaChuTaiKhoanVayDN(string role)
        {
            return role == "Chủ" || role == "Chủ sở hữu" || role == "Chủ tài khoản";
        }

        private bool KiemTraQuyenMoVayDN(VayGridRowModel model)
        {
            if (model != null && LaChuTaiKhoanVayDN(model.VaiTro) &&
                model.RawKhachHang != null && string.Equals(model.CCCD, model.RawKhachHang.CCCD, StringComparison.OrdinalIgnoreCase)) return true;
            MessageBox.Show("Người được ủy quyền không được mở thêm tài khoản doanh nghiệp. Vui lòng chọn chủ tài khoản.",
                "Không có quyền thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void ThucHienMoThemTaiKhoanVay(VayGridRowModel model)
        {
            if (model == null || !KiemTraQuyenMoVayDN(model)) return;

            if (model.TrangThai == FrmMainDN.TrangThaiTK.Dong)
            {
                MessageBox.Show($"Tài khoản {model.SoTKVay} đang ở trạng thái ĐÃ ĐÓNG.\nKhông thể thực hiện mở thêm tài khoản vay cho tài khoản đã đóng!", "Cảnh báo không thể mở tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (model.NhomNoCode >= 3)
            {
                MessageBox.Show($"Khách hàng {model.HoTen} (CCCD: {model.CCCD}) đang thuộc {model.NhomNoTen}.\nKhông thể mở thêm tài khoản vay do khách hàng thuộc nhóm nợ từ 3 đến 5 (nợ xấu)!", "Cảnh báo không đủ điều kiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HienThiFormMoTaiKhoanVay(model);
        }

        // Được gọi từ FrmTietKiemDN để mở đúng form thêm khoản vay hiện có.
        // Chỉ chủ tài khoản được mở khoản vay bằng sổ tiết kiệm.
        public void MoFormVayBangSoTietKiem(string soSo, string cccdNguoiVay)
        {
            if (string.IsNullOrWhiteSpace(soSo) || string.IsNullOrWhiteSpace(cccdNguoiVay)) return;
            try
            {
                VayGridRowModel model;
                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"
                    SELECT TOP (1) x.HoTen, x.SoDienThoai, x.TrangThai, x.UuTien
                    FROM (
                        SELECT k.HoTen, k.SoDienThoai, s.TrangThai, 0 AS UuTien
                        FROM dbo.DN_SoTietKiem s
                        JOIN dbo.DN_KhachHang k ON k.CCCD=s.CCCD
                        WHERE s.SoTKTK=@stk AND s.CCCD=@cccd
                        UNION ALL
                        SELECT COALESCE(k.HoTen,d.HoTen),
                               COALESCE(k.SoDienThoai,d.SoDienThoai), s.TrangThai, 1
                        FROM dbo.DN_SoTietKiem s
                        JOIN dbo.DN_DongSoHuuSoTietKiem d ON d.SoTKTK=s.SoTKTK
                        LEFT JOIN dbo.DN_KhachHang k ON k.CCCD=d.CCCD
                        WHERE s.SoTKTK=@stk AND d.CCCD=@cccd
                    ) x ORDER BY x.UuTien", cn))
                {
                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                    cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccdNguoiVay;
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            MessageBox.Show("Người được chọn không phải chủ hoặc người được ủy quyền của sổ này.");
                            return;
                        }
                        if (Convert.ToInt32(rd["UuTien"]) != 0)
                        {
                            MessageBox.Show("Người được ủy quyền không được mở thêm tài khoản doanh nghiệp.", "Không có quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (Convert.ToInt32(rd["TrangThai"]) != 0)
                        {
                            MessageBox.Show("Chỉ có thể vay bằng sổ tiết kiệm đang hoạt động.");
                            return;
                        }
                        string ten = Convert.ToString(rd["HoTen"]);
                        string sdt = Convert.ToString(rd["SoDienThoai"]);
                        model = new VayGridRowModel
                        {
                            CCCD = cccdNguoiVay,
                            VaiTro = "Chủ tài khoản",
                            HoTen = ten,
                            SoDienThoai = sdt,
                            NhomNoCode = FrmMainDN.LayNhomNoHienTai(_connStr, cccdNguoiVay),
                            RawKhachHang = new FrmMainDN.KhachHangData
                            {
                                CCCD = cccdNguoiVay,
                                HoTen = ten,
                                SoDienThoai = sdt
                            }
                        };
                    }
                    FrmMain.KiemTraSoTietKiemChuaVay(cn, null, true, false, soSo);
                }
                if (model.NhomNoCode >= 3)
                {
                    MessageBox.Show("Người vay thuộc nhóm nợ 3-5, không đủ điều kiện mở khoản vay.");
                    return;
                }
                HienThiFormMoTaiKhoanVay(model, soSo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được form vay: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HienThiFormMoTaiKhoanVay(VayGridRowModel model, string soTietKiemMacDinh = null)
        {
            if (!KiemTraQuyenMoVayDN(model)) return;
            if (!FrmMain.ChoPhepMoVayTheoNhomNo(_connStr, true, model.CCCD, (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD))) return;
            using (Form dlgMoTK = new Form())
            {
                dlgMoTK.Text = "Thêm Tài Khoản Cho Khách Hàng";
                // Hộp thoại thế chấp có nhiều trường hơn tín chấp: dùng đủ chiều cao màn hình.
                Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
                int dialogHeight = string.IsNullOrWhiteSpace(soTietKiemMacDinh) ? 680 : 840;
                dlgMoTK.ClientSize = new Size(760, Math.Min(dialogHeight, Math.Max(320, workArea.Height - 80)));
                dlgMoTK.StartPosition = FormStartPosition.CenterScreen;
                dlgMoTK.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlgMoTK.MaximizeBox = false;
                dlgMoTK.MinimizeBox = false;
                dlgMoTK.BackColor = Color.FromArgb(245, 245, 247);
                dlgMoTK.AutoScroll = true;

                Font fontNormal = new Font("Segoe UI", 9F, FontStyle.Regular);
                Font fontBold = new Font("Segoe UI", 9F, FontStyle.Bold);

                Label lblCCCD = CreateLabelDynamicDesign1(fontNormal);
                TextBox txtCCCD = CreateTextBoxDynamicDesign2(model.CCCD, fontNormal);

                Label lblHoTen = CreateLabelDynamicDesign3(fontNormal);
                TextBox txtHoTen = CreateTextBoxDynamicDesign4(model.HoTen, fontNormal);

                Label lblSDT = CreateLabelDynamicDesign5(fontNormal);
                TextBox txtSDT = CreateTextBoxDynamicDesign6(model.SoDienThoai, fontNormal);

                Button btnDongSoHuu = CreateButtonDesign2();
                btnDongSoHuu.Text = "+ Thêm người được ủy quyền";
                btnDongSoHuu.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);

                // Người được ủy quyền: tối đa 3 người, lưu trực tiếp vào SQL cùng khoản vay.
                var dongSoHuuInputs = new List<Tuple<TextBox, TextBox, TextBox>>();
                NumericUpDown nudDongSoHuu = CreateNumericUpDownDesign3();
                Panel pnlDongSoHuu = CreatePanelDesign4();
                bool hienUyQuyen = false;
                Action renderDongSoHuu = () =>
                {
                    // Không xóa nội dung người dùng đã nhập khi thay đổi số lượng.
                    var old = dongSoHuuInputs.Select(x => new[] { x.Item1.Text, x.Item2.Text, x.Item3.Text }).ToList();
                    pnlDongSoHuu.Controls.Clear();
                    dongSoHuuInputs.Clear();
                    for (int i = 0; i < (int)nudDongSoHuu.Value; i++)
                    {
                        GroupBox group = CreateGroupBoxDynamicDesign7("Người được ủy quyền " + (i + 1), new Point(8, 8 + i * 64));
                        TextBox c = CreateTextBoxDesign5();
                        TextBox n = CreateTextBoxDesign6();
                        TextBox t = CreateTextBoxDesign7();
                        c.KeyPress += (ss, ee) => { if (!char.IsControl(ee.KeyChar) && !char.IsDigit(ee.KeyChar)) ee.Handled = true; };
                        t.KeyPress += (ss, ee) => { if (!char.IsControl(ee.KeyChar) && !char.IsDigit(ee.KeyChar)) ee.Handled = true; };
                        if (i < old.Count) { c.Text = old[i][0]; n.Text = old[i][1]; t.Text = old[i][2]; }
                        group.Size = new Size(664, 66);
                        group.Location = new Point(8, 8 + i * 72);
                        var lc = CreateLabelDesign8(); lc.Text = "CCCD"; lc.Location = new Point(10, 30);
                        var ln = CreateLabelDesign9(); ln.Text = "Họ tên"; ln.Location = new Point(200, 30);
                        var lt = CreateLabelDesign10(); lt.Location = new Point(465, 30);
                        c.SetBounds(62, 26, 128, 26); n.SetBounds(255, 26, 200, 26); t.SetBounds(505, 26, 145, 26);
                        group.Controls.AddRange(new Control[] { lc, c, ln, n, lt, t });
                        pnlDongSoHuu.Controls.Add(group);
                        dongSoHuuInputs.Add(Tuple.Create(c, n, t));
                        DinhDangControlFormPhu(group);
                    }
                };
                btnDongSoHuu.Click += (ss, ee) =>
                {
                    hienUyQuyen = true;
                    nudDongSoHuu.Visible = true;
                    pnlDongSoHuu.Visible = true;
                    if (dongSoHuuInputs.Count == 0) renderDongSoHuu();
                };
                nudDongSoHuu.ValueChanged += (ss, ee) => renderDongSoHuu();

                Label lblLoaiTK = CreateLabelDynamicDesign8(fontNormal);
                ComboBox cboLoaiTK = CreateComboBoxDynamicDesign9(fontNormal);
                cboLoaiTK.Items.Add("Vay");
                cboLoaiTK.SelectedIndex = 0;

                RadioButton rdoNgauNhien = CreateRadioButtonDynamicDesign10(fontNormal);
                RadioButton rdoSoDep = CreateRadioButtonDynamicDesign11(fontNormal);

                Label lblSTK = CreateLabelDynamicDesign12(fontNormal);
                TextBox txtSTK = CreateTextBoxDesign11();

                Random rnd = new Random();
                Action generateSTK = () =>
                {
                    if (rdoSoDep.Checked)
                        txtSTK.Text = "8888" + rnd.Next(1000, 9999).ToString("D4");
                    else
                        txtSTK.Text = "105" + rnd.Next(1000000, 9999999).ToString("D7");
                };

                rdoNgauNhien.CheckedChanged += (s, ev) => generateSTK();
                rdoSoDep.CheckedChanged += (s, ev) => generateSTK();
                generateSTK();

                Panel pnlContainer = CreatePanelDesign12();
                pnlContainer.Width = 696;
                var headerMoVay = new Label
                {
                    Text = "MỞ TÀI KHOẢN VAY DOANH NGHIỆP",
                    AutoSize = true,
                    Location = new Point(24, 12),
                    Font = fontBold
                };
                lblCCCD.Text = "MST / mã hồ sơ:";
                lblHoTen.Text = "Tên doanh nghiệp:";
                lblCCCD.Location = new Point(24, 52); txtCCCD.SetBounds(190, 48, 500, 28);
                lblHoTen.Location = new Point(24, 88); txtHoTen.SetBounds(190, 84, 500, 28);
                lblSDT.Location = new Point(24, 124); txtSDT.SetBounds(190, 120, 500, 28);
                foreach (var input in new[] { txtCCCD, txtHoTen, txtSDT }) { input.Enabled = true; input.ReadOnly = true; }
                btnDongSoHuu.SetBounds(24, 158, 260, 32);
                nudDongSoHuu.SetBounds(300, 161, 80, 28);
                pnlDongSoHuu.SetBounds(24, 198, 696, 234);

                RadioButton rdoTinChap = CreateRadioButtonDynamicDesign13(fontBold);
                RadioButton rdoTheChap = CreateRadioButtonDynamicDesign14(fontBold);

                Label lblMucLuong = CreateLabelDynamicDesign15(fontNormal);
                TextBox txtMucLuong = CreateTextBoxDynamicDesign16(fontNormal);

                Label lblTenTaiSan = CreateLabelDynamicDesign17(fontNormal);
                TextBox txtTenTaiSan = CreateTextBoxDynamicDesign18(fontNormal);

                Label lblDinhGia = CreateLabelDynamicDesign19(fontNormal);
                TextBox txtDinhGia = CreateTextBoxDynamicDesign20(fontNormal);

                Label lblLoaiTaiSan = CreateLabelDynamicDesign21(fontNormal);
                ComboBox cboLoaiTaiSan = CreateComboBoxDesign13();
                cboLoaiTaiSan.Items.AddRange(new object[] { "Tài sản cố định", "Sổ tiết kiệm trong ngân hàng", "Sổ tiết kiệm ngoài ngân hàng" });
                cboLoaiTaiSan.SelectedIndex = 0;
                Label lblSoCamCo = CreateLabelDynamicDesign22(fontNormal);
                ComboBox cboSoCamCo = CreateComboBoxDesign14();
                TextBox txtSoNgoai = CreateTextBoxDesign15();
                Label lblGocNgoai = CreateLabelDesign16();
                TextBox txtGocNgoai = CreateTextBoxDesign17();
                Label lblLaiNgoai = CreateLabelDesign18();
                TextBox txtLaiNgoai = CreateTextBoxDesign19();
                Label lblGiaTriCamCo = CreateLabelDesign20();
                string cccdLaySoTheChap = model.CCCD;
                if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
                {
                    using (var cn = new SqlConnection(_connStr))
                    using (var cmd = new SqlCommand(
                        "SELECT CCCD FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk", cn))
                    {
                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soTietKiemMacDinh;
                        cn.Open();
                        cccdLaySoTheChap = Convert.ToString(cmd.ExecuteScalar());
                    }
                }
                try { cboSoCamCo.Items.AddRange(FrmMainDN.LaySoTietKiemTheChap(_connStr, cccdLaySoTheChap).Cast<object>().ToArray()); }
                catch (Exception ex) { MessageBox.Show("Không đọc được sổ tiết kiệm: " + ex.Message); }
                if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
                {
                    for (int i = cboSoCamCo.Items.Count - 1; i >= 0; i--)
                    {
                        var so = cboSoCamCo.Items[i] as FrmMainDN.SoTietKiemTheChap;
                        if (so == null || !string.Equals(so.SoTKTK, soTietKiemMacDinh,
                            StringComparison.OrdinalIgnoreCase)) cboSoCamCo.Items.RemoveAt(i);
                    }
                }
                if (cboSoCamCo.Items.Count > 0) cboSoCamCo.SelectedIndex = 0;
                if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
                {
                    for (int i = 0; i < cboSoCamCo.Items.Count; i++)
                    {
                        var so = cboSoCamCo.Items[i] as FrmMainDN.SoTietKiemTheChap;
                        if (so != null && string.Equals(so.SoTKTK, soTietKiemMacDinh,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            cboSoCamCo.SelectedIndex = i;
                            break;
                        }
                    }
                }

                Label lblNhomNo = CreateLabelDynamicDesign23(fontNormal);
                ComboBox cboNhomNo = CreateComboBoxDynamicDesign24(fontNormal);
                cboNhomNo.Items.AddRange(new object[] {
                    "Nhóm 1 (Đủ tiêu chuẩn)",
                    "Nhóm 2 (Cần chú ý)",
                    "Nhóm 3 (Dưới tiêu chuẩn)",
                    "Nhóm 4 (Nghi ngờ)",
                    "Nhóm 5 (Mất vốn)"
                });
                int initNhomIndex = Math.Max(0, FrmMainDN.LayNhomNoHienTai(_connStr, model.CCCD) - 1);
                if (initNhomIndex >= cboNhomNo.Items.Count) initNhomIndex = 0;
                cboNhomNo.SelectedIndex = initNhomIndex;
                // Nhóm nợ hiện tại chỉ để xem, không cho thay đổi khi mở tài khoản mới.
                // Việc chuyển nhóm nợ chỉ thực hiện tại chức năng Chuyển nhóm nợ.
                cboNhomNo.Enabled = false;

                Label lblSoTienToiDa = CreateLabelDesign21();

                Label lblSoTienVay = CreateLabelDynamicDesign25(fontNormal);
                TextBox txtSoTienVay = CreateTextBoxDynamicDesign26(fontNormal);

                Label lblKyHan = CreateLabelDynamicDesign27(fontNormal);
                ComboBox cboKyHan = CreateComboBoxDynamicDesign28(fontNormal);

                Button btnXacNhan = CreateButtonDynamicDesign29(fontBold);
                btnXacNhan.FlatAppearance.BorderSize = 0;

                Button btnHuy = CreateButtonDynamicDesign30(fontNormal);
                btnHuy.FlatAppearance.BorderColor = Color.LightGray;

                decimal maxLoanAmount = 0;

                Action updateLayoutAndMaxLoan = () =>
                {
                    bool tin = rdoTinChap.Checked;
                    int loai = cboLoaiTaiSan.SelectedIndex;
                    bool batDongSan = !tin && loai == 0;
                    bool soNoi = !tin && loai == 1;
                    bool soNgoai = !tin && loai == 2;
                    lblMucLuong.Visible = txtMucLuong.Visible = tin;
                    lblLoaiTaiSan.Visible = cboLoaiTaiSan.Visible = !tin;
                    lblTenTaiSan.Visible = txtTenTaiSan.Visible = batDongSan;
                    lblDinhGia.Visible = txtDinhGia.Visible = batDongSan;
                    lblSoCamCo.Visible = soNoi || soNgoai;
                    lblSoCamCo.Text = soNoi ? "Chọn sổ tiết kiệm:" : "Số sổ bên ngoài:";
                    cboSoCamCo.Visible = soNoi;
                    txtSoNgoai.Visible = soNgoai;
                    lblGocNgoai.Visible = txtGocNgoai.Visible = soNgoai;
                    lblLaiNgoai.Visible = txtLaiNgoai.Visible = soNgoai;
                    lblGiaTriCamCo.Visible = soNoi;
                    int yNhom = tin ? 90 : batDongSan ? 160 : 225;
                    foreach (Control input in new Control[] { txtMucLuong, txtTenTaiSan, txtDinhGia, cboLoaiTaiSan, cboSoCamCo, txtSoNgoai, txtGocNgoai, txtLaiNgoai, cboNhomNo, txtSoTienVay, cboKyHan }) input.Width = 460;
                    lblTenTaiSan.Location = new Point(25, 90); txtTenTaiSan.Location = new Point(180, 87);
                    lblDinhGia.Location = new Point(25, 125); txtDinhGia.Location = new Point(180, 122);
                    lblNhomNo.Location = new Point(25, yNhom);
                    cboNhomNo.Location = new Point(180, yNhom - 3);
                    lblSoTienToiDa.Location = new Point(25, yNhom + 40);
                    lblSoTienVay.Location = new Point(25, yNhom + 78);
                    txtSoTienVay.Location = new Point(180, yNhom + 75);
                    lblKyHan.Location = new Point(25, yNhom + 113);
                    cboKyHan.Location = new Point(180, yNhom + 110);
                    btnXacNhan.SetBounds(180, yNhom + 160, 180, 36);
                    btnHuy.SetBounds(375, yNhom + 160, 150, 36);
                    pnlContainer.Height = yNhom + 215;
                    cboKyHan.Items.Clear();
                    if (tin) cboKyHan.Items.AddRange(new object[] { "6 tháng: 10.5%", "12 tháng: 11.0%", "24 tháng: 12.0%", "36 tháng: 12.5%" });
                    else if (soNoi) cboKyHan.Items.AddRange(new object[] { "6 tháng: 7.0%", "12 tháng: 7.0%", "24 tháng: 7.0%", "36 tháng: 7.0%" });
                    else if (soNgoai) cboKyHan.Items.AddRange(new object[] { "6 tháng: 8.5%", "12 tháng: 8.5%", "24 tháng: 8.5%", "36 tháng: 8.5%" });
                    else cboKyHan.Items.AddRange(new object[] { "6 tháng cố định: Từ 5.0%", "12 tháng cố định: Từ 6.5%", "24 tháng cố định: Từ 7.5%", "36 tháng cố định: Từ 8.5%", "60 tháng cố định: Từ 9.5%" });
                    cboKyHan.SelectedIndex = 0;
                    decimal parse(TextBox t) { decimal.TryParse(new string(t.Text.Where(char.IsDigit).ToArray()), out decimal n); return n; }
                    if (tin) maxLoanAmount = parse(txtMucLuong) * 10m;
                    else if (batDongSan) maxLoanAmount = parse(txtDinhGia) * .70m;
                    else if (soNoi)
                    {
                        var so = cboSoCamCo.SelectedItem as FrmMainDN.SoTietKiemTheChap;
                        maxLoanAmount = so == null ? 0 : Math.Floor(so.TongGiaTri * .95m);
                        lblGiaTriCamCo.Text = so == null ? "Không có sổ hợp lệ" : $"Gốc: {so.GocConLai:N0} | Lãi: {so.LaiTamTinh:N0} | Tổng: {so.TongGiaTri:N0}";
                    }
                    else maxLoanAmount = Math.Floor((parse(txtGocNgoai) + parse(txtLaiNgoai)) * .85m);
                    if (cboNhomNo.SelectedIndex >= 2) maxLoanAmount = 0;
                    lblSoTienToiDa.Text = $"Số tiền tối đa có thể vay: {maxLoanAmount:N0} VNĐ";
                    dlgMoTK.AutoScrollMinSize = new Size(0, pnlContainer.Bottom + 45);
                };
                rdoTinChap.CheckedChanged += (s, ev) => updateLayoutAndMaxLoan();
                rdoTheChap.CheckedChanged += (s, ev) => updateLayoutAndMaxLoan();
                // Chỉ khởi tạo trạng thái sau khi RadioButton đã cùng thuộc một panel.
                // Không chọn ở đây vì rdoTinChap còn Checked=true từ Designer.
                cboLoaiTaiSan.SelectedIndexChanged += (s, ev) => updateLayoutAndMaxLoan();
                cboSoCamCo.SelectedIndexChanged += (s, ev) => updateLayoutAndMaxLoan();
                txtGocNgoai.TextChanged += (s, ev) => updateLayoutAndMaxLoan();
                txtLaiNgoai.TextChanged += (s, ev) => updateLayoutAndMaxLoan();
                txtMucLuong.TextChanged += (s, ev) => updateLayoutAndMaxLoan();
                txtDinhGia.TextChanged += (s, ev) => updateLayoutAndMaxLoan();

                txtSoTienVay.KeyPress += (s, ev) => { if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true; };
                txtMucLuong.KeyPress += (s, ev) => { if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true; };
                txtDinhGia.KeyPress += (s, ev) => { if (!char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar)) ev.Handled = true; };

                txtSoTienVay.TextChanged += (s, ev) =>
                {
                    string digits = new string(txtSoTienVay.Text.Where(char.IsDigit).ToArray());
                    if (decimal.TryParse(digits, out decimal val))
                    {
                        txtSoTienVay.Text = $"{val:N0}";
                        txtSoTienVay.SelectionStart = txtSoTienVay.Text.Length;
                    }
                };

                txtMucLuong.TextChanged += (s, ev) =>
                {
                    string digits = new string(txtMucLuong.Text.Where(char.IsDigit).ToArray());
                    if (decimal.TryParse(digits, out decimal val))
                    {
                        txtMucLuong.Text = $"{val:N0}";
                        txtMucLuong.SelectionStart = txtMucLuong.Text.Length;
                    }
                };

                txtDinhGia.TextChanged += (s, ev) =>
                {
                    string digits = new string(txtDinhGia.Text.Where(char.IsDigit).ToArray());
                    if (decimal.TryParse(digits, out decimal val))
                    {
                        txtDinhGia.Text = $"{val:N0}";
                        txtDinhGia.SelectionStart = txtDinhGia.Text.Length;
                    }
                };

                updateLayoutAndMaxLoan();

                // Dịch phần nhập khoản vay xuống khi bật danh sách người được ủy quyền.
                Action updateDongSoHuuLayout = () =>
                {
                    int offset = hienUyQuyen ? pnlDongSoHuu.Height + 10 : 0;
                    lblLoaiTK.Location = new Point(24, 204 + offset);
                    cboLoaiTK.SetBounds(190, 200 + offset, 500, 28);
                    rdoNgauNhien.Location = new Point(24, 240 + offset);
                    rdoSoDep.Location = new Point(260, 240 + offset);
                    lblSTK.Location = new Point(24, 276 + offset);
                    txtSTK.SetBounds(190, 272 + offset, 500, 28);
                    pnlContainer.Location = new Point(24, 310 + offset);
                    dlgMoTK.AutoScrollMinSize = new Size(0, pnlContainer.Bottom + 45);
                };
                pnlDongSoHuu.VisibleChanged += (ss, ee) => updateDongSoHuuLayout();
                updateDongSoHuuLayout();

                btnHuy.Click += (s, ev) => dlgMoTK.Close();

                btnXacNhan.Click += (s, ev) =>
                {
                    if (!KiemTraQuyenMoVayDN(model)) return;
                    string digitsMoney = new string(txtSoTienVay.Text.Where(char.IsDigit).ToArray());
                    if (!decimal.TryParse(digitsMoney, out decimal valSoTienVay) || valSoTienVay <= 0)
                    {
                        MessageBox.Show("Vui lòng nhập số tiền vay hợp lệ (lớn hơn 0)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSoTienVay.Focus();
                        return;
                    }

                    if (valSoTienVay > maxLoanAmount)
                    {
                        MessageBox.Show($"Số tiền muốn vay ({valSoTienVay:N0} VNĐ) vượt quá số tiền tối đa có thể vay ({maxLoanAmount:N0} VNĐ)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSoTienVay.Focus();
                        return;
                    }

                    string stkMoi = txtSTK.Text.Trim();
                    if (string.IsNullOrEmpty(stkMoi))
                    {
                        MessageBox.Show("Chưa tạo được số tài khoản vay!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string kyHanSel = cboKyHan.SelectedItem.ToString();
                    string kyHanTen = "12 tháng";
                    double laiSuatVal = 8.5;

                    if (kyHanSel.Contains("6 tháng")) kyHanTen = "6 tháng";
                    else if (kyHanSel.Contains("12 tháng")) kyHanTen = "12 tháng";
                    else if (kyHanSel.Contains("24 tháng")) kyHanTen = "24 tháng";
                    else if (kyHanSel.Contains("36 tháng")) kyHanTen = "36 tháng";
                    else if (kyHanSel.Contains("60 tháng")) kyHanTen = "60 tháng";

                    int idxPercent = kyHanSel.IndexOf('%');
                    if (idxPercent > 0)
                    {
                        int idxStart = idxPercent - 1;
                        while (idxStart >= 0 && (char.IsDigit(kyHanSel[idxStart]) || kyHanSel[idxStart] == '.' || kyHanSel[idxStart] == ','))
                        {
                            idxStart--;
                        }
                        string numStr = kyHanSel.Substring(idxStart + 1, idxPercent - idxStart - 1).Replace(',', '.').Trim();
                        double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out laiSuatVal);
                    }

                    if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh) &&
                        (rdoTinChap.Checked || !rdoTheChap.Checked || cboLoaiTaiSan.SelectedIndex != 1 ||
                         !(cboSoCamCo.SelectedItem is FrmMainDN.SoTietKiemTheChap) ||
                         !string.Equals(((FrmMainDN.SoTietKiemTheChap)cboSoCamCo.SelectedItem).SoTKTK,
                                        soTietKiemMacDinh, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("Khoản vay phải thế chấp bằng đúng sổ tiết kiệm đã chọn.",
                            "Sổ thế chấp không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 1 && cboSoCamCo.SelectedItem == null)
                    { MessageBox.Show("Chưa có sổ tiết kiệm nội bộ hợp lệ để thế chấp."); return; }
                    if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 2 && string.IsNullOrWhiteSpace(txtSoNgoai.Text))
                    { MessageBox.Show("Nhập số sổ tiết kiệm ngoài ngân hàng."); return; }
                    if (cboNhomNo.SelectedIndex >= 2)
                    { MessageBox.Show("Nhóm nợ hiện tại không đủ điều kiện mở khoản vay."); return; }
                    if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 1) laiSuatVal = 7.0;
                    if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex == 2) laiSuatVal = 8.5;
                    int selectedNhomCode = cboNhomNo.SelectedIndex + 1;
                    string selectedNhomTen = cboNhomNo.SelectedItem.ToString();

                    // Kiểm tra trùng CCCD/SĐT trong biểu mẫu và dữ liệu SQL.
                    var dongSoHuuMoi = new List<Tuple<string, string, string>>();
                    var cccdMoi = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD) };
                    var sdtMoi = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var fields in dongSoHuuInputs)
                    {
                        string c = fields.Item1.Text.Trim(), n = fields.Item2.Text.Trim(), t = fields.Item3.Text.Trim();
                        if (c.Length != 12 || !c.All(char.IsDigit) || string.IsNullOrWhiteSpace(n) ||
                            t.Length < 10 || t.Length > 11 || !t.All(char.IsDigit) ||
                            !cccdMoi.Add(c) || !sdtMoi.Add(t))
                        {
                            MessageBox.Show("Thông tin người được ủy quyền không hợp lệ hoặc bị trùng CCCD/SĐT!", "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        dongSoHuuMoi.Add(Tuple.Create(c, n, t));
                    }
                    if (dongSoHuuMoi.Count < 1)
                    {
                        MessageBox.Show("Vui lòng thêm ít nhất 1 người được ủy quyền trước khi mở tài khoản.",
                            "Thiếu người được ủy quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!TrySqlAction(() =>
                    {
                        using (var conn = new SqlConnection(_connStr))
                        {
                            conn.Open();
                            using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                try
                                {
                                    if (stkMoi.Length > 20 || valSoTienVay > 999999999999999999m)
                                        throw new InvalidOperationException("Số tài khoản hoặc số tiền vay vượt giới hạn dữ liệu.");
                                    using (var checkSTK = new SqlCommand(@"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk)
                                        OR EXISTS(SELECT 1 FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk)
                                        OR EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk) THEN 1 ELSE 0 END", conn, transaction))
                                    {
                                        checkSTK.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkMoi;
                                        if (Convert.ToInt32(checkSTK.ExecuteScalar()) != 0)
                                            throw new InvalidOperationException("Số tài khoản đã tồn tại. Vui lòng tạo số khác.");
                                    }
                                    foreach (var d in dongSoHuuMoi)
                                        FrmMainDN.KiemTraNguoiUyQuyenKhiThemDN(conn, transaction, d.Item1, d.Item2, d.Item3);
                                    // Không tạo hồ sơ doanh nghiệp từ CCCD của người được ủy quyền.
                                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.DN_KhachHang WHERE CCCD=@cccd", conn, transaction))
                                    {
                                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = model.CCCD;
                                        if (Convert.ToInt32(cmd.ExecuteScalar()) != 1)
                                            throw new InvalidOperationException("Không tìm thấy hồ sơ chủ tài khoản doanh nghiệp.");
                                    }
                                    FrmMain.KiemTraNhomNoTruocMoVay(conn, transaction, true, model.CCCD, (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD));
                                    if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex > 0)
                                        FrmMain.KiemTraSoTietKiemChuaVay(conn, transaction, true, cboLoaiTaiSan.SelectedIndex == 2,
                                            cboLoaiTaiSan.SelectedIndex == 1 ? ((FrmMainDN.SoTietKiemTheChap)cboSoCamCo.SelectedItem).SoTKTK : txtSoNgoai.Text.Trim());
                                    using (var cmd = new SqlCommand(@"INSERT INTO dbo.DN_TaiKhoanVay
                                        (SoTKVay, CCCD, SoTienVay, SoTienDaTra, LaiSuat, KyHan, TrangThai, NgayMo)
                                        VALUES (@stk,@cccd,@tien,0,@lai,@kyhan,0,GETDATE())", conn, transaction))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 30).Value = stkMoi;
                                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD);
                                        cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = valSoTienVay;
                                        cmd.Parameters.Add("@lai", SqlDbType.Float).Value = laiSuatVal;
                                        cmd.Parameters.Add("@kyhan", SqlDbType.NVarChar, 30).Value = kyHanTen;
                                        cmd.ExecuteNonQuery();
                                    }
                                    foreach (var d in dongSoHuuMoi)
                                    {
                                        using (var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@chu AND CCCD=@cccd)
                                            INSERT INTO dbo.DN_DongSoHuu(CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai) VALUES (@chu,@cccd,@ten,@sdt);
                                            INSERT INTO dbo.DN_DongSoHuuTaiKhoanVay(SoTKVay,CCCD,HoTen,SoDienThoai) VALUES(@stk,@cccd,@ten,@sdt);", conn, transaction))
                                        {
                                            cmd.Parameters.Add("@chu", SqlDbType.VarChar, 12).Value = (model.RawKhachHang != null ? model.RawKhachHang.CCCD : model.CCCD);
                                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkMoi;
                                            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = d.Item1;
                                            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 200).Value = d.Item2;
                                            cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 11).Value = d.Item3;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    {
                                        string ghiChu = rdoTinChap.Checked ? "Vay tín chấp" : "Vay thế chấp tài sản cố định";
                                        if (rdoTheChap.Checked && cboLoaiTaiSan.SelectedIndex > 0)
                                        {
                                            string loaiCamCo = cboLoaiTaiSan.SelectedIndex == 1 ? "Trong ngân hàng" : "Ngoài ngân hàng";
                                            string soCamCo = cboLoaiTaiSan.SelectedIndex == 1
                                                ? ((FrmMainDN.SoTietKiemTheChap)cboSoCamCo.SelectedItem).SoTKTK : txtSoNgoai.Text.Trim();
                                            ghiChu = $"Vay thế chấp sổ tiết kiệm {loaiCamCo}; Số sổ: {soCamCo}; Hạn mức: {maxLoanAmount:N0} VND; Lãi suất vay: {laiSuatVal}%/năm";
                                        }
                                        using (var history = new SqlCommand(@"INSERT INTO dbo.DN_LichSuGiaoDichVay
                                            (SoTKVay, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, TongTien, SoDuConLai, GhiChu, NguoiThucHien)
                                            VALUES (@stk, GETDATE(), N'Giải ngân', @tien, 0, @tien, @tien, @note, @nguoi)", conn, transaction))
                                        {
                                            history.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                            history.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stkMoi;
                                            history.Parameters.Add("@tien", SqlDbType.Decimal).Value = valSoTienVay;
                                            history.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value = ghiChu;
                                            history.ExecuteNonQuery();
                                        }
                                    }
                                    transaction.Commit();
                                }
                                catch { transaction.Rollback(); throw; }
                            }
                        }
                    })) return;
                    RefreshSqlData();

                    string loaiVayStr = rdoTinChap.Checked ? "Vay Tín Chấp" : "Vay Thế Chấp";
                    string phiText = rdoSoDep.Checked ? "\n(Phí mở số đẹp: 500,000 VNĐ)" : "";
                    MessageBox.Show($"Mở thành công tài khoản vay mới!\n\n• Loại vay: {loaiVayStr}\n• Chủ sở hữu: {model.HoTen}\n• Số TK Vay: {stkMoi}\n• Số tiền vay: {valSoTienVay:N0} VNĐ\n• Lãi suất: {laiSuatVal}%\n• Kỳ hạn: {kyHanTen}\n• Nhóm nợ: {selectedNhomTen}{phiText}", "Thông báo thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    dlgMoTK.DialogResult = DialogResult.OK;
                    OnDataChanged();
                };

                pnlContainer.Controls.AddRange(new Control[] {
                    rdoTinChap, rdoTheChap,
                    lblMucLuong, txtMucLuong,
                    lblTenTaiSan, txtTenTaiSan, lblDinhGia, txtDinhGia,
                    lblLoaiTaiSan, cboLoaiTaiSan, lblSoCamCo, cboSoCamCo, txtSoNgoai,
                    lblGocNgoai, txtGocNgoai, lblLaiNgoai, txtLaiNgoai, lblGiaTriCamCo,
                    lblNhomNo, cboNhomNo,
                    lblSoTienToiDa,
                    lblSoTienVay, txtSoTienVay,
                    lblKyHan, cboKyHan,
                    btnXacNhan, btnHuy
                });

                dlgMoTK.Controls.AddRange(new Control[] {
                    headerMoVay, lblCCCD, txtCCCD, lblHoTen, txtHoTen, lblSDT, txtSDT, btnDongSoHuu, nudDongSoHuu, pnlDongSoHuu,
                    lblLoaiTK, cboLoaiTK, rdoNgauNhien, rdoSoDep,
                    lblSTK, txtSTK, pnlContainer
                });

                // QUAN TRỌNG: Sau khi hai RadioButton thuộc CÙNG pnlContainer,
                // mới chuyển từ tín chấp sang thế chấp; nếu làm trước, cả hai
                // có thể cùng Checked=true và công thức vẫn tính theo tín chấp.
                if (!string.IsNullOrWhiteSpace(soTietKiemMacDinh))
                {
                    rdoTinChap.Checked = false;
                    rdoTheChap.Checked = true;
                    cboLoaiTaiSan.SelectedIndex = 1; // Sổ tiết kiệm trong ngân hàng
                    int indexSo = -1;
                    for (int i = 0; i < cboSoCamCo.Items.Count; i++)
                    {
                        var itemSo = cboSoCamCo.Items[i] as FrmMainDN.SoTietKiemTheChap;
                        if (itemSo != null && string.Equals(itemSo.SoTKTK, soTietKiemMacDinh, StringComparison.OrdinalIgnoreCase))
                        { indexSo = i; break; }
                    }
                    if (indexSo >= 0) cboSoCamCo.SelectedIndex = indexSo;
                    else cboSoCamCo.SelectedIndex = -1;
                    // Luồng gọi từ sổ tiết kiệm chỉ được mở vay bằng sổ đang chọn.
                    rdoTinChap.Enabled = false;
                    rdoTheChap.Enabled = false;
                    cboLoaiTaiSan.Enabled = false;
                    cboSoCamCo.Enabled = false;
                }
                updateLayoutAndMaxLoan();
                updateDongSoHuuLayout();
                DongBoMauFormPhu(dlgMoTK);
                if (dlgMoTK.ShowDialog() == DialogResult.OK)
                {
                    HienThiDanhSachVay(false);
                }
            }
        }
        #endregion

        #region Tính Toán & Chuyển Nhóm Nợ Đồng Bộ
        public static string GetTenNhomNoStatic(int code)
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

        private string GetTenNhomNo(int code) => GetTenNhomNoStatic(code);

        public static void ChuyenNhomNoChoTatCaTaiKhoanTheoCCCD(IList danhSachKhachHang, string targetCCCD, int newCode)
        {
            if (string.IsNullOrWhiteSpace(targetCCCD) || danhSachKhachHang == null) return;

            HashSet<string> dsCCCDLienQuan = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { targetCCCD };

            foreach (object kh in danhSachKhachHang)
            {
                string cccdKh = LayStringStatic(kh, "CCCD");
                IList dsDSH = LayDanhSachStatic(kh, "DanhSachDongSoHuu");

                bool coLienQuan = cccdKh.Equals(targetCCCD, StringComparison.OrdinalIgnoreCase);
                if (!coLienQuan)
                {
                    foreach (object dsh in dsDSH)
                    {
                        if (LayStringStatic(dsh, "CCCD").Equals(targetCCCD, StringComparison.OrdinalIgnoreCase))
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
                        string cccdDsh = LayStringStatic(dsh, "CCCD");
                        if (!string.IsNullOrEmpty(cccdDsh)) dsCCCDLienQuan.Add(cccdDsh);
                    }
                }
            }

            string tenNhomMoi = GetTenNhomNoStatic(newCode);

            foreach (object kh in danhSachKhachHang)
            {
                string cccdKh = LayStringStatic(kh, "CCCD");
                IList dsDSH = LayDanhSachStatic(kh, "DanhSachDongSoHuu");

                bool thuocNhom = dsCCCDLienQuan.Contains(cccdKh);
                if (!thuocNhom)
                {
                    foreach (object dsh in dsDSH)
                    {
                        if (dsCCCDLienQuan.Contains(LayStringStatic(dsh, "CCCD")))
                        {
                            thuocNhom = true;
                            break;
                        }
                    }
                }

                if (thuocNhom)
                {
                    IList dsVay = LayDanhSachStatic(kh, "DanhSachVay");
                    foreach (object vay in dsVay)
                    {
                        string stk = LayStringStatic(vay, "SoTKVay");
                        if (!string.IsNullOrEmpty(stk))
                        {
                            dictCustomNhomNo[stk] = newCode;
                            CapNhatThuocTinh(vay, "NhomNo", tenNhomMoi);
                        }
                    }

                    IList dsTK = LayDanhSachStatic(kh, "DanhSachTietKiem");
                    foreach (object tk in dsTK)
                    {
                        string stk = LayStringStatic(tk, "SoTKTK");
                        if (!string.IsNullOrEmpty(stk))
                        {
                            dictCustomNhomNo[stk] = newCode;
                            CapNhatThuocTinh(tk, "NhomNo", tenNhomMoi);
                        }
                    }

                    IList dsTT = LayDanhSachStatic(kh, "DanhSachThanhToan");
                    foreach (object tt in dsTT)
                    {
                        string stk = LayStringStatic(tt, "SoTKTT");
                        if (!string.IsNullOrEmpty(stk))
                        {
                            dictCustomNhomNo[stk] = newCode;
                            CapNhatThuocTinh(tt, "NhomNo", tenNhomMoi);
                        }
                    }
                }
            }
        }

        private static string LayStringStatic(object obj, string propName)
        {
            if (obj == null) return "";
            PropertyInfo p = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            return p?.GetValue(obj, null)?.ToString() ?? "";
        }

        private static IList LayDanhSachStatic(object obj, string propName)
        {
            if (obj == null) return new ArrayList();
            PropertyInfo p = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            object val = p?.GetValue(obj, null);
            if (val is IList list) return list;
            return new ArrayList();
        }

        private static void CapNhatThuocTinh(object obj, string propName, object val)
        {
            if (obj == null) return;
            PropertyInfo p = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.CanWrite)
            {
                p.SetValue(obj, val, null);
            }
        }

        private void TinhNhomNo(VayGridRowModel item)
        {
            if (dictCustomNhomNo.ContainsKey(item.SoTKVay))
            {
                item.NhomNoCode = dictCustomNhomNo[item.SoTKVay];
                item.NhomNoTen = GetTenNhomNo(item.NhomNoCode);
                SyncNhomNoToRawItem(item.RawVayItem, item.NhomNoTen);
                return;
            }

            if (item.DuNo <= 0)
            {
                item.NhomNoCode = 1;
                item.NhomNoTen = "Nhóm 1 (Đủ tiêu chuẩn)";
                SyncNhomNoToRawItem(item.RawVayItem, item.NhomNoTen);
                return;
            }

            int months = ParseKyHanToMonths(item.KyHan);
            DateTime ngayDaoHan = item.NgayMo.AddMonths(months);

            if (DateTime.Now <= ngayDaoHan)
            {
                item.NhomNoCode = 1;
                item.NhomNoTen = "Nhóm 1 (Đủ tiêu chuẩn)";
                SyncNhomNoToRawItem(item.RawVayItem, item.NhomNoTen);
                return;
            }

            int soNgayQuaHan = (int)(DateTime.Now - ngayDaoHan).TotalDays;

            if (soNgayQuaHan < 10)
            {
                item.NhomNoCode = 1;
                item.NhomNoTen = "Nhóm 1 (Đủ tiêu chuẩn)";
            }
            else if (soNgayQuaHan <= 90)
            {
                item.NhomNoCode = 2;
                item.NhomNoTen = "Nhóm 2 (Cần chú ý)";
            }
            else if (soNgayQuaHan <= 180)
            {
                item.NhomNoCode = 3;
                item.NhomNoTen = "Nhóm 3 (Dưới tiêu chuẩn)";
            }
            else if (soNgayQuaHan <= 360)
            {
                item.NhomNoCode = 4;
                item.NhomNoTen = "Nhóm 4 (Nghi ngờ)";
            }
            else
            {
                item.NhomNoCode = 5;
                item.NhomNoTen = "Nhóm 5 (Mất vốn)";
            }

            SyncNhomNoToRawItem(item.RawVayItem, item.NhomNoTen);
        }

        private void SyncNhomNoToRawItem(FrmMainDN.TaiKhoanVayItem rawItem, string nhomNoTen)
        {
            if (rawItem == null) return;
            PropertyInfo pNhomNo = rawItem.GetType().GetProperty("NhomNo");
            if (pNhomNo != null && pNhomNo.CanWrite)
            {
                pNhomNo.SetValue(rawItem, nhomNoTen, null);
            }
        }

        // Tìm các khoản vay liên quan đến CCCD, bao gồm chủ và người được ủy quyền,
        // cùng quy tắc nhóm người đang dùng trong form.
        private HashSet<string> LayTaiKhoanVayLienQuan(string targetCCCD)
        {
            var cccdLienQuan = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { targetCCCD };
            foreach (var kh in listKhachHang)
            {
                bool coLienQuan = string.Equals(kh.CCCD, targetCCCD, StringComparison.OrdinalIgnoreCase)
                    || kh.DanhSachDongSoHuu.Any(d => string.Equals(d.CCCD, targetCCCD, StringComparison.OrdinalIgnoreCase));
                if (!coLienQuan) continue;
                cccdLienQuan.Add(kh.CCCD);
                foreach (var d in kh.DanhSachDongSoHuu) cccdLienQuan.Add(d.CCCD);
            }

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kh in listKhachHang)
            {
                bool coLienQuan = cccdLienQuan.Contains(kh.CCCD)
                    || kh.DanhSachDongSoHuu.Any(d => cccdLienQuan.Contains(d.CCCD));
                if (!coLienQuan) continue;
                foreach (var vay in kh.DanhSachVay)
                    if (!string.IsNullOrWhiteSpace(vay.SoTKVay)) result.Add(vay.SoTKVay);
            }
            return result;
        }

        // Cùng một transaction: lưu nhóm nợ + ngày cập nhật cho toàn bộ khoản vay liên quan.
        // Không sửa NgayMo hay TrangThai (khóa/hoạt động/đóng) của tài khoản.
        private void LuuChuyenNhomNoVaoSql(IEnumerable<string> soTaiKhoan, int nhomNo)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (string stk in soTaiKhoan)
                        {
                            using (var cmd = new SqlCommand(@"
                                UPDATE dbo.DN_TaiKhoanVay
                                SET NgayCapNhatTrangThai = GETDATE()
                                WHERE SoTKVay = @stk", conn, tx))
                            {
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                if (cmd.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("Không tìm thấy tài khoản vay: " + stk);
                            }
                            using (var cmd = new SqlCommand(@"
                                UPDATE dbo.DN_NhomNoTaiKhoan
                                SET NhomNo = @nhom, NgayCapNhat = GETDATE()
                                WHERE SoTaiKhoan = @stk;
                                IF @@ROWCOUNT = 0
                                    INSERT INTO dbo.DN_NhomNoTaiKhoan (SoTaiKhoan, NhomNo, NgayCapNhat)
                                    VALUES (@stk, @nhom, GETDATE());", conn, tx))
                            {
                                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                cmd.Parameters.Add("@nhom", SqlDbType.Int).Value = nhomNo;
                                cmd.ExecuteNonQuery();
                            }
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
        }

        private void MenuChuyenNhomNo_Click(object sender, EventArgs e)
        {
            var model = GetSelectedRowModel();
            if (model == null) return;

            using (Form dlgChuyenNhom = new Form())
            {
                dlgChuyenNhom.Text = $"Chuyển nhóm nợ - CCCD: {model.CCCD} (TK: {model.SoTKVay})";
                dlgChuyenNhom.Size = new Size(420, 210);
                dlgChuyenNhom.StartPosition = FormStartPosition.CenterParent;
                dlgChuyenNhom.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlgChuyenNhom.MaximizeBox = false;
                dlgChuyenNhom.MinimizeBox = false;

                Label lblPrompt = CreateLabelDynamicDesign31($"Chọn nhóm nợ mới cho TẤT CẢ TK thuộc CCCD {model.CCCD} ({model.HoTen}):");
                ComboBox cboNhomMoi = CreateComboBoxDesign22();

                cboNhomMoi.Items.Add("Nhóm 1 (Đủ tiêu chuẩn)");
                cboNhomMoi.Items.Add("Nhóm 2 (Cần chú ý)");
                cboNhomMoi.Items.Add("Nhóm 3 (Dưới tiêu chuẩn)");
                cboNhomMoi.Items.Add("Nhóm 4 (Nghi ngờ)");
                cboNhomMoi.Items.Add("Nhóm 5 (Mất vốn)");

                int currentIndex = model.NhomNoCode - 1;
                if (currentIndex >= 0 && currentIndex < 5) cboNhomMoi.SelectedIndex = currentIndex;
                else cboNhomMoi.SelectedIndex = 0;

                Button btnXacNhan = CreateButtonDesign23();
                Button btnHuy = CreateButtonDesign24();

                btnXacNhan.Click += (s, ev) =>
                {
                    int newCode = cboNhomMoi.SelectedIndex + 1;
                    // Ghi nhóm nợ và NGÀY CẬP NHẬT vào SQL trước khi cập nhật giao diện.
                    // Chỉ sửa những khoản vay thực sự thuộc nhóm liên quan.
                    var affectedLoans = LayTaiKhoanVayLienQuan(model.CCCD);
                    if (affectedLoans.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy tài khoản vay cần cập nhật.", "Thông báo");
                        return;
                    }
                    if (!TrySqlAction(() => LuuChuyenNhomNoVaoSql(affectedLoans, newCode))) return;
                    ChuyenNhomNoChoTatCaTaiKhoanTheoCCCD(listKhachHang, model.CCCD, newCode);
                    RefreshSqlData();

                    MessageBox.Show($"Đã chuyển tất cả tài khoản thuộc CCCD {model.CCCD} sang {cboNhomMoi.SelectedItem}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlgChuyenNhom.DialogResult = DialogResult.OK;
                    OnDataChanged();
                };

                btnHuy.Click += (s, ev) => dlgChuyenNhom.Close();

                dlgChuyenNhom.Controls.AddRange(new Control[] { lblPrompt, cboNhomMoi, btnXacNhan, btnHuy });
                DongBoMauFormPhu(dlgChuyenNhom);
                if (dlgChuyenNhom.ShowDialog() == DialogResult.OK)
                {
                    HienThiDanhSachVay(false);
                }
            }
        }
        #endregion

        #region Hiển Thị Danh Sách Lên DataGridView & Thống Kê
        private HashSet<string> LayCacKhoanVayBangSoTietKiem()
        {
            var ketQua = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                SELECT DISTINCT h.SoTKVay
                FROM dbo.DN_LichSuGiaoDichVay h
                WHERE h.LoaiGiaoDich=N'Giải ngân'
                  AND h.GhiChu LIKE N'%Số sổ: %'
                  AND h.GhiChu LIKE N'%Vay thế chấp sổ tiết kiệm%'", cn))
            {
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        ketQua.Add(Convert.ToString(rd[0]));
            }
            return ketQua;
        }

        // Chỉ cộng lãi và phí của các giao dịch thanh toán. Gốc đã trả lấy từ
        // TaiKhoanVay.SoTienDaTra để giữ nguyên nghiệp vụ tính dư nợ hiện tại.
        // Loại trừ giải ngân: đó là tiền ngân hàng cấp, không phải tiền khách đã trả.
        private Dictionary<string, decimal> LayTongLaiVaPhiDaThanhToan()
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                SELECT SoTKVay, SUM(ISNULL(SoTienLai, 0) + ISNULL(PhiPhat, 0)) AS LaiVaPhi
                FROM dbo.DN_LichSuGiaoDichVay
                WHERE LoaiGiaoDich <> N'Giải ngân'
                GROUP BY SoTKVay", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        result[Convert.ToString(r["SoTKVay"])] = Convert.ToDecimal(r["LaiVaPhi"]);
            }
            return result;
        }

        private void HienThiDanhSachVay(bool taiTongHop = true)
        {
            if (_dangDungBangVay || IsDisposed || Disposing) return;
            _timerLocVay.Stop();
            _dangDungBangVay = true;
            dgvVay.SuspendLayout();
            try
            {
                // Khoản vay thế chấp sổ tiết kiệm chỉ hiển thị người đứng tên
                // trong TaiKhoanVay.CCCD. Không tự kế thừa người được ủy quyền của sổ.
                if (taiTongHop || _cacheVayBangSo == null || _cacheLaiVaPhi == null)
                {
                    try
                    {
                        var vayMoi = LayCacKhoanVayBangSoTietKiem();
                        var laiMoi = LayTongLaiVaPhiDaThanhToan();
                        _cacheVayBangSo = vayMoi;
                        _cacheLaiVaPhi = laiMoi;
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Không thể tải tổng hợp khoản vay: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                HashSet<string> vayBangSo = _cacheVayBangSo;
                Dictionary<string, decimal> laiVaPhiDaTra = _cacheLaiVaPhi;
                dgvVay.Rows.Clear();
                List<VayGridRowModel> gridList = new List<VayGridRowModel>();

                foreach (var kh in listKhachHang)
                {
                    foreach (var vay in kh.DanhSachVay)
                    {
                        if (!dictSoTienGocDaTra.ContainsKey(vay.SoTKVay))
                        {
                            dictSoTienGocDaTra[vay.SoTKVay] = vay.SoTienDaTra;
                        }

                        var rowChuo = new VayGridRowModel
                        {
                            SoTKVay = vay.SoTKVay,
                            CCCD = kh.CCCD,
                            HoTen = kh.HoTen,
                            SoDienThoai = kh.SoDienThoai,
                            SoTienVay = vay.SoTienVay,
                            SoTienDaTra = vay.SoTienDaTra,
                            SoTienGocDaTra = dictSoTienGocDaTra[vay.SoTKVay],
                            TongTienDaThanhToan = vay.SoTienDaTra + (laiVaPhiDaTra.TryGetValue(vay.SoTKVay, out decimal tongLaiPhi) ? tongLaiPhi : 0m),
                            LaiSuat = vay.LaiSuat,
                            KyHan = vay.KyHan,
                            NgayMo = vay.NgayMo,
                            NgayCapNhatTrangThai = vay.NgayCapNhatTrangThai,
                            VaiTro = "Chủ sở hữu",
                            TrangThai = vay.TrangThai,
                            RawVayItem = vay,
                            RawKhachHang = kh
                        };
                        TinhNhomNo(rowChuo);
                        gridList.Add(rowChuo);

                        foreach (var dsh in LayUyQuyenKhoanVayDN(vay.SoTKVay))
                        {
                            var rowDsh = new VayGridRowModel
                            {
                                SoTKVay = vay.SoTKVay,
                                CCCD = dsh.CCCD,
                                HoTen = dsh.HoTen,
                                SoDienThoai = dsh.SoDienThoai,
                                SoTienVay = vay.SoTienVay,
                                SoTienDaTra = vay.SoTienDaTra,
                                SoTienGocDaTra = dictSoTienGocDaTra[vay.SoTKVay],
                                TongTienDaThanhToan = vay.SoTienDaTra + (laiVaPhiDaTra.TryGetValue(vay.SoTKVay, out decimal tongLaiPhiDsh) ? tongLaiPhiDsh : 0m),
                                LaiSuat = vay.LaiSuat,
                                KyHan = vay.KyHan,
                                NgayMo = vay.NgayMo,
                                NgayCapNhatTrangThai = vay.NgayCapNhatTrangThai,
                                VaiTro = "Người được ủy quyền",
                                TrangThai = vay.TrangThai,
                                RawVayItem = vay,
                                RawKhachHang = kh
                            };
                            TinhNhomNo(rowDsh);
                            gridList.Add(rowDsh);
                        }
                    }
                }

                // Đồng bộ Nhóm nợ cao nhất cho tất cả các tài khoản của mỗi CCCD
                var cccdMaxNhom = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in gridList)
                {
                    if (!cccdMaxNhom.ContainsKey(item.CCCD) || item.NhomNoCode > cccdMaxNhom[item.CCCD])
                    {
                        cccdMaxNhom[item.CCCD] = item.NhomNoCode;
                    }
                }

                foreach (var item in gridList)
                {
                    if (cccdMaxNhom.TryGetValue(item.CCCD, out int maxCode) && maxCode > item.NhomNoCode)
                    {
                        item.NhomNoCode = maxCode;
                        item.NhomNoTen = GetTenNhomNo(maxCode);
                        SyncNhomNoToRawItem(item.RawVayItem, item.NhomNoTen);
                    }
                }

                // 1. LỌC THEO CCCD (Từ ô txtCCCDSearch)
                string filterCCCD = txtCCCDSearch.Text.Trim();
                if (string.IsNullOrEmpty(filterCCCD) && !string.IsNullOrEmpty(SelectedCCCD))
                {
                    filterCCCD = SelectedCCCD.Trim();
                    txtCCCDSearch.Text = filterCCCD;
                }

                // Khi mở từ FrmMainDN bằng số tài khoản, phải giữ cả chủ sở hữu
                // và người được ủy quyền của cùng tài khoản, không lọc mất các dòng
                // có CCCD khác với khách hàng đang chọn.
                bool dangXemTaiKhoanDuocChon =
                    (SelectedSoTKList != null && SelectedSoTKList.Count > 0) ||
                    !string.IsNullOrWhiteSpace(SelectedSoTK);

                if (!dangXemTaiKhoanDuocChon && !string.IsNullOrWhiteSpace(filterCCCD))
                {
                    gridList = gridList.Where(x =>
                        x.CCCD != null && x.CCCD.Contains(filterCCCD)).ToList();
                }

                // 2. Lọc theo Số TK Vay (Nhiều TK hoặc 1 TK cụ thể)
                if (SelectedSoTKList != null && SelectedSoTKList.Count > 0)
                {
                    gridList = gridList.Where(x => SelectedSoTKList.Contains(x.SoTKVay)).ToList();
                }
                else if (!string.IsNullOrEmpty(SelectedSoTK))
                {
                    gridList = gridList.Where(x => x.SoTKVay != null && x.SoTKVay.Equals(SelectedSoTK, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // 3. Lọc Nhóm nợ
                if (cboLocNhom.SelectedIndex > 0)
                {
                    int nhomSelected = cboLocNhom.SelectedIndex;
                    gridList = gridList.Where(x => x.NhomNoCode == nhomSelected).ToList();
                }

                // 4. Lọc Kỳ hạn
                if (cboLocKyHan.SelectedIndex > 0)
                {
                    string kyHanSelected = cboLocKyHan.SelectedItem.ToString();
                    gridList = gridList.Where(x => x.KyHan.Equals(kyHanSelected, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // 5. Lọc Thời gian
                string optThoiGian = cboLocThoiGian.SelectedItem != null ? cboLocThoiGian.SelectedItem.ToString() : "Tất cả";
                DateTime today = DateTime.Today;

                switch (optThoiGian)
                {
                    case "Hôm nay":
                        gridList = gridList.Where(x => x.NgayMo.Date == today).ToList();
                        break;
                    case "Hôm qua":
                        gridList = gridList.Where(x => x.NgayMo.Date == today.AddDays(-1)).ToList();
                        break;
                    case "7 ngày":
                        gridList = gridList.Where(x => x.NgayMo.Date >= today.AddDays(-7) && x.NgayMo.Date <= today).ToList();
                        break;
                    case "1 tháng":
                        gridList = gridList.Where(x => x.NgayMo.Date >= today.AddMonths(-1) && x.NgayMo.Date <= today).ToList();
                        break;
                    case "3 tháng":
                        gridList = gridList.Where(x => x.NgayMo.Date >= today.AddMonths(-3) && x.NgayMo.Date <= today).ToList();
                        break;
                    case "1 năm":
                        gridList = gridList.Where(x => x.NgayMo.Date >= today.AddYears(-1) && x.NgayMo.Date <= today).ToList();
                        break;
                    case "Tùy chọn":
                        DateTime tuNgay = dtpTuNgay.Value.Date;
                        DateTime denNgay = dtpDenNgay.Value.Date;
                        gridList = gridList.Where(x => x.NgayMo.Date >= tuNgay && x.NgayMo.Date <= denNgay).ToList();
                        break;
                    case "Tất cả":
                    default:
                        break;
                }

                int stt = 1;
                foreach (var item in gridList)
                {
                    // Luôn hiển thị ngày cập nhật trạng thái gần nhất từ SQL,
                    // kể cả khi tài khoản đã được mở khóa và trở lại trạng thái Hoạt động.
                    string strNgayCapNhat = item.NgayCapNhatTrangThai.HasValue
                        ? item.NgayCapNhatTrangThai.Value.ToString("dd/MM/yyyy HH:mm:ss")
                        : "";

                    int rowIndex = dgvVay.Rows.Add(
                        stt++,
                        item.SoTKVay,
                        item.CCCD,
                        item.HoTen,
                        item.SoDienThoai,
                        $"{item.SoTienVay:N0} VNĐ",
                        $"{item.TongTienDaThanhToan:N0} VNĐ",
                        $"{item.LaiSuat}% / năm",
                        item.KyHan,
                        item.NgayMo.ToString("dd/MM/yyyy HH:mm:ss"),
                        item.NhomNoTen,
                        strNgayCapNhat,
                        item.VaiTro,
                        GetStatusText(item.TrangThai)
                    );

                    if (item.NhomNoCode >= 3)
                    {
                        dgvVay.Rows[rowIndex].Cells[10].Style.ForeColor = Color.Red;
                        dgvVay.Rows[rowIndex].Cells[10].Style.Font = _fontNhomNoVay;
                    }

                    dgvVay.Rows[rowIndex].Tag = item;
                    var ten = dgvVay.Rows[rowIndex].Cells["colHoTen"];
                    bool laChu = item.VaiTro == "Chủ" || item.VaiTro == "Chủ sở hữu" || item.VaiTro == "Chủ tài khoản";
                    ten.Style.ForeColor = laChu ? Color.FromArgb(96, 165, 250) : Color.FromArgb(192, 132, 252);
                    ten.Style.Padding = new Padding(laChu ? 0 : 16, 0, 0, 0);
                    ten.Style.SelectionForeColor = laChu ? Color.FromArgb(219, 234, 254) : Color.FromArgb(243, 232, 255);
                    if (laChu) ten.Style.Font = _fontNhomNoVay;
                    if (item.RawKhachHang != null)
                        ten.ToolTipText = "Doanh nghiệp: " + item.RawKhachHang.HoTen + " | MST: " + item.RawKhachHang.CCCD + " | STK: " + item.SoTKVay;
                }

                CapNhatThongKe(gridList);
                dgvVay.ClearSelection();
            }
            finally
            {
                _dangDungBangVay = false;
                dgvVay.ResumeLayout();
            }
            UpdateSelectedCustomerInfo();
        }

        private void CapNhatThongKe(List<VayGridRowModel> list)
        {
            var uniqueAccounts = list.GroupBy(x => x.SoTKVay).Select(g => g.First()).ToList();
            int tongSoTK = uniqueAccounts.Count;
            decimal tongSoTienVay = uniqueAccounts.Sum(x => x.SoTienVay);
            decimal tongDaTra = uniqueAccounts.Sum(x => x.TongTienDaThanhToan);

            lblTongSoTK.Text = $"📊 Tổng tài khoản: {tongSoTK}";
            lblTongNo.Text = $"💰 Tổng tiền vay: {tongSoTienVay:N0} VNĐ";
            lblTongDaTra.Text = $"✅ Tổng đã thanh toán (gốc + lãi + phí): {tongDaTra:N0} VNĐ";
        }

        private void DgvVay_SelectionChanged(object sender, EventArgs e)
        {
            if (!_dangDungBangVay) UpdateSelectedCustomerInfo();
        }

        private void UpdateSelectedCustomerInfo()
        {
            var model = GetSelectedRowModel();
            if (model != null)
            {
                lblSelectedCustomerInfo.Text = $"👤 Đang chọn: Họ tên: {model.HoTen} | MST: {model.CCCD} | SĐT: {model.SoDienThoai} | Tổng tiền vay: {model.SoTienVay:N0} VNĐ | Tổng đã thanh toán: {model.TongTienDaThanhToan:N0} VNĐ";
            }
            else
            {
                lblSelectedCustomerInfo.Text = "👤 Đang chọn: Chưa chọn khách hàng";
            }
        }

        private string GetStatusText(FrmMainDN.TrangThaiTK st)
        {
            switch (st)
            {
                case FrmMainDN.TrangThaiTK.HoatDong: return "Hoạt động";
                case FrmMainDN.TrangThaiTK.Khoa: return "Đang khóa";
                case FrmMainDN.TrangThaiTK.Dong: return "Đã đóng";
                default: return "Hoạt động";
            }
        }
        #endregion

        #region Xử lý Menu Chuột Phải
        private void DgvVay_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = dgvVay.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    dgvVay.ClearSelection();
                    dgvVay.Rows[hit.RowIndex].Selected = true;
                    dgvVay.CurrentCell = dgvVay.Rows[hit.RowIndex].Cells[Math.Max(0, hit.ColumnIndex)];

                    VayGridRowModel model = dgvVay.Rows[hit.RowIndex].Tag as VayGridRowModel;
                    if (model != null)
                    {
                        bool isClosed = model.TrangThai == FrmMainDN.TrangThaiTK.Dong;
                        bool isLocked = model.TrangThai == FrmMainDN.TrangThaiTK.Khoa;
                        bool isBadDebt = model.NhomNoCode >= 3;

                        menuMoThemTK.Enabled = !isClosed && !isBadDebt && LaChuTaiKhoanVayDN(model.VaiTro);
                        menuChuyenNhomNo.Enabled = !isClosed;
                        menuThanhToan.Enabled = !isClosed && !isLocked;
                        menuDongTK.Enabled = !isClosed;
                        menuKhoaMoTK.Enabled = !isClosed;

                        menuKhoaMoTK.Text = isLocked ? "🔓 Mở tài khoản" : "🔒 Khóa tạm thời tài khoản";

                        contextMenuStripVay.Show(dgvVay, e.Location);
                    }
                }
            }
        }


        // Nhấp đúp: thông tin và nghiệp vụ trong CÙNG MỘT cửa sổ.
        private void DgvVay_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvVay.Rows.Count) return;
            dgvVay.ClearSelection();
            dgvVay.Rows[e.RowIndex].Selected = true;
            dgvVay.CurrentCell = dgvVay.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
            var model = dgvVay.Rows[e.RowIndex].Tag as VayGridRowModel;
            if (model != null) HienThiChiTietVaChucNangVay(model);
        }

        // Chỉ thay đổi giao diện cửa sổ chi tiết; giữ nguyên các handler nghiệp vụ.
        private void HienThiChiTietVaChucNangVay(VayGridRowModel model)
        {
            // Sau khi giao dịch thành công, dựng lại cửa sổ từ dữ liệu SQL mới
            // mà không yêu cầu người dùng đóng rồi nhấp đúp tài khoản lần nữa.
            string soTaiKhoanDangXem = model.SoTKVay;
            while (model != null)
            {
                bool canTaiChiTiet = false;
                bool daThayDoiDuLieu = false;
                EventHandler theoDoiThayDoi = (s, e) => daThayDoiDuLieu = true;
                DataChanged += theoDoiThayDoi;
                using (Form dlg = new Form())
                {
                    dlg.Text = "Chi tiết & chức năng tài khoản vay - " + model.SoTKVay;
                    dlg.WindowState = FormWindowState.Maximized;
                    dlg.MinimumSize = new Size(1040, 720);
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.BackColor = colorContentBg;
                    dlg.ForeColor = Color.White;
                    dlg.Font = new Font("Segoe UI", 10F);

                    // Bố cục vừa một màn hình, không sử dụng thanh cuộn toàn cửa sổ.
                    var layout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 1,
                        RowCount = 8,
                        Padding = new Padding(24, 12, 24, 12),
                        BackColor = colorContentBg
                    };
                    layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F)); // tiêu đề
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 106F)); // 3 thẻ
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 102F)); // thông tin
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F)); // tiêu đề sở hữu
                    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // bảng sở hữu co giãn
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); // tiêu đề chức năng
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 146F)); // 4 nhóm
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F)); // đóng
                    dlg.Controls.Add(layout);
                    int sectionIndex = 0;
                    Action<Control, int> addSection = (control, bottomGap) => {
                        control.Margin = new Padding(0, 0, 0, Math.Min(bottomGap, 8));
                        layout.Controls.Add(control, 0, sectionIndex++);
                    };

                    var heading = new Label
                    {
                        Text = "CHI TIẾT TÀI KHOẢN VAY  •  " + model.SoTKVay,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(96, 165, 250),
                        Padding = new Padding(0, 0, 0, 6)
                    };
                    addSection(heading, 15);

                    // Ba thẻ tổng quan, tự chia đều chiều ngang.
                    var overview = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        Height = 100,
                        ColumnCount = 3,
                        RowCount = 1,
                        Margin = new Padding(0),
                        BackColor = colorContentBg
                    };
                    for (int i = 0; i < 3; i++) overview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
                    AddVaySummaryCard(overview, 0, "SỐ TIỀN VAY", model.SoTienVay.ToString("N0") + " VNĐ", Color.FromArgb(96, 165, 250));
                    AddVaySummaryCard(overview, 1, "TỔNG ĐÃ TRẢ (GỐC + LÃI + PHÍ)", model.TongTienDaThanhToan.ToString("N0") + " VNĐ", Color.FromArgb(74, 222, 128));
                    AddVaySummaryCard(overview, 2, "DƯ NỢ CÒN LẠI", model.DuNo.ToString("N0") + " VNĐ", Color.FromArgb(251, 191, 36));
                    addSection(overview, 16);

                    var infoPanel = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        Height = 100,
                        ColumnCount = 3,
                        RowCount = 2,
                        Padding = new Padding(12, 10, 12, 8),
                        BackColor = colorCardBg
                    };
                    for (int i = 0; i < 3; i++) infoPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
                    infoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    infoPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    AddVayInfoCell(infoPanel, 0, 0, "Lãi suất", model.LaiSuat.ToString("0.##") + "%/năm");
                    AddVayInfoCell(infoPanel, 1, 0, "Kỳ hạn", model.KyHan);
                    AddVayInfoCell(infoPanel, 2, 0, "Nhóm nợ", model.NhomNoTen);
                    AddVayInfoCell(infoPanel, 0, 1, "Trạng thái", GetStatusText(model.TrangThai));
                    AddVayInfoCell(infoPanel, 1, 1, "Ngày mở", model.NgayMo.ToString("dd/MM/yyyy HH:mm:ss"));
                    AddVayInfoCell(infoPanel, 2, 1, "Ngày cập nhật", model.NgayCapNhatTrangThai.HasValue ? model.NgayCapNhatTrangThai.Value.ToString("dd/MM/yyyy HH:mm:ss") : "Chưa cập nhật");
                    addSection(infoPanel, 24);

                    addSection(new Label
                    {
                        Text = "CHỦ SỞ HỮU / NGƯỜI ĐƯỢC ỦY QUYỀN",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(74, 222, 128)
                    }, 10);

                    var owners = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        Height = 175,
                        ReadOnly = true,
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        RowHeadersVisible = false,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        BackgroundColor = colorCardBg,
                        BorderStyle = BorderStyle.None,
                        EnableHeadersVisualStyles = false,
                        ColumnHeadersHeight = 42,
                        RowTemplate = { Height = 39 },
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        MultiSelect = false,
                        GridColor = Color.FromArgb(51, 65, 85)
                    };
                    owners.Columns.Add("role", "Vai trò"); owners.Columns.Add("name", "Tên doanh nghiệp / Họ tên");
                    owners.Columns.Add("cccd", "MST / CCCD"); owners.Columns.Add("phone", "Số điện thoại");
                    owners.Columns.Add("status", "Trạng thái TK");
                    owners.Columns["role"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    owners.CellFormatting += ToMauVaiTroTrangThaiVay;
                    owners.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 65, 85);
                    owners.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    owners.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    owners.DefaultCellStyle.BackColor = colorCardBg;
                    owners.DefaultCellStyle.ForeColor = Color.White;
                    owners.DefaultCellStyle.SelectionBackColor = colorActiveBlue;
                    owners.DefaultCellStyle.SelectionForeColor = Color.White;
                    owners.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 47, 66);
                    string status = GetStatusText(model.TrangThai);
                    string chuCCCD = model.RawKhachHang?.CCCD ?? model.CCCD;
                    string chuTen = model.RawKhachHang?.HoTen ?? model.HoTen;
                    string chuSDT = model.RawKhachHang?.SoDienThoai ?? model.SoDienThoai;
                    owners.Rows.Add("Chủ tài khoản", chuTen, chuCCCD, chuSDT, status);
                    try
                    {
                        using (var conn = new SqlConnection(_connStr))
                        using (var cmd = new SqlCommand("SELECT CCCD, HoTen, SoDienThoai FROM dbo.DN_DongSoHuuTaiKhoanVay WHERE SoTKVay=@stk AND CCCD<>@cccd ORDER BY CCCD", conn))
                        {
                            cmd.Parameters.AddWithValue("@cccd", chuCCCD);
                            cmd.Parameters.AddWithValue("@stk", model.SoTKVay);
                            conn.Open();
                            using (var rd = cmd.ExecuteReader())
                                while (rd.Read()) owners.Rows.Add("Người được ủy quyền", rd["HoTen"], rd["CCCD"], rd["SoDienThoai"], status);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Không tải được danh sách người được ủy quyền: " + ex.Message); }
                    // Khôi phục đúng người đã chọn từ danh sách chính.
                    owners.ClearSelection();
                    foreach (DataGridViewRow ownerRow in owners.Rows)
                    {
                        if (string.Equals(Convert.ToString(ownerRow.Cells["cccd"].Value), model.CCCD, StringComparison.OrdinalIgnoreCase))
                        { owners.CurrentCell = ownerRow.Cells[0]; ownerRow.Selected = true; break; }
                    }
                    // Màu vai trò và trạng thái do CellFormatting xử lý.
                    addSection(owners, 24);

                    addSection(new Label
                    {
                        Text = "CHỨC NĂNG TÀI KHOẢN VAY",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(74, 222, 128)
                    }, 12);

                    // Giữ nguyên các handler nghiệp vụ; chỉ làm mới cửa sổ
                    // nếu một handler thực sự phát thông báo DataChanged.
                    Action<Action> thucHienVaLamMoi = xuLy =>
                    {
                        ChonLaiTaiKhoanVay(soTaiKhoanDangXem);
                        daThayDoiDuLieu = false;
                        xuLy();
                        if (daThayDoiDuLieu && !dlg.IsDisposed)
                        {
                            canTaiChiTiet = true;
                            dlg.Close();
                        }
                    };

                    bool closed = model.TrangThai == FrmMainDN.TrangThaiTK.Dong;
                    bool locked = model.TrangThai == FrmMainDN.TrangThaiTK.Khoa;
                    var groups = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        Height = 140,
                        ColumnCount = 4,
                        RowCount = 1,
                        BackColor = colorContentBg,
                        Margin = new Padding(0)
                    };
                    for (int i = 0; i < 4; i++) groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                    AddVayGroup(groups, "Thanh toán", 0, new[] {
                    new Tuple<string, Action, bool>("Thanh toán khoản vay", () => thucHienVaLamMoi(() => MenuThanhToan_Click(null, EventArgs.Empty)), !closed && !locked)
                });
                    AddVayGroup(groups, "Trạng thái tài khoản", 1, new[] {
                    new Tuple<string, Action, bool>(locked ? "Mở khóa tài khoản" : "Khóa tạm thời", () => thucHienVaLamMoi(() => MenuKhoaMoTK_Click(null, EventArgs.Empty)), !closed),
                    new Tuple<string, Action, bool>("Đóng tài khoản", () => thucHienVaLamMoi(() => MenuDongTK_Click(null, EventArgs.Empty)), !closed)
                });
                    AddVayGroup(groups, "Quản lý nợ", 2, new[] {
                    // Sử dụng trực tiếp model của cửa sổ chi tiết, không phụ thuộc
                    // vào dòng đang được chọn trên bảng chính.
                    new Tuple<string, Action, bool>("Mở thêm tài khoản vay", () =>
                    {
                        if (owners.CurrentRow == null || !LaChuTaiKhoanVayDN(Convert.ToString(owners.CurrentRow.Cells["role"].Value)) ||
                            !string.Equals(Convert.ToString(owners.CurrentRow.Cells["cccd"].Value), chuCCCD, StringComparison.OrdinalIgnoreCase)) return;
                        var chuModel = dgvVay.Rows.Cast<DataGridViewRow>().Select(r => r.Tag as VayGridRowModel)
                            .FirstOrDefault(m => m != null && m.SoTKVay == soTaiKhoanDangXem && LaChuTaiKhoanVayDN(m.VaiTro) && m.CCCD == chuCCCD);
                        if (chuModel == null) { MessageBox.Show("Không tìm thấy dòng chủ tài khoản. Vui lòng làm mới danh sách."); return; }
                        thucHienVaLamMoi(() => ThucHienMoThemTaiKhoanVay(chuModel));
                    }, !closed && model.NhomNoCode < 3),
                    new Tuple<string, Action, bool>("Chuyển nhóm nợ", () => thucHienVaLamMoi(() => MenuChuyenNhomNo_Click(null, EventArgs.Empty)), !closed),
                    new Tuple<string, Action, bool>("Thay đổi thông tin", () => thucHienVaLamMoi(() =>
                    {
                        var row = owners.CurrentRow;
                        if (row == null) return;
                        SuaThongTinKhachHangVay(model, Convert.ToString(row.Cells["cccd"].Value),
                            Convert.ToString(row.Cells["role"].Value) == "Chủ tài khoản");
                    }), !closed)
                });
                    AddVayGroup(groups, "Lịch sử & báo cáo", 3, new[] {
                    new Tuple<string, Action, bool>("Xem lịch sử giao dịch", () => MoLichSuTuChiTiet(soTaiKhoanDangXem, true), true),
                    new Tuple<string, Action, bool>("Xem lịch sử hoạt động", () => MoLichSuTuChiTiet(soTaiKhoanDangXem, false), true)
                });
                    var debtGroup = groups.GetControlFromPosition(2, 0) as GroupBox;
                    var debtActions = debtGroup == null ? null : debtGroup.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
                    var openMore = debtActions == null ? null : debtActions.GetControlFromPosition(0, 0) as Button;
                    Action capNhatQuyenMo = () =>
                    {
                        if (openMore == null) return;
                        bool chu = owners.CurrentRow != null && LaChuTaiKhoanVayDN(Convert.ToString(owners.CurrentRow.Cells["role"].Value)) &&
                            string.Equals(Convert.ToString(owners.CurrentRow.Cells["cccd"].Value), chuCCCD, StringComparison.OrdinalIgnoreCase);
                        openMore.Enabled = chu && !closed && model.NhomNoCode < 3;
                        openMore.Text = chu ? "Mở thêm tài khoản vay" : "Mở thêm TK (Chỉ chủ tài khoản)";
                    };
                    owners.SelectionChanged += (senderOwner, argsOwner) => capNhatQuyenMo();
                    capNhatQuyenMo();
                    addSection(groups, 20);

                    var close = new Button
                    {
                        Text = "Đóng cửa sổ",
                        Width = 132,
                        Height = 34,
                        BackColor = colorCardBg,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };
                    close.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
                    close.Click += (s, e) => dlg.Close();
                    addSection(close, 0);
                    try { dlg.ShowDialog(this); }
                    finally { DataChanged -= theoDoiThayDoi; }
                }
                if (!canTaiChiTiet) break;

                // RefreshSqlData() đã chạy trong handler sau khi SQL lưu thành công.
                // Tìm đúng tài khoản theo số TK, không dùng chỉ số dòng (có thể thay đổi).
                model = ChonLaiTaiKhoanVay(soTaiKhoanDangXem);
                if (model == null) break;
            }
        }

        // Chuyển tới danh mục Lịch sử; form chi tiết sẽ được đóng bởi FrmMain.
        private void MoLichSuTuChiTiet(string soTaiKhoanVay, bool laGiaoDich)
        {
            if (string.IsNullOrWhiteSpace(soTaiKhoanVay)) return;
            FrmMain main = Application.OpenForms.OfType<FrmMain>()
                .FirstOrDefault(f => !f.IsDisposed && !f.Disposing);
            if (main == null)
            {
                MessageBox.Show("Không tìm thấy FrmMain để chuyển đến Lịch sử.",
                    "Điều hướng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            main.MoLichSuTuTaiKhoan(this, true, "Vay", soTaiKhoanVay, laGiaoDich);
        }

        private void AddVaySummaryCard(TableLayoutPanel parent, int column, string title, string value, Color accent)
        {
            var card = new Panel { Dock = DockStyle.Fill, BackColor = colorCardBg, Margin = new Padding(0, 0, column == 2 ? 0 : 12, 0) };
            card.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(16, 17, 0, 0),
                ForeColor = colorTextMuted,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });
            card.Controls.Add(new Label
            {
                Text = value,
                Dock = DockStyle.Bottom,
                Height = 65,
                Padding = new Padding(16, 5, 8, 0),
                ForeColor = accent,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                AutoEllipsis = true
            });
            parent.Controls.Add(card, column, 0);
        }

        private void AddVayInfoCell(TableLayoutPanel parent, int column, int row, string title, string value)
        {
            parent.Controls.Add(new Label
            {
                Text = title + ":  " + value,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Margin = new Padding(10, 2, 5, 2)
            }, column, row);
        }

        // Chỉnh sửa thông tin khách hàng của khoản vay, đồng bộ theo CCCD.
        // Không thay đổi số tài khoản, số tiền vay, lãi suất, kỳ hạn hoặc trạng thái.
        private void SuaThongTinKhachHangVay(VayGridRowModel model, string maDinhDanh, bool laDoanhNghiep)
        {
            if (model == null || string.IsNullOrWhiteSpace(maDinhDanh)) return;
            string cccd = maDinhDanh.Trim();
            string hoTen = model.HoTen ?? "";
            string soDienThoai = model.SoDienThoai ?? "";

            try
            {
                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"
                    SELECT TOP (1) HoTen, SoDienThoai FROM
                    (SELECT HoTen, SoDienThoai, 0 AS UuTien FROM dbo.DN_KhachHang WHERE CCCD=@cccd
                     UNION ALL
                     SELECT HoTen, SoDienThoai, 1 AS UuTien FROM dbo.DN_DongSoHuu WHERE CCCD=@cccd) x
                    ORDER BY UuTien", cn))
                {
                    cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 20).Value = cccd;
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            MessageBox.Show("Không tìm thấy khách hàng để chỉnh sửa.");
                            return;
                        }
                        hoTen = Convert.ToString(rd["HoTen"]);
                        soDienThoai = Convert.ToString(rd["SoDienThoai"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được thông tin khách hàng:\n" + ex.Message,
                    "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = "Thay đổi thông tin tài khoản vay - " + model.SoTKVay;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ClientSize = new Size(530, 290);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = colorContentBg;
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 10F);

                var lblHeader = new Label
                {
                    Text = laDoanhNghiep ? "THÔNG TIN DOANH NGHIỆP" : "THÔNG TIN NGƯỜI ĐƯỢC ỦY QUYỀN",
                    Location = new Point(22, 17),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(96, 165, 250),
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };
                var lblCCCD = new Label
                {
                    Text = (laDoanhNghiep ? "MST: " : "CCCD: ") + cccd + "    |    Số TK vay: " + model.SoTKVay,
                    Location = new Point(22, 52),
                    AutoSize = true,
                    ForeColor = colorTextMuted
                };
                var lblTen = new Label { Text = laDoanhNghiep ? "Tên doanh nghiệp:" : "Họ và tên:", Location = new Point(22, 99), AutoSize = true };
                var txtTen = new TextBox
                {
                    Location = new Point(172, 95),
                    Size = new Size(325, 28),
                    Text = hoTen,
                    MaxLength = 100,
                    BackColor = colorCardBg,
                    ForeColor = Color.White
                };
                var lblSDT = new Label { Text = "Số điện thoại:", Location = new Point(22, 145), AutoSize = true };
                var txtSDT = new TextBox
                {
                    Location = new Point(172, 141),
                    Size = new Size(325, 28),
                    Text = soDienThoai,
                    MaxLength = 11,
                    BackColor = colorCardBg,
                    ForeColor = Color.White
                };
                lblSDT.Visible = txtSDT.Visible = !laDoanhNghiep;
                txtSDT.KeyPress += (sender, e) =>
                {
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
                };
                var lblNote = new Label
                {
                    Text = laDoanhNghiep ? "Thông tin được đồng bộ theo MST trên các tài khoản liên quan."
                        : "Thông tin được đồng bộ theo CCCD trên các tài khoản liên quan.",
                    Location = new Point(22, 186),
                    AutoSize = true,
                    ForeColor = colorTextMuted,
                    Font = new Font("Segoe UI", 8.5F)
                };
                var btnLuu = new Button
                {
                    Text = "Lưu thay đổi",
                    Location = new Point(236, 227),
                    Size = new Size(140, 36),
                    BackColor = colorActiveBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(391, 227),
                    Size = new Size(105, 36),
                    BackColor = Color.FromArgb(51, 65, 85),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnLuu.FlatAppearance.BorderSize = 0;
                btnHuy.FlatAppearance.BorderSize = 0;
                btnHuy.Click += (sender, e) => dlg.Close();
                btnLuu.Click += (sender, e) =>
                {
                    string tenMoi = txtTen.Text.Trim();
                    string sdtMoi = txtSDT.Text.Trim();
                    if (string.IsNullOrWhiteSpace(tenMoi))
                    {
                        MessageBox.Show("Vui lòng nhập họ tên khách hàng."); return;
                    }
                    if (!laDoanhNghiep && ((sdtMoi.Length != 10 && sdtMoi.Length != 11) || !sdtMoi.All(char.IsDigit)))
                    {
                        MessageBox.Show("Số điện thoại phải gồm 10 hoặc 11 chữ số."); return;
                    }
                    try
                    {
                        using (var cn = new SqlConnection(_connStr))
                        {
                            cn.Open();
                            using (var tran = cn.BeginTransaction())
                            {
                                try
                                {
                                    using (var cmd = new SqlCommand(laDoanhNghiep ? @"
                                        UPDATE dbo.DN_KhachHang SET HoTen=@ten WHERE CCCD=@cccd;
                                        IF @@ROWCOUNT=0 THROW 50001, N'Không tìm thấy doanh nghiệp.', 1;" : @"
                                        UPDATE dbo.DN_KhachHang SET HoTen=@ten, SoDienThoai=@sdt WHERE CCCD=@cccd;
                                        UPDATE dbo.DN_DongSoHuu SET HoTen=@ten, SoDienThoai=@sdt WHERE CCCD=@cccd;
                                        IF OBJECT_ID(N'dbo.DN_DongSoHuuTaiKhoanThanhToan', N'U') IS NOT NULL
                                        BEGIN
                                            EXEC sys.sp_executesql N'UPDATE dbo.DN_DongSoHuuTaiKhoanThanhToan
                                                SET HoTen=@t, SoDienThoai=@p WHERE CCCD=@c',
                                                N'@t nvarchar(100), @p varchar(11), @c varchar(20)',
                                                @t=@ten, @p=@sdt, @c=@cccd;
                                        END
                                        IF NOT EXISTS(SELECT 1 FROM dbo.DN_KhachHang WHERE CCCD=@cccd)
                                           AND NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD=@cccd)
                                            THROW 50001, N'Không tìm thấy CCCD trong dữ liệu khách hàng.', 1;", cn, tran))
                                    {
                                        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = tenMoi;
                                        cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 11).Value = sdtMoi;
                                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 20).Value = cccd;
                                        cmd.ExecuteNonQuery();
                                    }
                                    tran.Commit();
                                }
                                catch { tran.Rollback(); throw; }
                            }
                        }
                        MessageBox.Show("Đã cập nhật và đồng bộ thông tin khách hàng!",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể cập nhật thông tin:\n" + ex.Message,
                            "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                dlg.Controls.AddRange(new Control[] { lblHeader, lblCCCD, lblTen, txtTen,
                    lblSDT, txtSDT, lblNote, btnLuu, btnHuy });
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshSqlData();
                    OnDataChanged();
                }
            }
        }

        private void AddVayGroup(TableLayoutPanel parent, string title, int column, Tuple<string, Action, bool>[] items)
        {
            var group = new GroupBox
            {
                Text = title,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, column == 3 ? 0 : 12, 0),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = colorContentBg,
                Padding = new Padding(10, 18, 10, 8)
            };

            // Riêng nhóm Quản lý nợ: nút mở tài khoản chiếm toàn hàng đầu;
            // Chuyển nhóm nợ và Thay đổi thông tin chia đôi hàng thứ hai.
            // Giữ nguyên Action, trạng thái Enabled và logic nghiệp vụ.
            if (title == "Quản lý nợ" && items.Length == 3)
            {
                var gridActions = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = colorContentBg,
                    ColumnCount = 2,
                    RowCount = 2,
                    Margin = Padding.Empty,
                    Padding = new Padding(0, 2, 0, 2)
                };
                gridActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                gridActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                gridActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                gridActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                for (int i = 0; i < items.Length; i++)
                {
                    var spec = items[i];
                    var action = new Button
                    {
                        Text = spec.Item1 + (spec.Item3 ? "" : " (Không khả dụng)"),
                        Dock = DockStyle.Fill,
                        Margin = i == 1 ? new Padding(2, 3, 4, 3)
                              : i == 2 ? new Padding(4, 3, 2, 3)
                              : new Padding(2, 3, 2, 3),
                        TextAlign = ContentAlignment.MiddleCenter,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(35, 50, 74),
                        ForeColor = spec.Item3 ? Color.White : Color.FromArgb(239, 68, 68),
                        Enabled = spec.Item3,
                        Cursor = spec.Item3 ? Cursors.Hand : Cursors.Default,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                        AutoEllipsis = true
                    };
                    action.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
                    action.Click += (sender, args) => spec.Item2();
                    if (i == 0)
                    {
                        gridActions.Controls.Add(action, 0, 0);
                        gridActions.SetColumnSpan(action, 2);
                    }
                    else gridActions.Controls.Add(action, i - 1, 1);
                }
                group.Controls.Add(gridActions);
                parent.Controls.Add(group, column, 0);
                return;
            }

            // Panel thường thay FlowLayoutPanel: tránh tình trạng các nút bị
            // đẩy ra ngoài vùng hiển thị khi cửa sổ được phóng tối đa.
            var surface = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = colorContentBg,
                Margin = new Padding(0),
                Padding = new Padding(0),
                AutoScroll = false
            };
            var actionButtons = new List<Button>();
            for (int i = 0; i < items.Length; i++)
            {
                var spec = items[i];
                var button = new Button
                {
                    Text = spec.Item1 + (spec.Item3 ? "" : " (Không khả dụng)"),
                    Height = 36,
                    Width = 180,
                    Location = new Point(2, 4 + i * 43),
                    TextAlign = ContentAlignment.MiddleCenter,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(35, 50, 74),
                    ForeColor = spec.Item3 ? Color.White : Color.FromArgb(239, 68, 68),
                    Enabled = spec.Item3,
                    Cursor = spec.Item3 ? Cursors.Hand : Cursors.Default,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                button.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
                button.Click += (s, e) => spec.Item2();
                surface.Controls.Add(button);
                actionButtons.Add(button);
            }
            Action resizeButtons = () => {
                int width = Math.Max(80, surface.ClientSize.Width - 6);
                for (int i = 0; i < actionButtons.Count; i++)
                {
                    actionButtons[i].Width = width;
                    actionButtons[i].Location = new Point(2, 4 + i * 43);
                    actionButtons[i].BringToFront();
                }
            };
            surface.Resize += (s, e) => resizeButtons();
            group.Controls.Add(surface);
            parent.Controls.Add(group, column, 0);
            resizeButtons();
        }

        private VayGridRowModel GetSelectedRowModel()
        {
            if (dgvVay.SelectedRows.Count > 0)
            {
                return dgvVay.SelectedRows[0].Tag as VayGridRowModel;
            }
            return null;
        }

        // 1. THANH TOÁN TIỀN
        private void MenuThanhToan_Click(object sender, EventArgs e)
        {
            var model = GetSelectedRowModel();
            if (model == null) return;

            Form dlgThanhToan = CreateFormDynamicDesign32($"Thanh toán Khoản Vay - Số TK: {model.SoTKVay}");

            int tongSoThang = ParseKyHanToMonths(model.KyHan);
            int soThangDaQua = 1;
            while (soThangDaQua < tongSoThang && model.NgayMo.AddMonths(soThangDaQua) < DateTime.Now.Date)
                soThangDaQua++;
            bool isQuathang = DateTime.Today > model.NgayMo.Date.AddMonths(tongSoThang);

            decimal duNoHienTai = model.DuNo;
            double laiSuatThang = model.LaiSuat / 12 / 100;
            decimal tienLaiThang = (decimal)((double)duNoHienTai * laiSuatThang);
            decimal phiPhatQuaHan = isQuathang ? (duNoHienTai * 0.02m) : 0m;
            decimal phiPhatTraHetHoacTruoc = model.SoTienVay * 0.01m;

            GroupBox gbBangTính = CreateGroupBoxDesign25();

            Label lblDuNo = CreateLabelDynamicDesign33($"Dư nợ gốc còn lại: {duNoHienTai:N0} VNĐ");
            Label lblTienLai = CreateLabelDynamicDesign34($"Tiền lãi kỳ này ({model.LaiSuat}%/năm): {tienLaiThang:N0} VNĐ");
            Label lblPhiPhat = CreateLabelDynamicDesign35($"Phí phạt quá hạn kỳ này: {phiPhatQuaHan:N0} VNĐ {(isQuathang ? "(Đã quá kỳ hạn)" : "")}", isQuathang ? Color.Red : Color.Black);
            Label lblTongOwed = CreateLabelDynamicDesign36($"TỔNG LÃI + PHÍ NỢ KỲ NÀY: {(tienLaiThang + phiPhatQuaHan):N0} VNĐ");
            gbBangTính.Controls.AddRange(new Control[] { lblDuNo, lblTienLai, lblPhiPhat, lblTongOwed });

            GroupBox gbPhuongThuc = CreateGroupBoxDesign26();

            RadioButton rdoTraHet = CreateRadioButtonDynamicDesign37($"Trả hết (Gốc + Lãi + Phí phạt quá hạn + Phí 1% tổng tiền vay: {phiPhatTraHetHoacTruoc:N0} VNĐ)");
            RadioButton rdoTraTheoKy = CreateRadioButtonDesign27();
            RadioButton rdoTraTruoc = CreateRadioButtonDynamicDesign38($"Trả trước (Nhập tiền gốc + Phí 1% tổng tiền vay: {phiPhatTraHetHoacTruoc:N0} VNĐ)");

            TextBox txtSoTienTraTruoc = CreateTextBoxDesign28();
            txtSoTienTraTruoc.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);

            gbPhuongThuc.Controls.AddRange(new Control[] { rdoTraHet, rdoTraTheoKy, rdoTraTruoc, txtSoTienTraTruoc });

            Label lblTongPay = CreateLabelDesign29();

            Action TinhTienPay = () =>
            {
                decimal gocTra = 0;
                decimal phiPhatPhatSinh = phiPhatQuaHan;

                if (rdoTraHet.Checked)
                {
                    txtSoTienTraTruoc.Enabled = false;
                    gocTra = duNoHienTai;
                    phiPhatPhatSinh += phiPhatTraHetHoacTruoc;
                }
                else if (rdoTraTheoKy.Checked)
                {
                    txtSoTienTraTruoc.Enabled = false;
                    gocTra = TinhGocPhaiTraTheoKy(model.SoTienVay, model.SoTienGocDaTra, tongSoThang, soThangDaQua);
                }
                else if (rdoTraTruoc.Checked)
                {
                    txtSoTienTraTruoc.Enabled = true;
                    decimal.TryParse(txtSoTienTraTruoc.Text, out gocTra);
                    if (gocTra > duNoHienTai) gocTra = duNoHienTai;
                    phiPhatPhatSinh += phiPhatTraHetHoacTruoc;
                }

                decimal tongThanhToan = gocTra + tienLaiThang + phiPhatPhatSinh;
                lblTongPay.Text = $"Tổng số tiền cần thanh toán: {tongThanhToan:N0} VNĐ\n(Gốc: {gocTra:N0} | Lãi: {tienLaiThang:N0} | Phí phạt: {phiPhatPhatSinh:N0})";
            };

            rdoTraHet.CheckedChanged += (s, ev) => TinhTienPay();
            rdoTraTheoKy.CheckedChanged += (s, ev) => TinhTienPay();
            rdoTraTruoc.CheckedChanged += (s, ev) => TinhTienPay();
            txtSoTienTraTruoc.TextChanged += (s, ev) => TinhTienPay();
            TinhTienPay();

            Button btnXacNhan = CreateButtonDesign30();
            Button btnHuy = CreateButtonDesign31();

            btnXacNhan.Click += (s, ev) =>
            {
                decimal gocTra = 0;
                decimal phiPhatPhatSinh = phiPhatQuaHan;
                string ptStr = "";

                if (rdoTraHet.Checked)
                {
                    gocTra = duNoHienTai;
                    phiPhatPhatSinh += phiPhatTraHetHoacTruoc;
                    ptStr = "Trả hết (Phí 1%)";
                }
                else if (rdoTraTheoKy.Checked)
                {
                    gocTra = TinhGocPhaiTraTheoKy(model.SoTienVay, model.SoTienGocDaTra, tongSoThang, soThangDaQua);
                    ptStr = "Trả theo kỳ";
                }
                else if (rdoTraTruoc.Checked)
                {
                    decimal.TryParse(txtSoTienTraTruoc.Text, out gocTra);
                    phiPhatPhatSinh += phiPhatTraHetHoacTruoc;
                    ptStr = "Trả trước (Phí 1%)";
                    if (gocTra <= 0 || gocTra > duNoHienTai)
                    {
                        MessageBox.Show("Số tiền trả trước không hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (gocTra <= 0 && rdoTraTheoKy.Checked)
                {
                    MessageBox.Show("Tiền gốc đến hạn của kỳ này đã được thanh toán. Không thể ghi nhận trùng tiền gốc.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (gocTra <= 0) { MessageBox.Show("Vui lòng chọn phương thức thanh toán hợp lệ."); return; }
                decimal tongThanhToanMoi = gocTra + tienLaiThang + phiPhatPhatSinh;
                long idGiaoDichMoi = 0;

                if (!TrySqlAction(() =>
                {
                    using (var conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (var tx = conn.BeginTransaction())
                        {
                            // Đọc lại dư nợ ngay trong giao dịch để tránh dùng số liệu cũ trên giao diện.
                            decimal gocDaTraTrongSql;
                            decimal tongVayTrongSql;
                            using (var check = new SqlCommand("SELECT SoTienVay, SoTienDaTra, LaiSuat, KyHan, NgayMo FROM dbo.DN_TaiKhoanVay WITH (UPDLOCK, HOLDLOCK) WHERE SoTKVay=@stk AND TrangThai=0", conn, tx))
                            {
                                check.Parameters.Add("@stk", SqlDbType.VarChar, 30).Value = model.SoTKVay;
                                using (var reader = check.ExecuteReader())
                                {
                                    if (!reader.Read()) throw new InvalidOperationException("Tài khoản vay không còn hoạt động.");
                                    tongVayTrongSql = Convert.ToDecimal(reader["SoTienVay"]);
                                    gocDaTraTrongSql = Convert.ToDecimal(reader["SoTienDaTra"]);
                                    if (tongVayTrongSql != model.SoTienVay || gocDaTraTrongSql != model.SoTienGocDaTra ||
                                        Convert.ToDouble(reader["LaiSuat"]) != model.LaiSuat ||
                                        Convert.ToString(reader["KyHan"]) != model.KyHan ||
                                        Convert.ToDateTime(reader["NgayMo"]) != model.NgayMo)
                                        throw new InvalidOperationException("Thông tin khoản vay đã thay đổi. Hãy đóng hộp thoại và làm mới dữ liệu trước khi thanh toán.");
                                }
                            }
                            decimal duNoMoi = Math.Max(0m, tongVayTrongSql - gocDaTraTrongSql);
                            if (rdoTraTheoKy.Checked)
                                gocTra = TinhGocPhaiTraTheoKy(tongVayTrongSql, gocDaTraTrongSql, tongSoThang, soThangDaQua);
                            else if (rdoTraHet.Checked) gocTra = duNoMoi;
                            if (gocTra <= 0 || gocTra > duNoMoi)
                                throw new InvalidOperationException("Gốc cần thanh toán đã thay đổi hoặc kỳ này đã trả đủ. Hãy làm mới dữ liệu.");
                            tongThanhToanMoi = gocTra + tienLaiThang + phiPhatPhatSinh;
                            using (var cmd = new SqlCommand("UPDATE dbo.DN_TaiKhoanVay SET SoTienDaTra = SoTienDaTra + @goc WHERE SoTKVay = @stk AND TrangThai = 0 AND SoTienVay - SoTienDaTra >= @goc", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@goc", gocTra);
                                cmd.Parameters.AddWithValue("@stk", model.SoTKVay);
                                if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Tài khoản không hợp lệ hoặc số dư nợ đã thay đổi.");
                            }
                            using (var cmd = new SqlCommand("INSERT INTO dbo.DN_LichSuGiaoDichVay (SoTKVay, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, PhiPhat, TongTien, SoDuConLai, GhiChu, NguoiThucHien) OUTPUT INSERTED.Id VALUES (@stk,GETDATE(),@loai,@goc,@lai,@phi,@tong,@con,@note,@nguoi)", conn, tx))
                            {
                                cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                cmd.Parameters.AddWithValue("@stk", model.SoTKVay);
                                cmd.Parameters.AddWithValue("@loai", ptStr);
                                cmd.Parameters.AddWithValue("@goc", gocTra);
                                cmd.Parameters.AddWithValue("@lai", tienLaiThang);
                                cmd.Parameters.AddWithValue("@phi", phiPhatPhatSinh);
                                cmd.Parameters.AddWithValue("@tong", tongThanhToanMoi);
                                cmd.Parameters.AddWithValue("@con", Math.Max(0, duNoMoi - gocTra));
                                cmd.Parameters.AddWithValue("@note", "Phí phạt: " + phiPhatPhatSinh.ToString("N0") + " VNĐ");
                                idGiaoDichMoi = Convert.ToInt64(cmd.ExecuteScalar());
                            }
                            tx.Commit();
                        }
                    }
                })) return;
                RefreshSqlData();

                if (MessageBox.Show("Thanh toán tiền vay thành công!\nBạn có muốn xuất hóa đơn giao dịch vừa thực hiện không?",
                    "Thanh toán thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    XuatHoaDonThanhToanVay(model, idGiaoDichMoi);
                dlgThanhToan.DialogResult = DialogResult.OK;
                OnDataChanged();
            };

            btnHuy.Click += (s, ev) => dlgThanhToan.Close();

            dlgThanhToan.Controls.AddRange(new Control[] { gbBangTính, gbPhuongThuc, lblTongPay, btnXacNhan, btnHuy });

            DongBoMauFormPhu(dlgThanhToan);
            if (dlgThanhToan.ShowDialog() == DialogResult.OK)
            {
                HienThiDanhSachVay(false);
            }
        }

        // Gốc trả đều theo từng kỳ: lấy số tiền vay ban đầu chia cho tổng số kỳ.
        // Không thu dồn toàn bộ các kỳ trước vào một giao dịch trả theo kỳ.
        // Kỳ cuối chỉ thu phần dư nợ gốc còn lại để tránh vượt tổng số tiền vay.
        private static decimal TinhGocPhaiTraTheoKy(decimal tongVay, decimal gocDaTra, int tongKy, int kyHienTai)
        {
            tongKy = Math.Max(1, tongKy);
            kyHienTai = Math.Max(1, Math.Min(tongKy, kyHienTai));
            decimal duNoConLai = Math.Max(0m, tongVay - gocDaTra);
            decimal gocMotKy = Math.Round(tongVay / tongKy, 0, MidpointRounding.AwayFromZero);
            return Math.Min(duNoConLai, gocMotKy);
        }

        // Hóa đơn của giao dịch vừa thực hiện: tạo HTML để xem trên trình duyệt và in ra PDF.
        private void XuatHoaDonThanhToanVay(VayGridRowModel model, long idGiaoDich)
        {
            try
            {
                DateTime ngay;
                string loai, ghiChu;
                decimal goc, lai, phi, tong, duCon;
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"SELECT NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, PhiPhat, TongTien, SoDuConLai, GhiChu
                    FROM dbo.DN_LichSuGiaoDichVay WHERE Id=@id AND SoTKVay=@stk", conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = idGiaoDich;
                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 30).Value = model.SoTKVay;
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) throw new InvalidOperationException("Không tìm thấy giao dịch vừa thanh toán.");
                        ngay = Convert.ToDateTime(r["NgayGiaoDich"]);
                        loai = Convert.ToString(r["LoaiGiaoDich"]);
                        goc = Convert.ToDecimal(r["SoTienGoc"]);
                        lai = Convert.ToDecimal(r["SoTienLai"]);
                        phi = Convert.ToDecimal(r["PhiPhat"]);
                        tong = Convert.ToDecimal(r["TongTien"]);
                        duCon = r["SoDuConLai"] == DBNull.Value ? 0 : Convert.ToDecimal(r["SoDuConLai"]);
                        ghiChu = Convert.ToString(r["GhiChu"]);
                    }
                }

                Func<string, string> enc = value => WebUtility.HtmlEncode(value ?? "");
                Func<decimal, string> money = value => value.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ";
                Func<string, string, string> detail = (label, value) =>
                    "<div class='detail'><span>" + enc(label) + "</span><strong>" + enc(value) + "</strong></div>";
                var body = new StringBuilder();
                body.Append("<section><h2>THÔNG TIN KHÁCH HÀNG</h2>");
                body.Append(detail("Họ và tên", model.HoTen));
                body.Append(detail("CCCD", model.CCCD));
                body.Append(detail("Số điện thoại", model.SoDienThoai));
                body.Append("</section><section><h2>THÔNG TIN KHOẢN VAY</h2>");
                body.Append(detail("Số tài khoản vay", model.SoTKVay));
                body.Append(detail("Loại giao dịch", loai));
                body.Append(detail("Kỳ hạn", model.KyHan));
                body.Append(detail("Lãi suất", model.LaiSuat.ToString("0.##", CultureInfo.InvariantCulture) + "%/năm"));
                body.Append("</section><section><h2>CHI TIẾT GIAO DỊCH VỪA THỰC HIỆN</h2>");
                body.Append(detail("Tiền gốc thanh toán", money(goc)));
                body.Append(detail("Tiền lãi thanh toán", money(lai)));
                body.Append(detail("Phí phát sinh", money(phi)));
                body.Append("<div class='total'><span>TỔNG THANH TOÁN</span><strong>" + enc(money(tong)) + "</strong></div>");
                body.Append(detail("Dư nợ gốc còn lại", money(duCon)));
                if (!string.IsNullOrWhiteSpace(ghiChu))
                    body.Append("<p class='note'><b>Ghi chú:</b> " + enc(ghiChu) + "</p>");
                body.Append("</section>");

                string html = @"<!DOCTYPE html>
<html lang='vi'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>
<title>Hóa đơn thanh toán khoản vay</title>
<style>
*{box-sizing:border-box;-webkit-print-color-adjust:exact;print-color-adjust:exact}
body{margin:0;background:#e8eef6;color:#0f172a;font-family:'Segoe UI',Arial,sans-serif;font-size:13px}
.toolbar{max-width:850px;margin:18px auto;display:flex;justify-content:space-between;align-items:center;gap:12px}
.toolbar button{background:#2563eb;color:white;border:0;border-radius:8px;padding:11px 18px;cursor:pointer;font-weight:700}
.paper{width:210mm;min-height:297mm;margin:0 auto 28px;padding:19mm 17mm;background:#fff;box-shadow:0 8px 28px #0002}
.bank{text-align:center;color:#2563eb;font-size:16px;font-weight:700;letter-spacing:1px}
h1{text-align:center;font-size:22px;margin:10px 0 18px;padding-bottom:14px;border-bottom:3px solid #2563eb}
.meta{display:flex;justify-content:space-between;color:#475569;margin-bottom:22px}
section{margin:16px 0 20px;break-inside:avoid}
h2{font-size:13px;background:#eff6ff;color:#2563eb;border:1px solid #cbd5e1;padding:11px 13px;margin:0 0 8px}
.detail{display:flex;justify-content:space-between;gap:20px;padding:9px 12px;border-bottom:1px solid #e2e8f0}
.detail span{color:#475569}.detail strong{text-align:right;overflow-wrap:anywhere}
.total{display:flex;justify-content:space-between;gap:20px;background:#2563eb;color:white;padding:15px 13px;margin:12px 0 8px;font-weight:700;font-size:15px}
.note{padding:6px 12px;color:#475569;overflow-wrap:anywhere}
.signatures{display:flex;justify-content:space-between;text-align:center;margin-top:35px;gap:25px}
.signature{width:46%;font-weight:700}.signature small{display:block;color:#64748b;font-weight:400;margin-top:7px}
.signature .name{margin-top:75px}
@page{size:A4 portrait;margin:0}
@media print{body{background:white}.toolbar{display:none}.paper{box-shadow:none;margin:0;width:210mm;min-height:297mm}}
</style></head><body>
<div class='toolbar'><span>Xem trước hóa đơn · Nhấn Ctrl + P để lưu PDF</span><button onclick='window.print()'>In / Lưu PDF</button></div>
<main class='paper'><div class='bank'>NGÂN HÀNG QLTKNH</div>
<h1>HÓA ĐƠN THANH TOÁN KHOẢN VAY</h1>
<div class='meta'><span>Mã giao dịch: VAY-" + idGiaoDich + @"</span><span>" + ngay.ToString("dd/MM/yyyy HH:mm:ss") + @"</span></div>" + body + @"
<div class='signatures'><div class='signature'>KHÁCH HÀNG<small>(Ký, ghi rõ họ tên)</small><div class='name'>" + enc(model.HoTen) + @"</div></div>
<div class='signature'>GIAO DỊCH VIÊN<small>(Ký và xác nhận)</small><div class='name'>HỆ THỐNG QLTKNH</div></div></div>
</main></body></html>";

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Lưu hóa đơn HTML để xem trên web và xuất PDF";
                    sfd.Filter = "Trang web HTML (*.html)|*.html";
                    sfd.DefaultExt = "html";
                    sfd.AddExtension = true;
                    sfd.FileName = "HoaDonThanhToanVay_" + model.SoTKVay + "_" + idGiaoDich + ".html";
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllText(sfd.FileName, html, new UTF8Encoding(false));
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName)
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất hóa đơn HTML: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 2. ĐÓNG TÀI KHOẢN
        private void MenuDongTK_Click(object sender, EventArgs e)
        {
            var model = GetSelectedRowModel();
            if (model == null) return;

            if (model.DuNo > 0)
            {
                MessageBox.Show($"Tài khoản này chưa thanh toán hết nợ gốc!\nDư nợ gốc còn lại: {model.DuNo:N0} VNĐ.\nBạn phải thanh toán hết nợ trước khi đóng tài khoản.", "Cảnh báo không thể đóng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogRes = MessageBox.Show($"Bạn có chắc chắn muốn ĐÓNG tài khoản vay số: {model.SoTKVay}?\n(Thao tác này xác nhận khách hàng không còn vay thêm khoản này).", "Xác nhận đóng tài khoản", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogRes == DialogResult.Yes)
            {
                if (!TrySqlAction(() =>
                {
                    string nguoi = LayTenNguoiThucHien();
                    using (var conn = new SqlConnection(_connStr))
                    {
                        conn.Open();
                        using (var tx = conn.BeginTransaction())
                        {
                            using (var cmd = new SqlCommand(@"UPDATE dbo.DN_TaiKhoanVay SET TrangThai=2, NgayCapNhatTrangThai=GETDATE()
                                WHERE SoTKVay=@stk AND TrangThai<>2 AND SoTienVay<=SoTienDaTra", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@stk", model.SoTKVay);
                                if (cmd.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("Không tìm thấy tài khoản hợp lệ để đóng.");
                            }
                            using (var cmd = new SqlCommand(@"INSERT INTO dbo.DN_LichSuGiaoDichVay
                                (SoTKVay, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, PhiPhat, TongTien, SoDuConLai, GhiChu, NguoiThucHien)
                                VALUES (@stk, GETDATE(), N'Đóng tài khoản vay', 0, 0, 0, 0, 0, N'Đóng tài khoản vay sau khi trả hết gốc', @nguoi)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@stk", model.SoTKVay);
                                cmd.Parameters.AddWithValue("@nguoi", nguoi);
                                cmd.ExecuteNonQuery();
                            }
                            tx.Commit();
                        }
                    }
                })) return;
                RefreshSqlData();
                MessageBox.Show("Đã đóng tài khoản vay thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThiDanhSachVay(false);
                OnDataChanged();
            }
        }

        // 3. KHÓA TẠM THỜI / MỞ TÀI KHOẢN
        private void MenuKhoaMoTK_Click(object sender, EventArgs e)
        {
            var model = GetSelectedRowModel();
            if (model == null) return;

            bool isCurrentlyLocked = model.TrangThai == FrmMainDN.TrangThaiTK.Khoa;
            string actionName = isCurrentlyLocked ? "MỞ TÀI KHOẢN" : "KHÓA TẠM THỜI";

            using (Form dlgReason = new Form())
            {
                dlgReason.Text = $"{actionName} - Số TK: {model.SoTKVay}";
                dlgReason.Size = new Size(400, 220);
                dlgReason.StartPosition = FormStartPosition.CenterParent;
                dlgReason.FormBorderStyle = FormBorderStyle.FixedDialog;

                Label lblPrompt = CreateLabelDynamicDesign39($"Nhập lý do {actionName.ToLower()}:");
                TextBox txtReason = CreateTextBoxDesign32();
                Button btnOk = CreateButtonDesign33();

                btnOk.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtReason.Text))
                    {
                        MessageBox.Show("Vui lòng nhập lý do!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!TrySqlAction(() =>
                    {
                        string nguoi = LayTenNguoiThucHien();
                        using (var cn = new SqlConnection(_connStr))
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction())
                            {
                                using (var cmd = new SqlCommand(@"
                                    UPDATE dbo.DN_TaiKhoanVay
                                    SET TrangThai=@tt, NgayCapNhatTrangThai=GETDATE()
                                    WHERE SoTKVay=@stk AND TrangThai=@ttCu;
                                    IF @@ROWCOUNT <> 1
                                        THROW 51001, N'Không tìm thấy tài khoản vay để cập nhật.', 1;
                                    INSERT INTO dbo.DN_LichSuGiaoDichVay
                                        (SoTKVay, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai,
                                         PhiPhat, TongTien, SoDuConLai, GhiChu, NguoiThucHien)
                                    SELECT SoTKVay, NgayCapNhatTrangThai, @loai, 0, 0, 0, 0,
                                           CASE WHEN SoTienVay > SoTienDaTra THEN SoTienVay-SoTienDaTra ELSE 0 END,
                                           @note, @nguoi
                                    FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk;", cn, tx))
                                {
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = model.SoTKVay;
                                    cmd.Parameters.Add("@tt", SqlDbType.TinyInt).Value = isCurrentlyLocked ? 0 : 1;
                                    cmd.Parameters.Add("@ttCu", SqlDbType.TinyInt).Value = isCurrentlyLocked ? 1 : 0;
                                    cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 50).Value = isCurrentlyLocked ? "Mở khóa tài khoản" : "Khóa tài khoản";
                                    string note = "Lý do: " + txtReason.Text.Trim();
                                    if (note.Length > 500) throw new InvalidOperationException("Lý do quá dài (tối đa 493 ký tự).");
                                    cmd.Parameters.Add("@note", SqlDbType.NVarChar, 500).Value = note;
                                    cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = nguoi;
                                    cmd.ExecuteNonQuery();
                                }
                                tx.Commit();
                            }
                        }
                    })) return;
                    RefreshSqlData();

                    MessageBox.Show($"Thực hiện {actionName} thành công.\nLý do: {txtReason.Text}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlgReason.DialogResult = DialogResult.OK;
                    OnDataChanged();
                };

                dlgReason.Controls.AddRange(new Control[] { lblPrompt, txtReason, btnOk });
                DongBoMauFormPhu(dlgReason);
                if (dlgReason.ShowDialog() == DialogResult.OK)
                {
                    HienThiDanhSachVay(false);
                }
            }
        }

        // 4. XEM LỊCH SỬ ĐÓNG TIỀN
        private void MenuLichSu_Click(object sender, EventArgs e)
        {
            var model = GetSelectedRowModel();
            if (model == null) return;

            Form dlgLichSu = CreateFormDynamicDesign40($"Lịch sử đóng tiền - Số TK Vay: {model.SoTKVay} ({model.HoTen})");

            DataGridView dgvHistory = CreateDataGridViewDesign34();

            dgvHistory.Columns.Add("colNgay", "Ngày Giao Dịch");
            dgvHistory.Columns.Add("colPT", "Phương Thức");
            dgvHistory.Columns.Add("colGoc", "Tiền Gốc Trả");
            dgvHistory.Columns.Add("colLai", "Tiền Lãi");
            dgvHistory.Columns.Add("colPhat", "Phí Phạt");
            dgvHistory.Columns.Add("colTong", "Tổng Tiền Đóng");
            dgvHistory.Columns.Add("colGhiChu", "Ghi Chú / Dư Nợ Còn");

            try
            {
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand("SELECT NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, PhiPhat, TongTien, GhiChu, SoDuConLai, NguoiThucHien FROM dbo.DN_LichSuGiaoDichVay WHERE SoTKVay=@stk ORDER BY NgayGiaoDich DESC, Id DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@stk", model.SoTKVay);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            string note = Convert.ToString(r["GhiChu"]);
                            if (r["SoDuConLai"] != DBNull.Value) note += " | Dư nợ: " + Convert.ToDecimal(r["SoDuConLai"]).ToString("N0");
                            if (r["NguoiThucHien"] != DBNull.Value) note += " | " + Convert.ToString(r["NguoiThucHien"]);
                            dgvHistory.Rows.Add(Convert.ToDateTime(r["NgayGiaoDich"]).ToString("dd/MM/yyyy HH:mm:ss"),
                                Convert.ToString(r["LoaiGiaoDich"]), Convert.ToDecimal(r["SoTienGoc"]).ToString("N0") + " VNĐ",
                                Convert.ToDecimal(r["SoTienLai"]).ToString("N0") + " VNĐ",
                                Convert.ToDecimal(r["PhiPhat"]).ToString("N0") + " VNĐ",
                                Convert.ToDecimal(r["TongTien"]).ToString("N0") + " VNĐ", note);
                        }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải lịch sử SQL: " + ex.Message); }

            Panel pnlExport = CreatePanelDesign35();
            Button btnXuatPDF = CreateButtonDesign36();
            btnXuatPDF.FlatAppearance.BorderSize = 0;
            btnXuatPDF.Click += (ss, ee) => XuatPhieuVayPDF(model);
            pnlExport.Controls.Add(btnXuatPDF);
            dlgLichSu.Controls.Add(dgvHistory);
            dlgLichSu.Controls.Add(pnlExport);
            DongBoMauFormPhu(dlgLichSu);
            dlgLichSu.ShowDialog();
        }

        // Xuất HTML chuẩn in A4; trình duyệt hỗ trợ Ctrl+P > Save as PDF.
        // Đọc lại lịch sử trực tiếp từ SQL, không dùng dữ liệu mẫu hoặc dữ liệu trên lưới.
        private void XuatPhieuVayPDF(VayGridRowModel model)
        {
            if (model == null) return;
            try
            {
                var rows = new StringBuilder();
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"SELECT NgayGiaoDich, LoaiGiaoDich, SoTienGoc,
                       SoTienLai, PhiPhat, TongTien, SoDuConLai, GhiChu
                       FROM dbo.DN_LichSuGiaoDichVay WHERE SoTKVay=@stk
                       ORDER BY NgayGiaoDich DESC, Id DESC", conn))
                {
                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 30).Value = model.SoTKVay;
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string date = Convert.ToDateTime(r["NgayGiaoDich"]).ToString("dd/MM/yyyy HH:mm");
                            string loai = WebUtility.HtmlEncode(Convert.ToString(r["LoaiGiaoDich"]));
                            string goc = Convert.ToDecimal(r["SoTienGoc"]).ToString("N0");
                            string lai = Convert.ToDecimal(r["SoTienLai"]).ToString("N0");
                            string phiPhat = Convert.ToDecimal(r["PhiPhat"]).ToString("N0");
                            string tong = Convert.ToDecimal(r["TongTien"]).ToString("N0");
                            string du = r["SoDuConLai"] == DBNull.Value ? "—" : Convert.ToDecimal(r["SoDuConLai"]).ToString("N0");
                            string note = WebUtility.HtmlEncode(Convert.ToString(r["GhiChu"]));
                            rows.Append($@"<tr><td>{date}</td><td>{loai}</td><td class='num'>{goc}</td><td class='num'>{lai}</td><td class='num'>{phiPhat}</td><td class='num'>{tong}</td><td class='num'>{du}</td><td>{note}</td></tr>");
                        }
                    }
                }
                if (rows.Length == 0) rows.Append("<tr><td colspan='8' class='empty'>Chưa có giao dịch phát sinh.</td></tr>");
                Func<string, string> enc = WebUtility.HtmlEncode;
                string html = $@"<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'>
<title>Phiếu giao dịch khoản vay - {enc(model.SoTKVay)}</title>
<style>
*{{box-sizing:border-box;-webkit-print-color-adjust:exact;print-color-adjust:exact}}
body{{margin:0;background:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;color:#1e293b}}
.page{{max-width:1000px;margin:28px auto;background:white;padding:38px;border:1px solid #e2e8f0;border-radius:12px}}
header{{display:flex;justify-content:space-between;border-bottom:3px solid #2563eb;padding-bottom:20px;gap:20px}}
h1{{font-size:19px;margin:0;color:#0f172a}}.sub{{color:#64748b;font-size:12px;margin-top:6px}}
h2{{text-align:center;font-size:20px;margin:30px 0 5px}}.desc{{text-align:center;color:#64748b;font-size:13px;margin-bottom:25px}}
.grid{{display:grid;grid-template-columns:1fr 1fr;gap:18px}}.card{{background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:16px}}
.card h3{{font-size:14px;color:#2563eb;border-bottom:1px solid #cbd5e1;padding-bottom:8px;margin-top:0}}
.item{{display:flex;justify-content:space-between;gap:12px;margin:9px 0;font-size:13px}}.item span{{color:#64748b}}.item b{{text-align:right}}
h3{{font-size:15px;margin-top:26px}}table{{border-collapse:collapse;width:100%;font-size:11px}}
th{{background:#eaf0fa;text-align:left}}td,th{{padding:9px 6px;border-bottom:1px solid #e2e8f0;vertical-align:top}}
.num{{text-align:right;white-space:nowrap}}.empty{{text-align:center;color:#64748b}}
.sign{{display:grid;grid-template-columns:1fr 1fr;text-align:center;margin-top:40px;font-size:13px}}.space{{height:75px}}
@page{{size:A4 landscape;margin:12mm}}@media print{{body{{background:white}}.page{{max-width:none;border:0;margin:0;padding:0}}tr{{break-inside:avoid}}}}
</style></head><body><div class='page'>
<header><div><h1>NGÂN HÀNG THƯƠNG MẠI QLTKNH</h1><div class='sub'>Hệ thống quản lý tài khoản vay</div></div>
<div class='sub'>Mã chứng từ: VAY-{DateTime.Now:yyyyMMddHHmmss}<br>Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}</div></header>
<h2>PHIẾU XÁC NHẬN GIAO DỊCH KHOẢN VAY</h2><div class='desc'>Thông tin khoản vay và lịch sử thanh toán</div>
<div class='grid'><section class='card'><h3>THÔNG TIN KHÁCH HÀNG</h3>
<div class='item'><span>Họ tên</span><b>{enc(model.HoTen)}</b></div>
<div class='item'><span>CCCD</span><b>{enc(model.CCCD)}</b></div>
<div class='item'><span>Số điện thoại</span><b>{enc(model.SoDienThoai)}</b></div>
<div class='item'><span>Vai trò</span><b>{enc(model.VaiTro)}</b></div></section>
<section class='card'><h3>THÔNG TIN KHOẢN VAY</h3>
<div class='item'><span>Số tài khoản vay</span><b>{enc(model.SoTKVay)}</b></div>
<div class='item'><span>Số tiền vay</span><b>{model.SoTienVay:N0} VNĐ</b></div>
<div class='item'><span>Tổng đã thanh toán (gốc + lãi + phí)</span><b>{model.TongTienDaThanhToan:N0} VNĐ</b></div>
<div class='item'><span>Dư nợ gốc</span><b>{model.DuNo:N0} VNĐ</b></div>
<div class='item'><span>Lãi suất</span><b>{model.LaiSuat:N2}%</b></div>
<div class='item'><span>Kỳ hạn</span><b>{enc(model.KyHan)}</b></div>
<div class='item'><span>Ngày mở</span><b>{model.NgayMo:dd/MM/yyyy}</b></div>
<div class='item'><span>Trạng thái</span><b>{enc(GetStatusText(model.TrangThai))}</b></div>
<div class='item'><span>Nhóm nợ</span><b>{enc(model.NhomNoTen)}</b></div></section></div>
<h3>LỊCH SỬ THANH TOÁN KHOẢN VAY</h3>
<table><thead><tr><th>Thời gian</th><th>Loại giao dịch</th><th>Gốc trả</th><th>Lãi</th><th>Phí phạt</th><th>Tổng thanh toán</th><th>Dư nợ còn</th><th>Ghi chú</th></tr></thead><tbody>{rows}</tbody></table>
<div class='sign'><div><b>KHÁCH HÀNG</b><div class='sub'>(Ký, ghi rõ họ tên)</div><div class='space'></div><b>{enc(model.HoTen)}</b></div>
<div><b>GIAO DỊCH VIÊN XÁC NHẬN</b><div class='sub'>(Ký, đóng dấu)</div><div class='space'></div><b>Hệ thống QLTKNH</b></div></div>
</div></body></html>";
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Xuất phiếu giao dịch khoản vay (in PDF)";
                    sfd.Filter = "HTML Document (*.html)|*.html";
                    sfd.FileName = "PhieuVay_" + model.SoTKVay + ".html";
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllText(sfd.FileName, html, new UTF8Encoding(false));
                    if (MessageBox.Show("Đã tạo phiếu giao dịch. Mở trình duyệt để in PDF (Ctrl+P → Save as PDF)?",
                        "Xuất phiếu vay", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất phiếu giao dịch vay: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ParseKyHanToMonths(string kyHanStr)
        {
            if (string.IsNullOrEmpty(kyHanStr)) return 12;
            string clean = new string(kyHanStr.Where(char.IsDigit).ToArray());
            if (int.TryParse(clean, out int months)) return months;
            return 12;
        }
        #endregion

        private void lblSelectedCustomerInfo_Click(object sender, EventArgs e)
        {

        }

        private void panelSummary_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvVay_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}