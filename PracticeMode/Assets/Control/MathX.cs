using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathX
{
    // Limit the input angle to plus or minus 180 degrees:
    public static float AngleWithin180(float inputAngle)
    {
        while (inputAngle > 180)
        {
            inputAngle -= 360f;
        }

        while (inputAngle < -180)
        {
            inputAngle += 360f;
        }

        return inputAngle;
    }

    // Get the horizontal distance between two points; a position and a target:
    public static float Distance(float positionX, float positionZ, float targetX, float targetZ)
    {
        float xDistance = targetX - positionX;
        float zDistance = targetZ - positionZ;
        return Mathf.Sqrt((xDistance * xDistance) + (zDistance * zDistance));
    }

    // Works similar to Mathf.Clamp, but designed specifically for position and velocity.
    //
    // If the given input position is out of bounds:
    //   ~ Clamp the position (by reference) to the applicable bound.
    //   ~ Set the velocity (by reference) to zero.
    public static void Clamp(ref float position, float min, float max, ref float velocity)
    {
        if (position < min)
        {
            position = min;

            if (velocity < 0f)
            {
                velocity = 0f;
            }
        }
        else if (position > max)
        {
            position = max;

            if (velocity > 0f)
            {
                velocity = 0f;
            }
        }
    }
}
