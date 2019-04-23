using System;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace E_Namad
{
    public partial class Form1 : Form
    {
        public string cookie = "";
        public Form1()
        {
            InitializeComponent();
        }
        void doit(int offset,int limit=100)
        {
            toolStripStatusLabel1.Text = "Fetching Data : ";
            StreamWriter st = new StreamWriter(@Path.GetTempPath().ToString()+"result.json");
            toolStripProgressBar1.Maximum = offset;
            for (int x = 0; x <= offset; x += limit)
            {
                toolStripProgressBar1.Value = x;
                string url = $"http://enamad.ir/Home/DomainListDataForMIMT?order=asc&offset={x}&limit={limit}";
                WebClient wb = new WebClient();
                wb.Headers[HttpRequestHeader.Cookie] = cookie;
                wb.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                string result = wb.DownloadString(url);

                JObject Json = JObject.Parse(result);

                if (Json.ToString().Contains("\"rows\": []"))
                {
                    st.WriteLine(Json);
                    break;
                }
                else
                {
                    st.WriteLine(Json);
                }
            }
            st.Close();
            toolStripProgressBar1.Maximum = toolStripProgressBar1.Value;

            StreamReader rd = new StreamReader(@Path.GetTempPath().ToString() + "result.json");
            textBox1.Text = rd.ReadToEnd();
            rd.Close();

            //Remove something
            textBox1.Text = textBox1.Text.Replace("      \"cycleStatus\": null\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"postalcode\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"address\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"mobile\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"name\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"nationalId\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"datetime\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"phone\": null,\r\n", "");
            textBox1.Text = textBox1.Text.Replace("     \"datetimeop\": null,\r\n", "");

            toolStripStatusLabel1.Text = "Progress is finished -> ";

            button2.Enabled = true;
            button3.Enabled = true;
        }
        void saveurl()
        {
            toolStripStatusLabel1.Text = "Spliting url : ";

            MatchCollection m = Regex.Matches(textBox1.Text, "\"domain\": \"(.*?)\"");

            StreamWriter sw = new StreamWriter("Rp-Enamad(Url).txt");

            toolStripProgressBar1.Maximum = m.Count;
            for (int x = 0; x < m.Count; x++)
            {
                toolStripProgressBar1.Value = x;
                sw.WriteLine(m[x].Groups[1].Value);
            }
            sw.Close();
            toolStripStatusLabel1.Text = "Progress is finished -> ";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            int total;

            //Get New Cookie
            string url = "http://enamad.ir";
            using (WebClient wb = new WebClient())
            {
                wb.OpenRead(url);
                cookie = wb.ResponseHeaders["Set-Cookie"];
            }
            int c = cookie.IndexOf("cookiesession1");
            int v = cookie.LastIndexOf(";Path=");
            cookie = cookie.Substring(c, v - c);

            //Get Totat Domain Address Fast!!
            url = "http://enamad.ir/Home/DomainListDataForMIMT?order=asc&offset=0&limit=1";
            using (WebClient wb = new WebClient())
            {
                wb.Headers[HttpRequestHeader.Cookie] = cookie;
                wb.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                string result = wb.DownloadString(url);

                JObject Json = JObject.Parse(result);

                total = int.Parse(Json["total"].ToString());
            }

            Thread t = new Thread(() => doit(total));
            t.Start();
            MessageBox.Show("It May Take a While","Info",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StreamWriter sr = new StreamWriter("Rp-Enamad.txt");
            sr.WriteLine(textBox1.Text);
            sr.Close();
            toolStripStatusLabel1.Text = "Saved.";
            button2.Enabled = false;
            button1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = DateTime.Now.ToLongTimeString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button1.Enabled = true;
            Thread t = new Thread(saveurl);
            t.Start();
            MessageBox.Show("It May Take a While", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
