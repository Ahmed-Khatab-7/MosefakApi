﻿namespace MosefakApp.Core.Dtos.User.Requests
{
    public class AddressUserRequest
    {
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
    }
}
