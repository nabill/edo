using System;

namespace HappyTravel.Edo.Data.Agents
{
    public class UploadedImage
    {
        public int Id { get; set; }

        public int AgencyId { get; set; }

        public string FileName { get; set; }

        public string Url { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Updated { get; set; }
    }
}