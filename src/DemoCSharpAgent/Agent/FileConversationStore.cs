using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Agents.AI;

namespace DemoCSharpAgent.Agent;

public sealed class FileConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<string, AgentSession> _cache = new();
    private readonly ConcurrentDictionary<string, object> _locks = new();
    private readonly string _storeDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileConversationStore()
    {
        _storeDirectory = Path.Combine(AppContext.BaseDirectory, "conversations");
        Directory.CreateDirectory(_storeDirectory);
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
        };
    }

    public AgentSession? GetThread(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            return null;

        if (_cache.TryGetValue(conversationId, out var cached))
            return cached;

        var gate = _locks.GetOrAdd(conversationId, static _ => new object());
        lock (gate)
        {
            if (_cache.TryGetValue(conversationId, out cached))
                return cached;

            var path = GetConversationPath(conversationId);
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            var session = JsonSerializer.Deserialize<AgentSession>(json, _jsonOptions);
            if (session is null)
                return null;

            _cache[conversationId] = session;
            return session;
        }
    }

    public void SaveThread(string conversationId, AgentSession thread)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation id cannot be null or empty.", nameof(conversationId));
        if (thread is null)
            throw new ArgumentNullException(nameof(thread));

        _cache[conversationId] = thread;

        var gate = _locks.GetOrAdd(conversationId, static _ => new object());
        lock (gate)
        {
            var path = GetConversationPath(conversationId);
            var tmpPath = path + ".tmp";

            var json = JsonSerializer.Serialize(thread, _jsonOptions);
            Directory.CreateDirectory(_storeDirectory);
            File.WriteAllText(tmpPath, json);

            if (File.Exists(path))
                File.Replace(tmpPath, path, destinationBackupFileName: null);
            else
                File.Move(tmpPath, path);
        }
    }

    private string GetConversationPath(string conversationId)
    {
        var safeName = MakeSafeFileName(conversationId);
        return Path.Combine(_storeDirectory, safeName + ".json");
    }

    private static string MakeSafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (invalid.Contains(chars[i]))
                chars[i] = '_';
        }

        return new string(chars);
    }
}

