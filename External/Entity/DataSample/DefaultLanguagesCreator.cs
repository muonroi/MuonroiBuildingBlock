namespace Muonroi.BuildingBlock.External.Entity.DataSample
{
    public class DefaultLanguagesCreator<TContext>(TContext context)
        where TContext : MDbContext
    {
        public static List<MLanguage> InitialLanguages => GetInitialLanguages();

        private readonly TContext _context = context;

        private static List<MLanguage> GetInitialLanguages()
        {
            return
            [
            new MLanguage("en", "English", "famfamfam-flags us"),
            new MLanguage("vi", "Tiếng Việt", "famfamfam-flags vn"),
            ];
        }

        public void Create()
        {
            CreateLanguages();
        }

        private void CreateLanguages()
        {
            foreach (MLanguage language in InitialLanguages)
            {
                AddLanguageIfNotExists(language);
            }
        }

        private void AddLanguageIfNotExists(MLanguage language)
        {
            language.CreatedDateTS = DateTime.UtcNow.GetTimeStamp();
            if (_context.Languages.IgnoreQueryFilters().Any(l => l.Name == language.Name))
            {
                return;
            }
            _ = _context.Languages.Add(language);
            _ = _context.SaveChanges();
        }
    }
}