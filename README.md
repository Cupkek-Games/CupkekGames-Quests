# CupkekGames Quests

A small quest framework on the CupkekGames Data architecture: authored quest
definitions in a catalog, and a saved log of small states that name them by
`CatalogKey`. A definition is never saved.

## The parts

- **`QuestDefinition`**: what a quest is. A title, a description, objectives,
  rewards (`IQuestReward : IFeature`) and game data (`IQuestFeature : IFeature`)
  as `[SerializeReference]` lists. `Ordered` makes the objectives complete one
  after another.
- **`QuestObjectiveDefinition`**: plain data. A `Kind` ("cook", "hire",
  "boss"), a `Key` (which meal, which hero; empty accepts any) and a `Required`
  count. The game owns the vocabulary.
- **`QuestSO`** and **`QuestCatalog : AssetCatalog<QuestSO>`**: authored quests,
  listed by key. A generated quest is authored too, as a template.
- **`QuestState`**: a quest the player has, and the only part that is saved:
  its `CatalogKey`, progress per objective, status, an optional deadline, and
  per-feature state (`IFeatureStateData`, like `InventoryItem`). **`QuestRoll`**
  is the package's own feature state: the key and count each objective rolled,
  for a quest generated from a template.
- **`QuestLog`**: the player's quests (`IData`, saved with the game state).

## Using it

```csharp
QuestLog log = new QuestLog();
log.Bind(key => catalog.GetValue(key.Key).Definition); // after creating or loading

QuestState story = log.Add(storyKey);                  // an authored quest

QuestState contract = new QuestState(templateKey, deadline: 24); // a generated one
contract.GetOrCreateState<QuestRoll>().Set(0, "Camp", 3);        // what it rolled
log.Add(contract);

log.Attach(offer);                 // show a quest that is not taken yet

log.Signal("cook", "stew");        // something happened: matching objectives move
log.Report("fame", null, 3);       // a value the game owns: objectives rise to it
log.Tick(2);                       // time passed: deadlines run down, 0 fails

log.Ready += q => ...;             // every objective done
log.TurnIn(story);                 // Ready -> Completed, rewards granted
```

A quest is `Active`, then `Ready` when every objective is done, then
`Completed` on `TurnIn`, or `Failed` by its deadline or `Fail`. Turning in is the
game's call: a quest with no ceremony can turn itself in from the `Ready` event.
Rewards are handed the quest (`Grant(quest)`), so they can read its roll or
feature state.

Saving: `QuestState` is an opt-in data contract (its key, progress, status,
deadline and feature state). Register your `IFeatureStateData` types with your
serializer.
