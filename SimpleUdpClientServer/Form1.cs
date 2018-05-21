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
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Drawing2D;
using System.Net;
using System.Net.Sockets;

namespace SimpleUdpClientServer
{
    public delegate void TemplEvent(List<Point> mas);
    public partial class Form1 : Form
    {
        protected UdpClient Client;
        List<Point> points = new List<Point>();
        public event TemplEvent GetList;
        public delegate void InvokeDelegate();
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            MouseMove += Form1_MouseMove;
            Paint += Form1_Paint;

            GetList += GetPoints;
            Task.Factory.StartNew(() => Server(this, 3000));
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (points.Count > 1)
            {
                using (Pen pen = new Pen(Color.Red, 20))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    e.Graphics.DrawLines(pen, points.ToArray());
                }
            }
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                points.Add(e.Location);
                this.Refresh();
            }
        }
        private void GetPoints(List<Point> PointList)
        {
            this.points = PointList;
            this.BeginInvoke(new InvokeDelegate(InvokeMethod));
        }
        public void InvokeMethod()
        {
            this.Refresh();
        }
        private void Server(Form1 form, int Port)
        {
            UdpClient server = new UdpClient(Port);
            IPEndPoint ipPoint = null;
            try
            {
                while (true)
                {
                    byte[] message = server.Receive(ref ipPoint);

                    var mStream = new MemoryStream();
                    var binFormatter = new BinaryFormatter();
                    mStream.Write(message, 0, message.Length);
                    mStream.Position = 0;
                    var myObject = binFormatter.Deserialize(mStream) as List<Point>;
                    form.GetList.Invoke(myObject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                server.Close();
            }
        }
        private void Send_Click(object sender, EventArgs e)
        {
            UdpClient client = new UdpClient();
            client.Connect("127.0.0.1", 3000);

            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, points);
            byte[] message = mStream.ToArray();

            client.Send(message, message.Length);

            client.Close();
        }
    }
}
