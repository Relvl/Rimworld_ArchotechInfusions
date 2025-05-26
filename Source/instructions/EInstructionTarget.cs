using System;

namespace ArchotechInfusions.instructions;

[Flags]
public enum EInstructionTarget
{
    None = 0,
    Apparel = 1,
    RangedWeapon = 2,
    MeleeWeapon = 4,
    Any = Apparel | RangedWeapon | MeleeWeapon
}