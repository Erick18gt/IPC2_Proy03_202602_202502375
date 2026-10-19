using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Servicio2_API.Utilidades;


public class ConversorTextoFlexible : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type tipo, JsonSerializerOptions opciones)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();                          // ya era texto, lo dejo igual
            case JsonTokenType.Number:
                return Encoding.UTF8.GetString(reader.ValueSpan);   // el número tal cual venía escrito
            case JsonTokenType.True:
                return "true";
            case JsonTokenType.False:
                return "false";
            default:
                throw new JsonException($"No se puede leer un texto desde {reader.TokenType}");
        }
    }

    // Para escribir (cuando respondo) no hago nada especial.
    public override void Write(Utf8JsonWriter writer, string valor, JsonSerializerOptions opciones)
        => writer.WriteStringValue(valor);
}
