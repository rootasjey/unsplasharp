
# Obtaining an API Key

To use the Unsplash API, you need to register as a developer and create an application to get an API key.

## Steps

1. **Create an Unsplash Account**: If you don't have an Unsplash account, you'll need to create one at [unsplash.com](https://unsplash.com).

2. **Join as a Developer**: Go to the [Unsplash for Developers](https://unsplash.com/developers) page and click "Join as a Developer".

3. **Register a New Application**:
    - Go to your [applications dashboard](https://unsplash.com/oauth/applications).
    - Click the "New Application" button.
    - Read and accept the API terms.
    - Fill out the application details:
        - **Application Name**: A descriptive name for your application.
        - **Description**: A brief description of what your application does.
    - Click "Create Application".

4. **Get Your API Keys**:
    - After creating the application, you'll be redirected to the application's page.
    - You will find your **Access Key** and **Secret Key**.
    - The **Access Key** is what you'll use as your `ApplicationId` in Unsplasharp.

## API Usage Limits

The Unsplash API has the following rate limits for new applications:

- **Demo**: 50 requests per hour.
- **Production**: 5000 requests per hour (requires approval from Unsplash).

You can see your current rate limit status in the response headers of each API call. Unsplasharp provides this information in the `RateLimitInfo` property of the `UnsplasharpException` and `ErrorContext` classes.
