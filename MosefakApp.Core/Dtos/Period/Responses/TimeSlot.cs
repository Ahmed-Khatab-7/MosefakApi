namespace MosefakApp.Core.Dtos.Period.Responses
{
    public class TimeSlot
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        //// Optional: Add a property to store the full DateTime if needed
        //public DateTime StartDateTime { get; set; }
        //public DateTime EndDateTime { get; set; }
    }
}
