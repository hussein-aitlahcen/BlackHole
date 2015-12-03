// BlackHole.Stub.cpp : définit le point d'entrée pour l'application.
//

#include "stdafx.h"
#include "BlackHole.Stub.h"
#include "ntddk.h"

VOID CleanUp(IN PDRIVER_OBJECT pDriverObject);
NTSTATUS DriverEntry(IN PDRIVER_OBJECT TheDriverObject, IN PUNICODE_STRING TheRegistryPath)
{
	DbgPrint("This is my first driver baby!!");
	TheDriverObject - &gt; DriverUnload = CleanUp;

	return STATUS_SUCCESS;
}

// This is the UnLoad Routine
VOID CleanUp(IN PDRIVER_OBJECT pDriverObject)
{
	DbgPrint("CleanUp routine called");
}