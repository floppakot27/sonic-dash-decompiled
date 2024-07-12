using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HLSociaIImageHelper
{
	public static Texture2D getImage(string addressToken, Texture2D defaultImage)
	{
		int num = Convert.ToInt32(addressToken);
		if (num != 0)
		{
			IntPtr source = new IntPtr(num);
			byte[] array = new byte[65536];
			Marshal.Copy(source, array, 0, 65536);
			Color32[] array2 = new Color32[16384];
			for (int i = 0; i < 16384; i++)
			{
				array2[i].r = array[i * 4];
				array2[i].g = array[i * 4 + 1];
				array2[i].b = array[i * 4 + 2];
				array2[i].a = array[i * 4 + 3];
			}
			Texture2D texture2D = new Texture2D(128, 128);
			texture2D.SetPixels32(array2);
			texture2D.Apply();
			return texture2D;
		}
		return defaultImage;
	}
}
