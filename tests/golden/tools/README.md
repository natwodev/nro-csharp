# PlayerDAO `createNewPlayer` golden provenance

`PlayerNewGolden.java` is a standalone, database-free Java harness for the
string construction and 47 ordered INSERT arguments in
`jbcd/dao/PlayerDAO.java:createNewPlayer` (source lines 57–532).  It preserves
the source's branch/order/value logic; every wall-clock value is deliberately
replaced with the fixed epoch `1700000000000` so that the result is
reproducible.  The two Java `Timestamp` arguments are represented by the
explicit `*_epoch_ms` JSON fields used by these golden files.

The source's `items_box_Collection`, `myFather`, `myMother`, `BoughtSkill`,
`LastTimeLoginGame` names are emitted with the established snake_case golden
keys.  This is a key-name normalization only; it does not alter the 47 INSERT
argument values or their order.

## Exact local command (Windows / JDK 17)

Run from `D:\nam\ngocrong\nro-csharp`.  `<temp>` must be an empty disposable
directory outside the repository.

```powershell
$temp = 'C:\Users\HQSOFT\AppData\Local\Temp\nro-player-new-golden'
$jar = 'D:\nam\ngocrong\java-run\runtime\AWN_Version.jar'
javac -encoding UTF-8 -cp $jar -d $temp tests\golden\tools\PlayerNewGolden.java
java -cp "$temp;$jar" PlayerNewGolden "$temp\out"
Get-FileHash "$temp\out\player-new-gender*.json", tests\golden\player-new-gender*.json -Algorithm SHA256
```

JDK measured on this machine: Microsoft OpenJDK `17.0.19+10-LTS` (JDK 17 LTS).
The jar supplies `org.json.simple.JSONArray` and `org.json.simple.JSONObject`.

## Measured source/dependency/golden hashes

| Artifact | SHA-256 |
| --- | --- |
| `source-game/SRC_6/AWN_Version/AWN_Version/src/jbcd/dao/PlayerDAO.java` | `67088b91222404e1bc9278652cb4ccdf625c8d3d938c11c9595e640b2a1c040b` |
| `java-run/runtime/AWN_Version.jar` | `663aafbf044453ca7275d75d9b96306048afaeb3a0d7f794ff7d49a1a0b1d516` |
| `tests/golden/player-new-gender0.json` | `d77bba8f24155e10aefadfa125f45515eaabe84f797b9c1e60249505bbdd5010` |
| `tests/golden/player-new-gender1.json` | `b65fe4f7400c51844e95913ed7c56262455c584babde2d9949dbd1f6fa04fc4a` |
| `tests/golden/player-new-gender2.json` | `8bc5793be7424fdce82d075d09a418f9138cdbd12298dd397859e03e15009a7c` |

The recorded run is [run-log.txt](run-log.txt): all three generated JSON files
matched their existing golden counterparts byte-for-byte.  Temporary compiler
and output files were deleted after that comparison; the harness never writes
to `tests/golden`.
