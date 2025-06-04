using Google.Cloud.Firestore;
using PaymentMicroservice.Models;
using System.Security.Cryptography.X509Certificates;

namespace PaymentMicroservice.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _configuration;

        private readonly FirestoreDb _db;
        

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            _db = FirestoreDb.Create("festive-athlete-423809-g7"); 
        }

        
        public async Task<double> GetCabFareAsync(string pickup, string dropoff)
        {
            var apiKey = _configuration["RapidApi:Key"];
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "taxi-fare-calculator.p.rapidapi.com");

            string url = $"https://taxi-fare-calculator.p.rapidapi.com/taxi-fare?pickup={pickup}&dropoff={dropoff}";

            var response = await client.GetFromJsonAsync<Dictionary<string, double>>(url);

            if (response != null && response.TryGetValue("cab_fare", out double fare))
                return fare;

            return -1; // or throw exception
        }

        public async Task<double> CalculateFarePriceAsync(string cabType, int passengers, DateTime dateTime, string startLocation, string endLocation)
        {
            double baseFare = await GetCabFareAsync(startLocation, endLocation);

            //cab_multiplier
            double cabMultiplier = cabType.ToLower() switch
            {
                "Economic" => 1.0,
                "premium" => 1.2,
                "executive" => 1.4
            };

            //daytime_mlutiplier
            double dayTimeMultiplier = (dateTime.Hour >= 0 && dateTime.Hour < 8) ? 1.2 : 1.0;

            //passenger_multiplier
            double passengerMultiplier = passengers <= 4 ? 1 : 2;

            return baseFare * cabMultiplier * passengerMultiplier * dayTimeMultiplier;
        }

        //creates collection in db and saves payment
        public async Task<string> SavePaymentAsync(Payment payment)
        {
            payment.Id = Guid.NewGuid().ToString();
            await _db.Collection("payments").Document(payment.Id).SetAsync(payment);
            return payment.Id;
        }
    }
}
