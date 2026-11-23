"""Export the same deterministic ground surface as MeadowWorld.Height, in metres."""
from pathlib import Path
import math

root = Path(__file__).resolve().parents[1] / 'Assets' / 'Art' / 'MapModels'
root.mkdir(parents=True, exist_ok=True)
def height(x,z):
    return 9+math.sin(x*.024+.6)*math.cos(z*.029)*6+math.sin(x*.043)*math.cos(z*.037)*3
with (root/'Grassland.obj').open('w', encoding='utf-8') as f:
    f.write('# Sky Meadow original terrain; metres; Y-up; 240m square\nmtllib Grassland.mtl\no Grassland\nusemtl Grass\n')
    for z in range(121):
        for x in range(121):
            px,pz=x*2-120,z*2-120
            f.write(f'v {px} {height(px,pz):.6f} {pz}\n')
    for z in range(120):
        for x in range(120):
            a=z*121+x+1;b=a+1;c=a+121;d=c+1
            f.write(f'f {a} {c} {b}\nf {b} {c} {d}\n')
(root/'Grassland.mtl').write_text('newmtl Grass\nKd 0.37 0.69 0.24\nKa 0.2 0.3 0.15\n',encoding='utf-8')
print('PASS: exported Grassland.obj: 14,641 vertices / 28,800 triangles')
