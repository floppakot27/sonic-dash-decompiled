using System.Text;

public class LSONWriter
{
	public static string Write(LSON.Root[] lsonContent)
	{
		StringBuilder lsonText = new StringBuilder();
		lsonText = WriteRoot(lsonContent, lsonText);
		return lsonText.ToString();
	}

	private static StringBuilder WriteRoot(LSON.Root[] lsonRoots, StringBuilder lsonText)
	{
		foreach (LSON.Root root in lsonRoots)
		{
			if (!string.IsNullOrEmpty(root.m_name) && root.m_properties != null && root.m_properties.Length != 0)
			{
				lsonText.Append($"root: {root.m_name}\n");
				lsonText.Append("{\n");
				lsonText = WriteContent(root.m_properties, lsonText);
				lsonText.Append("}\n\n");
			}
		}
		return lsonText;
	}

	private static StringBuilder WriteContent(LSON.Property[] lsonProperties, StringBuilder lsonText)
	{
		foreach (LSON.Property property in lsonProperties)
		{
			if (property != null)
			{
				lsonText.Append($"\t{property.m_name}: {property.m_value}\n");
			}
		}
		return lsonText;
	}
}
