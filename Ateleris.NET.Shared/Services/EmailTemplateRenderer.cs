using System;
using System.Text;

namespace Ateleris.NET.Shared.Services;

public class EmailTemplateRenderer(EmailTemplateOptions? options = null)
{
    private readonly EmailTemplateOptions _options = options ?? new EmailTemplateOptions();

    public string RenderStandardEmail(string title, string message, string buttonUrl, string buttonText)
    {
        var content = new StringBuilder();
        content.Append(FormatElement(_options.TitleFormat, title));
        content.Append(FormatElement(_options.ContentFormat, $"<p>{ConvertNewlinesToHtml(message)}</p>"));

        if (!string.IsNullOrEmpty(buttonUrl) && !string.IsNullOrEmpty(buttonText))
        {
            content.Append(FormatElement(_options.ButtonFormat, buttonUrl, buttonText));
        }

        content.Append(FormatElement(_options.FooterFormat, DateTime.Now.Year, _options.Domain));

        return WrapInContainer(content.ToString());
    }

    public string RenderCodeEmail(string title, string message, string code)
    {
        var content = new StringBuilder();
        content.Append(FormatElement(_options.TitleFormat, title));
        content.Append(FormatElement(_options.ContentFormat, $"<p>{ConvertNewlinesToHtml(message)}</p>"));
        content.Append(FormatElement(_options.CodeFormat, code));
        content.Append(FormatElement(_options.FooterFormat, DateTime.Now.Year, _options.Domain));

        return WrapInContainer(content.ToString());
    }

    private string WrapInContainer(string content)
    {
        return $"{_options.ContainerStart}{content}{_options.ContainerEnd}";
    }

    private static string FormatElement(string format, params object[] args)
    {
        return string.Format(format, args);
    }

    private static string ConvertNewlinesToHtml(string text)
    {
        return text.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
    }
}

public class EmailTemplateOptions
{
    public string Domain { get; set; } = "example.com";
    public string ContainerStart { get; set; } = "<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0; padding: 20px; color: #333;'>";
    public string ContainerEnd { get; set; } = "</div>";
    public string TitleFormat { get; set; } = "<div style='margin-bottom: 30px;'><h2 style='color: #2c3e50;'>{0}</h2></div>";
    public string ContentFormat { get; set; } = "<div style='margin-bottom: 30px; line-height: 1.5;'>{0}</div>";
    public string ButtonFormat { get; set; } = "<div style='margin: 30px 0;'><a href='{0}' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>{1}</a></div>";
    public string FooterFormat { get; set; } = "<div style='margin-top: 30px; font-size: 12px; color: #7f8c8d;'><p>© {0} {1}</p></div>";
    public string CodeFormat { get; set; } = "<div style='margin: 30px 0; padding: 12px; background-color: #f8f9fa; border: 1px dashed #ccc;'><span style='font-size: 20px; font-weight: bold; letter-spacing: 2px;'>{0}</span></div>";
}
