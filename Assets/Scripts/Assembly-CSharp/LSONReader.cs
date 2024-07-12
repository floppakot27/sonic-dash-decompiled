public class LSONReader
{
	private enum Property
	{
		Name,
		Value
	}

	private const int InvalidRootIndex = -1;

	public static LSON.Root[] Parse(string fileContent)
	{
		if (fileContent == null || fileContent.Length == 0)
		{
			return null;
		}
		string[] array = ParseString(fileContent);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		return ParseStringArray(array);
	}

	private static LSON.Root[] ParseStringArray(string[] parsedSource)
	{
		if (!ValidateSource(parsedSource))
		{
			return null;
		}
		int rootNodeCount = GetRootNodeCount(parsedSource);
		if (rootNodeCount == 0)
		{
			return null;
		}
		LSON.Root[] array = new LSON.Root[rootNodeCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new LSON.Root();
		}
		PopulateRootNodes(parsedSource, array);
		return array;
	}

	private static string[] ParseString(string stringContent)
	{
		string[] array = stringContent.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}

	private static bool ValidateSource(string[] parsedSource)
	{
		int num = 0;
		foreach (string text in parsedSource)
		{
			if (text == "{")
			{
				num++;
			}
			if (text == "}")
			{
				num--;
			}
		}
		return num == 0;
	}

	private static int GetRootNodeCount(string[] parsedSource)
	{
		bool flag = false;
		int num = 0;
		foreach (string text in parsedSource)
		{
			if (text == "{")
			{
				flag = true;
			}
			if (text == "}")
			{
				flag = false;
			}
			if (ValidRootNode(text) && !flag)
			{
				num++;
			}
		}
		return num;
	}

	private static int GetPropertyNodes(string[] parsedSource, int rootNodeStartIndex, LSON.Property[] propertyList)
	{
		int num = 0;
		for (int i = rootNodeStartIndex; i < parsedSource.Length; i++)
		{
			if (i == rootNodeStartIndex)
			{
				continue;
			}
			string text = parsedSource[i];
			if (!(text == "{"))
			{
				if (text == "}")
				{
					break;
				}
				string[] property = GetProperty(text);
				if (property != null)
				{
					num++;
				}
				if (propertyList != null && property != null)
				{
					propertyList[num - 1].m_name = property[0];
					propertyList[num - 1].m_value = property[1];
				}
			}
		}
		return num;
	}

	private static void PopulateRootNodes(string[] parsedSource, LSON.Root[] rootNodes)
	{
		for (int i = 0; i < rootNodes.Length; i++)
		{
			int rootNodeStartIndex = GetRootNodeStartIndex(parsedSource, i);
			if (rootNodeStartIndex == -1)
			{
				continue;
			}
			string[] property = GetProperty(parsedSource[rootNodeStartIndex]);
			if (property != null)
			{
				rootNodes[i].m_name = property[1];
			}
			if (rootNodes[i].m_name == null)
			{
				continue;
			}
			int propertyNodes = GetPropertyNodes(parsedSource, rootNodeStartIndex, null);
			if (propertyNodes != 0)
			{
				rootNodes[i].m_properties = new LSON.Property[propertyNodes];
				for (int j = 0; j < propertyNodes; j++)
				{
					rootNodes[i].m_properties[j] = new LSON.Property();
				}
				GetPropertyNodes(parsedSource, rootNodeStartIndex, rootNodes[i].m_properties);
			}
		}
	}

	private static int GetRootNodeStartIndex(string[] parsedSource, int indexToFind)
	{
		int num = 0;
		bool flag = false;
		for (int i = 0; i < parsedSource.Length; i++)
		{
			if (parsedSource[i] == "{")
			{
				flag = true;
			}
			if (parsedSource[i] == "}")
			{
				flag = false;
			}
			if (ValidRootNode(parsedSource[i]) && !flag)
			{
				if (num == indexToFind)
				{
					return i;
				}
				num++;
			}
		}
		return -1;
	}

	private static string[] GetProperty(string line)
	{
		string[] array = line.Split(':');
		if (array.Length != 2)
		{
			return null;
		}
		bool flag = true;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
			if (array[i].Length == 0)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return null;
		}
		return array;
	}

	private static bool ValidRootNode(string line)
	{
		if (!line.Contains("root"))
		{
			return false;
		}
		string[] property = GetProperty(line);
		if (property == null)
		{
			return false;
		}
		if (property[0] != "root")
		{
			return false;
		}
		return true;
	}
}
