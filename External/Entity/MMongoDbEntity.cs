﻿namespace Muonroi.BuildingBlock.External.Entity;

public abstract class MMongoDbEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public virtual string? Id { get; protected init; }

    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [BsonElement("lastModifiedDate")]
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}