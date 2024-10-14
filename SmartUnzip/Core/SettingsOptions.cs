using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartUnzip.Core;

public partial class SettingsOptions : ObservableObject
{
    private const string DefaultSettingsDirectoryPath = "config";
    const string DefaultSettingSaveFilePath = @$"{DefaultSettingsDirectoryPath}\setting.json";
    const string DefaultPasswordSaveFilePath = @$"{DefaultSettingsDirectoryPath}\password.json";

    public UnzipOptions UnzipOptions { get; set; } = new();

    public string? SevenZFilePath { get; set; } = $@"";

    [ObservableProperty] int _maxUnzipThreadCount = 10;
    [ObservableProperty] int _maxTestPasswordThreadCount = 10;

    // [ObservableProperty] KeyGesture _fastAddPasswordKeyGesture = new(Key.F1);

    [JsonIgnore] public ObservableCollection<PasswordModel> Passwords { get; set; } = [];

    public bool AddPassword(string password)
    {
        if (Passwords.Any(x => x.Value == password))
        {
            return false;
        }

        Passwords.Add(new PasswordModel()
        {
            Value = password
        });

        return true;
    }

    public bool RemovePassword(string password)
    {
        var passwordModel = Passwords.FirstOrDefault(x => x.Value == password);
        if (passwordModel == null)
        {
            return false;
        }

        return Passwords.Remove(passwordModel);
    }

    public void LoadSettings()
    {
        if (File.Exists(DefaultSettingSaveFilePath))
        {
            var json = File.ReadAllText(DefaultSettingSaveFilePath);
            var settings = JsonSerializer.Deserialize<SettingsOptions>(json);
            if (settings != null)
            {
                UnzipOptions = settings.UnzipOptions;
                SevenZFilePath = settings.SevenZFilePath;
                Passwords = settings.Passwords;
            }
        }

        if (File.Exists(DefaultPasswordSaveFilePath))
        {
            var json = File.ReadAllText(DefaultPasswordSaveFilePath);
            var passwords = JsonSerializer.Deserialize<ObservableCollection<PasswordModel>>(json);
            if (passwords != null)
            {
                Passwords = passwords;
            }
        }
    }

    public void SaveSettings()
    {
        if (!Directory.Exists(DefaultSettingsDirectoryPath))
            Directory.CreateDirectory(DefaultSettingsDirectoryPath);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(DefaultSettingSaveFilePath, json);

        var passwordJson = JsonSerializer.Serialize(Passwords, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(DefaultPasswordSaveFilePath, passwordJson);
    }
}