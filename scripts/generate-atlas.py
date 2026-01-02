import os, json
from PIL import Image
from rectpack import newPacker

TILE_DIR = "resources/tiles"
JSON_FILE = "resources/tiles/atlas_tiles.json"
ATLAS_FILE = "resources/tiles/Atlas.png"

ATLAS_SOURCE = "resources/tiles/atlas_source.json"

images = []
with open(ATLAS_SOURCE) as f:
    atlas_source_json = json.load(f)


for path in atlas_source_json["source"]:
    img = Image.open(os.path.join(TILE_DIR, path))
    images.append(img)

packer = newPacker()
for idx, img in enumerate(images):
    # print(i)
    packer.add_rect(img.width, img.height, rid=f"{idx}")
packer.add_bin(1024, 1024)
packer.pack()  # type: ignore

atlas = Image.new("RGBA", (1024, 1024))
mapping = {}

for rect in packer.rect_list():
    _, x, y, w, h, id = rect
    img = next(im for idx, im in enumerate(images) if f"{idx}" == id)
    atlas.paste(img, (x, y))

atlas.save(ATLAS_FILE)

