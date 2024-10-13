using System.Text;
using Microsoft.Extensions.Primitives;

namespace ModularDiscordBot.Constants;

public static class IconUrls
{
    public static string FooterIcon =>
        new StringBuilder()
            .Append("https://cdn.discordapp.com/attachments/")
            .Append("593969947579777035/1248147974245056553/")
            .Append("Lolth_Icon.png?")
            .Append("ex=66629be2&is=66614a62&hm=4e8bb603b14859f7360070cd3c7b38e886eb776edcb9942c3a95f6f7d49a4014&")
            .ToString();

    public static string ThumbnailIcon =>
        new StringBuilder()
            .Append("https://media.discordapp.net/attachments/")
            .Append("1243587366271189084/1246818328991764530/")
            .Append("3xb0si6diyi91_copy.png?")
            .Append("ex=665dc58e&is=665c740e&hm=")
            .Append("263eb17e934939d411b04341df87f30483162ceb486bc7f5d904feb47c66b963&=&format=webp&quality=lossless")
            .ToString();
}