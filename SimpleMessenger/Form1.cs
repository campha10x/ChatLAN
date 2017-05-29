using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatLAN
{
    public partial class Form1 : Form
    {
        #region Properties
        public Server server;
        public Client client;

        public delegate void UpdateTextCallback(string text);
        private delegate void ReconnectCallback();//Handle Cross-Thread exception.

        //private Socket sock;
        //private byte[] data = new byte[1024];
        #endregion

        public Form1()
        {
            InitializeComponent();
            Text += " " + Constant.SOFTWAREVERSION;

            IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < IPHost.AddressList.Length; i++)
            {
                textBox1.AppendText("My IP address is: " + IPHost.AddressList[i].ToString() + "\r\n");
            }
            string ipText = IPHost.AddressList[IPHost.AddressList.Length - 1].ToString();
            string[] IPs = ipText.Split(new char[] { '.' });
            textBox3.Text = IPs[0];
            textBox4.Text = IPs[1];
            textBox5.Text = IPs[2];
            textBox6.Text = IPs[3];
            
        }


        #region Event Handlers
        //MenuStrip
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)//Exit
        {
            if (server != null) server.Disconnect();
            if (client != null) client.Disconnect();
            Application.Exit();
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)//AboutSocketDemo
        {
           
        }
        //Buttons
        private void button1_Click(object sender, EventArgs e)//Send
        {
            if (textBox2.Text != "")
            {
                if (radioButton1.Checked == true)//Server Mode
                {
                    try
                    {
                        if (server != null)
                        {
                            byte[] bytes = Encoding.GetEncoding(Constant.SimplifiedChineseCode).GetBytes(textBox2.Text);
                            server.Send(bytes);
                            string str = "";
                            if (checkBox1.Checked == false)//Display data in text
                            {
                                str = "\r\nServer said: (@" + DateTime.Now.ToString() + ")\r\n" + textBox2.Text;
                            }
                            else//Display data in Hex
                            {
                                str = "\r\nServer said: (@" + DateTime.Now.ToString() + ")\r\n";
                                for (int i = 0; i < bytes.Length; i++)
                                {
                                    str += (bytes[i].ToString("X").Length == 2 ? bytes[i].ToString("X") : "0" + bytes[i].ToString("X")) + " ";
                                }
                            }
                            textBox1.AppendText(str + "\r\n");
                            textBox2.Clear();
                        }
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("Server send error!\r\n" + se.Message);
                    }
                }
                else if (radioButton2.Checked == true)//Client Mode
                {
                    try
                    {
                        if (client != null)
                        {
                            byte[] bytes = Encoding.GetEncoding(Constant.SimplifiedChineseCode).GetBytes(textBox2.Text);
                            client.Send(bytes);
                            string str = "";
                            if (checkBox1.Checked == false)//Display data in text
                            {
                                str = "\r\nClient said: (@" + DateTime.Now.ToString() + ")\r\n" + textBox2.Text;
                            }
                            else//Display data in Hex
                            {
                                str = "\r\nClient said: (@" + DateTime.Now.ToString() + ")\r\n";
                                for (int i = 0; i < bytes.Length; i++)
                                {
                                    str += (bytes[i].ToString("X").Length == 2 ? bytes[i].ToString("X") : "0" + bytes[i].ToString("X")) + " ";
                                }
                            }
                            textBox1.AppendText(str + "\r\n");
                            textBox2.Clear();
                        }
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("Client send error!\r\n" + se.Message);
                    }
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)//Connect
        {
            string ipAddr = textBox3.Text + "." + textBox4.Text + "." + textBox5.Text + "." + textBox6.Text;
            string port = textBox7.Text;
            if (IsValidIPAddress(ipAddr) == true)
            {
                if (radioButton1.Checked == true)//Server Mode
                {
                    try
                    {
                        if (server == null)
                            server = new Server(this);
                        
                        server.Connect(ipAddr, port);

                        button2.Enabled = false;
                        button3.Enabled = true;
                        textBox2.Focus();
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("Server Connect Error.\r\n" + se.ToString());
                    }
                }
                else if (radioButton2.Checked == true)//Client Mode
                {
                    try
                    {
                        if (client == null)
                            client = new Client(this);

                        client.Connect(ipAddr, port);

                        string data = "Hello, I'm a Client";
                        client.Send(Encoding.GetEncoding(Constant.SimplifiedChineseCode).GetBytes(data));
                        textBox1.AppendText("\r\nClient said: (@" + DateTime.Now.ToString() + ")\r\n" + data + "\r\n");
                        button2.Enabled = false;
                        button3.Enabled = true;
                        textBox2.Focus();
                    }
                    catch (SocketException se)
                    {
                        MessageBox.Show("Client Connect Error.\r\n" + se.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid IP address input.");
            }
        }
        private void button3_Click(object sender, EventArgs e)//Disconnect
        {
            if (radioButton1.Checked == true)//Server Mode
            {
                server.Disconnect();
                if (button2.Enabled == false)
                {
                    button2.Enabled = true;
                    button3.Enabled = false;
                }
            }
            else if (radioButton2.Checked == true)//Client Mode
            {
                client.Disconnect();
                if (button2.Enabled == false)
                {
                    button2.Enabled = true;
                    button3.Enabled = false;
                }
            }

        }
        private void button4_Click(object sender, EventArgs e)//Reconnect
        {
            if (button3.Enabled == true)
                button3_Click(sender, e);//Disconnect
            Thread.Sleep(200);
            button2_Click(sender, e);//Connect
        }
        private void button5_Click(object sender, EventArgs e)//Clear
        {
            textBox1.Clear();
        }
        //TextBox2
        private void textBox2_KeyDown(object sender, KeyEventArgs e)//Send the message
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, (EventArgs)e);//Send
            }
        }
        private void textBox2_KeyUp(object sender, KeyEventArgs e)//Clear the text in textBox2
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox2.Lines.Length > 0)
                    textBox2.Lines = new string[] { };
            }
        }
        //Timer
        private void timer1_Tick(object sender, EventArgs e)//Timer1
        {
            toolStripStatusLabel2.Text = DateTime.Now.ToLocalTime().ToLongDateString() + " " + DateTime.Now.ToLocalTime().ToLongTimeString() + "  By Hantou";
        }
        #endregion


        #region Helper Methods
        
        public void UpdateText(string text)//Update the text on textBox1
        {
            if (this.textBox1.InvokeRequired)
            {
                UpdateTextCallback temp = new UpdateTextCallback(UpdateText);
                this.Invoke(temp, new object[] { text });
            }
            else
            {
                string str = "";
                if (checkBox1.Checked == false)
                {
                    if (radioButton1.Checked == true) str = "\r\nClient said: (@" + DateTime.Now.ToString() + ")\r\n" + text;
                    else if (radioButton2.Checked == true) str = "\r\nServer said: (@" + DateTime.Now.ToString() + ")\r\n" + text;
                }
                else
                {
                    //Revise required!
                    if (radioButton1.Checked == true) str = "\r\nClient said: (@" + DateTime.Now.ToString() + ")\r\n" + text;
                    else if (radioButton2.Checked == true) str = "\r\nServer said: (@" + DateTime.Now.ToString() + ")\r\n" + text;
                }

                textBox1.AppendText(str);
            }
        }
        private bool IsValidIPAddress(string ipaddr)//Validate the input IP address
        {
            try
            {
                IPAddress.Parse(ipaddr);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "IsValidIPAddress Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void Reconnect()//Reconnect the Ethernet
        {
            try
            {
                if (button4.InvokeRequired)
                {
                    ReconnectCallback r = new ReconnectCallback(Reconnect);
                    this.Invoke(r, new object[] { });
                }
                else
                {
                    button4_Click(null, null);//Reconnect
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Reconnect failed.  Please restart.\r\n" + e.Message, "Reconnect Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool IsHex//Check if 'Hex' checkbox is checked
        {
            get { return checkBox1.Checked; }
        }
        public bool IsNoPrintOnReceiving//Check if 'No Print On Receiving' checkbox is checked
        {
            get { return checkBox2.Checked; }
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// A helper class wraps a Socket and an array of byte.
    /// </summary>
    public class KeyValuePair
    {
        public Socket socket;
        public byte[] dataBuffer = new byte[1];
    }
}