namespace MosefakApp.Core.AutoMapper
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            // User

            CreateMap<AppUser, UserProfileResponse>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom<UserProfileImageResolver>())
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<Address, AddressUserResponse>().ReverseMap();

            // Doctor

            CreateMap<Doctor,CompleteDoctorProfileRequest>().ReverseMap();

            CreateMap<DoctorRequest, Doctor>()
                .ForMember(dest => dest.NumberOfReviews, opt => opt.MapFrom(src => 0)); // Default value for NumberOfReviews

            CreateMap<AppointmentTypeRequest, AppointmentType>().ReverseMap();
            CreateMap<SpecializationRequest, Specialization>().ReverseMap();
            CreateMap<WorkingTimeRequest, WorkingTime>().ReverseMap();


            CreateMap<CompleteDoctorProfileRequest, Doctor>()
                .ForMember(dest => dest.NumberOfReviews, opt => opt.MapFrom(src => 0));

            // Clinic
            
            CreateMap<ClinicRequest, Clinic>().ReverseMap();
            CreateMap<Clinic, ClinicResponse>().ReverseMap();

            // working times

            CreateMap<WorkingTimeRequest, WorkingTime>().ReverseMap();
            CreateMap<WorkingTime, WorkingTimeResponse>().ReverseMap();

            // Period

            CreateMap<PeriodRequest, Period>().ReverseMap();
            CreateMap<Period, PeriodResponse>().ReverseMap();

            // Review

            CreateMap<ReviewRequest, Review>().ReverseMap();
            CreateMap<Review, ReviewResponse>().ReverseMap();

            // appointment

            CreateMap<Appointment, AppointmentResponse>().ReverseMap();

            CreateMap<Appointment, AppointmentDto>()
                .ReverseMap();

            // appointment type

            CreateMap<AppointmentType, AppointmentTypeResponse>().ReverseMap();
            CreateMap<AppointmentTypeRequest, AppointmentType>().ReverseMap();

            CreateMap<Specialization, SpecializationResponse>().ReverseMap();
            CreateMap<SpecializationRequest, Specialization>().ReverseMap();

            CreateMap<Award, AwardResponse>().ReverseMap();
            CreateMap<AwardRequest, Award>().ReverseMap();

            CreateMap<Experience, ExperienceResponse>().ReverseMap();
            CreateMap<ExperienceRequest, Experience>().ReverseMap();

            CreateMap<Education, EducationResponse>().ReverseMap();
            CreateMap<EducationRequest, Education>().ReverseMap();

            CreateMap<AppUser,PatientViewModel>().ReverseMap();
        }
    }
}
