import traceback
import argparse
import os

import json
import numpy as np

# This is for creating and writing to the svg image.
import drawsvg as draw
# This is for saving the image to other file types.
import cairosvg

# Custom utility functions.
import scripts.get_prop as prop

def normalize_positions():
	min_x = 99999
	min_y = 99999
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

def get_plot_size(vertices, radius, padding, scale):
	leftmost = 0
	rightmost = -99999
	upmost = 0
	downmost = -99999

	# Calculate the center point of each node, as specified in its properties
	positions = {}
	for v in vertices:
		# Get the enumerated coordinate values in the vertex properties and adjust them for the image.
		x = int(radius + padding + scale * float(prop.get_prop(v, 'xPos')))
		y = int(radius + padding + scale * float(prop.get_prop(v, 'yPos')))
		positions.update({v["Id"]: (x,y)})
		# Find the extreme coordinates.
		if x < leftmost:
			leftmost = x
		if x > rightmost:
			rightmost = x
		if y < upmost:
			upmost = y
		if y > downmost:
			downmost = y

	# Return the list of positions for each vertex, as well as the calculated size of the image.
	return positions, (downmost - upmost + radius + padding, rightmost - leftmost + radius + padding)

def save_file(file_name, d, save_as_svg, save_as_png, save_as_pdf):
	# Initially save the image as a temporary svg file to be read again into a different data type.
	d.save_svg(f"{file_name}_temp.svg")

	# Save to file types (svg, png, pdf) if desired.
	if save_as_svg:
		d.save_svg(f"{file_name}.svg")
		print(f"Saved to {file_name}.svg")
	if save_as_png:
		cairosvg.svg2png(url=f"{file_name}_temp.svg", write_to=f"{file_name}.png")
		print(f"Saved to {file_name}.png")
	if save_as_pdf:
		cairosvg.svg2pdf(url=f"{file_name}_temp.svg", write_to=f"{file_name}.pdf")
		print(f"Saved to {file_name}.pdf")
	
	# Delete the temp file.
	os.remove(f"{file_name}_temp.svg")

def draw_edges(graph, positions, plot_size):
	edges = graph['Edges']

	# Draw the edges and weights, if applicable.
	for i in range(len(edges)):
		# Extract the initial and terminal nodes this edge connects to.
		initial = int(edges[i]['Initial'])
		terminal = int(edges[i]['Terminal'])

		# Cache coordinates of each node.
		# Half the plot size is subtracted so the math is centered around the top-left corner instead of the middle of the image.
		start_x = positions[initial][0] - plot_size[1] / 2
		start_y = positions[initial][1] - plot_size[0] / 2
		end_x = positions[terminal][0] - plot_size[1] / 2
		end_y = positions[terminal][1] - plot_size[0] / 2
		
		# Find the slopes of the line connecting the node.
		dx = start_x - end_x
		dy = start_y - end_y

		# Calculate the unit vector pointing from the terminal node to the initial node.
		magnitude = np.sqrt(dx*dx + dy*dy)
		back_unit_vector = (dx / magnitude, dy / magnitude)

		# Place the origin of the text at the average point of the line.
		text_origin = (int((start_x + end_x) / 2), int((start_y + end_y) / 2))

		# Only perform if the graph is directed.
		if graph['Directed']:
			# Move the text origin 30% of the way from the terminal to initial node.
			# This way, in a digraph, the edge weight is writted closer to the node it enters instead of the average point of the line.
			# This ensures that in a 2-way connection, the weights are not written on top of each other.
			end_x = end_x + 1.3 * radius * back_unit_vector[0]
			end_y = end_y + 1.3 * radius * back_unit_vector[1]
			# Scale the positions back up (since they were calculated based on a unit vector) and reset the origin point.
			text_origin = (end_x + 30 * back_unit_vector[0], end_y + 30 * back_unit_vector[1])

			# Draw an arrowhead at the end of the line.
			# This code is taken straight from the documentation and I don't really get how it works.
			# For some reason, the arrowheads disappear when converting to png.
			arrow = draw.Marker(-1, -5, 9, 5, scale=0.5, orient='auto')
			arrow.append(draw.Lines(-1, -5, -1, 5, 9, 0, fill='#000000', close=True))
			# Draw a line from start to end.
			d.append(draw.Line(start_x, -start_y, end_x, -end_y, stroke_width=element_thickness, stroke='#000000', marker_end=arrow))
		# Only perform if the graph is *not* directed.
		else:
			# Draw a straight line from the initial to the terminal node.
			d.append(draw.Line(start_x, -start_y, end_x, -end_y, stroke_width=element_thickness, stroke='#000000'))

		# Get the weight of this edge.
		weight = prop.get_prop(edges[i], 'weight')
		# If the edge has a weight property.
		if weight != None:
			# Calculate some offset to try to get the text off of the line.
			offset_x = 0
			offset_y = 0 
			# Find which direction to offset the text.
			if dy > dx:
				offset_y = 30
			else:
				offset_x = 30
			# Write the text at the calculated + offset position.
			d.append(draw.Text(str(weight), 0.75 * radius, text_origin[0] - offset_x, -text_origin[1] + offset_y, fill='#ff0000', text_anchor='middle', valign='middle'))

