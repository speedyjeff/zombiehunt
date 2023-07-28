## Zombie Hunt
Zombie Hunt is an adaptation of a classic game (played with the Fox and Bear).  The goal of the game is to score the highest points against your opponent.

![game](https://github.com/speedyjeff/zombiehunt/blob/main/media/game.png)

### Rules
```
    //   2 players, they take turns
    //   On each turn you can either flip a card or move a card
    //
    //    1. Flipping over a face down card
    //       Any card that is face down may be flipped over (the orientation of the flipped card cannot be changed)
    //    2. Move a card
    //       a. Blue player may move both Blue and Green pieces
    //       b. Brown player may move both Brown and Green pieces
    //       c. No player may move Red cards
    //       d. Cards may only move in straight lines over cleared paths (Blank)
    //       e. Cards can only be moved the number of spaces permitted
    //          i.   (Blue) Hero      = 1
    //          ii.  (Blue) Vigilanty = unlimited
    //          iii. (Brown) Rats     = 1
    //          iv.  (Brown) Zombie   = unlimited (in the direction pointing)
    //          v.   (Green) Woman    = unlimited
    //          vi.  (Green) Guy      = unlimited
    //       f. No none moves (must move)
    //       g. Cannot move a card back to a place just moved by the other player (or yourself)
    //       h. Each tile has a point value (when captured or thru exit)
    //          i.   (Blue) Hero      = 10
    //          ii.  (Blue) Vigilanty = 5
    //          iii. (Brown) Rats     = 5
    //          iv.  (Brown) Zombie   = 5
    //          v.   (Green) Woman    = 3
    //          vi.  (Green) Guy      = 2
    //          vii. (Red) Corpse     = 2
    //       i. There are rules on who can attack who
    //          (Blue) Hero    - (Brown) Rats, (Brown) Zombie
    //          (Blue) Vigilanty    - (Green) Woman, (Green) Guy
    //          (Brown) Rats   - (Red) Corpse
    //          (Brown) Zombie - (Blue) Hero, (Blue) Vigilanty, (Green) Woman, (Green) Guy (but only in the direction that Zombie is pointing)
    //          (Red) Corpse - none
    //          (Green) Woman/Guy - none
    //    3. Once all the tiles are flipped, the 'end-phase' begins
    //       5 turns remain
    //       Pieces may be moved off the board to add to their own points
    //
    //    Game ends IFF:
    //       A player has no colors of their own, or if there are no valid moves
    //       Highest score wins
```

