namespace HappyTravel.Edo.Api.Models.Messaging;

public class MailAttachment
{
    public MailAttachment(byte[] content, string type, string filename, string disposition)
    {
        Content = content;
        Type = type;
        Filename = filename;
        Disposition = disposition;
    }


    public byte[] Content { get; }
    public string Type { get; }
    public string Filename { get; }
    public string Disposition { get; }
}