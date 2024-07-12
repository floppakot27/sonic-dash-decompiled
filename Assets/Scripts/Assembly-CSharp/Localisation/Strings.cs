using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Localisation;

public class Strings
{
	public enum Type
	{
		Primary,
		Platform
	}

	private const string LocalisationNode = "localisation";

	private const string StringNode = "string";

	private const string TextNode = "text";

	private const string EntryNode = "entry";

	private const string IdNode = "id";

	private const string LanguageNode = "language";

	private const string StringCountAttribute = "count";

	private Dictionary<uint, string> m_strings;

	public string GetString(string id)
	{
		if (m_strings == null)
		{
			return "Localisation file not loaded";
		}
		uint num = CRC32.Generate(GetStringID(id), CRC32.Case.Upper);
		if (m_strings.ContainsKey(num))
		{
			return m_strings[num];
		}
		return $"ID: {id} (CRC {num}) UNKNOWN";
	}

	public bool GetStringEntries(TextAsset textAsset, string languageToLoad, ref string[] identifiers, ref string[] strings)
	{
		int stringCount = 0;
		XmlDocument xmlDocument = LoadXMLDocument(textAsset, Type.Primary, out stringCount);
		if (xmlDocument == null)
		{
			return false;
		}
		identifiers = new string[stringCount];
		strings = new string[stringCount];
		string languageToLoad2 = languageToLoad.ToLower();
		PopulateStringContainer(identifiers, strings, xmlDocument, languageToLoad2, textAsset.name);
		return true;
	}

	public bool LoadXMLStringsFile(TextAsset textAsset, string languageToLoad, Type loadType)
	{
		int stringCount = 0;
		XmlDocument xmlDocument = LoadXMLDocument(textAsset, loadType, out stringCount);
		if (xmlDocument == null)
		{
			return false;
		}
		if (loadType == Type.Primary || m_strings == null)
		{
			m_strings = new Dictionary<uint, string>(stringCount);
		}
		string languageToLoad2 = languageToLoad.ToLower();
		PopulateStringContainer(m_strings, xmlDocument, languageToLoad2, textAsset.name, loadType);
		return true;
	}

	private XmlDocument LoadXMLDocument(TextAsset xmlText, Type fileType, out int stringCount)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		xmlDocument.LoadXml(xmlText.text);
		stringCount = GetStringCount(xmlDocument, xmlText.name, fileType);
		if (stringCount == 0)
		{
			return null;
		}
		return xmlDocument;
	}

	private int GetStringCount(XmlDocument xmlDocument, string fileToLoad, Type loadType)
	{
		XmlNode xmlNode = xmlDocument.SelectSingleNode("//localisation");
		XmlAttribute xmlAttribute = xmlNode.Attributes["count"];
		int num = int.Parse(xmlAttribute.Value);
		if (loadType != 0 || num == 0)
		{
		}
		return num;
	}

	private void PopulateStringContainer(Dictionary<uint, string> m_strings, XmlDocument xmlDocument, string languageToLoad, string fileToLoad, Type loadType)
	{
		XmlNode rootNode = GetRootNode(xmlDocument, fileToLoad);
		if (rootNode == null)
		{
			return;
		}
		foreach (XmlNode childNode in rootNode.ChildNodes)
		{
			if (!(childNode.Name != "string"))
			{
				string identifierName = GetIdentifierName(childNode, fileToLoad);
				string stringEntry = GetStringEntry(childNode, languageToLoad, fileToLoad);
				if (stringEntry != null && stringEntry.Length > 0)
				{
					uint key = CRC32.Generate(GetStringID(identifierName), CRC32.Case.Upper);
					m_strings[key] = stringEntry;
				}
			}
		}
	}

	private void PopulateStringContainer(string[] identifiers, string[] strings, XmlDocument xmlDocument, string languageToLoad, string fileToLoad)
	{
		XmlNode rootNode = GetRootNode(xmlDocument, fileToLoad);
		if (rootNode != null)
		{
			for (int i = 0; i < rootNode.ChildNodes.Count; i++)
			{
				XmlNode xmlNode = rootNode.ChildNodes[i];
				string name = xmlNode.Name;
				string stringEntry = GetStringEntry(xmlNode, languageToLoad, fileToLoad);
				identifiers[i] = name;
				strings[i] = stringEntry;
			}
		}
	}

	private string GetIdentifierName(XmlNode stringNode, string fileToLoad)
	{
		XmlAttribute xmlAttribute = stringNode.Attributes["id"];
		return xmlAttribute.Value;
	}

	private string GetStringEntry(XmlNode stringNode, string languageToLoad, string fileToLoad)
	{
		XmlNode textContainer = stringNode.SelectSingleNode("text");
		XmlNode languageEntry = GetLanguageEntry(textContainer, languageToLoad, fileToLoad);
		return languageEntry.InnerText;
	}

	private XmlNode GetLanguageEntry(XmlNode textContainer, string languageToLoad, string fileToLoad)
	{
		XmlNodeList xmlNodeList = textContainer.SelectNodes("entry");
		for (int i = 0; i < xmlNodeList.Count; i++)
		{
			XmlAttribute xmlAttribute = xmlNodeList[i].Attributes["language"];
			if (xmlAttribute != null)
			{
				string value = xmlAttribute.Value;
				if (value == languageToLoad)
				{
					return xmlNodeList[i];
				}
			}
		}
		return null;
	}

	private XmlNode GetRootNode(XmlDocument xmlDocument, string fileToLoad)
	{
		return xmlDocument.SelectSingleNode("//localisation");
	}

	private string GetStringID(string currentID)
	{
		return currentID.TrimEnd('\r', '\n', ' ').TrimStart(' ');
	}
}
