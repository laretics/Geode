// circuitLayoutClient.h
#pragma once
#ifndef _CIRCUITLAYOUTCLIENT_h
#define _CIRCUITLAYOUTCLIENT_h
#include "childClient.h"
#include "frog.h"
#define MAX_FROGS_PER_LAYOUT 4
#define MAX_LAYOUTS 4
/// <summary>
/// Controlador de aparatos de v�a (con o sin comprobaci�n) para dotar un enclavamiento
/// </summary>
namespace helper
{
	class circuitLayoutClient :public childClient
	{
	protected:
		FrogClass* mcolFrog[MAX_FROGS_PER_LAYOUT]; //Motores de aguja.
		bool mcolLayouts[MAX_LAYOUTS][MAX_FROGS_PER_LAYOUT]; //Configuraciones de agujas seg�n los posibles estados de este circuito.
		uint8_t mvarLayoutsCount; //N�mero de configuraciones de agujas que tiene este circuito.
		uint8_t mvarFrogsCount; //N�mero de agujas que tiene este circuito.
		uint16_t mvarFrogMovingDelay; //Valor en milisegundos para que el circuito de v�a se disponga en el peor de los casos (para detectar error de operaci�n).
		bool mvarSimultaneous; //Flag para establecer la forma en que se disponen las agujas...
		//Si es true, todas las agujas se dispondr�n al mismo tiempo. Ser� m�s r�pido, pero los motores consumir�n mucha energ�a al mismo tiempo.
		//Si es false, cada aguja se ir� moviendo cuando la anterior termine su turno. Ser� m�s lento, pero cada motor ir� en un turno.
	public:
		circuitLayoutClient();
		void init(uint8_t id) override;
		inline void setSequential(bool rhs) { mvarSimultaneous = !rhs; }
		inline void setFrogMovingDelay(uint16_t rhs) { mvarFrogMovingDelay = rhs; } //Tiempo m�ximo de retardo en mover los aparatos de v�a		
		inline uint16_t getFrogMovingDelay() { return mvarFrogMovingDelay; }
		inline uint8_t getLayoutCount() { return mvarLayoutsCount; } //N�mero de configuraciones que tiene este circuito.
		inline uint8_t getFrogCount() { return mvarFrogsCount; } //N�mero de aparatos de v�a gestionados que tiene este circuito.
		void addFrog(FrogClass* rhs); //A�ade un nuevo dispositivo de control de agujas al circuito de v�a.
		FrogClass* getFrog(uint8_t index) { return mcolFrog[index]; } //Obtiene una referencia a una aguja concreta.
		void setLayoutFrog(uint8_t layoutIndex, uint8_t frogId, bool straight); //Asigna un estado de aguja seg�n la configuraci�n.
		void setLayout(uint8_t layoutIndex); //Act�a sobre los aparatos de v�a.	
		uint8_t getLayout(); //Devuelve el itinerario seleccionado.
		bool layoutSetCompleted(); //Indica si han terminado de colocarse los aparatos de v�a.
		void loop() override;
		bool hasError(); //Enclavamiento averiado por fallo en uno de sus componentes.
		void showConfig() override; //Muestra la configuraci�n de un cliente concreto
		void showStatus() override; //Muestra el estado actual de este cliente
	private:
		uint8_t mvarFrogMovingIndex; //S�lo tiene sentido si mvarSimultaneous es false... establece cu�l es la aguja que se va a mover ahora.
		uint8_t mvarTargetLayout; //Objetivo del movimiento de este circuito de v�a.
		FrogClass::frogPosition getCurrentFrogTarget(uint8_t frogId);

	};
}
#endif