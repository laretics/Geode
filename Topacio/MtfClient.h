//Cliente de la se�alizaci�n.
//Un cliente contiene se�ales, aparatos de v�a, detectores de v�a o detectores de aparatos de v�a.
//El cliente puede contener varios m�dulos de cada tipo.
//Est� pensado para integrarse con la clase mtfBios, de forma que la carga de las prestaciones se hace
//en la inicializaci�n del m�dulo.
//Dos clientes distintos s�lo se diferencian en el contenido de la EEPROM de cada uno de ellos.
#pragma once
#ifndef _CLIENT_h
#define _CLIENT_h
#include "MtfClientDevices.h"
#include "MtfEthernet.h"
#include "MtfInterface.h"

#define FIRMWARE_BOOT_FLAG 69
//Aqu� est� la versi�n que se muestra en el display (la que hay que cambiar cada poco)
#define FIRMWARE_VERSION 15
//===================================================================================
#define FACTORY_ARRAY_LENGTH 18

#define CLIENT_VERSION_ADDRESS 1
#define INTERFACE_TYPE_ADDRESS 2
#define CLIENT_ADDRESS_START 3

namespace helper
{
	class MtfClient :EventObject
	{
	public:
		//Estructura de datos del cliente

		//Cuando un cliente tiene este ID no puede enviar eventos.
#define PASSIVE_CLIENT 0xff
		MtfClient();

	protected:
		MtfClientDevices mcolDevices; //Colecci�n de dispositivos bajo control en este cliente.
		MtfEthernet mvarEthernet; //M�dulo de comunicaciones Ethernet
		MtfInterface mvarInterface; //M�dulo de gesti�n del MultifunctionShield
		bool deserializeV1(); //Deserializa la estructura asumiendo que lo que hay en la Bios es una versi�n 1.

	public:
		void init();
		void loop(); //Refresca las se�ales.		

		void showConfig(); //Saca por el puerto serie toda la informaci�n de este cliente.
		void HandleDeviceTypeSelection() override;
		void HandleButtonPressed(uint8_t buttonId) override;
		void HandleAdvancedAction(uint8_t actionId) override;

	private:
		void resetFirmwareV1(bool mask16);
		bool manageSerial(); //Lee del puerto serie para ejecutar comandos directos
		void memoryDump(uint8_t length); //Vuelca en el puerto serie todo el contenido de la EEPROM

		bool processTCPRequest(uint8_t* buffer, uint16_t length); //Procesa el comando que ha llegado por Ethernet
		bool processUDPRequest(uint8_t* buffer, uint16_t length); //Procesa la informaci�n que acaba de llegar por UDP a mcolBuffer.
		bool updateFirmwareFromBuffer(uint8_t* buffer, uint16_t length); //Intenta ejecutar la actualizaci�n desde el buffer ya descargado.

		static const uint8_t initialFirmwareV1[]; //Array con el firmware por defecto de la versi�n 1.
		//Comunicaciones:

		//uint8_t mvarLastButtons;
		bool mvarEnabled;

		uint8_t mvarCurrentLocalSignalId; //Id de la se�al que tengo seleccionada ahora en modo local.
		uint8_t mvarCurrentLocalLayoutId; //Id del circuito de v�a de agujas que tengo seleccionado ahora en modo local.
		uint8_t mvarCurrentLocalDetectorId; //Id del detector de ocupaci�n de v�a que tengo ahora mismo localizado.
		void refreshLocal(); //Actualiza la representaci�n en el display en modo local.
		void refreshCurrentSignalToInterface(); //Actualiza la informaci�n de la se�al actual en la interface
		void refreshCurrentLayoutToInterface(); //Actualiza la informaci�n del circuito de agujas actual en la interface
		void refreshCurrentDetectorToInterface(); //Actualiza la informaci�n del detector actual en la interface
		void refreshSetupModeToInterface(); //Actualiza la informaci�n con el men� de configuraci�n.

	};
}

#endif
