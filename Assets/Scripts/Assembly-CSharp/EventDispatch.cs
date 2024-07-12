using System;
using System.Collections.Generic;
using System.Reflection;

public class EventDispatch
{
	public enum Priority
	{
		Highest,
		High,
		Normal,
		Low,
		Lowest
	}

	private class ObjectProperties
	{
		public object m_interestedObject;

		public MethodInfo m_eventMethod;

		public Priority m_priority = Priority.Normal;

		public ObjectProperties(object interestedObject, MethodInfo eventMethod, Priority priority)
		{
			m_interestedObject = interestedObject;
			m_eventMethod = eventMethod;
			m_priority = priority;
		}
	}

	private static Dictionary<uint, List<ObjectProperties>> s_eventObjects;

	static EventDispatch()
	{
		ClearDispatcher();
	}

	public static void ClearDispatcher()
	{
		s_eventObjects = new Dictionary<uint, List<ObjectProperties>>();
	}

	public static void GenerateEvent(string eventName)
	{
		GenerateEvent(eventName, null);
	}

	public static void GenerateEvent(string eventName, object parameter)
	{
		List<ObjectProperties> list = FindObjectList(eventName, createIfInvalid: false);
		if (list != null)
		{
			object[] parameters = null;
			if (parameter != null)
			{
				parameters = new object[1] { parameter };
			}
			SendEventToObjectList(list, parameters);
		}
	}

	public static void GenerateEvent(string eventName, object[] parameters)
	{
		List<ObjectProperties> list = FindObjectList(eventName, createIfInvalid: false);
		if (list != null)
		{
			SendEventToObjectList(list, parameters);
		}
	}

	public static void RegisterInterest(string eventName, object interestedObject)
	{
		RegisterInterest(eventName, interestedObject, Priority.Normal);
	}

	public static void RegisterInterest(string eventName, object interestedObject, Priority priority)
	{
		List<ObjectProperties> list = FindObjectList(eventName, createIfInvalid: true);
		if (!list.Exists((ObjectProperties thisElement) => interestedObject == thisElement.m_interestedObject))
		{
			MethodInfo methodInfo = GetMethodInfo(interestedObject, eventName);
			list.Add(new ObjectProperties(interestedObject, methodInfo, priority));
			list.Sort((ObjectProperties first, ObjectProperties second) => first.m_priority.CompareTo(second.m_priority));
		}
	}

	public static void UnregisterInterest(string eventName, object uninterestedObject)
	{
		FindObjectList(eventName, createIfInvalid: false)?.RemoveAll((ObjectProperties thisElement) => uninterestedObject == thisElement.m_interestedObject);
	}

	public static void UnregisterAllInterest(object uninterestedObject)
	{
		foreach (List<ObjectProperties> value in s_eventObjects.Values)
		{
			value.RemoveAll((ObjectProperties listenerPair) => uninterestedObject == listenerPair.m_interestedObject);
		}
	}

	private static List<ObjectProperties> FindObjectList(string eventName, bool createIfInvalid)
	{
		uint key = CRC32.Generate(eventName, CRC32.Case.AsIs);
		List<ObjectProperties> value = null;
		s_eventObjects.TryGetValue(key, out value);
		if (value == null && createIfInvalid)
		{
			value = new List<ObjectProperties>();
			s_eventObjects.Add(key, value);
		}
		return value;
	}

	private static void SendEventToObjectList(List<ObjectProperties> objectList, object[] parameters)
	{
		foreach (ObjectProperties @object in objectList)
		{
			if (@object.m_interestedObject != null && @object.m_eventMethod != null)
			{
				try
				{
					@object.m_eventMethod.Invoke(@object.m_interestedObject, parameters);
				}
				catch (TargetParameterCountException)
				{
					string text = @object.m_interestedObject.ToString();
					int num = ((parameters != null) ? parameters.Length : 0);
				}
			}
		}
	}

	private static MethodInfo GetMethodInfo(object interestedObject, string eventName)
	{
		string eventMethodName = GetEventMethodName(eventName);
		Type type = interestedObject.GetType();
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		return type.GetMethod(eventMethodName, bindingAttr);
	}

	private static string GetEventMethodName(string eventName)
	{
		return $"Event_{eventName}";
	}
}
