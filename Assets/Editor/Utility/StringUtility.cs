public static class StringUtility
{
	public static string SubStringByIndex(this string str, int startIndex, int endIndex)
	{
		int length = str.Length;
		if (startIndex < 0) startIndex = length + startIndex;
		if (endIndex < 0) endIndex = length + endIndex;
		return str.Substring(startIndex, endIndex - startIndex);
	}
}