namespace DotNeet;

/// <summary>
/// Basic functions that will be used througout
/// the program
/// </summary>
public class Utils {
	private static ConsoleColor _defaultForeground = Console.ForegroundColor;
	private static ConsoleColor _defaultBackground = Console.BackgroundColor;

	/// <summary>
	/// Write Text with color and reset it back the the default one
	/// </summary>
	public static void WriteColor(string output, ConsoleColor foreground = (ConsoleColor)(-1), ConsoleColor background = (ConsoleColor)(-1)) {
		if (foreground == (ConsoleColor)(-1))
			foreground = _defaultForeground;
		if (background == (ConsoleColor)(-1))
			background = _defaultBackground;

		Console.BackgroundColor = background;
		Console.ForegroundColor = foreground;
		Console.Write(output);
		Console.BackgroundColor = _defaultBackground;
		Console.ForegroundColor = _defaultForeground;
	}

	/// <summary>
	/// Write Text with new line 
	/// with color and reset it back the the default one
	/// </summary>
	public static void WriteLineColor(string output, ConsoleColor foreground = (ConsoleColor)(-1), ConsoleColor background = (ConsoleColor)(-1))
		=> WriteColor(output + '\n', foreground, background);

	/// <summary>
	/// Write Error message with dark red colour
	/// </summary>
	public static void WriteError(string output)
		=> WriteLineColor(output, ConsoleColor.DarkRed);
}
