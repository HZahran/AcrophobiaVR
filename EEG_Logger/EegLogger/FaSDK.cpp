/*----------------------------------------------------------------------------*/
//  Project:	FaSDK
/*----------------------------------------------------------------------------*/
//  Name:		FaSDK.cpp
//  Purpose:	Demo of FirstAmp SDK
//  Author:		Phuong Nguyen
//  Date:		10-Jul-2008
//  Version:	1.00
//  Revision:	$LastChangedRevision: $
/*----------------------------------------------------------------------------*/
#include "stdafx.h"
#include ".\FirstAmp.h"
#include ".\Device.h"

// PN
#include ".\GenericData.h"
CGenericData l_GenericData;
CGenericData::DeviceInfo l_DeviceInfo;
char l_szRawDataFile[256] = "C:\\Tmp\\VAmp\\Demo";

static CDevice *l_pDevice = NULL;		// Instance of device.

BOOL ServeImpedance()
//; Data serving process for serving impedance measurement
{
	BOOL bRun = TRUE;
	
	int nMaxChannels = 0, nAnalogChannels = 0, nRefChanIdx = 0;
	int tblChannelsPos[4], tblChannelsNeg[4];
	double dSamplingRate = 500; // 500 Hz default.
	LPSAFEARRAY pData;
	CComSafeArray<double, VT_R8> pResolutions;	// table of channel resolution.
	CComSafeArray<char, VT_I1> pChTypes;		// table of channel type.

	// (1) Open device
	if (S_OK != l_pDevice->Open()) goto Finit;
	// (2) Setup configuration
	nMaxChannels = l_pDevice->GetChannels();
	nAnalogChannels = l_pDevice->GetAnalogChannels();
	l_pDevice->SetDataMode(dmNormal);
	l_pDevice->SetSamplingRate(dSamplingRate);
	l_pDevice->GetChannelTypes(&pData);  
	pChTypes.Attach(pData);
	l_pDevice->GetChannelResolutions(&pData); 
	pResolutions.Attach(pData);
	l_pDevice->GetRefChannel(&nRefChanIdx);
	
	// Prepare for raw data file
	l_DeviceInfo.vChannelNames.clear();
	for (int i = 0 ; i < nMaxChannels - 1; i++ ) // without trigger
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

	// in case of highspeed mode (> 2 kHz)
	for (UINT n = 0; n < 4; n++)
	{
		// differential pair 2 (channel n - channel REF)
		tblChannelsPos[n] = n;
		tblChannelsNeg[n] = nRefChanIdx; // channel REF
	}
	// (3) start data acquisition
	if (S_OK != l_pDevice->Start(IAC_AT_IMPEDANCE, dmNormal, tblChannelsPos, tblChannelsNeg))
	{
		goto Finit;
	}
	printf("> Start successful\n");
	printf("\t- Channels: %d\n", nMaxChannels);
	printf("\t- Sampling rate Hz: %g Hz\n", dSamplingRate);

	int nPoints = 0;
	UINT nBlockCnt = 0;
	DWORD dwStartTime = timeGetTime();
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
			CComSafeArray<float, VT_R4> saImpedance;		// SafeArray for Monitoring, TestSignal.
			DATE date = 0;
			LPSAFEARRAY sa;
			int nErrorSeverity = 0;
			HRESULT hResult = S_FALSE;
			int nEegMax = 0;
			hResult = l_pDevice->GetImpedance(&sa, &nMaxChannels, &nEegMax, &date, &nErrorSeverity);
			if (hResult == S_OK) 
			{
				saImpedance.Attach(sa);
				UINT *pImpedance = (UINT *)saImpedance.m_psa->pvData;
				int i = 0;
				// nMaxChannels: EEG channels + 1 Ref, 1 Gnd.
				printf("\r %10g s - %d: c1 = %5g kOhm \t c2 = %5g kOhm \t c3 = %5g kOhm ", 
					float(((float)timeGetTime() - (float)dwStartTime) / 1000.f),
					nBlockCnt++,
					float(*(pImpedance + i)) / 1000.f, 
					float(*(pImpedance + i + 1)) / 1000.f,
					float(*(pImpedance + i + 2)) / 1000.f);
			}
			// *(pData++) = *(pImpedance + nEegMax) / 1000.f;		// Ref channel. 
			// *(pData++) = *(pImpedance + nEegMax + 1) / 1000.f;  // Gnd channel.  
		}
	}
