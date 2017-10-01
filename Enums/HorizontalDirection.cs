public enum HorizontalDirection
{
	Left = -1,
	None = 0,
	Right = 1
}

public static partial class DirectionUtil {
    
    public static HorizontalDirection ToHorizontalDirection(float value) {
        if (value < 0)
            return HorizontalDirection.Left;
        else if (value > 0)
            return HorizontalDirection.Right;
        else
            return HorizontalDirection.None;
    }

}