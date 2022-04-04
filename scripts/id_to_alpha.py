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
	chars.append(chr(index + alpha_type))

	# Due to the lack of the Stack data structure, a list is used but is read last-to-first.
	for i in range(len(chars)):
		name = name + chars[len(chars) - (i + 1)]

	return name