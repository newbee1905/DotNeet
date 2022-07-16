namespace DotNeet.Providers;

public class ProvidersList
{
	public static string[] GetProviderList
		=> new string[] {
			Providers.ComickFun.Provider.Alias,
			Providers.Battwo.Provider.Alias
		};

	public static dynamic GetProvider(string name) =>
		(name.ToLower()) switch
		{
			Providers.ComickFun.Provider.Alias => new Providers.ComickFun.Provider(),
			Providers.Battwo.Provider.Alias => new Providers.Battwo.Provider(),
			_ => null
		};
}
