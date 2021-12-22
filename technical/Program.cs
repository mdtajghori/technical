using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace technical
{
    class MainClass
    {
        enum Tables
        {
            A,
            B,
            C
        }

        class Interval
        {
            public int Start { get; set; }
            public int End { get; set; }
        }

        public static void Main(string[] args)
        {

            //input file path here
            List<ReservationData> reservationData = ParseFile(@"/users/mdtajghori/Downloads/interview_data.json");

            //We have 12 total seats across the tables
            Dictionary<int, List<Interval>> seatToTimeMap = new Dictionary<int, List<Interval>>();
            Dictionary<int, Tables> seatToTableMap = new Dictionary<int, Tables>();
            for (int i = 1; i <= 12; i++)
            {
                seatToTimeMap.Add(i, new List<Interval>());
                seatToTableMap.Add(i, GetTableForSeat(i));
            }

            //sort based on received time
            SortReservationData(reservationData);

            Dictionary<int, bool> result = new Dictionary<int, bool>();

            foreach(ReservationData reservation in reservationData)
            {
                result.Add(reservation.requestID, ProcessReservation(reservation, seatToTimeMap, seatToTableMap));
            }

            foreach(int key in result.Keys)
            {
                Console.WriteLine($"ReservationId:{key} - Available:{result[key]}");
            }

            
        }

        /// <summary>
        /// Parse the file to retrieve the reservation data
        /// </summary>
        /// <param name="filePath">path to the file with reservation data</param>
        /// <returns></returns>
        public static List<ReservationData> ParseFile(string filePath)
        {
            string jsonContent = File.ReadAllText(filePath);
            List<ReservationData> reservationData = JsonConvert.DeserializeObject<List<ReservationData>>(jsonContent);
            
            return reservationData;
        }

        /// <summary>
        /// Sort the reservation data based on the received time, and then by number of people to maximize the tables we can have filled
        /// </summary>
        /// <param name="reservationData"></param>
        public static void SortReservationData(List<ReservationData> reservationData)
        {
            reservationData.Sort((x, y) =>
            {
                int compareResult = x.receivedTime.CompareTo(y.receivedTime);
                return compareResult != 0 ? compareResult : x.numberOfPeople.CompareTo(y.numberOfPeople);
            });
        }

        private static bool ProcessReservation(ReservationData reservation, Dictionary<int, List<Interval>> seatToTimeMap, Dictionary<int, Tables> seatToTableMap)
        {
            for (int i = 1; i <= 12; i++)
            {
                //look for consequitive seats being available
                for (int j = i; j < i + reservation.numberOfPeople; j++)
                {
                    //no more seats available
                    if (j > 12)
                    {
                        return false;
                    }

                    if (!IsSeatAvailable(reservation, seatToTimeMap, j))
                    {
                        break;
                    }
                    else
                    {
                        //we have consequitive seats on the same table
                        if (j == i + reservation.numberOfPeople - 1 && seatToTableMap[i] == seatToTableMap[j])
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }

        private static bool IsSeatAvailable(ReservationData reservation, Dictionary<int, List<Interval>> seatToTimeMap, int seat)
        {
            //if the seat is not reserved then it is available
            if (seatToTimeMap[seat].Count == 0)
            {
                seatToTimeMap[seat].Add(new Interval
                {
                    End = reservation.startTime + reservation.duration,
                    Start = reservation.startTime

                });
                return true;
            }

            bool available = false;

            //sort based on the start time of the seat being reserved
            seatToTimeMap[seat].Sort((x, y) =>
            {
                return x.Start.CompareTo(y.Start);
            });

            //look through the intervals where the seat is reserved and see if it is available for the requested reservation
            foreach(Interval interval in seatToTimeMap[seat])
            {
                if (interval.Start <= reservation.startTime && interval.End > reservation.startTime)
                {
                    available = available || false;
                }
                else if (interval.Start >= reservation.startTime + reservation.duration)
                {
                    available = available || true;
                }

                if(available)
                {
                    seatToTimeMap[seat].Add(new Interval
                    {
                        End = reservation.startTime + reservation.duration,
                        Start = reservation.startTime

                    });
                    break;
                }
            }

            return available;
        }

        private static Tables GetTableForSeat(int seat)
        {
            if (seat <= 4)
            {
                return Tables.A;
            }
            else if (seat <= 8)
            {
                return Tables.B;
            }
            else
            {
                return Tables.C;
            }
        }
    }
}
