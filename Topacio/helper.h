// helper.h
#pragma once
#ifndef _HELPER_h
#define _HELPER_h
#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif
#include <Ethernet.h>
#define ETHERNET_BUFFER_LENGTH 255
#define COM_PORT_FOR_DEBUG Serial
#define NULL_CHANNEL 0xff
namespace helper
{
	//Tipos enumerados para los circuitos de vías y los aspectos de las señales.
	enum statusType {
		error, //Fallo del enclavamiento
		disabled, //Circuito no activo
		iddle, //Sin ocupar
		locked, //Enclavado
		occupied, //Ocupado
		shunt //Maniobras
	};
	enum orderType
	{
		toViaLibre = 0,
		toParada = 1,
		toAvisoDeParada = 2,
		toPrecaucion = 3,
		toRebaseAutorizado = 4
	};
	enum deviceType
	{
		//Tipos de dispositivos para manipulación en modo local
		dtNone = 0,		//Ningún tipo seleccionado (Modo de abandono)
		dtSignal = 1,	//Señales
		dtLayout = 2,	//Secciones de enclavamiento
		dtDetector = 3,	//Circuitos detectores 
		dtSetup = 4		//Modo de ajustes para formatear.
	};

	const char* const auxOrders[] = { "Free","Stop","To stop","Warning","Shunt" };
	inline const char* orderString(orderType rhs) { return auxOrders[rhs]; }

	//Manejo de bits
	inline uint8_t bit_set(uint8_t number, uint8_t n, bool x) { return (number & ~((uint8_t)1 << n)) | ((uint8_t)x << n); }
	inline uint8_t bit_check(uint8_t number, uint8_t n) { return (number >> n) & (uint8_t)1; }

	//Rutinas de impresión
#ifdef DEBUG_SERIAL
	static bool verbose = true;
#else
	static bool verbose = false;
#endif // DEBUG_SERIAL

	inline void pp(const __FlashStringHelper* rhs) { COM_PORT_FOR_DEBUG.print(rhs); } //Pinta cadena F()
	inline void pp(uint8_t rhs) { COM_PORT_FOR_DEBUG.print(rhs); } //Pinta un valor numérico sencillo
	inline void pp(const char* rhs) { COM_PORT_FOR_DEBUG.print(rhs); } //Pinta una cadena normal
	void pp(IPAddress rhs);	
	void pp(uint8_t* mac);

	inline void pl() { COM_PORT_FOR_DEBUG.println(); } //Retorno de carro.
	void pl(uint8_t num);		   //Retornos de carro.	
	void ps(uint8_t size); //Pinta una línea horizontal de separación

	inline void ps() { ps(48); } //Pinta una línea horizontal de separación
	inline void pt() { COM_PORT_FOR_DEBUG.print(F("\t")); } //Tabulador.
	void pt(uint8_t num);  //Tabuladores.
	void timeStamp(); 

	//Esto es una clase que implementa la respuesta a eventos desde una interface.
	class EventObject
	{
	public:
		virtual void HandleDeviceTypeSelection() = 0;
		virtual void HandleButtonPressed(uint8_t buttonId) = 0;
		virtual void HandleAdvancedAction(uint8_t actionId) = 0; //Fuerza al dispositivo a ejecutar alguna acción
	};
};
#endif