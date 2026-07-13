using System.Collections.Generic;
using System.Linq;

namespace PickMyRandoms.PickMyRandomsCode;

public static class ExcludedPool
{
    // character ModelIds excluded from random, persisted via config as csv
    private static HashSet<string> _excluded = new();

    public static bool IsExcluded(string characterId) => _excluded.Contains(characterId);

    public static void Toggle(string characterId)
    {
        if (!_excluded.Remove(characterId))
            _excluded.Add(characterId);
        PoolConfig.ExcludedIds = string.Join(",", _excluded);
        MainFile.Config.Save();
    }

    public static void LoadFromConfig()
    {
        _excluded = PoolConfig.ExcludedIds
            .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
    }
}