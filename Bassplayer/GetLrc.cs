using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Bassplayer
{
    class GetLrc
    {
        WebClient web = new WebClient();
        string webSite, html;
        byte[] buffer;
        public static string[] lrc;
        string filePath = Environment.CurrentDirectory + "\\Lyrics\\";//保存lrc
        /// <summary>
        /// 从酷狗获取默认歌词
        /// </summary>
        /// <param name="name">歌曲名</param>
        /// <param name="singer">歌手</param>
        /// <param name="time">歌曲时间</param>
        public void GetKuGouLrc(string name,string singer,double time)
        {
            if(GetAllKuGouLrc(name, singer, time).Length == 0) { lrc = new string[1] { "暂无歌词！"};return; }
            WriteFile(GetAllKuGouLrc(name, singer, time)[0], singer + " - " + name + ".lrc");
        }
        public void SetLrc(string name)
        {
            string[] fen = new string[] { "\r\n", "\\\\n", "\n" };
            int i = 0;
            lrc = Regex.Split(File.ReadAllText(filePath + name + ".lrc"), fen[i]);
            while (lrc.Length < 2)
            {
                i++;
                lrc = Regex.Split(File.ReadAllText(filePath + name + ".lrc"), fen[i]);
            }
        }
        public string singerss { get; set; }
       /// <summary>
       /// 酷狗搜索所有歌词
       /// </summary>
       /// <param name="name">歌曲名</param>
       /// <param name="singer">歌手</param>
       /// <param name="time">歌曲时间</param>
       /// <returns>搜索结果</returns>
        public string[] GetAllKuGouLrc(string name, string singer, double time)
        {
            webSite = @"http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword=" + name + "&duration=" + (int)(time *1000)+ "&hash=";
            buffer = web.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("accesskey\":\"(?<key>.+?)\".+?\"id\":\"(?<id>.+?)\""+ singerss);
            MatchCollection mc = Regex.Matches(html, match);
            string[] all = new string[mc.Count];
            for(int i = 0; i < mc.Count; i++)
            {
                all[i] = Lrcs(mc[i].Groups["key"].Value, mc[i].Groups["id"].Value);
                if (singer != "") all[i] += "|" + mc[i].Groups["singer"].Value;
            }
            return all;
        }
        /// <summary>
        /// 获取酷狗歌词
        /// </summary>
        /// <param name="key">文件安全密匙</param>
        /// <param name="id">歌曲id</param>
        /// <returns></returns>
        public string Lrcs(string key,string id)
        {
            webSite = @"http://lyrics.kugou.com/download?ver=1&client=pc&id=" + id + "&accesskey=" + key + "&fmt=lrc&charset=utf8";
            buffer = web.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("content\":\"(?<lrc>.+?)\"");
            MatchCollection mc = Regex.Matches(html, match);
            if (mc != null)
            {
                return toBase64(mc[0].Groups["lrc"].Value);
            }else
            {
                return null;
            }
        }
        /// <summary>
        /// 从网页下载歌词
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="name">文件全路径</param>
        public void DownlownLrc(string url,string name)
        {
            web.DownloadFile(url,  name + ".lrc");
        }
        public void SetBaiduLrc(string name)
        {
            lrc = File.ReadAllLines(filePath + name + ".lrc", Encoding.UTF8);
        }
        /// <summary>
        /// 获取网易云歌词
        /// </summary>
        /// <param name="id">歌曲id</param>
        public void GetSmusicLrc(string id,string name)
        {
            webSite = @"http://music.163.com/api/song/lyric?os=pc&id=" + id + "&lv=-1&kv=-1&tv=-1";
            buffer = web.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("lyric\":\"(?<lrc>.+?)\"");
            MatchCollection mc = Regex.Matches(html, match);
            WriteFile(mc[0].Groups["lrc"].Value, name + ".lrc");
        }
        public void WriteFile(string lrcs,string name)
        {
            FileStream fs = File.Create(filePath+ name);
            fs.Close();
            StreamWriter sw = new StreamWriter(filePath+ name);
            sw.Write(lrcs);
            sw.Flush();
            sw.Close();
        }
        /// <summary>
        /// Base64转成中文
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string toBase64(string s)
        {
            return DecodeBase64(Encoding.UTF8, s);
        }
        public static string DecodeBase64(Encoding encode, string result)
        {
            string decode = "";
            if (result == null) { return ""; }
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encode.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }
        
    }
}
