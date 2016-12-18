using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotorDriver_VComTool
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        VComControl VCom = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        //初期化
        private void WindowInit(object sender, EventArgs e)
        {
            VCom = new VComControl(this);
            VCom.DataReceived += WriteData; 
        }

        //受信したデータの書き込み
        public void WriteData(byte[] data)
        {
            textBox_Result.Text += Encoding.ASCII.GetString(data);
            textBox_Result.ScrollToEnd();
        }

        //Send(送信)ボタンを押したときの処理
        private void Send_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (VCom.isConnected())
            {
                string command = textBox_Command.Text + "\r";
                VCom.Send(command);
            }
        }

        //Open(開く)/Close(閉じる)ボタンを押したときの処理
        private void Open_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (!VCom.isConnected())
            {
                int portNum;
                int.TryParse(textBox_PortNum.Text, out portNum);
                if (VCom.Start(portNum))
                {
                    button_Open.Content = "Close";
                }
            }
            else
            {
                VCom.Stop();
                button_Open.Content = "Open";
            }
        }

        //終了時の処理
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VCom.Stop();
        }
    }
}
