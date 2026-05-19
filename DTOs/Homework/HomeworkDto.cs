using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimelyBackEnd.DTOs.Homework
{
    public record HomeworkDto(
        Guid Id,
        string Subject,
        string Description,
        DateTime CreatedAt,
        [property: JsonConverter(typeof(DateOnlyJsonConverter))] DateOnly? DueDate,
        string CreatedBy,
        string? ImageUrl
    );

    public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;
                
                if (DateOnly.TryParse(str, out var date))
                    return date;
            }
            
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
            else
                writer.WriteNullValue();
        }
    }
}