using EBOM.Core.Models;

namespace EBOM.Core.Interfaces;

public interface IValidationService
{
    Task<ValidationResult> ValidateTemplateFileAsync(Stream fileStream, string fileName);
    Task<ValidationResult> ValidateDataFileAsync(Stream fileStream, string fileName);
    ValidationResult ValidateFileNaming(string fileName);
    ValidationResult ValidateSheetStructure(object excelPackage); // Using object to avoid EPPlus dependency in Core
}