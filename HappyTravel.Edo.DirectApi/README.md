# API Overview

## General

The general purpose of this API is to find and book appropriate rooms.

Overall process:

1. Find accommodations (or places) that meet the search criteria.
2. Find accommodations that have suitable available rooms.
3. Choose an accommodation and find possible bookings, called room contract sets.
4. Choose a room contract set and confirm the final contract terms and price.
5. Book the selected room contract set.

You can also manage bookings by retrieving details or canceling a booking.

## Data types

Accommodations and booking data include:

- _Static data_: Accommodation details that rarely change, such as hotel name, address, and star rating
- _Dynamic data_: Accommodation details that change constantly, such as current availability and prices
- _Booking data_: Details about a particular booking

### Static data

Static data does not change often, so you do not need to download it every time you use the API. Consider updating this data weekly.

Clients search static data locally. A client uses this data to select the accommodations to search for available rooms with the API.
For more info about the endpoints and models, see [Accommodations-related endpoints](/index.html#tag/Static-Data).

### Dynamic data

This data structure includes:

- _Accommodation_: A hotel or other property with available rooms.
- _Room contract sets_: Accommodation data includes a list of _room contract sets_. You use a room contract set to make a booking.
- _Rooms_: A room contract set includes a list of one or more _rooms_. You book or manage rooms using a room contract set.

Each dynamic data structure has its own details and includes other data structures.

### Booking data

You use booking data for tasks such as searching, checking, and canceling existing bookings.

## Authorization

### Introduction

This API is available only for authorized clients. You need a JWT token for authorization.
More info on this: [JSON Web Token (Wikipedia)](https://en.wikipedia.org/wiki/JSON_Web_Token), [RFC 7519](https://datatracker.ietf.org/doc/html/rfc7519), and [Token debugger](https://jwt.io/).

For authorization, you provide your client credentials to receive a token. You then use it in API requests by attaching it to the header.

A token expires after 10 minutes. After that, you must request a new token.

### Flow

1. Send a request with your credentials to the Identity Service.

   POST `https://identity.happytravel.com/connect/token`

   The JSON response contains a token in the `access_token` field. Request and response examples are below.

2. Add the token to the `Authorization` header of your requests, using the following format:

   `Authorization: Bearer <token>`, where `<token>` means your actual token.

3. Ten minutes after you receive the token, it expires. Send an authorization request again to receive a new one.

### Request examples

#### Receive a token

Request:

```curl
curl --request POST \
  --url https://identity.happytravel.com/connect/token \
  --header 'Content-Type: application/x-www-form-urlencoded' \
  --data 'grant_type=client_credentials' \
  --data 'client_secret=ExamplePassword' \
  --data 'client_id=ExampleUserName'
```

Response:

```json
{
    "access_token": "<token>",
    "expires_in": 600,
    "token_type": "Bearer",
    "scope": "dac.api"
}
```

#### Use a token in a request

The text `<token>` stands for an actual token.

Request:

```curl
curl --request GET \
  --url https://edo.happytravel.com/en/api/1.0/accommodations/availabilities/searches/b1265bf7-7d9f-4a3e-846b-88330703786d/state \
  --header 'Authorization: Bearer <token>'
```

## Availability search

### Search steps

#### Before starting: Static data search

The API provides static data but not the means to search through it. To start, the client must first download the static data and select the IDs for places, accommodations, or both. Then, the client provides these to the API to search for availability.

#### Availability search

The API search has three steps:

1. [Wide availability search](/index.html#operation/Availability%20Search)

   This step returns accommodations and room contract sets that match the search criteria. This search fetches cached data. The availability may change after the cache update, so the returned room contract sets at this step may not be the most current.

2. [Room selection](/index.html#tag/Availability-Search/paths/~1api~11.0~1availabilities~1searches~1{searchId}~1accommodations~1{accommodationId}/get)

   This step returns the full list of room contract sets for the chosen accommodation. The data is more accurate because the search is not as broad as in the previous step.

3. [Booking evaluation (prebooking)](/index.html#tag/Availability-Search/paths/~1api~11.0~1availabilities~1searches~1{searchId}~1accommodations~1{accommodationId}~1room-contract-sets~1{roomContractSetId}/get)

   This final step of the search fetches the final price and terms for the selected room contract set and confirms that booking is possible.

The process starts as a wide search and narrows the results at each step:

![search schematic](https://user-images.githubusercontent.com/41554067/153322312-2f8d9609-8cfe-4510-8c71-444864141946.png)

Every step uses information from the step before, so you must follow them in this order.

Data from each step:

- Wide availability search provides `SearchId`.
- Room selection provides `AccommodationId`.
- Booking evaluation provides `RoomContractSetId`.

You use all three IDs during booking and can only fetch them during the search steps. The first step returns all three, but the initial `RoomContractSetId` can change and may not be valid for booking evaluation or booking.

### Wide availability search types

You can do three types of wide availability search:

- Single country search
- Single city search
- Multiple hotel search (up to 1000 hotels per request)

The `ids` field in the request to [start wide availability search](/index.html#operation/Availability%20Search) selects the search type. You can add multiple location IDs to the request, where each is a country ID, locality ID, or accommodation ID.

For example, this request searches within `Locality_607184`:

```json
{
  "ids": [
    "Locality_607184"
  ],
  "checkInDate": "2022-11-04T00:00:00Z",
  "checkOutDate": "2022-11-05T00:00:00Z",
  "nationality": "RU",
  "residency": "RU",
  "roomDetails": [
    {
      "adultsNumber": 2,
      "childrenAges": [
        5
      ],
      "type": "NotSpecified"
    }
  ]
}
```

This request searches within three accommodation IDs:

```json
{
  "ids": [
    "Accommodation_9594995",
    "Accommodation_729495",
    "Accommodation_346843"
  ],
  "checkInDate": "2022-11-04T00:00:00Z",
  "checkOutDate": "2022-11-05T00:00:00Z",
  "nationality": "RU",
  "residency": "RU",
  "roomDetails": [
    {
      "adultsNumber": 2,
      "childrenAges": [
        5
      ],
      "type": "NotSpecified"
    }
  ]
}
```

In the current API version, you can only search for accommodations in one country or locality. Requests including multiple countries or localities fail.

> Note: Do not rely on the formats of location or accommodation IDs because they may change.

### Wide availability search polling

The wide availability search may take a long time, especially for a large number of hotels. You can access partial results before the search is complete.

You can start this flow, called the _polling loop_, after you [start the wide availability search](/index.html#operation/Availability%20Search). During this loop, you can [get availability results](/index.html#tag/Availability-Search/paths/~1api~11.0~1availabilities~1searches~1{searchId}/get) until the search finishes or times out.
> **Note:** The polling request interval must be more than 2 seconds.

This endpoint returns the search state and results in a single model:

![search polling loop](https://user-images.githubusercontent.com/41554067/153536132-9a1c809d-2d0b-4757-8f02-712ca0edd4e6.png)

### Search result lifetimes

Every search step returns info with a short lifetime. You can use this for booking until it expires.

Limits:

- Wide availability search: _10 minutes_
- Room selection: _10 minutes_
- Booking evaluation: _10 minutes_

For example, if you search for a hotel, wait an hour, and then try to book, the booking fails. Make sure to handle lifetimes correctly so you do not try to book with expired results.

## Booking flow

You use data from the Booking evaluation step to book a room contract set. This creates a booking, which you can then manage.

### General booking flow

The booking flow has two steps:

1. [Registration](/index.html#operation/Bookings)
2. [Finalization](/index.html#tag/Bookings/paths/~1api~11.0~1bookings~1{clientReferenceCode}~1finalize/post)

Booking process:

1. **Registration** validates a booking, creates a database record, and prepares the system to execute a "real" booking by sending a request to the supplier. This step generates a _Reference code_.

    You can safely abandon a booking if you do not continue to Finalization. If there is an error during this step, you can be sure that you have not made a real booking.

2. **Finalization** uses the booking registration from the first step to make a real booking in a supplier's or hotel's system.

    Many inter-system communication errors are possible. Even if this request fails, the supplier or hotel might still make the booking. Make sure to check the status of bookings that fail at this stage.

### Booking failure handling logic

If a booking request fails, the action you should take depends on the step of the process and the server response.

Handling booking failures:

![booking failure schematic](https://user-images.githubusercontent.com/41554067/153322418-4d686626-faaa-47b2-aee7-b76835aa9b16.png)

### Booking reference codes

Every booking has two unique identifiers: the _reference code_ and the _client reference code_.

The _reference code_ is unique across our system.

The _client reference code_ is unique for your client and is the main identifier in our API. It is important for booking API stability, and you must provide one for each booking request.

| Reference code | Client reference code |
|-----|-----|
| Generated by HappyTravel | Generated by the client |
| Not for getting bookings from the API | Used to get bookings from the API |
| Strict format (for example, `HTL-AE-0007W3-01`) | Any client format (for example, `124003982` or `bkn-298845`) |

### Booking management

You can manage bookings as follows:

- [Retrieve a list of all bookings](/index.html#operation/Bookings)
- [Retrieve details of a particular booking](/index.html#tag/Bookings/paths/~1api~11.0~1bookings~1{clientReferenceCode}/get)
- [Cancel a booking](/index.html#tag/Bookings/paths/~1api~11.0~1bookings~1{clientReferenceCode}~1cancel/post)

You need a client reference code to work with a particular booking, such as for cancellation.

#### Booking cancellation policies

When you cancel a booking, there might be a cancellation penalty, depending on the date.

The cancellation penalty rate varies from 0 to 100 percent and depends on the date. This data is available at the booking evaluation step.

## Payment flow

The API supports only the credit flow, either prepaid or contracted.

Payments for bookings come from the agency account. The Accounts team adds money to your account, based on payment documents or your contract.

You can access the account balance using the agent application on [Happytravel.com](https://happytravel.com).

### Account charging flow

You pay the booking price from your account balance.

There are two main cases:

- For non-refundable (APR) bookings or bookings after the deadline, the payment is immediate. The booking fails if the balance is too low.
- For all other bookings, the payment is taken on the deadline date. The booking is _automatically canceled_ if the balance is too low.

## Error handling

Messages and status codes returned by the API (bound to HTTP status codes):

- `200 OK` - Request succeeded
- `400 Bad Request` - Bad request or validation error
- `401 Unauthorized` - Authorization failure
- `403 Forbidden` - Permission denied
- `404 Not Found` - Resource was not found
- `405 Method Not Allowed` - Incorrect HTTP method
- `500 Internal Server Error` - Unexpected error

### Bad Request (400) errors

In most cases, _Bad Request_ indicates an invalid or unacceptable request.

Common errors are:

| Error                                         | Description |
|-----------------------------------------------|-----|
| **'{Field}' must not be empty**               | A mandatory request field is missing or set to the default value (`"Unspecified"` for enums). Fill in the field and try again. |
| **'{Date}'  must be greater than '{another date}'** | A date field has an incorrect value. For example, the check-in date is in the past. |
| **Wrong country ISO code**                    | The nationality or residency ISO code has an incorrect format or value. |
| **'Adults Number' must be greater than '0'**      | A requested room has no adults. You must specify the number of adults in the request. |
| **Passengers don't have a leader**            | You must have at least one passenger with the `"IsLeader"` flag set to `"TRUE"` in a booking request. |

### Unauthorized (401) and Forbidden (403) errors

The API does not return a reason for _Unauthorized_ responses. Typical causes are:

- Invalid or corrupted token format
- Token expired
- Token signature validation failure

If you are not sure, try parsing the token on a website like [JWT.io](https://jwt.io) to check the token validity and data.

_Forbidden_ means that the client has a valid token but does not have permission for an operation. This is not common, and the best solution is to check that the URL is correct.

### Not Found (404) and Method Not Allowed (405) errors

These error codes show that the request is incorrect. _Not Found_ means that the URL is incorrect, and _Method Not Allowed_ means that the HTTP method is incorrect.

## Requests walkthrough

Here are example requests in the typical order.

(The authorization example is in the Authorization section)

### Static data download

#### Get the list of accommodations

<details>
  <summary>Request</summary>

The `top` parameter is 1 to make the response example shorter. In reality, a larger number is common.

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/static/accommodations?top=1' \
--header 'Authorization: Bearer <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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

#### Get an accommodation by ID

<details>
  <summary>Request</summary>

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/static/accommodations/Accommodation_11064745' \
--header 'Authorization: Bearer <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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

### Search

#### Wide availability: Start search

<details>
  <summary>Request</summary>

```curl
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

```json
{
    "searchId": "ab72b222-cc9e-4411-8211-a4135d941f81"
}
```

</details>

#### Wide availability: Get results

<details>
  <summary>Request</summary>

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/availabilities/searches/ab72b222-cc9e-4411-8211-a4135d941f81' \
--header 'Authorization: Bearer <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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
                        "totalPrice": {
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
                                "totalPrice": {
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

#### Room selection: Get room contract sets

<details>
  <summary>Request</summary>

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/availabilities/searches/ab72b222-cc9e-4411-8211-a4135d941f81/accommodations/Accommodation_11064994' \
--header 'Authorization: <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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
                "totalPrice": {
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
                        "totalPrice": {
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

#### Booking evaluation: Get room contract set details

<details>
  <summary>Request</summary>

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/availabilities/searches/8a0a244e-6d12-4b0e-8a4f-87ba024618d1/accommodations/Accommodation_11064994/room-contract-sets/806c0ab1-757e-4546-8c46-4e1a24f6f263' \
--header 'Authorization: Bearer <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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
            "totalPrice": {
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
                    "totalPrice": {
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

### Booking and booking management

#### Register booking

Registering is the first of two steps to make a booking. This requires data from a completed search.
<details>
  <summary>Request</summary>

```curl
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/bookings' \
--header 'Authorization: Bearer <token>' \
--header 'Content-Type: application/json' \
--data-raw '{
    "accommodationId": "Accommodation_11064994",
    "searchId": "0a984683-08c3-40df-babf-496f83aefcd3",
    "roomContractSetId": "f7ee0157-91cd-4cc0-8c48-095e1462c0fe",
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
            "type": "NotSpecified"
        }
    ]
}'
```

</details>

<details>
  <summary>Response</summary>

```json
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
    "isPackageRate": false
}
```

</details>

#### Finalize booking

Finalizing is the second of two steps to make a booking.
<details>
  <summary>Request</summary>

```curl
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/bookings/AAAAA-81/finalize' \
--header 'Authorization: Bearer <token>`
```

</details>

<details>
  <summary>Response</summary>

```json
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
    "isPackageRate": false
}
```

</details>

#### Get a list of bookings

<details>
  <summary>Request</summary>

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/bookings?CheckinFrom=2022-01-24T00:00:00&CheckinTo=2022-01-26T00:00:00' \
--header 'Authorization: Bearer <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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
        "leadPassengerName": "John Smith"
    }
]
```

</details>

#### Get a booking by reference code

<details>
  <summary>Request</summary>

```curl
curl --location --request GET 'https://api-dev.happytravel.com/api/1.0/bookings/AAAAA-81' \
--header 'Authorization: Bearer <token>'
```

</details>

<details>
  <summary>Response</summary>

```json
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
    "isPackageRate": false
}
```

</details>

#### Cancel a booking

<details>
  <summary>Request</summary>

```curl
curl --location --request POST 'https://api-dev.happytravel.com/api/1.0/bookings/AAAAA-81/cancel' \
--header 'Authorization: Bearer
```

</details>

<details>
  <summary>Response</summary>

```json
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
    "isPackageRate": false
}
```

</details>
