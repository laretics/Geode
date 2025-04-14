// circuitLayoutClient.h
#pragma once
#ifndef _CIRCUITLAYOUTCLIENT_h
#define _CIRCUITLAYOUTCLIENT_h
#include "childClient.h"
#include "frog.h"
#define MAX_FROGS_PER_LAYOUT 4
#define MAX_LAYOUTS 4
/// <summary>
/// Controlador de aparatos de vía (con o sin comprobación) para dotar un enclavamiento
/// </summary>
namespace helper
{
	class circuitLayoutClient :public childClient
	{
	protected:
		FrogClass* mcolFrog[MAX_FROGS_PER_LAYOUT]; //Motores de aguja.
		bool mcolLayouts[MAX_LAYOUTS][MAX_FROGS_PER_LAYOUT]; //Configuraciones de agujas según los posibles estados de este circuito.
		uint8_t mvarLayoutsCount; //Número de configuraciones de agujas que tiene este circuito.
		uint8_t mvarFrogsCount; //Número de agujas que tiene este circuito.
		uint16_t mvarFrogMovingDelay; //Valor en milisegundos para que el circuito de vía se disponga en el peor de los casos (para detectar error de operación).
		bool mvarSimultaneous; //Flag para establecer la forma en que se disponen las agujas...
		//Si es true, todas las agujas se dispondrán al mismo tiempo. Será más rápido, pero los motores consumirán mucha energía al mismo tiempo.
		//Si es false, cada aguja se irá moviendo cuando la anterior termine su turno. Será más lento, pero cada motor irá en un turno.
	public:
		circuitLayoutClient();
		void init(uint8_t id) override;
		inline void setSequential(bool rhs) { mvarSimultaneous = !rhs; }
		inline void setFrogMovingDelay(uint16_t rhs) { mvarFrogMovingDelay = rhs; } //Tiempo máximo de retardo en mover los aparatos de vía		
		inline uint16_t getFrogMovingDelay() { return mvarFrogMovingDelay; }
		inline uint8_t getLayoutCount() { return mvarLayoutsCount; } //Número de configuraciones que tiene este circuito.
		inline uint8_t getFrogCount() { return mvarFrogsCount; } //Número de aparatos de vía gestionados que tiene este circuito.
		void addFrog(FrogClass* rhs); //Añade un nuevo dispositivo de control de agujas al circuito de vía.
		FrogClass* getFrog(uint8_t index) { return mcolFrog[index]; } //Obtiene una referencia a una aguja concreta.
		void setLayoutFrog(uint8_t layoutIndex, uint8_t frogId, bool straight); //Asigna un estado de aguja según la configuración.
		void setLayout(uint8_t layoutIndex); //Actúa sobre los aparatos de vía.	
		uint8_t getLayout(); //Devuelve el itinerario seleccionado.
		bool layoutSetCompleted(); //Indica si han terminado de colocarse los aparatos de vía.
		void loop() override;
		bool hasError(); //Enclavamiento averiado por fallo en uno de sus componentes.
		void showConfig() override; //Muestra la configuración de un cliente concreto
		void showStatus() override; //Muestra el estado actual de este cliente
	private:
		uint8_t mvarFrogMovingIndex; //Sólo tiene sentido si mvarSimultaneous es false... establece cuál es la aguja que se va a mover ahora.
		uint8_t mvarTargetLayout; //Objetivo del movimiento de este circuito de vía.
		FrogClass::frogPosition getCurrentFrogTarget(uint8_t frogId);

	};
}
#endif