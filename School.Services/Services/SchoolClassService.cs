using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class SchoolClassService : ISchoolClassService
{
    private readonly ISchoolClassRepository _schoolClassRepository;

    public SchoolClassService(ISchoolClassRepository schoolClassRepository)
    {
        _schoolClassRepository = schoolClassRepository;
    }

    public async Task<List<SchoolClass>> GetAllAsync()
    {
        return await _schoolClassRepository.GetAllAsync();
    }

    public async Task<SchoolClass?> GetByIdAsync(int id)
    {
        return await _schoolClassRepository.GetByIdAsync(id);
    }

    public async Task AddAsync(SchoolClass schoolClass)
    {
        ValidateSchoolClass(schoolClass);
        await _schoolClassRepository.AddAsync(schoolClass);
    }

    public async Task UpdateAsync(SchoolClass schoolClass)
    {
        ValidateSchoolClass(schoolClass);
        await _schoolClassRepository.UpdateAsync(schoolClass);
    }

    public async Task DeleteAsync(int id)
    {
        await _schoolClassRepository.DeleteAsync(id);
    }

    private void ValidateSchoolClass(SchoolClass schoolClass)
    {
        if (string.IsNullOrWhiteSpace(schoolClass.Name))
        {
            throw new ArgumentException("Название класса обязательно для заполнения.");
        }
    }
}