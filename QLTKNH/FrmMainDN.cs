using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QLTKNH
{
    public partial class FrmMainDN : Form
    {
        private readonly Color colorContentBg = Color.FromArgb(15, 23, 42);      // #0F172A
        private readonly Color colorActiveBlue = Color.FromArgb(37, 99, 235);    // #2563EB
        private readonly Color colorInactiveTab = Color.FromArgb(23, 33, 60);    // #17213C
        private readonly Color colorHover = Color.FromArgb(30, 41, 59);         // #1E293B
        private readonly Color colorTextMuted = Color.FromArgb(160, 174, 192);   // #A0AEC0
        private readonly Color colorGreenHighlight = Color.FromArgb(34, 197, 94); // #22C55E

        private readonly string _connStr =
            @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        private Button currentActiveMenuBtn;
        private Form activeChildForm;
        private string currentMode = "DoanhNghiep";

        private List<KhachHangData> listKhachHang = new List<KhachHangData>();
        private KhachHangData selectedKhachHang = null;

        public FrmMainDN()
        {
            InitializeComponent();
            if (dgvKhachHang.Columns.Contains("colCCCD"))
                dgvKhachHang.Columns["colCCCD"].HeaderText = "CCCD / MST";
            // A single user control belongs to the main app window.
            // FrmMainDN is hosted inside FrmMain: no second profile button.
            this.Shown += (sender, e) => { if (this.TopLevel && this.FindForm() == this) KhoiTaoThongTinTaiKhoanSidebar(); };
            // Đồng bộ nhãn cột hiện hữu với các giá trị SQL; không sửa tên control Designer.
            colVaySoTienDaTra.HeaderText = "Tổng tiền đã trả (Gốc + Lãi + Phạt)";
            colTKSoTienGui.HeaderText = "Gốc còn lại";
            colTKSoTienDaRut.HeaderText = "Tổng tiền đã nhận";
            LoadDataFromSql();
            SetupSearchAndEvents();
            dgvKhachHang.CellFormatting += (sender, args) =>
            {
                if (args.RowIndex < 0 || args.ColumnIndex != 2) return;
                var row = dgvKhachHang.Rows[args.RowIndex];
                if (row.Cells[2].Style.Padding.Left > 0)
                    args.Value = "↳ " + Convert.ToString(args.Value);
            };
            SetupSearchGroup();
            DongBoMauToiFrmMain();
            VeDuongPhanCachNhomTaiKhoan();
            SwitchTab("DoanhNghiep");
            this.Load += FrmMain_Load;
            this.Shown += (s, e) => CapNhatThongTinTaiKhoanSidebar();
            this.Shown += (s, e) => { if (!BankSecurity.IsLoggedIn) this.Close(); };
            this.Activated += async (s, e) => { CapNhatThongTinTaiKhoanSidebar(); await DongBoKhiKichHoatAsync(); };
            KhoiTaoTheoDoiPhienNhanVien();
        }

        /// <summary>
        /// Khi FrmMainDN được mở như trang chính trong FrmMain,
        /// chỉ hiển thị danh sách doanh nghiệp và các bảng tài khoản.
        /// Thanh menu trái do FrmMain quản lý, tránh hiển thị hai menu.
        /// Không ảnh hưởng đến cách sử dụng FrmMainDN như một cửa sổ độc lập.
        /// </summary>
        public void HienThiNoiDungTrongFrmMain()
        {
            if (panelSidebar != null)
                panelSidebar.Visible = false;
            if (panelContent != null)
            {
                panelContent.Dock = DockStyle.Fill;
                panelContent.BringToFront();
            }
            if (panelMainLayout != null)
            {
                panelMainLayout.Dock = DockStyle.Fill;
                panelMainLayout.BringToFront();
            }
        }

        // Chỉ bổ sung theo dõi đăng nhập; không thay đổi dữ liệu/nghiệp vụ SQL của Main.
        private System.Windows.Forms.Timer _timerPhienNhanVien;
        private void KhoiTaoTheoDoiPhienNhanVien()
        {
            _timerPhienNhanVien = new System.Windows.Forms.Timer { Interval = 45000 };
            _timerPhienNhanVien.Tick += (sender, args) =>
            {
                if (IsDisposed || Disposing) return;
                try
                {
                    if (!BankSecurity.Heartbeat())
                    {
                        _timerPhienNhanVien.Stop();
                        MessageBox.Show("Phiên đăng nhập không còn hợp lệ. Vui lòng đăng nhập lại.");
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    _timerPhienNhanVien.Stop();
                    MessageBox.Show("Không thể xác nhận phiên đăng nhập với SQL: " + ex.Message);
                    Close();
                }
            };
            this.Shown += (sender, args) => { if (BankSecurity.IsLoggedIn) _timerPhienNhanVien.Start(); };
            this.FormClosed += (sender, args) => { _timerPhienNhanVien.Stop(); _timerPhienNhanVien.Dispose(); };
        }

        // Chỉ đồng bộ màu cho các control của FrmMainDN; không sửa layout, sự kiện hoặc dữ liệu.
        private void DongBoMauToiFrmMain()
        {
            Color nen = colorContentBg;
            Color nenPhu = colorHover;
            Color tieuDe = colorInactiveTab;
            Color chu = Color.FromArgb(226, 232, 240);
            Color vien = colorActiveBlue;

            this.BackColor = nen;
            Action<Control> toMau = null;
            toMau = control =>
            {
                if (control is DataGridView dgv)
                {
                    // Giảm nhấp nháy khi cập nhật bảng, không đổi kiểu hiển thị.
                    if (!SystemInformation.TerminalServerSession)
                    {
                        try { typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dgv, true, null); }
                        catch (Exception) { /* Giữ cách vẽ mặc định nếu không hỗ trợ. */ }
                    }
                    dgv.BackgroundColor = nen;
                    dgv.GridColor = vien;
                    dgv.BorderStyle = BorderStyle.None;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = tieuDe;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.RowHeadersDefaultCellStyle.BackColor = tieuDe;
                    dgv.RowHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.DefaultCellStyle.BackColor = nen;
                    dgv.DefaultCellStyle.ForeColor = chu;
                    dgv.DefaultCellStyle.SelectionBackColor = vien;
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                    dgv.RowsDefaultCellStyle.BackColor = nen;
                    dgv.RowsDefaultCellStyle.ForeColor = chu;
                    dgv.RowsDefaultCellStyle.SelectionBackColor = vien;
                    dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = nenPhu;
                    dgv.AlternatingRowsDefaultCellStyle.ForeColor = chu;
                    dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = vien;
                    dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
                }
                else if (control is TextBox txt)
                {
                    txt.BackColor = nenPhu;
                    txt.ForeColor = Color.White;
                }
                else if (control is ComboBox cbo)
                {
                    cbo.BackColor = nenPhu;
                    cbo.ForeColor = Color.White;
                }
                else if (control is Label lbl)
                {
                    // Giữ màu nhấn của các nhãn trạng thái / số liệu đang có màu riêng.
                    if (lbl.ForeColor == Color.Black || lbl.ForeColor == SystemColors.ControlText ||
                        lbl.ForeColor == Color.FromArgb(15, 23, 42))
                        lbl.ForeColor = chu;
                }
                else if (control is GroupBox group)
                {
                    group.BackColor = nen;
                    group.ForeColor = chu;
                }
                else if (control is Panel panel)
                {
                    // Sidebar giữ nền xanh đen riêng, vùng nội dung dùng nền tối chung.
                    panel.BackColor = panel == panelMenu ? Color.FromArgb(12, 20, 40) : nen;
                }
                else if (control is Button button)
                {
                    // Giữ màu nút đang nổi bật, màu active sẽ tiếp tục do logic cũ điều khiển.
                    if (button != btnCaNhan && button != btnDoanhNghiep && button != btnTimCCCD)
                    {
                        button.BackColor = nenPhu;
                        button.ForeColor = chu;
                    }
                }
                foreach (Control child in control.Controls)
                    toMau(child);
            };
            foreach (Control control in this.Controls)
                toMau(control);
        }

        // Hai thanh trắng đặt nổi phía trên các panel, không thay đổi bố cục hoặc nghiệp vụ.
        private Panel _phanCachVayTietKiem;
        private Panel _phanCachTietKiemThanhToan;
        private Panel _phanCachDocDanhSach;
        private bool _dangCanChinhPhanCach;

        private void VeDuongPhanCachNhomTaiKhoan()
        {
            // Chờ các control hoàn tất bố trí trước khi xác định vị trí thực tế.
            this.Shown += (s, e) => CapNhatThanhPhanCach();
            this.Resize += (s, e) => CapNhatThanhPhanCach();
            foreach (DataGridView grid in new[] { dgvKhachHang, dgvVay, dgvTietKiem, dgvThanhToan })
            {
                grid.LocationChanged += (s, e) => CapNhatThanhPhanCach();
                grid.SizeChanged += (s, e) => CapNhatThanhPhanCach();
                grid.VisibleChanged += (s, e) => CapNhatThanhPhanCach();
            }
        }

        private void CapNhatThanhPhanCach()
        {
            if (_dangCanChinhPhanCach || !this.IsHandleCreated || this.IsDisposed) return;
            _dangCanChinhPhanCach = true;
            try
            {
                Control parent = dgvVay.Parent;
                // Tìm control cha chung để thanh không bị che bởi các panel chứa bảng.
                while (parent != null &&
                       (!LaControlCon(parent, dgvTietKiem) || !LaControlCon(parent, dgvThanhToan) || !LaControlCon(parent, dgvKhachHang)))
                    parent = parent.Parent;
                if (parent == null) return;

                if (_phanCachVayTietKiem == null || _phanCachVayTietKiem.IsDisposed || _phanCachVayTietKiem.Parent != parent)
                {
                    _phanCachVayTietKiem = new Panel { BackColor = Color.FromArgb(235, 242, 255), Height = 2, Enabled = false };
                    parent.Controls.Add(_phanCachVayTietKiem);
                }
                if (_phanCachTietKiemThanhToan == null || _phanCachTietKiemThanhToan.IsDisposed || _phanCachTietKiemThanhToan.Parent != parent)
                {
                    _phanCachTietKiemThanhToan = new Panel { BackColor = Color.FromArgb(235, 242, 255), Height = 2, Enabled = false };
                    parent.Controls.Add(_phanCachTietKiemThanhToan);
                }

                if (_phanCachDocDanhSach == null || _phanCachDocDanhSach.IsDisposed || _phanCachDocDanhSach.Parent != parent)
                {
                    _phanCachDocDanhSach = new Panel { BackColor = Color.FromArgb(235, 242, 255), Width = 2, Enabled = false };
                    parent.Controls.Add(_phanCachDocDanhSach);
                }

                Rectangle vay = new Rectangle(parent.PointToClient(dgvVay.PointToScreen(Point.Empty)), dgvVay.Size);
                Rectangle tietKiem = new Rectangle(parent.PointToClient(dgvTietKiem.PointToScreen(Point.Empty)), dgvTietKiem.Size);
                Rectangle thanhToan = new Rectangle(parent.PointToClient(dgvThanhToan.PointToScreen(Point.Empty)), dgvThanhToan.Size);
                DatThanhPhanCach(_phanCachVayTietKiem, vay, tietKiem);
                DatThanhPhanCach(_phanCachTietKiemThanhToan, tietKiem, thanhToan);
                Rectangle danhSach = new Rectangle(parent.PointToClient(dgvKhachHang.PointToScreen(Point.Empty)), dgvKhachHang.Size);
                int leftEdge = danhSach.Right;
                int rightEdge = Math.Min(vay.Left, Math.Min(tietKiem.Left, thanhToan.Left));
                _phanCachDocDanhSach.Visible = rightEdge - leftEdge > 8;
                if (_phanCachDocDanhSach.Visible)
                {
                    int top = Math.Min(danhSach.Top, vay.Top) - 28;
                    int bottom = Math.Max(danhSach.Bottom, Math.Max(vay.Bottom, Math.Max(tietKiem.Bottom, thanhToan.Bottom)));
                    _phanCachDocDanhSach.Bounds = new Rectangle(leftEdge + (rightEdge - leftEdge) / 2, Math.Max(0, top), 2, Math.Max(2, bottom - Math.Max(0, top)));
                    _phanCachDocDanhSach.BringToFront();
                }
            }
            finally { _dangCanChinhPhanCach = false; }
        }

        private static bool LaControlCon(Control cha, Control con)
        {
            for (Control c = con; c != null; c = c.Parent)
                if (c == cha) return true;
            return false;
        }

        private static void DatThanhPhanCach(Panel thanh, Rectangle tren, Rectangle duoi)
        {
            int khoangTrong = duoi.Top - tren.Bottom;
            thanh.Visible = khoangTrong > 12;
            if (!thanh.Visible) return;
            // Đặt đường kẻ phía trên tiêu đề nhóm tiếp theo, tránh cắt ngang chữ.
            int y = Math.Max(tren.Bottom + 8, duoi.Top - 35);
            int trai = Math.Min(tren.Left, duoi.Left);
            int phai = Math.Max(tren.Right, duoi.Right);
            thanh.Bounds = new Rectangle(trai, y, Math.Max(1, phai - trai), 2);
            thanh.BringToFront();
        }

        // Đồng bộ dữ liệu mới nhất sau khi sửa thông tin ở các form con.
        // Không sửa dữ liệu SQL và không thay đổi các sự kiện nghiệp vụ cũ.
        private bool _dangDongBoThongTin;
        private bool _dangTaiKhiKichHoat;
        private long _phienBanDuLieu;

        // Chỉ đọc SQL trên luồng nền; mọi cập nhật model và control vẫn ở UI thread.
        private async System.Threading.Tasks.Task DongBoKhiKichHoatAsync()
        {
            if (_dangTaiKhiKichHoat || _dangDongBoThongTin || IsDisposed || Disposing) return;
            _dangTaiKhiKichHoat = true;
            long phienBan = _phienBanDuLieu;
            try
            {
                var ds = await System.Threading.Tasks.Task.Run(() => DocDuLieuTuSql());
                if (IsDisposed || Disposing || phienBan != _phienBanDuLieu) return;
                string cccd = selectedKhachHang?.CCCD;
                ApDungDuLieuSql(ds);
                FilterCustomerList(txtCCCDSearch.Text.Trim(), !string.IsNullOrWhiteSpace(txtCCCDSearch.Text));
                if (!string.IsNullOrWhiteSpace(cccd)) SelectCustomerInGrid(cccd);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing && phienBan == _phienBanDuLieu)
                    MessageBox.Show("Không thể tải dữ liệu từ SQL Server QLTKNH:\n" + ex.Message,
                        "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _dangTaiKhiKichHoat = false; }
        }
        public void DongBoThongTinTuSql()
        {
            if (_dangDongBoThongTin || IsDisposed || Disposing) return;
            _dangDongBoThongTin = true;
            try
            {
                string cccdDangChon = selectedKhachHang?.CCCD;
                LoadDataFromSql();
                if (!string.IsNullOrWhiteSpace(cccdDangChon))
                    selectedKhachHang = listKhachHang.FirstOrDefault(k => k.CCCD == cccdDangChon);
                FilterCustomerList(txtCCCDSearch.Text.Trim(), !string.IsNullOrWhiteSpace(txtCCCDSearch.Text));
                if (!string.IsNullOrWhiteSpace(cccdDangChon)) SelectCustomerInGrid(cccdDangChon);
            }
            finally { _dangDongBoThongTin = false; }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ClearAllSelections();
        }

        #region Khai Báo Data Models
        public enum TrangThaiTK { HoatDong, Khoa, Dong }

        public class TaiKhoanVayItem
        {
            public string SoTKVay { get; set; }
            public decimal SoTienVay { get; set; }
            public decimal SoTienDaTra { get; set; } = 0;
            // Tổng khách đã thanh toán: gốc + lãi + phí phạt; không dùng để tính dư nợ.
            public decimal TongTienDaThanhToan { get; set; } = 0;
            public double LaiSuat { get; set; }
            public string KyHan { get; set; }
            public TrangThaiTK TrangThai { get; set; } = TrangThaiTK.HoatDong;
            public DateTime NgayMo { get; set; } = DateTime.Now;
            public DateTime? NgayCapNhatTrangThai { get; set; } = DateTime.Now;
        }

        public class SoTietKiemItem
        {
            public string SoTKTK { get; set; }
            public decimal SoTienGui { get; set; }
            public double LaiSuat { get; set; }
            public decimal SoTienDaRut { get; set; } = 0;
            // Tổng tiền đã nhận (gốc + lãi) lưu trong bảng SoTietKiem.
            public decimal TongTienDaNhan { get; set; } = 0;
            public string KyHan { get; set; }
            public TrangThaiTK TrangThai { get; set; } = TrangThaiTK.HoatDong;
            public DateTime NgayMo { get; set; } = DateTime.Now;
            public DateTime? NgayCapNhatTT { get; set; } = DateTime.Now;
        }

        public class TaiKhoanThanhToanItem
        {
            public string SoTKTT { get; set; }
            public decimal SoDu { get; set; }
            public TrangThaiTK TrangThai { get; set; } = TrangThaiTK.HoatDong;
            public DateTime NgayMo { get; set; } = DateTime.Now;
        }

        public class DongSoHuuData
        {
            public string CCCD { get; set; }
            public string HoTen { get; set; }
            public string SoDienThoai { get; set; }
            public string CCCD_ChuSoHuu { get; set; }
        }

        public class KhachHangData
        {
            public string CCCD { get; set; }
            public string HoTen { get; set; }
            public DateTime NgaySinh { get; set; }
            public string SoDienThoai { get; set; }
            public List<DongSoHuuData> DanhSachDongSoHuu { get; set; } = new List<DongSoHuuData>();
            public decimal NoTruocDo { get; set; } = 0;
            public List<TaiKhoanVayItem> DanhSachVay { get; set; } = new List<TaiKhoanVayItem>();
            public List<SoTietKiemItem> DanhSachTietKiem { get; set; } = new List<SoTietKiemItem>();
            public List<TaiKhoanThanhToanItem> DanhSachThanhToan { get; set; } = new List<TaiKhoanThanhToanItem>();

            public bool IsDongSoHuuOnly { get; set; } = false;
            public HashSet<string> TaiKhoanDuocUyQuyen { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public string ChuSoHuuCCCD { get; set; } = null;
        }
        #endregion


        // Sổ tiết kiệm hợp lệ để dùng làm tài sản bảo đảm (đọc trực tiếp SQL).
        public sealed class SoTietKiemTheChap
        {
            public string SoTKTK { get; set; }
            public decimal GocConLai { get; set; }
            public decimal LaiTamTinh { get; set; }
            public decimal TongGiaTri => GocConLai + LaiTamTinh;
            public override string ToString() => SoTKTK + " | " + TongGiaTri.ToString("N0") + " VNĐ";
        }

        public static List<SoTietKiemTheChap> LaySoTietKiemTheChap(string connStr, string cccd)
        {
            var result = new List<SoTietKiemTheChap>();
            using (var cn = new SqlConnection(connStr))
            {
                cn.Open();
                var soDaVay = FrmMain.LaySoTietKiemDaVay(cn, null, true, false);
                using (var cmd = new SqlCommand(@"SELECT SoTKTK, SoTienGui, SoTienDaRut, LaiSuat, NgayMo
                    FROM dbo.DN_SoTietKiem
                    WHERE CCCD = @cccd AND TrangThai = 0 AND SoTienGui > 0
                    ORDER BY SoTKTK", cn))
                {
                    cmd.Parameters.Add("@cccd", System.Data.SqlDbType.VarChar, 12).Value = cccd;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            if (soDaVay.Contains(Convert.ToString(rd["SoTKTK"]).Trim())) continue;
                            decimal goc = Math.Max(0m, Convert.ToDecimal(rd["SoTienGui"]));
                            decimal laiSuat = Convert.ToDecimal(rd["LaiSuat"]);
                            DateTime ngayMo = Convert.ToDateTime(rd["NgayMo"]);
                            decimal days = Math.Max(0, (DateTime.Today - ngayMo.Date).Days);
                            decimal lai = Math.Round(goc * laiSuat / 100m * days / 365m, 0, MidpointRounding.AwayFromZero);
                            result.Add(new SoTietKiemTheChap { SoTKTK = rd["SoTKTK"].ToString(), GocConLai = goc, LaiTamTinh = lai });
                        }
                    }
                }
            }
            return result;
        }

        // Dùng nhóm nợ lưu trong SQL, không tự ghi đè khi mở khoản vay.
        public static int LayNhomNoHienTai(string connStr, string cccd)
        {
            return FrmMain.LayNhomNoKhiXetVay(connStr, true, cccd);
        }


        #region Đọc dữ liệu từ SQL Server QLTKNH
        private void LoadDataFromSql()
        {
            ++_phienBanDuLieu;
            try
            {
                ApDungDuLieuSql(DocDuLieuTuSql());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu từ SQL Server QLTKNH:\n" + ex.Message,
                    "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<KhachHangData> DocDuLieuTuSql()
        {
            var ds = new List<KhachHangData>();
            var theoCCCD = new Dictionary<string, KhachHangData>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                // 1. Đọc danh sách Khách hàng chính (Chủ sở hữu)
                using (SqlCommand cmd = new SqlCommand(


                    "SELECT CCCD, HoTen, NgaySinh, SoDienThoai, NoTruocDo FROM dbo.DN_KhachHang ORDER BY HoTen, CCCD", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var kh = new KhachHangData
                        {
                            CCCD = DbText(r, "CCCD"),
                            HoTen = DbText(r, "HoTen"),
                            NgaySinh = DbDate(r, "NgaySinh"),
                            SoDienThoai = DbText(r, "SoDienThoai"),
                            NoTruocDo = DbDecimal(r, "NoTruocDo")
                        };
                        ds.Add(kh);
                        theoCCCD[kh.CCCD] = kh;
                    }
                }

                // 2. Đọc danh sách Người được ủy quyền
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai FROM dbo.DN_DongSoHuu ORDER BY Id", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string chuCCCD = DbText(r, "CCCD_ChuSoHuu");
                        string coCCCD = DbText(r, "CCCD");
                        string coHoTen = DbText(r, "HoTen");
                        string coSDT = DbText(r, "SoDienThoai");
                        // Giữ tên cá nhân trong liên kết ủy quyền; HoTen của hồ sơ chủ
                        // tài khoản riêng có thể là tên doanh nghiệp, không ghi đè vào đây.

                        if (!string.IsNullOrEmpty(chuCCCD) && theoCCCD.TryGetValue(chuCCCD, out KhachHangData mainKh))
                        {
                            if (!mainKh.DanhSachDongSoHuu.Any(d => d.CCCD.Equals(coCCCD, StringComparison.OrdinalIgnoreCase)))
                            {
                                mainKh.DanhSachDongSoHuu.Add(new DongSoHuuData
                                {
                                    CCCD = coCCCD,
                                    HoTen = coHoTen,
                                    SoDienThoai = coSDT,
                                    CCCD_ChuSoHuu = chuCCCD
                                });
                            }
                        }

                        // Tạo bản ghi cho Người được ủy quyền nếu chưa có trong bảng KhachHang
                        if (!theoCCCD.ContainsKey(coCCCD))
                        {
                            var coKh = new KhachHangData
                            {
                                CCCD = coCCCD,
                                HoTen = coHoTen,
                                SoDienThoai = coSDT,
                                IsDongSoHuuOnly = true,
                                ChuSoHuuCCCD = chuCCCD
                            };
                            ds.Add(coKh);
                            theoCCCD[coCCCD] = coKh;
                        }
                        else
                        {
                            theoCCCD[coCCCD].ChuSoHuuCCCD = chuCCCD;
                        }
                    }
                }

                // 3. Đọc dữ liệu Tài khoản vay
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT v.SoTKVay, v.CCCD, v.SoTienVay, v.SoTienDaTra, v.LaiSuat, v.KyHan, v.TrangThai, v.NgayMo, v.NgayCapNhatTrangThai,
                             v.SoTienDaTra + ISNULL(h.LaiVaPhi, 0) AS TongTienDaThanhToan
                             FROM dbo.DN_TaiKhoanVay v
                             LEFT JOIN (SELECT SoTKVay, SUM(ISNULL(SoTienLai,0) + ISNULL(PhiPhat,0)) AS LaiVaPhi
                                        FROM dbo.DN_LichSuGiaoDichVay
                                        WHERE LoaiGiaoDich <> N'Giải ngân'
                                        GROUP BY SoTKVay) h ON h.SoTKVay = v.SoTKVay ORDER BY v.SoTKVay", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!theoCCCD.TryGetValue(DbText(r, "CCCD"), out KhachHangData kh)) continue;
                        kh.DanhSachVay.Add(new TaiKhoanVayItem
                        {
                            SoTKVay = DbText(r, "SoTKVay"),
                            SoTienVay = DbDecimal(r, "SoTienVay"),
                            SoTienDaTra = DbDecimal(r, "SoTienDaTra"),
                            TongTienDaThanhToan = DbDecimal(r, "TongTienDaThanhToan"),
                            LaiSuat = DbDouble(r, "LaiSuat"),
                            KyHan = DbText(r, "KyHan"),
                            TrangThai = DbStatus(r, "TrangThai"),
                            NgayMo = DbDate(r, "NgayMo"),
                            NgayCapNhatTrangThai = DbNullableDate(r, "NgayCapNhatTrangThai")
                        });
                    }
                }

                // 4. Đọc dữ liệu Sổ tiết kiệm
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT SoTKTK, CCCD, SoTienGui, LaiSuat, SoTienDaRut, KyHan, TrangThai, NgayMo, NgayCapNhatTT, ISNULL(TongTienDaNhan,0) AS TongTienDaNhan FROM dbo.DN_SoTietKiem ORDER BY SoTKTK", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!theoCCCD.TryGetValue(DbText(r, "CCCD"), out KhachHangData kh)) continue;
                        kh.DanhSachTietKiem.Add(new SoTietKiemItem
                        {
                            SoTKTK = DbText(r, "SoTKTK"),
                            SoTienGui = DbDecimal(r, "SoTienGui"),
                            LaiSuat = DbDouble(r, "LaiSuat"),
                            SoTienDaRut = DbDecimal(r, "SoTienDaRut"),
                            TongTienDaNhan = DbDecimal(r, "TongTienDaNhan"),
                            KyHan = DbText(r, "KyHan"),
                            TrangThai = DbStatus(r, "TrangThai"),
                            NgayMo = DbDate(r, "NgayMo"),
                            NgayCapNhatTT = DbNullableDate(r, "NgayCapNhatTT")
                        });
                    }
                }

                // 5. Đọc dữ liệu Tài khoản thanh toán
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT SoTKTT, CCCD, SoDu, TrangThai, NgayMo FROM dbo.DN_TaiKhoanThanhToan ORDER BY SoTKTT", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!theoCCCD.TryGetValue(DbText(r, "CCCD"), out KhachHangData kh)) continue;
                        kh.DanhSachThanhToan.Add(new TaiKhoanThanhToanItem
                        {
                            SoTKTT = DbText(r, "SoTKTT"),
                            SoDu = DbDecimal(r, "SoDu"),
                            TrangThai = DbStatus(r, "TrangThai"),
                            NgayMo = DbDate(r, "NgayMo")
                        });
                    }
                }
                using (var cmd = new SqlCommand(@"SELECT N'V|' + SoTKVay AS Ma, CCCD FROM dbo.DN_DongSoHuuTaiKhoanVay
                    UNION ALL SELECT N'K|' + SoTKTK, CCCD FROM dbo.DN_DongSoHuuSoTietKiem
                    UNION ALL SELECT N'T|' + SoTKTT, CCCD FROM dbo.DN_DongSoHuuTaiKhoanThanhToan", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        if (theoCCCD.TryGetValue(DbText(r, "CCCD"), out KhachHangData nguoi))
                            nguoi.TaiKhoanDuocUyQuyen.Add(DbText(r, "Ma"));
            }
            return ds;
        }

        private void ApDungDuLieuSql(List<KhachHangData> ds)
        {
            string previousCCCD = selectedKhachHang?.CCCD;
            listKhachHang = ds;
            selectedKhachHang = !string.IsNullOrEmpty(previousCCCD)
                ? listKhachHang.FirstOrDefault(k => k.CCCD == previousCCCD)
                : null;
        }

        private static string DbText(SqlDataReader r, string column) => r[column] == DBNull.Value ? "" : Convert.ToString(r[column]);
        private static decimal DbDecimal(SqlDataReader r, string column) => r[column] == DBNull.Value ? 0m : Convert.ToDecimal(r[column]);
        private static double DbDouble(SqlDataReader r, string column) => r[column] == DBNull.Value ? 0d : Convert.ToDouble(r[column]);
        private static DateTime DbDate(SqlDataReader r, string column) => r[column] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r[column]);
        private static DateTime? DbNullableDate(SqlDataReader r, string column) => r[column] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[column]);

        private static TrangThaiTK DbStatus(SqlDataReader r, string column)
        {
            int code = Convert.ToInt32(r[column]);
            if (code < 0 || code > 2)
                throw new InvalidOperationException("Trạng thái tài khoản không hợp lệ: " + code);
            return (TrangThaiTK)code;
        }
        #endregion


        // GroupBox bao phủ toàn bộ thanh tìm kiếm; chỉ vùng trống / nhãn mới làm mới.
        private GroupBox _searchGroup;
        private bool _searchRefreshing;

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
                selectedKhachHang = null;
                LoadDataFromSql();
                selectedKhachHang = null;
                FilterCustomerList("");
                foreach (var grid in new[] { dgvKhachHang, dgvVay, dgvTietKiem, dgvThanhToan })
                    grid.CurrentCell = null;
                ClearAllSelections();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể làm mới dữ liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _searchRefreshing = false; }
        }

        private void SetupSearchAndEvents()
        {
            txtCCCDSearch.TextChanged += TxtCCCDSearch_TextChanged;
            txtCCCDSearch.KeyPress += TxtCCCDSearch_KeyPress;
            btnTimCCCD.Click += BtnTimCCCD_Click;

            // Cấu hình bôi xanh khi chọn Khách Hàng bên trái
            SetupCustomerGridStyling();

            // Sự kiện Bảng Khách hàng
            dgvKhachHang.CellClick += DgvKhachHang_CellClick;
            dgvKhachHang.CellDoubleClick += DgvKhachHang_CellDoubleClick;
            dgvKhachHang.MouseDown += DgvKhachHang_MouseDown;

            // Double-click mở Form chi tiết tương ứng
            dgvVay.CellDoubleClick += DgvVay_CellDoubleClick;
            dgvTietKiem.CellDoubleClick += DgvTietKiem_CellDoubleClick;
            dgvThanhToan.CellDoubleClick += DgvThanhToan_CellDoubleClick;

            // Cấu hình chọn nhiều dòng (Multi-Select) & Menu Chuột Phải cho các bảng tài khoản
            SetupGridContextMenu(dgvVay, (listSoTK) => MoFormVaySelected(listSoTK), "Tài khoản Vay", "colVaySoTK");
            SetupGridContextMenu(dgvTietKiem, (listSoTK) => MoFormTietKiemSelected(listSoTK), "Sổ Tiết Kiệm", "colTKSoTK");
            SetupGridContextMenu(dgvThanhToan, (listSoTK) => MoFormThanhToanSelected(listSoTK), "Tài khoản Thanh Toán", "colTTSoTK");

            FilterCustomerList("");
        }

        private void SetupCustomerGridStyling()
        {
            dgvKhachHang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKhachHang.MultiSelect = false;
            dgvKhachHang.DefaultCellStyle.BackColor = colorContentBg;
            dgvKhachHang.DefaultCellStyle.ForeColor = Color.White;
            dgvKhachHang.DefaultCellStyle.SelectionBackColor = colorActiveBlue;
            dgvKhachHang.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvKhachHang.RowsDefaultCellStyle.SelectionBackColor = colorActiveBlue;
            dgvKhachHang.RowsDefaultCellStyle.SelectionForeColor = Color.White;
        }

        #region Cấu hình Menu Chuột Phải Bảng Tài Khoản
        private void SetupGridContextMenu(DataGridView grid, Action<List<string>> openAction, string typeName, string colNameSoTK)
        {
            grid.DefaultCellStyle.SelectionBackColor = colorActiveBlue;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.MultiSelect = true;

            grid.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var hit = grid.HitTest(e.X, e.Y);
                    if (hit.RowIndex >= 0)
                    {
                        if (!grid.Rows[hit.RowIndex].Selected)
                        {
                            grid.ClearSelection();
                            grid.Rows[hit.RowIndex].Selected = true;
                        }

                        List<string> listSoTK = new List<string>();
                        foreach (DataGridViewRow row in grid.SelectedRows)
                        {
                            if (row.Cells[colNameSoTK].Value != null)
                            {
                                string stk = row.Cells[colNameSoTK].Value.ToString().Trim();
                                if (!string.IsNullOrEmpty(stk)) listSoTK.Add(stk);
                            }
                        }

                        ContextMenuStrip cms = new ContextMenuStrip();
                        int count = listSoTK.Count;
                        ToolStripMenuItem itemOpen = new ToolStripMenuItem($"Mở {typeName} đã chọn ({count})");
                        itemOpen.Click += (sender, args) => openAction(listSoTK);
                        cms.Items.Add(itemOpen);
                        cms.Show(grid, e.Location);
                    }
                }
            };
        }

        private void ClearAccountGridSelections()
        {
            dgvVay.ClearSelection();
            dgvTietKiem.ClearSelection();
            dgvThanhToan.ClearSelection();
        }

        private void ClearAllSelections()
        {
            dgvKhachHang.ClearSelection();
            ClearAccountGridSelections();
        }
        #endregion

        #region Tìm Kiếm & Lọc CCCD
        private void TxtCCCDSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TxtCCCDSearch_TextChanged(object sender, EventArgs e)
        {
            if (_searchRefreshing) return;
            string cleanText = new string(txtCCCDSearch.Text.Where(char.IsDigit).ToArray());
            if (cleanText.Length > 12) cleanText = cleanText.Substring(0, 12);

            if (txtCCCDSearch.Text != cleanText)
            {
                int cursorPos = txtCCCDSearch.SelectionStart;
                txtCCCDSearch.Text = cleanText;
                txtCCCDSearch.SelectionStart = Math.Min(cursorPos, txtCCCDSearch.Text.Length);
                return;
            }

            FilterCustomerList(cleanText, !string.IsNullOrEmpty(cleanText));
        }

        private void FilterCustomerList(string filterCCCD, bool highlightSelected = false)
        {
            dgvKhachHang.Rows.Clear();
            var filtered = string.IsNullOrWhiteSpace(filterCCCD)
                ? listKhachHang
                : listKhachHang.Where(k => k.CCCD.Contains(filterCCCD)).ToList();

            int stt = 1;
            // Phân cấp theo liên kết doanh nghiệp hiện có, không đổi dữ liệu hay điều kiện lọc.
            var nhom = filtered.GroupBy(k => k.IsDongSoHuuOnly && !string.IsNullOrEmpty(k.ChuSoHuuCCCD)
                ? k.ChuSoHuuCCCD : k.CCCD, StringComparer.OrdinalIgnoreCase);
            foreach (var col in dgvKhachHang.Columns.Cast<DataGridViewColumn>())
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            foreach (var g in nhom)
            {
                var rows = g.OrderBy(k => k.IsDongSoHuuOnly ? 1 : 0).ToList();
                var chu = listKhachHang.FirstOrDefault(k => k.CCCD == g.Key && !k.IsDongSoHuuOnly);
                foreach (var kh in rows)
                {
                    int idx = dgvKhachHang.Rows.Add(stt++, kh.CCCD, kh.HoTen, kh.SoDienThoai);
                    var cell = dgvKhachHang.Rows[idx].Cells[2];
                    cell.Style.ForeColor = kh.IsDongSoHuuOnly ? Color.FromArgb(192, 132, 252) : Color.FromArgb(96, 165, 250);
                    if (kh.IsDongSoHuuOnly)
                    {
                        cell.Style.Padding = new Padding(18, 0, 0, 0);
                        cell.ToolTipText = "Người được ủy quyền | Doanh nghiệp: " + (chu == null ? g.Key : chu.HoTen + " | MST: " + chu.CCCD);
                    }
                    else cell.ToolTipText = "Chủ sở hữu | Mã hồ sơ: " + kh.CCCD +
                        (kh.TaiKhoanDuocUyQuyen.Count > 0 ? " | Đồng thời là người được ủy quyền ở tài khoản khác" : "");
                }
            }

            if (filtered.Count > 0 && (selectedKhachHang != null || !string.IsNullOrWhiteSpace(filterCCCD)))
            {
                if (selectedKhachHang == null || !filtered.Contains(selectedKhachHang))
                {
                    selectedKhachHang = filtered[0];
                }
            }
            else
            {
                selectedKhachHang = null;
            }

            DisplayCustomerDetails();

            if (highlightSelected && selectedKhachHang != null)
            {
                SelectCustomerInGrid(selectedKhachHang.CCCD);
            }
            else
            {
                dgvKhachHang.ClearSelection();
            }
        }

        private int GetCurrentNhomNoIndexForCustomer(string cccd)
        {
            if (string.IsNullOrWhiteSpace(cccd)) return 0;

            int maxCode = 1;
            var kh = listKhachHang.FirstOrDefault(k => k.CCCD == cccd);
            if (kh != null)
            {
                foreach (var vay in kh.DanhSachVay)
                {
                    int code = 1;
                    if (FrmVayDN.dictCustomNhomNo.TryGetValue(vay.SoTKVay, out int customCode))
                    {
                        code = customCode;
                    }
                    else
                    {
                        decimal duNo = Math.Max(0, vay.SoTienVay - vay.SoTienDaTra);
                        if (duNo > 0)
                        {
                            int months = 12;
                            string clean = new string((vay.KyHan ?? "").Where(char.IsDigit).ToArray());
                            if (int.TryParse(clean, out int m)) months = m;
                            DateTime ngayDaoHan = vay.NgayMo.AddMonths(months);
                            if (DateTime.Now > ngayDaoHan)
                            {
                                int soNgayQuaHan = (int)(DateTime.Now - ngayDaoHan).TotalDays;
                                if (soNgayQuaHan >= 361) code = 5;
                                else if (soNgayQuaHan >= 181) code = 4;
                                else if (soNgayQuaHan >= 91) code = 3;
                                else if (soNgayQuaHan >= 10) code = 2;
                            }
                        }
                    }
                    if (code > maxCode) maxCode = code;
                }

                foreach (var tk in kh.DanhSachTietKiem)
                {
                    if (FrmVayDN.dictCustomNhomNo.TryGetValue(tk.SoTKTK, out int customCode))
                    {
                        if (customCode > maxCode) maxCode = customCode;
                    }
                }

                foreach (var tt in kh.DanhSachThanhToan)
                {
                    if (FrmVayDN.dictCustomNhomNo.TryGetValue(tt.SoTKTT, out int customCode))
                    {
                        if (customCode > maxCode) maxCode = customCode;
                    }
                }
            }
            return Math.Max(0, maxCode - 1);
        }

        private void BtnTimCCCD_Click(object sender, EventArgs e)
        {
            string cccd = txtCCCDSearch.Text.Trim();
            if (string.IsNullOrEmpty(cccd))
            {
                MessageBox.Show("Vui lòng nhập CCCD / MST cần tìm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var target = listKhachHang.FirstOrDefault(k => k.CCCD == cccd);
            if (target != null)
            {
                selectedKhachHang = target;
                DisplayCustomerDetails();
                SelectCustomerInGrid(target.CCCD);
            }
            else
            {
                OpenRegistrationDialog(cccd);
            }
        }

        // Quy tắc đồ án: chỉ doanh nghiệp chủ tài khoản được mở thêm tài khoản.
        private const string LoiUyQuyenMoTaiKhoanDN = "Người được ủy quyền không được mở tài khoản cho doanh nghiệp. Vui lòng chọn doanh nghiệp chủ tài khoản.";

        private bool LaNguoiDuocUyQuyenDN(KhachHangData kh)
        {
            return kh != null && (kh.IsDongSoHuuOnly || kh.TaiKhoanDuocUyQuyen.Count > 0 ||
                listKhachHang.Any(k => !string.Equals(k.CCCD, kh.CCCD, StringComparison.OrdinalIgnoreCase) &&
                    k.DanhSachDongSoHuu.Any(d => string.Equals(d.CCCD, kh.CCCD, StringComparison.OrdinalIgnoreCase))));
        }

        private static void KiemTraQuyenMoTaiKhoanDN(SqlConnection cn, SqlTransaction tx, string ma)
        {
            using (var cmd = new SqlCommand(@"SELECT CASE WHEN
                EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD=@ma AND CCCD_ChuSoHuu<>@ma)
                OR EXISTS(SELECT 1 FROM dbo.DN_DongSoHuuTaiKhoanThanhToan WHERE CCCD=@ma)
                OR EXISTS(SELECT 1 FROM dbo.DN_DongSoHuuSoTietKiem WHERE CCCD=@ma)
                OR EXISTS(SELECT 1 FROM dbo.DN_DongSoHuuTaiKhoanVay WHERE CCCD=@ma)
                THEN 1 ELSE 0 END", cn, tx))
            {
                cmd.Parameters.Add("@ma", System.Data.SqlDbType.VarChar, 12).Value = ma;
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                    throw new InvalidOperationException(LoiUyQuyenMoTaiKhoanDN);
            }
        }

        private bool ChoPhepMoTaiKhoanDN(string ma, KhachHangData kh = null)
        {
            try
            {
                if (LaNguoiDuocUyQuyenDN(kh)) throw new InvalidOperationException(LoiUyQuyenMoTaiKhoanDN);
                if (!string.IsNullOrWhiteSpace(ma))
                    using (var cn = new SqlConnection(_connStr))
                    {
                        cn.Open();
                        KiemTraQuyenMoTaiKhoanDN(cn, null, ma.Trim());
                    }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể mở tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        // Lưu tài khoản và lịch sử DN trong cùng một giao dịch SQL.
        private bool LuuTaiKhoanMoiDN(KhachHangData kh, string stk, bool tietKiem,
            decimal tien = 0m, double lai = 0, string ky = "")
        {
            try
            {
                var nv = BankSecurity.Current;
                if (nv == null) throw new InvalidOperationException("Chưa đăng nhập.");
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var tr = cn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        KiemTraQuyenMoTaiKhoanDN(cn, tr, kh.CCCD);
                        using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk)
 OR EXISTS(SELECT 1 FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk)
 OR EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk)
 THROW 52001,N'Số tài khoản đã tồn tại.',1;
 IF @laUyQuyen=1 AND EXISTS(SELECT 1 FROM dbo.DN_KhachHang WHERE CCCD=@c AND HoTen<>@n)
 THROW 52003,N'Tên hồ sơ chủ tài khoản đã tồn tại hoặc thay đổi. Vui lòng tải lại dữ liệu.',1;
 IF NOT EXISTS(SELECT 1 FROM dbo.DN_KhachHang WHERE CCCD=@c)
 INSERT dbo.DN_KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo,MaSoThue) VALUES(@c,@n,@p,0,CASE WHEN @laUyQuyen=1 THEN NULL ELSE @c END);", cn, tr))
                        {
                            cmd.Parameters.AddWithValue("@stk", stk);
                            cmd.Parameters.AddWithValue("@c", kh.CCCD);
                            cmd.Parameters.AddWithValue("@laUyQuyen", kh.ChuSoHuuCCCD != null);
                            cmd.Parameters.AddWithValue("@n", kh.HoTen);
                            cmd.Parameters.AddWithValue("@p", kh.SoDienThoai);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand(tietKiem ? @"
 INSERT dbo.DN_SoTietKiem(SoTKTK,CCCD,SoTienGui,LaiSuat,SoTienDaRut,KyHan,TrangThai,NgayMo,NgayCapNhatTT,SoTienGocBanDau,TongTienDaNhan)
 VALUES(@stk,@c,@t,@lai,0,@ky,0,GETDATE(),GETDATE(),@t,0);
 INSERT dbo.DN_LichSuGiaoDichTietKiem(SoTKTK,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,TongTienNhan,GhiChu,NguoiThucHien)
 VALUES(@stk,GETDATE(),N'Mở sổ tiết kiệm',@t,0,0,N'Mở sổ tiết kiệm',@nv);" : @"
 INSERT dbo.DN_TaiKhoanThanhToan(SoTKTT,CCCD,SoDu,TrangThai,NgayMo,NgayCapNhatTT,HanMucNgay,HanMucLan,LoaiTaiKhoan,TenHienThi,LoaiTienTe,SoTienPhongToa)
 VALUES(@stk,@c,0,0,GETDATE(),GETDATE(),1000000000,200000000,N'Doanh nghiệp',@n,N'VND',0);
 INSERT dbo.DN_LichSuGiaoDich(SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
 VALUES(@stk,N'Mở tài khoản doanh nghiệp',0,0,GETDATE(),@nv);", cn, tr))
                        {
                            cmd.Parameters.AddWithValue("@stk", stk);
                            cmd.Parameters.AddWithValue("@c", kh.CCCD);
                            cmd.Parameters.AddWithValue("@n", kh.HoTen);
                            cmd.Parameters.AddWithValue("@t", tien);
                            cmd.Parameters.AddWithValue("@lai", lai);
                            cmd.Parameters.AddWithValue("@ky", ky);
                            cmd.Parameters.AddWithValue("@nv", nv.HoTen);
                            cmd.ExecuteNonQuery();
                        }
                        foreach (var co in kh.DanhSachDongSoHuu)
                            using (var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@chu AND CCCD=@c)
 INSERT dbo.DN_DongSoHuu(CCCD_ChuSoHuu,CCCD,HoTen,SoDienThoai) VALUES(@chu,@c,@n,@p);" +
     (tietKiem ? @"INSERT dbo.DN_DongSoHuuSoTietKiem(SoTKTK,CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p);" :
     @"INSERT dbo.DN_DongSoHuuTaiKhoanThanhToan(SoTKTT,CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p);"), cn, tr))
                            {
                                cmd.Parameters.AddWithValue("@chu", kh.CCCD);
                                cmd.Parameters.AddWithValue("@stk", stk);
                                cmd.Parameters.AddWithValue("@c", co.CCCD);
                                cmd.Parameters.AddWithValue("@n", co.HoTen);
                                cmd.Parameters.AddWithValue("@p", co.SoDienThoai);
                                cmd.ExecuteNonQuery();
                            }
                        tr.Commit();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu tài khoản doanh nghiệp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void MoKhungThemThanhToanTuTimKiem(string mst, IWin32Window owner,
            Func<KhachHangData, string, bool> luuThanhToan)
        {
            if (luuThanhToan == null) throw new ArgumentNullException(nameof(luuThanhToan));
            // Đọc mới để không dùng hồ sơ hoặc liên kết cũ.
            ApDungDuLieuSql(DocDuLieuTuSql());
            var kh = listKhachHang.FirstOrDefault(k => k.CCCD == mst);
            OpenRegistrationDialog(mst ?? "", kh, owner, luuThanhToan);
        }

        // Dùng chính hộp thoại đăng ký của MainDN; form gọi cung cấp phần nghiệp vụ riêng.
        public sealed class KhungMoTaiKhoanTuTimKiemDN
        {
            public Form HopThoai { get; internal set; }
            public Panel NoiDung { get; internal set; }
            public TextBox SoTaiKhoan { get; internal set; }
            public RadioButton SoDep { get; internal set; }
            public string MaBanDau { get; internal set; }
            public Func<KhachHangData> LayKhachHang { get; internal set; }
        }

        private static FrmMainDN TimMainDNChoKhungThemMoi(Control root)
        {
            var main = root as FrmMainDN;
            if (main != null && !main.IsDisposed && !main.Disposing) return main;
            foreach (Control child in root.Controls)
            {
                var found = TimMainDNChoKhungThemMoi(child);
                if (found != null) return found;
            }
            return null;
        }

        public static void MoKhungThemMoiTuTimKiemDN(string mst, IWin32Window owner, string loai,
            Func<bool, string> taoSo, Action<KhungMoTaiKhoanTuTimKiemDN> ganNoiDung)
        {
            if (loai != "Tiết kiệm" && loai != "Vay") throw new ArgumentException("Loại tài khoản không hợp lệ.", nameof(loai));
            if (taoSo == null || ganNoiDung == null) throw new ArgumentNullException("Thiếu nghiệp vụ mở tài khoản.");
            mst = (mst ?? "").Trim();
            if (mst.Length < 10 || mst.Length > 12 || !mst.All(c => c >= '0' && c <= '9'))
                throw new InvalidOperationException("Vui lòng nhập MST hợp lệ từ 10 đến 12 chữ số.");
            FrmMainDN main = null;
            foreach (Form form in Application.OpenForms)
            {
                main = TimMainDNChoKhungThemMoi(form);
                if (main != null) break;
            }
            if (main != null) main.MoKhungThemMoiChuyenNganh(mst, owner, loai, taoSo, ganNoiDung);
            else using (var tam = new FrmMainDN())
                    tam.MoKhungThemMoiChuyenNganh(mst, owner, loai, taoSo, ganNoiDung);
        }

        private void MoKhungThemMoiChuyenNganh(string mst, IWin32Window owner, string loai,
            Func<bool, string> taoSo, Action<KhungMoTaiKhoanTuTimKiemDN> ganNoiDung)
        {
            ApDungDuLieuSql(DocDuLieuTuSql());
            var kh = listKhachHang.FirstOrDefault(k => k.CCCD == mst);
            OpenRegistrationDialog(mst, kh, owner, null, loai, taoSo, ganNoiDung);
        }

        // Chỉ gọi trong transaction của nghiệp vụ mở mới. Không ghi dữ liệu khi hủy hộp thoại.
        public static void DamBaoHoSoMoMoiTuTimKiemDN(SqlConnection cn, SqlTransaction tx, KhachHangData kh)
        {
            if (tx == null) throw new ArgumentNullException(nameof(tx));
            if (kh == null || kh.IsDongSoHuuOnly || string.IsNullOrWhiteSpace(kh.CCCD) ||
                kh.CCCD.Length < 10 || kh.CCCD.Length > 12 || !kh.CCCD.All(c => c >= '0' && c <= '9') ||
                string.IsNullOrWhiteSpace(kh.HoTen) || kh.HoTen.Length > 100 ||
                string.IsNullOrWhiteSpace(kh.SoDienThoai) || kh.SoDienThoai.Length < 10 || kh.SoDienThoai.Length > 11 ||
                !kh.SoDienThoai.All(c => c >= '0' && c <= '9'))
                throw new InvalidOperationException("Thông tin doanh nghiệp không hợp lệ.");
            KiemTraQuyenMoTaiKhoanDN(cn, tx, kh.CCCD);
            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.DN_KhachHang WITH(UPDLOCK,HOLDLOCK)
                WHERE CCCD=@mst AND (HoTen<>@ten OR SoDienThoai<>@sdt))
                THROW 52004,N'Thông tin doanh nghiệp đã thay đổi. Hãy tra cứu lại MST.',1;
                IF NOT EXISTS(SELECT 1 FROM dbo.DN_KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@mst)
                INSERT dbo.DN_KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo,MaSoThue)
                VALUES(@mst,@ten,@sdt,0,@mst);", cn, tx))
            {
                cmd.Parameters.Add("@mst", System.Data.SqlDbType.VarChar, 12).Value = kh.CCCD;
                cmd.Parameters.Add("@ten", System.Data.SqlDbType.NVarChar, 100).Value = kh.HoTen;
                cmd.Parameters.Add("@sdt", System.Data.SqlDbType.VarChar, 11).Value = kh.SoDienThoai;
                cmd.ExecuteNonQuery();
            }
        }

        // Chỉ đổi màu khung đăng ký và các control được tạo động trong khung này.
        private static void DongBoMauKhungThemMoiDN(Form dlg)
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

        private void OpenRegistrationDialog(string initialCCCD, KhachHangData khachHangDaCo = null,
            IWin32Window owner = null, Func<KhachHangData, string, bool> luuThanhToan = null,
            string loaiChuyenNganh = null, Func<bool, string> taoSoChuyenNganh = null,
            Action<KhungMoTaiKhoanTuTimKiemDN> ganNoiDungChuyenNganh = null)
        {
            if (!ChoPhepMoTaiKhoanDN(initialCCCD, khachHangDaCo)) return;
            // Luồng mở tài khoản riêng từ người được ủy quyền đã bị khóa.
            bool moTuNguoiUyQuyen = false;
            bool tuTimThanhToan = luuThanhToan != null;
            bool tuTimChuyenNganh = ganNoiDungChuyenNganh != null;
            using (Form dlg = new Form())
            {
                dlg.Text = khachHangDaCo == null ? "Đăng Ký Khách Hàng & Mở Tài Khoản Mới" : "Thêm Tài Khoản Cho Khách Hàng";
                dlg.Size = new Size(tuTimChuyenNganh && loaiChuyenNganh == "Vay" ? 760 : 620, 900);
                if (tuTimChuyenNganh)
                    dlg.Height = Math.Min(dlg.Height, Math.Max(320, Screen.PrimaryScreen.WorkingArea.Height - 40));
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label lblCCCD = CreateLabelDesign2();
                bool laDongSoHuu = khachHangDaCo != null && (khachHangDaCo.IsDongSoHuuOnly || listKhachHang.Any(k => k.CCCD != khachHangDaCo.CCCD && k.DanhSachDongSoHuu.Any(d => d.CCCD == khachHangDaCo.CCCD)));
                string nhanDinhDanh = laDongSoHuu ? "CCCD" : "MST";
                lblCCCD.Text = nhanDinhDanh + ":";
                TextBox txtCCCD = CreateTextBoxDynamicDesign1(initialCCCD, khachHangDaCo != null || tuTimChuyenNganh);
                txtCCCD.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                Label lblHoTen = CreateLabelDesign3();
                if (tuTimThanhToan || tuTimChuyenNganh) lblHoTen.Text = "Tên doanh nghiệp:";
                TextBox txtHoTen = CreateTextBoxDynamicDesign2(khachHangDaCo?.HoTen ?? "", khachHangDaCo != null);
                Label lblSĐT = CreateLabelDesign4();
                TextBox txtSĐT = CreateTextBoxDynamicDesign3(khachHangDaCo?.SoDienThoai ?? "", khachHangDaCo != null);
                if (tuTimChuyenNganh) { txtHoTen.MaxLength = 100; txtSĐT.MaxLength = 11; }
                txtSĐT.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);

                // Tên doanh nghiệp chỉ nhập khi tạo hồ sơ chủ tài khoản riêng.
                Label lblTenDoanhNghiepMoi = new Label { Text = "Tên doanh nghiệp:", Location = new Point(20, 125), AutoSize = true, Visible = moTuNguoiUyQuyen };
                TextBox txtTenDoanhNghiepMoi = new TextBox
                {
                    Location = new Point(150, 123),
                    Width = 425,
                    MaxLength = 100,
                    Visible = moTuNguoiUyQuyen,
                    ReadOnly = moTuNguoiUyQuyen && !khachHangDaCo.IsDongSoHuuOnly,
                    Text = moTuNguoiUyQuyen && !khachHangDaCo.IsDongSoHuuOnly ? khachHangDaCo.HoTen : ""
                };
                if (moTuNguoiUyQuyen)
                {
                    lblHoTen.Text = "Họ tên cá nhân:";
                    var lienKet = listKhachHang.SelectMany(k => k.DanhSachDongSoHuu).FirstOrDefault(d => d.CCCD == initialCCCD);
                    if (lienKet != null) txtHoTen.Text = lienKet.HoTen;
                }

                Button btnThemDongSoHuu = CreateButtonDesign5();
                btnThemDongSoHuu.Text = "Thêm người được ủy quyền";
                btnThemDongSoHuu.FlatAppearance.BorderColor = colorActiveBlue;
                NumericUpDown nudSoLuongDongSoHuu = CreateNumericUpDownDesign6();
                if (tuTimChuyenNganh)
                {
                    nudSoLuongDongSoHuu.Maximum = loaiChuyenNganh == "Tiết kiệm" ? 2 : 3;
                    btnThemDongSoHuu.Width = 190;
                }
                Panel pnlDongSoHuu = CreatePanelDesign7();
                if (moTuNguoiUyQuyen)
                {
                    nudSoLuongDongSoHuu.Top += 40;
                    pnlDongSoHuu.Top += 40;
                }
                dlg.AutoScroll = true;
                List<TextBox> dsCCCDDongSoHuu = new List<TextBox>();
                List<TextBox> dsHoTenDongSoHuu = new List<TextBox>();
                List<TextBox> dsSDTDongSoHuu = new List<TextBox>();

                Action RenderDongSoHuu = () =>
                {
                    var daNhap = tuTimThanhToan || tuTimChuyenNganh
                        ? dsCCCDDongSoHuu.Select((t, i) => new DongSoHuuData { CCCD = t.Text, HoTen = dsHoTenDongSoHuu[i].Text, SoDienThoai = dsSDTDongSoHuu[i].Text }).ToList()
                        : new List<DongSoHuuData>();
                    pnlDongSoHuu.Controls.Clear(); dsCCCDDongSoHuu.Clear(); dsHoTenDongSoHuu.Clear(); dsSDTDongSoHuu.Clear();
                    for (int i = 0; i < (int)nudSoLuongDongSoHuu.Value; i++)
                    {
                        GroupBox group = CreateGroupBoxDynamicDesign4($"Người được ủy quyền {i + 1}", new Point(8, 8 + i * 64));
                        Label l1 = CreateLabelDesign8();
                        TextBox t1 = CreateTextBoxDesign9(); t1.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                        Label l2 = CreateLabelDesign10();
                        TextBox t2 = CreateTextBoxDesign11();
                        Label l3 = CreateLabelDesign12();
                        TextBox t3 = CreateTextBoxDesign13(); t3.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                        group.Controls.AddRange(new Control[] { l1, t1, l2, t2, l3, t3 }); pnlDongSoHuu.Controls.Add(group);
                        dsCCCDDongSoHuu.Add(t1); dsHoTenDongSoHuu.Add(t2); dsSDTDongSoHuu.Add(t3);
                        if (i < daNhap.Count) { t1.Text = daNhap[i].CCCD; t2.Text = daNhap[i].HoTen; t3.Text = daNhap[i].SoDienThoai; }
                    }
                };
                btnThemDongSoHuu.Click += (s, ev) => { nudSoLuongDongSoHuu.Visible = true; pnlDongSoHuu.Visible = true; RenderDongSoHuu(); };
                nudSoLuongDongSoHuu.ValueChanged += (s, ev) => { pnlDongSoHuu.Visible = true; RenderDongSoHuu(); };

                Label lblLoaiTK = CreateLabelDesign14();
                ComboBox cboLoaiTK = CreateComboBoxDesign15(); cboLoaiTK.Items.AddRange(new object[] { "Thanh toán", "Vay", "Tiết kiệm" }); cboLoaiTK.SelectedIndex = 0;
                if (tuTimThanhToan) cboLoaiTK.Enabled = false;
                if (tuTimChuyenNganh) { cboLoaiTK.SelectedItem = loaiChuyenNganh; cboLoaiTK.Enabled = false; }
                RadioButton rdoNgauNhien = CreateRadioButtonDesign16();
                RadioButton rdoSoDep = CreateRadioButtonDesign17();
                Label lblPhiSoDep = CreateLabelDesign18();
                Label lblSTK = CreateLabelDesign19();
                TextBox txtSTK = CreateTextBoxDesign20();
                Panel pnlSubForm = CreatePanelDesign21();
                if (tuTimChuyenNganh && loaiChuyenNganh == "Vay") pnlSubForm.Width = 696;
                if (tuTimThanhToan) { rdoSoDep.Enabled = false; rdoNgauNhien.Checked = true; }
                Action genSTK = () => { if (tuTimChuyenNganh) { txtSTK.Text = taoSoChuyenNganh(rdoSoDep.Checked) ?? ""; return; } if (tuTimThanhToan) { txtSTK.Text = "DNTT" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(); return; } Random r = new Random(); txtSTK.Text = rdoSoDep.Checked ? "88889999" + r.Next(10, 99) : "10" + r.Next(10000000, 99999999); };
                rdoSoDep.CheckedChanged += (s, ev) => { lblPhiSoDep.Visible = rdoSoDep.Checked; genSTK(); }; rdoNgauNhien.CheckedChanged += (s, ev) => genSTK(); genSTK();

                Action CapNhatViTri = () =>
                {
                    int offset = (pnlDongSoHuu.Visible ? 220 : 0) + (moTuNguoiUyQuyen ? 40 : 0);
                    lblLoaiTK.Location = new Point(20, 160 + offset); cboLoaiTK.Location = new Point(150, 158 + offset); rdoNgauNhien.Location = new Point(20, 195 + offset); rdoSoDep.Location = new Point(180, 195 + offset); lblPhiSoDep.Location = new Point(340, 197 + offset); lblSTK.Location = new Point(20, 230 + offset); txtSTK.Location = new Point(150, 228 + offset); pnlSubForm.Location = new Point(20, 265 + offset);
                };
                pnlDongSoHuu.VisibleChanged += (s, ev) => CapNhatViTri();
                if (tuTimChuyenNganh)
                {
                    Action capNhatCuon = () => dlg.AutoScrollMinSize = new Size(0, pnlSubForm.Bottom + 45);
                    pnlSubForm.SizeChanged += (s, ev) => capNhatCuon();
                    pnlSubForm.LocationChanged += (s, ev) => capNhatCuon();
                    dlg.Shown += (s, ev) => { CapNhatViTri(); capNhatCuon(); };
                }

                KhachHangData newKH = null;
                Func<bool> KiemTra = () =>
                {
                    if (!ChoPhepMoTaiKhoanDN(txtCCCD.Text.Trim(), khachHangDaCo)) return false;
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
                    if (tuTimThanhToan) return true; // Nghiệp vụ kiểm tra và lưu do FrmThanhToanDN thực hiện.
                    if (tuTimChuyenNganh)
                    {
                        string ma = txtCCCD.Text.Trim(), ten = txtHoTen.Text.Trim(), sdtDN = txtSĐT.Text.Trim();
                        if (ma.Length < 10 || ma.Length > 12 || !ma.All(c => c >= '0' && c <= '9') ||
                            string.IsNullOrWhiteSpace(ten) || ten.Length > 100 || sdtDN.Length < 10 || sdtDN.Length > 11 ||
                            !sdtDN.All(c => c >= '0' && c <= '9'))
                        {
                            MessageBox.Show(dlg, "Vui lòng nhập MST, tên doanh nghiệp và số điện thoại hợp lệ.", "Kiểm tra thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                        return true; // Các điều kiện riêng và transaction do form gọi thực hiện.
                    }
                    if (moTuNguoiUyQuyen && (string.IsNullOrWhiteSpace(txtTenDoanhNghiepMoi.Text) || txtTenDoanhNghiepMoi.Text.Trim().Length > 100))
                    { MessageBox.Show("Vui lòng nhập tên doanh nghiệp (tối đa 100 ký tự).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtTenDoanhNghiepMoi.Focus(); return false; }
                    if (txtCCCD.Text.Length != 12 || string.IsNullOrWhiteSpace(txtHoTen.Text)) { MessageBox.Show($"Vui lòng nhập đủ 12 số {nhanDinhDanh} và Họ tên hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    if (txtSĐT.Text.Length < 10 || txtSĐT.Text.Length > 11) { MessageBox.Show("Số điện thoại phải bao gồm từ 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    HashSet<string> cccd = new HashSet<string>(StringComparer.OrdinalIgnoreCase), sdt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KhachHangData kh in listKhachHang) { cccd.Add(kh.CCCD ?? ""); sdt.Add(kh.SoDienThoai ?? ""); foreach (DongSoHuuData ds in kh.DanhSachDongSoHuu) { cccd.Add(ds.CCCD ?? ""); sdt.Add(ds.SoDienThoai ?? ""); } }
                    if (khachHangDaCo == null)
                    {
                        if (cccd.Contains(txtCCCD.Text.Trim())) { MessageBox.Show($"{nhanDinhDanh} của khách hàng đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (sdt.Contains(txtSĐT.Text.Trim())) { MessageBox.Show("Số điện thoại của khách hàng đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    }
                    cccd.Add(txtCCCD.Text.Trim());
                    sdt.Add(txtSĐT.Text.Trim());
                    for (int i = 0; i < dsCCCDDongSoHuu.Count; i++)
                    {
                        string c = dsCCCDDongSoHuu[i].Text.Trim(), n = dsHoTenDongSoHuu[i].Text.Trim(), p = dsSDTDongSoHuu[i].Text.Trim();
                        if (c.Length != 12 || string.IsNullOrWhiteSpace(n)) { MessageBox.Show($"Vui lòng nhập đúng CCCD 12 số và Họ tên cho Người được ủy quyền {i + 1}!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (p.Length < 10 || p.Length > 11) { MessageBox.Show($"Số điện thoại của Người được ủy quyền {i + 1} phải gồm 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (!cccd.Add(c)) { MessageBox.Show($"CCCD của Người được ủy quyền {i + 1} đã tồn tại hoặc bị trùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (!sdt.Add(p)) { MessageBox.Show($"Số điện thoại của Người được ủy quyền {i + 1} đã tồn tại hoặc bị trùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    }
                    return true;
                };
                Action TaoKhachHang = () =>
                {
                    newKH = new KhachHangData { CCCD = txtCCCD.Text.Trim(), HoTen = txtHoTen.Text.Trim(), SoDienThoai = txtSĐT.Text.Trim() };
                    newKH.IsDongSoHuuOnly = false;
                    if (moTuNguoiUyQuyen) newKH.ChuSoHuuCCCD = khachHangDaCo.ChuSoHuuCCCD ?? "";
                    if (khachHangDaCo != null)
                    {
                        newKH.CCCD = txtCCCD.Text.Trim();
                        newKH.HoTen = txtHoTen.Text.Trim();
                        newKH.SoDienThoai = txtSĐT.Text.Trim();
                    }
                    if (moTuNguoiUyQuyen) newKH.HoTen = txtTenDoanhNghiepMoi.Text.Trim();
                    for (int i = 0; i < dsCCCDDongSoHuu.Count; i++)
                        newKH.DanhSachDongSoHuu.Add(new DongSoHuuData { CCCD = dsCCCDDongSoHuu[i].Text.Trim(), HoTen = dsHoTenDongSoHuu[i].Text.Trim(), SoDienThoai = dsSDTDongSoHuu[i].Text.Trim(), CCCD_ChuSoHuu = newKH.CCCD });
                };

                Action renderSubForm = () =>
                {
                    pnlSubForm.Controls.Clear(); string selectedType = cboLoaiTK.SelectedItem.ToString();
                    if (tuTimChuyenNganh)
                    {
                        ganNoiDungChuyenNganh(new KhungMoTaiKhoanTuTimKiemDN
                        {
                            HopThoai = dlg,
                            NoiDung = pnlSubForm,
                            SoTaiKhoan = txtSTK,
                            SoDep = rdoSoDep,
                            MaBanDau = initialCCCD,
                            LayKhachHang = () => { if (!KiemTra()) return null; TaoKhachHang(); return newKH; }
                        });
                        return;
                    }
                    if (selectedType == "Thanh toán")
                    {
                        Button btnSubmit = CreateButtonDesign22();
                        btnSubmit.Click += (s, ev) => { if (!KiemTra()) return; TaoKhachHang(); if (!(tuTimThanhToan ? luuThanhToan(newKH, txtSTK.Text) : LuuTaiKhoanMoiDN(newKH, txtSTK.Text, false))) return; newKH.DanhSachThanhToan.Add(new TaiKhoanThanhToanItem { SoTKTT = txtSTK.Text, SoDu = 0 }); MessageBox.Show($"Mở tài khoản thanh toán thành công với STK: {txtSTK.Text}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); dlg.DialogResult = DialogResult.OK; }; pnlSubForm.Controls.Add(btnSubmit);
                    }
                    else if (selectedType == "Vay")
                    {
                        RadioButton rdoTinChap = CreateRadioButtonDesign23();
                        RadioButton rdoTheChap = CreateRadioButtonDesign24();
                        Panel pnlVayBody = CreatePanelDesign25();

                        int savedNhomNoIndex = LayNhomNoHienTai(_connStr, txtCCCD.Text.Trim()) - 1;

                        Action loadVayBody = () =>
                        {
                            pnlVayBody.Controls.Clear();
                            bool isTinChap = rdoTinChap.Checked;
                            ComboBox cboLoaiTheChap = CreateComboBoxDesign26();
                            cboLoaiTheChap.Items.AddRange(new object[] { "Tài sản cố định", "Sổ tiết kiệm trong ngân hàng", "Sổ tiết kiệm ngoài ngân hàng" });
                            cboLoaiTheChap.SelectedIndex = 0;
                            ComboBox cboSoNoiBo = CreateComboBoxDesign27();
                            Label lblSo = CreateLabelDesign28();
                            TextBox txtSoNgoai = CreateTextBoxDesign29();
                            TextBox txtGiaTriNgoai = CreateTextBoxDesign30();
                            TextBox txtLaiNgoai = CreateTextBoxDesign31();
                            Label lblGiaTri = CreateLabelDesign32();
                            Label lblLai = CreateLabelDesign33();
                            Label lblGiaTriThongTin = CreateLabelDesign34();
                            try { cboSoNoiBo.Items.AddRange(LaySoTietKiemTheChap(_connStr, txtCCCD.Text.Trim()).Cast<object>().ToArray()); }
                            catch (Exception ex) { MessageBox.Show("Không tải được sổ tiết kiệm: " + ex.Message); }
                            if (cboSoNoiBo.Items.Count > 0) cboSoNoiBo.SelectedIndex = 0;
                            decimal maxLoan = 0;
                            Label lblMax = CreateLabelDesign35();
                            TextBox txtLuong = CreateTextBoxDesign36();
                            TextBox txtTS = CreateTextBoxDesign37();
                            TextBox txtDG = CreateTextBoxDesign38();
                            ComboBox cboNhomNo = CreateComboBoxDynamicDesign5(new Point(180, isTinChap ? 42 : 74));

                            cboNhomNo.Items.AddRange(new object[] { "Nhóm 1 (Đủ tiêu chuẩn)", "Nhóm 2 (Cần chú ý)", "Nhóm 3 (Dưới tiêu chuẩn)", "Nhóm 4 (Nghi ngờ)", "Nhóm 5 (Mất vốn)" });

                            cboNhomNo.Enabled = false;
                            if (savedNhomNoIndex >= 0 && savedNhomNoIndex < cboNhomNo.Items.Count)
                            {
                                cboNhomNo.SelectedIndex = savedNhomNoIndex;
                            }
                            else
                            {
                                cboNhomNo.SelectedIndex = 0;
                            }

                            Action calcMax = () =>
                            {

                                if (cboNhomNo.SelectedIndex >= 2)
                                {
                                    maxLoan = 0;
                                    lblMax.ForeColor = Color.Red;
                                    lblMax.Text = "Khách hàng thuộc nhóm nợ 3-5: Không đủ điều kiện mở khoản vay mới!";
                                    return;
                                }

                                lblMax.ForeColor = Color.Blue;
                                double h = cboNhomNo.SelectedIndex == 0 ? 1.0 : 0.8;
                                decimal noTruocDo = 0;
                                if (isTinChap)
                                {
                                    decimal.TryParse(txtLuong.Text, out decimal luong);
                                    maxLoan = Math.Max(0, luong * 10 * (decimal)h - noTruocDo);
                                }
                                else
                                {
                                    int type = cboLoaiTheChap.SelectedIndex;
                                    if (type == 1)
                                    {
                                        var so = cboSoNoiBo.SelectedItem as SoTietKiemTheChap;
                                        maxLoan = so == null ? 0m : Math.Floor(so.TongGiaTri * .95m);
                                        lblGiaTriThongTin.Text = so == null ? "Không có sổ hợp lệ" : $"Gốc: {so.GocConLai:N0} | Lãi: {so.LaiTamTinh:N0} | Tổng: {so.TongGiaTri:N0}";
                                    }
                                    else if (type == 2)
                                    {
                                        decimal.TryParse(txtGiaTriNgoai.Text, out decimal gocNgoai);
                                        decimal.TryParse(txtLaiNgoai.Text, out decimal laiNgoai);
                                        maxLoan = Math.Floor(Math.Max(0, gocNgoai + laiNgoai) * .85m);
                                    }
                                    else
                                    {
                                        decimal.TryParse(txtDG.Text, out decimal dg);
                                        maxLoan = Math.Max(0, dg * 0.7m * (decimal)h - noTruocDo);
                                    }
                                }
                                lblMax.Text = $"Số tiền tối đa có thể vay: {maxLoan:N0} VNĐ";
                            };

                            cboNhomNo.SelectedIndexChanged += (s, ev) => calcMax();
                            txtLuong.TextChanged += (s, ev) => calcMax();
                            txtDG.TextChanged += (s, ev) => calcMax();

                            if (isTinChap)
                            {
                                pnlVayBody.Controls.Add(CreateLabelDesign39());
                                pnlVayBody.Controls.Add(txtLuong);
                                pnlVayBody.Controls.Add(CreateLabelDesign40());
                                pnlVayBody.Controls.Add(cboNhomNo);
                            }
                            else
                            {
                                pnlVayBody.Controls.Add(CreateLabelDesign41());
                                pnlVayBody.Controls.Add(cboLoaiTheChap);
                                pnlVayBody.Controls.Add(lblSo);
                                pnlVayBody.Controls.Add(cboSoNoiBo);
                                pnlVayBody.Controls.Add(txtSoNgoai);
                                pnlVayBody.Controls.Add(txtGiaTriNgoai);
                                pnlVayBody.Controls.Add(txtLaiNgoai);
                                pnlVayBody.Controls.Add(lblGiaTri);
                                pnlVayBody.Controls.Add(lblLai);
                                pnlVayBody.Controls.Add(lblGiaTriThongTin);
                                pnlVayBody.Controls.Add(CreateLabelDesign42());
                                pnlVayBody.Controls.Add(txtTS);
                                pnlVayBody.Controls.Add(CreateLabelDesign43());
                                pnlVayBody.Controls.Add(txtDG);
                                pnlVayBody.Controls.Add(CreateLabelDesign44());
                                cboNhomNo.Location = new Point(180, 164);
                                pnlVayBody.Controls.Add(cboNhomNo);
                                Action switchLoai = () =>
                                {
                                    bool ts = cboLoaiTheChap.SelectedIndex == 0;
                                    bool noi = cboLoaiTheChap.SelectedIndex == 1;
                                    bool ngoai = cboLoaiTheChap.SelectedIndex == 2;
                                    txtTS.Visible = txtDG.Visible = ts;
                                    pnlVayBody.Controls.Find("lblTenTS", false)[0].Visible = ts;
                                    pnlVayBody.Controls.Find("lblDinhGiaTS", false)[0].Visible = ts;
                                    lblSo.Visible = noi || ngoai;
                                    lblSo.Text = noi ? "Chọn sổ tiết kiệm:" : "Số sổ bên ngoài:";
                                    cboSoNoiBo.Visible = noi;
                                    txtSoNgoai.Visible = ngoai;
                                    txtGiaTriNgoai.Visible = txtLaiNgoai.Visible = lblGiaTri.Visible = lblLai.Visible = ngoai;
                                    lblGiaTriThongTin.Visible = noi;
                                    calcMax();
                                };
                                cboLoaiTheChap.SelectedIndexChanged += (s, ev) => switchLoai();
                                cboSoNoiBo.SelectedIndexChanged += (s, ev) => calcMax();
                                txtGiaTriNgoai.TextChanged += (s, ev) => calcMax();
                                txtLaiNgoai.TextChanged += (s, ev) => calcMax();
                                switchLoai();
                            }

                            pnlVayBody.Controls.Add(lblMax);
                            lblMax.Location = new Point(10, isTinChap ? 105 : 196);
                            int curY = isTinChap ? 135 : 225;
                            pnlVayBody.AutoScrollMinSize = new Size(0, isTinChap ? 290 : 370);
                            pnlVayBody.Controls.Add(CreateLabelDynamicDesign6(new Point(10, curY)));
                            TextBox txtSoTienVay = CreateTextBoxDynamicDesign7(new Point(180, curY - 2));
                            pnlVayBody.Controls.Add(txtSoTienVay);
                            pnlVayBody.Controls.Add(CreateLabelDynamicDesign8(new Point(10, curY + 32)));
                            ComboBox cboKH = CreateComboBoxDynamicDesign9(new Point(180, curY + 30));

                            if (isTinChap)
                                cboKH.Items.AddRange(new object[] { "6 tháng: 10.5%", "12 tháng: 12.0%", "18 tháng: Từ 13.5%", "24 tháng: Từ 14.5%", "36 tháng: Từ 16.0%" });
                            else
                                cboKH.Items.AddRange(new object[] { "6 tháng cố định: Từ 5.0%", "12 tháng cố định: Từ 5.5%", "18 tháng cố định: Từ 6.8%", "24 tháng cố định: Từ 7.0%", "36 tháng cố định: Từ 7.9%" });

                            cboKH.SelectedIndex = 0;
                            pnlVayBody.Controls.Add(cboKH);

                            Button btnOk = CreateButtonDynamicDesign10(new Point(110, curY + 75));
                            Button btnCancel = CreateButtonDynamicDesign11(new Point(230, curY + 75));

                            btnOk.Click += (s, ev) =>
                            {
                                if (!string.IsNullOrWhiteSpace(txtCCCD.Text) &&
                                    !FrmMain.ChoPhepMoVayTheoNhomNo(_connStr, true, txtCCCD.Text.Trim())) return;
                                if (cboNhomNo.SelectedIndex >= 2)
                                {
                                    MessageBox.Show("Khách hàng có khoản vay thuộc nhóm nợ 3-5 không thể mở thêm khoản vay mới!", "Cảnh báo ngắt giao dịch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                if (!decimal.TryParse(txtSoTienVay.Text, out decimal valVay) || valVay <= 0 || valVay > maxLoan)
                                {
                                    MessageBox.Show("Số tiền vay không hợp lệ hoặc vượt quá hạn mức tối đa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                if (!isTinChap && cboLoaiTheChap.SelectedIndex == 1 && cboSoNoiBo.SelectedItem == null)
                                { MessageBox.Show("Khách hàng không có sổ tiết kiệm nội bộ hợp lệ."); return; }
                                if (!isTinChap && cboLoaiTheChap.SelectedIndex == 2 &&
                                    (string.IsNullOrWhiteSpace(txtSoNgoai.Text) || maxLoan <= 0))
                                { MessageBox.Show("Nhập số sổ và giá trị gốc, lãi sổ ngoài ngân hàng."); return; }
                                if (!KiemTra()) return;

                                var nv = BankSecurity.Current;
                                if (nv == null) { MessageBox.Show("Chưa đăng nhập."); return; }
                                TaoKhachHang();
                                double[] ratesTin = { 10.5, 12.0, 13.5, 14.5, 16.0 }, ratesThe = { 5.0, 5.5, 6.8, 7.0, 7.9 };
                                double rate = isTinChap ? ratesTin[cboKH.SelectedIndex] :
                                    cboLoaiTheChap.SelectedIndex == 1 ? 7.0 : cboLoaiTheChap.SelectedIndex == 2 ? 8.5 : ratesThe[cboKH.SelectedIndex];
                                string khStr = (cboKH.SelectedIndex == 0 ? "6" : cboKH.SelectedIndex == 1 ? "12" : cboKH.SelectedIndex == 2 ? "18" : cboKH.SelectedIndex == 3 ? "24" : "36") + " tháng";

                                string moTaTheChap = isTinChap ? "Vay tín chấp" :
                                    cboLoaiTheChap.SelectedIndex == 1 ? "Vay thế chấp sổ nội bộ: " + ((SoTietKiemTheChap)cboSoNoiBo.SelectedItem).SoTKTK :
                                    cboLoaiTheChap.SelectedIndex == 2 ? "Vay thế chấp sổ ngoài: " + txtSoNgoai.Text.Trim() :
                                    "Vay thế chấp bất động sản: " + txtTS.Text.Trim();
                                try
                                {
                                    using (var cn = new SqlConnection(_connStr))
                                    {
                                        cn.Open();
                                        using (var tr = cn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                                        {
                                            try
                                            {
                                                KiemTraQuyenMoTaiKhoanDN(cn, tr, newKH.CCCD);
                                                using (var check = new SqlCommand(@"SELECT
                                                    (SELECT COUNT(*) FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk) +
                                                    (SELECT COUNT(*) FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk) +
                                                    (SELECT COUNT(*) FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk)", cn, tr))
                                                {
                                                    check.Parameters.Add("@stk", System.Data.SqlDbType.VarChar, 20).Value = txtSTK.Text;
                                                    if (Convert.ToInt32(check.ExecuteScalar()) != 0)
                                                        throw new InvalidOperationException("Số tài khoản vừa tạo đã tồn tại. Vui lòng đổi số và thử lại.");
                                                }
                                                {
                                                    using (var insertKH = new SqlCommand(@"IF @laUyQuyen=1 AND EXISTS(SELECT 1 FROM dbo.DN_KhachHang WHERE CCCD=@cccd AND HoTen<>@ten)
                                                        THROW 52003,N'Tên hồ sơ chủ tài khoản đã tồn tại hoặc thay đổi. Vui lòng tải lại dữ liệu.',1;
                                                        IF NOT EXISTS (SELECT 1 FROM dbo.DN_KhachHang WHERE CCCD=@cccd)
                                                        INSERT INTO dbo.DN_KhachHang
                                                        (CCCD, HoTen, SoDienThoai, NoTruocDo, MaSoThue)
                                                        VALUES (@cccd,@ten,@sdt,0,CASE WHEN @laUyQuyen=1 THEN NULL ELSE @cccd END)", cn, tr))
                                                    {
                                                        insertKH.Parameters.AddWithValue("@laUyQuyen", moTuNguoiUyQuyen);
                                                        insertKH.Parameters.Add("@cccd", System.Data.SqlDbType.VarChar, 12).Value = newKH.CCCD;
                                                        insertKH.Parameters.Add("@ten", System.Data.SqlDbType.NVarChar, 100).Value = newKH.HoTen;
                                                        insertKH.Parameters.Add("@sdt", System.Data.SqlDbType.VarChar, 11).Value = newKH.SoDienThoai;
                                                        insertKH.ExecuteNonQuery();
                                                    }
                                                }
                                                FrmMain.KiemTraNhomNoTruocMoVay(cn, tr, true, newKH.CCCD);
                                                if (!isTinChap && cboLoaiTheChap.SelectedIndex > 0)
                                                    FrmMain.KiemTraSoTietKiemChuaVay(cn, tr, true, cboLoaiTheChap.SelectedIndex == 2,
                                                        cboLoaiTheChap.SelectedIndex == 1 ? ((SoTietKiemTheChap)cboSoNoiBo.SelectedItem).SoTKTK : txtSoNgoai.Text.Trim());
                                                using (var insertVay = new SqlCommand(@"INSERT INTO dbo.DN_TaiKhoanVay
                                                    (SoTKVay, CCCD, SoTienVay, SoTienDaTra, LaiSuat, KyHan, TrangThai, NgayMo)
                                                    VALUES (@stk,@cccd,@tien,0,@lai,@kyhan,0,GETDATE())", cn, tr))
                                                {
                                                    insertVay.Parameters.Add("@stk", System.Data.SqlDbType.VarChar, 20).Value = txtSTK.Text;
                                                    insertVay.Parameters.Add("@cccd", System.Data.SqlDbType.VarChar, 12).Value = newKH.CCCD;
                                                    insertVay.Parameters.Add("@tien", System.Data.SqlDbType.Decimal).Value = valVay;
                                                    insertVay.Parameters.Add("@lai", System.Data.SqlDbType.Float).Value = rate;
                                                    insertVay.Parameters.Add("@kyhan", System.Data.SqlDbType.NVarChar, 20).Value = khStr;
                                                    insertVay.ExecuteNonQuery();
                                                }
                                                foreach (var co in dsCCCDDongSoHuu.Select((x, i) => new { C = x.Text.Trim(), N = dsHoTenDongSoHuu[i].Text.Trim(), P = dsSDTDongSoHuu[i].Text.Trim() }))
                                                {
                                                    using (var insertCo = new SqlCommand(@"INSERT INTO dbo.DN_DongSoHuu
                                                        (CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai)
                                                        VALUES (@chu,@c,@n,@p)", cn, tr))
                                                    {
                                                        insertCo.Parameters.Add("@chu", System.Data.SqlDbType.VarChar, 12).Value = newKH.CCCD;
                                                        insertCo.Parameters.Add("@c", System.Data.SqlDbType.VarChar, 12).Value = co.C;
                                                        insertCo.Parameters.Add("@n", System.Data.SqlDbType.NVarChar, 100).Value = co.N;
                                                        insertCo.Parameters.Add("@p", System.Data.SqlDbType.VarChar, 11).Value = co.P;
                                                        insertCo.ExecuteNonQuery();
                                                    }
                                                    using (var link = new SqlCommand(@"INSERT dbo.DN_DongSoHuuTaiKhoanVay(SoTKVay,CCCD,HoTen,SoDienThoai)
                                                        VALUES(@stk,@c,@n,@p)", cn, tr))
                                                    {
                                                        link.Parameters.AddWithValue("@stk", txtSTK.Text);
                                                        link.Parameters.AddWithValue("@c", co.C);
                                                        link.Parameters.AddWithValue("@n", co.N);
                                                        link.Parameters.AddWithValue("@p", co.P);
                                                        link.ExecuteNonQuery();
                                                    }
                                                }
                                                using (var history = new SqlCommand(@"INSERT INTO dbo.DN_LichSuGiaoDichVay
                                                    (SoTKVay, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, TongTien, SoDuConLai, GhiChu, NguoiThucHien)
                                                    VALUES (@stk, GETDATE(), N'Giải ngân', @tien, 0, @tien, @tien, @note, @nv)", cn, tr))
                                                {
                                                    history.Parameters.Add("@stk", System.Data.SqlDbType.VarChar, 20).Value = txtSTK.Text;
                                                    history.Parameters.Add("@tien", System.Data.SqlDbType.Decimal).Value = valVay;
                                                    history.Parameters.Add("@note", System.Data.SqlDbType.NVarChar, 500).Value = moTaTheChap + "; hạn mức " + maxLoan.ToString("N0") + "; lãi vay " + rate + "%/năm";
                                                    history.Parameters.Add("@nv", System.Data.SqlDbType.NVarChar, 100).Value = nv.HoTen;
                                                    history.ExecuteNonQuery();
                                                }
                                                tr.Commit();
                                            }
                                            catch { tr.Rollback(); throw; }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Không thể mở khoản vay trên SQL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                newKH.DanhSachVay.Add(new TaiKhoanVayItem { SoTKVay = txtSTK.Text, SoTienVay = valVay, SoTienDaTra = 0, LaiSuat = rate, KyHan = khStr });
                                MessageBox.Show("Đã mở tài khoản vay thành công.\n" + moTaTheChap, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                dlg.DialogResult = DialogResult.OK;
                            };

                            btnCancel.Click += (s, ev) => dlg.Close();
                            pnlVayBody.Controls.AddRange(new Control[] { btnOk, btnCancel });
                            calcMax();
                        };

                        rdoTinChap.CheckedChanged += (s, ev) => loadVayBody();
                        rdoTheChap.CheckedChanged += (s, ev) => loadVayBody();
                        pnlSubForm.Controls.AddRange(new Control[] { rdoTinChap, rdoTheChap, pnlVayBody });
                        loadVayBody();
                    }
                    else
                    {
                        pnlSubForm.Controls.Add(CreateLabelDesign45()); TextBox txtGui = CreateTextBoxDesign46(); pnlSubForm.Controls.Add(txtGui); pnlSubForm.Controls.Add(CreateLabelDesign47()); ComboBox cboKH = CreateComboBoxDesign48(); cboKH.Items.AddRange(new object[] { "Kỳ hạn 6 tháng : 6.60%/năm", "Kỳ hạn 12 tháng : 6.80%/năm", "Kỳ hạn 18 tháng : 6.90%/năm", "Kỳ hạn 24 tháng : 7.00%/năm", "Kỳ hạn 36 tháng : 7.10%/năm" }); cboKH.SelectedIndex = 0; pnlSubForm.Controls.Add(cboKH); pnlSubForm.Controls.Add(CreateLabelDesign49());
                        Button btnOk = CreateButtonDesign50(); Button btnCancel = CreateButtonDesign51();
                        btnOk.Click += (s, ev) => { if (!decimal.TryParse(txtGui.Text, out decimal valGui) || valGui <= 0) { MessageBox.Show("Số tiền gửi không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } if (!KiemTra()) return; TaoKhachHang(); double[] rates = { 6.60, 6.80, 6.90, 7.00, 7.10 }; string[] khs = { "6 tháng", "12 tháng", "18 tháng", "24 tháng", "36 tháng" }; if (!LuuTaiKhoanMoiDN(newKH, txtSTK.Text, true, valGui, rates[cboKH.SelectedIndex], khs[cboKH.SelectedIndex])) return; newKH.DanhSachTietKiem.Add(new SoTietKiemItem { SoTKTK = txtSTK.Text, SoTienGui = valGui, LaiSuat = rates[cboKH.SelectedIndex], SoTienDaRut = 0, KyHan = khs[cboKH.SelectedIndex] }); MessageBox.Show("Đã mở tài khoản tiết kiệm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); dlg.DialogResult = DialogResult.OK; }; btnCancel.Click += (s, ev) => dlg.Close(); pnlSubForm.Controls.AddRange(new Control[] { btnOk, btnCancel });
                    }
                };

                cboLoaiTK.SelectedIndexChanged += (s, ev) => renderSubForm();
                dlg.Controls.AddRange(new Control[] { lblCCCD, txtCCCD, lblHoTen, txtHoTen, lblSĐT, txtSĐT, lblTenDoanhNghiepMoi, txtTenDoanhNghiepMoi, btnThemDongSoHuu, nudSoLuongDongSoHuu, pnlDongSoHuu, lblLoaiTK, cboLoaiTK, rdoNgauNhien, rdoSoDep, lblPhiSoDep, lblSTK, txtSTK, pnlSubForm });
                if (tuTimThanhToan)
                {
                    var repsCu = khachHangDaCo == null ? new List<DongSoHuuData>() : khachHangDaCo.DanhSachDongSoHuu;
                    nudSoLuongDongSoHuu.Maximum = Math.Max(nudSoLuongDongSoHuu.Maximum, Math.Max(2, repsCu.Count));
                    pnlDongSoHuu.AutoScroll = true;
                    nudSoLuongDongSoHuu.Value = Math.Max(nudSoLuongDongSoHuu.Minimum, Math.Max(2, repsCu.Count));
                    nudSoLuongDongSoHuu.Visible = true;
                    pnlDongSoHuu.Visible = true;
                    RenderDongSoHuu();
                    for (int i = 0; i < repsCu.Count; i++)
                    {
                        dsCCCDDongSoHuu[i].Text = repsCu[i].CCCD;
                        dsHoTenDongSoHuu[i].Text = repsCu[i].HoTen;
                        dsSDTDongSoHuu[i].Text = repsCu[i].SoDienThoai;
                    }
                    // Visible hiệu dụng có thể false trước ShowDialog; chỉnh lại khi hiện.
                    dlg.Shown += (s, ev) => CapNhatViTri();
                }
                renderSubForm(); CapNhatViTri();
                DongBoMauKhungThemMoiDN(dlg);
                if ((owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner)) == DialogResult.OK && newKH != null)
                {
                    LoadDataFromSql();
                    selectedKhachHang = listKhachHang.FirstOrDefault(k => k.CCCD == newKH.CCCD);
                    // Giữ nguyên bộ lọc hiện tại, không tự lọc bảng về một khách hàng.
                    FilterCustomerList(txtCCCDSearch.Text.Trim(), false);
                    SelectCustomerInGrid(newKH.CCCD);
                }
            }
        }
        #endregion

        #region Xử Lý Bảng Khách Hàng Bên Trái
        private void SelectCustomerInGrid(string cccd)
        {
            if (string.IsNullOrEmpty(cccd)) return;

            foreach (DataGridViewRow row in dgvKhachHang.Rows)
            {
                if (row.Cells["colCCCD"].Value?.ToString() == cccd)
                {
                    dgvKhachHang.ClearSelection();
                    row.Selected = true;
                    if (row.Cells.Count > 1)
                    {
                        dgvKhachHang.CurrentCell = row.Cells[1];
                    }
                    if (row.Index >= 0 && row.Index < dgvKhachHang.RowCount)
                    {
                        dgvKhachHang.FirstDisplayedScrollingRowIndex = row.Index;
                    }
                    break;
                }
            }
        }

        private void DgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvKhachHang.Rows.Count)
            {
                // Bôi xanh dòng khách hàng đang được ấn chuột chọn
                dgvKhachHang.ClearSelection();
                dgvKhachHang.Rows[e.RowIndex].Selected = true;

                string cccd = dgvKhachHang.Rows[e.RowIndex].Cells["colCCCD"].Value?.ToString();
                selectedKhachHang = listKhachHang.FirstOrDefault(k => k.CCCD == cccd);
                DisplayCustomerDetails();
            }
        }

        private void DgvKhachHang_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                MoFormQuanLyKhachHang();
            }
        }

        private void DgvKhachHang_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = dgvKhachHang.HitTest(e.X, e.Y);
            if (hit.RowIndex >= 0)
            {
                dgvKhachHang.ClearSelection();
                dgvKhachHang.Rows[hit.RowIndex].Selected = true;

                string cccd = dgvKhachHang.Rows[hit.RowIndex].Cells["colCCCD"].Value?.ToString();
                selectedKhachHang = listKhachHang.FirstOrDefault(k => k.CCCD == cccd);
                DisplayCustomerDetails();

                if (e.Button == MouseButtons.Right)
                {
                    ContextMenuStrip cms = new ContextMenuStrip();
                    ToolStripMenuItem itemManage = new ToolStripMenuItem("Quản lý khách hàng");
                    ToolStripMenuItem itemAddAccount = new ToolStripMenuItem("Thêm tài khoản");
                    itemAddAccount.Enabled = selectedKhachHang != null && !LaNguoiDuocUyQuyenDN(selectedKhachHang);
                    if (!itemAddAccount.Enabled) itemAddAccount.ToolTipText = LoiUyQuyenMoTaiKhoanDN;

                    itemManage.Click += (s, ev) => MoFormQuanLyKhachHang();
                    itemAddAccount.Click += (s, ev) => MoFormThemTaiKhoanChoKhachHang();

                    cms.Items.AddRange(new ToolStripItem[] { itemManage, itemAddAccount, new ToolStripSeparator() });
                    cms.Show(dgvKhachHang, e.Location);
                }
            }
        }

        private void MoFormQuanLyKhachHang()
        {
            if (selectedKhachHang == null) return;

            Form frmKH = CreateFormInstance(typeof(FrmKhachHangDN),
                new object[] { this, selectedKhachHang.CCCD },
                new object[] { selectedKhachHang.CCCD },
                new object[] { this });

            SetFormProperty(frmKH, "SelectedCCCD", selectedKhachHang.CCCD);
            SetFormProperty(frmKH, "CCCD", selectedKhachHang.CCCD);


            OpenChildForm(frmKH);
            ApplyAccountFilterToChildForm(frmKH, selectedKhachHang.CCCD, null);
        }

        private void MoFormThemTaiKhoanChoKhachHang()
        {
            if (selectedKhachHang == null) return;
            // Chỉ cập nhật bảng khi người dùng xác nhận mở tài khoản thành công.
            // Đóng/Hủy hộp thoại không được thay đổi bộ lọc danh sách khách hàng.
            OpenRegistrationDialog(selectedKhachHang.CCCD, selectedKhachHang);
        }
        #endregion

        #region Hiển Thị Chi Tiết Khung Phải & Hiện Cột "Vai Trò"
        public void DisplayCustomerDetails()
        {
            ClearAccountGridSelections();

            if (selectedKhachHang == null)
            {
                lblInfoSelectedCustomer.Text = "THÔNG TIN KHÁCH HÀNG ĐANG CHỌN: (Chưa chọn)";
                lblTotalVayHeader.Text = "Số TK Vay: 0 | Tổng số tiền vay: 0 VNĐ";
                lblTotalTKHeader.Text = "Số TK Tiết kiệm: 0 | Tổng số tiền TK: 0 VNĐ";
                lblTotalTTHeader.Text = "Số TK Thanh toán: 0 | Tổng số tiền dùng có: 0 VNĐ";
                dgvVay.Rows.Clear();
                dgvTietKiem.Rows.Clear();
                dgvThanhToan.Rows.Clear();
                return;
            }

            var khChon = selectedKhachHang;
            var dsChuLienQuan = listKhachHang.Where(k => k.CCCD != khChon.CCCD &&
                k.DanhSachDongSoHuu.Any(d => string.Equals(d.CCCD, khChon.CCCD, StringComparison.OrdinalIgnoreCase))).ToList();
            bool coHoSoRieng = !khChon.IsDongSoHuuOnly;
            lblInfoSelectedCustomer.Text = $"ĐANG CHỌN: {khChon.HoTen.ToUpper()} ({(dsChuLienQuan.Count > 0 ? "CCCD" : "MST")}: {khChon.CCCD})";
            var displayVayList = new List<(TaiKhoanVayItem Item, string VaiTro)>();
            var displayTKList = new List<(SoTietKiemItem Item, string VaiTro)>();
            var displayTTList = new List<(TaiKhoanThanhToanItem Item, string VaiTro)>();
            if (coHoSoRieng)
            {
                string vaiTro = "Chủ sở hữu (Người được ủy quyền: " +
                    (khChon.DanhSachDongSoHuu.Count == 0 ? "Không có" : string.Join(", ", khChon.DanhSachDongSoHuu.Select(d => d.HoTen))) + ")";
                foreach (var item in khChon.DanhSachVay) displayVayList.Add((item, vaiTro));
                foreach (var item in khChon.DanhSachTietKiem) displayTKList.Add((item, vaiTro));
                foreach (var item in khChon.DanhSachThanhToan) displayTTList.Add((item, vaiTro));
            }
            foreach (var chu in dsChuLienQuan)
            {
                string vaiTro = "Người được ủy quyền (Chủ sở hữu: " + chu.HoTen + ")";
                foreach (var item in chu.DanhSachVay)
                    if (khChon.TaiKhoanDuocUyQuyen.Contains("V|" + item.SoTKVay) && !displayVayList.Any(x => x.Item.SoTKVay == item.SoTKVay)) displayVayList.Add((item, vaiTro));
                foreach (var item in chu.DanhSachTietKiem)
                    if (khChon.TaiKhoanDuocUyQuyen.Contains("K|" + item.SoTKTK) && !displayTKList.Any(x => x.Item.SoTKTK == item.SoTKTK)) displayTKList.Add((item, vaiTro));
                foreach (var item in chu.DanhSachThanhToan)
                    if (khChon.TaiKhoanDuocUyQuyen.Contains("T|" + item.SoTKTT) && !displayTTList.Any(x => x.Item.SoTKTT == item.SoTKTT)) displayTTList.Add((item, vaiTro));
            }

            // 4. Cập nhật nhãn tổng kết số lượng & số tiền
            lblTotalVayHeader.Text = $"Số TK Vay: {displayVayList.Count} | Tổng số tiền vay: {displayVayList.Sum(x => x.Item.SoTienVay):N0} VNĐ";
            lblTotalTKHeader.Text = $"Số TK Tiết kiệm: {displayTKList.Count} | Tổng số tiền TK: {displayTKList.Sum(x => x.Item.SoTienGui):N0} VNĐ";
            lblTotalTTHeader.Text = $"Số TK Thanh toán: {displayTTList.Count} | Tổng số tiền dùng có: {displayTTList.Sum(x => x.Item.SoDu):N0} VNĐ";

            // 5. Đổ dữ liệu vào 3 DataGridView
            // Cột thứ 4 của Vay: tổng trả cả gốc + lãi + phí; cột thứ 5 của tiết kiệm: tổng thực nhận.
            if (dgvVay.Columns.Count > 3) dgvVay.Columns[3].HeaderText = "Tổng đã trả (gốc + lãi + phí)";
            if (dgvTietKiem.Columns.Count > 4) dgvTietKiem.Columns[4].HeaderText = "Tổng tiền đã nhận";
            dgvVay.Rows.Clear();
            int stt = 1;
            foreach (var t in displayVayList)
            {
                dgvVay.Rows.Add(stt++, t.Item.SoTKVay, $"{t.Item.SoTienVay:N0} VNĐ", $"{t.Item.TongTienDaThanhToan:N0} VNĐ", $"{t.Item.LaiSuat}% / năm", t.Item.KyHan, GetStatusText(t.Item.TrangThai), t.VaiTro);
            }

            dgvTietKiem.Rows.Clear();
            stt = 1;
            foreach (var t in displayTKList)
            {
                dgvTietKiem.Rows.Add(stt++, t.Item.SoTKTK, $"{t.Item.SoTienGui:N0} VNĐ", $"{t.Item.LaiSuat}% / năm", $"{t.Item.TongTienDaNhan:N0} VNĐ", t.Item.KyHan, GetStatusText(t.Item.TrangThai), t.VaiTro);
            }

            dgvThanhToan.Rows.Clear();
            stt = 1;
            foreach (var t in displayTTList)
            {
                dgvThanhToan.Rows.Add(stt++, t.Item.SoTKTT, $"{t.Item.SoDu:N0} VNĐ", GetStatusText(t.Item.TrangThai), t.VaiTro);
            }

            ClearAccountGridSelections();
        }

        private string GetStatusText(TrangThaiTK st)
        {
            switch (st)
            {
                case TrangThaiTK.HoatDong: return "Hoạt động";
                case TrangThaiTK.Khoa: return "Đang khóa";
                case TrangThaiTK.Dong: return "Đã đóng";
                default: return "Hoạt động";
            }
        }

        private void DgvVay_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvVay.Rows.Count)
            {
                string soTK = dgvVay.Rows[e.RowIndex].Cells["colVaySoTK"].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(soTK))
                {
                    MoFormVaySelected(new List<string> { soTK });
                }
            }
        }

        private void DgvTietKiem_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvTietKiem.Rows.Count)
            {
                string soTK = dgvTietKiem.Rows[e.RowIndex].Cells["colTKSoTK"].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(soTK))
                {
                    MoFormTietKiemSelected(new List<string> { soTK });
                }
            }
        }

        private void DgvThanhToan_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvThanhToan.Rows.Count)
            {
                string soTK = dgvThanhToan.Rows[e.RowIndex].Cells["colTTSoTK"].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(soTK))
                {
                    MoFormThanhToanSelected(new List<string> { soTK });
                }
            }
        }

        private void MoFormVaySelected(List<string> listSoTK)
        {
            if (selectedKhachHang == null || listSoTK == null || listSoTK.Count == 0) return;

            string firstSoTK = listSoTK[0];
            Form frmVay = CreateFormInstance(typeof(FrmVayDN),
                new object[] { this.listKhachHang, selectedKhachHang.CCCD, listSoTK },
                new object[] { this.listKhachHang, selectedKhachHang.CCCD, firstSoTK },
                new object[] { this, selectedKhachHang.CCCD, firstSoTK },
                new object[] { selectedKhachHang.CCCD, firstSoTK },
                new object[] { this.listKhachHang, selectedKhachHang.CCCD },
                new object[] { this.listKhachHang });

            SetFormProperty(frmVay, "SelectedSoTKList", listSoTK);
            SetFormProperty(frmVay, "SelectedSoTK", firstSoTK);
            SetFormProperty(frmVay, "SoTK", firstSoTK);
            SetFormProperty(frmVay, "SelectedCCCD", selectedKhachHang.CCCD);
            SetFormProperty(frmVay, "CCCD", selectedKhachHang.CCCD);


            OpenChildForm(frmVay);
            ApplyAccountFilterToChildForm(frmVay, selectedKhachHang.CCCD, listSoTK);
        }

        private void MoFormTietKiemSelected(List<string> listSoTK)
        {
            if (selectedKhachHang == null || listSoTK == null || listSoTK.Count == 0) return;

            string firstSoTK = listSoTK[0];
            Form frmTK = CreateFormInstance(typeof(FrmTietKiemDN),
                new object[] { this, selectedKhachHang.CCCD, listSoTK },
                new object[] { this, selectedKhachHang.CCCD, firstSoTK },
                new object[] { selectedKhachHang.CCCD, firstSoTK },
                new object[] { this, selectedKhachHang.CCCD },
                new object[] { selectedKhachHang.CCCD });

            SetFormProperty(frmTK, "SelectedSoTKList", listSoTK);
            SetFormProperty(frmTK, "SelectedSoTK", firstSoTK);
            SetFormProperty(frmTK, "SoTK", firstSoTK);
            SetFormProperty(frmTK, "SelectedCCCD", selectedKhachHang.CCCD);
            SetFormProperty(frmTK, "CCCD", selectedKhachHang.CCCD);


            OpenChildForm(frmTK);
            ApplyAccountFilterToChildForm(frmTK, selectedKhachHang.CCCD, listSoTK);
        }

        private void MoFormThanhToanSelected(List<string> listSoTK)
        {
            if (selectedKhachHang == null || listSoTK == null || listSoTK.Count == 0) return;

            string firstSoTK = listSoTK[0];
            Form frmTT = CreateFormInstance(typeof(FrmThanhToanDN),
                new object[] { this, selectedKhachHang.CCCD, listSoTK },
                new object[] { this, selectedKhachHang.CCCD, firstSoTK },
                new object[] { selectedKhachHang.CCCD, firstSoTK },
                new object[] { this, selectedKhachHang.CCCD },
                new object[] { selectedKhachHang.CCCD });

            SetFormProperty(frmTT, "SelectedSoTKList", listSoTK);
            SetFormProperty(frmTT, "SelectedSoTK", firstSoTK);
            SetFormProperty(frmTT, "SoTK", firstSoTK);
            SetFormProperty(frmTT, "SelectedCCCD", selectedKhachHang.CCCD);
            SetFormProperty(frmTT, "CCCD", selectedKhachHang.CCCD);

            frmTT.FormClosed += (s, e) => DongBoThongTinTuSql();

            OpenChildForm(frmTT);
            ApplyAccountFilterToChildForm(frmTT, selectedKhachHang.CCCD, listSoTK);
        }

        #region Helper Đẩy Thông Tin Vào Form Con
        private void ApplyAccountFilterToChildForm(Form frm, string cccd, List<string> listSoTK)
        {
            if (frm == null) return;
            string singleSoTK = (listSoTK != null && listSoTK.Count > 0) ? listSoTK[0] : null;

            string[] methodListNames = new string[] { "FilterByListSoTK", "SetSelectedAccounts", "FilterMultipleAccounts", "LoadDanhSachSoTK" };
            foreach (string mName in methodListNames)
            {
                MethodInfo mi = frm.GetType().GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null)
                {
                    var pars = mi.GetParameters();
                    if (pars.Length == 1 && pars[0].ParameterType == typeof(List<string>))
                    {
                        try { mi.Invoke(frm, new object[] { listSoTK }); } catch { }
                    }
                    else if (pars.Length == 2 && pars[0].ParameterType == typeof(string) && pars[1].ParameterType == typeof(List<string>))
                    {
                        try { mi.Invoke(frm, new object[] { cccd, listSoTK }); } catch { }
                    }
                }
            }

            if (!string.IsNullOrEmpty(singleSoTK))
            {
                string[] methodSingleNames = new string[] { "SelectAccount", "FocusAccount", "FilterBySoTK", "SetSoTK", "LoadSoTK", "TimKiemSoTK", "HienThiTaiKhoan" };
                foreach (string mName in methodSingleNames)
                {
                    MethodInfo mi = frm.GetType().GetMethod(mName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mi != null)
                    {
                        var pars = mi.GetParameters();
                        if (pars.Length == 1 && pars[0].ParameterType == typeof(string))
                        {
                            try { mi.Invoke(frm, new object[] { singleSoTK }); } catch { }
                        }
                        else if (pars.Length == 2 && pars[0].ParameterType == typeof(string) && pars[1].ParameterType == typeof(string))
                        {
                            try { mi.Invoke(frm, new object[] { cccd, singleSoTK }); } catch { }
                        }
                    }
                }
            }

            Action<Control> searchAndSetControls = null;
            searchAndSetControls = (parent) =>
            {
                foreach (Control c in parent.Controls)
                {
                    if (c is ComboBox cb)
                    {
                        if (cb.Name.IndexOf("SoTK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            cb.Name.IndexOf("TaiKhoan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            cb.Name.IndexOf("TK", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (!string.IsNullOrEmpty(singleSoTK))
                            {
                                for (int i = 0; i < cb.Items.Count; i++)
                                {
                                    if (cb.Items[i].ToString().Contains(singleSoTK))
                                    {
                                        cb.SelectedIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (c is TextBox txt)
                    {
                        if (!string.IsNullOrEmpty(cccd) && txt.Name.IndexOf("CCCD", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            txt.Text = cccd;
                        }
                        else if (!string.IsNullOrEmpty(singleSoTK) && (
                            txt.Name.IndexOf("SoTK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            txt.Name.IndexOf("TaiKhoan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            txt.Name.IndexOf("Search", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            txt.Text = singleSoTK;
                        }
                    }

                    if (c is DataGridView dgv && listSoTK != null && listSoTK.Count > 0)
                    {
                        try
                        {
                            dgv.ClearSelection();
                            bool foundAny = false;

                            CurrencyManager cm = null;
                            if (dgv.DataSource != null)
                            {
                                cm = (CurrencyManager)dgv.BindingContext[dgv.DataSource, dgv.DataMember];
                                cm?.SuspendBinding();
                            }

                            foreach (DataGridViewRow row in dgv.Rows)
                            {
                                if (row.IsNewRow) continue;
                                bool isMatch = false;

                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    if (cell.Value != null)
                                    {
                                        string cellVal = cell.Value.ToString().Trim();
                                        if (listSoTK.Any(stk => cellVal.Equals(stk, StringComparison.OrdinalIgnoreCase) || cellVal.Contains(stk)))
                                        {
                                            isMatch = true;
                                            break;
                                        }
                                    }
                                }

                                if (isMatch)
                                {
                                    row.Visible = true;
                                    row.Selected = true;
                                    if (!foundAny && row.Cells.Count > 0 && row.Cells[0].Visible)
                                    {
                                        dgv.CurrentCell = row.Cells[0];
                                        foundAny = true;
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        row.Visible = false;
                                    }
                                    catch
                                    {
                                        row.Selected = false;
                                    }
                                }
                            }

                            cm?.ResumeBinding();
                        }
                        catch { }
                    }

                    if (c.HasChildren)
                    {
                        searchAndSetControls(c);
                    }
                }
            };

            searchAndSetControls(frm);
        }

        private Form CreateFormInstance(Type formType, params object[][] candidateArgLists)
        {
            foreach (var args in candidateArgLists)
            {
                try
                {
                    ConstructorInfo ctor = formType.GetConstructor(args.Select(a => a?.GetType() ?? typeof(object)).ToArray());
                    if (ctor != null)
                    {
                        return (Form)ctor.Invoke(args);
                    }
                }
                catch { }
            }

            try
            {
                ConstructorInfo defaultCtor = formType.GetConstructor(Type.EmptyTypes);
                if (defaultCtor != null)
                {
                    return (Form)defaultCtor.Invoke(null);
                }
            }
            catch { }

            var ctors = formType.GetConstructors();
            if (ctors.Length > 0)
            {
                var first = ctors[0];
                var pars = first.GetParameters();
                object[] dummyArgs = new object[pars.Length];
                return (Form)first.Invoke(dummyArgs);
            }

            return (Form)Activator.CreateInstance(formType);
        }

        private void SetFormProperty(Form frm, string propName, object val)
        {
            if (frm == null || val == null) return;
            try
            {
                PropertyInfo p = frm.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.CanWrite)
                {
                    p.SetValue(frm, val, null);
                }
                else
                {
                    FieldInfo f = frm.GetType().GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null)
                    {
                        f.SetValue(frm, val);
                    }
                }
            }
            catch { }
        }

        private void SubscribeDataChanged(Form frm)
        {
            if (frm == null) return;
            try
            {
                EventInfo ev = frm.GetType().GetEvent("DataChanged", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (ev != null)
                {
                    EventHandler handler = (s, e) => DongBoThongTinTuSql();
                    ev.AddEventHandler(frm, handler);
                }
            }
            catch { }
        }
        #endregion
        #endregion

        #region Điều Hướng & Đồng Bộ Menu Danh Mục Bên Trái Khi Chuyển Form
        public void MoFormTheoLoaiTK(string maLoaiTK, string cccd, string soTK = "")
        {
            KhachHangData kh = listKhachHang.FirstOrDefault(k =>
                k.CCCD.Equals(cccd, StringComparison.OrdinalIgnoreCase) ||
                k.DanhSachVay.Any(v => v.SoTKVay.Equals(cccd, StringComparison.OrdinalIgnoreCase)) ||
                k.DanhSachTietKiem.Any(t => t.SoTKTK.Equals(cccd, StringComparison.OrdinalIgnoreCase)) ||
                k.DanhSachThanhToan.Any(tt => tt.SoTKTT.Equals(cccd, StringComparison.OrdinalIgnoreCase))
            );

            string realCCCD = kh != null ? kh.CCCD : cccd;
            string realSoTK = string.IsNullOrEmpty(soTK) && kh != null && kh.DanhSachVay.Any(v => v.SoTKVay.Equals(cccd, StringComparison.OrdinalIgnoreCase)) ? cccd : soTK;

            Form frmTarget = null;
            List<string> listSoTK = !string.IsNullOrEmpty(realSoTK) ? new List<string> { realSoTK } : null;

            if (maLoaiTK == "TK_THANHTOAN" || maLoaiTK == "Thanh toán")
            {
                frmTarget = CreateFormInstance(typeof(FrmThanhToanDN), new object[] { this, realCCCD, listSoTK }, new object[] { this, realCCCD, realSoTK }, new object[] { realCCCD, realSoTK }, new object[] { this, realCCCD }, new object[] { realCCCD });
            }
            else if (maLoaiTK == "TK_TIETKIEM" || maLoaiTK == "Tiết kiệm")
            {
                frmTarget = CreateFormInstance(typeof(FrmTietKiemDN), new object[] { this, realCCCD, listSoTK }, new object[] { this, realCCCD, realSoTK }, new object[] { realCCCD, realSoTK }, new object[] { this, realCCCD }, new object[] { realCCCD });
            }
            else if (maLoaiTK == "TK_VAY" || maLoaiTK == "Vay")
            {
                frmTarget = CreateFormInstance(typeof(FrmVayDN), new object[] { this.listKhachHang, realCCCD, listSoTK }, new object[] { this.listKhachHang, realCCCD, realSoTK }, new object[] { this, realCCCD, realSoTK }, new object[] { realCCCD, realSoTK }, new object[] { this.listKhachHang, realCCCD }, new object[] { this.listKhachHang });
            }

            if (frmTarget != null)
            {
                if (!string.IsNullOrEmpty(realSoTK))
                {
                    SetFormProperty(frmTarget, "SelectedSoTKList", listSoTK);
                    SetFormProperty(frmTarget, "SelectedSoTK", realSoTK);
                    SetFormProperty(frmTarget, "SoTK", realSoTK);
                }
                SetFormProperty(frmTarget, "SelectedCCCD", realCCCD);
                SetFormProperty(frmTarget, "CCCD", realCCCD);

                OpenChildForm(frmTarget);
                ApplyAccountFilterToChildForm(frmTarget, realCCCD, listSoTK);
            }
        }

        private void btnCaNhan_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hãy chọn tab Cá nhân ở FrmMain để trở về dữ liệu cá nhân.");
        }

        private void btnDoanhNghiep_Click(object sender, EventArgs e)
        {
            SwitchTab("DoanhNghiep");
        }

        // Tách biệt chế độ hiển thị; không sử dụng dữ liệu Cá nhân cho màn hình Doanh nghiệp.
        private void SwitchTab(string mode)
        {
            if (mode != "CaNhan" && mode != "DoanhNghiep") return;

            // Khi đổi chế độ, đóng form của chế độ trước để không hiển thị nhầm dữ liệu.
            if (activeChildForm != null)
            {
                activeChildForm.Close();
                activeChildForm = null;
            }

            currentMode = mode;

            if (mode == "CaNhan")
            {
                btnCaNhan.BackColor = colorActiveBlue;
                btnCaNhan.ForeColor = Color.White;
                btnDoanhNghiep.BackColor = colorInactiveTab;
                btnDoanhNghiep.ForeColor = colorTextMuted;
            }
            else
            {
                btnDoanhNghiep.BackColor = colorActiveBlue;
                btnDoanhNghiep.ForeColor = Color.White;
                btnCaNhan.BackColor = colorInactiveTab;
                btnCaNhan.ForeColor = colorTextMuted;
            }

            BuildSidebarMenu();
        }

        // Tạo form Doanh nghiệp khi người dùng mở mục tương ứng.
        // Không chuyển sang form Cá nhân nếu thiếu form Doanh nghiệp.
        // Dùng tên lớp để FrmMainDN vẫn biên dịch được khi chưa thêm đủ các form DN.
        private Form TaoFormDoanhNghiep(string tenLop)
        {
            Type loai = typeof(FrmMainDN).Assembly.GetType("QLTKNH." + tenLop, false);
            if (loai == null || !typeof(Form).IsAssignableFrom(loai))
            {
                MessageBox.Show(
                    "Chưa tìm thấy form Doanh nghiệp: " + tenLop +
                    "\nHãy thêm file .cs và .Designer.cs của form này vào project.",
                    "Thiếu form Doanh nghiệp",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
            try
            {
                return (Form)Activator.CreateInstance(loai);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được " + tenLop + ":\n" +
                    (ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                    "Lỗi mở form", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Khối định danh trên thanh bên: BANKING SYSTEM -> người dùng -> Cá nhân/Doanh nghiệp.
        // Chỉ lấy từ phiên xác thực, không dùng tên mẫu hoặc bảng tài khoản khác.
        private Panel _pnlNguoiDungSidebar;
        private Label _lblTenSidebar;
        private Label _lblVaiTroSidebar;

        private bool LaGiamDocSidebar()
        {
            return BankSecurity.IsLoggedIn && BankSecurity.Current != null &&
                   string.Equals(BankSecurity.Current.ChucVu, "Giám đốc", StringComparison.OrdinalIgnoreCase);
        }

        private void KhoiTaoThongTinTaiKhoanSidebar()
        {
            if (_pnlNguoiDungSidebar != null || panelSidebar == null) return;
            _pnlNguoiDungSidebar = new Panel
            {
                Name = "pnlNguoiDungSidebar",
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(11, 19, 43),
                Padding = new Padding(6, 6, 2, 6)
            };
            var avatar = new Label
            {
                Text = "👤",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 99, 235),
                Font = new Font("Segoe UI Emoji", 14F),
                Location = new Point(6, 16),
                Size = new Size(32, 32)
            };
            _lblTenSidebar = new Label
            {
                Name = "lblTenDangNhapSidebar",
                AutoEllipsis = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(44, 12),
                Size = new Size(220, 21)
            };
            _lblVaiTroSidebar = new Label
            {
                Name = "lblVaiTroSidebar",
                AutoEllipsis = true,
                ForeColor = Color.FromArgb(170, 190, 217),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(44, 33),
                Size = new Size(220, 19)
            };
            _pnlNguoiDungSidebar.Controls.Add(avatar);
            _pnlNguoiDungSidebar.Controls.Add(_lblTenSidebar);
            _pnlNguoiDungSidebar.Controls.Add(_lblVaiTroSidebar);

            // Chuông cố định ở góc phải khung nhân viên, không nằm trong menu cuộn.
            var btnChuongThongBao = new Button
            {
                Name = "btnChuongThongBao",
                Size = new Size(32, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorActiveBlue,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                AccessibleName = "Trung tâm thông báo",
                AccessibleDescription = "Mở danh sách thông báo"
            };
            btnChuongThongBao.FlatAppearance.BorderSize = 0;
            // Vẽ chuông bằng GDI+ để không phụ thuộc font emoji của Windows.
            btnChuongThongBao.Paint += (sender, args) =>
            {
                var g = args.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int x = btnChuongThongBao.ClientSize.Width / 2;
                int y = btnChuongThongBao.ClientSize.Height / 2 + 5;
                using (var pen = new Pen(btnChuongThongBao.Enabled ? Color.White : colorTextMuted, 1.4F))
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddBezier(x - 7, y + 4, x - 4, y + 1, x - 5, y - 6, x, y - 6);
                    path.AddBezier(x, y - 6, x + 5, y - 6, x + 4, y + 1, x + 7, y + 4);
                    path.CloseFigure();
                    g.DrawPath(pen, path);
                    g.DrawArc(pen, x - 2, y + 4, 4, 4, 0, 180);
                }
                int? unread = FrmNotification.UnreadCount;
                if (unread.GetValueOrDefault() > 0)
                {
                    string number = unread.Value > 99 ? "99+" : unread.Value.ToString();
                    Rectangle badge = new Rectangle(btnChuongThongBao.ClientSize.Width - 23, 1, 22, 13);
                    using (var red = new SolidBrush(Color.FromArgb(220, 38, 38)))
                    using (var font = new Font("Segoe UI", 7F, FontStyle.Bold))
                    {
                        g.FillEllipse(red, badge);
                        TextRenderer.DrawText(g, number, font, badge, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    }
                }
            };
            btnChuongThongBao.Click += (sender, args) =>
            {
                if (!BankSecurity.IsLoggedIn || !BankSecurity.CanUseFinance)
                {
                    MessageBox.Show(this, "Bạn không có quyền xem trung tâm thông báo.");
                    return;
                }
                if (activeChildForm is FrmNotification && !activeChildForm.IsDisposed) return;
                // Mở bằng điều hướng hiện có, không thêm truy vấn hay timer.
                OpenChildForm(new FrmNotification());
            };
            var tipChuong = new ToolTip();
            tipChuong.SetToolTip(btnChuongThongBao, "Trung tâm thông báo");
            EventHandler capNhatSoThongBao = (sender, args) =>
            {
                if (btnChuongThongBao.IsDisposed || btnChuongThongBao.Disposing) return;
                int? n = FrmNotification.UnreadCount;
                tipChuong.SetToolTip(btnChuongThongBao, n.HasValue ? "Thông báo chưa đọc: " + n.Value : "Trung tâm thông báo (chưa có số đếm)");
                btnChuongThongBao.Invalidate();
            };
            FrmNotification.UnreadCountChanged += capNhatSoThongBao;
            btnChuongThongBao.Disposed += (sender, args) =>
            {
                FrmNotification.UnreadCountChanged -= capNhatSoThongBao;
                tipChuong.Dispose();
            };
            // MainDN chỉ tạo khung này khi chạy độc lập. Main ngoài dùng chung chuông CN/DN.
            async void TaiSoThongBao()
            {
                await FrmNotification.InitializeBadgeAsync();
                if (!btnChuongThongBao.IsDisposed) capNhatSoThongBao(null, EventArgs.Empty);
            }
            if (Visible && IsHandleCreated) TaiSoThongBao();
            else Shown += (sender, args) => TaiSoThongBao();
            _pnlNguoiDungSidebar.Controls.Add(btnChuongThongBao);
            Action canChinhChuong = () =>
            {
                int giua = _pnlNguoiDungSidebar.ClientSize.Height / 2;
                avatar.Top = giua - avatar.Height / 2;
                int tongCao = _lblTenSidebar.Height + _lblVaiTroSidebar.Height;
                _lblTenSidebar.Top = giua - tongCao / 2;
                _lblVaiTroSidebar.Top = _lblTenSidebar.Bottom;
                int trai = Math.Max(44, _pnlNguoiDungSidebar.ClientSize.Width - btnChuongThongBao.Width - 5);
                btnChuongThongBao.Location = new Point(trai, (_pnlNguoiDungSidebar.ClientSize.Height - btnChuongThongBao.Height) / 2);
                _lblTenSidebar.Width = Math.Max(0, trai - _lblTenSidebar.Left - 6);
                _lblVaiTroSidebar.Width = Math.Max(0, trai - _lblVaiTroSidebar.Left - 6);
                btnChuongThongBao.BringToFront();
            };
            _pnlNguoiDungSidebar.SizeChanged += (sender, args) => canChinhChuong();

            panelSidebar.Controls.Add(_pnlNguoiDungSidebar);
            // Original order: panelMenu(0), tableTabSwitcher(1), lblTitle(2).
            // Place profile between title and tabs in Dock.Top order.
            panelSidebar.Controls.SetChildIndex(_pnlNguoiDungSidebar, 2);
            canChinhChuong();
            CapNhatThongTinTaiKhoanSidebar();
        }

        private void CapNhatThongTinTaiKhoanSidebar()
        {
            if (_lblTenSidebar == null || _lblVaiTroSidebar == null) return;
            var nv = BankSecurity.Current;
            _lblTenSidebar.Text = nv == null ? "Chưa đăng nhập" :
                (string.IsNullOrWhiteSpace(nv.HoTen) ? nv.TenDangNhap : nv.HoTen);
            _lblVaiTroSidebar.Text = nv == null ? "Vai trò: —" :
                (LaGiamDocSidebar() ? "Vai trò: Giám đốc" : "Vai trò: Nhân viên");
        }

        private void BuildSidebarMenu()
        {
            // Clear không giải phóng các Button cũ.
            foreach (Control oldMenu in panelMenu.Controls.Cast<Control>().ToArray())
            {
                panelMenu.Controls.Remove(oldMenu);
                oldMenu.Dispose();
            }
            currentActiveMenuBtn = null;
            panelMenu.AutoScroll = true;

            // Giữ nguyên thứ tự menu cũ, thêm mục giám sát rủi ro dùng chung.
            // Khối tạo form Cá nhân được giữ nguyên theo file gốc.
            var menuItems = new List<Tuple<string, string, Func<Form>>>
            {
                Tuple.Create("Trang chính", "📊", (Func<Form>)(() =>
                    (Form)null)),

                Tuple.Create("Khách hàng", "👤", (Func<Form>)(() =>
                    TaoFormDoanhNghiep("FrmKhachHangDN"))),

                Tuple.Create("Tài khoản vay", "💰", (Func<Form>)(() =>
                {
                    return TaoFormDoanhNghiep("FrmVayDN");
                })),

                Tuple.Create("Sổ tiết kiệm", "🏦", (Func<Form>)(() =>
                    TaoFormDoanhNghiep("FrmTietKiemDN"))),

                Tuple.Create("Tài khoản thanh toán", "💳", (Func<Form>)(() =>
                    TaoFormDoanhNghiep("FrmThanhToanDN"))),

                Tuple.Create("Loại tài khoản", "🏷️", (Func<Form>)(() =>
                    TaoFormDoanhNghiep("FrmLoaiTaiKhoanDN"))),

                Tuple.Create("Lịch sử", "📜", (Func<Form>)(() =>
                    TaoFormDoanhNghiep("FrmLichSuDN"))),

                Tuple.Create("Thống kê", "📈", (Func<Form>)(() =>
                    new FrmThongKeDN())),
                Tuple.Create("Giám sát rủi ro", "🛡", (Func<Form>)(() => new FrmRiskCenter())),
                Tuple.Create("Hồ sơ", "👤", (Func<Form>)(() => null)),
                Tuple.Create("Quản lý nhân viên", "👥", (Func<Form>)(() => null)),
                Tuple.Create("Đăng xuất", "↪", (Func<Form>)(() => null))
            };

            for (int i = menuItems.Count - 1; i >= 0; i--)
            {
                var item = menuItems[i];
                string tenMuc = item.Item1;
                if (tenMuc == "Quản lý nhân viên" && !LaGiamDocSidebar()) continue;
                string bieuTuong = item.Item2;
                Func<Form> taoForm = item.Item3;

                Button menuBtn = CreateButtonDynamicDesign12($"    {bieuTuong}    {tenMuc}");
                if ((tenMuc == "Tài khoản vay" && !BankSecurity.CanUseLoans) ||
                    ((tenMuc == "Tài khoản thanh toán" || tenMuc == "Sổ tiết kiệm" || tenMuc == "Giám sát rủi ro") && !BankSecurity.CanUseFinance))
                    menuBtn.Enabled = false;
                menuBtn.FlatAppearance.BorderSize = 0;

                menuBtn.MouseEnter += (s, e) =>
                {
                    if (menuBtn != currentActiveMenuBtn) menuBtn.BackColor = colorHover;
                };
                menuBtn.MouseLeave += (s, e) =>
                {
                    if (menuBtn != currentActiveMenuBtn) menuBtn.BackColor = Color.Transparent;
                };

                menuBtn.Click += (s, e) =>
                {
                    // Tạo form trước: nếu form DN chưa có, giữ màn hình DN hiện tại,
                    // tuyệt đối không mở dữ liệu Cá nhân thay thế.
                    if (tenMuc == "Hồ sơ")
                    {
                        var frm = new FrmHoSoNhanVien();
                        frm.HoSoDaCapNhat += (senderHoSo, argsHoSo) => CapNhatThongTinTaiKhoanSidebar();
                        OpenChildForm(frm);
                        CapNhatThongTinTaiKhoanSidebar();
                        return;
                    }
                    if (tenMuc == "Quản lý nhân viên")
                    {
                        if (!LaGiamDocSidebar())
                        {
                            MessageBox.Show(this, "Chỉ Giám đốc được quản lý nhân viên.");
                            return;
                        }
                        OpenChildForm(new FrmQuanLyNhanVien());
                        CapNhatThongTinTaiKhoanSidebar();
                        return;
                    }
                    if (tenMuc == "Đăng xuất")
                    {
                        if (MessageBox.Show(this, "Bạn muốn đăng xuất và quay lại màn hình đăng nhập?",
                            "Xác nhận đăng xuất", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) == DialogResult.Yes)
                            DangXuatVeDangNhap();
                        return;
                    }
                    if (tenMuc == "Giám sát rủi ro" && !BankSecurity.CanUseFinance)
                    {
                        MessageBox.Show(this, "Bạn không có quyền xem trung tâm giám sát rủi ro.");
                        return;
                    }
                    Form f = taoForm();
                    if (f == null && tenMuc != "Trang chính")
                        return;

                    if (currentActiveMenuBtn != null)
                    {
                        currentActiveMenuBtn.BackColor = Color.Transparent;
                        currentActiveMenuBtn.ForeColor = colorTextMuted;
                    }
                    currentActiveMenuBtn = menuBtn;
                    menuBtn.BackColor = colorActiveBlue;
                    menuBtn.ForeColor = Color.White;

                    if (f != null) OpenChildForm(f);
                    else CloseActiveChildForm();
                };

                panelMenu.Controls.Add(menuBtn);
            }

            if (panelMenu.Controls.Count > 0)
                ((Button)panelMenu.Controls[panelMenu.Controls.Count - 1]).PerformClick();
        }

        // Đăng xuất đúng phiên hiện tại rồi trả quyền điều khiển về Program.cs.
        // Program.cs sẽ khởi tạo FrmDangNhap mới, không kết thúc ứng dụng.
        private void DangXuatVeDangNhap()
        {
            if (_timerPhienNhanVien != null)
                _timerPhienNhanVien.Stop();

            try
            {
                // BankSecurity.Logout() đã ghi nhật ký và chuyển nhân viên sang Offline.
                BankSecurity.Logout();
            }
            catch (Exception ex)
            {
                // Không giữ phiên đăng nhập cũ nếu SQL tạm thời mất kết nối.
                MessageBox.Show(this, "Không thể cập nhật trạng thái đăng xuất trên SQL: " + ex.Message,
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Đóng cửa sổ chính. Vòng lặp tại Program.Main sẽ mở đăng nhập.
            Close();
        }

        private void CloseActiveChildForm()
        {
            if (activeChildForm != null)
            {
                activeChildForm.Close();
                activeChildForm = null;
            }
            DongBoThongTinTuSql();
            panelMainLayout.BringToFront();
            HighlightSidebarMenu("Trang chính");
            DongBoMenuFrmMainCha(this);
        }

        public void OpenChildForm(Form childForm)
        {
            if (activeChildForm != null)
            {
                activeChildForm.Close();
            }

            SubscribeDataChanged(childForm);
            activeChildForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

            SyncSidebarMenuWithChildForm(childForm);
            DongBoMenuFrmMainCha(childForm);
        }

        // Đồng bộ thanh menu thực tế khi FrmMainDN được nhúng trong FrmMain.
        private void DongBoMenuFrmMainCha(Form formDangHienThi)
        {
            for (Control cha = Parent; cha != null; cha = cha.Parent)
            {
                if (cha is FrmMain main && !main.IsDisposed && !main.Disposing)
                {
                    // Hỗ trợ cả phiên bản FrmMain có hàm đồng bộ private hoặc public.
                    var method = typeof(FrmMain).GetMethod("SyncSidebarMenuWithChildForm",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(Form) }, null);
                    if (method != null) method.Invoke(main, new object[] { formDangHienThi });
                    break;
                }
            }
        }

        private void SyncSidebarMenuWithChildForm(Form childForm)
        {
            if (childForm == null) return;

            string targetMenu = null;
            string formTypeName = childForm.GetType().Name;

            if (formTypeName == "FrmVayDN" || formTypeName == "FrmVayDN") targetMenu = "Tài khoản vay";
            else if (formTypeName == "FrmTietKiemDN" || formTypeName == "FrmTietKiemDN") targetMenu = "Sổ tiết kiệm";
            else if (formTypeName == "FrmThanhToanDN" || formTypeName == "FrmThanhToanDN") targetMenu = "Tài khoản thanh toán";
            else if (formTypeName == "FrmKhachHangDN" || formTypeName == "FrmKhachHangDN") targetMenu = "Khách hàng";
            else if (formTypeName == "FrmLoaiTaiKhoanDN" || formTypeName == "FrmLoaiTaiKhoanDN") targetMenu = "Loại tài khoản";
            else if (formTypeName == "FrmLichSuDN" || formTypeName == "FrmLichSuDN") targetMenu = "Lịch sử";
            else if (formTypeName == "FrmThongKe" || formTypeName == "FrmThongKeDN") targetMenu = "Thống kê";
            else if (formTypeName == "FrmRiskCenter") targetMenu = "Giám sát rủi ro";
            else if (formTypeName == "FrmNotification")
            {
                if (currentActiveMenuBtn != null)
                {
                    currentActiveMenuBtn.BackColor = Color.Transparent;
                    currentActiveMenuBtn.ForeColor = colorTextMuted;
                    currentActiveMenuBtn = null;
                }
                return;
            }
            else if (formTypeName == "FrmHoSoNhanVien") targetMenu = "Hồ sơ";
            else if (formTypeName == "FrmQuanLyNhanVien") targetMenu = "Quản lý nhân viên";
            else if (formTypeName == "FrmMainDN") targetMenu = "Trang chính";


            if (!string.IsNullOrEmpty(targetMenu))
                HighlightSidebarMenu(targetMenu);
        }

        private void HighlightSidebarMenu(string menuName)
        {
            foreach (Control c in panelMenu.Controls)
            {
                if (c is Button btn)
                {
                    if (btn.Text.IndexOf(menuName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (currentActiveMenuBtn != null && currentActiveMenuBtn != btn)
                        {
                            currentActiveMenuBtn.BackColor = Color.Transparent;
                            currentActiveMenuBtn.ForeColor = colorTextMuted;
                        }

                        currentActiveMenuBtn = btn;
                        currentActiveMenuBtn.BackColor = colorActiveBlue;
                        currentActiveMenuBtn.ForeColor = Color.White;
                        break;
                    }
                }
            }
        }
        #endregion

        private void panelTopSearch_Paint(object sender, PaintEventArgs e)
        {
        }

        private void dgvTietKiem_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void txtCCCDSearch_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}