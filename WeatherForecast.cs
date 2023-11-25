namespace CSharpServer
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
    public class TransactionModel
    {
        public int Step { get; set; }

        public string ?Customer { get; set; }
        public string ?Age { get; set; }
        public string ?Gender { get; set; }
        public string ?ZipcodeOri { get; set; }
        public string ?Merchant { get; set; }
        public string ?ZipMerchant { get; set; }
        public string ?Category { get; set; }
        public float Amount { get; set; }
    }
    public class Transaction
    {
        public int Step { get; set; }

        public string? Customer { get; set; }
        public string? Age { get; set; }
        public string? Gender { get; set; }
        public string? ZipcodeOri { get; set; }
        public string? Merchant { get; set; }
        public string? ZipMerchant { get; set; }
        public string? Category { get; set; }
        public float Amount { get; set; }
    }
}