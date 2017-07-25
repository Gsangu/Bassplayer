using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Bassplayer
{
    class RankAPI
    {
        WebClient web = new WebClient();
        string webSite, html;
        byte[] buffer;
        /// <summary>
        /// 获取榜单数据
        /// </summary>
        /// <param name="type">类型(1-新歌榜,2-热歌榜,11-摇滚榜,12-爵士,16-流行,21-欧美金曲榜,22-经典老歌榜,23-情歌对唱榜,24-影视金曲榜,25-网络歌曲榜)</param>
        /// <param name="size">条数(1-500)</param>
        /// <param name="page">从第几条开始</param>
        /// <returns></returns>
        public string[] GetRank(int type, int size, int page)
        {
            webSite = @"http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.billboard.billList&type=" + type + "&size=" + size + "&offset=" + page;
            buffer = web.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("lrclink\":\"(?<lrc>.+?)\".+?g_id\":\"(?<id>.+?)\".+?title\":\"(?<name>.+?)\".+?author\":\"(?<singer>.+?)\"");
            MatchCollection mc = Regex.Matches(html, match);
            string[] Result = new string[size];
            int i = 0;
            foreach(Match item in mc)
            {
                if (item.Success)
                {
                    Result[i] = ToGB2312(item.Groups["name"].Value) + "*";
                    Result[i] += ToGB2312(item.Groups["singer"].Value) + "*";
                    Result[i] += GetMusic(item.Groups["id"].Value) + "*";
                    Result[i] += item.Groups["lrc"].Value.Replace("\\", "") + "*" + item.Groups["id"].Value;
                }
                i++;
            }
            return Result;
        }
        public string GetMusic(string id)
        {
            webSite = @"http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.song.playAAC&songid=" + id;
            buffer = web.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("link\":\"(?<mp3>.+?)\".+?rate\":\"(?<rate>.+?)\".+?premium\":\"(?<img>.+?)\".+?publishtime\":\"(?<publishtime>.+?)\".+?album_title\":\"(?<album>.+?)\"");
            MatchCollection mc = Regex.Matches(html, match);
            bool flag = false;//无损格式标志
            string[] rate = mc[0].Groups["rate"].Value.Split(',');
            for (int i = 0; i < rate.Length; i++)
            {
                if (rate[i] == "flac")
                {
                    flag = true;
                }
            }
            string img = mc[0].Groups["img"].Value.Replace("\\", "");
            string album = ToGB2312(mc[0].Groups["album"].Value);
            string publishtime = mc[0].Groups["publishtime"].Value;
            return album + "*" + flag + "*" + publishtime + "*" + mc[0].Groups["mp3"].Value.Replace("\\", "") + "*" + img;
        }
        public static string ToGB2312(string str)
        {
            string r = "";
            MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            string orther = Regex.Replace(str, @"\\u([\w]{2})([\w]{2})", "");
            byte[] bts = new byte[2];
            foreach (Match m in mc)
            {
                bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                r += Encoding.Unicode.GetString(bts);

            }
            return r + orther;
        }
    }
}
