using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Messaging;

public class MailMessage
{
    /// <summary>
    ///     SendGrid template Id
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;
    
    /// <summary>
    ///     Recipients e-mail adresses
    /// </summary>
    public IEnumerable<string> Recipients { get; set; } = Array.Empty<string>();
    
    /// <summary>
    ///     E-mail data
    /// </summary>
    public object? Data { get; set; }
}