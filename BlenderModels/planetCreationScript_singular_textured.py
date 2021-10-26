import bpy
import math


class Vect3:
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z

    def __str__(self):
        return f"({self.x},{self.y},{self.z})"

    def __repr__(self):
        return f"({self.x},{self.y},{self.z})"

    def __eq__(self, other):
        return self.x == other.x and self.y == other.y and self.z == other.z

    def __hash__(self):
        return int(self.x + self.y * 1000 + self.z * 10000)


def normalize(varray3, r, offset):
    norm = math.sqrt(varray3[0] * varray3[0] + varray3[1] * varray3[1] + varray3[2] * varray3[2])
    if norm == 0:
        return [0, 0, 0]
    #norm = 1
    #r = 1
    return Vect3((varray3[0] * r / norm) + offset[0], (varray3[1] * r / norm) + offset[1],
                 (varray3[2] * r / norm) + offset[2])


RESOLUTION = 20
RADIUS = 10

xmax = RESOLUTION + 1
ymax = RESOLUTION + 1

dx = 1 / RESOLUTION
dy = 1 / RESOLUTION

basePlane = [(dx * x - 0.5, dy * y - 0.5) for y in range(ymax) for x in range(xmax)]

vertsTop = [normalize([x, y, 0.5], RADIUS, [0, 0, 0]) for (x, y) in basePlane]
vertsRight = [normalize([z, 0.5, x], RADIUS, [0, 0, 0]) for (x, z) in basePlane]
vertsFront = [normalize([0.5, x, z], RADIUS, [0, 0, 0]) for (x, z) in basePlane]

vertsBottom = [normalize([y, x, -0.5], RADIUS, [0, 0, 0]) for (x, y) in basePlane]
vertsLeft = [normalize([x, -0.5, z], RADIUS, [0, 0, 0]) for (x, z) in basePlane]
vertsBack = [normalize([-0.5, z, x], RADIUS, [0, 0, 0]) for (x, z) in basePlane]

vertList = [vertsTop, vertsRight, vertsFront, vertsBottom, vertsLeft, vertsBack]

faces1 = [(x + y * xmax, x + y * xmax + 1, x + y * xmax + 1 + xmax) for y in range(ymax - 1) for x in range(xmax) if
          x % xmax != xmax - 1]
faces2 = [(x + y * xmax, x + y * xmax + 1 + xmax, x + y * xmax + xmax) for y in range(ymax - 1) for x in range(xmax) if
          x % xmax != xmax - 1]
faces = faces1 + faces2

objnames = ["PlanetTop", "PlanetRight", "PlanetFront", "PlanetBottom", "PlanetLeft", "PlanetBack"]

verts = [(v.x, v.y, v.z) for v in vertsTop]
vdict = {v: i for i, v in enumerate(vertsTop)}
triangles = [f for f in faces]
nextIndex = len(vertsTop)


def addFace(vertsIn, trianglesIn):
    global triangles
    global vdict
    global verts
    global nextIndex

    for i, v in enumerate(vertsIn):
        if v not in vdict:
            vdict[v] = nextIndex
            verts.append((v.x, v.y, v.z))
            nextIndex += 1

    for (a, b, c) in trianglesIn:
        triangles.append((vdict[vertsIn[a]], vdict[vertsIn[b]], vdict[vertsIn[c]]))


addFace(vertsRight, faces)
addFace(vertsFront, faces)
addFace(vertsBottom, faces)
addFace(vertsLeft, faces)
addFace(vertsBack, faces)

print(f"Triangles ({len(triangles)}) :", triangles)
print(f"Vertices ({len(verts)}) :", verts)


mesh = bpy.data.meshes.new("mesh_Planet")
planetObject = bpy.data.objects.new("PlanetObject", mesh)
mesh.from_pydata(verts, [], triangles)
bpy.context.collection.objects.link(planetObject)

