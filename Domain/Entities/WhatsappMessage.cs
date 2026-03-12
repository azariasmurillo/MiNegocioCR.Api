using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using System;

namespace MiNegocioCR.Api.Domain.Entities
{
    public class WhatsAppMessage
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string MessageId { get; set; } = default!; // wamid
        public string PhoneNumber { get; set; } = default!;
        public string From { get; set; } = default!;
        public string To { get; set; } = default!;
        public string Body { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public MessageDirection Direction { get; set; }
        public MessageStatus Status { get; set; }
        public Guid? CustomerId { get; set; }
        public Business Business { get; set; } = default!;

        public string? MetaMessageId { get; set; }        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }

    }
}

