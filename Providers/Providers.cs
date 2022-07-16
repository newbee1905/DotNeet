namespace DotNeet.Providers;

public class ProvidersList
{
	public static string[] GetProviderList
		=> new string[] {
			Providers.ComickFun.Provider.Alias
		};

	public static dynamic GetProvider(string name) =>
		(name.ToLower()) switch
		{
			Providers.ComickFun.Provider.Alias => new Providers.ComickFun.Provider(),
			_ => null
		};
}
