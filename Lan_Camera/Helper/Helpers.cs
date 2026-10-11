using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

public static class Helpers
{
    public static object? GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    } 
    
    public static bool IsValidIP(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;
        if (ip.Contains(":"))
        {
            string tmp = ip.Split(':')[0];
            return IPAddress.TryParse(tmp, out IPAddress? address) 
                && address.AddressFamily == AddressFamily.InterNetwork;
        }
        else     
            return IPAddress.TryParse(ip, out IPAddress? address) 
                && address.AddressFamily == AddressFamily.InterNetwork;
    }
    
    public static string InputOrDefault(string? input, string defaultValue) =>
        string.IsNullOrWhiteSpace(input) ? defaultValue : input;

    public static void Print(object? data)
    {
        Console.WriteLine(JsonSerializer.Serialize(
            data,
            AppJsonContext.Default.ObservableCollectionCamera));
    }

    private static string GetAppDataPath()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(folder, "Lan_Camera");
        Directory.CreateDirectory(appFolder);
        return appFolder;
    }

    public static async Task SaveAsync(Store store)
    {

        string json = JsonSerializer.Serialize(store.CamerasList, AppJsonContext.Default.ObservableCollectionCamera);

        string path = Path.Combine(GetAppDataPath(), "Cameras.json");

        await File.WriteAllTextAsync(path, json);
        Console.WriteLine("Saved to: " + path);
    }

    public static async Task<ObservableCollection<Camera>?> LoadAsync()
    {
        string path = Path.Combine(GetAppDataPath(), "Cameras.json");

        try
        {
            Console.WriteLine("Loading...");

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found. Creating new list.");
                return null;
            }

            string json = await File.ReadAllTextAsync(path);
            Console.WriteLine("Loaded from: " + path);

            return JsonSerializer.Deserialize(
                       json,
                       AppJsonContext.Default.ObservableCollectionCamera)
                   ?? null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading file: " + ex.Message);
            return null;
        }
    }
}