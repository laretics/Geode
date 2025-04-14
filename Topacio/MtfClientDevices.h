#pragma once
#ifndef _CLIENT_DEVICES
#define _CLIENT_DEVICES
#include "circuitDetectorClient.h"
#include "circuitLayoutClient.h"
#include "helper.h"
#include "signalClient.h"
#include <EEPROM.h>
#define MAX_OF_SIGNALS 8
#define MAX_OF_CIRCUITS 8
#define MAX_OF_SERVOS 8
#define MAX_OF_FROGS 8
//Tenemos un array de señales y de circuitos predefinido.

namespace helper
{
	class MtfClientDevices
	{
	public:
		//Número máximo de circuitos o señales direccionados en cada uno de los buffers
#define MAX_ELEMENTS 64
//Número de uint8_ts de los contadores de circuitos (Uint16) y señales (Uint16)-> 2+2
#define CIRCUIT_AND_SIGNALS 4
//Cuando una señal tiene esta dirección no se puede usar.
#define NULL_CHANNEL 0xff		

		MtfClientDevices();

	protected:
		//Estructura de datos con los clientes.
		signalClient mcolSignalClient[MAX_OF_SIGNALS]; //Colección de señales luminosas y semáforos
		circuitDetectorClient mcolDetectors[MAX_OF_CIRCUITS]; //Colección de detectores de ocupación de vía
		circuitLayoutClient mcolLayouts[MAX_OF_CIRCUITS]; //Colección de enclavamientos de circuitos
		Servo mcolServo[MAX_OF_SERVOS]; //Colección de servos
		FrogClass mcolFrogs[MAX_OF_FROGS]; //Colección de mecanismos de control de agujas.
		uint16_t deserializeSignal(uint16_t pointer); //Carga una señal o semáforo desde la EEPROM. Devuelve el nuevo puntero.
		uint16_t deserializeLayoutUnit(uint16_t pointer, uint8_t motorPointer); //Carga una sección de enclavamiento (circuito de vía con agujas).
		uint16_t deserializeDetector(uint16_t pointer); //Carga un detector de presencia de tren desde la EEPROM.

		uint8_t mvarSignalClientCount = 0; //Número de señales que se han creado
		uint8_t mvarCircuitDetectorCount = 0; //Número de detectores de ocupación de vía
		uint8_t mvarCircuitLayoutCount = 0; //Número de enclavamientos de circuitos
		uint8_t mvarServoCount = 0; //Número de servos en uso
		uint8_t mvarFrogCount = 0; //Número de mecanismos de control de agujas en uso

	public:
		bool deserializeV1(uint16_t pointer); //Carga la estructura desde la EEPROM.
		FrogClass* addFrog(); //Obtiene la referencia a un nuevo mecanismo de control de agujas.
		circuitLayoutClient* addLayout(uint8_t circuitId); //Carga un nuevo gestor de enclavamiento.
		circuitDetectorClient* addDetector(uint8_t circuitId, uint8_t port, uint8_t occupancyDelay = 0); //Carga un nuevo detector de circuitos.
		void showConfig(); //Saca por el puerto serie toda la información de este cliente.
		void setSignalOrderByAddress(uint8_t id, helper::orderType value); //Asigna la indicación de una señal pasándole su identificador
		void setSignalOrderByIndex(uint8_t id, helper::orderType value); //Asigna la indicación de una señal pasándole el índice interno en el array
		void setLayoutByAddress(uint8_t id, uint8_t value); //Asigna un itinerario
		void setLayoutByIndex(uint8_t index, uint8_t value);
		bool processBuffer(byte* rhs);//Ajusta circuitos y señales con la información que lee del buffer.
		void loop(); //Realiza el ciclo
		inline uint8_t SignalCount() { return mvarSignalClientCount; }
		inline helper::orderType getSignalOrderByIndex(uint8_t index);
		inline helper::orderType getSignalOrderByAddress(uint8_t address) { return mcolSignalClient[address].getOrder(); }
		inline uint8_t LayoutCount() { return mvarCircuitLayoutCount; }
		inline uint8_t getLayout(uint8_t id) { return mcolLayouts[id].getLayout(); }
		inline uint8_t getLayoutMax(uint8_t id) { return mcolLayouts[id].getLayoutCount(); }
		inline uint8_t DetectorCount() { return mvarCircuitDetectorCount; }
		inline uint8_t getDetector(uint8_t id) { return mcolDetectors[id].getOccupancy(); }
	};
}
#endif // !_CLIENT_DEVICES
