using System.Text.Json.Serialization;
using MosefakApp.Domains.Enums;

namespace MosefakApp.Core.Dtos.Specialization.Responses
{
    public class SpecializationResponse
    {
        public string Id { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Specialty Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpecialtyCategory Category { get; set; }
    }
}