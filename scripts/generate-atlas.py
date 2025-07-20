import os, json
from PIL import Image
from rectpack import newPacker

TILE_DIR   = "resources/tiles"
JSON_FILE  = "resources/tiles/atlas_tiles.json"
ATLAS_FILE = "resources/tiles/Atlas.png"

SPLIT_JSON = "resources/tiles/split.json"

images = []
with open(SPLIT_JSON) as f:
    split_json = json.load(f)
# print(split_json)
for (k, v) in split_json.items():
    img = Image.open(os.path.join(TILE_DIR, k))
    for (idx, i) in enumerate(v):
        x = i["region"]["x"]
        y = i["region"]["y"]
        w = i["region"]["w"]
        h = i["region"]["h"]
        split_img = img.crop((x, y, x+w, y+h))
        images.append((k[:-4], idx, split_img))       # (name, PIL.Image)

packer = newPacker()
for (id, idx, img) in images:
    # print(i)
    packer.add_rect(img.width, img.height, rid=f"{id}${idx}")
packer.add_bin(1024, 1024)
packer.pack() # type: ignore

atlas = Image.new("RGBA", (1024, 1024))
mapping = {}

for rect in packer.rect_list():
    _, x, y, w, h, id = rect
    img = next(im for n,idx, im in images if f"{n}${idx}" == id)
    atlas.paste(img, (x, y))
    name = id.split("$")[0]
    idx = int(id.split("$")[1])
    mapping[name] = {"x": x, "y": y, "w": w, "h": h}

atlas.save(ATLAS_FILE)
output = {
    "sources": {
        "Atlas.png": {
            "size": {
                "w": 1024,
                "h": 1024
            }
        }
    }
}
# print(mapping)
for k, v in mapping.items():
    v_transformed = {"x": v["x"]/1024.0,
               "y": v["y"]/1024.0,
               "w": v["w"]/1024.0,
               "h": v["h"]/1024.0,
               "source": "Atlas.png"}
    output.setdefault(k, []) # type: ignore
    output[k].append(v_transformed) # type: ignore
# print(output)

json.dump(output,
          open(JSON_FILE, "w"), indent=2)
