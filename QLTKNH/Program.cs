using System;
using System.Windows.Forms;

namespace QLTKNH
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// Đăng nhập -> màn hình chính -> đăng xuất -> đăng nhập lại.
			// Đóng/hủy FrmDangNhap sẽ kết thúc ứng dụng.
			while (true)
			{
				// Đảm bảo mỗi vòng đăng nhập là một phiên mới.
				BankSecurity.Logout();

				using (FrmDangNhap login = new FrmDangNhap())
				{
					if (login.ShowDialog() != DialogResult.OK || !BankSecurity.IsLoggedIn)
						break;
				}

				try
				{
					using (FrmMain main = new FrmMain())
					{
						Application.Run(main);
					}
				}
				finally
				{
					// Cả khi đóng cửa sổ bằng nút X cũng không giữ phiên cũ.
					BankSecurity.Logout();
				}
			}
		}
	}
}
