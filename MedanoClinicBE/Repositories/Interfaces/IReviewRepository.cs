using MedanoClinicBE.DTOs;

namespace MedanoClinicBE.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string clientId);
        Task<List<ReviewDto>> GetAllReviewsAsync();
    }
}