namespace MosefakApp.Core.Dtos.Doctor.Requests
{
    public class DoctorUnifiedSearchFilter
    {
        public string? Name { get; set; }
        public SpecialtyCategory? SpecialtyCategory { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; }
        public string? SortBy { get; set; } // "rating", "price", etc.
        public bool SortDescending { get; set; } = false;
    }
}
