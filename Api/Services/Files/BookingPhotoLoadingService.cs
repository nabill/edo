using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Files;

public class BookingPhotoLoadingService : IBookingPhotoLoadingService
{
    public BookingPhotoLoadingService(EdoContext context, IHttpClientFactory clientFactory, IAmazonS3ClientService amazonS3ClientService,
        IOptions<ImageFileServiceOptions> imageFileServiceOptions, ILogger<BookingPhotoLoadingService> logger)
    {
        _context = context;
        _clientFactory = clientFactory;
        _amazonS3ClientService = amazonS3ClientService;
        _imageFileServiceOptions = imageFileServiceOptions.Value;
        _logger = logger;
    }


    public void StartBookingPhotoLoading(int bookingId)
    {
        Task.Run(async () =>
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking?.AccommodationInfo?.Photo is null)
                return;

            await UploadFile(booking.AccommodationInfo.Photo.SourceUrl, bookingId)
                .Map(newUrl => booking.AccommodationInfo.Photo.SourceUrl = newUrl)
                .Tap(_ => _context.Update(booking))
                .Tap(_ => _context.SaveChangesAsync())
                .OnFailure(message => 
                    _logger.LogError($"Booking photo loading error, booking id: {bookingId}, message: {message}"));
        });
    }


    private async Task<Result<string>> UploadFile(string url, long bookingId)
    {
        var key = $"{_imageFileServiceOptions.S3FolderName}/{bookingId}/photo.jpg";

        try
        {
            using HttpResponseMessage response = await _clientFactory.CreateClient().GetAsync(url);
            await using var streamToReadFrom = await response.Content.ReadAsStreamAsync();
            return await _amazonS3ClientService.Add(_imageFileServiceOptions.Bucket,
                key, streamToReadFrom, S3CannedACL.PublicRead);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(e.Message);
        }
    }


    private readonly EdoContext _context;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IAmazonS3ClientService _amazonS3ClientService;
    private readonly ImageFileServiceOptions _imageFileServiceOptions;
    private readonly ILogger<BookingPhotoLoadingService> _logger;
}