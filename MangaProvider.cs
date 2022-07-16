namespace DotNeet;

using System;
using System.IO;

public abstract class BasePage
{
	public BasePage() { }
	public int width { get; set; }
	public int height { get; set; }
	public string base64 { get; set; }
	public byte[] bytes { get; set; }
	public Stream stream { get; set; }
	public string src { get; set; }
}

public abstract class BaseChapter<TPage>
{
	public BaseChapter() { }
}

public abstract class BaseManga<TChapter>
{
	public BaseManga()
		=> chapters = new();
	public List<TChapter> chapters { get; set; }
}

public abstract class MangaProvider<TManga, TChapter, TPage> : IDisposable
where TManga : BaseManga<TChapter>, new()
where TChapter : BaseChapter<TPage>, new()
where TPage : BasePage, new()
{
	private bool _disposed = false;

	public readonly string BaseUrl;
	public static readonly HttpClient Client = new();
	public static readonly HttpClient PageClient = new();
	public readonly Renderer<TPage> Renderer = new();
	public string Lang = "en";

	/// <summary>
	/// Setup provider's necessary features
	/// HttpClients and CDN links
	/// </summary>
	public MangaProvider(string url)
	{
		BaseUrl = url;
		Client.BaseAddress = new Uri(url);
		Client.Timeout = new TimeSpan(0, 0, 5);
		Client.DefaultRequestHeaders.Accept.Clear();
		Client.DefaultRequestHeaders.Add("User-Agent", ".NEET");

		PageClient.Timeout = new TimeSpan(0, 0, 5);
		PageClient.DefaultRequestHeaders.Accept.Clear();
		PageClient.DefaultRequestHeaders.Add("User-Agent", ".NEET");
	}

	~MangaProvider() => Dispose(false);

	public void Dispose() => Dispose(true);

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		Client.Dispose();
		Renderer.Dispose();

		_disposed = true;
	}

	public virtual async Task<List<TManga>> GetMangaList(string query)
		=> await Task.FromResult(new List<TManga>());

	/// <summary>
	/// Method <c>GetManga</c> takes in input of the user and return the manga
	/// accordingly from the list
	/// </summary>
	/// <param><c>id</c> is the position of the manga in the list plus one</param>
	/// <param><c>mangaList</c> manga's list get above</param>
	public virtual async Task<TManga> GetManga(int mangaIndex, List<TManga> mangaList)
		=> await Task.FromResult(new TManga());

	/// <summary>
	/// Method <c>GetChapter</c> takes in input of the user and return a chapter
	/// accordingly from the list
	/// </summary>
	/// <param><c>id</c> is the position of the manga in the list plus one</param>
	/// <param><c>chapterList</c> chapter's list get above</param>
	public virtual async Task<TChapter> GetChapter(int chapIndex, List<TChapter> chapterList)
		=> await Task.FromResult(new TChapter());

	/// <summary>
	/// Method <c>GetChapterUrl</c> return the chapter's url
	/// from the manga and chapter object
	/// </summary>
	/// <param><c>manga</c> the manga</param>
	/// <param><c>chapter</c> the chapter</param>
	public virtual String GetChapterUrl(TManga manga, TChapter chap)
		=> "";

	/// <summary>
	/// Method <c>GetManga</c> takes in input of the user and return the manga
	/// accordingly from the list
	/// </summary>
	/// <param><c>id</c> is the position of the manga in the list plus one</param>
	/// <param><c>mangaList</c> manga's list get above</param>
	public virtual async Task<List<TPage>> GetPages(TChapter chap)
		=> await Task.FromResult(new List<TPage>());

	/// <summary>
	/// Method <c>GetManga</c> takes in input of the user and return the manga
	/// accordingly from the list
	/// </summary>
	/// <param><c>id</c> is the position of the manga in the list plus one</param>
	/// <param><c>mangaList</c> manga's list get above</param>
	public virtual async Task<dynamic> GetPage(string name, int cdnId)
		=> await Task.FromResult("");
}
