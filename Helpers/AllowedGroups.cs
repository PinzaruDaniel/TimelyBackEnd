using System.ComponentModel.DataAnnotations;

namespace TimelyBackEnd.Helpers;

public static class AllowedGroups
{
    private static readonly IReadOnlyCollection<string> GroupNames = new List<string>
    {
        "PTPP-251",
        "DAW-251",
        "RC-251",
        "C-251",
        "TRT-251",
        "DTTA-251",
        "TA-251",
        "TAP-251",
        "MSP-251",
        "UTIA-251",
        "TPM-251",
        "PTPP-241",
        "DAW-241",
        "RC-241",
        "C-241",
        "TRT-241",
        "DTTA-241",
        "TA-241",
        "TAP-241",
        "MSP-241",
        "UTIA-241",
        "TPM-241",
        "PAPP-231",
        "AAW-231",
        "RC-231",
        "C-231",
        "TRT-231",
        "DTTA-231",
        "TA-231",
        "TAP-231",
        "MSP-231",
        "UTIA-231",
        "TPM-231",
        "PAPP-221",
        "AAW-221",
        "RC-221",
        "C-221",
        "TRT-221",
        "DTTA-221",
        "TA-221",
        "TAP-221",
        "MSP-221",
        "UTIA-221",
        "TPM-221"
    };

    public static readonly ISet<string> Names = new HashSet<string>(GroupNames, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string groupName)
    {
        return groupName.Trim().ToUpperInvariant();
    }
}

public sealed class AllowedGroupAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string groupName || string.IsNullOrWhiteSpace(groupName))
        {
            return new ValidationResult("Group is required.");
        }

        var normalized = AllowedGroups.Normalize(groupName);
        if (!AllowedGroups.Names.Contains(normalized))
        {
            return new ValidationResult("Group must be one of the allowed values.");
        }

        return ValidationResult.Success;
    }
}

