import bpy


verts = [ (0,0,0), (0,2,0), (2,2,0), (2,0,0) ]
faces = [ (0,1,2,3) ]

mesh = bpy.data.meshes.new("PlaneMesh")
object = bpy.data.objects.new("PlaneObject", mesh)

bpy.context.collection.objects.link(object)

mesh.from_pydata(verts, [], faces)
