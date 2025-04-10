using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class BoolArray2DConverter : JsonConverter<bool[,]>
{
    public override bool[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected StartArray token");

        using var document = JsonDocument.ParseValue(ref reader);
        var dimention2D = document.RootElement.EnumerateArray().FirstOrDefault();

        int cols = 0;
        int rows = document.RootElement.GetArrayLength();

        if (dimention2D.ValueKind != JsonValueKind.Undefined)
            cols = document.RootElement.EnumerateArray().FirstOrDefault().GetArrayLength();

        bool[,] grid = new bool[rows, cols];

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
