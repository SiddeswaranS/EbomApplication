using EBOM.Core.Interfaces;
using EBOM.Core.Models;
using EBOM.Common.Models.Configuration;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace EBOM.Services.Implementations;

public class ValidationService : IValidationService
{
    private readonly EbomSettings _settings;

    public ValidationService(IOptions<EbomSettings> settings)
    {
        _settings = settings.Value;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ValidationResult> ValidateTemplateFileAsync(Stream fileStream, string fileName)
    {
        var result = new ValidationResult();

        try
        {
            // Validate file naming
            var namingResult = ValidateFileNaming(fileName);
            if (!namingResult.IsValid)
            {
                return namingResult;
            }

            // Load and validate Excel structure
            using var package = new ExcelPackage(fileStream);
            var sheetResult = ValidateSheetStructure(package);
            
            if (!sheetResult.IsValid)
            {
                return sheetResult;
            }

            // Validate configuration sheet
            var configSheet = package.Workbook.Worksheets["Configuration"];
            if (configSheet == null)
            {
                result.AddError("Configuration sheet is required");
                return result;
            }

            // Validate data sheets
            var dataSheets = package.Workbook.Worksheets
                .Where(ws => Regex.IsMatch(ws.Name, _settings.DataValidation.RequiredSheets.DataSheetPattern))
                .ToList();

            if (!dataSheets.Any())
            {
                result.AddError("At least one data sheet (DataXXofYY) is required");
            }

            // Validate each data sheet has headers
            foreach (var sheet in dataSheets)
            {
                if (sheet.Dimension == null || sheet.Dimension.Rows < 1)
                {
                    result.AddError($"Sheet '{sheet.Name}' is empty");
                    continue;
                }

                // Check for separator column
                var headerRow = 1;
                var hasSeperatorColumn = false;
                
                for (int col = 1; col <= sheet.Dimension.Columns; col++)
                {
                    var cellValue = sheet.Cells[headerRow, col].Value?.ToString();
                    if (cellValue == _settings.SeparatorColumn)
                    {
                        hasSeperatorColumn = true;
                        break;
                    }
                }

                if (!hasSeperatorColumn)
                {
                    result.AddWarning($"Sheet '{sheet.Name}' does not contain separator column '{_settings.SeparatorColumn}'");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            result.AddError($"Error validating file: {ex.Message}");
            return result;
        }
    }

    public async Task<ValidationResult> ValidateDataFileAsync(Stream fileStream, string fileName)
    {
        var result = new ValidationResult();

        try
        {
            // Basic file validation
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) && 
                !fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            {
                result.AddError("File must be in Excel format (.xlsx or .xls)");
                return result;
            }

            // Load Excel file
            using var package = new ExcelPackage(fileStream);
            
            if (package.Workbook.Worksheets.Count == 0)
            {
                result.AddError("Excel file contains no worksheets");
                return result;
            }

            // Validate data sheets
            var dataSheets = package.Workbook.Worksheets
                .Where(ws => Regex.IsMatch(ws.Name, @"^Data\d{2}of\d{2}$"))
                .ToList();

            if (!dataSheets.Any())
            {
                result.AddError("No valid data sheets found. Sheets must follow pattern: DataXXofYY");
                return result;
            }

            // Validate sheet sequence
            var sheetNumbers = dataSheets
                .Select(ws => 
                {
                    var match = Regex.Match(ws.Name, @"Data(\d{2})of(\d{2})");
                    return new { Current = int.Parse(match.Groups[1].Value), Total = int.Parse(match.Groups[2].Value) };
                })
                .ToList();

            var totalSheets = sheetNumbers.First().Total;
            
            if (sheetNumbers.Any(s => s.Total != totalSheets))
            {
                result.AddError("All data sheets must have the same total count");
                return result;
            }

            // Check for missing sheets in sequence
            for (int i = 1; i <= totalSheets; i++)
            {
                if (!sheetNumbers.Any(s => s.Current == i))
                {
                    result.AddError($"Missing sheet: Data{i:D2}of{totalSheets:D2}");
                }
            }

            // Validate each sheet has data
            foreach (var sheet in dataSheets)
            {
                if (sheet.Dimension == null || sheet.Dimension.Rows <= 1)
                {
                    result.AddError($"Sheet '{sheet.Name}' contains no data rows");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            result.AddError($"Error validating data file: {ex.Message}");
            return result;
        }
    }

    public ValidationResult ValidateFileNaming(string fileName)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(fileName))
        {
            result.AddError("File name is required");
            return result;
        }

        var extension = Path.GetExtension(fileName);
        if (!_settings.SupportedFileExtensions.Contains(extension.ToLowerInvariant()))
        {
            result.AddError($"Invalid file extension. Supported: {string.Join(", ", _settings.SupportedFileExtensions)}");
            return result;
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExtension.Split('_');

        if (parts.Length < 2)
        {
            result.AddError("File name must follow pattern: [EntityType]_[EntityName].xlsx");
            return result;
        }

        var validPrefixes = new[] { "ISR", "PSR", "CSR", "CMN" };
        if (!validPrefixes.Contains(parts[0]))
        {
            result.AddError($"Invalid entity type. Must be one of: {string.Join(", ", validPrefixes)}");
        }

        return result;
    }

    public ValidationResult ValidateSheetStructure(object excelPackage)
    {
        var result = new ValidationResult();
        
        if (excelPackage is not ExcelPackage package)
        {
            result.AddError("Invalid Excel package");
            return result;
        }

        if (package.Workbook.Worksheets.Count == 0)
        {
            result.AddError("Excel file contains no worksheets");
            return result;
        }

        // Check for Configuration sheet
        var hasConfigSheet = package.Workbook.Worksheets.Any(ws => 
            ws.Name.Equals(_settings.DataValidation.RequiredSheets.ConfigurationSheet, StringComparison.OrdinalIgnoreCase));

        if (!hasConfigSheet)
        {
            result.AddError($"Required sheet '{_settings.DataValidation.RequiredSheets.ConfigurationSheet}' not found");
        }

        // Check for at least one data sheet
        var hasDataSheet = package.Workbook.Worksheets.Any(ws => 
            Regex.IsMatch(ws.Name, _settings.DataValidation.RequiredSheets.DataSheetPattern));

        if (!hasDataSheet)
        {
            result.AddError("At least one data sheet (DataXXofYY) is required");
        }

        return result;
    }
}