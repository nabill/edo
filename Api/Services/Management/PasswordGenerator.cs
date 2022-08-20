using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace HappyTravel.Edo.Api.Services.Management;

public static class PasswordGenerator
{
    public static string Generate()
    {
        var randomBytes = new byte[sizeof(int)];
        using var generator = RandomNumberGenerator.Create(); 
        generator.GetBytes(randomBytes);
        var random = new Random(BitConverter.ToInt32(randomBytes));

        var chars = new List<char>(RequiredLength);

        for (var i = 0; i < RequiredLength - RequiredSymbols.Count; i++)
        {
            var selectedSymbols = AllowedSymbols[random.Next(0, AllowedSymbols.Count)];
            chars.Add(selectedSymbols[random.Next(0, selectedSymbols.Length)]);
        }

        foreach (var symbol in RequiredSymbols)
        {
            chars.Insert(random.Next(0, chars.Count), symbol[random.Next(0, symbol.Length)]);
        }

        return new string(chars.ToArray());
    }


    private const string UpperCaseLetters = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
    private const string LowerCaseLetters = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string NonAlphanumericSymbols = " !\"#$%&()*+,-./:;=?@[\\]^_{|}~";
    
    private static readonly List<string> RequiredSymbols = new()
    {
        UpperCaseLetters,
        LowerCaseLetters,
        Digits,
        NonAlphanumericSymbols
    };

    private static readonly Dictionary<int, string> AllowedSymbols = new()
    {
        {0, UpperCaseLetters},
        {1, LowerCaseLetters},
        {2, Digits},
        {3, NonAlphanumericSymbols}
    };

    private const int RequiredLength = 20;
}