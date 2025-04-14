//Circuito de v�a con detector de ocupaci�n y agujas telemandadas.
#pragma once
#ifndef _CIRCUITDETECTOR_h
#define _CIRCUITDETECTOR_h
#include "childClient.h"
#define MAX_DETECTORS 16
#define INVALID_PORT 0xff
namespace helper
{
	class circuitDetectorClient :childClient
	{
	protected:
		uint8_t mvarOccupancyDetectorPort; //Puerto de la entrada digital del sensor de ocupaci�n (circuito de v�a)
		uint8_t mvarFreeOccupancyDelay; //Valor en segundos para liberar el circuito autom�ticamente tras detectar la ocupaci�n (s�lo para sensores de efecto hall)		
		bool mvarOccupied; //Indica si hay un tren o no en este circuito.		
	public:
		circuitDetectorClient();
		void init(uint8_t id) override; //Asigna los recursos de hardware a los controladores e inicia la estructura.		
		void init(uint8_t id, uint8_t port);
		void setOccupancy(bool occupancy); //Ocupa o libera el circuito
		inline void setOccupancyDelay(uint8_t rhs) { mvarFreeOccupancyDelay = rhs; } //Asigna el tiempo de retraso (en segundos) de la liberaci�n autom�tica del circuito
		inline bool getOccupancy() { return mvarOccupied; } //Devuelve la ocupaci�n actual de este circuito.
		inline uint8_t getOccupancyDelay() { return mvarFreeOccupancyDelay; }
		uint64_t getOccupancyLapse(); //Tiempo que lleva ocupado este circuito (suponiendo que lo est�).
		void loop() override; //Actualiza el estado de los elementos del circuito que pudieran estar movi�ndose.
		void showConfig() override; //Muestra la configuraci�n de un cliente concreto
		void showStatus() override; //Muestra el estado actual de este cliente
	};
}

#endif