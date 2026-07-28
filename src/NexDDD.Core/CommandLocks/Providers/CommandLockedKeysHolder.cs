namespace NexDDD.Core.CommandLocks;

/// <summary>
/// 负责命令并发控制模块中的 CommandLockedKeysHolder 能力，并限定其与上层调用者和基础设施组件的协作边界。
/// </summary>
class CommandLockedKeysHolder
{
    private static readonly AsyncLocal<KeysHolder> KeysHolderCurrent = new AsyncLocal<KeysHolder>();

    public KeysHolder LockedKeys => KeysHolderCurrent.Value ??= new KeysHolder();
}

/// <summary>
/// 负责命令并发控制模块中的 KeysHolder 能力，并限定其与上层调用者和基础设施组件的协作边界。
/// </summary>
class KeysHolder
{
    public HashSet<string> Keys { get; set; } = new HashSet<string>();
}