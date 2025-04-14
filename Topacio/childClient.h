#pragma once
#ifndef _CHILD_CLIENT_H
#define _CHILD_CLIENT_H
#define INVALID_PORT 0xff
#include "helper.h"

namespace helper
{
	class childClient
	{
	public:
		uint8_t channelId; //Canal que tiene este cliente en su respectivo medio de almacenamiento.

		inline childClient() { mvarTimeStamp = millis(); }
		virtual void init(uint8_t id);
		virtual void loop();
		inline const uint8_t id() { return channelId; }
		virtual void showConfig() { pp(channelId); } //Muestra la configuración de un cliente concreto
		virtual void showStatus() { pp(channelId); } //Muestra el estado actual de este cliente
	protected:
		uint64_t mvarTimeStamp; //Variable interna para las temporizaciones.
	};
}
#endif