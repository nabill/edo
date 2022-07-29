using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Files;

public class BookingPhotoLoadingService : IBookingPhotoLoadingService
{
    public BookingPhotoLoadingService(IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory, IAmazonS3ClientService amazonS3ClientService,
        IOptions<ImageFileServiceOptions> imageFileServiceOptions, ILogger<BookingPhotoLoadingService> logger)
    {
        _scopeFactory = scopeFactory;
        _clientFactory = clientFactory;
        _amazonS3ClientService = amazonS3ClientService;
        _imageFileServiceOptions = imageFileServiceOptions.Value;
        _logger = logger;
    }


    public void StartBookingPhotoLoading(int bookingId)
    {
        Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<EdoContext>();
            var key = $"{_imageFileServiceOptions.S3FolderName}/{bookingId}/accommodation.jpg";
            
            await GetBookingRecord()
                .Bind(Validate)
                .Bind(UploadFile)
                .Tap(UpdateBooking)
                .OnFailure(LogError);


            async Task<Result<Booking?>> GetBookingRecord()
            {
                return await context.Bookings.FindAsync(bookingId);
            }

            Result<Booking> Validate(Booking? booking)
            {
                if (booking is null)
                    return Result.Failure<Booking>($"Could not find booking");
                
                if (booking.AccommodationInfo?.Photo?.SourceUrl is null)
                    return Result.Failure<Booking>($"Booking photo SourceUrl is null");
                
                if (booking.AccommodationInfo.Photo.SourceUrl.Contains(key))
                    return Result.Failure<Booking>($"Booking photo has already loaded to S3");
                
                return Result.Success(booking);
            }

            async Task<Result<(Booking, string)>> UploadFile(Booking booking)
            {
                try
                {
                    using HttpResponseMessage response = await _clientFactory.CreateClient().GetAsync(booking.AccommodationInfo.Photo.SourceUrl);
                    await using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
                    return await _amazonS3ClientService.Add(_imageFileServiceOptions.Bucket,
                            key, streamToReadFrom, S3CannedACL.PublicRead)
                        .Bind(s => Result.Success((booking, s)));
                }
                catch (Exception e)
                {
                    return Result.Failure<(Booking, string)>(e.Message);
                }
            }
            
            async Task UpdateBooking((Booking booking, string newUrl) bookingWithNewUrl)
            {
                var (booking, newUrl) = bookingWithNewUrl;
                booking.AccommodationInfo!.Photo!.SourceUrl = newUrl;
                context.Attach(booking).Property(b => b.AccommodationInfo).IsModified = true;
                await context.SaveChangesAsync();
            }


            void LogError(string message)
            {
                _logger.LogError($"Booking photo loading error, booking id: {bookingId}, message: {message}");
            }
        });
    }
    
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAmazonS3ClientService _amazonS3ClientService;
    private readonly ImageFileServiceOptions _imageFileServiceOptions;
    private readonly ILogger<BookingPhotoLoadingService> _logger;
}