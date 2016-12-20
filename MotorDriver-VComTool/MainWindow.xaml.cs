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
using System.IO.Ports;

namespace MotorDriver_VComTool
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        VComControl VCom = null;
        List<string> commandHistory = new List<string>();
        int historyIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        //初期化
        private void WindowInit(object sender, EventArgs e)
        {
            VCom = new VComControl(this);
            VCom.DataReceived += WriteData; 
            foreach(string portName in VCom.GetPortList())
            {
                comboBox_ComPort.Items.Add(portName);
            }
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

                if (commandHistory.Count == 0
                    || !commandHistory[commandHistory.Count - 1].Equals(textBox_Command.Text))
                {
                    commandHistory.Add(textBox_Command.Text);
                }
                textBox_Command.Clear();
                historyIndex = commandHistory.Count;
            }
        }

        //Open(開く)/Close(閉じる)ボタンを押したときの処理
        private void Open_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (!VCom.isConnected())
            {
                if (VCom.Start(
                    comboBox_ComPort.SelectionBoxItem.ToString(),
                    int.Parse(comboBox_BaudRate.SelectionBoxItem.ToString()),
                    GetParityCode(comboBox_Parity.SelectionBoxItem.ToString()),
                    int.Parse(comboBox_DataBits.SelectionBoxItem.ToString()),
                    GetStopBitsCode(comboBox_StopBits.SelectionBoxItem.ToString())))
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

        private Parity GetParityCode(string parityString)
        {
            if (parityString.Equals("None")) return Parity.None;
            else if (parityString.Equals("Odd")) return Parity.Odd;
            else if (parityString.Equals("Even")) return Parity.Even;
            else if (parityString.Equals("Mark")) return Parity.Mark;
            else if (parityString.Equals("Space")) return Parity.Space;
            else return Parity.None; // Unknown parity
        }

        private StopBits GetStopBitsCode(string stopBitsString)
        {
            if (stopBitsString.Equals("1")) return StopBits.One;
            else if (stopBitsString.Equals("1.5")) return StopBits.OnePointFive;
            else if (stopBitsString.Equals("2")) return StopBits.Two;
            else return StopBits.One; // Unknown stop bits
        }

        //テキストボックス内で何かキーを打ち込んだ時の処理
        private void Command_KeyDown(object sender, KeyEventArgs e)
        {
            Key pressedKey = e.Key;
            if (pressedKey == Key.Enter) Send_Button_Clicked(sender, e);
            if (pressedKey == Key.Up)
            {
                if (historyIndex > 0)
                {
                    historyIndex--;
                    if (commandHistory.Count != 0) textBox_Command.Text = commandHistory[historyIndex];
                }
            }
            if (pressedKey == Key.Down)
            {
                if (historyIndex < commandHistory.Count)
                {
                    historyIndex++;
                    if (historyIndex < commandHistory.Count) textBox_Command.Text = commandHistory[historyIndex];
                    else textBox_Command.Clear();
                }
            }
        }

        //終了時の処理
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VCom.Stop();
        }
    }
}
