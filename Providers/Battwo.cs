namespace DotNeet.Providers.Battwo;

using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using PuppeteerSharp;

public class Page : BasePage
{
}

public class Chapter : BaseChapter<Page>
{
	public string title { get; set; }
	public string id { get; set; }
	public string chap { get; set; }
	public List<Page> md_images { get; set; }

}

public class Manga : BaseManga<Chapter>
{
	public string title { get; set; }
	public string slug { get; set; }
	public string id { get; set; }
	public string desc { get; set; }
	public string last_chapter { get; set; }
}

public class Provider : MangaProvider<Manga, Chapter, Page>
{
	public readonly Regex MangaListRx, ChapterIdRx, ChapterIndexRx;

	public Provider() : base("https://battwo.com")
	{
		MangaListRx = new Regex(@"\/series\/([0-9]+)\/(.+)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		ChapterIdRx = new Regex(@"\/chapter\/([0-9]+)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		ChapterIndexRx = new Regex(@".*Chapter ([0-9]+)",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}
	public const string Alias = "battwo";

	public override async Task<List<Manga>> GetMangaList(string query)
	{
		using (var mangaRes = await Client.GetAsync(
				$"search?word={query}",
				HttpCompletionOption.ResponseHeadersRead)
			)
		{
			mangaRes.EnsureSuccessStatusCode();

			var mangaHtml = await mangaRes.Content.ReadAsStringAsync();
			var mangaHtmlDoc = new HtmlDocument();
			mangaHtmlDoc.LoadHtml(mangaHtml);
			return mangaHtmlDoc
				.DocumentNode
				.SelectNodes("//a[contains(@class, 'item-title')]")
				.Select(node =>
				{
					var manga = new Manga();
					var _match = MangaListRx.Match(node.Attributes["href"].Value);
					manga.id = _match.Groups[1].Value;
					manga.slug = _match.Groups[2].Value;
					manga.title = node.InnerText.Replace("&nbsp;", " ");
					return manga;
				})
				.ToList();
		}
	}

	public override async Task<Manga> GetManga(int mangaIndex, List<Manga> mangaList)
	{
		var selectedManga = mangaList[mangaIndex - 1];
		using (var mangaRes = await Client.GetAsync(
				$"series/{selectedManga.id}/{selectedManga.slug}",
				HttpCompletionOption.ResponseHeadersRead
			)
		)
		{
			mangaRes.EnsureSuccessStatusCode();

			var mangaHtml = await mangaRes.Content.ReadAsStringAsync();
			var mangaHtmlDoc = new HtmlDocument();
			mangaHtmlDoc.LoadHtml(mangaHtml);

			selectedManga.desc = mangaHtmlDoc
				.DocumentNode
				.SelectSingleNode(@"//div[contains(@id, 'limit-height-body-summary')]")
				.InnerText;

			selectedManga.chapters = mangaHtmlDoc
				.DocumentNode
				.SelectNodes("//a[contains(@class, 'chapt')]")
				.Select(node =>
				{
					var chapter = new Chapter();
					chapter.title = "";
					chapter.id = ChapterIdRx.Match(node.Attributes["href"].Value).Groups[1].Value;
					chapter.chap = ChapterIndexRx.Match(node.InnerText).Groups[1].Value;
					return chapter;
				})
				.ToList();
			selectedManga.last_chapter = selectedManga.chapters[0].chap;

			return selectedManga;
		}
	}

	public override async Task<Chapter> GetChapter(int chapIndex, List<Chapter> chapterList, bool search = false)
	{
		var selectedChapter = chapterList[chapterList.Count - chapIndex];
		var options = new LaunchOptions { Headless = true };
		await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
		using (var browser = await Puppeteer.LaunchAsync(options))
		using (var page = await browser.NewPageAsync())
		{
			await page.SetUserAgentAsync(".NEET");
			await page.GoToAsync(
				$"{BaseUrl}/chapter/{selectedChapter.id}", 
				new NavigationOptions {
					Timeout = 0,
					WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
				}
			);

			selectedChapter.title = 
				(await page.EvaluateExpressionAsync<string>(@"document.querySelector('h3.nav-title').textContent;"))
				.Trim();

			if (search)
				return selectedChapter;

			selectedChapter.md_images = 
				(await page.EvaluateExpressionAsync<Page[]>(@"
					Array.from(document.querySelectorAll('img.page-img'))
						.map(img => {
							return { 
								src: img.src,
								width: img.width,
								height: img.height
							}
						})
				"))
				.ToList();

			return selectedChapter;
		}
	}

	public override async Task<List<Page>> GetPages(Chapter chap)
	{
		var tasks = new List<Task>();
		for (int i = 0; i < chap.md_images.Count; i++)
		{
			int index = i;
			tasks.Add(Task.Run(async () =>
			{
				try
				{
					var page = await GetPage(chap.md_images[index].src);
					chap.md_images[index].base64 = page.base64;
					chap.md_images[index].stream = page.stream;
					chap.md_images[index].bytes = page.bytes;
					chap.md_images[index].filetype = "webp";
				}
				catch (Exception)
				{
					chap.md_images[index].src = new String("");
				}
			}));
		}
		Task.WaitAll(tasks.ToArray());
		return await Task.FromResult(chap.md_images);
	}

	public override async Task<dynamic> GetPage(string url)
	{
		using (var pageRes = await Client.GetAsync(
				url,
				HttpCompletionOption.ResponseHeadersRead
			)
		)
		{
			pageRes.EnsureSuccessStatusCode();
			var pageBytes = await pageRes.Content.ReadAsByteArrayAsync();
			var pageStream = await pageRes.Content.ReadAsStreamAsync();
			var pageBase64 = "data:image/webp;base64," + Convert.ToBase64String(pageBytes);
			// All necessary information for rendering
			// Currently only using cdnlink and bytes
			return await Task.FromResult(new
			{
				cdnLink = url,
				base64 = pageBase64,
				stream = pageStream,
				bytes = pageBytes
			});
		}
	}

	public override String GetChapterUrl(Manga manga, Chapter chap)
		=> $"{BaseUrl}/chapter/{chap.id}";
}
