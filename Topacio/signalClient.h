//Objeto se�al luminosa en un enclavamiento
#pragma once
#ifndef _SIGNAL_CLIENT_
#define _SIGNAL_CLIENT_
//Inclu�mos la referencia a la biblioteca de servos por si es un sem�foro, aunque sea una se�al luminosa que no tiene de eso.
#include <Servo.h>
#include "childClient.h"
#define SERVO_INCREMENT 2
#define ANGLE_VIA_LIBRE 90
#define ANGLE_AVISO_DE_PARADA 45
#define ANGLE_PARADA 0
namespace helper
{
    class signalClient : public childClient
    {
    public:
        static const char* const helper::signalClient::auxColores[];
        signalClient();
        inline helper::orderType getOrder() { return mvarOrder; }
        void setInverted(bool green, bool red, bool yellow, bool white); //Ajusta el mapa de inversiones.
        void setLedPort(uint8_t green, uint8_t red, uint8_t yellow, uint8_t white, uint8_t servo); //Ajusta los puertos de las luces.
        inline uint8_t getLedPort(uint8_t color) { return mcolLedPort[color]; }
        void init(uint8_t id); //Ajusta los sentidos de salida y prepara las se�ales para la primera visualizaci�n.
        virtual void setOrder(helper::orderType order); //Asigna la orden.
        void loop() override; //Hay que llamarla peri�dicamente
        void showConfig() override; //Muestra la configuraci�n de un cliente concreto
        void showStatus() override; //Muestra el estado actual de este cliente

        //C�digo para sem�foro
        void setServo(Servo* rhs, uint8_t servoPort, uint16_t stopAngle, uint16_t midAngle, uint16_t allowAngle, bool tomeuMode, bool dynamic, bool speed0, bool speed1);

        inline Servo* getServo() { return mvarServo; }
        inline void setSemaphoreSpeed(uint8_t speed) { mvarSemaphoreIncrement = speed; } //Puede ajustar la velocidad del sem�foro de forma personalizada.

    protected:
        bool mcolInverted[5]; //Colecci�n con las inversiones de los cuatro fuegos de una se�al.
        //Atenci�n: El bool 5 se usa para los sem�foros "tipo Tomeu"
        uint8_t mcolLedPort[5]; //Colecci�n con los cuatro fuegos de una se�al.            
        //Atenci�n: El puerto 5 se usa en los sem�foros para la salida del servo.
        uint16_t mcolSemaphorePosition[3]; //Posici�n en �ngulos del brazo para cada aspecto de la se�al
        helper::orderType mvarOrder; //�ltima orden recibida
        virtual void auxSetOrder(); //Asigna la �ltima orden recibida        
    private:
        //C�digo com�n
        uint8_t mvarLights; //Flags con las luces que se deben encender.
        void auxRefresh(); //Escribe la constante de las luces en las salidas del Arduino.
        //C�digo para sem�foro        
        Servo* mvarServo; //Referencia al servo (si es que hay)
        uint8_t mvarServoPort; //N�mero de puerto del servo
        uint16_t mvarSemaphorePosition; //Posici�n actual que tiene el brazo del sem�foro.
        uint16_t mvarSemaphoreTarget; //Posici�n a la que deber�a tender el brazo del sem�foro.
        uint8_t mvarSemaphoreIncrement; //Factor de velocidad del sem�foro.
        bool mvarTomeuMode; //Modo Tomeu.
        bool mvarDynamic; //Gesti�n din�mica
        uint8_t mvarSpeedDelay; //Retardo del servo en movimiento
    };
}
#endif
