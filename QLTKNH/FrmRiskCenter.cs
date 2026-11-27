using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using Label = System.Windows.Forms.Label;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLTKNH
{
    // Form độc lập, chỉ SELECT. Không sửa trạng thái, số dư hay nhóm nợ.
    public partial class FrmRiskCenter : Form
    {
        private readonly Label[] counts;
        private List<Alert> alerts = new List<Alert>();
        private bool loading;
        private bool resettingFilters;
        private sealed class BufferedRiskPanel : Panel
        {
            public BufferedRiskPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
        private readonly Panel riskBreakdown = new BufferedRiskPanel();
        private readonly System.Windows.Forms.Timer searchDelay = new System.Windows.Forms.Timer { Interval = 300 };
        private System.Threading.CancellationTokenSource reloadCancellation;
        private readonly ComboBox riskCategory = new ComboBox();
        private int[] riskTotals = new int[4];
        private sealed class Alert
        {
            public string Scope, Kind, Account, Customer, Name, Reason, Unit;
            public int Level, Category;
            public decimal Amount;
            public DateTime? Due;
        }
        public FrmRiskCenter()
        {
            InitializeComponent();
            EnableGridBuffering(grid);
            searchDelay.Tick += (s, e) => { searchDelay.Stop(); Render(); };
            Disposed += (s, e) =>
            {
                searchDelay.Stop(); searchDelay.Dispose();
                var pending = reloadCancellation;
                if (pending != null) pending.Cancel();
            };
            riskBreakdown.Dock = DockStyle.Fill;
            riskBreakdown.BackColor = Color.FromArgb(30, 41, 59);
            riskBreakdown.Paint += (s, e) =>
            {
                int total = riskTotals.Sum();
                string[] names = { "Tín dụng", "Giao dịch", "Dữ liệu / vận hành", "Tài sản bảo đảm" };
                Color[] colors = { Color.FromArgb(248, 113, 113), Color.FromArgb(251, 146, 60), Color.FromArgb(96, 165, 250), Color.FromArgb(52, 211, 153) };
                using (var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (var textFont = new Font("Segoe UI", 9F))
                using (var white = new SolidBrush(Color.White))
                {
                    e.Graphics.DrawString("PHÂN BỐ RỦI RO  |  " + total + " cảnh báo", titleFont, white, 12, 7);
                    int cellWidth = Math.Max(1, (riskBreakdown.ClientSize.Width - 24) / 4);
                    for (int i = 0; i < 4; i++)
                    {
                        int x = 12 + i * cellWidth;
                        e.Graphics.DrawString(names[i] + " : " + riskTotals[i], textFont, white, x, 31);
                        int width = Math.Max(1, cellWidth - 14);
                        using (var bg = new SolidBrush(Color.FromArgb(51, 65, 85)))
                        using (var color = new SolidBrush(colors[i]))
                        {
                            e.Graphics.FillRectangle(bg, x, 55, width, 9);
                            if (total > 0) e.Graphics.FillRectangle(color, x, 55, (float)width * riskTotals[i] / total, 9);
                        }
                    }
                }
            };
            root.Controls.Add(riskBreakdown, 0, 4);
            riskCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            riskCategory.BackColor = Color.FromArgb(30, 41, 59);
            riskCategory.ForeColor = Color.White;
            riskCategory.Width = 156;
            riskCategory.Items.AddRange(new object[] { "Mọi loại rủi ro", "Tín dụng", "Giao dịch", "Dữ liệu / vận hành", "Tài sản bảo đảm" });
            riskCategory.SelectedIndex = 0;
            filters.Controls.Add(riskCategory);
            riskCategory.SelectedIndexChanged += (s, e) => Render();
            counts = new[] { lblTotal, lblHigh, lblMedium, lblWatch };
            scope.SelectedIndex = level.SelectedIndex = 0;
            scope.SelectedIndexChanged += (s, e) => Render();
            level.SelectedIndexChanged += (s, e) => Render();
            search.TextChanged += (s, e) => { if (!resettingFilters) { searchDelay.Stop(); searchDelay.Start(); } };
            refresh.Click += async (s, e) => await Reload(true);
            btnIntegrity.Click += (s, e) => OpenIntegrity();
            Load += async (s, e) => await Reload();
        }
        private async Task Reload(bool resetFilters = false)
        {
            if (loading || IsDisposed || Disposing) return;
            if (!BankSecurity.CanUseFinance) { alerts.Clear(); Render(); status.Text = "Không có quyền xem trung tâm giám sát."; return; }
            searchDelay.Stop();
            var cancellation = new System.Threading.CancellationTokenSource();
            reloadCancellation = cancellation;
            object session = BankSecurity.Current;
            loading = true; refresh.Enabled = false; btnIntegrity.Enabled = false; status.Text = "Đang đọc dữ liệu SQL...";
            try
            {
                if (resetFilters)
                {
                    resettingFilters = true;
                    try
                    {
                        scope.SelectedIndex = 0;
                        level.SelectedIndex = 0;
                        search.Clear();
                        riskCategory.SelectedIndex = 0;
                    }
                    finally { resettingFilters = false; }
                }
                var result = await Task.Run(() => ReadAlerts(cancellation.Token), cancellation.Token);
                if (IsDisposed || Disposing) return;
                if (!BankSecurity.CanUseFinance || !ReferenceEquals(session, BankSecurity.Current)) { alerts.Clear(); Render(); status.Text = "Phiên đăng nhập không còn quyền truy cập."; return; }
                alerts = result; Render(); status.Text = "Cập nhật: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " | Mỗi tài khoản có thể có nhiều cảnh báo. Chỉ dùng để rà soát.";
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { if (!IsDisposed && !Disposing) { alerts.Clear(); Render(); status.Text = "Không tải được dữ liệu SQL: " + ex.Message; } }
            finally
            {
                reloadCancellation = null;
                cancellation.Dispose();
                loading = false;
                if (!IsDisposed && !Disposing) { refresh.Enabled = true; btnIntegrity.Enabled = true; }
            }
        }
        private void Render()
        {
            if (IsDisposed || Disposing || resettingFilters) return;
            string text = search.Text.Trim();
            var items = alerts.Where(a => (scope.SelectedIndex <= 0 || a.Scope == Convert.ToString(scope.SelectedItem)) && (text.Length == 0 || new[] { a.Account, a.Customer, a.Name }.Any(v => (v ?? "").IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0))).ToList();
            riskTotals = new[] { items.Count(a => RiskCategoryOf(a)==1),
                items.Count(a => RiskCategoryOf(a)==2), items.Count(a => RiskCategoryOf(a)==3), items.Count(a => RiskCategoryOf(a)==4) };
            riskBreakdown.Invalidate();
            items = items.Where(a => riskCategory.SelectedIndex <= 0 || RiskCategoryOf(a) == riskCategory.SelectedIndex).ToList();
            int[] totals = { items.Count, items.Count(a => a.Level == 3), items.Count(a => a.Level == 2), items.Count(a => a.Level == 1) };
            for (int i = 0; i < 4; i++) counts[i].Text = counts[i].Tag + "\n" + totals[i].ToString("N0");
            var rows = new List<DataGridViewRow>();
            grid.SuspendLayout(); try
            {
                foreach (var a in items.Where(a => level.SelectedIndex <= 0 || a.Level == 4 - level.SelectedIndex).OrderByDescending(a => a.Level).ThenBy(a => a.Due ?? DateTime.MaxValue).ThenBy(a => a.Account))
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(grid, a.Level == 3 ? "Cao" : a.Level == 2 ? "Trung bình" : "Theo dõi", a.Scope, a.Kind, a.Account, a.Customer, a.Name, a.Reason, a.Amount.ToString("N0") + " " + a.Unit, a.Due.HasValue ? a.Due.Value.ToString("dd/MM/yyyy") : "—");
                    rows.Add(row);
                    row.Cells[0].Style.ForeColor = a.Level == 3 ? Color.FromArgb(248, 113, 113) : a.Level == 2 ? Color.FromArgb(251, 146, 60) : Color.FromArgb(96, 165, 250);
                }
                grid.Rows.Clear();
                if (rows.Count > 0) grid.Rows.AddRange(rows.ToArray());
            }
            finally { grid.ResumeLayout(); }
        }
        private static void EnableGridBuffering(DataGridView target)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null) property.SetValue(target, true, null);
        }
        private static int RiskCategoryOf(Alert a)
        {
            if (a.Category > 0) return a.Category;
            if (a.Kind == "Vay" && (a.Reason.StartsWith("Nhóm nợ") || a.Reason.StartsWith("Đã quá hạn") || a.Reason.StartsWith("Dư nợ lớn") || a.Reason.StartsWith("Khoản vay đã đóng"))) return 1;
            if (a.Reason == "Tiền phong tỏa vượt số dư") return 2;
            return 3;
        }

        private static DateTime? DueDate(object opened, object term)
        {
            var m = Regex.Match(Convert.ToString(term) ?? "", @"^\s*(\d+)\s*tháng", RegexOptions.IgnoreCase);
            int months; if (!m.Success || !int.TryParse(m.Groups[1].Value, out months) || months <= 0 || months > 1200 || opened == DBNull.Value) return null;
            try { return Convert.ToDateTime(opened).Date.AddMonths(months); } catch (ArgumentOutOfRangeException) { return null; }
        }

        // Kiem tra doc lap, chi SELECT; khong thay doi danh sach canh bao hien tai.
        private void OpenIntegrity()
        {
            if (!BankSecurity.CanUseFinance || IsDisposed || Disposing) return;
            using (var dlg = new IntegrityDialog()) dlg.ShowDialog(this);
        }

        private sealed class IntegrityCheck
        {
            public string Scope, Name, State, Detail;
            public int Kind = -1; // 0: thanh toán, 1: tiết kiệm, 2: vay; -1: nhiều loại
            public bool OwnerKey; // Kiểm tra bảng liên kết chung dùng CCCD/MST, không phải STK.
            public List<string[]> Rows = new List<string[]>();
        }

        private static List<IntegrityCheck> ReadIntegrity()
        {
            var checks = new List<IntegrityCheck>();
            using (var cn = new SqlConnection(BankSecurity.ConnStr))
            {
                cn.Open();
                foreach (string p in new[] { "", "DN_" })
                {
                    string sc = p == "" ? "Cá nhân" : "Doanh nghiệp";
                    string vaiTroLienKet = p == "" ? "Đồng sở hữu" : "Người được ủy quyền";
                    string tt = "dbo." + p + "TaiKhoanThanhToan", tk = "dbo." + p + "SoTietKiem", vay = "dbo." + p + "TaiKhoanVay", kh = "dbo." + p + "KhachHang";
                    Action<string, string, bool> test = (name, sql, review) =>
                    {
                        var c = new IntegrityCheck { Scope = sc, Name = name };
                        c.OwnerKey = name.EndsWith(" (chung)", StringComparison.Ordinal);
                        if (name.StartsWith("Thanh toán - ", StringComparison.Ordinal) || name == "Số dư / phong tỏa" || name == "Số dư so với lịch sử gần nhất") c.Kind = 0;
                        else if (name.StartsWith("Tiết kiệm - ", StringComparison.Ordinal) || name == "Gốc tiết kiệm") c.Kind = 1;
                        else if (name.StartsWith("Vay - ", StringComparison.Ordinal) || name == "Gốc khoản vay" || name == "Khoản vay đóng còn dư nợ") c.Kind = 2;
                        checks.Add(c);
                        try
                        {
                            using (var cmd = new SqlCommand(sql, cn))
                            {
                                cmd.CommandTimeout = 30;
                                using (var rd = cmd.ExecuteReader())
                                    while (rd.Read()) c.Rows.Add(new[] { Convert.ToString(rd[0]), Convert.ToString(rd[1]) });
                            }
                            c.State = c.Rows.Count == 0 ? "Không phát hiện" : review ? "Cần rà soát" : "Sai lệch";
                            c.Detail = c.Rows.Count.ToString("N0") + " dòng cần kiểm tra";
                        }
                        catch (SqlException ex)
                        {
                            c.Rows.Clear(); c.State = "Chưa kiểm tra"; c.Detail = ex.Message;
                        }
                    };
                    test("Số dư / phong tỏa", "SELECT SoTKTT, CONCAT(N'Số dư: ',SoDu,N'; phong tỏa: ',SoTienPhongToa) FROM " + tt + " WHERE SoDu<0 OR SoTienPhongToa<0 OR SoTienPhongToa>SoDu OR SoDu IS NULL OR SoTienPhongToa IS NULL", false);
                    test("Gốc khoản vay", "SELECT SoTKVay, CONCAT(N'Vay: ',SoTienVay,N'; gốc đã trả: ',SoTienDaTra) FROM " + vay + " WHERE SoTienVay<0 OR SoTienDaTra<0 OR SoTienDaTra>SoTienVay OR SoTienVay IS NULL OR SoTienDaTra IS NULL", false);
                    test("Gốc tiết kiệm", "SELECT SoTKTK, CONCAT(N'Gốc còn lại: ',SoTienGui,N'; gốc đã rút: ',SoTienDaRut) FROM " + tk + " WHERE SoTienGui<0 OR SoTienDaRut<0 OR SoTienGui IS NULL OR SoTienDaRut IS NULL", false);
                    string[] tables = { tt, tk, vay };
                    string[] keys = { "SoTKTT", "SoTKTK", "SoTKVay" };
                    string[] kinds = { "Thanh toán", "Tiết kiệm", "Vay" };
                    string[] hist = { "LichSuGiaoDich", "LichSuGiaoDichTietKiem", "LichSuGiaoDichVay" };
                    string[] co = { "DongSoHuuTaiKhoanThanhToan", "DongSoHuuSoTietKiem", "DongSoHuuTaiKhoanVay" };
                    for (int i = 0; i < 3; i++)
                    {
                        string t = tables[i], k = keys[i], h = "dbo." + p + hist[i], d = "dbo." + p + co[i];
                        test(kinds[i] + " - Chủ sở hữu", "SELECT t." + k + ", CONCAT(N'Không có hồ sơ: ',t.CCCD) FROM " + t + " t LEFT JOIN " + kh + " c ON c.CCCD=t.CCCD WHERE c.CCCD IS NULL", false);
                        test(kinds[i] + " - Trạng thái", "SELECT " + k + ", CONCAT(N'Mã trạng thái: ',TrangThai) FROM " + t + " WHERE TrangThai NOT IN (0,1,2) OR TrangThai IS NULL", false);
                        // Bảng liên kết theo STK là tùy chọn trong các phiên bản SQL cũ.
                        bool coBangRieng;
                        using (var cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@table, N'U') IS NULL THEN 0 ELSE 1 END", cn))
                        {
                            cmd.Parameters.AddWithValue("@table", d);
                            coBangRieng = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                        }
                        if (coBangRieng)
                        {
                            test(kinds[i] + " - " + vaiTroLienKet, "SELECT d." + k + ", CONCAT(N'Liên kết không có tài khoản; CCCD: ',d.CCCD) FROM " + d + " d LEFT JOIN " + t + " t ON t." + k + "=d." + k + " WHERE t." + k + " IS NULL", false);
                        }
                        else
                        {
                            string chung = "dbo." + p + "DongSoHuu";
                            test(kinds[i] + " - " + vaiTroLienKet + " (chung)", "SELECT d.CCCD_ChuSoHuu, CONCAT(N'Không có hồ sơ chủ sở hữu; CCCD " + vaiTroLienKet.ToLowerInvariant() + ": ',d.CCCD) FROM " + chung + " d LEFT JOIN " + kh + " c ON c.CCCD=d.CCCD_ChuSoHuu WHERE c.CCCD IS NULL", false);
                            var ketQua = checks[checks.Count - 1];
                            if (ketQua.State == "Không phát hiện")
                                ketQua.State = "Chưa kiểm tra";
                            ketQua.Detail += "; chưa có bảng riêng theo STK; chỉ kiểm tra liên kết chủ sở hữu chung.";
                        }
                        test(kinds[i] + " - Thiếu lịch sử", "SELECT t." + k + ", N'Không có dòng lịch sử; có thể là dữ liệu cũ' FROM " + t + " t WHERE NOT EXISTS (SELECT 1 FROM " + h + " h WHERE h." + k + "=t." + k + ")", true);
                        test(kinds[i] + " - Lịch sử mồ côi", "SELECT h." + k + ", CONCAT(N'ID lịch sử: ',h.Id) FROM " + h + " h LEFT JOIN " + t + " t ON t." + k + "=h." + k + " WHERE t." + k + " IS NULL", false);
                        test(kinds[i] + " - Thiếu người thực hiện", "SELECT " + k + ", CONCAT(N'ID lịch sử: ',Id) FROM " + h + " WHERE NULLIF(LTRIM(RTRIM(NguoiThucHien)),N'') IS NULL", true);
                    }
                    test("Trùng STK trong CN hoặc DN", "SELECT x.STK, CONCAT(N'Số lần xuất hiện: ',COUNT_BIG(*)) FROM (SELECT SoTKTT AS STK FROM " + tt + " UNION ALL SELECT SoTKTK FROM " + tk + " UNION ALL SELECT SoTKVay FROM " + vay + ") x GROUP BY x.STK HAVING COUNT_BIG(*)>1", false);
                    test("Số dư so với lịch sử gần nhất", "SELECT t.SoTKTT, CONCAT(N'SQL: ',t.SoDu,N'; lịch sử: ',h.SoDuSau,N'; ID: ',h.Id) FROM " + tt + " t CROSS APPLY (SELECT TOP (1) Id,SoDuSau FROM dbo." + p + "LichSuGiaoDich h WHERE h.SoTKTT=t.SoTKTT AND h.SoDuSau IS NOT NULL ORDER BY h.ThoiGian DESC,h.Id DESC) h WHERE t.SoDu<>h.SoDuSau", true);
                    test("Khoản vay đóng còn dư nợ", "SELECT SoTKVay, CONCAT(N'Dư nợ gốc: ',SoTienVay-SoTienDaTra) FROM " + vay + " WHERE TrangThai=2 AND SoTienVay>SoTienDaTra", false);
                }
            }
            return checks;
        }


        // Tra cứu độc lập, chỉ SELECT. ID lịch sử luôn đi kèm STK, phân hệ và loại TK.
        private static string ReadFindingDetail(IntegrityCheck check, string[] finding)
        {
            string p = check.Scope == "Doanh nghiệp" ? "DN_" : "";
            string[] tables = { "TaiKhoanThanhToan", "SoTietKiem", "TaiKhoanVay" };
            string[] keys = { "SoTKTT", "SoTKTK", "SoTKVay" };
            string[] kinds = { "Thanh toán", "Tiết kiệm", "Vay" };
            string[] hist = { "LichSuGiaoDich", "LichSuGiaoDichTietKiem", "LichSuGiaoDichVay" };
            string[] fields = {
                "t.SoDu AS [Số dư hiện tại], t.SoTienPhongToa AS [Tiền phong tỏa]",
                "t.SoTienGui AS [Gốc còn lại], t.SoTienDaRut AS [Gốc đã rút], t.KyHan AS [Kỳ hạn], t.LaiSuat AS [Lãi suất]",
                "t.SoTienVay AS [Tiền vay], t.SoTienDaTra AS [Gốc đã trả], (t.SoTienVay-t.SoTienDaTra) AS [Chênh lệch gốc], t.KyHan AS [Kỳ hạn], t.LaiSuat AS [Lãi suất]"
            };
            string[] historyFields = {
                "ThoiGian AS [Ngày giao dịch], NoiDung AS [Nội dung], SoTien AS [Số tiền], SoDuSau AS [Số dư sau], NguoiThucHien AS [Người thực hiện]",
                "NgayGiaoDich AS [Ngày giao dịch], LoaiGiaoDich AS [Loại giao dịch], SoTienGoc AS [Tiền gốc], SoTienLai AS [Tiền lãi], TongTienNhan AS [Tổng tiền nhận], GhiChu AS [Ghi chú], NguoiThucHien AS [Người thực hiện]",
                "NgayGiaoDich AS [Ngày giao dịch], LoaiGiaoDich AS [Loại giao dịch], SoTienGoc AS [Tiền gốc], SoTienLai AS [Tiền lãi], PhiPhat AS [Phí phạt], TongTien AS [Tổng tiền], SoDuConLai AS [Dư nợ sau], GhiChu AS [Ghi chú], NguoiThucHien AS [Người thực hiện]"
            };
            var text = new System.Text.StringBuilder();
            text.AppendLine("KẾT QUẢ KIỂM TRA: " + check.State);
            text.AppendLine("Đối tượng: " + check.Scope);
            text.AppendLine("Nội dung: " + check.Name);
            text.AppendLine((check.OwnerKey ? "CCCD/MST chủ sở hữu: " : "Số tài khoản: ") + finding[0]);
            text.AppendLine("Chi tiết tại lần kiểm tra: " + finding[1]);
            text.AppendLine();
            // Chỉ nhận ID từ hai định dạng kết quả được ReadIntegrity tạo ra.
            var match = Regex.Match(finding[1], @"(?:^ID lịch sử: |; ID: )(\d+)$");
            long id = 0;
            bool hasId = match.Success && long.TryParse(match.Groups[1].Value, out id);
            using (var cn = new SqlConnection(BankSecurity.ConnStr))
            {
                cn.Open();
                if (check.OwnerKey)
                {
                    text.AppendLine("LIÊN KẾT CHUNG");
                    text.AppendLine("Kết quả dùng mã chủ sở hữu, không xác định được một STK cụ thể.");
                    text.AppendLine("Các tài khoản dưới đây chỉ là tài khoản cùng mã chủ, không khẳng định liên kết lỗi thuộc từng tài khoản.");
                    text.AppendLine();
                }
                for (int i = 0; i < 3; i++)
                {
                    if (!check.OwnerKey && check.Kind >= 0 && check.Kind != i) continue;
                    text.AppendLine("TÀI KHOẢN " + kinds[i].ToUpperInvariant() + " (dữ liệu hiện tại)");
                    string sql = "SELECT t." + keys[i] + " AS [Số tài khoản], t.CCCD AS [CCCD/MST], k.HoTen AS [Chủ sở hữu], "
                        + "CASE t.TrangThai WHEN 0 THEN N'Hoạt động' WHEN 1 THEN N'Đang khóa' WHEN 2 THEN N'Đã đóng' ELSE N'Không xác định' END AS [Trạng thái], "
                        + "t.NgayMo AS [Ngày mở], " + fields[i] + " FROM dbo." + p + tables[i]
                        + " t LEFT JOIN dbo." + p + "KhachHang k ON k.CCCD=t.CCCD WHERE t."
                        + (check.OwnerKey ? "CCCD" : keys[i]) + "=@key";
                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.Add("@key", System.Data.SqlDbType.VarChar, 20).Value = finding[0];
                        using (var r = cmd.ExecuteReader())
                        {
                            bool found = false;
                            while (r.Read()) { found = true; AppendFindingFields(text, r); text.AppendLine(); }
                            if (!found) text.AppendLine("Không tìm thấy tài khoản tương ứng.");
                        }
                    }
                    if (!check.OwnerKey && check.Kind == i && hasId)
                    {
                        text.AppendLine("BẢN GHI LỊCH SỬ ID " + id);
                        using (var cmd = new SqlCommand("SELECT " + historyFields[i] + " FROM dbo." + p + hist[i] + " WHERE Id=@id AND " + keys[i] + "=@key", cn))
                        {
                            cmd.Parameters.Add("@id", System.Data.SqlDbType.BigInt).Value = id;
                            cmd.Parameters.Add("@key", System.Data.SqlDbType.VarChar, 20).Value = finding[0];
                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read()) AppendFindingFields(text, r);
                                else text.AppendLine("Không còn bản ghi khớp cả ID và số tài khoản.");
                            }
                        }
                    }
                    text.AppendLine();
                }
            }
            text.AppendLine("HƯỚNG RÀ SOÁT");
            if (check.Name.EndsWith("Thiếu người thực hiện", StringComparison.Ordinal))
                text.AppendLine("Đối chiếu nhật ký và chứng từ gốc để xác định nhân viên. Không tự gán tên nhân viên hiện tại.");
            else if (check.Name.EndsWith("Thiếu lịch sử", StringComparison.Ordinal))
                text.AppendLine("Đối chiếu hồ sơ mở tài khoản/giải ngân và chứng từ. Không tạo lịch sử giả.");
            else if (check.OwnerKey)
                text.AppendLine("Kiểm tra hồ sơ chủ sở hữu và nguồn liên kết chung trước khi gắn theo từng STK.");
            else
                text.AppendLine("Đối chiếu bản ghi và chứng từ liên quan; chạy lại phép kiểm tra sau khi xác minh.");
            text.AppendLine("Chỉ đọc dữ liệu. Không tự động sửa SQL.");
            return text.ToString();
        }

        private static void AppendFindingFields(System.Text.StringBuilder text, SqlDataReader r)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                object v = r.GetValue(i);
                string value = v == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(v))
                    ? "Chưa ghi nhận" : v is DateTime ? ((DateTime)v).ToString("dd/MM/yyyy HH:mm:ss") : Convert.ToString(v);
                text.AppendLine(r.GetName(i) + ": " + value);
            }
        }

        private sealed class IntegrityDialog : Form
        {
            private readonly Button run = new Button();
            private readonly Label summary = new Label();
            private readonly DataGridView results = new DataGridView();
            private readonly DataGridView details = new DataGridView();
            private bool busy;
            private bool openingFinding;
            public IntegrityDialog()
            {
                Text = "Banking Integrity Center - Kiểm tra toàn vẹn";
                StartPosition = FormStartPosition.CenterParent;
                Size = new Size(1050, 680); MinimumSize = new Size(680, 450);
                BackColor = Color.FromArgb(15, 23, 42); ForeColor = Color.White; Font = new Font("Segoe UI", 10F);
                var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12) };
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
                var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
                run.Text = "Kiểm tra toàn bộ CN/DN"; run.AutoSize = true; run.Height = 32; run.BackColor = Color.FromArgb(37, 99, 235); run.ForeColor = Color.White; run.FlatStyle = FlatStyle.Flat;
                var close = new Button { Text = "Đóng", AutoSize = true, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
                close.Click += (s, e) => Close(); bar.Controls.Add(run); bar.Controls.Add(close);
                summary.Dock = DockStyle.Fill; summary.ForeColor = Color.FromArgb(148, 163, 184); summary.Text = "Nhấn kiểm tra để đọc SQL. Không tự sửa dữ liệu.";
                StyleGrid(results); StyleGrid(details);
                results.Columns.Add("scope", "Đối tượng"); results.Columns.Add("check", "Nội dung kiểm tra"); results.Columns.Add("state", "Kết quả"); results.Columns.Add("note", "Ghi chú");
                results.Columns[1].FillWeight = 160; results.Columns[3].FillWeight = 180;
                details.Columns.Add("account", "Số tài khoản"); details.Columns.Add("detail", "Chi tiết cần kiểm tra"); details.Columns[1].FillWeight = 300;
                layout.Controls.Add(bar, 0, 0); layout.Controls.Add(summary, 0, 1); layout.Controls.Add(results, 0, 2);
                layout.Controls.Add(new Label { Text = "CHI TIẾT — nhấp đúp dòng bên dưới để xem tài khoản / lịch sử", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(148, 163, 184) }, 0, 3);
                layout.Controls.Add(details, 0, 4); Controls.Add(layout);
                results.SelectionChanged += (s, e) => { if (!busy) ShowDetails(); };
                details.CellDoubleClick += async (s, e) => { if (e.RowIndex >= 0) await OpenFinding(e.RowIndex); };
                details.KeyDown += async (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter && details.CurrentRow != null)
                    {
                        e.Handled = true; e.SuppressKeyPress = true;
                        await OpenFinding(details.CurrentRow.Index);
                    }
                };
                run.Click += async (s, e) => await RunChecks();
            }
            private static void StyleGrid(DataGridView g)
            {
                EnableGridBuffering(g);
                g.Dock = DockStyle.Fill; g.ReadOnly = true; g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false; g.RowHeadersVisible = false; g.MultiSelect = false; g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; g.BackgroundColor = Color.FromArgb(15, 23, 42); g.EnableHeadersVisualStyles = false;
                g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59); g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                g.DefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42); g.DefaultCellStyle.ForeColor = Color.White; g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235); g.DefaultCellStyle.SelectionForeColor = Color.White;
                g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            }
            private void ShowDetails()
            {
                details.Rows.Clear();
                var c = results.CurrentRow == null ? null : results.CurrentRow.Tag as IntegrityCheck;
                if (c == null) return;
                details.Columns[0].HeaderText = c.OwnerKey ? "CCCD/MST chủ" : "Số tài khoản";
                foreach (var r in c.Rows)
                {
                    int index = details.Rows.Add(r[0], r[1]);
                    details.Rows[index].Tag = Tuple.Create(c, r);
                }
            }

            private async Task OpenFinding(int rowIndex)
            {
                if (busy || openingFinding || IsDisposed || Disposing || !BankSecurity.CanUseFinance ||
                    rowIndex < 0 || rowIndex >= details.Rows.Count) return;
                var finding = details.Rows[rowIndex].Tag as Tuple<IntegrityCheck, string[]>;
                if (finding == null) return;
                object session = BankSecurity.Current;
                openingFinding = true;
                run.Enabled = false; results.Enabled = false; details.Enabled = false;
                try
                {
                    string content = await Task.Run(() => ReadFindingDetail(finding.Item1, finding.Item2));
                    if (IsDisposed || Disposing || !BankSecurity.CanUseFinance || !ReferenceEquals(session, BankSecurity.Current)) return;
                    using (var dlg = new Form())
                    {
                        dlg.Text = "Chi tiết tài khoản cần rà soát";
                        dlg.StartPosition = FormStartPosition.CenterParent;
                        dlg.ClientSize = new Size(760, 580); dlg.MinimumSize = new Size(480, 350);
                        dlg.BackColor = Color.FromArgb(15, 23, 42); dlg.ForeColor = Color.White;
                        dlg.Font = Font; dlg.ShowInTaskbar = false; dlg.MinimizeBox = false;
                        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(14) };
                        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
                        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
                        var title = new Label { Dock = DockStyle.Fill, Text = finding.Item1.Scope + " | " + finding.Item1.Name, ForeColor = Color.FromArgb(96, 165, 250), AutoEllipsis = true };
                        var body = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Text = content };
                        var close = new Button { Text = "Đóng", Dock = DockStyle.Right, Width = 110, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White };
                        layout.Controls.Add(title, 0, 0); layout.Controls.Add(body, 0, 1); layout.Controls.Add(close, 0, 2);
                        dlg.Controls.Add(layout); dlg.CancelButton = close;
                        body.Select(0, 0);
                        dlg.ShowDialog(this);
                    }
                }
                catch (Exception ex)
                {
                    if (!IsDisposed && !Disposing && BankSecurity.CanUseFinance && ReferenceEquals(session, BankSecurity.Current))
                        MessageBox.Show(this, "Không thể tải chi tiết: " + ex.Message + "\nKết quả kiểm tra ban đầu: " + finding.Item2[1], "Tra cứu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    openingFinding = false;
                    if (!IsDisposed && !Disposing) { run.Enabled = true; results.Enabled = true; details.Enabled = true; }
                }
            }

            private async Task RunChecks()
            {
                if (busy || openingFinding || IsDisposed || Disposing || !BankSecurity.CanUseFinance) return;
                busy = true; run.Enabled = false; results.Rows.Clear(); details.Rows.Clear(); summary.Text = "Đang kiểm tra SQL...";
                try
                {
                    var checks = await Task.Run(() => ReadIntegrity());
                    if (IsDisposed || Disposing) return;
                    if (!BankSecurity.CanUseFinance) { summary.Text = "Không còn quyền truy cập."; return; }
                    foreach (var c in checks)
                    {
                        int i = results.Rows.Add(c.Scope, c.Name, c.State, c.Detail); results.Rows[i].Tag = c;
                        results.Rows[i].Cells[2].Style.ForeColor = c.State == "Sai lệch" ? Color.FromArgb(248, 113, 113) : c.State == "Không phát hiện" ? Color.FromArgb(34, 197, 94) : Color.FromArgb(251, 146, 60);
                    }
                    summary.Text = string.Format("{0} phép kiểm tra | {1} không phát hiện | {2} sai lệch | {3} cần rà soát | {4} chưa kiểm tra", checks.Count, checks.Count(c => c.State == "Không phát hiện"), checks.Count(c => c.State == "Sai lệch"), checks.Count(c => c.State == "Cần rà soát"), checks.Count(c => c.State == "Chưa kiểm tra"));
                    ShowDetails();
                }
                catch (Exception ex) { if (!IsDisposed && !Disposing) summary.Text = "Không hoàn tất kiểm tra: " + ex.Message; }
                finally { busy = false; if (!IsDisposed && !Disposing) run.Enabled = true; }
            }
        }

        private static List<Alert> ReadAlerts(System.Threading.CancellationToken token)
        {
            var result = new List<Alert>(); DateTime today = DateTime.Today;
            using (var cn = new SqlConnection(BankSecurity.ConnStr))
            {
                cn.Open();
                foreach (bool dn in new[] { false, true })
                {
                    // Tên bảng chỉ lấy từ hai hằng nội bộ, không nhận từ người dùng.
                    string p = dn ? "DN_" : "", sc = dn ? "Doanh nghiệp" : "Cá nhân";
                    string[] kinds = { "Thanh toán", "Tiết kiệm", "Vay" };
                    string[] queries ={
                        "SELECT t.SoTKTT,t.CCCD,k.HoTen,t.TrangThai,t.SoDu,t.SoTienPhongToa,t.LoaiTienTe FROM dbo."+p+"TaiKhoanThanhToan t LEFT JOIN dbo."+p+"KhachHang k ON k.CCCD=t.CCCD",
                        "SELECT t.SoTKTK,t.CCCD,k.HoTen,t.TrangThai,t.SoTienGui,t.SoTienDaRut,t.NgayMo,t.KyHan FROM dbo."+p+"SoTietKiem t LEFT JOIN dbo."+p+"KhachHang k ON k.CCCD=t.CCCD",
                        "SELECT t.SoTKVay,t.CCCD,k.HoTen,t.TrangThai,t.SoTienVay,t.SoTienDaTra,t.NgayMo,t.KyHan,n.NhomNo FROM dbo."+p+"TaiKhoanVay t LEFT JOIN dbo."+p+"KhachHang k ON k.CCCD=t.CCCD LEFT JOIN dbo."+p+"NhomNoTaiKhoan n ON n.SoTaiKhoan=t.SoTKVay"};
                    for (int i = 0; i < queries.Length; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        using (var cmd = new SqlCommand(queries[i], cn)) using (var r = cmd.ExecuteReader()) while (r.Read())
                            {
                                token.ThrowIfCancellationRequested();
                                string account = Convert.ToString(r[0]), customer = Convert.ToString(r[1]), name = Convert.ToString(r[2]); int state = r.IsDBNull(3) ? -1 : Convert.ToInt32(r[3]); decimal a = r.IsDBNull(4) ? 0 : Convert.ToDecimal(r[4]), b = r.IsDBNull(5) ? 0 : Convert.ToDecimal(r[5]); DateTime? due = i == 0 ? null : DueDate(r[6], r[7]); decimal amount = i == 2 ? Math.Max(0, a - b) : a;
                                Action<int, string, decimal> add = (severity, reason, money) => result.Add(new Alert { Scope = sc, Kind = kinds[i], Account = account, Customer = customer, Name = name, Level = severity, Reason = reason, Amount = money, Due = due, Unit = i == 0 ? Convert.ToString(r[6]) : "VND" });
                                if (r.IsDBNull(2)) add(3, "Không tìm thấy hồ sơ chủ tài khoản", amount);
                                if (state < 0 || state > 2) add(3, "Mã trạng thái không hợp lệ", amount);
                                if (r.IsDBNull(4) || r.IsDBNull(5)) add(3, "Thiếu dữ liệu số tiền", amount);
                                if (a < 0 || b < 0) add(3, "Số liệu tiền âm", amount);
                                if (i == 0) { if (b > a && b > 0) add(3, "Tiền phong tỏa vượt số dư", b); }
                                if (i == 2)
                                {
                                    if (b > a) add(3, "Gốc đã trả vượt tiền vay", b); if (state == 2 && amount > 0) add(3, "Khoản vay đã đóng nhưng còn dư nợ", amount); if (state != 2 && amount > 0)
                                    {
                                        int group = r.IsDBNull(8) ? 1 : Convert.ToInt32(r[8]);
                                        int severity = group >= 4 ? 3 : group == 3 ? 2 : group == 2 ? 1 : 0;
                                        if (!due.HasValue) add(2, "Không xác định được ngày đáo hạn", amount);
                                        else if (due.Value < today)
                                        {
                                            int days = (today - due.Value).Days;
                                            severity = Math.Max(severity, days > 90 ? 3 : days >= 10 ? 2 : 1);
                                            add(severity, "Đã quá hạn " + days + " ngày" + (group > 1 ? " | Nhóm nợ SQL: " + group : ""), amount);
                                        }
                                        else if (severity > 0) add(severity, "Nhóm nợ lưu trong SQL: " + group, amount);
                                        if (amount >= 500000000m) add(1, "Dư nợ lớn từ 500 triệu VND", amount);
                                    }
                                }
                                if (i == 1 && state != 2 && a > 0) { if (!due.HasValue) add(2, "Không xác định được ngày đáo hạn", a); }
                            }
                    }
                    token.ThrowIfCancellationRequested();
                    ReadExtraRisks(cn, p, sc, today, result, token);
                }
            }
            return result;
        }
        // Quy tắc giám sát minh họa; chỉ đọc SQL, không sửa số dư hay lịch sử.
        private static void ReadExtraRisks(SqlConnection cn, string p, string sc, DateTime today, List<Alert> result, System.Threading.CancellationToken token)
        {
            string tt = "dbo." + p + "TaiKhoanThanhToan", kh = "dbo." + p + "KhachHang";
            string hist = "dbo." + p + "LichSuGiaoDich", tk = "dbo." + p + "SoTietKiem";
            // Gom theo STK và ngày ngay tại SQL, giữ nguyên ba quy tắc chuyển khoản.
            string sql = "SELECT t.SoTKTT,t.CCCD,k.HoTen,t.LoaiTienTe,CAST(h.ThoiGian AS date), " +
                "SUM(ABS(h.SoTien)),MAX(ABS(h.SoTien)),COUNT_BIG(*),t.HanMucLan,t.HanMucNgay " +
                "FROM " + hist + " h JOIN " + tt + " t ON t.SoTKTT=h.SoTKTT LEFT JOIN " + kh + " k ON k.CCCD=t.CCCD " +
                "WHERE h.ThoiGian>=@start AND h.ThoiGian<@end AND h.NoiDung LIKE N'Chuyển khoản đến%' AND h.SoTien IS NOT NULL AND h.SoTien<>0 " +
                "GROUP BY t.SoTKTT,t.CCCD,k.HoTen,t.LoaiTienTe,CAST(h.ThoiGian AS date),t.HanMucLan,t.HanMucNgay";
            token.ThrowIfCancellationRequested();
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@start", today.AddDays(-6));
                cmd.Parameters.AddWithValue("@end", today.AddDays(1));
                using (var r = cmd.ExecuteReader()) while (r.Read())
                    {
                        token.ThrowIfCancellationRequested();
                        decimal total = Convert.ToDecimal(r[5]), max = Convert.ToDecimal(r[6]);
                        long count = Convert.ToInt64(r[7]);
                        decimal limit = r.IsDBNull(8) ? 0 : Convert.ToDecimal(r[8]);
                        decimal daily = r.IsDBNull(9) ? 0 : Convert.ToDecimal(r[9]);
                        string account = Convert.ToString(r[0]), customer = Convert.ToString(r[1]), name = Convert.ToString(r[2]), unit = Convert.ToString(r[3]);
                        string day = Convert.ToDateTime(r[4]).ToString("dd/MM/yyyy");
                        Action<int, string, decimal> add = (l, n, m) => result.Add(new Alert { Scope = sc, Kind = "Thanh toán", Account = account, Customer = customer, Name = name, Unit = unit, Amount = m, Category = 2, Level = l, Reason = n + " | " + day });
                        if (limit > 0 && max >= limit * 0.8m) add(1, "Chuyển khoản từ 80% hạn mức/lần hiện tại", max);
                        if (count >= 5) add(2, "Có " + count + " lượt chuyển khoản đi trong ngày", total);
                        if (daily > 0 && total > daily) add(2, "Tổng chuyển đi vượt hạn mức ngày hiện tại; cần đối chiếu lịch sử hạn mức", total);
                    }
            }
            token.ThrowIfCancellationRequested();
            using (var cmd = new SqlCommand("SELECT t.SoTKTT,t.CCCD,k.HoTen,t.LoaiTienTe,t.SoDu,h.SoDuSau FROM " + tt + " t LEFT JOIN " + kh + " k ON k.CCCD=t.CCCD CROSS APPLY (SELECT TOP(1) SoDuSau FROM " + hist + " h WHERE h.SoTKTT=t.SoTKTT AND h.SoDuSau IS NOT NULL ORDER BY ThoiGian DESC,Id DESC) h WHERE t.SoDu<>h.SoDuSau", cn))
            using (var r = cmd.ExecuteReader()) while (r.Read()) result.Add(new Alert { Scope = sc, Kind = "Thanh toán", Account = Convert.ToString(r[0]), Customer = Convert.ToString(r[1]), Name = Convert.ToString(r[2]), Unit = Convert.ToString(r[3]), Amount = Math.Abs(Convert.ToDecimal(r[4]) - Convert.ToDecimal(r[5])), Category = 3, Level = 2, Reason = "Số dư khác lịch sử gần nhất; cần đối chiếu" });
            token.ThrowIfCancellationRequested();
            var pledges = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT SoTKVay,GhiChu FROM dbo." + p + "LichSuGiaoDichVay WHERE LoaiGiaoDich=N'Giải ngân' AND GhiChu IS NOT NULL", cn))
            using (var r = cmd.ExecuteReader()) while (r.Read())
                {
                    token.ThrowIfCancellationRequested();
                    string note = Convert.ToString(r[1]);
                    var m = Regex.Match(note, @"^Vay thế chấp sổ (?:nội bộ:\s*|tiết kiệm Trong ngân hàng; Số sổ:\s*)([^;]+)", RegexOptions.IgnoreCase);
                    if (!m.Success) continue;
                    string key = m.Groups[1].Value.Trim(); if (key.Length == 0) continue;
                    HashSet<string> set; if (!pledges.TryGetValue(key, out set)) pledges[key] = set = new HashSet<string>();
                    set.Add(Convert.ToString(r[0]));
                }
            token.ThrowIfCancellationRequested();
            if (!pledges.Values.Any(set => set.Count > 1)) return;
            using (var cmd = new SqlCommand("SELECT t.SoTKTK,t.CCCD,k.HoTen,t.SoTienGui FROM " + tk + " t LEFT JOIN " + kh + " k ON k.CCCD=t.CCCD", cn))
            using (var r = cmd.ExecuteReader()) while (r.Read())
                {
                    HashSet<string> set; string key = Convert.ToString(r[0]);
                    if (pledges.TryGetValue(key, out set) && set.Count > 1) result.Add(new Alert { Scope = sc, Kind = "Tiết kiệm", Account = key, Customer = Convert.ToString(r[1]), Name = Convert.ToString(r[2]), Unit = "VND", Amount = Convert.ToDecimal(r[3]), Category = 4, Level = 3, Reason = "Sổ đã dùng cho " + set.Count + " khoản vay khác nhau" });
                }
        }
    }
}
