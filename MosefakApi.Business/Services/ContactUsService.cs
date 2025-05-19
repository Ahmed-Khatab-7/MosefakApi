namespace MosefakApi.Business.Services
{
    public class ContactUsService : IContactUsService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        public ContactUsService(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<bool> CreateContactMessage(int userId, ContactUsRequest request)
        {
            var user = await _userManager.Users.Where(x => x.Id == userId)
                                               .Select(x => new { x.FirstName, x.LastName, x.Email })
                                               .FirstOrDefaultAsync();

            if (user == null)
                throw new ItemNotFound("user does not exist");

            var contact = new ContactUs
            {
                AppUserId = userId,
                Message = request.Message,
            };

            await _unitOfWork.Repository<ContactUs>().AddEntityAsync(contact);
            // Save changes to the database
            var isSaved = await _unitOfWork.CommitAsync() > 0;
            if (!isSaved)
                return false;

            // Prepare email body
            var emailBody = await _emailBodyBuilder.GenerateEmailBody(
                templateName: "ContactUsTemplate.html",
                imageUrl: "https://yourdomain.com/logo.png",
                header: "New Contact Message Received",
                TextBody: $"Hello {user.FirstName},\n\nYour message has been received. We will contact you shortly."
            );

            // Fire-and-forget email sending to avoid blocking the API response
            _ = Task.Run(() => _emailSender.SendEmailAsync(user.Email, subject: "Your Contact Request", emailBody));

            return true;
        }

        public async Task<PaginatedResponse<ContactUsResponse>> GetMessages(int pageNumber, int pageSize)
        {
            (var query, var totalCount) = await _unitOfWork.Repository<ContactUs>()
                                                           .GetAllAsync(pageNumber, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (!query.Any())
                return new PaginatedResponse<ContactUsResponse>
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Data = new List<ContactUsResponse>()
                };

            var userIds = query.Select(x => x.AppUserId).ToHashSet();

            // Fetch users in a single query and convert to a dictionary for fast lookup
            var usersDict = await _userManager.Users
                .Where(x => userIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email })
                .ToDictionaryAsync(x => x.Id);

            // Convert query result to response model efficiently
            var responseList = query
                .Where(item => usersDict.ContainsKey(item.AppUserId)) // Ensure user exists
                .Select(item => new ContactUsResponse
                {
                    Id = item.Id.ToString(),
                    CreatedAt = item.CreatedAt,
                    Message = item.Message,
                    Name = $"{usersDict[item.AppUserId].FirstName} {usersDict[item.AppUserId].LastName}",
                    Email = usersDict[item.AppUserId].Email
                })
                .ToList();

            return new PaginatedResponse<ContactUsResponse>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = responseList
            };
        }
    }
}