def draw_nodes(graph, positions, plot_size):
	vertices = graph['Vertices']

	for i in range(len(vertices)):
		# Convert integer id to alpha name.
		text = vertices[i]['Name']

		# Find origin point of the node.
		origin = (positions[vertices[i]["Id"]][0] - plot_size[1]/2, positions[vertices[i]["Id"]][1] - plot_size[0]/2)
		
		# Draw a circle centered at that origin with radius as calculated above.
		color = prop.get_prop(vertices[i], "color")
		if color == None:
			d.append(draw.Circle(origin[0], -origin[1], radius, fill='#ffffff', stroke_width=element_thickness, stroke='#000000'))
		else:
			d.append(draw.Circle(origin[0], -origin[1], radius, fill=f'#{color}', stroke_width=element_thickness, stroke='#000000'))
		
		if not bool(prop.get_prop(vertices[i], "hideName")):
			# Write the id/name of the node in the center of the circle.
			font_size = (1.8 * radius) / len(text)
			d.append(draw.Text(text, font_size, origin[0] + 1, -origin[1] + 5, fill='#000000', text_anchor='middle', valign='middle', stroke='#ffffff', stroke_width=str(font_size / 100)))


if __name__ == "__main__":
	parser = argparse.ArgumentParser(description='Converts a generated .graph file into a visual output.')

	parser.add_argument('input', type=str, help='.graph format file to draw')

	parser.add_argument('-s', '--scale', required=False, type=int, default=1024, help='Output image size')
	parser.add_argument('-o', '--out-dir', required=False, type=str, default=".", help="Directory to save the output images in")

	parser.add_argument('--svg', action='store_true', required=False, help='Output as an svg image')
	parser.add_argument('--png', action='store_true', required=False, help='Output as a png image')
	parser.add_argument('--pdf', action='store_true', required=False, help='Output as a pdf')

	parser.add_argument('--transparent', action='store_true', required=False, help='Use a transparent background?')

	args = parser.parse_args()

	if not (args.png or args.svg or args.pdf):
		print("Specify an output format: --svg, --png, or --pdf")
		exit(1)

	try:
		resolution = args.scale
		file = open(args.input, "r")

		folder = args.out_dir
		# Remove trailing '/' since that gets added back in later
		if folder[-1] == '/':
			folder = folder[:-1]

		save_as_svg = args.svg
		save_as_png = args.png
		save_as_pdf = args.pdf

		# The radius of the nodes.
		# Calculated as a proportion of the resolution.
		radius = int(resolution / 32)
		# Scale of the elements. Used to adjust the size of the elements based on the image resolution.
		scale = int(resolution / 5)
		# Thickness of things like the stroke width. Scales with resolution.
		element_thickness = 0.01 * scale
		# Extra space around the border of the image.
		# Adds white space between the edge of the nodes and the outside of the image.
		padding = 32 * element_thickness

		# Load JSON data from the given file.
		json_str = file.read()
		data = json.loads(json_str)
		# Extract lists containing vertex and edge data.
		vertices = data['Vertices']
		edges = data['Edges']

		# Adjust coordinates so the top-leftmost node is at (0, 0).
		normalize_positions()
		# Cache node positions & calculate image size based on that information.
		positions, plot_size = get_plot_size(vertices, radius, padding, scale)

		# Create a new svg image with the previously determined size.
		d = draw.Drawing(plot_size[1], plot_size[0], origin='center', displayInline=False)
		if not args.transparent:
			# Draw a white rectangle taking up the entire image to act as a background.
			d.append(draw.Rectangle(-plot_size[1] / 2, -plot_size[0] / 2, plot_size[1], plot_size[0], fill='#ffffff'))

		# Draw the graph
		draw_edges(data, positions, plot_size)
		draw_nodes(data, positions, plot_size)

		# Make the file name the same as the name of the graph, as specified in the JSON file and replaced spaces with underscores.
		file_name = data['Name'].replace(" ", "_")
		# Save the image to the specified file types.
		save_file(f"{folder}/{file_name}", d, args.svg, args.png, args.pdf)
	except Exception as e:
		print("DrawGraph.py:" + str(e))
		traceback.print_exc()