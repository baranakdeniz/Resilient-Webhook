# Resilient Webhook API

This project is a production-grade .NET Minimal API designed to securely, efficiently, and seamlessly ingest webhook requests from external systems (e.g., Stripe, GitHub). 

Unlike a standard HTTP Post endpoint, this architecture is engineered to enterprise standards to prevent system crashes during sudden traffic spikes and to block malicious requests at the gateway.

## Core Architecture & Features

* **Cryptographic Security (HMAC-SHA256):** Validates incoming payloads using a secret key to ensure data has not been tampered with in transit. Utilizes `CryptographicOperations.FixedTimeEquals` to prevent timing attacks.
* **Replay Attack Protection:** Verifies the timestamp in the request headers, rejecting delayed or duplicated requests (with a 5-minute tolerance) to thwart attackers listening to network traffic.
* **True "Zero-Allocation" Performance:** Parses massive JSON payloads directly via the ASP.NET Core pipeline (`PipeReader`) and `Utf8JsonReader` with zero memory allocation, avoiding unnecessary `string` allocations in RAM.
* **Asynchronous Queue (System.Threading.Channels):** The API gateway does not wait for database operations. Validated requests are immediately pushed to an in-memory queue (Channel), returning a `200 OK` to the external system instantly.
* **Background Worker:** Queued items are processed asynchronously by an integrated `IHostedService` (BackgroundWorker) without blocking the main API traffic.

## Roadmap (Coming Soon)

* **Idempotency:** A database-level protection layer to prevent processing the same webhook event (e.g., a payment confirmation) twice in the event of network retries from the external provider.

## Setup & Configuration

Before running the project, you must configure your secret key for encryption and signature validation in `appsettings.json` (or via User Secrets / Environment Variables):

```json
{
  "WebhookSecret": "your-super-secret-key-goes-here"
}
