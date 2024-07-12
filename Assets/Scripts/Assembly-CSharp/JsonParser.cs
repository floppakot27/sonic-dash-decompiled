using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

internal class JsonParser
{
	private enum TOKEN
	{
		NONE,
		OPENING_BRACE,
		CLOSING_BRACE,
		OPENING_BRACKET,
		CLOSING_BRACKET,
		COMMA,
		COLON,
		OTHER
	}

	private string json;

	private int index;

	private char[] Tokens = new char[6] { '{', '}', '[', ']', ',', ':' };

	public JsonParser(string json)
	{
		this.json = Regex.Replace(json, "\\s", string.Empty);
	}

	public Dictionary<string, object> Parse()
	{
		return ParseObject();
	}

	private Dictionary<string, object> ParseObject()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Expect(TOKEN.OPENING_BRACE);
		while (true)
		{
			switch (PeekNextToken())
			{
			case TOKEN.CLOSING_BRACE:
				index++;
				return dictionary;
			case TOKEN.COMMA:
				index++;
				break;
			default:
			{
				KeyValuePair<string, object> keyValuePair = ParseDataPair();
				dictionary.Add(keyValuePair.Key, keyValuePair.Value);
				break;
			}
			}
		}
	}

	private void Expect(TOKEN token)
	{
		if (PeekNextToken() != token)
		{
			throw new UnityException(string.Concat(token, " expected"));
		}
		index++;
	}

	private void Expect(char c)
	{
		if (json[index++] != c)
		{
			throw new UnityException(c + " expected");
		}
	}

	private bool IsEnd()
	{
		return index == json.Length;
	}

	private TOKEN PeekNextToken()
	{
		if (IsEnd())
		{
			return TOKEN.NONE;
		}
		int num = json.IndexOfAny(Tokens, index);
		if (num == -1)
		{
			return TOKEN.OTHER;
		}
		return GetTokenFromChar(json[index]);
	}

	private char PeekChar()
	{
		return json[index + 1];
	}

	private char CurrentChar()
	{
		return json[index];
	}

	private TOKEN GetTokenFromChar(char c)
	{
		return c switch
		{
			'{' => TOKEN.OPENING_BRACE, 
			'}' => TOKEN.CLOSING_BRACE, 
			'[' => TOKEN.OPENING_BRACKET, 
			']' => TOKEN.CLOSING_BRACKET, 
			',' => TOKEN.COMMA, 
			':' => TOKEN.COLON, 
			_ => TOKEN.OTHER, 
		};
	}

	private KeyValuePair<string, object> ParseDataPair()
	{
		string key = ParseString();
		Expect(TOKEN.COLON);
		object value = ParseValue();
		return new KeyValuePair<string, object>(key, value);
	}

	private string ParseString()
	{
		Expect('"');
		StringBuilder stringBuilder = new StringBuilder();
		while (index < json.Length && (json[index - 1] == '\\' || json[index] != '"'))
		{
			stringBuilder.Append(json[index]);
			index++;
		}
		Expect('"');
		return stringBuilder.ToString();
	}

	private object ParseValue()
	{
		switch (PeekNextToken())
		{
		case TOKEN.OTHER:
		{
			char c = CurrentChar();
			switch (c)
			{
			case '"':
				return ParseString();
			case 't':
				if (json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
				{
					index += 4;
					return true;
				}
				break;
			}
			if (c == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
			{
				index += 5;
				return false;
			}
			if (c == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 4] == 'l')
			{
				index += 4;
				return null;
			}
			return ParseNumber();
		}
		case TOKEN.OPENING_BRACE:
			return ParseObject();
		case TOKEN.OPENING_BRACKET:
			return ParseArray();
		default:
			throw new UnityException("Expecting a value type");
		}
	}

	private List<object> ParseArray()
	{
		Expect(TOKEN.OPENING_BRACKET);
		List<object> list = new List<object>();
		while (true)
		{
			switch (PeekNextToken())
			{
			case TOKEN.CLOSING_BRACKET:
				index++;
				return list;
			case TOKEN.COMMA:
				index++;
				break;
			default:
				list.Add(ParseValue());
				break;
			}
		}
	}

	private double ParseNumber()
	{
		int num = json.IndexOfAny(new char[3] { ',', ']', '}' }, index);
		string text = json.Substring(index, num - index);
		index += text.Length;
		return double.Parse(text);
	}
}
