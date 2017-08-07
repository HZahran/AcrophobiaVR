#include "stdafx.h"
#include "./FirstAmp.h"
#include "Device.h"
#include <string>
#include <iostream>
#include <fstream>
#include <math.h>
#include "rapidjson\document.h"
#include "rapidjson\writer.h"
#include "rapidjson\stringbuffer.h"
#include "zmq.hpp"
#include "zmq_addon.hpp"
#include "kiss_fft.h"
#include "kiss_fftr.h"
#include "_kiss_fft_guts.h"

#include ".\GenericData.h"
CGenericData l_GenericData;
CGenericData::DeviceInfo l_DeviceInfo;
char l_szRawDataFile[256] = "C:\\Tmp\\VAmp\\Demo";

#define STANDARD_DATA_AQUISITION 1
#define SQ(a) a*a
#define PI (3.141592653589793)
const int NUM_VALUES_FFT = 100;

// Device
static CDevice *l_pDevice = NULL;
ofstream logfile("log.txt");

// Prototypes
BOOL finishDataRetrieval();
BOOL saveDataToArray(int);

// ----------- MAIN -----------
int main(int argc, char *argv[])
{
	l_pDevice = CDevice::GetInstance();
	if (l_pDevice == NULL)
	{
		printf("Cannot create device instance.\n");
		return 1;
	}



	printf("Device instance created.\n");
	BOOL success = saveDataToArray(IAC_AT_DATA);	// const defined in Device.h

	printf("\nPress any key to exit...\n");
	char c = _getch();

	return 0;
}

