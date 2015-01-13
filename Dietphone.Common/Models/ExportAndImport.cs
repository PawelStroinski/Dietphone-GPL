using System;
using System.Collections.Generic;
using Dietphone.Tools;
using System.Linq;

namespace Dietphone.Models
{
    public interface ExportAndImport
    {
        string Export();
        void Import(string data);
    }

    public class ExportAndImportImpl : ExportAndImport
    {
        private ExportAndImportDTO dto;
        private readonly Factories factories;
        private readonly Finder finder;
        private readonly AppVersion appVersion = new AppVersion();
        private const string NAMESPACE = "http://www.pabloware.com/wp7";

        public ExportAndImportImpl(Factories factories)
        {
            this.factories = factories;
            finder = factories.Finder;
        }

        public string Export()
        {
            dto = new ExportAndImportDTO
            {
                AppVersion = appVersion.GetAppVersion(),
                Meals = ExportMeals(),
                MealNames = factories.MealNames,
                Products = finder.FindProductsAddedByUser(),
                Categories = factories.Categories,
                Sugars = factories.Sugars,
                Insulins = ExportInsulins(),
                InsulinCircumstances = factories.InsulinCircumstances,
                Settings = factories.Settings
            };
            return dto.Serialize(NAMESPACE);
        }

        public void Import(string data)
        {
            dto = data.Deserialize<ExportAndImportDTO>(NAMESPACE);
            ImportMeals();
            ImportMealNames();
            ImportProducts();
            ImportCategories();
            ImportSugars();
            ImportInsulins();
            ImportInsulinCircumstances();
            ImportSettings();
        }

        private List<MealDTO> ExportMeals()
        {
            var targets = new List<MealDTO>();
            foreach (var source in factories.Meals)
            {
                var target = DTOFactory.MealToDTO(source);
                targets.Add(target);
            }
            return targets;
        }

        private List<InsulinDTO> ExportInsulins()
        {
            var targets = new List<InsulinDTO>();
            foreach (var source in factories.Insulins)
            {
                var target = DTOFactory.InsulinToDTO(source);
                targets.Add(target);
            }
            return targets;
        }

        private void ImportMeals()
        {
            foreach (var source in dto.Meals)
            {
                var target = finder.FindMealById(source.Id);
                if (target == null)
                {
                    target = factories.CreateMeal();
                }
                DTOReader.DTOToMeal(source, target);
            }
        }

        private void ImportMealNames()
        {
            var importer = new GenericImporter<MealName>
            {
                Sources = dto.MealNames,
                Targets = factories.MealNames
            };
            importer.Create += factories.CreateMealName;
            importer.Execute();
        }

        private void ImportProducts()
        {
            var importer = new GenericImporter<Product>
            {
                Sources = dto.Products,
                Targets = factories.Products
            };
            importer.Create += factories.CreateProduct;
            importer.Execute();
        }

        private void ImportCategories()
        {
            var importer = new GenericImporter<Category>
            {
                Sources = dto.Categories,
                Targets = factories.Categories
            };
            importer.Create += factories.CreateCategory;
            importer.Execute();
        }

        private void ImportSugars()
        {
            var importer = new GenericImporter<Sugar>
            {
                Sources = dto.Sugars,
                Targets = factories.Sugars
            };
            importer.Create += factories.CreateSugar;
            importer.Execute();
        }

        private void ImportInsulins()
        {
            foreach (var source in dto.Insulins)
            {
                var target = finder.FindInsulinById(source.Id);
                if (target == null)
                {
                    target = factories.CreateInsulin();
                }
                DTOReader.DTOToInsulin(source, target);
            }
        }

        private void ImportInsulinCircumstances()
        {
            var importer = new GenericImporter<InsulinCircumstance>
            {
                Sources = dto.InsulinCircumstances,
                Targets = factories.InsulinCircumstances
            };
            importer.Create += factories.CreateInsulinCircumstance;
            importer.Execute();
        }

        private void ImportSettings()
        {
            var source = dto.Settings;
            var target = factories.Settings;
            target.CopyFrom(source);
        }

