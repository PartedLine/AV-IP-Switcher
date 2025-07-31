using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using IpSwitcher2.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace IpSwitcher2.Classes;

public static class FileOperations
{
    private static FilePickerFileType XmlFileType => new("XML Files")
    {
        Patterns = ["*.xml"],
        MimeTypes = ["application/xml"]
    };

    public static async Task ExportAddresses(IStorageProvider storageProvider, Window parent)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Export Saved Addresses",
            SuggestedFileName = "saved_addresses.xml",
            DefaultExtension = "xml",
            FileTypeChoices = [XmlFileType]
        };

        var file = await storageProvider.SaveFilePickerAsync(options);
        if (file == null) return;

        try
        {
            await using var sourceStream = File.OpenRead(FileService.SavedAddressesPath);
            await using var destinationStream = await file.OpenWriteAsync();
            await sourceStream.CopyToAsync(destinationStream);

            await ShowSuccess(parent, "Addresses exported successfully!");
        }
        catch (Exception ex)
        {
            await ShowError(parent, $"Failed to export addresses: {ex.Message}");
        }
    }

    public static async Task ImportAddresses(IStorageProvider storageProvider, Window parent, Action onSuccess)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Import Saved Addresses",
            FileTypeFilter = [XmlFileType],
            AllowMultiple = false
        };

        var files = await storageProvider.OpenFilePickerAsync(options);
        var file = files.FirstOrDefault();
        if (file == null) return;

        try
        {
            var importedEntries = await ReadImportedFile(file);
            var existingEntries = FileService.LoadSavedAddresses();
            var nextId = FileService.GetNextId(existingEntries);

            foreach (var entry in importedEntries)
            {
                entry.Id = nextId++;
                existingEntries.Add(entry);
            }

            FileService.SaveAddresses(existingEntries);
            onSuccess();

            await ShowSuccess(parent, $"Successfully imported {importedEntries.Count} addresses");
        }
        catch (Exception ex)
        {
            await ShowError(parent, $"Failed to import addresses: {ex.Message}");
        }
    }

    private static async Task<List<IpCompact>> ReadImportedFile(IStorageFile file)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return (List<IpCompact>)new XmlSerializer(typeof(List<IpCompact>)).Deserialize(reader)!;
    }

    private static Task<ButtonResult> ShowSuccess(Window parent, string message)
    {
        return MessageBoxManager.GetMessageBoxStandard("Success", message, ButtonEnum.Ok, Icon.Success)
            .ShowWindowDialogAsync(parent);
    }

    private static Task<ButtonResult> ShowError(Window parent, string message)
    {
        return MessageBoxManager.GetMessageBoxStandard("Error", message, ButtonEnum.Ok, Icon.Error)
            .ShowWindowDialogAsync(parent);
    }
}