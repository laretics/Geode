#pragma once
#ifndef _MTF_ETHERNET
#define _MTF_ETHERNET
#include <SPI.h>
#include <Ethernet.h>
#include <EthernetUdp.h>
#include <EEPROM.h>
#include "helper.h"

#define ETHERNET_MAIN_PORT 1100
//Puerto de recepci�n de �rdenes desde el servidor TCP/IP
#define ETHERNET_UDP_PORT 1101
//Puerto de recepci�n de datos de enclavamiento desde UDP
#define ETHERNET_ERROR 0xffff
//Error de ethernet

namespace helper
{
	class MtfEthernet
	{
	public:
		//Tama�o del buffer que contiene el m�ximo de circuitos o se�ales
#define BUFFER_SIZE HEADER_COUNT+1+1+MAX_ELEMENTS+1
	// Tama�o de la cabecera (HEADER_COUNT) + 
	// Tama�o del comando (1) +
	// Tama�o del n�mero de componentes (1) +
	// M�ximo n�mero de elementos (MAX_ELEMENTS) +
	// CRC (1)MtfEthernet();

		bool init();
		uint16_t deserializeV1(uint16_t pointer);
		void showParams();
		bool clientAvailable();
		uint16_t loopTCP();
		uint16_t loopUDP();
		void writeResponse(uint8_t responseCode); //Escribe en el buffer una respuesta hacia Ethernet.		
		uint8_t* getBuffer() { return mcolBuffer; }
		void flush(); //Vac�a las comunicaciones pendientes de recibir.
	private:
		EthernetServer mvarEthernetMainServer = EthernetServer(ETHERNET_MAIN_PORT);
		EthernetUDP mvarEthernetUDPClient = EthernetUDP();
		IPAddress LocalAddress;
		IPAddress LocalMask;
		uint8_t mcolMAC[6];
		uint8_t mcolBuffer[ETHERNET_BUFFER_LENGTH];
		bool checkEthernetHeader(); //Comprueba si el buffer contiene la cabecera correcta de Ethernet.
		bool checkUDPHeader(); //Comprueba si el buffer contiene la cabecera correcta de un paquete UDP.
		EthernetClient mvarTcpClient; //Cliente TCP que ha logrado establecer conexi�n.
	};
}

#endif // !_MTF_ETHERNET