# Builds InventoryServiceGenerator.java.txt: method bodies are extracted VERBATIM from the original InventoryService.java, wrapped with small stubs.
import re, sys, hashlib
SRC = sys.argv[1]   # original InventoryService.java
OUT = sys.argv[2]
text = open(SRC, encoding="utf-8").read()
lines = text.split("\n")

def extract(sig_regex, occurrence=0):
    found = 0
    for i, ln in enumerate(lines):
        if re.match(r"^    (public|private) .*" + sig_regex, ln):
            if found == occurrence:
                depth = 0; out = []; started = False
                for j in range(i, len(lines)):
                    out.append(lines[j]); depth += lines[j].count("{") - lines[j].count("}")
                    if "{" in lines[j]: started = True
                    if started and depth == 0:
                        return "\n".join(out), (i + 1, j + 1)
            found += 1
    raise Exception("not found: " + sig_regex)

methods = [
    r"boolean addItemList\(", r"boolean hasOptionTemplateId\(", r"int\[\] isItemIncrementalOption\(",
    r"void sortItems\(", r"void sortItemv2\(", r"void removeItem\(List<Item> items, int index\)", r"void removeItem\(List<Item> items, Item item\)",
    r"void subQuantityItem\(", r"byte getCountEmptyListItem\(",
    r"void updateSet\(", r"void resetSet\(", r"void updateSetNew\(", r"void resetSetNew\(",
    r"int HpItemsInBoxCollection\(", r"int MpItemsInBoxCollection\(", r"int DamageItemsInBoxCollection\(", r"int CritItemsInBoxCollection\(",
    r"void removeAndAddOptionTemplate\(", r"void removeAndAddOptionTemplateHSD\(",
    r"void sendItemBag\(", r"void sendItemBody\(", r"void sendItemBox\(", r"void openBox\(",
]
body = []
ranges = []
for m in methods:
    code, rng = extract(m)
    ranges.append((m, rng))
    if "removeAndAddOptionTemplate" in m:
        # the only edits: make the JDK's own random sources the seeded one, so the output is reproducible
        code = code.replace("Collections.shuffle(availableIds);", "Collections.shuffle(availableIds, Util.rand);   // EDIT: was Collections.shuffle(availableIds)")
        code = code.replace("Random rand = new Random();", "Random rand = Util.rand;   // EDIT: was new Random()")
        assert "EDIT" in code, "the random sources were not replaced"
    body.append(code)

sha = hashlib.sha256(open(SRC, "rb").read()).hexdigest()
header = open("harness_header.txt", encoding="utf-8").read().replace("@SHA@", sha)
rangetxt = "\n".join("//   %-60s lines %d-%d" % (m, a, b) for m, (a, b) in ranges)
main = open("harness_main.txt", encoding="utf-8").read()
stubs = open("harness_stubs.txt", encoding="utf-8").read()
out = header.replace("@RANGES@", rangetxt) + "\n" + main.replace("@METHODS@", "\n\n".join(body)) + "\n" + stubs
open(OUT, "w", encoding="utf-8").write(out)
print("ok", sha)
