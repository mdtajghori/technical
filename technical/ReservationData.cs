using System;
namespace technical
{
    public class ReservationData
    {
        public ReservationData()
        {
        }

        public int requestID { get; set; }

        public int receivedTime { get; set; }

        public int startTime { get; set; }

        public int duration { get; set; }

        public int numberOfPeople { get; set; }
    }
}
