using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Dietphone.Models.Tests
{
    public abstract class ModelBasedTests
    {
        protected Factories factories;
        protected DateTime basedate;

        [SetUp]
        public void Initialize()
        {
            basedate = new DateTime(2013, 07, 24);
            factories = new FactoriesImpl();
            factories.StorageCreator = new StorageCreatorStub();
            AddProduct(1, energy: 100, carbs: 100, protein: 100, fat: 100);
            AddProduct(2, energy: 100, carbs: 100, protein: 100, fat: 100);
            AddProduct(3, energy: 100, carbs: 100, protein: 100, fat: 100);
        }

        protected Product AddProduct(byte productId, short energy, short carbs, short protein, short fat)
        {
            var product = factories.CreateProduct();
            product.Id = productId.ToGuid();
            product.EnergyPer100g = energy;
            product.CarbsTotalPer100g = carbs;
            product.ProteinPer100g = protein;
            product.FatPer100g = fat;
            return product;
        }

        protected Meal AddMealInsulinAndSugars(string meal, string insulinWithoutHour,
            string sugarsBeforeAndAfterWithoutHours = "")
        // e.g. ("12:00", "1", "100 120 140") -> ("12:00", "12:00 1", "12:00 100" "13:00 120" "14:00 140")
        {
            var addedMeal = AddMeal(meal);
            var mealHour = meal.Split(' ').First();
            AddInsulin(mealHour + " " + insulinWithoutHour);
            var sugarsSplet = sugarsBeforeAndAfterWithoutHours.Split(' ');
            for (int i = 0; i < sugarsSplet.Count(); i++)
            {
                var sugarHour = (TimeSpan.Parse(mealHour) + TimeSpan.FromHours(i)).ToString();
                AddSugars(sugarHour + " " + sugarsSplet[i]);
            }
            return addedMeal;
        }

        protected Meal AddMeal(string hourAndProductIdsAndValues) // e.g. "12:00 1 100g 2 100g"
        {
            var splet = hourAndProductIdsAndValues.Split(' ');
            var meal = factories.CreateMeal();
            meal.DateTime = basedate + TimeSpan.Parse(splet[0]);
            for (int i = 1; i < splet.Count(); i += 2)
            {
                var item = meal.AddItem();
                item.ProductId = splet[i].ToGuid();
                var match = Regex.Match(splet[i + 1], @"(\d*)(\D*)");
                var value = match.Groups[1].Value;
                var unit = match.Groups[2].Value;
                item.Value = int.Parse(value);
                item.Unit = item.Unit.TryGetValueOfAbbreviation(unit);
            }
            return meal;
        }

        protected Insulin AddInsulin(string hourAndNormalBolusAndSquareWaveBolusAndSquareWaveBolusHoursAndCircumstances)
        // e.g. "12:00 1", "12:00 1 0 0", "12:00 2 2 3 1 2 3", "12:00 1.5"
        {
            var culture = new CultureInfo("en");
            var splet = hourAndNormalBolusAndSquareWaveBolusAndSquareWaveBolusHoursAndCircumstances.Split(' ');
            var insulin = factories.CreateInsulin();
            insulin.DateTime = basedate + TimeSpan.Parse(splet[0]);
            insulin.NormalBolus = float.Parse(splet[1], culture);
            if (splet.Length > 2)
            {
                insulin.SquareWaveBolus = float.Parse(splet[2], culture);
                insulin.SquareWaveBolusHours = float.Parse(splet[3], culture);
                foreach (var circumstanceId in splet.Skip(4).Select(s => byte.Parse(s).ToGuid()))
                    insulin.AddCircumstance(new InsulinCircumstance { Id = circumstanceId });
            }
            return insulin;
        }

        protected List<Sugar> AddSugars(string hoursAndBloodSugars) // e.g. "12:00 100", "12:00 100 14:00 120"
        {
            var sugars = new List<Sugar>();
            var splet = hoursAndBloodSugars.Split(' ');
            for (int i = 0; i < splet.Count(); i += 2)
            {
                var sugar = factories.CreateSugar();
                sugar.DateTime = basedate + TimeSpan.Parse(splet[i]);
                sugar.BloodSugar = int.Parse(splet[i + 1]);
                sugars.Add(sugar);
            }
            return sugars;
        }
    }
}
