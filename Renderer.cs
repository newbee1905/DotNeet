namespace DotNeet;

using System.Text;
using SharpWebview;
using SharpWebview.Content;

/// <summary>
/// Since this program is more focused on c# and oop
/// This renderer will only basically render all the images
/// max width.
///
/// Normally, render all image with base64 is not recommened
/// since it will make the html file too large.
/// Howerver, in this case where we directly insert the string
/// big file is not a problem.
/// </summary>
public class Renderer<TPage> : Webview where TPage : BasePage, new()
{
	public Renderer() : base() { }

	public void Start(List<TPage> pages, string mangaName, string chapterName)
	{
		StringBuilder html = new();
		SetTitle($"{mangaName} - {chapterName}");
		foreach (var page in pages)
			html.Append($"<img src={page.base64} width='100%' style='display:block;margin-left:auto;margin-right:auto;'/>");
		Navigate(new HtmlContent(html.ToString()));
		Run();
	}
}
