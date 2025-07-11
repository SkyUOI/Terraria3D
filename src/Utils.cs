using Godot;

public class Utils
{
    public static Vector2 GetChunk(Vector3 pos)
    {
        return new Vector2((int)pos.X / 16, (int)pos.Z / 16);
    }
}


public class StatefulRandom
{
    // 线性同余生成器参数（常用数值）
    private const uint a = 1664525;
    private const uint c = 1013904223;
    private ulong _state;

    public StatefulRandom(ulong seed)
    {
        _state = seed;
    }

    // 获取当前状态（用于保存）
    public ulong GetState() => _state;

    // 设置状态（用于加载）
    public void SetState(ulong state)
    {
        _state = state;
    }

    // 生成下一个随机数
    public long Next()
    {
        _state = _state * a + c;
        return (long)(_state >> 16); // 取高16位作为随机数
    }
}
