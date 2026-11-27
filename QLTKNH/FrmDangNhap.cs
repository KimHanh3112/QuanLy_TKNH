using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace QLTKNH
{
    // Chỉ chứa sự kiện/logic. Giao diện FrmDangNhap nằm trong FrmDangNhap.Designer.cs.
    public partial class FrmDangNhap : Form
    {
        public FrmDangNhap()
        {
            InitializeComponent();
            KhoiTaoDashboard();
            pnlBenTrai.Paint += VeNenThuongHieu;
            pnlBenTrai.Resize += (s, e) => pnlBenTrai.Invalidate();
            pnlCard.Paint += VeVienDangNhap;
            // Gắn xử lý căn chỉnh tại runtime, không đặt lambda trong Designer.
            Resize += FrmDangNhap_CanChinh;
            pnlBenPhai.Resize += FrmDangNhap_CanChinh;
            Shown += FrmDangNhap_CanChinh;
            txtTenDangNhap.Focus();
            FormClosing += (s, e) =>
            {
                // Không để phiên đăng nhập đang xử lý chạy tiếp sau khi form đóng.
                if (_dangDangNhap && DialogResult != DialogResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
                if (DialogResult != DialogResult.OK) BankSecurity.Logout();
                if (!e.Cancel && _dashboardClock != null) _dashboardClock.Stop();
            };
        }

        // Tất cả tính toán co giãn đặt ngoài InitializeComponent để Visual Studio Designer đọc được.
        private bool _dangCanChinh;
        private bool _dangDangNhap;
        private static readonly System.Globalization.CultureInfo _vi =
            System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        private Timer _dashboardClock;
        private Label _lblClock;
        private int _hoverCard = -1;
        private int _hoverMonth = -1;
        private Rectangle _plotArea;
        private Rectangle[] _dashboardCards = new Rectangle[4];

        private void KhoiTaoDashboard()
        {
            // Dashboard chỉ đọc số liệu tổng hợp, không đọc hồ sơ khách hàng.
            Shown += TaiDashboardSql;
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)?.SetValue(pnlBenTrai, true, null);
            _lblClock = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(147, 197, 253),
                BackColor = Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            pnlBenPhai.Controls.Add(_lblClock);
            _dashboardClock = new Timer(components) { Interval = 1000 };
            _dashboardClock.Tick += (s, e) => CapNhatDongHo();
            CapNhatDongHo();
            _dashboardClock.Start();
            pnlBenTrai.MouseMove += (s, e) => {
                int next = Array.FindIndex(_dashboardCards, r => r.Contains(e.Location));
                int month = _counts != null && _plotArea.Contains(e.Location) && _plotArea.Width > 0
                    ? Math.Max(0, Math.Min(5, (int)Math.Round((e.X - _plotArea.Left) * 5.0 / _plotArea.Width))) : -1;
                if (next != _hoverCard || month != _hoverMonth) { _hoverCard = next; _hoverMonth = month; pnlBenTrai.Invalidate(); }
            };
            pnlBenTrai.MouseLeave += (s, e) =>
            {
                if (_hoverCard == -1 && _hoverMonth == -1) return;
                _hoverCard = -1; _hoverMonth = -1; pnlBenTrai.Invalidate();
            };
            btnDangNhap.FlatAppearance.MouseOverBackColor = Color.FromArgb(59, 130, 246);
            btnDangNhap.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);
            pnlUser.Paint += VeVienNhap;
            pnlPass.Paint += VeVienNhap;
            txtTenDangNhap.Enter += (s, e) => pnlUser.Invalidate();
            txtTenDangNhap.Leave += (s, e) => pnlUser.Invalidate();
            txtMatKhau.Enter += (s, e) => pnlPass.Invalidate();
            txtMatKhau.Leave += (s, e) => pnlPass.Invalidate();
        }

        private long[] _counts;
        private long[] _monthly = new long[6];
        private DateTime _firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
        private string _dashboardStatus = "Đang tải dữ liệu SQL...";
        private bool _loadingDashboard;

        private async void TaiDashboardSql(object sender, EventArgs e)
        {
            if (_loadingDashboard || IsDisposed || Disposing) return;
            _loadingDashboard = true;
            try
            {
                long[] counts = new long[8];
                long[] monthly = new long[6];
                DateTime first = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
                using (var cn = new SqlConnection(BankSecurity.ConnStr))
                using (var cmd = new SqlCommand(@"SELECT (SELECT COUNT_BIG(*) FROM dbo.KhachHang),
                       (SELECT COUNT_BIG(*) FROM dbo.DN_KhachHang),
                       (SELECT COUNT_BIG(*) FROM dbo.TaiKhoanThanhToan),
                       (SELECT COUNT_BIG(*) FROM dbo.SoTietKiem),
                       (SELECT COUNT_BIG(*) FROM dbo.TaiKhoanVay),
                       (SELECT COUNT_BIG(*) FROM dbo.DN_TaiKhoanThanhToan),
                       (SELECT COUNT_BIG(*) FROM dbo.DN_SoTietKiem),
                       (SELECT COUNT_BIG(*) FROM dbo.DN_TaiKhoanVay);
                SELECT DATEPART(year, Ngay), DATEPART(month, Ngay), COUNT_BIG(*)
                FROM (
                    SELECT ThoiGian AS Ngay FROM dbo.LichSuGiaoDich WHERE ThoiGian>=@from AND ThoiGian<@to
                    UNION ALL
                    SELECT NgayGiaoDich AS Ngay FROM dbo.LichSuGiaoDichTietKiem WHERE NgayGiaoDich>=@from AND NgayGiaoDich<@to
                    UNION ALL
                    SELECT NgayGiaoDich AS Ngay FROM dbo.LichSuGiaoDichVay WHERE NgayGiaoDich>=@from AND NgayGiaoDich<@to
                    UNION ALL
                    SELECT ThoiGian AS Ngay FROM dbo.DN_LichSuGiaoDich WHERE ThoiGian>=@from AND ThoiGian<@to
                    UNION ALL
                    SELECT NgayGiaoDich AS Ngay FROM dbo.DN_LichSuGiaoDichTietKiem WHERE NgayGiaoDich>=@from AND NgayGiaoDich<@to
                    UNION ALL
                    SELECT NgayGiaoDich AS Ngay FROM dbo.DN_LichSuGiaoDichVay WHERE NgayGiaoDich>=@from AND NgayGiaoDich<@to
                ) AS h GROUP BY DATEPART(year, Ngay), DATEPART(month, Ngay);", cn))
                {
                    cmd.CommandTimeout = 10;
                    cmd.Parameters.Add("@from", SqlDbType.DateTime).Value = first;
                    cmd.Parameters.Add("@to", SqlDbType.DateTime).Value = first.AddMonths(6);
                    await cn.OpenAsync();
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        if (!await rd.ReadAsync()) throw new InvalidOperationException();
                        for (int i = 0; i < counts.Length; i++) counts[i] = rd.GetInt64(i);
                        if (await rd.NextResultAsync())
                            while (await rd.ReadAsync())
                            {
                                int index = (rd.GetInt32(0) - first.Year) * 12 + rd.GetInt32(1) - first.Month;
                                if (index >= 0 && index < 6) monthly[index] = rd.GetInt64(2);
                            }
                    }
                }
                _counts = counts; _monthly = monthly; _firstMonth = first;
                _dashboardStatus = "SQL • Cập nhật " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception)
            {
                _dashboardStatus = "Không tải được dữ liệu SQL";
            }
            finally
            {
                _loadingDashboard = false;
                if (!IsDisposed && !Disposing && !pnlBenTrai.IsDisposed)
                {
                    lblIntro.Text = _dashboardStatus;
                    pnlBenTrai.Invalidate();
                }
            }
        }

        private void CapNhatDongHo()
        {
            if (_lblClock == null || _lblClock.IsDisposed) return;
            string text = DateTime.Now.ToString("dddd, dd/MM/yyyy  •  HH:mm:ss", _vi);
            if (_lblClock.Text != text) _lblClock.Text = text;
        }

        private void FrmDangNhap_CanChinh(object sender, EventArgs e)
        {
            if (_dangCanChinh || pnlBenTrai == null || pnlBenPhai == null || pnlCard == null) return;
            _dangCanChinh = true;
            try
            {
                pnlBenTrai.Width = (int)(ClientSize.Width * .56);
                int rw = pnlBenPhai.ClientSize.Width;
                int cw = Math.Min(530, Math.Max(300, rw - 48));
                pnlCard.Size = new Size(cw, 540);
                pnlCard.Location = new Point(Math.Max(8, (rw - cw) / 2), Math.Max(8, (pnlBenPhai.ClientSize.Height - 600) / 2));
                if (_lblClock != null) _lblClock.SetBounds(pnlCard.Left, pnlCard.Bottom + 12, cw, 38);
                int margin = Math.Max(24, (pnlBenTrai.Width - 760) / 2);
                int width = pnlBenTrai.Width - margin * 2;
                lblBrand.SetBounds(margin, 20, width, 32);
                lblHero.SetBounds(margin, 56, width, 42);
                lblIntro.SetBounds(margin, 100, width, 25);
                lblFeature.SetBounds(margin, 128, width, 23);
                lblBrandFooter.SetBounds(margin, pnlBenTrai.Height - 32, width, 24);
                pnlBenTrai.Invalidate();
            }
            finally { _dangCanChinh = false; }
        }

        private static void VeVienNhap(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            using (var pen = new Pen(panel.ContainsFocus ? Color.FromArgb(96, 165, 250) : Color.FromArgb(71, 85, 105), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        private static GraphicsPath KhungBoGoc(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void VeKhung(Graphics g, Rectangle r, Color fill)
        {
            using (var path = KhungBoGoc(r, 10))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(Color.FromArgb(51, 65, 85)))
            { g.FillPath(brush, path); g.DrawPath(pen, path); }
        }

        private void VeNenThuongHieu(object sender, PaintEventArgs e)
        {
            Rectangle r = pnlBenTrai.ClientRectangle;
            if (r.Width < 320 || r.Height < 400) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = new LinearGradientBrush(r, Color.FromArgb(11, 18, 32), Color.FromArgb(23, 37, 84), 35f))
                g.FillRectangle(bg, r);
            int x = lblFeature.Left, w = lblFeature.Width, y = lblFeature.Bottom + 6;
            int gap = 10, cw = (w - gap) / 2;
            int available = Math.Max(320, lblBrandFooter.Top - y - 8);
            int ch = Math.Min(96, Math.Max(62, (int)(available * .14)));
            int lineH = Math.Max(175, (int)(available * .39));
            int pieH = Math.Max(104, available - 2 * ch - lineH - 3 * gap);
            string[] names = { "Khách hàng cá nhân", "Doanh nghiệp", "Sổ tiết kiệm", "Tài khoản vay" };
            string[] nums = _counts == null ? new[] { "—", "—", "—", "—" } :
                new[] { _counts[0], _counts[1], _counts[3] + _counts[6], _counts[4] + _counts[7] }.Select(n => n.ToString("N0", _vi)).ToArray();
            Color[] colors = { Color.FromArgb(96, 165, 250), Color.FromArgb(45, 212, 191), Color.FromArgb(74, 222, 128), Color.FromArgb(248, 113, 113) };
            using (var ink = new SolidBrush(Color.FromArgb(241, 245, 249)))
            using (var muted = new SolidBrush(Color.FromArgb(148, 163, 184)))
            using (var grid = new Pen(Color.FromArgb(51, 65, 85)))
            using (var title = new Font("Segoe UI", 11, FontStyle.Bold))
            using (var small = new Font("Segoe UI", 9))
            using (var number = new Font("Segoe UI", 21, FontStyle.Bold))
            {
                for (int i = 0; i < 4; i++)
                {
                    var card = new Rectangle(x + i % 2 * (cw + gap), y + i / 2 * (ch + gap), cw, ch);
                    _dashboardCards[i] = card;
                    VeKhung(g, card, i == _hoverCard ? Color.FromArgb(39, 57, 85) : Color.FromArgb(30, 41, 59));
                    using (var accent = new SolidBrush(colors[i]))
                    {
                        g.FillEllipse(accent, card.Right - 24, card.Y + 13, 10, 10);
                        g.DrawString(names[i], small, muted, card.X + 12, card.Y + 8);
                        g.DrawString(nums[i], number, accent, card.X + 10, card.Y + 27);
                    }
                }
                int ly = y + 2 * (ch + gap);
                VeKhung(g, new Rectangle(x, ly, w, lineH), Color.FromArgb(17, 30, 56));
                VeBieuDoTerminal(g, new Rectangle(x, ly, w, lineH), title, small, ink, muted);
                int py = ly + lineH + gap;
                VeBieuDoCoCau(g, new Rectangle(x, py, cw, pieH), "Cá nhân", 2, title, small, ink, muted);
                VeBieuDoCoCau(g, new Rectangle(x + cw + gap, py, cw, pieH), "Doanh nghiệp", 5, title, small, ink, muted);
            }
        }

        // Trading terminal: chỉ trình bày số liệu lịch sử, không thay đổi nguồn SQL.
        private void VeBieuDoTerminal(Graphics g, Rectangle card, Font title, Font small, Brush ink, Brush muted)
        {
            _plotArea = Rectangle.Empty;
            var vi = _vi;
            g.DrawString("HOẠT ĐỘNG • 6 THÁNG", small, muted, card.X + 12, card.Y + 8);
            if (_counts == null)
            {
                g.DrawString(_dashboardStatus, title, ink, card.X + 12, card.Y + 36);
                return;
            }
            decimal total = _monthly.Aggregate(0m, (sum, n) => sum + n);
            long last = _monthly[5], previous = _monthly[4];
            decimal change = (decimal)last - previous;
            Color trend = change > 0 ? Color.FromArgb(74, 222, 128) : change < 0 ? Color.FromArgb(248, 113, 113) : Color.FromArgb(148, 163, 184);
            string delta = previous == 0 ? (last == 0 ? "0 • nền 0" : "+" + change.ToString("N0", vi) + " • nền 0") :
                (change > 0 ? "+" : "") + (change / previous).ToString("P1", vi);
            using (var big = new Font("Segoe UI", 19F, FontStyle.Bold))
            using (var mono = new Font("Consolas", 9F, FontStyle.Bold))
            using (var trendBrush = new SolidBrush(trend))
            using (var blue = new SolidBrush(Color.FromArgb(147, 197, 253)))
            using (var grid = new Pen(Color.FromArgb(51, 65, 85)) { DashStyle = DashStyle.Dot })
            using (var cross = new Pen(Color.FromArgb(148, 163, 184)) { DashStyle = DashStyle.Dash })
            using (var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
            using (var right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
            {
                g.DrawString(total.ToString("N0", vi), big, ink, card.X + 10, card.Y + 23);
                g.DrawString("Tổng bản ghi", small, muted, card.X + 12, card.Y + 55);
                int headerWidth = Math.Max(100, card.Width / 2 - 14);
                g.DrawString("Tháng này / tháng trước*", small, muted, new RectangleF(card.Right - headerWidth - 10, card.Y + 8, headerWidth, 20), right);
                g.DrawString(delta, mono, trendBrush, new RectangleF(card.Right - headerWidth - 10, card.Y + 30, headerWidth, 22), right);
                g.DrawString("*Tháng hiện tại chưa kết thúc", small, muted, new RectangleF(card.Right - headerWidth - 10, card.Y + 52, headerWidth, 18), right);
                double rawMax = Math.Max(1L, _monthly.Max());
                double unit = Math.Pow(10, Math.Floor(Math.Log10(rawMax / 3.0)));
                double ratio = rawMax / 3.0 / unit;
                double step = Math.Max(1, (ratio <= 1 ? 1 : ratio <= 2 ? 2 : ratio <= 5 ? 5 : 10) * unit);
                double top = Math.Ceiling(rawMax / step) * step;
                int axis = Math.Max(36, (int)Math.Ceiling(g.MeasureString(top.ToString("N0", vi), small).Width) + 10);
                int gx = card.X + 22, gy = card.Y + 92;
                int gw = card.Width - axis - 43, gh = card.Bottom - 26 - gy;
                if (gw < 60 || gh < 15) return;
                _plotArea = new Rectangle(gx, gy, gw, gh);
                int ticks = (int)Math.Round(top / step);
                for (int i = 0; i <= ticks; i++)
                {
                    float yy = gy + gh - (float)(i * step / top * gh);
                    g.DrawLine(grid, gx, yy, gx + gw, yy);
                    g.DrawString((i * step).ToString("N0", vi), small, muted, gx + gw + 6, yy - 8);
                }
                PointF[] pts = _monthly.Select((n, i) => new PointF(gx + i * gw / 5f, gy + gh - (float)(n / top * gh))).ToArray();
                using (var fill = new LinearGradientBrush(_plotArea, Color.FromArgb(85, 59, 130, 246), Color.FromArgb(0, 59, 130, 246), 90f))
                    g.FillPolygon(fill, pts.Concat(new[] { new PointF(gx + gw, gy + gh), new PointF(gx, gy + gh) }).ToArray());
                using (var line = new Pen(Color.FromArgb(96, 165, 250), 2.3f)) g.DrawLines(line, pts);
                for (int i = 0; i < 6; i++)
                {
                    PointF pt = pts[i];
                    g.DrawLine(grid, pt.X, gy, pt.X, gy + gh);
                    g.FillEllipse(blue, pt.X - 3, pt.Y - 3, 6, 6);
                    string value = _monthly[i].ToString("N0", vi);
                    float width = Math.Min(gw / 5f, Math.Max(24, g.MeasureString(value, mono).Width + 4));
                    g.DrawString(value, mono, ink, new RectangleF(pt.X - width / 2, pt.Y - 20, width, 17), center);
                    g.DrawString(_firstMonth.AddMonths(i).ToString("MM/yy"), small, muted, new RectangleF(pt.X - 24, gy + gh + 5, 48, 18), center);
                }
                if (_hoverMonth >= 0 && _hoverMonth < 6)
                {
                    PointF pt = pts[_hoverMonth];
                    g.DrawLine(cross, pt.X, gy, pt.X, gy + gh);
                    g.DrawLine(cross, gx, pt.Y, gx + gw, pt.Y);
                    g.FillEllipse(blue, pt.X - 5, pt.Y - 5, 10, 10);
                    string tip = _firstMonth.AddMonths(_hoverMonth).ToString("MM/yyyy") + "  |  " + _monthly[_hoverMonth].ToString("N0", vi) + " bản ghi";
                    int tw = Math.Min(card.Width - 16, (int)Math.Ceiling(g.MeasureString(tip, mono).Width) + 20);
                    var tr = new Rectangle(Math.Max(card.X + 8, Math.Min((int)pt.X - tw / 2, card.Right - tw - 8)), card.Y + 72, tw, 21);
                    using (var bg = new SolidBrush(Color.FromArgb(30, 58, 105))) g.FillRectangle(bg, tr);
                    g.DrawString(tip, mono, ink, tr, center);
                }
            }
        }

        private void VeBieuDoCoCau(Graphics g, Rectangle card, string name, int offset, Font title, Font small, Brush ink, Brush muted)
        {
            VeKhung(g, card, Color.FromArgb(30, 41, 59));
            g.DrawString(name, title, ink, card.X + 12, card.Y + 8);
            int size = Math.Min(Math.Min(100, card.Width / 3), card.Height - 38);
            if (size < 24) return;
            var ring = new Rectangle(card.X + 12, card.Y + 32, size, size);
            using (var pen = new Pen(Color.FromArgb(71, 85, 105), 10))
                g.DrawEllipse(pen, ring.X + 5, ring.Y + 5, size - 10, size - 10);
            Color[] colors = { Color.FromArgb(59, 130, 246), Color.FromArgb(34, 197, 94), Color.FromArgb(239, 68, 68) };
            string[] names = { "Thanh toán", "Tiết kiệm", "Vay" };
            long total = _counts == null ? 0 : _counts[offset] + _counts[offset + 1] + _counts[offset + 2];
            float angle = -90;
            for (int i = 0; i < 3; i++)
            {
                long n = _counts == null ? 0 : _counts[offset + i];
                double ratio = total == 0 ? 0 : (double)n / total;
                float sweep = (float)(ratio * 360);
                if (sweep > 0)
                    using (var pen = new Pen(colors[i], 10)) g.DrawArc(pen, ring.X + 5, ring.Y + 5, size - 10, size - 10, angle, sweep);
                angle += sweep;
                int lx = ring.Right + 9, ly = card.Y + 32 + i * 21;
                using (var brush = new SolidBrush(colors[i])) g.FillEllipse(brush, lx, ly + 5, 7, 7);
                string value = _counts == null ? "—" : n + " (" + ratio.ToString("P0") + ")";
                g.DrawString(names[i] + " " + value, small, ink, new RectangleF(lx + 10, ly, Math.Max(1, card.Right - lx - 12), 21));
            }
            using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(_counts == null ? "—" : total.ToString(), title, ink, ring, fmt);
        }

        private void VeVienDangNhap(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(71, 85, 105)))
                e.Graphics.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
        }

        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            if (_dangDangNhap || IsDisposed || Disposing) return;
            _dangDangNhap = true;
            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text;
            bool userReadOnly = txtTenDangNhap.ReadOnly;
            bool passReadOnly = txtMatKhau.ReadOnly;
            btnDangNhap.Enabled = false;
            txtTenDangNhap.ReadOnly = true;
            txtMatKhau.ReadOnly = true;
            try
            {
                // Giữ nguyên BankSecurity.Login; chỉ chuyển SQL và PBKDF2 khỏi UI thread.
                var result = await System.Threading.Tasks.Task.Run(() =>
                {
                    string message;
                    bool ok = BankSecurity.Login(username, password, out message);
                    return Tuple.Create(ok, message);
                });
                if (IsDisposed || Disposing) return;
                if (!result.Item1)
                {
                    lblThongBao.Text = result.Item2;
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                    return;
                }
                txtMatKhau.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblThongBao.Text = "Không thể kết nối SQL. Vui lòng kiểm tra server và script bổ sung.";
                MessageBox.Show("Lỗi đăng nhập: " + ex.Message, "SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _dangDangNhap = false;
                if (!IsDisposed && !Disposing)
                {
                    txtTenDangNhap.ReadOnly = userReadOnly;
                    txtMatKhau.ReadOnly = passReadOnly;
                    btnDangNhap.Enabled = true;
                }
            }
        }

        private void btnHienMatKhau_Click(object sender, EventArgs e)
        {
            txtMatKhau.UseSystemPasswordChar = !txtMatKhau.UseSystemPasswordChar;
            btnHienMatKhau.Text = txtMatKhau.UseSystemPasswordChar ? "Hiện mật khẩu" : "Ẩn mật khẩu";
        }
    }

    // Lưu phiên người dùng trong RAM, thông tin tài khoản và nhật ký ở SQL.
    public static class BankSecurity
    {
        public sealed class NhanVienSession
        {
            public int Id { get; set; }
            public string TenDangNhap { get; set; }
            public string HoTen { get; set; }
            public string ChucVu { get; set; }
            public string CaLam { get; set; }
        }

        // Dùng đúng server của file FrmMain.cs mới được cung cấp.
        public const string ConnStr = @"server=KIMHANH; database=QLTKNH; Integrated Security=true; TrustServerCertificate=True;";
        public static readonly string[] Roles = { "Giám đốc", "Phó giám đốc", "Trưởng phòng", "Kiểm soát viên", "Chuyên viên tín dụng", "Giao dịch viên", "Nhân viên hỗ trợ" };
        // Sức chứa theo chức vụ: vị trí khác được phép bỏ trống; GDV và hỗ trợ không giới hạn.
        public static int RoleCapacity(string role)
        {
            switch (role)
            {
                case "Giám đốc": return 1;
                case "Phó giám đốc": return 2;
                case "Trưởng phòng": return 3;
                case "Kiểm soát viên": return 4;
                case "Chuyên viên tín dụng": return 5;
                case "Giao dịch viên":
                case "Nhân viên hỗ trợ": return int.MaxValue;
                default: throw new ArgumentException("Chức vụ không hợp lệ.");
            }
        }
        public static DataTable CompanyPositions()
        {
            if (!IsLoggedIn) throw new UnauthorizedAccessException("Vui lòng đăng nhập.");
            DataTable data = new DataTable();
            data.Columns.Add("Chức vụ", typeof(string));
            data.Columns.Add("Định biên", typeof(string));
            data.Columns.Add("Đang làm", typeof(int));
            data.Columns.Add("Còn trống", typeof(string));
            data.Columns.Add("Nhân viên đang làm", typeof(string));
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"SELECT ChucVu,HoTen,TenDangNhap FROM dbo.NhanVienNganHang
                WHERE TrangThai<>N'Nghỉ việc' ORDER BY HoTen", cn))
            {
                var employees = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(employees);
                foreach (string role in Roles)
                {
                    var people = employees.Rows.Cast<DataRow>().Where(r => Convert.ToString(r["ChucVu"]) == role).ToList();
                    int max = RoleCapacity(role);
                    data.Rows.Add(role, max == int.MaxValue ? "Không giới hạn" : max.ToString(),
                        people.Count, max == int.MaxValue ? "Không giới hạn" : Math.Max(0, max - people.Count).ToString(),
                        string.Join(", ", people.Select(p => Convert.ToString(p["HoTen"]) + " (" + Convert.ToString(p["TenDangNhap"]) + ")")));
                }
            }
            return data;
        }
        private static void CheckRoleCapacity(SqlConnection cn, SqlTransaction tx, string role, int exceptId = -1)
        {
            int cap = RoleCapacity(role);
            if (cap == int.MaxValue) return;
            using (var cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.NhanVienNganHang WITH (UPDLOCK,HOLDLOCK)
                WHERE ChucVu=@role AND TrangThai<>N'Nghỉ việc' AND Id<>@except", cn, tx))
            {
                AddParam(cmd, "@role", role);
                AddParam(cmd, "@except", exceptId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) >= cap)
                    throw new InvalidOperationException("Chức vụ " + role + " đã đủ " + cap + " người đang làm.");
            }
        }
        public static NhanVienSession Current { get; private set; }
        public static bool IsLoggedIn => Current != null;
        // Dữ liệu mẫu: cho phép thử các tài khoản ngoài ca. Đặt true khi muốn chặn ngoài ca.
        public const bool EnforceShiftAtLogin = false;
        public static int Rank(string role)
        {
            int i = Array.IndexOf(Roles, role);
            return i < 0 ? 99 : i;
        }
        public static bool CanManageEmployees => IsLoggedIn && Current.ChucVu == "Giám đốc";
        public static bool CanSeeAudit => IsLoggedIn;
        public static bool CanViewCustomers => IsLoggedIn;
        public static bool CanUseFinance => IsLoggedIn && Rank(Current.ChucVu) <= 5;
        public static bool CanUseLoans => IsLoggedIn;
        public static bool CanManageEmployee(NhanVienSession employee)
        {
            return CanManageEmployees && employee != null && employee.Id != Current.Id &&
                Rank(employee.ChucVu) > Rank(Current.ChucVu);
        }
        private static SqlConnection Open()
        {
            var cn = new SqlConnection(ConnStr);
            try
            {
                cn.Open();
                return cn;
            }
            catch
            {
                cn.Dispose();
                throw;
            }
        }
        private static byte[] HashPassword(string password, byte[] salt)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, salt, 150000))
                return kdf.GetBytes(32);
        }
        private static bool EqualHashes(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
        private static void AddParam(SqlCommand c, string name, object value) =>
            c.Parameters.AddWithValue(name, value ?? (object)DBNull.Value);
        private static int Execute(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] pars)
        {
            using (var cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddRange(pars);
                return cmd.ExecuteNonQuery();
            }
        }
        private static SqlParameter P(string key, object value) => new SqlParameter(key, value ?? DBNull.Value);

        public static bool Login(string username, string password, out string message)
        {
            message = "Tên tài khoản hoặc mật khẩu không đúng.";
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) return false;
            Logout();
            NhanVienSession session;
            byte[] verifiedHash, verifiedSalt;
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"SELECT TOP 1 Id, TenDangNhap, MatKhauHash, MatKhauSalt, HoTen, ChucVu, CaLam, TrangThai, BiKhoa
                                           FROM dbo.NhanVienNganHang WHERE TenDangNhap=@login", cn))
            {
                AddParam(cmd, "@login", username);
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return false;
                    int id = Convert.ToInt32(rd["Id"]);
                    string role = Convert.ToString(rd["ChucVu"]);
                    string shift = Convert.ToString(rd["CaLam"]);
                    string status = Convert.ToString(rd["TrangThai"]);
                    byte[] hash = (byte[])rd["MatKhauHash"];
                    byte[] salt = (byte[])rd["MatKhauSalt"];
                    if (!EqualHashes(hash, HashPassword(password, salt))) return false;
                    if (Convert.ToBoolean(rd["BiKhoa"])) { message = "Tài khoản đã khóa bởi Giám đốc. Vui lòng liên hệ Giám đốc để được mở khóa."; return false; }
                    if (status == "Nghỉ việc") { message = "Nhân viên đã nghỉ việc, không thể đăng nhập."; return false; }
                    // Chế độ ca nghiêm ngặt: chỉ đăng nhập khi đang ở ca làm.
                    // Bỏ kiểm tra dưới đây nếu muốn thử tài khoản ca chiều vào buổi sáng.
                    if (EnforceShiftAtLogin && !IsOnShift(shift)) { message = "Chưa đến ca làm việc của tài khoản này."; return false; }
                    verifiedHash = hash;
                    verifiedSalt = salt;
                    session = new NhanVienSession
                    {
                        Id = id,
                        TenDangNhap = Convert.ToString(rd["TenDangNhap"]),
                        HoTen = Convert.ToString(rd["HoTen"]),
                        ChucVu = role,
                        CaLam = shift
                    };
                }
                // Chỉ xác nhận phiên sau khi cả trạng thái và nhật ký đã được lưu.
                using (var tx = cn.BeginTransaction())
                {
                    using (var update = new SqlCommand(@"UPDATE dbo.NhanVienNganHang SET DangHoatDong=1,
                                                     TrangThai=N'Đang hoạt động',
                                                     DangNhapLanCuoi=GETDATE(), LanHoatDongCuoi=GETDATE()
                                                     WHERE Id=@id AND TrangThai<>N'Nghỉ việc' AND ISNULL(BiKhoa,0)=0
                                                       AND MatKhauHash=@hash AND MatKhauSalt=@salt
                                                       AND ChucVu=@role AND CaLam=@shift", cn, tx))
                    {
                        AddParam(update, "@id", session.Id);
                        AddParam(update, "@hash", verifiedHash);
                        AddParam(update, "@salt", verifiedSalt);
                        AddParam(update, "@role", session.ChucVu);
                        AddParam(update, "@shift", session.CaLam);
                        if (update.ExecuteNonQuery() != 1) return false;
                    }
                    Execute(cn, tx, @"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet,ThoiGian)
                        VALUES(@id,@login,N'Đăng nhập',N'Đăng nhập hệ thống thành công',GETDATE())",
                        P("@id", session.Id), P("@login", session.TenDangNhap));
                    tx.Commit();
                }
                Current = session;
                message = "Đăng nhập thành công.";
                return true;
            }
        }

        public static bool IsOnShift(string shift)
        {
            int hour = DateTime.Now.Hour;
            if (shift == "Cả hai") return hour >= 6 && hour < 22;
            return shift == "Sáng" ? hour >= 6 && hour < 14 : hour >= 14 && hour < 22;
        }
        public static void Logout()
        {
            var old = Current;
            if (old == null) return;
            Current = null;
            try
            {
                using (var cn = Open())
                {
                    using (var cmd = new SqlCommand("UPDATE dbo.NhanVienNganHang SET DangHoatDong=0, TrangThai=CASE WHEN TrangThai=N'Nghỉ việc' THEN TrangThai ELSE N'Offline' END WHERE Id=@id", cn))
                    { AddParam(cmd, "@id", old.Id); cmd.ExecuteNonQuery(); }
                    using (var log = new SqlCommand(@"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet,ThoiGian)
                        VALUES(@id,@login,N'Đăng xuất',N'Đăng xuất hệ thống',GETDATE())", cn))
                    { AddParam(log, "@id", old.Id); AddParam(log, "@login", old.TenDangNhap); log.ExecuteNonQuery(); }
                }
            }
            catch (SqlException) { /* Không ngăn thoát chương trình nếu DB mất kết nối. */ }
        }
        public static bool ValidateSession()
        {
            if (!IsLoggedIn) return false;
            try
            {
                using (var cn = Open())
                using (var cmd = new SqlCommand("SELECT TrangThai, CaLam, ISNULL(BiKhoa,0) AS BiKhoa FROM dbo.NhanVienNganHang WHERE Id=@id", cn))
                {
                    AddParam(cmd, "@id", Current.Id);
                    using (var rd = cmd.ExecuteReader())
                        return rd.Read() && !Convert.ToBoolean(rd["BiKhoa"]) && Convert.ToString(rd["TrangThai"]) != "Nghỉ việc" && (!EnforceShiftAtLogin || IsOnShift(Convert.ToString(rd["CaLam"])));
                }
            }
            catch { return false; }
        }
        public static bool Heartbeat()
        {
            if (!ValidateSession()) return false;
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"UPDATE dbo.NhanVienNganHang
                SET LanHoatDongCuoi=GETDATE(), TrangThai=N'Đang hoạt động'
                WHERE Id=@id AND DangHoatDong=1 AND TrangThai<>N'Nghỉ việc' AND ISNULL(BiKhoa,0)=0", cn))
            {
                AddParam(cmd, "@id", Current.Id);
                return cmd.ExecuteNonQuery() == 1;
            }
        }
        public static DataTable Employees()
        {
            if (!CanManageEmployees) throw new UnauthorizedAccessException("Chỉ Giám đốc được xem danh sách nhân viên.");
            var result = new DataTable();
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"SELECT Id,TenDangNhap,HoTen,SoDienThoai,DiaChi,ChucVu,CaLam,SoMay,SoPhong,
                    CASE WHEN TrangThai=N'Nghỉ việc' THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS NghiViec,
                    DangHoatDong,LanHoatDongCuoi,DangNhapLanCuoi
                    FROM dbo.NhanVienNganHang
                    ORDER BY CASE ChucVu WHEN N'Giám đốc' THEN 1 WHEN N'Phó giám đốc' THEN 2
                        WHEN N'Trưởng phòng' THEN 3 WHEN N'Kiểm soát viên' THEN 4
                        WHEN N'Chuyên viên tín dụng' THEN 5 WHEN N'Giao dịch viên' THEN 6 ELSE 7 END, HoTen", cn))
            using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(result);
            return result;
        }
        // Chỉ tra cứu, không tạo bản ghi nhật ký. Lọc đủ ngày và giờ.
        public static DataTable History(DateTime? from = null, DateTime? to = null, string keyword = "")
        {
            if (!CanManageEmployees) throw new UnauthorizedAccessException("Chỉ Giám đốc xem được nhật ký của mọi nhân viên.");
            if (from.HasValue && to.HasValue && from > to)
                throw new ArgumentException("Ngày giờ bắt đầu không thể sau ngày giờ kết thúc.");
            var dt = new DataTable();
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"SELECT TOP 5000 n.ThoiGian AS [Thời gian], nv.HoTen AS [Họ tên],
                     n.TenDangNhap AS [Tài khoản], n.HanhDong AS [Hành động], n.ChiTiet AS [Chi tiết]
                     FROM dbo.NhatKyNhanVien AS n
                     LEFT JOIN dbo.NhanVienNganHang AS nv ON nv.Id = n.NhanVienId
                     WHERE (@from IS NULL OR n.ThoiGian >= @from)
                       AND (@to IS NULL OR n.ThoiGian <= @to)
                       AND (@key = N'' OR n.TenDangNhap LIKE N'%' + @key + N'%' OR nv.HoTen LIKE N'%' + @key + N'%'
                            OR n.HanhDong LIKE N'%' + @key + N'%' OR n.ChiTiet LIKE N'%' + @key + N'%')
                     ORDER BY n.ThoiGian DESC, n.Id DESC", cn))
            {
                AddParam(cmd, "@from", from.HasValue ? (object)from.Value : DBNull.Value);
                AddParam(cmd, "@to", to.HasValue ? (object)to.Value : DBNull.Value);
                AddParam(cmd, "@key", (keyword ?? "").Trim());
                using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(dt);
            }
            return dt;
        }
        // Lịch sử riêng theo ID nhân viên, bao gồm cả tài khoản đã nghỉ việc.
        // Truy vấn chỉ đọc, không tạo nhật ký xem dữ liệu.
        public static DataTable HistoryForEmployee(int employeeId, string login, DateTime? from, DateTime? to, string keyword)
        {
            if (!CanManageEmployees) throw new UnauthorizedAccessException("Chỉ Giám đốc xem được nhật ký của mọi nhân viên.");
            if (from.HasValue && to.HasValue && from.Value > to.Value)
                throw new ArgumentException("Ngày giờ bắt đầu phải trước ngày giờ kết thúc.");
            DataTable result = new DataTable();
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"SELECT TOP 5000 n.ThoiGian AS [Thời gian],
                    n.TenDangNhap AS [Tài khoản], n.HanhDong AS [Hành động], n.ChiTiet AS [Chi tiết]
                FROM dbo.NhatKyNhanVien n
                WHERE (n.NhanVienId=@id OR (n.NhanVienId IS NULL AND n.TenDangNhap=@login))
                  AND (@from IS NULL OR n.ThoiGian>=@from)
                  AND (@to IS NULL OR n.ThoiGian<=@to)
                  AND (@key=N'' OR n.HanhDong LIKE N'%' + @key + N'%'
                      OR n.ChiTiet LIKE N'%' + @key + N'%'
                      OR n.TenDangNhap LIKE N'%' + @key + N'%')
                ORDER BY n.ThoiGian DESC, n.Id DESC", cn))
            {
                AddParam(cmd, "@id", employeeId);
                AddParam(cmd, "@login", login ?? "");
                AddParam(cmd, "@from", from.HasValue ? (object)from.Value : DBNull.Value);
                AddParam(cmd, "@to", to.HasValue ? (object)to.Value : DBNull.Value);
                AddParam(cmd, "@key", (keyword ?? "").Trim());
                using (var da = new SqlDataAdapter(cmd)) da.Fill(result);
            }
            return result;
        }
        public static void AuditCurrent(string action, string details = "")
        {
            if (!IsLoggedIn) return;
            Audit(Current.Id, Current.TenDangNhap, action, details);
        }
        public static void Audit(int? id, string username, string action, string detail)
        {
            using (var cn = Open())
            using (var cmd = new SqlCommand(@"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet,ThoiGian)
                                              VALUES (@id,@login,@action,@detail,GETDATE())", cn))
            {
                AddParam(cmd, "@id", id.HasValue ? (object)id.Value : DBNull.Value);
                AddParam(cmd, "@login", username);
                AddParam(cmd, "@action", action);
                AddParam(cmd, "@detail", detail);
                cmd.ExecuteNonQuery();
            }
        }
        public static void AddEmployee(string login, string password, string name, string cccd, string phone, string address,
                                       string role, string shift, string desk, string room)
        {
            if (!CanManageEmployees || Rank(role) <= Rank(Current.ChucVu))
                throw new UnauthorizedAccessException("Bạn không được thêm nhân viên cấp bằng hoặc cao hơn.");
            if (string.IsNullOrWhiteSpace(login) || (password == null || password.Length < 10) || string.IsNullOrWhiteSpace(name) ||
                cccd == null || cccd.Length != 12 || !cccd.All(char.IsDigit) || string.IsNullOrWhiteSpace(phone) ||
                !(shift == "Sáng" || shift == "Chiều" || shift == "Cả hai"))
                throw new ArgumentException("Thông tin không hợp lệ: mật khẩu ít nhất 10 ký tự; điện thoại 10–11 chữ số.");
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash = HashPassword(password, salt);
            using (var cn = Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    CheckRoleCapacity(cn, tx, role);
                    Execute(cn, tx, @"INSERT dbo.NhanVienNganHang(TenDangNhap,MatKhauHash,MatKhauSalt,HoTen,SoDienThoai,
                        DiaChi,ChucVu,CaLam,SoMay,SoPhong,TrangThai)
                        VALUES(@login,@hash,@salt,@name,@phone,@addr,@role,@shift,@desk,@room,N'Offline')",
                        P("@login", login.Trim()), P("@hash", hash), P("@salt", salt), P("@name", name.Trim()),
                        P("@phone", phone), P("@addr", address), P("@role", role), P("@shift", shift),
                        P("@desk", role == "Giao dịch viên" ? (object)desk : DBNull.Value),
                        P("@room", role == "Giao dịch viên" ? (object)DBNull.Value : room));
                    Execute(cn, tx, @"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet)
                        VALUES(@id,@login,N'Thêm nhân viên',@detail)", P("@id", Current.Id),
                        P("@login", Current.TenDangNhap), P("@detail", "Tạo nhân viên " + login));
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }
        public static void UpdateEmployee(int id, string name, string phone, string address,
                                          string role, string shift, string desk, string room, bool fire)
        {
            if (!CanManageEmployees) throw new UnauthorizedAccessException("Không đủ quyền.");
            using (var cn = Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    string oldRole, oldStatus;
                    string oldName, oldPhone, oldAddress, oldShift, oldDesk, oldRoom;
                    using (var cmd = new SqlCommand(@"SELECT ChucVu,TrangThai,HoTen,SoDienThoai,DiaChi,CaLam,SoMay,SoPhong
                            FROM dbo.NhanVienNganHang WITH (UPDLOCK,HOLDLOCK) WHERE Id=@id", cn, tx))
                    {
                        AddParam(cmd, "@id", id);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) throw new ArgumentException("Nhân viên không tồn tại.");
                            oldRole = Convert.ToString(r["ChucVu"]);
                            oldStatus = Convert.ToString(r["TrangThai"]);
                            oldName = Convert.ToString(r["HoTen"]);
                            oldPhone = Convert.ToString(r["SoDienThoai"]);
                            oldAddress = Convert.ToString(r["DiaChi"]);
                            oldShift = Convert.ToString(r["CaLam"]);
                            oldDesk = Convert.ToString(r["SoMay"]);
                            oldRoom = Convert.ToString(r["SoPhong"]);
                        }
                    }
                    if (id == Current.Id || Rank(oldRole) <= Rank(Current.ChucVu) || Rank(role) <= Rank(Current.ChucVu))
                        throw new UnauthorizedAccessException("Không được sửa nhân viên ngang/cao cấp hoặc tự sửa.");
                    if (oldStatus == "Nghỉ việc") throw new InvalidOperationException("Nhân viên đã nghỉ việc.");
                    if (!fire && role != oldRole) CheckRoleCapacity(cn, tx, role, id);
                    if (!fire && (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) ||
                        !(shift == "Sáng" || shift == "Chiều" || shift == "Cả hai")))
                        throw new ArgumentException("Thiếu thông tin nhân viên.");
                    if (!fire && oldName == (name ?? "").Trim() && oldPhone == (phone ?? "").Trim()
                        && oldAddress == (address ?? "").Trim() && oldRole == role && oldShift == shift
                        && oldDesk == (role == "Giao dịch viên" ? (desk ?? "").Trim() : "")
                        && oldRoom == (role == "Giao dịch viên" ? "" : (room ?? "").Trim()))
                    {
                        tx.Commit(); // Không thay đổi gì thì không ghi lịch sử.
                        return;
                    }
                    int count = Execute(cn, tx, @"UPDATE dbo.NhanVienNganHang SET
                        HoTen=CASE WHEN @fire=1 THEN HoTen ELSE @name END,
                        SoDienThoai=CASE WHEN @fire=1 THEN SoDienThoai ELSE @phone END,
                        DiaChi=CASE WHEN @fire=1 THEN DiaChi ELSE @addr END,
                        ChucVu=CASE WHEN @fire=1 THEN ChucVu ELSE @role END,
                        CaLam=CASE WHEN @fire=1 THEN CaLam ELSE @shift END,
                        SoMay=CASE WHEN @fire=1 THEN SoMay ELSE @desk END,
                        SoPhong=CASE WHEN @fire=1 THEN SoPhong ELSE @room END,
                        TrangThai=CASE WHEN @fire=1 THEN N'Nghỉ việc' ELSE TrangThai END,
                        DangHoatDong=CASE WHEN @fire=1 THEN 0 ELSE DangHoatDong END
                        WHERE Id=@id AND TrangThai <> N'Nghỉ việc'",
                        P("@fire", fire), P("@name", name), P("@phone", phone), P("@addr", address), P("@role", role),
                        P("@shift", shift), P("@desk", role == "Giao dịch viên" ? (object)desk : DBNull.Value),
                        P("@room", role == "Giao dịch viên" ? (object)DBNull.Value : room), P("@id", id));
                    if (count != 1) throw new InvalidOperationException("Không cập nhật được nhân viên.");
                    Execute(cn, tx, @"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet)
                        VALUES(@id,@login,@act,@detail)", P("@id", Current.Id), P("@login", Current.TenDangNhap),
                        P("@act", fire ? "Cho nghỉ việc" : oldRole != role ? "Điều chuyển vị trí" : "Sửa nhân viên"),
                        P("@detail", "Nhân viên ID=" + id + "; " + oldRole + " -> " + (fire ? "Nghỉ việc" : role)));
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }
        // Bất kỳ nhân viên nào không phải Giám đốc đều có thể tự xin nghỉ việc.
        // Giám đốc phải dùng DirectorRetireWithSuccessor để tránh ghế giám đốc trống.
        public static void ResignCurrentEmployee()
        {
            if (!IsLoggedIn) throw new UnauthorizedAccessException("Chưa đăng nhập.");
            if (Current.ChucVu == "Giám đốc")
                throw new InvalidOperationException("Giám đốc cần chỉ định phó giám đốc kế nhiệm.");
            NhanVienSession person = Current;
            using (var cn = Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    int count = Execute(cn, tx, @"UPDATE dbo.NhanVienNganHang
                        SET TrangThai=N'Nghỉ việc', DangHoatDong=0
                        WHERE Id=@id AND TrangThai<>N'Nghỉ việc' AND ChucVu<>N'Giám đốc'", P("@id", person.Id));
                    if (count != 1) throw new InvalidOperationException("Không thể cập nhật trạng thái nghỉ việc.");
                    Execute(cn, tx, @"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet,ThoiGian)
                        VALUES(@id,@login,N'Tự nghỉ việc',@detail,GETDATE())", P("@id", person.Id),
                        P("@login", person.TenDangNhap), P("@detail", person.HoTen + " (" + person.ChucVu + ") tự xin nghỉ việc"));
                    tx.Commit();
                    Current = null; // Phiên phải kết thúc khi nghỉ việc.
                }
                catch { tx.Rollback(); throw; }
            }
        }
        // Giám đốc hiện tại xin nghỉ: bắt buộc chuyển giao cho 1 phó giám đốc đang làm.
        // Dùng chung một transaction; giữ nguyên chức vụ cũ trong hồ sơ nghỉ việc.
        public static void DirectorRetireWithSuccessor(int successorId)
        {
            if (!IsLoggedIn || Current.ChucVu != "Giám đốc")
                throw new UnauthorizedAccessException("Chỉ giám đốc hiện tại được chuyển giao chức vụ khi nghỉ việc.");
            if (successorId == Current.Id) throw new ArgumentException("Người kế nhiệm phải là phó giám đốc.");
            using (var cn = Open())
            using (var tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    using (var check = new SqlCommand(@"SELECT COUNT(1) FROM dbo.NhanVienNganHang WITH (UPDLOCK,HOLDLOCK)
                         WHERE Id=@id AND ChucVu=N'Phó giám đốc' AND TrangThai<>N'Nghỉ việc'", cn, tx))
                    {
                        AddParam(check, "@id", successorId);
                        if (Convert.ToInt32(check.ExecuteScalar()) != 1)
                            throw new InvalidOperationException("Người kế nhiệm phải là phó giám đốc đang làm việc.");
                    }
                    int a = Execute(cn, tx, @"UPDATE dbo.NhanVienNganHang
                         SET TrangThai=N'Nghỉ việc', DangHoatDong=0
                         WHERE Id=@id AND ChucVu=N'Giám đốc' AND TrangThai<>N'Nghỉ việc'",
                         P("@id", Current.Id));
                    if (a != 1) throw new InvalidOperationException("Không thể cập nhật giám đốc cũ.");
                    int b = Execute(cn, tx, @"UPDATE dbo.NhanVienNganHang SET ChucVu=N'Giám đốc'
                         WHERE Id=@id AND ChucVu=N'Phó giám đốc' AND TrangThai<>N'Nghỉ việc'",
                         P("@id", successorId));
                    if (b != 1) throw new InvalidOperationException("Không thể bổ nhiệm giám đốc mới.");
                    Execute(cn, tx, @"INSERT dbo.NhatKyNhanVien(NhanVienId,TenDangNhap,HanhDong,ChiTiet)
                         VALUES(@id,@login,N'Chuyển giao giám đốc',@detail)",
                         P("@id", Current.Id), P("@login", Current.TenDangNhap),
                         P("@detail", "Giám đốc ID=" + Current.Id + " nghỉ việc; bổ nhiệm phó giám đốc ID=" + successorId));
                    tx.Commit();
                    Current = null; // Chấm dứt phiên của giám đốc nghỉ việc.
                }
                catch { tx.Rollback(); throw; }
            }
        }
    }
}
