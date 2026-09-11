using CatalogStudio.Models;
namespace CatalogStudio.Services;

public class AgeCalculatorService
{
    public string CalculateModelAge(AgeRange ageRange)
    {
        double average = (ageRange.MinimumAge + ageRange.MaximumAge) / 2d;
        var text = average % 1 == 0 ? $"{average:0}" : ageRange.AgeUnit == AgeUnit.Months ? $"{Math.Floor(average):0}-{Math.Ceiling(average) + 1:0}" : $"{Math.Floor(average):0}-{Math.Ceiling(average):0}";
        return $"approximately {text} {(ageRange.AgeUnit == AgeUnit.Months ? "months old baby" : "years old child")}";
    }
}
