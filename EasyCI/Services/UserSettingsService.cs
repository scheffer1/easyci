using EasyCI.Themes;
using System;
using System.IO;
using System.Text.Json;

namespace EasyCI.Services
{
    public class UserSettings
    {
        public ThemeType PreferredTheme { get; set; } = ThemeType.Light;
    }

    public class UserSettingsService
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EasyCI",
            "settings.json");

        private static UserSettings _currentSettings;

        public static UserSettings GetSettings()
        {
            if (_currentSettings == null)
            {
                LoadSettings();
            }

            return _currentSettings;
        }

        public static void SaveSettings(UserSettings settings)
        {
            _currentSettings = settings;

            try
            {
                // Garantir que o diretório existe
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));

                // Salvar as configurações em formato JSON
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                // Em um ambiente de produção, você pode querer registrar este erro
                Console.WriteLine($"Erro ao salvar configurações: {ex.Message}");
            }
        }

        public static void SaveThemePreference(ThemeType theme)
        {
            var settings = GetSettings();
            settings.PreferredTheme = theme;
            SaveSettings(settings);
        }

        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    _currentSettings = JsonSerializer.Deserialize<UserSettings>(json);
                }
                else
                {
                    _currentSettings = new UserSettings();
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, usar configurações padrão
                _currentSettings = new UserSettings();
                Console.WriteLine($"Erro ao carregar configurações: {ex.Message}");
            }
        }
    }
}
