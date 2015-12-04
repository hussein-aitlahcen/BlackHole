// BlackHole.RootkitInstaller.cpp : définit le point d'entrée pour l'application console.
//

#include "stdafx.h"
#include <atlbase.h>

using namespace std;


/*
Get a handle to the SCM database and then register the service
*/
SC_HANDLE installdriver(LPCTSTR drivername, LPCTSTR binarypath)
{
	SC_HANDLE scmdbhandle = NULL;
	SC_HANDLE svchandle = NULL;

	//open a handle to the SCM
	scmdbhandle = OpenSCManager
		(
			NULL,	//MAchine name, NULL = local machine
			NULL,	//database name, NULL = SERVICES_ACTIVE_DATABASE
			SC_MANAGER_ALL_ACCESS	//desired access
			);

	//Check if were able to open a handle to the SCM
	if (scmdbhandle == NULL)
	{
		cout << "installdriver, could not open handle to SCM manager" << endl;
		return NULL;
	}

	//create a service which the SCM will run
	svchandle = CreateService
		(
			scmdbhandle,	//handle to the SC manager
			drivername,		//service name
			drivername,		//display name
			SERVICE_ALL_ACCESS,	//desired access
			SERVICE_KERNEL_DRIVER,	//service type
			SERVICE_DEMAND_START,	//start type, service will start when we call start service
			SERVICE_ERROR_NORMAL,	//error control
			binarypath,		//binary path name
			NULL,		//load order group
			NULL,		//tag id
			NULL,		//Dependancies
			NULL,		//service start name (account name)
			NULL		//password for the account
			);

	//check if we were able to create a new service
	if (svchandle == NULL)
	{
		//check if the service already exists
		if (GetLastError() == ERROR_SERVICE_EXISTS)
		{
			cout << "Error service exists" << endl;
			svchandle = OpenService(scmdbhandle, drivername, SERVICE_ALL_ACCESS);

			//check if we were able to open the service
			if (svchandle == NULL)
			{
				cout << "Error, could not open handle to the driver" << endl;
				//close the handle to the service
				CloseServiceHandle(scmdbhandle);
				return NULL;
			}
			//close the handle to the service
			CloseServiceHandle(scmdbhandle);
			return NULL;
		}

		cout << "Error, could not open handle to the driver, and the service doesn't exist" << endl;
		//close the handle to the service
		CloseServiceHandle(scmdbhandle);
		return NULL;
	}

	cout << "Success, the service was created" << endl;
	//close the handle to the service
	CloseServiceHandle(scmdbhandle);
	return svchandle;
}

/*
This function will start running the
newly created service we just made
*/
bool loaddriver(SC_HANDLE svchandle)
{
	//load the driver into the kernel, check if we were able
	if (StartService(svchandle, 0, NULL) == 0)
	{
		//check if the service is already running
		if (GetLastError() == ERROR_SERVICE_ALREADY_RUNNING)
		{
			cout << "Error, the driver is already running" << endl;
			return true;
		}

		//the driver couldn't be loaded into the kernel
		else
		{
			cout << "Error, the driver could not be loaded into the kernel" << endl;
			return false;
		}
	}

	cout << "driver successfully loaded" << endl;
	return true;
}

/*
This function will stop the driver from running
*/
bool stopdriver(SC_HANDLE svchandle)
{
	SERVICE_STATUS status;

	//call the function to stop the driver from running
	if (ControlService(svchandle, SERVICE_CONTROL_STOP, &status) == 0)
	{
		cout << "stop driver, failed to unload driver" << endl;
		return false;
	}

	cout << "stop driver, driver successfully unloaded" << endl;
	return true;
}


/*
This function will remove the driver from memory
*/
bool deletedriver(SC_HANDLE svchandle)
{
	//call the function to remove the driver from memory
	if (DeleteService(svchandle) == 0)
	{
		cout << "delete driver, failed to delete driver" << endl;
		return false;
	}

	cout << "delete driver, driver successfully deleted" << endl;
	return true;
}


int main(int argc, char* argv[])
{
	//check to make sure we have to proper number of arguments
	if (argc < 3 || argc > 3)
	{
		cout << "error, incorrect # of arguments" << endl;
		return -1;
	}
	
	LPCTSTR path = argv[1];	//save the contents of the RK directory
	LPCTSTR driver_name = argv[2];
	
	SC_HANDLE svchandle = NULL;	//Handle to a service 
	int choice = 0;	//will hold the choice of the user

	while (1)
	{
		//print out the choices for the user to complete
		cout << "What would you like to do" << endl;
		cout << "1 -- install driver." << endl;	//choice 1 install
		cout << "2 -- run driver." << endl;	//choice 2 run
		cout << "3 -- stop driver." << endl;	//choice 3 stop
		cout << "4 -- kill driver." << endl;	//choice 4 delete
		cout << "5 -- quit the program." << endl;	//choice 5 quit


		cin >> choice;	//input the choice
		cout << endl;


		//case statement of the choice made by the user
		switch (choice)
		{
		case 1:
		{
			cout << "Installing the driver " << driver_name << "." << endl;
			svchandle = installdriver(driver_name, path);
		}
		case 2:
		{
			cout << "Running the driver " << driver_name << "." << endl;
			if (loaddriver(svchandle) == false)
			{
				cout << "Load Error, deleating driver....please reinstall driver and try again." << endl;
				if (deletedriver(svchandle) == false)
				{
					cout << "Could not delete driver, please try again." << endl;
				}
			}
		}
		case 3:
		{
			cout << "Stopping the driver " << driver_name << "." << endl;
			if (stopdriver(svchandle) == false)
			{
				cout << "Could not stop the driver, please try again." << endl;
			}
		}
		case 4:
		{
			cout << "Deleting the driver " << driver_name << "." << endl;
			if (deletedriver(svchandle) == false)
			{
				cout << "Could not delete driver, please try again." << endl;
			}
		}
		case 5:
		{
			cout << "Quiting the program....Goodbye." << endl;
			break;	//if we want to quit the program then break out of the infinite loop
		}
		//An attempt to prevent the user from breaking the program
		default:
		{
			cout << "Error, Invalid choice." << endl;
		}
		}
	}
	return 0;
}

