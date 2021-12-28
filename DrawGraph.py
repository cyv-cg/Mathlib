import sys
import json
# import cv2 as cv
import numpy as np
import drawSvg as draw

# import pyvips

from svglib.svglib import svg2rlg
from reportlab.graphics import renderPDF, renderPM

# import cairosvg

# This is exactly the same thing that's happening in the C# function in the Vertex class.
def id_to_alpha(id):
    # 65 for uppercase
    # 97 for lowercase
	alpha_type = 65

	name = ""
	chars = []

	index = id

	while index > 25:
		remainder = index % 26
		chars.append(chr(remainder + alpha_type))
		index = int(index / 26)
		if index < 26:
			index = index - 1
	chars.append(chr(index + alpha_type))

	for i in range (len(chars)):
		name = name + chars[len(chars) - (i + 1)]

	return name

def normalize_positions():
	min_x = 0
	min_y = 0
	# Find the minimum coordinates.
	for v in vertices:
		if v['Properties'][0]['Value'] < min_x:
			min_x = v['Properties'][0]['Value']
		if v['Properties'][1]['Value'] < min_y:
			min_y = v['Properties'][1]['Value']
	# Offset all coordinates by the minimum to they are normalized around (0, 0).
	for v in vertices:
		v['Properties'][0]['Value'] = v['Properties'][0]['Value'] - min_x
		v['Properties'][1]['Value'] = v['Properties'][1]['Value'] - min_y

# This script will be passed a few arguments when called:
# 1: image resolution
# 2: JSON file of the graph

resolution = int(sys.argv[1])
radius = int(resolution / 32)

padding = 32

scale = int(resolution / 5)
element_thickness = 4
# text_face = cv.FONT_HERSHEY_DUPLEX
# text_scale = 0.01 * scale

file = open(sys.argv[2], "r")
json_str = file.read()

data = json.loads(json_str)

vertices = data['Vertices']
edges = data['Edges']

normalize_positions()

leftmost = 0
rightmost = 0
upmost = 0
downmost = 0

# Calculate the center point of each node, as specified in its properties
positions = []
for v in vertices:
    x = int(radius + padding + scale * float(v['Properties'][0]['Value']))
    y = int(radius + padding + scale * float(v['Properties'][1]['Value']))
    positions.append((x, y))

    if x < leftmost:
        leftmost = x
    if x > rightmost:
        rightmost = x
    if y < upmost:
        upmost = y
    if y > downmost:
        downmost = y

plot_size = (downmost - upmost + radius + padding, rightmost - leftmost + radius + padding)

# center_x = 0
# center_y = 0
# for p in positions:
#     center_x = center_x + p[0]
#     center_y = center_y + p[1]
# center_x = center_x / len(positions)
# center_y = center_y / len(positions)
# graph_center = (center_x, center_y)

# Initialize a new image with given resolution.
img = np.ones((plot_size[0], plot_size[1], 3), dtype=np.uint8) * 255

d = draw.Drawing(plot_size[1], plot_size[0], origin='center', displayInline=False)
d.append(draw.Rectangle(-plot_size[1]/2, -plot_size[0]/2, plot_size[1], plot_size[0], fill='#ffffff'))

for i in range(len(edges)):
    initial = int(edges[i]['Initial']['Id'])
    terminal = int(edges[i]['Terminal']['Id'])
    # cv.line(img, positions[initial], positions[terminal], (0, 0, 0), element_thickness, cv.LINE_AA)

    start_x = positions[initial][0] - plot_size[1]/2
    start_y = positions[initial][1] - plot_size[0]/2
    end_x = positions[terminal][0] - plot_size[1]/2
    end_y = positions[terminal][1] - plot_size[0]/2
    
    dx = start_x - end_x
    dy = start_y - end_y

    back_unit_vector = (dx / np.sqrt(dx*dx + dy*dy), dy / np.sqrt(dx*dx + dy*dy))

    text_origin = (int((start_x + end_x) / 2), int((start_y + end_y) / 2))

    if data['Directed']:
        end_x = end_x + 1.3 * radius * back_unit_vector[0]
        end_y = end_y + 1.3 * radius * back_unit_vector[1]
        
        text_origin = (end_x + 30 * back_unit_vector[0], end_y + 30 * back_unit_vector[1])

        arrow = draw.Marker(-1, -5, 9, 5, scale=0.5, orient='auto')
        arrow.append(draw.Lines(-1, -5, -1, 5, 9, 0, fill='#000000', close=True))
        d.append(draw.Line(start_x, -start_y, end_x, -end_y, stroke_width=element_thickness, stroke='#000000', marker_end=arrow))
    else:
        d.append(draw.Line(start_x, -start_y, end_x, -end_y, stroke_width=element_thickness, stroke='#000000'))

    weight = int(edges[i]['Properties'][0]['Value'])
    # cv.putText(img, str(weight), text_origin, text_face, text_scale * 0.75, (0, 0, 255), int(element_thickness * 0.75), cv.LINE_AA)
    
    offset_x = 0
    offset_y = 0 

    if dy > dx:
        offset_y = 30
    else:
        offset_x = 30

    d.append(draw.Text(str(weight), 0.75 * radius, text_origin[0] - offset_x, -text_origin[1] + offset_y, fill='#ff0000', text_anchor='middle', valign='middle'))

for i in range(len(vertices)):
    # Center of the current node.
    # center = positions[i]
    # text = str(vertices[i]['Id'])
    text = id_to_alpha(int(vertices[i]['Id']))

    # text_size, _ = cv.getTextSize(text, text_face, text_scale, element_thickness)
    # text_origin = (center[0] - int(text_size[0] / 2), center[1] + int(text_size[1] / 2))

    # cv.circle(img, center, radius, (255, 255, 255), -1)
    # cv.circle(img, center, radius, (0, 0, 0), element_thickness, cv.LINE_AA)
    # cv.putText(img, text, text_origin, text_face, text_scale, (0, 0, 0), element_thickness, cv.LINE_AA)

    origin = (positions[i][0] - plot_size[1]/2, positions[i][1] - plot_size[0]/2)
    d.append(draw.Circle(origin[0], -origin[1], radius, fill='#ffffff', stroke_width=element_thickness, stroke='#000000'))
    # Write the id of the node in the center of the circle.
    d.append(draw.Text(text, 1.8 * radius, origin[0] + 1, -origin[1] + 5, fill='#000000', text_anchor='middle', valign='middle'))


# Save the image in the local directory.
# file_name = data['Name'].replace(" ", "_") + ".png"
# cv.imwrite(file_name, img)

file_name = data['Name'].replace(" ", "_") + ".svg"
print(f"Saved to {file_name}")
d.saveSvg(file_name)

drawing = svg2rlg("Sample_Graph.svg")
renderPM.drawToFile(drawing, "file.png", fmt="PNG")