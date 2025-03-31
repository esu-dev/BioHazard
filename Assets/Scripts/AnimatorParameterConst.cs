using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorParameterConst
{
    public static class PlayerAnimatorParameter
    {
        public static AnimatorParameterName SPEED = new AnimatorParameterName("Speed");
        public static AnimatorParameterName VELOCITY_X = new AnimatorParameterName("VelocityX");
        public static AnimatorParameterName vELOCITY_Y = new AnimatorParameterName("VelocityY");
    }
}

public class AnimatorParameterName
{
    public string Name { get; private set; }

    public AnimatorParameterName(string name)
    {
        Name = name;
    }
}
