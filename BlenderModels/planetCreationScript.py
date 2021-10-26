import bpy
import math

def normalize(varray3, r, offset):
    norm = math.sqrt(varray3[0] * varray3[0] + varray3[1] * varray3[1] + varray3[2] * varray3[2])
    if norm == 0:
        return [0,0,0]
    return [(varray3[0] * r / norm) + offset[0], (varray3[1] * r / norm) + offset[1], (varray3[2] * r / norm) + offset[2]]

RESOLUTION = 10
RADIUS = 10

xmax = RESOLUTION + 1
ymax = RESOLUTION + 1

dx = 1 / RESOLUTION
dy = 1 / RESOLUTION

basePlane = [ (dx * x - 0.5, dy * y - 0.5) for y in range(ymax) for x in range(xmax) ]

vertsTop = [ normalize([x,y,0.5], RADIUS, [0,0,0.5]) for (x,y) in basePlane ]
vertsRight = [ normalize([z,0.5,x], RADIUS, [0,0.5,0]) for (x,z) in basePlane ]
vertsFront = [ normalize([0.5,x,z], RADIUS, [0.5,0,0]) for (x,z) in basePlane ]

vertsBottom = [ normalize([y,x,-0.5], RADIUS, [0,0,0]) for (x,y) in basePlane ]
vertsLeft = [ normalize([x,-0.5,z], RADIUS, [0,0,0]) for (x,z) in basePlane ]
vertsBack = [ normalize([-0.5,z,x], RADIUS, [0,0,0]) for (x,z) in basePlane ]

vertList = [ vertsTop, vertsRight, vertsFront, vertsBottom, vertsLeft, vertsBack ]

faces1 = [ (x + y * xmax, x + y * xmax +1, x + y * xmax + 1 + xmax) for y in range(ymax - 1) for x in range(xmax) if x % xmax != xmax - 1 ]
faces2 = [ (x + y * xmax, x + y * xmax + 1 + xmax, x + y * xmax + xmax) for y in range(ymax - 1) for x in range(xmax) if x % xmax != xmax - 1 ]
faces = faces1 + faces2


objnames = [ "PlanetTop","PlanetRight", "PlanetFront", "PlanetBottom", "PlanetLeft", "PlanetBack" ]

planetObject = bpy.data.objects.new("PlanetObject", None)
bpy.context.collection.objects.link(planetObject)

for i in range(len(objnames)):
    mesh = bpy.data.meshes.new("mesh_" + objnames[i])
    object = bpy.data.objects.new("obj_" + objnames[i], mesh)
    mesh.from_pydata(vertList[i], [], faces)
    object.parent = planetObject
    bpy.context.collection.objects.link(object)

