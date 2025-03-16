using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class BoolArray2DConverter : JsonConverter<bool[,]>
{
    public override bool[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected StartArray token");

        using var document = JsonDocument.ParseValue(ref reader);
        var dim1Length = document.RootElement.GetArrayLength();
        var dim2Length = document.RootElement.EnumerateArray().First().GetArrayLength();

        bool[,] grid = new bool[dim1Length, dim2Length];

        int i = 0;
        foreach (var array in document.RootElement.EnumerateArray())
        {
            int j = 0;
            foreach (var boolean in array.EnumerateArray())
            {
                grid[i, j] = boolean.GetBoolean();
                j++;
            }
            i++;
        }

        return grid;
    }

    public override void Write(Utf8JsonWriter writer, bool[,] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        for (int i = 0; i < value.GetLength(0); i++)
        {
            writer.WriteStartArray();
            for (int j = 0; j < value.GetLength(1); j++)
            {
                writer.WriteBooleanValue(value[i, j]);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}
