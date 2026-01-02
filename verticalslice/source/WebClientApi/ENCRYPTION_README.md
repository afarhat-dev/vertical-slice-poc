# API Encryption Feature

## Overview

This implementation provides end-to-end encryption for REST API requests and responses using AES-256 encryption. All API endpoints in the Movies and Rentals controllers are automatically encrypted.

## Architecture

### Components

1. **IEncryptionService** - Interface for encryption operations
2. **AesEncryptionService** - Implementation using AES-256-CBC encryption
3. **EncryptedApiAttribute** - Action filter that handles encryption/decryption automatically
4. **EncryptedRequest/EncryptedResponse** - Wrapper classes for encrypted data transfer

### How It Works

#### Request Flow (Client → Server)
1. Client serializes request payload to JSON
2. Client encrypts JSON using shared AES key
3. Client wraps encrypted data in `EncryptedRequest` format:
   ```json
   {
     "encryptedData": "base64_encoded_encrypted_json"
   }
   ```
4. Server receives encrypted request
5. `EncryptedApiAttribute` intercepts the request
6. Filter decrypts the data using `AesEncryptionService`
7. Filter deserializes JSON and binds to action parameter
8. Controller action executes normally

#### Response Flow (Server → Client)
1. Controller action returns result
2. `EncryptedApiAttribute` intercepts the response
3. Filter serializes response to JSON
4. Filter encrypts JSON using `AesEncryptionService`
5. Filter wraps in `EncryptedResponse` format:
   ```json
   {
     "encryptedData": "base64_encoded_encrypted_json"
   }
   ```
6. Client receives encrypted response
7. Client decrypts using shared AES key
8. Client deserializes JSON

## Configuration

### appsettings.json

```json
{
  "Encryption": {
    "Key": "base64_encoded_256bit_key",
    "IV": "base64_encoded_128bit_iv"
  }
}
```

**Important Security Notes:**
- The encryption key must be 256 bits (32 bytes) - Base64 encoded
- The IV must be 128 bits (16 bytes) - Base64 encoded
- In production, store these values securely:
  - Use Azure Key Vault, AWS Secrets Manager, or similar
  - Use environment variables
  - Never commit production keys to source control
  - Rotate keys regularly

### Generating New Keys

To generate new encryption keys, use the following Python script:

```python
import os
import base64

# Generate 256-bit key for AES-256
key = base64.b64encode(os.urandom(32)).decode()
print(f"Key: {key}")

# Generate 128-bit IV
iv = base64.b64encode(os.urandom(16)).decode()
print(f"IV: {iv}")
```

Or use this bash one-liner:
```bash
python3 -c "import os, base64; print('Key:', base64.b64encode(os.urandom(32)).decode()); print('IV:', base64.b64encode(os.urandom(16)).decode())"
```

## Usage

### Applying Encryption to Controllers

The `[EncryptedApi]` attribute is applied at the controller level:

```csharp
[ApiController]
[Route("api/[controller]")]
[EncryptedApi]  // This encrypts all actions in this controller
public class MoviesController : ControllerBase
{
    // All actions are automatically encrypted
}
```

You can also apply it to individual actions:

```csharp
[HttpPost]
[EncryptedApi]  // Only this action is encrypted
public async Task<ActionResult> SensitiveOperation()
{
    // ...
}
```

### Client Implementation Example

Here's how a client would interact with the encrypted API:

```csharp
// C# Client Example
public class EncryptedApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IEncryptionService _encryptionService;

    public async Task<MovieDto> CreateMovie(AddMovieRequest request)
    {
        // 1. Serialize request to JSON
        var requestJson = JsonSerializer.Serialize(request);

        // 2. Encrypt the JSON
        var encryptedData = _encryptionService.Encrypt(requestJson);

        // 3. Wrap in EncryptedRequest
        var encryptedRequest = new EncryptedRequest
        {
            EncryptedData = encryptedData
        };

        // 4. Send to API
        var response = await _httpClient.PostAsJsonAsync(
            "api/movies",
            encryptedRequest
        );

        // 5. Get encrypted response
        var encryptedResponse = await response.Content
            .ReadFromJsonAsync<EncryptedResponse>();

        // 6. Decrypt response
        var responseJson = _encryptionService.Decrypt(
            encryptedResponse.EncryptedData
        );

        // 7. Deserialize to result
        var result = JsonSerializer.Deserialize<MovieDto>(responseJson);

        return result;
    }
}
```

## Security Considerations

1. **Transport Security**: Always use HTTPS in production. Encryption adds defense-in-depth but doesn't replace TLS.

2. **Key Management**:
   - Never hardcode keys in source code
   - Use secure key storage (Key Vault, Secrets Manager)
   - Implement key rotation policies
   - Use different keys for different environments

3. **IV Handling**:
   - Current implementation uses a static IV for simplicity
   - For enhanced security, consider using a random IV per request
   - If using random IVs, prepend IV to encrypted data

4. **GET Requests**:
   - GET requests are not encrypted (query parameters)
   - Avoid sending sensitive data in GET requests
   - Use POST for sensitive data transfer

5. **Error Handling**:
   - Decryption failures return generic error messages
   - Detailed encryption errors are logged server-side only
   - Prevents information leakage through error messages

## Testing

### Manual Testing with Postman

1. Create a request body:
```json
{
  "title": "The Matrix",
  "director": "Wachowski Sisters",
  "releaseYear": 1999,
  "genre": "Sci-Fi",
  "rating": 8.7,
  "description": "A computer hacker learns about the true nature of reality"
}
```

2. Encrypt this JSON using the encryption key/IV
3. Wrap in EncryptedRequest format
4. Send to endpoint
5. Decrypt the response

### Integration Testing

Create integration tests that verify encryption/decryption:

```csharp
[Fact]
public async Task CreateMovie_WithEncryption_ReturnsEncryptedResponse()
{
    // Arrange
    var request = new AddMovieRequest(...);
    var encryptedRequest = EncryptRequest(request);

    // Act
    var response = await _client.PostAsync("api/movies", encryptedRequest);

    // Assert
    var encryptedResponse = await response.Content
        .ReadFromJsonAsync<EncryptedResponse>();
    var decryptedResult = DecryptResponse<AddResult>(encryptedResponse);

    Assert.NotNull(decryptedResult.Id);
}
```

## Performance Considerations

- Encryption/decryption adds latency (~1-5ms per request)
- Consider caching encrypted responses for read-heavy endpoints
- Monitor CPU usage for encryption operations
- For high-throughput scenarios, consider:
  - Hardware-accelerated AES (AES-NI)
  - Async encryption operations
  - Response caching

## Troubleshooting

### "Invalid encrypted request" Error
- Verify the encryption key and IV match between client and server
- Ensure the request is properly formatted as `EncryptedRequest`
- Check that the encrypted data is valid Base64

### "Failed to decrypt request" Error
- Key/IV mismatch between client and server
- Data corruption during transmission
- Incorrect encryption algorithm or mode

### Build Errors
- Ensure all required NuGet packages are installed
- Verify namespace imports in controllers
- Check that encryption configuration exists in appsettings.json

## Future Enhancements

1. **Dynamic IV**: Use a random IV per request for enhanced security
2. **Key Rotation**: Implement automatic key rotation
3. **Selective Encryption**: Allow fine-grained control over which fields are encrypted
4. **Performance Optimization**: Implement async encryption and caching
5. **Audit Logging**: Add detailed logging for encryption operations
6. **Client SDK**: Provide a client library for easy integration
