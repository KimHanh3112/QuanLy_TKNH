using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace QLTKNH
{
    public partial class FrmThanhToanDN : Form
    {
        private readonly string cs = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";
        public event EventHandler DataChanged;
        public FrmThanhToanDN()
        {
            InitializeComponent();
            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(dgv, true, null);
            cboTrangThai.SelectedIndex = 0;
            cboThoiGian.SelectedIndex = 0;
            // Tự lọc khi nhập MST/CCCD; đợi ngừng gõ để tránh truy vấn SQL liên tục.
            _timerTimKiem.Tick += (s, e) => { _timerTimKiem.Stop(); NapDuLieu(true); };
            txtTim.TextChanged += (s, e) =>
            {
                _timerTimKiem.Stop();
                HenNapDuLieuDN();
            };
            Disposed += (s, e) => _timerTimKiem.Dispose();
            btnTim.Click += (s, e) => TimVaHoiMoTaiKhoan();
            txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TimVaHoiMoTaiKhoan(); } };
            cboTrangThai.SelectedIndexChanged += (s, e) => HenNapDuLieuDN();
            cboThoiGian.SelectedIndexChanged += (s, e) => { CapNhatNgay(); HenNapDuLieuDN(); };
            dtpTu.ValueChanged += (s, e) => { if (cboThoiGian.SelectedIndex == 7) HenNapDuLieuDN(); };
            dtpDen.ValueChanged += (s, e) => { if (cboThoiGian.SelectedIndex == 7) HenNapDuLieuDN(); };
            btnBaoCao.Visible = false; // Bỏ nút xuất báo cáo, giữ nguyên xuất hóa đơn.
            btnHoaDon.Click += XuatHoaDon;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) { dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[0]; ChiTietTaiKhoan(); } };
            dgv.CellMouseDown += (s, e) => { if (e.Button == MouseButtons.Right && e.RowIndex >= 0) { dgv.ClearSelection(); dgv.Rows[e.RowIndex].Selected = true; dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)]; } };
            // Không dùng dòng chủ tài khoản còn được chọn khi nhấp chuột phải vào vùng trống.
            dgv.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && dgv.HitTest(e.X, e.Y).RowIndex < 0)
                {
                    dgv.ClearSelection();
                    dgv.CurrentCell = null;
                }
            };
            cmsTaiKhoan.Opening += (s, e) => TaoMenuChuoiPhai();
            dgv.SelectionChanged += (s, e) => CapNhatTongQuan();
            dgv.CellFormatting += ToMauDanhSachThanhToanDN;
            Load += (s, e) => HenNapDuLieuDN();
            CapNhatNgay();
            GanLamMoiKhungTimKiem();
            pnlTop.Resize += (s, e) => { int h = cboThoiGian.SelectedIndex == 7 && pnlTop.Width < 1180 ? 140 : 100; if (pnlTop.Height != h) pnlTop.Height = h; };
            dgv.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left && dgv.HitTest(e.X, e.Y).Type == DataGridViewHitTestType.None) LamMoiKhungTimKiem(); };
            pnlBoLoc.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left && pnlBoLoc.GetChildAtPoint(e.Location) == null) LamMoiKhungTimKiem(); };
            pnlTop.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left && pnlTop.GetChildAtPoint(e.Location) == null) LamMoiKhungTimKiem(); };
        }

        // Cho phép FrmKhachHangDN truyền mã số thuế/chủ tài khoản đang chọn.
        public FrmThanhToanDN(string maSoThue) : this()
        {
            txtTim.Text = maSoThue ?? string.Empty;
        }
        // Cố định số tài khoản khi thực hiện các thao tác trong cửa sổ chi tiết.
        private string _stkChiTietDangMo;
        private string SoTK { get { return !string.IsNullOrEmpty(_stkChiTietDangMo) ? _stkChiTietDangMo : (dgv.CurrentRow == null || !dgv.Columns.Contains("SoTKTT") ? "" : Convert.ToString(dgv.CurrentRow.Cells["SoTKTT"].Value)); } }
        private string NguoiThucHien { get { return BankSecurity.Current == null ? "" : BankSecurity.Current.HoTen + " (" + BankSecurity.Current.TenDangNhap + ")"; } }
        private void Changed() { NapDuLieu(); if (DataChanged != null) DataChanged(this, EventArgs.Empty); }
        private readonly System.Windows.Forms.Timer _timerTimKiem = new System.Windows.Forms.Timer { Interval = 300 };
        private bool _dangNapDuLieu;
        private long _phienNapDN;
        private void HenNapDuLieuDN()
        {
            if (_dangLamMoi || IsDisposed || Disposing || !IsHandleCreated) return;
            ++_phienNapDN;
            _timerTimKiem.Stop();
            _timerTimKiem.Start();
        }
        private static DataTable DocDanhSachDN(SqlCommand cmd)
        {
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
            return dt;
        }
        private bool _napDuLieuThanhCong;
        private bool _dangHoiMoTaiKhoan;
        private bool _dangLamMoi;
        private void GanLamMoiKhungTimKiem()
        {
            Control khung = txtTim.Parent;
            if (khung == null || khung == this) return;
            khung.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && khung.GetChildAtPoint(e.Location) == null)
                    LamMoiKhungTimKiem();
            };
            foreach (Control c in khung.Controls)
                if (c is Label) c.Click += (s, e) => LamMoiKhungTimKiem();
        }
        private void LamMoiKhungTimKiem()
        {
            if (_dangLamMoi || _dangNapDuLieu) return;
            _dangLamMoi = true;
            try
            {
                _timerTimKiem.Stop();
                txtTim.Clear();
                cboTrangThai.SelectedIndex = 0;
                cboThoiGian.SelectedIndex = 0;
                CapNhatNgay();
                _timerTimKiem.Stop();
            }
            finally { _dangLamMoi = false; }
            NapDuLieu();
            dgv.ClearSelection();
            dgv.CurrentCell = null;
            CapNhatTongQuan();
            txtTim.Focus();
        }

        private void TimVaHoiMoTaiKhoan()
        {
            if (_dangHoiMoTaiKhoan || _dangNapDuLieu) return;
            _timerTimKiem.Stop();
            string q = txtTim.Text.Trim();
            NapDuLieu();
            if (!_napDuLieuThanhCong || string.IsNullOrWhiteSpace(q)) return;
            var dt = dgv.DataSource as DataTable;
            if (dt == null || dt.Rows.Count > 0) return;
            _dangHoiMoTaiKhoan = true;
            try
            {
                // Không nhầm tài khoản bị ẩn bởi bộ lọc với tài khoản chưa tồn tại.
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand(@"SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM dbo.DN_TaiKhoanThanhToan t
                    INNER JOIN dbo.DN_KhachHang k ON t.CCCD=k.CCCD
                    WHERE t.SoTKTT LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%' OR t.CCCD LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%'
                       OR k.HoTen LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%'
                       OR EXISTS (SELECT 1 FROM dbo.DN_DongSoHuuTaiKhoanThanhToan d
                                  WHERE d.SoTKTT=t.SoTKTT AND d.CCCD LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%')
                    ) THEN 1 ELSE 0 END", cn))
                {
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200).Value = q;
                    cn.Open();
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                    {
                        ThongBaoDN(this, "Có tài khoản nhưng không phù hợp bộ lọc hiện tại.",
                            "Kết quả tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                // CCCD người được ủy quyền không phải MST để mở tài khoản mới.
                if (MaSoHopLe(q, 10, 12))
                {
                    using (var cn = new SqlConnection(cs))
                    {
                        cn.Open();
                        KiemTraMaChuTaiKhoanMoMoiDN(cn, null, q);
                    }
                }
                if (ThongBaoDN(this, "Không tìm thấy tài khoản phù hợp với: " + q +
                    "\nBạn có muốn mở tài khoản mới không?", "Không tìm thấy tài khoản",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                // Chỉ điền sẵn khi mã nhập là MST của doanh nghiệp đã đăng ký.
                // Không dùng CCCD của người được ủy quyền làm MST của chủ tài khoản.
                string mst = "";
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT CCCD FROM dbo.DN_KhachHang WHERE CCCD=@q", cn))
                {
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200).Value = q;
                    cn.Open();
                    mst = Convert.ToString(cmd.ExecuteScalar()) ?? "";
                }
                if (string.IsNullOrEmpty(mst) && MaSoHopLe(q, 10, 12))
                {
                    using (var cn = new SqlConnection(cs))
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.DN_DongSoHuu WHERE CCCD=@q", cn))
                    { cmd.Parameters.AddWithValue("@q", q); cn.Open(); if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) mst = q; }
                }
                MoKhungThemMoiTuMainDN(mst);
            }
            catch (Exception ex)
            {
                ThongBaoDN(this, "Không thể kiểm tra tài khoản: " + ex.Message,
                    "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { _dangHoiMoTaiKhoan = false; }
        }
        private async void NapDuLieu(bool batDongBo = false)
        {
            if (_dangLamMoi || _dangNapDuLieu || IsDisposed || Disposing) return;
            _timerTimKiem.Stop();
            long phien = ++_phienNapDN;
            _napDuLieuThanhCong = false;
            try
            {
                string selected = SoTK;
                string maDangChon = dgv.CurrentRow != null && dgv.Columns.Contains("MaDinhDanh") ? Convert.ToString(dgv.CurrentRow.Cells["MaDinhDanh"].Value) : "";
                using (var cn = new SqlConnection(cs)) using (var cmd = new SqlCommand(@"
SELECT t.SoTKTT, t.CCCD AS MaSoThue, k.HoTen AS TenDoanhNghiep, k.SoDienThoai,
 t.SoDu, t.SoTienPhongToa, t.HanMucNgay, t.HanMucLan, t.LoaiTienTe,
 CASE t.TrangThai WHEN 0 THEN N'Hoạt động' WHEN 1 THEN N'Đang khóa' ELSE N'Đã đóng' END AS TrangThai,
 t.NgayMo, t.NgayCapNhatTT, o.MaDinhDanh, o.TenSoHuu, o.SDTSoHuu, o.VaiTro
FROM dbo.DN_TaiKhoanThanhToan t JOIN dbo.DN_KhachHang k ON t.CCCD=k.CCCD
CROSS APPLY (
 SELECT t.CCCD AS MaDinhDanh,k.HoTen AS TenSoHuu,k.SoDienThoai AS SDTSoHuu,N'Chủ' AS VaiTro
 UNION ALL
 SELECT d.CCCD,d.HoTen,d.SoDienThoai,N'Người được ủy quyền'
 FROM dbo.DN_DongSoHuuTaiKhoanThanhToan d WHERE d.SoTKTT=t.SoTKTT AND d.CCCD<>t.CCCD
) o
WHERE (@q=N'' OR t.SoTKTT LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%' OR t.CCCD LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%' OR k.HoTen LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%'
 OR EXISTS (SELECT 1 FROM dbo.DN_DongSoHuuTaiKhoanThanhToan d
            WHERE d.SoTKTT=t.SoTKTT AND d.CCCD LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%'))
 AND (@q=N'' OR o.MaDinhDanh LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%' OR t.CCCD LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%' OR t.SoTKTT LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%' OR k.HoTen LIKE N'%'+REPLACE(REPLACE(REPLACE(@q,N'[',N'[[]'),N'%',N'[%]'),N'_',N'[_]')+N'%')
 AND (@tt=N'Tất cả trạng thái' OR (@tt=N'Hoạt động' AND t.TrangThai=0) OR (@tt=N'Đang khóa' AND t.TrangThai=1) OR (@tt=N'Đã đóng' AND t.TrangThai=2))
 AND (@all=1 OR (t.NgayMo>=@from AND t.NgayMo<DATEADD(day,1,@to)))
ORDER BY t.NgayMo DESC,t.SoTKTT,CASE WHEN o.VaiTro=N'Chủ' THEN 0 ELSE 1 END,o.MaDinhDanh", cn))
                {
                    cmd.Parameters.Add("@q", SqlDbType.NVarChar, 200).Value = txtTim.Text.Trim();
                    cmd.Parameters.Add("@tt", SqlDbType.NVarChar, 40).Value = Convert.ToString(cboTrangThai.SelectedItem);
                    cmd.Parameters.Add("@all", SqlDbType.Bit).Value = cboThoiGian.SelectedIndex == 0;
                    cmd.Parameters.Add("@from", SqlDbType.DateTime).Value = dtpTu.Value.Date;
                    cmd.Parameters.Add("@to", SqlDbType.DateTime).Value = dtpDen.Value.Date;
                    var dt = batDongBo ? await Task.Run(() => DocDanhSachDN(cmd)) : DocDanhSachDN(cmd);
                    if (IsDisposed || Disposing || phien != _phienNapDN) return;
                    _dangNapDuLieu = true;
                    dgv.SuspendLayout();
                    dt.Columns.Add("STT", typeof(int));
                    var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int stt = 0;
                    foreach (DataRow row in dt.Rows) { row["STT"] = ++stt; seen.Add(Convert.ToString(row["SoTKTT"])); }
                    dgv.DataSource = dt;
                    if (dgv.Columns.Contains("SoTKTT")) dgv.Columns["SoTKTT"].HeaderText = "Số tài khoản";
                    if (dgv.Columns.Contains("MaSoThue")) dgv.Columns["MaSoThue"].HeaderText = "Mã số thuế";
                    if (dgv.Columns.Contains("TenDoanhNghiep")) dgv.Columns["TenDoanhNghiep"].HeaderText = "Tên doanh nghiệp";
                    if (dgv.Columns.Contains("SoDu")) { dgv.Columns["SoDu"].HeaderText = "Số dư"; dgv.Columns["SoDu"].DefaultCellStyle.Format = "N0"; }
                    if (dgv.Columns.Contains("SoTienPhongToa")) { dgv.Columns["SoTienPhongToa"].HeaderText = "Tiền phong tỏa"; dgv.Columns["SoTienPhongToa"].DefaultCellStyle.Format = "N0"; }
                    if (dgv.Columns.Contains("SoDienThoai")) dgv.Columns["SoDienThoai"].HeaderText = "Số điện thoại";
                    if (dgv.Columns.Contains("HanMucNgay")) dgv.Columns["HanMucNgay"].Visible = false;
                    if (dgv.Columns.Contains("HanMucLan")) dgv.Columns["HanMucLan"].Visible = false;
                    if (dgv.Columns.Contains("LoaiTienTe")) { dgv.Columns["LoaiTienTe"].Visible = true; dgv.Columns["LoaiTienTe"].HeaderText = "Tiền tệ"; dgv.Columns["LoaiTienTe"].FillWeight = 55; }
                    if (dgv.Columns.Contains("NgayMo")) { dgv.Columns["NgayMo"].HeaderText = "Ngày mở"; dgv.Columns["NgayMo"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                    if (dgv.Columns.Contains("NgayCapNhatTT")) { dgv.Columns["NgayCapNhatTT"].HeaderText = "Ngày cập nhật"; dgv.Columns["NgayCapNhatTT"].DefaultCellStyle.Format = "dd/MM/yyyy"; }
                    if (dgv.Columns.Contains("TrangThai")) dgv.Columns["TrangThai"].HeaderText = "Trạng thái";
                    lblStatus.Text = "Tổng tài khoản doanh nghiệp: " + seen.Count + " | Nhấp đúp để thao tác · Chuột phải để mở thêm tài khoản";
                    foreach (string c in new[] { "MaSoThue", "TenDoanhNghiep", "SoDienThoai" }) dgv.Columns[c].Visible = false;
                    dgv.Columns["STT"].DisplayIndex = 0; dgv.Columns["STT"].FillWeight = 30;
                    dgv.Columns["SoTKTT"].DisplayIndex = 1;
                    dgv.Columns["MaDinhDanh"].HeaderText = "CCCD / MST"; dgv.Columns["MaDinhDanh"].DisplayIndex = 2;
                    dgv.Columns["TenSoHuu"].HeaderText = "Tên doanh nghiệp / Họ tên"; dgv.Columns["TenSoHuu"].DisplayIndex = 3; dgv.Columns["TenSoHuu"].FillWeight = 150;
                    dgv.Columns["SDTSoHuu"].HeaderText = "SĐT"; dgv.Columns["SDTSoHuu"].DisplayIndex = 4;
                    dgv.Columns["VaiTro"].HeaderText = "Vai trò";
                    dgv.Columns["VaiTro"].MinimumWidth = 145;
                    foreach (DataGridViewColumn col in dgv.Columns) col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgv.Columns["TrangThai"].DisplayIndex = dgv.Columns.Count - 1;
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        row.Cells["VaiTro"].Style.ForeColor = Convert.ToString(row.Cells["VaiTro"].Value) == "Chủ" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(168, 85, 247);
                        row.Cells["VaiTro"].Style.SelectionForeColor = row.Cells["VaiTro"].Style.ForeColor;
                        bool laChu = Convert.ToString(row.Cells["VaiTro"].Value) == "Chủ";
                        var ten = row.Cells["TenSoHuu"];
                        ten.Style.ForeColor = laChu ? Color.FromArgb(96, 165, 250) : Color.FromArgb(192, 132, 252);
                        ten.Style.Padding = new Padding(laChu ? 0 : 16, 0, 0, 0);
                        ten.ToolTipText = "Doanh nghiệp: " + Convert.ToString(row.Cells["TenDoanhNghiep"].Value)
                            + " | MST: " + Convert.ToString(row.Cells["MaSoThue"].Value) + " | STK: " + Convert.ToString(row.Cells["SoTKTT"].Value);
                        string tt = Convert.ToString(row.Cells["TrangThai"].Value);
                        row.Cells["TrangThai"].Style.ForeColor = MauTrangThaiDN(tt);
                        row.Cells["TrangThai"].Style.SelectionForeColor = row.Cells["TrangThai"].Style.ForeColor;
                    }
                    foreach (DataGridViewRow r in dgv.Rows) if (Convert.ToString(r.Cells["SoTKTT"].Value) == selected && (string.IsNullOrEmpty(maDangChon) || Convert.ToString(r.Cells["MaDinhDanh"].Value) == maDangChon)) { r.Selected = true; dgv.CurrentCell = r.Cells[0]; break; }
                    _napDuLieuThanhCong = true;
                }
            }
            catch (Exception ex) { if (!IsDisposed && !Disposing && phien == _phienNapDN) lblStatus.Text = "Lỗi tải dữ liệu: " + ex.Message; }
            finally
            {
                if (_dangNapDuLieu && !IsDisposed && !Disposing)
                { _dangNapDuLieu = false; dgv.ResumeLayout(); CapNhatTongQuan(); }
            }
        }
        private static string Prompt(string caption, string text, string value = "")
        {
            using (var f = new Form { Text = caption, Size = new Size(510, 185), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false })
            {
                var l = new Label { Text = text, Left = 15, Top = 15, Width = 465, Height = 38 }; var t = new TextBox { Left = 15, Top = 55, Width = 455, Text = value };
                var b = new Button { Text = "Xác nhận", Left = 365, Top = 92, Width = 105, DialogResult = DialogResult.OK };
                f.Controls.AddRange(new Control[] { l, t, b }); f.AcceptButton = b;
                DongBoMauHopThoaiDN(f);
                return f.ShowDialog() == DialogResult.OK ? t.Text.Trim() : null;
            }
        }
        private static decimal Amount(string str)
        {
            decimal d;
            if (!decimal.TryParse(str, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out d) &&
               !decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
                throw new InvalidOperationException("Số tiền không hợp lệ.");
            if (d <= 0 || d != decimal.Truncate(d)) throw new InvalidOperationException("Số tiền phải là số nguyên dương.");
            if (d > 999999999999999999m) throw new InvalidOperationException("Số tiền vượt giới hạn lưu trữ SQL.");
            return d;
        }
        private void Run(Action action)
        {
            try { if (string.IsNullOrWhiteSpace(NguoiThucHien)) throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập."); action(); }
            catch (Exception ex) { ThongBaoDN(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try { Changed(); }
            catch (Exception ex) { ThongBaoDN("Thao tác đã hoàn tất nhưng không thể làm mới giao diện: " + ex.Message, "Lỗi đồng bộ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        private const string LoiUyQuyenMoTaiKhoanDN =
            "Người được ủy quyền không được phép mở thêm tài khoản cho doanh nghiệp. Chỉ chủ tài khoản doanh nghiệp được thực hiện chức năng này.";

        private bool LaDongChuTaiKhoanDangChonDN()
        {
            var row = dgv.CurrentRow;
            if (row == null || !row.Selected || row.IsNewRow ||
                !dgv.Columns.Contains("VaiTro") || !dgv.Columns.Contains("MaDinhDanh") ||
                !dgv.Columns.Contains("MaSoThue")) return false;
            string role = Convert.ToString(row.Cells["VaiTro"].Value).Trim();
            string ma = Convert.ToString(row.Cells["MaDinhDanh"].Value).Trim();
            string mst = Convert.ToString(row.Cells["MaSoThue"].Value).Trim();
            return string.Equals(role, "Chủ", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(mst)
                && string.Equals(ma, mst, StringComparison.OrdinalIgnoreCase);
        }

        // Chặn dùng CCCD người được ủy quyền làm MST (kể cả khi hồ sơ cũ bị tạo nhầm).
        // Tên bảng cố định nội bộ, dữ liệu định danh luôn truyền bằng tham số SQL.
        private static void KiemTraMaChuTaiKhoanMoMoiDN(SqlConnection cn, SqlTransaction tx, string ma)
        {
            foreach (string table in new[] { "dbo.DN_DongSoHuu", "dbo.DN_DongSoHuuTaiKhoanThanhToan",
                "dbo.DN_DongSoHuuSoTietKiem", "dbo.DN_DongSoHuuTaiKhoanVay" })
            {
                // Hai bảng liên kết tiết kiệm/vay có thể chưa tồn tại ở phiên bản SQL cũ.
                if (table == "dbo.DN_DongSoHuuSoTietKiem" || table == "dbo.DN_DongSoHuuTaiKhoanVay")
                {
                    using (var exists = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@table,N'U') IS NULL THEN 0 ELSE 1 END", cn, tx))
                    {
                        exists.Parameters.Add("@table", SqlDbType.NVarChar, 128).Value = table;
                        if (Convert.ToInt32(exists.ExecuteScalar()) == 0) continue;
                    }
                }
                using (var cmd = new SqlCommand("SELECT CASE WHEN EXISTS(SELECT 1 FROM " + table +
                    " WHERE CCCD=@ma) THEN 1 ELSE 0 END", cn, tx))
                {
                    cmd.Parameters.Add("@ma", SqlDbType.VarChar, 12).Value = ma;
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                        throw new InvalidOperationException(LoiUyQuyenMoTaiKhoanDN +
                            "\nMã đã nhập thuộc danh sách người được ủy quyền. Vui lòng sử dụng MST của doanh nghiệp.");
                }
            }
        }

        // Mở từ kết quả tìm kiếm bằng giao diện FrmMainDN, giữ nghiệp vụ lưu của ThanhToanDN.
        private bool LuuThanhToanTuKhungMainDN(FrmMainDN.KhachHangData kh, string stk)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NguoiThucHien)) throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập.");
                string ma = kh.CCCD;
                if (!MaSoHopLe(ma, 10, 12) || string.IsNullOrWhiteSpace(kh.HoTen) || kh.HoTen.Length > 100 || !MaSoHopLe(kh.SoDienThoai, 10, 11))
                    throw new InvalidOperationException("Kiểm tra MST, tên và số điện thoại doanh nghiệp.");
                var reps = new System.Collections.Generic.Dictionary<string, Tuple<string, string>>();
                foreach (var rep in kh.DanhSachDongSoHuu)
                {
                    if (!MaSoHopLe(rep.CCCD, 12, 12) || rep.CCCD == ma || string.IsNullOrWhiteSpace(rep.HoTen) || rep.HoTen.Length > 100 || !MaSoHopLe(rep.SoDienThoai, 10, 11) || reps.ContainsKey(rep.CCCD))
                        throw new InvalidOperationException("Thông tin người được ủy quyền không hợp lệ hoặc trùng CCCD.");
                    reps.Add(rep.CCCD, Tuple.Create(rep.HoTen, rep.SoDienThoai));
                }
                if (reps.Count < 2) throw new InvalidOperationException("Cần ít nhất 2 người được ủy quyền.");
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        // Kiểm tra lại trong transaction trước khi ghi bất kỳ dữ liệu nào.
                        KiemTraMaChuTaiKhoanMoMoiDN(cn, tx, ma);
                        using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.DN_KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@mst AND (HoTen<>@ten OR SoDienThoai<>@sdt)) THROW 52004,N'Thông tin doanh nghiệp đã thay đổi. Hãy tra cứu lại MST.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.DN_KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@mst)
INSERT dbo.DN_KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo,MaSoThue) VALUES(@mst,@ten,@sdt,0,@mst);
IF EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk) OR EXISTS(SELECT 1 FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk) OR EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk) THROW 52002,N'Số tài khoản đã tồn tại.',1;
INSERT dbo.DN_TaiKhoanThanhToan(SoTKTT,CCCD,SoDu,TrangThai,NgayMo,HanMucNgay,HanMucLan,LoaiTaiKhoan,TenHienThi,LoaiTienTe,SoTienPhongToa,NgayCapNhatTT)
VALUES(@stk,@mst,0,0,GETDATE(),1000000000,200000000,N'Doanh nghiệp',@ten,N'VND',0,GETDATE());", cn, tx))
                        { cmd.Parameters.AddWithValue("@mst", ma); cmd.Parameters.AddWithValue("@ten", kh.HoTen); cmd.Parameters.AddWithValue("@sdt", kh.SoDienThoai); cmd.Parameters.AddWithValue("@stk", stk); cmd.ExecuteNonQuery(); }
                        foreach (var rep in reps)
                            using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@mst AND CCCD=@c AND (HoTen<>@n OR SoDienThoai<>@p)) THROW 52003,N'Thông tin người được ủy quyền khác dữ liệu đã đăng ký. Hãy sửa thông tin tại danh mục khách hàng trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@mst AND CCCD=@c)
INSERT dbo.DN_DongSoHuu(CCCD_ChuSoHuu,CCCD,HoTen,SoDienThoai) VALUES(@mst,@c,@n,@p);
INSERT dbo.DN_DongSoHuuTaiKhoanThanhToan(SoTKTT,CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p);", cn, tx))
                            { cmd.Parameters.AddWithValue("@mst", ma); cmd.Parameters.AddWithValue("@stk", stk); cmd.Parameters.AddWithValue("@c", rep.Key); cmd.Parameters.AddWithValue("@n", rep.Value.Item1); cmd.Parameters.AddWithValue("@p", rep.Value.Item2); cmd.ExecuteNonQuery(); }
                        Ghi(cn, tx, stk, "Mở tài khoản doanh nghiệp", 0, 0); tx.Commit();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ThongBaoDN(ex.Message, "Không thể mở tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private static FrmMainDN TimMainDNTrongControl(Control root)
        {
            if (root is FrmMainDN main && !main.IsDisposed && !main.Disposing) return main;
            foreach (Control child in root.Controls)
            {
                var found = TimMainDNTrongControl(child);
                if (found != null) return found;
            }
            return null;
        }

        private void MoKhungThemMoiTuMainDN(string mst)
        {
            FrmMainDN main = null;
            foreach (Form form in Application.OpenForms)
            {
                main = TimMainDNTrongControl(form);
                if (main != null) break;
            }
            string maDaMo = null;
            Func<FrmMainDN.KhachHangData, string, bool> save = (kh, stk) =>
            {
                if (!LuuThanhToanTuKhungMainDN(kh, stk)) return false;
                maDaMo = kh.CCCD;
                return true;
            };
            if (main != null) main.MoKhungThemThanhToanTuTimKiem(mst, this, save);
            else
            {
                using (var tam = new FrmMainDN())
                    tam.MoKhungThemThanhToanTuTimKiem(mst, this, save);
            }
            if (string.IsNullOrEmpty(maDaMo)) return;
            _dangLamMoi = true;
            try { txtTim.Text = maDaMo; cboTrangThai.SelectedIndex = 0; cboThoiGian.SelectedIndex = 0; CapNhatNgay(); }
            finally { _dangLamMoi = false; }
            Changed();
        }

        private void MoTaiKhoan(object sender, EventArgs e)
        {
            if (!LaDongChuTaiKhoanDangChonDN())
            {
                ThongBaoDN(this, LoiUyQuyenMoTaiKhoanDN, "Không có quyền mở tài khoản",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MoTaiKhoanMoi(Convert.ToString(dgv.CurrentRow.Cells["MaSoThue"].Value));
        }
        private void MoTaiKhoanMoi(string mstBanDau)
        {
            if (MaSoHopLe((mstBanDau ?? "").Trim(), 10, 12))
            {
                try
                {
                    using (var cn = new SqlConnection(cs))
                    {
                        cn.Open();
                        KiemTraMaChuTaiKhoanMoMoiDN(cn, null, mstBanDau.Trim());
                    }
                }
                catch (Exception ex)
                {
                    ThongBaoDN(this, ex.Message, "Không thể mở tài khoản",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            using (var dlg = new Form { Text = "Mở tài khoản thanh toán doanh nghiệp", ClientSize = new Size(680, 580), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White, Font = new Font("Segoe UI", 10F) })
            {
                var mst = new TextBox { MaxLength = 12, Text = mstBanDau ?? "" };
                var ten = new TextBox { MaxLength = 100 };
                var sdt = new TextBox { MaxLength = 11 };
                var stk = new TextBox { ReadOnly = true };
                var loai = new TextBox { Text = "Thanh toán - Doanh nghiệp", ReadOnly = true };
                var tai = new Button { Text = "Tra cứu MST", AutoSize = true };
                var ngauNhien = new RadioButton { Text = "Số ngẫu nhiên", Checked = true, AutoSize = true };
                var soDep = new RadioButton { Text = "Số đẹp (chưa áp dụng)", AutoSize = true, Enabled = false };
                var ds = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.FromArgb(30, 41, 59), ForeColor = Color.Black };
                ds.Columns.Add("cccd", "CCCD"); ds.Columns.Add("ten", "Họ tên"); ds.Columns.Add("sdt", "SĐT");
                var them = new Button { Text = "+ Thêm người được ủy quyền", AutoSize = true };
                var xoa = new Button { Text = "Bỏ dòng đã chọn", AutoSize = true };
                var ok = new Button { Text = "Xác nhận mở", AutoSize = true };
                var huy = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, AutoSize = true };
                var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 9 };
                root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175)); root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                for (int i = 0; i < 7; i++) root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
                Action<string, Control, int> add = (label, control, row) => { root.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row); control.Dock = DockStyle.Fill; root.Controls.Add(control, 1, row); };
                var mstPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false }; mst.Width = 230; mstPanel.Controls.Add(mst); mstPanel.Controls.Add(tai);
                add("MST chủ sở hữu:", mstPanel, 0); add("Tên doanh nghiệp:", ten, 1); add("Số điện thoại:", sdt, 2); add("Loại tài khoản:", loai, 3);
                var radios = new FlowLayoutPanel(); radios.Controls.Add(ngauNhien); radios.Controls.Add(soDep); add("Hình thức cấp số:", radios, 4); add("Số tài khoản:", stk, 5);
                var tools = new FlowLayoutPanel(); tools.Controls.Add(them); tools.Controls.Add(xoa); add("Người được ủy quyền:", tools, 6);
                root.Controls.Add(ds, 0, 7); root.SetColumnSpan(ds, 2);
                var footer = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill }; footer.Controls.Add(huy); footer.Controls.Add(ok); root.Controls.Add(footer, 0, 8); root.SetColumnSpan(footer, 2);
                dlg.Controls.Add(root); dlg.CancelButton = huy;
                foreach (var button in new[] { tai, them, xoa, ok, huy }) { button.BackColor = Color.FromArgb(37, 99, 235); button.ForeColor = Color.White; button.FlatStyle = FlatStyle.Flat; }
                Action gen = () => stk.Text = (soDep.Checked ? "DNTT8888" : "DNTT") + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
                ngauNhien.CheckedChanged += (s, e) => { if (ngauNhien.Checked) gen(); }; soDep.CheckedChanged += (s, e) => { if (soDep.Checked) gen(); }; gen();
                string daTraCuu = null;
                bool daDangKy = false;
                mst.TextChanged += (s, e) => { daTraCuu = null; ten.Clear(); sdt.Clear(); ten.ReadOnly = sdt.ReadOnly = false; ds.Rows.Clear(); };
                Action traCuu = () =>
                {
                    string ma = mst.Text.Trim();
                    if (!MaSoHopLe(ma, 10, 12)) throw new InvalidOperationException("Nhập MST gồm 10 đến 12 chữ số theo dữ liệu hiện tại.");
                    using (var cn = new SqlConnection(cs))
                    {
                        cn.Open();
                        KiemTraMaChuTaiKhoanMoMoiDN(cn, null, ma);
                        using (var cmd = new SqlCommand("SELECT HoTen,SoDienThoai FROM dbo.DN_KhachHang WHERE CCCD=@mst", cn))
                        {
                            cmd.Parameters.AddWithValue("@mst", ma);
                            using (var r = cmd.ExecuteReader()) { daDangKy = r.Read(); ten.Text = daDangKy ? Convert.ToString(r[0]) : ""; sdt.Text = daDangKy ? Convert.ToString(r[1]) : ""; }
                        }
                        ten.ReadOnly = sdt.ReadOnly = daDangKy;
                        ds.Rows.Clear();
                        using (var cmd = new SqlCommand("SELECT DISTINCT CCCD,HoTen,SoDienThoai FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@mst", cn))
                        {
                            cmd.Parameters.AddWithValue("@mst", ma);
                            using (var r = cmd.ExecuteReader()) while (r.Read()) ds.Rows.Add(r[0], r[1], r[2]);
                        }
                    }
                    daTraCuu = ma;
                };
                tai.Click += (s, e) => { try { traCuu(); } catch (Exception ex) { ThongBaoDN(dlg, ex.Message); } };
                dlg.Shown += (s, e) => { if (MaSoHopLe(mst.Text.Trim(), 10, 12)) { try { traCuu(); } catch (Exception ex) { ThongBaoDN(dlg, ex.Message); } } };
                them.Click += (s, e) => ds.Rows.Add();
                xoa.Click += (s, e) => { if (ds.CurrentRow != null) ds.Rows.Remove(ds.CurrentRow); };
                ok.Click += (s, e) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(NguoiThucHien)) throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập.");
                        string ma = mst.Text.Trim();
                        if (daTraCuu != ma) { traCuu(); ThongBaoDN(dlg, "Kiểm tra thông tin doanh nghiệp và người được ủy quyền rồi xác nhận lại."); return; }
                        ds.EndEdit();
                        if (string.IsNullOrWhiteSpace(ten.Text) || ten.Text.Trim().Length > 100 || !MaSoHopLe(sdt.Text.Trim(), 10, 11)) throw new InvalidOperationException("Kiểm tra tên và số điện thoại doanh nghiệp.");
                        var reps = new System.Collections.Generic.Dictionary<string, Tuple<string, string>>();
                        foreach (DataGridViewRow row in ds.Rows)
                        {
                            string c = Convert.ToString(row.Cells[0].Value).Trim(), n = Convert.ToString(row.Cells[1].Value).Trim(), p = Convert.ToString(row.Cells[2].Value).Trim();
                            if (!MaSoHopLe(c, 12, 12) || c == ma || n.Length == 0 || n.Length > 100 || !MaSoHopLe(p, 10, 11) || reps.ContainsKey(c)) throw new InvalidOperationException("Thông tin người được ủy quyền không hợp lệ hoặc trùng CCCD.");
                            reps.Add(c, Tuple.Create(n, p));
                        }
                        if (reps.Count < 2) throw new InvalidOperationException("Cần ít nhất 2 người được ủy quyền.");
                        using (var cn = new SqlConnection(cs))
                        {
                            cn.Open(); using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                // Kiểm tra lại trong transaction trước khi ghi bất kỳ dữ liệu nào.
                                KiemTraMaChuTaiKhoanMoMoiDN(cn, tx, ma);
                                using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.DN_KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@mst AND (HoTen<>@ten OR SoDienThoai<>@sdt)) THROW 52004,N'Thông tin doanh nghiệp đã thay đổi. Hãy tra cứu lại MST.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.DN_KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@mst)
INSERT dbo.DN_KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo,MaSoThue) VALUES(@mst,@ten,@sdt,0,@mst);
IF EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk) OR EXISTS(SELECT 1 FROM dbo.DN_SoTietKiem WHERE SoTKTK=@stk) OR EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanVay WHERE SoTKVay=@stk) THROW 52002,N'Số tài khoản đã tồn tại.',1;
INSERT dbo.DN_TaiKhoanThanhToan(SoTKTT,CCCD,SoDu,TrangThai,NgayMo,HanMucNgay,HanMucLan,LoaiTaiKhoan,TenHienThi,LoaiTienTe,SoTienPhongToa,NgayCapNhatTT)
VALUES(@stk,@mst,0,0,GETDATE(),1000000000,200000000,N'Doanh nghiệp',@ten,N'VND',0,GETDATE());", cn, tx))
                                { cmd.Parameters.AddWithValue("@mst", ma); cmd.Parameters.AddWithValue("@ten", ten.Text.Trim()); cmd.Parameters.AddWithValue("@sdt", sdt.Text.Trim()); cmd.Parameters.AddWithValue("@stk", stk.Text); cmd.ExecuteNonQuery(); }
                                foreach (var rep in reps)
                                    using (var cmd = new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@mst AND CCCD=@c AND (HoTen<>@n OR SoDienThoai<>@p)) THROW 52003,N'Thông tin người được ủy quyền khác dữ liệu đã đăng ký. Hãy sửa thông tin tại danh mục khách hàng trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.DN_DongSoHuu WHERE CCCD_ChuSoHuu=@mst AND CCCD=@c)
INSERT dbo.DN_DongSoHuu(CCCD_ChuSoHuu,CCCD,HoTen,SoDienThoai) VALUES(@mst,@c,@n,@p);
INSERT dbo.DN_DongSoHuuTaiKhoanThanhToan(SoTKTT,CCCD,HoTen,SoDienThoai) VALUES(@stk,@c,@n,@p);", cn, tx))
                                    { cmd.Parameters.AddWithValue("@mst", ma); cmd.Parameters.AddWithValue("@stk", stk.Text); cmd.Parameters.AddWithValue("@c", rep.Key); cmd.Parameters.AddWithValue("@n", rep.Value.Item1); cmd.Parameters.AddWithValue("@p", rep.Value.Item2); cmd.ExecuteNonQuery(); }
                                Ghi(cn, tx, stk.Text, "Mở tài khoản doanh nghiệp", 0, 0); tx.Commit();
                            }
                        }
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex) { ThongBaoDN(dlg, ex.Message, "Không thể mở tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                };
                DongBoMauHopThoaiDN(dlg);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _dangLamMoi = true;
                    try { txtTim.Text = mst.Text.Trim(); cboTrangThai.SelectedIndex = 0; cboThoiGian.SelectedIndex = 0; CapNhatNgay(); }
                    finally { _dangLamMoi = false; }
                    Changed();
                    ThongBaoDN(this, "Đã mở tài khoản doanh nghiệp: " + stk.Text);
                }
            }
        }
        private static bool MaSoHopLe(string s, int min, int max)
        {
            if (s == null || s.Length < min || s.Length > max) return false;
            foreach (char c in s) if (c < '0' || c > '9') return false;
            return true;
        }
        private int Ghi(SqlConnection cn, SqlTransaction tx, string stk, string noiDung, decimal sotien, decimal sodu)
        {
            using (var c = new SqlCommand(@"INSERT dbo.DN_LichSuGiaoDich(SoTKTT,NoiDung,SoTien,SoDuSau,ThoiGian,NguoiThucHien)
VALUES(@stk,@nd,@t,@du,GETDATE(),@nv); SELECT CONVERT(int,SCOPE_IDENTITY());", cn, tx))
            {
                c.Parameters.AddWithValue("@stk", stk); c.Parameters.AddWithValue("@nd", noiDung);
                c.Parameters.AddWithValue("@t", sotien); c.Parameters.AddWithValue("@du", sodu);
                c.Parameters.AddWithValue("@nv", NguoiThucHien); return Convert.ToInt32(c.ExecuteScalar());
            }
        }
        private void GiaoDich(bool nap)
        {
            string stk = SoTK; if (stk == "") { ThongBaoDN("Chọn tài khoản trước."); return; }
            string str = nap ? NhapTienTheoMenhGia(stk) : NhapTienRutDN(stk); if (str == null) return;
            int idHoaDon = 0;
            Run(() => {
                decimal tien = Amount(str);
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        decimal du, phongToa, hanMuc, hanMucNgay;
                        using (var c = new SqlCommand("SELECT SoDu,SoTienPhongToa,HanMucLan,HanMucNgay,LoaiTienTe FROM dbo.DN_TaiKhoanThanhToan WITH(UPDLOCK,HOLDLOCK) WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                        {
                            c.Parameters.AddWithValue("@stk", stk); using (var r = c.ExecuteReader())
                            { if (!r.Read()) throw new InvalidOperationException("Tài khoản đang khóa hoặc đã đóng."); if (!string.Equals(Convert.ToString(r[4]), "VND", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Chức năng này chỉ hỗ trợ tài khoản VND."); du = r.GetDecimal(0); phongToa = r.GetDecimal(1); hanMuc = Convert.ToDecimal(r[2]); hanMucNgay = Convert.ToDecimal(r[3]); }
                        }
                        if (!nap && (du - phongToa < tien || tien > hanMuc)) throw new InvalidOperationException("Số dư khả dụng hoặc hạn mức mỗi lần không đủ.");
                        if (!nap) KiemTraHanMucNgay(cn, tx, stk, tien, hanMucNgay);
                        decimal sau = du + (nap ? tien : -tien);
                        using (var c = new SqlCommand("UPDATE dbo.DN_TaiKhoanThanhToan SET SoDu=@du,NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk", cn, tx))
                        { c.Parameters.AddWithValue("@du", sau); c.Parameters.AddWithValue("@stk", stk); c.ExecuteNonQuery(); }
                        int id = Ghi(cn, tx, stk, nap ? "Nạp tiền doanh nghiệp" : "Rút tiền doanh nghiệp", tien, sau); tx.Commit();
                        idHoaDon = id;
                    }
                }
            });
            if (idHoaDon > 0) InHoaDonSauGiaoDichDN(stk, idHoaDon, nap ? "HÓA ĐƠN NỘP TIỀN MẶT" : "HÓA ĐƠN RÚT TIỀN MẶT");
        }
        // Chỉ thay giao diện nhập tiền rút; GiaoDich(false) vẫn kiểm tra và ghi SQL như cũ.
        private string NhapTienRutDN(string stk)
        {
            decimal soDu, phongToa, hanMuc;
            try
            {
                using (var cn = new SqlConnection(cs))
                using (var cmd = new SqlCommand("SELECT SoDu,SoTienPhongToa,HanMucLan FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk", cn))
                {
                    cmd.Parameters.AddWithValue("@stk", stk);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) throw new InvalidOperationException("Không tìm thấy tài khoản.");
                        soDu = Convert.ToDecimal(rd[0]);
                        phongToa = Convert.ToDecimal(rd[1]);
                        hanMuc = Convert.ToDecimal(rd[2]);
                    }
                }
            }
            catch (Exception ex) { ThongBaoDN(ex.Message); return null; }
            decimal khaDung = Math.Max(0m, soDu - phongToa);
            using (var f = new Form { Text = "Rút tiền mặt doanh nghiệp", ClientSize = new Size(620, 370), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, Font = new Font("Segoe UI", 10F) })
            {
                var title = new Label { Text = "RÚT TIỀN THANH TOÁN - DOANH NGHIỆP", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
                var info = new Label { Text = "STK: " + stk + "\nSố dư: " + soDu.ToString("N0") + " VNĐ\nSố dư khả dụng: " + khaDung.ToString("N0") + " VNĐ | Hạn mức/lần: " + hanMuc.ToString("N0") + " VNĐ", Location = new Point(20, 47), Size = new Size(580, 80) };
                var all = new Button { Text = "RÚT TOÀN BỘ KHẢ DỤNG", Location = new Point(20, 135), Size = new Size(280, 42) };
                var part = new Button { Text = "RÚT MỘT PHẦN", Location = new Point(320, 135), Size = new Size(280, 42) };
                var panel = new Panel { Location = new Point(20, 190), Size = new Size(580, 160), BorderStyle = BorderStyle.FixedSingle };
                var label = new Label { Text = "Số tiền muốn rút (VNĐ):", Location = new Point(12, 12), AutoSize = true };
                var input = new TextBox { Location = new Point(255, 10), Width = 310 };
                var result = new Label { Location = new Point(12, 45), Size = new Size(550, 55), ForeColor = Color.LightGreen };
                var ok = new Button { Text = "Xác nhận rút", Location = new Point(310, 112), Size = new Size(145, 36) };
                var cancel = new Button { Text = "Hủy", Location = new Point(465, 112), Size = new Size(100, 36), DialogResult = DialogResult.Cancel };
                string amount = null;
                Action preview = () =>
                {
                    try { decimal v = Amount(input.Text.Trim()); result.Text = "Tiền rút: " + v.ToString("N0") + " VNĐ\nSố dư dự kiến: " + (soDu - v).ToString("N0") + " VNĐ"; }
                    catch { result.Text = "Nhập số tiền để xem số dư dự kiến."; }
                };
                input.TextChanged += (senderNhap, argsNhap) => preview();
                all.Click += (senderAll, argsAll) => { input.Text = khaDung.ToString("0", CultureInfo.InvariantCulture); input.Focus(); };
                part.Click += (senderPart, argsPart) => { input.Clear(); input.Focus(); };
                ok.Click += (senderOK, argsOK) =>
                {
                    try { amount = Amount(input.Text.Trim()).ToString("0", CultureInfo.InvariantCulture); f.DialogResult = DialogResult.OK; }
                    catch (Exception ex) { ThongBaoDN(f, ex.Message); }
                };
                panel.Controls.AddRange(new Control[] { label, input, result, ok, cancel });
                f.Controls.AddRange(new Control[] { title, info, all, part, panel });
                f.AcceptButton = ok; f.CancelButton = cancel;
                DongBoMauHopThoaiDN(f);
                title.ForeColor = Color.FromArgb(147, 197, 253);
                all.BackColor = Color.FromArgb(185, 28, 28);
                preview();
                return f.ShowDialog(this) == DialogResult.OK ? amount : null;
            }
        }

        private string NhapTienTheoMenhGia(string stk)
        {
            using (var f = new Form { Text = "Nộp tiền mặt vào tài khoản doanh nghiệp", ClientSize = new Size(490, 500), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White, Font = new Font("Segoe UI", 10F) })
            {
                f.Controls.Add(new Label { Text = "Số tài khoản: " + stk, Location = new Point(20, 12), AutoSize = true });
                int[] menhGia = { 500000, 200000, 100000, 50000, 20000, 10000, 5000, 2000, 1000 };
                var soTo = new NumericUpDown[menhGia.Length];
                var tong = new Label { Location = new Point(20, 380), AutoSize = true, ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
                decimal tien = 0;
                Action tinh = () => { tien = 0; for (int i = 0; i < menhGia.Length; i++) if (soTo[i] != null) tien += menhGia[i] * soTo[i].Value; tong.Text = "TỔNG TIỀN NỘP: " + tien.ToString("N0") + " VNĐ"; };
                for (int i = 0; i < menhGia.Length; i++)
                {
                    f.Controls.Add(new Label { Text = menhGia[i].ToString("N0") + " VNĐ", Location = new Point(20, 48 + i * 35), AutoSize = true });
                    soTo[i] = new NumericUpDown { Location = new Point(200, 45 + i * 35), Width = 160, Maximum = 100000, ThousandsSeparator = true };
                    soTo[i].ValueChanged += (s, e) => tinh(); f.Controls.Add(soTo[i]);
                }
                var ok = new Button { Text = "Xác nhận nộp", Location = new Point(190, 430), Size = new Size(140, 36), BackColor = Color.FromArgb(37, 99, 235), FlatStyle = FlatStyle.Flat };
                var cancel = new Button { Text = "Hủy", Location = new Point(340, 430), Size = new Size(100, 36), DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(51, 65, 85), FlatStyle = FlatStyle.Flat };
                ok.Click += (s, e) => { tinh(); if (tien <= 0) { ThongBaoDN(f, "Vui lòng nhập số lượng tiền nộp."); return; } f.DialogResult = DialogResult.OK; };
                f.Controls.AddRange(new Control[] { tong, ok, cancel }); f.AcceptButton = ok; f.CancelButton = cancel; tinh();
                DongBoMauHopThoaiDN(f);
                return f.ShowDialog(this) == DialogResult.OK ? tien.ToString("0", CultureInfo.InvariantCulture) : null;
            }
        }
        private void NapTien(object s, EventArgs e) { GiaoDich(true); }
        private void RutTien(object s, EventArgs e) { GiaoDich(false); }
        private void ChuyenKhoan(object sender, EventArgs e)
        {
            MoKhungChuyenTienDN(Form.ActiveForm ?? this, SoTK, false);
        }

        private void MoKhungChuyenTienDN(Form owner, string nguon, bool moQRNgay)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NguoiThucHien)) throw new InvalidOperationException("Vui lòng đăng nhập.");
                DataRow src = DocTaiKhoanQRDN(nguon);
                using (var dlg = new Form { Text = "Chuyển tiền doanh nghiệp", ClientSize = new Size(520, 365), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, Font = new Font("Segoe UI", 10F) })
                {
                    var info = new Label { Text = "TK nguồn: " + nguon + " (" + Convert.ToString(src["TenDoanhNghiep"]) + ")\nSố dư: " + Convert.ToDecimal(src["SoDu"]).ToString("N0") + " VNĐ", Bounds = new Rectangle(20, 14, 480, 68), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
                    var loai = new ComboBox { Bounds = new Rectangle(170, 87, 330, 28), DropDownStyle = ComboBoxStyle.DropDownList };
                    loai.Items.Add("Trong hệ thống"); loai.SelectedIndex = 0;
                    var bank = new ComboBox { Bounds = new Rectangle(170, 127, 330, 28), DropDownStyle = ComboBoxStyle.DropDownList };
                    bank.Items.Add("Softech Bank (Nội bộ)"); bank.SelectedIndex = 0;
                    var stk = new TextBox { Bounds = new Rectangle(170, 167, 232, 28), MaxLength = 20 };
                    var qr = new Button { Text = "Đọc QR", Bounds = new Rectangle(410, 164, 90, 32) };
                    var ten = new TextBox { Bounds = new Rectangle(170, 207, 330, 28), ReadOnly = true, TextAlign = HorizontalAlignment.Left, RightToLeft = RightToLeft.No };
                    var amount = new TextBox { Bounds = new Rectangle(170, 247, 330, 28), MaxLength = 30 };
                    var ok = new Button { Text = "Chuyển tiền", Bounds = new Rectangle(170, 301, 160, 40) };
                    var cancel = new Button { Text = "Hủy", Bounds = new Rectangle(350, 301, 150, 40), DialogResult = DialogResult.Cancel };
                    string qrStk = null;
                    Action traCuu = () => { ten.Clear(); if (string.IsNullOrWhiteSpace(stk.Text)) return; DataRow dst = DocTaiKhoanQRDN(stk.Text.Trim()); ten.Text = Convert.ToString(dst["TenDoanhNghiep"]); ten.Select(0, 0); };
                    stk.TextChanged += (s, e) => { ten.Clear(); qrStk = null; };
                    stk.Leave += (s, e) => { try { traCuu(); } catch { ten.Clear(); } };
                    qr.Click += (s, e) =>
                    {
                        try
                        {
                            string dich, raw;
                            if (!DocAnhQRChuyenTienDN(dlg, nguon, out dich, out raw)) return;
                            stk.Text = dich; traCuu(); qrStk = dich; amount.Text = raw; amount.Focus();
                        }
                        catch (Exception ex) { ThongBaoDN(dlg, ex.Message); }
                    };
                    ok.Click += (s, e) =>
                    {
                        try
                        {
                            traCuu(); string dich = stk.Text.Trim(); decimal tien = Amount(amount.Text.Trim());
                            if (!XacNhanChuyenTienDN(dlg, nguon, dich, tien, qrStk == dich)) return;
                            dlg.DialogResult = DialogResult.OK;
                        }
                        catch (Exception ex) { ThongBaoDN(dlg, ex.Message); }
                    };
                    string[] labels = { "Loại chuyển:", "Ngân hàng nhận:", "STK nhận:", "Tên doanh nghiệp:", "Số tiền chuyển:" };
                    for (int i = 0; i < labels.Length; i++) dlg.Controls.Add(new Label { Text = labels[i], Bounds = new Rectangle(20, 87 + i * 40, 145, 28), TextAlign = ContentAlignment.MiddleLeft });
                    dlg.Controls.AddRange(new Control[] { info, loai, bank, stk, qr, ten, amount, ok, cancel });
                    dlg.CancelButton = cancel; DongBoMauHopThoaiDN(dlg);
                    info.ForeColor = Color.FromArgb(34, 197, 94);
                    ten.TextAlign = HorizontalAlignment.Left; ten.RightToLeft = RightToLeft.No;
                    if (moQRNgay) dlg.Shown += (s, e) => dlg.BeginInvoke(new Action(() => qr.PerformClick()));
                    if (dlg.ShowDialog(owner) == DialogResult.OK)
                        ThucHienChuyenKhoanDN(nguon, stk.Text.Trim(), amount.Text.Trim());
                }
            }
            catch (Exception ex) { ThongBaoDN(owner, ex.Message); }
        }

        // Chỉ hiển thị thông tin xác nhận; không cập nhật SQL tại đây.
        private bool XacNhanChuyenTienDN(IWin32Window owner, string nguon, string dich, decimal tien, bool quaQR)
        {
            if (string.Equals(nguon, dich, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Không thể chuyển cho chính tài khoản nguồn.");
            DataRow src = DocTaiKhoanQRDN(nguon);
            DataRow dst = DocTaiKhoanQRDN(dich);
            Color navy = Color.FromArgb(15, 23, 42), card = Color.FromArgb(30, 41, 59);
            Color ink = Color.FromArgb(241, 245, 249), muted = Color.FromArgb(148, 163, 184), blue = Color.FromArgb(147, 197, 253);
            using (var f = new Form
            {
                Text = "Xác nhận chuyển tiền doanh nghiệp",
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
                label(f, quaQR ? "Chuyển khoản nội bộ  /  Qua QR" : "Chuyển khoản nội bộ  /  Nhập thủ công", 24, 55, 552, 25, blue, normal);
                var money = new Panel { Bounds = new Rectangle(24, 92, 552, 88), BackColor = card };
                label(money, "SỐ TIỀN CHUYỂN", 16, 8, 520, 23, muted, normal);
                label(money, tien.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ", 16, 31, 520, 48, blue, large);
                f.Controls.Add(money);
                var from = new Panel { Bounds = new Rectangle(24, 192, 552, 108), BackColor = card };
                label(from, "01  /  TÀI KHOẢN NGUỒN", 16, 8, 520, 22, muted, normal);
                label(from, Convert.ToString(src["SoTKTT"]), 16, 30, 520, 26, blue, bold);
                label(from, Convert.ToString(src["TenDoanhNghiep"]), 16, 57, 520, 44, ink, normal);
                f.Controls.Add(from);
                label(f, "↓   CHUYỂN ĐẾN", 40, 302, 520, 30, blue, bold);
                var to = new Panel { Bounds = new Rectangle(24, 334, 552, 108), BackColor = card };
                label(to, "02  /  TÀI KHOẢN NHẬN", 16, 8, 520, 22, muted, normal);
                label(to, Convert.ToString(dst["SoTKTT"]), 16, 30, 520, 26, blue, bold);
                label(to, Convert.ToString(dst["TenDoanhNghiep"]), 16, 57, 520, 44, ink, normal);
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

        // Dùng chung nguyên nghiệp vụ chuyển khoản cho nhập tay và QR.
        private void ThucHienChuyenKhoanDN(string nguon, string dich, string amount)
        {
            int idHoaDon = 0;
            Run(() => {
                decimal tien = Amount(amount); if (string.Equals(dich, nguon, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Không thể chuyển cho chính tài khoản nguồn.");
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        decimal from = 0, to = 0, phong = 0, lim = 0, limNgay = 0; int found = 0;
                        // Lock theo thứ tự số tài khoản để giảm nguy cơ deadlock.
                        foreach (string stk in string.CompareOrdinal(nguon, dich) < 0 ? new[] { nguon, dich } : new[] { dich, nguon })
                            using (var c = new SqlCommand("SELECT SoDu,SoTienPhongToa,HanMucLan,HanMucNgay,LoaiTienTe FROM dbo.DN_TaiKhoanThanhToan WITH(UPDLOCK,HOLDLOCK) WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                            {
                                c.Parameters.AddWithValue("@stk", stk); using (var r = c.ExecuteReader())
                                {
                                    if (!r.Read()) throw new InvalidOperationException("Tài khoản nguồn hoặc đích không hoạt động.");
                                    if (!string.Equals(Convert.ToString(r[4]), "VND", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Chức năng này chỉ hỗ trợ tài khoản VND.");
                                    if (string.Equals(stk, nguon, StringComparison.OrdinalIgnoreCase)) { from = Convert.ToDecimal(r[0]); phong = Convert.ToDecimal(r[1]); lim = Convert.ToDecimal(r[2]); limNgay = Convert.ToDecimal(r[3]); } else to = r.GetDecimal(0); found++;
                                }
                            }
                        if (found != 2 || from - phong < tien || tien > lim) throw new InvalidOperationException("Số dư khả dụng hoặc hạn mức chuyển khoản không đủ.");
                        KiemTraHanMucNgay(cn, tx, nguon, tien, limNgay);
                        using (var c = new SqlCommand("UPDATE dbo.DN_TaiKhoanThanhToan SET SoDu=CASE WHEN SoTKTT=@src THEN SoDu-@t ELSE SoDu+@t END,NgayCapNhatTT=GETDATE() WHERE SoTKTT IN(@src,@dst)", cn, tx))
                        {
                            c.Parameters.AddWithValue("@src", nguon); c.Parameters.AddWithValue("@dst", dich); c.Parameters.AddWithValue("@t", tien);
                            if (c.ExecuteNonQuery() != 2) throw new InvalidOperationException("Không cập nhật đủ hai tài khoản.");
                        }
                        int id = Ghi(cn, tx, nguon, "Chuyển khoản đến " + dich, -tien, from - tien);
                        Ghi(cn, tx, dich, "Nhận chuyển khoản từ " + nguon, tien, to + tien); tx.Commit();
                        idHoaDon = id;
                    }
                }
            });
            if (idHoaDon > 0) InHoaDonSauGiaoDichDN(nguon, idHoaDon, "HÓA ĐƠN CHUYỂN TIỀN", dich);
        }

        // QR noi bo doanh nghiep, khong phai VietQR. Khong chua CCCD/MST hay so du.
        private DataRow DocTaiKhoanQRDN(string stk)
        {
            if (string.IsNullOrWhiteSpace(stk) || stk.Length > 20)
                throw new InvalidOperationException("Số tài khoản QR không hợp lệ.");
            DataTable dt = DocChiTietDN(stk);
            if (dt.Rows.Count != 1) throw new InvalidOperationException("Không tìm thấy tài khoản doanh nghiệp.");
            DataRow r = dt.Rows[0];
            if (Convert.ToInt32(r["TrangThai"]) != 0 || !string.Equals(Convert.ToString(r["LoaiTienTe"]), "VND", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("QR chỉ hỗ trợ tài khoản doanh nghiệp VND đang hoạt động.");
            return r;
        }

        private void HienThiQRNhanTienDN(Form owner, string stk)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NguoiThucHien)) throw new InvalidOperationException("Vui lòng đăng nhập.");
                DataRow r = DocTaiKhoanQRDN(stk);
                using (var f = new Form { Text = "QR nhận tiền doanh nghiệp", ClientSize = new Size(500, 560), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, Font = new Font("Segoe UI", 10F) })
                {
                    var title = new Label { Text = "QR NHẬN TIỀN DOANH NGHIỆP", Bounds = new Rectangle(15, 12, 470, 32), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14F, FontStyle.Bold) };
                    var pic = new PictureBox { Bounds = new Rectangle(120, 52, 260, 260), BackColor = Color.White, SizeMode = PictureBoxSizeMode.Zoom };
                    var info = new Label { Text = Convert.ToString(r["TenDoanhNghiep"]) + "\n" + stk, Bounds = new Rectangle(15, 320, 470, 58), TextAlign = ContentAlignment.MiddleCenter };
                    var label = new Label { Text = "Số tiền VNĐ (để trống nếu không cố định):", Bounds = new Rectangle(25, 385, 450, 24) };
                    var amount = new TextBox { Bounds = new Rectangle(25, 415, 450, 28), MaxLength = 20 };
                    var create = new Button { Text = "Tạo QR", Bounds = new Rectangle(25, 458, 215, 40) };
                    var save = new Button { Text = "Lưu ảnh PNG", Bounds = new Rectangle(260, 458, 215, 40), Enabled = false };
                    var note = new Label { Text = "QR nội bộ QLTKNH • Không phải VietQR", Bounds = new Rectangle(20, 510, 460, 35), TextAlign = ContentAlignment.MiddleCenter };
                    Action clear = () => { Image old = pic.Image; pic.Image = null; if (old != null) old.Dispose(); save.Enabled = false; };
                    Action render = () =>
                    {
                        clear();
                        try
                        {
                            string raw = amount.Text.Trim();
                            if (raw.Length > 0) raw = Amount(raw).ToString("0", CultureInfo.InvariantCulture);
                            var writer = new ZXing.BarcodeWriter { Format = ZXing.BarcodeFormat.QR_CODE, Options = new ZXing.Common.EncodingOptions { Width = 520, Height = 520, Margin = 4 } };
                            pic.Image = writer.Write("QLTKNH:TT:DN:1|" + stk + "|" + raw);
                            save.Enabled = true;
                        }
                        catch (Exception ex) { ThongBaoDN(f, ex.Message); }
                    };
                    amount.TextChanged += (s, e) => clear();
                    create.Click += (s, e) => render();
                    save.Click += (s, e) =>
                    {
                        if (pic.Image == null) return;
                        using (var file = new SaveFileDialog { Filter = "PNG (*.png)|*.png", FileName = "QR_" + stk + ".png" })
                            if (file.ShowDialog(f) == DialogResult.OK)
                                try { pic.Image.Save(file.FileName, System.Drawing.Imaging.ImageFormat.Png); }
                                catch (Exception ex) { ThongBaoDN(f, ex.Message); }
                    };
                    f.Controls.AddRange(new Control[] { title, pic, info, label, amount, create, save, note });
                    DongBoMauHopThoaiDN(f);
                    pic.BackColor = Color.White;
                    try { render(); f.ShowDialog(owner); } finally { clear(); }
                }
            }
            catch (Exception ex) { ThongBaoDN(owner, ex.Message); }
        }

        private void ChuyenTienQuaQRDN(Form owner, string nguon)
        {
            MoKhungChuyenTienDN(owner, nguon, true);
        }

        private bool DocAnhQRChuyenTienDN(Form owner, string nguon, out string dich, out string raw)
        {
            dich = null; raw = null;
            string payload;
            using (var file = new OpenFileDialog { Filter = "Ảnh QR (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp" })
            {
                if (file.ShowDialog(owner) != DialogResult.OK) return false;
                using (var bmp = new Bitmap(file.FileName))
                {
                    var reader = new ZXing.BarcodeReader();
                    reader.Options.PossibleFormats = new System.Collections.Generic.List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE };
                    var result = reader.Decode(bmp);
                    if (result == null) throw new InvalidOperationException("Không đọc được mã QR.");
                    payload = result.Text;
                }
            }
            string[] parts = payload.Split('|');
            if (parts.Length != 3 || parts[0] != "QLTKNH:TT:DN:1" || parts[1].Length == 0 || parts[1].Length > 20)
                throw new InvalidOperationException("QR không đúng định dạng doanh nghiệp QLTKNH.");
            foreach (char c in parts[1]) if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                    throw new InvalidOperationException("Số tài khoản QR không hợp lệ.");
            DataRow r = DocTaiKhoanQRDN(parts[1]);
            dich = Convert.ToString(r["SoTKTT"]);
            if (string.Equals(nguon, dich, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Không thể chuyển cho chính tài khoản nguồn.");
            raw = parts[2];
            decimal parsed;
            if (raw.Length > 0 && (!decimal.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out parsed) || parsed <= 0))
                throw new InvalidOperationException("Số tiền trong QR không hợp lệ.");
            return true;
        }

        // Gộp các giao dịch chi ra trong ngày; được kiểm tra trong cùng transaction với giao dịch.
        private void KiemTraHanMucNgay(SqlConnection cn, SqlTransaction tx, string stk, decimal soTien, decimal hanMucNgay)
        {
            using (var c = new SqlCommand(@"SELECT ISNULL(SUM(ABS(SoTien)),0)
FROM dbo.DN_LichSuGiaoDich WITH(UPDLOCK,HOLDLOCK)
WHERE SoTKTT=@stk AND ThoiGian>=CONVERT(date,GETDATE()) AND ThoiGian<DATEADD(day,1,CONVERT(date,GETDATE()))
AND (NoiDung LIKE N'Rút tiền doanh nghiệp%' OR NoiDung LIKE N'Chuyển khoản đến%')", cn, tx))
            {
                c.Parameters.AddWithValue("@stk", stk);
                decimal daChi = Convert.ToDecimal(c.ExecuteScalar());
                if (soTien > hanMucNgay - daChi) throw new InvalidOperationException("Vượt hạn mức giao dịch trong ngày.");
            }
        }
        // Ap dung mau ngay khi ve o, ke ca sau khi form cha dong bo giao dien.
        private void ToMauDanhSachThanhToanDN(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            string cot = dgv.Columns[e.ColumnIndex].Name;
            string giaTri = Convert.ToString(e.Value);
            Color mau;
            if (cot == "TenSoHuu" && dgv.Columns.Contains("VaiTro"))
            {
                if (Convert.ToString(dgv.Rows[e.RowIndex].Cells["VaiTro"].Value) == "Người được ủy quyền")
                    e.Value = "↳ " + giaTri;
                return;
            }
            if (cot == "VaiTro")
                mau = giaTri == "Chủ" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(168, 85, 247);
            else if (cot == "TrangThai")
                mau = MauTrangThaiDN(giaTri);
            else return;
            e.CellStyle.ForeColor = mau;
            e.CellStyle.SelectionForeColor = mau;
            // Nen chon toi giup mau xanh va tim van de doc.
            e.CellStyle.SelectionBackColor = Color.FromArgb(30, 41, 59);
        }

        private static Color MauTrangThaiDN(string trangThai)
        {
            return trangThai == "Hoạt động" ? Color.FromArgb(34, 197, 94) :
                trangThai == "Đang khóa" ? Color.FromArgb(249, 115, 22) : Color.FromArgb(239, 68, 68);
        }

        private string NhapLyDoTrangThaiDN(string stk, string thaoTac, string ghiChu)
        {
            using (var f = new Form
            {
                Text = thaoTac,
                ClientSize = new Size(520, 290),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Font = new Font("Segoe UI", 10F)
            })
            {
                var title = new Label { Text = thaoTac.ToUpperInvariant(), Location = new Point(18, 15), Size = new Size(480, 28), Font = new Font("Segoe UI", 13F, FontStyle.Bold) };
                var info = new Label { Text = "Số tài khoản: " + stk + "\n" + ghiChu, Location = new Point(18, 50), Size = new Size(480, 55) };
                var lbl = new Label { Text = "Lý do (bắt buộc):", Location = new Point(18, 110), AutoSize = true };
                var txt = new TextBox { Location = new Point(18, 138), Size = new Size(484, 85), Multiline = true, MaxLength = 400, ScrollBars = ScrollBars.Vertical };
                var ok = new Button { Text = "Xác nhận", Location = new Point(260, 240), Size = new Size(118, 34) };
                var cancel = new Button { Text = "Hủy", Location = new Point(388, 240), Size = new Size(114, 34), DialogResult = DialogResult.Cancel };
                ok.Click += (senderLuu, argsLuu) =>
                {
                    if (string.IsNullOrWhiteSpace(txt.Text)) { ThongBaoDN("Vui lòng nhập lý do.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); txt.Focus(); return; }
                    f.DialogResult = DialogResult.OK;
                };
                f.Controls.AddRange(new Control[] { title, info, lbl, txt, ok, cancel });
                f.AcceptButton = ok; f.CancelButton = cancel;
                DongBoMauHopThoaiDN(f);
                return f.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : null;
            }
        }

        private void KhoaMo(object sender, EventArgs e)
        {
            string stk = SoTK; if (stk == "") return;
            int trangThaiBanDau;
            try
            {
                DataTable dt = DocChiTietDN(stk);
                if (dt.Rows.Count == 0) throw new InvalidOperationException("Không tìm thấy tài khoản.");
                trangThaiBanDau = Convert.ToInt32(dt.Rows[0]["TrangThai"]);
                if (trangThaiBanDau != 0 && trangThaiBanDau != 1) throw new InvalidOperationException("Tài khoản không thể khóa hoặc mở khóa.");
            }
            catch (Exception ex) { ThongBaoDN(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string lyDo = NhapLyDoTrangThaiDN(stk, trangThaiBanDau == 0 ? "Tạm khóa tài khoản" : "Mở khóa tài khoản", "Nhập lý do thay đổi trạng thái tài khoản.");
            if (lyDo == null) return;
            Run(() => {
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        int old; decimal bal;
                        using (var c = new SqlCommand("SELECT TrangThai,SoDu FROM dbo.DN_TaiKhoanThanhToan WITH(UPDLOCK,HOLDLOCK) WHERE SoTKTT=@stk", cn, tx))
                        {
                            c.Parameters.AddWithValue("@stk", stk);
                            using (var r = c.ExecuteReader()) { if (!r.Read()) throw new InvalidOperationException("Không tìm thấy tài khoản."); old = Convert.ToInt32(r[0]); bal = Convert.ToDecimal(r[1]); }
                        }
                        if (old == 2) throw new InvalidOperationException("Tài khoản đã đóng.");
                        if (old != trangThaiBanDau) throw new InvalidOperationException("Trạng thái đã thay đổi. Vui lòng thực hiện lại.");
                        int next = old == 0 ? 1 : 0;
                        using (var c = new SqlCommand("UPDATE dbo.DN_TaiKhoanThanhToan SET TrangThai=@tt,SoTienPhongToa=CASE WHEN @tt=1 THEN SoDu ELSE 0 END,NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk", cn, tx))
                        {
                            c.Parameters.AddWithValue("@tt", next); c.Parameters.AddWithValue("@stk", stk); c.ExecuteNonQuery();
                        }
                        Ghi(cn, tx, stk, (next == 1 ? "Khóa tài khoản doanh nghiệp" : "Mở khóa tài khoản doanh nghiệp") + " | Lý do: " + lyDo, 0, bal);
                        tx.Commit();
                    }
                }
            });
        }
        private void DongTaiKhoan(object sender, EventArgs e)
        {
            string stk = SoTK; if (stk == "") return;
            string lyDo = NhapLyDoTrangThaiDN(stk, "Đóng tài khoản", "Chỉ thực hiện khi số dư và phong tỏa đều bằng 0.");
            if (lyDo == null) return;
            Run(() => {
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (var c = new SqlCommand("UPDATE dbo.DN_TaiKhoanThanhToan SET TrangThai=2,NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk AND TrangThai<>2 AND SoDu=0 AND SoTienPhongToa=0", cn, tx))
                        {
                            c.Parameters.AddWithValue("@stk", stk); if (c.ExecuteNonQuery() != 1) throw new InvalidOperationException("Tài khoản còn số dư/phong tỏa hoặc đã đóng.");
                        }
                        Ghi(cn, tx, stk, "Đóng tài khoản doanh nghiệp | Lý do: " + lyDo, 0, 0); tx.Commit();
                    }
                }
            });
        }
        private DataTable LichSu(string stk)
        {
            return DocLichSu(stk, false);
        }
        private DataTable DocLichSu(string stk, bool ganNhat)
        {
            using (var cn = new SqlConnection(cs)) using (var c = new SqlCommand("SELECT " + (ganNhat ? "TOP (1) " : "") + "ThoiGian,NoiDung,SoTien,SoDuSau,NguoiThucHien FROM dbo.DN_LichSuGiaoDich WHERE SoTKTT=@stk ORDER BY ThoiGian DESC,Id DESC", cn))
            { c.Parameters.AddWithValue("@stk", stk); var dt = new DataTable(); new SqlDataAdapter(c).Fill(dt); return dt; }
        }
        private void XemLichSu(object sender, EventArgs e)
        {
            string stk = SoTK; if (stk == "") return;
            try
            {
                var f = new Form { Text = "Lịch sử tài khoản doanh nghiệp " + stk, Size = new Size(1080, 650), StartPosition = FormStartPosition.CenterParent };
                var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, DataSource = LichSu(stk) };
                f.Controls.Add(grid); DongBoMauHopThoaiDN(f); f.ShowDialog(this);
            }
            catch (Exception ex) { ThongBaoDN(ex.Message); }
        }
        // Chỉ dùng cho xuất chứng từ; không thay đổi lịch sử hoạt động đang hiển thị.
        private static string ChuanHoaNoiDungXuat(string value)
        {
            var b = new StringBuilder();
            foreach (char c in (value ?? "").Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    b.Append(char.ToLowerInvariant(c));
            return b.ToString().Normalize(NormalizationForm.FormC).Trim();
        }

        private static bool LaGiaoDichDeXuat(DataRow r)
        {
            string a = ChuanHoaNoiDungXuat(Convert.ToString(r["NoiDung"]));
            // Ưu tiên hành động quản lý ở đầu nội dung, kể cả tên hiển thị chứa từ chuyển tiền.
            string[] hoatDong = { "mo tai khoan", "mo them tai khoan", "mo tk", "dong tai khoan", "da dong tai khoan", "dong tk", "dong so", "dong khoan vay", "tam khoa", "mo khoa", "khoa", "phong toa", "go phong toa", "cap lai", "sang ten", "chuyen quyen", "sua thong tin", "thay doi thong tin", "cap nhat", "dang ky", "dieu chinh", "chuyen nhom no", "chuyen trang thai", "xu ly rui ro", "thiet lap", "huy dang ky", "thay doi trang thai" };
            foreach (string tu in hoatDong)
                if (a.StartsWith(tu, StringComparison.Ordinal)) return false;
            if (a.StartsWith("chuyen ", StringComparison.Ordinal)) return true;
            string[] giaoDich = { "chuyen ngoai", "chuyen noi bo", "chuyen lien ngan hang", "chuyen khoan", "chuyen tien", "chuyen den", "chuyen di", "citad", "napas", "chuyen nhanh", "chuyen cung ngan hang", "nap tien", "nop tien", "rut tien", "rut lai", "rut goc", "rut 1 phan", "rut mot phan", "gui them", "gui tien", "tra goc", "tra lai", "tra theo ky", "tra no", "thanh toan khoan vay", "thanh toan no", "giai ngan", "tat toan", "thu phi", "phi phat", "nhan tien", "in sao ke", "sao ke", "lai dinh ky" };
            foreach (string tu in giaoDich)
                if (a.Contains(tu)) return true;
            return ChuanHoaNoiDungXuat(Convert.ToString(r["LoaiLichSu"])) == "giao dich";
        }

        private DataTable DocGiaoDichDeXuat(string stk)
        {
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                bool coLoai;
                using (var check = new SqlCommand("SELECT COL_LENGTH(N'dbo.DN_LichSuGiaoDich', N'LoaiLichSu')", cn))
                {
                    object v = check.ExecuteScalar();
                    coLoai = v != null && v != DBNull.Value;
                }
                string sql = @"SELECT ls.ThoiGian,ls.NoiDung,ISNULL(ls.SoTien,0) AS SoTien,ls.SoDuSau,
                    ls.NguoiThucHien,t.CCCD AS MST,k.HoTen AS TenDoanhNghiep," +
                    (coLoai ? "ls.LoaiLichSu" : "CAST(NULL AS NVARCHAR(20))") + @" AS LoaiLichSu
                    FROM dbo.DN_LichSuGiaoDich ls
                    INNER JOIN dbo.DN_TaiKhoanThanhToan t ON ls.SoTKTT=t.SoTKTT
                    INNER JOIN dbo.DN_KhachHang k ON t.CCCD=k.CCCD
                    WHERE ls.SoTKTT=@stk ORDER BY ls.ThoiGian DESC,ls.Id DESC";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@stk", stk);
                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(dt);
                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        if (!LaGiaoDichDeXuat(dt.Rows[i])) dt.Rows.RemoveAt(i);
                    return dt;
                }
            }
        }

        private static string NhanVienTrenChungTu(DataRow r)
        {
            string ten = Convert.ToString(r["NguoiThucHien"]).Trim();
            return string.IsNullOrWhiteSpace(ten) || string.Equals(ten, "Tự thực hiện", StringComparison.OrdinalIgnoreCase)
                ? "Chưa xác định" : ten;
        }
        private static string TienTrenChungTu(object value)
        {
            return value == null || value == DBNull.Value ? "Chưa ghi nhận" : Convert.ToDecimal(value).ToString("N0") + " VNĐ";
        }
        private static string H(string value) { return WebUtility.HtmlEncode(value ?? ""); }

        private void XuatBaoCao(object sender, EventArgs e) { XuatChungTuThanhToan(false); }
        private void XuatHoaDon(object sender, EventArgs e) { XuatChungTuThanhToan(true); }

        private void XuatChungTuThanhToan(bool hoaDon)
        {
            string stk = SoTK;
            if (string.IsNullOrWhiteSpace(stk)) { ThongBaoDN("Vui lòng chọn tài khoản trước."); return; }
            try
            {
                DataTable dt = DocGiaoDichDeXuat(stk);
                if (!hoaDon)
                {
                    DateTime? tu, den;
                    if (!ChonKyBaoCao(out tu, out den)) return;
                    if (tu.HasValue && den.HasValue)
                        for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        {
                            DateTime ngay = Convert.ToDateTime(dt.Rows[i]["ThoiGian"]).Date;
                            if (ngay < tu.Value || ngay > den.Value) dt.Rows.RemoveAt(i);
                        }
                }
                if (dt.Rows.Count == 0) { ThongBaoDN("Không có giao dịch để xuất chứng từ."); return; }
                // Hóa đơn chỉ lấy một giao dịch sau khi đã loại hoạt động quản lý.
                if (hoaDon) while (dt.Rows.Count > 1) dt.Rows.RemoveAt(dt.Rows.Count - 1);
                string tieuDe = hoaDon ? "HÓA ĐƠN GIAO DỊCH" : "BÁO CÁO LỊCH SỬ GIAO DỊCH";
                using (var save = new SaveFileDialog())
                {
                    save.Title = hoaDon ? "Lưu hóa đơn để in PDF" : "Xuất báo cáo toàn bộ giao dịch";
                    save.Filter = hoaDon ? "Trang in HTML (*.html)|*.html" : "Tệp văn bản (*.txt)|*.txt|Trang in HTML (*.html)|*.html";
                    save.DefaultExt = hoaDon ? "html" : "txt";
                    save.AddExtension = true;
                    save.FileName = (hoaDon ? "HoaDonDN_" : "BaoCaoDN_") + stk + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    if (save.ShowDialog(this) != DialogResult.OK) return;
                    if (!hoaDon && save.FilterIndex == 1)
                    {
                        var txt = new StringBuilder();
                        txt.AppendLine(tieuDe).AppendLine("Số tài khoản: " + stk)
                            .AppendLine("MST: " + dt.Rows[0]["MST"]).AppendLine("Doanh nghiệp: " + dt.Rows[0]["TenDoanhNghiep"])
                            .AppendLine("Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                            .AppendLine("Tổng số giao dịch: " + dt.Rows.Count);
                        int i = 0;
                        foreach (DataRow r in dt.Rows)
                        {
                            txt.AppendLine(new string('-', 66)).AppendLine("GIAO DỊCH " + (++i))
                                .AppendLine("Thời gian: " + Convert.ToDateTime(r["ThoiGian"]).ToString("dd/MM/yyyy HH:mm:ss"))
                                .AppendLine("Nội dung: " + r["NoiDung"]).AppendLine("Số tiền: " + TienTrenChungTu(r["SoTien"]))
                                .AppendLine("Số dư sau: " + TienTrenChungTu(r["SoDuSau"]))
                                .AppendLine("Người thực hiện: " + NhanVienTrenChungTu(r));
                        }
                        File.WriteAllText(save.FileName, txt.ToString(), new UTF8Encoding(true));
                        ThongBaoDN("Đã xuất báo cáo TXT gồm " + dt.Rows.Count + " giao dịch.");
                        return;
                    }
                    var html = new StringBuilder();
                    html.Append(@"<!doctype html><html lang='vi'><head><meta charset='utf-8'><title>QLTKNH - Chứng từ</title><style>
@page{size:A4 landscape;margin:12mm}body{font-family:'Segoe UI',Arial,sans-serif;color:#17213a;margin:24px}h1{text-align:center;color:#1d4ed8;font-size:22px}.subtitle{text-align:center;color:#475569;margin-bottom:16px}table{width:100%;border-collapse:collapse;font-size:11px}th{background:#1e3a8a;color:white;padding:8px;text-align:left}td{border:1px solid #cbd5e1;padding:6px;vertical-align:top;overflow-wrap:anywhere}tr:nth-child(even) td{background:#f1f5f9}.num{text-align:right;white-space:nowrap}.footer{margin-top:20px;font-size:11px;color:#64748b}@media print{body{margin:0}thead{display:table-header-group}tr{break-inside:avoid}}</style></head><body>");
                    html.Append("<h1>").Append(H(tieuDe)).Append("</h1><div class='subtitle'>Tài khoản thanh toán doanh nghiệp</div>")
                        .Append("<p>Số tài khoản: <b>").Append(H(stk)).Append("</b> | MST: ").Append(H(Convert.ToString(dt.Rows[0]["MST"])))
                        .Append(" | Doanh nghiệp: ").Append(H(Convert.ToString(dt.Rows[0]["TenDoanhNghiep"]))).Append("</p>")
                        .Append("<p>Tổng số giao dịch: ").Append(dt.Rows.Count).Append(" | Ngày lập: ").Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).Append("</p>")
                        .Append("<table><thead><tr><th>STT</th><th>Ngày giao dịch</th><th>Nội dung</th><th>Số tiền</th><th>Số dư sau</th><th>Người thực hiện</th></tr></thead><tbody>");
                    int stt = 0;
                    foreach (DataRow r in dt.Rows)
                        html.Append("<tr><td>").Append(++stt).Append("</td><td>").Append(Convert.ToDateTime(r["ThoiGian"]).ToString("dd/MM/yyyy HH:mm:ss"))
                            .Append("</td><td>").Append(H(Convert.ToString(r["NoiDung"])))
                            .Append("</td><td class='num'>").Append(H(TienTrenChungTu(r["SoTien"])))
                            .Append("</td><td class='num'>").Append(H(TienTrenChungTu(r["SoDuSau"])))
                            .Append("</td><td>").Append(H(NhanVienTrenChungTu(r))).Append("</td></tr>");
                    html.Append("</tbody></table><div class='footer'>Dữ liệu lịch sử QLTKNH. Dùng Ctrl+P để in hoặc lưu PDF.</div></body></html>");
                    File.WriteAllText(save.FileName, html.ToString(), new UTF8Encoding(true));
                    Process.Start(new ProcessStartInfo(save.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex) { ThongBaoDN("Không thể xuất chứng từ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        // Chỉ gọi sau Commit. Lấy đúng Id lịch sử vừa ghi, không lấy TOP 1.
        private void InHoaDonSauGiaoDichDN(string stk, int id, string loaiHoaDon, string dich = null)
        {
            try
            {
                IWin32Window owner = Form.ActiveForm ?? this;
                if (ThongBaoDN(owner, "Giao dịch thành công. Bạn có muốn in hóa đơn PDF cho giao dịch này?",
                    "In hóa đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                var dt = new DataTable();
                string tenNhan = "";
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"SELECT ls.ThoiGian,ls.NoiDung,ls.SoTien,ls.SoDuSau,ls.NguoiThucHien,
                        t.CCCD AS MST,k.HoTen AS TenDoanhNghiep FROM dbo.DN_LichSuGiaoDich ls
                        JOIN dbo.DN_TaiKhoanThanhToan t ON t.SoTKTT=ls.SoTKTT
                        JOIN dbo.DN_KhachHang k ON k.CCCD=t.CCCD WHERE ls.Id=@id AND ls.SoTKTT=@stk", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                        using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                    }
                    if (!string.IsNullOrWhiteSpace(dich))
                        using (var cmd = new SqlCommand(@"SELECT k.HoTen FROM dbo.DN_TaiKhoanThanhToan t
                            JOIN dbo.DN_KhachHang k ON k.CCCD=t.CCCD WHERE t.SoTKTT=@stk", cn))
                        {
                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = dich;
                            tenNhan = Convert.ToString(cmd.ExecuteScalar());
                        }
                }
                if (dt.Rows.Count != 1) throw new InvalidOperationException("Không tìm thấy giao dịch vừa thực hiện.");
                DataRow r = dt.Rows[0];
                DateTime ngay = Convert.ToDateTime(r["ThoiGian"]);
                string noiDung = Convert.ToString(r["NoiDung"]);
                if (!string.IsNullOrWhiteSpace(dich)) noiDung += "\nNgười nhận: " + tenNhan + "\nSTK nhận: " + dich;
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
<div class='muted'>Tài khoản thanh toán doanh nghiệp</div></div>
<div class='muted'>Mã hóa đơn: <b>{{MA}}</b><br>Ngày giao dịch: {{NGAY}}</div></header>
<h2>{{LOAI}}</h2><div class='subtitle'>Chứng từ cho một giao dịch đã thực hiện</div>
<section class='card'>
<div class='line'><span>Doanh nghiệp</span><b>{{TEN}}</b></div>
<div class='line'><span>Mã số thuế</span><b>{{MST}}</b></div>
<div class='line'><span>Số tài khoản</span><b>{{STK}}</b></div>
<div class='line'><span>Nội dung giao dịch</span><b>{{NOIDUNG}}</b></div>
<div class='line total'><span>Số tiền giao dịch</span><b>{{SOTIEN}}</b></div>
<div class='line'><span>Số dư sau giao dịch</span><b>{{SODU}}</b></div>
<div class='line'><span>Trạng thái</span><b>Thành công</b></div>
</section>
<div class='sign'><div><b>Khách hàng</b><p class='muted'>(Ký, ghi rõ họ tên)</p><div class='space'></div><b>{{TEN}}</b></div>
<div><b>Giao dịch viên</b><p class='muted'>(Ký xác nhận)</p><div class='space'></div><b>{{NV}}</b></div></div>
</main></body></html>";
                var fields = new System.Collections.Generic.Dictionary<string, string>
                {
                    {"MA", "HD-DN-" + id}, {"NGAY", ngay.ToString("dd/MM/yyyy HH:mm:ss")},
                    {"LOAI", loaiHoaDon}, {"TEN", Convert.ToString(r["TenDoanhNghiep"])},
                    {"MST", Convert.ToString(r["MST"])}, {"STK", stk}, {"NOIDUNG", noiDung},
                    {"SOTIEN", Math.Abs(Convert.ToDecimal(r["SoTien"])).ToString("N0") + " VNĐ"},
                    {"SODU", TienTrenChungTu(r["SoDuSau"])}, {"NV", NhanVienTrenChungTu(r)}
                };
                html = System.Text.RegularExpressions.Regex.Replace(html, @"\{\{([A-Z]+)\}\}", m => H(fields[m.Groups[1].Value]));
                using (var save = new SaveFileDialog { Title = "Lưu hóa đơn để in PDF", Filter = "Trang hóa đơn (*.html)|*.html", DefaultExt = "html", AddExtension = true, FileName = "HoaDonDN_" + stk + "_" + id + ".html" })
                {
                    if (save.ShowDialog(owner) != DialogResult.OK) return;
                    File.WriteAllText(save.FileName, html, new UTF8Encoding(true));
                    if (ThongBaoDN(owner, "Đã lưu hóa đơn. Mở trình duyệt, nhấn Ctrl+P và chọn Save as PDF để lưu PDF?", "In hóa đơn PDF", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        Process.Start(new ProcessStartInfo(save.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                ThongBaoDN("Giao dịch đã thành công. Không thể xuất hóa đơn: " + ex.Message, "Lỗi xuất hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ChonKyBaoCao(out DateTime? tu, out DateTime? den)
        {
            tu = den = null;
            using (var f = new Form { Text = "Chọn kỳ báo cáo", ClientSize = new Size(440, 190), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White, Font = new Font("Segoe UI", 10F) })
            {
                var all = new CheckBox { Text = "Tất cả ngày", Checked = true, AutoSize = true, Location = new Point(20, 15) };
                var a = new DateTimePicker { Location = new Point(130, 50), Width = 245, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = DateTime.Today.AddMonths(-1), Enabled = false };
                var b = new DateTimePicker { Location = new Point(130, 85), Width = 245, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Enabled = false };
                var ok = new Button { Text = "Xuất báo cáo", Location = new Point(165, 135), Size = new Size(125, 35), BackColor = Color.FromArgb(37, 99, 235), FlatStyle = FlatStyle.Flat };
                var cancel = new Button { Text = "Hủy", Location = new Point(300, 135), Size = new Size(75, 35), DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(51, 65, 85), FlatStyle = FlatStyle.Flat };
                all.CheckedChanged += (s, e) => a.Enabled = b.Enabled = !all.Checked;
                ok.Click += (s, e) => { if (!all.Checked && a.Value.Date > b.Value.Date) { ThongBaoDN(f, "Ngày bắt đầu không được lớn hơn ngày kết thúc."); return; } f.DialogResult = DialogResult.OK; };
                f.Controls.AddRange(new Control[] { all, new Label { Text = "Từ ngày:", Location = new Point(20, 53), AutoSize = true }, a, new Label { Text = "Đến ngày:", Location = new Point(20, 88), AutoSize = true }, b, ok, cancel });
                f.AcceptButton = ok; f.CancelButton = cancel;
                DongBoMauHopThoaiDN(f);
                if (f.ShowDialog(this) != DialogResult.OK) return false;
                if (!all.Checked) { tu = a.Value.Date; den = b.Value.Date; }
                return true;
            }
        }
        private void CapNhatNgay()
        {
            bool custom = cboThoiGian.SelectedIndex == 7;
            pnlTop.Height = custom && pnlTop.Width < 1180 ? 140 : 100;
            lblTu.Visible = lblDen.Visible = dtpTu.Visible = dtpDen.Visible = custom;
            DateTime now = DateTime.Today;
            switch (cboThoiGian.SelectedIndex)
            {
                case 1: dtpTu.Value = now; dtpDen.Value = now; break;
                case 2: dtpTu.Value = now.AddDays(-1); dtpDen.Value = now.AddDays(-1); break;
                case 3: dtpTu.Value = now.AddDays(-6); dtpDen.Value = now; break;
                case 4: dtpTu.Value = now.AddDays(-29); dtpDen.Value = now; break;
                case 5: dtpTu.Value = now.AddMonths(-3); dtpDen.Value = now; break;
                case 6: dtpTu.Value = now.AddYears(-1); dtpDen.Value = now; break;
            }
        }
        private void CapNhatTongQuan()
        {
            if (_dangNapDuLieu || lblOverview == null) return;
            if (dgv.CurrentRow == null || !dgv.Columns.Contains("SoTKTT") || !dgv.Columns.Contains("MaDinhDanh")) { lblOverview.Text = "Chưa chọn tài khoản · Nhấp đúp để thao tác, chuột phải để mở menu"; return; }
            DataGridViewRow r = dgv.CurrentRow;
            lblOverview.Text = "Đã chọn: " + Convert.ToString(r.Cells["SoTKTT"].Value) + " | " + Convert.ToString(r.Cells["TenSoHuu"].Value) + " | " + (Convert.ToString(r.Cells["VaiTro"].Value) == "Chủ" ? "MST: " : "CCCD: ") + Convert.ToString(r.Cells["MaDinhDanh"].Value) + " | Số dư: " + Convert.ToDecimal(r.Cells["SoDu"].Value).ToString("N0") + " " + Convert.ToString(r.Cells["LoaiTienTe"].Value);
        }
        private int TrangThaiDangChon()
        {
            if (dgv.CurrentRow == null || !dgv.Columns.Contains("TrangThai")) return -1;
            string value = Convert.ToString(dgv.CurrentRow.Cells["TrangThai"].Value);
            return value == "Hoạt động" ? 0 : value == "Đang khóa" ? 1 : 2;
        }
        private ToolStripMenuItem MenuItem(string text, EventHandler action, bool enabled = true)
        {
            var item = new ToolStripMenuItem(text) { Enabled = enabled }; item.Click += action; return item;
        }
        private void TaoMenuChuoiPhai()
        {
            while (cmsTaiKhoan.Items.Count > 0) { var old = cmsTaiKhoan.Items[0]; cmsTaiKhoan.Items.RemoveAt(0); old.Dispose(); }
            cmsTaiKhoan.Items.Add(
                MenuItem("Mở thêm tài khoản", MoTaiKhoan, LaDongChuTaiKhoanDangChonDN())
            );
        }
        // Giao diện nhấp đúp đồng bộ với FrmThanhToan cá nhân:
        // ba thẻ tổng quan, bảng sở hữu lớn, bốn nhóm chức năng trên một hàng.
        private void ChiTietTaiKhoan()
        {
            string stk = SoTK;
            if (string.IsNullOrWhiteSpace(stk)) return;
            // Không truy vấn SQL đồng bộ trước khi cửa sổ hiện lên.
            // Việc đọc số dư và danh sách sở hữu được thực hiện sau sự kiện Shown.

            Color bg = Color.FromArgb(15, 23, 42);
            Color card = Color.FromArgb(25, 36, 57);
            Color white = Color.FromArgb(241, 245, 249);
            Color muted = Color.FromArgb(160, 177, 202);
            Color border = Color.FromArgb(52, 67, 92);
            using (var dlg = new Form())
            {
                dlg.Text = "Chi tiết & chức năng tài khoản thanh toán doanh nghiệp - " + stk;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.MinimumSize = new Size(1120, 690);
                dlg.BackColor = bg;
                dlg.ForeColor = white;
                dlg.Font = new Font("Segoe UI", 10F);

                var root = new TableLayoutPanel
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

                var header = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
                var title = new Label
                {
                    Text = "TÀI KHOẢN THANH TOÁN DOANH NGHIỆP",
                    Dock = DockStyle.Top,
                    Height = 34,
                    ForeColor = white,
                    Font = new Font("Segoe UI", 19F, FontStyle.Bold)
                };
                var subtitle = new Label
                {
                    Dock = DockStyle.Bottom,
                    Height = 21,
                    ForeColor = muted,
                    Font = new Font("Segoe UI", 10.5F)
                };
                header.Controls.Add(title); header.Controls.Add(subtitle);
                root.Controls.Add(header, 0, 0);

                var infoCards = new TableLayoutPanel
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

                var balanceCard = TaoTheThongTinDN("SỐ DƯ HIỆN TẠI", card, muted);
                var balance = new Label
                {
                    Left = 16,
                    Top = 42,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(74, 222, 128)
                };
                var state = new Label
                {
                    Left = 18,
                    Top = 99,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = white
                };
                balanceCard.Controls.Add(balance); balanceCard.Controls.Add(state);
                infoCards.Controls.Add(balanceCard, 0, 0);

                var accountCard = TaoTheThongTinDN("THÔNG TIN TÀI KHOẢN", card, muted);
                var accountInfo = new Label
                {
                    Left = 17,
                    Top = 44,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = white
                };
                var accountType = new Label { Left = 17, Top = 44, AutoSize = true, Font = accountInfo.Font, ForeColor = Color.FromArgb(147, 197, 253) };
                accountInfo.Top = 70;
                accountCard.Controls.Add(accountType);
                accountCard.Controls.Add(accountInfo); infoCards.Controls.Add(accountCard, 1, 0);

                var limitsCard = TaoTheThongTinDN("HẠN MỨC GIAO DỊCH", card, muted);
                var limits = new Label
                {
                    Left = 17,
                    Top = 45,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 13F),
                    ForeColor = white
                };
                limitsCard.Controls.Add(limits); infoCards.Controls.Add(limitsCard, 2, 0);

                root.Controls.Add(new Label
                {
                    Text = "CHỦ SỞ HỮU VÀ NGƯỜI ĐƯỢC ỦY QUYỀN DOANH NGHIỆP",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = white,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold)
                }, 0, 2);

                var owners = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 0, 0, 2),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                    BackgroundColor = card,
                    BorderStyle = BorderStyle.None,
                    EnableHeadersVisualStyles = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    ScrollBars = ScrollBars.None,
                    Font = new Font("Segoe UI", 12F),
                    GridColor = border
                };
                owners.Columns.Add("role", "Vai trò");
                owners.Columns.Add("name", "Tên doanh nghiệp / Họ tên");
                owners.Columns.Add("cccd", "MST / CCCD");
                owners.Columns.Add("phone", "Số điện thoại");
                owners.Columns.Add("status", "Trạng thái TK");
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
                // Cấu hình cả năm cột trước khi bật Fill, tránh tự phân bổ lại tỷ lệ.
                owners.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                owners.Columns["role"].MinimumWidth = 180;
                owners.Columns["name"].MinimumWidth = 220;
                owners.Columns["cccd"].MinimumWidth = 130;
                owners.Columns["phone"].MinimumWidth = 120;
                owners.Columns["status"].MinimumWidth = 105;
                owners.Columns["role"].FillWeight = 22F;
                owners.Columns["name"].FillWeight = 35F;
                owners.Columns["cccd"].FillWeight = 17F;
                owners.Columns["phone"].FillWeight = 14F;
                owners.Columns["status"].FillWeight = 12F;
                foreach (DataGridViewColumn column in owners.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    column.Resizable = DataGridViewTriState.False;
                }
                owners.ScrollBars = ScrollBars.Both;
                owners.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                root.Controls.Add(owners, 0, 3);

                root.Controls.Add(new Label
                {
                    Text = "CHỨC NĂNG TÀI KHOẢN",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = white,
                    Font = new Font("Segoe UI", 13F, FontStyle.Bold)
                }, 0, 4);

                var groups = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 1,
                    Margin = Padding.Empty
                };
                for (int i = 0; i < 4; i++) groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                groups.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.Controls.Add(groups, 0, 5);

                // Tải dữ liệu ngoài UI thread, chỉ dựng lại giao diện khi có kết quả.
                // Tuyệt đối không gắn truy vấn SQL vào Activated/Resize/Paint.
                bool dangTai = false;
                var qrNhan = new Button { Text = "QR nhận tiền", Dock = DockStyle.Left, Width = 180, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = white, Cursor = Cursors.Hand };
                var qrChuyen = new Button { Text = "Chuyển tiền qua QR", Dock = DockStyle.Left, Width = 210, Enabled = false, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = white, Cursor = Cursors.Hand };
                qrNhan.FlatAppearance.BorderSize = 0;
                qrChuyen.FlatAppearance.BorderSize = 0;
                qrNhan.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
                qrNhan.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);
                qrChuyen.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
                qrChuyen.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);
                Action refresh = null;
                refresh = async () =>
                {
                    if (dlg.IsDisposed || dlg.Disposing || dangTai) return;
                    dangTai = true;
                    qrNhan.Enabled = qrChuyen.Enabled = false;
                    subtitle.Text = "Đang tải thông tin tài khoản...";
                    try
                    {
                        var result = await Task.Run(() =>
                        {
                            DataTable chiTiet = DocChiTietDN(stk);
                            DataTable dongSoHuu = chiTiet.Rows.Count == 0
                                ? new DataTable()
                                : DocDongSoHuuDN(stk, Convert.ToString(chiTiet.Rows[0]["MaSoThue"]));
                            return Tuple.Create(chiTiet, dongSoHuu);
                        });
                        if (dlg.IsDisposed || dlg.Disposing) return;
                        DataTable fresh = result.Item1;
                        DataTable ds = result.Item2;
                        if (fresh.Rows.Count == 0)
                        {
                            subtitle.Text = "Không tìm thấy tài khoản doanh nghiệp trong SQL.";
                            return;
                        }
                        DataRow r = fresh.Rows[0];
                        int statusValue = Convert.ToInt32(r["TrangThai"]);
                        string statusName = TenTrangThaiDN(statusValue);
                        subtitle.Text = "Số tài khoản  " + stk + "     •     " + statusName;
                        balance.Text = Convert.ToDecimal(r["SoDu"]).ToString("N0") + " " + Convert.ToString(r["LoaiTienTe"]);
                        state.Text = "Trạng thái: " + statusName + "     |     " + Convert.ToString(r["LoaiTienTe"]);
                        state.ForeColor = MauTrangThaiDN(statusName);
                        subtitle.ForeColor = MauTrangThaiDN(statusName);
                        string ten = Convert.ToString(r["TenHienThi"]);
                        accountType.Text = "Loại TK: " + Convert.ToString(r["LoaiTaiKhoan"]);
                        accountInfo.Text =
                            "Ngày mở: " + Convert.ToDateTime(r["NgayMo"]).ToString("dd/MM/yyyy HH:mm:ss") +
                            (string.IsNullOrWhiteSpace(ten) ? "" : "\nTên hiển thị: " + ten);
                        limits.Text = "Theo ngày: " + Convert.ToDecimal(r["HanMucNgay"]).ToString("N0") + " VNĐ\n" +
                            "Mỗi lần: " + Convert.ToDecimal(r["HanMucLan"]).ToString("N0") + " VNĐ";
                        owners.Rows.Clear();
                        int idx = owners.Rows.Add("Chủ tài khoản", r["TenDoanhNghiep"], r["MaSoThue"], r["SoDienThoai"], statusName);
                        owners.Rows[idx].Cells[0].Style.ForeColor = Color.FromArgb(37, 99, 235);
                        owners.Rows[idx].Cells["status"].Style.ForeColor = MauTrangThaiDN(statusName);
                        owners.Rows[idx].Cells["status"].Style.SelectionForeColor = MauTrangThaiDN(statusName);
                        owners.Rows[idx].Cells[0].Style.SelectionForeColor = owners.Rows[idx].Cells[0].Style.ForeColor;
                        foreach (DataRow o in ds.Rows)
                        {
                            int j = owners.Rows.Add("Người được ủy quyền", o["HoTen"], o["CCCD"], o["SoDienThoai"], statusName);
                            owners.Rows[j].Cells[0].Style.ForeColor = Color.FromArgb(168, 85, 247);
                            owners.Rows[j].Cells["status"].Style.ForeColor = MauTrangThaiDN(statusName);
                            owners.Rows[j].Cells["status"].Style.SelectionForeColor = MauTrangThaiDN(statusName);
                            owners.Rows[j].Cells[0].Style.SelectionForeColor = owners.Rows[j].Cells[0].Style.ForeColor;
                        }
                        // Bật/tắt lại các nút theo trạng thái tài khoản mới nhất.
                        groups.SuspendLayout();
                        // Tránh rò rỉ control khi làm mới nhiều lần.
                        while (groups.Controls.Count > 0)
                        {
                            Control old = groups.Controls[0];
                            groups.Controls.RemoveAt(0);
                            old.Dispose();
                        }
                        bool active = statusValue == 0;
                        qrNhan.Enabled = qrChuyen.Enabled = active && string.Equals(Convert.ToString(r["LoaiTienTe"]), "VND", StringComparison.OrdinalIgnoreCase);
                        bool notClosed = statusValue != 2;
                        ThemNhomGiongCaNhan(groups, 0, "Giao dịch", new[] {
                        new Tuple<string, EventHandler, bool>("Nạp tiền", NapTien, active),
                        new Tuple<string, EventHandler, bool>("Rút tiền", RutTien, active),
                        new Tuple<string, EventHandler, bool>("Chuyển tiền", ChuyenKhoan, active)
                    }, stk, refresh);
                        ThemNhomGiongCaNhan(groups, 1, "Trạng thái tài khoản", new[] {
                        new Tuple<string, EventHandler, bool>(statusValue == 1 ? "Mở khóa" : "Tạm khóa", KhoaMo, notClosed),
                        new Tuple<string, EventHandler, bool>("Đóng tài khoản", DongTaiKhoan, notClosed)
                    }, stk, refresh);
                        ThemNhomGiongCaNhan(groups, 2, "Quản lý tài khoản", new[] {
                        new Tuple<string, EventHandler, bool>("Thay đổi thông tin", (s, e) => SuaThongTinTheoDongSoHuu(owners, s, e), active),
                        new Tuple<string, EventHandler, bool>("Đăng ký hạn mức", DangKyHanMuc, active),
                        new Tuple<string, EventHandler, bool>("Đăng ký dịch vụ", DangKyDichVu, active)
                    }, stk, refresh);
                        ThemNhomGiongCaNhan(groups, 3, "Lịch sử & báo cáo", new[] {
                        new Tuple<string, EventHandler, bool>("Xem lịch sử giao dịch", (s, e) => MoLichSuTuChiTiet(stk, true), true),
                        new Tuple<string, EventHandler, bool>("Xem lịch sử hoạt động", (s, e) => MoLichSuTuChiTiet(stk, false), true)
                    }, stk, refresh);
                        groups.ResumeLayout(true);
                    }
                    catch (Exception ex)
                    {
                        if (!dlg.IsDisposed && !dlg.Disposing)
                        {
                            subtitle.Text = "Không tải được dữ liệu chi tiết.";
                            ThongBaoDN(dlg, "Không thể tải thông tin doanh nghiệp từ SQL:\n" + ex.Message,
                                "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    finally { dangTai = false; }
                };

                var footer = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
                var close = new Button
                {
                    Text = "Đóng cửa sổ",
                    Dock = DockStyle.Right,
                    Width = 160,
                    Margin = new Padding(0, 6, 0, 4),
                    BackColor = Color.FromArgb(42, 58, 82),
                    ForeColor = white,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                close.FlatAppearance.BorderSize = 0;
                close.Click += (s, e) => dlg.Close();
                qrNhan.Click += (s, e) => HienThiQRNhanTienDN(dlg, stk);
                qrChuyen.Click += (s, e) => { ChuyenTienQuaQRDN(dlg, stk); refresh(); };
                footer.Controls.Add(qrChuyen);
                footer.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 10 });
                footer.Controls.Add(qrNhan);
                footer.Controls.Add(close);
                root.Controls.Add(footer, 0, 6);
                // Chỉ tải lần đầu khi cửa sổ đã được vẽ; tránh lặp mỗi lần lấy focus.
                dlg.Shown += (s, e) => refresh();
                dlg.ShowDialog(this);
            }
        }

        // Mở danh mục lịch sử chính, chọn sẵn chế độ và lọc theo MST doanh nghiệp.
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
                ThongBaoDN("Không tìm thấy FrmMain để mở danh mục Lịch sử.",
                    "Điều hướng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            main.MoLichSuTuThanhToan(this, true, stk, giaoDich, cs);
        }

        private static string TenTrangThaiDN(int trangThai)
        {
            return trangThai == 0 ? "Hoạt động" : trangThai == 1 ? "Đang khóa" : "Đã đóng";
        }

        private DataTable DocChiTietDN(string stk)
        {
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"SELECT t.SoTKTT, t.CCCD AS MaSoThue,
                k.HoTen AS TenDoanhNghiep, k.SoDienThoai, t.SoDu, t.TrangThai,
                t.NgayMo, t.LoaiTienTe, t.LoaiTaiKhoan, t.HanMucNgay,
                t.HanMucLan, t.TenHienThi
                FROM dbo.DN_TaiKhoanThanhToan t
                INNER JOIN dbo.DN_KhachHang k ON k.CCCD = t.CCCD
                WHERE t.SoTKTT = @stk", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                var dt = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(dt);
                return dt;
            }
        }

        private DataTable DocDongSoHuuDN(string stk, string mst)
        {
            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"SELECT CCCD, HoTen, SoDienThoai
                FROM dbo.DN_DongSoHuuTaiKhoanThanhToan
                WHERE SoTKTT=@stk ORDER BY HoTen", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                var dt = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(dt);
                return dt;
            }
        }

        private Panel TaoTheThongTinDN(string title, Color backColor, Color labelColor)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backColor,
                Margin = new Padding(0, 0, 12, 0)
            };
            panel.Controls.Add(new Label
            {
                Text = title,
                Location = new Point(17, 12),
                AutoSize = true,
                ForeColor = labelColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            });
            return panel;
        }

        private void ThemNhomGiongCaNhan(TableLayoutPanel parent, int column, string title,
            Tuple<string, EventHandler, bool>[] items, string stk, Action refresh)
        {
            Color card = Color.FromArgb(25, 36, 57);
            Color white = Color.FromArgb(241, 245, 249);
            var group = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = card,
                Margin = new Padding(column == 0 ? 0 : 5, 0, column == 3 ? 0 : 5, 0)
            };
            group.Controls.Add(new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(147, 197, 253),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            });
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var button = new Button
                {
                    Text = item.Item1 + (item.Item3 ? "" : " (Không khả dụng)"),
                    BackColor = Color.FromArgb(40, 56, 80),
                    ForeColor = item.Item3 ? white : Color.FromArgb(248, 113, 113),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F),
                    Enabled = item.Item3,
                    Height = items.Length > 3 ? 32 : 39,
                    Left = 10,
                    Top = 39 + i * (items.Length > 3 ? 37 : 47),
                    Cursor = item.Item3 ? Cursors.Hand : Cursors.Default
                };
                button.FlatAppearance.BorderSize = 0;
                button.Click += (s, e) =>
                {
                    // Cửa sổ con luôn làm việc với đúng STK khi mở, dù bảng chính đổi dòng.
                    string prev = _stkChiTietDangMo;
                    _stkChiTietDangMo = stk;
                    try { item.Item2(s, e); }
                    finally { _stkChiTietDangMo = prev; }
                    if (!item.Item1.StartsWith("Xem lịch sử", StringComparison.Ordinal) && !item.Item1.StartsWith("Xuất", StringComparison.Ordinal)) refresh();
                };
                group.Controls.Add(button);
                Button current = button;
                group.Resize += (s, e) => current.Width = Math.Max(80, group.ClientSize.Width - 20);
            }
            parent.Controls.Add(group, column, 0);
        }

        // Chọn đúng người trên bảng sở hữu của cửa sổ chi tiết.
        private void SuaThongTinTheoDongSoHuu(DataGridView owners, object sender, EventArgs e)
        {
            var row = owners.CurrentRow;
            if (row == null || row.IsNewRow || !row.Selected)
            {
                ThongBaoDN("Vui lòng chọn chủ sở hữu hoặc người được ủy quyền cần sửa.");
                return;
            }
            string role = Convert.ToString(row.Cells["role"].Value);
            if (role == "Chủ tài khoản")
            {
                SuaThongTin(sender, e); // Giữ nguyên nghiệp vụ sửa tên hiển thị tài khoản.
                return;
            }
            if (role != "Người được ủy quyền") return;
            string stk = SoTK;
            string cccd = Convert.ToString(row.Cells["cccd"].Value).Trim();
            if (string.IsNullOrWhiteSpace(stk) || !MaSoHopLe(cccd, 12, 12)) return;
            using (var dlg = new Form
            {
                Text = "Sửa thông tin người được ủy quyền",
                ClientSize = new Size(510, 245),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            })
            {
                var id = new TextBox { Left = 150, Top = 20, Width = 335, ReadOnly = true, Text = cccd };
                var ten = new TextBox { Left = 150, Top = 66, Width = 335, MaxLength = 100, Text = Convert.ToString(row.Cells["name"].Value) };
                var sdt = new TextBox { Left = 150, Top = 112, Width = 335, MaxLength = 11, Text = Convert.ToString(row.Cells["phone"].Value) };
                var ok = new Button { Text = "Lưu thay đổi", Left = 225, Top = 175, Width = 130, Height = 38, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var cancel = new Button { Text = "Hủy", Left = 365, Top = 175, Width = 120, Height = 38, DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(51, 65, 85), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                dlg.Controls.AddRange(new Control[] {
                    new Label { Text = "CCCD:", Left = 20, Top = 23, AutoSize = true }, id,
                    new Label { Text = "Họ và tên:", Left = 20, Top = 69, AutoSize = true }, ten,
                    new Label { Text = "Số điện thoại:", Left = 20, Top = 115, AutoSize = true }, sdt, ok, cancel });
                dlg.AcceptButton = ok; dlg.CancelButton = cancel;
                ok.Click += (senderLuu, argsLuu) =>
                {
                    string n = ten.Text.Trim(), p = sdt.Text.Trim();
                    if (n.Length == 0 || !MaSoHopLe(p, 10, 11))
                    {
                        ThongBaoDN(dlg, "Vui lòng nhập họ tên và số điện thoại hợp lệ."); return;
                    }
                    try
                    {
                        if (string.IsNullOrWhiteSpace(NguoiThucHien)) throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập.");
                        using (var cn = new SqlConnection(cs))
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                using (var cmd = new SqlCommand(@"IF NOT EXISTS (SELECT 1 FROM dbo.DN_TaiKhoanThanhToan t WITH(UPDLOCK,HOLDLOCK)
                                    INNER JOIN dbo.DN_DongSoHuuTaiKhoanThanhToan d ON d.SoTKTT=t.SoTKTT
                                    WHERE t.SoTKTT=@stk AND t.TrangThai=0 AND d.CCCD=@c)
                                    THROW 52010,N'Người được ủy quyền không còn thuộc tài khoản hoặc tài khoản không hoạt động.',1;
                                    UPDATE dbo.DN_DongSoHuu SET HoTen=@n,SoDienThoai=@p WHERE CCCD=@c;
                                    UPDATE dbo.DN_DongSoHuuTaiKhoanThanhToan SET HoTen=@n,SoDienThoai=@p WHERE CCCD=@c;
                                    UPDATE dbo.DN_DongSoHuuTaiKhoanVay SET HoTen=@n,SoDienThoai=@p WHERE CCCD=@c;
                                    UPDATE dbo.DN_DongSoHuuSoTietKiem SET HoTen=@n,SoDienThoai=@p WHERE CCCD=@c;
                                    UPDATE dbo.DN_TaiKhoanThanhToan SET NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk;", cn, tx))
                                {
                                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                    cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = cccd;
                                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 100).Value = n;
                                    cmd.Parameters.Add("@p", SqlDbType.VarChar, 11).Value = p;
                                    cmd.ExecuteNonQuery();
                                }
                                Ghi(cn, tx, stk, "Sửa thông tin người được ủy quyền: " + cccd, 0, LaySoDu(cn, tx, stk));
                                tx.Commit();
                            }
                        }
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex) { ThongBaoDN(dlg, ex.Message, "Không thể sửa thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                };
                DongBoMauHopThoaiDN(dlg);
                if (dlg.ShowDialog(this) == DialogResult.OK) Changed();
            }
        }

        private void SuaThongTin(object sender, EventArgs e)
        {
            string stk = SoTK; if (stk == "") return;
            try
            {
                var dt = DocChiTietDN(stk); if (dt.Rows.Count == 0) return;
                string ten = Prompt("Thông tin tài khoản", "Tên hiển thị mới:", Convert.ToString(dt.Rows[0]["TenHienThi"]));
                if (ten == null) return;
                if (string.IsNullOrWhiteSpace(ten) || ten.Length > 100) throw new InvalidOperationException("Tên hiển thị cần từ 1 đến 100 ký tự.");
                Run(() => {
                    using (var cn = new SqlConnection(cs))
                    {
                        cn.Open(); using (var tx = cn.BeginTransaction())
                        {
                            using (var c = new SqlCommand("UPDATE dbo.DN_TaiKhoanThanhToan SET TenHienThi=@name,NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                            {
                                c.Parameters.AddWithValue("@name", ten); c.Parameters.AddWithValue("@stk", stk); if (c.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không thể sửa tài khoản.");
                            }
                            decimal bal = LaySoDu(cn, tx, stk); Ghi(cn, tx, stk, "Cập nhật tên hiển thị: " + ten, 0, bal); tx.Commit();
                        }
                    }
                });
            }
            catch (Exception ex) { ThongBaoDN(ex.Message); }
        }
        private decimal LaySoDu(SqlConnection cn, SqlTransaction tx, string stk)
        {
            using (var c = new SqlCommand("SELECT SoDu FROM dbo.DN_TaiKhoanThanhToan WHERE SoTKTT=@stk", cn, tx)) { c.Parameters.AddWithValue("@stk", stk); return Convert.ToDecimal(c.ExecuteScalar()); }
        }
        private void DangKyHanMuc(object sender, EventArgs e)
        {
            string stk = SoTK; if (stk == "") return;
            var dt = DocChiTietDN(stk); if (dt.Rows.Count == 0) return;
            string ngay = Prompt("Đăng ký hạn mức", "Hạn mức mỗi ngày (VND):", Convert.ToString(dt.Rows[0]["HanMucNgay"])); if (ngay == null) return;
            string lan = Prompt("Đăng ký hạn mức", "Hạn mức mỗi lần (VND):", Convert.ToString(dt.Rows[0]["HanMucLan"])); if (lan == null) return;
            Run(() => {
                decimal n = Amount(ngay), l = Amount(lan); if (l > n) throw new InvalidOperationException("Hạn mức mỗi lần không được vượt hạn mức ngày.");
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction())
                    {
                        using (var c = new SqlCommand("UPDATE dbo.DN_TaiKhoanThanhToan SET HanMucNgay=@n,HanMucLan=@l,NgayCapNhatTT=GETDATE() WHERE SoTKTT=@stk AND TrangThai=0", cn, tx))
                        {
                            c.Parameters.AddWithValue("@n", n); c.Parameters.AddWithValue("@l", l); c.Parameters.AddWithValue("@stk", stk); if (c.ExecuteNonQuery() != 1) throw new InvalidOperationException("Tài khoản không hoạt động.");
                        }
                        Ghi(cn, tx, stk, "Đăng ký hạn mức: ngày=" + n.ToString("N0") + ", lần=" + l.ToString("N0"), 0, LaySoDu(cn, tx, stk)); tx.Commit();
                    }
                }
            });
        }
        private void DangKyDichVu(object sender, EventArgs e)
        {
            string stk = SoTK; if (stk == "") return;
            string ten = Prompt("Đăng ký dịch vụ", "Tên dịch vụ (SMS Banking, Mobile Banking, ...):"); if (ten == null) return;
            if (string.IsNullOrWhiteSpace(ten) || ten.Length > 100) { ThongBaoDN("Tên dịch vụ không hợp lệ."); return; }
            Run(() => {
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open(); using (var tx = cn.BeginTransaction())
                    {
                        using (var c = new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM dbo.DN_TaiKhoanThanhToan WITH(UPDLOCK,HOLDLOCK) WHERE SoTKTT=@stk AND TrangThai=0) THROW 52005,N'Tài khoản không hoạt động.',1;
IF EXISTS(SELECT 1 FROM dbo.DN_DichVuDangKy WITH(UPDLOCK,HOLDLOCK) WHERE SoTKTT=@stk AND TenDichVu=@ten)
 UPDATE dbo.DN_DichVuDangKy SET TrangThai=N'Đang hoạt động' WHERE SoTKTT=@stk AND TenDichVu=@ten
ELSE INSERT dbo.DN_DichVuDangKy(SoTKTT,TenDichVu,TrangThai,NgayDangKy) VALUES(@stk,@ten,N'Đang hoạt động',GETDATE())", cn, tx))
                        {
                            c.Parameters.AddWithValue("@stk", stk); c.Parameters.AddWithValue("@ten", ten); c.ExecuteNonQuery();
                        }
                        Ghi(cn, tx, stk, "Đăng ký dịch vụ: " + ten, 0, LaySoDu(cn, tx, stk)); tx.Commit();
                    }
                }
            });
        }


        // Chỉ đổi màu và cách trình bày; không thay đổi sự kiện hay dữ liệu.
        private static void DongBoMauHopThoaiDN(Form f)
        {
            f.BackColor = Color.FromArgb(15, 23, 42);
            f.ForeColor = Color.FromArgb(241, 245, 249);
            ToMauControlDN(f);
        }

        private static void ToMauControlDN(Control parent)
        {
            Color card = Color.FromArgb(30, 41, 59);
            Color input = Color.FromArgb(39, 53, 75);
            Color ink = Color.FromArgb(241, 245, 249);
            foreach (Control c in parent.Controls)
            {
                if (c is Button b)
                {
                    string t = (b.Text ?? "").ToLowerInvariant();
                    bool neutral = b.DialogResult == DialogResult.Cancel || t.Contains("hủy") || t == "không" || t == "đóng";
                    bool danger = t.Contains("xóa") || t.Contains("bỏ dòng") || t.Contains("tạm khóa") || t.Contains("đóng tài khoản");
                    bool success = t.Contains("xác nhận") || t.Contains("lưu thay đổi") || t.Contains("mở khóa");
                    b.UseVisualStyleBackColor = false;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    b.BackColor = neutral ? Color.FromArgb(71, 85, 105) : danger ? Color.FromArgb(185, 28, 28) : success ? Color.FromArgb(21, 128, 61) : Color.FromArgb(37, 99, 235);
                    b.ForeColor = Color.White;
                }
                else if (c is TextBox t)
                {
                    t.BackColor = t.ReadOnly ? card : input;
                    t.ForeColor = t.ReadOnly ? Color.FromArgb(147, 197, 253) : ink;
                    t.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                { cb.BackColor = input; cb.ForeColor = ink; cb.FlatStyle = FlatStyle.Flat; }
                else if (c is NumericUpDown)
                { c.BackColor = input; c.ForeColor = ink; }
                else if (c is DateTimePicker dp)
                {
                    dp.CalendarMonthBackground = card;
                    dp.CalendarForeColor = ink;
                    dp.CalendarTitleBackColor = Color.FromArgb(37, 99, 235);
                    dp.CalendarTitleForeColor = Color.White;
                }
                else if (c is DataGridView grid)
                {
                    grid.BackgroundColor = card;
                    grid.GridColor = Color.FromArgb(51, 65, 85);
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = card;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.DefaultCellStyle.BackColor = input;
                    grid.DefaultCellStyle.ForeColor = ink;
                    grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
                    grid.DefaultCellStyle.SelectionForeColor = Color.White;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(23, 33, 60);
                    grid.AlternatingRowsDefaultCellStyle.ForeColor = ink;
                    continue;
                }
                else if (c is Panel || c is GroupBox)
                { c.BackColor = card; c.ForeColor = ink; }
                else if (c is Label)
                { if (c.ForeColor.ToArgb() != Color.LightGreen.ToArgb()) c.ForeColor = ink; }
                else if (c is RadioButton || c is CheckBox)
                { c.BackColor = parent.BackColor; c.ForeColor = ink; }
                if (c.HasChildren && !(c is NumericUpDown)) ToMauControlDN(c);
            }
        }

        // Giữ nguyên kết quả Yes/No và OK của các thông báo cũ.
        private static DialogResult ThongBaoDN(string text, string caption = "Thông báo",
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return ThongBaoDN(Form.ActiveForm, text, caption, buttons, icon);
        }

        private static DialogResult ThongBaoDN(IWin32Window owner, string text, string caption = "Thông báo",
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            // Chỉ thay các kiểu thông báo đang được sử dụng trong form này.
            if (buttons != MessageBoxButtons.OK && buttons != MessageBoxButtons.YesNo)
                return MessageBox.Show(owner, text, caption, buttons, icon);
            using (var f = new Form())
            {
                f.Text = caption;
                f.ClientSize = new Size(520, 240);
                f.StartPosition = owner == null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false; f.MinimizeBox = false; f.ShowInTaskbar = false;
                f.Font = new Font("Segoe UI", 10F);
                var title = new Label { Text = caption, Dock = DockStyle.Top, Height = 40, Padding = new Padding(16, 10, 16, 0) };
                var message = new TextBox { Text = text, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, TabStop = false };
                var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8) };
                var ok = new Button { Text = buttons == MessageBoxButtons.YesNo ? "Có" : "OK", Size = new Size(100, 30), DialogResult = buttons == MessageBoxButtons.YesNo ? DialogResult.Yes : DialogResult.OK };
                if (buttons == MessageBoxButtons.YesNo)
                {
                    var no = new Button { Text = "Không", Size = new Size(100, 30), DialogResult = DialogResult.No };
                    footer.Controls.Add(no); f.CancelButton = no;
                    f.ControlBox = false;
                }
                else f.CancelButton = ok;
                footer.Controls.Add(ok); f.AcceptButton = ok;
                f.Controls.Add(message); f.Controls.Add(footer); f.Controls.Add(title);
                DongBoMauHopThoaiDN(f);
                title.ForeColor = icon == MessageBoxIcon.Error ? Color.FromArgb(248, 113, 113) : icon == MessageBoxIcon.Warning ? Color.FromArgb(251, 191, 36) : Color.FromArgb(147, 197, 253);
                if (buttons == MessageBoxButtons.YesNo) f.ActiveControl = f.CancelButton as Control;
                return owner == null ? f.ShowDialog() : f.ShowDialog(owner);
            }
        }

        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
