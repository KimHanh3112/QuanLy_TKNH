using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace QLTKNH
{
	public partial class FrmThongKe : Form
	{
		private string strConnectionString = @"Data Source= KIMHANH;Initial Catalog=QLTKNH;Integrated Security=True";

		public FrmThongKe()
		{
			InitializeComponent();
			if (!BankSecurity.IsLoggedIn) { Load += (s, e) => Close(); return; }
			SetupEvents();
			EnsureGridColumns();
			ApplyGridStyles();
			TaoDashboardChuyenNghiep();
			LoadThongKe();
		}


		// Dashboard chỉ thay đổi bố cục hiển thị. Tất cả truy vấn và xử lý nghiệp vụ bên dưới giữ nguyên.
		private TableLayoutPanel _dashboard;
		private readonly Color _bg = Color.FromArgb(15, 23, 42);
		private readonly Color _card = Color.FromArgb(27, 40, 61);
		private readonly Color _muted = Color.FromArgb(148, 163, 184);
		private readonly Color _blue = Color.FromArgb(59, 130, 246);
		private readonly Color _green = Color.FromArgb(34, 197, 94);
		private readonly Color _amber = Color.FromArgb(251, 191, 36);

		private Label TaoNhan(string text, float size, Color color, bool bold = false)
		{
			return new Label
			{
				Text = text,
				ForeColor = color,
				BackColor = Color.Transparent,
				Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft,
				AutoEllipsis = true
			};
		}

		private Panel TaoKhung()
		{
			return new Panel { Dock = DockStyle.Fill, BackColor = _card, Margin = new Padding(5), Padding = new Padding(12) };
		}

		private void ChuyenControl(Control control, Control cha)
		{
			if (control.Parent != null) control.Parent.Controls.Remove(control);
			cha.Controls.Add(control);
		}

		private Panel TaoTheChiSo(string title, Label giaTri, Color accent)
		{
			Panel p = TaoKhung();
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = _card };
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
			layout.Controls.Add(TaoNhan(title.ToUpperInvariant(), 9, _muted, true), 0, 0);
			ChuyenControl(giaTri, layout);
			giaTri.Dock = DockStyle.Fill;
			giaTri.TextAlign = ContentAlignment.MiddleLeft;
			giaTri.ForeColor = accent;
			giaTri.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
			giaTri.AutoSize = false;
			giaTri.AutoEllipsis = true;
			layout.SetCellPosition(giaTri, new TableLayoutPanelCellPosition(0, 1));
			p.Controls.Add(layout);
			return p;
		}

		private Panel TaoBieuDo(string title, PaintEventHandler ve)
		{
			Panel p = TaoKhung();
			Label heading = TaoNhan(title, 10.5F, Color.White, true);
			heading.Dock = DockStyle.Top;
			heading.Height = 27;
			p.Controls.Add(heading);
			Panel canvas = new Panel { Dock = DockStyle.Fill, BackColor = _card };
			canvas.Paint += ve;
			p.Controls.Add(canvas);
			canvas.BringToFront();
			heading.BringToFront();
			return p;
		}

		private Panel TaoBang(string title, DataGridView grid)
		{
			Panel p = TaoKhung();
			Label heading = TaoNhan(title, 10.5F, Color.White, true);
			heading.Dock = DockStyle.Top;
			heading.Height = 28;
			ChuyenControl(grid, p);
			grid.Dock = DockStyle.Fill;
			grid.Margin = Padding.Empty;
			grid.ColumnHeadersHeight = 32;
			grid.RowTemplate.Height = 27;
			grid.BringToFront();
			heading.BringToFront();
			return p;
		}

		private void TaoDashboardChuyenNghiep()
		{
			SuspendLayout();
			var old = Controls.Cast<Control>().ToArray();
			foreach (Control c in old) Controls.Remove(c);

			BackColor = _bg;
			AutoScroll = false;
			MinimumSize = new Size(850, 560);

			_dashboard = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 4,
				BackColor = _bg,
				Padding = new Padding(12, 8, 12, 10),
				Margin = Padding.Empty
			};
			_dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
			_dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 21));
			_dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 39));
			_dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
			Controls.Add(_dashboard);

			var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
			header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
			header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
			var title = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
			title.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
			title.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
			title.Controls.Add(TaoNhan("TỔNG QUAN NGÂN HÀNG", 18, Color.White, true), 0, 0);
			title.Controls.Add(TaoNhan("Báo cáo hoạt động • Dữ liệu trực tiếp từ SQL Server", 9, _muted), 0, 1);
			header.Controls.Add(title, 0, 0);

			FlowLayoutPanel filters = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.RightToLeft,
				WrapContents = false,
				AutoScroll = false,
				Padding = new Padding(0, 13, 0, 0)
			};
			ChuyenControl(btnLamMoi, filters);
			ChuyenControl(dtpDenNgay, filters);
			ChuyenControl(dtpTuNgay, filters);
			ChuyenControl(cboMocThoiGian, filters);
			btnLamMoi.Text = "Làm mới";
			btnLamMoi.BackColor = _blue;
			btnLamMoi.ForeColor = Color.White;
			btnLamMoi.FlatStyle = FlatStyle.Flat;
			btnLamMoi.FlatAppearance.BorderSize = 0;
			btnLamMoi.Width = 86;
			cboMocThoiGian.Width = 118;
			dtpTuNgay.Width = 112;
			dtpDenNgay.Width = 112;
			header.Controls.Add(filters, 1, 0);
			_dashboard.Controls.Add(header, 0, 0);

			var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
			for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
			kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
			kpis.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
			kpis.Controls.Add(TaoTheChiSo("Khách hàng", lblTongKH, _blue), 0, 0);
			kpis.Controls.Add(TaoTheChiSo("Tài khoản thanh toán", lblTongTKTT, _green), 1, 0);
			kpis.Controls.Add(TaoTheChiSo("Sổ tiết kiệm", lblTongTKTK, _amber), 2, 0);
			kpis.Controls.Add(TaoTheChiSo("Tài khoản vay", lblTongTKVay, Color.FromArgb(167, 139, 250)), 3, 0);
			kpis.Controls.Add(TaoTheChiSo("Tổng dư nợ", lblTongDuNo, _amber), 0, 1);
			kpis.Controls.Add(TaoTheChiSo("Tiền gửi tiết kiệm", lblTongTienGui, _green), 1, 1);
			kpis.Controls.Add(TaoTheChiSo("Số dư thanh toán", lblTongSoDuTT, _blue), 2, 1);
			kpis.Controls.Add(TaoTheChiSo("Doanh thu ròng", lblDoanhThuRong, _green), 3, 1);
			_dashboard.Controls.Add(kpis, 0, 1);

			var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
			charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
			charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
			charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
			charts.Controls.Add(TaoBieuDo("TỶ TRỌNG TÀI KHOẢN • BIỂU ĐỒ TRÒN", VeCoCauTaiKhoan), 0, 0);
			charts.Controls.Add(TaoBieuDo("GIAO DỊCH • ĐƯỜNG CONG", VeGiaoDich), 1, 0);
			charts.Controls.Add(TaoBieuDo("THU / CHI LÃI • BIỂU ĐỒ CỘT", VeLai), 2, 0);
			_dashboard.Controls.Add(charts, 0, 2);

			var tables = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
			tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
			tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
			tables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
			tables.Controls.Add(TaoBang("TRẠNG THÁI TÀI KHOẢN", dgvTrangThai), 0, 0);
			tables.Controls.Add(TaoBang("TOP 10 LÃI VAY", dgvTopLai), 1, 0);
			tables.Controls.Add(TaoBang("PHÂN BỔ KỲ HẠN", dgvKyHan), 2, 0);
			_dashboard.Controls.Add(tables, 0, 3);
			ResumeLayout(true);
		}

		private decimal SoTuNhan(Label label)
		{
			string value = new string((label.Text ?? "").Where(c => char.IsDigit(c) || c == '-').ToArray());
			decimal n;
			return decimal.TryParse(value, out n) ? n : 0m;
		}

		private void VeCoCauTaiKhoan(object sender, PaintEventArgs e)
		{
			var p = (Panel)sender;
			VeBieuDoTron(e.Graphics, p.ClientRectangle,
				new[] { "Thanh toán", "Tiết kiệm", "Vay" },
				new[] { SoTuNhan(lblTongTKTT), SoTuNhan(lblTongTKTK), SoTuNhan(lblTongTKVay) },
				new[] { _blue, _green, _amber });
		}

		private void VeGiaoDich(object sender, PaintEventArgs e)
		{
			var p = (Panel)sender;
			VeBieuDoDuong(e.Graphics, p.ClientRectangle,
				new[] { "Thanh toán", "Tiết kiệm", "Vay" },
				new[] { SoTuNhan(lblSoGDTT), SoTuNhan(lblSoGDTK), SoTuNhan(lblSoGDVay) },
				new[] { _blue, _green, _amber });
		}

		private void VeLai(object sender, PaintEventArgs e)
		{
			var p = (Panel)sender;
			VeBieuDoCot(e.Graphics, p.ClientRectangle,
				new[] { "Lãi thu vay", "Lãi chi TK", "Ròng" },
				new[] { SoTuNhan(lblLaiThuVay), SoTuNhan(lblLaiChiTK), SoTuNhan(lblDoanhThuRong) },
				new[] { _green, _amber, _blue });
		}

		private void VeBieuDoTron(Graphics g, Rectangle bounds, string[] names, decimal[] values, Color[] colors)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			if (bounds.Width < 120 || bounds.Height < 95) return;
			decimal total = values.Sum(v => Math.Max(0m, v));
			if (total <= 0)
			{
				using (Brush b = new SolidBrush(_muted))
					g.DrawString("Chưa có dữ liệu", new Font("Segoe UI", 10F), b, 15, bounds.Height / 2f);
				return;
			}
			float diameter = Math.Min(bounds.Height - 20, bounds.Width * 0.47f);
			RectangleF circle = new RectangleF(10, (bounds.Height - diameter) / 2f, diameter, diameter);
			float start = -90f;
			for (int i = 0; i < values.Length; i++)
			{
				float sweep = (float)(Math.Max(0m, values[i]) / total * 360m);
				if (sweep <= 0) continue;
				using (Brush brush = new SolidBrush(colors[i])) g.FillPie(brush, circle.X, circle.Y, circle.Width, circle.Height, start, sweep);
				start += sweep;
			}
			using (Brush hole = new SolidBrush(_card))
			{
				float inset = diameter * .29f;
				g.FillEllipse(hole, circle.X + inset, circle.Y + inset, diameter - inset * 2, diameter - inset * 2);
			}
			using (Font font = new Font("Segoe UI", 8.5F))
			using (Brush labelBrush = new SolidBrush(Color.White))
			{
				float x = circle.Right + 13;
				for (int i = 0; i < names.Length; i++)
				{
					float y = bounds.Height / 2f - 39 + i * 28;
					using (Brush b = new SolidBrush(colors[i])) g.FillEllipse(b, x, y + 4, 9, 9);
					string label = names[i] + "  " + (values[i] / total * 100m).ToString("0.#") + "%";
					g.DrawString(label, font, labelBrush, x + 13, y);
				}
			}
		}

		// Đường cong mềm và vùng tô gradient giống phong cách dashboard tham khảo.
		// Dữ liệu vẫn là tổng giao dịch theo từng loại tài khoản, không giả lập chuỗi thời gian.
		private void VeBieuDoDuong(Graphics g, Rectangle bounds, string[] names, decimal[] values, Color[] colors)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			if (bounds.Width < 140 || bounds.Height < 105 || values.Length < 2) return;

			float left = 38f, right = bounds.Width - 28f;
			float top = 29f, bottom = bounds.Height - 37f;
			float height = Math.Max(12f, bottom - top);
			decimal max = Math.Max(1m, values.Max(v => Math.Max(0m, v)));
			PointF[] points = new PointF[values.Length];

			using (Font font = new Font("Segoe UI", 8F))
			using (Font valueFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
			using (Brush muted = new SolidBrush(_muted))
			using (Brush white = new SolidBrush(Color.White))
			using (Pen grid = new Pen(Color.FromArgb(45, 77, 98), 1f))
			{
				for (int i = 0; i < 4; i++)
				{
					float y = top + i * height / 3f;
					g.DrawLine(grid, left, y, right, y);
				}

				for (int i = 0; i < values.Length; i++)
				{
					float x = left + (right - left) * i / (values.Length - 1f);
					float y = bottom - (float)(Math.Max(0m, values[i]) / max) * height;
					points[i] = new PointF(x, y);
					SizeF sz = g.MeasureString(names[i], font);
					g.DrawString(names[i], font, muted, x - sz.Width / 2f, bottom + 10f);
				}

				using (GraphicsPath area = new GraphicsPath())
				{
					area.AddCurve(points, 0.42f);
					area.AddLine(points[points.Length - 1], new PointF(right, bottom));
					area.AddLine(new PointF(right, bottom), new PointF(left, bottom));
					area.CloseFigure();
					using (LinearGradientBrush gradient = new LinearGradientBrush(
						new PointF(0, top), new PointF(0, bottom),
						Color.FromArgb(205, 15, 184, 166), Color.FromArgb(28, 15, 184, 166)))
						g.FillPath(gradient, area);
				}
				using (Pen glow = new Pen(Color.FromArgb(95, 45, 212, 195), 7f))
					g.DrawCurve(glow, points, 0.42f);
				using (Pen line = new Pen(Color.FromArgb(190, 255, 248), 2.8f))
					g.DrawCurve(line, points, 0.42f);

				for (int i = 0; i < points.Length; i++)
				{
					using (Brush dot = new SolidBrush(Color.FromArgb(20, 184, 166)))
						g.FillEllipse(dot, points[i].X - 4, points[i].Y - 4, 8, 8);
					using (Pen ring = new Pen(Color.FromArgb(210, 255, 248), 1.5f))
						g.DrawEllipse(ring, points[i].X - 4, points[i].Y - 4, 8, 8);
					string amount = RutGon(values[i]);
					SizeF label = g.MeasureString(amount, valueFont);
					g.DrawString(amount, valueFont, white, points[i].X - label.Width / 2f,
						Math.Max(1f, points[i].Y - 23f));
				}
			}
		}

		private void VeBieuDoCot(Graphics g, Rectangle bounds, string[] names, decimal[] values, Color[] colors)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			if (bounds.Width < 100 || bounds.Height < 80) return;
			float left = 16, top = 14, bottom = bounds.Height - 32;
			float plotHeight = Math.Max(20, bottom - top - 22);
			decimal max = Math.Max(1m, values.Max(v => Math.Abs(v)));
			float slot = (bounds.Width - 32f) / values.Length;
			using (Pen grid = new Pen(Color.FromArgb(54, 70, 94), 1))
			using (Font font = new Font("Segoe UI", 8F))
			using (Font numFont = new Font("Segoe UI", 8F, FontStyle.Bold))
			{
				for (int i = 0; i < 3; i++)
				{
					float y = top + plotHeight * i / 2f;
					g.DrawLine(grid, left, y, bounds.Width - 12, y);
				}
				for (int i = 0; i < values.Length; i++)
				{
					float h = (float)(Math.Abs(values[i]) / max) * plotHeight;
					float x = left + i * slot + slot * 0.22f;
					float w = slot * 0.56f;
					using (Brush brush = new SolidBrush(colors[i]))
						g.FillRectangle(brush, x, top + plotHeight - h, w, Math.Max(2, h));
					string amount = RutGon(values[i]);
					SizeF a = g.MeasureString(amount, numFont);
					using (Brush white = new SolidBrush(Color.White))
						g.DrawString(amount, numFont, white, x + (w - a.Width) / 2f, top + plotHeight - h - 18);
					SizeF t = g.MeasureString(names[i], font);
					using (Brush muted = new SolidBrush(_muted))
						g.DrawString(names[i], font, muted, left + i * slot + (slot - t.Width) / 2f, bottom - 2);
				}
			}
		}

		private string RutGon(decimal n)
		{
			decimal a = Math.Abs(n);
			if (a >= 1000000000m) return (n / 1000000000m).ToString("0.#") + " tỷ";
			if (a >= 1000000m) return (n / 1000000m).ToString("0.#") + " tr";
			if (a >= 1000m) return (n / 1000m).ToString("0.#") + "k";
			return n.ToString("N0");
		}

		private void SetupEvents()
		{
			cboMocThoiGian.SelectedIndexChanged += (s, e) =>
			{
				bool isCustom = cboMocThoiGian.SelectedItem?.ToString() == "Tùy chọn";
				dtpTuNgay.Enabled = isCustom;
				dtpDenNgay.Enabled = isCustom;
				LoadThongKe();
			};

			dtpTuNgay.ValueChanged += (s, e) => { if (dtpTuNgay.Enabled) LoadThongKe(); };
			dtpDenNgay.ValueChanged += (s, e) => { if (dtpDenNgay.Enabled) LoadThongKe(); };
			btnLamMoi.Click += (s, e) => LoadThongKe();
		}

		private void EnsureGridColumns()
		{
			if (dgvTrangThai.Columns.Count == 0)
			{
				dgvTrangThai.Columns.Add("Loai", "Loại TK");
				dgvTrangThai.Columns.Add("HoatDong", "Hoạt động");
				dgvTrangThai.Columns.Add("Khoa", "Khóa");
				dgvTrangThai.Columns.Add("Dong", "Đóng");
				dgvTrangThai.Columns.Add("Tong", "Tổng");
			}

			if (dgvTopLai.Columns.Count == 0)
			{
				dgvTopLai.Columns.Add("STT", "STT");
				dgvTopLai.Columns.Add("SoTK", "Số TK Vay");
				dgvTopLai.Columns.Add("HoTen", "Khách hàng");
				dgvTopLai.Columns.Add("Lai", "Tổng lãi thu");
			}

			if (dgvKyHan.Columns.Count == 0)
			{
				dgvKyHan.Columns.Add("KyHan", "Kỳ hạn");
				dgvKyHan.Columns.Add("SoLuong", "Số lượng");
				dgvKyHan.Columns.Add("TongTien", "Tổng tiền vay");
			}
		}

		/// <summary>
		/// Chỉnh màu chữ / nền để luôn nhìn thấy, không cần bôi đen
		/// </summary>
		private void ApplyGridStyles()
		{
			ApplyOneGridStyle(dgvTrangThai);
			ApplyOneGridStyle(dgvTopLai);
			ApplyOneGridStyle(dgvKyHan);
		}

		private void ApplyOneGridStyle(DataGridView dgv)
		{
			dgv.BackgroundColor = Color.FromArgb(30, 41, 59);
			dgv.BorderStyle = BorderStyle.None;
			dgv.EnableHeadersVisualStyles = false;
			dgv.RowHeadersVisible = false;
			dgv.AllowUserToAddRows = false;
			dgv.ReadOnly = true;
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.MultiSelect = false;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

			// Header
			dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 23, 42);
			dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

			// Ô dữ liệu (mặc định)
			dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
			dgv.DefaultCellStyle.ForeColor = Color.White;                 // chữ trắng
			dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235); // khi chọn: xanh
			dgv.DefaultCellStyle.SelectionForeColor = Color.White;
			dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

			// Hàng xen kẽ cho dễ nhìn
			dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(37, 50, 70);
			dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
			dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
			dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

			dgv.GridColor = Color.FromArgb(51, 65, 85);
		}

		private void LoadThongKe()
		{
			DateTime? tuNgay = null;
			DateTime? denNgay = null;
			DateTime today = DateTime.Today;

			string moc = cboMocThoiGian.SelectedItem?.ToString() ?? "Tất cả";

			switch (moc)
			{
				case "Hôm nay":
					tuNgay = today;
					denNgay = today.AddDays(1).AddSeconds(-1);
					break;
				case "Tháng này":
					tuNgay = new DateTime(today.Year, today.Month, 1);
					denNgay = tuNgay.Value.AddMonths(1).AddSeconds(-1);
					break;
				case "Năm này":
					tuNgay = new DateTime(today.Year, 1, 1);
					denNgay = new DateTime(today.Year, 12, 31, 23, 59, 59);
					break;
				case "Tùy chọn":
					tuNgay = dtpTuNgay.Value.Date;
					denNgay = dtpDenNgay.Value.Date.AddDays(1).AddSeconds(-1);
					break;
			}

			try
			{
				using (SqlConnection conn = new SqlConnection(strConnectionString))
				{
					conn.Open();

					int soTKVay = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanVay");
					int soTKTK = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.SoTietKiem");
					int soTKTT = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanThanhToan");
					int soKH = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.KhachHang");

					decimal tongTienVay = ExecuteScalarDecimal(conn, "SELECT ISNULL(SUM(SoTienVay),0) FROM dbo.TaiKhoanVay");
					decimal tongDaTra = ExecuteScalarDecimal(conn, "SELECT ISNULL(SUM(SoTienDaTra),0) FROM dbo.TaiKhoanVay");
					decimal tongDuNo = tongTienVay - tongDaTra;
					decimal tongTienGui = ExecuteScalarDecimal(conn, "SELECT ISNULL(SUM(SoTienGui - SoTienDaRut),0) FROM dbo.SoTietKiem");
					decimal tongSoDuTT = ExecuteScalarDecimal(conn, "SELECT ISNULL(SUM(SoDu),0) FROM dbo.TaiKhoanThanhToan");

					string filterDateVay = "";
					string filterDateTK = "";
					string filterDateTT = "";

					if (tuNgay.HasValue && denNgay.HasValue)
					{
						string tu = tuNgay.Value.ToString("yyyy-MM-dd HH:mm:ss");
						string den = denNgay.Value.ToString("yyyy-MM-dd HH:mm:ss");
						filterDateVay = $" AND NgayGiaoDich >= '{tu}' AND NgayGiaoDich <= '{den}'";
						filterDateTK = $" AND NgayGiaoDich >= '{tu}' AND NgayGiaoDich <= '{den}'";
						filterDateTT = $" AND ThoiGian >= '{tu}' AND ThoiGian <= '{den}'";
					}

					decimal laiThuTuVay = ExecuteScalarDecimal(conn,
						$"SELECT ISNULL(SUM(SoTienLai),0) FROM dbo.LichSuGiaoDichVay WHERE 1=1 {filterDateVay}");

					decimal laiChiTietKiem = ExecuteScalarDecimal(conn,
						$"SELECT ISNULL(SUM(SoTienLai),0) FROM dbo.LichSuGiaoDichTietKiem WHERE 1=1 {filterDateTK}");

					decimal doanhThuRong = laiThuTuVay - laiChiTietKiem;

					int soGDVay = ExecuteScalarInt(conn, $"SELECT COUNT(*) FROM dbo.LichSuGiaoDichVay WHERE 1=1 {filterDateVay}");
					int soGDTK = ExecuteScalarInt(conn, $"SELECT COUNT(*) FROM dbo.LichSuGiaoDichTietKiem WHERE 1=1 {filterDateTK}");
					int soGDTT = ExecuteScalarInt(conn, $"SELECT COUNT(*) FROM dbo.LichSuGiaoDich WHERE 1=1 {filterDateTT}");

					int vayHoatDong = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanVay WHERE TrangThai = 0");
					int vayKhoa = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanVay WHERE TrangThai = 1");
					int vayDong = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanVay WHERE TrangThai = 2");

					int tkHoatDong = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.SoTietKiem WHERE TrangThai = 0");
					int tkKhoa = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.SoTietKiem WHERE TrangThai = 1");
					int tkDong = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.SoTietKiem WHERE TrangThai = 2");

					int ttHoatDong = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanThanhToan WHERE TrangThai = 0");
					int ttKhoa = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanThanhToan WHERE TrangThai = 1");
					int ttDong = ExecuteScalarInt(conn, "SELECT COUNT(*) FROM dbo.TaiKhoanThanhToan WHERE TrangThai = 2");

					lblTongKH.Text = soKH.ToString("N0");
					lblTongTKVay.Text = soTKVay.ToString("N0");
					lblTongTKTK.Text = soTKTK.ToString("N0");
					lblTongTKTT.Text = soTKTT.ToString("N0");

					lblTongTienVay.Text = $"{tongTienVay:N0} VNĐ";
					lblTongDaTra.Text = $"{tongDaTra:N0} VNĐ";
					lblTongDuNo.Text = $"{tongDuNo:N0} VNĐ";
					lblTongTienGui.Text = $"{tongTienGui:N0} VNĐ";
					lblTongSoDuTT.Text = $"{tongSoDuTT:N0} VNĐ";

					lblLaiThuVay.Text = $"{laiThuTuVay:N0} VNĐ";
					lblLaiChiTK.Text = $"{laiChiTietKiem:N0} VNĐ";
					lblDoanhThuRong.Text = $"{doanhThuRong:N0} VNĐ";
					lblDoanhThuRong.ForeColor = doanhThuRong >= 0
						? Color.FromArgb(34, 197, 94)
						: Color.FromArgb(239, 68, 68);

					lblSoGDVay.Text = soGDVay.ToString("N0");
					lblSoGDTK.Text = soGDTK.ToString("N0");
					lblSoGDTT.Text = soGDTT.ToString("N0");
					lblTongGD.Text = (soGDVay + soGDTK + soGDTT).ToString("N0");

					// Bảng trạng thái
					dgvTrangThai.Rows.Clear();
					dgvTrangThai.Rows.Add("Tài khoản Vay", vayHoatDong, vayKhoa, vayDong, soTKVay);
					dgvTrangThai.Rows.Add("Sổ Tiết kiệm", tkHoatDong, tkKhoa, tkDong, soTKTK);
					dgvTrangThai.Rows.Add("Tài khoản TT", ttHoatDong, ttKhoa, ttDong, soTKTT);

					LoadTopLaiVay(conn, filterDateVay);
					LoadPhanBoKyHan(conn);
					if (_dashboard != null) _dashboard.Invalidate(true);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Lỗi thống kê:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadTopLaiVay(SqlConnection conn, string filterDate)
		{
			dgvTopLai.Rows.Clear();

			string sql = $@"
                SELECT TOP 10 
                    ls.SoTKVay,
                    kh.HoTen,
                    ISNULL(SUM(ls.SoTienLai), 0) AS TongLai
                FROM dbo.LichSuGiaoDichVay ls
                INNER JOIN dbo.TaiKhoanVay tk ON ls.SoTKVay = tk.SoTKVay
                INNER JOIN dbo.KhachHang kh ON tk.CCCD = kh.CCCD
                WHERE 1=1 {filterDate}
                GROUP BY ls.SoTKVay, kh.HoTen
                ORDER BY TongLai DESC";

			using (SqlCommand cmd = new SqlCommand(sql, conn))
			using (SqlDataReader rd = cmd.ExecuteReader())
			{
				int stt = 1;
				while (rd.Read())
				{
					decimal tongLai = Convert.ToDecimal(rd["TongLai"]);
					dgvTopLai.Rows.Add(
						stt++,
						rd["SoTKVay"].ToString(),
						rd["HoTen"].ToString(),
						$"{tongLai:N0} VNĐ"
					);
				}
			}

			if (dgvTopLai.Rows.Count == 0)
				dgvTopLai.Rows.Add("—", "—", "Không có dữ liệu trong khoảng thời gian", "—");
		}

		private void LoadPhanBoKyHan(SqlConnection conn)
		{
			dgvKyHan.Rows.Clear();

			string sql = @"
                SELECT 
                    KyHan,
                    COUNT(*) AS SoLuong,
                    ISNULL(SUM(SoTienVay), 0) AS TongTien
                FROM dbo.TaiKhoanVay
                GROUP BY KyHan
                ORDER BY SoLuong DESC";

			using (SqlCommand cmd = new SqlCommand(sql, conn))
			using (SqlDataReader rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					dgvKyHan.Rows.Add(
						rd["KyHan"].ToString(),
						Convert.ToInt32(rd["SoLuong"]).ToString("N0"),
						$"{Convert.ToDecimal(rd["TongTien"]):N0} VNĐ"
					);
				}
			}

			if (dgvKyHan.Rows.Count == 0)
				dgvKyHan.Rows.Add("—", "0", "Không có dữ liệu");
		}

		private int ExecuteScalarInt(SqlConnection conn, string sql)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				object result = cmd.ExecuteScalar();
				return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
			}
		}

		private decimal ExecuteScalarDecimal(SqlConnection conn, string sql)
		{
			using (SqlCommand cmd = new SqlCommand(sql, conn))
			{
				object result = cmd.ExecuteScalar();
				return result == null || result == DBNull.Value ? 0 : Convert.ToDecimal(result);
			}
		}
	}
}