using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;

namespace Serial
{
    public partial class MainForm : Form
    {

        private SerialPort Serial = new SerialPort();
        //public int[] wghtlng = new int[2];
        List<long> x;
        List<long> y;

        #region Local Helpers
        private void UpdateCOMPortList()
        {
            // Get all existing Com Port names
            string[] Ports = System.IO.Ports.SerialPort.GetPortNames();
            cboxComport.Items.Clear();

            // Append existing COM to the cboxComport list
            foreach (var item in Ports)
            {
                cboxComport.Items.Add(item);
            }
        }
        #endregion

        #region Delegates
        public delegate void UPDATE_OUTPUT_TEXT(String Str);
        public void UpdateOutputText(String Str)
        {
            string[] subs = Str.Split(',');
            tboxReceive.Text = Str + tboxReceive.Text;
            tboxReceive.ScrollToCaret();
            try
            {
                long strenghth = long.Parse(subs[0]);
                long length = long.Parse(subs[1]);


            labelCurrentData.Text = "усилие " + strenghth + ", расстояние " + length;
            chart1.Series["Series1"].LegendText = "График XY";

            x = new List<long>();
            y = new List<long>();

            x.Add(length);
            y.Add(strenghth);
            }
            catch { }

            for (int i = 0; i < x.Count; i++)
            {
                chart1.Series["Series1"].Points.AddXY(x[i], y[i]);
            }
            chart1.Series["Series1"].LegendText = "График XY";
            for (int i = 0; i < x.Count; i++)
            {
                chart1.Series["Series1"].Points.AddXY(x[i], y[i]);
            }
        }
        #endregion

        #region Handlers
        void SerialOnReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            String str = Serial.ReadExisting();
            
            Invoke(new UPDATE_OUTPUT_TEXT(UpdateOutputText), str);
        }
        #endregion

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // If user click disconnect
            if ("Disconnect" == btnConnect.Text.ToString())
            {
                if (true == Serial.IsOpen)
                {
                    Serial.Close();
                }

                btnConnect.Text = "Connect";
                cboxComport.Enabled = true;
                //cboxBaudrate.Enabled = true;
                btnRefresh.Enabled = true;

                return;
            }

            // else we gonna open the desired COM port
            // Get user comport from cbox
            try
            {
                Serial.PortName = cboxComport.Text;
            }
            catch
            {
                MessageBox.Show("Error! No COM Port selected");
                return;
            }

            // Serial Port Configuration
            Serial.BaudRate = 115200;
            Serial.Parity = Parity.None;
            Serial.DataBits = 8;
            Serial.ReceivedBytesThreshold = 1;
            Serial.StopBits = StopBits.One;
            Serial.Handshake = Handshake.None;
            Serial.WriteTimeout = 3000;

            // Check if com port is opened by other application
            if (false == Serial.IsOpen)
            {
                try
                {
                    // Com port available
                    Serial.Open();
                }
                catch
                {
                    MessageBox.Show("The COM port is not accessible", "Error");
                    return;
                };


                // double comform it is opened
                if (true == Serial.IsOpen)
                {
                    btnConnect.Text = "Disconnect";
                    cboxComport.Enabled = false;
                    //cboxBaudrate.Enabled = false;
                    btnRefresh.Enabled = false;

                    // Add callback handler for receiving
                    Serial.DataReceived += new SerialDataReceivedEventHandler(SerialOnReceivedHandler);

                }

            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // We need to populate the lists during mainform is loading
            UpdateCOMPortList();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // We need to update all lists again if user requested
            UpdateCOMPortList();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(null != Serial)
            {
                if(true == Serial.IsOpen)
                {
                    Serial.Write(tboxData.Text);
                }
                else
                {
                    MessageBox.Show("COM Port is not Opened");
                }
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (null != Serial)
            {
                if (true == Serial.IsOpen)
                {
                    Serial.Write("1");
                }
                else
                {
                    MessageBox.Show("COM Port is not Opened");
                }
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (null != Serial)
            {
                if (true == Serial.IsOpen)
                {
                    Serial.Write("0");
                }
                else
                {
                    MessageBox.Show("COM Port is not Opened");
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (null != Serial)
            {
                if (true == Serial.IsOpen)
                {
                    Serial.Write("s");
                }
                else
                {
                    MessageBox.Show("COM Port is not Opened");
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            {
                //Создаем поток для записи в файл и загружаем в него файл
                //при отсутствии файла он будет создан
                string fileName = DateTime.Now.ToString("yyyy.MM.dd hh.mm") + ".csv";
                FileStream filestream =
                  new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                //Очищаем поток
                filestream.SetLength(0);
                //StreamWriter создаем для потока filestream
                StreamWriter streamwriter = new StreamWriter(filestream);
                //Записываем текст, введенный в textBox1 в файл
                streamwriter.Write(tboxReceive.Text);
                //Освобождаем ресурсы
                streamwriter.Flush();
                streamwriter.Close();
                filestream.Close();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            tboxReceive.Text = "";
            chart1.Series["Series1"].Points.Clear();
        }
    }
}
