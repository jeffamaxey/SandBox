using Newtonsoft.Json;
using System;

/// <summary>
/// This module contains simple data models for events from Data License Notification Subsystem.
/// </summary>

namespace BEAPSamples
{
    /// <summary>
    /// This class represents 'context' part of DLNS notification.
    /// </summary>
    public class ContextModel
    {
        [JsonProperty("@vocab")]
        public string Vocab { get; set; }
    }

    /// <summary>
    /// This class represents 'digest' part of DLNS notification.
    /// </summary>
    public class DigestModel
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("digestValue")]
        public string DigestValue { get; set; }

        [JsonProperty("digestAlgorithm")]
        public string DigestAlgorithm { get; set; }
    }

    /// <summary>
    /// This class represents 'catalog' part of DLNS notification.
    /// </summary>
    public class CatalogModel
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }
    }

    /// <summary>
    /// This class represents 'dataset' part of DLNS notification.
    /// </summary>
    public class DatasetModel
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("catalog")]
        public CatalogModel Catalog { get; set; }
    }

    /// <summary>
    /// This class represents 'snapshot' part of DLNS notification.
    /// </summary>
    public class SnapshotModel
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("issued")]
        public string Issued { get; set; }

        [JsonProperty("dataset")]
        public DatasetModel Dataset { get; set; }
    }

    /// <summary>
    /// This class represents so-called 'generated' part of DLNS notification.
    /// </summary>
    public class DistributionModel
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("digest")]
        public DigestModel Digest { get; set; }

        [JsonProperty("snapshot")]
        public SnapshotModel Snapshot { get; set; }
    }

    /// <summary>
    /// This class is a data model for delivery notifications from BEAP.
    /// </summary>
    class DeliveryNotification
    {
        [JsonProperty("@context")]
        public ContextModel Context { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("endedAtTime")]
        public string EndedAtTime { get; set; }

        [JsonProperty("generated")]
        public DistributionModel Generated { get; set; }
    }
}
