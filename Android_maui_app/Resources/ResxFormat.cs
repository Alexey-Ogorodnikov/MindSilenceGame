using System.Globalization;

namespace MindSilence.Resources;

/// <summary>
/// Maps Android <c>%1$d</c> / <c>%2$d</c> patterns from <see cref="AppResources"/> to .NET format.
/// </summary>
internal static class ResxFormat
{
	private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

	public static string Format(string androidPattern, params object[] args)
	{
		var format = androidPattern
			.Replace("%1$d", "{0}", StringComparison.Ordinal)
			.Replace("%2$d", "{1}", StringComparison.Ordinal);
		return string.Format(English, format, args);
	}
}