Finit:
	// (5) Close device 
	l_pDevice->Stop();
	l_pDevice->Close();
	printf("> Impedance measurement exited.\n");
	return TRUE;
}

BOOL ServeDataAcquisition(int nAcquisitionType)
//; Data serving process for mode monitoring or calibration
{
	BOOL bRun = TRUE;
	
	int nMaxChannels = 0, nAnalogChannels = 0, nRefChanIdx = 0;
	int nDataMode = dmNormal, tblChannelsPos[4], tblChannelsNeg[4];
	double dSamplingRate = 2000; // 2 kHz
	LPSAFEARRAY pData;
	CComSafeArray<double, VT_R8> pResolutions;	// table of channel resolution.
	CComSafeArray<char, VT_I1> pChTypes;		// table of channel type.

	if (nAcquisitionType == IAC_AT_CALIBRATION) 
	{
		nDataMode = dmNormal;
	}
	// (1) Open device
	if (S_OK != l_pDevice->Open()) goto Finit;
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
	for (int i = 0 ; i < nMaxChannels - 1; i++ ) // without trigger
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
		goto Finit;
	}
	printf("> Start successful\n");
	printf("\t- Channels: %d\n", nMaxChannels);
	printf("\t- Data mode: %s\n", nDataMode == dmNormal ? 
		"Normal mode - all channels at 2 kHz" : "Fast mode - 20 kHz, 4 Channels, no auxiliary");
	printf("\t- Sampling rate Hz: %g Hz\n", dSamplingRate);

	int nPoints = 0;
	UINT nBlockCnt = 0;
	DWORD dwStartTime = timeGetTime();
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
			if (hResult == S_OK)
			{
				saData.Attach(sa);
				float *pBuffer = (float *)saData.m_psa->pvData;
				l_GenericData.WriteData(pBuffer, nPoints, nMaxChannels - 1, 1); // ignore trigger channel
				int i = 0;
				// order of channel : 
				// VAmp 16: EEG: 0..15; AUX: 16, 17; Trigger: 18
				// firstAmp 8: EEG: 0..7; AUX: 8, 9; Trigger: 10
				printf("\r %10g s - %d: c1 = %g uV \t c2 = %g uV \t c3 = %g uV ", 
					float(((float)timeGetTime() - (float)dwStartTime) / 1000.f),
					nBlockCnt++,
					*(pBuffer + i) * (float)pResolutions.GetAt(i), 
					*(pBuffer + i + 1) * (float)pResolutions.GetAt(i + 1),
					*(pBuffer + i + 2) * (float)pResolutions.GetAt(i + 2));
			}
		}
	}
Finit:
	// (5) Close device 
	l_pDevice->Stop();
	l_pDevice->Close();
	if (nAcquisitionType == IAC_AT_DATA)
	{
		printf("> ServeDataAcquisition exited.\n");
	}
	else if (nAcquisitionType == IAC_AT_CALIBRATION)
	{
		printf("> Test Nignal exited.\n");
	}
	return TRUE;
}

void StartDemo()
//; Main process for serving impedance data.
{
	l_pDevice = CDevice::GetInstance();
	if (l_pDevice == NULL) 
	{
		printf("Cannot find device.\n");
		return;
	}
	printf("1 - Start Monitoring\n");
	printf("2 - Start Impedance\n");
	printf("3 - Start Test Signal\n");
	printf("Esc - Cancel data acquisition\n");
	char c = char(_getch());
	switch (c)
	{
	//*** Start monitoring ************************
	case '1': 
		printf("\n S T A R T  M O N I T O R I N G\n");
		ServeDataAcquisition(IAC_AT_DATA);
		break;
	//*** Stop impedance ************************
	case '2':
		printf("\n S T A R T  I M P E D A N C E\n");
		ServeImpedance();
		break;
	//*** Testsignal ************************
	case '3':
		printf("\n S T A R T  T E S T  S I G N A L\n");
		ServeDataAcquisition(IAC_AT_CALIBRATION);
		break;
	case 27: // ESC
		printf("> Exit.\n");
		break;
	}
}

int _tmain(int argc, _TCHAR* argv[])
//; Main entry routine.
{
	StartDemo();
	return 0;
}

