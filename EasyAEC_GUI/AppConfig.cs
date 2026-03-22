using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyAEC_GUI;

/// <summary>
/// 应用程序配置类，用于序列化和反序列化配置信息。
/// </summary>
public class AppConfig
{
    [JsonPropertyName("inDevice")]
    public string InDevice { get; set; } = string.Empty;

    [JsonPropertyName("refDevice")]
    public string RefDevice { get; set; } = string.Empty;

    [JsonPropertyName("outDevice")]
    public string OutDevice { get; set; } = string.Empty;

    [JsonPropertyName("suppressionLevel")]
    public string SuppressionLevel { get; set; } = "标准";

    [JsonPropertyName("delayMs")]
    public int DelayMs { get; set; } = 60;

    [JsonPropertyName("isDebug")]
    public bool IsDebug { get; set; } = false;

    /// <summary>
    /// 将配置序列化为 JSON 字符串。
    /// </summary>
    public string ToJson()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// 从 JSON 字符串反序列化配置。
    /// </summary>
    public static AppConfig? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AppConfig>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 从配置文件加载配置。
    /// </summary>
    public static AppConfig? LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return FromJson(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 保存配置到文件。
    /// </summary>
    public void SaveToFile(string filePath)
    {
        try
        {
            var json = ToJson();
            File.WriteAllText(filePath, json);
        }
        catch
        {
            throw;
        }
    }
}
