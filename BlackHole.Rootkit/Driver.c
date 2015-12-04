#include <ntddk.h>

#define DeviceName L"\Device\IRPdev"
#define LnkDeviceName L"\Global??\IRPdev" // <=>  DosDevicesIRPdev


void Print(PCSTR log)
{
	DbgPrint("BlackHole.Rootkit -> %s", log);
}

NTSTATUS DriverUnload(IN PDRIVER_OBJECT DriverObject)
{
	UNICODE_STRING usLnkName;
	RtlInitUnicodeString(&usLnkName, LnkDeviceName);
	IoDeleteSymbolicLink(&usLnkName);

	IoDeleteDevice(DriverObject->DeviceObject);
	Print(__FUNCTION__" unload");
	return STATUS_SUCCESS;
}

NTSTATUS DriverCustomDispatch(IN PDEVICE_OBJECT DeviceObject, IN PIRP Irp)
{
	Irp->IoStatus.Status = STATUS_SUCCESS;
	IoCompleteRequest(Irp, IO_NO_INCREMENT);
	return Irp->IoStatus.Status;
}

NTSTATUS DriverEntry(PDRIVER_OBJECT  pDriverObject, PUNICODE_STRING  pRegistryPath)
{
	/*ULONG i, NtStatus;
	PDEVICE_OBJECT pDeviceObject = NULL;
	UNICODE_STRING usDriverName, usLnkName;

	Print(__FUNCTION__" hello from kernel :))");

	for (int i = 0; i < IRP_MJ_MAXIMUM_FUNCTION; i++)
		pDriverObject->MajorFunction[i] = DriverCustomDispatch;

	RtlInitUnicodeString(&usDriverName, DeviceName);
	RtlInitUnicodeString(&usLnkName, LnkDeviceName);

	NtStatus = IoCreateDevice(pDriverObject,
		0,
		&usDriverName,
		FILE_DEVICE_UNKNOWN,
		FILE_DEVICE_SECURE_OPEN,
		FALSE,
		&pDeviceObject);
	if (NtStatus != STATUS_SUCCESS)
	{
		Print(__FUNCTION__" error with IoCreateDevice : 0x%x", NtStatus);
		return STATUS_UNSUCCESSFUL;
	}

	pDeviceObject->Flags |= DO_DIRECT_IO;

	NtStatus = IoCreateSymbolicLink(&usLnkName, &usDriverName);
	if (NtStatus != STATUS_SUCCESS)
	{
		Print(__FUNCTION__" error with IoCreateSymbolicLink : 0x%x", NtStatus);
		return STATUS_UNSUCCESSFUL;
	}

	pDriverObject->DriverUnload = DriverUnload;
*/
	return STATUS_SUCCESS;
}