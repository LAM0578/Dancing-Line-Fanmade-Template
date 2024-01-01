using System.IO;

namespace DancingLineSample.Utility
{
	public static class FileUtility
	{
		public static byte[] ReadBytesFromFile(string path)
		{
			if (File.Exists(path))
			{
				return File.ReadAllBytes(path);
			}
			File.WriteAllBytes(path, new byte[] { });
			return new byte[] { };
		}
		
		public static byte[] TryReadBytesToFile(string path, byte[] bytes)
		{
			if (File.Exists(path))
			{
				return File.ReadAllBytes(path);
			}
			File.WriteAllBytes(path, bytes);
			return bytes;
		}
	}
}