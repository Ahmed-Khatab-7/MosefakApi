namespace MosefakApp.Core.IRepositories.Non_Generic
{
    public interface IDoctorRepositoryAsync : IGenericRepositoryAsync<Doctor>
    {
        Task<List<DoctorResponse>?> GetTopTenDoctors();
        Task<Doctor> GetDoctorById(int doctorId);
        Task<Dictionary<int, (string FullName, string ImagePath)>> GetUserDetailsAsync(IEnumerable<int> appUserIds);
        Task<Dictionary<int, (string FullName, string ImagePath)>> GetUserDetailsAsync(int appUserIds);
        Task<DoctorProfileResponse> GetDoctorProfile(int appUserIdFromClaims);
        Task<DoctorEarningsResponse> GetEarningsReportAsync(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate);
        Task<PaginatedResponse<DoctorResponse>> SearchDoctorsAsync(DoctorUnifiedSearchFilter filter, int pageNumber = 1, int pageSize = 10);
    }
}
