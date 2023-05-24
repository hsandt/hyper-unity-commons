namespace CommonsHelper
{

	public enum HorizontalDirection
	{
		Left = -1,
		None = 0,
		Right = 1
	}

	public static partial class DirectionUtil {
    
	    public static HorizontalDirection ToHorizontalDirection(float x) {
	        if (x < 0)
	            return HorizontalDirection.Left;
	        else if (x > 0)
	            return HorizontalDirection.Right;
	        else
	            return HorizontalDirection.None;
	    }

	    public static float ToSignX(HorizontalDirection horizontalDirection) {
	        if (horizontalDirection == HorizontalDirection.Left)
	            return -1f;
	        else if (horizontalDirection == HorizontalDirection.Right)
	            return 1f;
	        else
	            return 0f;
	    }

	}
}

