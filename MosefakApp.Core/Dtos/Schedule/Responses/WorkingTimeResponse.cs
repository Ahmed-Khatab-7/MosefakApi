using System.Text.Json.Serialization;

namespace MosefakApp.Core.Dtos.Schedule.Responses
{
    public class WorkingTimeResponse
    {
        public string Id { get; set; } = null!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeek Day { get; set; }
        public List<PeriodResponse> Periods { get; set; } = new List<PeriodResponse>();
        public bool IsAvailable { get; set; }
    }
}
