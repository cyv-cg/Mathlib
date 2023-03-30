#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <iomanip>
#include <limits.h>

#define u16 uint16_t

using namespace std;

int* PathFinder(vector<vector<u16>> matrix, int src, int size);
int MinDist(int distance[], bool Tset[], int size)
{
	int minimum = INT_MAX, ind;
	
	for (int k = 0; k < size; k++) 
	{
		if (Tset[k] == false && distance[k] <= minimum)      
		{
			minimum = distance[k];
			ind = k;
		}
	}
	return ind;
}

vector<vector<u16>> ParseMatrix(string fileName);

void ComputeCloseness(vector<vector<u16>> matrix, u16 size, bool normalize, string outputFile);