using System.Collections;
using UnityEngine;

public class Tests_LSONReader : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(Test_EmptyFileResultsInNullLSON());
		StartCoroutine(Test_JsonFileResultsInNullLSON());
		StartCoroutine(Test_RootsHaveZeroLengthNames("www.leewinder.co.uk/ab/emptyrootnames_nonames.lson", "www.leewinder.co.uk/ab/emptyrootnames_singlename.lson"));
		StartCoroutine(Test_RootsHaveZeroLengthNames("www.leewinder.co.uk/ab/norootproperties_noproperties.lson", "www.leewinder.co.uk/ab/norootproperties_singleproperty.lson"));
		StartCoroutine(Test_RootsHaveNoProperties());
		StartCoroutine(Test_PropertyHasRootInName());
		StartCoroutine(Test_PropertyHasInvalidValue());
	}

	private IEnumerator Test_EmptyFileResultsInNullLSON()
	{
		WWW www = new WWW("www.leewinder.co.uk/ab/empty.lson");
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
	}

	private IEnumerator Test_JsonFileResultsInNullLSON()
	{
		WWW www = new WWW("www.leewinder.co.uk/ab/jsonfile.json");
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
	}

	private IEnumerator Test_RootsHaveZeroLengthNames(string pathWithNoNames, string pathWithSingleName)
	{
		WWW www = new WWW(pathWithNoNames);
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
		www = new WWW(pathWithSingleName);
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
	}

	private IEnumerator Test_RootsHaveNoProperties()
	{
		WWW www = new WWW("www.leewinder.co.uk/ab/nopropertiesinroot.lson");
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
	}

	private IEnumerator Test_PropertyHasRootInName()
	{
		WWW www = new WWW("www.leewinder.co.uk/ab/propertynamedroot.lson");
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
	}

	private IEnumerator Test_PropertyHasInvalidValue()
	{
		WWW www = new WWW("www.leewinder.co.uk/ab/invalidproperties.lson");
		yield return www;
		if (www.error == null)
		{
			LSON.Root[] root = LSONReader.Parse(www.text);
		}
	}
}
