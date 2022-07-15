namespace DotNeet;

using DotNeet.Commands;

public class Program {

	public static RootCommand root;

	public static async Task Main(string[] args) {
		root = new();
		root.Add(new ProvidersListingCommand());
		root.Add(new SearchCommand());
		root.Add(new DownloadCommand());
		root.Add(new ReadCommand());
		// root.SetHandler(() => {
		//
		// });
		await root.InvokeAsync(args);
	}
}
