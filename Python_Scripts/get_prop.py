def get_prop(item, prop_name):
	if item['Properties'] == None:
		return None

	# Look through each property of this item.
	for p in item['Properties']:
		# If there is a property with the given key...
		if p['Key'] == prop_name:
			# ...return the value of this property.
			return p['Value']
	# If the property was not found, return nothing.
	return None