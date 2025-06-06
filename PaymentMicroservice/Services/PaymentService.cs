using Google.Cloud.Firestore;
using PaymentMicroservice.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

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

            string url = "https://taxi-fare-calculator.p.rapidapi.com/search-geo?dep_lat=52.50&dep_lng=13.43&arr_lat=52.47&arr_lng=13.63";
            var response = await client.GetAsync(url);
            var raw = await response.Content.ReadAsStringAsync();

            Console.WriteLine("----- RAW JSON RESPONSE -----");
            Console.WriteLine(raw);

            // Step 2: Check if it's double-encoded
            if (raw.StartsWith("\"") && raw.Contains("\\"))
            {
                raw = JsonSerializer.Deserialize<string>(raw); // Unwrap the outer quotes
            }

            Console.WriteLine("Unwrapped JSON:");
            Console.WriteLine(raw);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API error: {response.StatusCode} - {raw}");

            using var doc = JsonDocument.Parse(raw);

            JsonElement root = doc.RootElement;
            JsonElement journeyElement;
            JsonElement faresArray = default;

            if (!root.TryGetProperty("journey", out journeyElement))
            {
                Console.WriteLine("❌ 'journey' property not found.");
                throw new Exception("No 'journey' property found in root.");
            }

            if (!journeyElement.TryGetProperty("fares", out faresArray) || faresArray.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine("❌ 'fares' array not found inside 'journey'.");
                throw new Exception("No 'fares' array found inside 'journey'.");
            }

            Console.WriteLine("✅ fares array found inside journey.");

            foreach (var fare in faresArray.EnumerateArray())
            {
                Console.WriteLine("--- Fare entry found ---");
                Console.WriteLine(fare.ToString());

                if (fare.TryGetProperty("price_in_cents", out JsonElement priceElement))
                {
                    Console.WriteLine("price_in_cents raw type: " + priceElement.ValueKind);
                    Console.WriteLine("price_in_cents value: " + priceElement.ToString());

                    if (priceElement.ValueKind == JsonValueKind.Number && priceElement.TryGetInt32(out int cents))
                    {
                        Console.WriteLine($"✅ Parsed numeric fare: {cents}");
                        return cents / 100.0;
                    }

                    if (priceElement.ValueKind == JsonValueKind.String && int.TryParse(priceElement.GetString(), out cents))
                    {
                        Console.WriteLine($"✅ Parsed string fare: {cents}");
                        return cents / 100.0;
                    }
                }
                else
                {
                    Console.WriteLine("❌ price_in_cents not found in this fare.");
                }
            }

            Console.WriteLine("❌ No valid cab fare found in parsed JSON.");
            throw new Exception("No valid cab fare found in API response.");
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


        //gets pasyment details from db

        public async Task<Payment?> GetPaymentDetailsById(string paymentId)
        {
            DocumentReference docRef = _db.Collection("payments").Document(paymentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            return snapshot.ConvertTo<Payment>();
        }
    }
}
