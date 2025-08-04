using EBOM.Core.Models;
using OfficeOpenXml;

namespace EBOM.Core.Interfaces;

public interface IValidationService
{
    Task<ValidationResult> ValidateTemplateFileAsync(Stream fileStream, string fileName);
    Task<ValidationResult> ValidateDataFileAsync(Stream fileStream, string fileName);
    ValidationResult ValidateFileNaming(string fileName);
    ValidationResult ValidateSheetStructure(ExcelPackage package);
}