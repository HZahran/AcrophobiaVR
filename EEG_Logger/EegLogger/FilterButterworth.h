// MODULE: FilterButterworth.h
//: written by: Henning Nordholz
//+       date: 11-Jan-00
//+ 
//+ Description:
//. 	Declaration of class CFilterButterworth.

#pragma once

using namespace std;

class CFilterButterworth
//: Butterworth filter (24 dB/oct, degree 4), for low cutoff, high cutoff or notch.
//. Single pass filter (may cause a frequency dependend phase shift)
{
public:
	float*	m_pnData;							// Pointer to data.

	enum eFilterType 
	{
		eLowCutoff, eHighCutoff 
	};
	
	CFilterButterworth(int nChannels, eFilterType eType, float fSlope, double dSamplingRate, float* pfFrequency);
		//: Constructor
		//+ 
		//+ Parameter(s)
		//+ 	nChannels			- Number of channels to filter
		//+ 	eType				- Filter type
		//+		fSlope				- Slope, only 24 and 12 are allowed.
		//+ 	dSamplingRate		- Sampling rate in Hertz
		//+ 	pfFrequency			- Array of frequencies, one frequency for each channel
		//							  If a value is less or equal 0, the channel will not be filtered.

	~CFilterButterworth();

	void StoreDCOffsets();
		// Store the DCOffsets.

	void DoFilter(DWORD dwStart, DWORD dwPoints);
		// This function does the filtering
protected:
	void complexsqrt(float ArgReal, float ArgImag, float *SqrtReal, float *SqrtImag);
		// This function is used to calculate the complex squareroot in the calculation
		// of the notchfilter.

	struct Coefficients
	//; Filter coefficients of one channel.
	{
		float	m_A[6];
		float	m_B[6];
	};
	
	vector<Coefficients> m_tblCoefficients;	// Table of coefficients for all channels.
	vector<bool> m_tblFiltered;				// Flag for each channel, wether it is filtered.

	int				m_nChannels;			// Number of channels
	eFilterType		m_eFilterType;			// Filter type
	int				m_nDegree;				// Number of poles, order, 2 = 12 dB, 4 = 24 dB
	float*			m_pTmpStartSettings;	// Table of start settings of filters.
	float*			m_pfDCOffsets;			// Table of DCOffsets
};
