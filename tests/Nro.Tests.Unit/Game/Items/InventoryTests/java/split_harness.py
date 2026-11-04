import os, re, sys
# usage: python split_harness.py <harness.java.txt> <tmpdir>   -> writes <tmpdir>/<MainClass>.java and every "// FILE:" stub
src, out = sys.argv[1], sys.argv[2]
lines = open(src, encoding="utf-8").read().splitlines()
main, cur, files = [], None, {}
for ln in lines:
    m = re.match(r"// FILE: (\S+)", ln)
    if m:
        cur = m.group(1); files[cur] = []; continue
    if cur is None: main.append(ln)
    else: files[cur].append(re.sub(r"^// ?", "", ln))
cls = re.search(r"public class (\w+)", "\n".join(main)).group(1)
os.makedirs(out, exist_ok=True)
open(os.path.join(out, cls + ".java"), "w", encoding="utf-8").write("\n".join(main) + "\n")
for rel, body in files.items():
    p = os.path.join(out, rel); os.makedirs(os.path.dirname(p), exist_ok=True)
    open(p, "w", encoding="utf-8").write("\n".join(body) + "\n")
print(cls)
