#pragma once
#ifndef _MTF_INTERFACE
#define _MTF_INTERFACE
#include "helper.h"
#include <TimerOne.h>
#include <Wire.h>
#include <MultiFuncShield.h>
#define DISPLAY_MULTIFUNCTION_SHIELD 0
#define DISPLAY_NULL 1
#define DISPLAY_LCD 2

//0: No hay interface.		PRESENTE
//1: Multifunction shield.	PRESENTE
//2: LCD screen.			NO DESARROLLADO
//0xff: Cualquier otro tipo.

namespace helper
{
	class MtfInterface
	{
	public:
#define BEEP_PORT 3
#define ANAL_THRESOLD 512
		void Init(uint8_t deviceType);
		void Led(uint8_t id, bool value);
		void Sound(uint8_t duration);
		void Sound(uint8_t p1, uint8_t p2, uint8_t p3);
		void pp(const char* rhs);
		void loop(); //Retardos varios
		void refreshLocalDisplay(uint8_t deviceId, char* deviceStatus); //Muestra la información del modo local
		void refreshLocalDisplaynum(uint8_t deviceId, uint8_t deviceStatus); //Muestra la información del modo local
		deviceType CurrentDeviceType; //Tipo del dispositivo seleccionado en el modo local
		inline bool ResetFirmwareRequested() { return mvarResetFirmwareRequested; }
		inline bool LocalMode() { return mvarLocalMode; }
		EventObject* ClientObject = NULL; //Elemento que va a recibir los eventos de las pulsaciones.
	private:
		uint16_t mvarBlinking;
		uint8_t mvarInterfaceType; //Tipo de dispositivo para mostrar info.
		bool mvarLocalMode; //Variable que informa si estamos en modo local o remoto.
		bool mvarResetFirmwareRequested; //Indica que se ha solicitado un reset.

		void queryButton(); //Rutina que comprueba los botones.		
	};
}
#endif // !_MTF_INTERFACE