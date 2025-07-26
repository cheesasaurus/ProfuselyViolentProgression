# Design notes

The only solution I can think of for balancing pure power-creep mechanics, is moving them from individual unlocks, to a new global unlock system. Once the threshold is reached, the mechanic is unlocked for all players. The unlock should be retroactive (for new players joining afterwards)

- Unlock threshold (one of the following. maybe some fallback for low player count):
  - X percent of players active in last Y hours accomplished the goal at any time in history.
  - X count of players active in last Y hours accomplished the goal at any time in history.
  - X count of players accomplished the goal at any time in history.

## Gatekept mechanics

Power creep to deal with. Stay locked until the threshold is reached, then unlock for everybody:

- prison cells (from Vincent)
  - dominate. This is fine to unlock individually through quests. But it should also be unlocked for everyone when prison cells are unlocked. 
  - glass bottles? (for use with prison cells). probably preferable to add alternative ways for lower-level players to get bottles.
    - idea: when a player puts down a prison cell for the first time, it spawns with one bottle inside. Be careful with that bottle; it's the only freebie you'll get.
  - fishing pole? plenty of other ways to get fish early game. Will leave this unlock as is.
  - change the prison cell recipe to copper bars and regular planks? There are ways to get iron bars and reinforced planks early, but its unknown/difficult for casual players.
- weapon coatings (from Stavros)
  - 1x corrupted fish gives 12 sap, which is enough to make a coating with alchemy floors. The flowers are not hard to get.
- Blood Homogenizer (from Lucile)
- count of unlocked passive slots
  - each slot unlock should also unlock 1 passive skill. The 5 passive skills should synergize, with a low skill floor. For players who already had the skill unlocked, unlock 1 random skill from the same tier. (regular stygie vs greater stygie)
  - Increase the stygie cost of passive unlocks, to offset all the freebies?
  - Automatically equip the associated skill when the slot is unlocked. 
  - The most noob-friendly build to start with is probably a tank build. This should help counteract the fast TTK of high-damage builds, and give players a feeling that they had a chance to do something.
    - skill1: Bastion
    - skill2: Dark Enchantment
    - skill3: Feral Haste (wolf speed crucial for running away. regular move speed helps survive AOE spells).
    - skill4: Turbulent velocity (move speed to help survive AOE spells)
    - skill5: Hunger for Power
  - Bosses that give passive slots:
    - Elena
    - Cassius
    - Cyril
    - Jakira
    - Simon

## Catchup mechanics

Catchup unlocks for late joiners / slower players. Unlock for everybody on a threshold, but can be immediately unlocked individually.

- Crossbow recipe (from Rufus). Basic ranged weapon.
- Blood rose brew (T1 research)
- Merciless charge (T3 spell point) (from Quincey). Automatically unlock and equip if no ult equipped.
- Veil of blood (T4 spell point) (from beatrice). Automatically unlock and equip if no dash equipped.
- Elixir of the prowler (from meredith). Gives move speed and veil CD rate.
- Elixir of the beast (from frostmaw). Gives extra health and healing received.
- Pistols recipe (from Jade)
- Iron whip recipe (from Domina)
- Dominate horse (from quest)
- Shroud of the forest (from Old Wanderer)
- Rage / Witch pots
- Slashers recipe (from Jakira)

## Chat commands

Chat command to check state of global unlocks. Doesn't need to show the catchup stuff, just what's being gatekept.


## Unlock at start

Some QOL things could immediately be unlocked at the start.

- Treasury floors


## Configuration

3 main categories

- prefab guids to remove from boss reward sets
- global progress triggers and associated rewards
  - global triggers:
    - killed vblood
    - unlocked recipe
    - unlocked shapeshift
    - unlocked blueprint
  - rewards:
    - recipe unlocks
    - shapeshift unlocks
    - blueprint unlocks
    - passive ability unlocks
    - passive points to add
    - spell book ability unlocks
- things to unlock at start
  - each category that could be a reward, i guess
