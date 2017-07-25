using System.Collections.Generic;

namespace Bassplayer
{
    class List
    {
        public static List<string> lsong = new List<string>();//记录当前播放歌曲地址

        public static List<string> localmusic = new List<string>();//记录本地音乐地址

        public static List<string> rankmusic = new List<string>(), rankSite = new List<string>();//记录榜单歌曲地址

        public static List<string> searchmusic = new List<string>(), searchSite = new List<string>();//记录搜索歌曲地址

        public static List<double> listTime = new List<double>();//存储歌词时间

        public static List<string> listLrc = new List<string>();//存储歌词
    }
}
