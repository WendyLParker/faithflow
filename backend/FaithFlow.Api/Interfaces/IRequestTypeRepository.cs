namespace FaithFlow.Backend.Interfaces;

public interface IRequestTypeRepository
{
    Task<IReadOnlyList<Models.RequestType>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}
