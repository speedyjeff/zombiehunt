using System;

namespace ZombieHunt
{
    public enum ErrorDef 
    { 
        None, 
        NotSelectable, 
        InvalidCard, 
        NonPlayerCard, 
        InvalidDiagonalMove, 
        NonClearPath, 
        ExceedMaxMove, 
        InvalidAttack, 
        InvalidAttackDirection 
    };

}
