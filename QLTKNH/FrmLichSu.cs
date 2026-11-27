using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace QLTKNH
{
    public partial class FrmLichSu : Form
    {
        private readonly string _connStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        private List<LichSuItem> listAllHistory = new List<LichSuItem>();
        // Hai chế độ hiển thị; không xóa hay sửa lịch sử đã lưu trong SQL.
        private bool _dangXemGiaoDich = true;
        private Button _btnGiaoDich, _btnHoatDong;
        private Panel _panelCheDo;
        private Panel _thanhInGiaoDich;
        private TableLayoutPanel _boCucLichSu;

        public class LichSuItem
        {
            public DateTime NgayGiaoDich { get; set; }
            public string LoaiTaiKhoan { get; set; }
            public string SoTaiKhoan { get; set; }
            public string CCCD { get; set; }
            public string HoTen { get; set; }
            public string LoaiGiaoDich { get; set; }
            public decimal SoTien { get; set; }
            public decimal? SoDuSau { get; set; }
            public string NguoiThucHien { get; set; }
            public string GhiChu { get; set; }
            public string LoaiLichSu { get; set; }
        }

        public FrmLichSu()
        {
            InitializeComponent();
            DongBoGiaoDienLichSu();
            KhoiTaoHaiCheDo();
            KhoiTaoXuatPdfLichSu();
            dgvLichSu.CellMouseDoubleClick += DgvLichSu_CellMouseDoubleClick;
            ThemNutInToanBoGiaoDich();
            SapXepBoCucLichSu();
            _timerLocLichSu.Tick += (s, e) => { _timerLocLichSu.Stop(); ApplyFilter(); };
            Disposed += (s, e) => _timerLocLichSu.Dispose();
            Load += (s, e) => LoadAllHistoryFromSQL();
            ApplyFilter();
        }

        // Hai nút nằm dưới bộ lọc, giữ nguyên cấu trúc Designer để không ảnh hưởng form khác.
        private void KhoiTaoHaiCheDo()
        {
            _panelCheDo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 47,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(16, 6, 8, 6)
            };
            _btnGiaoDich = new Button
            {
                Text = "Lịch sử giao dịch",
                Name = "btnLichSuGiaoDich",
                Location = new Point(16, 5),
                Size = new Size(190, 36),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnHoatDong = new Button
            {
                Text = "Lịch sử hoạt động",
                Name = "btnLichSuHoatDong",
                Location = new Point(214, 5),
                Size = new Size(190, 36),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnGiaoDich.FlatAppearance.BorderSize = 0;
            _btnHoatDong.FlatAppearance.BorderSize = 0;
            _btnGiaoDich.Click += (sender, args) => ChuyenCheDo(true);
            _btnHoatDong.Click += (sender, args) => ChuyenCheDo(false);
            _panelCheDo.Controls.Add(_btnGiaoDich);
            _panelCheDo.Controls.Add(_btnHoatDong);
            // Bố cục sẽ được xây dựng trong SapXepBoCucLichSu(),
            // tránh việc DataGridView nằm sau thanh lọc và che các STT đầu tiên.
            ToMauHaiCheDo();
        }

        // Cố định 3 vùng độc lập: nút chế độ + bộ lọc / bảng / thanh in.
        // Không cho bảng nằm dưới panel lọc dù thay đổi kích thước cửa sổ.
        private void SapXepBoCucLichSu()
        {
            Controls.Remove(panelFilter);
            Controls.Remove(dgvLichSu);
            if (_thanhInGiaoDich != null) Controls.Remove(_thanhInGiaoDich);

            _boCucLichSu = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = NenChinh
            };
            _boCucLichSu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _boCucLichSu.RowStyles.Add(new RowStyle(SizeType.Absolute, 147F));
            _boCucLichSu.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _boCucLichSu.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

            var header = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = NenChinh };
            _panelCheDo.Dock = DockStyle.None;
            _panelCheDo.Location = new Point(0, 0);
            _panelCheDo.Height = 47;
            panelFilter.Dock = DockStyle.None;
            panelFilter.Location = new Point(0, 47);
            panelFilter.Height = 100;
            header.Controls.Add(_panelCheDo);
            header.Controls.Add(panelFilter);
            GanLamMoiVungTrongLichSu(header);
            header.Resize += (sender, args) =>
            {
                _panelCheDo.Width = header.ClientSize.Width;
                panelFilter.Width = header.ClientSize.Width;
            };

            dgvLichSu.Dock = DockStyle.Fill;
            dgvLichSu.Margin = Padding.Empty;
            dgvLichSu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLichSu.ScrollBars = ScrollBars.Both;
            if (_thanhInGiaoDich != null)
            {
                _thanhInGiaoDich.Dock = DockStyle.Fill;
                _thanhInGiaoDich.Margin = Padding.Empty;
            }

            _boCucLichSu.Controls.Add(header, 0, 0);
            _boCucLichSu.Controls.Add(dgvLichSu, 0, 1);
            if (_thanhInGiaoDich != null)
                _boCucLichSu.Controls.Add(_thanhInGiaoDich, 0, 2);
            Controls.Add(_boCucLichSu);
            _boCucLichSu.BringToFront();
            CapNhatQuyenIn();
        }

        // API điều hướng dùng chung từ FrmMain cho Thanh toán / Tiết kiệm / Vay.
        // Không gọi PerformClick khi nút chưa được hiển thị: chọn trực tiếp chế độ.
        public void ChonCheDoLichSu(bool laGiaoDich, string maKhachHang = null)
        {
            if (IsDisposed || Disposing) return;
            if (!string.IsNullOrWhiteSpace(maKhachHang) && txtCCCD.Text != maKhachHang)
                txtCCCD.Text = maKhachHang;

            if (_dangXemGiaoDich != laGiaoDich)
                ChuyenCheDo(laGiaoDich);
            else
            {
                // Bảo đảm lọc cả khi chọn tab đang mở hoặc thay mã khách hàng.
                ToMauHaiCheDo();
                CapNhatQuyenIn();
                LoadAllHistoryFromSQL();
                ApplyFilter();
            }
        }

        private void ChuyenCheDo(bool giaoDich)
        {
            if (_dangXemGiaoDich == giaoDich) return;
            _dangXemGiaoDich = giaoDich;
            LoadAllHistoryFromSQL();
            ToMauHaiCheDo();
            CapNhatQuyenIn();
            ApplyFilter();
        }

        private void ToMauHaiCheDo()
        {
            if (_btnGiaoDich == null || _btnHoatDong == null) return;
            _btnGiaoDich.BackColor = _dangXemGiaoDich ? XanhChinh : NenThe;
            _btnHoatDong.BackColor = _dangXemGiaoDich ? NenThe : XanhChinh;
            _btnGiaoDich.ForeColor = Color.White;
            _btnHoatDong.ForeColor = Color.White;
        }

        // Lịch sử hoạt động chỉ được xem; mọi nút in và xuất đều bị chặn.
        private void CapNhatQuyenIn()
        {
            if (_thanhInGiaoDich != null && !_thanhInGiaoDich.IsDisposed)
                _thanhInGiaoDich.Visible = _dangXemGiaoDich;
            if (_boCucLichSu != null)
                _boCucLichSu.RowStyles[2].Height = _dangXemGiaoDich ? 48F : 0F;
        }

        // Bỏ dấu để nhận đúng các biến thể nội dung lịch sử đã lưu (kể cả tiếng Việt).
        private static string KhongDau(string value)
        {
            string source = (value ?? "").Replace('đ', 'd').Replace('Đ', 'D')
                                        .Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(source.Length);
            foreach (char c in source)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    result.Append(char.ToLowerInvariant(c));
            return result.ToString().Normalize(NormalizationForm.FormC);
        }

        // Theo yêu cầu: mở/khóa/mở khóa là hoạt động; nạp/rút/chuyển/in sao kê là giao dịch.
        // Các hành động tài chính khác (trả nợ, tất toán, giải ngân...) là giao dịch.
        private sealed class PhanLoaiCache
        {
            public string LoaiTK, NoiDung, LoaiLS;
            public bool KetQua;
        }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<LichSuItem, PhanLoaiCache> _cachePhanLoai =
            new System.Runtime.CompilerServices.ConditionalWeakTable<LichSuItem, PhanLoaiCache>();

        private static bool LaLichSuGiaoDich(LichSuItem item)
        {
            if (item == null) return false;
            PhanLoaiCache cache;
            if (_cachePhanLoai.TryGetValue(item, out cache) && cache.LoaiTK == item.LoaiTaiKhoan &&
                cache.NoiDung == item.LoaiGiaoDich && cache.LoaiLS == item.LoaiLichSu)
                return cache.KetQua;
            bool ketQua = PhanLoaiLichSuGoc(item);
            _cachePhanLoai.Remove(item);
            _cachePhanLoai.Add(item, new PhanLoaiCache
            {
                LoaiTK = item.LoaiTaiKhoan,
                NoiDung = item.LoaiGiaoDich,
                LoaiLS = item.LoaiLichSu,
                KetQua = ketQua
            });
            return ketQua;
        }

        private static bool PhanLoaiLichSuGoc(LichSuItem item)
        {
            if (item == null) return false;
            string action = " " + KhongDau(item.LoaiGiaoDich).Trim() + " ";

            // Ưu tiên tiền tố nghiệp vụ quản lý, kể cả bản ghi cũ có từ "gửi tiền".
            string noiDung = action.Trim();
            string[] tienToHoatDong = {
                "mo tai khoan", "mo them tai khoan", "mo so", "mo tk", "mo khoa",
                "dong tai khoan", "dong so", "dong tk", "dong khoan vay", "da dong tai khoan",
                "khoa tai khoan", "khoa tam thoi", "tam khoa", "khoa so",
                "chuyen giao quyen", "chuyen quyen", "chuyen nhom no", "chuyen trang thai",
                "sang ten", "cap lai", "huy so", "xu ly rui ro", "vay bang so tiet kiem",
                "dang ky han muc", "dang ky dich vu", "thay doi thong tin", "sua thong tin",
                "phong toa", "go phong toa"
            };
            if (tienToHoatDong.Any(tu => noiDung == tu || noiDung.StartsWith(tu + " ", StringComparison.Ordinal)
                || noiDung.StartsWith(tu + ":", StringComparison.Ordinal) || noiDung.StartsWith(tu + "(", StringComparison.Ordinal)))
                return false;

            // Nhận diện giao dịch thanh toán theo tiền tố: bao gồm Chuyển ngoài CITAD/NAPAS.
            // Loại trừ các hành động quản lý cũng bắt đầu bằng "Chuyển".
            if (item.LoaiTaiKhoan == "Thanh toán" && action.TrimStart().StartsWith("chuyen ")
                && !action.Contains("chuyen quyen")
                && !action.Contains("chuyen nhom no")
                && !action.Contains("chuyen trang thai"))
                return true;

            // Các cách gọi chuyển tiền: "Chuyển ngoài (CITAD/NAPAS)",
            // "Chuyển khoản", "Chuyển nội bộ", "Chuyển tiền"...
            // PHẢI kiểm tra trước cột LoaiLichSu vì SQL cũ có thể gán sai Hoạt động.
            string[] giaoDichChacChan = {
                "chuyen ngoai", "chuyen noi bo", "chuyen lien ngan hang",
                "chuyen khoan", "chuyen tien", "chuyen den", "chuyen di",
                "citad", "napas", "chuyen nhanh", "chuyen cung ngan hang",
                "nap tien", "nop tien", "rut tien", "rut lai", "rut goc",
                "rut 1 phan", "rut mot phan", "rut toan bo", "gui them", "gui tien",
                "tra goc", "tra lai", "tra theo ky", "tra no", "tra het", "tra truoc",
                "thanh toan khoan vay", "thanh toan no", "giai ngan",
                "tat toan", "thu phi", "phi phat", "nhan tien",
                "in sao ke", "sao ke", "lai dinh ky"
            };
            foreach (string tu in giaoDichChacChan)
                if (action.Contains(tu)) return true;

            string[] hoatDongChacChan = {
                "mo tai khoan", "mo them tai khoan", "mo so", "mo tk",
                "dong tai khoan", "dong so", "tam khoa", "mo khoa",
                "khoa tai khoan", "khoa so", "phong toa", "go phong toa",
                "cap lai", "sang ten", "chuyen quyen", "sua thong tin",
                "thay doi thong tin", "cap nhat thong tin", "dang ky han muc",
                "dang ky dich vu", "dieu chinh han muc", "chuyen nhom no",
                "xu ly rui ro", "dieu chinh lai suat", "thiet lap",
                "huy dang ky", "thay doi trang thai"
            };
            foreach (string tu in hoatDongChacChan)
                if (action.Contains(tu)) return false;

            // Chỉ với nội dung không nhận diện được mới dùng phân loại SQL.
            string nhanSql = KhongDau(item.LoaiLichSu).Trim();
            if (nhanSql == "giao dich") return true;
            if (nhanSql == "hoat dong") return false;
            return false; // Chưa xác định: chỉ xem, không in.
        }

        private List<LichSuItem> LayDanhSachTheoBoLoc()
        {
            string cccd = txtCCCD.Text.Trim();
            string moc = cboMocThoiGian.SelectedItem?.ToString() ?? "Tất cả";
            string loai = cboLoaiTK.SelectedItem?.ToString() ?? "Tất cả";
            DateTime homNay = DateTime.Today;
            IEnumerable<LichSuItem> ds = listAllHistory.Where(x => LaLichSuGiaoDich(x) == _dangXemGiaoDich);
            if (!string.IsNullOrWhiteSpace(cccd))
                ds = _locChinhXacKhachHang
                    ? ds.Where(x => string.Equals((x.CCCD ?? "").Trim(), cccd, StringComparison.Ordinal))
                    : ds.Where(x => (x.CCCD ?? "").Contains(cccd));
            if (loai != "Tất cả")
                ds = ds.Where(x => x.LoaiTaiKhoan == loai);
            switch (moc)
            {
                case "Hôm nay": ds = ds.Where(x => x.NgayGiaoDich.Date == homNay); break;
                case "Tháng này": ds = ds.Where(x => x.NgayGiaoDich.Year == homNay.Year && x.NgayGiaoDich.Month == homNay.Month); break;
                case "Năm này": ds = ds.Where(x => x.NgayGiaoDich.Year == homNay.Year); break;
                case "Tùy chọn":
                    DateTime tu = dtpTuNgay.Value.Date, den = dtpDenNgay.Value.Date;
                    ds = ds.Where(x => x.NgayGiaoDich.Date >= tu && x.NgayGiaoDich.Date <= den);
                    break;
            }
            return ds.OrderByDescending(x => x.NgayGiaoDich).ToList();
        }

        // Chỉ đồng bộ màu sắc/kiểu hiển thị, không thay đổi bố cục hoặc sự kiện Designer.
        private static readonly Color NenChinh = Color.FromArgb(15, 23, 42);
        private static readonly Color NenThe = Color.FromArgb(30, 41, 59);
        private static readonly Color NenNhap = Color.FromArgb(23, 33, 55);
        private static readonly Color XanhChinh = Color.FromArgb(37, 99, 235);
        private static readonly Color XanhTieuDe = Color.FromArgb(96, 165, 250);
        private static readonly Color ChuPhu = Color.FromArgb(160, 174, 192);

        private void DongBoGiaoDienLichSu()
        {
            BackColor = NenChinh;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            DinhDangCacControl(this);
            DinhDangBangLichSu(dgvLichSu);
            // Chỉ tô màu chữ theo loại tài khoản; không thay đổi dữ liệu hoặc logic lọc.
            dgvLichSu.CellFormatting += ToMauLoaiTaiKhoan;
            dgvLichSu.FontChanged += (s, e) =>
            {
                if (_fontLoaiTaiKhoan != null) { _fontLoaiTaiKhoan.Dispose(); _fontLoaiTaiKhoan = null; }
            };
            Disposed += (s, e) => { if (_fontLoaiTaiKhoan != null) _fontLoaiTaiKhoan.Dispose(); };
            // Giam nhap nhay khi cuon va ve lai bang.
            try
            {
                var prop = typeof(DataGridView).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(dgvLichSu, true, null);
            }
            catch (Exception) { /* Van dung cach ve mac dinh neu khong ho tro. */ }
        }

        private Font _fontLoaiTaiKhoan;
        private void ToMauLoaiTaiKhoan(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView bang = sender as DataGridView;
            if (bang == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string tenCot = bang.Columns[e.ColumnIndex].Name;
            if (tenCot != "colLoaiTK" && tenCot != "colLoaiTaiKhoan" && tenCot != "LoaiTK"
                && !string.Equals(bang.Columns[e.ColumnIndex].HeaderText, "Loại TK", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(bang.Columns[e.ColumnIndex].HeaderText, "Loại tài khoản", StringComparison.OrdinalIgnoreCase)) return;

            string loai = Convert.ToString(e.Value)?.Trim() ?? "";
            Color mau;
            if (string.Equals(loai, "Vay", StringComparison.OrdinalIgnoreCase))
                mau = Color.FromArgb(248, 113, 113); // Đỏ san hô: vay
            else if (string.Equals(loai, "Tiết kiệm", StringComparison.OrdinalIgnoreCase))
                mau = Color.FromArgb(74, 222, 128); // Xanh lá: tiết kiệm
            else if (string.Equals(loai, "Thanh toán", StringComparison.OrdinalIgnoreCase))
                mau = Color.FromArgb(56, 189, 248); // Xanh cyan: thanh toán
            else return;

            e.CellStyle.ForeColor = mau;
            e.CellStyle.SelectionForeColor = mau;
            if (_fontLoaiTaiKhoan == null) _fontLoaiTaiKhoan = new Font(bang.Font, FontStyle.Bold);
            e.CellStyle.Font = _fontLoaiTaiKhoan;
        }

        private static void DinhDangCacControl(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is DataGridView bang)
                {
                    DinhDangBangLichSu(bang);
                }
                else if (control is Button nut)
                {
                    nut.FlatStyle = FlatStyle.Flat;
                    nut.FlatAppearance.BorderSize = 1;
                    nut.FlatAppearance.BorderColor = Color.FromArgb(65, 82, 112);
                    nut.BackColor = NenThe;
                    nut.ForeColor = Color.White;
                    nut.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    nut.Cursor = Cursors.Hand;
                }
                else if (control is TextBox oNhap)
                {
                    oNhap.BackColor = NenNhap;
                    oNhap.ForeColor = Color.White;
                    oNhap.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is ComboBox danhSach)
                {
                    danhSach.BackColor = NenNhap;
                    danhSach.ForeColor = Color.White;
                    danhSach.FlatStyle = FlatStyle.Flat;
                }
                else if (control is DateTimePicker chonNgay)
                {
                    chonNgay.CalendarMonthBackground = NenThe;
                    chonNgay.CalendarForeColor = Color.White;
                    chonNgay.CalendarTitleBackColor = XanhChinh;
                    chonNgay.CalendarTitleForeColor = Color.White;
                }
                else if (control is GroupBox nhom)
                {
                    nhom.BackColor = NenChinh;
                    nhom.ForeColor = XanhTieuDe;
                    nhom.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (control is Panel khung)
                {
                    khung.BackColor = NenChinh;
                }
                else if (control is Label nhan)
                {
                    nhan.ForeColor = nhan.Font.Bold ? XanhTieuDe : ChuPhu;
                    nhan.BackColor = Color.Transparent;
                }
                if (control.HasChildren) DinhDangCacControl(control);
            }
        }

        private static void DinhDangBangLichSu(DataGridView bang)
        {
            bang.BackgroundColor = NenChinh;
            bang.BorderStyle = BorderStyle.None;
            bang.GridColor = Color.FromArgb(49, 68, 106);
            bang.EnableHeadersVisualStyles = false;
            bang.ColumnHeadersDefaultCellStyle.BackColor = NenThe;
            bang.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            bang.ColumnHeadersDefaultCellStyle.SelectionBackColor = NenThe;
            bang.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            bang.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            bang.DefaultCellStyle.BackColor = NenChinh;
            bang.DefaultCellStyle.ForeColor = Color.White;
            bang.DefaultCellStyle.SelectionBackColor = XanhChinh;
            bang.DefaultCellStyle.SelectionForeColor = Color.White;
            bang.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(22, 31, 52);
            bang.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
            bang.AlternatingRowsDefaultCellStyle.SelectionBackColor = XanhChinh;
            bang.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
            bang.RowHeadersDefaultCellStyle.BackColor = NenChinh;
            bang.RowHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        // Chuột phải vào hàng nào thì chỉ in hóa đơn của giao dịch thuộc hàng đó.
        // Lưu đối tượng ngay trên DataGridViewRow.Tag để không nhầm các giao dịch trùng thời gian.
        private void KhoiTaoXuatPdfLichSu()
        {
            GanMenuInGiaoDich(dgvLichSu);
        }

        private void GanMenuInGiaoDich(DataGridView bang)
        {
            ContextMenuStrip menu = bang.ContextMenuStrip;
            if (menu == null)
            {
                menu = new ContextMenuStrip();
                bang.ContextMenuStrip = menu;
            }

            ToolStripMenuItem xemChiTiet = new ToolStripMenuItem("Xem chi tiết bản ghi");
            ToolStripMenuItem inHoaDon = new ToolStripMenuItem("In hóa đơn giao dịch tại hàng này");
            LichSuItem giaoDichDuocChon = null;

            bang.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                DataGridView.HitTestInfo hit = bang.HitTest(e.X, e.Y);
                giaoDichDuocChon = null;
                if (hit.RowIndex < 0 || hit.RowIndex >= bang.Rows.Count) return;
                DataGridViewRow hang = bang.Rows[hit.RowIndex];
                if (hang.IsNewRow) return;
                bang.ClearSelection();
                hang.Selected = true;
                if (hit.ColumnIndex >= 0) bang.CurrentCell = hang.Cells[hit.ColumnIndex];
                giaoDichDuocChon = hang.Tag as LichSuItem;
            };

            xemChiTiet.Click += (s, e) =>
            {
                LichSuItem item = giaoDichDuocChon;
                if (item == null) return;
                string thongTin = "Thời gian: " + item.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss")
                    + "\nSố tài khoản: " + item.SoTaiKhoan
                    + "\nLoại tài khoản: " + item.LoaiTaiKhoan
                    + "\nNội dung: " + item.LoaiGiaoDich
                    + "\nSố tiền: " + item.SoTien.ToString("N0") + " VNĐ"
                    + "\nSố dư sau / Dư nợ: " + (item.SoDuSau.HasValue ? item.SoDuSau.Value.ToString("N0") + " VNĐ" : "Chưa ghi nhận")
                    + "\nNgười thực hiện: " + item.NguoiThucHien
                    + "\nGhi chú: " + item.GhiChu;
                MessageBox.Show(thongTin, "Chi tiết lịch sử", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            menu.Opening += (s, e) =>
            {
                xemChiTiet.Enabled = giaoDichDuocChon != null;
                inHoaDon.Visible = _dangXemGiaoDich;
                inHoaDon.Enabled = _dangXemGiaoDich && giaoDichDuocChon != null;
            };

            inHoaDon.Click += (s, e) =>
            {
                if (!_dangXemGiaoDich) return;
                LichSuItem item = giaoDichDuocChon;
                if (item == null || !LaLichSuGiaoDich(item)) return;
                // Giữ cơ chế xuất HTML / In PDF sẵn có, chỉ truyền đúng một giao dịch.
                BeginInvoke(new Action(() => {
                    if (!_dangXemGiaoDich) return;
                    XuatPdfLichSu(
                    new List<LichSuItem> { item },
                    "HÓA ĐƠN GIAO DỊCH",
                    "Số tài khoản: " + item.SoTaiKhoan + " | " + item.LoaiGiaoDich);
                }));
            };
            if (menu.Items.Count > 0) menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(xemChiTiet);
            menu.Items.Add(inHoaDon);
            menu.BackColor = NenThe;
            menu.ForeColor = Color.White;
            menu.RenderMode = ToolStripRenderMode.System;
        }

        // Nút xuất báo cáo toàn bộ giao dịch, độc lập với menu chuột phải in từng dòng.
        // Không thay đổi các sự kiện, dữ liệu và truy vấn có sẵn.
        private void ThemNutInToanBoGiaoDich()
        {
            Panel thanhIn = new Panel
            {
                Name = "pnlInToanBoGiaoDich",
                Dock = DockStyle.Bottom,
                Height = 48,
                Padding = new Padding(8, 6, 16, 6),
                BackColor = NenChinh
            };

            Button btnInToanBo = new Button
            {
                Name = "btnInToanBoGiaoDich",
                Text = "In lịch sử giao dịch",
                Dock = DockStyle.Right,
                Width = 205,
                BackColor = XanhChinh,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnInToanBo.FlatAppearance.BorderSize = 0;
            btnInToanBo.Click += (sender, args) =>
            {
                if (!_dangXemGiaoDich) return;
                XuatPdfLichSu(LayLichSuDangLoc(),
                    "BÁO CÁO LỊCH SỬ GIAO DỊCH",
                    "Các giao dịch theo bộ lọc đang chọn");
            };

            thanhIn.Controls.Add(btnInToanBo);
            Controls.Add(thanhIn);
            _thanhInGiaoDich = thanhIn;
            CapNhatQuyenIn();
            thanhIn.BringToFront();
        }

        // Dùng cùng điều kiện lọc với bảng chính để PDF không chứa các dòng bị ẩn.
        private List<LichSuItem> LayLichSuDangLoc()
        {
            return LayDanhSachTheoBoLoc();
        }

        private static string Html(string value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? "");
        }

        private void XuatPdfLichSu(List<LichSuItem> items, string tieuDe, string moTa)
        {
            if (_loiTaiLichSu || _dangTaiLichSu)
            {
                MessageBox.Show("Dữ liệu chưa được cập nhật từ SQL. Vui lòng làm mới trước khi in.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Chặn cả lời gọi xuất trực tiếp nếu đang xem lịch sử hoạt động.
            if (!_dangXemGiaoDich || (items != null && items.Any(x => !LaLichSuGiaoDich(x))))
            {
                MessageBox.Show("Lịch sử hoạt động chỉ được xem, không được in hoặc xuất PDF.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("Không có giao dịch để xuất PDF.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var sb = new StringBuilder();
                sb.Append(@"<!doctype html><html lang='vi'><head><meta charset='utf-8'><title>Lịch sử giao dịch</title>
<style>
@page { size: A4 landscape; margin: 12mm; }
body { font-family: 'Segoe UI',Arial,sans-serif; color:#17213a; margin:24px; }
h1 { text-align:center; color:#1d4ed8; font-size:22px; margin:8px 0; }
.subtitle { text-align:center; color:#475569; margin-bottom:16px; }
.info { display:flex; justify-content:space-between; font-size:12px; margin-bottom:14px; }
table { width:100%; border-collapse:collapse; font-size:10px; }
th { background:#1e3a8a; color:white; padding:8px 5px; text-align:left; }
td { border:1px solid #cbd5e1; padding:6px 5px; vertical-align:top; overflow-wrap:anywhere; }
tr:nth-child(even) td { background:#f1f5f9; }
.num { text-align:right; white-space:nowrap; }
.footer { margin-top:20px; font-size:11px; color:#64748b; text-align:right; }
@media print { body { margin:0; } thead { display:table-header-group; } tr { break-inside:avoid; } }
</style></head><body>");
                sb.Append("<h1>").Append(Html(tieuDe)).Append("</h1>");
                sb.Append("<div class='subtitle'>").Append(Html(moTa)).Append("</div>");
                sb.Append("<div class='info'><span>Tổng số giao dịch: <b>").Append(items.Count)
                  .Append("</b></span><span>Ngày lập báo cáo: ").Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                  .Append("</span></div>");
                sb.Append("<table><thead><tr><th>STT</th><th>Ngày giao dịch</th><th>Loại TK</th><th>Số tài khoản</th><th>CCCD</th><th>Họ tên</th><th>Loại giao dịch</th><th>Số tiền</th><th>Số dư sau</th><th>Người thực hiện</th><th>Ghi chú</th></tr></thead><tbody>");
                int stt = 0;
                foreach (var item in items)
                {
                    sb.Append("<tr><td>").Append(++stt).Append("</td><td>")
                      .Append(item.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"))
                      .Append("</td><td>").Append(Html(item.LoaiTaiKhoan))
                      .Append("</td><td>").Append(Html(item.SoTaiKhoan))
                      .Append("</td><td>").Append(Html(item.CCCD))
                      .Append("</td><td>").Append(Html(item.HoTen))
                      .Append("</td><td>").Append(Html(item.LoaiGiaoDich))
                      .Append("</td><td class='num'>").Append(item.SoTien == 0 ? "—" : item.SoTien.ToString("N0") + " VNĐ")
                      .Append("</td><td class='num'>").Append(item.SoDuSau.HasValue ? item.SoDuSau.Value.ToString("N0") + " VNĐ" : "Chưa ghi nhận")
                      .Append("</td><td>").Append(Html(item.NguoiThucHien))
                      .Append("</td><td>").Append(Html(item.GhiChu)).Append("</td></tr>");
                }
                sb.Append("</tbody></table><div class='footer'>Báo cáo được lập từ dữ liệu lịch sử giao dịch của hệ thống QLTKNH.</div></body></html>");

                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "Lưu báo cáo lịch sử để in PDF";
                    dialog.Filter = "Trang in HTML (*.html)|*.html";
                    dialog.FileName = "LichSuGiaoDich_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html";
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
                    Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tạo báo cáo PDF: " + ex.Message, "Lỗi xuất báo cáo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmLichSuTongHop_Load(object sender, EventArgs e)
        {
        }

        private static bool CotPhanLoaiTonTai(SqlConnection conn, string tenBang)
        {
            using (var cmd = new SqlCommand("SELECT COL_LENGTH(@tenBang, N'LoaiLichSu')", conn))
            {
                cmd.Parameters.AddWithValue("@tenBang", tenBang);
                return cmd.ExecuteScalar() != DBNull.Value;
            }
        }

        // Cột số dư còn lại được bổ sung bằng script cập nhật an toàn.
        // Nếu chưa chạy script, lịch sử tiết kiệm cũ vẫn mở được bình thường.
        private static bool CoCotSoDuSauTietKiem(SqlConnection conn, string tenBang)
        {
            using (var cmd = new SqlCommand("SELECT COL_LENGTH(@bang, N'SoDuSau')", conn))
            {
                cmd.Parameters.AddWithValue("@bang", tenBang);
                object ketQua = cmd.ExecuteScalar();
                return ketQua != null && ketQua != DBNull.Value;
            }
        }

        private bool _dangTaiLichSu;
        private bool _loiTaiLichSu;
        private async void LoadAllHistoryFromSQL()
        {
            if (_dangTaiLichSu || IsDisposed || Disposing) return;
            _dangTaiLichSu = true;
            bool daBat = Enabled;
            Enabled = false;
            try
            {
                lblThongKe.Text = "Đang tải lịch sử từ SQL...";
                var lichSuMoi = await System.Threading.Tasks.Task.Run(() => DocLichSuTuSql());
                if (IsDisposed || Disposing) return;
                listAllHistory = lichSuMoi;
                _loiTaiLichSu = false;
            }
            catch (Exception ex)
            {
                _loiTaiLichSu = true;
                if (!IsDisposed && !Disposing)
                    MessageBox.Show("Lỗi load lịch sử từ SQL:\n" + ex.Message, "Lỗi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _dangTaiLichSu = false;
                if (!IsDisposed && !Disposing)
                {
                    Enabled = daBat;
                    ApplyFilter();
                }
            }
        }

        private List<LichSuItem> DocLichSuTuSql()
        {
            var lichSuMoi = new List<LichSuItem>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    conn.Open();

                    // 1. Vay
                    string sqlVay = @"
                        SELECT ls.NgayGiaoDich, ls.SoTKVay AS SoTaiKhoan, kh.CCCD, kh.HoTen,
                               ls.LoaiGiaoDich, ls.TongTien AS SoTien, ls.SoDuConLai AS SoDuSau,
                               COALESCE(NULLIF(LTRIM(RTRIM(ls.NguoiThucHien)), N''), N'Chưa xác định') AS NguoiThucHien,
                               ISNULL(ls.GhiChu, '') AS GhiChu, PHAN_LOAI_THAY_THE
                        FROM dbo.LichSuGiaoDichVay ls
                        INNER JOIN dbo.TaiKhoanVay tk ON ls.SoTKVay = tk.SoTKVay
                        INNER JOIN dbo.KhachHang kh ON tk.CCCD = kh.CCCD";
                    sqlVay = sqlVay.Replace("PHAN_LOAI_THAY_THE",
                        (CotPhanLoaiTonTai(conn, "dbo.LichSuGiaoDichVay") ? "ls.LoaiLichSu" : "CAST(NULL AS NVARCHAR(20))") + " AS LoaiLichSu");

                    using (SqlCommand cmd = new SqlCommand(sqlVay, conn))
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lichSuMoi.Add(new LichSuItem
                            {
                                NgayGiaoDich = Convert.ToDateTime(rd["NgayGiaoDich"]),
                                LoaiTaiKhoan = "Vay",
                                SoTaiKhoan = rd["SoTaiKhoan"].ToString(),
                                CCCD = rd["CCCD"].ToString(),
                                HoTen = rd["HoTen"].ToString(),
                                LoaiGiaoDich = rd["LoaiGiaoDich"].ToString(),
                                SoTien = Convert.ToDecimal(rd["SoTien"]),
                                SoDuSau = rd["SoDuSau"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["SoDuSau"]),
                                NguoiThucHien = rd["NguoiThucHien"].ToString(),
                                GhiChu = rd["GhiChu"].ToString(),
                                LoaiLichSu = rd["LoaiLichSu"] == DBNull.Value ? "" : rd["LoaiLichSu"].ToString()
                            });
                        }
                    }

                    // 2. Tiết kiệm
                    string sqlTK = @"
                        SELECT ls.NgayGiaoDich, ls.SoTKTK AS SoTaiKhoan, kh.CCCD, kh.HoTen,
                               ls.LoaiGiaoDich, ls.TongTienNhan AS SoTien, SO_DU_SAU_TIET_KIEM AS SoDuSau,
                               COALESCE(NULLIF(LTRIM(RTRIM(ls.NguoiThucHien)), N''), N'Chưa xác định') AS NguoiThucHien,
                               ISNULL(ls.GhiChu, '') AS GhiChu, PHAN_LOAI_THAY_THE
                        FROM dbo.LichSuGiaoDichTietKiem ls
                        INNER JOIN dbo.SoTietKiem tk ON ls.SoTKTK = tk.SoTKTK
                        INNER JOIN dbo.KhachHang kh ON tk.CCCD = kh.CCCD";
                    // Chỉ đọc snapshot đã ghi ngay khi giao dịch diễn ra; không lấy số dư HIỆN TẠI.
                    // Riêng "Mở sổ" có gốc ban đầu lưu trong chính chứng từ nên biết số dư lúc mở.
                    string bieuThucSoDu = CoCotSoDuSauTietKiem(conn, "dbo.LichSuGiaoDichTietKiem")
                        ? "COALESCE(ls.SoDuSau, CASE WHEN ls.LoaiGiaoDich LIKE N'Mở sổ%' AND ls.SoTienGoc > 0 THEN ls.SoTienGoc ELSE NULL END)"
                        : "CASE WHEN ls.LoaiGiaoDich LIKE N'Mở sổ%' AND ls.SoTienGoc > 0 THEN ls.SoTienGoc ELSE NULL END";
                    sqlTK = sqlTK.Replace("SO_DU_SAU_TIET_KIEM", bieuThucSoDu);
                    sqlTK = sqlTK.Replace("PHAN_LOAI_THAY_THE",
                                            (CotPhanLoaiTonTai(conn, "dbo.LichSuGiaoDichTietKiem") ? "ls.LoaiLichSu" : "CAST(NULL AS NVARCHAR(20))") + " AS LoaiLichSu");

                    using (SqlCommand cmd = new SqlCommand(sqlTK, conn))
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lichSuMoi.Add(new LichSuItem
                            {
                                NgayGiaoDich = Convert.ToDateTime(rd["NgayGiaoDich"]),
                                LoaiTaiKhoan = "Tiết kiệm",
                                SoTaiKhoan = rd["SoTaiKhoan"].ToString(),
                                CCCD = rd["CCCD"].ToString(),
                                HoTen = rd["HoTen"].ToString(),
                                LoaiGiaoDich = rd["LoaiGiaoDich"].ToString(),
                                SoTien = Convert.ToDecimal(rd["SoTien"]),
                                SoDuSau = rd["SoDuSau"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["SoDuSau"]),
                                NguoiThucHien = rd["NguoiThucHien"].ToString(),
                                GhiChu = rd["GhiChu"].ToString(),
                                LoaiLichSu = rd["LoaiLichSu"] == DBNull.Value ? "" : rd["LoaiLichSu"].ToString()
                            });
                        }
                    }

                    // 3. Thanh toán
                    string sqlTT = @"
                        SELECT ls.ThoiGian AS NgayGiaoDich, ls.SoTKTT AS SoTaiKhoan, kh.CCCD, kh.HoTen,
                               ls.NoiDung AS LoaiGiaoDich, ISNULL(ls.SoTien, 0) AS SoTien, ls.SoDuSau,
                               COALESCE(NULLIF(LTRIM(RTRIM(ls.NguoiThucHien)), N''), N'Chưa xác định') AS NguoiThucHien,
                               '' AS GhiChu, PHAN_LOAI_THAY_THE
                        FROM dbo.LichSuGiaoDich ls
                        INNER JOIN dbo.TaiKhoanThanhToan tk ON ls.SoTKTT = tk.SoTKTT
                        INNER JOIN dbo.KhachHang kh ON tk.CCCD = kh.CCCD";
                    sqlTT = sqlTT.Replace("PHAN_LOAI_THAY_THE",
                        (CotPhanLoaiTonTai(conn, "dbo.LichSuGiaoDich") ? "ls.LoaiLichSu" : "CAST(NULL AS NVARCHAR(20))") + " AS LoaiLichSu");

                    using (SqlCommand cmd = new SqlCommand(sqlTT, conn))
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            lichSuMoi.Add(new LichSuItem
                            {
                                NgayGiaoDich = Convert.ToDateTime(rd["NgayGiaoDich"]),
                                LoaiTaiKhoan = "Thanh toán",
                                SoTaiKhoan = rd["SoTaiKhoan"].ToString(),
                                CCCD = rd["CCCD"].ToString(),
                                HoTen = rd["HoTen"].ToString(),
                                LoaiGiaoDich = rd["LoaiGiaoDich"].ToString(),
                                SoTien = Convert.ToDecimal(rd["SoTien"]),
                                SoDuSau = rd["SoDuSau"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rd["SoDuSau"]),
                                NguoiThucHien = rd["NguoiThucHien"].ToString(),
                                GhiChu = "",
                                LoaiLichSu = rd["LoaiLichSu"] == DBNull.Value ? "" : rd["LoaiLichSu"].ToString()
                            });
                        }
                    }
                    // Chỉ đổi tên tài khoản nhân viên đã lưu thành họ tên khi hiển thị.
                    // Không gán người đang đăng nhập cho giao dịch lịch sử.
                    DoiTenDangNhapThanhHoTen(conn, lichSuMoi);
                }
            }
            catch { throw; }
            return lichSuMoi;
        }

        // Chỉ đọc từ SQL. Lịch sử Vay/Tiết kiệm/Thanh toán vẫn là nguồn dữ liệu duy nhất.
        // Nếu bản ghi có tên đăng nhập nhân viên, đổi sang họ tên để hiển thị.
        private void DoiTenDangNhapThanhHoTen(SqlConnection conn, List<LichSuItem> lichSuMoi)
        {
            var tenNhanVien = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var cmd = new SqlCommand(
                    "SELECT TenDangNhap, HoTen FROM dbo.NhanVienNganHang", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string login = Convert.ToString(reader["TenDangNhap"]).Trim();
                        string hoTen = Convert.ToString(reader["HoTen"]).Trim();
                        if (login.Length > 0 && hoTen.Length > 0)
                            tenNhanVien[login] = hoTen;
                    }
                }
            }
            catch (SqlException)
            {
                // Vẫn hiển thị tên đã lưu nếu bảng nhân viên chưa có hoặc không truy cập được.
            }

            foreach (LichSuItem giaoDich in lichSuMoi)
            {
                string daLuu = (giaoDich.NguoiThucHien ?? "").Trim();
                if (daLuu.Length == 0 ||
                    string.Equals(daLuu, "Tự thực hiện", StringComparison.OrdinalIgnoreCase))
                {
                    // Không biết ai đã thao tác: không lấy tên phiên đăng nhập hiện tại để đoán.
                    giaoDich.NguoiThucHien = "Chưa xác định";
                }
                else if (tenNhanVien.TryGetValue(daLuu, out string hoTen))
                {
                    giaoDich.NguoiThucHien = hoTen;
                }
                else if (daLuu.StartsWith("GDV:", StringComparison.OrdinalIgnoreCase) &&
                         tenNhanVien.TryGetValue(daLuu.Substring(4).Trim(), out hoTen))
                {
                    giaoDich.NguoiThucHien = hoTen;
                }
            }
        }

        private readonly Timer _timerLocLichSu = new Timer { Interval = 250 };
        private int _phienLoc;
        private bool _locChinhXacKhachHang;
        private bool _dangChonLichSuKhachHang;
        private void ApplyFilter()
        {
            if (_dangTaiLichSu || _dangChonLichSuKhachHang || IsDisposed || Disposing) return;
            _timerLocLichSu.Stop();
            int phienLoc = ++_phienLoc;
            List<LichSuItem> result = LayDanhSachTheoBoLoc();
            dgvLichSu.SuspendLayout();
            try
            {
                dgvLichSu.Rows.Clear();
                int stt = 1;
                var rows = new List<DataGridViewRow>(result.Count);
                foreach (var item in result)
                {
                    var hang = new DataGridViewRow();
                    hang.CreateCells(dgvLichSu,
                        stt++, item.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"),
                        item.LoaiTaiKhoan, item.SoTaiKhoan, item.CCCD, item.HoTen,
                        item.LoaiGiaoDich, item.SoTien == 0 ? "—" : $"{item.SoTien:N0} VNĐ",
                        item.SoDuSau.HasValue ? $"{item.SoDuSau:N0} VNĐ" : "Chưa ghi nhận",
                        item.NguoiThucHien, item.GhiChu);
                    hang.Tag = item;
                    rows.Add(hang);
                }
                if (rows.Count > 0) dgvLichSu.Rows.AddRange(rows.ToArray());
                // Mỗi lần lọc/đổi tab, hiển thị lại từ bản ghi số 1,
                // không giữ vị trí cuộn cũ (ví dụ đang ở dòng 7).
                dgvLichSu.ClearSelection();
                dgvLichSu.CurrentCell = null;
            }
            finally { dgvLichSu.ResumeLayout(); }
            if (dgvLichSu.Rows.Count > 0 && dgvLichSu.IsHandleCreated)
            {
                dgvLichSu.FirstDisplayedScrollingRowIndex = 0;
                if (dgvLichSu.Columns.Count > 0)
                    dgvLichSu.FirstDisplayedScrollingColumnIndex = 0;
            }
            if (IsHandleCreated && !IsDisposed)
            {
                BeginInvoke(new Action(() =>
                {
                    if (phienLoc != _phienLoc || IsDisposed || dgvLichSu.IsDisposed || dgvLichSu.Rows.Count == 0) return;
                    try
                    {
                        dgvLichSu.ClearSelection();
                        dgvLichSu.CurrentCell = null;
                        dgvLichSu.FirstDisplayedScrollingRowIndex = 0;
                        if (dgvLichSu.Columns.Count > 0)
                            dgvLichSu.FirstDisplayedScrollingColumnIndex = 0;
                    }
                    catch (InvalidOperationException) { /* Grid đang được bố trí lại. */ }
                }));
            }
            string nhan = _dangXemGiaoDich ? "giao dịch" : "hoạt động";
            lblThongKe.Text = $"Tổng số {nhan}: {result.Count}" +
                (_loiTaiLichSu ? " | Lỗi tải SQL - dữ liệu chưa cập nhật" : "");
        }

        private void cboMocThoiGian_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isCustom = cboMocThoiGian.SelectedItem?.ToString() == "Tùy chọn";
            dtpTuNgay.Enabled = isCustom;
            dtpDenNgay.Enabled = isCustom;
            ApplyFilter();
        }

        private void txtCCCD_TextChanged(object sender, EventArgs e)
        {
            if (_dangChonLichSuKhachHang) return;
            _locChinhXacKhachHang = false;
            string clean = new string(txtCCCD.Text.Where(char.IsDigit).ToArray());
            if (clean.Length > 12) clean = clean.Substring(0, 12);
            if (txtCCCD.Text != clean)
            {
                txtCCCD.Text = clean;
                txtCCCD.SelectionStart = txtCCCD.Text.Length;
                return; // TextChanged long nhau da loc chuoi sach.
            }
            _timerLocLichSu.Stop();
            if (IsHandleCreated && !IsDisposed && !Disposing) _timerLocLichSu.Start();
        }

        private void cboLoaiTK_SelectedIndexChanged(object sender, EventArgs e) => ApplyFilter();
        private void dtpTuNgay_ValueChanged(object sender, EventArgs e) { if (dtpTuNgay.Enabled) ApplyFilter(); }
        private void dtpDenNgay_ValueChanged(object sender, EventArgs e) { if (dtpDenNgay.Enabled) ApplyFilter(); }

        // Chỉ bắt chuột trái trên nền trống của các khung phía trên bảng.
        private void GanLamMoiVungTrongLichSu(Control khung)
        {
            khung.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && khung.GetChildAtPoint(e.Location) == null)
                    LamMoiVungTrongLichSu();
            };
            foreach (Control con in khung.Controls)
                if (con is Panel || con is GroupBox)
                    GanLamMoiVungTrongLichSu(con);
        }

        private void LamMoiVungTrongLichSu()
        {
            if (_dangTaiLichSu || _dangChonLichSuKhachHang || IsDisposed || Disposing) return;
            _dangChonLichSuKhachHang = true;
            try
            {
                _timerLocLichSu.Stop();
                _locChinhXacKhachHang = false;
                txtCCCD.Clear();
                cboMocThoiGian.SelectedIndex = cboMocThoiGian.Items.IndexOf("Tất cả");
                cboLoaiTK.SelectedIndex = cboLoaiTK.Items.IndexOf("Tất cả");
                dtpTuNgay.Enabled = false;
                dtpDenNgay.Enabled = false;
                _timerLocLichSu.Stop();
            }
            finally { _dangChonLichSuKhachHang = false; }
            // Giữ tab hiện tại, tải lại SQL bằng hàm bất đồng bộ có sẵn.
            LoadAllHistoryFromSQL();
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            LoadAllHistoryFromSQL();
            ApplyFilter();
        }

        // Nhấp đúp: xem mọi lịch sử của khách hàng trên chính form hiện tại.
        // Giữ nguyên tab giao dịch/hoạt động và không truy vấn lại SQL.
        private void DgvLichSu_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.RowIndex < 0 ||
                e.RowIndex >= dgvLichSu.Rows.Count || _dangTaiLichSu ||
                _dangChonLichSuKhachHang || IsDisposed || Disposing) return;
            var item = dgvLichSu.Rows[e.RowIndex].Tag as LichSuItem;
            if (item == null || string.IsNullOrWhiteSpace(item.CCCD)) return;

            string cccd = item.CCCD.Trim();
            _timerLocLichSu.Stop();
            _dangChonLichSuKhachHang = true;
            try
            {
                txtCCCD.Text = cccd;
                _locChinhXacKhachHang = true;
                // Không giới hạn kỳ thời gian hay loại tài khoản.
                cboMocThoiGian.SelectedIndex = cboMocThoiGian.Items.IndexOf("Tất cả");
                cboLoaiTK.SelectedIndex = cboLoaiTK.Items.IndexOf("Tất cả");
                dtpTuNgay.Enabled = false;
                dtpDenNgay.Enabled = false;
            }
            finally { _dangChonLichSuKhachHang = false; }
            // Tránh dựng lại hàng ngay trong sự kiện chuột của DataGridView.
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed && !Disposing) ApplyFilter();
            }));
        }

    }
}