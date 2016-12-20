using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace MotorDriver_VComTool
{
    class VComControl
    {
        private SerialPort myPort = null;
        private Thread receiveThread = null;
        private MainWindow window = null;
        private bool connected = false;

        public VComControl(MainWindow caller)
        {
            window = caller;
        }

        public string[] GetPortList()
        {
            return SerialPort.GetPortNames();
        }

        //COMポートを開く
        //成功したらtrue、失敗したらfalse
        public bool Start(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            try {
                myPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                myPort.Open();

                receiveThread = new Thread(VComControl.Receive);
                receiveThread.Start(this);
                connected = true;
            }
            catch (Exception){
                return false;
            }
            return true;
        }

        //COMポートを閉じる
        public void Stop()
        {
            if (myPort != null) myPort.Close();
            myPort = null;
            connected = false;
            if (receiveThread != null) receiveThread.Join();
            receiveThread = null;
        }

        //受信データ取得ループ
        public static void Receive(object target)
        {
            VComControl my = target as VComControl;
            while (my.isConnected())
            {
                try {
                    my.ReceiveData();
                    Thread.Sleep(10);
                }
                catch (Exception)
                {
                    //rebootでCOMポートが閉じられるなどして受信できなくなった場合の処理
                    if (my.myPort != null) my.myPort.Close();
                    my.myPort = null;
                    my.connected = false;
                }
            }
        }

        public delegate void DataReceivedHandler(byte[] data);
        public event DataReceivedHandler DataReceived;

        //データ受け取り処理
        public void ReceiveData()
        {
            int rbyte = myPort.BytesToRead;
            byte[] bufread = new byte[rbyte];
            int read = 0;
            while (read < rbyte)
            {
                int length = myPort.Read(bufread, read, rbyte - read);
                read += length;
            }
            if (rbyte > 0)
            {
                window.Dispatcher.Invoke(() => DataReceived(bufread));
            }
        }

        //データ送信処理
        public void Send(string command)
        {
            if (isConnected())
            {
                byte[] bufwrite = Encoding.ASCII.GetBytes(command);
                myPort.Write(bufwrite, 0, command.Length);
            }
        }

        //COMポートに接続していればtrue
        public bool isConnected()
        {
            return connected;
        }
    }
}
