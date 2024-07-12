public class LSONProperties
{
	public static LSON.Property[] GetProperties(LSON.Root[] root, string rootName)
	{
		if (root == null || root.Length == 0)
		{
			return null;
		}
		if (string.IsNullOrEmpty(rootName))
		{
			return null;
		}
		string text = rootName.ToLowerInvariant();
		for (int i = 0; i < root.Length; i++)
		{
			string text2 = root[i].m_name.ToLowerInvariant();
			if (text2 == text)
			{
				return root[i].m_properties;
			}
		}
		return null;
	}

	public static LSON.Property GetProperty(LSON.Property[] properties, string propertyName)
	{
		if (properties == null || properties.Length == 0)
		{
			return null;
		}
		if (string.IsNullOrEmpty(propertyName))
		{
			return null;
		}
		string text = propertyName.ToLowerInvariant();
		for (int i = 0; i < properties.Length; i++)
		{
			string text2 = properties[i].m_name.ToLowerInvariant();
			if (text2 == text)
			{
				return properties[i];
			}
		}
		return null;
	}

	public static bool AsCRC(LSON.Property property, out uint crcValue, CRC32.Case crcCase)
	{
		crcValue = 0u;
		if (property == null || property.m_value == null)
		{
			return false;
		}
		crcValue = CRC32.Generate(property.m_value, crcCase);
		return true;
	}

	public static bool AsString(LSON.Property property, out string stringValue)
	{
		stringValue = null;
		if (property == null || property.m_value == null)
		{
			return false;
		}
		stringValue = property.m_value;
		return true;
	}

	public static bool AsBool(LSON.Property property, out bool boolValue)
	{
		boolValue = false;
		if (property == null || property.m_value == null)
		{
			return false;
		}
		if (property.m_value.ToLower() == "true" || property.m_value.ToLower() == "yes" || property.m_value == "1")
		{
			boolValue = true;
		}
		else
		{
			boolValue = false;
		}
		return true;
	}

	public static bool AsFloat(LSON.Property property, out float floatValue)
	{
		floatValue = 0f;
		if (property == null || property.m_value == null)
		{
			return false;
		}
		return float.TryParse(property.m_value, out floatValue);
	}

	public static bool AsInt(LSON.Property property, out int intValue)
	{
		intValue = 0;
		if (property == null || property.m_value == null)
		{
			return false;
		}
		return int.TryParse(property.m_value, out intValue);
	}
}
