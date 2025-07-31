using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using IpSwitcher2.Models;

namespace IpSwitcher2.Classes;

public static class FileService
{
    private static readonly XmlSerializer Serializer = new(typeof(List<IpCompact>));

    public static string AppDataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IpSwitcher");

    public static string SavedAddressesPath => Path.Combine(AppDataPath, "saved_addresses.xml");

    public static List<IpCompact> LoadSavedAddresses()
    {
        if (!File.Exists(SavedAddressesPath)) return [];

        try
        {
            using var reader = new StreamReader(SavedAddressesPath);
            return (List<IpCompact>)Serializer.Deserialize(reader)!;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public static void SaveAddresses(List<IpCompact> addresses)
    {
        Directory.CreateDirectory(AppDataPath);
        using var writer = new StreamWriter(SavedAddressesPath);
        Serializer.Serialize(writer, addresses);
    }

    public static int GetNextId(List<IpCompact> existingEntries)
    {
        return existingEntries.Count > 0 ? existingEntries.Max(x => x.Id) + 1 : 0;
    }
}