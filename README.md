# CupkekGames Quests

A small quest framework: authored quests, a saved log, and objectives that move
on signals the game posts.

## The parts

- **`QuestDefinition`**: what a quest is. A title, a description, objectives,
  rewards (`IQuestReward`) and game data (`IQuestFeature`). `Ordered` makes the
  objectives complete one after another.
- **`QuestObjectiveDefinition`**: plain data. A `Kind` ("cook", "hire",
  "boss"), a `Key` (which meal, which hero; empty accepts any) and a `Required`
  count. The game owns the vocabulary.
- **`QuestSO`** and **`QuestCatalog`**: authored quests, listed by key.
- **`QuestLog`**: the player's quests (`IData`, saved with the game state). Each
  entry is a **`QuestState`**: the quest's catalog key (or, for a quest a
  generator built, its definition inline), progress per objective, status and
  an optional deadline. Definitions are never copied into a save.

## Using it

```csharp
QuestLog log = new QuestLog();
log.Bind(catalog);                  // after creating or loading, before anything else

QuestState quest = log.Add("story_01");          // an authored quest
log.Add(generatedDefinition, deadline: 24);      // a generated one, failing after 24 units

log.Signal("cook", "stew");        // something happened: matching objectives move
log.Report("fame", null, 3);       // a value the game owns: objectives rise to it
log.Tick(2);                       // time passed: deadlines run down, 0 fails

log.Ready += q => ...;             // every objective done
log.TurnIn(quest);                 // Ready -> Completed, rewards granted
```

A quest is `Active`, then `Ready` when every objective is done, then
`Completed` on `TurnIn`, or `Failed` by its deadline or `Fail`. Turning in is the
game's call: a quest with no ceremony can turn itself in from the `Ready`
event.

Saving: the log serializes as plain data. Register your `IQuestReward` and
`IQuestFeature` types with your serializer; they are saved only inside inline
(generated) definitions.
