using System;
using Dietphone.Tools;
using Dietphone.Views;

namespace Dietphone.Models
{
    public class MealItemBase : Entity
    {
        public float Value { get; set; }
        public Unit Unit { get; set; }
        private Guid productId;
        private Product foundProduct;
        private bool searchedForProduct;

        public Guid ProductId
        {
            get
            {
                return productId;
            }
            set
            {
                if (productId != value)
                {
                    productId = value;
                    searchedForProduct = false;
                }
            }
        }

        public Product Product
        {
            // Debatable. Maybe searching each time will be fast enough.
            get
            {
                VerifySearchedForProduct();
                if (!searchedForProduct)
                {
                    foundProduct = Finder.FindProductById(ProductId);
                    searchedForProduct = true;
                }
                if (foundProduct == null)
                {
                    return DefaultEntities.Product;
                }
                else
                {
                    return foundProduct;
                }
            }
        }

        private void VerifySearchedForProduct()
        {
            var canVerify = foundProduct != null;
            if (searchedForProduct && canVerify)
            {
                var products = Owner.Products;
                var removedOrReplaced = !products.Contains(foundProduct);
                if (removedOrReplaced)
                {
                    searchedForProduct = false;
                }
            }
        }

        public override bool Equals(object obj)
        {
            var mealItem = obj as MealItemBase;
            if (mealItem == null
                 || mealItem.Value != Value
                 || mealItem.Unit != Unit
                 || mealItem.productId != productId)
                return false;
            else
                return true;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() * 2
                 + Unit.GetHashCode() * 3
                 + productId.GetHashCode() * 5;
        }
    }

    public class MealItemWithNutrientsPerUnit : MealItemBase
    {
        private const float BASE_PER_100G_IN_GRAMS = 100;

        protected float EnergyPerUnit
        {
            get
            {
                if (UnitUsability.AreNutrientsPer100gUsable)
                {
                    return Product.EnergyPer100g / BasePer100g;
                }
                else
                    if (UnitUsability.AreNutrientsPerServingUsable)
                    {
                        return Product.EnergyPerServing / BasePerServing;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        protected float ProteinPerUnit
        {
            get
            {
                if (UnitUsability.AreNutrientsPer100gUsable)
                {
                    return Product.ProteinPer100g / BasePer100g;
                }
                else
                    if (UnitUsability.AreNutrientsPerServingUsable)
                    {
                        return Product.ProteinPerServing / BasePerServing;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        protected float FatPerUnit
        {
            get
            {
                if (UnitUsability.AreNutrientsPer100gUsable)
                {
                    return Product.FatPer100g / BasePer100g;
                }
                else
                    if (UnitUsability.AreNutrientsPerServingUsable)
                    {
                        return Product.FatPerServing / BasePerServing;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        protected float DigestibleCarbsPerUnit
        {
            get
            {
                if (UnitUsability.AreNutrientsPer100gUsable)
                {
                    return Product.DigestibleCarbsPer100g / BasePer100g;
                }
                else
                    if (UnitUsability.AreNutrientsPerServingUsable)
                    {
                        return Product.DigestibleCarbsPerServing / BasePerServing;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        protected UnitUsability UnitUsability
        {
            get
            {
                return new UnitUsability()
                {
                    Product = Product,
                    Unit = Unit
                };
            }
        }

        private float BasePer100g
        {
            get
            {
                if (Unit.IsConvertibleToGram())
                {
                    return Unit.Gram.ConvertTo(Unit, BASE_PER_100G_IN_GRAMS);
                }
                else
                {
                    var servingSizeUnit = Product.ServingSizeUnit;
                    var servingSizeValue = Product.ServingSizeValue;
                    return BASE_PER_100G_IN_GRAMS / servingSizeUnit.ConvertTo(Unit.Gram, servingSizeValue);
                }
            }
        }

        private float BasePerServing
        {
            get
            {
                if (Unit == Unit.ServingSize)
                {
                    return 1;
                }
                else
                {
                    var servingSizeUnit = Product.ServingSizeUnit;
                    var servingSizeValue = Product.ServingSizeValue;
                    return servingSizeUnit.ConvertTo(Unit, servingSizeValue);
                }
            }
        }
    }

    public class MealItemWithNutrients : MealItemWithNutrientsPerUnit
    {
        public short Energy
        {
            get
            {
                var energy = EnergyPerUnit * Value;
                var roundedEnergy = Math.Round(energy);
                return (short)roundedEnergy;
            }
        }

        public float Protein
        {
            get
            {
                return ProteinPerUnit * Value;
            }
        }

        public float Fat
        {
            get
            {
                return FatPerUnit * Value;
            }
        }

        public float DigestibleCarbs
        {
            get
            {
                return DigestibleCarbsPerUnit * Value;
            }
        }

        public float Cu
        {
            get
            {
                var calculator = new Calculator()
                {
                    DigestibleCarbs = DigestibleCarbs
                };
                return calculator.Cu;
            }
        }

        public float Fpu
        {
            get
            {
                var calculator = new Calculator()
                {
                    Protein = Protein,
                    Fat = Fat
                };
                return calculator.Fpu;
            }
        }

        public byte PercentOfEnergyInMeal(Meal meal)
        {
            return (byte)Math.Round((double)Energy / meal.Energy * 100);
        }
    }

    public class MealItemWithValidation : MealItemWithNutrients
    {
        public string Validate()
        {
            string[] validation = { ValidateProduct(), ValidateValue(), ValidateUnit() };
            return validation.JoinOptionalSentences();
        }

        private string ValidateProduct()
        {
            if (Product == DefaultEntities.Product)
            {
                return Translations.TheProductDoesNotExist;
            }
            return string.Empty;
        }

        private string ValidateValue()
        {
            if (Value == 0)
            {
                return Translations.QuantityOfTheIngredientWasNotSpecified;
            }
            return string.Empty;
        }

        private string ValidateUnit()
        {
            var canValidate = Product != DefaultEntities.Product;
            if (canValidate && !UnitUsability.AnyNutrientsPerUnitPresent)
            {
                var unit = Unit.GetAbbreviationOrServingSizeDesc(Product);
                return string.Format(Translations.NoInformationAboutNutritionalValuePerUnit, unit);
            }
            return string.Empty;
        }
    }

    public sealed class MealItem : MealItemWithValidation
    {
    }

    public sealed class UnitUsability
    {
        public Product Product { get; set; }
        public Unit Unit { get; set; }

        public bool AnyNutrientsPerUnitPresent
        {
            get
            {
                return AreNutrientsPer100gUsable || AreNutrientsPerServingUsable;
            }
        }

        public bool AreNutrientsPer100gUsable
        {
            get
            {
                var unitsMatches = Unit.IsConvertibleToGram();
                if (unitsMatches && Product.AnyNutrientsPer100gPresent)
                    return true;
                if (AreNutrientsPerServingUsable)
                    return false;
                var servingSizeUnitMatches = Unit == Unit.ServingSize && Product.ServingSizeUnit.IsConvertibleToGram();
                var sizePresent = Product.ServingSizeValue != 0;
                return servingSizeUnitMatches && sizePresent && Product.AnyNutrientsPer100gPresent;
            }
        }

        public bool AreNutrientsPerServingUsable
        {
            get
            {
                var unitsMatches = Unit.IsConvertibleTo(Product.ServingSizeUnit) || Unit == Unit.ServingSize;
                var sizePresent = Product.ServingSizeValue != 0;
                return unitsMatches && sizePresent && Product.AnyNutrientsPerServingPresent;
            }
        }
    }
}