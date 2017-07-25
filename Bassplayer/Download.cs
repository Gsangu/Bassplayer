using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bassplayer
{
    public partial class Download : Form
    {

        public Download()
        {
            InitializeComponent();
        }
        #region 拖动窗口移动
        private Point offset;//当前窗口坐标

        public void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left != e.Button) return;

            Point cur = this.PointToScreen(e.Location);
            offset = new Point(cur.X - this.Left, cur.Y - this.Top);
        }
        public void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left != e.Button) return;

            Point cur = MousePosition;
            this.Location = new Point(cur.X - offset.X, cur.Y - offset.Y);
        }
        #endregion
        public static string target = Environment.CurrentDirectory + "\\Download\\",site = "", name = "";
        public static string[] sites;
        public static int type;
        WebClient web = new WebClient();
        string webSite, html;
        byte[] buffer;
        string filePath = Environment.CurrentDirectory + "\\download.lst";
        public void SetSite(string name,string site,int type)
        {
            Download.site = site;
            Download.name = name;
            Download.type = type;
        }

        private void ChangeSite_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                skinTextBox1.Text = ofd.SelectedPath+"\\";
                target = skinTextBox1.Text;
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(target))
            { Directory.CreateDirectory(target); }
            if (!File.Exists(filePath))
            {
                FileStream fs = File.Create(filePath);
                fs.Close();
            }
            if (type <= 2)
            {
                int count = 0;
                sites = site.Split('*');
                for(int i = 0; i < panel1.Controls.Count; i++)
                {
                    if (panel1.Controls[i] is RadioButton)
                    {
                        RadioButton r = panel1.Controls[i] as RadioButton;
                        if (r.Checked)
                        {
                            site = sites[i%(sites.Length-1)];
                            count++;
                        }
                    }
                }
                if (count == 0) { MessageBox.Show("请选择需要下载的音质！"); return; }
            }
            try
            {
                web.DownloadFile(site, target + name + ".mp3");
                MessageBox.Show("下载成功！");
                string re = File.ReadAllText(filePath);
                StreamWriter sw = new StreamWriter(filePath);
                DateTime dt = DateTime.Now;
                sw.Write(re+target + name + ".mp3"+ "*"+name + "*" + string.Format("{0:d}", dt)+"|");
                sw.Flush();
                sw.Close();
            }
            catch { MessageBox.Show("下载失败！"); }
        }

        private void Download_Load(object sender, EventArgs e)
        {
            skinTextBox1.Text = target;
            MusicName.Text = name;
            string[] check = new string[] { "标准", "低品", "高品", "最高" };
            if (type <= 2)
            {
                string id = name.Split('*')[1];
                string[] Result = new string[2];
                name = name.Split('*')[0];
                MusicName.Text = name;
                if (type == 1) { Result = GetBaiduSite(id); }
                else
                {
                    check = new string[] { "标准", "高品" };
                    SearchAPI s = new SearchAPI();
                    Result[1] = s.GetSmusic(id, 320000);
                    Result[0] = s.GetSmusic(id, 128001);
                }
                for (int i = 0; i < Result.Length; i++)
                {
                    double size = Convert.ToDouble(Result[i].Split('*')[0].Remove(0,1));
                    RadioButton r = new RadioButton();
                    Label l = new Label();
                    r.AutoSize = true; l.AutoSize = true;
                    r.Text = check[i] + "音质"; l.Text = (size / 1024 / 1024).ToString("0.0") + "MB/" + Result[i].Split('*')[1] + "KBPS";
                    r.Font = l.Font = new Font("微软雅黑", 9);
                    r.Location = new Point(0, 20 * i); l.Location = new Point(100, 20 * i);
                    site += Result[i].Split('*')[2] + "*";
                    panel1.Controls.Add(r); panel1.Controls.Add(l);
                }
            }
            else
            {
                RadioButton r = new RadioButton();
                r.AutoSize = true;
                r.Font = new Font("微软雅黑", 9);
                r.Location = new Point(0, 0);
                r.Text = "标准音质";
                r.Checked = true;
                panel1.Controls.Add(r);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            site = "";
            name = "";
            type = 0;
            Close();
        }
        public string[] GetBaiduSite(string id)
        {
            webSite = @"http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.song.downWeb&songid=" + id + "&bit=flac";
            buffer = web.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("file_bitrate\":(?<bit>.+?),.+?link\":\"(?<site>http.+?)\".+?file_size\":(?<size>.+?),");
            MatchCollection mc = Regex.Matches(html, match);
            string[] Result = new string[mc.Count];
            for(int i = 0; i < mc.Count; i++)
            {
                if (mc[i].Groups["site"].Value.IndexOf("mp3") > 0)
                {
                    string size = mc[i].Groups["size"].Value;
                    string bit = mc[i].Groups["bit"].Value;
                    string site = mc[i].Groups["site"].Value.Replace("\\", "");
                    Result[i] = size + "*" + bit + "*" + site;
                }
                  
            }
            return Result;
        }

    }
}
