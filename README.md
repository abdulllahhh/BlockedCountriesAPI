🧾 Blocked Countries API

A .NET 8 Web API to manage blocked countries, check IPs via third-party geolocation services, and log blocked access attempts — all using in-memory storage.

🚀 Setup Instructions
1️⃣ Clone the repository
git clone https://github.com/<your-username>/BlockedCountriesApi.git
cd BlockedCountriesApi

2️⃣ Configure API Key

Edit appsettings.json:

{
  "GeoApi": {
    "BaseUrl": "https://ipapi.co",
    "ApiKey": "<YOUR_API_KEY>"
  }
}

3️⃣ Run the app
dotnet run


Then open:
👉 https://localhost:5001/swagger
 for interactive API docs.

🧩 Endpoints Overview
Feature	Method	Endpoint	Description
Add Blocked Country	POST	/api/countries/block	Add a country to permanent block list
Delete Blocked Country	DELETE	/api/countries/block/{countryCode}	Remove blocked country
Get Blocked Countries	GET	/api/countries/blocked	List all blocked countries (with pagination & search)
IP Lookup	GET	/api/ip/lookup?ipAddress={ip}	Get geolocation info for IP
Check IP Block	GET	/api/ip/check-block	Check if caller’s IP is blocked
Get Blocked Attempts	GET	/api/logs/blocked-attempts	Paginated list of blocked IP attempts
Temporary Block	POST	/api/countries/temporal-block	Temporarily block a country for N minutes
🧠 Features

✅ Uses HttpClient + async/await for geolocation API
✅ Thread-safe in-memory storage using ConcurrentDictionary
✅ Background cleanup service for expired temporary blocks
✅ Swagger documentation enabled by default
✅ separate controllers, services, DTOs


🧩 Technologies Used

.NET 8 (ASP.NET Core Web API)

HttpClient (Microsoft.Extensions.Http)

Newtonsoft.Json

Swagger (Swashbuckle)

C# 12

👨‍💻 Author

Name: Abdullah Ibrahim Ahmed

Email: abdullahibrahim.business@gmail.com

GitHub: github.com/abdulllahhh
