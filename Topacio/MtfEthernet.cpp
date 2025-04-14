#include "MtfEthernet.h"

bool helper::MtfEthernet::init()
{
	Ethernet.begin(mcolMAC, LocalAddress, IPAddress(0, 0, 0, 0), IPAddress(0, 0, 0, 0), LocalMask);
	mvarEthernetMainServer.begin();
	mvarEthernetUDPClient.begin(ETHERNET_UDP_PORT);
	pp(F("Ethernet server started!")); pl();
	return true;
}
uint16_t helper::MtfEthernet::deserializeV1(uint16_t pointer)
{
	for (uint8_t i = 0; i < 6; i++)
	{
		mcolMAC[i] = EEPROM.read(pointer++);
	}
	//Lectura de IP
	for (uint8_t i = 0; i < 4; i++)
	{
		LocalAddress[i] = EEPROM.read(pointer++);
	}
	//Lectura de máscara de red
	uint8_t auxMask = EEPROM.read(pointer++);
	uint32_t auxMascaraCompleta = 0xFFFFFFFF;
	auxMascaraCompleta <<= (32 - auxMask);
	LocalMask = IPAddress(auxMascaraCompleta >> 24,
		(auxMascaraCompleta >> 16) & 0xFF,
		(auxMascaraCompleta >> 8) & 0xFF,
		auxMascaraCompleta & 0xFF);
	return pointer;
}
void helper::MtfEthernet::showParams()
{
	pp(F("MAC Address: "));
	pp(mcolMAC);
	pl();
	pp(F("IP Address: "));
	pp(LocalAddress);
	pl();
	pp(F("Mask: "));
	pp(LocalMask);
	pl();
}
bool helper::MtfEthernet::clientAvailable()
{
	if (mvarTcpClient) return false; //No va a detectar nuevas comunicaciones mientras quede un paquete TCP por procesar.
	EthernetClient auxCliente = mvarEthernetMainServer.available();
	uint16_t auxUdpSize = mvarEthernetUDPClient.parsePacket();
	return (auxCliente || auxUdpSize);
}
uint16_t helper::MtfEthernet::loopTCP()
{
	//Escuchamos los clientes TCP que se conectan.
	mvarTcpClient = mvarEthernetMainServer.available();
	if (mvarTcpClient)
	{
		IPAddress mvarClientIp = mvarTcpClient.remoteIP();
		pp(F("New message from "));	pp(mvarClientIp); pl();
		//Leemos todo el contenido de la petición de este cliente en el buffer de datos
		uint16_t auxIndex = 0;
		bool procesado = false;
		while (mvarTcpClient.connected() && !procesado)
		{
			delay(100);
			while (mvarTcpClient.available())
			{
				mcolBuffer[auxIndex++] = mvarTcpClient.read();
				delay(10);
			}
			if (!checkEthernetHeader())
			{
				pp(F("Error: TCP message header not valid")); pl();
				writeResponse(4); //Error de cabecera inválida
				return ETHERNET_ERROR;
			}
			procesado = auxIndex;
		}
		return auxIndex;
	}
	return 0;
}
uint16_t helper::MtfEthernet::loopUDP()
{
	uint16_t auxUdpSize = mvarEthernetUDPClient.parsePacket();
	if (auxUdpSize > 0)
	{
		mvarEthernetUDPClient.read(mcolBuffer, ETHERNET_BUFFER_LENGTH);
		if (!checkUDPHeader())
		{
			pp(F("Error: UDP message header not valid")); pl();
			return ETHERNET_ERROR;
		}
	}
	return auxUdpSize;
}

/// <summary>
/// Escribe en el buffer una respuesta tipo
/// </summary>
/// <param name="client">Referencia al cliente al que escribimos el mensaje
/// <param name="responseCode"></param>
/// /// <returns>Tamaño del array a enviar</returns>
void helper::MtfEthernet::writeResponse(uint8_t responseCode)
{
	uint16_t auxPointer = 0;
	//Escribe la cabecera
	mcolBuffer[auxPointer++] = 111;
	mcolBuffer[auxPointer++] = 255;
	mcolBuffer[auxPointer++] = 69;
	mcolBuffer[auxPointer++] = responseCode;
	mvarTcpClient.write(mcolBuffer, auxPointer);
	mvarTcpClient.stop();
}
void helper::MtfEthernet::flush()
{
	mvarTcpClient = mvarEthernetMainServer.available();
	if (mvarTcpClient)
	{
		mvarTcpClient.flush();
	}
	uint16_t auxUdpSize = mvarEthernetUDPClient.parsePacket();
	if (auxUdpSize > 0)
	{
		mvarEthernetUDPClient.flush();
	}
}
bool helper::MtfEthernet::checkEthernetHeader()
{
	if (mcolBuffer[0] != 69) { return false; }
	if (mcolBuffer[1] != 107) { return false; }
	if (mcolBuffer[2] != 33) { return false; }
	return true;
}
bool helper::MtfEthernet::checkUDPHeader()
{
	if (mcolBuffer[0] != 11) { return false; }
	if (mcolBuffer[1] != 22) { return false; }
	if (mcolBuffer[2] != 33) { return false; }
	return true;
}
