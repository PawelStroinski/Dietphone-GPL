using System;
using System.Collections.Generic;
using Dietphone.Tools;

namespace Dietphone.Models
{
    public class ExportAndImport
    {
        private ExportAndImportDTO dto;
        private readonly Factories factories;
        private readonly Finder finder;
        private readonly AppVersion appVersion = new AppVersion();
        private const string NAMESPACE = "http://www.pabloware.com/wp7";

        public ExportAndImport(Factories factories)
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
                var target = new MealDTO();
                target.CopyFrom(source);
                target.DTOCopyItemsFrom(source);
                targets.Add(target);
            }
            return targets;
        }

        private List<InsulinDTO> ExportInsulins()
        {
            var targets = new List<InsulinDTO>();
            foreach (var source in factories.Insulins)
            {
                var target = new InsulinDTO();
                target.CopyFrom(source);
                target.CopyCircumstancesFrom(source);
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
                target.CopyFrom(source);
                target.CopyItemsFrom(source);
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
                target.CopyFrom(source);
                target.CopyCircumstancesFrom(source);
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
}
