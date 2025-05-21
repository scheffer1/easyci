using System;
using System.Windows;
using EasyCI.Services;

namespace EasyCI.Themes
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        private const string LightThemeSource = "/EasyCI;component/Themes/LightTheme.xaml";
        private const string DarkThemeSource = "/EasyCI;component/Themes/DarkTheme.xaml";

        public static ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

        public static event EventHandler<ThemeType> ThemeChanged;

        public static void Initialize()
        {
            // Carregar o tema das configurações do usuário
            var settings = UserSettingsService.GetSettings();
            ApplyTheme(settings.PreferredTheme);
        }

        public static void ToggleTheme()
        {
            ThemeType newTheme = CurrentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light;
            ApplyTheme(newTheme);
        }

        public static void ApplyTheme(ThemeType theme)
        {
            // Remover temas existentes
            for (int i = Application.Current.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var resource = Application.Current.Resources.MergedDictionaries[i];
                if (resource.Source != null &&
                    (resource.Source.OriginalString == LightThemeSource ||
                     resource.Source.OriginalString == DarkThemeSource))
                {
                    Application.Current.Resources.MergedDictionaries.RemoveAt(i);
                }
            }

            // Adicionar o novo tema
            var newThemeUri = new Uri(theme == ThemeType.Light ? LightThemeSource : DarkThemeSource, UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = newThemeUri });

            // Atualizar o tema atual
            CurrentTheme = theme;

            // Salvar a preferência do usuário
            UserSettingsService.SaveThemePreference(theme);

            // Notificar sobre a mudança de tema
            ThemeChanged?.Invoke(null, theme);
        }
    }
}
