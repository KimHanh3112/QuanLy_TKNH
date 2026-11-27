using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace QLTKNH
{
    // Form độc lập, chỉ SELECT. Không sửa trạng thái, số dư hay nhóm nợ.
    public partial class FrmNotification : Form
    {
        private readonly Label[] counts;
        private List<Alert> alerts = new List<Alert>();
        private bool loading;
        // Dùng chung trong một phiên đăng nhập; không ghi SQL.
        private static object readSession;
        private static readonly HashSet<string> readKeys = new HashSet<string>(StringComparer.Ordinal);
        private static List<Alert> badgeAlerts;
        private static long badgeVersion;
        private static bool badgeLoading;
        public static event EventHandler UnreadCountChanged;

        private static void EnsureReadSession()
        {
            if (ReferenceEquals(readSession, BankSecurity.Current)) return;
            readSession = BankSecurity.Current;
            readKeys.Clear();
            badgeAlerts = null;
            badgeLoading = false;
            ++badgeVersion;
        }

        public static int? UnreadCount
        {
            get
            {
                EnsureReadSession();
                if (!BankSecurity.CanUseFinance || badgeAlerts == null) return null;
                return badgeAlerts.Count(a => !readKeys.Contains(Key(a)));
            }
        }

        private static void PublishBadge(List<Alert> values)
        {
            EnsureReadSession();
            badgeAlerts = values;
            ++badgeVersion;
            UnreadCountChanged?.Invoke(null, EventArgs.Empty);
        }

        // Đọc một lần khi mở Main; không chạy timer hoặc ghi dữ liệu.
        public static async Task InitializeBadgeAsync()
        {
            EnsureReadSession();
            if (!BankSecurity.CanUseFinance || badgeAlerts != null || badgeLoading) return;
            object session = BankSecurity.Current;
            long version = badgeVersion;
            badgeLoading = true;
            try
            {
                var result = await Task.Run(() => ReadAlerts());
                if (ReferenceEquals(session, BankSecurity.Current) && BankSecurity.CanUseFinance && version == badgeVersion)
                    PublishBadge(result);
            }
            catch (Exception)
            {
                // Không làm gián đoạn Main khi SQL không đọc được; số đếm chưa xác định.
            }
            finally
            {
                if (ReferenceEquals(session, readSession)) badgeLoading = false;
            }
        }
        private static string Key(Alert a) { return string.Join("|", a.Scope, a.Kind, a.Account, a.Reason, a.Due.HasValue ? a.Due.Value.ToString("yyyyMMdd") : ""); }
        private Font unreadFont;
        private bool detailOpen;
        private bool resettingFilters;
        private bool rendering;

        private void UpdateDetailButton()
        {
            if (IsDisposed || Disposing) return;
            markAll.Enabled = !loading && !detailOpen && BankSecurity.CanUseFinance && alerts.Any(a => !readKeys.Contains(Key(a)));
            markRead.Enabled = !loading && !rendering && !detailOpen && BankSecurity.CanUseFinance &&
                grid.CurrentRow != null && grid.CurrentRow.Selected && grid.CurrentRow.Tag is Alert;
        }

        private async Task RefreshAll()
        {
            if (loading || detailOpen || IsDisposed || Disposing) return;
            resettingFilters = true;
            try
            {
                scope.SelectedIndex = level.SelectedIndex = readFilter.SelectedIndex = 0;
                search.Clear();
            }
            finally { resettingFilters = false; }
            await Reload();
        }

        private void ShowSelectedDetail(int rowIndex = -1)
        {
            if (loading || rendering || detailOpen || !BankSecurity.CanUseFinance || IsDisposed || Disposing) return;
            if (rowIndex >= grid.Rows.Count) return;
            DataGridViewRow row = rowIndex >= 0 ? grid.Rows[rowIndex] : grid.CurrentRow;
            var a = row == null || (rowIndex < 0 && !row.Selected) ? null : row.Tag as Alert;
            if (a == null)
            {
                status.Text = "Vui lòng chọn một thông báo để xem chi tiết.";
                return;
            }
            object detailSession = BankSecurity.Current;
            detailOpen = true;
            UpdateDetailButton();
            try
            {
                using (var dialog = new Form())
                using (var detailFont = new Font("Segoe UI", 10F))
                using (var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold))
                {
                    dialog.Text = "Chi tiết thông báo";
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.ClientSize = new Size(680, 470);
                    dialog.MinimumSize = new Size(480, 350);
                    dialog.BackColor = Color.FromArgb(15, 23, 42);
                    dialog.ForeColor = Color.White;
                    dialog.Font = detailFont;
                    dialog.ShowInTaskbar = false;
                    dialog.MinimizeBox = false;
                    dialog.MaximizeBox = false;
                    var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 3 };
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                    var title = new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = a.Reason,
                        Font = titleFont,
                        ForeColor = a.Level == 3 ? Color.FromArgb(251, 146, 60) : a.Level == 2 ? Color.FromArgb(250, 204, 21) : Color.FromArgb(96, 165, 250)
                    };
                    var body = new TextBox
                    {
                        Dock = DockStyle.Fill,
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        BackColor = Color.FromArgb(30, 41, 59),
                        ForeColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Text = string.Join(Environment.NewLine + Environment.NewLine, new[] {
                            "Thời hạn: " + PhaseText(a.Level),
                            "Đối tượng: " + a.Scope + " | " + a.Kind,
                            "Số tài khoản: " + a.Account,
                            "CCCD/MST: " + a.Customer,
                            "Khách hàng: " + a.Name,
                            "Nội dung: " + a.Reason,
                            "Số tiền liên quan: " + a.Amount.ToString("N0") + " VNĐ",
                            "Ngày đáo hạn: " + (a.Due.HasValue ? a.Due.Value.ToString("dd/MM/yyyy") : "Không áp dụng / chưa xác định"),
                            "Nhắc nhở nghiệp vụ, không tự tất toán hoặc thanh toán tài khoản."
                        })
                    };
                    var close = new Button
                    {
                        Text = "Đóng",
                        DialogResult = DialogResult.OK,
                        Dock = DockStyle.Right,
                        Width = 120,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(37, 99, 235),
                        ForeColor = Color.White
                    };
                    layout.Controls.Add(title, 0, 0);
                    layout.Controls.Add(body, 0, 1);
                    layout.Controls.Add(close, 0, 2);
                    dialog.Controls.Add(layout);
                    dialog.AcceptButton = close;
                    dialog.CancelButton = close;
                    dialog.ShowDialog(this);
                }
                // Chỉ đánh dấu sau khi hộp thoại chi tiết đã đóng.
                if (!IsDisposed && !Disposing && BankSecurity.CanUseFinance && ReferenceEquals(detailSession, BankSecurity.Current))
                {
                    EnsureReadSession();
                    readKeys.Add(Key(a));
                    Render();
                    UnreadCountChanged?.Invoke(null, EventArgs.Empty);
                }
            }
            finally { detailOpen = false; UpdateDetailButton(); }
        }
        private const int WarningDays = 7;
        private sealed class Alert
        {
            public string Scope, Kind, Account, Customer, Name, Reason;
            public int Level;
            public decimal Amount;
            public DateTime? Due;
        }
        public FrmNotification()
        {
            InitializeComponent();
            EnsureReadSession();
            unreadFont = new Font(grid.Font, FontStyle.Bold);
            Disposed += (s, e) => { if (unreadFont != null) { unreadFont.Dispose(); unreadFont = null; } };
            counts = new[] { lblTotal, lblHigh, lblMedium, lblWatch };
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            scope.SelectedIndex = level.SelectedIndex = readFilter.SelectedIndex = 0;
            readFilter.SelectedIndexChanged += (s, e) => Render();
            markRead.Click += (s, e) => ShowSelectedDetail();
            markAll.Click += (s, e) => MarkAllRead();
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ShowSelectedDetail(e.RowIndex); };
            scope.SelectedIndexChanged += (s, e) => Render();
            level.SelectedIndexChanged += (s, e) => Render();
            search.TextChanged += (s, e) => Render();
            grid.SelectionChanged += (s, e) => UpdateDetailButton();
            grid.CurrentCellChanged += (s, e) => UpdateDetailButton();
            refresh.Click += async (s, e) => await RefreshAll();
            UpdateDetailButton();
            Load += async (s, e) => { if (!DesignMode && System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime) await Reload(); };
        }
        private async Task Reload()
        {
            if (loading || IsDisposed || Disposing) return;
            if (!BankSecurity.CanUseFinance) { alerts.Clear(); Render(); status.Text = "Không có quyền xem trung tâm thông báo."; return; }
            EnsureReadSession();
            object session = BankSecurity.Current;
            loading = true; refresh.Enabled = false; status.Text = "Đang đọc dữ liệu SQL...";
            scope.Enabled = level.Enabled = readFilter.Enabled = search.Enabled = false;
            grid.Enabled = false;
            UpdateDetailButton();
            try
            {
                var result = await Task.Run(() => ReadAlerts());
                if (IsDisposed || Disposing) return;
                if (!BankSecurity.CanUseFinance || !ReferenceEquals(session, BankSecurity.Current)) { alerts.Clear(); Render(); status.Text = "Phiên đăng nhập không còn quyền truy cập."; return; }
                alerts = result; Render(); PublishBadge(result); status.Text = "Cập nhật: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " | Nhấp đôi để xem chi tiết; trạng thái đọc lưu trong phiên đăng nhập.";
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                {
                    alerts.Clear(); Render();
                    if (ReferenceEquals(session, BankSecurity.Current)) PublishBadge(null);
                    status.Text = "Không tải được dữ liệu SQL: " + ex.Message;
                }
            }
            finally
            {
                loading = false;
                if (!IsDisposed && !Disposing)
                {
                    refresh.Enabled = true;
                    scope.Enabled = level.Enabled = readFilter.Enabled = search.Enabled = true;
                    grid.Enabled = true;
                    UpdateDetailButton();
                }
            }
        }
        private void Render()
        {
            if (resettingFilters || rendering || IsDisposed || Disposing) return;
            EnsureReadSession();
            if (!BankSecurity.CanUseFinance) alerts.Clear();
            string text = search.Text.Trim();
            var items = alerts.Where(a => (scope.SelectedIndex <= 0 || a.Scope == Convert.ToString(scope.SelectedItem)) && (text.Length == 0 || new[] { a.Account, a.Customer, a.Name }.Any(v => (v ?? "").IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))).ToList();
            int[] totals = { items.Count, items.Count(a => !readKeys.Contains(Key(a))), items.Count(a => a.Level >= 2), items.Count(a => a.Level == 1) };
            for (int i = 0; i < 4; i++) counts[i].Text = counts[i].Tag + "\n" + totals[i].ToString("N0");
            rendering = true;
            grid.SuspendLayout(); try
            {
                grid.Rows.Clear(); foreach (var a in items.Where(a => (level.SelectedIndex <= 0 || a.Level == 4 - level.SelectedIndex) && (readFilter.SelectedIndex <= 0 || (readFilter.SelectedIndex == 1 && !readKeys.Contains(Key(a))) || (readFilter.SelectedIndex == 2 && readKeys.Contains(Key(a))))).OrderByDescending(a => a.Level).ThenBy(a => a.Due ?? DateTime.MaxValue).ThenBy(a => a.Account))
                {
                    int row = grid.Rows.Add(PhaseText(a.Level), a.Scope, a.Kind, a.Account, a.Customer, a.Name, a.Reason, a.Amount.ToString("N0"), a.Due.HasValue ? a.Due.Value.ToString("dd/MM/yyyy") : "—", readKeys.Contains(Key(a)) ? "Đã đọc" : "Chưa đọc");
                    grid.Rows[row].Tag = a;
                    grid.Rows[row].DefaultCellStyle.Font = readKeys.Contains(Key(a)) ? grid.Font : unreadFont;
                    grid.Rows[row].Cells[0].Style.ForeColor = a.Level == 3 ? Color.FromArgb(251, 146, 60) : a.Level == 2 ? Color.FromArgb(250, 204, 21) : Color.FromArgb(96, 165, 250);
                }
            }
            finally
            {
                grid.ClearSelection(); grid.CurrentCell = null; grid.ResumeLayout();
                rendering = false;
                UpdateDetailButton();
            }
        }
        private static DateTime? DueDate(object opened, object term)
        {
            var m = Regex.Match(Convert.ToString(term) ?? "", @"^\s*(\d+)\s*tháng", RegexOptions.IgnoreCase);
            int months; if (!m.Success || !int.TryParse(m.Groups[1].Value, out months) || months <= 0 || months > 1200 || opened == DBNull.Value) return null;
            try { return Convert.ToDateTime(opened).Date.AddMonths(months); } catch (ArgumentOutOfRangeException) { return null; }
        }
        private static List<Alert> ReadAlerts()
        {
            var result = new List<Alert>();
            DateTime today = DateTime.Today;
            using (var cn = new SqlConnection(BankSecurity.ConnStr))
            {
                cn.Open();
                foreach (string p in new[] { "", "DN_" })
                {
                    string sc = p == "" ? "Cá nhân" : "Doanh nghiệp";
                    string[] queries = {
                        "SELECT t.SoTKTK,t.CCCD,k.HoTen,t.SoTienGui,t.NgayMo,t.KyHan FROM dbo."+p+"SoTietKiem t INNER JOIN dbo."+p+"KhachHang k ON k.CCCD=t.CCCD WHERE t.TrangThai IN (0,1) AND t.SoTienGui>0",
                        "SELECT t.SoTKVay,t.CCCD,k.HoTen,t.SoTienVay-t.SoTienDaTra,t.NgayMo,t.KyHan FROM dbo."+p+"TaiKhoanVay t INNER JOIN dbo."+p+"KhachHang k ON k.CCCD=t.CCCD WHERE t.TrangThai IN (0,1) AND t.SoTienDaTra>=0 AND t.SoTienVay>t.SoTienDaTra"
                    };
                    for (int i = 0; i < queries.Length; i++)
                        using (var cmd = new SqlCommand(queries[i], cn))
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                DateTime? due = DueDate(r[4], r[5]);
                                if (!due.HasValue) continue;
                                int days = (due.Value - today).Days;
                                if (days > WarningDays || (i == 1 && days < 0)) continue;
                                int phase = days < 0 ? 3 : days == 0 ? 2 : 1;
                                string reason = i == 0
                                    ? (phase == 3 ? "Sổ đã đáo hạn - rà soát tất toán" : phase == 2 ? "Sổ tiết kiệm đáo hạn hôm nay" : "Sổ tiết kiệm sắp đáo hạn")
                                    : (phase == 2 ? "Khoản vay đến hạn hợp đồng hôm nay" : "Khoản vay sắp đến hạn hợp đồng");
                                result.Add(new Alert
                                {
                                    Scope = sc,
                                    Kind = i == 0 ? "Tiết kiệm" : "Vay",
                                    Account = Convert.ToString(r[0]),
                                    Customer = Convert.ToString(r[1]),
                                    Name = Convert.ToString(r[2]),
                                    Amount = Convert.ToDecimal(r[3]),
                                    Due = due,
                                    Level = phase,
                                    Reason = reason
                                });
                            }
                        }
                }
            }
            return result;
        }

        private static string PhaseText(int phase)
        {
            return phase == 3 ? "Đã đến hạn" : phase == 2 ? "Hôm nay" : "Sắp đến hạn";
        }

        private void MarkAllRead()
        {
            if (loading || detailOpen || rendering || IsDisposed || Disposing || !BankSecurity.CanUseFinance) return;
            EnsureReadSession();
            foreach (var a in alerts) readKeys.Add(Key(a));
            Render();
            UnreadCountChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
