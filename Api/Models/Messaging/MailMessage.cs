using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Messaging;

public class MailMessage
{
    public string TemplateId { get; set; } = string.Empty;
    public IEnumerable<string> Recipients { get; set; } = Array.Empty<string>();
    public object? Data { get; set; }
}