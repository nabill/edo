# API functionality overview
## General
The whole system's purpose, in general, is to find appropriate rooms and book them.

Omitting the details, this involves several steps:
- finding a collection of accommodations (or places) which meet the requirements to search rooms in
- finding accommodations which have suitable available rooms
- after choosing an accommodation, finding more detailed and latest info about suitable available room contract sets there
- after choosing a room contract set, evaluating it to get the final contract terms and prices
- booking the selected room contract set

Booking management might be required afterwards in order to retrieve information about existing bookings or cancel a booking.

## Static and dynamic data
In general, the accommodation-related information can be divided into
- **static data**: accommodations info and locality info, which changes relatively rarely (hotels, star ratings, addresses, places, etc.)
- **dynamic data**: current availability info and prices, which change constantly

**Static data** is intended to be downloaded and then synchronized periodically by API users due to the fact that it does not change much and there is no point requesting it from the API every time, instead.

Static data synchronization is recommended once a week.

Static data search is done on the client side, so that when the client uses the API search, they already know, which accommodations or which places they need the available rooms to be found in.
For more information about the endpoints and models see [Accommodations related endpoints](/index.html#tag/Accommodations)

**Dynamic data** includes the following structure:
- _Accommodation_ (a hotel or another type of place where available room could be found)
- _Room_ contract set. It takes place inside a list of room contract sets in accommodation data. Room contract set is the entity that is being booked.
- _Room_. It takes place inside a list of rooms inside a room contract set. Rooms cannot be booked or otherwise managed, only a room contract set can.

Every entity above also includes details of its own, involving other data structures.

**Booking data** also takes place. It is required for booking management: querying existing bookings, cancelling, monitoring status, etc.

## Authorization

### Introduction
We use JWT tokens for authorization and API is available only for authorized clients.
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

## Availability search
### Search steps
####Static part
As mentioned above, when starting a search, the API client must already know the static data criteria (where to search) and provide a collection of ids of places or accommodations (ids of places and accommodations can be included in one collection simultaneously).

Thus searching the static data is entirely done by the API client, not by the API itself. The API itself only provides the static data, but not the means to search through it.

####Availability search
The search is done 3 steps:
1. [Wide availability search](/index.html#tag/Search/paths/~1api~11.0~1availabilities~1searches/post)
   Finds accommodations that match the search criteria and have room contract sets that match the search criteria.
   A number of room contract sets is also fetched to each found accommodation, however, it is not guaranteed that a list would contain all room contract sets for a given accommodation.
   The data fetched at this step is pulled from cache, therefore must not be considered as latest. Changes could have occurred since the cache update.
2. [Room selection](/index.html#tag/Search/paths/~1api~11.0~1availabilities~1searches~1{searchId}~1accommodations~1{accommodationId}/get)
   Finds a full list of room contract sets within chosen accommodation. The data is more likely to be correct due to the fact that search is not as broad as previous step.
3. [Booking evaluation (prebooking)](/index.html#tag/Search/paths/~1api~11.0~1availabilities~1searches~1{searchId}~1accommodations~1{accommodationId}~1room-contract-sets~1{roomContractSetId}/get)
   This step concludes the search. Selected room contract set is evaluated to fetch the final price and terms and determine that booking is possible.
   
Search is starting from wider search to more specific, narrowing the results from step to step, as in the scheme:

![image](https://user-images.githubusercontent.com/43397444/151672111-454f9e0c-292b-4472-baef-0749bd8c6e6c.png)

> **Note:** Results, returned during each step have the 10 minutes lifetime

Every next step uses information from the previous and cannot be executed in any other order than described above.
- Wide availability search introduces `SearchId`
- Room selection introduces `AccommodationId`
- Booking evaluation introduces `RoomContractSetId`

All the three parameters from these steps are used during booking and can be fetched only while steps executing.
Although the first step returns all three of them inside its models, it is not guaranteed that `RoomContractSetId` will be preserved same and will be valid for booking evaluation step or booking.


### Supported search models

Wide availability search can be executed in 3 modes:
1. One country search
2. One city search
3. Multiple hotel search (up to 1000 hotels in request)

The search mode is selected based on `SearchLocations` field of [AvailabilityRequest](/index.html#tag/Search/paths/~1api~11.0~1availabilities~1searches/post)  model and supports adding multiple location ids to the request, where each location id can be country id, locality id or accommodation id.
E.g. the following requests executes a search for locality with id `ff`:
```
// TODO example request
```
And the following request executes a search for 3 hotel ids:
```
// TODO example request
```

In current API version searching for accommodations in multiple countries or localities is not supported and validation will fail when trying to execute the request.

### Wide availability search polling

Since wide availability search may take a long time to complete, especially for large number of the hotels, there
is a way to get a part of results before search is fully finished.
The flow may be described as the polling loop, which can be started after [starting search](/index.html#tag/Search/paths/~1api~11.0~1availabilities~1searches/post)
done. During this loop client can continue executing the [get availability results endpoint](/index.html#tag/Search/paths/~1api~11.0~1availabilities~1searches~1{searchId}/get) until
search is complete or reached given timeout.
Endpoint returns the search state and ready results in a single model, and can be used as following:

![image](https://user-images.githubusercontent.com/43397444/151672982-603de243-dfab-4931-b5f0-f8cebb0220e9.png)

> **Note:** Polling request interval must be larger than 2 seconds 

## Booking flow
Data from the Booking evaluation step can be used to book a room contract set.
This creates a booking, which can then be managed.

### 2-step booking flow explanation
In our system we use 2-step booking flow, containing the following steps:
1. [Registration](/index.html#tag/Booking/paths/~1api~11.0~1bookings/post)
2. [Finalization](/index.html#tag/Booking/paths/~1api~11.0~1bookings~1{clientReferenceCode}~1finalize/post)

Two-step booking divides the booking process to 2 stages to allow clients to implement a safe flow.
1. **Registration** step is used to validate a booking, create a database record and prepare the system to execute a "real" booking with executing request to the final supplier. During this step a _Reference code_ is generated. It is safe to abandon such booking if the next step is not executed. If there is an error during step execution, client can be sure, that booking did not really go through.
2. **Finalization** step is used to make a real booking in supplier's or hotel's system, based on the booking registration, made before. Due to a broad variety of inter-system communication errors possible, this request may fail, while actually the booking will succeed on the supplier or the hotel side.

### Booking failure handling logic details

Booking request results interpretation based on the executing step and server response.
The flowchart explaining this:
![image](https://user-images.githubusercontent.com/43397444/151673402-d1015f7f-cfa0-4321-b26c-1f937fd5f8ec.png)

### Booking reference codes
Every booking in our system has a couple of unique identifiers: _Reference code_ and _supplier reference code_.
_Reference code_ is unique system wide and _supplier reference code_ is unique API-client wide.
The main identifier in our API is a client reference code, which must be provided by the client during booking request and plays an important role in booing API stability.

| Reference code                                 | Client reference code                                                  |
|------------------------------------------------|------------------------------------------------------------------------|
| Generated by HT system                         | Generated by client                                                    |
| Cannot be used to get the booking from the API | Can be used to get the booking by the API                              |
| Has strict format (e.g. `HTL-AE-0007W3-01`)    | Format is up to client (e.g. `124003982` or `bkn-298845` or any other) |


### Booking management

Following operations can be performed:
- [Retrieve a list of all bookings](/index.html#tag/Booking/paths/~1api~11.0~1bookings/get)
- [Retrieve details of a particular booking](/index.html#tag/Booking/paths/~1api~11.0~1bookings~1{clientReferenceCode}/get)
- [Cancel a booking](/index.html#tag/Booking/paths/~1api~11.0~1bookings~1{clientReferenceCode}~1cancel/post)
Operations involving a certain booking, such as cancellation, needs client reference code to pass. [More about reference codes.](/index.html#section/API-functionality-overview/Booking-reference-codes)

#### Booking cancellation and cancellation policies
When a booking is cancelled, there might be a cancellation penalty, depending on cancellation date.
The cancellation penalty rate may vary from 0 to 100 percent and is defined for each date.
This data is available on booking evaluation step.

## Payments flow
API supports only the credit flow, either prepaid or contracted.
Payments for the bookings are charged from the agency account, which is replenished by the Accounts team, based on payment documents or contract.
Account balance is accessible from the agent application on [HappyTravel.com](https://happytravel.com)

### Account charging flow
Booking price is charged from the account balance.
There are two main cases possible:
- For non-refundable (APR) bookings or bookings within deadline money is charged immediately. Booking fails if balance insufficient
- For all other bookings money is charged on the deadline date. Booking is **auto cancelled** if balance is insufficient

## Requests walkthrough
Here are the examples of requests in such order in which they would likely be used.
(Authorization request example is in the authorization section)

### Static data download

Get hotels list
<details>
  <summary>Request</summary>

(Top parameter is set to 1 in order to make the response example shorter. In reality it is practical to set a larger number)
```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/static/accommodations?top=1' \
--header 'Authorization: Bearer <token>'
```
</details>

<details>
  <summary>Response</summary>

```
[
    {
        "id": "Accommodation_9594451",
        "name": "Test Hotel Bhean",
        "category": null,
        "contacts": {
            "emails": [],
            "faxes": [],
            "phones": [],
            "webSites": []
        },
        "location": {
            "address": "Ookolhufinolhu, Lhaviyani Atoll 07040, Maldives",
            "coordinates": {
                "latitude": 5.38333,
                "longitude": 73.48333
            },
            "countryCode": "AE",
            "countryId": "Country_3658",
            "country": "The United Arab Emirates",
            "isHistoricalBuilding": false,
            "localityId": "Locality_607184",
            "locality": "Dubai",
            "localityZoneId": "LocalityZone_3194",
            "localityZone": "Na",
            "locationDescriptionCode": "SeaOrBeach",
            "pointsOfInterests": []
        },
        "photos": [
            {
                "caption": "",
                "sourceUrl": "http://www.travco.iolcloud.com/Iolimages//Documents/imagelibrary/Thumbnails/90400326.jpg"
            },
            {
                "caption": "",
                "sourceUrl": "http://www.travco.iolcloud.com/Iolimages//Documents/imagelibrary/Thumbnails/215911052.jpg"
            },
            {
                "caption": "",
                "sourceUrl": "http://www.travco.iolcloud.com/Iolimages//Documents/imagelibrary/Thumbnails/215910471.jpg"
            },
            {
                "caption": "",
                "sourceUrl": "http://www.travco.iolcloud.com/Iolimages//Documents/imagelibrary/Thumbnails/215498285.jpg"
            },
            {
                "caption": "",
                "sourceUrl": "http://www.travco.iolcloud.com/Iolimages//Documents/imagelibrary/Thumbnails/157213150.jpg"
            }
        ],
        "rating": "FiveStars",
        "schedule": {
            "checkInTime": null,
            "checkOutTime": null,
            "portersStartTime": "",
            "portersEndTime": "",
            "roomServiceStartTime": "",
            "roomServiceEndTime": ""
        },
        "textualDescriptions": [
            {
                "description": "<p>Offering an outdoor pool, year-round outdoor pool and barbecue, Cocoon Maldives is located on the pristinely stunning island of Ookolhufinolhu in Lhaviyani Atoll. The resort has a private beach area and water sports facilities, and guests can enjoy a meal at the restaurant. The property is designed by Daniele Lago, chairman and designer of Lago.</p>\n<p>All rooms are surrounded by green vegetation, clearly visible through the glass partitions that connect with the open-air bathrooms, which in itself creates a sense of connection with nature, in cool contrast with the expanse of the turquoise sea.</p>\n<p>Lago Community Table in the restaurant area was designed for exclusive events with cultural sharing and exchange increasing the value of the experience.</p>\n<p>Lago Talking Furniture project powers interaction with the furniture, connecting to each guest''s smartphone device through a wired chip in the furniture, to activate a direct interaction with the resort, to learn about themed evenings or indeed to share images and reviews of the experience.</p>",
                "type": "Unspecified"
            }
        ],
        "type": "Hotels",
        "hotelChain": null,
        "accommodationAmenities": [
            "24 Hours Concierge",
            "Bar",
            "Business Centre",
            "Laundry Services",
            "Safey Deposit Box",
            "WiFi in Public Areas",
            "Baby Sitting",
            "Spa",
            "Ooutdoor Pool"
        ],
        "additionalInfo": {},
        "modified": "2021-08-11T13:29:44.586132"
    }
]
```
</details>
<br>

Get a hotel by id
<details>
  <summary>Request</summary>

```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/static/accommodations/Accommodation_11064745' \
--header 'Authorization: Bearer <token>'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "id": "Accommodation_11064745",
    "name": "Hotel Test Pruebas Travelgate",
    "category": "1",
    "contacts": {
        "emails": [
            "hotelpush@xmltravelgate.com"
        ],
        "faxes": [
            ""
        ],
        "phones": [
            "971858585"
        ],
        "webSites": [
            ""
        ]
    },
    "location": {
        "address": "Calle Prueba TEST",
        "coordinates": {
            "latitude": 50.07,
            "longitude": 14.4
        },
        "countryCode": "AD",
        "countryId": "Country_3739",
        "country": "The Principality Of Andorra",
        "isHistoricalBuilding": false,
        "localityId": "Locality_643740",
        "locality": "Andorra La Vella",
        "localityZoneId": "",
        "localityZone": null,
        "locationDescriptionCode": "Unspecified",
        "pointsOfInterests": []
    },
    "photos": [],
    "rating": "OneStar",
    "schedule": {
        "checkInTime": null,
        "checkOutTime": null,
        "portersStartTime": "",
        "portersEndTime": "",
        "roomServiceStartTime": "",
        "roomServiceEndTime": ""
    },
    "textualDescriptions": [
        {
            "description": "",
            "type": "Unspecified"
        }
    ],
    "type": "Hotels",
    "hotelChain": null,
    "accommodationAmenities": [],
    "additionalInfo": {},
    "modified": "2021-08-04T19:54:36.875028"
}
```
</details>
<br>

### Search

Start search
<details>
  <summary>Request</summary>

```
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/availabilities/searches' \
--header 'Authorization: Bearer <token>' \
--header 'Content-Type: application/json' \
--data-raw '{
    "ids": [
        "Accommodation_10097818", 
        "Country_3748"
    ],
    "checkInDate": "2022-01-16T00:00:00Z",
    "checkOutDate": "2022-01-17T00:00:00Z",
    "nationality": "RU",
    "residency": "RU",
    "roomDetails": [
        {
            "adultsNumber": 1,
            "childrenAges": null
        }
    ]
}'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "searchId": "ab72b222-cc9e-4411-8211-a4135d941f81"
}
```
</details>
<br>

Get the wide search results
<details>
  <summary>Request</summary>

```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/availabilities/searches/ab72b222-cc9e-4411-8211-a4135d941f81' \
--header 'Authorization: Bearer <token>'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "searchId": "ab72b222-cc9e-4411-8211-a4135d941f81",
    "isComplete": true,
    "accommodations": [
        {
            "accommodationId": "Accommodation_11064994",
            "checkInDate": "2022-01-25T00:00:00Z",
            "checkOutDate": "2022-01-26T00:00:00Z",
            "roomContractSets": [
                {
                    "id": "4c895240-bfa2-4e6c-9137-64d559f517e2",
                    "rate": {
                        "currency": "USD",
                        "description": "",
                        "gross": {
                            "amount": 154.81,
                            "currency": "USD"
                        },
                        "discounts": [],
                        "finalPrice": {
                            "amount": 147.43,
                            "currency": "USD"
                        }
                    },
                    "deadline": {
                        "date": "2022-01-17T00:00:00Z",
                        "isFinal": true,
                        "policies": [
                            {
                                "fromDate": "2022-01-17T00:00:00Z",
                                "percentage": 100.0
                            }
                        ],
                        "remarks": []
                    },
                    "isAdvancePurchaseRate": true,
                    "rooms": [
                        {
                            "boardBasis": "RoomOnly",
                            "mealPlan": "Room only",
                            "contractTypeCode": "0",
                            "isAvailableImmediately": true,
                            "isDynamic": false,
                            "contractDescription": "Standard - 1 Queen Bed",
                            "rate": {
                                "currency": "USD",
                                "description": "",
                                "gross": {
                                    "amount": 154.81,
                                    "currency": "USD"
                                },
                                "discounts": [],
                                "finalPrice": {
                                    "amount": 147.43,
                                    "currency": "USD"
                                }
                            },
                            "remarks": [
                                {
                                    "key": "Food",
                                    "value": "The food inclusion does not always correspond to the number of people staying in the room"
                                },
                                {
                                    "key": "Policy",
                                    "value": ""
                                },
                                {
                                    "key": "Beds",
                                    "value": "queen - 1"
                                },
                                {
                                    "key": "Beds remarks",
                                    "value": "The date referring to the beds is not guaranteed"
                                }
                            ],
                            "adultsNumber": 1,
                            "childrenNumber": 0,
                            "deadline": {
                                "date": "2022-01-17T00:00:00Z",
                                "isFinal": true,
                                "policies": [
                                    {
                                        "fromDate": "2022-01-17T00:00:00Z",
                                        "percentage": 100.0
                                    }
                                ],
                                "remarks": []
                            },
                            "isAdvancePurchaseRate": true,
                            "dailyRoomRates": [],
                            "type": "NotSpecified"
                        }
                    ],
                    "isPackageRate": false
                }
            ]
        }
    ]
}
```
</details>
<br>

Get the accommodation search results
<details>
  <summary>Request</summary>

```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/availabilities/searches/ab72b222-cc9e-4411-8211-a4135d941f81/accommodations/Accommodation_11064994' \
--header 'Authorization: <token>'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "searchId": "ab72b222-cc9e-4411-8211-a4135d941f81",
    "accommodationId": "Accommodation_11064994",
    "roomContractSets": [
        {
            "id": "53793eca-29c0-4d67-bc14-15a157526e2b",
            "rate": {
                "currency": "USD",
                "description": "",
                "gross": {
                    "amount": 154.81,
                    "currency": "USD"
                },
                "discounts": [],
                "finalPrice": {
                    "amount": 147.43,
                    "currency": "USD"
                }
            },
            "deadline": {
                "date": "2022-01-17T00:00:00",
                "isFinal": true,
                "policies": [
                    {
                        "fromDate": "2022-01-17T00:00:00",
                        "percentage": 100.0
                    }
                ],
                "remarks": []
            },
            "isAdvancePurchaseRate": true,
            "rooms": [
                {
                    "boardBasis": "RoomOnly",
                    "mealPlan": "Room only",
                    "contractTypeCode": "0",
                    "isAvailableImmediately": true,
                    "isDynamic": false,
                    "contractDescription": "Standard - 1 Queen Bed",
                    "rate": {
                        "currency": "USD",
                        "description": "",
                        "gross": {
                            "amount": 154.81,
                            "currency": "USD"
                        },
                        "discounts": [],
                        "finalPrice": {
                            "amount": 147.43,
                            "currency": "USD"
                        }
                    },
                    "remarks": [
                        {
                            "key": "Food",
                            "value": "The food inclusion does not always correspond to the number of people staying in the room"
                        },
                        {
                            "key": "Policy",
                            "value": ""
                        },
                        {
                            "key": "Beds",
                            "value": "queen - 1"
                        },
                        {
                            "key": "Beds remarks",
                            "value": "The date referring to the beds is not guaranteed"
                        }
                    ],
                    "adultsNumber": 1,
                    "childrenNumber": 0,
                    "deadline": {
                        "date": "2022-01-17T00:00:00",
                        "isFinal": true,
                        "policies": [
                            {
                                "fromDate": "2022-01-17T00:00:00",
                                "percentage": 100.0
                            }
                        ],
                        "remarks": []
                    },
                    "isAdvancePurchaseRate": true,
                    "dailyRoomRates": [],
                    "type": "NotSpecified"
                }
            ],
            "isPackageRate": false
        }
    ]
}
```
</details>
<br>

Evaluation results
<details>
  <summary>Request</summary>

```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/availabilities/searches/8a0a244e-6d12-4b0e-8a4f-87ba024618d1/accommodations/Accommodation_11064994/room-contract-sets/806c0ab1-757e-4546-8c46-4e1a24f6f263' \
--header 'Authorization: Bearer <token>'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "searchId": "8a0a244e-6d12-4b0e-8a4f-87ba024618d1",
    "accommodationId": "Accommodation_11064994",
    "roomContractSet": {
        "id": "806c0ab1-757e-4546-8c46-4e1a24f6f263",
        "rate": {
            "currency": "USD",
            "description": "",
            "gross": {
                "amount": 154.81,
                "currency": "USD"
            },
            "discounts": [],
            "finalPrice": {
                "amount": 147.43,
                "currency": "USD"
            }
        },
        "deadline": {
            "date": "2022-01-17T00:00:00",
            "isFinal": true,
            "policies": [
                {
                    "fromDate": "2022-01-17T00:00:00",
                    "percentage": 100.0
                }
            ],
            "remarks": []
        },
        "isAdvancePurchaseRate": true,
        "rooms": [
            {
                "boardBasis": "RoomOnly",
                "mealPlan": "Room only",
                "contractTypeCode": "0",
                "isAvailableImmediately": true,
                "isDynamic": false,
                "contractDescription": "Standard - 1 Queen Bed",
                "rate": {
                    "currency": "USD",
                    "description": "",
                    "gross": {
                        "amount": 154.81,
                        "currency": "USD"
                    },
                    "discounts": [],
                    "finalPrice": {
                        "amount": 147.43,
                        "currency": "USD"
                    }
                },
                "remarks": [
                    {
                        "key": "Food",
                        "value": "The food inclusion does not always correspond to the number of people staying in the room"
                    },
                    {
                        "key": "Policy",
                        "value": ""
                    },
                    {
                        "key": "Beds",
                        "value": "queen - 1"
                    },
                    {
                        "key": "Beds remarks",
                        "value": "The date referring to the beds is not guaranteed"
                    }
                ],
                "adultsNumber": 1,
                "childrenNumber": 0,
                "deadline": {
                    "date": "2022-01-17T00:00:00",
                    "isFinal": true,
                    "policies": [
                        {
                            "fromDate": "2022-01-17T00:00:00",
                            "percentage": 100.0
                        }
                    ],
                    "remarks": []
                },
                "isAdvancePurchaseRate": true,
                "dailyRoomRates": [],
                "type": "NotSpecified"
            }
        ],
        "isPackageRate": false
    }
}
```
</details>
<br>

### Booking and booking management

Register booking (consists of 2 steps, register, then finalize, requires data from a completed search)
<details>
  <summary>Request</summary>

```
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/bookings' \
--header 'Authorization: Bearer <token>' \
--header 'Content-Type: application/json' \
--data-raw '{
    "nationality": "ru",
    "rejectIfUnavailable": true,
    "residency": "ru",
    "searchId": "0a984683-08c3-40df-babf-496f83aefcd3",
    "accommodationId": "Accommodation_11064994",
    "roomContractSetId": "f7ee0157-91cd-4cc0-8c48-095e1462c0fe",
    "evaluationToken": "",
    "mainPassengerName": "John Smith",
    "itineraryNumber": "",
    "clientReferenceCode": "AAAAA-81",
    "roomDetails": [
        {
            "passengers": [
                {
                    "age": null,
                    "firstName": "John",
                    "isLeader": true,
                    "lastName": "Smith",
                    "title": "Mr"
                }
            ],
            "type": "NotSpecified",
            "isExtraBedNeeded": true
        }
    ],
    "features": [
        {
            "type": "None",
            "value": "string"
        }
    ]
}'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "clientReferenceCode": "AAAAA-81",
    "referenceCode": "DEV-HTL-000GZ5-01",
    "accommodationId": "Accommodation_11064994",
    "created": "2022-01-18T21:47:05.1133132",
    "checkInDate": "2022-01-25T00:00:00",
    "checkOutDate": "2022-01-26T00:00:00",
    "totalPrice": {
        "amount": 147.43,
        "currency": "USD"
    },
    "status": "Created",
    "rooms": [
        {
            "boardBasis": "RoomOnly",
            "mealPlan": "Room only",
            "contractDescription": "Standard - 1 Queen Bed",
            "remarks": [
                {
                    "key": "Food",
                    "value": "The food inclusion does not always correspond to the number of people staying in the room"
                },
                {
                    "key": "Policy",
                    "value": ""
                },
                {
                    "key": "Beds",
                    "value": "queen - 1"
                },
                {
                    "key": "Beds remarks",
                    "value": "The date referring to the beds is not guaranteed"
                }
            ],
            "deadline": {
                "date": "2022-01-17T00:00:00",
                "policies": [
                    {
                        "fromDate": "2022-01-17T00:00:00",
                        "percentage": 100.0
                    }
                ],
                "remarks": []
            },
            "type": "NotSpecified",
            "price": {
                "amount": 147.43,
                "currency": "USD"
            },
            "passengers": [
                {
                    "age": null,
                    "firstName": "John",
                    "isLeader": true,
                    "lastName": "Smith",
                    "title": "Mr"
                }
            ]
        }
    ],
    "cancellationPolicies": [
        {
            "fromDate": "2022-01-17T00:00:00",
            "percentage": 100.0
        }
    ],
    "cancelled": null,
    "isAdvancePurchaseRate": true,
    "isPackage": false
}
```
</details>
<br>

Finalize booking
<details>
  <summary>Request</summary>

```
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/bookings/AAAAA-81/finalize' \
--header 'Authorization: Bearer <token>`
```
</details>

<details>
  <summary>Response</summary>

```
{
    "clientReferenceCode": "AAAAA-81",
    "referenceCode": "DEV-HTL-000GZ5-01",
    "accommodationId": "Accommodation_11064994",
    "created": "2022-01-18T21:47:05.113313",
    "checkInDate": "2022-01-25T00:00:00",
    "checkOutDate": "2022-01-26T00:00:00",
    "totalPrice": {
        "amount": 147.43,
        "currency": "USD"
    },
    "status": "Confirmed",
    "rooms": [
        {
            "boardBasis": "RoomOnly",
            "mealPlan": "Room only",
            "contractDescription": "Standard - 1 Queen Bed",
            "remarks": [
                {
                    "key": "Food",
                    "value": "The food inclusion does not always correspond to the number of people staying in the room"
                },
                {
                    "key": "Policy",
                    "value": ""
                },
                {
                    "key": "Beds",
                    "value": "queen - 1"
                },
                {
                    "key": "Beds remarks",
                    "value": "The date referring to the beds is not guaranteed"
                }
            ],
            "deadline": {
                "date": "2022-01-17T00:00:00",
                "policies": [
                    {
                        "fromDate": "2022-01-17T00:00:00",
                        "percentage": 100.0
                    }
                ],
                "remarks": []
            },
            "type": "NotSpecified",
            "price": {
                "amount": 147.43,
                "currency": "USD"
            },
            "passengers": [
                {
                    "age": null,
                    "firstName": "John",
                    "isLeader": true,
                    "lastName": "Smith",
                    "title": "Mr"
                }
            ]
        }
    ],
    "cancellationPolicies": [
        {
            "fromDate": "2022-01-17T00:00:00",
            "percentage": 100.0
        }
    ],
    "cancelled": null,
    "isAdvancePurchaseRate": true,
    "isPackage": false
}
```
</details>
<br>

Get bookings list
<details>
  <summary>Request</summary>

```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/bookings?CheckinFrom=2022-01-24T00:00:00&CheckinTo=2022-01-26T00:00:00' \
--header 'Authorization: Bearer <token>'
```
</details>

<details>
  <summary>Response</summary>

```
[
    {
        "clientReferenceCode": "AAAAA-81",
        "referenceCode": "DEV-HTL-000GZ5-01",
        "checkInDate": "2022-01-25T00:00:00",
        "checkOutDate": "2022-01-26T00:00:00",
        "accommodationId": "Accommodation_11064994",
        "totalPrice": {
            "amount": 147.43,
            "currency": "USD"
        },
        "isAdvancePurchaseRate": true,
        "status": "Confirmed",
        "mainPassengerName": "John Smith"
    }
]
```
</details>
<br>

Get a booking by reference code
<details>
  <summary>Request</summary>

```
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/bookings/AAAAA-81' \
--header 'Authorization: Bearer <token>'
```
</details>

<details>
  <summary>Response</summary>

```
{
    "clientReferenceCode": "AAAAA-81",
    "referenceCode": "DEV-HTL-000GZ5-01",
    "accommodationId": "Accommodation_11064994",
    "created": "2022-01-18T21:47:05.113313",
    "checkInDate": "2022-01-25T00:00:00",
    "checkOutDate": "2022-01-26T00:00:00",
    "totalPrice": {
        "amount": 147.43,
        "currency": "USD"
    },
    "status": "Confirmed",
    "rooms": [
        {
            "boardBasis": "RoomOnly",
            "mealPlan": "Room only",
            "contractDescription": "Standard - 1 Queen Bed",
            "remarks": [
                {
                    "key": "Food",
                    "value": "The food inclusion does not always correspond to the number of people staying in the room"
                },
                {
                    "key": "Policy",
                    "value": ""
                },
                {
                    "key": "Beds",
                    "value": "queen - 1"
                },
                {
                    "key": "Beds remarks",
                    "value": "The date referring to the beds is not guaranteed"
                }
            ],
            "deadline": {
                "date": "2022-01-17T00:00:00",
                "policies": [
                    {
                        "fromDate": "2022-01-17T00:00:00",
                        "percentage": 100.0
                    }
                ],
                "remarks": []
            },
            "type": "NotSpecified",
            "price": {
                "amount": 147.43,
                "currency": "USD"
            },
            "passengers": [
                {
                    "age": null,
                    "firstName": "John",
                    "isLeader": true,
                    "lastName": "Smith",
                    "title": "Mr"
                }
            ]
        }
    ],
    "cancellationPolicies": [
        {
            "fromDate": "2022-01-17T00:00:00",
            "percentage": 100.0
        }
    ],
    "cancelled": null,
    "isAdvancePurchaseRate": true,
    "isPackage": false
}
```
</details>
<br>

Cancel a booking

<details>
  <summary>Request</summary>

```
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/bookings/AAAAA-81/cancel' \
--header 'Authorization: Bearer
```
</details>

<details>
  <summary>Response</summary>

```
{
    "clientReferenceCode": "AAAAA-81",
    "referenceCode": "DEV-HTL-000GZ5-01",
    "accommodationId": "Accommodation_11064994",
    "created": "2022-01-18T21:47:05.113313",
    "checkInDate": "2022-01-25T00:00:00",
    "checkOutDate": "2022-01-26T00:00:00",
    "totalPrice": {
        "amount": 147.43,
        "currency": "USD"
    },
    "status": "Cancelled",
    "rooms": [
        {
            "boardBasis": "RoomOnly",
            "mealPlan": "Room only",
            "contractDescription": "Standard - 1 Queen Bed",
            "remarks": [
                {
                    "key": "Food",
                    "value": "The food inclusion does not always correspond to the number of people staying in the room"
                },
                {
                    "key": "Policy",
                    "value": ""
                },
                {
                    "key": "Beds",
                    "value": "queen - 1"
                },
                {
                    "key": "Beds remarks",
                    "value": "The date referring to the beds is not guaranteed"
                }
            ],
            "deadline": {
                "date": "2022-01-17T00:00:00",
                "policies": [
                    {
                        "fromDate": "2022-01-17T00:00:00",
                        "percentage": 100.0
                    }
                ],
                "remarks": []
            },
            "type": "NotSpecified",
            "price": {
                "amount": 147.43,
                "currency": "USD"
            },
            "passengers": [
                {
                    "age": null,
                    "firstName": "John",
                    "isLeader": true,
                    "lastName": "Smith",
                    "title": "Mr"
                }
            ]
        }
    ],
    "cancellationPolicies": [
        {
            "fromDate": "2022-01-17T00:00:00",
            "percentage": 100.0
        }
    ],
    "cancelled": "2022-01-18T22:13:25.937693",
    "isAdvancePurchaseRate": true,
    "isPackage": false
}
```
</details>




