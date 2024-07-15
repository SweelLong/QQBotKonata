using System.Text;

namespace QQBotKonata
{
    internal class AntiRecall
    {
        internal static string path = Path.Combine(Directory.GetCurrentDirectory(), "AntiRecall.txt");

        internal static Dictionary<uint, string> HistoryCall = new();

        internal static void Read()
        {
            if(!File.Exists(path))
            {
                Write(); 
            }
            StreamReader sr = new(path, Encoding.UTF8);
            while (sr.ReadLine() != null)
            {
                string[] li = sr.ReadLine().Split(",");
                HistoryCall.Add(uint.Parse(li[0]), li[1]);
            }
        }

        internal static void Write()
        {
            FileStream fs = new(path, FileMode.Create);
            StreamWriter sw = new(fs);
            foreach (var d in HistoryCall)
            {
                sw.WriteLine(d.Key + "," + d.Value);
            }
            sw.Flush();
            sw.Close();
            fs.Dispose();
            if (HistoryCall.Count > 100)// 储存消息条数上限
            {
                HistoryCall.Clear();
            }
        }
    }
}