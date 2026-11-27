using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace QLTKNH
{
    public partial class FrmTietKiem : Form
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

        private readonly Color colorBg = Color.FromArgb(15, 23, 42);
        private readonly Color colorDarkPanel = Color.FromArgb(30, 41, 59);
        private readonly Color colorBlue = Color.FromArgb(37, 99, 235);
        private readonly Color colorGreen = Color.FromArgb(34, 197, 94);
        private readonly Color colorRed = Color.FromArgb(239, 68, 68);
        private readonly Color colorOrange = Color.FromArgb(249, 115, 22);

        private IList danhSachKhachHang = new List<FrmMain.KhachHangData>();
        private readonly string _connStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        // Đồng sở hữu theo từng SỐ SỔ, không chia sẻ sang sổ khác của cùng một chủ.
        private sealed class DongSoHuuTheoSo
        {
            public string CCCD { get; set; }
            public string HoTen { get; set; }
            public string SoDienThoai { get; set; }
        }
        private readonly Dictionary<string, List<DongSoHuuTheoSo>> _dongSoHuuTheoSo =
            new Dictionary<string, List<DongSoHuuTheoSo>>(StringComparer.OrdinalIgnoreCase);

        // Chỉ bổ sung bảng mới. Khi tạo lần đầu, sao chép các liên kết cũ để giữ dữ liệu hiển thị.
        // Các sổ mới sẽ chỉ gắn đồng sở hữu theo SoTKTK.
        private bool _daDamBaoBangDongSoHuu;
        private void DamBaoBangDongSoHuuTheoSo()
        {
            if (_daDamBaoBangDongSoHuu) return;
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                    SET XACT_ABORT ON;
                    BEGIN TRANSACTION;
                    IF OBJECT_ID(N'dbo.DongSoHuuSoTietKiem', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.DongSoHuuSoTietKiem
                        (
                            SoTKTK VARCHAR(20) NOT NULL,
                            CCCD VARCHAR(12) NOT NULL,
                            HoTen NVARCHAR(100) NOT NULL,
                            SoDienThoai VARCHAR(11) NOT NULL,
                            CONSTRAINT PK_DongSoHuuSoTietKiem PRIMARY KEY (SoTKTK, CCCD),
                            CONSTRAINT FK_DongSoHuuSoTietKiem_SoTietKiem
                                FOREIGN KEY (SoTKTK) REFERENCES dbo.SoTietKiem(SoTKTK)
                        );
                        EXEC sys.sp_executesql N'
                            INSERT INTO dbo.DongSoHuuSoTietKiem (SoTKTK, CCCD, HoTen, SoDienThoai)
                            SELECT st.SoTKTK, d.CCCD, MAX(d.HoTen), MAX(d.SoDienThoai)
                            FROM dbo.SoTietKiem st
                            INNER JOIN dbo.DongSoHuu d ON d.CCCD_ChuSoHuu = st.CCCD
                            WHERE d.CCCD <> st.CCCD
                            GROUP BY st.SoTKTK, d.CCCD';
                    END
                    COMMIT TRANSACTION;", cn))
            {
                cn.Open();
                cmd.ExecuteNonQuery();
            }
            _daDamBaoBangDongSoHuu = true;
        }

        // Số sổ phải duy nhất trên cả Thanh toán, Tiết kiệm và Vay.
        private bool SoTaiKhoanDaTonTai(string stk)
        {
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                    SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.SoTietKiem WHERE SoTKTK=@stk)
                         OR EXISTS(SELECT 1 FROM dbo.TaiKhoanVay WHERE SoTKVay=@stk)
                         OR EXISTS(SELECT 1 FROM dbo.TaiKhoanThanhToan WHERE SoTKTT=@stk)
                    THEN 1 ELSE 0 END", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                cn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) != 0;
            }
        }


        private void ThucThi(string sql, params SqlParameter[] parameters)
        {
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                cn.Open();
                if (cmd.ExecuteNonQuery() <= 0) throw new InvalidOperationException("Không có dữ liệu nào được cập nhật trong SQL.");
            }
        }
        private SqlParameter P(string name, object value) => new SqlParameter(name, value ?? DBNull.Value);


        // Chỉ dùng khi ghi giao dịch mới: lưu đúng họ tên người đang đăng nhập tại thời điểm thao tác.
        private static string LayTenNguoiThucHien()
        {
            var nhanVien = BankSecurity.Current;
            if (nhanVien == null || string.IsNullOrWhiteSpace(nhanVien.HoTen))
                throw new InvalidOperationException("Không xác định được nhân viên đang đăng nhập. Vui lòng đăng nhập lại trước khi thực hiện giao dịch.");
            return nhanVien.HoTen.Trim();
        }

        private void GhiLichSu(LichSuGiaoDichTietKiem ls)
        {
            ThucThi(@"INSERT INTO dbo.LichSuGiaoDichTietKiem
                    (SoTKTK, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, TongTienNhan, GhiChu, NguoiThucHien)
                    VALUES (@stk,@ngay,@loai,@goc,@lai,@tong,@ghichu,@nguoi)",
                P("@stk", ls.SoTKTK), P("@ngay", ls.NgayGiaoDich), P("@loai", ls.LoaiGiaoDich),
                P("@goc", ls.SoTienGoc), P("@lai", ls.SoTienLai), P("@tong", ls.TongTienNhan), P("@ghichu", ls.GhiChu),
                P("@nguoi", LayTenNguoiThucHien()));
            // Chi nhung giao dich THUC NHAN moi duoc cong vao tong da nhan.
            // Du lieu goc tai SoTienDaRut duoc luu rieng, khong bao gio cong tien lai.
            if (ls.TongTienNhan > 0m &&
                (ls.LoaiGiaoDich.StartsWith("Rút", StringComparison.OrdinalIgnoreCase) ||
                 ls.LoaiGiaoDich == "Tất toán đúng hạn" || ls.LoaiGiaoDich == "Tất toán"))
            {
                ThucThi(@"UPDATE dbo.SoTietKiem
                           SET TongTienDaNhan = (
                               SELECT ISNULL(SUM(h.TongTienNhan),0)
                               FROM dbo.LichSuGiaoDichTietKiem h
                               WHERE h.SoTKTK = @stk AND h.TongTienNhan > 0
                                 AND (h.LoaiGiaoDich LIKE N'Rút%' OR
                                      h.LoaiGiaoDich IN (N'Tất toán đúng hạn',N'Tất toán'))
                           )
                           WHERE SoTKTK = @stk", P("@stk", ls.SoTKTK));
            }
            dsLichSu.Add(ls);
            // Các giao dịch đã có lời mời in riêng giữ nguyên; bổ sung cho
            // giao dịch trạng thái/quyền sở hữu vốn chưa có hóa đơn.
            if (ls.LoaiGiaoDich != "Rút 1 phần" &&
                ls.LoaiGiaoDich != "Rút toàn bộ" &&
                ls.LoaiGiaoDich != "Tất toán")
            {
                try { MoiXuatHoaDonGiaoDichTietKiem(ls, LayKhachHangChoHoaDon(ls.SoTKTK)); }
                catch (Exception ex)
                {
                    MessageBox.Show("Giao dịch đã lưu, nhưng không thể tạo hóa đơn: " + ex.Message,
                        "Hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        // Lưu tiền gốc, lịch sử và tổng đã nhận trong cùng một transaction.
        private bool LuuGiaoDichRutTietKiem(LichSuGiaoDichTietKiem ls, decimal gocTruoc, object khObj)
        {
            try
            {
                string nguoi = LayTenNguoiThucHien();
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (var cmd = new SqlCommand(@"UPDATE dbo.SoTietKiem
                            SET SoTienGui=SoTienGui-@goc, SoTienDaRut=SoTienDaRut+@goc
                            WHERE SoTKTK=@stk AND TrangThai=0 AND SoTienGui=@truoc
                              AND CCCD=@chu AND SoTienGui>=@goc AND @goc>0", cn, tx))
                        {
                            cmd.Parameters.AddRange(new[] { P("@stk", ls.SoTKTK), P("@goc", ls.SoTienGoc),
                                P("@truoc", gocTruoc), P("@chu", LayString(khObj, "CCCD")) });
                            if (cmd.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("Số dư, chủ sở hữu hoặc trạng thái sổ đã thay đổi. Hãy đóng cửa sổ và tải lại dữ liệu.");
                        }
                        using (var cmd = new SqlCommand(@"INSERT dbo.LichSuGiaoDichTietKiem
                            (SoTKTK,NgayGiaoDich,LoaiGiaoDich,SoTienGoc,SoTienLai,TongTienNhan,GhiChu,NguoiThucHien)
                            VALUES(@stk,@ngay,@loai,@goc,@lai,@tong,@ghichu,@nguoi)", cn, tx))
                        {
                            cmd.Parameters.AddRange(new[] { P("@stk",ls.SoTKTK), P("@ngay",ls.NgayGiaoDich),
                                P("@loai",ls.LoaiGiaoDich), P("@goc",ls.SoTienGoc), P("@lai",ls.SoTienLai),
                                P("@tong",ls.TongTienNhan), P("@ghichu",ls.GhiChu), P("@nguoi",nguoi) });
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand(@"UPDATE dbo.SoTietKiem SET TongTienDaNhan=(
                            SELECT ISNULL(SUM(h.TongTienNhan),0) FROM dbo.LichSuGiaoDichTietKiem h
                            WHERE h.SoTKTK=@stk AND h.TongTienNhan>0 AND
                            (h.LoaiGiaoDich LIKE N'Rút%' OR h.LoaiGiaoDich IN(N'Tất toán đúng hạn',N'Tất toán')))
                            WHERE SoTKTK=@stk", cn, tx))
                        {
                            cmd.Parameters.Add(P("@stk", ls.SoTKTK));
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu giao dịch tiết kiệm: " + ex.Message,
                    "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            dsLichSu.Add(ls);
            // Chỉ mời xuất hóa đơn một lần, sau khi SQL đã commit.
            try { MoiXuatHoaDonGiaoDichTietKiem(ls, khObj); }
            catch (Exception ex)
            {
                MessageBox.Show("Giao dịch đã lưu, nhưng không thể tạo hóa đơn: " + ex.Message,
                    "Hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        private void TaiLai()
        {
            LayDanhSachKhachHangTuFrmMain();
            HienThiDanhSach(txtCCCD.Text.Trim());
            OnDataChanged();
        }

        private string cccdMoTuFrmMain;

        private class KyHanLaiSuat
        {
            public int Thang { get; set; }
            public decimal LaiSuat { get; set; }
            public override string ToString() => $"Kỳ hạn {Thang} tháng : {LaiSuat:0.##}%/năm";
        }

        public event EventHandler DataChanged;

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public class LichSuGiaoDichTietKiem
        {
            public string SoTKTK { get; set; }
            public DateTime NgayGiaoDich { get; set; }
            public string LoaiGiaoDich { get; set; }
            public decimal SoTienGoc { get; set; }
            public decimal SoTienLai { get; set; }
            public decimal TongTienNhan { get; set; }
            public string GhiChu { get; set; }
        }

        public static List<LichSuGiaoDichTietKiem> dsLichSu = new List<LichSuGiaoDichTietKiem>();

        // Cac chi tieu duoc doc tu SQL theo tung so so, khong tinh lap theo dong dong so huu.
        private sealed class TongHopTienSo
        {
            public decimal GocBanDau;
            public decimal GocConLai;
            public decimal TongDaNhan;
        }
        private readonly Dictionary<string, TongHopTienSo> _tongHopTienSo =
            new Dictionary<string, TongHopTienSo>(StringComparer.OrdinalIgnoreCase);

        private TongHopTienSo LayTongHopTienSo(string stk, decimal gocConLai)
        {
            TongHopTienSo kq;
            if (_tongHopTienSo.TryGetValue(stk ?? "", out kq)) return kq;
            // Truong hop so chua duoc nap lai: so goc hien tai la so du goc, khong phai tong nhan.
            return new TongHopTienSo { GocBanDau = gocConLai, GocConLai = gocConLai, TongDaNhan = 0m };
        }


        // Chỉ chuyển lệnh mở sổ sang menu chuột phải; không tạo toolbar/nút mới.
        private bool _dangMoTrungTamThaoTac;

        public FrmTietKiem() : this(null, "") { }

        public FrmTietKiem(FrmMain frmMain, string cccd) : this(cccd)
        {
            if (frmMain != null) this.Owner = frmMain;
        }

        public FrmTietKiem(string cccd)
        {
            cccdMoTuFrmMain = cccd ?? "";
            InitializeComponent();
            // Dùng chung font cho các dòng và giảm nhấp nháy khi vẽ bảng.
            _fontVaiTro = new Font("Segoe UI", 9F, FontStyle.Bold);
            GanPhanCapCaNhan(dgvTietKiem, 3, 11, _fontVaiTro);
            Disposed += (s, e) => _fontVaiTro.Dispose();
            var doubleBuffered = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (doubleBuffered != null) doubleBuffered.SetValue(dgvTietKiem, true, null);
            // Doi ten cot hien co, giu nguyen kich thuoc, thu tu va giao dien Designer.
            if (dgvTietKiem.Columns.Contains("colTongTienGui"))
                dgvTietKiem.Columns["colTongTienGui"].HeaderText = "Tổng gốc còn lại";
            if (dgvTietKiem.Columns.Contains("colSoTienNhan"))
                dgvTietKiem.Columns["colSoTienNhan"].HeaderText = "Tổng tiền đã nhận";

            KhoiTaoMenuMoSoChuotPhai(); // Sử dụng menu có sẵn trong Designer

            _timerLocTietKiem.Tick += (s, e) =>
            {
                _timerLocTietKiem.Stop();
                HienThiDanhSach(txtCCCD.Text.Trim());
            };
            Disposed += (s, e) => _timerLocTietKiem.Dispose();
            this.Load += FrmTietKiem_Load;
            this.Activated += (s, e) =>
            {
                if (!_daTaiLanDau || _dangTaiLanDau || _dangMoTrungTamThaoTac || _dangXemChiTietTietKiem) return;
                if ((DateTime.UtcNow - _lanTaiTietKiem).TotalSeconds < 2) return;
                LayDanhSachKhachHangTuFrmMain();
                HienThiDanhSach(txtCCCD.Text.Trim());
            };

            txtCCCD.TextChanged += TxtCCCD_TextChanged;
            txtCCCD.KeyPress += TxtCCCD_KeyPress;
            btnTimCCCD.Click += BtnTimCCCD_Click;

            // Nhap doi chuot trai vao dong de mo trung tam thao tac.
            dgvTietKiem.CellDoubleClick += DgvTietKiem_CellDoubleClick;
            dgvTietKiem.CellMouseDown += DgvTietKiem_CellMouseDown;
            // Nhấp một lần vào bất kỳ dòng nào sẽ cập nhật thanh thông tin phía trên.
            dgvTietKiem.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvTietKiem.Rows.Count &&
                    !dgvTietKiem.Rows[e.RowIndex].IsNewRow)
                    CapNhatThongTinKhachHangChon(e.RowIndex);
            };
            // Hỗ trợ chọn dòng bằng bàn phím hoặc thay đổi CurrentRow.
            dgvTietKiem.SelectionChanged += (s, e) =>
            {
                if (!_dangVeDanhSach && dgvTietKiem.CurrentRow != null && !dgvTietKiem.CurrentRow.IsNewRow)
                    CapNhatThongTinKhachHangChon(dgvTietKiem.CurrentRow.Index);
            };

            cboLocKyHan.SelectedIndexChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
            cboLocTrangThai.SelectedIndexChanged += (s, e) => HienThiDanhSach(txtCCCD.Text.Trim());
            cboLocThoiGian.SelectedIndexChanged += CboLocThoiGian_SelectedIndexChanged;

            dtpTuNgay.ValueChanged += (s, e) => { if (cboLocThoiGian.SelectedIndex == 7) HienThiDanhSach(txtCCCD.Text.Trim()); };
            dtpDenNgay.ValueChanged += (s, e) => { if (cboLocThoiGian.SelectedIndex == 7) HienThiDanhSach(txtCCCD.Text.Trim()); };
            DangKyResetVungTrongTietKiem(this);
        }

        // Chỉ nhận chuột trái trên nền phía trên bảng, không gắn vào ô nhập,
        // nút, bộ lọc hoặc DataGridView.
        private void DangKyResetVungTrongTietKiem(Control cha)
        {
            if (cha is Form || cha is Panel || cha is GroupBox || cha is UserControl ||
                cha is SplitContainer)
                cha.MouseClick += VungTrongTietKiem_MouseClick;

            foreach (Control con in cha.Controls)
            {
                if (con is DataGridView) continue;
                DangKyResetVungTrongTietKiem(con);
            }
        }

        private bool _dangResetVungTrongTietKiem;
        private void VungTrongTietKiem_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _dangResetVungTrongTietKiem ||
                _dangKhoiTaoBoLoc || _dangTaiLanDau || _dangVeDanhSach ||
                _dangXemChiTietTietKiem || _dangMoTrungTamThaoTac || IsDisposed || Disposing)
                return;

            Control nen = sender as Control;
            if (nen == null || !dgvTietKiem.Visible) return;
            // So sánh tọa độ màn hình để hỗ trợ cả các panel lồng nhau.
            if (nen.PointToScreen(e.Location).Y >= dgvTietKiem.PointToScreen(Point.Empty).Y)
                return;

            _dangResetVungTrongTietKiem = true;
            try
            {
                _timerLocTietKiem.Stop();
                _dangKhoiTaoBoLoc = true;
                try
                {
                    txtCCCD.Clear();
                    if (cboLocKyHan.Items.Count > 0) cboLocKyHan.SelectedIndex = 0;
                    if (cboLocTrangThai.Items.Count > 0) cboLocTrangThai.SelectedIndex = 0;
                    if (cboLocThoiGian.Items.Count > 0) cboLocThoiGian.SelectedIndex = 0;
                    dtpTuNgay.Value = DateTime.Today;
                    dtpDenNgay.Value = DateTime.Today;
                    lblTuNgay.Visible = dtpTuNgay.Visible = false;
                    lblDenNgay.Visible = dtpDenNgay.Visible = false;
                }
                finally { _dangKhoiTaoBoLoc = false; }

                // Dùng dữ liệu SQL đã nạp, chỉ dựng lại bảng một lần.
                // Không tạo giao dịch hay gọi DataChanged khi chỉ reset bộ lọc.
                HienThiDanhSach("");
                dgvTietKiem.CurrentCell = null;
                dgvTietKiem.ClearSelection();
                lblKhachHangChon.Text = "Chưa chọn | CCCD: - | SĐT: - | Gốc còn lại: 0 VNĐ | Đã nhận: 0 VNĐ";
            }
            finally { _dangResetVungTrongTietKiem = false; }
        }

        /// <summary>
        /// Chỉ hiện lệnh Mở sổ tiết kiệm khi nhấn chuột phải trên bảng.
        /// Tái sử dụng cmsTietKiem và tsmiMoSo có sẵn trong Designer.
        /// Không thay đổi kích thước, vị trí hoặc các chức năng khác.
        /// </summary>
        private void KhoiTaoMenuMoSoChuotPhai()
        {
            // Menu Designer có các lệnh cũ; bảng giờ chỉ hiển thị Mở sổ.
            // Những nghiệp vụ khác vẫn nằm trong cửa sổ chi tiết mở bằng nhấp đúp.
            cmsTietKiem.Items.Clear();
            cmsTietKiem.Items.Add(tsmiMoSo);
            tsmiMoSo.Text = "+ Mở sổ tiết kiệm";
            tsmiMoSo.Click += (s, e) => MoSoTietKiemMoi();
            dgvTietKiem.ContextMenuStrip = cmsTietKiem;
        }

        // Chuột phải chọn đúng dòng; ContextMenuStrip sẽ hiện lệnh Mở sổ.
        private void DgvTietKiem_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.Button != MouseButtons.Right) return;

            dgvTietKiem.ClearSelection();
            dgvTietKiem.Rows[e.RowIndex].Selected = true;
            dgvTietKiem.CurrentCell = dgvTietKiem.Rows[e.RowIndex].Cells[e.ColumnIndex];
            CapNhatThongTinKhachHangChon(e.RowIndex);
        }

        /// <summary>
        /// Mot cua so gom 4 nhom. Click thao tac goi dung ham nghiep vu cu.
        /// Khong doi SQL, khong doi form nghiep vu va khong sua Designer.
        /// </summary>
        private void MoTrungTamThaoTac()
        {
            if (_dangMoTrungTamThaoTac) return;
            DataGridViewRow row = dgvTietKiem.CurrentRow;
            if (row == null || row.IsNewRow) return;

            string stk = Convert.ToString(row.Cells["colSTK"].Value) ?? "";
            // Tra trạng thái cấp lại trực tiếp từ SQL trước, không chặn sổ cũ
            // chỉ vì nó đã đóng hoặc không còn nằm trong danh sách đối tượng đang hoạt động.
            bool daCapLai = LaSoDaDuocCapLai(stk);
            object tkObj = TimSoTietKiemTheoSTK(stk, out _);
            if (tkObj == null && !daCapLai)
            {
                MessageBox.Show("Không tìm thấy sổ tiết kiệm đang chọn.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sổ đã được cấp lại vẫn cho phép tra cứu lịch sử và xuất báo cáo/hóa đơn.
            // Các nghiệp vụ làm thay đổi sổ cũ sẽ hiển thị màu đỏ và bị vô hiệu hóa.
            string trangThai = daCapLai ? "Đã đóng" : LayTrangThai(tkObj);
            bool daDong = trangThai == "Đã đóng" || daCapLai;
            bool dangKhoa = trangThai == "Đang khóa";
            string hoTen = Convert.ToString(row.Cells["colHoTen"].Value) ?? "";
            string cccd = Convert.ToString(row.Cells["colCCCD"].Value) ?? "";
            string vaiTro = Convert.ToString(row.Cells["colVaiTro"].Value) ?? "";
            string thaoTacDuocChon = null;

            using (Form dlg = new Form())
            {
                dlg.Text = "TRUNG TÂM THAO TÁC - SỔ TIẾT KIỆM";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ClientSize = new Size(735, 485);
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9F);
                dlg.ShowInTaskbar = false;

                Label header = new Label
                {
                    Text = "QUẢN LÝ SỔ TIẾT KIỆM",
                    Location = new Point(23, 18),
                    Size = new Size(650, 28),
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = Color.White
                };
                Label subtitle = new Label
                {
                    Text = $"Sổ: {stk}   |   {hoTen} ({vaiTro})   |   CCCD: {cccd}",
                    Location = new Point(24, 55),
                    Size = new Size(680, 23),
                    ForeColor = Color.FromArgb(192, 202, 218),
                    AutoEllipsis = true
                };
                Label status = new Label
                {
                    Text = daCapLai ? "Trạng thái: Đã cấp lại - Sổ cũ không còn hiệu lực (chỉ tra cứu)" : "Trạng thái: " + trangThai,
                    Location = new Point(24, 81),
                    Size = new Size(650, 22),
                    ForeColor = daDong ? colorRed : dangKhoa ? colorOrange : colorGreen
                };

                Panel nav = new Panel
                {
                    Location = new Point(18, 119),
                    Size = new Size(190, 314),
                    BackColor = colorDarkPanel
                };
                Panel content = new Panel
                {
                    Location = new Point(220, 119),
                    Size = new Size(496, 314),
                    BackColor = colorDarkPanel
                };
                Label groupTitle = new Label
                {
                    Location = new Point(16, 15),
                    Size = new Size(455, 30),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };
                FlowLayoutPanel actions = new FlowLayoutPanel
                {
                    Location = new Point(13, 55),
                    Size = new Size(470, 240),
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    BackColor = colorDarkPanel
                };
                content.Controls.Add(groupTitle);
                content.Controls.Add(actions);

                // Mọi tên mã bên dưới chỉ dùng để điều hướng, không đổi nghiệp vụ cũ.
                string[] groupNames =
                {
                        "Giao dịch", "Trạng thái sổ", "Quản lý sổ", "Lịch sử & báo cáo"
                    };
                Button[] navButtons = new Button[groupNames.Length];
                Action<string, string, bool> addAction = (code, name, allowed) =>
                {
                    // Với sổ đã cấp lại, dùng Label thay cho Button cho các nghiệp vụ cấm.
                    // Label giữ được màu chữ đỏ rõ ràng (Button.Enabled=false thường vẽ chữ xám)
                    // và hoàn toàn không có sự kiện Click để thực hiện nghiệp vụ.
                    if (daCapLai && !allowed)
                    {
                        Label blocked = new Label
                        {
                            Text = "   " + name + "    (Không khả dụng)",
                            TextAlign = ContentAlignment.MiddleLeft,
                            Size = new Size(436, 48),
                            Margin = new Padding(4, 4, 4, 6),
                            BackColor = Color.FromArgb(23, 33, 54),
                            ForeColor = colorRed,
                            Font = new Font("Segoe UI", 10F),
                            BorderStyle = BorderStyle.FixedSingle,
                            Cursor = Cursors.No
                        };
                        actions.Controls.Add(blocked);
                        return;
                    }

                    Button item = new Button
                    {
                        Text = "   " + name + "    ›",
                        TextAlign = ContentAlignment.MiddleLeft,
                        Size = new Size(436, 48),
                        Margin = new Padding(4, 4, 4, 6),
                        BackColor = Color.FromArgb(23, 33, 54),
                        ForeColor = allowed ? Color.White : Color.FromArgb(133, 146, 165),
                        Font = new Font("Segoe UI", 10F),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = allowed ? Cursors.Hand : Cursors.Default,
                        Enabled = allowed
                    };
                    item.FlatAppearance.BorderColor = Color.FromArgb(58, 74, 103);
                    item.Click += (s, e) =>
                    {
                        thaoTacDuocChon = code;
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    };
                    actions.Controls.Add(item);
                };

                Action<int> showGroup = index =>
                {
                    groupTitle.Text = groupNames[index];
                    while (actions.Controls.Count > 0)
                    {
                        Control old = actions.Controls[0];
                        actions.Controls.RemoveAt(0);
                        old.Dispose();
                    }
                    for (int i = 0; i < navButtons.Length; i++)
                        navButtons[i].BackColor = i == index ? colorBlue : colorDarkPanel;
                    switch (index)
                    {
                        case 0:
                            addAction("TAT_TOAN", "Tất toán sổ", !daDong && !dangKhoa);
                            addAction("RUT_TIEN", "Rút tiền tiết kiệm", !daDong && !dangKhoa);
                            break;
                        case 1:
                            addAction("KHOA_MO", dangKhoa ? "Mở khóa tài khoản" : "Khóa tạm thời", !daDong);
                            addAction("DONG", "Đóng tài khoản", !daDong && !dangKhoa);
                            break;
                        case 2:
                            addAction("CHUYEN_GIAO", "Sang tên / Chuyển quyền", !daDong && !dangKhoa);
                            addAction("RUI_RO", "Xử lý rủi ro (Hủy/Cấp lại)", !daDong);
                            break;
                        case 3:
                            addAction("LICH_SU", "Xem lịch sử giao dịch", true);
                            addAction("LICH_SU_HD", "Xem lịch sử hoạt động", true);
                            break;
                    }
                };

                for (int i = 0; i < groupNames.Length; i++)
                {
                    int index = i;
                    Button button = new Button
                    {
                        Text = "   " + groupNames[i],
                        TextAlign = ContentAlignment.MiddleLeft,
                        Location = new Point(7, 9 + i * 61),
                        Size = new Size(176, 51),
                        FlatStyle = FlatStyle.Flat,
                        ForeColor = Color.White,
                        BackColor = colorDarkPanel,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    button.FlatAppearance.BorderSize = 0;
                    button.Click += (s, e) => showGroup(index);
                    navButtons[i] = button;
                    nav.Controls.Add(button);
                }

                Button close = new Button
                {
                    Text = "Đóng",
                    Location = new Point(611, 445),
                    Size = new Size(105, 29),
                    BackColor = colorDarkPanel,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };
                close.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
                dlg.CancelButton = close;
                dlg.Controls.AddRange(new Control[] { header, subtitle, status, nav, content, close });
                showGroup(0);
                _dangMoTrungTamThaoTac = true;
                try
                {
                    dlg.ShowDialog(this);
                }
                finally
                {
                    _dangMoTrungTamThaoTac = false;
                }
            }

            // Kiểm tra lần cuối ở SQL để tránh trường hợp sổ được cấp lại
            // bởi cửa sổ khác trong lúc trung tâm thao tác đang mở.
            if (!string.IsNullOrEmpty(thaoTacDuocChon) &&
                thaoTacDuocChon != "LICH_SU" &&
                thaoTacDuocChon != "LICH_SU_HD" && LaSoDaDuocCapLai(stk))
            {
                MessageBox.Show("Sổ cũ đã cấp lại chỉ được xem lịch sử và xuất báo cáo/hóa đơn.",
                    "Sổ không còn hiệu lực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gọi lại nguyên vẹn các hàm nghiệp vụ hiện có.
            // Thực hiện sau khi đóng dialog để tránh hai dialog lồng nhau.
            switch (thaoTacDuocChon)
            {
                case "TAT_TOAN": ThucHienTatToan(); break;
                case "RUT_TIEN": ThucHienRutTien(); break;
                case "KHOA_MO": ThucHienKhoaMoTaiKhoan(); break;
                case "DONG": ThucHienDongTaiKhoan(); break;
                case "CHUYEN_GIAO": ThucHienChuyenGiao(); break;
                case "RUI_RO": ThucHienXuLyRuiRo(); break;
                case "LICH_SU": MoLichSuTuChiTiet(stk, true); break;
                case "LICH_SU_HD": MoLichSuTuChiTiet(stk, false); break;
                case "BAO_CAO_TXT": ThucHienXuatBaoCaoTxt(); break;
                case "PDF": ThucHienXuatHoaDon(); break;
            }
        }

        // Hai nút trong nhóm Lịch sử & báo cáo mở đúng danh mục của FrmMain.
        // Không dùng ShowDialog lịch sử riêng; không thực hiện giao dịch SQL.
        private void MoLichSuTuChiTiet(string soSo, bool laGiaoDich)
        {
            if (string.IsNullOrWhiteSpace(soSo)) return;
            FrmMain main = Application.OpenForms.OfType<FrmMain>()
                .FirstOrDefault(f => !f.IsDisposed && !f.Disposing);
            if (main == null)
            {
                MessageBox.Show("Không tìm thấy giao diện FrmMain để mở danh mục Lịch sử.",
                    "Điều hướng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            main.MoLichSuTuTaiKhoan(this, false, "TietKiem", soSo, laGiaoDich, _connStr);
        }

        private readonly System.Windows.Forms.Timer _timerLocTietKiem = new System.Windows.Forms.Timer { Interval = 300 };
        private bool _daTaiLanDau, _dangTaiLanDau, _dangXemChiTietTietKiem;
        private DateTime _lanTaiTietKiem = DateTime.MinValue;

        private async void FrmTietKiem_Load(object sender, EventArgs e)
        {
            _dangKhoiTaoBoLoc = true;
            try
            {
                cboLocKyHan.SelectedIndex = 0;
                cboLocTrangThai.SelectedIndex = 0;
                cboLocThoiGian.SelectedIndex = 0;
                if (!string.IsNullOrWhiteSpace(cccdMoTuFrmMain)) txtCCCD.Text = cccdMoTuFrmMain;
            }
            finally { _dangKhoiTaoBoLoc = false; }
            if (_dangTaiLanDau || _daTaiLanDau) return;
            _dangTaiLanDau = true;
            bool enabledCu = Enabled;
            Enabled = false;
            try
            {
                var snapshot = await System.Threading.Tasks.Task.Run(() => DocDuLieuTietKiem());
                if (IsDisposed || Disposing) return;
                ApDungDuLieuTietKiem(snapshot);
                _daTaiLanDau = true;
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing) BaoLoiTaiTietKiem(ex);
            }
            finally
            {
                _dangTaiLanDau = false;
                if (!IsDisposed && !Disposing) Enabled = enabledCu;
            }
            if (!IsDisposed && !Disposing) HienThiDanhSach(txtCCCD.Text.Trim());
        }

        private sealed class DuLieuTietKiem
        {
            public readonly List<FrmMain.KhachHangData> KhachHang = new List<FrmMain.KhachHangData>();
            public readonly Dictionary<string, List<DongSoHuuTheoSo>> DongSoHuu = new Dictionary<string, List<DongSoHuuTheoSo>>(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, TongHopTienSo> TongHop = new Dictionary<string, TongHopTienSo>(StringComparer.OrdinalIgnoreCase);
            public readonly List<LichSuGiaoDichTietKiem> LichSu = new List<LichSuGiaoDichTietKiem>();
        }

        private void ApDungDuLieuTietKiem(DuLieuTietKiem snapshot)
        {
            danhSachKhachHang = snapshot.KhachHang;
            _dongSoHuuTheoSo.Clear();
            foreach (var pair in snapshot.DongSoHuu) _dongSoHuuTheoSo.Add(pair.Key, pair.Value);
            _tongHopTienSo.Clear();
            foreach (var pair in snapshot.TongHop) _tongHopTienSo.Add(pair.Key, pair.Value);
            dsLichSu.Clear();
            dsLichSu.AddRange(snapshot.LichSu);
            _lanTaiTietKiem = DateTime.UtcNow;
            _daTaiLanDau = true;
        }

        private void BaoLoiTaiTietKiem(Exception ex)
        {
            MessageBox.Show("Không thể đọc dữ liệu tiết kiệm từ SQL: " + ex.Message, "SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            danhSachKhachHang = new List<FrmMain.KhachHangData>();
        }

        private void LayDanhSachKhachHangTuFrmMain()
        {
            if (_dangTaiLanDau || IsDisposed || Disposing) return;
            try { ApDungDuLieuTietKiem(DocDuLieuTietKiem()); }
            catch (Exception ex) { BaoLoiTaiTietKiem(ex); }
        }

        private DuLieuTietKiem DocDuLieuTietKiem()
        {
            var snapshot = new DuLieuTietKiem();
            var ds = snapshot.KhachHang;
            var map = new Dictionary<string, FrmMain.KhachHangData>(StringComparer.OrdinalIgnoreCase);
            {
                DamBaoBangDongSoHuuTheoSo();
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"SELECT SoTKTK, CCCD, HoTen, SoDienThoai
                            FROM dbo.DongSoHuuSoTietKiem", cn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            string stk = rd.GetString(0);
                            if (!snapshot.DongSoHuu.TryGetValue(stk, out var owners))
                            {
                                owners = new List<DongSoHuuTheoSo>();
                                snapshot.DongSoHuu[stk] = owners;
                            }
                            owners.Add(new DongSoHuuTheoSo
                            {
                                CCCD = rd.GetString(1),
                                HoTen = rd.GetString(2),
                                SoDienThoai = rd.GetString(3)
                            });
                        }
                    }
                    using (var cmd = new SqlCommand("SELECT CCCD, HoTen, SoDienThoai FROM dbo.KhachHang", cn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            var kh = new FrmMain.KhachHangData { CCCD = rd.GetString(0), HoTen = rd.GetString(1), SoDienThoai = rd.GetString(2) };
                            ds.Add(kh); map.Add(kh.CCCD, kh);
                        }
                    using (var cmd = new SqlCommand("SELECT CCCD_ChuSoHuu, CCCD, HoTen, SoDienThoai FROM dbo.DongSoHuu", cn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            FrmMain.KhachHangData kh;
                            if (map.TryGetValue(rd.GetString(0), out kh))
                                kh.DanhSachDongSoHuu.Add(new FrmMain.DongSoHuuData { CCCD = rd.GetString(1), HoTen = rd.GetString(2), SoDienThoai = rd.GetString(3) });
                        }
                    using (var cmd = new SqlCommand(@"SELECT SoTKTK, CCCD, SoTienGui, LaiSuat, SoTienDaRut, KyHan, TrangThai, NgayMo, NgayCapNhatTT, SoTienGocBanDau, TongTienDaNhan FROM dbo.SoTietKiem", cn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                        {
                            snapshot.TongHop[rd.GetString(0)] = new TongHopTienSo
                            {
                                GocBanDau = Convert.ToDecimal(rd[9]),
                                GocConLai = Math.Max(0m, Convert.ToDecimal(rd[2])),
                                TongDaNhan = Convert.ToDecimal(rd[10])
                            };
                            FrmMain.KhachHangData kh;
                            if (!map.TryGetValue(rd.GetString(1), out kh)) continue;
                            kh.DanhSachTietKiem.Add(new FrmMain.SoTietKiemItem
                            {
                                SoTKTK = rd.GetString(0),
                                SoTienGui = rd.GetDecimal(2),
                                LaiSuat = rd.GetDouble(3),
                                SoTienDaRut = rd.GetDecimal(4),
                                KyHan = rd.GetString(5),
                                TrangThai = (FrmMain.TrangThaiTK)Convert.ToInt32(rd.GetValue(6)),
                                NgayMo = rd.GetDateTime(7),
                                NgayCapNhatTT = rd.IsDBNull(8) ? (DateTime?)null : rd.GetDateTime(8)
                            });
                        }
                    using (var cmd = new SqlCommand(@"SELECT SoTKTK, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai, TongTienNhan, GhiChu FROM dbo.LichSuGiaoDichTietKiem", cn))
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read()) snapshot.LichSu.Add(new LichSuGiaoDichTietKiem
                        {
                            SoTKTK = rd.GetString(0),
                            NgayGiaoDich = rd.GetDateTime(1),
                            LoaiGiaoDich = rd.GetString(2),
                            SoTienGoc = rd.GetDecimal(3),
                            SoTienLai = rd.GetDecimal(4),
                            TongTienNhan = rd.GetDecimal(5),
                            GhiChu = rd.IsDBNull(6) ? "" : rd.GetString(6)
                        });
                }
            }
            return snapshot;
        }

        private FrmMain TimFrmMainDangChuaForm()
        {
            Form form = this.Owner;
            if (form is FrmMain) return form as FrmMain;

            Form parent = this.Parent as Form;
            while (parent != null)
            {
                if (parent is FrmMain) return parent as FrmMain;
                parent = parent.Parent as Form;
            }

            foreach (Form f in Application.OpenForms)
            {
                if (f is FrmMain) return f as FrmMain;
            }

            return null;
        }

        private void TxtCCCD_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }

        private void TxtCCCD_TextChanged(object sender, EventArgs e)
        {
            string text = txtCCCD.Text ?? "";
            string cleanText = new string(text.Where(char.IsDigit).ToArray());
            if (cleanText.Length > 12) cleanText = cleanText.Substring(0, 12);

            if (txtCCCD.Text != cleanText)
            {
                int cursor = txtCCCD.SelectionStart;
                txtCCCD.Text = cleanText;
                txtCCCD.SelectionStart = Math.Min(cursor, cleanText.Length);
                return; // TextChanged đã chạy lại với chuỗi hợp lệ.
            }

            if (_dangKhoiTaoBoLoc || _dangTaiLanDau || IsDisposed || Disposing) return;
            _timerLocTietKiem.Stop();
            _timerLocTietKiem.Start();
        }

        private void BtnTimCCCD_Click(object sender, EventArgs e)
        {
            string cccd = txtCCCD.Text.Trim();
            _timerLocTietKiem.Stop();
            if (string.IsNullOrWhiteSpace(cccd))
            {
                MessageBox.Show("Vui lòng nhập CCCD cần tìm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            HienThiDanhSach(cccd);
            // Chỉ hỏi đăng ký khi người dùng tìm CCCD đầy đủ, không tự mở khi đang gõ.
            if (cccd.Length != 12 || !cccd.All(c => c >= '0' && c <= '9')) return;
            try
            {
                DamBaoBangDongSoHuuTheoSo();
                var kq = FrmMain.TraCuuTimMoMoiCaNhan(_connStr, cccd, "Tiết kiệm");
                if (kq.CoTaiKhoan) return; // Có tài khoản nhưng có thể đang bị bộ lọc ẩn.
                string noiDung = kq.CoDanhTinh
                    ? "Khách hàng chưa có tài khoản loại này. Bạn có muốn mở mới?"
                    : "CCCD chưa tồn tại. Bạn có muốn đăng ký khách hàng và mở tài khoản mới?";
                if (MessageBox.Show(this, noiDung, "Không tìm thấy tài khoản",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    MoSoTietKiemMoi(cccd, kq);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kiểm tra hoặc mở tài khoản: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private bool KiemTraKyHanFilter(object taiKhoan)
        {
            if (cboLocKyHan.SelectedIndex <= 0) return true;
            string khStr = LayString(taiKhoan, "KyHan");
            string sel = cboLocKyHan.SelectedItem.ToString();
            return ParseKyHanToMonths(khStr) == ParseKyHanToMonths(sel);
        }

        private bool KiemTraTrangThaiFilter(object taiKhoan)
        {
            if (cboLocTrangThai.SelectedIndex <= 0) return true;
            string tt = LayTrangThai(taiKhoan);
            int sel = cboLocTrangThai.SelectedIndex;

            if (sel == 1) return tt == "Hoạt động";
            if (sel == 2) return tt == "Đang khóa";
            if (sel == 3) return tt == "Đã đóng";

            return true;
        }

        private DateTime? LayNgayLamDateTime(object taiKhoan)
        {
            object value = LayGiaTri(taiKhoan, "NgayMo") ?? LayGiaTri(taiKhoan, "NgayLam") ?? LayGiaTri(taiKhoan, "NgayCapNhatTrangThai") ?? LayGiaTri(taiKhoan, "NgayCapNhatTT");
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

        private Font _fontVaiTro;
        private bool _dangKhoiTaoBoLoc;
        private bool _dangVeDanhSach;
        private void HienThiDanhSach(string filterCCCD)
        {
            if (_dangKhoiTaoBoLoc || _dangTaiLanDau || _dangVeDanhSach || IsDisposed || Disposing) return;
            _timerLocTietKiem.Stop();
            _dangVeDanhSach = true;
            dgvTietKiem.SuspendLayout();
            try { VeDanhSachTietKiem(filterCCCD); }
            finally { dgvTietKiem.ResumeLayout(); _dangVeDanhSach = false; }
        }

        private void VeDanhSachTietKiem(string filterCCCD)
        {
            dgvTietKiem.Rows.Clear();
            if (danhSachKhachHang == null) return;

            int stt = 1;
            decimal tongGui = 0;
            decimal tongDaNhan = 0;

            foreach (object kh in danhSachKhachHang)
            {
                string maCCCD = LayString(kh, "CCCD");
                string hoTen = LayString(kh, "HoTen");
                string sdt = LayString(kh, "SoDienThoai");
                // Đồng sở hữu được lọc RIÊNG cho mỗi số sổ bên dưới.

                IList dsTietKiem = LayDanhSach(kh, "DanhSachTietKiem");
                foreach (object tk in dsTietKiem)
                {
                    if (!KiemTraKyHanFilter(tk) || !KiemTraTrangThaiFilter(tk) || !KiemTraThoiGianFilter(tk)) continue;
                    string soTKDuocXet = LayString(tk, "SoTKTK");
                    if (!_dongSoHuuTheoSo.TryGetValue(soTKDuocXet, out var dsDongSoHuu))
                        dsDongSoHuu = new List<DongSoHuuTheoSo>();
                    if (!string.IsNullOrWhiteSpace(filterCCCD)
                        && !maCCCD.Contains(filterCCCD)
                        && !dsDongSoHuu.Any(d => d.CCCD.Contains(filterCCCD))) continue;

                    decimal soTienGui = LayDecimal(tk, "SoTienGui");
                    string stk = LayString(tk, "SoTKTK");
                    string kyHan = LayString(tk, "KyHan");
                    string laiSuat = LayLaiSuat(tk);
                    string ngayLam = LayNgayLam(tk);
                    string ngayCapNhat = LayNgayCapNhat(tk);
                    string trangThai = LayTrangThai(tk);

                    TongHopTienSo thongKe = LayTongHopTienSo(stk, soTienGui);
                    tongGui += thongKe.GocConLai;
                    tongDaNhan += thongKe.TongDaNhan;

                    // Dòng Chủ tài khoản
                    AddRow(stt++, stk, maCCCD, hoTen, sdt, LayTien(thongKe.GocConLai), LayTien(thongKe.TongDaNhan), laiSuat, kyHan, ngayLam, ngayCapNhat, "Chủ", trangThai);

                    // Dòng Đồng sở hữu
                    foreach (DongSoHuuTheoSo ds in dsDongSoHuu)
                    {
                        AddRow(stt++, stk, ds.CCCD, ds.HoTen, ds.SoDienThoai, LayTien(thongKe.GocConLai), LayTien(thongKe.TongDaNhan), laiSuat, kyHan, ngayLam, ngayCapNhat, "Đồng sở hữu", trangThai);
                    }
                }
            }

            lblTongTaiKhoan.Text = $"Tổng tài khoản: {stt - 1:N0} dòng / {dgvTietKiem.Rows.Cast<DataGridViewRow>().Select(r => r.Cells["colSTK"].Value?.ToString()).Distinct().Count():N0} sổ";
            lblTongTienGui.Text = $"Tổng gốc còn lại: {tongGui:N0} VNĐ";
            lblTongDaNhan.Text = $"Tổng tiền đã nhận: {tongDaNhan:N0} VNĐ";

            if (dgvTietKiem.Rows.Count > 0)
            {
                CapNhatThongTinKhachHangChon(0);
            }
            else
            {
                lblKhachHangChon.Text = "Đã chọn KH: [Chưa chọn] | CCCD: - | SĐT: - | Gốc còn lại: 0 VNĐ | Đã nhận: 0 VNĐ";
            }
        }

        private void AddRow(int stt, string stk, string cccd, string hoTen, string sdt, string tongGui, string daNhan, string laiSuat, string kyHan, string ngayLam, string ngayCapNhat, string vaiTro, string trangThai)
        {
            int rowIndex = dgvTietKiem.Rows.Add(stt, stk, cccd, hoTen, sdt, tongGui, daNhan, laiSuat, kyHan, ngayLam, ngayCapNhat, vaiTro, trangThai);
            DataGridViewRow row = dgvTietKiem.Rows[rowIndex];

            if (vaiTro == "Chủ")
            {
                row.Cells["colVaiTro"].Style.ForeColor = colorBlue;
                row.Cells["colVaiTro"].Style.Font = _fontVaiTro;
            }
            else
            {
                row.Cells["colVaiTro"].Style.ForeColor = Color.FromArgb(168, 85, 247);
                row.Cells["colVaiTro"].Style.Font = _fontVaiTro;
            }

            if (trangThai == "Hoạt động") row.Cells["colTrangThai"].Style.ForeColor = colorGreen;
            else if (trangThai == "Đang khóa") row.Cells["colTrangThai"].Style.ForeColor = colorOrange;
            else row.Cells["colTrangThai"].Style.ForeColor = colorRed;
        }

        private void DgvTietKiem_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvTietKiem.Rows[e.RowIndex].IsNewRow) return;

            // Luôn mở trung tâm thao tác, kể cả sổ đã bị hủy và cấp lại.
            // Đặt CurrentCell vào đúng dòng được nhấp đúp, tránh giữ sổ chọn trước đó.
            if (dgvTietKiem.CurrentCell == null || dgvTietKiem.CurrentCell.RowIndex != e.RowIndex)
                dgvTietKiem.CurrentCell = dgvTietKiem.Rows[e.RowIndex].Cells[e.ColumnIndex];
            CapNhatThongTinKhachHangChon(e.RowIndex);
            MoChiTietSoTietKiem();
        }


        // Nhấp đúp: mở thông tin đầy đủ. Nút "Thao tác sổ" giữ nguyên trung tâm
        // chức năng cũ (bao gồm Lịch sử & báo cáo cho sổ đã cấp lại).
        // Giữ trung tâm chức năng hoạt động sau mỗi thao tác.
        // Mỗi lần quay lại đều lấy dữ liệu mới từ SQL và chọn lại đúng số sổ.
        private void MoChiTietSoTietKiem()
        {
            if (_dangXemChiTietTietKiem || _dangTaiLanDau) return;
            _timerLocTietKiem.Stop();
            _dangXemChiTietTietKiem = true;
            try { MoChiTietSoTietKiemCore(); }
            finally { _dangXemChiTietTietKiem = false; }
        }

        private void MoChiTietSoTietKiemCore()
        {
            DataGridViewRow selected = dgvTietKiem.CurrentRow;
            if (selected == null || selected.IsNewRow) return;
            string soSoDangXem = Convert.ToString(selected.Cells["colSTK"].Value)?.Trim();
            string cccdDangXem = Convert.ToString(selected.Cells["colCCCD"].Value)?.Trim();
            if (string.IsNullOrWhiteSpace(soSoDangXem)) return;

            while (!IsDisposed && !Disposing)
            {
                // Giữ nguyên nghiệp vụ: dialog đóng trước khi thực hiện hành động.
                // Sau đó mở lại trung tâm với thông tin và trạng thái mới nhất.
                bool daChonThaoTac = MoChiTietSoTietKiemMotLan();
                if (!daChonThaoTac || IsDisposed || Disposing) break;

                LayDanhSachKhachHangTuFrmMain();
                HienThiDanhSach(txtCCCD.Text.Trim());

                DataGridViewRow dongCanChon = dgvTietKiem.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(r => !r.IsNewRow &&
                        string.Equals(Convert.ToString(r.Cells["colSTK"].Value)?.Trim(), soSoDangXem, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(Convert.ToString(r.Cells["colCCCD"].Value)?.Trim(), cccdDangXem, StringComparison.OrdinalIgnoreCase));
                if (dongCanChon == null)
                    dongCanChon = dgvTietKiem.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(r => !r.IsNewRow &&
                            string.Equals(Convert.ToString(r.Cells["colSTK"].Value)?.Trim(), soSoDangXem, StringComparison.OrdinalIgnoreCase));
                if (dongCanChon == null) break;

                dgvTietKiem.ClearSelection();
                dongCanChon.Selected = true;
                dgvTietKiem.CurrentCell = dongCanChon.Cells["colSTK"];
                CapNhatThongTinKhachHangChon(dongCanChon.Index);
            }
        }

        private bool MoChiTietSoTietKiemMotLan()
        {
            DataGridViewRow selected = dgvTietKiem.CurrentRow;
            if (selected == null || selected.IsNewRow) return false;
            string soSo = Convert.ToString(selected.Cells["colSTK"].Value)?.Trim();
            if (string.IsNullOrWhiteSpace(soSo)) return false;

            DataTable chuVaDongSoHuu = new DataTable();
            chuVaDongSoHuu.Columns.Add("Vai trò");
            chuVaDongSoHuu.Columns.Add("Họ và tên");
            chuVaDongSoHuu.Columns.Add("CCCD");
            chuVaDongSoHuu.Columns.Add("Số điện thoại");
            chuVaDongSoHuu.Columns.Add("Trạng thái TK");

            decimal tienGui, tongTienGuiBanDau, tongTienDaNhan = 0m;
            double laiSuat;
            string kyHan, trangThai, soThayThe = "";
            DateTime ngayMo;
            DateTime? ngayCapNhat = null, ngayCapLai = null;
            try
            {
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"SELECT s.SoTienGui, s.SoTienDaRut,
                                        s.LaiSuat, s.KyHan, s.TrangThai, s.NgayMo,
                                        s.NgayCapNhatTT, s.CCCD, k.HoTen, k.SoDienThoai
                                    FROM dbo.SoTietKiem s
                                    INNER JOIN dbo.KhachHang k ON k.CCCD = s.CCCD
                                    WHERE s.SoTKTK = @stk", cn))
                    {
                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                MessageBox.Show("Không tìm thấy thông tin sổ " + soSo + " trong SQL.",
                                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                            tienGui = Convert.ToDecimal(rd["SoTienGui"]);
                            laiSuat = Convert.ToDouble(rd["LaiSuat"]);
                            kyHan = Convert.ToString(rd["KyHan"]);
                            int maTrangThai = Convert.ToInt32(rd["TrangThai"]);
                            trangThai = maTrangThai == 0 ? "Hoạt động" : maTrangThai == 1 ? "Đang khóa" : "Đã đóng";
                            ngayMo = Convert.ToDateTime(rd["NgayMo"]);
                            if (rd["NgayCapNhatTT"] != DBNull.Value)
                                ngayCapNhat = Convert.ToDateTime(rd["NgayCapNhatTT"]);
                            chuVaDongSoHuu.Rows.Add("Chủ tài khoản", Convert.ToString(rd["HoTen"]),
                                Convert.ToString(rd["CCCD"]), Convert.ToString(rd["SoDienThoai"]), trangThai);
                        }
                    }

                    // Goc ban dau luu rieng: so hien tai (SoTienGui) la GOC CON LAI,
                    // con TongTienDaNhan luu tong cac lan rut/thanh toan (goc + lai).
                    using (var cmd = new SqlCommand(
                        "SELECT SoTienGocBanDau, ISNULL(TongTienDaNhan,0) FROM dbo.SoTietKiem WHERE SoTKTK=@stk", cn))
                    {
                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read()) throw new InvalidOperationException("Không tìm thấy số liệu thống kê sổ tiết kiệm.");
                            tongTienGuiBanDau = Convert.ToDecimal(rd[0]);
                            tongTienDaNhan = Convert.ToDecimal(rd[1]);
                        }
                    }

                    // Các cột này chỉ xuất hiện sau khi chạy phần SQL bổ sung cấp lại.
                    bool coSoThayThe;
                    bool coNgayCapLai;
                    using (var cmd = new SqlCommand(@"SELECT
                            CASE WHEN COL_LENGTH('dbo.SoTietKiem','SoTKTKThayThe') IS NULL THEN 0 ELSE 1 END,
                            CASE WHEN COL_LENGTH('dbo.SoTietKiem','NgayCapLai') IS NULL THEN 0 ELSE 1 END", cn))
                    using (var rd = cmd.ExecuteReader())
                    {
                        rd.Read();
                        coSoThayThe = rd.GetInt32(0) == 1;
                        coNgayCapLai = rd.GetInt32(1) == 1;
                    }
                    if (coSoThayThe || coNgayCapLai)
                    {
                        string sql = "SELECT " + (coSoThayThe ? "SoTKTKThayThe" : "CAST(NULL AS VARCHAR(20))")
                            + ", " + (coNgayCapLai ? "NgayCapLai" : "CAST(NULL AS DATETIME)")
                            + " FROM dbo.SoTietKiem WHERE SoTKTK=@stk";
                        using (var cmd = new SqlCommand(sql, cn))
                        {
                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                            using (var rd = cmd.ExecuteReader())
                            {
                                if (rd.Read())
                                {
                                    if (!rd.IsDBNull(0)) soThayThe = rd.GetString(0);
                                    if (!rd.IsDBNull(1)) ngayCapLai = rd.GetDateTime(1);
                                }
                            }
                        }
                    }

                    // Đồng sở hữu gắn trực tiếp với số sổ: không lấy nhầm người
                    // đồng sở hữu của các sổ khác cùng chủ tài khoản.
                    if (coSoThayThe && !string.IsNullOrEmpty(soThayThe)) trangThai = "Đã đóng (đã cấp lại)";
                    using (var cmd = new SqlCommand(@"IF OBJECT_ID(N'dbo.DongSoHuuSoTietKiem', N'U') IS NOT NULL
                             SELECT CCCD, HoTen, SoDienThoai FROM dbo.DongSoHuuSoTietKiem
                             WHERE SoTKTK=@stk ORDER BY HoTen", cn))
                    {
                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read())
                                chuVaDongSoHuu.Rows.Add("Đồng sở hữu", Convert.ToString(rd["HoTen"]),
                                    Convert.ToString(rd["CCCD"]), Convert.ToString(rd["SoDienThoai"]), trangThai);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải chi tiết sổ tiết kiệm:\n" + ex.Message,
                    "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Chi tiet va tat ca chuc nang nam TRONG CUNG MOT CUA SO.
            // Nghiệp vụ chỉ gọi sau khi dialog đóng, bảo toàn luồng xử lý cũ.
            string lenhDuocChon = null;
            string cccdNguoiCanSua = null;
            bool daCapLai = !string.IsNullOrWhiteSpace(soThayThe) || LaSoDaDuocCapLai(soSo);
            bool daDong = trangThai.StartsWith("Đã đóng", StringComparison.OrdinalIgnoreCase) || daCapLai;
            bool dangKhoa = trangThai == "Đang khóa";

            using (var dlg = new Form())
            {
                // Modern Banking: chỉ thay bố cục, không thay mã lệnh hay điều kiện nghiệp vụ.
                Color surface = Color.FromArgb(23, 33, 54);
                Color muted = Color.FromArgb(166, 182, 207);
                Color line = Color.FromArgb(49, 65, 88);
                dlg.Text = "Chi tiết & chức năng sổ tiết kiệm - " + soSo;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.WindowState = FormWindowState.Maximized;
                dlg.MinimumSize = new Size(1060, 660);
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 10F);
                dlg.AutoScroll = false;

                var root = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 5,
                    Padding = new Padding(24, 16, 24, 14),
                    BackColor = colorBg
                };
                root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 57F));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 154F));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 49F));
                dlg.Controls.Add(root);

                var heading = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "SỔ TIẾT KIỆM  /  " + soSo,
                    Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(96, 165, 250),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };
                root.Controls.Add(heading, 0, 0);

                // Bốn thẻ chi tiết: gốc ban đầu, tổng thực nhận, gốc còn lại, trạng thái.
                // Giao diện thẻ được đặt trong partial class FrmTietKiem.ChiTiet.Designer.cs,
                // hoàn toàn độc lập với Designer của màn hình danh sách sổ.
                var cards = TaoBangTheThongKeChiTiet();
                root.Controls.Add(cards, 0, 1);
                cards.Controls.Add(TaoTheThongKeChiTiet("TỔNG TIỀN GỬI BAN ĐẦU",
                    tongTienGuiBanDau.ToString("N0") + " VNĐ",
                    "Gốc đã rút: " + Math.Max(0m, tongTienGuiBanDau - tienGui).ToString("N0") + " VNĐ", Color.White), 0, 0);
                cards.Controls.Add(TaoTheThongKeChiTiet("TỔNG TIỀN ĐÃ NHẬN",
                    tongTienDaNhan.ToString("N0") + " VNĐ",
                    "Gốc và lãi thực nhận từ các giao dịch", Color.FromArgb(125, 211, 252)), 1, 0);
                cards.Controls.Add(TaoTheThongKeChiTiet("GỐC CÒN LẠI",
                    Math.Max(0m, tienGui).ToString("N0") + " VNĐ",
                    "Lãi suất: " + laiSuat.ToString("0.##") + "%/năm  •  Kỳ hạn: " + kyHan, colorGreen), 2, 0);
                cards.Controls.Add(TaoTheThongKeChiTiet("TRẠNG THÁI SỔ",
                    trangThai,
                    "Mở: " + ngayMo.ToString("dd/MM/yyyy") + "  •  Cập nhật: " + (ngayCapNhat?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật"),
                    daDong ? colorRed : dangKhoa ? colorOrange : colorGreen), 3, 0);

                var ownerArea = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 8, 0, 6) };
                ownerArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                ownerArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                ownerArea.Controls.Add(new Label { Text = "DANH SÁCH CHỦ SỞ HỮU / ĐỒNG SỞ HỮU", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12.5F, FontStyle.Bold), ForeColor = colorGreen, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AutoGenerateColumns = true,
                    DataSource = chuVaDongSoHuu,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = surface,
                    BorderStyle = BorderStyle.None,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    EnableHeadersVisualStyles = false,
                    GridColor = line,
                    RowTemplate = { Height = 39 },
                    ColumnHeadersHeight = 42,
                    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
                };
                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(43, 60, 85);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                grid.DefaultCellStyle.BackColor = surface;
                grid.DefaultCellStyle.ForeColor = Color.White;
                grid.DefaultCellStyle.Font = new Font("Segoe UI", 11F);
                grid.DefaultCellStyle.SelectionBackColor = colorBlue;
                grid.DefaultCellStyle.SelectionForeColor = Color.White;
                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(29, 42, 64);
                GanPhanCapCaNhan(grid, 1, 0, _fontVaiTro);
                grid.DataBindingComplete += (s, e) =>
                {
                    CanDeuBangSoHuuCaNhan(grid);
                    foreach (DataGridViewRow r in grid.Rows)
                        r.Cells["Vai trò"].Style.ForeColor = Convert.ToString(r.Cells["Vai trò"].Value) == "Đồng sở hữu"
                            ? Color.FromArgb(192, 132, 252) : Color.FromArgb(147, 197, 253);
                    grid.ClearSelection();
                };
                // Ghi nhận người được chọn trong bảng sở hữu để sửa đúng CCCD.
                Action ghiNhanNguoiChon = () =>
                {
                    if (grid.CurrentRow != null && !grid.CurrentRow.IsNewRow &&
                        grid.CurrentRow.Selected)
                        cccdNguoiCanSua = Convert.ToString(grid.CurrentRow.Cells["CCCD"].Value);
                    else
                        cccdNguoiCanSua = null;
                };
                grid.SelectionChanged += (s, e) => ghiNhanNguoiChon();
                grid.CellClick += (s, e) => ghiNhanNguoiChon();
                ownerArea.Controls.Add(grid, 0, 1);
                root.Controls.Add(ownerArea, 0, 2);

                var actionArea = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 6, 0, 0) };
                actionArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                actionArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                actionArea.Controls.Add(new Label { Text = "CHỨC NĂNG SỔ TIẾT KIỆM", Dock = DockStyle.Fill, ForeColor = colorGreen, Font = new Font("Segoe UI", 12.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
                var groups = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = colorBg };
                for (int i = 0; i < 4; i++) groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
                groups.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                string[] groupTitles = { "Giao dịch", "Trạng thái sổ", "Quản lý sổ", "Lịch sử & báo cáo" };
                TableLayoutPanel[] columns = new TableLayoutPanel[4];
                int[] counts = { 0, 0, 0, 0 };
                for (int i = 0; i < 4; i++)
                {
                    var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = surface, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(12, 7, 12, 9) };
                    panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
                    panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    panel.Controls.Add(new Label { Text = groupTitles[i], Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
                    var column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = surface };
                    column.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    for (int j = 0; j < 4; j++) column.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
                    panel.Controls.Add(column, 0, 1);
                    groups.Controls.Add(panel, i, 0);
                    columns[i] = column;
                }
                Action<int, string, string, bool> add = (index, code, name, allowed) =>
                {
                    int rowIndex = counts[index]++;
                    if (!allowed)
                    {
                        var blocked = new Label { Text = name + " (Không khả dụng)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Margin = new Padding(0, 3, 0, 3), ForeColor = colorRed, BackColor = Color.FromArgb(31, 41, 59), Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.No, AutoEllipsis = true };
                        columns[index].Controls.Add(blocked, 0, rowIndex);
                        return;
                    }
                    var button = new Button { Text = name, Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3), BackColor = Color.FromArgb(37, 52, 76), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9.5F), AutoEllipsis = true };
                    button.FlatAppearance.BorderColor = line;
                    button.Click += (s, e) => { lenhDuocChon = code; dlg.DialogResult = DialogResult.OK; dlg.Close(); };
                    columns[index].Controls.Add(button, 0, rowIndex);
                };
                add(0, "TAT_TOAN", "Tất toán sổ", !daDong && !dangKhoa);
                add(0, "RUT_TIEN", "Rút tiền tiết kiệm", !daDong && !dangKhoa);
                // Sổ đóng, bị khóa hoặc không còn gốc thì không được đem thế chấp.
                // Biểu mẫu vay sử dụng trực tiếp FrmVay.MoFormVayBangSoTietKiem.
                add(0, "VAY_BANG_SO", "Vay bằng sổ tiết kiệm", !daDong && !dangKhoa && tienGui > 0m);
                add(1, "KHOA_MO", dangKhoa ? "Mở khóa tài khoản" : "Khóa tạm thời", !daDong);
                add(1, "DONG", "Đóng tài khoản", !daDong && !dangKhoa);
                add(2, "CHUYEN_GIAO", "Sang tên / Chuyển quyền", !daDong && !dangKhoa);
                add(2, "SUA_THONG_TIN", "Sửa thông tin người được chọn", !daDong);
                add(2, "RUI_RO", "Xử lý rủi ro (Hủy/Cấp lại)", !daDong);
                add(3, "LICH_SU", "Xem lịch sử giao dịch", true);
                add(3, "LICH_SU_HD", "Xem lịch sử hoạt động", true);
                actionArea.Controls.Add(groups, 0, 1);
                root.Controls.Add(actionArea, 0, 3);

                var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
                footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
                string replacement = !string.IsNullOrEmpty(soThayThe) || ngayCapLai.HasValue
                    ? "Sổ thay thế: " + (string.IsNullOrEmpty(soThayThe) ? "-" : soThayThe) + "   |   Ngày cấp lại: " + (ngayCapLai?.ToString("dd/MM/yyyy HH:mm") ?? "-")
                    : "";
                footer.Controls.Add(new Label { Text = replacement, Dock = DockStyle.Fill, ForeColor = colorOrange, Font = new Font("Segoe UI", 10F), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true }, 0, 0);
                var btnDong = new Button { Text = "Đóng", Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 5), BackColor = surface, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };
                btnDong.FlatAppearance.BorderColor = line;
                btnDong.Click += (s, e) => dlg.Close();
                footer.Controls.Add(btnDong, 1, 0);
                root.Controls.Add(footer, 0, 4);
                dlg.CancelButton = btnDong;

                _dangMoTrungTamThaoTac = true;
                try { dlg.ShowDialog(this); }
                finally { _dangMoTrungTamThaoTac = false; }
            }

            // Kiểm tra lại SQL ngay trước khi thực hiện nghiệp vụ làm thay đổi sổ.
            if (!string.IsNullOrEmpty(lenhDuocChon) && lenhDuocChon != "LICH_SU" &&
                lenhDuocChon != "LICH_SU_HD" &&
                LaSoDaDuocCapLai(soSo))
            {
                MessageBox.Show("Sổ cũ đã được cấp lại chỉ có thể xem lịch sử và báo cáo.",
                    "Sổ không còn hiệu lực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            switch (lenhDuocChon)
            {
                case "TAT_TOAN": ThucHienTatToan(); break;
                case "RUT_TIEN": ThucHienRutTien(); break;
                case "VAY_BANG_SO":
                    MoVayBangSoTietKiem(soSo,
                        string.IsNullOrWhiteSpace(cccdNguoiCanSua)
                            ? Convert.ToString(selected.Cells["colCCCD"].Value)
                            : cccdNguoiCanSua);
                    break;
                case "KHOA_MO": ThucHienKhoaMoTaiKhoan(); break;
                case "DONG": ThucHienDongTaiKhoan(); break;
                case "CHUYEN_GIAO": ThucHienChuyenGiao(); break;
                case "SUA_THONG_TIN": SuaThongTinNguoiSoTietKiem(soSo, cccdNguoiCanSua); break;
                case "RUI_RO": ThucHienXuLyRuiRo(); break;
                case "LICH_SU": MoLichSuTuChiTiet(soSo, true); break;
                case "LICH_SU_HD": MoLichSuTuChiTiet(soSo, false); break;
                case "BAO_CAO_TXT": ThucHienXuatBaoCaoTxt(); break;
                case "PDF": ThucHienXuatHoaDon(); break;
            }
            // Chuyển tới Lịch sử: dừng vòng lặp cửa sổ chi tiết để trang mới hiện ngay.
            return !string.IsNullOrEmpty(lenhDuocChon) &&
                lenhDuocChon != "LICH_SU" && lenhDuocChon != "LICH_SU_HD";
        }

        // Mở đúng form thêm tài khoản vay của FrmVay, không tạo biểu mẫu vay khác.
        // CCCD được lấy từ người đang chọn trong bảng sở hữu của đúng số sổ.
        // Chỉ chuyển hai thông tin: số sổ tiết kiệm và CCCD người đứng tên khoản vay.
        private void MoVayBangSoTietKiem(string soSo, string cccdNguoiVay)
        {
            soSo = (soSo ?? "").Trim();
            cccdNguoiVay = (cccdNguoiVay ?? "").Trim();
            if (soSo.Length == 0 || cccdNguoiVay.Length == 0)
            {
                MessageBox.Show("Vui lòng chọn người vay và sổ tiết kiệm.",
                    "Chưa chọn đủ thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Không cho phép dùng sổ đã đóng/khóa/đã cấp lại hoặc hết gốc.
                // Đọc lại SQL ngay thời điểm chọn vay, tránh dữ liệu màn hình bị cũ.
                if (LaSoDaDuocCapLai(soSo))
                {
                    MessageBox.Show("Sổ tiết kiệm đã được cấp lại, không thể dùng số sổ cũ để vay.",
                        "Sổ không còn hiệu lực", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"
                    SELECT CASE WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.SoTietKiem AS s
                        WHERE s.SoTKTK = @stk
                          AND s.TrangThai = 0
                          AND s.SoTienGui > 0
                          AND (
                              s.CCCD = @cccd
                              OR EXISTS
                              (
                                  SELECT 1
                                  FROM dbo.DongSoHuuSoTietKiem AS d
                                  WHERE d.SoTKTK = s.SoTKTK
                                    AND d.CCCD = @cccd
                              )
                          )
                    ) THEN 1 ELSE 0 END", cn))
                {
                    cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                    cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccdNguoiVay;
                    cn.Open();
                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 1)
                    {
                        MessageBox.Show("Không thể vay: người được chọn không thuộc sổ này, " +
                            "hoặc sổ đã khóa/đóng/hết tiền gốc.",
                            "Không đủ điều kiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // FrmVay tự chọn loại VAY THẾ CHẤP, loại tài sản sổ tiết kiệm
                // trong ngân hàng và đúng số sổ sau khi tất cả control đã tạo xong.
                // Không can thiệp Application.Idle: tránh chỉnh form sau khi nó đã hiển thị.
                using (var frmVay = new FrmVay(cccdNguoiVay))
                    frmVay.MoFormVayBangSoTietKiem(soSo, cccdNguoiVay);

                // Sau khi đóng biểu mẫu vay, làm mới dữ liệu từ SQL trên form tiết kiệm.
                TaiLai();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở biểu mẫu vay bằng sổ tiết kiệm: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Sửa thông tin người được chọn: chủ hoặc đồng sở hữu của chính số sổ.
        // Không thay đổi số sổ, số dư, kỳ hạn, lãi suất hoặc quyền sở hữu.
        private void SuaThongTinNguoiSoTietKiem(string soSo, string cccd)
        {
            if (string.IsNullOrWhiteSpace(cccd))
            {
                MessageBox.Show("Vui lòng chọn chủ sở hữu hoặc đồng sở hữu trong bảng phía trên trước khi nhấn Sửa.",
                    "Chưa chọn người", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string hoTen = "", soDienThoai = "", vaiTro = "";
            try
            {
                using (var cn = new SqlConnection(_connStr))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT TOP (1) HoTen, SoDienThoai, VaiTro
                        FROM
                        (
                            SELECT k.HoTen, k.SoDienThoai,
                                   N'Chủ sở hữu' AS VaiTro
                            FROM dbo.SoTietKiem s
                            JOIN dbo.KhachHang k ON k.CCCD=s.CCCD
                            WHERE s.SoTKTK=@stk AND k.CCCD=@cccd
                            UNION ALL
                            SELECT d.HoTen, d.SoDienThoai,
                                   N'Đồng sở hữu' AS VaiTro
                            FROM dbo.DongSoHuuSoTietKiem d
                            WHERE d.SoTKTK=@stk AND d.CCCD=@cccd
                        ) AS nguoi", cn))
                    {
                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                        cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccd;
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                MessageBox.Show("Người được chọn không thuộc sổ tiết kiệm này.");
                                return;
                            }
                            hoTen = Convert.ToString(rd["HoTen"]);
                            soDienThoai = Convert.ToString(rd["SoDienThoai"]);
                            vaiTro = Convert.ToString(rd["VaiTro"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc thông tin: " + ex.Message);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = "Sửa thông tin " + vaiTro.ToLowerInvariant();
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ClientSize = new Size(510, 290);
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 10F);
                var info = new Label
                {
                    Text = "Sổ: " + soSo + "   |   " + vaiTro + "   |   CCCD: " + cccd,
                    Location = new Point(22, 20),
                    Size = new Size(470, 48),
                    ForeColor = Color.FromArgb(147, 197, 253)
                };
                var lblTen = new Label { Text = "Họ và tên:", Location = new Point(22, 91), AutoSize = true };
                var txtTen = new TextBox { Text = hoTen, Location = new Point(170, 85), Size = new Size(310, 30), MaxLength = 100 };
                var lblSdt = new Label { Text = "Số điện thoại:", Location = new Point(22, 145), AutoSize = true };
                var txtSdt = new TextBox { Text = soDienThoai, Location = new Point(170, 139), Size = new Size(310, 30), MaxLength = 11 };
                txtSdt.KeyPress += (s, e) =>
                {
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
                };
                var btnLuu = new Button
                {
                    Text = "Lưu thay đổi",
                    Location = new Point(170, 213),
                    Size = new Size(150, 39),
                    BackColor = colorBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                var btnHuy = new Button
                {
                    Text = "Hủy",
                    Location = new Point(333, 213),
                    Size = new Size(147, 39),
                    BackColor = colorDarkPanel,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel
                };
                btnLuu.Click += (s, e) =>
                {
                    string tenMoi = txtTen.Text.Trim();
                    string sdtMoi = txtSdt.Text.Trim();
                    if (string.IsNullOrWhiteSpace(tenMoi) ||
                        sdtMoi.Length < 10 || sdtMoi.Length > 11 ||
                        !sdtMoi.All(char.IsDigit))
                    {
                        MessageBox.Show("Vui lòng nhập họ tên và số điện thoại hợp lệ (10-11 chữ số).");
                        return;
                    }
                    try
                    {
                        using (var cn = new SqlConnection(_connStr))
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction())
                            {
                                try
                                {
                                    // Kiểm tra lại người được chọn thuộc đúng sổ.
                                    using (var check = new SqlCommand(@"
                                        SELECT CASE WHEN EXISTS(
                                            SELECT 1 FROM dbo.SoTietKiem
                                            WHERE SoTKTK=@stk AND CCCD=@cccd)
                                            OR EXISTS(
                                            SELECT 1 FROM dbo.DongSoHuuSoTietKiem
                                            WHERE SoTKTK=@stk AND CCCD=@cccd)
                                        THEN 1 ELSE 0 END", cn, tx))
                                    {
                                        check.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                                        check.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccd;
                                        if (Convert.ToInt32(check.ExecuteScalar()) != 1)
                                            throw new InvalidOperationException("Người này không còn thuộc sổ đã chọn.");
                                    }
                                    // Đồng bộ thông tin cùng CCCD ở các sổ/tài khoản.
                                    string[] sqlUpdates =
                                    {
                                        "UPDATE dbo.KhachHang SET HoTen=@ten, SoDienThoai=@sdt WHERE CCCD=@cccd",
                                        "UPDATE dbo.DongSoHuu SET HoTen=@ten, SoDienThoai=@sdt WHERE CCCD=@cccd",
                                        "UPDATE dbo.DongSoHuuSoTietKiem SET HoTen=@ten, SoDienThoai=@sdt WHERE CCCD=@cccd"
                                    };
                                    foreach (string sql in sqlUpdates)
                                    {
                                        using (var cmd = new SqlCommand(sql, cn, tx))
                                        {
                                            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = tenMoi;
                                            cmd.Parameters.Add("@sdt", SqlDbType.VarChar, 11).Value = sdtMoi;
                                            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 12).Value = cccd;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    using (var cmd = new SqlCommand(
                                        "UPDATE dbo.SoTietKiem SET NgayCapNhatTT=GETDATE() WHERE SoTKTK=@stk", cn, tx))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = soSo;
                                        cmd.ExecuteNonQuery();
                                    }
                                    tx.Commit();
                                }
                                catch { tx.Rollback(); throw; }
                            }
                        }
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                        MessageBox.Show("Đã cập nhật thông tin " + vaiTro.ToLowerInvariant() + " và đồng bộ theo CCCD.",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TaiLai();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể lưu thông tin: " + ex.Message,
                            "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                dlg.Controls.AddRange(new Control[] { info, lblTen, txtTen, lblSdt, txtSdt, btnLuu, btnHuy });
                dlg.AcceptButton = btnLuu;
                dlg.CancelButton = btnHuy;
                dlg.ShowDialog(this);
            }
        }

        private void CapNhatThongTinKhachHangChon(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvTietKiem.Rows.Count) return;
            DataGridViewRow row = dgvTietKiem.Rows[rowIndex];

            string cccd = row.Cells["colCCCD"].Value?.ToString() ?? "-";
            string hoTen = row.Cells["colHoTen"].Value?.ToString() ?? "-";
            string sdt = row.Cells["colSDT"].Value?.ToString() ?? "-";
            string tongGui = row.Cells["colTongTienGui"].Value?.ToString() ?? "0 VNĐ";
            string daNhan = row.Cells["colSoTienNhan"].Value?.ToString() ?? "0 VNĐ";

            lblKhachHangChon.Text = $"Đã chọn KH: {hoTen} | CCCD: {cccd} | SĐT: {sdt} | Gốc còn lại: {tongGui} | Tổng đã nhận: {daNhan}";
        }

        // 1. MỞ TÀI KHOẢN MỚI
        private void MoSoTietKiemMoi(string initialCCCD = "", FrmMain.KetQuaTimMoMoiCaNhan kq = null)
        {
            // Người đang chọn là chủ sổ mới, kể cả trước đây là đồng sở hữu sổ khác.
            string currentCCCD = (initialCCCD ?? "").Trim();
            if (string.IsNullOrEmpty(currentCCCD) && dgvTietKiem.CurrentRow != null)
                currentCCCD = Convert.ToString(dgvTietKiem.CurrentRow.Cells["colCCCD"].Value)?.Trim() ?? "";
            if (string.IsNullOrEmpty(currentCCCD)) currentCCCD = txtCCCD.Text.Trim();
            if (currentCCCD.Length != 12 || !currentCCCD.All(char.IsDigit))
            {
                MessageBox.Show("Vui lòng chọn người trên bảng hoặc nhập đúng CCCD 12 số trước khi mở sổ.",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentHoTen = "", currentSDT = "";
            bool existedInKhachHang = false;
            try
            {
                using (var cn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(@"
                        SELECT TOP 1 HoTen, SoDienThoai, LaKhachHang FROM
                        (
                            SELECT HoTen, SoDienThoai, 1 AS LaKhachHang, 0 AS UuTien
                            FROM dbo.KhachHang WHERE CCCD = @c
                            UNION ALL
                            SELECT HoTen, SoDienThoai, 0 AS LaKhachHang, 1 AS UuTien
                            FROM dbo.DongSoHuu WHERE CCCD = @c
                            UNION ALL
                            SELECT HoTen, SoDienThoai, 0 AS LaKhachHang, 2 AS UuTien
                            FROM dbo.DongSoHuuSoTietKiem WHERE CCCD = @c
                        ) X ORDER BY UuTien", cn))
                {
                    cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = currentCCCD;
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                        if (rd.Read())
                        {
                            currentHoTen = rd.GetString(0);
                            currentSDT = rd.GetString(1);
                            existedInKhachHang = rd.GetInt32(2) == 1;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lấy được thông tin khách hàng từ SQL: " + ex.Message);
                return;
            }
            bool khachMoi = kq != null && !kq.CoDanhTinh;
            if (kq != null && kq.CoDanhTinh)
            {
                currentHoTen = kq.HoTen;
                currentSDT = kq.SoDienThoai;
                existedInKhachHang = kq.CoHoSoChinh;
            }
            if (string.IsNullOrWhiteSpace(currentHoTen) && !khachMoi)
            {
                MessageBox.Show("CCCD chưa có trong hệ thống. Vui lòng đăng ký khách hàng trước khi mở sổ.",
                    "Không tìm thấy khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Form dlg = new Form())
            {
                dlg.Text = "Thêm Tài Khoản Cho Khách Hàng - Tiết kiệm";
                dlg.Size = new Size(580, 680);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.AutoScroll = true;
                dlg.BackColor = Color.FromArgb(245, 245, 247);
                dlg.ForeColor = Color.Black;
                Font fontNormal = new Font("Segoe UI", 9F);
                Font fontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
                var lblCCCD = new Label { Text = "CCCD (12 số):", Location = new Point(35, 25), AutoSize = true, Font = fontNormal };
                var txtCCCDDlg = new TextBox { Location = new Point(165, 22), Width = 230, Text = currentCCCD, ReadOnly = true, Enabled = false, Font = fontNormal };
                var lblHoTen = new Label { Text = "Họ và Tên:", Location = new Point(35, 60), AutoSize = true, Font = fontNormal };
                var txtHoTenDlg = new TextBox { Location = new Point(165, 57), Width = 230, Text = currentHoTen, ReadOnly = !khachMoi, Enabled = khachMoi, MaxLength = 100, Font = fontNormal };
                var lblSDT = new Label { Text = "Số điện thoại:", Location = new Point(35, 95), AutoSize = true, Font = fontNormal };
                var txtSDTDlg = new TextBox { Location = new Point(165, 92), Width = 230, Text = currentSDT, ReadOnly = !khachMoi, Enabled = khachMoi, MaxLength = 11, Font = fontNormal };

                var btnThemDSH = new Button { Text = "+ Thêm đồng sở hữu", Location = new Point(410, 90), Size = new Size(135, 28), FlatStyle = FlatStyle.Flat, ForeColor = colorBlue, BackColor = Color.White, Font = fontNormal };
                btnThemDSH.FlatAppearance.BorderColor = colorBlue;
                var nudDSH = new NumericUpDown { Location = new Point(410, 125), Width = 135, Minimum = 1, Maximum = 2, Value = 1, Visible = false };
                var pnlDSH = new Panel { Location = new Point(25, 160), Size = new Size(520, 205), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, AutoScroll = true, Visible = false };
                var inputsDSH = new List<Tuple<TextBox, TextBox, TextBox>>();
                Action renderDSH = () =>
                {
                    var previous = inputsDSH.Select(x => new[] { x.Item1.Text, x.Item2.Text, x.Item3.Text }).ToList();
                    pnlDSH.Controls.Clear(); inputsDSH.Clear();
                    for (int i = 0; i < (int)nudDSH.Value; i++)
                    {
                        var g = new GroupBox { Text = "Đồng sở hữu " + (i + 1), Location = new Point(8, 8 + 64 * i), Size = new Size(490, 58) };
                        var c = new TextBox { Location = new Point(50, 20), Width = 115, MaxLength = 12 };
                        var n = new TextBox { Location = new Point(225, 20), Width = 105, MaxLength = 100 };
                        var p = new TextBox { Location = new Point(375, 20), Width = 105, MaxLength = 11 };
                        c.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
                        p.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
                        if (i < previous.Count) { c.Text = previous[i][0]; n.Text = previous[i][1]; p.Text = previous[i][2]; }
                        g.Controls.AddRange(new Control[] {
                                new Label { Text = "CCCD", Location = new Point(7, 23), AutoSize = true }, c,
                                new Label { Text = "Họ tên", Location = new Point(172, 23), AutoSize = true }, n,
                                new Label { Text = "SĐT", Location = new Point(340, 23), AutoSize = true }, p
                            });
                        pnlDSH.Controls.Add(g);
                        inputsDSH.Add(Tuple.Create(c, n, p));
                    }
                };
                nudDSH.ValueChanged += (s, e) => renderDSH();

                var lblLoaiTK = new Label { Text = "Loại tài khoản:", Location = new Point(35, 130), AutoSize = true, Font = fontNormal };
                var cboLoaiTK = new ComboBox { Location = new Point(165, 127), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, Font = fontNormal, Enabled = false };
                cboLoaiTK.Items.AddRange(new object[] { "Thanh toán", "Tiết kiệm", "Vay" });
                cboLoaiTK.SelectedIndex = 1; // Luôn tiết kiệm.
                var rdoNgauNhien = new RadioButton { Text = "Số ngẫu nhiên (Free)", Location = new Point(35, 165), AutoSize = true, Checked = true, Font = fontNormal };
                var rdoSoDep = new RadioButton { Text = "Số đẹp (Tính phí)", Location = new Point(200, 165), AutoSize = true, Font = fontNormal };
                var lblSTK = new Label { Text = "Số TK được cấp:", Location = new Point(35, 200), AutoSize = true, Font = fontNormal };
                var txtSTK = new TextBox { Location = new Point(165, 197), Width = 230, ReadOnly = true, BackColor = Color.White, Font = fontBold };

                var random = new Random();
                Action taoSo = () =>
                {
                    try
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            string candidate = rdoSoDep.Checked
                                ? "8888" + random.Next(100000, 1000000).ToString("D6")
                                : "107" + random.Next(1000000, 10000000).ToString("D7");
                            if (!SoTaiKhoanDaTonTai(candidate)) { txtSTK.Text = candidate; return; }
                        }
                        txtSTK.Clear();
                        MessageBox.Show("Chưa tạo được số tài khoản không trùng. Vui lòng thử lại.");
                    }
                    catch (Exception ex) { txtSTK.Clear(); MessageBox.Show("Không kiểm tra được số tài khoản: " + ex.Message); }
                };
                rdoNgauNhien.CheckedChanged += (s, e) => { if (rdoNgauNhien.Checked) taoSo(); };
                rdoSoDep.CheckedChanged += (s, e) => { if (rdoSoDep.Checked) taoSo(); };
                taoSo();

                var pnlTietKiem = new Panel { Location = new Point(25, 235), Size = new Size(520, 335), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
                var lblHeader = new Label { Text = "THÔNG TIN SỔ TIẾT KIỆM", Location = new Point(25, 16), AutoSize = true, Font = fontBold, ForeColor = colorBlue };
                var lblGui = new Label { Text = "Số tiền gửi (VNĐ):", Location = new Point(25, 58), AutoSize = true, Font = fontNormal };
                var txtGui = new TextBox { Location = new Point(180, 55), Width = 220, Font = fontNormal };
                txtGui.KeyPress += (s, e) => { if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
                var lblKH = new Label { Text = "Kỳ hạn gửi:", Location = new Point(25, 100), AutoSize = true, Font = fontNormal };
                var cboKH = new ComboBox { Location = new Point(180, 97), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = fontNormal };
                try
                {
                    using (var cn = new SqlConnection(_connStr))
                    using (var cmd = new SqlCommand(@"SELECT Thang, LaiSuat FROM dbo.CauHinhLoaiTaiKhoan
                            WHERE MaLoaiTK='TK_TIETKIEM' AND Thang > 0 ORDER BY Thang", cn))
                    {
                        cn.Open();
                        using (var rd = cmd.ExecuteReader())
                            while (rd.Read())
                                cboKH.Items.Add(new KyHanLaiSuat { Thang = rd.GetInt32(0), LaiSuat = rd.GetDecimal(1) });
                    }
                }
                catch (Exception ex) { MessageBox.Show("Không tải được cấu hình lãi suất: " + ex.Message); }
                if (cboKH.Items.Count > 0) cboKH.SelectedIndex = 0;
                var lblLai = new Label { Text = "Lãi suất theo kỳ hạn", Location = new Point(25, 148), AutoSize = true, ForeColor = colorBlue, Font = fontBold };
                cboKH.SelectedIndexChanged += (s, e) =>
                {
                    var k = cboKH.SelectedItem as KyHanLaiSuat;
                    lblLai.Text = k == null ? "Lãi suất: -" : $"Lãi suất: {k.LaiSuat:0.##}%/năm";
                };
                if (cboKH.SelectedItem is KyHanLaiSuat initKH)
                    lblLai.Text = $"Lãi suất: {initKH.LaiSuat:0.##}%/năm";
                var lblGhiChu = new Label { Text = "* Rút trước hạn áp dụng mức lãi theo quy định của hệ thống.", Location = new Point(25, 180), Size = new Size(460, 35), Font = new Font("Segoe UI", 8F, FontStyle.Italic), ForeColor = Color.Gray };
                var btnXacNhan = new Button { Text = "Xác nhận mở", Location = new Point(110, 248), Size = new Size(125, 36), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = fontBold };
                var btnHuy = new Button { Text = "Hủy", Location = new Point(250, 248), Size = new Size(110, 36), BackColor = Color.White, FlatStyle = FlatStyle.Flat, Font = fontNormal };
                btnHuy.Click += (s, e) => dlg.Close();
                pnlTietKiem.Controls.AddRange(new Control[] { lblHeader, lblGui, txtGui, lblKH, cboKH, lblLai, lblGhiChu, btnXacNhan, btnHuy });

                Action updateLayout = () =>
                {
                    int offset = pnlDSH.Visible ? 220 : 0;
                    lblLoaiTK.Top = 130 + offset; cboLoaiTK.Top = 127 + offset;
                    rdoNgauNhien.Top = 165 + offset; rdoSoDep.Top = 165 + offset;
                    lblSTK.Top = 200 + offset; txtSTK.Top = 197 + offset;
                    pnlTietKiem.Top = 235 + offset;
                    dlg.AutoScrollMinSize = new Size(0, pnlTietKiem.Bottom + 50);
                };
                btnThemDSH.Click += (s, e) =>
                {
                    nudDSH.Visible = true;
                    pnlDSH.Visible = true;
                    if (inputsDSH.Count == 0) renderDSH();
                    updateLayout();
                };
                pnlDSH.VisibleChanged += (s, e) => updateLayout();
                updateLayout();

                btnXacNhan.Click += (s, e) =>
                {
                    if (khachMoi)
                    {
                        currentHoTen = txtHoTenDlg.Text.Trim(); currentSDT = txtSDTDlg.Text.Trim();
                        if (!FrmMain.KiemTraThongTinTimMoMoiCaNhan(currentCCCD, currentHoTen, currentSDT)) return;
                    }
                    if (txtSTK.Text.Length == 0 || cboKH.SelectedItem == null)
                    { MessageBox.Show("Vui lòng kiểm tra số tài khoản và kỳ hạn gửi."); return; }
                    if (!decimal.TryParse(txtGui.Text.Trim(), out decimal tienGui) || tienGui <= 0)
                    { MessageBox.Show("Số tiền gửi phải lớn hơn 0 VNĐ."); return; }
                    if (tienGui > 999999999999999999m)
                    { MessageBox.Show("Số tiền gửi vượt giới hạn lưu trữ."); return; }
                    var cauHinh = (KyHanLaiSuat)cboKH.SelectedItem;
                    var dsMoi = new List<Tuple<string, string, string>>();
                    var cccdDaCo = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { currentCCCD };
                    var sdtDaCo = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { currentSDT };
                    foreach (var t in inputsDSH)
                    {
                        string c = t.Item1.Text.Trim(), n = t.Item2.Text.Trim(), p = t.Item3.Text.Trim();
                        if (c.Length != 12 || !c.All(char.IsDigit) || string.IsNullOrWhiteSpace(n) ||
                            p.Length < 10 || p.Length > 11 || !p.All(char.IsDigit) ||
                            !cccdDaCo.Add(c) || !sdtDaCo.Add(p))
                        {
                            MessageBox.Show("Đồng sở hữu phải có CCCD 12 số, họ tên, SĐT hợp lệ và không được trùng chủ hoặc người khác.");
                            return;
                        }
                        dsMoi.Add(Tuple.Create(c, n, p));
                    }
                    string stk = txtSTK.Text.Trim();
                    try
                    {
                        using (var cn = new SqlConnection(_connStr))
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                            {
                                try
                                {
                                    using (var check = new SqlCommand(@"
                                            IF EXISTS(SELECT 1 FROM dbo.SoTietKiem WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTK=@stk)
                                               OR EXISTS(SELECT 1 FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTT=@stk)
                                               OR EXISTS(SELECT 1 FROM dbo.TaiKhoanVay WITH (UPDLOCK, HOLDLOCK) WHERE SoTKVay=@stk)
                                                THROW 50002, N'Số tài khoản đã tồn tại.', 1;", cn, tx))
                                    {
                                        check.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        check.ExecuteNonQuery();
                                    }
                                    // Đồng sở hữu cũ trở thành CHỦ mới, không ảnh hưởng sổ cũ.
                                    if (khachMoi) FrmMain.KiemTraTrungKhachHangTimMoMoiCaNhan(cn, tx, currentCCCD, currentSDT);
                                    if (!existedInKhachHang)
                                    {
                                        using (var cmd = new SqlCommand(@"
                                                IF NOT EXISTS(SELECT 1 FROM dbo.KhachHang WITH (UPDLOCK, HOLDLOCK) WHERE CCCD=@c)
                                                    INSERT INTO dbo.KhachHang(CCCD, HoTen, SoDienThoai)
                                                    VALUES (@c, @n, @p);", cn, tx))
                                        {
                                            cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = currentCCCD;
                                            cmd.Parameters.Add("@n", SqlDbType.NVarChar, 100).Value = currentHoTen;
                                            cmd.Parameters.Add("@p", SqlDbType.VarChar, 11).Value = currentSDT;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    foreach (var d in dsMoi)
                                    {
                                        using (var check = new SqlCommand(@"
                                                SELECT TOP 1 HoTen, SoDienThoai FROM
                                                (SELECT HoTen, SoDienThoai, 0 AS ord FROM dbo.KhachHang WHERE CCCD=@c
                                                 UNION ALL SELECT HoTen, SoDienThoai, 1 FROM dbo.DongSoHuu WHERE CCCD=@c
                                                 UNION ALL SELECT HoTen, SoDienThoai, 2 FROM dbo.DongSoHuuSoTietKiem WHERE CCCD=@c) x
                                                ORDER BY ord", cn, tx))
                                        {
                                            check.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = d.Item1;
                                            using (var rd = check.ExecuteReader())
                                            {
                                                if (rd.Read() &&
                                                    (!string.Equals(rd.GetString(0).Trim(), d.Item2, StringComparison.OrdinalIgnoreCase)
                                                    || rd.GetString(1) != d.Item3))
                                                    throw new InvalidOperationException("Thông tin CCCD đã có trong hệ thống không khớp: " + d.Item1);
                                            }
                                        }
                                    }
                                    using (var cmd = new SqlCommand(@"
                                            INSERT INTO dbo.SoTietKiem
                                            (SoTKTK, CCCD, SoTienGui, LaiSuat, SoTienDaRut, KyHan, TrangThai, NgayMo,
                                             SoTienGocBanDau, TongTienDaNhan)
                                            VALUES (@stk, @c, @t, @lai, 0, @kyhan, 0, GETDATE(), @t, 0);", cn, tx))
                                    {
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = currentCCCD;
                                        var pTien = cmd.Parameters.Add("@t", SqlDbType.Decimal); pTien.Precision = 18; pTien.Scale = 0; pTien.Value = tienGui;
                                        cmd.Parameters.Add("@lai", SqlDbType.Float).Value = (double)cauHinh.LaiSuat;
                                        cmd.Parameters.Add("@kyhan", SqlDbType.NVarChar, 20).Value = cauHinh.Thang + " tháng";
                                        cmd.ExecuteNonQuery();
                                    }
                                    foreach (var d in dsMoi)
                                    {
                                        using (var cmd = new SqlCommand(@"
                                                INSERT INTO dbo.DongSoHuuSoTietKiem(SoTKTK, CCCD, HoTen, SoDienThoai)
                                                VALUES (@stk, @c, @n, @p);", cn, tx))
                                        {
                                            cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                            cmd.Parameters.Add("@c", SqlDbType.VarChar, 12).Value = d.Item1;
                                            cmd.Parameters.Add("@n", SqlDbType.NVarChar, 100).Value = d.Item2;
                                            cmd.Parameters.Add("@p", SqlDbType.VarChar, 11).Value = d.Item3;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    using (var cmd = new SqlCommand(@"
                                            INSERT INTO dbo.LichSuGiaoDichTietKiem
                                            (SoTKTK, NgayGiaoDich, LoaiGiaoDich, SoTienGoc, SoTienLai,
                                             TongTienNhan, GhiChu, NguoiThucHien)
                                            VALUES (@stk, GETDATE(), N'Mở sổ tiết kiệm', @t, 0, 0, @ghichu, @nguoi);", cn, tx))
                                    {
                                        cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                                        cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                                        var pTien = cmd.Parameters.Add("@t", SqlDbType.Decimal); pTien.Precision = 18; pTien.Scale = 0; pTien.Value = tienGui;
                                        cmd.Parameters.Add("@ghichu", SqlDbType.NVarChar, 500).Value =
                                            $"Gửi mới: {tienGui:N0} VNĐ, kỳ hạn {cauHinh.Thang} tháng, lãi suất {cauHinh.LaiSuat:0.##}%/năm.";
                                        cmd.ExecuteNonQuery();
                                    }
                                    tx.Commit();
                                }
                                catch { tx.Rollback(); throw; }
                            }
                        }
                        MessageBox.Show($"Mở sổ tiết kiệm thành công!\nChủ sở hữu: {currentHoTen}\nSố sổ: {stk}\nSố đồng sở hữu: {dsMoi.Count}\nSố tiền gửi: {tienGui:N0} VNĐ",
                            "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không mở được sổ tiết kiệm: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                dlg.Controls.AddRange(new Control[] { lblCCCD, txtCCCDDlg, lblHoTen, txtHoTenDlg,
                        lblSDT, txtSDTDlg, btnThemDSH, nudDSH, pnlDSH, lblLoaiTK, cboLoaiTK,
                        rdoNgauNhien, rdoSoDep, lblSTK, txtSTK, pnlTietKiem });
                if (kq != null) FrmMain.DongBoMauKhungMoTaiKhoan(dlg);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtCCCD.Text = currentCCCD;
                    LayDanhSachKhachHangTuFrmMain();
                    HienThiDanhSach(txtCCCD.Text.Trim());
                    OnDataChanged();
                }
            }
        }

        // 2. TẤT TOÁN SỔ TIẾT KIỆM
        private void ThucHienTatToan()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = dgvTietKiem.CurrentRow.Cells["colSTK"].Value?.ToString();
            object tkObj = TimSoTietKiemTheoSTK(stk, out object khObj);

            if (tkObj == null) return;

            string trangThai = LayTrangThai(tkObj);
            if (trangThai == "Đã đóng")
            {
                MessageBox.Show("Sổ tiết kiệm này đã đóng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (trangThai == "Đang khóa")
            {
                MessageBox.Show("Tài khoản đang bị khóa, không thể thực hiện tất toán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal soTienGui = LayDecimal(tkObj, "SoTienGui");
            if (soTienGui <= 0)
            {
                MessageBox.Show("Số dư gốc hiện tại bằng 0, không có số tiền để tất toán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double laiSuat = LayDouble(tkObj, "LaiSuat");
            string kyHanStr = LayString(tkObj, "KyHan");
            int soThang = ParseKyHanToMonths(kyHanStr);

            decimal tienLaiDungHan = soTienGui * (decimal)(laiSuat / 100.0) * ((decimal)soThang / 12m);
            decimal tongThucNhan = soTienGui + tienLaiDungHan;

            using (Form dlg = new Form())
            {
                dlg.Text = "Bảng Tính Lãi Tất Toán Tiết Kiệm Đúng Kỳ Hạn";
                dlg.Size = new Size(480, 380);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;

                Label lblTitle = new Label { Text = "TẤT TOÁN SỔ TIẾT KIỆM", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = colorGreen, Location = new Point(20, 15), AutoSize = true };
                Label lblInfo = new Label { Text = $"STK: {stk} | Khách hàng: {LayString(khObj, "HoTen")}", Location = new Point(20, 50), AutoSize = true };

                Label lblGoc = new Label { Text = $"Số tiền gốc gửi: {soTienGui:N0} VNĐ", Location = new Point(20, 90), AutoSize = true };
                Label lblKH = new Label { Text = $"Kỳ hạn gửi: {kyHanStr}", Location = new Point(20, 120), AutoSize = true };
                Label lblLS = new Label { Text = $"Lãi suất kỳ hạn: {laiSuat:0.##}% / năm", Location = new Point(20, 150), AutoSize = true };
                Label lblLai = new Label { Text = $"Tiền lãi nhận được: {tienLaiDungHan:N0} VNĐ", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = colorGreen, Location = new Point(20, 190), AutoSize = true };
                Label lblTong = new Label { Text = $"TỔNG TIỀN TẤT TOÁN: {tongThucNhan:N0} VNĐ", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = colorOrange, Location = new Point(20, 230), AutoSize = true };

                Button btnXacNhan = new Button { Text = "Xác nhận Tất Toán", Location = new Point(130, 280), Size = new Size(140, 38), BackColor = colorGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(280, 280), Size = new Size(100, 38) };

                dlg.Controls.AddRange(new Control[] { lblTitle, lblInfo, lblGoc, lblKH, lblLS, lblLai, lblTong, btnXacNhan, btnHuy });

                btnXacNhan.Click += (s, ev) =>
                {
                    var giaoDichVuaTao = new LichSuGiaoDichTietKiem
                    {
                        SoTKTK = stk,
                        NgayGiaoDich = DateTime.Now,
                        LoaiGiaoDich = "Tất toán đúng hạn",
                        SoTienGoc = soTienGui,
                        SoTienLai = tienLaiDungHan,
                        TongTienNhan = tongThucNhan,
                        GhiChu = $"Tất toán sổ tiết kiệm kỳ hạn {kyHanStr}"
                    };

                    if (!LuuGiaoDichRutTietKiem(giaoDichVuaTao, soTienGui, khObj)) return;

                    MessageBox.Show($"Đã tất toán sổ tiết kiệm {stk} thành công! Số tiền tất toán: {tongThucNhan:N0} VNĐ.\nSố tiền gửi hiện tại đã về 0 VNĐ, bạn đã có thể thực hiện Đóng tài khoản.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                };

                btnHuy.Click += (s, ev) => dlg.Close();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LayDanhSachKhachHangTuFrmMain();
                    HienThiDanhSach(txtCCCD.Text.Trim());
                }
            }
            OnDataChanged();
        }

        // 3. RÚT TIỀN TIẾT KIỆM
        private void ThucHienRutTien()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = dgvTietKiem.CurrentRow.Cells["colSTK"].Value?.ToString();
            object tkObj = TimSoTietKiemTheoSTK(stk, out object khObj);

            if (tkObj == null) return;

            string trangThai = LayTrangThai(tkObj);
            if (trangThai == "Đã đóng")
            {
                MessageBox.Show("Sổ tiết kiệm này đã đóng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (trangThai == "Đang khóa")
            {
                MessageBox.Show("Tài khoản đang bị khóa, không thể rút tiền!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal soTienGui = LayDecimal(tkObj, "SoTienGui");
            if (soTienGui <= 0)
            {
                MessageBox.Show("Số dư sổ tiết kiệm bằng 0, không thể rút tiền!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime? ngayMo = LayNgayLamDateTime(tkObj) ?? DateTime.Now;
            int soNgayGui = Math.Max(1, (int)(DateTime.Now - ngayMo.Value).TotalDays);

            using (Form dlg = new Form())
            {
                dlg.Text = "Tùy Chọn Rút Tiền Tiết Kiệm";
                dlg.Size = new Size(540, 420);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;

                Label lblTitle = new Label { Text = $"RÚT TIỀN TIẾT KIỆM - STK: {stk}", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = colorBlue, Location = new Point(20, 15), AutoSize = true };
                Label lblGoc = new Label { Text = $"Số tiền gốc hiện tại: {soTienGui:N0} VNĐ", Font = new Font("Segoe UI", 10F, FontStyle.Regular), Location = new Point(20, 48), AutoSize = true };

                Label lblCongThuc = new Label
                {
                    Text = $"* Công thức tính lãi suất rút trước kỳ (0.1%/năm):\n  Lãi trước kỳ = (Số tiền rút) × 0.1% × ({soNgayGui} ngày / 365)",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(250, 204, 21),
                    Location = new Point(20, 75),
                    AutoSize = true
                };

                Button btnRutToanBo = new Button { Text = "RÚT TOÀN BỘ TIỀN", Location = new Point(20, 125), Size = new Size(220, 45), BackColor = colorRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
                Button btnRut1Phan = new Button { Text = "RÚT 1 PHẦN TIỀN", Location = new Point(260, 125), Size = new Size(220, 45), BackColor = colorBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };

                Panel pnlRut1Phan = new Panel { Location = new Point(20, 185), Size = new Size(480, 175), BorderStyle = BorderStyle.FixedSingle, Visible = false };
                Label lblNhapTien = new Label { Text = "Nhập số tiền muốn rút (VNĐ):", Location = new Point(10, 12), AutoSize = true };
                TextBox txtRutPart = new TextBox { Location = new Point(200, 10), Width = 250 };
                Label lblKetQuaPart = new Label { Text = "Nhập số tiền rút để xem lãi trước kỳ (0.1%/năm tính trên số tiền rút)...", Location = new Point(10, 45), AutoSize = true, ForeColor = colorGreen };
                Button btnXacNhanPart = new Button { Text = "Xác nhận rút 1 phần", Location = new Point(200, 125), Size = new Size(160, 35), BackColor = colorGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

                pnlRut1Phan.Controls.AddRange(new Control[] { lblNhapTien, txtRutPart, lblKetQuaPart, btnXacNhanPart });
                dlg.Controls.AddRange(new Control[] { lblTitle, lblGoc, lblCongThuc, btnRutToanBo, btnRut1Phan, pnlRut1Phan });

                btnRutToanBo.Click += (s, ev) =>
                {
                    decimal soTienRut = soTienGui;
                    decimal laiTruocKy = soTienRut * (0.001m / 365m) * soNgayGui;
                    decimal tongNhan = soTienRut + laiTruocKy;

                    DialogResult dr = MessageBox.Show($"Rút toàn bộ số tiền gốc: {soTienGui:N0} VNĐ.\nLãi trước kỳ (0.1%/năm tính trên số tiền rút): {laiTruocKy:N0} VNĐ.\n\nTổng nhận: {tongNhan:N0} VNĐ.\nBạn có chắc chắn muốn rút toàn bộ không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        var giaoDichVuaTao = new LichSuGiaoDichTietKiem
                        {
                            SoTKTK = stk,
                            NgayGiaoDich = DateTime.Now,
                            LoaiGiaoDich = "Rút toàn bộ",
                            SoTienGoc = soTienGui,
                            SoTienLai = laiTruocKy,
                            TongTienNhan = tongNhan,
                            GhiChu = $"Rút toàn bộ trước hạn (Lãi trước kỳ 0.1%: {laiTruocKy:N0} VNĐ)"
                        };

                        if (!LuuGiaoDichRutTietKiem(giaoDichVuaTao, soTienGui, khObj)) return;

                        MessageBox.Show($"Đã rút toàn bộ tiền thành công! Số tiền gửi hiện tại của sổ {stk} đã về 0 VNĐ, bạn đã có thể thực hiện Đóng tài khoản.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                    }
                };

                btnRut1Phan.Click += (s, ev) => { pnlRut1Phan.Visible = true; };

                txtRutPart.TextChanged += (s, ev) =>
                {
                    if (decimal.TryParse(txtRutPart.Text, out decimal valPart) && valPart > 0 && valPart < soTienGui)
                    {
                        decimal conLai = soTienGui - valPart;
                        decimal laiTruocKy = valPart * (0.001m / 365m) * soNgayGui;
                        decimal tongNhanPart = valPart + laiTruocKy;

                        lblKetQuaPart.Text = $"Tiền rút: {valPart:N0} VNĐ\nSố dư còn lại: {conLai:N0} VNĐ\nLãi trước kỳ (0.1%/năm của số tiền rút): {laiTruocKy:N0} VNĐ\nTổng thực nhận: {tongNhanPart:N0} VNĐ";
                    }
                    else
                    {
                        lblKetQuaPart.Text = "Số tiền rút phải lớn hơn 0 và nhỏ hơn số tiền gốc hiện tại!";
                    }
                };

                btnXacNhanPart.Click += (s, ev) =>
                {
                    if (!decimal.TryParse(txtRutPart.Text, out decimal valPart) || valPart <= 0 || valPart >= soTienGui)
                    {
                        MessageBox.Show("Số tiền rút không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    decimal conLai = soTienGui - valPart;
                    decimal laiTruocKy = valPart * (0.001m / 365m) * soNgayGui;
                    decimal tongNhanPart = valPart + laiTruocKy;

                    var giaoDichVuaTao = new LichSuGiaoDichTietKiem
                    {
                        SoTKTK = stk,
                        NgayGiaoDich = DateTime.Now,
                        LoaiGiaoDich = "Rút 1 phần",
                        SoTienGoc = valPart,
                        SoTienLai = laiTruocKy,
                        TongTienNhan = tongNhanPart,
                        GhiChu = $"Rút {valPart:N0} VNĐ. Số dư còn lại {conLai:N0} VNĐ (Lãi trước kỳ 0.1% tính trên số tiền rút: {laiTruocKy:N0} VNĐ)."
                    };

                    if (!LuuGiaoDichRutTietKiem(giaoDichVuaTao, soTienGui, khObj)) return;

                    MessageBox.Show($"Đã rút thành công {valPart:N0} VNĐ. Lãi trước kỳ nhận từ số tiền rút: {laiTruocKy:N0} VNĐ.\nSố tiền còn lại trong sổ: {conLai:N0} VNĐ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                };

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LayDanhSachKhachHangTuFrmMain();
                    HienThiDanhSach(txtCCCD.Text.Trim());
                }
            }
            OnDataChanged();
        }

        // 4. ĐÓNG TÀI KHOẢN
        private void ThucHienDongTaiKhoan()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = dgvTietKiem.CurrentRow.Cells["colSTK"].Value?.ToString();
            object tkObj = TimSoTietKiemTheoSTK(stk, out _);

            if (tkObj == null) return;

            if (LayTrangThai(tkObj) == "Đã đóng")
            {
                MessageBox.Show("Sổ tiết kiệm này đã ở trạng thái Đã đóng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal soTienGui = LayDecimal(tkObj, "SoTienGui");
            if (soTienGui > 0)
            {
                MessageBox.Show($"Sổ tiết kiệm đang còn số tiền gửi là {soTienGui:N0} VNĐ.\n\nBạn phải thực hiện Tất toán hoặc Rút toàn bộ tiền để số tiền gửi về 0 VNĐ mới có thể thực hiện Đóng tài khoản!", "Cảnh báo dòng tiền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dr = MessageBox.Show($"Xác nhận đóng tài khoản tiết kiệm {stk}?", "Xác nhận đóng tài khoản", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            DateTime now = DateTime.Now;

            CapNhatGiaTri(tkObj, "TrangThai", FrmMain.TrangThaiTK.Dong);
            CapNhatGiaTri(tkObj, "NgayCapNhatTrangThai", now);
            CapNhatGiaTri(tkObj, "NgayCapNhatTT", now);

            GhiLichSu(new LichSuGiaoDichTietKiem
            {
                SoTKTK = stk,
                NgayGiaoDich = now,
                LoaiGiaoDich = "Đóng tài khoản",
                SoTienGoc = 0,
                SoTienLai = 0,
                TongTienNhan = 0,
                GhiChu = "Đóng tài khoản tiết kiệm (số dư gốc 0 VNĐ)"
            });

            MessageBox.Show($"Đã đóng tài khoản tiết kiệm {stk} thành công vào lúc {now:dd/MM/yyyy HH:mm:ss}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LayDanhSachKhachHangTuFrmMain();
            HienThiDanhSach(txtCCCD.Text.Trim());
            OnDataChanged();
        }

        // 5. KHÓA / MỞ TÀI KHOẢN
        private void ThucHienKhoaMoTaiKhoan()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = dgvTietKiem.CurrentRow.Cells["colSTK"].Value?.ToString();
            object tkObj = TimSoTietKiemTheoSTK(stk, out _);

            if (tkObj == null) return;

            string trangThaiHienTai = dgvTietKiem.CurrentRow.Cells["colTrangThai"].Value?.ToString() ?? LayTrangThai(tkObj);

            if (trangThaiHienTai == "Đã đóng")
            {
                MessageBox.Show("Sổ tiết kiệm đã đóng, không thể khóa/mở lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool laKhoa = (trangThaiHienTai == "Hoạt động");
            string title = laKhoa ? "Khóa Tạm Thời Tài Khoản" : "Mở Khóa Tài Khoản";

            using (Form dlg = new Form())
            {
                dlg.Text = title;
                dlg.Size = new Size(420, 220);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;

                Label lblLyDo = new Label { Text = laKhoa ? "Nhập lý do khóa tạm thời:" : "Nhập lý do mở lại tài khoản:", Location = new Point(20, 20), AutoSize = true };
                TextBox txtLyDo = new TextBox { Location = new Point(20, 50), Width = 360, Height = 50, Multiline = true };

                Button btnLuu = new Button { Text = "Xác nhận", Location = new Point(140, 120), Size = new Size(110, 35), BackColor = laKhoa ? colorOrange : colorGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(260, 120), Size = new Size(100, 35) };

                dlg.Controls.AddRange(new Control[] { lblLyDo, txtLyDo, btnLuu, btnHuy });

                btnLuu.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtLyDo.Text))
                    {
                        MessageBox.Show("Vui lòng nhập lý do!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DateTime now = DateTime.Now;

                    if (laKhoa)
                    {
                        CapNhatGiaTri(tkObj, "TrangThai", FrmMain.TrangThaiTK.Khoa);
                    }
                    else
                    {
                        CapNhatGiaTri(tkObj, "TrangThai", FrmMain.TrangThaiTK.HoatDong);
                    }

                    CapNhatGiaTri(tkObj, "NgayCapNhatTrangThai", now);
                    CapNhatGiaTri(tkObj, "NgayCapNhatTT", now);

                    GhiLichSu(new LichSuGiaoDichTietKiem
                    {
                        SoTKTK = stk,
                        NgayGiaoDich = now,
                        LoaiGiaoDich = laKhoa ? "Khóa tạm thời" : "Mở tài khoản",
                        SoTienGoc = 0,
                        SoTienLai = 0,
                        TongTienNhan = 0,
                        GhiChu = (laKhoa ? "Lý do khóa: " : "Lý do mở: ") + txtLyDo.Text.Trim()
                    });

                    MessageBox.Show($"Đã {(laKhoa ? "khóa" : "mở")} tài khoản {stk} thành công vào lúc {now:dd/MM/yyyy HH:mm:ss}!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                };

                btnHuy.Click += (s, ev) => dlg.Close();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LayDanhSachKhachHangTuFrmMain();
                    HienThiDanhSach(txtCCCD.Text.Trim());
                }
            }
            OnDataChanged();
        }

        // Đọc nghiệp vụ vay dùng chính sổ này làm tài sản thế chấp.
        // Không tạo thêm dòng giao dịch rút tiền và không sửa dữ liệu SQL.
        private List<LichSuGiaoDichTietKiem> LayLichSuVayBangSo(string soSo)
        {
            var result = new List<LichSuGiaoDichTietKiem>();
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                SELECT h.NgayGiaoDich, h.SoTKVay, h.SoTienGoc,
                       v.CCCD, k.HoTen, h.GhiChu
                FROM dbo.LichSuGiaoDichVay h
                INNER JOIN dbo.TaiKhoanVay v ON v.SoTKVay = h.SoTKVay
                LEFT JOIN dbo.KhachHang k ON k.CCCD = v.CCCD
                WHERE h.LoaiGiaoDich = N'Giải ngân'
                  AND h.GhiChu LIKE N'%Vay thế chấp sổ tiết kiệm Trong ngân hàng%'
                  AND CHARINDEX(N'Số sổ: ' + @so + N';', h.GhiChu) > 0
                ORDER BY h.NgayGiaoDich DESC", cn))
            {
                cmd.Parameters.Add("@so", SqlDbType.VarChar, 20).Value = soSo;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                    {
                        string soVay = Convert.ToString(rd["SoTKVay"]);
                        string cccd = Convert.ToString(rd["CCCD"]);
                        string ten = rd["HoTen"] == DBNull.Value ? "" : Convert.ToString(rd["HoTen"]);
                        result.Add(new LichSuGiaoDichTietKiem
                        {
                            SoTKTK = soSo,
                            NgayGiaoDich = Convert.ToDateTime(rd["NgayGiaoDich"]),
                            LoaiGiaoDich = "Vay bằng sổ tiết kiệm",
                            SoTienGoc = Convert.ToDecimal(rd["SoTienGoc"]),
                            SoTienLai = 0m,
                            TongTienNhan = Convert.ToDecimal(rd["SoTienGoc"]),
                            GhiChu = "TK vay: " + soVay + "; Người vay: " + ten +
                                     " (CCCD: " + cccd + "). Đây là giải ngân khoản vay, không phải rút tiền tiết kiệm."
                        });
                    }
            }
            return result;
        }

        private List<LichSuGiaoDichTietKiem> LayToanBoLichSuSo(string soSo)
        {
            var result = LayLichSuDeXuat(soSo);
            result.AddRange(LayLichSuVayBangSo(soSo));
            return result.OrderByDescending(x => x.NgayGiaoDich).ToList();
        }

        // 6. XEM LỊCH SỬ NHẬN TIỀN
        private void XemLichSuNhanTien()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = dgvTietKiem.CurrentRow.Cells["colSTK"].Value?.ToString();

            using (Form dlg = new Form())
            {
                dlg.Text = $"Lịch Sử Nhận Tiền & Giao Dịch - STK: {stk}";
                dlg.Size = new Size(780, 450);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;

                DataGridView dgvLichSu = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = colorBg,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    EnableHeadersVisualStyles = false
                };

                dgvLichSu.ColumnHeadersDefaultCellStyle.BackColor = colorDarkPanel;
                dgvLichSu.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvLichSu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                dgvLichSu.DefaultCellStyle.BackColor = Color.FromArgb(23, 33, 60);
                dgvLichSu.DefaultCellStyle.ForeColor = Color.White;

                dgvLichSu.Columns.Add("colNgay", "Thời gian");
                dgvLichSu.Columns.Add("colLoai", "Loại giao dịch");
                dgvLichSu.Columns.Add("colGoc", "Số tiền gốc");
                dgvLichSu.Columns.Add("colLai", "Số tiền lãi");
                dgvLichSu.Columns.Add("colTong", "Tổng thực nhận");
                dgvLichSu.Columns.Add("colGhiChu", "Ghi chú");

                List<LichSuGiaoDichTietKiem> filteredHistory;
                try { filteredHistory = LayToanBoLichSuSo(stk); }
                catch (Exception ex)
                {
                    MessageBox.Show("Không tải được lịch sử giao dịch: " + ex.Message,
                        "SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (var item in filteredHistory)
                {
                    dgvLichSu.Rows.Add(
                        item.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"),
                        item.LoaiGiaoDich,
                        LayTien(item.SoTienGoc),
                        LayTien(item.SoTienLai),
                        LayTien(item.TongTienNhan),
                        item.GhiChu
                    );
                }

                if (filteredHistory.Count == 0)
                {
                    Label lblEmpty = new Label { Text = "Chưa có lịch sử nhận tiền / rút tiền nào cho tài khoản này.", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, Height = 40, ForeColor = Color.Gray };
                    dlg.Controls.Add(lblEmpty);
                }

                dlg.Controls.Add(dgvLichSu);
                dlg.ShowDialog(this);
            }
            OnDataChanged();
        }

        // 7. CHUYỂN GIAO QUYỀN SỞ HỮU (SANG TÊN SỔ)
        private void ThucHienChuyenGiao()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = dgvTietKiem.CurrentRow.Cells["colSTK"].Value?.ToString();
            object tkObj = TimSoTietKiemTheoSTK(stk, out object khCu);

            if (tkObj == null || khCu == null) return;

            using (Form dlg = new Form())
            {
                dlg.Text = "Chuyển Giao Quyền Sở Hữu (Sang tên sổ)";
                dlg.Size = new Size(420, 280);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.BackColor = colorBg;
                dlg.ForeColor = Color.White;

                Label lblCccd = new Label { Text = "CCCD người nhận:", Location = new Point(20, 30), AutoSize = true };
                TextBox txtCccd = new TextBox { Location = new Point(160, 27), Width = 200, MaxLength = 12 };

                Label lblName = new Label { Text = "Họ Tên người nhận:", Location = new Point(20, 70), AutoSize = true };
                TextBox txtName = new TextBox { Location = new Point(160, 67), Width = 200 };

                Label lblSdt = new Label { Text = "SĐT người nhận:", Location = new Point(20, 110), AutoSize = true };
                TextBox txtSdt = new TextBox { Location = new Point(160, 107), Width = 200 };

                Button btnSangTen = new Button { Text = "Xác nhận sang tên", Location = new Point(110, 170), Size = new Size(130, 35), BackColor = colorOrange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                Button btnHuy = new Button { Text = "Hủy", Location = new Point(250, 170), Size = new Size(100, 35) };

                dlg.Controls.AddRange(new Control[] { lblCccd, txtCccd, lblName, txtName, lblSdt, txtSdt, btnSangTen, btnHuy });

                btnSangTen.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtCccd.Text) || string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show("Vui lòng nhập đầy đủ CCCD và Họ tên người nhận!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        using (var cn = new SqlConnection(_connStr))
                        {
                            cn.Open();
                            using (var tx = cn.BeginTransaction())
                            {
                                using (var cmd = new SqlCommand(@"IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang WHERE CCCD=@cccd)
                                        INSERT INTO dbo.KhachHang(CCCD,HoTen,SoDienThoai,NoTruocDo)
                                        VALUES(@cccd,@hoten,@sdt,0)", cn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@cccd", txtCccd.Text.Trim());
                                    cmd.Parameters.AddWithValue("@hoten", txtName.Text.Trim());
                                    cmd.Parameters.AddWithValue("@sdt", txtSdt.Text.Trim());
                                    cmd.ExecuteNonQuery();
                                }
                                using (var cmd = new SqlCommand("UPDATE dbo.SoTietKiem SET CCCD=@cccd WHERE SoTKTK=@stk", cn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@cccd", txtCccd.Text.Trim());
                                    cmd.Parameters.AddWithValue("@stk", stk);
                                    if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không tìm thấy sổ cần sang tên.");
                                }
                                tx.Commit();
                            }
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Sang tên thất bại: " + ex.Message); return; }

                    GhiLichSu(new LichSuGiaoDichTietKiem
                    {
                        SoTKTK = stk,
                        NgayGiaoDich = DateTime.Now,
                        LoaiGiaoDich = "Chuyển giao quyền sở hữu",
                        SoTienGoc = LayDecimal(tkObj, "SoTienGui"),
                        SoTienLai = 0,
                        TongTienNhan = 0,
                        GhiChu = $"Sang tên quyền sở hữu sổ (Số dư {LayDecimal(tkObj, "SoTienGui"):N0} VNĐ) từ ông/bà {LayString(khCu, "HoTen")} sang {txtName.Text}"
                    });

                    MessageBox.Show("Thực hiện sang tên chuyển giao quyền sở hữu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                };

                btnHuy.Click += (s, ev) => dlg.Close();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LayDanhSachKhachHangTuFrmMain();
                    HienThiDanhSach(txtCCCD.Text.Trim());
                    OnDataChanged();
                }
            }
        }

        // 8. XỬ LÝ RỦI RO: giữ số cũ để tra soát, cấp số mới duy nhất.
        private void ThucHienXuLyRuiRo()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stkCu = Convert.ToString(dgvTietKiem.CurrentRow.Cells["colSTK"].Value);
            object tkObj = TimSoTietKiemTheoSTK(stkCu, out _);
            if (tkObj == null) return;

            if (LaSoDaDuocCapLai(stkCu) || LayTrangThai(tkObj) == "Đã đóng")
            {
                MessageBox.Show("Sổ cũ đã ngừng hiệu lực hoặc đã đóng, không thể cấp lại lần nữa.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (LayTrangThai(tkObj) == "Đang khóa")
            {
                MessageBox.Show("Sổ đang khóa. Cần xử lý trạng thái khóa trước khi cấp lại.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dr = MessageBox.Show(
                $"Xác nhận báo mất/hỏng sổ {stkCu}?\n\n" +
                "Sổ cũ được lưu lại nhưng khóa vĩnh viễn tất cả chức năng. " +
                "Số dư gốc còn lại, kỳ hạn, lãi suất và đồng sở hữu sẽ được chuyển sang số sổ mới. " +
                "Lịch sử giao dịch cũ vẫn được giữ nguyên.",
                "Xác nhận cấp lại sổ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            try
            {
                string stkMoi = null;
                // Trường hợp hiếm số được sinh ra bị dùng bởi giao dịch khác,
                // cấp lại số mới và thử lại. Mỗi lần chuyển đều nằm trong một transaction.
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    stkMoi = TaoSoTKMoi();
                    try
                    {
                        CapLaiSoTietKiem(stkCu, stkMoi);
                        break;
                    }
                    catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                    {
                        if (attempt == 4) throw;
                        stkMoi = null;
                    }
                }
                if (string.IsNullOrEmpty(stkMoi))
                    throw new InvalidOperationException("Không thể cấp số mới duy nhất.");

                LayDanhSachKhachHangTuFrmMain();
                HienThiDanhSach(txtCCCD.Text.Trim());
                OnDataChanged();
                MessageBox.Show($"Cấp lại sổ thành công!\n\nSổ cũ: {stkCu} (Đã đóng - ngừng sử dụng)\nSổ mới: {stkMoi} (Hoạt động)\n\nLịch sử và thông tin sổ cũ vẫn được lưu trong SQL.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không cấp lại được sổ: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 9. XUAT BAO CAO TXT: TAT CA GIAO DICH CUA SO DANG CHON.
        // XUAT HOA DON PDF: DUY NHAT GIAO DICH GAN NHAT CUA SO DANG CHON.
        // Chi doc SQL, khong thay doi bat ky nghiep vu hay du lieu nao.
        private List<LichSuGiaoDichTietKiem> LayLichSuDeXuat(string stk)
        {
            var result = new List<LichSuGiaoDichTietKiem>();
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                    SELECT SoTKTK, NgayGiaoDich, LoaiGiaoDich, SoTienGoc,
                           SoTienLai, TongTienNhan, GhiChu
                    FROM dbo.LichSuGiaoDichTietKiem
                    WHERE LTRIM(RTRIM(SoTKTK)) = @stk
                    ORDER BY NgayGiaoDich DESC", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk.Trim();
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        result.Add(new LichSuGiaoDichTietKiem
                        {
                            SoTKTK = Convert.ToString(rd["SoTKTK"]),
                            NgayGiaoDich = Convert.ToDateTime(rd["NgayGiaoDich"]),
                            LoaiGiaoDich = Convert.ToString(rd["LoaiGiaoDich"]),
                            SoTienGoc = Convert.ToDecimal(rd["SoTienGoc"]),
                            SoTienLai = Convert.ToDecimal(rd["SoTienLai"]),
                            TongTienNhan = Convert.ToDecimal(rd["TongTienNhan"]),
                            GhiChu = rd["GhiChu"] == DBNull.Value ? "" : Convert.ToString(rd["GhiChu"])
                        });
                    }
                }
            }
            return result;
        }

        private void ThucHienXuatBaoCaoTxt()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = Convert.ToString(dgvTietKiem.CurrentRow.Cells["colSTK"].Value);
            if (string.IsNullOrWhiteSpace(stk)) return;

            try
            {
                var lichSu = LayToanBoLichSuSo(stk);
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Xuất báo cáo toàn bộ giao dịch tiết kiệm";
                    sfd.Filter = "Tệp văn bản (*.txt)|*.txt";
                    sfd.DefaultExt = "txt";
                    sfd.AddExtension = true;
                    sfd.FileName = "BaoCaoGiaoDich_STK_" + stk + ".txt";
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    var b = new System.Text.StringBuilder();
                    b.AppendLine("BAO CAO TOAN BO GIAO DICH SO TIET KIEM");
                    b.AppendLine(new string('=', 66));
                    b.AppendLine("So tiet kiem: " + stk);
                    b.AppendLine("Ngay xuat: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    b.AppendLine("Tong so giao dich: " + lichSu.Count);
                    b.AppendLine(new string('=', 66));
                    if (lichSu.Count == 0)
                        b.AppendLine("Chua co giao dich phat sinh.");
                    for (int i = 0; i < lichSu.Count; i++)
                    {
                        var gd = lichSu[i];
                        b.AppendLine();
                        b.AppendLine("GIAO DICH " + (i + 1));
                        b.AppendLine("Thoi gian: " + gd.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"));
                        b.AppendLine("Loai giao dich: " + gd.LoaiGiaoDich);
                        b.AppendLine("So tien goc: " + gd.SoTienGoc.ToString("N0") + " VND");
                        b.AppendLine("So tien lai: " + gd.SoTienLai.ToString("N0") + " VND");
                        b.AppendLine("Tong tien nhan: " + gd.TongTienNhan.ToString("N0") + " VND");
                        b.AppendLine("Ghi chu: " + gd.GhiChu);
                        b.AppendLine(new string('-', 66));
                    }
                    System.IO.File.WriteAllText(sfd.FileName, b.ToString(), new System.Text.UTF8Encoding(true));
                    MessageBox.Show("Đã xuất báo cáo TXT gồm " + lichSu.Count + " giao dịch.",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xuất được báo cáo TXT: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Chọn ngày giao dịch trước khi xuất hóa đơn PDF.
        // Nếu trong ngày có nhiều giao dịch, người dùng chọn chính xác một dòng.
        private void ThucHienXuatHoaDon()
        {
            if (dgvTietKiem.CurrentRow == null) return;
            string stk = Convert.ToString(dgvTietKiem.CurrentRow.Cells["colSTK"].Value);
            if (string.IsNullOrWhiteSpace(stk)) return;
            try
            {
                // Chỉ liệt kê giao dịch phát sinh trong FrmTietKiem.
                // Lịch sử vay vẫn được giữ ở màn hình lịch sử và báo cáo TXT.
                var history = LayLichSuDeXuat(stk)
                    .Where(x => !string.Equals(x.LoaiGiaoDich,
                        "Vay bằng sổ tiết kiệm", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (history.Count == 0)
                {
                    MessageBox.Show("Sổ này chưa có giao dịch để hiển thị.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using (var dlg = new Form())
                {
                    dlg.Text = "Chọn ngày và giao dịch xuất PDF - " + stk;
                    dlg.StartPosition = FormStartPosition.CenterParent;
                    dlg.Size = new Size(860, 490);
                    dlg.MinimumSize = new Size(720, 380);
                    dlg.BackColor = colorBg;
                    dlg.ForeColor = Color.White;
                    dlg.Font = new Font("Segoe UI", 10F);
                    var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = colorDarkPanel };
                    var label = new Label
                    {
                        Text = "Ngày giao dịch cần xuất:",
                        AutoSize = true,
                        Location = new Point(16, 23),
                        ForeColor = Color.White
                    };
                    var picker = new DateTimePicker
                    {
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "dd/MM/yyyy",
                        Location = new Point(220, 18),
                        Width = 160,
                        Value = history[0].NgayGiaoDich.Date
                    };
                    var btnAll = new Button
                    {
                        Text = "Tất cả ngày",
                        Location = new Point(398, 15),
                        Width = 135,
                        Height = 36,
                        BackColor = colorBlue,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    top.Controls.AddRange(new Control[] { label, picker, btnAll });
                    var grid = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        ReadOnly = true,
                        AllowUserToAddRows = false,
                        RowHeadersVisible = false,
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        MultiSelect = true,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        BackgroundColor = colorBg,
                        EnableHeadersVisualStyles = false,
                        AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                        RowTemplate = { Height = 34 },
                        ScrollBars = ScrollBars.Both
                    };
                    grid.ColumnHeadersVisible = true;
                    grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                    grid.ColumnHeadersHeight = 38;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = colorDarkPanel;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.DefaultCellStyle.BackColor = Color.FromArgb(23, 33, 60);
                    grid.DefaultCellStyle.ForeColor = Color.White;
                    grid.DefaultCellStyle.SelectionBackColor = colorBlue;
                    grid.Columns.Add("Ngay", "Thời gian");
                    grid.Columns.Add("Loai", "Loại giao dịch");
                    grid.Columns.Add("Tien", "Tổng thực nhận");
                    grid.Columns.Add("Note", "Ghi chú");
                    // Thanh nút luôn hiển thị bên dưới, không bị bảng che hoặc cắt
                    // khi thay đổi ngày, thu nhỏ cửa sổ hay dùng DPI cao.
                    var bottom = new Panel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 70,
                        BackColor = colorDarkPanel,
                        Padding = new Padding(12, 10, 12, 10)
                    };
                    var buttons = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.RightToLeft,
                        WrapContents = false,
                        AutoScroll = false,
                        BackColor = colorDarkPanel
                    };
                    var btnPrint = new Button
                    {
                        Text = "Xuất PDF các giao dịch theo ngày",
                        Width = 255,
                        Height = 42,
                        Margin = new Padding(6, 3, 6, 3),
                        BackColor = colorGreen,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    var btnCancel = new Button
                    {
                        Text = "Đóng",
                        Width = 105,
                        Height = 42,
                        Margin = new Padding(6, 3, 6, 3),
                        BackColor = colorDarkPanel,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        DialogResult = DialogResult.Cancel
                    };
                    buttons.Controls.Add(btnCancel);
                    buttons.Controls.Add(btnPrint);
                    bottom.Controls.Add(buttons);
                    dlg.AcceptButton = btnPrint;
                    dlg.CancelButton = btnCancel;
                    // Mặc định hiển thị toàn bộ giao dịch; chỉ lọc khi người dùng đổi ngày.
                    bool allDates = true;
                    Action refresh = () =>
                    {
                        grid.Rows.Clear();
                        foreach (var gd in history.Where(x => allDates || x.NgayGiaoDich.Date == picker.Value.Date))
                        {
                            int idx = grid.Rows.Add(gd.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"),
                                gd.LoaiGiaoDich, LayTien(gd.TongTienNhan), gd.GhiChu);
                            grid.Rows[idx].Tag = gd;
                        }
                        foreach (DataGridViewRow r in grid.Rows)
                        {
                            r.Visible = true;
                            r.Height = 34;
                        }
                        grid.ClearSelection();
                        // Mặc định chọn toàn bộ giao dịch đang hiển thị theo bộ lọc.
                        if (grid.Rows.Count > 0)
                            grid.SelectAll();
                        grid.PerformLayout();
                        grid.Invalidate();
                        btnPrint.Enabled = grid.Rows.Count > 0;
                        btnAll.BackColor = allDates ? colorGreen : colorBlue;
                        dlg.Text = "Chọn ngày và giao dịch xuất PDF - " + stk +
                            " (" + grid.Rows.Count + " giao dịch)";
                    };
                    picker.ValueChanged += (sender, args) =>
                    {
                        // Chỉ chuyển sang lọc ngày khi chính người dùng chỉnh ngày.
                        // Tránh sự kiện ValueChanged phát sinh khi form khởi tạo/lấy focus.
                        if (!picker.Focused && !picker.ContainsFocus) return;
                        allDates = false;
                        refresh();
                    };
                    // Tất cả ngày: truy vấn lại TOÀN BỘ lịch sử của đúng số sổ
                    // trực tiếp từ SQL, không dùng danh sách đã lọc/cached.
                    btnAll.Click += (sender, args) =>
                    {
                        try
                        {
                            history = LayLichSuDeXuat(stk)
                                .Where(x => !string.Equals(x.LoaiGiaoDich,
                                    "Vay bằng sổ tiết kiệm", StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(x => x.NgayGiaoDich)
                                .ToList();
                            allDates = true;
                            refresh();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Không thể tải đầy đủ lịch sử giao dịch: " + ex.Message,
                                "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    btnPrint.Click += (sender, args) =>
                    {
                        // Xuất toàn bộ các dòng được chọn trong ngày đang lọc.
                        // Khi chọn "Tất cả ngày", xuất toàn bộ dòng được chọn của sổ.
                        var selected = grid.SelectedRows.Cast<DataGridViewRow>()
                            .Where(r => r.Tag is LichSuGiaoDichTietKiem)
                            .OrderBy(r => r.Index)
                            .Select(r => (LichSuGiaoDichTietKiem)r.Tag)
                            .ToList();
                        if (selected.Count == 0)
                        {
                            MessageBox.Show("Hãy chọn ít nhất một giao dịch để xuất. " +
                                "Có thể nhấn Ctrl+A để chọn tất cả các dòng đang hiển thị.",
                                "Chưa chọn giao dịch", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        try { XuatBaoCaoPdfNhieuGiaoDichTietKiem(stk, selected); }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Không thể xuất PDF: " + ex.Message,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    // TableLayoutPanel dành vùng riêng cho bộ lọc, bảng và thanh xuất.
                    // Không dùng Dock chồng lớp: tránh che tiêu đề/các dòng đầu của bảng
                    // khi tối đa hóa cửa sổ hoặc thay đổi DPI.
                    var pdfLayout = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 1,
                        RowCount = 3,
                        Margin = Padding.Empty,
                        Padding = Padding.Empty,
                        BackColor = colorBg
                    };
                    pdfLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    pdfLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
                    pdfLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                    pdfLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
                    top.Dock = DockStyle.Fill;
                    grid.Dock = DockStyle.Fill;
                    bottom.Dock = DockStyle.Fill;
                    top.Margin = Padding.Empty;
                    grid.Margin = Padding.Empty;
                    bottom.Margin = Padding.Empty;
                    pdfLayout.Controls.Add(top, 0, 0);
                    pdfLayout.Controls.Add(grid, 0, 1);
                    pdfLayout.Controls.Add(bottom, 0, 2);
                    dlg.Controls.Add(pdfLayout);
                    refresh();
                    dlg.Shown += (sender, args) => { grid.PerformLayout(); grid.Invalidate(); };
                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải giao dịch để xuất PDF: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Lấy chủ sổ từ SQL để hóa đơn vẫn in được khi sổ đã đóng/cấp lại.
        private object LayKhachHangChoHoaDon(string stk)
        {
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"SELECT TOP 1 k.HoTen, k.CCCD, k.SoDienThoai
                    FROM dbo.SoTietKiem s INNER JOIN dbo.KhachHang k ON k.CCCD=s.CCCD
                    WHERE s.SoTKTK=@stk", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read()) return new
                    {
                        HoTen = Convert.ToString(rd["HoTen"]),
                        CCCD = Convert.ToString(rd["CCCD"]),
                        SoDienThoai = Convert.ToString(rd["SoDienThoai"])
                    };
                }
            }
            return null;
        }

        // Dấu hiệu sổ đã bị thu hồi do cấp lại. Không dùng riêng TrangThai=2
        // vì đó cũng là trạng thái đóng thông thường.
        private string TaoSoTKMoi()
        {
            Random random = new Random();
            for (int i = 0; i < 150; i++)
            {
                string candidate = "107" + random.Next(1000000, 10000000).ToString("D7");
                if (!SoTaiKhoanDaTonTai(candidate)) return candidate;
            }
            throw new InvalidOperationException("Không thể cấp số sổ không trùng trong hệ thống.");
        }

        private bool LaSoDaDuocCapLai(string stk)
        {
            using (var cn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(@"
                    SELECT CASE WHEN SoTKTKThayThe IS NOT NULL THEN 1 ELSE 0 END
                    FROM dbo.SoTietKiem WHERE SoTKTK=@stk", cn))
            {
                cmd.Parameters.Add("@stk", SqlDbType.VarChar, 20).Value = stk;
                cn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
            }
        }

        private void CapLaiSoTietKiem(string stkCu, string stkMoi)
        {
            if (string.IsNullOrWhiteSpace(stkCu) || string.IsNullOrWhiteSpace(stkMoi) ||
                string.Equals(stkCu, stkMoi, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Số sổ cũ và mới phải khác nhau và không được để trống.");

            using (var cn = new SqlConnection(_connStr))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        // Khóa sổ nguồn và kiểm tra trạng thái một lần nữa trên SQL,
                        // tránh cấp lại đồng thời cùng một sổ.
                        decimal soTienGocConLai;
                        using (var cmd = new SqlCommand(@"
                                SELECT SoTienGui
                                FROM dbo.SoTietKiem WITH (UPDLOCK, HOLDLOCK)
                                WHERE SoTKTK=@cu AND TrangThai=0 AND SoTKTKThayThe IS NULL", cn, tx))
                        {
                            cmd.Parameters.Add("@cu", SqlDbType.VarChar, 20).Value = stkCu;
                            object val = cmd.ExecuteScalar();
                            if (val == null || val == DBNull.Value)
                                throw new InvalidOperationException("Sổ cũ không hoạt động hoặc đã được cấp lại.");
                            soTienGocConLai = Convert.ToDecimal(val);
                            if (soTienGocConLai < 0)
                                throw new InvalidOperationException("Dữ liệu tiền gốc của sổ cũ không hợp lệ.");
                        }
                        // Không trùng bất kỳ số TK hiện có nào, kể cả số đã đóng.
                        using (var cmd = new SqlCommand(@"
                                IF EXISTS(SELECT 1 FROM dbo.SoTietKiem WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTK=@moi)
                                OR EXISTS(SELECT 1 FROM dbo.TaiKhoanThanhToan WITH (UPDLOCK, HOLDLOCK) WHERE SoTKTT=@moi)
                                OR EXISTS(SELECT 1 FROM dbo.TaiKhoanVay WITH (UPDLOCK, HOLDLOCK) WHERE SoTKVay=@moi)
                                    THROW 50003, N'Số sổ mới đã tồn tại trong hệ thống.', 1;", cn, tx))
                        {
                            cmd.Parameters.Add("@moi", SqlDbType.VarChar, 20).Value = stkMoi;
                            cmd.ExecuteNonQuery();
                        }

                        // Sổ mới kế thừa khoản tiền gửi chưa rút và điều kiện ban đầu.
                        using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.SoTietKiem
                                    (SoTKTK, CCCD, SoTienGui, LaiSuat, SoTienDaRut,
                                     KyHan, TrangThai, NgayMo, NgayCapNhatTT,
                                     SoTienGocBanDau, TongTienDaNhan)
                                SELECT @moi, CCCD, SoTienGui, LaiSuat, 0,
                                       KyHan, 0, NgayMo, GETDATE(), SoTienGui, 0
                                FROM dbo.SoTietKiem WHERE SoTKTK=@cu;", cn, tx))
                        {
                            cmd.Parameters.Add("@moi", SqlDbType.VarChar, 20).Value = stkMoi;
                            cmd.Parameters.Add("@cu", SqlDbType.VarChar, 20).Value = stkCu;
                            if (cmd.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("Không thể tạo sổ mới.");
                        }
                        // Bảo lưu đồng sở hữu cho sổ mới; bảng sổ cũ không bị xóa.
                        using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.DongSoHuuSoTietKiem(SoTKTK, CCCD, HoTen, SoDienThoai)
                                SELECT @moi, CCCD, HoTen, SoDienThoai
                                FROM dbo.DongSoHuuSoTietKiem WHERE SoTKTK=@cu;", cn, tx))
                        {
                            cmd.Parameters.Add("@moi", SqlDbType.VarChar, 20).Value = stkMoi;
                            cmd.Parameters.Add("@cu", SqlDbType.VarChar, 20).Value = stkCu;
                            cmd.ExecuteNonQuery();
                        }
                        // Ghi nhận cấp lại trên cả hai sổ, không sửa khóa ngoại lịch sử cũ.
                        using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.LichSuGiaoDichTietKiem
                                    (SoTKTK, NgayGiaoDich, LoaiGiaoDich, SoTienGoc,
                                     SoTienLai, TongTienNhan, GhiChu, NguoiThucHien)
                                VALUES
                                    (@cu, GETDATE(), N'Hủy sổ - Cấp lại', 0, 0, 0,
                                     N'Sổ cũ bị vô hiệu hóa; cấp số mới: ' + @moi, @nguoi),
                                    (@moi, GETDATE(), N'Cấp lại sổ (Rủi ro)', @sodu, 0, 0,
                                     N'Nhận chuyển tiếp tiền gốc từ sổ ' + @cu + N'; không phát sinh thu/chi tiền.', @nguoi);", cn, tx))
                        {
                            cmd.Parameters.Add("@nguoi", SqlDbType.NVarChar, 100).Value = LayTenNguoiThucHien();
                            cmd.Parameters.Add("@cu", SqlDbType.VarChar, 20).Value = stkCu;
                            cmd.Parameters.Add("@moi", SqlDbType.VarChar, 20).Value = stkMoi;
                            var amount = cmd.Parameters.Add("@sodu", SqlDbType.Decimal);
                            amount.Precision = 18; amount.Scale = 0; amount.Value = soTienGocConLai;
                            cmd.ExecuteNonQuery();
                        }
                        // Ghi marker cấp lại và làm số dư khả dụng sổ cũ về 0.
                        // KHÔNG DELETE sổ cũ, KHÔNG chuyển lịch sử cũ sang sổ mới.
                        using (var cmd = new SqlCommand(@"
                                UPDATE dbo.SoTietKiem
                                SET SoTienGui = 0,
                                    TrangThai = 2,
                                    NgayCapNhatTT = GETDATE(),
                                    SoTKTKThayThe = @moi,
                                    NgayCapLai = GETDATE()
                                WHERE SoTKTK = @cu AND TrangThai = 0 AND SoTKTKThayThe IS NULL;", cn, tx))
                        {
                            cmd.Parameters.Add("@cu", SqlDbType.VarChar, 20).Value = stkCu;
                            cmd.Parameters.Add("@moi", SqlDbType.VarChar, 20).Value = stkMoi;
                            if (cmd.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("Không thể vô hiệu hóa sổ cũ.");
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

        // CÁC HÀM TIỆN ÍCH DÙNG CHUNG REFLECTION
        private object TimSoTietKiemTheoSTK(string stk, out object khachHangChuaTK)
        {
            khachHangChuaTK = null;
            if (string.IsNullOrEmpty(stk) || danhSachKhachHang == null) return null;

            foreach (object kh in danhSachKhachHang)
            {
                IList dsTK = LayDanhSach(kh, "DanhSachTietKiem");
                foreach (object tk in dsTK)
                {
                    if (LayString(tk, "SoTKTK") == stk)
                    {
                        khachHangChuaTK = kh;
                        return tk;
                    }
                }
            }
            return null;
        }

        private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _cacheThuocTinhTietKiem = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private object LayGiaTri(object obj, string propertyName)
        {
            if (obj == null) return null;
            Type type = obj.GetType();
            Dictionary<string, PropertyInfo> properties;
            if (!_cacheThuocTinhTietKiem.TryGetValue(type, out properties))
            {
                properties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
                _cacheThuocTinhTietKiem.Add(type, properties);
            }
            PropertyInfo property;
            if (!properties.TryGetValue(propertyName, out property))
            {
                property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                properties.Add(propertyName, property);
            }
            if (property == null) return null;
            try { return property.GetValue(obj, null); } catch { return null; }
        }

        private void CapNhatGiaTri(object obj, string propertyName, object val)
        {
            if (obj == null) return;
            if (propertyName == "NgayCapNhatTrangThai") return; // SoTietKiem chỉ có NgayCapNhatTT
            string[] allowed = { "SoTienGui", "SoTienDaRut", "TrangThai", "NgayCapNhatTT" };
            if (!allowed.Contains(propertyName)) throw new InvalidOperationException("Không hỗ trợ cập nhật trường: " + propertyName);
            string stk = LayString(obj, "SoTKTK");
            object sqlVal = val is Enum ? (object)Convert.ToInt32(val) : val;
            ThucThi("UPDATE dbo.SoTietKiem SET " + propertyName + "=@val WHERE SoTKTK=@stk", P("@val", sqlVal), P("@stk", stk));
            PropertyInfo prop = obj.GetType().GetProperty(propertyName);
            if (prop != null) prop.SetValue(obj, val, null);
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
            return $"{lai:0.##}%/năm";
        }

        private int ParseKyHanToMonths(string kyHanStr)
        {
            if (string.IsNullOrEmpty(kyHanStr)) return 12;
            string clean = new string(kyHanStr.Where(char.IsDigit).ToArray());
            if (int.TryParse(clean, out int months)) return months;
            return 12;
        }

        private string LayNgayLam(object taiKhoan)
        {
            object value = LayGiaTri(taiKhoan, "NgayMo") ?? LayGiaTri(taiKhoan, "NgayLam");
            if (value is DateTime date) return date.ToString("dd/MM/yyyy HH:mm:ss");
            if (value != null && DateTime.TryParse(value.ToString(), out DateTime parsed)) return parsed.ToString("dd/MM/yyyy HH:mm:ss");
            return "-";
        }

        private string LayNgayCapNhat(object taiKhoan)
        {
            string trangThai = LayTrangThai(taiKhoan);
            if (trangThai == "Hoạt động") return "-";

            object value = LayGiaTri(taiKhoan, "NgayCapNhatTT") ?? LayGiaTri(taiKhoan, "NgayCapNhatTrangThai");
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
            if (text == "Khoa" || text == "Đang khóa" || text.EndsWith(".Khoa")) return "Đang khóa";
            if (text == "Dong" || text == "Đã đóng" || text.EndsWith(".Dong")) return "Đã đóng";
            return "Hoạt động";
        }
        // Báo cáo nhiều giao dịch, độc lập với hóa đơn một giao dịch hiện có.
        // Dùng HTML in ra PDF như cách xuất PDF cũ; không sửa mẫu hóa đơn riêng.
        private void XuatBaoCaoPdfNhieuGiaoDichTietKiem(
            string soSo, List<LichSuGiaoDichTietKiem> giaoDich)
        {
            if (giaoDich == null || giaoDich.Count == 0) return;
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Lưu báo cáo giao dịch tiết kiệm để in PDF";
                sfd.Filter = "Trang HTML (*.html)|*.html";
                sfd.FileName = "GiaoDichTietKiem_" + soSo + "_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                Func<string, string> enc = x => System.Net.WebUtility.HtmlEncode(x ?? "");
                var html = new System.Text.StringBuilder();
                html.Append(@"<!doctype html><html lang='vi'><head><meta charset='utf-8'>
<title>Báo cáo giao dịch tiết kiệm</title><style>
@page{size:A4 landscape;margin:13mm}
body{font-family:'Segoe UI',Arial,sans-serif;color:#17213a;margin:25px}
h1{text-align:center;color:#1d4ed8;font-size:21px}
.sub{text-align:center;color:#64748b;margin-bottom:22px}
table{border-collapse:collapse;width:100%;font-size:12px}
th,td{border:1px solid #cbd5e1;padding:9px;text-align:left;vertical-align:top}
th{background:#eff6ff;color:#1e40af}
.money{text-align:right;white-space:nowrap}
.total{font-weight:bold;background:#f1f5f9}
.actions{text-align:center;margin:18px}
button{background:#2563eb;color:white;border:0;border-radius:6px;padding:12px 22px;cursor:pointer}
@media print{.actions{display:none}body{margin:0}}
</style></head><body><div class='actions'><button onclick='window.print()'>In / Lưu PDF</button></div>
<h1>BÁO CÁO GIAO DỊCH SỔ TIẾT KIỆM</h1>");
                html.Append("<div class='sub'>Số sổ: <b>").Append(enc(soSo))
                    .Append("</b> | Số giao dịch: ").Append(giaoDich.Count)
                    .Append(" | Thời gian: ")
                    .Append(giaoDich.Min(x => x.NgayGiaoDich).ToString("dd/MM/yyyy"))
                    .Append(" - ")
                    .Append(giaoDich.Max(x => x.NgayGiaoDich).ToString("dd/MM/yyyy"))
                    .Append("</div><table><thead><tr><th>STT</th><th>Ngày giao dịch</th>")
                    .Append("<th>Loại giao dịch</th><th>Tiền gốc</th><th>Tiền lãi</th>")
                    .Append("<th>Tổng thực nhận</th><th>Ghi chú</th></tr></thead><tbody>");
                for (int i = 0; i < giaoDich.Count; i++)
                {
                    var gd = giaoDich[i];
                    html.Append("<tr><td>").Append(i + 1).Append("</td><td>")
                        .Append(gd.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss"))
                        .Append("</td><td>").Append(enc(gd.LoaiGiaoDich))
                        .Append("</td><td class='money'>").Append(gd.SoTienGoc.ToString("N0"))
                        .Append("</td><td class='money'>").Append(gd.SoTienLai.ToString("N0"))
                        .Append("</td><td class='money'>").Append(gd.TongTienNhan.ToString("N0"))
                        .Append("</td><td>").Append(enc(gd.GhiChu)).Append("</td></tr>");
                }
                html.Append("<tr class='total'><td colspan='5'>TỔNG THỰC NHẬN</td><td class='money'>")
                    .Append(giaoDich.Sum(x => x.TongTienNhan).ToString("N0"))
                    .Append(" VNĐ</td><td></td></tr></tbody></table></body></html>");
                File.WriteAllText(sfd.FileName, html.ToString(),
                    new System.Text.UTF8Encoding(true));
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName)
                    { UseShellExecute = true });
            }
        }

        // Hóa đơn chỉ của giao dịch vừa ghi, không lấy toàn bộ lịch sử.
        // Mở HTML trên trình duyệt; Ctrl+P -> Save as PDF.
        private void MoiXuatHoaDonGiaoDichTietKiem(LichSuGiaoDichTietKiem gd, object khObj)
        {
            if (gd == null) return;
            if (MessageBox.Show("Giao dịch thành công. Bạn có muốn xuất hóa đơn của giao dịch này không?",
                "Xuất hóa đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "Lưu hóa đơn giao dịch tiết kiệm";
                    sfd.Filter = "Trang HTML (*.html)|*.html";
                    sfd.FileName = "HoaDon_" + gd.SoTKTK + "_" + gd.NgayGiaoDich.ToString("yyyyMMdd_HHmmss") + ".html";
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    Func<string, string> encode = value => System.Net.WebUtility.HtmlEncode(value ?? "");
                    string ten = encode(LayString(khObj, "HoTen"));
                    string cccd = encode(LayString(khObj, "CCCD"));
                    string sdt = encode(LayString(khObj, "SoDienThoai"));
                    string loai = encode(gd.LoaiGiaoDich);
                    string stk = encode(gd.SoTKTK);
                    string ghiChu = encode(gd.GhiChu);
                    string html = @"<!doctype html><html lang='vi'><head><meta charset='utf-8'><title>Hóa đơn giao dịch tiết kiệm</title>
<style>
@page{size:A4;margin:15mm}*{box-sizing:border-box;-webkit-print-color-adjust:exact;print-color-adjust:exact}
body{font-family:'Segoe UI',Arial,sans-serif;background:#eef2f7;color:#17213a;margin:0}
.wrap{max-width:790px;margin:30px auto;padding:38px;background:white;border:1px solid #dbe3ee;border-radius:12px;box-shadow:0 8px 24px #0001}
.header{display:flex;justify-content:space-between;align-items:center;border-bottom:3px solid #2563eb;padding-bottom:16px}
.brand{font-weight:800;font-size:18px;color:#1e3a8a}.muted{color:#64748b;font-size:12px}
h1{text-align:center;font-size:21px;margin:28px 0 8px}.sub{text-align:center;color:#64748b;margin-bottom:26px}
.grid{display:grid;grid-template-columns:1fr 1fr;gap:16px}.panel{background:#f8fafc;border:1px solid #e2e8f0;padding:16px;border-radius:8px}
h3{font-size:13px;color:#2563eb;margin:0 0 13px;border-bottom:1px solid #cbd5e1;padding-bottom:8px}
.line{display:flex;justify-content:space-between;gap:12px;margin:9px 0;font-size:13px}.line b{text-align:right}
.summary{margin-top:20px;border-collapse:collapse;width:100%}.summary td{padding:12px;border-bottom:1px solid #e2e8f0}
.summary td:last-child{text-align:right;font-weight:600}.total{background:#eff6ff;color:#1d4ed8;font-size:18px;font-weight:bold}
.note{font-size:12px;margin-top:20px;color:#475569}.sign{display:flex;justify-content:space-around;text-align:center;margin-top:44px;font-size:13px}.sign div{width:45%;height:100px}
.actions{text-align:center;margin:22px}.actions button{background:#2563eb;color:white;border:0;padding:12px 22px;border-radius:7px;cursor:pointer}
@media print{body{background:white}.wrap{box-shadow:none;border:none;margin:0;padding:0;max-width:none}.actions{display:none}}
</style></head><body><div class='actions'><button onclick='window.print()'>In / Lưu PDF</button></div><main class='wrap'>
<div class='header'><div><div class='brand'>NGÂN HÀNG QLTKNH</div><div class='muted'>Hệ thống quản lý sổ tiết kiệm</div></div><div class='muted'>CHỨNG TỪ GIAO DỊCH</div></div>
<h1>HÓA ĐƠN GIAO DỊCH TIẾT KIỆM</h1><div class='sub'>Chỉ ghi nhận một giao dịch đã thực hiện</div>
<div class='grid'><section class='panel'><h3>THÔNG TIN KHÁCH HÀNG</h3>
<div class='line'><span>Họ tên</span><b>" + ten + @"</b></div><div class='line'><span>CCCD</span><b>" + cccd + @"</b></div><div class='line'><span>Điện thoại</span><b>" + sdt + @"</b></div></section>
<section class='panel'><h3>THÔNG TIN GIAO DỊCH</h3>
<div class='line'><span>Số sổ</span><b>" + stk + @"</b></div><div class='line'><span>Loại giao dịch</span><b>" + loai + @"</b></div><div class='line'><span>Thời gian</span><b>" + gd.NgayGiaoDich.ToString("dd/MM/yyyy HH:mm:ss") + @"</b></div></section></div>
<table class='summary'><tr><td>Tiền gốc</td><td>" + gd.SoTienGoc.ToString("N0") + @" VNĐ</td></tr>
<tr><td>Tiền lãi</td><td>" + gd.SoTienLai.ToString("N0") + @" VNĐ</td></tr>
<tr class='total'><td>TỔNG TIỀN THỰC NHẬN</td><td>" + gd.TongTienNhan.ToString("N0") + @" VNĐ</td></tr></table>
<div class='note'><b>Ghi chú:</b> " + ghiChu + @"</div>
<div class='sign'><div><b>KHÁCH HÀNG</b><p class='muted'>(Ký và ghi rõ họ tên)</p></div><div><b>GIAO DỊCH VIÊN</b><p class='muted'>(Ký và ghi rõ họ tên)</p></div></div>
</main></body></html>";
                    File.WriteAllText(sfd.FileName, html, new System.Text.UTF8Encoding(true));
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giao dịch đã được ghi nhận, nhưng không thể xuất hóa đơn: " + ex.Message,
                    "Lỗi xuất hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        // Giao diện chi tiết tích hợp vào FrmTietKiem.cs để không cần file Designer bổ sung.
        // Bốn ô nằm trên cùng một hàng:
        // Tổng tiền gửi ban đầu | Tổng tiền đã nhận | Gốc còn lại | Trạng thái sổ.
        private TableLayoutPanel TaoBangTheThongKeChiTiet()
        {
            var cards = new TableLayoutPanel
            {
                Name = "tblThongKeChiTietSo",
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0, 0, 0, 9),
                BackColor = Color.FromArgb(15, 23, 42),
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };

            for (int i = 0; i < 4; i++)
                cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            return cards;
        }

        private Panel TaoTheThongKeChiTiet(string tieuDe, string giaTri,
            string moTa, Color mauGiaTri)
        {
            Color nen = Color.FromArgb(23, 33, 54);
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = nen,
                Margin = new Padding(0, 2, 12, 2),
                Padding = new Padding(14, 10, 8, 7)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = nen,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var lblTieuDe = new Label
            {
                Text = tieuDe,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(166, 182, 207),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            var lblGiaTri = new Label
            {
                Text = giaTri,
                Dock = DockStyle.Fill,
                ForeColor = mauGiaTri,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            var lblMoTa = new Label
            {
                Text = moTa,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            layout.Controls.Add(lblTieuDe, 0, 0);
            layout.Controls.Add(lblGiaTri, 0, 1);
            layout.Controls.Add(lblMoTa, 0, 2);
            panel.Controls.Add(layout);
            return panel;
        }

    }
}