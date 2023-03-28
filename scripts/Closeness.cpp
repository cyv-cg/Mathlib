#include "Closeness.h"

// Arguments:
// 1) path to .adjmat file
// 2) true/false whether or not to use normalized closeness
int main(int argc, char* argv[])
{
	string fileName = argv[1];

	vector<vector<u16>> matrix = ParseMatrix(fileName);
	ComputeCloseness(matrix, matrix.size(), argv[2] == "true", fileName.replace(fileName.find(".adjmat"), 7, ".closeness"));

	return 0;
}

vector<vector<u16>> ParseMatrix(string fileName)
{
	// Initialize length to something it can't realistically be.
	int size = -1;

	string line;
	ifstream file(fileName);

	// Open file.
	if (file.is_open())
	{
		// Read line to determine the size of the matrix.
		// Since the matrix is square, we just need the length of the first line.
		while (getline(file, line))
		{
			// Find length and cache.
			if (size == -1)
			{
				size = line.length();
				// Once we have the length, we can stop reading.
				break;
			}
		}
		file.close();
	}
	else
	{
		std::cout << "Could not open file." << endl;
		return {{}};
	}

	// Create a list of vectors to act as the adjascency matrix.
	vector<vector<u16>> matrix;
	matrix.reserve(size);

	file.open(fileName);
	// Get all values and append to the matrix.
	for (int i = 0; i < size; i++)
	{
		vector<u16> row;
		row.reserve(size);

		getline(file, line);
		
		for (int j = 0; j < line.length(); j++)
		{
			// Subtract 48 because the ASCII value for '0' is 48.
			row.push_back(int8_t(line[j] - '0'));
		}
		matrix.push_back(row);
	}

	// Finally close the file since we have all necessary information.
	file.close();

	return matrix;
}

int* PathFinder(vector<vector<u16>> matrix, int src, int size)
{
	// Array to calculate the minimum distance for each node.
	int *distance = (int*)malloc(size * sizeof(int));
	// Boolean array to mark visited and unvisited for each node.                      
	bool Tset[size];
	
	// Initialize values.
	for (int k = 0; k < size; k++)
	{
		distance[k] = INT_MAX;
		Tset[k] = false;    
	}
	
	// Source vertex distance is set 0
	distance[src] = 0;
	
	for (int k = 0; k < size; k++)                           
	{
		int m = MinDist(distance, Tset, size); 
		Tset[m] = true;
		for (int k = 0; k < size; k++)                  
		{
			// updating the distance of neighbouring vertex
			if (!Tset[k] && matrix[m].at(k) && distance[m] != INT_MAX && distance[m] + matrix[m].at(k) < distance[k])
				distance[k] = distance[m] + matrix[m].at(k);
		}
	}

	return distance;
}

void ComputeCloseness(vector<vector<u16>> matrix, u16 size, bool normalize, string outputFile)
{
	// Create a new output file to store the data.
	ofstream output (outputFile);
	// Loop through each vertex.
	for (int vertIndex = 0; vertIndex < size; vertIndex++)
	{
		// Compute the length of the shortest path to every other vertex.
		int *distances = PathFinder(matrix, vertIndex, size);
		// Sum up the path lengths.
		double sumOfDistances = 0.0;
		for (int i = 0; i < size; i++)
			sumOfDistances += *(distances + i);
		// Compute the closeness value.
		double closeness = 1.0 / sumOfDistances;

		// Use the normalized formula, if specified.
		if (normalize)
			closeness *= size - 1;

		// Write values to a file.
		output << std::fixed << std::setprecision(16) << closeness;
		if (vertIndex < size - 1)
			output << std::endl;

		// Free up the space used by the distances array.
		free(distances);
	}
	output.close();
}
