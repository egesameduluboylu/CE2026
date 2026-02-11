using BuildingBlocks.Abstractions.Domain;

namespace Modules.Identity.Infrastructure.Persistence.Entities
{
    public class BackgroundJob : BaseEntity
    {
        /// <summary>
        /// Job type (Email, DataProcessing, ScheduledTask, etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Job status (Pending, Processing, Completed, Failed, Cancelled)
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Job priority (Low, Normal, High, Critical)
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// Serialized job data
        /// </summary>
        public string? Data { get; set; }
        
        /// <summary>
        /// Job result
        /// </summary>
        public string? Result { get; set; }
        
        /// <summary>
        /// Error message if job failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Stack trace if job failed
        /// </summary>
        public string? StackTrace { get; set; }
        
        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;
        
        /// <summary>
        /// Maximum retry attempts
        /// </summary>
        public int MaxRetries { get; set; } = 3;
        
        /// <summary>
        /// When the job started processing
        /// </summary>
        public DateTimeOffset? StartedAt { get; set; }
        
        /// <summary>
        /// When the job completed
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }
        
        /// <summary>
        /// When the job should be scheduled to run
        /// </summary>
        public DateTimeOffset? ScheduledAt { get; set; }
        
        /// <summary>
        /// When the job should expire
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }
        
        /// <summary>
        /// Additional metadata (JSON)
        /// </summary>
        public string? Metadata { get; set; }
        
        // Navigation properties
        public ICollection<BackgroundJobLog> Logs { get; set; } = new List<BackgroundJobLog>();
    }

    public class BackgroundJobLog : BaseEntity
    {
        /// <summary>
        /// Associated job ID
        /// </summary>
        public Guid JobId { get; set; }
        
        /// <summary>
        /// Log level (Debug, Info, Warning, Error)
        /// </summary>
        public string Level { get; set; } = "Info";
        
        /// <summary>
        /// Log message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional data (JSON)
        /// </summary>
        public string? Data { get; set; }
        
        // Navigation property
        public BackgroundJob Job { get; set; } = null!;
    }
}
