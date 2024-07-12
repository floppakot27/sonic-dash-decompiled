using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Utils
{
	[StructLayout(0, Size = 1)]
	public struct ClosestPoint
	{
		public float LineDistance { get; set; }

		public float SqrError { get; set; }
	}

	public delegate float Mapper(float inVal);

	public static float SmoothDamp(float current, float target, ref float vel, float smoothing)
	{
		if (current == target)
		{
			vel = 0f;
			return target;
		}
		return Mathf.SmoothDamp(current, target, ref vel, smoothing);
	}

	public static T GetComponentInChildren<T>(GameObject source) where T : MonoBehaviour
	{
		T[] componentsInChildren = source.GetComponentsInChildren<T>(includeInactive: true);
		if (componentsInChildren == null)
		{
			return (T)null;
		}
		if (componentsInChildren.Length == 0)
		{
			return (T)null;
		}
		return componentsInChildren[0];
	}

	public static T GetComponentInParent<T>(GameObject source) where T : MonoBehaviour
	{
		T val = (T)null;
		Transform parent = source.transform.parent;
		do
		{
			val = (T)parent.GetComponent<T>();
			parent = parent.parent;
		}
		while (parent != null && val == null);
		return val;
	}

	public static int GetEnumCount<T>()
	{
		return Enum.GetValues(typeof(T)).Length;
	}

	public static GameObject FindChildByName(GameObject source, string name)
	{
		Transform[] componentsInChildren = source.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == name)
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	public static GameObject FindTagInChildren(GameObject source, string tag)
	{
		Transform[] componentsInChildren = source.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.tag == tag)
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	public static T FindBehaviourInTree<T>(MonoBehaviour source, T target) where T : MonoBehaviour
	{
		target = (T)source.GetComponent<T>();
		if (target == null)
		{
			target = (T)source.GetComponentInChildren<T>();
		}
		if (target == null)
		{
			Transform parent = source.transform.parent;
			do
			{
				target = (T)parent.GetComponent<T>();
				if (target != null)
				{
					break;
				}
				parent = parent.transform.parent;
			}
			while (parent != null);
		}
		return target;
	}

	public static float MapValue(float inVal, float inFrom, float inTo, float outFrom, float outTo)
	{
		float value = MapValue_NoClamp(inVal, inFrom, inTo, outFrom, outTo);
		return (!(outFrom < outTo)) ? Mathf.Clamp(value, outTo, outFrom) : Mathf.Clamp(value, outFrom, outTo);
	}

	public static float MapValue_NoClamp(float inVal, float inFrom, float inTo, float outFrom, float outTo)
	{
		float num = ((inFrom == inTo) ? 0f : ((inVal - inFrom) / (inTo - inFrom)));
		return outFrom + num * (outTo - outFrom);
	}

	public static Mapper MakeMap(float inFrom, float inTo, float outFrom, float outTo)
	{
		return (float inVal) => MapValue(inVal, inFrom, inTo, outFrom, outTo);
	}

	public static Mapper MakeCliffMap(float inStart, float inCliff, float outStart, float outEnd, float outCliff)
	{
		return (float inVal) => (!(inVal > inCliff)) ? MapValue(inVal, inStart, inCliff, outStart, outEnd) : outCliff;
	}

	public static ClosestPoint CalculateClosestPoint(Vector3 lineStart, Vector3 lineDir, float lineLength, Vector3 point)
	{
		Vector3 lhs = point - lineStart;
		if (lineLength == 0f)
		{
			ClosestPoint result = default(ClosestPoint);
			result.LineDistance = 0f;
			result.SqrError = lhs.sqrMagnitude;
			return result;
		}
		Vector3 rhs = lineDir * lineLength;
		float num = lineLength * lineLength;
		float num2 = Vector3.Dot(lhs, rhs);
		float value = num2 / num;
		float num3 = Mathf.Clamp(value, 0f, 1f);
		float num4 = num3 * lineLength;
		Vector3 vector = lineStart + num4 * lineDir;
		float sqrMagnitude = (point - vector).sqrMagnitude;
		ClosestPoint result2 = default(ClosestPoint);
		result2.LineDistance = num4;
		result2.SqrError = sqrMagnitude;
		return result2;
	}

	public static Quaternion GetQuaternionFromMatrix(Matrix4x4 m)
	{
		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
	}

	public static Matrix4x4 CopyMatrixWithNoScaling(Matrix4x4 m)
	{
		Vector3 pos = m.MultiplyPoint3x4(Vector3.zero);
		Quaternion quaternionFromMatrix = GetQuaternionFromMatrix(m);
		return Matrix4x4.TRS(pos, quaternionFromMatrix, Vector3.one);
	}

	public static bool Approximately(Matrix4x4 m1, Matrix4x4 m2)
	{
		for (int i = 0; i < 16; i++)
		{
			if (!Mathf.Approximately(m1[i], m2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static float SmoothlyApproach1(float t, float minT, float maxT, float smoothness)
	{
		float num = MapValue(t, minT, maxT, 0f, 1f);
		return 1f - Mathf.Pow(1f - num, smoothness);
	}

	public static void InstantlyInvoke(UnityEngine.Object instance, string methodName, object[] param, SendMessageOptions msgOptions)
	{
		Type type = instance.GetType();
		type.GetMethod(methodName)?.Invoke(instance, param);
	}

	public static IEnumerable<int> IndiciesWhere<T>(IEnumerable<T> source, Func<T, bool> predicate)
	{
		int index = 0;
		foreach (T element in source)
		{
			if (predicate(element))
			{
				yield return index;
			}
			index++;
		}
	}

	public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source, System.Random rng)
	{
		T[] elements = source.ToArray();
		for (int i = elements.Length - 1; i > 0; i--)
		{
			int swapIndex = rng.Next(i + 1);
			yield return elements[swapIndex];
			elements[swapIndex] = elements[i];
		}
		yield return elements[0];
	}

	public static Vector3 Smooth(Vector3 current, Vector3 target, float factor)
	{
		return (current * factor + target) / (factor + 1f);
	}

	public static float NormalDistribution(System.Random rng, float center, float radius, int confidence)
	{
		double num = 0.0;
		for (int i = 0; i < confidence; i++)
		{
			num += rng.NextDouble();
		}
		num /= (double)confidence;
		float num2 = (float)(num / (double)confidence);
		return center + radius * (2f * num2 - 1f);
	}

	public static int GetBitCount(uint x)
	{
		int num = 0;
		while (x != 0)
		{
			num++;
			x &= x - 1;
		}
		return num;
	}
}
