namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactUsController : ControllerBase
    {
        private readonly IContactUsService _contactUsService;

        public ContactUsController(IContactUsService contactUsService)
        {
            _contactUsService = contactUsService;
        }

        [HttpGet("messages")]
        [RequiredPermission(Permissions.Contacts.View)] // for admin only
        public async Task<ActionResult<PaginatedResponse<ContactUsResponse>>> GetMessages(int pageNumber = 1, int pageSize = 10)
        {
            var query = await _contactUsService.GetMessages(pageNumber, pageSize);

            return Ok(query);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<bool>> CreateContactMessage(ContactUsRequest request)
        {
            int userId = User.GetUserId();

            var query = await _contactUsService.CreateContactMessage(userId, request);

            return Ok(query);
        }
    }
}
