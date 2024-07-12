using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudStorage : MonoBehaviour
{
	private enum State
	{
		Idle,
		Saving,
		Loading,
		CloudDidChange,
		GameDidChange
	}

	private const string FeatureStateRoot = "state";

	private const string ICloudStateProperty = "icloud";

	private const int SaveVersion = 1;

	private const string PropertyKeyDesc = "SonicDashProperties";

	private const string PropertyTimeStampDesc = "TimeStamp";

	private const string PropertyDeviceDesc = "Device";

	private static CloudStorage s_instance;

	private static bool s_active = true;

	private bool m_firstTime = true;

	private static State s_state;

	private static Hashtable s_properties;

	public static CloudStorage Instance => s_instance;

	public static bool CloudDidChange => s_state == State.CloudDidChange;

	public static string DeviceDesc => SystemInfo.deviceName;

	private void Awake()
	{
		s_instance = this;
		s_properties = new Hashtable();
		s_state = State.Idle;
		EventDispatch.RegisterInterest("OnAllAssetsLoaded", this);
		EventDispatch.RegisterInterest("FeatureStateReady", this);
		Log("Constructor");
	}

	private void OnEnable()
	{
		PropertyStore.AddLoadDelegateEvent(CloudLoad);
		Log("Enabled");
	}

	private void OnDisable()
	{
		PropertyStore.RemoveLoadDelegateEvent(CloudLoad);
		Log("Disabled");
	}

	private void Event_OnAllAssetsLoaded()
	{
		Init();
	}

	private bool Init()
	{
		if (m_firstTime)
		{
			m_firstTime = false;
			UpdateServerActiveFlag();
			if (!s_active)
			{
				return false;
			}
			if (s_state != State.CloudDidChange)
			{
				string cloudProperty = GetCloudProperty("TimeStamp", fromLocal: false);
				if (cloudProperty == null)
				{
					return false;
				}
				string @string = PropertyStore.ActiveProperties().GetString("TimeStamp");
				if (cloudProperty != @string)
				{
					s_state = State.CloudDidChange;
				}
			}
			if (CloudDidChange)
			{
				if (PlayerStats.GetStat(PlayerStats.StatNames.NumberOfSessions_Total) == 1)
				{
					Load();
					return true;
				}
				return Sync();
			}
		}
		return false;
	}

	public static bool Sync()
	{
		if (s_active)
		{
			bool flag = SyncCloudData();
			if (flag && CloudDidChange)
			{
				Dialog_CloudConflict.Display();
			}
			else
			{
				Save(overrideCloud: false);
			}
			return flag;
		}
		return false;
	}

	public static void Save(bool overrideCloud)
	{
		if (s_active)
		{
			s_state = State.Saving;
			UpdateCloudData();
			if (overrideCloud || s_state == State.GameDidChange)
			{
				SaveCloudData();
				Log("CloudStorageSaved");
				SetPropertyStore("TimeStamp");
				PropertyStore.Save();
			}
			s_state = State.Idle;
		}
	}

	public static void Load()
	{
		if (s_active && CloudDidChange)
		{
			PropertyStore.Load(loadDelegates: true);
			PropertyStore.Save();
		}
	}

	private static void CloudLoad()
	{
		if (CloudDidChange)
		{
			s_state = State.Loading;
			Log("CloudStorage Load called.");
			if (LoadCloudData())
			{
				UpdatePropertyStore();
				Log("CloudStorageLoaded");
			}
		}
		s_state = State.Idle;
	}

	private static void UpdatePropertyStore()
	{
		SetPropertyStore("TimeStamp");
		foreach (string key in s_properties.Keys)
		{
			if (key != "CharacterSelection")
			{
				SetPropertyStore(key);
			}
		}
	}

	private static void SetPropertyStore(string name)
	{
		string cloudProperty = GetCloudProperty(name, fromLocal: true);
		if (cloudProperty != null)
		{
			PropertyStore.Store(name, cloudProperty);
		}
	}

	public static void Reset()
	{
		s_state = State.Loading;
		s_properties.Clear();
		s_state = State.Idle;
		Log("Reset");
	}

	private static void UpdateCloudData()
	{
		List<PropertyStore.Property> propertyList = PropertyStore.ActiveProperties().PropertyList;
		for (int i = 0; i < propertyList.Count; i++)
		{
			PropertyStore.Property property = propertyList[i];
			SetCloudProperty(property.m_name, property.m_value);
		}
	}

	private static void SetCloudPlayerStatsProperty<T1, T2>(T1 nameEnum, T2[] stats)
	{
		SetCloudProperty(nameEnum.ToString(), stats[Convert.ToInt32(nameEnum)]);
	}

	public static string GetCloudProperty(string name, bool fromLocal)
	{
		object obj = null;
		if (fromLocal)
		{
			obj = s_properties[name];
		}
		if (obj != null)
		{
			return Convert.ToString(obj);
		}
		return null;
	}

	public static string GetCloudPropertyDeviceDesc(bool fromLocal)
	{
		return GetCloudProperty("Device", fromLocal);
	}

	private static void SetCloudProperty<T>(string name, T value)
	{
		string text = value.ToString();
		bool flag = s_properties.Contains(name);
		string cloudProperty = GetCloudProperty(name, fromLocal: true);
		if (!flag || cloudProperty != text)
		{
			s_properties[name] = text;
			s_state = State.GameDidChange;
		}
	}

	private static void Event_KeyValueStoreDidChange(List<object> keys)
	{
		Log("********************************************************");
		Log("***** Event_KeyValueStoreDidChange.  changed keys: *****");
		foreach (object key in keys)
		{
			Log(string.Empty + key.ToString());
		}
		Log("********************************************************");
	}

	private static void Event_UbiquityIdentityDidChange()
	{
		Log("Event_UbiquityIdentityDidChange");
	}

	private static void Event_EntitlementsMissing()
	{
		Log("Event_EntitlementsMissing");
	}

	private static void SaveCloudData()
	{
		Log("SaveCloudData");
		SetCloudProperty("TimeStamp", DCTime.GetCurrentTime().Ticks);
		SetCloudProperty("Device", DeviceDesc);
	}

	private static bool LoadCloudData()
	{
		Log("LoadCloudData");
		Log("No dictionary found");
		return false;
	}

	private static bool SyncCloudData()
	{
		return false;
	}

	private void Event_FeatureStateReady()
	{
		UpdateServerActiveFlag();
	}

	private void UpdateServerActiveFlag()
	{
		s_active = true;
		LSON.Property stateProperty = FeatureState.GetStateProperty("state", "icloud");
		if (stateProperty != null)
		{
			bool boolValue = true;
			if (LSONProperties.AsBool(stateProperty, out boolValue) && !boolValue)
			{
				s_active = false;
			}
		}
	}

	private static void Log(string msg)
	{
	}
}
