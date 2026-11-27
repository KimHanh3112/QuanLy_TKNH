using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QLTKNH
{
    public partial class FrmMain : Form
    {
        private readonly Color colorContentBg = Color.FromArgb(15, 23, 42);      // #0F172A
        private readonly Color colorActiveBlue = Color.FromArgb(37, 99, 235);    // #2563EB
        private readonly Color colorInactiveTab = Color.FromArgb(23, 33, 60);    // #17213C
        private readonly Color colorHover = Color.FromArgb(30, 41, 59);         // #1E293B
        private readonly Color colorTextMuted = Color.FromArgb(160, 174, 192);   // #A0AEC0
        private readonly Color colorGreenHighlight = Color.FromArgb(34, 197, 94); // #22C55E

        private readonly string _connStr =
            @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        private readonly Font _fontPhanCapCaNhan = new Font("Segoe UI", 9F, FontStyle.Bold);

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

        private Button currentActiveMenuBtn;
        private Form activeChildForm;
        private string currentMode = "CaNhan";

        private List<KhachHangData> listKhachHang = new List<KhachHangData>();
        private KhachHangData selectedKhachHang = null;

        public FrmMain()
        {
            InitializeComponent();
            KhoiTaoThongTinTaiKhoanSidebar();
            // Đồng bộ nhãn cột hiện hữu với các giá trị SQL; không sửa tên control Designer.
            colVaySoTienDaTra.HeaderText = "Tổng tiền đã trả (Gốc + Lãi + Phạt)";
            colTKSoTienGui.HeaderText = "Gốc còn lại";
            colTKSoTienDaRut.HeaderText = "Tổng tiền đã nhận";
            // SQL sẽ được đọc sau khi form bắt đầu hiển thị.
            SetupSearchAndEvents();
            foreach (DataGridViewColumn col in dgvKhachHang.Columns) col.SortMode = DataGridViewColumnSortMode.NotSortable;
            dgvKhachHang.CellFormatting += (sender, args) =>
            {
                if (args.RowIndex < 0 || args.ColumnIndex != 2) return;
                if (dgvKhachHang.Rows[args.RowIndex].Cells[2].Style.ForeColor == Color.FromArgb(192, 132, 252))
                { args.Value = "  ↳ " + Convert.ToString(args.Value); args.FormattingApplied = true; }
            };
            GanPhanCapCaNhan(dgvVay, -1, 7, _fontPhanCapCaNhan);
            GanPhanCapCaNhan(dgvTietKiem, -1, 7, _fontPhanCapCaNhan);
            GanPhanCapCaNhan(dgvThanhToan, -1, 4, _fontPhanCapCaNhan);
            Disposed += (sender, args) => _fontPhanCapCaNhan.Dispose();
            SetupSearchGroup();
            DongBoMauToiFrmMain();
            VeDuongPhanCachNhomTaiKhoan();
            SwitchTab("CaNhan");
            this.Load += FrmMain_Load;
            this.Shown += (s, e) => CapNhatThongTinTaiKhoanSidebar();
            this.Shown += (s, e) => { if (!BankSecurity.IsLoggedIn) this.Close(); };
            this.Activated += async (s, e) => { CapNhatThongTinTaiKhoanSidebar(); if (_daBatDauLoad && (_daTaiLanDau || !_dangTaiKhiKichHoat)) await DongBoKhiKichHoatAsync(); };
            KhoiTaoTheoDoiPhienNhanVien();
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

        // Chỉ đồng bộ màu cho các control của FrmMain; không sửa layout, sự kiện hoặc dữ liệu.
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
        private bool _daTaiLanDau;
        private bool _daBatDauLoad;
        private bool _yeuCauTaiLai;
        private DateTime _lanTaiGanNhatUtc = DateTime.MinValue;

        // Gom các thông báo thay đổi liên tiếp; không bỏ mất yêu cầu tải mới.
        private async void YeuCauDongBoSql()
        {
            if (IsDisposed || Disposing) return;
            _yeuCauTaiLai = true;
            await DongBoKhiKichHoatAsync(true);
        }

        // Chỉ đọc SQL trên luồng nền; mọi cập nhật model và control vẫn ở UI thread.
        private async System.Threading.Tasks.Task DongBoKhiKichHoatAsync(bool batBuoc = false)
        {
            if (IsDisposed || Disposing || !BankSecurity.IsLoggedIn) return;
            if (batBuoc) _yeuCauTaiLai = true;
            if (_dangTaiKhiKichHoat || _dangDongBoThongTin) return;
            // Chỉ hạn chế tải do Activated; thông báo thay đổi dữ liệu luôn được xử lý.
            if (!_yeuCauTaiLai && (DateTime.UtcNow - _lanTaiGanNhatUtc).TotalSeconds < 2) return;
            _dangTaiKhiKichHoat = true;
            try
            {
                do
                {
                    _yeuCauTaiLai = false;
                    long phienBan = _phienBanDuLieu;
                    try
                    {
                        var ds = await System.Threading.Tasks.Task.Run(() => DocDuLieuTuSql());
                        if (IsDisposed || Disposing || !BankSecurity.IsLoggedIn) return;
                        if (phienBan != _phienBanDuLieu || _yeuCauTaiLai) continue;
                        string cccd = selectedKhachHang?.CCCD;
                        ApDungDuLieuSql(ds);
                        FilterCustomerList(txtCCCDSearch.Text.Trim(), !string.IsNullOrWhiteSpace(txtCCCDSearch.Text));
                        if (!string.IsNullOrWhiteSpace(cccd)) SelectCustomerInGrid(cccd);
                        _daTaiLanDau = true;
                    }
                    catch (Exception ex)
                    {
                        if (!IsDisposed && !Disposing && phienBan == _phienBanDuLieu)
                            MessageBox.Show("Không thể tải dữ liệu từ SQL Server QLTKNH:\n" + ex.Message,
                                "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally { _lanTaiGanNhatUtc = DateTime.UtcNow; }
                } while (_yeuCauTaiLai && !IsDisposed && !Disposing);
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

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            ClearAllSelections();
            // Không cho tìm/mở tài khoản trên danh sách chưa được tải.
            Control[] controls = { panelMenu, panelTopSearch, dgvKhachHang, dgvVay,
                dgvTietKiem, dgvThanhToan, btnCaNhan, btnDoanhNghiep };
            bool[] enabled = controls.Select(c => c.Enabled).ToArray();
            foreach (Control c in controls) c.Enabled = false;
            try
            {
                await System.Threading.Tasks.Task.Yield();
                _daBatDauLoad = true;
                await DongBoKhiKichHoatAsync(true);
                if (!IsDisposed && !Disposing) ClearAllSelections();
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    for (int i = 0; i < controls.Length; i++) controls[i].Enabled = enabled[i];
            }
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

        // Mỗi số sổ chỉ được dùng để giải ngân một lần, kể cả đã tất toán khoản vay.
        // Đọc cả hai định dạng ghi chú đang được FrmMain và FrmVay sử dụng.
        internal static HashSet<string> LaySoTietKiemDaVay(SqlConnection cn, SqlTransaction tx,
            bool dn, bool soNgoai)
        {
            string p = dn ? "DN_" : "";
            string tienToMain = soNgoai ? "Vay thế chấp sổ ngoài: " : "Vay thế chấp sổ nội bộ: ";
            string tienToVay = "Vay thế chấp sổ tiết kiệm " +
                (soNgoai ? "Ngoài ngân hàng" : "Trong ngân hàng") + "; Số sổ: ";
            string hint = tx == null ? "" : " WITH (UPDLOCK, HOLDLOCK)";
            var daVay = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT GhiChu FROM dbo." + p +
                "LichSuGiaoDichVay" + hint + @" WHERE LoaiGiaoDich=N'Giải ngân'
                AND (GhiChu LIKE @main OR GhiChu LIKE @vay)", cn, tx))
            {
                cmd.Parameters.Add("@main", System.Data.SqlDbType.NVarChar, 150).Value = tienToMain + "%";
                cmd.Parameters.Add("@vay", System.Data.SqlDbType.NVarChar, 150).Value = tienToVay + "%";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string note = Convert.ToString(rd[0]);
                        string so;
                        if (note.StartsWith(tienToMain, StringComparison.OrdinalIgnoreCase))
                            so = note.Substring(tienToMain.Length).Trim();
                        else if (note.StartsWith(tienToVay, StringComparison.OrdinalIgnoreCase))
                        {
                            string phanSau = note.Substring(tienToVay.Length);
                            int end = phanSau.LastIndexOf("; Hạn mức:", StringComparison.OrdinalIgnoreCase);
                            so = (end >= 0 ? phanSau.Substring(0, end) : phanSau).Trim();
                        }
                        else continue;
                        if (!string.IsNullOrWhiteSpace(so)) daVay.Add(so);
                    }
                }
            }
            // Sổ cấp lại vẫn là cùng một khoản tiền gửi; không được dùng số mới để vay lần hai.
            if (!soNgoai)
            {
                var thayThe = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = new SqlCommand("SELECT SoTKTK, SoTKTKThayThe FROM dbo." + p +
                    "SoTietKiem" + hint + " WHERE SoTKTKThayThe IS NOT NULL", cn, tx))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string cu = Convert.ToString(rd[0]).Trim();
                        string moi = Convert.ToString(rd[1]).Trim();
                        if (cu.Length == 0 || moi.Length == 0) continue;
                        List<string> ds;
                        if (!thayThe.TryGetValue(cu, out ds))
                            thayThe[cu] = ds = new List<string>();
                        ds.Add(moi);
                        if (!thayThe.TryGetValue(moi, out ds))
                            thayThe[moi] = ds = new List<string>();
                        ds.Add(cu);
                    }
                }
                var hangDoi = new Queue<string>(daVay);
                while (hangDoi.Count > 0)
                {
                    List<string> ds;
                    if (!thayThe.TryGetValue(hangDoi.Dequeue(), out ds)) continue;
                    foreach (string so in ds)
                        if (daVay.Add(so)) hangDoi.Enqueue(so);
                }
            }
            return daVay;
        }

        // Gọi trong transaction giải ngân Serializable, trước khi INSERT khoản vay.
        // Không sửa số dư, trạng thái sổ hay lịch sử cũ.
        internal static void KiemTraSoTietKiemChuaVay(SqlConnection cn, SqlTransaction tx,
            bool dn, bool soNgoai, string soSo)
        {
            soSo = (soSo ?? "").Trim();
            if (soSo.Length == 0)
                throw new InvalidOperationException("Chưa xác định số sổ tiết kiệm dùng để vay.");
            // Ghi chú giải ngân tối đa 500 ký tự: giữ nguyên số sổ để tra cứu chính xác.
            if (soNgoai && soSo.Length > 200)
                throw new InvalidOperationException("Số sổ ngoài ngân hàng quá dài (tối đa 200 ký tự).");
            if (LaySoTietKiemDaVay(cn, tx, dn, soNgoai).Contains(soSo))
                throw new InvalidOperationException("Sổ tiết kiệm " + soSo +
                    " đã được sử dụng để vay. Mỗi sổ chỉ được vay một lần, kể cả khoản vay trước đã tất toán.");
        }

        public static List<SoTietKiemTheChap> LaySoTietKiemTheChap(string connStr, string cccd)
        {
            var result = new List<SoTietKiemTheChap>();
            using (var cn = new SqlConnection(connStr))
            {
                cn.Open();
                var soDaVay = FrmMain.LaySoTietKiemDaVay(cn, null, false, false);
                using (var cmd = new SqlCommand(@"SELECT SoTKTK, SoTienGui, SoTienDaRut, LaiSuat, NgayMo
                    FROM dbo.SoTietKiem
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
            return FrmMain.LayNhomNoKhiXetVay(connStr, false, cccd);
        }

        private static int TinhNhomKhoanVay(int? nhomDaLuu, decimal duNo,
            DateTime ngayMo, string kyHan, DateTime ngayTinh)
        {
            if (nhomDaLuu.HasValue)
            {
                if (nhomDaLuu.Value < 1 || nhomDaLuu.Value > 5)
                    throw new InvalidOperationException("Nhóm nợ trong SQL không hợp lệ.");
                return nhomDaLuu.Value;
            }
            if (duNo <= 0) return 1;
            int thang;
            if (!int.TryParse(new string((kyHan ?? "").Where(char.IsDigit).ToArray()), out thang)) thang = 12;
            DateTime daoHan = ngayMo.AddMonths(thang);
            int ngay = (int)(ngayTinh - daoHan).TotalDays;
            return ngay > 360 ? 5 : ngay > 180 ? 4 : ngay > 90 ? 3 : ngay >= 10 ? 2 : 1;
        }

        private static bool CoBang(SqlConnection cn, SqlTransaction tx, string bang)
        {
            using (var cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@b,N'U') IS NULL THEN 0 ELSE 1 END", cn, tx))
            {
                cmd.Parameters.Add("@b", System.Data.SqlDbType.NVarChar, 128).Value = bang;
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        // Đọc cả nhóm đã lưu và nhóm tự tính. Một khách hàng có thể có nhiều STK.
        // Chỉ lấy các liên kết theo STK nếu bảng riêng tồn tại; hệ cũ dùng liên kết theo chủ.
        private static Dictionary<string, int> DocNhomKhachHang(SqlConnection cn, SqlTransaction tx, bool dn, string ma = null)
        {
            string p = dn ? "DN_" : "";
            string sql = @";WITH A AS (
 SELECT SoTKVay AS STK, CCCD, 1 AS Loai, SoTienVay-SoTienDaTra AS DuNo, NgayMo, KyHan FROM dbo." + p + @"TaiKhoanVay
 UNION ALL SELECT SoTKTK, CCCD, 2, CAST(0 AS decimal(18,0)), NgayMo, N'' FROM dbo." + p + @"SoTietKiem
 UNION ALL SELECT SoTKTT, CCCD, 3, CAST(0 AS decimal(18,0)), NgayMo, N'' FROM dbo." + p + @"TaiKhoanThanhToan
 ), L AS (SELECT STK, Loai, CCCD FROM A";
            string[] bang = { "DongSoHuuTaiKhoanVay", "DongSoHuuSoTietKiem", "DongSoHuuTaiKhoanThanhToan" };
            string[] cot = { "SoTKVay", "SoTKTK", "SoTKTT" };
            for (int i = 0; i < bang.Length; i++)
            {
                string b = "dbo." + p + bang[i];
                if (CoBang(cn, tx, b))
                    sql += " UNION SELECT d." + cot[i] + "," + (i + 1) + ",d.CCCD FROM " + b + " d";
                else
                    sql += " UNION SELECT a.STK,a.Loai,d.CCCD FROM A a JOIN dbo." + p + "DongSoHuu d ON d.CCCD_ChuSoHuu=a.CCCD WHERE a.Loai=" + (i + 1);
            }
            sql += @") SELECT l.CCCD, a.Loai, a.DuNo, a.NgayMo, a.KyHan, n.NhomNo
 FROM A a JOIN L l ON l.STK=a.STK AND l.Loai=a.Loai
 LEFT JOIN dbo." + p + @"NhomNoTaiKhoan n ON n.SoTaiKhoan=a.STK
 WHERE (@ma IS NULL OR l.CCCD=@ma)";
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            DateTime ngay = DateTime.Now;
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.Add("@ma", System.Data.SqlDbType.VarChar, 12).Value = ma == null ? (object)DBNull.Value : ma;
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        string c = Convert.ToString(r[0]);
                        int? saved = r.IsDBNull(5) ? (int?)null : Convert.ToInt32(r[5]);
                        int nhom = TinhNhomKhoanVay(saved, Convert.ToInt32(r[1]) == 1 ? Convert.ToDecimal(r[2]) : 0m,
                            Convert.ToDateTime(r[3]), Convert.ToString(r[4]), ngay);
                        int old;
                        if (!result.TryGetValue(c, out old) || nhom > old) result[c] = nhom;
                    }
            }
            return result;
        }

        private static int LayNhom(SqlConnection cn, SqlTransaction tx, bool dn, string ma)
        {
            if (string.IsNullOrWhiteSpace(ma)) throw new InvalidOperationException("Chưa xác định CCCD/MST cần xét duyệt.");
            int n;
            return DocNhomKhachHang(cn, tx, dn, ma.Trim()).TryGetValue(ma.Trim(), out n) ? n : 1;
        }
        private static int LayNhom(string connStr, bool dn, string ma)
        {
            using (var cn = new SqlConnection(connStr)) { cn.Open(); return LayNhom(cn, null, dn, ma); }
        }

        // Kiểm tra chỉ khi mở khoản vay, không sửa nhóm nợ của tài khoản cũ.
        internal static void KiemTraNhomNoTruocMoVay(SqlConnection cn, SqlTransaction tx,
            bool dn, params string[] danhSachMa)
        {
            foreach (string ma in danhSachMa.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                int nhom = LayNhom(cn, tx, dn, ma);
                if (nhom >= 3)
                    throw new InvalidOperationException("Khách hàng " + ma + " thuộc nhóm nợ " + nhom +
                        ". Không được mở thêm khoản vay, kể cả vay bằng sổ tiết kiệm.");
            }
        }

        internal static bool ChoPhepMoVayTheoNhomNo(string connStr, bool dn, params string[] danhSachMa)
        {
            try
            {
                using (var cn = new SqlConnection(connStr))
                {
                    cn.Open();
                    KiemTraNhomNoTruocMoVay(cn, null, dn, danhSachMa);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể mở khoản vay", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        internal static int LayNhomNoKhiXetVay(string connStr, bool dn, string ma)
        {
            // Khung đăng ký mới có thể chưa nhập mã; xác nhận lưu vẫn bắt buộc kiểm tra mã thật.
            if (string.IsNullOrWhiteSpace(ma)) return 1;
            return LayNhom(connStr, dn, ma);
        }


        #region Đọc dữ liệu từ SQL Server QLTKNH
        private void LoadDataFromSql()
        {
            ++_phienBanDuLieu;
            try
            {
                ApDungDuLieuSql(DocDuLieuTuSql());
                _daTaiLanDau = true;
                _lanTaiGanNhatUtc = DateTime.UtcNow;
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


                    "SELECT CCCD, HoTen, NgaySinh, SoDienThoai, NoTruocDo FROM dbo.KhachHang ORDER BY HoTen, CCCD", conn))
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

                // 2. Đọc danh sách Đồng sở hữu
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai FROM dbo.DongSoHuu ORDER BY Id", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string chuCCCD = DbText(r, "CCCD_ChuSoHuu");
                        string coCCCD = DbText(r, "CCCD");
                        string coHoTen = DbText(r, "HoTen");
                        string coSDT = DbText(r, "SoDienThoai");
                        // Nếu CCCD cũng là khách hàng chính, ưu tiên thông tin đã đồng bộ ở KhachHang.
                        if (theoCCCD.TryGetValue(coCCCD, out KhachHangData khDongSoHuu))
                        {
                            if (!string.IsNullOrWhiteSpace(khDongSoHuu.HoTen)) coHoTen = khDongSoHuu.HoTen;
                            if (!string.IsNullOrWhiteSpace(khDongSoHuu.SoDienThoai)) coSDT = khDongSoHuu.SoDienThoai;
                        }

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

                        // Tạo bản ghi cho Đồng sở hữu nếu chưa có trong bảng KhachHang
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
                             FROM dbo.TaiKhoanVay v
                             LEFT JOIN (SELECT SoTKVay, SUM(ISNULL(SoTienLai,0) + ISNULL(PhiPhat,0)) AS LaiVaPhi
                                        FROM dbo.LichSuGiaoDichVay
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
                    @"SELECT SoTKTK, CCCD, SoTienGui, LaiSuat, SoTienDaRut, KyHan, TrangThai, NgayMo, NgayCapNhatTT, ISNULL(TongTienDaNhan,0) AS TongTienDaNhan FROM dbo.SoTietKiem ORDER BY SoTKTK", conn))
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
                    @"SELECT SoTKTT, CCCD, SoDu, TrangThai, NgayMo FROM dbo.TaiKhoanThanhToan ORDER BY SoTKTT", conn))
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
                // FilterCustomerList tự chọn khách hàng đầu tiên. Reset phải bỏ chọn
                // sau khi dựng danh sách và xóa đồng thời cả ba bảng tài khoản.
                selectedKhachHang = null;
                DisplayCustomerDetails();
                foreach (var grid in new[] { dgvKhachHang, dgvVay, dgvTietKiem, dgvThanhToan })
                {
                    grid.CurrentCell = null;
                    grid.ClearSelection();
                }
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
            DataGridView[] grids = { dgvKhachHang, dgvVay, dgvTietKiem, dgvThanhToan };
            foreach (var grid in grids) grid.SuspendLayout();
            try { FilterCustomerListCore(filterCCCD, highlightSelected); }
            finally { foreach (var grid in grids) grid.ResumeLayout(true); }
        }

        private void FilterCustomerListCore(string filterCCCD, bool highlightSelected)
        {
            dgvKhachHang.Rows.Clear();
            var filtered = string.IsNullOrWhiteSpace(filterCCCD)
                ? listKhachHang
                : listKhachHang.Where(k => k.CCCD.Contains(filterCCCD)).ToList();

            int stt = 1;
            var visibleIds = new HashSet<string>(filtered.Select(k => k.CCCD), StringComparer.OrdinalIgnoreCase);
            var children = filtered.Where(k => k.IsDongSoHuuOnly && !string.IsNullOrEmpty(k.ChuSoHuuCCCD) && visibleIds.Contains(k.ChuSoHuuCCCD))
                .ToLookup(k => k.ChuSoHuuCCCD, StringComparer.OrdinalIgnoreCase);
            var shown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Action<KhachHangData> add = kh =>
            {
                if (!shown.Add(kh.CCCD)) return;
                int idx = dgvKhachHang.Rows.Add(stt++, kh.CCCD, kh.HoTen, kh.SoDienThoai);
                var cell = dgvKhachHang.Rows[idx].Cells[2];
                cell.Style.ForeColor = kh.IsDongSoHuuOnly ? Color.FromArgb(192, 132, 252) : Color.FromArgb(96, 165, 250);
                cell.Style.SelectionForeColor = kh.IsDongSoHuuOnly ? Color.FromArgb(233, 213, 255) : Color.FromArgb(219, 234, 254);
                if (!kh.IsDongSoHuuOnly) cell.Style.Font = _fontPhanCapCaNhan;
                if (kh.IsDongSoHuuOnly) cell.ToolTipText = "Đồng sở hữu | Chủ liên kết: " + kh.ChuSoHuuCCCD;
            };
            foreach (var kh in filtered)
            {
                if (kh.IsDongSoHuuOnly && !string.IsNullOrEmpty(kh.ChuSoHuuCCCD) && visibleIds.Contains(kh.ChuSoHuuCCCD)) continue;
                add(kh);
                foreach (var child in children[kh.CCCD]) add(child);
            }
            foreach (var kh in filtered) add(kh);

            if (filtered.Count > 0)
            {
                if (selectedKhachHang == null || !filtered.Contains(selectedKhachHang))
                {
                    // Không tự chọn lại khách hàng khi làm mới danh sách không có bộ lọc.
                    selectedKhachHang = string.IsNullOrWhiteSpace(filterCCCD) ? null : filtered[0];
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
                    if (FrmVay.dictCustomNhomNo.TryGetValue(vay.SoTKVay, out int customCode))
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
                    if (FrmVay.dictCustomNhomNo.TryGetValue(tk.SoTKTK, out int customCode))
                    {
                        if (customCode > maxCode) maxCode = customCode;
                    }
                }

                foreach (var tt in kh.DanhSachThanhToan)
                {
                    if (FrmVay.dictCustomNhomNo.TryGetValue(tt.SoTKTT, out int customCode))
                    {
                        if (customCode > maxCode) maxCode = customCode;
                    }
                }
            }
            return Math.Max(0, maxCode - 1);
        }

        private void BtnTimCCCD_Click(object sender, EventArgs e)
        {
            if (!_daTaiLanDau)
            {
                YeuCauDongBoSql();
                MessageBox.Show("Chưa tải được danh sách khách hàng. Vui lòng thử lại sau khi dữ liệu được tải.");
                return;
            }
            string cccd = txtCCCDSearch.Text.Trim();
            if (string.IsNullOrEmpty(cccd))
            {
                MessageBox.Show("Vui lòng nhập số CCCD cần tìm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                if (cccd.Length != 12 || !cccd.All(c => c >= '0' && c <= '9'))
                {
                    MessageBox.Show("Vui lòng nhập đúng CCCD 12 chữ số để đăng ký mới.");
                    return;
                }
                if (MessageBox.Show("Không tìm thấy khách hàng. Bạn có muốn đăng ký và mở tài khoản mới?",
                    "Không tìm thấy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    OpenRegistrationDialog(cccd);
            }
        }

        // Lưu tài khoản mới và lịch sử trong cùng một transaction.
        private bool LuuTaiKhoanMoiSql(KhachHangData kh, string stk, bool tietKiem,
            decimal tien = 0m, double lai = 0, string kyHan = "")
        {
            try
            {
                var nv = BankSecurity.Current;
                if (nv == null || string.IsNullOrWhiteSpace(nv.HoTen))
                    throw new InvalidOperationException("Không xác định được nhân viên đăng nhập.");
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var tr = cn.BeginTransaction(System.Data.IsolationLevel.Serializable))
                    {
                        using (var cmd = new SqlCommand(@"IF EXISTS (SELECT 1 FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk)
 OR EXISTS (SELECT 1 FROM dbo.SoTietKiem WHERE SoTKTK=@stk)
 OR EXISTS (SELECT 1 FROM dbo.TaiKhoanVay WHERE SoTKVay=@stk)
 THROW 52001, N'Số tài khoản đã tồn tại. Vui lòng đổi số.', 1;
 IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD=@c)
 INSERT INTO dbo.KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo) VALUES(@c,@n,@p,0);", cn, tr))
                        {
                            cmd.Parameters.AddWithValue("@stk", stk);
                            cmd.Parameters.AddWithValue("@c", kh.CCCD);
                            cmd.Parameters.AddWithValue("@n", kh.HoTen);
                            cmd.Parameters.AddWithValue("@p", kh.SoDienThoai);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand(tietKiem ? @"
 INSERT INTO dbo.SoTietKiem(SoTKTK,CCCD,SoTienGui,LaiSuat,SoTienDaRut,KyHan,TrangThai,NgayMo,NgayCapNhatTT,SoTienGocBanDau,TongTienDaNhan)
 VALUES(@stk,@c,@t,@lai,0,@ky,0,GETDATE(),GETDATE(),@t,0);
 INSERT INTO dbo.LichSuGiaoDichTietKiem(SoTKTK,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,TongTienNhan,GhiChu,NguoiThucHien)
 VALUES(@stk,GETDATE(),N'Mở sổ tiết kiệm',@t,0,0,N'Mở sổ tiết kiệm',@nv);" : @"
 INSERT INTO dbo.TaiKhoanThanhToan(SoTKTT,CCCD,SoDu,TrangThai,NgayMo,NgayCapNhatTT)
 VALUES(@stk,@c,0,0,GETDATE(),GETDATE());
 INSERT INTO dbo.LichSuGiaoDich(SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
 VALUES(@stk,N'Mở tài khoản mới',NULL,0,GETDATE(),@nv);", cn, tr))
                        {
                            cmd.Parameters.AddWithValue("@stk", stk);
                            cmd.Parameters.AddWithValue("@c", kh.CCCD);
                            cmd.Parameters.AddWithValue("@t", tien);
                            cmd.Parameters.AddWithValue("@lai", lai);
                            cmd.Parameters.AddWithValue("@ky", kyHan);
                            cmd.Parameters.AddWithValue("@nv", nv.HoTen);
                            cmd.ExecuteNonQuery();
                        }
                        foreach (var co in kh.DanhSachDongSoHuu)
                        {
                            using (var cmd = new SqlCommand(@"IF NOT EXISTS (SELECT 1 FROM dbo.DongSoHuu WHERE CCCD_ChuSoHuu=@chu AND CCCD=@c)
 INSERT INTO dbo.DongSoHuu(CCCD_ChuSoHuu,CCCD,HoTen,SoDienThoai) VALUES(@chu,@c,@n,@p);" +
                                (tietKiem ? @"
 INSERT INTO dbo.DongSoHuuSoTietKiem(SoTKTK,CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p);" : @"
 INSERT INTO dbo.DongSoHuuTaiKhoanThanhToan(SoTKTT,CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p);"), cn, tr))
                            {
                                cmd.Parameters.AddWithValue("@chu", kh.CCCD);
                                cmd.Parameters.AddWithValue("@stk", stk);
                                cmd.Parameters.AddWithValue("@c", co.CCCD);
                                cmd.Parameters.AddWithValue("@n", co.HoTen);
                                cmd.Parameters.AddWithValue("@p", co.SoDienThoai);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tr.Commit();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu tài khoản vào SQL: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Chỉ đổi màu hộp thoại mở tài khoản, kể cả control được tạo động khi đổi loại vay.

        // Tra cứu độc lập với bộ lọc giao diện. Chỉ dùng cho thao tác tìm/mở mới cá nhân.
        internal sealed class KetQuaTimMoMoiCaNhan
        {
            public bool CoTaiKhoan;
            public bool CoHoSoChinh;
            public bool CoDanhTinh;
            public string HoTen = "";
            public string SoDienThoai = "";
        }

        private static bool CoBangTimMoMoiCaNhan(SqlConnection cn, SqlTransaction tx, string bang)
        {
            using (var cmd = new SqlCommand("SELECT OBJECT_ID(@b,N'U')", cn, tx))
            {
                cmd.Parameters.Add("@b", System.Data.SqlDbType.NVarChar, 128).Value = bang;
                var v = cmd.ExecuteScalar();
                return v != null && v != DBNull.Value;
            }
        }

        // Tên bảng/cột chỉ lấy từ danh sách cố định bên dưới, không lấy từ người dùng.
        private static readonly string[,] BangLienKetTimMoMoiCaNhan = {
            { "dbo.DongSoHuuTaiKhoanThanhToan", "dbo.TaiKhoanThanhToan", "SoTKTT" },
            { "dbo.DongSoHuuSoTietKiem", "dbo.SoTietKiem", "SoTKTK" },
            { "dbo.DongSoHuuTaiKhoanVay", "dbo.TaiKhoanVay", "SoTKVay" }
        };

        internal static KetQuaTimMoMoiCaNhan TraCuuTimMoMoiCaNhan(string connStr, string cccd, string loai)
        {
            var ketQua = new KetQuaTimMoMoiCaNhan();
            using (var cn = new SqlConnection(connStr))
            {
                cn.Open();
                var bangDanhTinh = new List<string> { "dbo.KhachHang", "dbo.DongSoHuu" };
                for (int i = 0; i < 3; i++)
                    if (CoBangTimMoMoiCaNhan(cn, null, BangLienKetTimMoMoiCaNhan[i, 0]))
                        bangDanhTinh.Add(BangLienKetTimMoMoiCaNhan[i, 0]);

                foreach (string bang in bangDanhTinh)
                {
                    using (var cmd = new SqlCommand("SELECT TOP (1) HoTen, SoDienThoai FROM " + bang + " WHERE CCCD=@c", cn))
                    {
                        cmd.Parameters.Add("@c", System.Data.SqlDbType.VarChar, 12).Value = cccd;
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read()) continue;
                            ketQua.CoDanhTinh = true;
                            ketQua.CoHoSoChinh = bang == "dbo.KhachHang";
                            ketQua.HoTen = Convert.ToString(rd[0]);
                            ketQua.SoDienThoai = Convert.ToString(rd[1]);
                            break;
                        }
                    }
                }

                int idx = loai == "Thanh toán" ? 0 : loai == "Tiết kiệm" ? 1 : 2;
                string bangTK = BangLienKetTimMoMoiCaNhan[idx, 1];
                string bangDSH = BangLienKetTimMoMoiCaNhan[idx, 0];
                string cot = BangLienKetTimMoMoiCaNhan[idx, 2];
                // Tôn trọng liên kết theo STK nếu có; dùng liên kết cũ cho CSDL chưa có bảng riêng.
                string lienKet = bangDanhTinh.Contains(bangDSH)
                    ? "EXISTS(SELECT 1 FROM " + bangDSH + " d WHERE d." + cot + "=t." + cot + " AND d.CCCD=@c)"
                    : "EXISTS(SELECT 1 FROM dbo.DongSoHuu d WHERE d.CCCD_ChuSoHuu=t.CCCD AND d.CCCD=@c)";
                using (var cmd = new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM " + bangTK +
                    " t WHERE t.CCCD=@c OR " + lienKet + ") THEN 1 ELSE 0 END", cn))
                {
                    cmd.Parameters.Add("@c", System.Data.SqlDbType.VarChar, 12).Value = cccd;
                    ketQua.CoTaiKhoan = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
            }
            return ketQua;
        }

        internal static bool KiemTraThongTinTimMoMoiCaNhan(string cccd, string ten, string sdt)
        {
            if (cccd.Length == 12 && cccd.All(c => c >= '0' && c <= '9') &&
                !string.IsNullOrWhiteSpace(ten) && ten.Length <= 100 &&
                sdt.Length >= 10 && sdt.Length <= 11 && sdt.All(c => c >= '0' && c <= '9')) return true;
            MessageBox.Show("Vui lòng nhập CCCD 12 số, họ tên hợp lệ và số điện thoại 10–11 chữ số.",
                "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // Kiểm tra lại khách hàng hoàn toàn mới trong transaction ghi tài khoản.
        // Không dùng điều kiện này để chặn người đã có vai trò đồng sở hữu.
        internal static void KiemTraTrungKhachHangTimMoMoiCaNhan(SqlConnection cn, SqlTransaction tx, string cccd, string sdt)
        {
            var bang = new List<string> { "dbo.KhachHang", "dbo.DongSoHuu" };
            for (int i = 0; i < 3; i++)
                if (CoBangTimMoMoiCaNhan(cn, tx, BangLienKetTimMoMoiCaNhan[i, 0]))
                    bang.Add(BangLienKetTimMoMoiCaNhan[i, 0]);
            foreach (string b in bang)
                using (var cmd = new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM " + b +
                    " WITH (UPDLOCK,HOLDLOCK) WHERE CCCD=@c OR SoDienThoai=@p) THEN 1 ELSE 0 END", cn, tx))
                {
                    cmd.Parameters.Add("@c", System.Data.SqlDbType.VarChar, 12).Value = cccd;
                    cmd.Parameters.Add("@p", System.Data.SqlDbType.VarChar, 11).Value = sdt;
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                        throw new InvalidOperationException("CCCD hoặc số điện thoại đã tồn tại. Hãy tìm lại thông tin khách hàng.");
                }
        }

        internal static void DongBoMauKhungMoTaiKhoan(Control control)
        {
            Color chu = Color.FromArgb(226, 232, 240);
            if (control is TextBoxBase || control is ComboBox || control is NumericUpDown)
            {
                control.BackColor = Color.FromArgb(30, 41, 59);
                control.ForeColor = chu;
                if (control is ComboBox cbo) cbo.FlatStyle = FlatStyle.Flat;
            }
            else if (control is Button nut)
            {
                bool huy = nut.Text.IndexOf("Hủy", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           nut.Text.IndexOf("Hủy bỏ", StringComparison.OrdinalIgnoreCase) >= 0;
                nut.BackColor = huy ? Color.FromArgb(30, 41, 59) : Color.FromArgb(37, 99, 235);
                nut.ForeColor = Color.White;
                nut.UseVisualStyleBackColor = false;
                nut.FlatStyle = FlatStyle.Flat;
                nut.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);
            }
            else if (control is Label lbl)
            {
                lbl.BackColor = Color.Transparent;
                if (lbl.ForeColor == Color.Blue) lbl.ForeColor = Color.FromArgb(96, 165, 250);
                else if (lbl.ForeColor == Color.Red) lbl.ForeColor = Color.FromArgb(248, 113, 113);
                else lbl.ForeColor = chu;
            }
            else if (control is RadioButton || control is CheckBox)
            {
                control.BackColor = Color.FromArgb(15, 23, 42);
                control.ForeColor = chu;
            }
            else if (control is Form || control is Panel || control is GroupBox)
            {
                control.BackColor = Color.FromArgb(15, 23, 42);
                control.ForeColor = chu;
            }
            foreach (Control con in control.Controls) DongBoMauKhungMoTaiKhoan(con);
            control.ControlAdded += (sender, args) => DongBoMauKhungMoTaiKhoan(args.Control);
        }

        private void OpenRegistrationDialog(string initialCCCD, KhachHangData khachHangDaCo = null)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = khachHangDaCo == null ? "Đăng Ký Khách Hàng & Mở Tài Khoản Mới" : "Thêm Tài Khoản Cho Khách Hàng";
                dlg.Size = new Size(620, 900);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                DongBoMauKhungMoTaiKhoan(dlg);

                Label lblCCCD = CreateLabelDesign2();
                TextBox txtCCCD = CreateTextBoxDynamicDesign1(initialCCCD, khachHangDaCo != null);
                txtCCCD.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                Label lblHoTen = CreateLabelDesign3();
                TextBox txtHoTen = CreateTextBoxDynamicDesign2(khachHangDaCo?.HoTen ?? "", khachHangDaCo != null);
                Label lblSĐT = CreateLabelDesign4();
                TextBox txtSĐT = CreateTextBoxDynamicDesign3(khachHangDaCo?.SoDienThoai ?? "", khachHangDaCo != null);
                txtSĐT.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);

                Button btnThemDongSoHuu = CreateButtonDesign5();
                btnThemDongSoHuu.FlatAppearance.BorderColor = colorActiveBlue;
                NumericUpDown nudSoLuongDongSoHuu = CreateNumericUpDownDesign6();
                Panel pnlDongSoHuu = CreatePanelDesign7();
                List<TextBox> dsCCCDDongSoHuu = new List<TextBox>();
                List<TextBox> dsHoTenDongSoHuu = new List<TextBox>();
                List<TextBox> dsSDTDongSoHuu = new List<TextBox>();

                Action RenderDongSoHuu = () =>
                {
                    pnlDongSoHuu.Controls.Clear(); dsCCCDDongSoHuu.Clear(); dsHoTenDongSoHuu.Clear(); dsSDTDongSoHuu.Clear();
                    for (int i = 0; i < (int)nudSoLuongDongSoHuu.Value; i++)
                    {
                        GroupBox group = CreateGroupBoxDynamicDesign4($"Đồng sở hữu {i + 1}", new Point(8, 8 + i * 64));
                        Label l1 = CreateLabelDesign8();
                        TextBox t1 = CreateTextBoxDesign9(); t1.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                        Label l2 = CreateLabelDesign10();
                        TextBox t2 = CreateTextBoxDesign11();
                        Label l3 = CreateLabelDesign12();
                        TextBox t3 = CreateTextBoxDesign13(); t3.KeyPress += (s, ev) => ev.Handled = !char.IsControl(ev.KeyChar) && !char.IsDigit(ev.KeyChar);
                        group.Controls.AddRange(new Control[] { l1, t1, l2, t2, l3, t3 }); pnlDongSoHuu.Controls.Add(group);
                        dsCCCDDongSoHuu.Add(t1); dsHoTenDongSoHuu.Add(t2); dsSDTDongSoHuu.Add(t3);
                    }
                };
                btnThemDongSoHuu.Click += (s, ev) => { nudSoLuongDongSoHuu.Visible = true; pnlDongSoHuu.Visible = true; RenderDongSoHuu(); };
                nudSoLuongDongSoHuu.ValueChanged += (s, ev) => { pnlDongSoHuu.Visible = true; RenderDongSoHuu(); };

                Label lblLoaiTK = CreateLabelDesign14();
                ComboBox cboLoaiTK = CreateComboBoxDesign15(); cboLoaiTK.Items.AddRange(new object[] { "Thanh toán", "Vay", "Tiết kiệm" }); cboLoaiTK.SelectedIndex = 0;
                RadioButton rdoNgauNhien = CreateRadioButtonDesign16();
                RadioButton rdoSoDep = CreateRadioButtonDesign17();
                Label lblPhiSoDep = CreateLabelDesign18();
                Label lblSTK = CreateLabelDesign19();
                TextBox txtSTK = CreateTextBoxDesign20();
                Panel pnlSubForm = CreatePanelDesign21();
                Action genSTK = () => { Random r = new Random(); txtSTK.Text = rdoSoDep.Checked ? "88889999" + r.Next(10, 99) : "10" + r.Next(10000000, 99999999); };
                rdoSoDep.CheckedChanged += (s, ev) => { lblPhiSoDep.Visible = rdoSoDep.Checked; genSTK(); }; rdoNgauNhien.CheckedChanged += (s, ev) => genSTK(); genSTK();

                Action CapNhatViTri = () =>
                {
                    int offset = pnlDongSoHuu.Visible ? 220 : 0;
                    lblLoaiTK.Location = new Point(20, 160 + offset); cboLoaiTK.Location = new Point(150, 158 + offset); rdoNgauNhien.Location = new Point(20, 195 + offset); rdoSoDep.Location = new Point(180, 195 + offset); lblPhiSoDep.Location = new Point(340, 197 + offset); lblSTK.Location = new Point(20, 230 + offset); txtSTK.Location = new Point(150, 228 + offset); pnlSubForm.Location = new Point(20, 265 + offset);
                };
                pnlDongSoHuu.VisibleChanged += (s, ev) => CapNhatViTri();

                KhachHangData newKH = null;
                Func<bool> KiemTra = () =>
                {
                    if (txtCCCD.Text.Length != 12 || string.IsNullOrWhiteSpace(txtHoTen.Text)) { MessageBox.Show("Vui lòng nhập đủ 12 số CCCD và Họ tên hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    if (txtSĐT.Text.Length < 10 || txtSĐT.Text.Length > 11) { MessageBox.Show("Số điện thoại phải bao gồm từ 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    HashSet<string> cccd = new HashSet<string>(StringComparer.OrdinalIgnoreCase), sdt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KhachHangData kh in listKhachHang) { cccd.Add(kh.CCCD ?? ""); sdt.Add(kh.SoDienThoai ?? ""); foreach (DongSoHuuData ds in kh.DanhSachDongSoHuu) { cccd.Add(ds.CCCD ?? ""); sdt.Add(ds.SoDienThoai ?? ""); } }
                    if (khachHangDaCo == null)
                    {
                        if (cccd.Contains(txtCCCD.Text.Trim())) { MessageBox.Show("CCCD của khách hàng đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (sdt.Contains(txtSĐT.Text.Trim())) { MessageBox.Show("Số điện thoại của khách hàng đã tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    }
                    // Kiểm tra cả thông tin chủ sở hữu vừa nhập, tránh trùng trong cùng hồ sơ mới.
                    cccd.Add(txtCCCD.Text.Trim());
                    sdt.Add(txtSĐT.Text.Trim());
                    for (int i = 0; i < dsCCCDDongSoHuu.Count; i++)
                    {
                        string c = dsCCCDDongSoHuu[i].Text.Trim(), n = dsHoTenDongSoHuu[i].Text.Trim(), p = dsSDTDongSoHuu[i].Text.Trim();
                        if (c.Length != 12 || string.IsNullOrWhiteSpace(n)) { MessageBox.Show($"Vui lòng nhập đúng CCCD 12 số và Họ tên cho Đồng sở hữu {i + 1}!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (p.Length < 10 || p.Length > 11) { MessageBox.Show($"Số điện thoại của Đồng sở hữu {i + 1} phải gồm 10 đến 11 chữ số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (!cccd.Add(c)) { MessageBox.Show($"CCCD của Đồng sở hữu {i + 1} đã tồn tại hoặc bị trùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                        if (!sdt.Add(p)) { MessageBox.Show($"Số điện thoại của Đồng sở hữu {i + 1} đã tồn tại hoặc bị trùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
                    }
                    return true;
                };
                Action TaoKhachHang = () =>
                {
                    newKH = new KhachHangData { CCCD = txtCCCD.Text.Trim(), HoTen = txtHoTen.Text.Trim(), SoDienThoai = txtSĐT.Text.Trim() };
                    newKH.IsDongSoHuuOnly = false;
                    if (khachHangDaCo != null)
                    {
                        newKH.CCCD = txtCCCD.Text.Trim();
                        newKH.HoTen = txtHoTen.Text.Trim();
                        newKH.SoDienThoai = txtSĐT.Text.Trim();
                    }
                    for (int i = 0; i < dsCCCDDongSoHuu.Count; i++)
                        newKH.DanhSachDongSoHuu.Add(new DongSoHuuData { CCCD = dsCCCDDongSoHuu[i].Text.Trim(), HoTen = dsHoTenDongSoHuu[i].Text.Trim(), SoDienThoai = dsSDTDongSoHuu[i].Text.Trim(), CCCD_ChuSoHuu = newKH.CCCD });
                };

                Action renderSubForm = () =>
                {
                    pnlSubForm.Controls.Clear(); string selectedType = cboLoaiTK.SelectedItem.ToString();
                    if (selectedType == "Thanh toán")
                    {
                        Button btnSubmit = CreateButtonDesign22();
                        btnSubmit.Click += (s, ev) => { if (!KiemTra()) return; TaoKhachHang(); if (!LuuTaiKhoanMoiSql(newKH, txtSTK.Text, false)) return; newKH.DanhSachThanhToan.Add(new TaiKhoanThanhToanItem { SoTKTT = txtSTK.Text, SoDu = 0 }); MessageBox.Show($"Mở tài khoản thanh toán thành công với STK: {txtSTK.Text}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); dlg.DialogResult = DialogResult.OK; }; pnlSubForm.Controls.Add(btnSubmit);
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
                                    lblMax.ForeColor = Color.FromArgb(248, 113, 113);
                                    lblMax.Text = "Khách hàng thuộc nhóm nợ 3-5: Không đủ điều kiện mở khoản vay mới!";
                                    return;
                                }

                                lblMax.ForeColor = Color.FromArgb(96, 165, 250);
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
                                Label lblTenTS = CreateLabelDesign42();
                                Label lblDinhGiaTS = CreateLabelDesign43();
                                // Hai dòng tài sản cố định nằm riêng, không đè lên các ô sổ thế chấp.
                                lblTenTS.Location = new Point(10, 48);
                                txtTS.Location = new Point(180, 46);
                                lblDinhGiaTS.Location = new Point(10, 88);
                                txtDG.Location = new Point(180, 86);
                                lblDinhGiaTS.Text = "Định giá TS (VNĐ):";
                                txtDG.Width = Math.Max(150, txtTS.Width);
                                pnlVayBody.Controls.AddRange(new Control[] { lblTenTS, txtTS, lblDinhGiaTS, txtDG });
                                pnlVayBody.Controls.Add(CreateLabelDesign44());
                                cboNhomNo.Location = new Point(180, 164);
                                pnlVayBody.Controls.Add(cboNhomNo);
                                Action switchLoai = () =>
                                {
                                    bool ts = cboLoaiTheChap.SelectedIndex == 0;
                                    bool noi = cboLoaiTheChap.SelectedIndex == 1;
                                    bool ngoai = cboLoaiTheChap.SelectedIndex == 2;
                                    txtTS.Visible = txtDG.Visible = ts;
                                    lblTenTS.Visible = lblDinhGiaTS.Visible = ts;
                                    if (ts) { lblTenTS.BringToFront(); txtTS.BringToFront(); lblDinhGiaTS.BringToFront(); txtDG.BringToFront(); }
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
                            pnlVayBody.AutoScroll = true;
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
                                    !FrmMain.ChoPhepMoVayTheoNhomNo(_connStr, false, txtCCCD.Text.Trim())) return;
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
                                if (nv == null || string.IsNullOrWhiteSpace(nv.HoTen))
                                {
                                    MessageBox.Show("Không xác định được nhân viên đăng nhập.");
                                    return;
                                }
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
                                                using (var check = new SqlCommand(@"SELECT
                                                    (SELECT COUNT(*) FROM dbo.SoTietKiem WHERE SoTKTK=@stk) +
                                                    (SELECT COUNT(*) FROM dbo.TaiKhoanVay WHERE SoTKVay=@stk) +
                                                    (SELECT COUNT(*) FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk)", cn, tr))
                                                {
                                                    check.Parameters.Add("@stk", System.Data.SqlDbType.VarChar, 20).Value = txtSTK.Text;
                                                    if (Convert.ToInt32(check.ExecuteScalar()) != 0)
                                                        throw new InvalidOperationException("Số tài khoản vừa tạo đã tồn tại. Vui lòng đổi số và thử lại.");
                                                }
                                                if (khachHangDaCo == null || khachHangDaCo.IsDongSoHuuOnly)
                                                {
                                                    using (var insertKH = new SqlCommand(@"INSERT INTO dbo.KhachHang
                                                        (CCCD, HoTen, SoDienThoai, NoTruocDo) VALUES (@cccd,@ten,@sdt,0)", cn, tr))
                                                    {
                                                        insertKH.Parameters.Add("@cccd", System.Data.SqlDbType.VarChar, 12).Value = newKH.CCCD;
                                                        insertKH.Parameters.Add("@ten", System.Data.SqlDbType.NVarChar, 100).Value = newKH.HoTen;
                                                        insertKH.Parameters.Add("@sdt", System.Data.SqlDbType.VarChar, 11).Value = newKH.SoDienThoai;
                                                        insertKH.ExecuteNonQuery();
                                                    }
                                                }
                                                FrmMain.KiemTraNhomNoTruocMoVay(cn, tr, false, newKH.CCCD);
                                                if (!isTinChap && cboLoaiTheChap.SelectedIndex > 0)
                                                    FrmMain.KiemTraSoTietKiemChuaVay(cn, tr, false, cboLoaiTheChap.SelectedIndex == 2,
                                                        cboLoaiTheChap.SelectedIndex == 1 ? ((SoTietKiemTheChap)cboSoNoiBo.SelectedItem).SoTKTK : txtSoNgoai.Text.Trim());
                                                using (var insertVay = new SqlCommand(@"INSERT INTO dbo.TaiKhoanVay
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
                                                    using (var insertCo = new SqlCommand(@"INSERT INTO dbo.DongSoHuu
                                                        (CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai)
                                                        VALUES (@chu,@c,@n,@p)", cn, tr))
                                                    {
                                                        insertCo.Parameters.Add("@chu", System.Data.SqlDbType.VarChar, 12).Value = newKH.CCCD;
                                                        insertCo.Parameters.Add("@c", System.Data.SqlDbType.VarChar, 12).Value = co.C;
                                                        insertCo.Parameters.Add("@n", System.Data.SqlDbType.NVarChar, 100).Value = co.N;
                                                        insertCo.Parameters.Add("@p", System.Data.SqlDbType.VarChar, 11).Value = co.P;
                                                        insertCo.ExecuteNonQuery();
                                                    }
                                                }
                                                using (var history = new SqlCommand(@"INSERT INTO dbo.LichSuGiaoDichVay
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
                        btnOk.Click += (s, ev) => { if (!decimal.TryParse(txtGui.Text, out decimal valGui) || valGui <= 0) { MessageBox.Show("Số tiền gửi không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } if (!KiemTra()) return; TaoKhachHang(); double[] rates = { 6.60, 6.80, 6.90, 7.00, 7.10 }; string[] khs = { "6 tháng", "12 tháng", "18 tháng", "24 tháng", "36 tháng" }; if (!LuuTaiKhoanMoiSql(newKH, txtSTK.Text, true, valGui, rates[cboKH.SelectedIndex], khs[cboKH.SelectedIndex])) return; newKH.DanhSachTietKiem.Add(new SoTietKiemItem { SoTKTK = txtSTK.Text, SoTienGui = valGui, LaiSuat = rates[cboKH.SelectedIndex], SoTienDaRut = 0, KyHan = khs[cboKH.SelectedIndex] }); MessageBox.Show("Đã mở tài khoản tiết kiệm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); dlg.DialogResult = DialogResult.OK; }; btnCancel.Click += (s, ev) => dlg.Close(); pnlSubForm.Controls.AddRange(new Control[] { btnOk, btnCancel });
                    }
                };

                cboLoaiTK.SelectedIndexChanged += (s, ev) => renderSubForm();
                dlg.Controls.AddRange(new Control[] { lblCCCD, txtCCCD, lblHoTen, txtHoTen, lblSĐT, txtSĐT, btnThemDongSoHuu, nudSoLuongDongSoHuu, pnlDongSoHuu, lblLoaiTK, cboLoaiTK, rdoNgauNhien, rdoSoDep, lblPhiSoDep, lblSTK, txtSTK, pnlSubForm });
                renderSubForm(); CapNhatViTri();
                if (dlg.ShowDialog() == DialogResult.OK && newKH != null)
                {
                    // Đọc lại dữ liệu đã commit, không đưa bản ghi tạm vào danh sách.
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

            Form frmKH = CreateFormInstance(typeof(FrmKhachHang),
                new object[] { this, selectedKhachHang.CCCD },
                new object[] { selectedKhachHang.CCCD },
                new object[] { this });

            SetFormProperty(frmKH, "SelectedCCCD", selectedKhachHang.CCCD);
            SetFormProperty(frmKH, "CCCD", selectedKhachHang.CCCD);
            SubscribeDataChanged(frmKH);

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

            // 1. Tìm Chủ sở hữu chính của nhóm (nếu khách hàng đang chọn là Đồng sở hữu)
            KhachHangData ownerByRel = listKhachHang.FirstOrDefault(k =>
                k.DanhSachDongSoHuu.Any(d => d.CCCD.Equals(selectedKhachHang.CCCD, StringComparison.OrdinalIgnoreCase)));

            if (ownerByRel == null && !string.IsNullOrEmpty(selectedKhachHang.ChuSoHuuCCCD))
            {
                ownerByRel = listKhachHang.FirstOrDefault(k =>
                    k.CCCD.Equals(selectedKhachHang.ChuSoHuuCCCD, StringComparison.OrdinalIgnoreCase));
            }

            bool isSelectedIsMainOwner = !selectedKhachHang.IsDongSoHuuOnly
                && (ownerByRel == null || ownerByRel.CCCD.Equals(selectedKhachHang.CCCD, StringComparison.OrdinalIgnoreCase));

            KhachHangData mainKH = isSelectedIsMainOwner ? selectedKhachHang : ownerByRel;
            var dsDongSoHuu = mainKH != null ? mainKH.DanhSachDongSoHuu : new List<DongSoHuuData>();

            // Chỉ hiển thị khách hàng đang chọn; không hiển thị dòng tổng hợp
            // "CHỦ SỞ HỮU ... | ĐỒNG SỞ HỮU ..." ở phía trên các bảng.
            lblInfoSelectedCustomer.Text =
                $"ĐANG CHỌN: {selectedKhachHang.HoTen.ToUpper()} ({selectedKhachHang.CCCD})";

            // 3. Xác định chuỗi cột "Vai trò" dựa trên vai trò của khách hàng đang được chọn
            string vaiTroTrenTaiKhoanChung;
            if (isSelectedIsMainOwner)
            {
                string dsTenDongSH = dsDongSoHuu.Count > 0
                    ? string.Join(", ", dsDongSoHuu.Select(d => d.HoTen))
                    : "Không có";
                vaiTroTrenTaiKhoanChung = $"Chủ sở hữu (Đồng sở hữu: {dsTenDongSH})";
            }
            else
            {
                string tenChuSH = (mainKH != null && !string.IsNullOrWhiteSpace(mainKH.HoTen))
                    ? mainKH.HoTen
                    : "Không có";
                vaiTroTrenTaiKhoanChung = $"Đồng sở hữu (Chủ sở hữu: {tenChuSH})";
            }

            var displayVayList = new List<(TaiKhoanVayItem Item, string VaiTro)>();
            var displayTKList = new List<(SoTietKiemItem Item, string VaiTro)>();
            var displayTTList = new List<(TaiKhoanThanhToanItem Item, string VaiTro)>();

            // Lấy các tài khoản thuộc Chủ sở hữu chính (nếu có)
            if (mainKH != null)
            {
                foreach (var item in mainKH.DanhSachVay) displayVayList.Add((item, vaiTroTrenTaiKhoanChung));
                foreach (var item in mainKH.DanhSachTietKiem) displayTKList.Add((item, vaiTroTrenTaiKhoanChung));
                foreach (var item in mainKH.DanhSachThanhToan) displayTTList.Add((item, vaiTroTrenTaiKhoanChung));
            }

            // Lấy các tài khoản đứng tên riêng của khách hàng đang chọn (nếu họ là Đồng sở hữu nhưng có tài khoản riêng)
            if (!isSelectedIsMainOwner)
            {
                string tenDongSHCuaRieng = selectedKhachHang.DanhSachDongSoHuu.Count > 0
                    ? string.Join(", ", selectedKhachHang.DanhSachDongSoHuu.Select(d => d.HoTen))
                    : "Không có";
                string vaiTroRieng = $"Chủ sở hữu (Đồng sở hữu: {tenDongSHCuaRieng})";

                foreach (var item in selectedKhachHang.DanhSachVay)
                {
                    if (!displayVayList.Any(x => x.Item.SoTKVay == item.SoTKVay))
                        displayVayList.Add((item, vaiTroRieng));
                }
                foreach (var item in selectedKhachHang.DanhSachTietKiem)
                {
                    if (!displayTKList.Any(x => x.Item.SoTKTK == item.SoTKTK))
                        displayTKList.Add((item, vaiTroRieng));
                }
                foreach (var item in selectedKhachHang.DanhSachThanhToan)
                {
                    if (!displayTTList.Any(x => x.Item.SoTKTT == item.SoTKTT))
                        displayTTList.Add((item, vaiTroRieng));
                }
            }

            // Lấy thêm tài khoản của các đồng sở hữu khác trong cùng nhóm (nếu có)
            foreach (var coOwner in dsDongSoHuu)
            {
                var coKh = listKhachHang.FirstOrDefault(k => k.CCCD.Equals(coOwner.CCCD, StringComparison.OrdinalIgnoreCase));
                if (coKh != null && !coKh.CCCD.Equals(selectedKhachHang.CCCD, StringComparison.OrdinalIgnoreCase))
                {
                    string vaiTroTaiKhoanDongSHKhac = $"Đồng sở hữu (Chủ sở hữu: {coKh.HoTen})";

                    foreach (var item in coKh.DanhSachVay)
                    {
                        if (!displayVayList.Any(x => x.Item.SoTKVay == item.SoTKVay))
                            displayVayList.Add((item, vaiTroTaiKhoanDongSHKhac));
                    }
                    foreach (var item in coKh.DanhSachTietKiem)
                    {
                        if (!displayTKList.Any(x => x.Item.SoTKTK == item.SoTKTK))
                            displayTKList.Add((item, vaiTroTaiKhoanDongSHKhac));
                    }
                    foreach (var item in coKh.DanhSachThanhToan)
                    {
                        if (!displayTTList.Any(x => x.Item.SoTKTT == item.SoTKTT))
                            displayTTList.Add((item, vaiTroTaiKhoanDongSHKhac));
                    }
                }
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
            Form frmVay = CreateFormInstance(typeof(FrmVay),
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
            SubscribeDataChanged(frmVay);

            OpenChildForm(frmVay);
            ApplyAccountFilterToChildForm(frmVay, selectedKhachHang.CCCD, listSoTK);
        }

        private void MoFormTietKiemSelected(List<string> listSoTK)
        {
            if (selectedKhachHang == null || listSoTK == null || listSoTK.Count == 0) return;

            string firstSoTK = listSoTK[0];
            Form frmTK = CreateFormInstance(typeof(FrmTietKiem),
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
            SubscribeDataChanged(frmTK);

            OpenChildForm(frmTK);
            ApplyAccountFilterToChildForm(frmTK, selectedKhachHang.CCCD, listSoTK);
        }

        private void MoFormThanhToanSelected(List<string> listSoTK)
        {
            if (selectedKhachHang == null || listSoTK == null || listSoTK.Count == 0) return;

            string firstSoTK = listSoTK[0];
            Form frmTT = CreateFormInstance(typeof(FrmThanhToan),
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
            SubscribeDataChanged(frmTT);
            frmTT.FormClosed += (s, e) => YeuCauDongBoSql();

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
                    EventHandler handler = (s, e) => YeuCauDongBoSql();
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
                frmTarget = CreateFormInstance(typeof(FrmThanhToan), new object[] { this, realCCCD, listSoTK }, new object[] { this, realCCCD, realSoTK }, new object[] { realCCCD, realSoTK }, new object[] { this, realCCCD }, new object[] { realCCCD });
            }
            else if (maLoaiTK == "TK_TIETKIEM" || maLoaiTK == "Tiết kiệm")
            {
                frmTarget = CreateFormInstance(typeof(FrmTietKiem), new object[] { this, realCCCD, listSoTK }, new object[] { this, realCCCD, realSoTK }, new object[] { realCCCD, realSoTK }, new object[] { this, realCCCD }, new object[] { realCCCD });
            }
            else if (maLoaiTK == "TK_VAY" || maLoaiTK == "Vay")
            {
                frmTarget = CreateFormInstance(typeof(FrmVay), new object[] { this.listKhachHang, realCCCD, listSoTK }, new object[] { this.listKhachHang, realCCCD, realSoTK }, new object[] { this, realCCCD, realSoTK }, new object[] { realCCCD, realSoTK }, new object[] { this.listKhachHang, realCCCD }, new object[] { this.listKhachHang });
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
                SubscribeDataChanged(frmTarget);
                OpenChildForm(frmTarget);
                ApplyAccountFilterToChildForm(frmTarget, realCCCD, listSoTK);
            }
        }

        private void btnCaNhan_Click(object sender, EventArgs e)
        {
            SwitchTab("CaNhan");
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
        // Dùng tên lớp để FrmMain vẫn biên dịch được khi chưa thêm đủ các form DN.
        private Form TaoFormDoanhNghiep(string tenLop)
        {
            Type loai = typeof(FrmMain).Assembly.GetType("QLTKNH." + tenLop, false);
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
                    currentMode == "DoanhNghiep" ? TaoFormDoanhNghiep("FrmMainDN") : null)),

                Tuple.Create("Khách hàng", "👤", (Func<Form>)(() =>
                    currentMode == "DoanhNghiep" ? TaoFormDoanhNghiep("FrmKhachHangDN") : new FrmKhachHang())),

                Tuple.Create("Tài khoản vay", "💰", (Func<Form>)(() =>
                {
                    if (currentMode == "DoanhNghiep") return TaoFormDoanhNghiep("FrmVayDN");
                    var frmVay = CreateFormInstance(typeof(FrmVay), new object[] { this.listKhachHang });
                    SubscribeDataChanged(frmVay);
                    return frmVay;
                })),

                Tuple.Create("Sổ tiết kiệm", "🏦", (Func<Form>)(() =>
                    currentMode == "DoanhNghiep" ? TaoFormDoanhNghiep("FrmTietKiemDN") : new FrmTietKiem())),

                Tuple.Create("Tài khoản thanh toán", "💳", (Func<Form>)(() =>
                    currentMode == "DoanhNghiep" ? TaoFormDoanhNghiep("FrmThanhToanDN") : new FrmThanhToan())),

                Tuple.Create("Loại tài khoản", "🏷️", (Func<Form>)(() =>
                    currentMode == "DoanhNghiep" ? TaoFormDoanhNghiep("FrmLoaiTaiKhoanDN") : new FrmLoaiTaiKhoan())),

                Tuple.Create("Lịch sử", "📜", (Func<Form>)(() =>
                    currentMode == "DoanhNghiep" ? TaoFormDoanhNghiep("FrmLichSuDN") : new FrmLichSu())),

                Tuple.Create("Thống kê", "📈", (Func<Form>)(() =>
                    currentMode == "DoanhNghiep" ? (Form)new FrmThongKeDN() : new FrmThongKe())),
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
                    if (f == null && !(currentMode == "CaNhan" && tenMuc == "Trang chính"))
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
            panelMainLayout.BringToFront();
        }

        // Điều hướng lịch sử từ hai nút trong chi tiết tài khoản thanh toán.
        // Đặt trực tiếp ở FrmMain, không phụ thuộc file DieuHuongLichSuThanhToan.cs.
        public void MoLichSuTuThanhToan(Form formNguon, bool laDoanhNghiep,
            string soTaiKhoan, bool laGiaoDich, string connectionString)
        {
            MoLichSuTuTaiKhoan(formNguon, laDoanhNghiep, "ThanhToan", soTaiKhoan,
                laGiaoDich, connectionString);
        }

        // Dùng chung cho Thanh toán, Tiết kiệm và Vay. Chỉ chuyển danh mục,
        // không phát sinh thay đổi dữ liệu hoặc nghiệp vụ SQL.
        public void MoLichSuTuTaiKhoan(Form formNguon, bool laDoanhNghiep,
            string loaiTaiKhoan, string soTaiKhoan, bool laGiaoDich,
            string connectionString = null)
        {
            if (IsDisposed || Disposing || string.IsNullOrWhiteSpace(soTaiKhoan)) return;

            string maKhachHang = string.Empty;
            try
            {
                // Tên bảng được chọn từ hai giá trị cố định, không ghép chuỗi từ người dùng.
                string bang;
                string cotSoTaiKhoan;
                switch ((loaiTaiKhoan ?? "").Trim().ToLowerInvariant())
                {
                    case "thanhtoan":
                        bang = laDoanhNghiep ? "dbo.DN_TaiKhoanThanhToan" : "dbo.TaiKhoanThanhToan";
                        cotSoTaiKhoan = "SoTKTT";
                        break;
                    case "tietkiem":
                        bang = laDoanhNghiep ? "dbo.DN_SoTietKiem" : "dbo.SoTietKiem";
                        cotSoTaiKhoan = "SoTKTK";
                        break;
                    case "vay":
                        bang = laDoanhNghiep ? "dbo.DN_TaiKhoanVay" : "dbo.TaiKhoanVay";
                        cotSoTaiKhoan = "SoTKVay";
                        break;
                    default:
                        throw new ArgumentException("Loại tài khoản không được hỗ trợ.", "loaiTaiKhoan");
                }
                string ketNoi = string.IsNullOrWhiteSpace(connectionString)
                    ? _connStr : connectionString;
                using (var cn = new SqlConnection(ketNoi))
                using (var cmd = new SqlCommand(
                    "SELECT CCCD FROM " + bang + " WHERE " + cotSoTaiKhoan + " = @stk", cn))
                {
                    cmd.Parameters.Add("@stk", System.Data.SqlDbType.VarChar, 50).Value = soTaiKhoan;
                    cn.Open();
                    object value = cmd.ExecuteScalar();
                    if (value != null && value != DBNull.Value)
                        maKhachHang = Convert.ToString(value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không lấy được CCCD/MST của tài khoản " +
                    soTaiKhoan + ": " + ex.Message +
                    "\nVẫn mở Lịch sử để bạn tự tìm kiếm.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Chi tiết tài khoản đang mở bằng ShowDialog; đóng trước khi đổi trang.
            Form chiTiet = Form.ActiveForm;
            if (chiTiet != null && chiTiet != this && chiTiet != formNguon &&
                chiTiet.Modal && !chiTiet.IsDisposed)
                chiTiet.Close();

            string maLoc = maKhachHang;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || Disposing) return;
                try
                {
                    string cheDo = laDoanhNghiep ? "DoanhNghiep" : "CaNhan";
                    if (currentMode != cheDo) SwitchTab(cheDo);

                    Form lichSu = laDoanhNghiep
                        ? TaoFormDoanhNghiep("FrmLichSuDN") : (Form)new FrmLichSu();
                    if (lichSu == null) return;
                    try
                    {
                        // Hiển thị form trước: PerformClick trước Show() có thể không phát sinh Click.
                        // Chọn chế độ trực tiếp bằng API của form Lịch sử, không dựa vào nút ẩn.
                        OpenChildForm(lichSu);
                        if (lichSu is FrmLichSu lichSuCaNhan)
                            lichSuCaNhan.ChonCheDoLichSu(laGiaoDich, maLoc);
                        else if (lichSu is FrmLichSuDN lichSuDoanhNghiep)
                            lichSuDoanhNghiep.ChonCheDoLichSu(laGiaoDich, maLoc);
                        else
                            throw new InvalidOperationException("Form lịch sử chưa hỗ trợ chọn chế độ.");
                    }
                    catch
                    {
                        lichSu.Dispose();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể chuyển đến Lịch sử: " + ex.Message,
                        "Lỗi điều hướng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        public void OpenChildForm(Form childForm)
        {
            if (activeChildForm != null)
            {
                activeChildForm.Close();
            }

            activeChildForm = childForm;
            // FrmMainDN là trang tổng quan doanh nghiệp, không phải cửa sổ có menu thứ hai.
            // Chỉ ẩn thanh menu của FrmMainDN khi nó được nhúng bên trong FrmMain.
            if (childForm is FrmMainDN trangChinhDN)
                trangChinhDN.HienThiNoiDungTrongFrmMain();
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

            SyncSidebarMenuWithChildForm(childForm);
        }

        private void SyncSidebarMenuWithChildForm(Form childForm)
        {
            if (childForm == null) return;

            string targetMenu = null;
            string formTypeName = childForm.GetType().Name;

            if (formTypeName == "FrmVay" || formTypeName == "FrmVayDN") targetMenu = "Tài khoản vay";
            else if (formTypeName == "FrmTietKiem" || formTypeName == "FrmTietKiemDN") targetMenu = "Sổ tiết kiệm";
            else if (formTypeName == "FrmThanhToan" || formTypeName == "FrmThanhToanDN") targetMenu = "Tài khoản thanh toán";
            else if (formTypeName == "FrmKhachHang" || formTypeName == "FrmKhachHangDN") targetMenu = "Khách hàng";
            else if (formTypeName == "FrmLoaiTaiKhoan" || formTypeName == "FrmLoaiTaiKhoanDN") targetMenu = "Loại tài khoản";
            else if (formTypeName == "FrmLichSu" || formTypeName == "FrmLichSuDN") targetMenu = "Lịch sử";
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
    }
}