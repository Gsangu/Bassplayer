using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Bassplayer
{
    class SearchAPI
    {
         WebClient web = new WebClient();
         string webSite, html;
         byte[] buffer;
        static string time;
        public string Count()
        {
            try
            {
                MatchCollection mc = Regex.Matches(html, "Count\":(?<count>.+?),");
                return mc[0].Groups["count"].Value;

            }catch
            {
                return "1";
            }
            
        }
        public string GetTime(string id)
        {
            try
            {
                webSite = @"http://music.163.com/api/song/detail/?id=" + id + "Id&ids=%5B" + id + "%5D&csrf_token=";
                buffer = web.DownloadData(webSite);
                html = Encoding.UTF8.GetString(buffer);
                MatchCollection mc = Regex.Matches(html, "playTime\":(?<time>.+?),\"");
                return mc[0].Groups["time"].Value.Replace("}", "");
            }
            catch
            {
                return time;
            }
            
        }
        /// <summary>
        /// 获取搜索结果
        /// </summary>
        /// <param name="type">搜索引擎</param>
        /// <param name="str">搜索内容</param>
        /// <param name="limit">条数</param>
        /// <param name="offset">从第几条开始</param>
        /// <returns></returns>
        public string[] GetHtmlFile(int type, string str, int limit, int offset)
        {
            string match = "";
            if (type == 1)
            {
                webSite = @"http://s.music.163.com/search/get/?src=lofter&type=" + type + "&filterDj=true&s=" + str
               + "&limit=" + limit + "&offset=" + offset + "&callback=loft.w.g.cbFuncSearchMusic";
                match = "\"id\":(?<id>.+?),.+?name\":\"(?<name>.+?)\".+?id\":(?<singerid>.+?),.+?name\":\"(?<singer>.+?)\".+?album\":{\"id\":(?<albumid>.+?),.+?name\":\"(?<album>.+?)\".+?Url\":\"(?<img>.+?)\".+?dio\":\"(?<mp3>.+?)\"";
            }
            else if (type == 2)
            {
                webSite = @"http://tingapi.ting.baidu.com/v1/restserver/ting?from=qianqian&version=2.1.0&method=baidu.ting.search.common&format=json&query=" + str + "&page_no=" + offset + "&page_size=" + limit;
                match = string.Format("title\":\"(?<name>.+?)\".+?id\":\"(?<id>.+?)\".+?author\":\"(?<singer>.+?)\".+?id\":\"(?<singerid>.+?)\".+?title\":\"(?<album>.+?)\".+?id\":\"(?<albumid>.+?)\".+?link\":\"(?<lrc>.+?)\"");
            }
            else if (type == 3)
            {
                webSite = @"http://www.xiami.com/web/search-songs?spm=0.0.0.0.lRIEJS&key=" + str;
                match = string.Format("id\":\"(?<id>.+?)\",\"title\":\"(?<name>.+?)\",\"author\":\"(?<singer>.+?)\",\"cover\":\"(?<img>.+?)\",\"src\":\"(?<mp3>.+?)\"");
            }
            else { return null; }
            WebClient web1 = new WebClient();
            buffer = web1.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            MatchCollection mc = Regex.Matches(html, match);
            string[] Result = new string[limit];
            for (int i = 0 ; i < mc.Count; i++)
            {
                Match item = mc[i];
                if (item.Success)
                {
                    Result[i] += item.Groups["id"].Value + "*";
                    Result[i] += RankAPI.ToGB2312( Regex.Replace(item.Groups["name"].Value, "<.+?>", ""))+ "*";
                    if (type == 3) { Result[i] += "123" + "*"; }
                    else { Result[i] += item.Groups["singerid"].Value + "*"; }
                    Result[i] += RankAPI.ToGB2312( item.Groups["singer"].Value).Replace("<em>","").Replace("<\\/em>", "") + "*";
                    if (type == 3) { Result[i] += "123" + "*" + "无" + "*"; }
                    else
                    {
                        Result[i] += item.Groups["albumid"].Value + "*";
                        if (item.Groups["album"].Value == "\",") { Result[i] += "无" + "*"; }
                        else { Result[i] += RankAPI.ToGB2312(item.Groups["album"].Value).Replace("<em>", "").Replace("<\\/em>", "") + "*"; }
                        if (type == 2) { Result[i] += GetMusic(item.Groups["id"].Value); Result[i] += "*" + item.Groups["lrc"].Value.Replace("\\",""); continue; }
                    }
                    Result[i] += item.Groups["img"].Value.Replace("\\", "") + "*";
                    if (type == 1) { Result[i] += GetSmusic(item.Groups["id"].Value, 128000);continue; }
                    Result[i] += item.Groups["mp3"].Value.Replace("\\", "");
                }
            }
            return Result;
        }
        public string GetMusic(string id)
        {
            WebClient web1 = new WebClient();
            webSite = @"http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.song.playAAC&songid=" + id;
            buffer = web1.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            string match = string.Format("link\":\"(?<mp3>.+?)\".+?big\":\"(?<img>.+?)\".+?publishtime\":\"(?<time>.+?)\"");
            MatchCollection mc = Regex.Matches(html, match);
            string result = mc[0].Groups["img"].Value.Replace("\\","")+"*" + mc[0].Groups["mp3"].Value.Replace("\\", "");
            time = mc[0].Groups["time"].Value;
            return result;
        }
        /// <summary>
        /// 获取网易云音乐地址
        /// </summary>
        /// <param name="id">歌曲id</param>
        /// <param name="br">歌曲码率(128000,320000)</param>
        /// <returns></returns>
        public string GetSmusic(string id,int br)
        {
            WebClient web1 = new WebClient();
            webSite = @"http://music.163.com/api/song/enhance/download/url?br=" + br + "&id=" + id;
            buffer = web1.DownloadData(webSite);
            html = Encoding.UTF8.GetString(buffer);
            MatchCollection mc = Regex.Matches(html, "\"url\":\"(?<mp3>.+?)\".+?size\"(?<size>.+?),");
            if(br== 128000)
            {
                return mc[0].Groups["mp3"].Value;
            }
            else if(br == 128001)
            {
                return mc[0].Groups["size"].Value + "*128*" + mc[0].Groups["mp3"].Value;
            }
            else
            {
                return mc[0].Groups["size"].Value + "*320*" + mc[0].Groups["mp3"].Value;
            }
        }
    }
}
