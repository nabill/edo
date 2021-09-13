using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.CreditCards.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            CreditCardServiceCardRequested = LoggerMessage.Define<DateTime, string, decimal, string>(LogLevel.Information,
                new EventId(100100, "CreditCardServiceCardRequested"),
                "Requested a credit card to {DueDate} for reference code '{ReferenceCode}'. Amount: {Amount} {Currency}");
            
            CreditCardServiceCardFailure = LoggerMessage.Define<string, string>(LogLevel.Warning,
                new EventId(100101, "CreditCardServiceCardFailure"),
                "Failed to get credit card for reference code '{ReferenceCode}'. Error: {Error}");
            
            CreditCardServiceCardSuccess = LoggerMessage.Define<string>(LogLevel.Information,
                new EventId(100102, "CreditCardServiceCardSuccess"),
                "Successfully got credit card for reference code '{ReferenceCode}'");
            
        }
    
                
         public static void LogCreditCardServiceCardRequested(this ILogger logger, DateTime DueDate, string ReferenceCode, decimal Amount, string Currency, Exception exception = null)
            => CreditCardServiceCardRequested(logger, DueDate, ReferenceCode, Amount, Currency, exception);
                
         public static void LogCreditCardServiceCardFailure(this ILogger logger, string ReferenceCode, string Error, Exception exception = null)
            => CreditCardServiceCardFailure(logger, ReferenceCode, Error, exception);
                
         public static void LogCreditCardServiceCardSuccess(this ILogger logger, string ReferenceCode, Exception exception = null)
            => CreditCardServiceCardSuccess(logger, ReferenceCode, exception);
    
    
        
        private static readonly Action<ILogger, DateTime, string, decimal, string, Exception> CreditCardServiceCardRequested;
        
        private static readonly Action<ILogger, string, string, Exception> CreditCardServiceCardFailure;
        
        private static readonly Action<ILogger, string, Exception> CreditCardServiceCardSuccess;
    }
}