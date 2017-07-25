using CCWin.SkinControl;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Bassplayer
{
    public partial class SearchAllLrc : Form
    {
        public SearchAllLrc()
        {
            InitializeComponent();
        }
        GetLrc gl = new GetLrc();
        Form1 f = new Form1();
        public static string name = "", singer = "", time = "";
        string[] alllrc;
        string filePath = Environment.CurrentDirectory + "\\Lyrics\\";

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                textBox5.Text = alllrc[listBox1.SelectedIndex].Split('|')[0];
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Close();
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

        private void 设置歌词ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.lrcFlag = true;
            string lrc= alllrc[listBox1.SelectedIndex].Split('|')[0];
            string[] fen = new string[] { "\r\n", "\\\\n", "\n" };
            int i = 0;
            WriteFile(lrc, singer + " - " + name + ".lrc");
            string[] lrcs = Regex.Split(alllrc[listBox1.SelectedIndex].Split('|')[0], fen[i]);
            while (lrc.Length < 2)
            {
                i++;
                lrcs = Regex.Split(alllrc[listBox1.SelectedIndex].Split('|')[0], fen[i]);
            }

            f.FormatLrc(lrcs);
        }
        public void WriteFile(string lrcs, string name)
        {
            if (!File.Exists(name)) { FileStream fs = File.Create(filePath + name); fs.Close(); }
            StreamWriter sw = new StreamWriter(filePath + name);
            sw.Write(lrcs);
            sw.Flush();
            sw.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            gl.singerss = ".+?singer\":\"(?<singer>.+?)\"";
            alllrc = gl.GetAllKuGouLrc(textBox1.Text, textBox2.Text, int.Parse(textBox3.Text) * 60 + int.Parse(textBox4.Text));
            int i = 1;
            foreach(string lrc in alllrc)
            {
                SkinListBoxItem s = new SkinListBoxItem(i + "."+alllrc[i-1].Split('|')[1] +" - "+ name);
                listBox1.Items.Add(s);
                i++;
            }
        }

        private void SearchAllLrc_Load(object sender, EventArgs e)
        {
            textBox1.Text = name;
            textBox2.Text = singer;
            textBox3.Text = time.Split(':')[0].Trim();
            textBox4.Text = time.Split(':')[1].Trim();
        }
    }
}
