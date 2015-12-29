from arcpy import da
import arcpy
import sys
reload(sys)
sys.setdefaultencoding('utf8')
ws_name = str(sys.argv[1])
file_name = str(sys.argv[2])
fc = ws_name + "\\" + file_name + ".shp"
#fc = "C:\\Users\\penguin\\Desktop\\data\\CC.shp"
desc = arcpy.Describe(fc)
Head = ["Data Type: ", "File path: ", "Catalog path: ", "File name: ", "Base name: ",
        "Name: ", "Feature type: ", "hasM: ", "hasZ: ", "Has SpatialIndex: ", "Shape field name: ", "Shape type: "]
Desc = [desc.dataType, desc.path, desc.catalogPath, desc.file, desc.baseName,
        desc.name, desc.featureType, desc.hasM, desc.hasZ, desc.hasSpatialIndex, desc.shapeFieldName, desc.shapeType]
desc_line = []
index = 0
for head in Head:
    desc_line.append(head + str(Desc[index]))
    desc_line.append("\n")
    index += 1

if "Point" == desc.shapeType:
    cursor = da.SearchCursor(fc, ["SHAPE@XY"])
    for row in cursor:
        x, y = row[0]
        desc_line.append("{0}, {1}\n".format(x, y))

if "Polyline" == desc.shapeType:
    cursor = da.SearchCursor(fc, ["OID@", "SHAPE@"])
    for row in cursor:
        desc_line.append("Feature {0}: \n".format(row[0]))
        for point in row[1].getPart(0):
            desc_line.append("{0}, {1}\n".format(point.X, point.Y))
			#desc_line.append("{0}\n".format(point))
    cursor = da.SearchCursor(fc, ["SHAPE@LENGTH"])
    length = 0
    index = 0
    for row in cursor:
        desc_line.append("Length of line {0}: {1}\n".format(index, row[0]))
        index += 1
        length += row[0]
    desc_line.append("Length: {0}\n".format(length))

if "Polygon" == desc.shapeType:
    cursor = da.SearchCursor(fc, ["OID@", "SHAPE"])
    for row in cursor:
        desc_line.append("Feature {0}: \n".format(row[0]))
        for point in row[1]:
            desc_line.append("{0}\t".format(point))
        desc_line.append("\n")

file = open(ws_name + "\\" + file_name + "_analyze.txt", 'w+')
#file = open("C:\\Users\\penguin\\Desktop\\data\\CC" + "_analyze.txt", 'w+')

file.writelines(desc_line)
file.close()
