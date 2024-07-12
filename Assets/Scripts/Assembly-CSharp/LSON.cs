using UnityEngine;

public class LSON : MonoBehaviour
{
	public class Property
	{
		public string m_name;

		public string m_value;
	}

	public class Root
	{
		public string m_name;

		public Property[] m_properties;
	}
}
