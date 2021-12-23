import sys
import cv2 as cv
import numpy as np

# Maybe this will be easier if I write this to a separate file and parse again in Python.

# This script will be passed a few arguments when called:
# 0: image resolution
# 1: radii of nodes
# 2: name of the current node
# 3: xPos of node
# 4: yPos of node
print(len(sys.argv))

resolution = 500
radius = 25

# Initialize a new image with given resolution.
img = np.ones((resolution, resolution, 3), dtype=np.uint8) * 255

# Everything after this point will need to be made more general to account for every node, not just a given node.

# Center of the current node.
center = (100, 100)
text_origin = (CENTER[0] - text_size[0] / 2, CENTER[1] + text_size[1] / 2)

cv.circle(img, center, radius, (0, 0, 0), 2)
# Write the id of the node in the center of the circle.
# Gotta make the "1" variable.
cv.putText(img, "1", text_origin, cv.FONT_HERSHEY_SIMPLEX, 1, (0,0,0), 2)

# Save the image in the local directory.
cv.imwrite("Graph.png", img)

print("Done")