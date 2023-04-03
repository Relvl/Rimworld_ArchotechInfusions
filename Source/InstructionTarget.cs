using System;

namespace ArchotechInfusions;

[Flags]
public enum InstructionTarget
{
    None = 0,
    Apparel = 1,
    RangedWeapon = 2,
    MeleeWeapon = 4
}