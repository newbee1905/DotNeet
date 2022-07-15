namespace DotNeet.Commands;

/// <summary>
/// This command is for downloading the manga's images
/// The user will select the manga from their search param
/// Then they will select the chapter that they want to
/// get the real url
/// </summary>
class SearchCommand : Command {
	private Argument<string> nameArgument =
		new("manga-name", "Manga's name or part of it that you want to search.");
	private Option<string> providerOption =
		new(name: "--provider", description: "Provider you want to search from", getDefaultValue: () => Providers.ComickFun.Provider.Alias);

	/// <summary>
	/// Setting up search command
	/// All selection list in this command will be printed as follow
	/// From color 9 - 14
	/// = Blue, Green, Cyan, Red, Magenta, Yellow
	/// </summary>
	public SearchCommand() : base("search", "Search Manga to read.") {
		this.AddArgument(nameArgument);
		this.AddOption(providerOption);

		this.SetHandler(async (mangaName, providerName) => {
			try {
				using (var provider = Providers.ProvidersList.GetProvider(providerName)) {
					if (provider is null)
						throw new Exception("Please select existed provider!");
					var mangaList = await provider.GetMangaList(mangaName);
					for (int i = 0; i < mangaList.Count; ++i)
						Utils.WriteLineColor($"[{i + 1}] {mangaList[i].title}", (ConsoleColor)(i % 5 + 9));
					Utils.WriteColor("> Select Manga Number: ", ConsoleColor.DarkBlue);
					switch (int.TryParse(Console.ReadLine(), out int mangaIndex)) {
					case true:
						if (mangaIndex < 1 || mangaIndex > mangaList.Count)
							goto default;
						var selectedManga = await provider.GetManga(mangaIndex, mangaList);
						Utils.WriteLineColor(selectedManga.desc, ConsoleColor.DarkGreen);

						for (int i = 0; i < selectedManga.total_chapter; ++i) {
							var chapter = selectedManga.chapters[selectedManga.total_chapter - i - 1];
							Utils.WriteLineColor($"[{i + 1}] Chapter {chapter.chap}: {chapter.title}", (ConsoleColor)(i % 5 + 9));
						}
						Utils.WriteColor(
							@$"The latest chapter is {selectedManga.last_chapter}
There are totally {selectedManga.total_chapter} chapters
> Select Chapter that you want to read: ",
							ConsoleColor.DarkBlue
						);

						switch (int.TryParse(Console.ReadLine(), out int chapIndex)) {
						case true:
						if (chapIndex < 1 || chapIndex > selectedManga.chapters.Count)
							goto default;
							var chapter = await provider.GetChapter(chapIndex, selectedManga.chapters);
							Utils.WriteLineColor(provider.GetChapterUrl(selectedManga, chapter), ConsoleColor.Green);
							break;
						default:
							Utils.WriteError("Please insert number with value in range of listed result");
							break;
						}
						break;
					default:
						Utils.WriteError("Please insert number with value in range of listed result");
						break;
					}
				}
			} catch (TimeoutException) {
				Utils.WriteError("Please recheck your internet connection!");
			} catch (Exception e) {
				Utils.WriteError(e.Message);
			}
		}, nameArgument, providerOption);
	}
}

