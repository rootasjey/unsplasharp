# Downloading a Photo

This section explains how to download an Unsplash photo using the Unsplasharp DSK (Developer SDK) library.

## Prerequisites

Before you begin, ensure you have:

*   An Unsplash API key. If you don't have one, refer to the [Obtaining an API Key](obtaining-an-api-key.md) guide.
*   The Unsplasharp library installed in your project.

## Example: Downloading a Photo

To download a photo, you typically need the photo's ID. You can then use the `UnsplasharpClient` to retrieve the photo details and its download link.

```csharp
using Unsplasharp;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

public class PhotoDownloader
{
    public static async Task Main(string[] args)
    {
        // Replace with your actual Unsplash API key
        string apiKey = "YOUR_UNSPLASH_API_KEY";
        // Replace with the ID of the photo you want to download
        string photoId = "PHOTO_ID_TO_DOWNLOAD"; 
        // Replace with the desired path to save the downloaded photo
        string downloadPath = "path/to/your/downloaded_photo.jpg"; 

        var client = new UnsplasharpClient(apiKey);

        try
        {
            // Get photo details
            var photo = await client.GetPhoto(photoId);

            if (photo != null && !string.IsNullOrEmpty(photo.Links.DownloadLocation))
            {
                // Unsplash requires you to hit the download location endpoint to register a download
                // before you can download the actual image.
                var downloadLink = await client.GetPhotoDownloadLink(photoId);

                if (!string.IsNullOrEmpty(downloadLink))
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        // Download the image
                        byte[] imageBytes = await httpClient.GetByteArrayAsync(downloadLink);
                        await File.WriteAllBytesAsync(downloadPath, imageBytes);
                        Console.WriteLine($"Photo downloaded successfully to: {downloadPath}");
                    }
                }
                else
                {
                    Console.WriteLine("Could not retrieve download link for the photo.");
                }
            }
            else
            {
                Console.WriteLine($"Photo with ID '{photoId}' not found or download link is missing.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
```

Remember to replace `"YOUR_UNSPLASH_API_KEY"`, `"PHOTO_ID_TO_DOWNLOAD"`, and `"path/to/your/downloaded_photo.jpg"` with your actual values.

## Important Considerations

*   **API Key:** Always keep your API key secure and do not expose it in client-side code.
*   **Download Tracking:** Unsplash requires you to trigger the `download_location` endpoint before you can download the actual image. This is important for their download statistics. The `GetPhotoDownloadLink` method handles this for you.
*   **Error Handling:** Implement robust error handling to gracefully manage API errors, network issues, or invalid photo IDs.
*   **Rate Limiting:** Be mindful of Unsplash API rate limits. If you exceed them, your requests might be temporarily blocked.
