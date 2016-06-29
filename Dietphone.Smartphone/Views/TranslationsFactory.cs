namespace Dietphone.Views
{
    public sealed class TranslationsFactory
    {
        private static Translations translations = null;
        private static readonly object translationsLock = new object();

        public static Translations Translations
        {
            get
            {
                lock (translationsLock)
                {
                    if (translations == null)
                    {
                        var publicTranslatons = new PublicTranslations();
                        translations = publicTranslatons.CreateTranslations();
                        SetCulture();
                    }
                    return translations;
                }
            }
        }

        private static void SetCulture()
        {
            Translations.Culture = MyApp.CurrentUiCultureInfo;
        }
    }
}