using System;
using System.Collections.Generic;

public class ActiveProperties
{
	public List<PropertyStore.Property> PropertyList { get; set; }

	public int GetPropertyCount()
	{
		return PropertyList.Count;
	}

	public string GetString(string propertyName)
	{
		return GetProperty(propertyName)?.m_value;
	}

	public int GetInt(string propertyName)
	{
		PropertyStore.Property property = GetProperty(propertyName);
		if (property == null)
		{
			return 0;
		}
		return Convert.ToInt32(property.m_value);
	}

	public float GetFloat(string propertyName)
	{
		PropertyStore.Property property = GetProperty(propertyName);
		if (property == null)
		{
			return 0f;
		}
		return Convert.ToSingle(property.m_value);
	}

	public double GetDouble(string propertyName)
	{
		PropertyStore.Property property = GetProperty(propertyName);
		if (property == null)
		{
			return 0.0;
		}
		return Convert.ToDouble(property.m_value);
	}

	public long GetLong(string propertyName)
	{
		PropertyStore.Property property = GetProperty(propertyName);
		if (property == null)
		{
			return 0L;
		}
		return Convert.ToInt64(property.m_value);
	}

	public bool GetBool(string propertyName)
	{
		PropertyStore.Property property = GetProperty(propertyName);
		if (property == null)
		{
			return false;
		}
		return Convert.ToBoolean(property.m_value);
	}

	public bool DoesPropertyExist(string propertyName)
	{
		PropertyStore.Property property = GetProperty(propertyName);
		if (property == null)
		{
			return false;
		}
		return true;
	}

	private PropertyStore.Property GetProperty(string propertyName)
	{
		if (propertyName == null)
		{
			return null;
		}
		if (PropertyList == null)
		{
			return null;
		}
		return PropertyStore.FindProperty(PropertyList, propertyName);
	}
}
