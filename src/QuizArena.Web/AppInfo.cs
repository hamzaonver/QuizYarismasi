using System.Reflection;

namespace QuizArena.Web;

// Hafta 8: Sitenin sürüm numarası. Değeri QuizArena.Web.csproj içindeki <Version> satırından gelir.
// Sayfanın altında (footer) gösteriyorum, böylece hangi sürümün çalıştığı görülür.
public static class AppInfo
{
    public static string Version
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null)
                return "0.0.0";
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
