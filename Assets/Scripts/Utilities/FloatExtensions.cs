public static class FloatExtensions
{
    /// <summary>
    /// Remap a float from (from1, to1) range to (from2, to2)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <param name="from2"></param>
    /// <param name="to2"></param>
    /// <returns></returns>
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    /// <summary>
    /// Remap a float from (from1, to1) range to (0.0f, 1.0f)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <returns></returns>
    public static float Remap01(this float value, float from1, float to1)
    {
        return (value - from1) / (to1 - from1);
    }
}