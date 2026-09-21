using System.IO;
using System.Reflection;

namespace Norsemen;

public static class EmbeddedResourceManager
{
	public static readonly Assembly assembly = Assembly.GetExecutingAssembly();

	public static string GetFile(string fileName, string directory = "Files")
	{
		string name = "Norsemen." + directory + "." + fileName;
		using Stream stream = assembly.GetManifestResourceStream(name);
		if (stream == null)
		{
			throw new FileNotFoundException("Embedded resource " + fileName + " not found.");
		}
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}
}
