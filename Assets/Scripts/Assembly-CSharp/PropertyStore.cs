using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class PropertyStore
{
	public class Property
	{
		public string m_name;

		public string m_value;

		public uint m_nameCRC;

		public Property(string name, string val)
		{
			m_name = name;
			m_value = val;
			m_nameCRC = CRC32.Generate(name, CRC32.Case.Lower);
		}
	}

	private enum State
	{
		Idle,
		Saving,
		Loading
	}

	public delegate void LoadDelegateCallBack();

	private const string FileName = "save.txt";

	private static List<Property> s_properties;

	private static ActiveProperties s_activeProperties;

	private static State s_state;

	private static bool s_gameLoaded;

	private static int SaveVersion => -3;

	[method: MethodImpl(32)]
	private static event LoadDelegateCallBack s_loadDelegateEvents;

	static PropertyStore()
	{
		s_properties = new List<Property>();
		s_activeProperties = new ActiveProperties();
		s_state = State.Idle;
		s_gameLoaded = false;
		s_activeProperties.PropertyList = s_properties;
	}

	public static void Save()
	{
		s_state = State.Saving;
		EventDispatch.GenerateEvent("OnGameDataSaveRequest");
		SavePropertyData("save.txt");
		s_state = State.Idle;
	}

	public static void Load(bool loadDelegates)
	{
		Reset();
		s_state = State.Loading;
		LoadPropertyData("save.txt");
		s_activeProperties.PropertyList = s_properties;
		if (loadDelegates)
		{
			PropertyStore.s_loadDelegateEvents();
		}
		EventDispatch.GenerateEvent("OnGameDataLoaded", s_activeProperties);
		s_state = State.Idle;
		s_gameLoaded = true;
	}

	public static void Reset()
	{
		s_state = State.Loading;
		s_properties.Clear();
		EventDispatch.GenerateEvent("OnGameDataLoaded", s_activeProperties);
		s_state = State.Idle;
	}

	public static void Store<T>(string propertyName, T property)
	{
		if (propertyName != null)
		{
			string text = property.ToString();
			Property property2 = FindProperty(s_properties, propertyName);
			if (property2 == null)
			{
				s_properties.Add(new Property(propertyName, text));
			}
			else
			{
				property2.m_value = text;
			}
		}
	}

	public static ActiveProperties ActiveProperties()
	{
		return s_activeProperties;
	}

	public static Property FindProperty(List<Property> propertyList, string propertyName)
	{
		uint num = CRC32.Generate(propertyName, CRC32.Case.Lower);
		foreach (Property property in propertyList)
		{
			if (property.m_nameCRC == num)
			{
				return property;
			}
		}
		return null;
	}

	public static void AddLoadDelegateEvent(LoadDelegateCallBack callback)
	{
		PropertyStore.s_loadDelegateEvents = (LoadDelegateCallBack)Delegate.Combine(PropertyStore.s_loadDelegateEvents, callback);
	}

	public static void RemoveLoadDelegateEvent(LoadDelegateCallBack callback)
	{
		PropertyStore.s_loadDelegateEvents = (LoadDelegateCallBack)Delegate.Remove(PropertyStore.s_loadDelegateEvents, callback);
	}

	private static uint CalculateSaltAndHashedString(string toHash)
	{
		string crcSource = toHash + "Artifical Key";
		return CRC32.Generate(crcSource, CRC32.Case.AsIs);
	}

	private static void SavePropertyData(string fileName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(SaveVersion.ToString());
		stringBuilder.AppendLine(s_properties.Count.ToString());
		foreach (Property s_property in s_properties)
		{
			stringBuilder.AppendLine(s_property.m_name);
			stringBuilder.AppendLine(s_property.m_value);
		}
		stringBuilder.AppendLine(CalculateSaltAndHashedString(stringBuilder.ToString()).ToString());
		string filePath = GetFilePath(fileName);
		File.WriteAllText(filePath, stringBuilder.ToString());
	}

	private static void LoadPropertyData(string fileName)
	{
		string filePath = GetFilePath(fileName);
		if (!File.Exists(filePath))
		{
			return;
		}
		StringBuilder saveContents = new StringBuilder();
		TextReader textReader = File.OpenText(filePath);
		try
		{
			Func<string> func = delegate
			{
				string text = textReader.ReadLine();
				saveContents.AppendLine(text);
				return text;
			};
			if (textReader.Peek() != 45)
			{
				Reset();
				return;
			}
			int num = int.Parse(func());
			string value = func();
			s_properties.Capacity = Convert.ToInt32(value);
			for (int i = 0; i < s_properties.Capacity; i++)
			{
				string name = func();
				string val = func();
				s_properties.Add(new Property(name, val));
			}
			if (num == -2)
			{
				uint.Parse(textReader.ReadLine());
			}
			if (num <= -3)
			{
				uint num2 = uint.Parse(textReader.ReadLine());
				uint num3 = CalculateSaltAndHashedString(saveContents.ToString());
				if (num3 != num2)
				{
					Reset();
				}
			}
		}
		finally
		{
			if (textReader != null)
			{
				((IDisposable)textReader).Dispose();
			}
		}
	}

	private static string GetFilePath(string filename)
	{
		return $"{Application.persistentDataPath}/{filename}";
	}
}
