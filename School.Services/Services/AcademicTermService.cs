using Microsoft.Extensions.Logging;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Models;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class AcademicTermService : IAcademicTermService
{
    private const string OverlapMessage = "Период пересекается с другим учебным периодом.";
    private const string ClosedTermDeleteMessage = "Закрытый период нельзя удалить. Сначала откройте его.";
    private const string TermWithGradesMessage = "Нельзя удалить период, в котором есть оценки.";

    private readonly IAcademicTermRepository _termRepository;
    private readonly ILogger<AcademicTermService> _logger;

    public AcademicTermService(IAcademicTermRepository termRepository, ILogger<AcademicTermService> logger)
    {
        _termRepository = termRepository;
        _logger = logger;
    }

    public async Task<List<AcademicTermSummary>> GetAllAsync()
    {
        return await _termRepository.GetAllWithGradeCountsAsync();
    }

    public async Task AddAsync(AcademicTerm term)
    {
        Normalize(term);
        EntityValidator.Validate(term);
        await EnsureNameIsFreeAsync(term.Name, null);
        await EnsureNoOverlapAsync(term, null);

        try
        {
            await _termRepository.AddAsync(term);
        }
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateNameMessage(term.Name), ex);
        }

        _logger.LogInformation("Academic term added: {Name} (Id: {Id})", term.Name, term.Id);
    }

    public async Task<bool> UpdateAsync(AcademicTerm term)
    {
        Normalize(term);
        EntityValidator.Validate(term);

        var existing = await _termRepository.GetByIdAsync(term.Id);
        if (existing == null)
        {
            return false;
        }

        if (existing.IsClosed)
        {
            throw new ArgumentException($"Период «{existing.Name}» закрыт: его можно только открыть.");
        }

        await EnsureNameIsFreeAsync(term.Name, term.Id);
        await EnsureNoOverlapAsync(term, term.Id);

        bool updated;
        try
        {
            updated = await _termRepository.UpdateAsync(term);
        }
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateNameMessage(term.Name), ex);
        }

        if (updated)
        {
            _logger.LogInformation("Academic term updated: Id {Id}", term.Id);
        }
        return updated;
    }

    public async Task<bool> SetClosedAsync(int id, bool isClosed)
    {
        var updated = await _termRepository.SetClosedAsync(id, isClosed);
        if (updated)
        {
            _logger.LogInformation("Academic term {Id} closed: {IsClosed}", id, isClosed);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _termRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return false;
        }

        if (existing.IsClosed)
        {
            throw new ArgumentException(ClosedTermDeleteMessage);
        }

        if (await _termRepository.HasGradesAsync(existing.StartDate, existing.EndDate))
        {
            throw new ArgumentException(TermWithGradesMessage);
        }

        var deleted = await _termRepository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("Academic term deleted: Id {Id}", id);
        }
        return deleted;
    }

    private static void Normalize(AcademicTerm term)
    {
        term.Name = (term.Name ?? string.Empty).Trim();
        term.StartDate = term.StartDate.Date;
        term.EndDate = term.EndDate.Date;
    }

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId)
    {
        if (await _termRepository.ExistsByNameAsync(name, excludeId))
        {
            throw new ArgumentException(DuplicateNameMessage(name));
        }
    }

    private async Task EnsureNoOverlapAsync(AcademicTerm term, int? excludeId)
    {
        if (await _termRepository.HasOverlapAsync(term.StartDate, term.EndDate, excludeId))
        {
            throw new ArgumentException(OverlapMessage);
        }
    }

    private static string DuplicateNameMessage(string name) => $"Период с названием \"{name}\" уже существует.";
}
