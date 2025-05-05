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
        public static AnimatorParameterName BITED = new AnimatorParameterName("Bited");
        public static AnimatorParameterName EXIT = new AnimatorParameterName("Exit");
    }

    public static class ZombieAnimatorParameter
    {
        public static AnimatorParameterName DIRECTION_X = new AnimatorParameterName("DirectionX");
        public static AnimatorParameterName DIRECTION_Y = new AnimatorParameterName("DirectionY");
        public static AnimatorParameterName DAMAGE = new AnimatorParameterName("Damage");
        public static AnimatorParameterName STRIDE = new AnimatorParameterName("Stride");
        public static AnimatorParameterName WALK_TYPE = new AnimatorParameterName("WalkType");
        public static AnimatorParameterName BITE = new AnimatorParameterName("Bite");
        public static AnimatorParameterName EXIT = new AnimatorParameterName("Exit");
    }

    public static class GunAnimatorParameter
    {
        public static AnimatorParameterName FIRE = new AnimatorParameterName("Fire");
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

public static class LayerConst
{
    public static int PLAYER = LayerMask.NameToLayer("Player");
    public static int ZOMBIE = LayerMask.NameToLayer("Zombie");
    public static int RAGDOLL = LayerMask.NameToLayer("Ragdoll");
}
