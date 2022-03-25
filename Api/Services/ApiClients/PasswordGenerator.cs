using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace HappyTravel.Edo.Api.Infrastructure;

public static class PasswordGenerator
{
    public static string Generate()
    {
        var randomBytes = new byte[sizeof(int)];
        using var generator = RandomNumberGenerator.Create(); 
        generator.GetBytes(randomBytes);
        var random = new Random(BitConverter.ToInt32(randomBytes));

        var chars = new List<char>(RequiredLength);

        var requiredSymbolsList = new List<string>
        {
            UpperCaseLetters,
            LowerCaseLetters,
            Digits,
            NonAlphanumericSymbols
        };

        var allowedSymbols = new Dictionary<int, string>
        {
            {0, UpperCaseLetters},
            {1, LowerCaseLetters},
            {2, Digits},
            {3, NonAlphanumericSymbols}
        };

        for (var i = 0; i < RequiredLength - requiredSymbolsList.Count; i++)
        {
            var selectedSymbols = allowedSymbols[random.Next(0, allowedSymbols.Count)];
            chars.Add(selectedSymbols[random.Next(0, selectedSymbols.Length)]);
        }

        foreach (var symbol in requiredSymbolsList)
        {
            chars.Insert(random.Next(0, chars.Count), symbol[random.Next(0, symbol.Length)]);
        }

        return new string(chars.ToArray());
    }


    private const string UpperCaseLetters = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
    private const string LowerCaseLetters = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string NonAlphanumericSymbols = " !\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

    private const int RequiredLength = 20;
}