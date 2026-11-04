using System.Text.Json.Serialization;

namespace SemsApi.Models
{
    /// <summary>
    /// Lifecycle of an evaluation session.
    /// Draft -> Open -> Closed -> Archived.
    /// Closed may be reopened to Open only by an Admin.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EvaluationSessionStatus
    {
        Draft = 0,
        Open = 1,
        Closed = 2,
        Archived = 3
    }
}
