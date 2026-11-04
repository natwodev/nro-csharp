NPoint characterization harness (task E-01)
===========================================

What it is: the ORIGINAL nro/player/NPoint.java (copied unchanged from the Java source tree at build time) compiled against small test doubles of the
classes around it (Player, Detu, ItemTime, EffectSkill, SetClothes, MapService, InventoryService, ...), driven by the scenarios in
nro/harness/NPointScenarios.java.txt. Its output (the inputs of every scenario plus everything the original did) is ../npoint-characterization.json, which
NPointCharacterizationTests replays on the C# port.

The doubles are not models of the game. They hold the data NPoint reads, with the field types of the original, and log the calls NPoint makes. The only
code in them that comes from the original is copied VERBATIM by the build script (Util: nextInt/nextLong/isTrue/getOne/Crisnext/CrisGH/canDoWithTime/getDistance;
SkillUtil: six formulas; ItemService.getPercentTrainArmor; the black ball and skill id constants), and its SHA-256 is written into the JSON ("meta").
Player.isPl / isPlMan / getBot are copied by hand without the "instanceof NonInteractiveNPC" test (that class is not part of the harness).
MapService predicates answer from per-map flags set by the scenario (the real predicates are map id tables owned by another track).

Run (JDK 17 on PATH, from port-csharp):
    python -I tools/npoint/run_npoint_characterization.py
Files here are *.java.txt so no tool mistakes them for sources of the C# solution; the script copies them to a temp tree without ".txt".
The random source of the original (Utils.Util.rand) is replaced per operation by new Random(seed); the port uses SeededRandomProvider(seed), a bit-exact port of it.
The original reads the real clock, so scenarios only use "expired" = 1 and "active" = 2^61 for every timestamp.
