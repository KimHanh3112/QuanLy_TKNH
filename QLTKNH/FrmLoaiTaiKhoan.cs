using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QLTKNH
{
    public partial class FrmLoaiTaiKhoan : Form
    {
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

        #region 1. Data Models & Danh Sách Dữ Liệu
        public class DanhMucLoaiTKItem
        {
            public string MaLoaiTK { get; set; }
            public string TenLoaiTK { get; set; }
            public override string ToString() => $"{TenLoaiTK} ({MaLoaiTK})";
        }

        private static readonly List<DanhMucLoaiTKItem> dsDanhMucCoSan = new List<DanhMucLoaiTKItem>();
        private static List<LoaiTaiKhoanModel> dsLoaiTK = new List<LoaiTaiKhoanModel>();
        private readonly string _connStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";

        private void TaiDuLieuLoaiTK()
        {
            var danhMucMoi = new List<DanhMucLoaiTKItem>();
            var cauHinhMoi = new List<LoaiTaiKhoanModel>();
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT MaLoaiTK, TenLoaiTK FROM dbo.DanhMucLoaiTaiKhoan ORDER BY MaLoaiTK", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) danhMucMoi.Add(new DanhMucLoaiTKItem { MaLoaiTK = r.GetString(0), TenLoaiTK = r.GetString(1) });
                using (var cmd = new SqlCommand(@"SELECT c.Id, c.MaLoaiTK, d.TenLoaiTK, c.Thang, c.LaiSuat, c.MoTa
					FROM dbo.CauHinhLoaiTaiKhoan c JOIN dbo.DanhMucLoaiTaiKhoan d ON d.MaLoaiTK=c.MaLoaiTK
					ORDER BY c.MaLoaiTK, c.Thang", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) cauHinhMoi.Add(new LoaiTaiKhoanModel
                    {
                        Id = r.GetInt32(0),
                        MaLoaiTK = r.GetString(1),
                        TenLoaiTK = r.GetString(2),
                        Thang = r.GetInt32(3),
                        LaiSuat = r.GetDecimal(4),
                        MoTa = r.IsDBNull(5) ? "" : r.GetString(5)
                    });
            }
            dsDanhMucCoSan.Clear();
            dsDanhMucCoSan.AddRange(danhMucMoi);
            dsLoaiTK = cauHinhMoi;
        }

        private void ThucThiSql(string sql, Action<SqlParameterCollection> ganThamSo)
        {
            QuyenHoSo.YeuCauGiamDoc();
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                ganThamSo(cmd.Parameters);
                conn.Open();
                if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("Không tìm thấy bản ghi cần thao tác.");
            }
        }

        private void NapLaiSauThayDoi()
        {
            TaiDuLieuLoaiTK();
            NapDataComboBoxDanhMuc();
            HienThiDanhSachLoaiTK();
        }

        private IList danhSachKhachHang;
        private LoaiTaiKhoanModel selectedModelDangChon = null;
        private bool dangDongBoBoLoc = false;
        private bool _dangDungBangLoai;
        private LoaiTaiKhoanModel _cauHinhVuaTai;
        private long _lanTaiChiTiet;
        #endregion

        #region 2. Khởi Tạo Form & Đăng Ký Sự Kiện

        // Vùng tìm kiếm được bao bằng GroupBox, không thay đổi Designer.
        private GroupBox _searchGroup;
        private bool _searchRefreshing;

        private void SetupSearchGroup()
        {
            Control search = cboDanhMucLoaiTK;
            Control action = btnTim;
            if (search == null || action == null || search.Parent == null || search.Parent != action.Parent) return;
            Control host = search.Parent;
            int left = Math.Min(search.Left, action.Left) - 12;
            int top = Math.Min(search.Top, action.Top) - 22;
            int right = Math.Max(search.Right, action.Right) + 12;
            int bottom = Math.Max(search.Bottom, action.Bottom) + 12;
            _searchGroup = new GroupBox
            {
                Name = "grpTimKiemTuLamMoi",
                Location = new Point(Math.Max(0, left), Math.Max(0, top)),
                Size = new Size(right - Math.Max(0, left), bottom - Math.Max(0, top)),
                ForeColor = search.ForeColor,
                BackColor = host.BackColor,
                Cursor = Cursors.Hand,
                Anchor = search.Anchor
            };
            host.Controls.Add(_searchGroup);
            _searchGroup.SendToBack();
            _searchGroup.Click += (sender, args) => RefreshFromSearchGroup();
            // Nhấn vào vùng trống của khung để làm mới; các ô nhập vẫn thao tác bình thường.
        }

        private void RefreshFromSearchGroup()
        {
            if (_searchRefreshing) return;
            _searchRefreshing = true;
            try
            {
                if (cboDanhMucLoaiTK.Items.Count > 0) cboDanhMucLoaiTK.SelectedIndex = 0;
                NapLaiSauThayDoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể làm mới dữ liệu: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _searchRefreshing = false; }
        }

        // Chỉ bố trí lại các control có sẵn; không tạo sự kiện nghiệp vụ mới.
        private void ThietKeBankingControlCenter()
        {
            var bg = System.Drawing.Color.FromArgb(15, 23, 42);
            var card = System.Drawing.Color.FromArgb(30, 41, 59);
            var muted = System.Drawing.Color.FromArgb(148, 163, 184);
            var blue = System.Drawing.Color.FromArgb(59, 130, 246);
            this.SuspendLayout();
            this.BackColor = bg;
            this.MinimumSize = new System.Drawing.Size(1050, 680);
            this.AutoScroll = false;

            // Di chuyển control thay vì tạo bản sao, giữ toàn bộ event trong FrmLoaiTaiKhoan.cs.
            this.spcTop.Panel1.Controls.Remove(this.grpInput);
            this.spcTop.Panel2.Controls.Remove(this.grpLoaiTK);
            this.spcMain.Panel2.Controls.Remove(this.grpChiTiet);
            this.Controls.Remove(this.spcMain);
            this.Controls.Remove(this.pnlHeader);

            var root = new System.Windows.Forms.TableLayoutPanel();
            root.Name = "layoutBankingControlCenter";
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.BackColor = bg;
            root.Padding = new System.Windows.Forms.Padding(16, 10, 16, 14);
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55));

            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.BackColor = bg;
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
            this.lblTitle.Text = "ACCOUNT CONTROL CENTER";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(8, 4);
            this.lblTitle.AutoSize = true;
            var subtitle = new System.Windows.Forms.Label();
            subtitle.Text = "QUẢN LÝ DANH MỤC & CẤU HÌNH LOẠI TÀI KHOẢN";
            subtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            subtitle.ForeColor = muted;
            subtitle.AutoSize = true;
            subtitle.Location = new System.Drawing.Point(11, 40);
            this.pnlHeader.Controls.Add(subtitle);
            root.Controls.Add(this.pnlHeader, 0, 0);

            var workspace = new System.Windows.Forms.TableLayoutPanel();
            workspace.Dock = System.Windows.Forms.DockStyle.Fill;
            workspace.ColumnCount = 2;
            workspace.RowCount = 1;
            workspace.Margin = new System.Windows.Forms.Padding(0);
            workspace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34));
            workspace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 66));
            this.grpInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLoaiTK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpInput.Text = "  BỘ ĐIỀU KHIỂN  ";
            this.grpLoaiTK.Text = "  DANH SÁCH CẤU HÌNH  ";
            this.grpInput.Margin = new System.Windows.Forms.Padding(5, 3, 7, 5);
            this.grpLoaiTK.Margin = new System.Windows.Forms.Padding(5, 3, 5, 5);
            this.grpInput.BackColor = card;
            this.grpLoaiTK.BackColor = card;
            workspace.Controls.Add(this.grpInput, 0, 0);
            workspace.Controls.Add(this.grpLoaiTK, 1, 0);
            root.Controls.Add(workspace, 0, 1);

            // Bố trí bộ lọc trong card trái, tự căn theo chiều rộng card.
            this.lblDanhMucLoai.Location = new System.Drawing.Point(20, 43);
            this.cboDanhMucLoaiTK.Location = new System.Drawing.Point(20, 68);
            this.cboDanhMucLoaiTK.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                            System.Windows.Forms.AnchorStyles.Left |
                                            System.Windows.Forms.AnchorStyles.Right;
            this.cboDanhMucLoaiTK.Width = this.grpInput.ClientSize.Width - 42;
            this.lblThang.Location = new System.Drawing.Point(20, 113);
            this.nudThang.Location = new System.Drawing.Point(20, 139);
            this.nudThang.Width = 160;
            this.btnTim.Location = new System.Drawing.Point(20, 186);
            this.btnTim.Size = new System.Drawing.Size(180, 38);
            this.btnTim.Text = "Tìm kiếm cấu hình";
            this.btnTim.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnLamMoi.Visible = false;
            this.grpInput.Resize += (sender, e) =>
            {
                int width = System.Math.Max(140, this.grpInput.ClientSize.Width - 42);
                this.cboDanhMucLoaiTK.Width = width;
                this.btnTim.Width = width;
            };

            this.grpChiTiet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpChiTiet.Margin = new System.Windows.Forms.Padding(5, 5, 5, 0);
            this.grpChiTiet.Text = "  DANH SÁCH TÀI KHOẢN LIÊN QUAN  ";
            this.grpChiTiet.BackColor = card;
            this.pnlFilterBar.BackColor = card;
            this.lblTongSoLuong.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                         System.Windows.Forms.AnchorStyles.Right;
            this.pnlFilterBar.Resize += (sender, e) =>
            {
                this.lblTongSoLuong.Left = System.Math.Max(
                    this.lblChiTietHeader.Right + 15,
                    this.pnlFilterBar.ClientSize.Width - this.lblTongSoLuong.Width - 20);
            };
            root.Controls.Add(this.grpChiTiet, 0, 2);

            foreach (var grid in new[] { this.dgvLoaiTK, this.dgvChiTietTK })
            {
                grid.RowHeadersVisible = false;
                grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
                grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
                grid.ColumnHeadersHeightSizeMode =
                    System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                grid.ColumnHeadersHeight = 38;
                grid.RowTemplate.Height = 32;
                grid.BackgroundColor = bg;
            }
            this.Controls.Add(root);
            this.ResumeLayout(true);
        }


        public FrmLoaiTaiKhoan()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || this.DesignMode)
                return;

            ThietKeBankingControlCenter();
            GanPhanCapCaNhan(dgvChiTietTK, 2, 7, _fontPhanCapCaNhan);
            Disposed += (sender, args) => _fontPhanCapCaNhan.Dispose();
            foreach (var grid in new[] { dgvLoaiTK, dgvChiTietTK })
            {
                try { typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(grid, true, null); }
                catch (Exception) { }
            }
            this.Load += (s, e) => { if (!QuyenHoSo.LaGiamDoc()) { tsmiXoa.Visible = false; cmsLoaiTK.Enabled = false; } };

            this.Load += FrmLoaiTaiKhoan_Load;

            cboDanhMucLoaiTK.SelectedIndexChanged += CboDanhMucLoaiTK_SelectedIndexChanged;

            btnTim.Click += BtnTim_Click;
            // Bỏ nút Làm mới khỏi giao diện; thay bằng thao tác nhấn vào GroupBox.
            btnLamMoi.Visible = false;
            SetupSearchGroup();

            // Bảng 1: Loại tài khoản
            dgvLoaiTK.CellClick += DgvLoaiTK_CellClick;
            dgvLoaiTK.CellDoubleClick += DgvLoaiTK_CellDoubleClick;
            dgvLoaiTK.SelectionChanged += DgvLoaiTK_SelectionChanged;
            dgvLoaiTK.MouseDown += DgvLoaiTK_MouseDown;

            // Bảng 2: Chi tiết tài khoản khách hàng - Double click chuyển Form
            dgvChiTietTK.CellDoubleClick += DgvChiTietTK_CellDoubleClick;

            tsmiXoa.Click += ThucHienXoa_Click;
        }

        private void FrmLoaiTaiKhoan_Load(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            try { NapLaiSauThayDoi(); }
            catch (Exception ex) { MessageBox.Show("Không thể tải loại tài khoản từ SQL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void CboDanhMucLoaiTK_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dangDongBoBoLoc) return;
            if (cboDanhMucLoaiTK.SelectedItem is DanhMucLoaiTKItem selectedDM)
            {
                if (selectedDM.MaLoaiTK == "TK_THANHTOAN")
                {
                    nudThang.Value = 0;
                    nudThang.Enabled = false;
                }
                else
                {
                    nudThang.Enabled = true;
                    if (nudThang.Value == 0) nudThang.Value = 6;
                }
            }
        }
        #endregion

        #region 3. Nạp Dữ Liệu & Hiển Thị Bảng
        private void NapDataComboBoxDanhMuc()
        {
            cboDanhMucLoaiTK.DataSource = null;
            cboDanhMucLoaiTK.DataSource = dsDanhMucCoSan;
            cboDanhMucLoaiTK.DisplayMember = "TenLoaiTK";
            cboDanhMucLoaiTK.ValueMember = "MaLoaiTK";
        }

        private void HienThiDanhSachLoaiTK(List<LoaiTaiKhoanModel> listHienThi = null, LoaiTaiKhoanModel itemHighlight = null)
        {
            dgvLoaiTK.SelectionChanged -= DgvLoaiTK_SelectionChanged;
            _dangDungBangLoai = true;
            dgvLoaiTK.SuspendLayout();
            try
            {
                dgvLoaiTK.Rows.Clear();

                var listData = listHienThi ?? dsLoaiTK;
                int targetIndex = -1;

                for (int i = 0; i < listData.Count; i++)
                {
                    var item = listData[i];
                    string strThang = item.Thang > 0 ? $"{item.Thang} tháng" : "Không kỳ hạn";
                    int idx = dgvLoaiTK.Rows.Add(item.MaLoaiTK, item.TenLoaiTK, strThang, $"{item.LaiSuat:0.##}%", item.MoTa);
                    dgvLoaiTK.Rows[idx].Tag = item;

                    if (itemHighlight != null && item == itemHighlight)
                    {
                        targetIndex = idx;
                    }
                }

                _cauHinhVuaTai = null;

                if (dgvLoaiTK.Rows.Count > 0)
                {
                    int selIdx = targetIndex >= 0 ? targetIndex : 0;
                    dgvLoaiTK.ClearSelection();
                    dgvLoaiTK.Rows[selIdx].Selected = true;
                    HienThiLenControls(selIdx);
                }
                else
                {
                    selectedModelDangChon = null;
                    dgvChiTietTK.Rows.Clear();
                    lblTongSoLuong.Text = "Tổng số dòng: 0";
                }
            }
            finally
            {
                _dangDungBangLoai = false;
                dgvLoaiTK.SelectionChanged += DgvLoaiTK_SelectionChanged;
                dgvLoaiTK.ResumeLayout();
            }
        }

        private void DgvLoaiTK_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            HienThiLenControls(e.RowIndex);
        }

        private void DgvLoaiTK_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvLoaiTK.Rows.Count &&
                !dgvLoaiTK.Rows[e.RowIndex].IsNewRow && QuyenHoSo.LaGiamDoc())
            {
                dgvLoaiTK.ClearSelection();
                if (e.ColumnIndex >= 0) dgvLoaiTK.CurrentCell = dgvLoaiTK.Rows[e.RowIndex].Cells[e.ColumnIndex];
                dgvLoaiTK.Rows[e.RowIndex].Selected = true;
                ThucHienCapNhat();
            }
        }

        private void DgvLoaiTK_SelectionChanged(object sender, EventArgs e)
        {
            if (!_dangDungBangLoai && dgvLoaiTK.SelectedRows.Count > 0)
            {
                int idx = dgvLoaiTK.SelectedRows[0].Index;
                HienThiLenControls(idx);
            }
        }

        private void DgvLoaiTK_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && QuyenHoSo.LaGiamDoc())
            {
                var hit = dgvLoaiTK.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    dgvLoaiTK.ClearSelection();
                    if (hit.ColumnIndex >= 0) dgvLoaiTK.CurrentCell = dgvLoaiTK.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
                    dgvLoaiTK.Rows[hit.RowIndex].Selected = true;
                    HienThiLenControls(hit.RowIndex);
                    cmsLoaiTK.Show(dgvLoaiTK, e.Location);
                }
            }
        }

        private void HienThiLenControls(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvLoaiTK.Rows.Count) return;
            selectedModelDangChon = dgvLoaiTK.Rows[rowIndex].Tag as LoaiTaiKhoanModel;
            if (selectedModelDangChon == null) return;

            dangDongBoBoLoc = true;
            try
            {
                cboDanhMucLoaiTK.SelectedValue = selectedModelDangChon.MaLoaiTK;
                nudThang.Value = Math.Min(nudThang.Maximum, Math.Max(nudThang.Minimum, selectedModelDangChon.Thang));
                nudThang.Enabled = selectedModelDangChon.MaLoaiTK != "TK_THANHTOAN";
            }
            finally { dangDongBoBoLoc = false; }

            // CellClick, SelectionChanged va MouseDown co the cung goi cho mot dong.
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            if (ReferenceEquals(_cauHinhVuaTai, selectedModelDangChon) &&
                (now - _lanTaiChiTiet) < System.Diagnostics.Stopwatch.Frequency / 3) return;
            HienThiChiTietTaiKhoanTheoLoai(selectedModelDangChon.MaLoaiTK, selectedModelDangChon.Thang);

        }
        #endregion

        #region 4. Xử Lý Luồng Tìm Kiếm & Mở Form Nhập Lãi Suất / Mô Tả
        private void BtnTim_Click(object sender, EventArgs e)
        {
            var selectedDM = cboDanhMucLoaiTK.SelectedItem as DanhMucLoaiTKItem;
            if (selectedDM == null) return;

            int thangTim = selectedDM.MaLoaiTK == "TK_THANHTOAN" ? 0 : (int)nudThang.Value;
            // Lọc chính xác cả mã loại tài khoản lẫn kỳ hạn.
            var ketQua = dsLoaiTK.Where(x => x.MaLoaiTK == selectedDM.MaLoaiTK && x.Thang == thangTim).ToList();
            if (ketQua.Count > 0)
            {
                HienThiDanhSachLoaiTK(ketQua, ketQua[0]);
                return;
            }

            // Không được tự hiển thị tài khoản của kỳ hạn khác khi không có cấu hình.
            HienThiDanhSachLoaiTK(ketQua);
            selectedModelDangChon = null;
            lblChiTietHeader.Text = "Không tìm thấy cấu hình tương ứng";
            string kyHan = thangTim == 0 ? "Không kỳ hạn" : thangTim + " tháng";
            if (QuyenHoSo.LaGiamDoc() && MessageBox.Show("Không tìm thấy cấu hình '" + selectedDM.TenLoaiTK +
                "' kỳ hạn " + kyHan + ".\nBạn có muốn thêm cấu hình này không?",
                "Không có dữ liệu", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                MoFormNhapLaiSuatMoTa(selectedDM, thangTim);
        }

        private void MoFormNhapLaiSuatMoTa(DanhMucLoaiTKItem selectedDM, int thang, LoaiTaiKhoanModel modelChinhSua = null)
        {
            if (!QuyenHoSo.KiemTraQuyen(this)) return;
            bool isEdit = (modelChinhSua != null);
            decimal lsBanDau = isEdit ? modelChinhSua.LaiSuat : 0m;
            string moTaBanDau = isEdit ? modelChinhSua.MoTa : "";

            using (FrmNhapLaiSuatMoTa frm = new FrmNhapLaiSuatMoTa(selectedDM.TenLoaiTK, thang, lsBanDau, moTaBanDau, isEdit))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    if (isEdit)
                    {
                        try
                        {
                            ThucThiSql("UPDATE dbo.CauHinhLoaiTaiKhoan SET LaiSuat=@ls, MoTa=@mt WHERE Id=@id", p => {
                                p.Add("@ls", SqlDbType.Decimal).Value = frm.LaiSuat;
                                p.Add("@mt", SqlDbType.NVarChar, 500).Value = frm.MoTa;
                                p.Add("@id", SqlDbType.Int).Value = modelChinhSua.Id;
                            });
                            NapLaiSauThayDoi();
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi cập nhật SQL: " + ex.Message); return; }

                        MessageBox.Show("Cập nhật Lãi suất & Mô tả thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        try
                        {
                            ThucThiSql("INSERT INTO dbo.CauHinhLoaiTaiKhoan(MaLoaiTK,Thang,LaiSuat,MoTa) VALUES(@ma,@thang,@ls,@mt)", p => {
                                p.Add("@ma", SqlDbType.VarChar, 20).Value = selectedDM.MaLoaiTK;
                                p.Add("@thang", SqlDbType.Int).Value = thang;
                                p.Add("@ls", SqlDbType.Decimal).Value = frm.LaiSuat;
                                p.Add("@mt", SqlDbType.NVarChar, 500).Value = frm.MoTa;
                            });
                            NapLaiSauThayDoi();
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi thêm SQL: " + ex.Message); return; }

                        MessageBox.Show("Thêm mới loại tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        #endregion

        #region 5. Xử Lý Cập Nhật & Xóa
        private void ThucHienCapNhat()
        {
            if (!QuyenHoSo.KiemTraQuyen(this)) return;
            if (dgvLoaiTK.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn loại tài khoản cần cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = dgvLoaiTK.SelectedRows[0].Tag as LoaiTaiKhoanModel;
            if (item == null) return;

            var dmItem = dsDanhMucCoSan.FirstOrDefault(x => x.MaLoaiTK == item.MaLoaiTK)
                ?? new DanhMucLoaiTKItem { MaLoaiTK = item.MaLoaiTK, TenLoaiTK = item.TenLoaiTK };

            MoFormNhapLaiSuatMoTa(dmItem, item.Thang, item);
        }

        private void ThucHienXoa_Click(object sender, EventArgs e)
        {
            if (!QuyenHoSo.KiemTraQuyen(this)) return;
            if (dgvLoaiTK.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn loại tài khoản cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var item = dgvLoaiTK.SelectedRows[0].Tag as LoaiTaiKhoanModel;
            if (item == null) return;

            string strThang = item.Thang > 0 ? $"{item.Thang} tháng" : "Không kỳ hạn";

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa cấu hình '{item.TenLoaiTK}' (Kỳ hạn: {strThang})?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    ThucThiSql("DELETE FROM dbo.CauHinhLoaiTaiKhoan WHERE Id=@id", p => p.Add("@id", SqlDbType.Int).Value = item.Id);
                    NapLaiSauThayDoi();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa SQL: " + ex.Message); return; }
                MessageBox.Show("Xóa cấu hình thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnLamMoi_Click(object sender, EventArgs e)
        {
            if (cboDanhMucLoaiTK.Items.Count > 0) cboDanhMucLoaiTK.SelectedIndex = 0;
            try { NapLaiSauThayDoi(); }
            catch (Exception ex) { MessageBox.Show("Lỗi làm mới SQL: " + ex.Message); }
        }
        #endregion

        #region 6. DataGrid 2 - Chi tiết đúng loại và đúng số tài khoản
        // Chọn một cấu hình ở bảng trên: hiển thị các tài khoản thuộc loại/kỳ hạn đó.
        // Nhấp đúp một dòng bảng dưới: chỉ hiển thị chủ và đồng sở hữu của đúng số tài khoản.
        // Chọn lại cấu hình ở bảng trên để xem toàn bộ danh sách.
        private void HienThiChiTietTaiKhoanTheoLoai(string maLoaiTK, int thang, string soTKDuocChon = null)
        {
            dgvChiTietTK.Rows.Clear();

            string bangTaiKhoan, cotSoTK, bieuThucSoDu, bangDongSoHuu, cotLienKet;
            bool coKyHan;
            switch (maLoaiTK)
            {
                case "TK_THANHTOAN":
                    bangTaiKhoan = "dbo.TaiKhoanThanhToan";
                    cotSoTK = "SoTKTT";
                    bieuThucSoDu = "t.SoDu";
                    bangDongSoHuu = "dbo.DongSoHuuTaiKhoanThanhToan";
                    cotLienKet = "SoTKTT";
                    coKyHan = false;
                    break;
                case "TK_TIETKIEM":
                    bangTaiKhoan = "dbo.SoTietKiem";
                    cotSoTK = "SoTKTK";
                    // SoTienGui đã là gốc còn lại sau các lần rút.
                    bieuThucSoDu = "t.SoTienGui";
                    bangDongSoHuu = "dbo.DongSoHuuSoTietKiem";
                    cotLienKet = "SoTKTK";
                    coKyHan = true;
                    break;
                case "TK_VAY":
                    bangTaiKhoan = "dbo.TaiKhoanVay";
                    cotSoTK = "SoTKVay";
                    bieuThucSoDu = "CASE WHEN t.SoTienVay > t.SoTienDaTra THEN t.SoTienVay - t.SoTienDaTra ELSE 0 END";
                    bangDongSoHuu = "dbo.DongSoHuuTaiKhoanVay";
                    cotLienKet = "SoTKVay";
                    coKyHan = true;
                    break;
                default:
                    lblChiTietHeader.Text = "Không có dữ liệu cho loại tài khoản này";
                    lblTongSoLuong.Text = "Tổng số dòng: 0";
                    return;
            }

            string tenLoai = maLoaiTK == "TK_THANHTOAN" ? "tài khoản thanh toán"
                : maLoaiTK == "TK_TIETKIEM" ? "sổ tiết kiệm" : "khoản vay";
            lblChiTietHeader.Text = string.IsNullOrWhiteSpace(soTKDuocChon)
                ? "Danh sách " + tenLoai + (coKyHan ? " - " + thang + " tháng" : " (không kỳ hạn)")
                : "Chi tiết " + tenLoai + " - Số tài khoản: " + soTKDuocChon;

            try
            {
                using (var conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    // Chỉ lấy đồng sở hữu từ bảng liên kết có khóa là chính số tài khoản.
                    // Không nối theo CCCD_ChuSoHuu: một người có thể sở hữu nhiều tài khoản khác nhau.
                    bool coBangDongSoHuu;
                    using (var kiemTra = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@bang, 'U') IS NOT NULL THEN 1 ELSE 0 END", conn))
                    {
                        kiemTra.Parameters.Add("@bang", SqlDbType.NVarChar, 128).Value = bangDongSoHuu;
                        coBangDongSoHuu = Convert.ToInt32(kiemTra.ExecuteScalar()) == 1;
                    }

                    string sql = @"WITH TaiKhoan AS (
                        SELECT t." + cotSoTK + @" AS SoTK, t.CCCD AS CCCDChu,
                               (" + bieuThucSoDu + @") AS SoDu,
                               t.NgayMo, t.TrangThai
                        FROM " + bangTaiKhoan + @" t
                        WHERE (@soTK IS NULL OR t." + cotSoTK + @" = @soTK)"
                        + (coKyHan ? " AND t.KyHan = @kyHan" : "") + @"
                    )
                    SELECT a.SoTK, k.HoTen, k.CCCD, a.SoDu, a.NgayMo, a.TrangThai,
                           CAST(N'Chủ sở hữu' AS NVARCHAR(60)) AS VaiTro,
                           a.CCCDChu, 0 AS ThuTu
                    FROM TaiKhoan a
                    INNER JOIN dbo.KhachHang k ON k.CCCD = a.CCCDChu";

                    if (coBangDongSoHuu)
                    {
                        sql += @"
                        UNION ALL
                        SELECT a.SoTK, d.HoTen, d.CCCD, a.SoDu, a.NgayMo, a.TrangThai,
                               CAST(N'Đồng sở hữu' AS NVARCHAR(60)),
                               a.CCCDChu, 1
                        FROM TaiKhoan a
                        INNER JOIN " + bangDongSoHuu + @" d ON d." + cotLienKet + @" = a.SoTK
                        WHERE d.CCCD <> a.CCCDChu";
                    }
                    sql += " ORDER BY SoTK, ThuTu, HoTen";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@soTK", SqlDbType.VarChar, 20).Value =
                            string.IsNullOrWhiteSpace(soTKDuocChon) ? (object)DBNull.Value : soTKDuocChon.Trim();
                        if (coKyHan)
                            cmd.Parameters.Add("@kyHan", SqlDbType.NVarChar, 20).Value = thang + " tháng";

                        using (var r = cmd.ExecuteReader())
                        {
                            int stt = 1;
                            while (r.Read())
                            {
                                int trangThai = Convert.ToInt32(r[5]);
                                string tt = trangThai == 0 ? "Hoạt động" : trangThai == 1 ? "Đang khóa" :
                                    trangThai == 2 ? "Đã đóng" : "Không xác định";
                                int idx = dgvChiTietTK.Rows.Add(
                                    stt++, Convert.ToString(r[0]), Convert.ToString(r[1]),
                                    Convert.ToString(r[2]),
                                    r.IsDBNull(3) ? "0 VNĐ" : Convert.ToDecimal(r[3]).ToString("N0") + " VNĐ",
                                    r.IsDBNull(4) ? "-" : Convert.ToDateTime(r[4]).ToString("dd/MM/yyyy HH:mm"),
                                    tt, Convert.ToString(r[6]));
                                dgvChiTietTK.Rows[idx].Tag = Convert.ToString(r[7]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dgvChiTietTK.Rows.Clear();
                lblChiTietHeader.Text = "Không thể tải chi tiết tài khoản";
                _cauHinhVuaTai = null;
                MessageBox.Show("Lỗi tải chi tiết tài khoản: " + ex.Message,
                    "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblTongSoLuong.Text = "Tổng số dòng: 0";
                return;
            }
            _cauHinhVuaTai = selectedModelDangChon;
            _lanTaiChiTiet = System.Diagnostics.Stopwatch.GetTimestamp();
            lblTongSoLuong.Text = "Tổng số dòng: " + dgvChiTietTK.Rows.Count.ToString("N0");
            dgvChiTietTK.ClearSelection();
        }

        private void DgvChiTietTK_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvChiTietTK.Rows.Count || selectedModelDangChon == null)
                return;

            DataGridViewRow row = dgvChiTietTK.Rows[e.RowIndex];
            // Tag lưu CCCD CHỦ tài khoản, kể cả khi dòng được nhấp là đồng sở hữu.
            string cccdChu = Convert.ToString(row.Tag)?.Trim();
            // Cột 1 là số tài khoản thực tế của dòng được chọn.
            string soTK = Convert.ToString(row.Cells[1].Value)?.Trim();
            string maLoaiTK = selectedModelDangChon.MaLoaiTK;

            if (string.IsNullOrWhiteSpace(cccdChu) || string.IsNullOrWhiteSpace(soTK))
            {
                MessageBox.Show("Không xác định được số tài khoản hoặc chủ sở hữu.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FrmMain frmMain = TimFrmMain();
            if (frmMain == null || frmMain.IsDisposed)
            {
                MessageBox.Show("Không tìm thấy FrmMain để chuyển danh mục.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Chuyển trong khung nội dung của FrmMain. Không Show/ShowDialog một form nổi,
            // không gọi sự kiện nhấp đúp của form đích nên không mở bảng chi tiết chức năng.
            // BeginInvoke tránh thao tác lên FrmLoaiTaiKhoan sau khi FrmMain đóng form con cũ.
            frmMain.BeginInvoke(new Action(() =>
            {
                if (frmMain.IsDisposed) return;
                try
                {
                    frmMain.MoFormTheoLoaiTK(maLoaiTK, cccdChu, soTK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể chuyển đến danh mục tài khoản:\n" + ex.Message,
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private void LayDanhSachKhachHangTuFrmMain()
        {
            try
            {
                FrmMain frmMain = TimFrmMain();
                if (frmMain != null)
                {
                    FieldInfo field = typeof(FrmMain).GetField("listKhachHang", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (field != null)
                    {
                        object value = field.GetValue(frmMain);
                        if (value is IList list) danhSachKhachHang = list;
                    }
                }
                if (danhSachKhachHang == null) danhSachKhachHang = new ArrayList();
            }
            catch
            {
                danhSachKhachHang = new ArrayList();
            }
        }

        private FrmMain TimFrmMain()
        {
            if (this.Owner is FrmMain main) return main;
            foreach (Form f in Application.OpenForms)
            {
                if (f is FrmMain m) return m;
            }
            return null;
        }

        #region Helper Functions Reflection
        private object LayGiaTri(object obj, string propertyName)
        {
            if (obj == null) return null;
            PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return null;
            try { return property.GetValue(obj, null); } catch { return null; }
        }

        private string LayString(object obj, string propertyName)
        {
            return LayGiaTri(obj, propertyName)?.ToString() ?? "";
        }

        private decimal LayDecimal(object obj, string propertyName)
        {
            object value = LayGiaTri(obj, propertyName);
            if (value == null) return 0;
            try { return Convert.ToDecimal(value); } catch { return 0; }
        }

        private IList LayDanhSach(object obj, string propertyName)
        {
            object value = LayGiaTri(obj, propertyName);
            if (value is IList list) return list;
            return new ArrayList();
        }

        private string LayNgayFormatted(object item, string propName)
        {
            object value = LayGiaTri(item, propName) ?? LayGiaTri(item, "NgayMo") ?? LayGiaTri(item, "NgayLam");
            if (value is DateTime date) return date.ToString("dd/MM/yyyy HH:mm");
            if (value != null && DateTime.TryParse(value.ToString(), out DateTime parsed)) return parsed.ToString("dd/MM/yyyy HH:mm");
            return "-";
        }

        private string LayTrangThai(object item)
        {
            if (item == null) return "Hoạt động";
            object value = LayGiaTri(item, "TrangThai");
            if (value == null) return "Hoạt động";

            string text = value.ToString();
            if (text == "HoatDong" || text == "Hoạt động") return "Hoạt động";
            if (text == "Khoa" || text == "Đang khóa") return "Đang khóa";
            if (text == "Dong" || text == "Đã đóng") return "Đã đóng";

            return text;
        }
        #endregion
        #endregion
    }

    #region 7. Model Dữ Liệu & Form Modal Điền Lãi Suất / Mô Tả
    public class LoaiTaiKhoanModel
    {
        public int Id { get; set; }
        public string MaLoaiTK { get; set; }
        public string TenLoaiTK { get; set; }
        public int Thang { get; set; }
        public decimal LaiSuat { get; set; }
        public string MoTa { get; set; }
    }

    public class FrmNhapLaiSuatMoTa : Form
    {
        public decimal LaiSuat { get; private set; }
        public string MoTa { get; private set; }

        private NumericUpDown nudLaiSuat;
        private TextBox txtMoTa;
        private Button btnLuu;
        private Button btnHuy;

        public FrmNhapLaiSuatMoTa(string tenLoaiTK, int thang, decimal laiSuatBanDau = 0m, string moTaBanDau = "", bool isEdit = false)
        {
            InitializeCustomComponents(tenLoaiTK, thang, laiSuatBanDau, moTaBanDau, isEdit);
        }

        private void InitializeCustomComponents(string tenLoaiTK, int thang, decimal laiSuatBanDau, string moTaBanDau, bool isEdit)
        {
            this.Text = isEdit ? "Cập Nhật Lãi Suất & Mô Tả" : "Thêm Mới Cấu Hình Loại Tài Khoản";
            this.Size = new Size(480, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;

            Label lblInfo = new Label
            {
                Text = $"Loại TK: {tenLoaiTK}\nKỳ hạn: {(thang > 0 ? thang + " tháng" : "Không kỳ hạn")}",
                Location = new Point(20, 15),
                Size = new Size(420, 42),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(96, 165, 250)
            };

            Label lblLaiSuat = new Label
            {
                Text = "Lãi suất (%/năm):",
                Location = new Point(20, 70),
                Size = new Size(120, 23),
                Font = new Font("Segoe UI", 9.5F)
            };

            nudLaiSuat = new NumericUpDown
            {
                Location = new Point(140, 67),
                Size = new Size(130, 25),
                DecimalPlaces = 2,
                Increment = 0.1m,
                Maximum = 100m,
                Minimum = 0m,
                Value = Math.Min(100m, Math.Max(0m, laiSuatBanDau)),
                Font = new Font("Segoe UI", 9.5F)
            };

            Label lblMoTa = new Label
            {
                Text = "Mô tả / Ghi chú:",
                Location = new Point(20, 105),
                Size = new Size(120, 23),
                Font = new Font("Segoe UI", 9.5F)
            };

            txtMoTa = new TextBox
            {
                Location = new Point(140, 105),
                Size = new Size(300, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = moTaBanDau,
                Font = new Font("Segoe UI", 9.5F)
            };

            btnLuu = new Button
            {
                Text = isEdit ? "💾 Lưu Cập Nhật" : "➕ Thêm Mới",
                Location = new Point(200, 222),
                Size = new Size(120, 36),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLuu.FlatAppearance.BorderSize = 0;
            btnLuu.Click += (s, e) =>
            {
                this.LaiSuat = nudLaiSuat.Value;
                this.MoTa = txtMoTa.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnHuy = new Button
            {
                Text = "❌ Hủy",
                Location = new Point(330, 222),
                Size = new Size(110, 36),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHuy.FlatAppearance.BorderSize = 0;
            btnHuy.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(lblLaiSuat);
            this.Controls.Add(nudLaiSuat);
            this.Controls.Add(lblMoTa);
            this.Controls.Add(txtMoTa);
            this.Controls.Add(btnLuu);
            this.Controls.Add(btnHuy);

            this.AcceptButton = btnLuu;
            this.CancelButton = btnHuy;
        }
    }
    #endregion
}