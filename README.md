# Unsplasharp

Unoficial C# wrapper around [Unplash](https://unsplash.com) API targeting .NET Standard 1.4.

This lib is compatible with .NET Core, .NET Framework 4.6.1, Xamarin (iOS, Android), Universal Windows Platform.

**Currently incomplete**

* Missing user's authentication actions

## Installation

[NuGet](https://preview.nuget.org/packages/unplasharp.api/0.5.0): ```Install-Package unplasharp.api```

## Usage

```csharp
using Unsplasharp;

var client = new Client("YOUR_APPLICATION_ID");
var photosFound = await client.SearchPhoto("mountains");
```

## API documentation

Official [API documentation](https://unsplash.com/documentation)

* [General](https://github.com/rootasjey/unsplasharp#General)
* [Photos](https://github.com/rootasjey/unsplasharp#Photos)
* [Collections](https://github.com/rootasjey/unsplasharp#Collections)
* [Users](https://github.com/rootasjey/unsplasharp#Users)

### Instanciate a new client

It's necessary to instanciate a new client with at least an application id to start making requests.

```csharp
var client = new Client("YOUR_APPLICATION_ID");
```

### General

#### Rates limits
Unplash has API requests [rates limits](https://unsplash.com/documentation#rate-limiting).

An Unplashsharp client has two properties to help you monitor API calls:

Max API calls allowed per hour

* ```MaxRateLimit```

API calls remaining for the current hour
* ```RateLimitRemaining``` 


### Photos

#### Get a single photo from an id

```csharp
var photo = await client.GetPhoto("TPv9dh822VA");

// custom parameters
var photoWidthHeight = await client.GetPhoto(id, width: 500, height: 500);
```

#### Get a random photo

```csharp
var photoRandom = await client.GetRandomPhoto();

// custom parameters
var randomPhotoFromCollections = await client.GetRandomPhoto(new string[] { "499830", "194162" });

var randomPhotoFromUser = await client.GetRandomPhoto(1, username: "matthewkane");

var randomPhotosFromQuery = await client.GetRandomPhoto(count: 3, query:"woman");
```

#### Get a list of all photos

```csharp
var listPhotos = await client.ListPhotos();

// custom parameters
var listPhotosPaged = await client.ListPhotos(page:2, perPage:15, orderBy: OrderBy.Popular);
```

#### Get a list of curated photos
```csharp
var listCuratedPhotos = await client.ListCuratedPhotos();
```


### Collections

#### Get a single collection from an id
```csharp
var collection = await client.GetCollection("771520");
```

#### Get a list of all collections
```csharp
var listCollection = await client.ListCollections();
```

#### Get a list of featured collections
```csharp
var listFeaturedCollection = await client.ListFeaturedCollections();
```

#### Get a list of curated collections
```csharp
var listCuratedCollection = await client.ListCuratedCollections();
```

#### Get a collection's photos from a collection's id
```csharp
var listPhotos = await client.GetCollectionPhotos("771520");
```

#### Get related collections from a collection's id

```csharp
var collectionsRelated = await client.ListRelatedCollections("771520");
```


### Users

#### Get a single user from his/her username

```csharp
var user = await client.GetUser("unsplash");

// custom parameters
var userCustomProfileImage = client.GetUser("seteales", width: 100, height: 100);
```

#### Get a list of user's collections

```csharp
var userCollections = await client.ListUserCollections("unsplash");
```

#### Get a list of user's photos

```csharp
var userPhotos = await client.ListUserPhotos("seteales");

var userPhotosCustomParam = await client.ListUserPhotos("seteales", page: 2, perPage: 2, stats: true);
```

#### Get a list of user's liked photos

```csharp
var userLikedPhotos = await client.ListUserLikedPhotos("seteales");
```

#### Get an user's statistics

```csharp
var userStats = await client.GetUserStats("seteales");
```


### Search
#### Search photos from a query

```csharp
var photosFound = await client.SearchPhoto("mountains");
```

#### Search collections from a query

```csharp
var collectionsFound = await client.SearchCollections("mountains");
```

#### Search users from a query

```csharp
var usersFound = await client.SearchUsers("mountains");
```

### Stats
#### Get Unplash [total stats](https://unsplash.com/documentation#totals)

```csharp
var totalStats = await client.GetTotalStats();
```

#### Get Unplash [monthly stats](https://unsplash.com/documentation#month)

```csharp
var monthlyStats = await client.GetMonthlyStats();
```

## Tests

Test are under UnsplashsharpTests project.

Unit tests are used to check the Unplash web service.

In this project, a test API key is used which is limited to 50 requests per hour. So ensure you're not off limit.

## Resources

* [Official Unplash documentation](https://unsplash.com/documentation)

## Personal API key

If you want ot get your personal API key from Unplash:

1. Go to [Unplash](https://unsplash.com)
2. Log in or create a new account
3. In the top bar, click on _'API/Developers'_
4. Go to _['Your applications'](https://unsplash.com/oauth/applications)_
5. Click on _'New Application'_ to create a new one and get an API key (and a Secret).

## TODO
* Add user's authentication actions
