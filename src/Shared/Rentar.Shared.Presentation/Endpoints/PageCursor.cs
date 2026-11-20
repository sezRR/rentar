using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rentar.Shared.Presentation.Endpoints;

/// <summary>
/// The opaque cursor a client continues a v2 list with: the key of the last item it has, as base64url-encoded JSON
/// without padding (<c>{"id":124}</c> is <c>eyJpZCI6MTI0fQ</c>). Clients pass it back as they got it.
/// </summary>
internal static class PageCursor
{
    public static string Encode(int after)
    {
        return Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(new CursorBody(after)));
    }

    /// <summary>Reads the key back; <c>false</c> for anything that is not a cursor this API wrote.</summary>
    public static bool TryDecode(string cursor, out int after)
    {
        after = 0;

        try
        {
            if (JsonSerializer.Deserialize<CursorBody>(Base64Url.DecodeFromChars(cursor)) is not { Id: { } id })
            {
                return false;
            }

            after = id;
            return true;
        }
        catch (Exception exception) when (exception is FormatException or JsonException)
        {
            return false;
        }
    }

    private sealed record CursorBody([property: JsonPropertyName("id")] int? Id);
}
