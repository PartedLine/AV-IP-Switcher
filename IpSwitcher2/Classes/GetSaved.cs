using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using IpSwitcher2.Models;

namespace IpSwitcher2.Classes;

public class GetSaved
{
    public static ObservableCollection<IpCompact> GetSavedIps()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IpSwitcher");
        var filePath = Path.Combine(appDataPath, "saved_addresses.xml");

        ObservableCollection<IpCompact>? existingEntries = [];

        var serializer = new XmlSerializer(typeof(List<IpCompact>));

        // Load existing entries if file exists
        if (!File.Exists(filePath)) return existingEntries;
        try
        {
            using var reader = new StreamReader(filePath);
            var list = (List<IpCompact>)serializer.Deserialize(reader)!;
            reader.Close();

            foreach (var ipCompact in list) existingEntries.Add(ipCompact);
        }
        catch (Exception)
        {
            // If there's an error reading the file, start with an empty list
            existingEntries = [];
        }

        return existingEntries;
    }
}