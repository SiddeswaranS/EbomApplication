using EBOM.Core.Models;

namespace EBOM.Core.Interfaces;

public interface ITemplateProcessor
{
    Task<ProcessingResult> ProcessTemplateAsync(Stream fileStream, string fileName, int userId);
    Task<List<TemplateInfo>> GetActiveTemplatesAsync();
    Task<TemplateInfo?> GetTemplateByEntityAsync(string entityName);
}