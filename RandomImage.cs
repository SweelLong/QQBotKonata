using Konata.Core;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
namespace QQBotKonata
{
    internal class RandomImage
    {
        internal static string ImageAPI = "https://www.dmoe.cc/random.php";

        internal static void Friend(Bot obj, string message, uint friendpUin)
        {
            MessageBuilder mb = new();
            if (message.Contains("图"))
            {
                mb.Image(new HttpClient().GetByteArrayAsync(ImageAPI).Result);
                obj.SendFriendMessage(friendpUin, "请求图片中...");
                obj.SendFriendMessage(friendpUin, mb);
            }
        }

        internal static void Group(Bot obj, string message, uint groupUin, uint memberUin)
        {
            MessageBuilder mb = new();
            if (message.ToLower().Contains("api") && !message.ToLower().StartsWith("newapi"))
            {
                obj.SendGroupMessage(groupUin, $"图片Api为：{ImageAPI}\n输入【NewApi】关键词 + URL网址】\n如 NewApi https://sweellong.up.railway.app.php 即可更换\n\n这里有API大全哦：https://blog.csdn.net/likepoems/article/details/123924270");
            }
            //if (message.ToLower().StartsWith("newapi") && memberUin == settings.AdminQQ)
            //{
            //    string newApi = message[7..];
            //    settings.ImageAPI = newApi;
            //    obj.SendGroupMessage(groupUin, $"图片Api成功更换为：{newApi}");
            //}
            //if (message.ToLower().StartsWith("newapi") && memberUin != settings.AdminQQ)
            //{
            //    obj.SendGroupMessage(groupUin, "图片Api更换失败！\n原因：你不是管理员");
            //}
            if (message.Contains("图"))
            {
                //if (settings.AtOperator)
                //{
                mb.At(memberUin);
                //}
                // mb.Text("");
                mb.Image(new HttpClient().GetByteArrayAsync(ImageAPI).Result);
                obj.SendGroupMessage(groupUin, mb);
            }
        }
    }
}