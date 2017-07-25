using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Un4seen.Bass;
using CCWin;
using CCWin.SkinControl;
using System.Threading;
using System.Threading.Tasks;

namespace Bassplayer
{
    public partial class Form1 : CCSkinMain
    {
        [DllImport("user32.dll", EntryPoint = "GetScrollPos")]
        public static extern int GetScrollPos(IntPtr hwnd, int nBar);//获取滚动条位置
        
        public Form1()
        {
            InitializeComponent();
            BassNet.Registration("gozhushi@qq.com", "2X152429150022");//注册bass音频库
        }
        Download d = new Download();
        WebClient web = new WebClient();
        public bool _wplay = false;//是否正在播放
        static int index = 1,musicIndex;//歌曲下标和当前播放歌曲下标
        static int stream; //音频流句柄。
        /// <summary>
        /// 音量0—100
        /// </summary>
        public int Volume
        {
            get
            {
                return Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM) / 100;
            }
            set
            {
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_GVOL_STREAM, value * 100);
            }
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            _wplay = false;
            Application.Exit();
        }
        public void RemoveAllPanel()
        {
            SearchMusic.Visible = false;
            LocalMusicc.Visible = false;
            RankMusic.Visible = false;
            DownloadPanel.Visible = false;
        }
        #region 播放器初始化
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //Control.CheckForIllegalCrossThreadCalls = false;
                string path = Environment.CurrentDirectory +"\\Codes\\";
                SoundTrack.Value = 50;//初始化赋予音量
                Volume = 50;
                string[] allSound = new string[] { "aac", "ac3", "alac", "ape", "cd", "flac", "mpc", "tta", "wma", "wv" };
                for (int i = 0; i < allSound.Length; i++)
                {
                    string code = path + "bass_" + allSound[i] + ".dll";
                    int handle = Bass.BASS_PluginLoad(code);
                }
                
                if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_CPSPEAKERS, this.Handle))//如果bass音频库加载出错
                {
                    MessageBox.Show("bass初始化出错" + Bass.BASS_ErrorGetCode().ToString());
                }
            }
            catch { }

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//关闭软件后释放资源
        {
            try
            {
                Bass.BASS_ChannelStop(stream);
                Bass.BASS_StreamFree(stream);
                Bass.BASS_Stop();
                Bass.BASS_Free();
            }
            catch { }
        }
        #endregion
        #region 按钮变化
        private void play_MouseMove(object sender, MouseEventArgs e)
        {
            if (stream != 0) { timer1.Enabled = false; }
            
            if (_wplay == true)
            {
                play.Image = Properties.Resources.pause_move;
            }
            else
            {
                play.Image = Properties.Resources.play_move;
            }
            
        }

        private void play_MouseLeave(object sender, EventArgs e)
        {
            if (_wplay == true)
            {
                play.Image = Properties.Resources.pause;
            }
            else
            {
                play.Image = Properties.Resources.play;
            }
        }

        private void back_MouseMove(object sender, MouseEventArgs e)
        {
            back.Image = Properties.Resources.back_move;
        }

        private void back_MouseLeave(object sender, EventArgs e)
        {
            back.Image = Properties.Resources.back;
        }

        private void next_MouseMove(object sender, MouseEventArgs e)
        {
            next.Image = Properties.Resources.next_move;
        }

        private void next_MouseLeave(object sender, EventArgs e)
        {
            next.Image = Properties.Resources.next;
        }

        private void sound_MouseMove(object sender, MouseEventArgs e)
        {
            sound.Image = Properties.Resources.sound_move;
        }

        private void sound_MouseLeave(object sender, EventArgs e)
        {
            sound.Image = Properties.Resources.sound;
        }

        private void Exit_MouseMove(object sender, MouseEventArgs e)
        {
            Exit.ForeColor = Color.White;
        }

        private void Exit_MouseLeave(object sender, EventArgs e)
        {
            Exit.ForeColor = Color.FromArgb(225, 145, 145);
        }

        private void btnSearch_MouseMove(object sender, MouseEventArgs e)
        {
            btnSearch.Image = Properties.Resources.Searchbg;
        }

        private void btnSearch_MouseLeave(object sender, EventArgs e)
        {
            btnSearch.Image = Properties.Resources.Search;
        }

        private void webMusic_MouseMove(object sender, MouseEventArgs e)
        {
            label2.ForeColor = Color.Black;
            pictureBox1.Image = Properties.Resources.webMusic_move;
        }

        private void webMusic_MouseLeave(object sender, EventArgs e)
        {
            label2.ForeColor = Color.FromArgb(138, 138, 138);
            pictureBox1.Image = Properties.Resources.webMusic;
        }

        private void localMusic_MouseMove(object sender, MouseEventArgs e)
        {
            label3.ForeColor = Color.Black;
            pictureBox2.Image = Properties.Resources.localMusic_move;
        }

        private void localMusic_MouseLeave(object sender, EventArgs e)
        {
            label3.ForeColor = Color.FromArgb(138, 138, 138);
            pictureBox2.Image = Properties.Resources.localMusic;
        }
        
        private void download_MouseLeave(object sender, EventArgs e)
        {
            label5.ForeColor = Color.FromArgb(138, 138, 138);
            pictureBox3.Image = Properties.Resources.Download;
        }

        private void download_MouseMove(object sender, MouseEventArgs e)
        {
            label5.ForeColor = Color.Black;
            pictureBox3.Image = Properties.Resources.Download_move;
        }
        private void MiniState_MouseMove(object sender, MouseEventArgs e)
        {
            MiniState.ForeColor = Color.White;
        }

        private void MiniState_MouseLeave(object sender, EventArgs e)
        {
            MiniState.ForeColor = Color.FromArgb(225, 145, 145);
        }
        private void About_MouseMove(object sender, MouseEventArgs e)
        {
            About.ForeColor = Color.White;
        }

        private void About_MouseLeave(object sender, EventArgs e)
        {
            About.ForeColor = Color.FromArgb(225, 145, 145);
        }

        #endregion
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
        #region LeftPanel
        private void localMusic_Click(object sender, EventArgs e)
        {
            List.lsong.Clear();
            List.localmusic.Clear();
            listsong.Items.Clear();
            if (!File.Exists(localPath))
            {
                FileStream fs = File.Create(localPath);
                fs.Close();
            }else
            {
                string[] lc = File.ReadAllText(localPath).Split('|'); 
                if (lc[0] != "")
                {
                    for (int i = 0; i < lc.Length; i++)
                    {
                        string[] l = lc[i].Split('*');
                        if (l[0] != "")
                        {
                            ListViewItem lv = new ListViewItem(l[0]);
                            for (int j = 1; j < l.Length - 1; j++)
                            {
                                lv.SubItems.Add(l[j]);
                            }
                            List.localmusic.Add(l[l.Length - 1]);
                            listsong.Items.Add(lv);
                        }
                    }
                }
            }
            RemoveAllPanel();
            LocalMusicc.Visible = true;
            open = 1;
        }
        private void webMusic_Click(object sender, EventArgs e)
        {
            List.lsong.Clear();
            RemoveAllPanel();
            RankMusic.Visible = true;
            open = 2;
        }
        private void label5_Click(object sender, EventArgs e)
        {
            RemoveAllPanel();
            DownloadPanel.Visible = true;
            DownloadMusic.Items.Clear();
            List.lsong.Clear();
            if (File.Exists(Environment.CurrentDirectory + "\\download.lst"))
            {
                string file = File.ReadAllText(Environment.CurrentDirectory + "\\download.lst");
                string[] Alllist = file.Split('|');
                foreach (string list in Alllist)
                {
                    if (list != "")
                        AddDownloadItems(list);
                }
            }
            dindex = 1;
            open = 4;
        }
        #endregion
        public void MyPlay(string songName)
        {
            Bass.BASS_ChannelStop(stream);   //停止前一首的歌曲 
            ListCount.Text = List.lsong.Count+"";
            if (songName.IndexOf("http://") >= 0)
            {
                stream = Bass.BASS_StreamCreateURL(songName, 0, BASSFlag.BASS_STREAM_STATUS, null, IntPtr.Zero);
            }
            else { stream = Bass.BASS_StreamCreateFile(songName, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT); }
            if (stream == 0) { MessageBox.Show("歌曲因某种原因不能播放！"); return; }//如果文件找不到
            _wplay = true;
            timer1.Enabled = true;
            timer2.Enabled = true;
            musicPlay.Visible = true;
            Bass.BASS_ChannelPlay(stream, true);//播放
            getAllTime();
            play.Image = Properties.Resources.pause;
            if (MusicLrcPannel.Visible) {  UpdatePanel(); }
            string[] temp = songName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (songName.IndexOf("http://") >= 0)
            {
                if(RankMusic.Visible)
                {
                    musicName.Text = RankList.Items[musicIndex].SubItems[1].Text;
                    Singer.Text = RankList.Items[musicIndex].SubItems[2].Text;//歌手
                    SetImg(List.rankSite[RankList.Items[musicIndex].Index].Split('*')[0]);
                }
                else if(SearchMusic.Visible)
                {
                    musicName.Text = SearchResult.Items[musicIndex].SubItems[1].Text;
                    Singer.Text = SearchResult.Items[musicIndex].SubItems[2].Text;//歌手
                    SetImg(List.searchSite[SearchResult.Items[musicIndex].Index].Split('*')[0]);
                }
                
                return;
            }
            try
            {
                musicName.Text = temp[temp.Length - 1].Split('-')[1].Split('.')[0].Trim();//歌曲名
                Singer.Text = temp[temp.Length - 1].Split('-')[0].Trim();//歌手
            }
            catch
            {
                musicName.Text = temp[temp.Length - 1].Split('.')[0];
                Singer.Text = "未知歌手";
            }
            SetImg(null);
        }
        static int ScollIndex = 4;//榜单列表滚动条位置
        static int ScollIndex1 = 1;//搜索歌曲列表滚动条位置
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (RankMusic.Visible)
            {
                int i = GetScrollPos(RankList.Handle, 1);
                if (i >= ScollIndex)
                {
                    label10.Text = "加载中...";
                    string[] allRanks = ra.GetRank(type, size, page);
                    foreach (string s in allRanks)
                    {
                        AddRankItem(s);
                    }
                    page += size;
                    ScollIndex += size;
                    label10.Text = "";
                    ListCount.Text = List.lsong.Count+"";
                }
            }
            if (SearchMusic.Visible)
            {
                int i = GetScrollPos(SearchResult.Handle, 1);
                int ok = 0;
                if (i >= ScollIndex1)
                {
                    string[] allRanks = sa.GetHtmlFile(SearchT, SearchStr, limit, offsets);
                    foreach (string s in allRanks)
                    {
                        if (s != null)
                        {
                            AddSearchItem(s);
                            ok++;
                        }
                    }
                    offsets += ok;
                    ScollIndex1 += ok;
                    ListCount.Text = List.lsong.Count + "";
                }
            }
        }
        public void timer2_Tick(object sender, EventArgs e)
        {
            double runningtime = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));//获取当前正播放歌曲的进度
            TimeTrack.Value = Convert.ToInt32(Schedule * 100);
            if (alltime <= runningtime)
            {
                next_Click(null, null);
            }//自动下一曲
            runTime.Text = ChangeTime(runningtime);
        }
        #region 播放器时间显示
        static double alltime;
        public void getAllTime()
        {
            alltime = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));//获取歌曲总长度
            allTime.Text = ChangeTime(alltime);
        }
        /// <summary>
        /// 转换时间格式
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string ChangeTime(double time)
        {
            StringBuilder sb = new StringBuilder();
            int temp = (int)time / 60;
            if (time < 0)
            {
                time = 0;
            }
            sb.Remove(0, sb.Length);
            sb.AppendFormat("{0:00}", temp);
            sb.Append(':');
            sb.AppendFormat("{0:00}", time - temp * 60);
            return sb.ToString();
        }
        #endregion
        #region LocalMusic
        string localPath = Environment.CurrentDirectory + "\\local.lst";
        public void AddItems(string fileNames)
        {
            index = listsong.Items.Count;
            string[] writer = File.ReadAllText(localPath).Split('|');
            List<string> write = new List<string>();//记录之前歌曲列表
            for(int i = 0; i < writer.Length; i++)
            {
                if (writer[i] != "") write.Add(writer[i]);
            }
            string str = "";//要插入的数据
            ListViewItem l = null;
            FileInfo file = new FileInfo(fileNames);
            List.localmusic.Add(fileNames);
            int temps = stream;
            int temstream = stream;//保存
            string[] temp = fileNames.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            l = new ListViewItem(index.ToString());
            try
            {
                l.SubItems.Add(temp[temp.Length - 1].Split('.')[0].Split('-')[1].Trim());//歌曲名
                l.SubItems.Add(temp[temp.Length - 1].Split('.')[0].Split('-')[0].Trim());//歌手
            }
            catch { l.SubItems.Add(temp[temp.Length - 1].Split('.')[0]); l.SubItems.Add("未知歌手"); }
            stream = Bass.BASS_StreamCreateFile(fileNames, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT); 
            l.SubItems.Add(ChangeTime(Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream))));//歌曲时间
            l.SubItems.Add(file.Extension.Replace(".", ""));//文件扩展名
            double Sizes = Convert.ToDouble( file.Length) / 1024 / 1024;//歌曲大小
            l.SubItems.Add(Sizes.ToString("0.0") + " MB");
            stream = temstream;
            listsong.Items.Add(l);
            for(int i = 0; i < l.SubItems.Count; i++)
            {
                str += l.SubItems[i].Text+"*";
            }
            write.Add(str+"*" + fileNames);
            stream = temps;
            for (int j = 0; j < List.localmusic.Count - 1; j++)
            {
                if (fileNames == List.localmusic[j])
                {
                    List.localmusic.RemoveAt(j);
                    listsong.Items.RemoveAt(j);
                    write.RemoveAt(j);
                    for(int k = j; k < List.lsong.Count; k++)
                    {
                        listsong.Items[k].SubItems[0].Text = (k + 1) + "";
                    }//index重新排序
                }
            }
            StreamWriter sw = new StreamWriter(localPath);
            string s = "";
            for(int i = 0; i < write.Count; i++)
            {
                s += write[i] + "|";
            }
            sw.Write(s);
            sw.Flush();
            sw.Close();
        }
        public void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "所有支持文件|*.flac;*.ape;*.mp3;*.ogg;*.wav;*.m4a;*.aac;*.fla|所有文件|*.*";
            ofd.Title = "请选择文件";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < ofd.FileNames.Length; i++)
                {
                    AddItems(ofd.FileNames[i]);
                }
            }
        }
        private void add_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void AddMore_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(ofd.SelectedPath);
                    string[] type = ("*.flac;*.ape;*.mp3;*.ogg;*.wav;*.m4a;*.aac;*.fla").Split(';');
                    for (int i = 0; i < type.Length; i++)
                    {
                        FileInfo[] files = dir.GetFiles(type[i]);
                        foreach (FileInfo s in files)
                        {
                            AddItems(s.FullName);
                        }
                    }

                }
                catch
                {

                }
            }
        }

        private void playAll_Click(object sender, EventArgs e)
        {
            if (List.localmusic.Count <= 0) return;
            List.lsong = List.localmusic;
            musicIndex = 0;
            MyPlay(List.lsong[0]);
            flag = false;
            timer3.Enabled = false;
        }//本地音乐播放全部
        private void 播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listsong.SelectedItems.Count != 0)
            {
                musicIndex = listsong.SelectedItems[0].Index;
                MyPlay(List.lsong[musicIndex]);
                musicPlay.Visible = false;
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (listsong.SelectedItems.Count > 0)
            {
                if (musicIndex == listsong.SelectedItems[0].Index)
                {
                    musicPlay.Visible = false;
                    Bass.BASS_ChannelStop(stream);
                }
                for (int k = listsong.SelectedItems[0].Index; k < List.lsong.Count; k++)
                {
                    listsong.Items[k].SubItems[0].Text = k  + "";
                }
                List.localmusic.RemoveAt(listsong.SelectedItems[0].Index);
                listsong.Items.RemoveAt(listsong.SelectedItems[0].Index);
                string s = "";
                for(int i = 0; i < listsong.Items.Count; i++)
                {
                    for(int j=0;j< listsong.Items[i].SubItems.Count; j++)
                    {
                        s += listsong.Items[i].SubItems[j].Text + "*";
                    }
                    s += List.localmusic[i] + "|";
                }
                StreamWriter sw = new StreamWriter(localPath);
                sw.Write(s);
                sw.Flush();
                sw.Close();
                musicIndex--;
            }
        }

        private void 清空列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(localPath);
            string s = "";
            sw.Write(s);
            sw.Flush();
            sw.Close();
            List.lsong.Clear();
            listsong.Items.Clear();
            Bass.BASS_ChannelStop(stream);
            musicPlay.Visible = false;
        }
        private void listsong_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listsong.SelectedItems.Count <= 0) { return; }
            List.lsong = List.localmusic;
            musicIndex = listsong.SelectedItems[0].Index;
            MyPlay(List.lsong[musicIndex]);
            
            flag = false;
            timer3.Enabled = false;
        }
        #endregion
        #region BottomPanel
        public void Stop()
        {
            Bass.BASS_ChannelStop(stream);
            stream = 0;
            musicPlay.Visible = false;
            runTime.Text = allTime.Text = "00:00";
            _wplay = false;
            play.Image = Properties.Resources.play;
        }
        private void play_Click(object sender, EventArgs e)
        {
            if (stream == 0) { return; }
            if (_wplay == false)
            {
                Bass.BASS_ChannelPlay(stream, false);
                play.Image = Properties.Resources.play_move;
                _wplay = true;
                timer2.Enabled = true;
            }
            else
            {
                Bass.BASS_ChannelPause(stream);
                play.Image = Properties.Resources.pause_move;
                _wplay = false;
                timer2.Enabled = false;
            }
        }
        private void next_Click(object sender, EventArgs e)
        {
            if (stream == 0) return;
            if (playChange == 1 || playChange == 2)
            {
                musicIndex++;
                if(musicIndex > List.lsong.Count - 1)
                {
                    if (playChange == 2)
                    {
                        musicIndex = 0;
                    }
                    else if (playChange == 1)
                    {
                        Stop();
                        return;
                    }
                }
            }else if (playChange == 4)
            {
                Random r = new Random();
                int i = r.Next(0, List.lsong.Count);
                musicIndex = i;
            }
            MyPlay(List.lsong[musicIndex]);
        }

        private void back_Click(object sender, EventArgs e)
        {
            if (stream == 0) return;
            if (playChange == 1 || playChange == 2)
            {
                musicIndex--;
                if (playChange == 2 && musicIndex < 0)
                {
                    musicIndex = List.lsong.Count - 1;
                }
                else if (playChange == 1 && musicIndex < 0)
                {
                    Stop();
                    return;
                }
            }
            else if (playChange == 4)
            {
                Random r = new Random();
                int i = r.Next(0, List.lsong.Count);
                musicIndex = i;
            }
            MyPlay(List.lsong[musicIndex]);
        }
        public void SetImg(string img)
        {
            string names = Singer.Text + " - " + musicName.Text;
            string filePath = Environment.CurrentDirectory + "\\Images\\";//头像保存
            if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
            if (!File.Exists(filePath + names + ".jpg")&& img!=null)
            {
                web.DownloadFile(img, filePath + names + ".jpg");
            }try
            {
                SingerImg.Image = Image.FromFile(Environment.CurrentDirectory + "\\Images\\" + names + ".jpg");
                MusicLrcImg.Image = SingerImg.Image;
            }
            catch
            {
                SingerImg.Image = Properties.Resources.UnKnown;
                MusicLrcImg.Image = Properties.Resources.UnKnown;
            }
        }
        private void TimeTrack_MouseDown(object sender, MouseEventArgs e)
        {
            TimeTrack.Value -= 0;
        }

        double schedule = 0;//播放进度

        private void SoundTrack_MouseDown(object sender, MouseEventArgs e)
        {
            SoundTrack.Value -= 0;
            Volume = SoundTrack.Value;
        }

        private void SoundTrack_Scroll(object sender, EventArgs e)
        {
            Volume = SoundTrack.Value;
        }
        bool TrackFlag = false;
        private void TimeTrack_Scroll(object sender, EventArgs e)
        {
            double s = Schedule;
            Schedule =  (double)TimeTrack.Value/100 ;
            if (MusicLrcPannel.Visible)
            {
                if (s < schedule)
                {
                    TrackFlag = false;
                }
                else
                {
                    TrackFlag = true;
                }
                   
            }
        }

        /// <summary>
        /// 播放进度0—1
        /// </summary>
        public double Schedule
        {
            get
            {
                if (stream == 0 || Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream)) == -1)
                {
                    schedule = 0;
                }
                else
                {
                    schedule = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream)) / Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                }
                return schedule;
            }
            set
            {
                schedule = value;
                double temp = schedule * Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                Bass.BASS_ChannelSetPosition(stream, Bass.BASS_ChannelSeconds2Bytes(stream, temp));
            }
        }
        #endregion
        #region webMusic
        RankAPI ra = new RankAPI();//获取排名api
        int type;//榜单类型
        static int indexRank = 1,page=0,size=22;
        string[] ranks;//当前列表信息
        

        private void RankList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (RankList.SelectedItems.Count > 0)
            {
                List.lsong = List.rankmusic;
                musicIndex = RankList.SelectedItems[0].Index;
                MyPlay(List.lsong[RankList.SelectedItems[0].Index]);
                flag = false;
                timer3.Enabled = false;
            }
        }
        public void ClickRank()
        {
            page = 0; indexRank = 1;
            RankList.Items.Clear();
            List.lsong.Clear();
            List.rankSite.Clear();
            try
            {
                string[] allRanks;
                allRanks = new string[size];
                allRanks = ra.GetRank(type, size, page);
                foreach (string s in allRanks)
                {
                    AddRankItem(s);
                }
                page += size;
            }
            catch
            {
                MessageBox.Show("网络丢失了...请检查你的网路");
                return;
            }
        }
        private void Rank1_Click(object sender, EventArgs e)
        {
            type = 1;
            ClickRank();
        }
        private void Rank2_Click(object sender, EventArgs e)
        {
            type = 2; 
            ClickRank();
        }

        private void Rank3_Click(object sender, EventArgs e)
        {
            type = 11; 
            ClickRank();
        }

        private void Rank4_Click(object sender, EventArgs e)
        {
            type = 12; 
            ClickRank();
        }

        private void Rank5_Click(object sender, EventArgs e)
        {
            type = 16; 
            ClickRank();
        }

        private void Rank6_Click(object sender, EventArgs e)
        {
            type = 21; 
            ClickRank();
        }

        private void Rank7_Click(object sender, EventArgs e)
        {
            type = 22; 
            ClickRank();
        }

        private void Rank8_Click(object sender, EventArgs e)
        {
            type = 23; 
            ClickRank();
        }


        private void Rank9_Click(object sender, EventArgs e)
        {
            type = 24;
            ClickRank();
        }

        private void Rank10_Click(object sender, EventArgs e)
        {
            type = 25;
            ClickRank();
        }



        public void AddRankItem(string str)
        {
            if (str == null) { return; }
            ranks = str.Split('*');
            ListViewItem l = new ListViewItem(indexRank.ToString());
            for(int i = 0; i < 5; i++)
            {
                if (i == 3)
                {
                    if (ranks[i] == "True") { ranks[i] = "✔"; }
                    else { ranks[i] = "✘"; }
                }
                l.SubItems.Add(ranks[i].Trim());
            }
            RankList.Items.Add(l);
            List.rankmusic.Add(ranks[5]);
            string more = ranks[6] + "*" + ranks[7] + "*" + ranks[8];
            List.rankSite.Add(more);
            indexRank++;
        }
        #endregion
        #region SearchMusic
        SearchAPI sa = new SearchAPI();
        
        static int SearchT = 1, limit = 30, offsets = 0, searIndex = 1;

        private void SearchResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SearchResult.SelectedItems.Count > 0)
            {
                List.lsong = List.searchmusic;
                musicIndex = SearchResult.SelectedItems[0].Index;
                MyPlay(List.lsong[SearchResult.SelectedItems[0].Index].Split('*')[0]);
                flag = false;
                timer3.Enabled = false;
            }
        }


        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(null, null);
            }//按回车调用按钮
        }

        static string SearchStr;//记录搜索内容
        public void ClearAllSearchStyle()
        {
            Search1.ForeColor = Color.Black;
            Search2.ForeColor = Color.Black;
            Search3.ForeColor = Color.Black;
            Search1.BackColor = Color.FromArgb(250, 250, 250);
            Search1.FlatAppearance.BorderColor= Color.FromArgb(198, 198, 198);
            Search2.BackColor = Color.FromArgb(250, 250, 250);
            Search2.FlatAppearance.BorderColor = Color.FromArgb(198, 198, 198);
            Search3.BackColor = Color.FromArgb(250, 250, 250);
            Search3.FlatAppearance.BorderColor = Color.FromArgb(198, 198, 198);
        }
        private void Search1_Click(object sender, EventArgs e)
        {
            if (Search1.ForeColor == Color.White) { return; }
            ClearAllSearchStyle();
            List.searchmusic.Clear();
            Search1.ForeColor = Color.White;
            Search1.BackColor = Color.FromArgb(198, 47, 47);
            Search1.FlatAppearance.BorderColor = Color.FromArgb(198, 47, 47);
            SearchT = 1;
            ClickSearch();
            label20.Text = "时长";
        }

        private void musicPlay_Click(object sender, EventArgs e)///safadgsfhsfghsgfh
        {
            MusicLrcPannel.Visible = true;
            //Thread th = new Thread(Draw);
            //th.IsBackground = true;
            //th.Start();
            
        }
        private void SingerImg_Click(object sender, EventArgs e)
        {
            MusicLrcPannel.Visible = true;
            
        }
        private void Search2_Click(object sender, EventArgs e)
        {
            if (Search2.ForeColor == Color.White) { return; }
            ClearAllSearchStyle();
            List.searchmusic.Clear();
            Search2.ForeColor = Color.White;
            Search2.BackColor = Color.FromArgb(198, 47, 47);
            Search2.FlatAppearance.BorderColor = Color.FromArgb(198, 47, 47);
            SearchT = 2;
            ClickSearch();
            label20.Text = "";

        }

        private void Search3_Click(object sender, EventArgs e)
        {
            if (Search3.ForeColor == Color.White) { return; }
            ClearAllSearchStyle();
            List.searchmusic.Clear();
            Search3.ForeColor = Color.White;
            Search3.BackColor = Color.FromArgb(198, 47, 47);
            Search3.FlatAppearance.BorderColor = Color.FromArgb(198, 47, 47);
            SearchT = 3;
            ClickSearch();
            label20.Text = "-";
        }
        string[] re;
        string[] SearResult;//记录搜索结果
        public void Get()
        {
            try
            {
                int ok = 0;
                re = sa.GetHtmlFile(SearchT, SearchStr, limit, offsets);
                foreach (string ss in re)
                {
                    AddSearchItem(ss);
                    ok++;
                }
                ScollIndex1 = ok - 25;
                label18.Text = "搜索\"" + SearchStr + "\"，搜索成功！";
                offsets += limit;
            }
            catch { label18.Text = "网络不给力哦，请检查你的网络设置~"; return; }
        }
        public void ClickSearch()
        {
            re = new string[limit];
            searIndex = 1;offsets = 0;
            SearchResult.Items.Clear();
            List.lsong.Clear();
            List.searchSite.Clear();
            Get();
            //Thread th = new Thread(new ThreadStart(Get));
            //th.SetApartmentState(ApartmentState.STA);
            //th.Start();
        }

        private void MusicLrcVis_Click(object sender, EventArgs e)
        {//显示打开歌词界面前的界面
            MusicLrcPannel.Visible = false;
            if (open == 1) { LocalMusicc.Visible = true; }
            else if(open==2) { RankMusic.Visible = true; }
            else if(open==3) { SearchMusic.Visible = true; }
            else { DownloadPanel.Visible = true; }
        }
        

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Trim() == "") { return; }
            List.searchmusic.Clear();
            MusicLrcPannel.Visible = false;
            label18.Text = "搜素中...";
            musicPlay.Visible = false;
            RemoveAllPanel();
            SearchMusic.Visible = true;
            SearchStr = txtSearch.Text.Trim();
            ClickSearch();
            open = 3;
        }
        public void AddSearchItem(string str)
        {
            if (str == null) { return; }
            SearResult = str.Split('*');
            ListViewItem l = new ListViewItem(searIndex.ToString());
            l.SubItems.Add(SearResult[1]);
            l.SubItems.Add(SearResult[3]);
            l.SubItems.Add(SearResult[5]);
            if (Search1.ForeColor == Color.White) { l.SubItems.Add(ChangeTime(double.Parse( sa.GetTime(SearResult[0])) / 1000)); }
            else if (Search2.ForeColor == Color.White) { l.SubItems.Add(sa.GetTime(SearResult[0])); }
            else { l.SubItems.Add("-"); }
         //   stream = Bass.BASS_StreamCreateURL(SearResult[9], 0, BASSFlag.BASS_STREAM_STATUS, null, IntPtr.Zero);
          //  l.SubItems.Add(ChangeTime(Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream))));
            SearchResult.Items.Add(l);
            List.searchmusic.Add( SearResult[7]);
            string allids = SearResult[6] + "*" + SearResult[0] + "*" + SearResult[2];
            if (SearchT == 2) { allids+=("*" + SearResult[8]); }
            List.searchSite.Add(allids);
            searIndex++;
        }
        #endregion
        #region MusicLrcPannel
        GetLrc gl = new GetLrc();
        /// <summary>
        /// 获取FFT采样数据，返回512个浮点采样数据
        /// </summary>
        /// <returns></returns>
        public float[] GetFFTData()
        {
            float[] fft = new float[512];
            Bass.BASS_ChannelGetData(stream, fft, (int)BASSData.BASS_DATA_FFT1024);
            return fft;
        }
        //public void Draw()
        //{
        //    while (true)
        //    {
        //        if (!_wplay) break;
        //        float[] s = GetFFTData();
        //        Graphics g = FFT.CreateGraphics();
        //        Pen p = new Pen(Color.White, 5);
        //        for (int i = 0; i < s.Length; i++)
        //        {
        //            g.DrawRectangle(p, i + 10, 0, 5, 100 * s[i]);
        //        }
        //        g.Clear(Color.FromArgb(221, 221, 221));
        //    }//绘制频谱
        //}
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (RankList.SelectedItems.Count > 0)
            {
                List.lsong = List.rankmusic;
                musicIndex = RankList.SelectedItems[0].Index;
                MyPlay(List.lsong[RankList.SelectedItems[0].Index]);
                flag = false;
                timer3.Enabled = false;
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (SearchResult.SelectedItems.Count > 0)
            {
                List.lsong = List.searchmusic;
                musicIndex = SearchResult.SelectedItems[0].Index;
                MyPlay(List.lsong[SearchResult.SelectedItems[0].Index].Split('*')[0]);
                flag = false;
                timer3.Enabled = false;
            }
        }

        //截取字符串
        bool flag = false;//记录MusicLrc是否打开过

        private void PlayRankAll_Click(object sender, EventArgs e)
        {
            if (List.rankmusic.Count <= 0) return;
            List.lsong = List.rankmusic;
            musicIndex = 0;
            MyPlay(List.lsong[0]);
            flag = false;
            timer3.Enabled = false;
        }


        private void PlayDownloadAll_Click(object sender, EventArgs e)
        {//下载管理的播放全部歌曲
            if (down.Count <= 0) return;
            List.lsong = down;
            musicIndex = 0;
            MyPlay(List.lsong[0]);
            flag = false;
            timer3.Enabled = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {//歌词搜索

            SearchAllLrc sal = new SearchAllLrc();
            SearchAllLrc.name = MusicLrcTitle.Text;
            SearchAllLrc.singer = MusicLrcSinger.Text.Remove(0,3);
            SearchAllLrc.time = allTime.Text;
            sal.ShowDialog();
        }
        static int playChange = 1;//播放方式
        private void PlayChange_Click(object sender, EventArgs e)
        {
            if (playChange == 1)
            {
                PlayChanges.Image = Properties.Resources._2;
                playChange = 2;
            }
            else if (playChange == 2)
            {
                PlayChanges.Image = Properties.Resources._3;
                playChange = 3;
            }else if (playChange == 3)
            {
                PlayChanges.Image = Properties.Resources._4;
                playChange = 4;
            }
            else
            {
                PlayChanges.Image = Properties.Resources._1;
                playChange = 1;
            }
        }

        static int open;//记录打开前页面

        private void DownloadMusic_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (DownloadMusic.SelectedItems.Count <= 0) return;
            List.lsong = down;
            musicIndex = DownloadMusic.SelectedItems[0].Index;
            MyPlay(List.lsong[musicIndex]);
            flag = false;
            timer3.Enabled = false;
        }

        private void MusicLrcPannel_VisibleChanged(object sender, EventArgs e)
        {//显示歌词界面显示
            if (MusicLrcPannel.Visible)
            {
                if (!flag)
                {
                    UpdatePanel();
                }
                if (LocalMusicc.Visible)
                {
                    LocalMusicc.Visible = false;
                    open = 1;
                }
                else if (RankMusic.Visible)
                {
                    RankMusic.Visible = false;
                    open = 2;
                }
                else if(SearchMusic.Visible)
                {
                    SearchMusic.Visible = false;
                    open = 3;
                }else
                {
                    DownloadPanel.Visible = false;
                    open = 4;
                }
            }
        }

        private void MiniState_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void skinLabel1_Click(object sender, EventArgs e)
        {
            if (musicPlay.Visible)
            {
                musicPlay_Click(null, null);
            }
        }

        private void PlayList_Click(object sender, EventArgs e)
        {
            if (skinListBox1.Visible)
            {
                skinListBox1.Visible = false;
            }else
            {
                skinListBox1.Visible = true;
                skinListBox1.Items.Clear();
                for (int i = 0; i < List.lsong.Count; i++)
                {
                    string[] temp = List.lsong[i].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    SkinListBoxItem sl = new SkinListBoxItem(temp[temp.Length-1]);
                    skinListBox1.Items.Add(sl);
                }
            }
            
        }

        public void UpdatePanel()
        {
            GetLrc.lrc = null;
            string frm = "", album = "";
            string filePath = Environment.CurrentDirectory + "\\Lyrics\\";//缓存保存
            string name = Singer.Text + " - " + musicName.Text;
            if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
            try
            {
                if (open == 1 || open == 4)
                {
                    album = "专辑：无";
                    frm = "来源：本地";
                    if (!File.Exists(filePath + name + ".lrc"))
                    {
                        gl.GetKuGouLrc(musicName.Text, Singer.Text, alltime);
                    }
                }
                else if (open == 2)
                {
                    album = "专辑：" + RankList.Items[musicIndex].SubItems[3].Text;
                    frm = "来源：百度歌单";
                    if (!File.Exists(filePath + name + ".lrc"))
                    {
                        gl.GetKuGouLrc(musicName.Text, Singer.Text, alltime);
                    }
                }
                else
                {
                    album = "专辑：" + SearchResult.Items[musicIndex].SubItems[3].Text;
                    if (Search1.ForeColor == Color.White)
                    {
                        frm = "来源：网易云搜索";
                        string musicid = List.searchSite[musicIndex].Split('*')[1];
                        if (!File.Exists(filePath + name + ".lrc"))
                        {
                            gl.GetSmusicLrc(musicid,name);
                        }
                    }
                    else if (Search2.ForeColor == Color.White)
                    {
                        frm = "来源：百度音乐搜索";
                        if (!File.Exists(filePath + name + ".lrc"))
                        {
                            gl.GetKuGouLrc(musicName.Text, Singer.Text, alltime);
                        }
                    }
                    else
                    {
                        frm = "来源：虾米音乐搜索";
                        if (!File.Exists(filePath + name + ".lrc"))
                        {
                            gl.GetKuGouLrc(musicName.Text, Singer.Text, alltime);
                        }
                    }
                }
            }
            catch { }
            try
            {
                gl.SetLrc(name);
            }
            catch { }
            MusicLrcFrom.Text = frm;
            MusicLrcAlbum.Text = album;
            FormatLrc(GetLrc.lrc);
            SetLrc();
            MusicLrcSinger.Text = "歌手：" + Singer.Text;
            MusicLrcTitle.Text = musicName.Text;
            flag = true;
            timer3.Enabled = true;
        }
        public void SetLrc()
        {
            LrcShow.Controls.Clear();
            for (int i = 0; i < List.listLrc.Count; i++)//添加歌词
            {
                Label lbl = new Label();
                lbl.AutoSize = false;
                lbl.Size = new Size(560, 30);
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Text = List.listLrc[i].Trim();
                lbl.Font = new Font("微软雅黑", 11);
                lbl.Location = new Point(0, 30 * i + (LrcShow.Height - 10) / 2);//歌词居中
                LrcShow.Controls.Add(lbl);
            }
        }
        public static bool lrcFlag = false;//歌词文件是否被改变
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (lrcFlag)
            {
                SetLrc();
                lrcFlag = false;
            }
            double currentTime = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));
            for (int i = 0; i < List.listTime.Count - 1; i++)
            {
                if ((currentTime >= List.listTime[i] && currentTime < List.listTime[i + 1]))
                {
                    try
                    {
                        LrcShow.Controls[i].ForeColor = Color.Red;
                        LrcShow.Controls[i].Font = new Font("微软雅黑", 15);
                        if (LrcShow.Controls[i].Location.Y > LrcShow.Height / 2&&TrackFlag==false)
                        {
                            for (int j = 0; j < LrcShow.Controls.Count; j++)
                            {
                                LrcShow.Controls[j].Location = new Point(LrcShow.Controls[j].Location.X, LrcShow.Controls[j].Location.Y - 1);
                            }

                        }else if (TrackFlag == true)
                        {
                            for (int j = 0; j < LrcShow.Controls.Count; j++)
                            {
                                LrcShow.Controls[j].Location = new Point(LrcShow.Controls[j].Location.X, LrcShow.Controls[j].Location.Y + 1);
                            }
                            if (LrcShow.Controls[i].Location.Y >= LrcShow.Height / 2) TrackFlag = false;
                        }
                    }
                    catch { }
                }
                else
                {
                    try { LrcShow.Controls[i].ForeColor = Color.Black;
                        LrcShow.Controls[i].Font = new Font("微软雅黑", 11);
                    }
                    catch { }//更换歌曲偶发错误
                }
            }
            if(List.listTime.Count>0)
            if(currentTime >= List.listTime[List.listTime.Count - 1])
            {
                LrcShow.Controls[LrcShow.Controls.Count-1].ForeColor = Color.Red;
                LrcShow.Controls[LrcShow.Controls.Count - 1].Font = new Font("微软雅黑", 15);
            }
        }

        private void skinListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {//双击播放列表
            MyPlay(List.lsong[skinListBox1.SelectedIndex]);
        }

        private void About_Click(object sender, EventArgs e)
        {
            AboutMe a = new AboutMe();
            a.ShowDialog();
        }
        static int vol = 50;
        private void sound_Click(object sender, EventArgs e)
        {
            if (SoundTrack.Value != 0)
            {
                vol = SoundTrack.Value;
                SoundTrack.Value = 0;
                Volume = SoundTrack.Value;
            }else
            {
                SoundTrack.Value = vol;
                Volume = SoundTrack.Value;
            }
            
        }

        public void FormatLrc(string[] lrcText)
        {
            List.listLrc.Clear();
            List.listTime.Clear();
            if (lrcText == null||lrcText[0]== "暂无歌词！") { List.listLrc.Add("暂无歌词！"); return; }
            for (int i = 0; i < lrcText.Length; i++)
            {
                //lrcTemp[0]  00:00.00 时间
                //lrcTemp[1]  歌词
                string[] lrcTemp = lrcText[i].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                //将歌词存储到集合中
                if (lrcTemp.Length <= 1) continue;
                List.listLrc.Add(lrcTemp[1]);
                string[] lrcNewTemp = lrcTemp[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                //00.00
                double time = double.Parse(lrcNewTemp[0]) * 60 + double.Parse(lrcNewTemp[1]);
                //将最终截取到的时间 放到List.listTime中
                List.listTime.Add(time);
            }
        }
        #endregion
        #region DownloadPanel
        public List<string> down = new List<string>();
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (SearchResult.SelectedItems.Count > 0)
            {
                int i = SearchResult.SelectedItems[0].Index;
                if (Search2.ForeColor == Color.White)
                {
                    d.SetSite(SearchResult.Items[i].SubItems[2].Text + " - " + SearchResult.Items[i].SubItems[1].Text + "*" + List.searchSite[i].Split('*')[1], null, 1);
                    d.ShowDialog();
                }
                else if (Search1.ForeColor == Color.White)
                {
                    d.SetSite(SearchResult.Items[i].SubItems[2].Text + " - " + SearchResult.Items[i].SubItems[1].Text +"*"+ List.searchSite[i].Split('*')[1], null, 2);
                    d.ShowDialog();
                }
                else
                {
                    d.SetSite(SearchResult.Items[i].SubItems[2].Text + " - " + SearchResult.Items[i].SubItems[1].Text, List.searchmusic[i].Split('*')[0], 3);
                    d.ShowDialog();
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (RankList.SelectedItems.Count > 0)
            {
                int i = RankList.SelectedItems[0].Index;
                d.SetSite(RankList.Items[i].SubItems[2].Text + " - " + RankList.Items[i].SubItems[1].Text + "*" + List.rankSite[i].Split('*')[2], null, 1);
                d.ShowDialog();
            }
        }
        int dindex = 1;
        public void AddDownloadItems(string str)
        {
            int temp = stream;
            string[] all = str.Split('*');
            down.Add(all[0]);
            FileInfo file = new FileInfo(all[0]);
            ListViewItem l = new ListViewItem(dindex.ToString());
            l.SubItems.Add(all[1].Split('-')[1].Trim());
            l.SubItems.Add(all[1].Split('-')[0].Trim());
            stream = Bass.BASS_StreamCreateFile(all[0], 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT);
            l.SubItems.Add(ChangeTime(Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream))));
            double size = Convert.ToDouble(file.Length) / 1024 / 1024;
            l.SubItems.Add(size.ToString("0.0"+"MB"));
            l.SubItems.Add(all[2]);
            stream = temp;
            dindex++;
            DownloadMusic.Items.Add(l);
        }
        #endregion
       
    }
    /*
        string filePath = Environment.CurrentDirectory + "\\Cache\\";//缓存保存
       if (!Directory.Exists(filePath)){ Directory.CreateDirectory(filePath); }
     */


}

