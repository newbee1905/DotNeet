namespace DotNeet.Providers.ComickFun;

public class Page : BasePage {
	public int h { get; set; }
	public int w { get; set; }
	public string b2key { get; set; }
}

public class Chapter : BaseChapter<Page> {
	public string title { get; set; }
	public string chap { get; set; }
	public string slug { get; set; }
	public int id { get; set; }
	public string hid { get; set; }
	public string rating { get; set; }
	public byte? demographic { get; set; }
	public List<Page> md_images { get; set; }

}

public class Manga : BaseManga<Chapter> {
	public string title { get; set; }
	public string slug { get; set; }
	public int id { get; set; }
	public string rating { get; set; }
	public string desc { get; set; }
	public string parsed { get; set; }
	public float last_chapter { get; set; }
	public int total_chapter { get; set; }
	public Manga comic { get; set; }
}

public class Provider : MangaProvider<Manga, Chapter, Page> {
	public readonly string PageCdn, SiteUrl;

	public Provider() : base("https://api.comick.fun") {
		PageCdn = "https://meo{0}.comick.pictures/{1}";
		SiteUrl = "https://preview.comick.fun/comic";
	}
	public const string Alias = "comickfun";

	public override async Task<List<Manga>> GetMangaList(string query) {
		using (var mangaListTask = Client.GetStreamAsync($"search?q={query}"))
			return await JsonSerializer.DeserializeAsync<List<Manga>>(await mangaListTask);
	}

	public override async Task<Manga> GetManga(int mangaIndex, List<Manga> mangaList) {
		var selectedManaga = mangaList[mangaIndex - 1];
		using (var mangaRes = await Client.GetAsync(
				$"comic/{selectedManaga.slug}",
				HttpCompletionOption.ResponseHeadersRead
			)
		) {
			mangaRes.EnsureSuccessStatusCode();

			var mangaStream = await mangaRes.Content.ReadAsStreamAsync();
			var manga = (await JsonSerializer.DeserializeAsync<Manga>(mangaStream)).comic;

			using (var chapterRes = await Client.GetAsync(
				$"comic/{selectedManaga.id}/chapter?lang={Lang}",
				HttpCompletionOption.ResponseHeadersRead
			)) {
				chapterRes.EnsureSuccessStatusCode();

				var chapterStream = await chapterRes.Content.ReadAsStreamAsync();
				var chapters = (await JsonSerializer.DeserializeAsync<dynamic>(chapterStream));

				manga.chapters = JsonSerializer.Deserialize<List<Chapter>>(chapters.GetProperty("chapters"));
				manga.total_chapter = JsonSerializer.Deserialize<int>(chapters.GetProperty("total"));

				return manga;
			}
		}
	}

	public override async Task<Chapter> GetChapter(int chapIndex, List<Chapter> chapterList) {
		var selectedChapter = chapterList[chapterList.Count - chapIndex];
		using (var chapterRes = await Client.GetAsync(
				$"chapter/{selectedChapter.hid}",
				HttpCompletionOption.ResponseHeadersRead
			)
		) {
			chapterRes.EnsureSuccessStatusCode();

			var chapterStream = await chapterRes.Content.ReadAsStreamAsync();
			var detailedChapter = await JsonSerializer.DeserializeAsync<dynamic>(chapterStream);
			selectedChapter.md_images = JsonSerializer.Deserialize<Chapter>(detailedChapter.GetProperty("chapter")).md_images;
			return selectedChapter;
		}
	}

	public override async Task<List<Page>> GetPages(Chapter chap) {
		var tasks = new List<Task>();
		for (int i = 0; i < chap.md_images.Count; i++) {
			int index = i;
			int cdnId = 1;
			tasks.Add(Task.Run(async () => {
				while (cdnId <= 4) {
					try {
						var page = await GetPage(chap.md_images[index].b2key, cdnId++);
						chap.md_images[index].src = page.cdnLink;
						chap.md_images[index].base64 = page.base64;
						chap.md_images[index].stream = page.stream;
						chap.md_images[index].bytes = page.bytes;
						// await File.WriteAllBytesAsync($"./page-{index}.jpg", page.bytes);
						// Utils.WriteLineColor($"sucess ({chap.md_images[index].b2key}): {cdnId - 1}", ConsoleColor.Green);
						// Console.WriteLine(chap.md_images[index].src);
						break;
					} catch (Exception) {
						// Utils.WriteLineColor($"error ({chap.md_images[index].b2key}): {cdnId - 1}", ConsoleColor.Red);
						chap.md_images[index].src = new String("");
					}
				}
			}));
		}
		Task.WaitAll(tasks.ToArray());
		// foreach (var page in chap.md_images)
		// 	Console.WriteLine(page.src);
		return await Task.FromResult(chap.md_images);
	}

	public override async Task<dynamic> GetPage(string name, int cdnId) {
		if (cdnId == 5)
			return await Task.FromResult("");

		var cdnLink = String.Format(PageCdn, cdnId == 1 ? "" : cdnId, name);
		// Utils.WriteLineColor($"test {cdnLink}", ConsoleColor.Yellow);

		using (var pageRes = await Client.GetAsync(
				cdnLink,
				HttpCompletionOption.ResponseHeadersRead
			)
		) {
			pageRes.EnsureSuccessStatusCode();
			var pageBytes = await pageRes.Content.ReadAsByteArrayAsync();
			var pageStream = await pageRes.Content.ReadAsStreamAsync();
			var pageBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(pageBytes);
			// All necessary information for rendering
			// Currently only using cdnlink and bytes
			return await Task.FromResult(new {
				cdnLink = cdnLink,
				base64 = pageBase64,
				stream = pageStream,
				bytes = pageBytes
			});
		}
	}

	public override String GetChapterUrl(Manga manga, Chapter chap)
		=> $"{SiteUrl}/{manga.slug}/{chap.hid}";
}
