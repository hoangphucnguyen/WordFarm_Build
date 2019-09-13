using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class StringUtils {

	// from https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5(v=vs.110).aspx
	public static string GetMd5Hash(string input)
	{
		MD5 md5Hash = MD5.Create ();

		// Convert the input string to a byte array and compute the hash.
		byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

		// Create a new Stringbuilder to collect the bytes
		// and create a string.
		StringBuilder sBuilder = new StringBuilder();

		// Loop through each byte of the hashed data 
		// and format each one as a hexadecimal string.
		for (int i = 0; i < data.Length; i++)
		{
			sBuilder.Append(data[i].ToString("x2"));
		}

		// Return the hexadecimal string.
		return sBuilder.ToString();
	}

	// Verify a hash against a string.
	public static bool VerifyMd5Hash(string input, string hash)
	{
		// Hash the input.
		string hashOfInput = GetMd5Hash(input);

		// Create a StringComparer an compare the hashes.
		StringComparer comparer = StringComparer.OrdinalIgnoreCase;

		if (0 == comparer.Compare(hashOfInput, hash))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool IsNullOrWhiteSpace(string value)
	{
		return value == null || value.Trim() == "";
	}
}
