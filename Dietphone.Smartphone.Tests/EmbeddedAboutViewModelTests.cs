using System.Text.RegularExpressions;
using Dietphone.ViewModels;
using Dietphone.Views;
using NUnit.Framework;

namespace Dietphone.Smartphone.Tests
{
    public class EmbeddedAboutViewModelTests
    {
        [Test]
        public void Title()
        {
            Assert.IsTrue(Regex.IsMatch(new EmbeddedAboutViewModel().Title,
                "^" + Translations.DiabetesSpyTitleCase + @" \d+\.\d+$"));
        }
    }
}
