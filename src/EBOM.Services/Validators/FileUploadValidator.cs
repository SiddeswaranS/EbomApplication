using FluentValidation;
using Microsoft.Extensions.Options;
using EBOM.Common.Models.Configuration;

namespace EBOM.Services.Validators;

public class FileUploadValidator : AbstractValidator<FileUploadRequest>
{
    private readonly EbomSettings _settings;

    public FileUploadValidator(IOptions<EbomSettings> settings)
    {
        _settings = settings.Value;

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .Must(BeValidFileName).WithMessage("File name contains invalid characters")
            .Must(HaveValidExtension).WithMessage($"File must have one of these extensions: {string.Join(", ", _settings.SupportedFileExtensions)}");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(_settings.MaxFileSize).WithMessage($"File size must not exceed {_settings.MaxFileSize / 1024 / 1024}MB");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required");
    }

    private bool BeValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        return !fileName.Any(ch => invalidChars.Contains(ch));
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _settings.SupportedFileExtensions.Contains(extension);
    }
}

public class FileUploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Stream FileStream { get; set; } = Stream.Null;
}

public class TemplateFileValidator : AbstractValidator<TemplateFileRequest>
{
    private readonly string[] _validPrefixes = { "ISR", "PSR", "CSR", "CMN" };

    public TemplateFileValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .Must(HaveValidNamingConvention).WithMessage("File name must follow pattern: [EntityType]_[EntityName].xlsx");

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .Must(x => _validPrefixes.Contains(x)).WithMessage("Entity type must be one of: ISR, PSR, CSR, CMN");

        RuleFor(x => x.EntityName)
            .NotEmpty().WithMessage("Entity name is required")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Entity name can only contain letters, numbers, and underscores");
    }

    private bool HaveValidNamingConvention(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExtension.Split('_');

        return parts.Length >= 2 && _validPrefixes.Contains(parts[0]);
    }
}

public class TemplateFileRequest : FileUploadRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
}

public class DataFileValidator : AbstractValidator<DataFileRequest>
{
    public DataFileValidator()
    {
        RuleFor(x => x.EntityId)
            .GreaterThan(0).WithMessage("Valid entity ID is required");

        RuleFor(x => x.DataSheets)
            .NotEmpty().WithMessage("At least one data sheet is required")
            .Must(HaveValidDataSheetNaming).WithMessage("Data sheets must follow naming pattern: DataXXofYY");
    }

    private bool HaveValidDataSheetNaming(List<string> sheetNames)
    {
        if (sheetNames == null || !sheetNames.Any())
            return false;

        var pattern = @"^Data\d{2}of\d{2}$";
        return sheetNames.All(name => System.Text.RegularExpressions.Regex.IsMatch(name, pattern));
    }
}

public class DataFileRequest : FileUploadRequest
{
    public int EntityId { get; set; }
    public List<string> DataSheets { get; set; } = new();
}