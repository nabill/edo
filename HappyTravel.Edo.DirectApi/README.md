## API functionality overview
### General information
The whole system's purpose, in general, is to find a room and book it.

Omitting the details, this involves several steps:
- finding a collection of accommodations (or places) which meet the requirements to search rooms in
- finding accommodations which have suitable available rooms
- after choosing an accommodation, finding more detailed and latest info about suitable available room contract sets there
- after choosing a room contract set, evaluating it to get the final contract terms and prices
- booking the selected room contract set

Booking management might be required afterwards in order to retrieve information about existing bookings or cancel a booking.

### Data
In general, the accommodation-related information could be divided into
- static data: accommodations info and locality info, which changes relatively rarely (hotels, star ratings, addresses, places, etc.)
- dynamic data: current availability info and prices, which change constantly

**Static data** is intended to be downloaded and then synchronized periodically by API users due to the fact that it does not change much and there is no point requesting it from the API every time, instead.

Static data synchronization is recommended once a week.

Static data search is done on the client side, so that when the client uses the API search, they already know, which accommodations or which places they need the available rooms to be found in.

**Dynamic data** includes the following structure:
- Accommodation (a hotel or another type of place where available room could be found)
- Room contract set. It takes place inside a list of room contract sets in accommodation data. Room contract set is the entity that is being booked.
- Room. It takes place inside a list of rooms inside a room contract set. Rooms cannot be booked or otherwise managed, only a room contract set can.

Every entity above also includes details of its own, involving other data structures.

**Booking data** also takes place. It is required for booking management: querying existing bookings, cancelling, monitoring status, etc.

### Search
**Static part**
As mentioned above, when starting a search, the API client must already know the static data criteria (where to search) and provide a collection of ids of places or accommodations (ids of places and accommodations can be included in one collection simultaneously).

Thus searching the static data is entirely done by the API client, not by the API itself. The API itself only provides the static data, but not the means to search through it.

**Availability search**
The search is done 3 steps:
1. Wide availability search.
   Finds accommodations that match the search criteria and have room contract sets that match the search criteria.
   A number of room contract sets is also fetched to each found accommodation, however, it is not guaranteed that a list would contain all room contract sets for a given accommodation.
   The data fetched at this step is pulled from cache, therefore must not be considered as latest. Changes could have occurred since the cache update.
2. Search within accommodation.
   Finds a full list of room contract sets within chosen accommodation. The data is more likely to be correct due to the fact that search is not as broad as previous step.
3. Room contract set evaluation.
   This step concludes the search. Selected room contract set is evaluated to fetch the final price and terms and determine that booking is possible.

## Booking
Data from a completed search can be used to book a room contract set.
This creates a booking, which can then be managed.

## Booking management
### General information
Following operations can be performed:
- Retrieve a list of all bookings
- Retrieve details of a particular booking
- Cancel a booking

### Booking cancellation and cancellation policies
When a booking is cancelled, there might be a cancellation penalty, depending on cancellation date.
The cancellation penalty rate may vary from 0 to 100 percent and is defined for each date.
This data is available before booking.






## Authorization

### Introduction
We use JWT tokens for authorization.
More info on this: [Wikipedia article](https://en.wikipedia.org/wiki/JSON_Web_Token), [rfc7519](https://datatracker.ietf.org/doc/html/rfc7519), [Token debugger](https://jwt.io/)

The authorization in general consists of receiving a token by providing your credentials and then using it in your requests by attaching the token in headers.

Note that tokens are temporary (10 minutes) and you will need to receive another once current token is outdated.

### Flow
1. Send a request with your credentials to the Identity Service.
   POST `https://identity.happytravel.com/connect/token`
   The response will contain json with a token and other info. The token is in `access_token` field.
   An example of a token request and response is below.

2. Put this token into the "Authorization" header of your requests, using the following format:
   `Authorization: Bearer token`, where "token" stands for an actual token received from the Identity Service.

3. If 10 minutes passed from the last time a token was received, send an authorization request again to receive a new token.

### Request examples
Receive a token:
Request:
```
curl --request POST \
  --url https://identity.happytravel.com/connect/token \
  --header 'Content-Type: application/x-www-form-urlencoded' \
  --data 'grant_type=client_credentials' \
  --data 'client_secret=ExamplePassword' \
  --data 'client_id=ExampleUserName'
```
Response:
```
{
    "access_token": "<access_token>",
    "expires_in": 3600,
    "token_type": "Bearer",
    "scope": "dac.api"
}
```

Use a token in a request (where "<access_token>" stands for an actual token)
```
curl --request GET \
  --url https://edo.happytravel.com/en/api/1.0/accommodations/availabilities/searches/b1265bf7-7d9f-4a3e-846b-88330703786d/state \
  --header 'Authorization: Bearer token'
```

