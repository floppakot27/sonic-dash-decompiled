using System;
using System.Collections;
using UnityEngine;

public class ABTest : MonoBehaviour
{
	public enum Names
	{
		FirstTest
	}

	public enum Groups
	{
		A,
		B
	}

	[Flags]
	private enum State
	{
		FileEmptyLoad = 1,
		FileLoaded = 2,
		ServerFailed = 4,
		Ready = 8
	}

	public const string m_testNameProperty = "ABTest_";

	public const string m_testActivationProperty = "ABTest_Activation_";

	private const string m_urlTest = "http://www.hardlightstudio.com/sd/ab.lson";

	private static Groups[] m_tests = new Groups[Enum.GetNames(typeof(Names)).Length];

	private static bool[] m_testsActivation = new bool[Enum.GetNames(typeof(Names)).Length];

	private static State m_state = (State)0;

	public static bool TestGroup(Names name, Groups testGroup)
	{
		return m_tests[(int)name] == testGroup;
	}

	public static Groups[] GetTestsValues()
	{
		return m_tests;
	}

	public static bool[] GetTestsActivations()
	{
		return m_testsActivation;
	}

	private void Start()
	{
		EventDispatch.RegisterInterest("OnGameDataLoaded", this);
		EventDispatch.RegisterInterest("OnGameDataSaveRequest", this);
		StartCoroutine(LoadingTests());
	}

	private void Event_OnGameDataSaveRequest()
	{
		string[] names = Enum.GetNames(typeof(Names));
		for (int i = 0; i < names.Length; i++)
		{
			PropertyStore.Store("ABTest_" + names[i], (int)m_tests[i]);
			PropertyStore.Store("ABTest_Activation_" + names[i], Convert.ToInt32(m_testsActivation[i]));
		}
	}

	private void Event_OnGameDataLoaded(ActiveProperties activeProperties)
	{
		string[] names = Enum.GetNames(typeof(Names));
		if ((m_state & State.Ready) == State.Ready)
		{
			return;
		}
		if ((m_state & State.FileEmptyLoad) != State.FileEmptyLoad)
		{
			m_state |= State.FileEmptyLoad;
			return;
		}
		for (int i = 0; i < names.Length; i++)
		{
			if (PropertyStore.ActiveProperties().DoesPropertyExist("ABTest_" + names[i]))
			{
				m_tests[i] = (Groups)activeProperties.GetInt("ABTest_" + names[i]);
				m_testsActivation[i] = Convert.ToBoolean(activeProperties.GetInt("ABTest_Activation_" + names[i]));
			}
		}
		m_state |= State.FileLoaded;
		SetTestValues(null);
	}

	private IEnumerator LoadingTests()
	{
		WWW testsData = new WWW("http://www.hardlightstudio.com/sd/ab.lson");
		yield return testsData;
		if (testsData.error != null)
		{
			m_state |= State.ServerFailed;
			SetTestValues(null);
			yield break;
		}
		LSON.Root[] roots = LSONReader.Parse(testsData.text);
		if ((m_state & State.FileLoaded) == State.FileLoaded)
		{
			for (int i = 0; i < m_tests.Length; i++)
			{
				m_tests[i] = Groups.A;
				m_testsActivation[i] = false;
			}
		}
		SetTestValues(roots);
	}

	private Groups RandomGroup(Names name)
	{
		string crcSource = name.ToString() + SystemInfo.deviceUniqueIdentifier;
		int seed = (int)CRC32.Generate(crcSource, CRC32.Case.AsIs);
		System.Random random = new System.Random(seed);
		return (Groups)random.Next(0, 2);
	}

	private void SetTestValues(LSON.Root[] roots)
	{
		if ((m_state & State.FileLoaded) == State.FileLoaded && (m_state & State.ServerFailed) == State.ServerFailed)
		{
			m_state |= State.Ready;
		}
		else
		{
			if (roots == null)
			{
				return;
			}
			string[] names = Enum.GetNames(typeof(Names));
			string[] names2 = Enum.GetNames(typeof(Groups));
			for (int i = 0; i < roots.Length; i++)
			{
				bool flag = false;
				string stringValue = null;
				string text = null;
				bool? flag2 = null;
				if (roots[i].m_name != "test")
				{
					continue;
				}
				LSON.Property[] properties = roots[i].m_properties;
				foreach (LSON.Property property in properties)
				{
					if (property.m_name == null)
					{
						continue;
					}
					switch (property.m_name.ToLower())
					{
					case "name":
						if (LSONProperties.AsString(property, out stringValue))
						{
							stringValue = stringValue.Replace(" ", string.Empty).ToLower();
						}
						break;
					case "group":
					{
						if (LSONProperties.AsString(property, out var stringValue2))
						{
							stringValue2 = stringValue2.ToLower();
							if (stringValue2 == names2[0].ToLower() || stringValue2 == names2[1].ToLower())
							{
								text = stringValue2;
							}
						}
						break;
					}
					case "active":
					{
						if (LSONProperties.AsBool(property, out var boolValue))
						{
							flag2 = boolValue;
						}
						break;
					}
					}
				}
				if (stringValue == null)
				{
					continue;
				}
				for (int k = 0; k < names.Length; k++)
				{
					if (!(stringValue == names[k].ToLower()))
					{
						continue;
					}
					flag = true;
					if (text == null)
					{
						m_tests[k] = RandomGroup((Names)k);
						m_testsActivation[k] = true;
						continue;
					}
					if (text == names2[0].ToLower())
					{
						m_tests[k] = Groups.A;
					}
					else if (text == names2[1].ToLower())
					{
						m_tests[k] = Groups.B;
					}
					if (flag2.HasValue && !flag2.Value)
					{
					}
				}
				if (flag)
				{
				}
			}
			m_state |= State.Ready;
		}
	}
}
