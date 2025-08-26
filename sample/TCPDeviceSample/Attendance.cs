namespace TCPDeviceSample
{
    public class Attendance
    {
        public required string CardId { get; set; }
        public DateTime Date { get; set; }
        public required string DeviceCode { get; set; }
    }
}
