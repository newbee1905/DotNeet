namespace DotNeet.Commands;

/// <summary>
/// List supported providers
/// </summary>
class ProvidersListingCommand : Command
{
	/// <summary>
	/// Setting up providers command
	/// List all provider in green with
	/// tab
	/// </summary>
	public ProvidersListingCommand() : base("providers", "List all supoprted providers")
	{
		this.SetHandler(() =>
		{
			var providerList = ProvidersList.GetProviderList;
			for (int i = 0; i < providerList.Length; i++)
				Utils.WriteLineColor($"\t- {providerList[i]}", ConsoleColor.Green);
		});
	}
}
