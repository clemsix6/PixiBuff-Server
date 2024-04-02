using System.Text.Json;
using System.Text.RegularExpressions;


namespace PXServer.Source.Middlewares;


public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return Regex.Replace(name, "(?<!^)([A-Z])", "_$1").ToLower();
    }
}