using Microsoft.Maui.Handlers;

namespace TDE3_BancoDeDados.Views
{
    /// <summary>
    /// Remove a borda/sublinhado nativo dos campos (Entry, Picker e DatePicker),
    /// para que eles usem a moldura arredondada definida no estilo "Campo".
    /// </summary>
    static class AparenciaCampos
    {
        public static void Configurar()
        {
            EntryHandler.Mapper.AppendToMapping("SemBorda", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = TintTransparente();
#elif WINDOWS
                Limpar(handler.PlatformView,
                    "TextControlBackgroundPointerOver", "TextControlBackgroundFocused",
                    "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused",
                    "TextControlBorderThemeThickness", "TextControlBorderThemeThicknessFocused");
#endif
            });

            PickerHandler.Mapper.AppendToMapping("SemBorda", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = TintTransparente();
#elif WINDOWS
                handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
                Limpar(handler.PlatformView,
                    "ComboBoxBackground", "ComboBoxBackgroundPointerOver", "ComboBoxBackgroundPressed",
                    "ComboBoxBackgroundUnfocused", "ComboBoxBackgroundFocused",
                    "ComboBoxBorderBrush", "ComboBoxBorderBrushPointerOver", "ComboBoxBorderBrushPressed",
                    "ComboBoxBackgroundBorderBrushFocused", "ComboBoxBackgroundBorderBrushUnfocused",
                    "ComboBoxBorderThemeThickness");
#endif
            });

            DatePickerHandler.Mapper.AppendToMapping("SemBorda", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = TintTransparente();
#elif WINDOWS
                handler.PlatformView.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
                Limpar(handler.PlatformView,
                    "CalendarDatePickerBackground", "CalendarDatePickerBackgroundPointerOver",
                    "CalendarDatePickerBackgroundPressed", "CalendarDatePickerBackgroundFocused",
                    "CalendarDatePickerBorderBrush", "CalendarDatePickerBorderBrushPointerOver",
                    "CalendarDatePickerBorderBrushPressed", "CalendarDatePickerBorderThemeThickness");
#endif
            });
        }

#if ANDROID
        static Android.Content.Res.ColorStateList TintTransparente() =>
            Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif WINDOWS
        static void Limpar(Microsoft.UI.Xaml.Controls.Control controle, params string[] recursos)
        {
            var transparente = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            var semBorda = new Microsoft.UI.Xaml.Thickness(0);

            controle.Background = transparente;
            controle.BorderThickness = semBorda;

            // Sobrescreve os recursos usados nos estados "mouse em cima" e "com foco"
            foreach (var chave in recursos)
                controle.Resources[chave] = chave.EndsWith("Thickness") || chave.EndsWith("ThicknessFocused")
                    ? semBorda
                    : transparente;
        }
#endif
    }
}