        public sealed class ExportAndImportDTO
        {
            public string AppVersion { get; set; }
            public List<MealDTO> Meals { get; set; }
            public List<MealName> MealNames { get; set; }
            public List<Product> Products { get; set; }
            public List<Category> Categories { get; set; }
            public List<Sugar> Sugars { get; set; }
            public List<InsulinDTO> Insulins { get; set; }
            public List<InsulinCircumstance> InsulinCircumstances { get; set; }
            public Settings Settings { get; set; }
        }

        private sealed class GenericImporter<T> where T : EntityWithId
        {
            public delegate T CreateHandler();

            public List<T> Sources { get; set; }
            public List<T> Targets { get; set; }
            public event CreateHandler Create;

            public void Execute()
            {
                foreach (var source in Sources)
                {
                    var target = Targets.FindById(source.Id);
                    if (target == null)
                    {
                        target = Create();
                    }
                    target.CopyFrom(source);
                }
            }
        }
    }

    public sealed class MealDTO : Meal
    {
        public new List<MealItem> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }

        public void DTOCopyItemsFrom(Meal source)
        {
            InternalCopyItemsFrom(source);
        }
    }

    public sealed class InsulinDTO : Insulin
    {
        public new List<Guid> Circumstances
        {
            get
            {
                return circumstances;
            }
            set
            {
                circumstances = value;
            }
        }
    }

    public sealed class PatternDTO
    {
        public byte RightnessPoints { get; set; }
        public MealItem Match { get; set; }
        public MealDTO From { get; set; }
        public InsulinDTO Insulin { get; set; }
        public Sugar Before { get; set; }
        public List<Sugar> After { get; set; }
        public MealItem For { get; set; }
        public float Factor { get; set; }
    }

    public class ReplacementItemDTO
    {
        public PatternDTO Pattern { get; set; }
        public List<PatternDTO> Alternatives { get; set; }
    }

    public static class DTOFactory
    {
        public static MealDTO MealToDTO(Meal meal)
        {
            var dto = new MealDTO();
            dto.CopyFrom(meal);
            dto.DTOCopyItemsFrom(meal);
            return dto;
        }

        public static InsulinDTO InsulinToDTO(Insulin insulin)
        {
            var dto = new InsulinDTO();
            dto.CopyFrom(insulin);
            dto.CopyCircumstancesFrom(insulin);
            return dto;
        }

        public static PatternDTO PatternToDTO(Pattern pattern)
        {
            return new PatternDTO
            {
                RightnessPoints = pattern.RightnessPoints,
                Match = pattern.Match,
                From = MealToDTO(pattern.From),
                Insulin = InsulinToDTO(pattern.Insulin),
                Before = pattern.Before,
                After = pattern.After.ToList(),
                For = pattern.For,
                Factor = pattern.Factor
            };
        }

        public static ReplacementItemDTO ReplacementItemToDTO(ReplacementItem replacementItem)
        {
            return new ReplacementItemDTO
            {
                Pattern = PatternToDTO(replacementItem.Pattern),
                Alternatives = replacementItem.Alternatives.Select(PatternToDTO).ToList()
            };
        }
    }

    public static class DTOReader
    {
        public static void DTOToMeal(MealDTO dto, Meal meal)
        {
            meal.CopyFrom(dto);
            meal.CopyItemsFrom(dto);
        }

        public static void DTOToInsulin(InsulinDTO dto, Insulin insulin)
        {
            insulin.CopyFrom(dto);
            insulin.CopyCircumstancesFrom(dto);
        }

        public static Pattern DTOToPattern(PatternDTO dto, Factories factories)
        {
            var pattern = new Pattern
            {
                RightnessPoints = dto.RightnessPoints,
                Match = dto.Match,
                From = new Meal(),
                Insulin = new Insulin(),
                Before = dto.Before,
                After = dto.After,
                For = dto.For,
                Factor = dto.Factor
            };
            var from = pattern.From;
            var insulin = pattern.Insulin;
            from.SetOwner(factories);
            insulin.SetOwner(factories);
            DTOToMeal(dto.From, pattern.From);
            DTOToInsulin(dto.Insulin, pattern.Insulin);
            return pattern;
        }

        public static ReplacementItem DTOToReplacementItem(ReplacementItemDTO dto, Factories factories)
        {
            return new ReplacementItem
            {
                Pattern = DTOToPattern(dto.Pattern, factories),
                Alternatives = dto.Alternatives.Select(pattern => DTOToPattern(pattern, factories)).ToList()
            };
        }
    }
}