BOOL saveDataToArray(int nAcquisitionType)
{
	BOOL bRun = TRUE;

	int nMaxChannels = 0, nAnalogChannels = 0, nRefChanIdx = 0;
	int nDataMode = dmNormal, tblChannelsPos[4], tblChannelsNeg[4];
	double dSamplingRate = 2000; // Default: 2000 - 2 kHz
	LPSAFEARRAY pData;
	CComSafeArray<double, VT_R8> pResolutions;	// table of channel resolution.
	CComSafeArray<char, VT_I1> pChTypes;		// table of channel type.

	if (nAcquisitionType == IAC_AT_CALIBRATION)
	{
		nDataMode = dmNormal;
	}
	// (1) Open device
	if (S_OK != l_pDevice->Open())
		finishDataRetrieval();

	// (2) Setup configuration
	nMaxChannels = l_pDevice->GetChannels();
	nAnalogChannels = l_pDevice->GetAnalogChannels();
	l_pDevice->SetDataMode(nDataMode);
	l_pDevice->SetSamplingRate(dSamplingRate);
	l_pDevice->GetChannelTypes(&pData);
	pChTypes.Attach(pData);
	l_pDevice->GetChannelResolutions(&pData);
	pResolutions.Attach(pData);
	l_pDevice->GetRefChannel(&nRefChanIdx);

	// Prepare for raw data file
	l_DeviceInfo.vChannelNames.clear();
	for (int i = 0; i < nMaxChannels - 1; i++) // without trigger
	{
		CStringW s; s.Format(L"%d", i + 1);
		l_DeviceInfo.vChannelNames.push_back(s);
		float fResolution = 1;
		if (i < (int)pResolutions.GetCount())
		{
			fResolution = (float)pResolutions.GetAt(i);

		}
		l_DeviceInfo.vResolutions.push_back(fResolution);
		l_DeviceInfo.vFormats.push_back(2); // floating
	}
	l_DeviceInfo.dSamplingInterval = (1.0 / double(dSamplingRate)) * 1e6; // µs

																		  // Preparing for raw data file
	l_GenericData.SetEEGFileName(l_szRawDataFile);
	l_GenericData.WriteHeaderInfo(l_DeviceInfo);

	// in case of highspeed mode (> 2 kHz)
	for (UINT n = 0; n < 4; n++)
	{
		// differential pair 2 (channel n - channel REF)
		tblChannelsPos[n] = n;
		tblChannelsNeg[n] = nRefChanIdx; // channel REF
	}

	// (3) start data acquisition
	if (S_OK != l_pDevice->Start(nAcquisitionType, nDataMode, tblChannelsPos, tblChannelsNeg))
	{
		finishDataRetrieval();
	}
	printf("> Start successful\n");
	printf("\t- Channels: %d\n", nMaxChannels);
	printf("\t- Data mode: %s\n", nDataMode == dmNormal ?
		"Normal mode - all channels at 2 kHz" : "Fast mode - 20 kHz, 4 Channels, no auxiliary");
	printf("\t- Sampling rate Hz: %g Hz\n", dSamplingRate);

	int nPoints = 0;
	UINT nBlockCnt = 0;
	DWORD dwStartTime = timeGetTime();

	// Socket
	zmq::context_t context(1);
	zmq::socket_t sender(context, ZMQ_PUSH);
	sender.bind("tcp://*:5557");

	// Array containing history of each node
	#define NUM_OF_NODES 4
	float graph[16][NUM_VALUES_FFT];
	int graphCounter = 0;
	kiss_fftr_cfg real_cfg;
	float real_in[NUM_VALUES_FFT];
	kiss_fft_cpx real_out[NUM_VALUES_FFT];

	real_cfg = kiss_fftr_alloc(NUM_VALUES_FFT, 0, 0, 0);
	float transformedArray[16][NUM_VALUES_FFT];

	while (bRun)
	{
		if (_kbhit())
		{
			char c = char(_getch());
			switch (c)
			{
			case 27: // ESC
				printf("> Exit.\n");
				bRun = FALSE;
				break;
			}
		}
		if (bRun)
		{
			// (4) Serve data
			CComSafeArray<float, VT_R4> saData;		// SafeArray for Monitoring, TestSignal.
			DATE date = 0;
			LPSAFEARRAY sa;
			int nErrorSeverity = 0;
			HRESULT hResult = S_FALSE;
			hResult = l_pDevice->GetData(&sa, &nMaxChannels, &nPoints, &date, &nErrorSeverity);

			// Max values of EEG frequencies
			float alphaMax[16];
			float betaMax[16];
			float gammaMax[16];

			if (hResult == S_OK)
			{
				/*
				Step 1: Measurement ------------------------------------------------------------------------------------------------
				order of channel :
				VAmp 16: EEG: 0..15; AUX: 16, 17; Trigger: 18
				firstAmp 8: EEG: 0..7; AUX: 8, 9; Trigger: 10
				*/
				saData.Attach(sa);
				float *pBuffer = (float *)saData.m_psa->pvData;
				l_GenericData.WriteData(pBuffer, nPoints, nMaxChannels - 1, 1); // ignore trigger channel

				// Create history of incoming data for each node

				if (graphCounter < NUM_VALUES_FFT)
				{
					for (int i = 0; i < 16; i++)
					{
						graph[i][graphCounter] = (float)(*(pBuffer + i) * (float)pResolutions.GetAt(i));
						// Test sinus graph
						//graph[i][graphCounter] = cos(graphCounter * 2 * PI / NUM_VALUES_FFT * 4);
					}
					graphCounter++;
				}
				else
				{
					// For Emotions Classification
					for (int j = 0; j < NUM_VALUES_FFT; j++) {
						graph[0][j] = (graph[0][j] + graph[1][j]) / 2.0f; // Node 1 = Fpz = avg(fp1, fp2)
						graph[2][j] -= graph[3][j]; // Node 3 = F3 - F4 Signal
					}

					graphCounter = 0;
					/*
					Step 2: Fourier Transformation -------------------------------------------------------------------------------------
					*/
					//Iterate through all 16 nodes and transform each graph (value history)
					for (int i = 0; i < 16; i++)
					{
						// Fill input array and transform graph
						for (int j = 0; j < NUM_VALUES_FFT; j++)
						{
							real_in[j] = graph[i][j];
						}

						kiss_fftr(real_cfg, real_in, real_out);

						// Sqrt over complex numbers of FFT output. We'll analyze only the range of 0 - 60 Hz
						for (int j = 0; j < 60; j++)
						{
							transformedArray[i][j] = sqrt(SQ(real_out[j].r) + SQ(real_out[j].i));
							if (!((0.00001 < transformedArray[i][j]) && (transformedArray[i][j] < 99999999)))
							{
								transformedArray[i][j] = 0.0;
							}
						}
					}

					//Iterate through all 16 nodes and transform each graph (value history)
					for (int i = 0; i < 16; i++) {
						/*
						Step 3: Filter highest amplitude of alpha, beta and gamma waves
						Alpha:  8-13Hz
						Beta:  13-30Hz
						Gamma:   >30Hz
						*/
						alphaMax[i] = 0.0;
						betaMax[i] = 0.0;
						gammaMax[i] = 0.0;
						for (int j = 8; j < 14; j++)
						{
							if (alphaMax[i] < transformedArray[i][j])
								alphaMax[i] = transformedArray[i][j];
						}
						for (int j = 13; j < 30; j++)
						{
							if (betaMax[i] < transformedArray[i][j])
								betaMax[i] = transformedArray[i][j];
						}
						for (int j = 30; j < 60; j++)
						{
							if (gammaMax[i] < transformedArray[i][j])
								gammaMax[i] = transformedArray[i][j];
						}
					}
					/*
					Step 4: JSON -------------------------------------------------------------------------------------------------------
					Save measured data to JSON file
					*/
					rapidjson::Value value;
					rapidjson::Document document;
					document.SetObject();

					rapidjson::Value nodeArray(rapidjson::kArrayType);
					{
						for (int i = 0; i < 16; i++)
						{
							rapidjson::Value valueArray(rapidjson::kArrayType);
							{
								value = alphaMax[i];
								valueArray.PushBack(value, document.GetAllocator());
								value = betaMax[i];
								valueArray.PushBack(value, document.GetAllocator());
								value = gammaMax[i];
								valueArray.PushBack(value, document.GetAllocator());
							}
							nodeArray.PushBack(valueArray, document.GetAllocator());
						}

					}
					document.AddMember("EEG", nodeArray, document.GetAllocator());
					/*
					Stringify JSON
					*/
					rapidjson::StringBuffer buffer;
					rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
					document.Accept(writer);
					const char *valuesJson = buffer.GetString();
					//cout << valuesJson;
					//cout << "\n";

					// write to log file
					logfile << valuesJson;
					logfile << "\n";

					/*
					Step 5: Socket -----------------------------------------------------------------------------------------------------
					Send data as JSON through socket
					*/
					zmq::message_t message(strlen(valuesJson));
					std::memcpy(message.data(), valuesJson, strlen(valuesJson));
					sender.send(message);

					Sleep(250);
				}
			}
		}
	}

	free(transformedArray);

	return finishDataRetrieval();
}

BOOL finishDataRetrieval()
{
	// 5. Close device 
	l_pDevice->Stop();
	l_pDevice->Close();

	if (logfile.is_open())
	{
		logfile.close();
	}
	printf("saveDataToArray exited!");

	return TRUE;
}